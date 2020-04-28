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
using UniAuto.UniBCS.Core;

namespace UniAuto.UniBCS.CSOT.LineSpecialService
{
    public partial class CFSpecialService : AbstractService
    {
        private const string MaterialVerificationRequestTimeout = "MaterialVerificationRequestTimeout";      
        private const string TargetLifeTimeReportTimeout = "TargetLifeTimeReportTimeout";
        private const string CstMappingDownloadTimeout = "CstControlCommandTimeout";
        private const string AccumulativeValueReportTimeout = "AccumulativeValueReportTimeout";

        public override bool Init()
        {
            return true;
        }

        #region [9.2.1 Material Verification Request]
        public void MaterialVerificationRequest(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true)
                    return;

                #region [拆出PLCAgent Data]
                string headID = string.Empty;
                eBitResult bitResult = (eBitResult)int.Parse(inputData[0][1][0].Value);
                string materialID = inputData[0][0][0].Value.Trim();
                string unitNo = inputData[0][0][1].Value;
                string unitID = string.Empty;
                string operatorID = inputData[0][0][2].Value;
                string slotNo = inputData[0][0][3].Value;
                if (slotNo == "1" || slotNo == "2")
                    headID = "ASIDE";
                else
                    headID = "BSIDE";
                eMaterialStatus verifyStatus = (eMaterialStatus)int.Parse(inputData[0][0][4].Value);
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

                #region [取得UnitID資訊]
                Unit unit = ObjectManager.UnitManager.GetUnit(eqpNo, unitNo);
                if (unit != null)
                {
                    unitID = unit.Data.UNITID;
                }
                #endregion

                #region [Report Off]
                if (bitResult == eBitResult.OFF)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF]"
                        , inputData.Metadata.NodeNo, inputData.TrackKey));
                    MaterialVerificationRequestReply(inputData.TrackKey, eBitResult.OFF, eReturnCode1.Unknown, eqpNo, unitNo);
                    return;
                }
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] MATERIAL ID=[{3}] UNIT NO.=[{4}] OPERATOR ID=[{5}] SLOT NO.=[{6}] VERIFY STATUS=[{7}].", 
                    inputData.Metadata.NodeNo, inputData.TrackKey, bitResult.ToString(), materialID, unitNo, operatorID, slotNo, verifyStatus.ToString()));
                
                #endregion

                #region Offline Skip to send MES
                if (line.File.HostMode == eHostMode.OFFLINE)
                {
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={1}] [BCS -> MES][{0}] Mask State Changed Send MES but OFFLINE LINENAME=[{1}].",
                        inputData.TrackKey, eqp.Data.LINEID));
                    //Offline下，always reply "NG" 2015/9/8 add by Frank
                    MaterialVerificationRequestReply(inputData.TrackKey, eBitResult.ON, eReturnCode1.NG, eqpNo, unitNo);
                    return;
                }
                #endregion

                #region [Report MES]
                else
                {
                    
                    switch (eqp.Data.NODEATTRIBUTE.ToUpper())
                    {
                        case "COATER":
                            //取出特定幾台機台內任一片玻璃的 Job ID 上報，若則無須上報。
                            IList<Equipment> EQPList = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID).
                                Where(e => e.Data.NODENO == "L2" || e.Data.NODENO == "L3" || e.Data.NODENO == "L4").ToList();
                            string productName = ObjectManager.JobManager.GetJobIDbyEQPList(EQPList);

                            if (verifyStatus == eMaterialStatus.MOUNT)
                            {
                                object[] _materialdata = new object[]
                            { 
                                inputData.TrackKey,   /*0  TrackKey*/
                                eqp.Data.LINEID,      /*1  LineName*/
                                eqp.Data.NODEID,      /*2  EQPID*/
                                "",                   /*3  LINERECIPENAME*/
                                "",                   /*4  MATERIALMODE*/ 
                                productName,          /*5  PRODUCTNAME*/
                                materialID,           /*6  MATERIALNAME*/
                                verifyStatus,         /*7  MATERIALSTATE*/
                                "",                   /*8  MATERIALWEIGHT*/
                                "PR",                 /*9  MATERIALTYPE*/
                                "",                   /*10  USEDCOUNT*/
                                "",                   /*11 LIFEQTIME*/
                                "",                   /*12 GROUPID*/
                                unitID,               /*13 UNITID*/
                                headID,               /*14 HEADID*/
                                "MaterialVerificationRequest"
                            };
                                //呼叫MES方法
                                Invoke(eServiceName.MESService, "MaterialStateChanged", _materialdata);

                                #region [Create Timer]
                                string timerID = string.Format("MaterialVerificationRequest_{0}_{1}_MaterialStateChanged", eqp.Data.NODENO, inputData.TrackKey);
                                if (line.File.HostMode == eHostMode.OFFLINE)
                                {
                                    return;
                                }
                                if (this.Timermanager.IsAliveTimer(timerID))
                                {
                                    Timermanager.TerminateTimer(timerID);
                                }
                                Timermanager.CreateTimer(timerID, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(MaterialStateChangedTimeoutForMES), unit.Data.UNITNO);
                                #endregion
                            }
                            else //Add by Frank 20160512
                            {
                                MaterialVerificationRequestReply(inputData.TrackKey, eBitResult.ON, eReturnCode1.OK, eqp.Data.NODENO, unit.Data.UNITNO);
                            }
                            break;
                        case "EXPOSURE":
                            if (verifyStatus == eMaterialStatus.PREPARE)
                            {
                                string glassId = "";
                                glassId = ObjectManager.JobManager.GetJobIDbyEQPNO(eqpNo);

                                if (line.File.HostMode != eHostMode.OFFLINE)
                                {
                                    object[] _dataPrepare = new object[5]
                                { 
                                    inputData.TrackKey,  /*0 TrackKey*/
                                    eqp.Data.LINEID,     /*1 LineName*/
                                    eqp.Data.NODEID,     /*2 EQPID*/
                                    materialID,          /*3 MASKNAME*/
                                    glassId,             /*4 PRODUCTNAME*/ 
                                };
                                    //呼叫MES方法
                                    Invoke(eServiceName.MESService, "ValidateMaskPrepareRequest", _dataPrepare);

                                    #region [Create Timer]
                                    string timerID = string.Format("{0}_{1}_MaterialVerificationRequest", eqp.Data.NODENO, inputData.TrackKey);
                                    if (this.Timermanager.IsAliveTimer(timerID))
                                    {
                                        Timermanager.TerminateTimer(timerID);
                                    }
                                    Timermanager.CreateTimer(timerID, false, ParameterManager["MESTIMEOUT"].GetInteger(), new System.Timers.ElapsedEventHandler(MaterialVerificationRequestTimeoutForMES), unit.Data.UNITNO);
                                    #endregion
                                }
                                else
                                {
                                    MaterialVerificationRequestReply(inputData.TrackKey, eBitResult.ON, eReturnCode1.OK, eqp.Data.NODENO, unit.Data.UNITNO);
                                }
                            }
                            break;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void MaterialVerificationRequestReply(string trxID, eBitResult bitResutl, eReturnCode1 reslut, string eqpNo, string uniNo)
        {
            try
            {

                string trxName = string.Format("{0}_MaterialVerificationRequestReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                if (bitResutl == eBitResult.ON)
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)reslut).ToString();
                    outputdata.EventGroups[0].Events[0].Items[1].Value = uniNo;
                }
                else
                {
                    // H.S.正常結束，無須填寫 Word
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                }
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)bitResutl).ToString(); //寫入bit
                outputdata.EventGroups[0].Events[0].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger(); //Delay Turn On Bit 
                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}", eqpNo, MaterialVerificationRequestTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (bitResutl == eBitResult.ON)
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BIT=[{2}] RETURNCODE=[{3}]",
                        eqpNo, trxID, bitResutl, reslut));
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BIT=[{2}]",
                       eqpNo, trxID, bitResutl));
                }


            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 紀錄mes time out
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="e"></param>
        public void MaterialStateChangedTimeoutForMES(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- MES][{1}] RETURNCODE=[NG] MES REPLY MATERIAL STATUS CHANGE REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[1], sArray[3]));

                MaterialVerificationRequestReply(sArray[1].ToString(), eBitResult.ON, eReturnCode1.NG, sArray[1].ToString(), sArray[2].ToString());

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 紀錄mes time out
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="e"></param>
        public void MaterialVerificationRequestTimeoutForMES(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- MES][{1}] RETURNCODE=[NG] MES REPLY MATERIAL VERIFICATION REQUEST TIMEOUT, SET BIT=[OFF].",
                    sArray[0], sArray[1]));

                MaterialVerificationRequestReply(sArray[1].ToString(), eBitResult.ON, eReturnCode1.NG, sArray[0].ToString(), timer.State.ToString());

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 紀錄EQP Clear Bit time out
        /// </summary>
        /// <param name="subject"></param>
        /// <param name="e"></param>
        public void MaterialVerificationRequestTimeoutForEQP(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] RETURNCODE=[NG] EQP REPLY MATERIAL VERIFICATION REQUEST TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                MaterialVerificationRequestReply(trackKey, eBitResult.OFF, eReturnCode1.Unknown, sArray[0].ToString(), sArray[1].ToString());

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion
        
        #region [9.2.16 Accumulative Value Report]
        public void AccumulativeValueReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true)
                    return;

                #region [取得EQP資訊]
                Equipment eqp;
                string eqpNo = inputData.Metadata.NodeNo;
                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO =[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                string accumulativevalue = inputData.EventGroups[0].Events[0].Items[0].Value;
                string productName = string.Empty;
                string materialtype = string.Empty;
                string materialname = string.Empty;

                //取出機台內任一片玻璃的 Job ID 上報，若則無須上報。
                Job job = ObjectManager.JobManager.GetJobs().FirstOrDefault(j => j.CurrentEQPNo == eqpNo);
                if (job != null)
                {
                    productName = job.GlassChipMaskBlockID;
                }

                switch (eqp.Data.NODEATTRIBUTE.ToUpper())
                {
                    case "DEVELOPER":
                        materialtype = "DEVHIGHKOH"; //Developer
                        break;
                    case "ETCH":
                        materialtype = "ETCHSOLUTION"; //Rework Etch
                        break;
                    case "STRIPPER":
                        materialtype = "STRIPPERSOLUTION"; //Rework Stripper
                        break;
                }
                #endregion

                #region [Report Off]
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Bit (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                    AccumulativeValueReportReply(inputData.TrackKey, eBitResult.OFF, eqpNo);
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Bit (ON)", inputData.Metadata.NodeNo, inputData.TrackKey));
                }
                #endregion

                #region [更新機台資訊]
                //save data in progarm
                lock (eqp) eqp.File.AccumulativeValue = double.Parse(accumulativevalue);
                //save progarm data in file
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] ACCUMULATIVE VALUE=[{2}].", eqpNo, inputData.TrackKey, eqp.File.AccumulativeValue.ToString()));
                #endregion

                #region [Reply EQ]
                AccumulativeValueReportReply(inputData.TrackKey, eBitResult.ON, eqpNo);
                #endregion

                #region [Report MES]
                object[] _data = new object[7]
                { 
                    inputData.TrackKey,       /*0  TrackKey*/
                    eqp.Data.LINEID,          /*1  LineName*/
                    eqp.Data.NODEID,          /*2  EQPID*/
                    materialname,             /*3  MaterialName*/
                    materialtype,             /*4  MaterialType*/
                    productName,              /*5  pnlID*/
                    accumulativevalue,        /*6  materialqty*/
                };
                //呼叫MES方法
                Invoke(eServiceName.MESService, "AutoDecreaseMaterialQuantity", _data);
                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void AccumulativeValueReportReply(string trxID, eBitResult bitResut, string eqpNo)
        {
            try
            {
                string trxName = string.Format("{0}_AccumulativeValueReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)bitResut).ToString(); //寫入bit
                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}", eqpNo, AccumulativeValueReportTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (bitResut.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(AccumulativeValueReportTimeoutForEQP), trxID);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ACCUMULATIVE VALUE REPORT REPLY SET BIT =[{2}].",
                    eqpNo, trxID, bitResut));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void AccumulativeValueReportTimeoutForEQP(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] ACCUMULATIVE VALUE REPORT TIMEOUT SET BIT (OFF).",
                    sArray[0], trackKey));

                AccumulativeValueReportReply(trackKey, eBitResult.OFF, sArray[0]);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [9.2.9 Rework Washable Count Report]
        public void ReworkWashableCountReport(Trx inputData)
        {
            try
            {
                #region [拆出PLCAgent Data]
                string reworkwashableCount = inputData.EventGroups[0].Events[0].Items[0].Value;
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

                #region [更新機台資訊]
                //save data in progarm
                lock (eqp) eqp.File.ReworkWashableCount = int.Parse(reworkwashableCount);
                //save progarm data in file
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] REWORK WASHABLE COUNT =[{2}].", eqpNo, inputData.TrackKey, eqp.File.ReworkWashableCount.ToString()));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Target Life Time Report]
        public void TargetLifeTimeReport(Trx inputData)
        {
            return; //20150617 Frank 此function T3 CF 暫無使用，
            try
            {
                if (inputData.IsInitTrigger == true)
                    return;

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                int totalTargetLifeTime = 0;
                int divisor = 7; //固定填7
                int average = 0;
                foreach (Item item in inputData.EventGroups[0].Events[0].Items.AllValues)
                {
                    totalTargetLifeTime += int.Parse(item.Value.ToString());
                }
                average = totalTargetLifeTime / divisor;
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
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (OFF)", inputData.Metadata.NodeNo, inputData.TrackKey));
                    TargetLifeTimeReportReply(inputData.TrackKey, eBitResult.OFF, eqpNo);
                    return;
                }
                else
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT (ON)", inputData.Metadata.NodeNo, inputData.TrackKey));
                }
                #endregion

                #region [Reply EQ]
                TargetLifeTimeReportReply(inputData.TrackKey, eBitResult.ON, eqpNo);
                #endregion

                #region [Report MES]
                IList<ChangeTargetLife.CHAMBERc> chamberList = new List<ChangeTargetLife.CHAMBERc>();
                ChangeTargetLife.CHAMBERc ctl = new ChangeTargetLife.CHAMBERc();
                ctl.CHAMBERID = string.Empty;
                ctl.QUANTITY = average.ToString();
                ctl.AVERAGE = string.Empty;
                chamberList.Add(ctl);

                object[] _data = new object[4]
                { 
                    inputData.TrackKey,   /*0  TrackKey*/
                    eqp.Data.LINEID,      /*1  LineName*/
                    eqp.Data.NODEID,      /*2  EQPID*/
                    chamberList           /*3  chamberList*/
                };
                //呼叫MES方法
                Invoke(eServiceName.MESService, "ChangeTargetLife", _data);
                #endregion

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void TargetLifeTimeReportReply(string trxID, eBitResult bitResut, string eqpNo)
        {
            try
            {
                string trxName = string.Format("{0}_TargetLifeTimeReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)bitResut).ToString(); //寫入bit
                //outputdata.EventGroups[0].Events[0].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger(); //Delay Turn On Bit 
                outputdata.TrackKey = trxID;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}", eqpNo, TankChangeReportTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (bitResut.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(TargetLifeTimeReportTimeoutForEQP), trxID);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] TARGET LIFE TIME REPORT REPLY SET BIT =[{2}].",
                    eqpNo, trxID, bitResut));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void TargetLifeTimeReportTimeoutForEQP(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] TARGET LIFE TIME REPORT REPLY TIMEOUT SET BIT (OFF).",
                    sArray[0], trackKey));

                TargetLifeTimeReportReply(trackKey, eBitResult.OFF, sArray[0]);

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [CF Cassette Map Download]

        public void CassetteMapDownload_UNPACK(Equipment local, Port port, IList<Job> slotData, Trx outputData)
        {
            try
            {
                #region Special Job Data
                foreach (Job j in slotData)
                {
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["Insp.JudgedData"].Value = j.InspJudgedData;
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();
                    if (j.CfSpecial.COAversion != string.Empty)
                        outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["COAVersion"].Value = j.CfSpecial.COAversion.PadRight(2).Substring(0, 2);
                }
                #endregion

                SendPLCData(outputData);

                string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        
        public void CassetteMapDownload_PHOTO(Equipment local, Port port, IList<Job> slotData, Trx outputData)
        {
            try
            {
                #region Special Job Data
                foreach (Job j in slotData)
                {
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSPReservations"].Value = j.INSPReservations;
                    int coaterCSPNo = 0;
                    int.TryParse(j.CfSpecial.CoaterCSPNo, out coaterCSPNo);
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CoaterCSPNo"].Value = coaterCSPNo.ToString();                    
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OCFlag"].Value = j.CfSpecial.OCFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["Insp.JudgedData1"].Value = j.InspJudgedData;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["Insp.JudgedData2"].Value = j.InspJudgedData2;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CFSpecialReserved"].Value = j.CFSpecialReserved;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag1"].Value = j.EQPFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag2"].Value = j.EQPFlag2;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();                    
                    if (j.CfSpecial.COAversion != string.Empty)
                        outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["COAVersion"].Value = j.CfSpecial.COAversion.PadRight(2).Substring(0, 2);
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetCassetteID"].Value = j.CfSpecial.TargetCSTID; 
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetPortNo"].Value = j.CfSpecial.TargetPortNo;                     
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetSlotNo"].Value = j.CfSpecial.TargetSlotNo; 
                    if (j.CfSpecial.AbnormalCode.ArrayPhotoPre_InlineID != string.Empty)
                        outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ArrayPhotoPre-InlineID"].Value =
                            ConstantManager["ARRAY_PHOTO_PRE_INLINE_ID"][j.CfSpecial.AbnormalCode.ArrayPhotoPre_InlineID.Trim().ToUpper()].Value;
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OvenHPSlotNumber"].Value = j.CfSpecial.OvenHPSlotNumber;                     
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["InlineReworkMaxCount"].Value = j.CfSpecial.InlineReworkMaxCount;       
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["InlineReworkRealCount"].Value = j.CfSpecial.InlineReworkRealCount;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["MarcoReserveFlag"].Value = j.CFMarcoReserveFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ProcessBackUp"].Value = j.CFProcessBackUp;

                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["SamplingValue"].Value = j.CfSpecial.SamplingValue;

                }
                #endregion

                SendPLCData(outputData);

                string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteMapDownload_MASK(Equipment local, Port port, IList<Job> slotData, Trx outputData)
        {
            try
            {
                #region Special Job Data
                foreach (Job j in slotData)
                {
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["Insp.JudgedData"].Value = j.InspJudgedData;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;

                    //string oxrInfo = string.Empty;
                    //for (int i = 0; i < j.OXRInformation.Length; i++)
                    //{
                    //    if (i.Equals(56)) break;
                    //    oxrInfo += ConstantManager["PLC_OXRINFO_AC"][j.OXRInformation.Substring(i, 1)].Value;
                    //}
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

                    if (j.CfSpecial.COAversion != string.Empty)
                        outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["COAVersion"].Value = j.CfSpecial.COAversion.PadRight(2).Substring(0, 2);
                }
                #endregion

                SendPLCData(outputData);

                string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteMapDownload_FCPSH(Equipment local, Port port, IList<Job> slotData, Trx outputData)
        {
            try
            {
                #region Special Job Data
                foreach (Job j in slotData)
                {
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSPReservations"].Value = j.INSPReservations;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["Insp.JudgedData"].Value = j.InspJudgedData;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
                    if (j.CfSpecial.RecyclingFlag == "Y")
                        outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CFSpecialReserved"].Value = "1000000000000000";
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LoaderBufferingFlag"].Value = j.CfSpecial.LoaderBufferingFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["RTCFlag"].Value = j.CfSpecial.RTCFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetCassetteID"].Value = j.TargetCSTID;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["SourcePortNo"].Value = j.CfSpecial.SourcePortNo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetPortNo"].Value = j.CfSpecial.TargetPortNo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetSlotNo"].Value = j.CfSpecial.TargetSlotNo;                   
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;
                    //string oxrInfo = string.Empty;
                    //for (int i = 0; i < j.OXRInformation.Length; i++)
                    //{
                    //    if (i.Equals(56)) break;
                    //    oxrInfo += ConstantManager["PLC_OXRINFO_AC"][j.OXRInformation.Substring(i, 1)].Value;
                    //}
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

                    if (j.CfSpecial.COAversion != string.Empty)
                        outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["COAVersion"].Value = j.CfSpecial.COAversion.PadRight(2).Substring(0, 2);
                }
                #endregion

                SendPLCData(outputData);

                string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        
        public void CassetteMapDownload_REWORK(Equipment local, Port port, IList<Job> slotData, Trx outputData)
        {
            try
            {
                #region Special Job Data
                foreach (Job j in slotData)
                {
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["Insp.JudgedData"].Value = j.InspJudgedData;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["VCRMismatchFlag"].Value = j.CfSpecial.VCRMismatchFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ReworkMaxCount"].Value = j.CfSpecial.ReworkMaxCount;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ReworkRealCount"].Value = "0";
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetCassetteID"].Value = j.TargetCSTID;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["SourcePortNo"].Value = j.CfSpecial.SourcePortNo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetPortNo"].Value = j.CfSpecial.TargetPortNo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetSlotNo"].Value = j.CfSpecial.TargetSlotNo;
                    //add by hujunpeng 20190117 for CF rework run cell rework PDR Current rework count add 1
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["MaxRwkCount"].Value = j.CellSpecial.MaxRwkCount;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CurrentRwkCount"].Value = j.CellSpecial.CurrentRwkCount;
                    //string oxrInfo = string.Empty;
                    //for (int i = 0; i < j.OXRInformation.Length; i++)
                    //{
                    //    if (i.Equals(56)) break;
                    //    oxrInfo += ConstantManager["PLC_OXRINFO_AC"][j.OXRInformation.Substring(i, 1)].Value;
                    //}
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

                    if (j.CfSpecial.COAversion != string.Empty)
                        outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["COAVersion"].Value = j.CfSpecial.COAversion.PadRight(2).Substring(0, 2);
                }
                #endregion

                SendPLCData(outputData);

                string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteMapDownload_FCSRT(Equipment local, Port port, IList<Job> slotData, Trx outputData)
        {
            try
            {
                #region Special Job Data
                foreach (Job j in slotData)
                {
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["Insp.JudgedData"].Value = j.InspJudgedData;
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["VCRMismatchFlag"].Value = j.CfSpecial.VCRMismatchFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetCassetteID"].Value = j.TargetCSTID;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["SourcePortNo"].Value = j.CfSpecial.SourcePortNo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetPortNo"].Value = j.CfSpecial.TargetPortNo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetSlotNo"].Value = j.CfSpecial.TargetSlotNo;              
                    //string oxrInfo = string.Empty;
                    //for (int i = 0; i < j.OXRInformation.Length; i++)
                    //{
                    //    if (i.Equals(56)) break;
                    //    oxrInfo += ConstantManager["PLC_OXRINFO_AC"][j.OXRInformation.Substring(i, 1)].Value;
                    //}
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

                    if (j.CfSpecial.COAversion != string.Empty)
                        outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["COAVersion"].Value = j.CfSpecial.COAversion.PadRight(2).Substring(0, 2);
                }
                #endregion

                SendPLCData(outputData);

                string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteMapDownload_REPAIR(Equipment local, Port port, IList<Job> slotData, Trx outputData)
        {
            try
            {
                #region Special Job Data
                foreach (Job j in slotData)
                {
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPReservations"].Value = j.EQPReservations;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["Insp.JudgedData"].Value = j.InspJudgedData;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["RTCFlag"].Value = j.CfSpecial.RTCFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LoaderBufferingFlag"].Value = j.CfSpecial.LoaderBufferingFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetCassetteID"].Value = j.TargetCSTID;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["SourcePortNo"].Value = j.CfSpecial.SourcePortNo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetPortNo"].Value = j.CfSpecial.TargetPortNo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetSlotNo"].Value = j.CfSpecial.TargetSlotNo;                   
                    //string oxrInfo = string.Empty;
                    //for (int i = 0; i < j.OXRInformation.Length; i++)
                    //{
                    //    if (i.Equals(56)) break;
                    //    oxrInfo += ConstantManager["PLC_OXRINFO_AC"][j.OXRInformation.Substring(i, 1)].Value;
                    //}
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

                    if (j.CfSpecial.COAversion != string.Empty)
                        outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["COAVersion"].Value = j.CfSpecial.COAversion.PadRight(2).Substring(0, 2);
                }
                #endregion

                SendPLCData(outputData);

                string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteMapDownload_MQC_1(Equipment local, Port port, IList<Job> slotData, Trx outputData)
        {
            try
            {
                #region Special Job Data
                foreach (Job j in slotData)
                {
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSPReservations"].Value = j.INSPReservations;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["Insp.JudgedData"].Value = j.InspJudgedData;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["RTCFlag"].Value = j.CfSpecial.RTCFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LoaderBufferingFlag"].Value = j.CfSpecial.LoaderBufferingFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetCassetteID"].Value = j.TargetCSTID;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["SourcePortNo"].Value = j.CfSpecial.SourcePortNo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetPortNo"].Value = j.CfSpecial.TargetPortNo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetSlotNo"].Value = j.CfSpecial.TargetSlotNo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["FlowPriorityInfo"].Value = j.CfSpecial.FlowPriorityInfo;                   
                    //string oxrInfo = string.Empty;
                    //for (int i = 0; i < j.OXRInformation.Length; i++)
                    //{
                    //    if (i.Equals(56)) break;
                    //    oxrInfo += ConstantManager["PLC_OXRINFO_AC"][j.OXRInformation.Substring(i, 1)].Value;
                    //}
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

                    if (j.CfSpecial.COAversion != string.Empty)
                        outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["COAVersion"].Value = j.CfSpecial.COAversion.PadRight(2).Substring(0, 2);
                }
                #endregion

                SendPLCData(outputData);

                string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteMapDownload_MQC_2(Equipment local, Port port, IList<Job> slotData, Trx outputData)
        {
            try
            {
                #region Special Job Data
                foreach (Job j in slotData)
                {
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSPReservations"].Value = j.INSPReservations;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["Insp.JudgedData"].Value = j.InspJudgedData;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["RTCFlag"].Value = j.CfSpecial.RTCFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LoaderBufferingFlag"].Value = j.CfSpecial.LoaderBufferingFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetCassetteID"].Value = j.TargetCSTID;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["SourcePortNo"].Value = j.CfSpecial.SourcePortNo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetPortNo"].Value = j.CfSpecial.TargetPortNo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetSlotNo"].Value = j.CfSpecial.TargetSlotNo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["FlowPriorityInfo"].Value = j.CfSpecial.FlowPriorityInfo;                    
                    //string oxrInfo = string.Empty;
                    //for (int i = 0; i < j.OXRInformation.Length; i++)
                    //{
                    //    if (i.Equals(56)) break;
                    //    oxrInfo += ConstantManager["PLC_OXRINFO_AC"][j.OXRInformation.Substring(i, 1)].Value;
                    //}
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

                    if (j.CfSpecial.COAversion != string.Empty)
                        outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["COAVersion"].Value = j.CfSpecial.COAversion.PadRight(2).Substring(0, 2);
                }
                #endregion

                SendPLCData(outputData);

                string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteMapDownload_FCMAC(Equipment local, Port port, IList<Job> slotData, Trx outputData)
        {
            try
            {
                #region Special Job Data
                foreach (Job j in slotData)
                {
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSPReservations"].Value = j.INSPReservations;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["Insp.JudgedData"].Value = j.InspJudgedData;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["RTCFlag"].Value = j.CfSpecial.RTCFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LoaderBufferingFlag"].Value = j.CfSpecial.LoaderBufferingFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetCassetteID"].Value = j.TargetCSTID;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["SourcePortNo"].Value = j.CfSpecial.SourcePortNo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetPortNo"].Value = j.CfSpecial.TargetPortNo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TargetSlotNo"].Value = j.CfSpecial.TargetSlotNo;
                    //string oxrInfo = string.Empty;
                    //for (int i = 0; i < j.OXRInformation.Length; i++)
                    //{
                    //    if (i.Equals(56)) break;
                    //    oxrInfo += ConstantManager["PLC_OXRINFO_AC"][j.OXRInformation.Substring(i, 1)].Value;
                    //}
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

                    if (j.CfSpecial.COAversion != string.Empty)
                        outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["COAVersion"].Value = j.CfSpecial.COAversion.PadRight(2).Substring(0, 2);
                }
                #endregion

                SendPLCData(outputData);

                string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteMapDownload_FCAOI(Equipment local, Port port, IList<Job> slotData, Trx outputData)
        {
            try
            {
                #region Special Job Data
                foreach (Job j in slotData)
                {
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSPReservations"].Value = j.INSPReservations;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["Insp.JudgedData"].Value = j.InspJudgedData;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["RTCFlag"].Value = j.CfSpecial.RTCFlag;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LoaderBufferingFlag"].Value = j.CfSpecial.LoaderBufferingFlag;
                    //string oxrInfo = string.Empty;
                    //for (int i = 0; i < j.OXRInformation.Length; i++)
                    //{
                    //    if (i.Equals(56)) break;
                    //    oxrInfo += ConstantManager["PLC_OXRINFO_AC"][j.OXRInformation.Substring(i, 1)].Value;
                    //}
                    //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
                    outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

                    if (j.CfSpecial.COAversion != string.Empty)
                        outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["COAVersion"].Value = j.CfSpecial.COAversion.PadRight(2).Substring(0, 2);
                }
                #endregion

                SendPLCData(outputData);

                string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        // Timeout
        private void CassetteMappingDownloadTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string[] obj = timer.State.ToString().Split('_');
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], CstMappingDownloadTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", sArray[0], sArray[1]);

                #region CassetteControlCommandForUPK Special Function
                //20160127 Add by Frank
                if (Workbench.LineType == eLineType.CF.FCUPK_TYPE1)
                {
                    Equipment eqp1 = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                    if (eqp1.Data.NODEATTRIBUTE == "UPK")
                        trxName = string.Format("{0}_Port#{1}CassetteControlCommandForUPK", sArray[0], sArray[1]);
                }
                #endregion

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].IsDisable = true;
                outputdata.EventGroups[1].Events[0].IsDisable = true;
                outputdata.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = obj[0];
                SendPLCData(outputdata);
                eCstControlCmd cmd = (eCstControlCmd)int.Parse(obj[1]);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] [PORT={2}] EQP REPLY,  CASSETTE CONTROL COMMAND=[{3}] REPLY TIMEOUT SET BIT (OFF).",
                    sArray[0], sArray[1], obj[0], cmd.ToString()));


                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { obj[0], ServerName, 
                        string.Format("CASSETTE{0}REPLY - EQUIPMENT=[{1}] PORT=[{2}] \"T1 TIMEOUT\"", 
                        cmd, sArray[0], sArray[1])});

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #endregion

        #region [Job Data Request]
        public void JobDataRequestReportReply_CF(Trx outputData, Job job, Line line, string eqpNo)
        {
            try
            {
                Event eVent = outputData.EventGroups[0].Events[0];

                #region CF Special Job Data
                eVent.Items[eJOBDATA.INSPReservations].Value = job.INSPReservations;   //BIN
                eVent.Items[eJOBDATA.EQPReservations].Value = job.EQPReservations;   //BIN
                eVent.Items[eJOBDATA.LastGlassFlag].Value = job.LastGlassFlag;   ////INT
                if (eVent.Items.AllKeys.Contains(eJOBDATA.InspJudgedData))
                    eVent.Items[eJOBDATA.InspJudgedData].Value = job.InspJudgedData;  //BIN
                else
                {
                    eVent.Items[eJOBDATA.InspJudgedData1].Value = job.InspJudgedData;
                    eVent.Items[eJOBDATA.InspJudgedData2].Value = job.InspJudgedData2;
                }
                eVent.Items[eJOBDATA.TrackingData].Value = job.TrackingData;   //BIN
                eVent.Items[eJOBDATA.CFSpecialReserved].Value = job.CFSpecialReserved;  //BIN
                if (eVent.Items.AllKeys.Contains(eJOBDATA.EQPFlag))
                    eVent.Items[eJOBDATA.EQPFlag].Value = job.EQPFlag;   //BIN
                else
                {
                    eVent.Items[eJOBDATA.EQPFlag1].Value = job.EQPFlag;
                    eVent.Items[eJOBDATA.EQPFlag2].Value = job.EQPFlag2;
                }
                eVent.Items[eJOBDATA.ChipCount].Value = job.ChipCount.ToString();   //INT
                eVent.Items[eJOBDATA.COAVersion].Value = job.CfSpecial.COAversion;   //ASCII

                switch (line.Data.JOBDATALINETYPE)
                {
                    case eJobDataLineType.CF.PHOTO_BMPS:
                    case eJobDataLineType.CF.PHOTO_GRB:
                        eVent.Items["TargetCassetteID"].Value = job.CfSpecial.TargetCSTID;
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
                        break;
                    case eJobDataLineType.CF.REWORK:
                        outputData.EventGroups[0].Events[0].Items["ReworkMaxCount"].Value = job.CfSpecial.ReworkMaxCount;   //INT
                        outputData.EventGroups[0].Events[0].Items["ReworkRealCount"].Value = job.CfSpecial.ReworkRealCount;   //INT
                        outputData.EventGroups[0].Events[0].Items["CFSpecialReserved"].Value = job.CFSpecialReserved;   //BIN
                        break;
                    case eJobDataLineType.CF.UNPACK:
                    case eJobDataLineType.CF.MASK:
                        outputData.EventGroups[0].Events[0].Items["CFSpecialReserved"].Value = job.CFSpecialReserved;   //INT
                        break;
                    case eJobDataLineType.CF.REPAIR:
                    case eJobDataLineType.CF.FCMAC:
                        outputData.EventGroups[0].Events[0].Items["RTCFlag"].Value = job.CfSpecial.RTCFlag;   //INT
                        outputData.EventGroups[0].Events[0].Items["LoaderBufferingFlag"].Value = job.CfSpecial.LoaderBufferingFlag;   //BIN
                        outputData.EventGroups[0].Events[0].Items["CFSpecialReserved"].Value = job.CFSpecialReserved;   //BIN
                        //outputData.EventGroups[0].Events[0].Items["TargetCSTID"].Value = job.TargetCSTID;   //ASCII
                        break;
                    case eJobDataLineType.CF.MQC_1:
                    case eJobDataLineType.CF.MQC_2:
                        outputData.EventGroups[0].Events[0].Items["RTCFlag"].Value = job.CfSpecial.RTCFlag;   //INT
                        outputData.EventGroups[0].Events[0].Items["LoaderBufferingFlag"].Value = job.CfSpecial.LoaderBufferingFlag;   //BIN
                        outputData.EventGroups[0].Events[0].Items["CFSpecialReserved"].Value = job.CFSpecialReserved;   //BIN
                        break;
                    /*case eJobDataLineType.CF.MQC_3:
                        outputData.EventGroups[0].Events[0].Items["RTCFlag"].Value = job.CfSpecial.RTCFlag;   //INT
                        outputData.EventGroups[0].Events[0].Items["LoaderBufferingFlag"].Value = job.CfSpecial.LoaderBufferingFlag;   //BIN
                        outputData.EventGroups[0].Events[0].Items["FlowPriorityInfo"].Value = job.CfSpecial.FlowPriorityInfo;   //INT
                        outputData.EventGroups[0].Events[0].Items["CFSpecialReserved"].Value = job.CFSpecialReserved;   //BIN
                        outputData.EventGroups[0].Events[0].Items["TargetCSTID"].Value = job.TargetCSTID;   //ASCII
                        break;
                     */
                    case eJobDataLineType.CF.FCSRT:
                        outputData.EventGroups[0].Events[0].Items["SourcePortNo"].Value = job.CfSpecial.SourcePortNo;   //BIN
                        outputData.EventGroups[0].Events[0].Items["TargetPortNo"].Value = job.CfSpecial.TargetPortNo;   //BIN
                        //outputData.EventGroups[0].Events[0].Items["TargetCSTID"].Value = job.TargetCSTID;   //ASCII
                        break;
                }
                #endregion

                //outputData.EventGroups[0].Events[0].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger(); //Delay Turn On Bit 
                SendPLCData(outputData);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, outputData.TrackKey, JobDataRequestReportReplyTimeOut);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }
                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(JobDataRequestReportTimeoutForEQP), outputData.TrackKey);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC REPLY, JOB DATA REQUEST REPORT SET BIT =[{2}], RETURN CODE=[{3}].",
                    eqpNo, outputData.TrackKey, eBitResult.ON, eReturnCode1.OK));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void JobDataRequestReportTimeoutForEQP(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subject as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeoutName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], JobDataRequestReportReplyTimeOut);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                string trxName = string.Format("{0}_JobDataRequestReportReply", sArray[0]);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].IsDisable = true;
                outputdata.EventGroups[0].Events[2].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = sArray[1];
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] EQP REPLY, JOB DATA REQUEST REPORT TIMEOUT SET BIT (OFF).",
                    sArray[0], sArray[1]));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Job Data Creater]
        public void JobDataCreater(Job job, XmlDocument xmlDoc)
        {
            try
            {
                IList<Job> localLineJobs = new List<Job>();
                IList<Job> mesJobs = new List<Job>();
                IList<RecipeCheckInfo> idCheckInfos = new List<RecipeCheckInfo>();
                IDictionary<string, IList<RecipeCheckInfo>> recipeIDCheckData = new Dictionary<string, IList<RecipeCheckInfo>>();
                Line line = ObjectManager.LineManager.GetLine(ServerName);
                List<string> items = new List<string>();
                string trxID = xmlDoc.SelectSingleNode("//MESSAGE/HEADER/TRANSACTIONID").InnerText;
                string mesppid = string.Empty;
                string ppid = string.Empty;
                string err = string.Empty;

                localLineJobs = ObjectManager.JobManager.GetJobs();
                Job OldJob = ObjectManager.JobManager.GetJob(job.CassetteSequenceNo, job.JobSequenceNo);

                if (OldJob == null)
                {
                    #region 獲取xmlDoc Body層資料
                    XmlNode body = GetMESBodyNode(xmlDoc);
                    XmlNodeList lotNodeList = body[keyHost.LOTLIST].ChildNodes;
                    XmlNodeList productList = lotNodeList[0][keyHost.PRODUCTLIST].ChildNodes;
                    Job NewJob = job;

                    #region Body
                    NewJob.MesCstBody.LINENAME = body[keyHost.LINENAME].InnerText.ToString();
                    NewJob.MesCstBody.LINERECIPENAME = body[keyHost.LINERECIPENAME].InnerText.ToString();
                    NewJob.MesCstBody.SELECTEDPOSITIONMAP = body[keyHost.SELECTEDPOSITIONMAP].InnerText.ToString();
                    #endregion

                    #region Lot List
                    NewJob.MesCstBody.LOTLIST[0].LOTNAME = lotNodeList[0][keyHost.LOTNAME].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].PRODUCTSPECNAME = lotNodeList[0][keyHost.PRODUCTSPECNAME].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].PRODUCTSPECVER = lotNodeList[0][keyHost.PRODUCTSPECVER].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].PROCESSFLOWNAME = lotNodeList[0][keyHost.PROCESSFLOWNAME].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME = lotNodeList[0][keyHost.PROCESSOPERATIONNAME].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].PRODUCTOWNER = lotNodeList[0][keyHost.PRODUCTOWNER].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].PRDCARRIERSETCODE = lotNodeList[0][keyHost.PRDCARRIERSETCODE].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].SALEORDER = lotNodeList[0][keyHost.SALEORDER].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].PRODUCTSIZETYPE = lotNodeList[0][keyHost.PRODUCTSIZETYPE].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].PRODUCTSIZE = lotNodeList[0][keyHost.PRODUCTSIZE].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].BCPRODUCTTYPE = lotNodeList[0][keyHost.BCPRODUCTTYPE].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].BCPRODUCTID = lotNodeList[0][keyHost.BCPRODUCTID].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].PRODUCTPROCESSTYPE = lotNodeList[0][keyHost.PRODUCTPROCESSTYPE].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].PROCESSTYPE = lotNodeList[0][keyHost.PROCESSTYPE].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].LINERECIPENAME = lotNodeList[0][keyHost.LINERECIPENAME].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].PPID = lotNodeList[0][keyHost.PPID].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].SUBPRODUCTSPECS = lotNodeList[0][keyHost.SUBPRODUCTSPECS].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].SUBPRODUCTNAMES = lotNodeList[0][keyHost.SUBPRODUCTNAMES].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].SUBPRODUCTLINES = lotNodeList[0][keyHost.SUBPRODUCTLINES].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].SUBPRODUCTSIZETYPES = lotNodeList[0][keyHost.SUBPRODUCTSIZETYPES].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].SUBPRODUCTSIZES = lotNodeList[0][keyHost.SUBPRODUCTSIZES].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].ORIENTEDSITE = lotNodeList[0][keyHost.ORIENTEDSITE].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].ORIENTEDFACTORYNAME = lotNodeList[0][keyHost.ORIENTEDFACTORYNAME].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].CURRENTSITE = lotNodeList[0][keyHost.CURRENTSITE].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].CURRENTFACTORYNAME = lotNodeList[0][keyHost.CURRENTFACTORYNAME].InnerText.ToString();
                    NewJob.MesCstBody.LOTLIST[0].PRODUCTTHICKNESS = lotNodeList[0][keyHost.PRODUCTTHICKNESS].InnerText.ToString();

                    NewJob.MesCstBody.LOTLIST[0].LINEQTIMELIST = new List<LINEQTIMEc>();
                    //STBPRODUCTSPECLIST - ??
                    NewJob.MesCstBody.LOTLIST[0].STBPRODUCTSPECLIST = new List<STBPRODUCTSPECc>();
                    #endregion

                    int position = 0;
                    for (int i = 0; i < productList.Count; i++)
                    {
                        if (job.GlassChipMaskBlockID == productList[i][keyHost.PRODUCTNAME].InnerText.ToString())
                        {
                            position = i;
                            break;
                        }
                        else
                        {
                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[BCS] GLASSID = [{0}], CAN'T FIND IN MES DOWNLOAD OF SHORT CUT GLASS INFORMATION.", job.GlassChipMaskBlockID));
                        }
                    }

                    #region PRODUCTLIST
                    NewJob.MesProduct.POSITION = productList[position][keyHost.POSITION].InnerText.ToString();
                    NewJob.MesProduct.PRODUCTNAME = productList[position][keyHost.PRODUCTNAME].InnerText.ToString();
                    NewJob.MesProduct.ARRAYPRODUCTNAME = productList[position][keyHost.ARRAYPRODUCTNAME].InnerText.ToString();
                    NewJob.MesProduct.CFPRODUCTNAME = productList[position][keyHost.CFPRODUCTNAME].InnerText.ToString();
                    NewJob.MesProduct.ARRAYPRODUCTSPECNAME = productList[position][keyHost.ARRAYPRODUCTSPECNAME].InnerText.ToString();
                    NewJob.MesProduct.ARRAYLOTNAME = productList[position][keyHost.ARRAYLOTNAME].InnerText.ToString();
                    NewJob.MesProduct.DENSEBOXID = productList[position][keyHost.DENSEBOXID].InnerText.ToString();
                    NewJob.MesProduct.PRODUCTJUDGE = productList[position][keyHost.PRODUCTJUDGE].InnerText.ToString();
                    NewJob.MesProduct.PRODUCTGRADE = productList[position][keyHost.PRODUCTGRADE].InnerText.ToString();
                    NewJob.MesProduct.SOURCEPART = productList[position][keyHost.SOURCEPART].InnerText.ToString();
                    NewJob.MesProduct.PRODUCTRECIPENAME = productList[position][keyHost.PRODUCTRECIPENAME].InnerText.ToString();
                    NewJob.MesProduct.SUBPRODUCTGRADES = productList[position][keyHost.SUBPRODUCTGRADES].InnerText.ToString();
                    NewJob.MesProduct.SUBPRODUCTDEFECTCODE = productList[position][keyHost.SUBPRODUCTDEFECTCODE].InnerText.ToString();
                    NewJob.MesProduct.SUBPRODUCTJPSGRADE = productList[position][keyHost.SUBPRODUCTJPSGRADE].InnerText.ToString();
                    NewJob.MesProduct.SUBPRODUCTJPSCODE = productList[position][keyHost.SUBPRODUCTJPSCODE].InnerText.ToString();
                    //NewJob.MesProduct.SUBPRODUCTJPSFLAG = productList[position][keyHost.SUBPRODUCTJPSFLAG].InnerText.ToString();
                    //NewJob.MesProduct.SUBPRODUCTPBGRADE = productList[position][keyHost.SUBPRODUCTPBGRADE].InnerText.ToString();
                    NewJob.MesProduct.ARRAYSUBPRODUCTGRADE = productList[position][keyHost.ARRAYSUBPRODUCTGRADE].InnerText.ToString();
                    NewJob.MesProduct.CFSUBPRODUCTGRADE = productList[position][keyHost.CFSUBPRODUCTGRADE].InnerText.ToString();
                    NewJob.MesProduct.GROUPID = productList[position][keyHost.GROUPID].InnerText.ToString();
                    NewJob.MesProduct.PRODUCTTYPE = productList[position][keyHost.PRODUCTTYPE].InnerText.ToString();
                    NewJob.MesProduct.DUMUSEDCOUNT = productList[position][keyHost.DUMUSEDCOUNT].InnerText.ToString();
                    NewJob.MesProduct.CFTYPE1REPAIRCOUNT = productList[position][keyHost.CFTYPE1REPAIRCOUNT].InnerText.ToString();
                    NewJob.MesProduct.CFTYPE2REPAIRCOUNT = productList[position][keyHost.CFTYPE2REPAIRCOUNT].InnerText.ToString();
                    NewJob.MesProduct.CARBONREPAIRCOUNT = productList[position][keyHost.CARBONREPAIRCOUNT].InnerText.ToString();
                    NewJob.MesProduct.LASERREPAIRCOUNT = productList[position][keyHost.LASERREPAIRCOUNT].InnerText.ToString();
                    NewJob.MesProduct.ITOSIDEFLAG = productList[position][keyHost.ITOSIDEFLAG].InnerText.ToString();
                    NewJob.MesProduct.SHORTCUTFLAG = productList[position][keyHost.SHORTCUTFLAG].InnerText.ToString();
                    NewJob.MesProduct.OWNERTYPE = productList[position][keyHost.OWNERTYPE].InnerText.ToString();
                    NewJob.MesProduct.OWNERID = productList[position][keyHost.OWNERID].InnerText.ToString();
                    NewJob.MesProduct.REVPROCESSOPERATIONNAME = productList[position][keyHost.REVPROCESSOPERATIONNAME].InnerText.ToString();
                    NewJob.MesProduct.TARGETPORTNAME = productList[position][keyHost.TARGETPORTNAME].InnerText.ToString();
                    NewJob.MesProduct.CHAMBERRUNMODE = productList[position][keyHost.CHAMBERRUNMODE].InnerText.ToString();
                    NewJob.MesProduct.TEMPERATUREFLAG = productList[position][keyHost.TEMPERATUREFLAG].InnerText.ToString();
                    //NewJob.MesProduct.MACHINEPROCESSSEQ = productList[position][keyHost.MACHINEPROCESSSEQ].InnerText.ToString();
                    NewJob.MesProduct.SCRAPCUTFLAG = productList[position][keyHost.SCRAPCUTFLAG].InnerText.ToString();
                    NewJob.MesProduct.PPID = productList[position][keyHost.PPID].InnerText.ToString();
                    NewJob.MesProduct.FMAFLAG = productList[position][keyHost.FMAFLAG].InnerText.ToString();
                    NewJob.MesProduct.MHUFLAG = productList[position][keyHost.MHUFLAG].InnerText.ToString();
                    NewJob.MesProduct.ARRAYPRODUCTSPECVER = productList[position][keyHost.ARRAYPRODUCTSPECVER].InnerText.ToString();
                    NewJob.MesProduct.AGINGENABLE = productList[position][keyHost.AGINGENABLE].InnerText.ToString();
                    NewJob.MesProduct.PROCESSFLAG = productList[position][keyHost.PROCESSFLAG].InnerText.ToString();

                    //將原始Data List 置換成新資料。
                    NewJob.MesProduct.ABNORMALCODELIST = new List<CODEc>();
                    NewJob.MesProduct.LCDROPLIST = new List<string>();
                    NewJob.MesProduct.REWORKLIST = new List<REWORKc>();
                    NewJob.MesProduct.DEFECTLIST = new List<DEFECTc>();
                    MESProductDataIntoJobObject(productList[position], ref NewJob);

                    //CFShortCutPermitReply - MES SPEC沒給，故維持初始資料，暫不給值。
                    //NewJob.MesProduct.ARRAYTTPEQVERCODE = productList[position][keyHost.ARRAYTTPEQVERCODE].InnerText.ToString();
                    //NewJob.MesProduct.ODFSITE = productList[position][keyHost.ODFSITE].InnerText.ToString();
                    //NewJob.MesProduct.RTPFLAG = productList[position][keyHost.RTPFLAG].InnerText.ToString();
                    #endregion

                    
                    NewJob.SourcePortID = string.Empty;
                    NewJob.TargetPortID = string.Empty;
                    NewJob.ToCstID = string.Empty;
                    NewJob.ToSlotNo = string.Empty;
                    NewJob.CurrentSlotNo = "0"; // add by bruce 20160412 for T2 Issue
                    NewJob.CurrentEQPNo = string.Empty;
                    NewJob.JobJudge = "1";
                    NewJob.JobGrade = productList[position][keyHost.PRODUCTGRADE].InnerText.Trim();
                    NewJob.JobType = ObjectManager.JobManager.M2P_GetJobType(productList[position][keyHost.PRODUCTTYPE].InnerText);
                    NewJob.LineRecipeName = productList[position][keyHost.PRODUCTRECIPENAME].InnerText.Trim();
                    NewJob.EQPJobID = productList[position][keyHost.PRODUCTNAME].InnerText.Trim();
                    NewJob.GlassChipMaskBlockID = productList[position][keyHost.PRODUCTNAME].InnerText.Trim();
                    NewJob.SamplingSlotFlag = productList[position][keyHost.PROCESSFLAG].InnerText.Equals("Y") ? "1" : "0";
                    NewJob.HostDefectCodeData = Transfer_DefectCodeToFileFormat(productList[position][keyHost.SUBPRODUCTDEFECTCODE].InnerText);

                    // 初始化參數內容
                    NewJob.EQPReservations = SpecialItemInitial("EQPReservations", 6);
                    NewJob.INSPReservations = SpecialItemInitial("INSPReservations", 6);
                    NewJob.InspJudgedData = SpecialItemInitial("Insp.JudgedData", 16);
                    NewJob.InspJudgedData2 = SpecialItemInitial("InspJudgedData2", 16);
                    NewJob.TrackingData = SpecialItemInitial("TrackingData", 16);
                    NewJob.CFSpecialReserved = SpecialItemInitial("CFSpecialReserved", 16);
                    NewJob.EQPFlag = SpecialItemInitial("EQPFlag", 16);
                    NewJob.EQPFlag2 = SpecialItemInitial("EQPFlag2", 16);
                    //CF Special Data Initial
                    NewJob.CfSpecial.PermitFlag = string.Empty;
                    NewJob.CfSpecial.CFShortCutRecipeIDCheckFlag = false;
                    NewJob.CfSpecial.CFShortCutRecipeParameterCheckFlag = false;
         
                    #endregion

                    #region 取得 Q Time 設定
                    ObjectManager.QtimeManager.ValidateQTimeSetting(NewJob);
                    #endregion

                    #region 產生 Product Type
                    items.Add(lotNodeList[0][keyHost.PRODUCTSPECNAME].InnerText);
                    items.Add(lotNodeList[0][keyHost.PRODUCTSPECVER].InnerText);
                    items.Add(lotNodeList[0][keyHost.PROCESSOPERATIONNAME].InnerText);
                    items.Add(lotNodeList[0][keyHost.PRODUCTOWNER].InnerText);
                    items.Add(productList[position][keyHost.OWNERID].InnerText);
                    items.Add(productList[position][keyHost.SOURCEPART].InnerText);
                    NewJob.ProductType.SetItemData(items);
                    ObjectManager.JobManager.GetProductType(eFabType.CF, productList[position][keyHost.OWNERTYPE].InnerText, localLineJobs, NewJob, out err);
                    #endregion

                    #region Get Recipe ID
                    // 取出原始 Port 資訊
                    Port port = ObjectManager.PortManager.GetPort(NewJob.MesCstBody.PORTNAME);

                    foreach (LOTc lot in job.MesCstBody.LOTLIST)
                    {
                        foreach (PROCESSLINEc processline in lot.PROCESSLINELIST)
                        {
                            if (processline.LINENAME == line.Data.LINEID)
                            {
                                if (AnalysisMesPPID_CFShortCut(port, processline, ref idCheckInfos, out ppid, out mesppid, out err))
                                {
                                    NewJob.PPID = ppid;
                                    NewJob.MES_PPID = processline.PPID;
                                }
                            }
                        }
                    }
                    #endregion

                    mesJobs.Add(NewJob);

                    //將資料放入JobManager
                    if (mesJobs.Count() > 0)
                    {
                        ObjectManager.JobManager.AddJobs(mesJobs);
                    }

                    NewJob.CfSpecial.CfShortCutWIPCheck = true;
                }
                else
                {
                    lock (OldJob)
                    {
                        OldJob.CfSpecial.CfShortCutWIPCheck = false;
                    }
                    ObjectManager.JobManager.EnqueueSave(OldJob);
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("JOB WIP DUPLICATE. CASSETTE SEQUENCE NO=[{0}], JOB SEQUENCE NO=[{1}]", job.CassetteSequenceNo, job.JobSequenceNo));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Job WIP Delete]
        public void JobWIPDelete(string eqpID, string cstSeq, string jobSeq, string jobID)
        {
            try
            {
                Job job = null;
                job = ObjectManager.JobManager.GetJob(cstSeq, jobSeq);
                if (job == null)
                    job = ObjectManager.JobManager.GetJob(jobID);
                if (job != null)
                    ObjectManager.JobManager.DeleteJob(job);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("JOB WIP DELETE. CASSETTE SEQUENCE NO=[{0}], JOB SEQUENCE NO=[{1}], JOB ID NO=[{2}]",
                    cstSeq, jobSeq, jobID));

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

        private XmlNode GetMESBodyNode(XmlDocument aDoc)
        {
            return aDoc.SelectSingleNode("//MESSAGE/BODY");
        }

        private string Transfer_DefectCodeToFileFormat(string hostDefectData)
        {
            string[] defect = hostDefectData.Split(',');
            string newData = string.Empty;

            for (int i = 0; i < defect.Count(); i++)
            {
                if (!string.IsNullOrEmpty(newData)) newData += ",";
                newData += defect[i].PadRight(5);
            }
            return newData;
        }

        /// <summary>
        /// 分析MES PPID
        /// </summary>
        public bool AnalysisMesPPID(string[] recipeIDs, Line line, out string ppid, out string errMsg)
        {
            errMsg = string.Empty;
            ppid = string.Empty;
            try
            {
                IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();

                if (eqps.Count != recipeIDs.Length)
                {
                    errMsg = "CASSETTE DATA TRANSFER ERROR: THE COUNT OF MES PPID IS NOT ENOUGH.";
                    return false;
                }

                for (int i = 0; i < recipeIDs.Length; i++)
                {
                    string eqpNo = "L" + (i + 2).ToString();
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    if (eqp == null)
                    {
                        errMsg = string.Format("CASSETTE DATA TRANSFER ERROR: CANNOT FOUND EQUIPMENT OBJECT, LINE NAME=[{0}], EQUIPMENT NO=[{1}].", line.Data.LINEID, eqpNo);
                        return false;
                    }

                    if (recipeIDs[i].Trim().Length > eqp.Data.RECIPELEN)
                    {
                        errMsg = string.Format("CASSETTE DATA TRANSFER ERROR: MES PPID LENGTH ERROR, LINE NAME=[{0}], EQUIPMENT NO=[{1}], PPID=[{2}] > RECIPE LEN=[{3}].",
                            line.Data.LINEID, eqpNo, recipeIDs[i], eqp.Data.RECIPELEN);
                        return false;
                    }
                    ppid += recipeIDs[i].PadRight(eqp.Data.RECIPELEN);
                }
                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                return false;
            }
        }

        /// <summary>
        /// 分析MES PPID
        /// </summary>
        public bool AnalysisMesPPID_CFShortCut(Port port, PROCESSLINEc nextLineInfos, ref IList<RecipeCheckInfo> idRecipeInfos, out string ppid, out string mesppid, out string errMsg)
        {
            mesppid = string.Empty;
            errMsg = string.Empty;
            ppid = string.Empty;
            try
            {

                string eqPPID = string.Empty;
                eqPPID = nextLineInfos.PPID;
                mesppid = eqPPID; //Keep MES Donwload PPID 不會有虛擬機台、跳號，隨時還要上報，不得更動

                //Watson Add 避免沒有要check的機台還加入Recipe Check，要下給機台的
                string eqFillnullPPID = string.Empty;
                string photo_EQP_PPID = string.Empty;
                Dictionary<string, string> EQP_RecipeNo = ObjectManager.JobManager.AddVirtualEQP_PPID_CFShortCut(nextLineInfos.LINENAME, eqPPID, out eqFillnullPPID);

                if (EQP_RecipeNo == null)
                {
                    errMsg = string.Format("{0} Cassette Data Download PPID{1} is Error!!", nextLineInfos.LINENAME, eqPPID);
                    return false;
                }

                string productRcpName = nextLineInfos.LINERECIPENAME;

                string eqpNo = string.Empty;
                foreach (string eqpno in EQP_RecipeNo.Keys)
                {
                    eqpNo = eqpno;
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                    if (eqp == null)
                    {
                        //ex: FBRPH Line 沒有機台L10, L15 and L20 不需要加入Recipe Check(不需丟給機台做Recipe Validate)
                        NLogManager.Logger.LogWarnWrite(null, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("CF Photo Line PPID Type is Special  Spec!! EQP No =[{0}] is not in DB, but must fill '00' in EQP PPID", eqpNo));
                        continue; //跨號機台不跳出!!
                    }

                    if (!idRecipeInfos.Any(r => r.EQPNo.Equals(eqpNo) && r.RecipeID.Equals(EQP_RecipeNo[eqpno])))
                    {
                        idRecipeInfos.Add(new RecipeCheckInfo(eqpNo, 1, int.Parse(port.Data.PORTNO), EQP_RecipeNo[eqpno]/*recipe*/, productRcpName));
                    }
                }
                //寫給機台的PPID不能有分號並且補足00
                ppid = eqFillnullPPID;

                return true;
            }
            catch (Exception ex)
            {
                errMsg = ex.ToString();
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        private string SpecialItemInitial(string itemName, int defaultLen)
        {
            try
            {
                int len = ObjectManager.SubJobDataManager.GetItemLenth(itemName);
                if (len != 0)
                    return new string('0', len);
                else
                    return new string('0', defaultLen);
            }
            catch
            {
                return new string('0', defaultLen);
            }
        }

        /// <summary>
        /// 將MES Product Data塞入Job Data裡
        /// </summary>
        private void MESProductDataIntoJobObject(XmlNode productNode, ref Job job)
        {
            ObjectManager.JobManager.ConvertXMLToObject(job.MesProduct, productNode);
        }
        /*
        public void AlignerCheckWarning(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger == true)
                    return;

                #region [拆出PLCAgent Data]
                
                eBitResult bitResult = (eBitResult)int.Parse(inputData[0][0][0].Value);

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
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF]"
                        , inputData.Metadata.NodeNo, inputData.TrackKey));
                    
                    return;
                }
                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] AlignerCheckWarning.",
                    inputData.Metadata.NodeNo, inputData.TrackKey, bitResult.ToString()));

                #endregion
                
                Invoke("EquipmentService", "TransferStopCommand", new object[] { "L5", "1", inputData.TrackKey });
                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData .TrackKey , line.Data.LINENAME  , 
                                    string.Format("[{0}]AlignerCheckWarning,Please check NSK Aligner ",inputData.Metadata.NodeNo  )});
                 
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
         */

        //add by qiumin 20180106 UPK ULD CST EMPTY SLOT COUNT LIMIT ,IF THIS COUNT BIGGER EMPTY SLOT COUNT  TRANSFER STOP 
        public void CheckUpkCstBuffer(string trxid ,Equipment eqp)
        {
            try
            {
                int upkemptycount = 0;  //
                int buffercount = ParameterManager["UPKCSTBUFFERCOUNT"].GetInteger();
                if (buffercount == 0) return;
                IList<Port> portlist = ObjectManager.PortManager.GetPorts(eqp.Data.NODEID).Where(p => p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING ||p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING  ).ToList();
                int y = 28;
                for (int x = 0; x < portlist.Count; x++)
                {
                    upkemptycount=upkemptycount+ y - int.Parse(portlist[x].File.JobCountInCassette);
                    
                }
                if (buffercount > upkemptycount)
                {
                    Invoke("EquipmentService", "TransferStopCommand", new object[] { "L2", "1", trxid });

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT=L2] [BCS -> EQP][{0}] BUFFER COUNT =[{1}],UPKEMPTYCOUNT=[{2}]TRANSFER_STOP=[1]", trxid, buffercount.ToString(), upkemptycount.ToString()));
                }
                else
                {
                    Invoke("EquipmentService", "TransferStopCommand", new object[] { "L2", "2", trxid });

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT=L2] [BCS -> EQP][{0}] BUFFER COUNT =[{1}],UPKFULLCOUNT=[{2}]TRANSFER_STOP=[2]", trxid, buffercount.ToString(), upkemptycount.ToString()));

                }
                
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }


    }
}
