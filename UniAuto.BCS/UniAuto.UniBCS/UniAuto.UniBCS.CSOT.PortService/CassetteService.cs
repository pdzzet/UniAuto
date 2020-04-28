using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.PLCAgent.PLC;
using System.Reflection;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.OpiSpec;
using System.Xml;
using UniAuto.UniBCS.MesSpec;
using UniAuto.UniBCS.Core;
using System.IO;
using System.Timers;
using System.Text.RegularExpressions;
using System.Collections;
using System.Diagnostics;


namespace UniAuto.UniBCS.CSOT.PortService
{
    public class CassetteService : AbstractService
    {
        private const string CstStsChangeTimeout = "CassetteStsChangeTimeout";
        private const string CstControlCommandTimeout = "CstControlCommandTimeout";
        private const string FileInfoNotifyCommandTimeout = "FileInfoNotifyCommandTimeout";

        private string _fileDataTempPath = @"D:\UnicomLog\";
        private int CassetteProcessEndScanCount = 0;

        private string armlastcstseq = string.Empty;  //add by yang  2017/6/8
        private string armlastjobseq = string.Empty;

        /// <summary>
        //可以通过Spring 进行修改
        /// </summary>
        public string FileDataTempPath
        {
            get { return _fileDataTempPath; }
            set { _fileDataTempPath = value; }
        }

        public override bool Init()
        {
            return true;
        }

        #region [Cassette Status Change]
        object obj = new object();

        /// <summary>
        /// 更新記億體資料
        /// </summary>
        /// <param name="inputData">同CassetteStatusChangeReport(Trx inputData)</param>
        /// <param name="log">記錄動作者</param>
        public void CassetteStatusChangeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                #region [取得EQP及Port資訊]
                Line line; Equipment eqp; Port port;

                string eqpNo = inputData.Metadata.NodeNo;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, ServerName));

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);

                if (port == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_PORT, portNo));
                #endregion
                string log;

                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    RefreshPortStatus_CELL(line, port, inputData, out log);
                }
                else
                {
                    RefreshPortStatus_AC(line, port, inputData, out log);
                }

                if (port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_CASSETTE_DATA)
                {
                    lock (port) port.File.OPI_SubCstState = eOPISubCstState.WACSTEDIT;
                }

                if (port.File.Status == ePortStatus.LC)
                {
                    Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));
                    if (cst == null)
                    {
                        #region [新增卡匣到Cassette Entity]
                        cst = new Cassette()
                        {
                            LineID = port.Data.LINEID.Trim(),
                            NodeID = port.Data.NODEID.Trim(),
                            NodeNo = port.Data.NODENO.Trim(),
                            PortID = port.Data.PORTID.Trim(),
                            PortNo = port.Data.PORTNO.Trim(),
                            CassetteID = port.File.CassetteID.Trim(),
                            CassetteSequenceNo = port.File.CassetteSequenceNo,
                            LoadTime = DateTime.Now,
                        };
                        ObjectManager.CassetteManager.CreateCassette(cst);
                        #endregion
                    }
                    cst = null;
                }


                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}]{3}.", eqp.Data.NODENO, sourceMethod, inputData.TrackKey, log));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteStatusChangeReport(Trx inputData)
        {
            try
            {
                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[1].Events[1].Items[0].Value);
                #endregion

                #region [取得EQP及Port資訊]
                Line line; Equipment eqp; Port port;

                string eqpNo = inputData.Metadata.NodeNo;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_INFORMATION=[{0}] IN LINEENTITY!", ServerName));

                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);

                if (port == null) throw new Exception(string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, inputData.Metadata.NodeNo));
                #endregion

                if (inputData.IsInitTrigger)
                {
                    CassetteStatusChangeReportUpdate(inputData, "CassetteStatusChangeReport_Initial");
                    return;
                }

                // Report Off
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF] PORT=[{2}] CSTID=[{3}].",
                        inputData.Metadata.NodeNo, inputData.TrackKey, port.Data.PORTNO, port.File.CassetteID));
                    CassetteStatusChangeReportReply(eqpNo, portNo, port.File.CassetteID, eBitResult.OFF, inputData.TrackKey);

                    //20170204 huangjiayin: add job grade check for pck, if mismatch with L3 job Grade , send cim message to L3
                    try
                    {
                        #region PCK Job Grade Check Special

                        if (line.Data.LINETYPE == eLineType.CELL.CCPCK
                            && (eCassetteStatus)int.Parse(inputData.EventGroups[1].Events[0].Items[1].Value) == eCassetteStatus.IN_PROCESSING
                            && eqpNo == "L2")
                        {
                            string pck_ld_grade = string.Empty;
                            string pck_pck_grade = string.Empty;
                            Cassette pck_ld_cst = ObjectManager.CassetteManager.GetCassette(Convert.ToInt32(inputData.EventGroups[1].Events[0].Items[2].Value));


                            if (pck_ld_cst != null)
                            {
                                pck_ld_grade = pck_ld_cst.MES_CstData.LOTLIST[0].PRODUCTLIST[0].PRODUCTGRADE;
                                if (!string.IsNullOrEmpty(pck_ld_grade))
                                {


                                    //L3 Job Count=0不check
                                    int pck_total_job = 0;
                                    pck_total_job = ObjectManager.EquipmentManager.GetEQP("L3").File.TotalTFTJobCount;

                                    foreach (Port p in ObjectManager.PortManager.GetPortsByLine(eqp.Data.LINEID))
                                    {
                                        if (p.File.Type == ePortType.UnloadingPort) pck_total_job += Convert.ToInt32(p.File.JobCountInCassette);
                                    }

                                    if (pck_total_job == 0) return;

                                    IList<Job> pck_jobs = ObjectManager.JobManager.GetJobs();
                                    Job pck_eqp_job = null;
                                    bool skip_pck_port = false;

                                    foreach (Job jb in pck_jobs)
                                    {
                                        //Check PCK Unloading Port Job Grade
                                        if (!skip_pck_port &&
                                            !string.IsNullOrEmpty(jb.ToCstID) &&
                                             !jb.RemoveFlag &&
                                             jb.JobSequenceNo != "60000" &&
                                             pck_ld_grade != jb.JobGrade &&
                                             DateTime.Now.AddDays(-1) < jb.LastUpdateTime) //只计算当前在L3并且是从L2投入的最近1天的Job，找到一片不符就不找了
                                        {
                                            pck_pck_grade = jb.JobGrade;
                                            Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommandForCELL", new object[] { this.CreateTrxID(), "L3", "[Warn] CST " + pck_ld_cst.CassetteID.Trim()+" ["+
                                            pck_ld_grade+"] is Mismatch with PCK["+pck_pck_grade+"] Box:<"+jb.ToCstID+">!!", "", "1" });
                                            skip_pck_port = true;
                                        }

                                        //Check PCK EQP Job Grade Only 1Job: Not in CST OR BOX: 找所有Job最近的一片
                                        if (ObjectManager.EquipmentManager.GetEQP("L3").File.TotalTFTJobCount > 0 &&
                                            string.IsNullOrEmpty(jb.ToCstID) &&
                                             !jb.RemoveFlag &&
                                             jb.JobSequenceNo != "60000" &&
                                             jb.CurrentEQPNo == "L3" &&
                                             jb.FromCstID.Trim().Length == 6
                                            )
                                        {
                                            if (pck_eqp_job == null) pck_eqp_job = jb;
                                            else if (pck_eqp_job.LastUpdateTime < jb.LastUpdateTime) pck_eqp_job = jb;
                                        }

                                    }

                                    //Send CIM Message
                                    if (pck_eqp_job != null)
                                    {
                                        if (pck_ld_grade != pck_eqp_job.JobGrade)
                                        {
                                            Invoke(eServiceName.CIMMessageService, "CIMMessageSetCommandForCELL", new object[] { this.CreateTrxID(), "L3", "[Warn] CST " + pck_ld_cst.CassetteID.Trim()+" ["+
                                            pck_ld_grade+"] is Mismatch with PCK["+pck_eqp_job.JobGrade+"] EQP:<"+pck_eqp_job.EQPJobID+">!!", "", "1" });

                                        }
                                    }



                                }

                            }


                        }

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "Can not get PCK Job Grade " + port.File.CassetteID);

                    }
                    ////

                    return;
                }

                //add for Robot fetch CST priority use 2016/03/28 cc.kuang
                Event evt = inputData.EventGroups[1].Events[0];
                eCassetteStatus cststs = (eCassetteStatus)int.Parse(evt.Items[1].Value);
                if (cststs == eCassetteStatus.WAITING_FOR_PROCESSING)
                {
                    DateTime waitforprocesstime = DateTime.Now;
                    string cstseq = evt.Items[2].Value;
                    IList<Job> lstJob;

                    lstJob = ObjectManager.JobManager.GetJobs(cstseq); //get all job by CST Sequence
                    if (lstJob != null)
                    {
                        foreach (Job jb in lstJob)
                        {
                            jb.WaitForProcessTime = waitforprocesstime;
                        }
                    }
                }

                string log;
                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    RefreshPortStatus_CELL(line, port, inputData, out log);
                }
                else
                {
                    RefreshPortStatus_AC(line, port, inputData, out log);
                }


                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}]{3}",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode.ToString(), log));
                //Reply EQP
                CassetteStatusChangeReportReply(eqpNo, portNo, port.File.CassetteID, eBitResult.ON, inputData.TrackKey);

                Cassette cst = null;

                #region Watson Add PMT Line Special Rule
                //LINE 必須從Port去取得，因為PMT 的LOADER是共用。
                //但在DB是屬於PMI500,  但是 PORT.DATA.LINEID 
                //會改變配置，也會存在DB 
                if (line.Data.LINETYPE == eLineType.CELL.CBPMT)
                {
                    line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                    if (line == null)
                    {
                        Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}]{3}",
                                eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode.ToString(), "PMT LINE : Port[" + port.Data.PORTNO + "] ASSIGMENT LINE ERROR!!"));
                        //取不到仍取原值，才不至影響其他的判斷
                        line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                    }
                }
                #endregion
                IList<Job> deleteJobs = new List<Job>();
                HandleCstStatus(inputData.TrackKey, line, eqp, port, inputData, out cst, out deleteJobs);

                Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, port });

                // 處理完記Cassette History
                RecordCassetteHistory(inputData.TrackKey, eqp, port, cst, string.Empty, string.Empty, string.Empty);

                //add by qiumin 20180106 CheckUpkCstBuffer 
                if (line.Data.LINEID.Contains("UPK") && eqp.Data.NODENO == "L5" && eqp.File.EquipmentRunMode != "RE-CLEAN" && (port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING || port.File.CassetteStatus == eCassetteStatus.IN_PROCESSING))
                {
                    Invoke(eServiceName.CFSpecialService, "CheckUpkCstBuffer", new object[] { inputData.TrackKey, eqp });
                }

                //20161226 sy add 將Delete 移到最後面 避免影響 port 屬性改完才存 History
                #region [Move/Delete Jobs]
                int delayTime = ParameterManager["FILEMOVEDELAYTIME"].GetInteger();
                if (!deleteJobs.Count().Equals(0))
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Move Job Start : Count [{0}]", deleteJobs.Count.ToString()));
                    ObjectManager.JobManager.MoveJobs(line, deleteJobs, delayTime);
                    ObjectManager.JobManager.RecordJobsHistory(deleteJobs, eqp.Data.NODEID, eqp.Data.NODENO, port.Data.PORTNO, eJobEvent.Delete_CST_Complete.ToString(), inputData.TrackKey);
                    MoveFileData(line, deleteJobs, port, delayTime);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Move Job End "));
                }
                #region FCUPK Unloader Delete Jobs
                //if ((line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1 || line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE2)
                //    && (port.File.Type == ePortType.UnloadingPort || port.File.Type == ePortType.BothPort))   //no need check both port,mark by yang 2017/4/19

                if ((line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1 || line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE2) && port.File.Type == ePortType.UnloadingPort)
                {
                    deleteJobs = ObjectManager.JobManager.GetJobs(port.File.CassetteSequenceNo);
                    if (!deleteJobs.Count().Equals(0))
                    {
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Move FCUPK Unloader Jobs Start : Count [{0}]", deleteJobs.Count.ToString()));
                        ObjectManager.JobManager.MoveJobs(line, deleteJobs, delayTime);
                        ObjectManager.JobManager.RecordJobsHistory(deleteJobs, eqp.Data.NODEID, eqp.Data.NODENO, port.Data.PORTNO, eJobEvent.Delete_CST_Complete.ToString(), inputData.TrackKey);
                        MoveFileData(line, deleteJobs, port, delayTime);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Move FCUPK Unloader Jobs  End "));
                    }
                }
                #endregion
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[1].Events[1].Items[0].Value.Equals("1"))
                {
                    int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                    string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                    CassetteStatusChangeReportReply(inputData.Name.Split('_')[0], portNo, "", eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void CassetteStatusChangeReportReply(string eqpNo, string portNo, string cstID, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_Port#{1}CassetteStatusChangeReportReply", eqpNo, portNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, CstStsChangeTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(CassetteStatusChangeReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}] CSTID=[{3}], SET BIT=[{4}].",
                    eqpNo, trackKey, portNo, cstID, value));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void CassetteStatusChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {

                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}] CASSETTE STATUS CHANGE REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey, sArray[1]));

                CassetteStatusChangeReportReply(sArray[0], sArray[1], "", eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 更新資料到Port的變數資料
        /// </summary>
        /// <param name="port"></param>
        /// <param name="inputData"></param>
        private void RefreshPortStatus_AC(Line line, Port port, Trx inputData, out string log)
        {
            log = string.Format(" PORT=[{0}]", port.Data.PORTNO);
            try
            {
                Event evt = inputData.EventGroups[1].Events[0];
                lock (port.File)
                {
                    string value = evt.Items[3].Value;
                    value = Regex.Replace(value, @"[^\x21-\x7E]|<|>|'", " ");//过滤不可显示的字符 20150211 tom
                    port.File.CassetteID = value.Trim();
                    log += string.Format(" CSTID=[{0}]", port.File.CassetteID);

                    port.File.Status = (ePortStatus)int.Parse(evt.Items[0].Value);
                    log += string.Format(" PORT_STATUS=[{0}]({1})", evt.Items[0].Value, port.File.Status.ToString());

                    port.File.CassetteStatus = (eCassetteStatus)int.Parse(evt.Items[1].Value);
                    log += string.Format(" CST_STATUS=[{0}]({1})", evt.Items[1].Value, port.File.CassetteStatus.ToString());

                    port.File.CassetteSequenceNo = evt.Items[2].Value;
                    log += string.Format(" CST_SEQNO=[{0}]", port.File.CassetteSequenceNo);

                    port.File.JobCountInCassette = evt.Items[4].Value;
                    log += string.Format(" JOB_COUNT_IN_CST=[{0}]", port.File.JobCountInCassette);

                    port.File.CompletedCassetteData = (eCompletedCassetteData)int.Parse(evt.Items[5].Value);
                    log += string.Format(" COMPLETED_CST_DATA=[{0}]({1})", evt.Items[5].Value, port.File.CompletedCassetteData.ToString());

                    value = evt.Items[6].Value;
                    value = Regex.Replace(value, @"[^\x21-\x7E]|<|>|'", " ");//过滤不可显示的字符 20150211 tom
                    port.File.OperationID = value;
                    log += string.Format(" OPERATIONID=[{0}]", port.File.OperationID);

                    port.File.JobExistenceSlot = evt.Items[7].Value.Substring(0, port.Data.MAXCOUNT);
                    log += string.Format(" JOB_EXISTENCE_SLOT=[{0}]", port.File.JobExistenceSlot);

                    port.File.LoadingCassetteType = (eLoadingCstType)int.Parse(evt.Items[8].Value);
                    log += string.Format(" LOADING_CST_TYPE=[{0}]({1})", evt.Items[8].Value, port.File.LoadingCassetteType.ToString());

                    port.File.QTimeFlag = (eQTime)int.Parse(evt.Items[9].Value);
                    log += string.Format(" Q_TIMEFLAG=[{0}]({1})", evt.Items[9].Value, port.File.QTimeFlag.ToString());

                    port.File.PartialFullFlag = (eParitalFull)int.Parse(evt.Items[10].Value);
                    log += string.Format(" PARTIAL_FULL_FLAG=[{0}]({1})", evt.Items[10].Value, port.File.PartialFullFlag.ToString());

                    if (line.Data.LINETYPE == eLineType.ARRAY.CAC_MYTEK)
                    {
                        //port.File.BACVByPassFlag = (eBACV_ByPass)int.Parse(evt.Items[11].Value);
                        //log += string.Format(" BACV_BYPASS_FLAG=[{0}]({1})", evt.Items[11].Value, port.File.BACVByPassFlag.ToString());

                        port.File.DistortionFlag = (eDistortion)int.Parse(evt.Items[13].Value);
                        log += string.Format(" DISTORTION_FLAG=[{0}]({1})", evt.Items[13].Value, port.File.DistortionFlag.ToString());

                        port.File.DirectionFlag = (eDirection)int.Parse(evt.Items[14].Value);
                        log += string.Format(" DIRECTION_FLAG=[{0}]({1})", evt.Items[14].Value, port.File.DirectionFlag.ToString());

                        port.File.GlassExist = (eGlassExist)int.Parse(evt.Items[15].Value);
                        log += string.Format(" GLASS_EXIST=[{0}]({1})", evt.Items[15].Value, port.File.GlassExist.ToString());
                    }

                    //if (line.Data.LINETYPE == eLineType.CF.FCSRT_TYPE1)
                    //{
                    //    port.File.MappingGrade = evt.Items["GradeType"].Value;
                    //    log += string.Format(" MAPPING_GRADE=[{0}]", port.File.MappingGrade);

                    //    port.File.ProductType = evt.Items["ProductType"].Value;
                    //    log += string.Format(" PRODUCT_TYPE=[{0}]", port.File.ProductType);
                    //}
                }
                ObjectManager.PortManager.EnqueueSave(port.File);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void RefreshPortStatus_CELL(Line line, Port port, Trx inputData, out string log)
        {
            log = string.Format(" PORT=[{0}]", port.Data.PORTNO);
            try
            {
                Event evt = inputData.EventGroups[1].Events[0];
                lock (port.File)
                {
                    string value = evt.Items[3].Value;
                    value = Regex.Replace(value, @"[^\x21-\x7E]|<|>|'", " ");//过滤不可显示的字符 20150211 tom
                    port.File.CassetteID = value.Trim();
                    log += string.Format(" CSTID=[{0}]", port.File.CassetteID);

                    port.File.Status = (ePortStatus)int.Parse(evt.Items[0].Value);
                    log += string.Format(" PORT_STATUS=[{0}]({1})", evt.Items[0].Value, port.File.Status.ToString());

                    port.File.CassetteStatus = (eCassetteStatus)int.Parse(evt.Items[1].Value);
                    log += string.Format(" CST_STATUS=[{0}]({1})", evt.Items[1].Value, port.File.CassetteStatus.ToString());

                    port.File.CassetteSequenceNo = evt.Items[2].Value;
                    log += string.Format(" CST_SEQNO=[{0}]", port.File.CassetteSequenceNo);

                    port.File.JobCountInCassette = evt.Items[4].Value;
                    log += string.Format(" JOB_COUNT_IN_CST=[{0}]", port.File.JobCountInCassette);

                    port.File.CompletedCassetteData = (eCompletedCassetteData)int.Parse(evt.Items[5].Value);
                    log += string.Format(" COMPLETED_CST_DATA=[{0}]({1})", evt.Items[5].Value, port.File.CompletedCassetteData.ToString());

                    value = evt.Items[6].Value;
                    value = Regex.Replace(value, @"[^\x21-\x7E]|<|>|'", " ");//过滤不可显示的字符 20150211 tom
                    port.File.OperationID = value.Trim();
                    log += string.Format(" OPERATIONID=[{0}]", port.File.OperationID);

                    port.File.JobExistenceSlot = evt.Items[7].Value.Substring(0, port.Data.MAXCOUNT);
                    log += string.Format(" JOB_EXISTENCE_SLOT=[{0}]", port.File.JobExistenceSlot);

                    port.File.LoadingCassetteType = (eLoadingCstType)int.Parse(evt.Items[8].Value);
                    log += string.Format(" LOADING_CST_TYPE=[{0}]({1})", evt.Items[8].Value, port.File.LoadingCassetteType.ToString());

                    port.File.QTimeFlag = (eQTime)int.Parse(evt.Items[9].Value);
                    log += string.Format(" Q_TIMEFLAG=[{0}]({1})", evt.Items[9].Value, port.File.QTimeFlag.ToString());

                    port.File.PartialFullFlag = (eParitalFull)int.Parse(evt.Items[10].Value);
                    log += string.Format(" PARTIAL_FULL_FLAG=[{0}]({1})", evt.Items[10].Value, port.File.PartialFullFlag.ToString());

                    port.File.CassetteSetCode = inputData.EventGroups[1].Events[0].Items[11].Value;
                    log += string.Format(" CST_SETCODE=[{0}]", port.File.CassetteSetCode.ToString());

                    port.File.MaxSlotCount = port.Data.MAXCOUNT.ToString();
                    if (evt.Items["MaxSlotCount"] != null)
                    {
                        port.File.MaxSlotCount = evt.Items["MaxSlotCount"].Value;
                        log += string.Format(" MAX_SLOT_COUNT=[{0}]", port.File.MaxSlotCount);
                    }
                    #region [CCCLN USE]
                    if (evt.Items["DistortionFlag"] != null)
                    {
                        port.File.DistortionFlag = (eDistortion)int.Parse(evt.Items["DistortionFlag"].Value);
                        log += string.Format(" DistortionFlag=[{0}]", port.File.DistortionFlag.ToString());
                    }
                    if (evt.Items["DirectionFlag"] != null)
                    {
                        port.File.DirectionFlag = (eDirection)int.Parse(evt.Items["DirectionFlag"].Value);
                        log += string.Format("DirectionFlag=[{0}]", port.File.DirectionFlag.ToString());
                    }
                    if (evt.Items["GlassExistFlag"] != null)
                    {
                        port.File.GlassExist = (eGlassExist)int.Parse(evt.Items["GlassExistFlag"].Value);
                        log += string.Format("GlassExistFlag=[{0}]", port.File.DirectionFlag.ToString());
                    }
                    #endregion

                    #region T2 USE
                    //port.File.CompletedCassetteReason = (eCompleteCassetteReason)int.Parse(evt.Items[12].Value);
                    //log += string.Format(" COMPLETE_CST_REASON=[{0}]({1})", evt.Items[12].Value, port.File.CompletedCassetteReason.ToString());

                    //if (line.Data.LINETYPE == eLineType.CELL.CBPOL_1 || line.Data.LINETYPE == eLineType.CELL.CBPOL_2 || line.Data.LINETYPE == eLineType.CELL.CBPOL_3)  //Jun Modify 20141225 使來Line Type來判斷 
                    //{
                    //    port.File.AutoClaveByPass = (eBitResult)int.Parse(inputData.EventGroups[1].Events[0].Items[13].Value);
                    //    log += string.Format(" AUTO_CLAVE_BYPASS=[{0}])({1})", evt.Items[13].Value, port.File.AutoClaveByPass.ToString());

                    //    port.File.MappingGrade = inputData.EventGroups[1].Events[0].Items[14].Value;
                    //    log += string.Format(" MAPPING_GRADE=[{0}]", port.File.MappingGrade);
                    //}

                    ////Watson Add 20150319 使用Line Type來判斷 
                    //if (line.Data.LINETYPE == eLineType.CELL.CBSOR_1 || line.Data.LINETYPE == eLineType.CELL.CBSOR_2 || line.Data.LINETYPE == eLineType.CELL.CBPRM || line.Data.LINETYPE == eLineType.CELL.CBPMT)
                    //{
                    //    port.File.MaxSlotCount = evt.Items[13].Value;
                    //    log += string.Format(" MAX_SLOT_COUNT=[{0}]", port.File.MaxSlotCount);
                    //}
                    #endregion
                }
                ObjectManager.PortManager.EnqueueSave(port.File);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ReportMESPortStatusChange(Port port, Trx inputData)
        {
            try
            {
                // MES Data
                object[] _data = new object[3]
                {
                    inputData.TrackKey,     /*0 TrackKey*/
                    port.Data.LINEID,       /*1 LineName*/
                    port                  /*2 Port*/
                };
                Invoke(eServiceName.MESService, "PortTransferStateChanged", _data);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void RecordPortHistory(string trxID, Equipment eqp, Port port)
        {
            try
            {
                // Save DB
                PORTHISTORY his = new PORTHISTORY();
                his.UPDATETIME = DateTime.Now;
                his.LINEID = eqp.Data.LINEID;
                his.NODEID = eqp.Data.NODEID;
                his.PORTID = port.Data.PORTID;
                his.PORTNO = int.Parse(port.Data.PORTNO);
                his.PORTTYPE = port.File.Type.ToString();
                his.PORTMODE = port.File.Mode.ToString();
                his.PORTENABLEMODE = port.File.EnableMode.ToString();
                his.PORTTRANSFERMODE = port.File.TransferMode.ToString();
                his.PORTSTATUS = port.File.Status.ToString();
                his.CASSETTESEQNO = int.Parse(port.File.CassetteSequenceNo);
                his.CASSETTESTATUS = port.File.CassetteStatus.ToString();
                his.QTIMEFLAG = port.File.QTimeFlag.ToString();
                his.TRANSACTIONID = trxID;
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line != null)
                {
                    if (line.Data.FABTYPE == eFabType.CELL.ToString())
                        his.GRADE = port.File.UseGrade;
                    else if (line.Data.FABTYPE == eFabType.CF.ToString())
                        his.GRADE = port.File.MappingGrade;
                    else
                        his.GRADE = port.File.Grade;
                }
                else
                {
                    his.GRADE = "NULL";
                }
                his.PRODUCTTYPE = port.File.ProductType;
                his.CASSETTESETCODE = port.File.CassetteSetCode;

                ObjectManager.PortManager.InsertDB(his);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void HandleCstStatus(string trxID, Line line, Equipment eqp, Port port, Trx inputData, out Cassette cst, out IList<Job> deleteJobs)
        {
            string err = string.Empty;
            cst = null;
            IList<Job> jobs = new List<Job>();
            deleteJobs = new List<Job>();//20161226 sy add 將Delete 移到最後面 避免影響 port 屬性改完才存 History
            string reasonCode;
            string reasonText;
            try
            {
                cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));
                lock (port) port.File.OPI_SubCstState = eOPISubCstState.NONE;

                switch (port.File.CassetteStatus)
                {
                    case eCassetteStatus.NO_CASSETTE_EXIST:
                        {
                            /* 机台cassette status上报 No Cassette Exist 时，
                             * 如果CST ID 有值，而且 Port Status 是 LDCM 時，則產生 Cassette Entity.  
                             * Add by Kasim 20150508 */
                            if (!string.IsNullOrEmpty(port.File.CassetteID) && port.File.Status == ePortStatus.LC)
                            {
                                #region [新增卡匣到Cassette Entity]
                                cst = new Cassette()
                                {
                                    LineID = port.Data.LINEID.Trim(),
                                    NodeID = port.Data.NODEID.Trim(),
                                    NodeNo = port.Data.NODENO.Trim(),
                                    PortID = port.Data.PORTID.Trim(),
                                    PortNo = port.Data.PORTNO.Trim(),
                                    CassetteID = port.File.CassetteID.Trim(),
                                    CassetteSequenceNo = port.File.CassetteSequenceNo,
                                    LoadTime = DateTime.Now,
                                };
                                ObjectManager.CassetteManager.CreateCassette(cst);
                                #endregion
                            }

                        }
                        break;
                    case eCassetteStatus.WAITING_FOR_CASSETTE_DATA:
                        {
                            Cassette oldcst = null;

                            if (cst != null)
                                cst.FirstGlassCheckReport = string.Empty;

                            /* Online 情况下，机台cassette status上报waiting for cassette data时，
                             * 如果CST ID 没有值，请BC 直接cancel, 并在Log中标明何种原因被cancel. 
                             * 不需再向MES 发validate cassette request. Add by Kasim 20150403 */
                            if (string.IsNullOrEmpty(port.File.CassetteID))
                            {
                                err = string.Format("[EQUIPMENT={0}] PORT=[{1}] CASSETTE ID IS NULL!", port.Data.NODENO, port.Data.PORTNO);
                                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);

                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                                    string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                                Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { eqp.Data.NODENO, port.Data.PORTNO });
                                return;
                            }


                            //mark by huangjiayin 20180428
                            // if (ParameterManager.ContainsKey("PANELBOXIDCHECK"))
                            // {
                            //  if (ParameterManager.Parameters["PANELBOXIDCHECK"].GetBoolean())
                            //  {

                            #region For Cell PCK&OVP Special,Get Panel ID List Add By Yangzhenteng20180306
                            string[] pnlBoxIDSpecialLines = { "CCPCK100", "CCPCK300", "CCPCK500", "CCPCK800", "CCOVP100" };
                            if (pnlBoxIDSpecialLines.Contains(line.Data.LINEID) && inputData.EventGroups.Count > 2)
                            {
                                Port PORT = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, port.Data.PORTNO);
                                List<string> PanelIDList = new List<string>();
                                string Panelid = string.Empty;
                                PanelIDList.Clear();
                                lock (PanelIDList)
                                {
                                    for (int j = 0; j < inputData.EventGroups[2].Events[0].Items.Count; j++)
                                    {
                                        Panelid = inputData.EventGroups[2].Events[0].Items[j].Value.Trim().ToString();
                                        if (!string.IsNullOrEmpty(Panelid))
                                        {
                                            PanelIDList.Add(Panelid);
                                        }
                                    }
                                    //先检查机台上报数据是否为空，如果为空，则退卡，不为空向MES要BOXID;
                                    string errmessage = string.Empty;
                                    if (PanelIDList.Count != 0)
                                    {
                                        Invoke(eServiceName.MESService, "BoxIDRequest", new object[] { inputData.TrackKey, port, line.Data.LINEID, PanelIDList, eqp.Data.NODEID });
                                    }
                                    if (PanelIDList.Count == 0)
                                    {
                                        errmessage = string.Format("[{0}]PortID[{1}]Report Panel ID IS Empty,Please Check!!", eqp.Data.NODEID, PORT.Data.PORTID);
                                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                                                string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, errmessage) });
                                        Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { eqp.Data.NODENO, port.Data.PORTNO });
                                        return;
                                    }
                                }
                                Thread.Sleep(1000);
                                List<string> BOXIDLIST = new List<string>();
                                BOXIDLIST = PORT.File.BoxIDList.ToList();
                                bool Result = false;
                                string ErrMessage = string.Empty;
                                if (!string.IsNullOrEmpty(BOXIDLIST.FirstOrDefault()))
                                {
                                    for (int i = 0; i < BOXIDLIST.Count; i++)
                                    {
                                        if (BOXIDLIST[0] == BOXIDLIST[i])
                                        {
                                            Result = true;
                                        }
                                        if (BOXIDLIST[0] != BOXIDLIST[i])
                                        {
                                            ErrMessage = string.Format("Panel Box Name Error, The Panel'S Box Name[{0}] IS Not Same With BoxName[{1}],Please Check!!", BOXIDLIST[0], BOXIDLIST[i]);
                                            Result = false;
                                            break;
                                        }
                                    }
                                    if (Result == true)
                                    {
                                        string MesBoxname = string.Empty;
                                        MesBoxname = PORT.File.CassetteID;
                                        if (MesBoxname != BOXIDLIST.FirstOrDefault())
                                        {
                                            ErrMessage = string.Format("Panel Box Name Error, The Panel'S Box ID[{0}] IS Not Same With The Cassette ID[{1}],Please Check!!", BOXIDLIST.FirstOrDefault(), MesBoxname);
                                        }
                                    }
                                }
                                if (string.IsNullOrEmpty(BOXIDLIST.FirstOrDefault()))
                                {
                                    ErrMessage = string.Format("[{0}]PortID[{1}]MES Reply Box Name Error, The Box Name IS Empty,Please Check!!", line.Data.LINEID, PORT.Data.PORTID);
                                }
                                BOXIDLIST.Clear();//清除此次处理过的;
                                PORT.File.BoxIDList.Clear();
                                lock (PORT.File)
                                {
                                    ObjectManager.PortManager.EnqueueSave(PORT.File);
                                }
                                if (!string.IsNullOrEmpty(ErrMessage))
                                {
                                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ErrMessage);
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                                            string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, ErrMessage) });
                                    Invoke(eServiceName.CassetteService, "CassetteProcessCancel", new object[] { eqp.Data.NODENO, port.Data.PORTNO });
                                    return;
                                }
                            }
                            #endregion

                            //   }

                            //   }

                            if (port.File.Status != ePortStatus.LC)
                            {
                                err = string.Format("[EQUIPMENT={0}] PORT=[{1}] PORT_STATUS=[{2}] CST_STATUS=[{3}] IS INVALID!",
                                    port.Data.NODENO, port.Data.PORTNO, port.File.Status.ToString(), port.File.CassetteStatus.ToString());
                                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);

                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                                    string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                                return;
                            }

                            if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.COOL_RUN_MODE)
                            {
                                // 會有LoaderPort 轉成Unloading Port, 但Loading Port會有不用抽的玻璃, 所以不用判斷
                                //if (port.File.Type == ePortType.LoadingPort || port.File.Type == ePortType.BothPort)
                                //{
                                // Cool Run時, CassetteSequenceNo 會變動, 所以使用CST ID來取得CST Object
                                oldcst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());

                                if (oldcst != null && (oldcst.CoolRunStart == false || line.File.CoolRunRemainCount == 0))
                                {
                                    ObjectManager.CassetteManager.DeleteCassette(oldcst.CassetteSequenceNo);
                                    oldcst = null;
                                }
                                //}
                            }

                            #region [新增卡匣到Cassette Entity]
                            cst = new Cassette()
                            {
                                LineID = port.Data.LINEID.Trim(),
                                NodeID = port.Data.NODEID.Trim(),
                                NodeNo = port.Data.NODENO.Trim(),
                                PortID = port.Data.PORTID.Trim(),
                                PortNo = port.Data.PORTNO.Trim(),
                                CassetteID = port.File.CassetteID.Trim(),
                                CassetteSequenceNo = port.File.CassetteSequenceNo,
                                LoadTime = DateTime.Now,
                            };
                            ObjectManager.CassetteManager.CreateCassette(cst);
                            #endregion

                            if (line.Data.LINETYPE == eLineType.ARRAY.CAC_MYTEK || line.Data.LINETYPE == eLineType.CELL.CCCLN)  //modify by yang  2017/5/24
                            {
                                //Invoke(eServiceName.JobService, "RecordCstHistory", new object[] { eqp, cst.CassetteSequenceNo, 
                                // modify by bruce 2015/9/16 for Array CAC line use
                                Invoke(eServiceName.JobService, "RecordCST_JOBHistory", new object[6] { eqp, cst.CassetteSequenceNo, cst.CassetteID, eJobEvent.Create, "", trxID });
                            }

                            if (line.File.HostMode == eHostMode.OFFLINE)
                            {
                                if (oldcst != null && //oldcst.Jobs.Count > 0 &&
                                    line.File.IndexOperMode == eINDEXER_OPERATION_MODE.COOL_RUN_MODE)
                                {
                                    // 使用此資料再重塞入到CST Data Map給EQ
                                    for (int i = 0; i < oldcst.Jobs.Count(); i++)
                                    {
                                        Job job = oldcst.Jobs[i];
                                        job.SetNewJobKey(int.Parse(port.File.CassetteSequenceNo), int.Parse(job.ToSlotNo));
                                        job.CassetteSequenceNo = port.File.CassetteSequenceNo;
                                        job.JobSequenceNo = job.ToSlotNo;
                                        job.SourcePortID = port.Data.PORTID;
                                        if (job.SamplingSlotFlag.Equals("0"))
                                        {
                                            // 不用抽的玻璃直接給目前的PortID
                                            job.TargetPortID = job.SourcePortID;
                                        }
                                        else
                                        {
                                            job.TargetPortID = "0";
                                        }
                                        job.FromCstID = port.File.CassetteID.Trim();
                                        job.TargetCSTID = string.Empty;
                                        job.FromSlotNo = job.ToSlotNo;
                                        job.ToSlotNo = "0";
                                        job.CurrentSlotNo = job.FromSlotNo; //add by bruce 20160412 for T2 Issue
                                        job.JobJudge = "0";
                                        job.FirstRunFlag = "0";
                                        job.LastGlassFlag = "0";
                                        //job.SamplingSlotFlag = "1"; // 使用原本的Sampling Flag, 防止有破片不抽
                                        //job.TrackingData = "0"; modify for robot control use
                                        job.TrackingData = new string('0', ObjectManager.SubJobDataManager.GetItemLenth("TrackingData"));
                                        job.InspJudgedData = "0";
                                        job.ArraySpecial.SourcePortNo = job.CfSpecial.SourcePortNo = port.Data.PORTNO;
                                        job.ArraySpecial.TargetPortNo = job.CfSpecial.TargetPortNo = "0";
                                        job.DefectCodes.Clear();
                                        job.HoldInforList.Clear();
                                        job.WriteFlag = true;

                                        job.RobotWIP.LastSendStageID = ""; //for Robot Cmd check use 2016/01/01 cc.kuang

                                        jobs.Add(job);

                                        #region Robot Create WIP   Return Failed Message and Cassette Command Failed!
                                        //Watson Modify 20151106 For 新增錯誤訊息及重大錯誤不能再下貨成功!!
                                        string returnMsg = string.Empty;
                                        object[] parameters = new object[] { eqp.Data.NODENO, job, returnMsg };
                                        bool result = (bool)Invoke(eServiceName.RobotCoreService, "CreateJobRobotWIPInfo", parameters,
                                                new Type[] { typeof(string), typeof(Job), typeof(string).MakeByRefType() });
                                        returnMsg = (string)parameters[2];

                                        //由BC決定要不要退卡匣，
                                        if ((!result) && (returnMsg != string.Empty))
                                        {
                                            err = string.Format("[EQUIPMENT={0}] PORT=[{1}] CASSETTEID=[{2}] ,ROBOT ROUTE CREATE FAILED ! REASON=[{3}]",
                                             port.Data.NODENO, port.Data.PORTID, job.FromCstID, returnMsg);
                                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                                new object[] { inputData.TrackKey, eqp.Data.LINEID, "[CassetteStatusChangeReport] " + err });
                                            throw new Exception(err);
                                        }
                                        #endregion

                                    }
                                    ObjectManager.JobManager.AddJobs(jobs);
                                    cst.CoolRunStart = oldcst.CoolRunStart;
                                    ObjectManager.CassetteManager.EnqueueSave(cst);
                                    CassetteMapDownload(eqp, port, jobs, inputData.TrackKey);
                                    ObjectManager.CassetteManager.DeleteCassette(oldcst.CassetteSequenceNo);
                                    CreateFileData(port, line, jobs);
                                }
                                else
                                {
                                    lock (port) port.File.OPI_SubCstState = eOPISubCstState.WACSTEDIT;
                                }
                            }
                            else
                            {
                                #region 上報給MES Data
                                // MES Data
                                object[] _data = new object[2]
                                {
                                    inputData.TrackKey,          /*0 TrackKey*/
                                    port
                                };

                                if (line.Data.LINETYPE == eLineType.ARRAY.CAC_MYTEK)
                                {
                                    Invoke(eServiceName.MESService, "ValidateCleanCassetteRequest", _data);
                                }
                                else if (line.Data.LINETYPE == eLineType.CELL.CCCLN)//sy add 20160704
                                {
                                    Invoke(eServiceName.MESService, "ValidateCleanCassetteRequest", _data);
                                }
                                else if (line.Data.FABTYPE == eFabType.CELL.ToString() && (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE || port.Data.PORTATTRIBUTE == keyCELLPORTAtt.BOX))
                                //else if (line.Data.FABTYPE == eFabType.CELL.ToString() & port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSECST)
                                {
                                    Invoke(eServiceName.MESService, "ValidateBoxRequest", _data);
                                }
                                else
                                {
                                    Invoke(eServiceName.MESService, "ValidateCassetteRequest", _data);
                                }

                                //if (line.Data.LINETYPE != eLineType.CELL.CBMCL)
                                //    Invoke(eServiceName.MESService, "ValidateCassetteRequest", _data);
                                //else
                                //    Invoke(eServiceName.MESService, "ValidateMaskByCarrierRequest", _data);
                                #endregion
                            }


                        }
                        break;
                    case eCassetteStatus.WAITING_FOR_START_COMMAND:
                        {
                            if (cst == null)
                            {
                                err = string.Format("[EQUIPMENT={1}] PORT=[{2}] CAN'T FIND CASSETTE INFORMATION, CST_SEQNO=[{0}] IN CASSETTE OBJECT!",
                                    port.File.CassetteSequenceNo, port.Data.NODENO, port.Data.PORTNO);
                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                    new object[] { inputData.TrackKey, eqp.Data.LINEID, "[CassetteStatusChangeReport] " + err });
                                throw new Exception(err);
                            }

                            #region [Sorter Mode Partial Full Cassette Check]
                            // 20151126 Add by Frank
                            if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.SORTER_MODE
                                && port.File.Type == ePortType.UnloadingPort
                                && port.File.PartialFullFlag == eParitalFull.PartialFull)
                            {
                                if (!port.File.JobCountInCassette.Equals(0))
                                {
                                    if (!PartialCSTCheck(inputData.TrackKey, eqp, port))
                                    {
                                        CassetteProcessCancel(eqp.Data.NODENO, port.Data.PORTNO);
                                    }
                                }
                            }
                            #endregion

                            //if (line.File.HostMode == eHostMode.REMOTE ||
                            //    (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.COOL_RUN_MODE && cst.CoolRunStart))          
                            //CELL LOCAL RUN ULD Port Auto Mapdownload,Not need OP ，qiumin20181012
                            if ((line.File.HostMode == eHostMode.REMOTE ||(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.COOL_RUN_MODE && cst.CoolRunStart))||(line.File.HostMode == eHostMode.LOCAL&&line.Data.FABTYPE == "CELL" && port.File.Type == ePortType.UnloadingPort))
                            {
                                if (port.Data.PROCESSSTARTTYPE.Equals("0"))
                                {
                                    CassetteProcessStart(port.Data.NODENO, port.Data.PORTNO, inputData.TrackKey);
                                }
                                else
                                {
                                    switch (line.Data.LINETYPE)
                                    {
                                        case eLineType.CF.FCUPK_TYPE1:
                                            if (((port.File.Type == ePortType.UnloadingPort) || (port.File.Type == ePortType.BothPort))
                                                && (eqp.File.EquipmentRunMode == "RE-CLEAN")) //UPK Line的UD在RE-CLEAN模式 不管Normal/Cool Run都是Start
                                                CassetteProcessStart(port.Data.NODENO, port.Data.PORTNO, inputData.TrackKey);
                                            else //其他是StartByCount
                                                CassetteProcessStartByCount(eqp.Data.NODENO, port.Data.PORTNO, int.Parse(port.File.StartByCount), inputData.TrackKey);
                                            break;
                                        case eLineType.CF.FCREW_TYPE1:
                                            if (port.File.Type != ePortType.LoadingPort) //Rework Line 只有在 Loading Port 是用 Start by Count 的方式下 Start.
                                                CassetteProcessStart(port.Data.NODENO, port.Data.PORTNO, inputData.TrackKey);
                                            else
                                                CassetteProcessStartByCount(eqp.Data.NODENO, port.Data.PORTNO, int.Parse(port.File.StartByCount), inputData.TrackKey);
                                            break;
                                    }
                                }
                            }
                            else // Offline及Local要由UI下命令
                            {
                                lock (port) port.File.OPI_SubCstState = eOPISubCstState.WASTART;
                                ObjectManager.PortManager.EnqueueSave(port.File);
                            }
                        }
                        break;
                    case eCassetteStatus.WAITING_FOR_PROCESSING:
                        {
                            if (line.File.HostMode == eHostMode.REMOTE || (line.File.HostMode == eHostMode.LOCAL && line.Data.FABTYPE == "CELL" && port.File.Type == ePortType.UnloadingPort))
                            {
                                // 要刪掉T9 TIMEOUT, REMOTE部份需等到 Process Start 之後再下
                                string timeoutName = string.Format("{0}_{1}_MES_ValidateCassetteReply", ObjectManager.LineManager.GetLineID(port.Data.LINEID), port.Data.PORTID);
                                if (_timerManager.IsAliveTimer(timeoutName))
                                {
                                    _timerManager.TerminateTimer(timeoutName);
                                }
                                string timeoutName1 = string.Format("{0}_MES_ValidateBoxReply", port.Data.PORTID);//FOR DANSECST by sy 20151119
                                if (_timerManager.IsAliveTimer(timeoutName1))
                                {
                                    _timerManager.TerminateTimer(timeoutName1);
                                }
                                string timeoutName2 = string.Format("{0}_{1}_MES_ValidateCleanCassetteReply", port.Data.LINEID, port.Data.PORTID);//For CST Cleaner by huangjiayin 20170523
                                if (_timerManager.IsAliveTimer(timeoutName2))
                                {
                                    _timerManager.TerminateTimer(timeoutName2);
                                }


                            }

                            if (cst == null)
                            {
                                err = string.Format("[EQUIPMENT={1}] PORT=[{2}] CAN'T FIND CASSETTE INFORMATION, CST_SEQNO=[{0}] IN CASSETTE OBJECT!",
                                    port.File.CassetteSequenceNo, port.Data.NODENO, port.Data.PORTNO);
                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                    new object[] { inputData.TrackKey, eqp.Data.LINEID, "[CassetteStatusChangeReport] " + err });
                                throw new Exception(err);
                            }
                            else
                            {
                                lock (cst)
                                {
                                    cst.StartTime = DateTime.Now;
                                    if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.COOL_RUN_MODE)
                                        cst.CoolRunStart = true;
                                }
                                ObjectManager.CassetteManager.EnqueueSave(cst);

                                line.File.LineRecipeName = cst.LineRecipeName; //2016/01/26 cc.kuang add for material mount/status report use
                            }

                            if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                            {

                                // PlanReadyStatusCheck(line, eqp, inputData.TrackKey);   //chn_old

                                if (line.File.HostMode == eHostMode.OFFLINE)
                                {
                                    PlanReadyStatusCheck(line, eqp, inputData.TrackKey);
                                }
                                else
                                {
                                    PlanReadyStatusCheckForOnline(line, eqp, inputData.TrackKey, "NEWCST");  //yang
                                }
                            }

                            #region  Sorter Mode Rule
                            if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.SORTER_MODE && port.File.Type == ePortType.LoadingPort)
                            {
                                IList<Job> sorJob = ObjectManager.JobManager.GetJobs(port.File.CassetteSequenceNo); //get all original job in cst;
                                IList<Port> tarPort = ObjectManager.PortManager.GetPorts(eqp.Data.NODEID).Where(p => p.File.Type == ePortType.UnloadingPort && p.File.Mode != ePortMode.Mismatch).ToList();
                                List<Sorter> gradeType = new List<Sorter>();

                                #region 計算出Source Cassette中的每種Job Grade之數量
                                foreach (Job j in sorJob)
                                {
                                    //2016/1/18 Add by Frank Sampling Flag ="1",才可分配Grade
                                    if (j.SamplingSlotFlag == "1")
                                    {
                                        if (gradeType.Find(x => x.SorterGrade.Equals(j.JobGrade)) != null)
                                        {
                                            if (gradeType.Find(x => x.ProductType.Equals(j.ProductType.Value)) != null)
                                            {
                                                gradeType.Find(x => x.SorterGrade.Equals(j.JobGrade) && x.ProductType.Equals(j.ProductType.Value)).GradeCount++;
                                            }
                                            else
                                            {
                                                Sorter newS = new Sorter(j.JobGrade, 1, j.ProductType.Value, int.Parse(ConstantManager["PRIORITY_JOB_GRADE"][j.JobGrade.Trim().ToString()].Value));
                                                gradeType.Add(newS);
                                            }
                                        }
                                        else
                                        {
                                            Sorter newS = new Sorter(j.JobGrade, 1, j.ProductType.Value, int.Parse(ConstantManager["PRIORITY_JOB_GRADE"][j.JobGrade.Trim().ToString()].Value));
                                            gradeType.Add(newS);
                                        }
                                    }
                                    else
                                        continue;
                                }

                                gradeType = gradeType.OrderByDescending(x => x.PriorityGrade).ToList();
                                #endregion

                                #region 檢查Target Ports是否有符合條件，可以賦予Product Type/Mapping Grade
                                if (tarPort.Count == 0)
                                {
                                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] THERE IS NO UNLOADING PORT CAN BE USED."
                                                        , inputData.Metadata.NodeNo, inputData.TrackKey));
                                }
                                else
                                {
                                    int i = 0;
                                    foreach (Port p in tarPort)
                                    {
                                        if (p.File.EnableMode == ePortEnableMode.Enabled
                                            && p.File.DownStatus == ePortDown.Normal
                                            && p.File.Mode == ePortMode.ByGrade
                                            && p.File.Status == ePortStatus.LR
                                            && p.File.CassetteStatus == eCassetteStatus.NO_CASSETTE_EXIST
                                            && p.File.MappingGrade.Equals(string.Empty))
                                        {
                                            lock (p)
                                            {
                                                p.File.MappingGrade = gradeType[i].SorterGrade;
                                                p.File.ProductType = gradeType[i].ProductType.ToString();
                                            }
                                            Invoke(eServiceName.UIService, "PortCSTStatusReport", new object[] { inputData.TrackKey, p });
                                            RecordPortHistory(trxID, eqp, p);
                                            ReportMESPortStatusChange(p, inputData);
                                            gradeType[i].ToMES = true;
                                            ObjectManager.PortManager.EnqueueSave(p.File);
                                            Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                       string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT_NO=[{2}], PORT_GRADE=[{3}], PORT_PRODUCTTYPE=[{4}].",
                                                       inputData.Metadata.NodeNo, inputData.TrackKey, p.Data.PORTNO, p.File.MappingGrade, p.File.ProductType));
                                            i++;
                                        }
                                    }
                                }
                                #endregion

                                #region 檢查2nd Source CST是否有與1st Source CST相同的Grade
                                //確認2nd Source CST內已有相同Job Grade已經再Target Port，故toMES Flag需改為True
                                foreach (Sorter s in gradeType)
                                {
                                    foreach (Port p in tarPort)
                                    {
                                        //檢查Target CST可否混不同的Product Type
                                        if (eqp.File.ProductTypeCheckMode == eEnableDisable.Enable)
                                        {
                                            if (p.File.MappingGrade.Equals(s.SorterGrade) && p.File.ProductType.Equals(s.ProductType.ToString()))
                                                s.ToMES = true;
                                        }
                                        else
                                        {
                                            if (p.File.MappingGrade.Equals(s.SorterGrade))
                                                s.ToMES = true;
                                        }
                                    }
                                }
                                #endregion

                                lock (port)
                                    port.File.SorterJobGrade = gradeType;
                                ObjectManager.PortManager.EnqueueSave(port.File);
                            }

                            #endregion

                            #region[ScrapRuleCommandForNotch]
                            if (line.Data.LINETYPE == eLineType.CELL.CCNLS || line.Data.LINETYPE == eLineType.CELL.CCNRD)
                            {
                                if (port.File.Type == ePortType.LoadingPort)
                                {
                                    string commandNo = string.Empty;
                                    if (port.Data.PORTID == "02") commandNo = "02";
                                    Job scrapRuleJob = ObjectManager.JobManager.GetJobs().FirstOrDefault(j => j.CassetteSequenceNo == port.File.CassetteSequenceNo);
                                    if (scrapRuleJob != null)
                                    {
                                        if (!string.IsNullOrEmpty(scrapRuleJob.CellSpecial.DisCardJudges))
                                            Invoke(eServiceName.CELLSpecialService, "ScrapRuleCommand", new object[] { "L2", eBitResult.ON, trxID, scrapRuleJob.CassetteSequenceNo, scrapRuleJob.CellSpecial.DisCardJudges, commandNo });
                                        else
                                        {
                                            string disCardJudges = new string('0', 32);
                                            //0:A 25:Z 23:X 不管有沒有給X 要下給機台都要有X
                                            disCardJudges = disCardJudges.Substring(0, 23) + "1" + disCardJudges.Substring(23 + 1, disCardJudges.Length - 1 - 23);
                                            Invoke(eServiceName.CELLSpecialService, "ScrapRuleCommand", new object[] { "L2", eBitResult.ON, trxID, scrapRuleJob.CassetteSequenceNo, disCardJudges, commandNo });
                                        }

                                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ScrapRuleCommand SET BIT =[{2}].",
                                        "L2", trxID, eBitResult.ON));
                                    }
                                }
                            }
                            #endregion

                        }
                        break;
                    case eCassetteStatus.IN_PROCESSING:
                        {

                            if (line.File.HostMode == eHostMode.REMOTE || (line.File.HostMode == eHostMode.LOCAL && line.Data.FABTYPE == "CELL" && port.File.Type == ePortType.UnloadingPort))
                            {
                                // 要刪掉T9 TIMEOUT, REMOTE部份需等到 Process Start 之後再下
                                string timeoutName = string.Format("{0}_{1}_MES_ValidateCassetteReply", line.Data.LINEID, port.Data.PORTID);
                                if (_timerManager.IsAliveTimer(timeoutName))
                                {
                                    _timerManager.TerminateTimer(timeoutName);
                                }
                                string timeoutName1 = string.Format("{0}_MES_ValidateBoxReply", port.Data.PORTID);//FOR DANSECST by sy 20151119
                                if (_timerManager.IsAliveTimer(timeoutName1))
                                {
                                    _timerManager.TerminateTimer(timeoutName1);
                                }
                            }

                            if (cst == null)
                            {
                                if (line.File.HostMode != eHostMode.OFFLINE)
                                {
                                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("PORT=[{0}] CSTID=[{1}] CST_STATUS=[INPROCESSING], BUT CAN'T FIND CST OBJECT, SKIP REPORT LotProcessStarted.",
                                        port.Data.PORTNO,
                                        port.File.CassetteID));
                                }
                                return;
                            }

                            if (cst.IsProcessed) return;

                            lock (cst) cst.IsProcessed = true;

                            #region Frank Add File Info Notify Command Rule 20150720
                            //只有第一次IN_PROCESSING BC需發File Info Notify Command
                            if ((port.File.Type == ePortType.LoadingPort) || (port.File.Type == ePortType.BothPort))
                            {
                                List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs().ToList<Equipment>();
                                FileInfoNotifyCommand(inputData.TrackKey, eqps, port.File.CassetteSequenceNo);
                            }
                            #endregion

                            if (line.File.HostMode != eHostMode.OFFLINE)
                            {
                                object[] _data = new object[3]
                                { 
                                    inputData.TrackKey,               /*0  TrackKey*/
                                    port,                             /*1  Port*/
                                    cst,                              /*2  Cassette*/
                                };
                                //呼叫MES方法
                                if (line.Data.FABTYPE == eFabType.CELL.ToString() && (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE || port.Data.PORTATTRIBUTE == keyCELLPORTAtt.BOX))//sy 2015 12 21 DPCassetteProcessCancel T3 目前沒有用到
                                //if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSECST)//shihyang 20151120 add 
                                {
                                    Invoke(eServiceName.MESService, "BoxProcessStarted", _data);
                                }
                                else
                                {
                                    Invoke(eServiceName.MESService, "LotProcessStarted", _data);
                                    Invoke(eServiceName.APCService, "LotProcessStart", _data);
                                }
                            }

                            if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE || line.File.LineOperMode == eMES_LINEOPERMODE.EXCHANGE)
                            {
                                IList<SLOTPLAN> currPlans;
                                string PlanID;

                                currPlans = ObjectManager.PlanManager.GetProductPlans(out PlanID);
                                if (line.File.CurrentPlanID.Trim().Length > 0 && line.File.PlanStatus == ePLAN_STATUS.READY)
                                {
                                    if ((currPlans.FirstOrDefault(slot => slot.SOURCE_CASSETTE_ID.Trim() == port.File.CassetteID.Trim()) != null) ||
                                         (currPlans.FirstOrDefault(slot => slot.TARGET_CASSETTE_ID.Trim() == port.File.CassetteID.Trim()) != null))
                                    {
                                        line.File.PlanStatus = ePLAN_STATUS.START;
                                        ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                                        if (line.File.HostMode != eHostMode.OFFLINE)
                                        {
                                            if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                                            {
                                                Invoke(eServiceName.MESService, "ChangePlanStarted", new object[] { inputData.TrackKey, line.Data.LINEID, PlanID });
                                                Thread.Sleep(1000); //move delay time 2016/05/06 cc.kuang
                                            }
                                        }
                                        // mark by box.zhai 2017/03/07 Online只有用Source CST去要Plan的Case
                                        //if (line.File.HostMode != eHostMode.OFFLINE && line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE) //check standby plan, if has any plan nulock port, request standby plan cc.kuang 2015/09/21
                                        //{
                                        //    IList<SLOTPLAN> Plans;
                                        //    string standbyPlanID;
                                        //    Plans = ObjectManager.PlanManager.GetProductPlansStandby(out standbyPlanID);
                                        //    //if (standbyPlanID.Trim().Length == 0) 
                                        //    //{
                                        //    //if (ObjectManager.PortManager.GetPorts(eqp.Data.NODEID).FirstOrDefault(
                                        //    //p => p.File.PlannedCassetteID.Trim().Length == 0) != null)
                                        //    //{
                                        //    Invoke(eServiceName.MESMessageService, "ChangePlanRequest",
                                        //    new object[] { inputData.TrackKey, line.Data.LINEID, "", PlanID });
                                        //    //}
                                        //    //}
                                        //}
                                        //Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
                                    }
                                }
                            }
                        }
                        break;
                    case eCassetteStatus.PROCESS_PAUSED:
                        {
                            //20160816 Modify by Frank Pause狀態下無需ON MPLC Interlock
                            //#region 将MPLCInterlock自动ON起来，防止CST恢复正常processing后，直接抽片，导致混ProductType
                            //eFabType fabType;
                            //Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);
                            //if (fabType == eFabType.CF && port.File.Type != ePortType.UnloadingPort)
                            //{
                            //    IDictionary<string, IList<SubBlock>> subBlocks = new Dictionary<string, IList<SubBlock>>();
                            //    subBlocks = ObjectManager.SubBlockManager.GetBlock(eqp.Data.NODENO, "P" + port.Data.PORTNO);
                            //    if (subBlocks == null || subBlocks.Count == 0)
                            //    {
                            //        break;
                            //    }
                            //    string startEq = string.Format("{0}:{1}", eqp.Data.NODENO, "P" + port.Data.PORTNO);
                            //    IList<SubBlock> list = subBlocks[startEq];
                            //    foreach (SubBlock item in list)
                            //    {
                            //        #region [先看ENABLED是否开启]
                            //        if (item.Data.ENABLED != "Y")
                            //        {
                            //            continue;
                            //        }
                            //        #endregion
                            //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            //            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] MPLCINTERLOCKNO =[{3}](ON): STARTEQUIPMENT=[{2}], CST STATUS IS PROCESS_PAUSED",
                            //            item.Data.CONTROLEQP, UtilityMethod.GetAgentTrackKey(), item.Data.STARTEQP, item.Data.INTERLOCKNO.PadLeft(2, '0')));
                            //        Invoke(eServiceName.SubBlockService, "MPLCInterlockCommand", new object[] { item.Data.CONTROLEQP, item.Data.INTERLOCKNO, eBitResult.ON });
                            //    }
                            //}
                            //#endregion
                        }
                        break;
                    case eCassetteStatus.PROCESS_COMPLETED:
                        {
                            if (ParameterManager.ContainsKey("CUTBURPORTDISABLE") && eqp.Data.NODEID.Contains("CCBUR"))
                            {
                                if (ParameterManager["CUTBURPORTDISABLE"].GetBoolean())
                                {
                                    err = string.Format("[EQUIPMENT={1}] PORT=[{2}] Ignore CCBUR PORT&CST End Message, CST_ID=[{0}]",
                                    port.File.CassetteID, port.Data.NODENO, port.Data.PORTNO);
                                    throw new Exception(err);
                                }
                            }
                            if (line.File.HostMode == eHostMode.REMOTE || (line.File.HostMode == eHostMode.LOCAL && line.Data.FABTYPE == "CELL" && port.File.Type == ePortType.UnloadingPort))
                            {
                                // 要刪掉T9 TIMEOUT, REMOTE部份需等到 Process Start 之後再下
                                string timeoutName = string.Format("{0}_{1}_MES_ValidateCassetteReply", line.Data.LINEID, port.Data.PORTID);
                                if (_timerManager.IsAliveTimer(timeoutName))
                                {
                                    _timerManager.TerminateTimer(timeoutName);
                                }
                                string timeoutName1 = string.Format("{0}_MES_ValidateBoxReply", port.Data.PORTID);//FOR DANSECST by sy 20151119
                                if (_timerManager.IsAliveTimer(timeoutName1))
                                {
                                    _timerManager.TerminateTimer(timeoutName1);
                                }
                            }

                            if (cst == null)
                            {
                                err = string.Format("[EQUIPMENT={1}] PORT=[{2}] CAN'T FIND CASSETTE INFORMATION, CST_SEQNO=[{0}] IN CASSETTE OBJECT!",
                                    port.File.CassetteSequenceNo, port.Data.NODENO, port.Data.PORTNO);
                                /* t2&t3 sync 2016/04/25 cc.kuang
                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                    new object[] { inputData.TrackKey, eqp.Data.LINEID, "[CassetteStatusChangeReport] " + err });
                                */
                                throw new Exception(err);
                            }
                            else
                            {
                                lock (cst)
                                {
                                    cst.EndTime = DateTime.Now;
                                    cst.CellBoxProcessed = eboxReport.NOReport;
                                }
                                ObjectManager.CassetteManager.EnqueueSave(cst);
                            }
                            // 20161019 sy modify APC LastGlass
                            #region [APC LastGlass]
                            if (port.File.Type == ePortType.LoadingPort)
                            {
                                string jobSeqNo = string.Empty;
                                DateTime maxTime = new DateTime();
                                foreach (Job aPCJob in ObjectManager.JobManager.GetJobs(cst.CassetteSequenceNo))
                                {
                                    if (!aPCJob.RemoveFlag && aPCJob.JobProcessStartTime > maxTime)
                                    {
                                        maxTime = aPCJob.JobProcessStartTime;
                                        jobSeqNo = aPCJob.JobSequenceNo;
                                    }
                                }
                                if (!string.IsNullOrEmpty(jobSeqNo))
                                {
                                    Job aPCLastJob = ObjectManager.JobManager.GetJob(cst.CassetteSequenceNo, jobSeqNo);
                                    if (aPCLastJob != null)
                                    {
                                        aPCLastJob.APCLastGlassFlag = true;
                                        ObjectManager.JobManager.EnqueueSave(aPCLastJob);
                                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[APC Last Glass] = [SET [Job = {0}] APC Last Glass Flag]]", aPCLastJob.JobKey));
                                    }
                                }
                            }
                            #endregion

                            GetPortJobData(inputData, port, ref jobs);

                            // 20150812 Add by Frank
                            // To do
                            // Lot To Lot AbnormalEnd時，相同Product Type的OK玻璃還沒回CST時需要Hold住。
                            #region CF Photo Line(LTOL)Hold Object
                            if (line.Data.JOBDATALINETYPE == eJobDataLineType.CF.PHOTO_BMPS || line.Data.JOBDATALINETYPE == eJobDataLineType.CF.PHOTO_GRB)
                            {
                                if (eqp.File.CSTOperationMode == eCSTOperationMode.LTOL)
                                {
                                    foreach (Job job in jobs)
                                    {
                                        if (job.JobJudge == "1")
                                        {
                                            if (job.HoldInforList.Count.Equals(0))
                                            {
                                                HoldInfo hold = new HoldInfo()
                                                {
                                                    NodeNo = eqp.Data.NODENO,
                                                    NodeID = eqp.Data.LINEID,
                                                    UnitNo = "0",
                                                    UnitID = "",
                                                    HoldReason = "HOLD_JOB",
                                                    OperatorID = "",
                                                };
                                                ObjectManager.JobManager.HoldEventRecord(job, hold);
                                            }
                                        }
                                    }
                                }
                            }
                            #endregion

                            #region CF REP Line(job judge RP && Process operation name:1570、2570、4570、6570)Hold Job
                            //20180829 add by hujunpeng
                            if (line.Data.LINENAME.Contains("FCREP"))
                            {
                                HoldInfo hold = new HoldInfo();
                                if (jobs!=null)
                                {
                                    foreach (Job job in jobs)
                                    {
                                        bool _isRP = false;
                                        if (job.MesCstBody.LOTLIST.Count > 0)
                                        {
                                            if (job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME == "1570" || job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME == "2570" || job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME == "4570" || job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME == "6570")
                                            {
                                                if (job.JobJudge == "5")
                                                {
                                                    if (job.HoldInforList.Count > 0)
                                                    {
                                                        for (int i = 0; i < job.HoldInforList.Count; i++)
                                                        {
                                                            if (job.HoldInforList[i].HoldReason.Contains("RP"))
                                                                _isRP = true;
                                                            break;
                                                        }
                                                    } 
                                                }
                                            }
                                        }
                                        if (_isRP) continue;
                                        hold.NodeID = eqp.Data.NODEID;
                                        hold.HoldReason = "Job in complete CST has RP judge";
                                        hold.OperatorID = "BCAuto";
                                        job.HoldInforList.Add(hold);
                                    }
                                }
                            }
                            #endregion

                            if (line.File.HostMode != eHostMode.OFFLINE)
                            {
                                #region Report MES
                                switch (port.File.CompletedCassetteData)
                                {
                                    case eCompletedCassetteData.NormalComplete:
                                        {
                                            #region [Report to MES]
                                            // MES Data
                                            object[] obj = new object[]
                                            {
                                                inputData.TrackKey,
                                                port,
                                                cst,
                                                jobs
                                            };
                                            object[] objForDenes = new object[]
                                            {
                                                inputData.TrackKey,
                                                line.Data.LINEID,
                                                port.Data.PORTID,
                                                cst,
                                                jobs
                                            };
                                            object[] _data = new object[3]
                                           { 
                                            inputData.TrackKey,               /*0  TrackKey*/
                                            port,                             /*1  Port*/
                                            cst,                              /*2  Cassette*/
                                           };
                                            if (line.Data.LINETYPE == eLineType.CELL.CCPCK && port.Data.PORTID == "05")//PCK 此port 特別處理 不抽片 直接退port
                                            {
                                                Invoke(eServiceName.MESService, "BoxProcessStarted", _data);
                                                System.Threading.Thread.Sleep(8000);//MES 閰波 表示不可連續上報，需要delay 6~7秒
                                                Invoke(eServiceName.MESService, "BoxProcessEnd", objForDenes);
                                            }
                                            else if (line.Data.LINETYPE == eLineType.CELL.CCOVP && port.File.Type == ePortType.LoadingPort)//sy add 20160704
                                            {
                                            }
                                            else if (line.Data.FABTYPE == eFabType.CELL.ToString() && (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE || port.Data.PORTATTRIBUTE == keyCELLPORTAtt.BOX))//sy 2015 12 21 DPCassetteProcessCancel T3 目前沒有用到
                                            //if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSECST)//shihyang 20151120 add 
                                            {
                                                Invoke(eServiceName.MESService, "BoxProcessEnd", objForDenes);
                                            }
                                            else if (line.Data.LINETYPE == eLineType.CELL.CCCLN)//sy add 20160704
                                            {
                                                //cst.ReasonCode = "";
                                                string reason = "";//20170112 sy modify  by MES SPEC 1.58
                                                if (port.File.DirectionFlag == eDirection.Reverse)
                                                {
                                                    reason += "Direction Reverse";
                                                }
                                                if (port.File.DistortionFlag == eDistortion.Distortion)
                                                {
                                                    if (!string.IsNullOrEmpty(reason)) reason += ",";
                                                    reason += "Distortion";
                                                }
                                                if (port.File.GlassExist == eGlassExist.Exist)
                                                {
                                                    if (!string.IsNullOrEmpty(reason)) reason += ",";
                                                    reason += "Glass Exist";
                                                }
                                                if (!string.IsNullOrEmpty(reason))
                                                {
                                                    cst.ReasonCode = reason;
                                                    cst.ReasonText = reason;
                                                }
                                                if (port.File.Type == ePortType.UnloadingPort)
                                                {
                                                    Invoke(eServiceName.MESService, "CassetteCleanEnd", new object[] { inputData.TrackKey, line.Data.LINEID, port.Data.PORTID, cst.CassetteID, cst.ReasonCode });
                                                }
                                            }
                                            else if (line.Data.LINETYPE == eLineType.ARRAY.CAC_MYTEK)   // modify by bruce 2015/9/16 for Array CAC line use
                                            {
                                                cst.ReasonCode = "";
                                                if (port.File.DirectionFlag == eDirection.Reverse) cst.ReasonCode = "3:Direction Reverse";
                                                if (port.File.DistortionFlag == eDistortion.Distortion) cst.ReasonCode = "2:Distortion";
                                                if (port.File.GlassExist == eGlassExist.Exist) cst.ReasonCode = "1:Glass Exist"; //modify by yang 20161206

                                                if (port.File.Type == ePortType.UnloadingPort)
                                                {
                                                    Invoke(eServiceName.MESService, "CassetteCleanEnd", new object[] { inputData.TrackKey, line.Data.LINEID, port.Data.PORTID, cst.CassetteID, cst.ReasonCode });
                                                }
                                            }
                                            else
                                            {
                                                Invoke(eServiceName.MESService, "LotProcessEnd", obj);
                                                //上报OEE LotProcessEnd 20150127 tom
                                                Invoke(eServiceName.OEEService, "LotProcessEnd", new object[] { inputData.TrackKey, line, port, jobs });
                                                //James at 20150528 上报APC LotProcessEnd
                                                Invoke(eServiceName.APCService, "LotProcessEnd", obj);
                                            }

                                            //if (line.Data.LINETYPE != eLineType.CELL.CBMCL)
                                            //{
                                            //    Invoke(eServiceName.MESService, "LotProcessEnd", obj);
                                            //    //上报OEE LotProcessEnd 20150127 tom
                                            //    Invoke(eServiceName.OEEService, "LotProcessEnd", new object[] { inputData.TrackKey, line, port, jobs});
                                            //    //James at 20150528 上报APC LotProcessEnd
                                            //    Invoke(eServiceName.APCService, "LotProcessEnd", obj);
                                            //}
                                            //else
                                            //    Invoke(eServiceName.MESService, "MaskProcessEnd_MCL", obj);

                                            #endregion
                                        }
                                        break;
                                    case eCompletedCassetteData.BCForcedToAbort:
                                    case eCompletedCassetteData.EQAutoAbort:
                                    case eCompletedCassetteData.OperatorForcedToAbort:
                                        {
                                            GetReasonCodeAndText(port, cst, out reasonCode, out reasonText);
                                            //if (line.Data.FABTYPE == eFabType.CELL.ToString() && (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE || port.Data.PORTATTRIBUTE == keyCELLPORTAtt.BOX)) return;//sy 2015 12 21 DPCassetteProcessCancel T3 目前沒有用到
                                            if (line.Data.FABTYPE == eFabType.CELL.ToString() && (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE || port.Data.PORTATTRIBUTE == keyCELLPORTAtt.BOX))
                                            {
                                                #region [Report to MES BoxProcessAborted]
                                                // MES Data
                                                object[] objForDenesAbort = new object[]
                                            {
                                                inputData.TrackKey,
                                                port,
                                                reasonCode,
                                                reasonText
                                            };

                                                Invoke(eServiceName.MESService, "BoxProcessAborted", objForDenesAbort);
                                                #endregion

                                                System.Threading.Thread.Sleep(50);

                                                #region [Report to MES BoxProcessAbnormalEnd]

                                                object[] objForDenes = new object[]
                                            {
                                                inputData.TrackKey,
                                                line.Data.LINEID,
                                                port.Data.PORTID,
                                                cst,
                                                jobs
                                            };

                                                Invoke(eServiceName.MESService, "BoxProcessAbnormalEnd", objForDenes);

                                                #endregion
                                            }
                                            else
                                            {
                                                #region [Report to MES LotProcessAborted]
                                                // MES Data
                                                object[] obj = new object[]
                                            {
                                                inputData.TrackKey,
                                                port,
                                                reasonCode,
                                                reasonText
                                            };

                                                Invoke(eServiceName.MESService, "LotProcessAborted", obj);
                                                #endregion

                                                System.Threading.Thread.Sleep(50);

                                                #region [Report to MES LotProcessAbnormalEnd]
                                                // MES Data
                                                obj = new object[]
                                            {
                                                inputData.TrackKey,
                                                port,
                                                cst ,
                                                jobs
                                            };

                                                if (line.Data.LINETYPE != eLineType.CELL.CBMCL)
                                                    Invoke(eServiceName.MESService, "LotProcessAbnormalEnd", obj);
                                                else
                                                    Invoke(eServiceName.MESService, "MaskProcessEnd_MCL", obj);

                                                #endregion
                                            }
                                        }
                                        break;
                                    case eCompletedCassetteData.BCForcedToCancel:
                                    case eCompletedCassetteData.EQAutoCancel:
                                    case eCompletedCassetteData.OperatorForcedToCancel:
                                        {
                                            #region 刪掉 T9 DataRequest Timeout
                                            string t9TimeoutName = string.Format("{0}_{1}_MES_ValidateCassetteReply", ObjectManager.LineManager.GetLineID(port.Data.LINEID),
                                                ObjectManager.PortManager.GetPortID(port.Data.NODENO, port.Data.PORTID));
                                            if (_timerManager.IsAliveTimer(t9TimeoutName))
                                            {
                                                _timerManager.TerminateTimer(t9TimeoutName);
                                            }
                                            string timeoutName1 = string.Format("{0}_MES_ValidateBoxReply", port.Data.PORTID);//FOR DANSECST by sy 20151119
                                            if (_timerManager.IsAliveTimer(timeoutName1))
                                            {
                                                _timerManager.TerminateTimer(timeoutName1);
                                            }
                                            #endregion

                                            #region [CELL CLN reason code]
                                            string reason = "";//20170112 sy modify  by MES SPEC 1.58
                                            if (line.Data.LINETYPE == eLineType.CELL.CCCLN)
                                            {
                                                //cst.ReasonCode = "";

                                                if (port.File.DirectionFlag == eDirection.Reverse)
                                                {
                                                    reason += "Direction Reverse";
                                                }
                                                if (port.File.DistortionFlag == eDistortion.Distortion)
                                                {
                                                    if (!string.IsNullOrEmpty(reason)) reason += ",";
                                                    reason += "Distortion";
                                                }
                                                if (port.File.GlassExist == eGlassExist.Exist)
                                                {
                                                    if (!string.IsNullOrEmpty(reason)) reason += ",";
                                                    reason += "Glass Exist";
                                                }

                                                cst.ReasonCode = reason;
                                                cst.ReasonText = reason;
                                            }
                                            #endregion
                                            reasonCode = string.Empty;
                                            reasonText = string.Empty;
                                            if (string.IsNullOrEmpty(reason))
                                                GetReasonCodeAndText(port, cst, out reasonCode, out reasonText);
                                            else
                                                reasonCode = reasonText = reason;


                                            #region [Report to MES]
                                            // MES Data
                                            object[] obj = new object[]
                                        {
                                            inputData.TrackKey,
                                            port,
                                            reasonCode,
                                            reasonText
                                        };
                                            if (line.Data.FABTYPE == eFabType.CELL.ToString() && (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE || port.Data.PORTATTRIBUTE == keyCELLPORTAtt.BOX))//sy 2015 12 21 DPCassetteProcessCancel T3 目前沒有用到
                                            //if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSECST)//shihyang 20151120 add 
                                            {
                                                object[] objForDenes = new object[]
                                                     {
                                                         inputData.TrackKey,
                                                         line.Data.LINEID,
                                                         port.Data.PORTID,
                                                         port.File.CassetteID,
                                                         reasonCode,
                                                         reasonText
                                                        };
                                                Invoke(eServiceName.MESService, "BoxProcessCanceled", objForDenes);
                                            }
                                            else if (line.Data.LINETYPE != eLineType.CELL.CBMCL)
                                                Invoke(eServiceName.MESService, "LotProcessCanceled", obj);
                                            else
                                                Invoke(eServiceName.MESService, "MaskProcessEndAbort", new object[] { inputData.TrackKey, port, jobs, reasonCode, reasonText });
                                            #endregion
                                        }
                                        break;
                                }
                                #endregion
                            }

                            if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.COOL_RUN_MODE)
                            {
                                // Loading Port會有不用抽的玻璃, 所以不用判斷
                                //if ((port.File.Type == ePortType.UnloadingPort) || (port.File.Type == ePortType.BothPort))
                                //{

                                lock (cst) cst.Jobs = jobs;
                                ObjectManager.CassetteManager.EnqueueSave(cst);
                                //}
                            }

                            int delayTime = ParameterManager["FILEMOVEDELAYTIME"].GetInteger();//20161115 sy add  
                            if (!jobs.Count().Equals(0))
                            {
                                //ObjectManager.JobManager.MoveJobs(line, jobs, delayTime);//20161031 sy add Move job, Delete wip
                                ////ObjectManager.JobManager.DeleteJobs(jobs);                                                             
                                ////Watson Add 20150416 For Save History.
                                //ObjectManager.JobManager.RecordJobsHistory(jobs, eqp.Data.NODEID, eqp.Data.NODENO, port.Data.PORTNO, eJobEvent.Delete_CST_Complete.ToString(), inputData.TrackKey);
                                ////DeleteFileData(line, jobs);
                                ////Jun Modify 20150518 多傳入Port Object在裡面判斷邏輯
                                //MoveFileData(line, jobs, port, delayTime);//Move File to  temp Path =“D:\UnicomLog\FileData\”

                                ////if (line.Data.LINETYPE == eLineType.CELL.CBPMT ||
                                ////    line.Data.LINETYPE == eLineType.CELL.CBPRM ||
                                ////    line.Data.LINETYPE == eLineType.CELL.CBSOR_1 ||
                                ////    line.Data.LINETYPE == eLineType.CELL.CBSOR_2)
                                ////    ObjectManager.RobotJobManager.DeleteRobotJobsByBCSJobs(eqp.Data.NODENO, jobs);
                                //20180302 CCUBR的Port经常乱报，BUR退Port不砍Job
                                if (!eqp.Data.NODEID.Contains("CCBUR")) deleteJobs = jobs; //20161226 sy add 將Delete 移到最後面 避免影響 port 屬性改完才存 History
                            }



                            //Jun Add 20150311 Cell ODF Line Check CGMO Flag by Group Index
                            CGMOFlagCheckUpdate(line);

                            if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1 ||
                                line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE2 ||
                                line.Data.LINETYPE == eLineType.CF.FCREW_TYPE1)
                            {
                                lock (port)
                                {
                                    port.File.ProductQuantity = string.Empty;
                                    port.File.PlannedQuantity = string.Empty;
                                    port.File.PlannedSourcePart = string.Empty;
                                    port.File.StartByCount = string.Empty;
                                }
                                ObjectManager.PortManager.EnqueueSave(port.File);
                            }

                            //for changer plan                             
                            if ((line.Data.FABTYPE == eFabType.ARRAY.ToString() || line.Data.FABTYPE == eFabType.CF.ToString()) &&
                                    (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE || line.File.LineOperMode == eMES_LINEOPERMODE.EXCHANGE) &&
                                    line.File.HostMode != eHostMode.OFFLINE)
                            {
                                #region Array Changer Mode
                                bool bCurrPlanChange = true;
                                bool bCurrPlanCST = false;
                                bool bStandbyPlanCST = false;
                                string PlanID = string.Empty;
                                IList<SLOTPLAN> currPlans;
                                IList<SLOTPLAN> standbyPlans;
                                List<string> lsSource = new List<string>(); //quit cst list
                                List<string> lsTarget = new List<string>(); //quit cst list

                                if (port.File.CompletedCassetteData != eCompletedCassetteData.NormalComplete)
                                {
                                    port.File.PlannedCassetteID = string.Empty;

                                    //檢查Current source CST & target CST List for Cancel Plan
                                    currPlans = ObjectManager.PlanManager.GetProductPlans(out PlanID);
                                    var lsSourceCassetteCurr = from slot in currPlans group slot by slot.SOURCE_CASSETTE_ID into g select g.First().SOURCE_CASSETTE_ID;
                                    var lsTargetCassetteCurr = from slot in currPlans group slot by slot.TARGET_CASSETTE_ID into g select g.First().TARGET_CASSETTE_ID;
                                    bCurrPlanCST = lsSourceCassetteCurr.Contains(port.File.CassetteID.Trim());
                                    if (!bCurrPlanCST)
                                        bCurrPlanCST = lsTargetCassetteCurr.Contains(port.File.CassetteID.Trim());
                                    if (bCurrPlanCST && line.File.CurrentPlanID.Trim().Length > 0 && (line.File.PlanStatus == ePLAN_STATUS.REQUEST || line.File.PlanStatus == ePLAN_STATUS.READY || line.File.PlanStatus == ePLAN_STATUS.START))
                                    {
                                        if (line.File.PlanStatus == ePLAN_STATUS.START)
                                            bCurrPlanChange = false;

                                        if (bCurrPlanChange)
                                        {
                                            lsSource.AddRange(lsSourceCassetteCurr.ToList<string>());
                                            lsTarget.AddRange(lsTargetCassetteCurr.ToList<string>());
                                            line.File.PlanStatus = ePLAN_STATUS.CANCEL;
                                            ObjectManager.PlanManager.RemoveChangePlan();
                                            ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                                            //report to MES
                                            if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                                                Invoke(eServiceName.MESService, "ChangePlanCanceled", new object[6] { inputData.TrackKey, line.Data.LINEID, PlanID, "0", port.File.CassetteID + " CASSETTE has be Quit", port.File.CassetteID });
                                        }
                                        else
                                        {
                                            line.File.PlanStatus = ePLAN_STATUS.ABORTING;
                                            ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                                        }

                                        Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
                                        lsSource.Remove(port.File.CassetteID); //complete cst not cancel again
                                        lsTarget.Remove(port.File.CassetteID);
                                    }
                                    else
                                    {
                                        bCurrPlanChange = false;
                                    }

                                    //檢查Standby source CST & target CST List for Cancel Plan
                                    standbyPlans = ObjectManager.PlanManager.GetProductPlansStandby(out PlanID);
                                    var lsSourceCassetteStandby = from slot in standbyPlans group slot by slot.SOURCE_CASSETTE_ID into g select g.First().SOURCE_CASSETTE_ID;
                                    var lsTargetCassetteStandby = from slot in standbyPlans group slot by slot.TARGET_CASSETTE_ID into g select g.First().TARGET_CASSETTE_ID;

                                    if (port.File.Type == ePortType.LoadingPort)
                                    {
                                        bStandbyPlanCST = lsSourceCassetteStandby.Contains(port.File.CassetteID.Trim());
                                        if (bStandbyPlanCST)
                                        {
                                            foreach (string cstid in lsSourceCassetteStandby)
                                            {
                                                if (lsSourceCassetteCurr.Contains(cstid.Trim()))
                                                    continue;
                                                else
                                                    lsSource.Add(cstid);
                                            }
                                        }
                                    }
                                    else if (port.File.Type == ePortType.UnloadingPort)
                                    {
                                        bStandbyPlanCST = lsTargetCassetteStandby.Contains(port.File.CassetteID.Trim());
                                        if (bStandbyPlanCST)
                                        {
                                            foreach (string cstid in lsTarget)
                                            {
                                                if (lsTargetCassetteCurr.Contains(cstid.Trim()))
                                                    continue;
                                                else
                                                    lsTarget.Add(cstid);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        bStandbyPlanCST = false;
                                    }
                                    lsSource.Remove(port.File.CassetteID); //complete cst not cancel again
                                    lsTarget.Remove(port.File.CassetteID);

                                    if (bStandbyPlanCST)
                                    {
                                        ObjectManager.PlanManager.RemoveChangePlanStandby();
                                        //report to MES
                                        if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                                            Invoke(eServiceName.MESService, "ChangePlanCanceled", new object[6] { inputData.TrackKey, line.Data.LINEID, PlanID, "0", port.File.CassetteID + " CASSETTE has be Quit", port.File.CassetteID });
                                    }

                                    if (bCurrPlanChange)
                                    {
                                        if (!bStandbyPlanCST && standbyPlans.Count > 0) //standby plan change to curr plan, and bonding to port
                                        {
                                            IList<SLOTPLAN> plans = new List<SLOTPLAN>();
                                            line.File.CurrentPlanID = PlanID;
                                            line.File.PlanStatus = ePLAN_STATUS.REQUEST;
                                            ObjectManager.LineManager.EnqueueSave(line.File);//add by box.zhai
                                            ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                                            foreach (SLOTPLAN pl in standbyPlans)
                                            {
                                                plans.Add(pl);
                                            }
                                            ObjectManager.PlanManager.AddOnlinePlans(PlanID, plans);
                                            Invoke(eServiceName.PortService, "CassetteBondToPort", new object[4] { line, standbyPlans, true, inputData.TrackKey });
                                            Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
                                            ObjectManager.PlanManager.RemoveChangePlanStandby();
                                        }
                                        else
                                        {
                                            if (lsSource.Count > 0 || lsTarget.Count > 0)
                                            {
                                                Invoke(eServiceName.PortService, "CassettePlanCancel", new object[4] { lsSource, lsTarget, 1, inputData.TrackKey });
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (lsSource.Count > 0 || lsTarget.Count > 0)
                                        {
                                            Invoke(eServiceName.PortService, "CassettePlanCancel", new object[4] { lsSource, lsTarget, 1, inputData.TrackKey });
                                        }
                                    }
                                }
                                #endregion
                            }

                            #region Sorter Mode Clear MappingGrade/ProductType of Unloader and clear Sorter List of Loader
                            //20151126 Add by Frank
                            if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.SORTER_MODE)
                            {
                                if (port.File.Type == ePortType.UnloadingPort)
                                {
                                    SorterUnloaderCheck(eqp, port, inputData.TrackKey); // 20160609 Add By Frank

                                    lock (port)
                                    {
                                        port.File.MappingGrade = string.Empty;
                                        port.File.ProductType = string.Empty;
                                    }
                                }
                                if (port.File.Type == ePortType.LoadingPort)
                                {
                                    lock (port)
                                        port.File.SorterJobGrade.Clear();
                                }
                            }
                            #endregion
                        }

                        if (line.Data.LINETYPE == eLineType.ARRAY.CVD_AKT || line.Data.LINETYPE == eLineType.ARRAY.CVD_ULVAC)
                        {
                            if (port.File.CompletedCassetteData != eCompletedCassetteData.NormalComplete)    //add by bruce 2016/1/25 CVD line 非正常退Port 需再下Cassette Unload 給Indexer
                            {
                                Invoke(eServiceName.CassetteService, "CassetteUnload", new object[] { eqp.Data.NODENO, port.Data.PORTNO });
                            }
                            else
                            {
                                if (line.File.HostMode == eHostMode.OFFLINE)
                                    Invoke(eServiceName.CassetteService, "CassetteUnload", new object[] { eqp.Data.NODENO, port.Data.PORTNO });
                            }
                        }
                        //20160330 sy add Update port.File.ProductType // 20160330 modify all shop
                        if (port.File.Type == ePortType.UnloadingPort)
                        {
                            port.File.ProductType = "0";
                        }

                        break;
                    case eCassetteStatus.CASSETTE_REMAP:
                        {
                            if (port.File.Type == ePortType.BothPort)
                                lock (port) port.File.OPI_SubCstState = eOPISubCstState.WAREMAPEDIT;
                        }
                        break;
                    case eCassetteStatus.IN_ABORTING:
                        {
                            // 20161019 sy modify APC LastGlass
                            #region [APC LastGlass]
                            string jobSeqNo = string.Empty;
                            DateTime maxTime = new DateTime();
                            foreach (Job aPCJob in ObjectManager.JobManager.GetJobs(cst.CassetteSequenceNo))
                            {
                                if (!aPCJob.RemoveFlag && aPCJob.JobProcessStartTime > maxTime)
                                {
                                    maxTime = aPCJob.JobProcessStartTime;
                                    jobSeqNo = aPCJob.JobSequenceNo;
                                }
                            }
                            if (!string.IsNullOrEmpty(jobSeqNo))
                            {
                                Job aPCLastJob = ObjectManager.JobManager.GetJob(cst.CassetteSequenceNo, jobSeqNo);
                                if (aPCLastJob != null)
                                {
                                    aPCLastJob.APCLastGlassFlag = true;
                                    ObjectManager.JobManager.EnqueueSave(aPCLastJob);
                                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                            string.Format("[APC Last Glass] = [SET [Job = {0}] APC Last Glass Flag]]", aPCLastJob.JobKey));
                                }
                            }
                            #endregion
                            ;
                        }
                        break;
                    default:

                        break;
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void GetPortJobData(Trx inputData, Port port, ref IList<Job> jobs)
        {
            try
            {
                int slotNo = 0;
                for (int i = 0; i < inputData.EventGroups[0].Events[0].Items.Count; i += 2)
                {
                    slotNo++;
                    // Cassette Sequence No 為0時, 表示沒有資料, 找下一片
                    if (inputData.EventGroups[0].Events[0].Items[i].Value.Equals("0")) continue;
                    // Job Sequence No 為0時, 表示沒有資料, 找下一片
                    if (inputData.EventGroups[0].Events[0].Items[i + 1].Value.Equals("0")) continue;

                    Job job = ObjectManager.JobManager.GetJob(inputData.EventGroups[0].Events[0].Items[i].Value, inputData.EventGroups[0].Events[0].Items[i + 1].Value);

                    if (job == null)
                    {
                        Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] PORTNO=[{1}] STATUS=[{2}] JOB IS NULL , CAS_SEQ_NO=[{3}], JOB_SEQ_NO=[{4}] .",
                            inputData.Metadata.NodeNo, port.Data.PORTNO, port.File.CassetteStatus.ToString(), inputData.EventGroups[0].Events[0].Items[i].Value,
                            inputData.EventGroups[0].Events[0].Items[i + 1].Value));
                        continue;
                    }

                    lock (job) job.ToSlotNo = slotNo.ToString();
                    jobs.Add(job);
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void CGMOFlagCheckUpdate(Line line)
        {
            try
            {
                if (line.Data.LINETYPE != eLineType.CELL.CBODF)
                    return;

                IList<Job> jobs = ObjectManager.JobManager.GetJobs();
                //Jun Modify 20150325 預防Jobs取到Null
                if (jobs == null) return;

                if (jobs.Count == 0)
                {
                    string[] keys = line.File.CGMOFlagCheck.Keys.ToArray();
                    foreach (string key in keys)
                    {
                        line.File.CGMOFlagCheck[key] = false;
                    }
                }
                else
                {
                    string[] keys = line.File.CGMOFlagCheck.Keys.ToArray();
                    foreach (string key in keys)
                    {
                        jobs = ObjectManager.JobManager.GetJobs().Where(s => s.GroupIndex == key && s.RemoveFlag == false).ToList();

                        //Jun Modify 20150325 預防Jobs取到Null
                        if (jobs == null)
                            line.File.CGMOFlagCheck[key] = false;
                        else
                            if (jobs.Count == 0)
                                line.File.CGMOFlagCheck[key] = false;
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void GetReasonCodeAndText(Port port, Cassette cst, out string reasonCode, out string reasonText)
        {
            reasonCode = reasonText = string.Empty;

            if (cst != null)
            {
                if (!string.IsNullOrEmpty(cst.ReasonCode))
                {
                    reasonCode = cst.ReasonCode;
                    reasonText = cst.ReasonText;
                    // 將資料有問題的更正
                    if (port.File.Type == ePortType.UnloadingPort)
                    {
                        reasonCode = reasonCode.Substring(0, 1).Equals("U") ? reasonCode : "U" + reasonCode.Substring(1);
                    }
                    else
                    {
                        reasonCode = reasonCode.Substring(0, 1).Equals("L") ? reasonCode : "L" + reasonCode.Substring(1);
                    }
                    if (string.IsNullOrEmpty(reasonText))
                    {
                        switch (reasonCode)
                        {
                            case MES_ReasonCode.Loader_EQ_Cancel:
                                reasonText = MES_ReasonText.Loader_EQ_Cancel;
                                break;
                            case MES_ReasonCode.Unloader_EQ_Cancel:
                                reasonText = MES_ReasonText.Unloader_EQ_Cancel;
                                break;
                            case MES_ReasonCode.Loader_BC_Cancel_From_BC_Client:
                                reasonText = MES_ReasonText.Loader_BC_Cancel_From_BC_Client;
                                break;
                            case MES_ReasonCode.Unloader_BC_Cancel_From_BC_Client:
                                reasonText = MES_ReasonText.Unloader_BC_Cancel_From_BC_Client;
                                break;
                            case MES_ReasonCode.Loader_OP_Cancel:
                                reasonText = MES_ReasonText.Loader_OP_Cancel;
                                break;
                            case MES_ReasonCode.Unloader_OP_Cancel:
                                reasonText = MES_ReasonText.Unloader_OP_Cancel;
                                break;
                            case MES_ReasonCode.Loader_EQ_Abort:
                                reasonText = MES_ReasonText.Loader_EQ_Abort;
                                break;
                            case MES_ReasonCode.Unloader_EQ_Abort:
                                reasonText = MES_ReasonText.Unloader_EQ_Abort;
                                break;
                            case MES_ReasonCode.Loader_BC_Abort:
                                reasonText = MES_ReasonText.Loader_BC_Abort;
                                break;
                            case MES_ReasonCode.Unloader_BC_Abort:
                                reasonText = MES_ReasonText.Unloader_BC_Abort;
                                break;
                            case MES_ReasonCode.Loader_OP_Abort:
                                reasonText = MES_ReasonText.Loader_OP_Abort;
                                break;
                            case MES_ReasonCode.Unloader_OP_Abort:
                                reasonText = MES_ReasonText.Unloader_OP_Abort;
                                break;
                        }
                    }
                    return;
                }
            }

            switch (port.File.CompletedCassetteData)
            {
                case eCompletedCassetteData.EQAutoCancel:
                    reasonCode = port.File.Type == ePortType.UnloadingPort ? MES_ReasonCode.Unloader_EQ_Cancel :
                        MES_ReasonCode.Loader_EQ_Cancel;
                    //TODO: 等CSOT整理好對應內容再填//sy add 201600402 先依照ReasonCode 說明填入
                    reasonText = port.File.Type == ePortType.UnloadingPort ? MES_ReasonText.Unloader_EQ_Cancel :
                        MES_ReasonText.Loader_EQ_Cancel;
                    break;
                case eCompletedCassetteData.BCForcedToCancel:
                    reasonCode = port.File.Type == ePortType.UnloadingPort ? MES_ReasonCode.Unloader_BC_Cancel_From_BC_Client :
                        MES_ReasonCode.Loader_BC_Cancel_From_BC_Client;
                    //TODO: 等CSOT整理好對應內容再填//sy add 201600402 先依照ReasonCode 說明填入
                    reasonText = port.File.Type == ePortType.UnloadingPort ? MES_ReasonText.Unloader_BC_Cancel_From_BC_Client :
                        MES_ReasonText.Loader_BC_Cancel_From_BC_Client;
                    break;
                case eCompletedCassetteData.OperatorForcedToCancel:
                    reasonCode = port.File.Type == ePortType.UnloadingPort ? MES_ReasonCode.Unloader_OP_Cancel :
                        MES_ReasonCode.Loader_OP_Cancel;
                    //TODO: 等CSOT整理好對應內容再填//sy add 201600402 先依照ReasonCode 說明填入
                    reasonText = port.File.Type == ePortType.UnloadingPort ? MES_ReasonText.Unloader_OP_Cancel :
                        MES_ReasonText.Loader_OP_Cancel;
                    break;
                case eCompletedCassetteData.EQAutoAbort:
                    reasonCode = port.File.Type == ePortType.UnloadingPort ? MES_ReasonCode.Unloader_EQ_Abort :
                        MES_ReasonCode.Loader_EQ_Abort;
                    //TODO: 等CSOT整理好對應內容再填//sy add 201600402 先依照ReasonCode 說明填入
                    reasonText = port.File.Type == ePortType.UnloadingPort ? MES_ReasonText.Unloader_EQ_Abort :
                        MES_ReasonText.Loader_EQ_Abort;
                    break;
                case eCompletedCassetteData.BCForcedToAbort:
                    reasonCode = port.File.Type == ePortType.UnloadingPort ? MES_ReasonCode.Unloader_BC_Abort :
                        MES_ReasonCode.Loader_BC_Abort;
                    //TODO: 等CSOT整理好對應內容再填//sy add 201600402 先依照ReasonCode 說明填入
                    reasonText = port.File.Type == ePortType.UnloadingPort ? MES_ReasonText.Unloader_BC_Abort :
                        MES_ReasonText.Loader_BC_Abort;
                    break;
                case eCompletedCassetteData.OperatorForcedToAbort:
                    reasonCode = port.File.Type == ePortType.UnloadingPort ? MES_ReasonCode.Unloader_OP_Abort :
                        MES_ReasonCode.Loader_OP_Abort;
                    //TODO: 等CSOT整理好對應內容再填//sy add 201600402 先依照ReasonCode 說明填入
                    reasonText = port.File.Type == ePortType.UnloadingPort ? MES_ReasonText.Unloader_OP_Abort :
                        MES_ReasonText.Loader_OP_Abort;
                    break;
            }
            if (port.File.DirectionFlag == eDirection.Reverse)
            {
                reasonCode = "3";
            }
            else if (port.File.DistortionFlag == eDistortion.Distortion)
            {
                reasonCode = "2";
            }
            else if (port.File.GlassExist == eGlassExist.Exist)
            {
                reasonCode = "1";
            }
        }

        private bool PartialCSTCheck(string TrackKey, Equipment eqp, Port port)
        {
            try
            {
                IList<Job> jobs = ObjectManager.JobManager.GetJobs(port.File.CassetteSequenceNo);
                IList<Port> souPorts = ObjectManager.PortManager.GetPorts(eqp.Data.NODEID).Where(p => p.File.Type == ePortType.LoadingPort).ToList();

                string msg = string.Empty;
                foreach (Job job in jobs)
                {
                    if (eqp.File.ProductTypeCheckMode == eEnableDisable.Enable)
                    {
                        if (job.ProductType.Value.ToString() != port.File.ProductType)
                        {
                            msg = string.Format("EQUIPMENT=[{0}] [BCS <- EQP][{1}] PORTNO=[{2}], PRODUCT_TYPE=[{3}] OF CASSETTE IS DIFFERENT WITH PRODUCT_TYPE=[{4}] OF PORT."
                                , eqp.Data.NODENO, TrackKey, port.Data.NODENO, job.ProductType.Value.ToString(), port.File.ProductType);

                            Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("{0} ", msg));

                            foreach (Port p in souPorts)
                            {
                                for (int i = 0; i < p.File.SorterJobGrade.Count; i++)
                                {
                                    if (port.File.MappingGrade == p.File.SorterJobGrade[i].SorterGrade
                                        && p.File.SorterJobGrade[i].ToMES == true)
                                        p.File.SorterJobGrade[i].ToMES = false;
                                }
                            }
                            return false;
                        }
                    }
                    if (job.JobGrade != port.File.MappingGrade)
                    {
                        msg = string.Format("EQUIPMENT=[{0}] [BCS <- EQP][{1}] PORTNO=[{2}], JOB_GRADE=[{3}] OF CASSETTE IS DIFFERENT WITH JOB_GRADE=[{4}] OF PORT."
                                , eqp.Data.NODENO, TrackKey, port.Data.NODENO, job.ProductType.Value.ToString(), port.File.ProductType);

                        Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("{0} ", msg));

                        foreach (Port p in souPorts)
                        {
                            for (int i = 0; i < p.File.SorterJobGrade.Count; i++)
                            {
                                if (port.File.MappingGrade == p.File.SorterJobGrade[i].SorterGrade
                                    && p.File.SorterJobGrade[i].ToMES == true)
                                    p.File.SorterJobGrade[i].ToMES = false;
                            }
                        }
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        #endregion ===================

        private void RecordCassetteHistory(string trxID, Equipment eqp, Port port, Cassette cst, string cstControlCmd, string cmdRetcode, string operID)
        {
            try
            {
                // Save DB
                CASSETTEHISTORY his = new CASSETTEHISTORY()
                {
                    UPDATETIME = DateTime.Now,
                    CASSETTEID = port.File.CassetteID,
                    CASSETTESEQNO = int.Parse(port.File.CassetteSequenceNo),
                    CASSETTESTATUS = port.File.CassetteStatus.ToString(),
                    NODEID = eqp.Data.NODEID,
                    JOBCOUNT = int.Parse(port.File.JobCountInCassette),
                    PORTID = port.Data.PORTID,
                    JOBEXISTENCE = port.File.JobExistenceSlot,
                    CASSETTECONTROLCOMMAND = cstControlCmd,
                    COMMANDRETURNCODE = cmdRetcode,
                    OPERATORID = operID,
                    COMPLETEDCASSETTEDATA = port.File.CompletedCassetteData.ToString(),
                    LOADINGCASSETTETYPE = port.File.LoadingCassetteType.ToString(),
                    QTIMEFLAG = (int)port.File.QTimeFlag,
                    PARTIALFULLFLAG = (int)port.File.PartialFullFlag,
                    CASSETTESETCODE = port.File.CassetteSetCode,
                    TRANSACTIONID = trxID
                };

                if (cst != null)
                {
                    his.LOADTIME = cst.LoadTime;
                    his.PROCESSSTARTTIME = cst.StartTime;
                    his.PROCESSENDTIME = cst.EndTime;
                }
                ObjectManager.CassetteManager.InsertDB(his);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #region [Cassette Control Command]

        public void CassetteProcessStart(string eqpNo, string portNo, string trackKey)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM CST_SEQNO=[{2}].", eqp.Data.NODENO, portNo, port.File.CassetteSequenceNo);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Start\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Start\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Start\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", eqpNo, portNo);
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK) outputData.EventGroups[0].IsDisable = true;

                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessStart).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = trackKey;
                SendPLCData(outputData);

                lock (cst) cst.CassetteControlCommand = eCstControlCmd.ProcessStart;
                ObjectManager.CassetteManager.EnqueueSave(cst);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, CstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessStart).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] PORT=[{3}] CSTID=[{4}] \"Cassette Process Start\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessStart.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void CassetteProcessStart_UI(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                //20141219 cy modify:Get Port無法by Equipment number，所以改成給Port ID來找
                //port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, msg.BODY.PORTNO);
                port = ObjectManager.PortManager.GetPort(msg.BODY.PORTID);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", msg.BODY.PORTNO, msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM CST_SEQNO=[{2}].",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteSequenceNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Start\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Start\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Start\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", eqp.Data.NODENO, msg.BODY.PORTNO);
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK) outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessStart).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = msg.HEADER.TRANSACTIONID;// UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);
                lock (cst) cst.CassetteControlCommand = eCstControlCmd.ProcessStart;
                ObjectManager.CassetteManager.EnqueueSave(cst);

                string timeoutName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, msg.BODY.PORTNO, CstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessStart).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Process Start\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessStart.ToString(), string.Empty, msg.BODY.OPERATORID);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteProcessStartByCount(string eqpNo, string portNo, int count, string trackKey)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM CST_SEQNO=[{2}].", eqp.Data.NODENO, portNo, port.File.CassetteSequenceNo);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Start By Count\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Start By Count\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Start By Count\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", eqpNo, portNo);

                #region CassetteControlCommandForUPK Special Function
                //20160127 Add by Frank
                if (Workbench.LineType == eLineType.CF.FCUPK_TYPE1 && eqp.Data.NODEATTRIBUTE == "UPK")
                    trxName = string.Format("{0}_Port#{1}CassetteControlCommandForUPK", port.Data.NODENO, port.Data.PORTNO);
                #endregion

                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK) outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessStartByCount).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[2].Value = count.ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = trackKey;// UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);
                lock (cst) cst.CassetteControlCommand = eCstControlCmd.ProcessStartByCount;
                ObjectManager.CassetteManager.EnqueueSave(cst);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, CstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessStartByCount).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Process Start By Count\" command JOB_EXISTENCE=[{5}] JOB_COUNT=[{6}], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT), count));

                // Add by Kasim 20150420
                string CASSETTECONTROLCOMMAND = eCstControlCmd.ProcessStartByCount.ToString() + " [" + count + "]";
                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, CASSETTECONTROLCOMMAND, string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void CassetteProcessStartByCount_UI(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, msg.BODY.PORTNO);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", msg.BODY.PORTNO, msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM CST_SEQNO=[{2}].",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteSequenceNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Start By Count\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Start By Count\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Start By Count\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", eqp.Data.NODENO, msg.BODY.PORTNO);

                #region CassetteControlCommandForUPK Special Function
                //20160127 Add by Frank
                if (Workbench.LineType == eLineType.CF.FCUPK_TYPE1 && eqp.Data.NODEATTRIBUTE == "UPK")
                    trxName = string.Format("{0}_Port#{1}CassetteControlCommandForUPK", port.Data.NODENO, port.Data.PORTNO);
                #endregion

                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK) outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessStartByCount).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[2].Value = msg.BODY.PROCESSCOUNT;
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = msg.HEADER.TRANSACTIONID;// UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);
                lock (cst) cst.CassetteControlCommand = eCstControlCmd.ProcessStartByCount;
                ObjectManager.CassetteManager.EnqueueSave(cst);

                string timeoutName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, msg.BODY.PORTNO, CstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessStartByCount).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Process Start By Count\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[{6}], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT), msg.BODY.PROCESSCOUNT));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessStartByCount.ToString(), string.Empty, msg.BODY.OPERATORID);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteProcessPause(string eqpNo, string portNo)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM CST_SEQNO=[{2}].", eqp.Data.NODENO, portNo, port.File.CassetteSequenceNo);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Pause\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Pause\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //add by bruce 2015/11/10 In Aborting 要能退Port
                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.IN_ABORTING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Pause\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", eqpNo, portNo);

                #region CassetteControlCommandForUPK Special Function
                //20160127 Add by Frank
                if (Workbench.LineType == eLineType.CF.FCUPK_TYPE1 && eqp.Data.NODEATTRIBUTE == "UPK")
                    trxName = string.Format("{0}_Port#{1}CassetteControlCommandForUPK", port.Data.NODENO, port.Data.PORTNO);
                #endregion

                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK) outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessPause).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);
                lock (cst) cst.CassetteControlCommand = eCstControlCmd.ProcessPause;
                ObjectManager.CassetteManager.EnqueueSave(cst);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, CstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessPause).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] PORT=[{3}] CSTID=[{4}] \"Cassette Process Pause\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessPause.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void CassetteProcessPause_UI(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, msg.BODY.PORTNO);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", msg.BODY.PORTNO, msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM CST_SEQNO=[{2}].",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteSequenceNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Pause\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Pause\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //add by bruce 2015/11/10 In Aborting 要能下Pause
                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.IN_ABORTING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Pause\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                #endregion

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", eqp.Data.NODENO, msg.BODY.PORTNO);

                #region CassetteControlCommandForUPK Special Function
                //20160127 Add by Frank
                if (Workbench.LineType == eLineType.CF.FCUPK_TYPE1 && eqp.Data.NODEATTRIBUTE == "UPK")
                    trxName = string.Format("{0}_Port#{1}CassetteControlCommandForUPK", port.Data.NODENO, port.Data.PORTNO);
                #endregion

                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK) outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessPause).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);
                lock (cst) cst.CassetteControlCommand = eCstControlCmd.ProcessPause;
                ObjectManager.CassetteManager.EnqueueSave(cst);

                string timeoutName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, msg.BODY.PORTNO, CstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessPause).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Process Pause\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessPause.ToString(), string.Empty, msg.BODY.OPERATORID);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteProcessResume(string eqpNo, string portNo)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM CST_SEQNO=[{2}].", eqp.Data.NODENO, portNo, port.File.CassetteSequenceNo);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Resume\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Resume\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //add by bruce 2015/11/10 In Aborting 要能下Resume
                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.IN_ABORTING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND &&
                    port.File.CassetteStatus != eCassetteStatus.PROCESS_PAUSED)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Resume\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", eqpNo, portNo);

                #region CassetteControlCommandForUPK Special Function
                //20160127 Add by Frank
                if (Workbench.LineType == eLineType.CF.FCUPK_TYPE1 && eqp.Data.NODEATTRIBUTE == "UPK")
                    trxName = string.Format("{0}_Port#{1}CassetteControlCommandForUPK", port.Data.NODENO, port.Data.PORTNO);
                #endregion

                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK) outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessResume).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);
                lock (cst) cst.CassetteControlCommand = eCstControlCmd.ProcessResume;
                ObjectManager.CassetteManager.EnqueueSave(cst);

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, CstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessResume).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] PORT=[{3}] CSTID=[{4}] \"Cassette Process Resume\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessResume.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void CassetteProcessResume_UI(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, msg.BODY.PORTNO);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", msg.BODY.PORTNO, msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM CST_SEQNO=[{2}].",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteSequenceNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Resume\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Resume\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //add by bruce 2015/11/10 In Aborting 要能下Resume
                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.IN_ABORTING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND &&
                    port.File.CassetteStatus != eCassetteStatus.PROCESS_PAUSED)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Resume\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", eqp.Data.NODENO, msg.BODY.PORTNO);

                #region CassetteControlCommandForUPK Special Function
                //20160127 Add by Frank
                if (Workbench.LineType == eLineType.CF.FCUPK_TYPE1 && eqp.Data.NODEATTRIBUTE == "UPK")
                    trxName = string.Format("{0}_Port#{1}CassetteControlCommandForUPK", port.Data.NODENO, port.Data.PORTNO);
                #endregion

                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK) outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessResume).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();

                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);
                lock (cst) cst.CassetteControlCommand = eCstControlCmd.ProcessResume;
                ObjectManager.CassetteManager.EnqueueSave(cst);

                string timeoutName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, msg.BODY.PORTNO, CstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessResume).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Process Resume\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessResume.ToString(), string.Empty, msg.BODY.OPERATORID);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteProcessAbort(string eqpNo, string portNo)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM CST_SEQNO=[{2}].", eqp.Data.NODENO, portNo, port.File.CassetteSequenceNo);
                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);

                    //Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                    //    string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    //throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Abort\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Abort\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //add by bruce 2015/11/10 In Aborting 要能下Abort
                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.IN_ABORTING &&
                    port.File.CassetteStatus != eCassetteStatus.CASSETTE_REMAP &&
                    !(port.File.CassetteStatus == eCassetteStatus.PROCESS_PAUSED &&
                    (cst != null && cst.IsProcessed)))
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Abort\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", eqpNo, portNo);

                #region CassetteControlCommandForUPK Special Function
                //20160127 Add by Frank
                if (Workbench.LineType == eLineType.CF.FCUPK_TYPE1 && eqp.Data.NODEATTRIBUTE == "UPK")
                    trxName = string.Format("{0}_Port#{1}CassetteControlCommandForUPK", port.Data.NODENO, port.Data.PORTNO);
                #endregion

                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK) outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessAbort).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);

                if (cst != null)
                {
                    lock (cst) cst.CassetteControlCommand = eCstControlCmd.ProcessAbort;
                    ObjectManager.CassetteManager.EnqueueSave(cst);
                }

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, CstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessAbort).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] PORT=[{3}] CSTID=[{4}] \"Cassette Process Abort\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessAbort.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void CassetteProcessAbort_UI(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, msg.BODY.PORTNO);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", msg.BODY.PORTNO, msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM CST_SEQNO=[{2}].",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteSequenceNo);
                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    //Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                    //    string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    //throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Abort\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Abort\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //add by bruce 2015/11/10 In Aborting 要能下Abort
                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.IN_ABORTING &&
                    port.File.CassetteStatus != eCassetteStatus.CASSETTE_REMAP && //add 2016/04/06
                    !(port.File.CassetteStatus == eCassetteStatus.PROCESS_PAUSED &&
                    (cst != null && cst.IsProcessed)))
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Abort\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", eqp.Data.NODENO, msg.BODY.PORTNO);

                #region CassetteControlCommandForUPK Special Function
                //20160127 Add by Frank
                if (Workbench.LineType == eLineType.CF.FCUPK_TYPE1 && eqp.Data.NODEATTRIBUTE == "UPK")
                    trxName = string.Format("{0}_Port#{1}CassetteControlCommandForUPK", port.Data.NODENO, port.Data.PORTNO);
                #endregion

                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK) outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessAbort).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = msg.HEADER.TRANSACTIONID;
                SendPLCData(outputData);

                if (cst != null)
                {
                    lock (cst)
                    {
                        cst.CassetteControlCommand = eCstControlCmd.ProcessAbort;
                        cst.ReasonCode = port.File.Type == ePortType.UnloadingPort ? MES_ReasonCode.Unloader_BC_Abort :
                            MES_ReasonCode.Loader_BC_Abort;
                    }
                    ObjectManager.CassetteManager.EnqueueSave(cst);
                }

                string timeoutName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, msg.BODY.PORTNO, CstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessAbort).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Process Abort\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessAbort.ToString(), string.Empty, msg.BODY.OPERATORID);

                #region 找最後一片
                if (port.File.Type == ePortType.UnloadingPort) return;
                IList<Job> jobs = ObjectManager.JobManager.GetJobs(port.File.CassetteSequenceNo.ToString());
                if (jobs == null || jobs.Count() == 0) return;

                // 1. 找此CST的資料是否有LastGlassFlag, 如果有的話就不用下資料
                if (jobs.Any(j => j.LastGlassFlag.Equals("1"))) return;

                // 2. 卡匣內的玻璃是否有Sampling Flag為On的狀態, 但還沒有抽的玻璃
                List<Job> jobs2 = (from j in jobs
                                   where j.RemoveFlag == false && j.CreateTime != j.JobProcessStartTime
                                   orderby j.JobProcessStartTime descending
                                   select j).ToList<Job>();

                if (jobs2.Count() == 0) return;

                Invoke(eServiceName.JobService, "SetLastGlassCommand", new object[] { msg.HEADER.TRANSACTIONID, "L2", jobs2.First() });
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteProcessAborting_UI(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, msg.BODY.PORTNO);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", msg.BODY.PORTNO, msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM CST_SEQNO=[{2}].",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteSequenceNo);
                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Abort\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Abort\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING &&
                    port.File.CassetteStatus != eCassetteStatus.CASSETTE_REMAP && //add 2016/04/06 cc.kuang
                    !(port.File.CassetteStatus == eCassetteStatus.PROCESS_PAUSED &&
                    (cst != null && cst.IsProcessed)))
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Aborting\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { msg.HEADER.TRANSACTIONID, eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", eqp.Data.NODENO, msg.BODY.PORTNO);
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK) outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessAborting).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = msg.HEADER.TRANSACTIONID;
                SendPLCData(outputData);

                string timeoutName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, msg.BODY.PORTNO, CstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessAbort).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Process Abort\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessAborting.ToString(), string.Empty, msg.BODY.OPERATORID);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteProcessCancel(string eqpNo, string portNo)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                //port = ObjectManager.PortManager.GetPort(eqp.Data.LINEID, eqpNo, portNo);
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM CST_SEQNO=[{2}].", eqp.Data.NODENO, portNo, port.File.CassetteSequenceNo);
                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);

                    //Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                    //    string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    //throw new Exception(err); modify for t2,t3 sync 2016/04/14 cc.kuang
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Cancel\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Cancel\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_CASSETTE_DATA &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND &&
                    port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Cancel\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", eqpNo, portNo);

                #region CassetteControlCommandForUPK Special Function
                //20160127 Add by Frank
                if (Workbench.LineType == eLineType.CF.FCUPK_TYPE1 && eqp.Data.NODEATTRIBUTE == "UPK")
                    trxName = string.Format("{0}_Port#{1}CassetteControlCommandForUPK", port.Data.NODENO, port.Data.PORTNO);
                #endregion

                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK) outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessCancel).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);
                if(port.Data.LINEID.Contains("TCPHL"))
                {
                Equipment eqp1 = ObjectManager.EquipmentManager.GetEQP("L4");
                lock (eqp1.File)
                {
                    eqp1.File.IsReveive = false;
                    eqp1.File.R2REQParameterDownloadRetrunCode = string.Empty;
                    eqp1.File.R2REQParameterDownloadDT = DateTime.Now;
                    eqp1.File.R2RRecipeReportReplyReturnCode = string.Empty;
                    eqp1.File.R2RRecipeReportStartDT = DateTime.Now;
                }
                ObjectManager.EquipmentManager.EnqueueSave(eqp1.File);
                }
                if (cst != null)
                {
                    lock (cst) cst.CassetteControlCommand = eCstControlCmd.ProcessCancel;
                    ObjectManager.CassetteManager.EnqueueSave(cst);
                }

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, CstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessCancel).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Process Cancel\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessCancel.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void CassetteUnload(string eqpNo, string portNo)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                //port = ObjectManager.PortManager.GetPort(eqp.Data.LINEID, eqpNo, portNo);
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM CST_SEQNO=[{2}].", eqp.Data.NODENO, portNo, port.File.CassetteSequenceNo);
                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);

                    //Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                    //    string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    //throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Unload\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Unload\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.PROCESS_COMPLETED)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Unload\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", eqpNo, portNo);
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK) outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[0].Value = ((int)eCstControlCmd.Unload).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);

                if (cst != null)
                {
                    lock (cst) cst.CassetteControlCommand = eCstControlCmd.Unload;
                    ObjectManager.CassetteManager.EnqueueSave(cst);
                }

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, CstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.Unload).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Unload\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.Unload.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void CassetteDoubleRun(string eqpNo, string portNo)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                //port = ObjectManager.PortManager.GetPort(eqp.Data.LINEID, eqpNo, portNo);
                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, eqpNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM CST_SEQNO=[{2}].", eqp.Data.NODENO, portNo, port.File.CassetteSequenceNo);
                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);

                    //Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                    //    string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    //throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Double Run\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Double Run\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.PROCESS_COMPLETED)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Double Run\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", eqpNo, portNo);
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK) outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[0].Value = ((int)eCstControlCmd.DoubleRun).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);

                if (cst != null)
                {
                    lock (cst) cst.CassetteControlCommand = eCstControlCmd.DoubleRun;
                    ObjectManager.CassetteManager.EnqueueSave(cst);
                }

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, CstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.DoubleRun).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Double Run\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.DoubleRun.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        public void CassetteProcessCancel_UI(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, msg.BODY.PORTNO);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", msg.BODY.PORTNO, msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM CST_SEQNO=[{2}].",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteSequenceNo);
                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);

                    //Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                    //    string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    //throw new Exception(err); modify for t2,t3 sync 2016/04/14 cc.kuang (if no cst data, can quit cst also
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Cancel\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Cancel\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                // Add by Kasim 20150508
                if (port.File.Status == ePortStatus.LC &&
                    port.File.CassetteStatus == eCassetteStatus.NO_CASSETTE_EXIST &&
                    !string.IsNullOrEmpty(port.File.CassetteID))
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] PORT_STATUS=[{2}]({3}) CSTID=[{4}] CST_STATUS=[{5}]({6}), BC DOWNLOAD \"Cassette Process Cancel\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, (int)port.File.Status, port.File.Status.ToString(), port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                }
                else
                {
                    if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_CASSETTE_DATA &&
                        port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_START_COMMAND &&
                        port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_PROCESSING)
                    {
                        err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Cancel\" COMMAND!",
                            eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                        return;
                    }
                }
                #endregion

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", eqp.Data.NODENO, msg.BODY.PORTNO);

                #region CassetteControlCommandForUPK Special Function
                //20160127 Add by Frank
                if (Workbench.LineType == eLineType.CF.FCUPK_TYPE1 && eqp.Data.NODEATTRIBUTE == "UPK")
                    trxName = string.Format("{0}_Port#{1}CassetteControlCommandForUPK", port.Data.NODENO, port.Data.PORTNO);
                #endregion

                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK) outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessCancel).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);

                if (cst != null)
                {
                    lock (cst)
                    {
                        cst.CassetteControlCommand = eCstControlCmd.ProcessCancel;
                        cst.ReasonCode = port.File.Type == ePortType.UnloadingPort ? MES_ReasonCode.Unloader_BC_Cancel_From_BC_Client :
                            MES_ReasonCode.Loader_BC_Cancel_From_BC_Client;
                    }
                    ObjectManager.CassetteManager.EnqueueSave(cst);
                }

                string timeoutName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, msg.BODY.PORTNO, CstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessCancel).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Process Cancel\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessCancel.ToString(), string.Empty, msg.BODY.OPERATORID);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteReload_UI(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Line line; Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null)
                {
                    err = string.Format("CAN'T FIND LINE_INFORMATION=[{0}] IN LINEENTITY!", eqp.Data.LINEID);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, msg.BODY.PORTNO);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", msg.BODY.PORTNO, msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                #endregion

                #region Data and Status Check
                // 2015.2.5 不管在什麼Host Mode都可以下此命令
                //if (line.File.HostMode != eHostMode.OFFLINE)
                //{
                //    err = string.Format("[EQUIPMENT={0}] Port=[{1}] CSTID=[{2}] HOST_MODE=[{3}], CAN NOT DOWNLOAD \"Cassette Reload\" COMMAND!",
                //        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, line.File.HostMode.ToString());
                //    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                //        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                //    return;
                //}

                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Reload\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.UR)
                {
                    err = string.Format("EQUIPMENT=[{0}] Port=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Reload\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, "[CassetteReload] " + err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //if (port.File.CassetteStatus != eCassetteStatus.PROCESS_COMPLETED)
                //{
                //    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Reload\" COMMAND!",
                //        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                //    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                //        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                //    return;
                //}

                #endregion

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", eqp.Data.NODENO, msg.BODY.PORTNO);

                #region CassetteControlCommandForUPK Special Function
                //20160127 Add by Frank
                if (Workbench.LineType == eLineType.CF.FCUPK_TYPE1 && eqp.Data.NODEATTRIBUTE == "UPK")
                    trxName = string.Format("{0}_Port#{1}CassetteControlCommandForUPK", port.Data.NODENO, port.Data.PORTNO);
                #endregion

                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK) outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[0].Value = ((int)eCstControlCmd.Reload).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);

                string timeoutName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, msg.BODY.PORTNO, CstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.Reload).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Reload\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, null, eCstControlCmd.Reload.ToString(), string.Empty, msg.BODY.OPERATORID);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteLoad_UI(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Line line; Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null)
                {
                    err = string.Format("CAN'T FIND LINE_INFORMATION=[{0}] IN LINEENTITY!", eqp.Data.LINEID);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, msg.BODY.PORTNO);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", msg.BODY.PORTNO, msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // 2015.2.5 不管在什麼Host Mode都可以下此命令
                //if (line.File.HostMode != eHostMode.OFFLINE)
                //{
                //    err = string.Format("[EQUIPMENT={0}] Port=[{1}] CSTID=[{2}] HOST_MODE=[{3}], CAN NOT DOWNLOAD \"Cassette Load\" COMMAND!",
                //        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, line.File.HostMode.ToString());
                //    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                //        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                //    return;
                //}

                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Load\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.UC && port.File.Status != ePortStatus.LR)
                {
                    err = string.Format("EQUIPMENT=[{0}] Port=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Load\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, "[CassetteReload] " + err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //if (port.File.CassetteStatus != eCassetteStatus.NO_CASSETTE_EXIST)
                //{
                //    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Load\" COMMAND!",
                //        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                //    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                //        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                //    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                //    return;
                //}

                #endregion

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", eqp.Data.NODENO, msg.BODY.PORTNO);

                #region CassetteControlCommandForUPK Special Function
                //20160127 Add by Frank
                if (Workbench.LineType == eLineType.CF.FCUPK_TYPE1 && eqp.Data.NODEATTRIBUTE == "UPK")
                    trxName = string.Format("{0}_Port#{1}CassetteControlCommandForUPK", port.Data.NODENO, port.Data.PORTNO);
                #endregion

                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK) outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[0].Value = ((int)eCstControlCmd.Load).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);

                string timeoutName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, msg.BODY.PORTNO, CstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.Load).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Load\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, null, eCstControlCmd.Load.ToString(), string.Empty, msg.BODY.OPERATORID);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteReMap_UI(CassetteCommandRequest msg)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Line line; Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(msg.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null)
                {
                    err = string.Format("CAN'T FIND LINE_INFORMATION=[{0}] IN LINEENTITY!", eqp.Data.LINEID);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, msg.BODY.PORTNO);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", msg.BODY.PORTNO, msg.BODY.EQUIPMENTNO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM CST_SEQNO=[{2}].",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteSequenceNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // 對MES 為Remote不能下此Command
                if (line.File.HostMode == eHostMode.REMOTE)
                {
                    err = string.Format("[EQUIPMENT={0}] Port=[{1}] CSTID=[{2}] HOST_MODE=[{3}], CAN NOT DOWNLOAD \"Cassette Re-Map\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, line.File.HostMode.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Re-Map\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                // Both Port才能下此Command
                if (port.File.Type != ePortType.BothPort)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1} CSTID=[{2}] PORT_TYPE=[{3}]({4}) IS NOT BOTH PORT, CAN NOT DOWNLOAD \"Cassette Re-Map\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.Type, port.File.Type.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, "[CassetteReMap] " + err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] Port=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Reload\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, "[CassetteReload] " + err });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Reload\" COMMAND!",
                        eqp.Data.NODENO, msg.BODY.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }
                #endregion

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", eqp.Data.NODENO, msg.BODY.PORTNO);
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK) outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[0].Value = ((int)eCstControlCmd.ReMap).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);
                cst.CassetteControlCommand = eCstControlCmd.ReMap;
                ObjectManager.CassetteManager.EnqueueSave(cst);

                string timeoutName = string.Format("{0}_{1}_{2}", eqp.Data.NODENO, msg.BODY.PORTNO, CstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ReMap).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] Port=[{3}] CSTID=[{4}] \"Cassette Re-Map\" COMMAND JOB_EXISTENCE=[{5}] JOB_COUNT=[0], SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID, new string('0', port.Data.MAXCOUNT)));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ReMap.ToString(), string.Empty, msg.BODY.OPERATORID);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteProcessEnd(string eqpNo, string portNo)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Equipment eqp; Port port;

                eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo);
                    throw new Exception(err);
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portNo);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT=[{0}] IN EQUIPMENT=[{1}]!", portNo, eqpNo);
                    throw new Exception(err);
                }

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM CST_SEQNO=[{2}].", eqp.Data.NODENO, portNo, port.File.CassetteSequenceNo);
                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Process Abort\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID);
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Process Abort\" COMMAND!",
                        eqp.Data.NODENO, portNo, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING)
                {
                    return;
                }
                #endregion

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", eqpNo, portNo);
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK) outputData.EventGroups[0].IsDisable = true;
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[0].Value = ((int)eCstControlCmd.ProcessEnd).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);

                if (cst != null)
                {
                    lock (cst) cst.CassetteControlCommand = eCstControlCmd.ProcessEnd;
                    ObjectManager.CassetteManager.EnqueueSave(cst);
                }

                string timeoutName = string.Format("{0}_{1}_{2}", eqpNo, portNo, CstControlCommandTimeout);
                if (_timerManager.IsAliveTimer(timeoutName))
                {
                    _timerManager.TerminateTimer(timeoutName);
                }

                _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
                    new System.Timers.ElapsedEventHandler(CassetteControlCommandTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.ProcessEnd).ToString());

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] PORT=[{3}] CSTID=[{4}] \"Cassette Process End\" COMMAND, SET BIT=[ON].",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.CassetteID));

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.ProcessEnd.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 创建File Data  20150301 tom
        /// </summary>
        /// <param name="port"></param>
        /// <param name="line"></param>
        /// <param name="jobs"></param>
        public void CreateFileData(Port port, Line line, IList<Job> jobs)
        {

            try
            {
                #region Ftp File
                FileFormatManager fileFormateManage = GetObject("FileFormatManager") as FileFormatManager;

                if (port.File.Type != ePortType.UnloadingPort)
                {
                    string subPath = string.Format(@"{0}\{1}", ServerName, port.File.CassetteSequenceNo);
                    for (int k = 0; k < jobs.Count(); k++)
                    {
                        if (line.Data.FABTYPE != eFabType.CELL.ToString())
                            fileFormateManage.CreateFormatFile("ACShop", subPath, jobs[k], true);
                        else
                        {
                            switch (line.Data.LINETYPE)
                            {
                                //case eLineType.CELL.CCPIL:
                                //case eLineType.CELL.CCODF:
                                //case eLineType.CELL.CCPDR:
                                //case eLineType.CELL.CCTAM:
                                //case eLineType.CELL.CCPTH:
                                //case eLineType.CELL.CCGAP:
                                //    fileFormateManage.CreateFormatFile("CELLShopFEOL", subPath, jobs[k], true);
                                //    break;
                                default://T3 使用default 集中管理
                                    Invoke(eServiceName.CELLSpecialService, "CreateFtpFile_CELL", new object[] { line, subPath, jobs[k], port });
                                    //fileFormateManage.CreateFormatFile("CELLShopBEOL", subPath, jobs[k], true);
                                    break;
                            }
                        }
                    }
                    LogInfo(MethodBase.GetCurrentMethod().Name, string.Format("Create File Data save to Path [{0}].", subPath));
                }
                #endregion
            }
            catch (System.Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        /// <summary>
        /// delete file Data 20150302  Tom
        /// </summary>
        /// <param name="line"></param>
        /// <param name="jobs"></param>
        public void DeleteFileData(Line line, IList<Job> jobs)
        {
            try
            {
                if (jobs != null)
                {
                    FileFormatManager fileFormateManage = GetObject("FileFormatManager") as FileFormatManager;

                    foreach (Job job in jobs)
                    {
                        string subPath = string.Format(@"{0}\{1}", DateTime.Now.ToString("yyyyMMdd"), job.CassetteSequenceNo);
                        string fileName = "";
                        if (line.Data.FABTYPE != eFabType.CELL.ToString())
                        {
                            fileName = job.GlassChipMaskBlockID + ".dat";
                            fileFormateManage.DeleteFormatFile("ACShop", subPath, fileName);
                        }
                        else
                        {
                            switch (line.Data.LINETYPE)
                            {
                                #region T2 USE
                                //case eLineType.CELL.CBPIL:
                                case eLineType.CELL.CBODF:
                                case eLineType.CELL.CBHVA:
                                case eLineType.CELL.CBPMT:
                                case eLineType.CELL.CBGAP:
                                    fileName = job.GlassChipMaskBlockID + ".dat";
                                    fileFormateManage.DeleteFormatFile("CELLShopFEOL", subPath, fileName);
                                    break;

                                case eLineType.CELL.CBLOI:
                                    fileName = job.GlassChipMaskBlockID + "_JPS.dat";
                                    fileFormateManage.DeleteFormatFile("CCLineJPS", subPath, fileName);
                                    break;
                                case eLineType.CELL.CBCUT_1:
                                case eLineType.CELL.CBCUT_2:
                                case eLineType.CELL.CBCUT_3:
                                    fileFormateManage.DeleteFormatFile("CCLineJPS", subPath, fileName);
                                    fileFormateManage.DeleteFormatFile("CELLShopBEOL", subPath, fileName);
                                    break;
                                #endregion
                                //#region T3 USE
                                //case eLineType.CELL.CCPIL: 
                                //    fileName = job.GlassChipMaskBlockID + ".dat";
                                //    fileFormateManage.DeleteFormatFile("CELLShopFEOL", subPath, fileName);
                                //    break;
                                //#endregion
                                default://T3 使用default 集中管理 sy
                                    if (ServerName.Contains("CCCUT100") || ServerName.Contains("CCCUT200") || ServerName.Contains("CCCUT300") || ServerName.Contains("CCCUT400") ||
                                        ServerName.Contains("CCCUT500") || ServerName.Contains("CCCUT600") || ServerName.Contains("CCCUT700") || ServerName.Contains("CCCUT800") || 
                                        ServerName.Contains("CCCUT900") || ServerName.Contains("CCCUTA00") || ServerName.Contains("CCCUTB00") || ServerName.Contains("CCCUTK00") ||
                                        ServerName.Contains("CCCUTC00") || ServerName.Contains("CCCUTD00") || ServerName.Contains("CCCUTE00") || ServerName.Contains("CCCUTF00") ||
                                        ServerName.Contains("CCCUTG00") || ServerName.Contains("CCCUTH00") || ServerName.Contains("CCCUTJ00") || ServerName.Contains("CCCUTL00") ||
                                        ServerName.Contains("CCCUTM00") || ServerName.Contains("CCCUTN00") || ServerName.Contains("CCCUTP00") || ServerName.Contains("CCNRD")||ServerName.Contains("CCNLS"))
                                    {
                                        System.Threading.Thread.Sleep(200);//Add By Yangzhenteng For CUT/NRD Job Data Delete;
                                    }
                                    else
                                    { }
                                    Invoke(eServiceName.CELLSpecialService, "DeleteFtpFile_CELL", new object[] { line, subPath, job });
                                    //fileName = job.GlassChipMaskBlockID + ".dat";
                                    //fileFormateManage.DeleteFormatFile("CELLShopBEOL", subPath, fileName);
                                    break;
                            }
                        }
                        LogInfo(MethodBase.GetCurrentMethod().Name, string.Format("Delete FileData  to Path [{0}] fileName [{1}].", subPath, fileName));
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        public void MoveFileData(Line line, IList<Job> jobs, Port port, int delayTime)
        {
            try
            {
                if (jobs != null)
                {
                    FileFormatManager fileFormateManage = GetObject("FileFormatManager") as FileFormatManager;

                    foreach (Job job in jobs)
                    {
                        string sourceSubPath = string.Format(@"{0}\{1}", line.Data.LINEID, job.CassetteSequenceNo);
                        string descSubPath = string.Format(@"{0}\FILEDATA\{1}\{2}", line.Data.LINEID, DateTime.Now.ToString("yyyyMMdd"), job.CassetteSequenceNo);
                        string descPath = Path.Combine(FileDataTempPath, descSubPath);
                        string fileName = "";
                        string fileNameJPS = "";
                        if (line.Data.FABTYPE != eFabType.CELL.ToString())
                        {
                            fileName = job.GlassChipMaskBlockID + ".dat";
                            fileFormateManage.MoveFormatFile("ACShop", sourceSubPath, descPath, fileName);
                        }
                        else
                        {
                            switch (line.Data.LINETYPE)
                            {
                                #region T2 USE
                                case eLineType.CELL.CBPIL:
                                case eLineType.CELL.CBODF:
                                case eLineType.CELL.CBHVA:
                                case eLineType.CELL.CBPMT:
                                case eLineType.CELL.CBGAP:
                                case eLineType.CELL.CBUVA:
                                case eLineType.CELL.CBMCL:
                                case eLineType.CELL.CBATS:
                                    fileName = job.GlassChipMaskBlockID + ".dat";

                                    fileFormateManage.MoveFormatFile("CELLShopFEOL", sourceSubPath, descPath, fileName);
                                    break;

                                case eLineType.CELL.CBLOI:
                                    fileName = job.GlassChipMaskBlockID + ".dat";
                                    fileNameJPS = job.GlassChipMaskBlockID + "_JPS.dat";

                                    fileFormateManage.MoveFormatFile("CELLShopBEOL", sourceSubPath, descPath, fileName);
                                    fileFormateManage.MoveFormatFile("CCLineJPS", sourceSubPath, descPath, fileNameJPS);

                                    break;
                                case eLineType.CELL.CBCUT_1: //Cut 上Port 的时候不产生File Data  20150313 Tom
                                case eLineType.CELL.CBCUT_2:
                                case eLineType.CELL.CBCUT_3:
                                    if (port.Data.PORTID == "01" || port.Data.PORTID == "02" ||
                                        port.Data.PORTID == "C01" || port.Data.PORTID == "C02")
                                    {
                                        //break;
                                    }
                                    else
                                    {
                                        fileName = job.GlassChipMaskBlockID + ".dat";
                                        fileNameJPS = job.GlassChipMaskBlockID + "_JPS.dat";

                                        fileFormateManage.MoveFormatFile("CELLShopBEOL", sourceSubPath, descPath, fileName);
                                        fileFormateManage.MoveFormatFile("CCLineJPS", sourceSubPath, descPath, fileNameJPS);
                                    }
                                    break;
                                #endregion

                                //#region T3 USE
                                //case eLineType.CELL.CCPIL:
                                //    fileName = job.GlassChipMaskBlockID + ".dat";

                                //    fileFormateManage.MoveFormatFile("CELLShopFEOL", sourceSubPath, descPath, fileName);
                                //    break;
                                //#endregion
                                default://T3 使用default 集中管理 sy
                                    if (int.Parse(job.CassetteSequenceNo) >= 50000)
                                    {
                                        string sourceFile = string.Format("D:\\FILEDATA\\{0}\\{1}\\", line.Data.SERVERNAME, job.CassetteSequenceNo);
                                        string targetPath = string.Format("D:\\UnicomLog\\{0}\\FILEDATA\\", line.Data.SERVERNAME);
                                        MoveJobFiledat(sourceFile, targetPath, job, delayTime);
                                    }
                                    else
                                    {
                                        Invoke(eServiceName.CELLSpecialService, "MoveFtpFile_CELL", new object[] { line, sourceSubPath, descPath, job, port });
                                    }
                                    fileName = job.GlassChipMaskBlockID; //20161219 sy modify

                                    //fileFormateManage.MoveFormatFile("CELLShopBEOL", sourceSubPath, descPath, fileName);
                                    break;
                            }
                        }
                        LogInfo(MethodBase.GetCurrentMethod().Name, string.Format("Move FileData  to Path [{0}] fileName [{1}].", descPath, fileName));
                        if (delayTime != 0)
                            Thread.Sleep(delayTime);//20161115 sy add 
                    }
                }
            }
            catch (System.Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        public bool MoveJobFiledat(string sourceFile, string targetPath, Job job, int delayTime)
        {
            try
            {
                //20161110 sy modify 手投sequence >= 5000 不刪除空資料夾待上相同sequence超過指定天數處理，目標資料夾固定 5000 
                string descFile = Path.Combine(targetPath, DateTime.Now.ToString("yyyyMMdd"), (int.Parse(job.CassetteSequenceNo) >= 50000 ? "50000" : job.JobKey.Split('_')[0]));
                string filedatName = job.GlassChipMaskBlockID + ".dat";
                string filejpsName = job.GlassChipMaskBlockID + "_JPS.dat";
                string filedat = Path.Combine(sourceFile, filedatName);
                string filejps = Path.Combine(sourceFile, filejpsName);
                if (!Directory.Exists(descFile)) Directory.CreateDirectory(descFile);
                if (File.Exists(filedat))
                {
                    File.Move(filedat, descFile + "\\" + filedatName);
                }
                if (File.Exists(filejps))
                {
                    if (delayTime != 0)
                        Thread.Sleep(delayTime);//20161115 sy add
                    File.Move(filejps, descFile + "\\" + filejpsName);
                }
                return true;
            }
            catch (Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name, ex);
                return false;
            }
        }

        public void CassetteMapDownload(Equipment eqp, Port port, IList<Job> slotData, string trxId)
        {
            string err = string.Empty;
            try
            {
                #region [取得EQP及Port資訊]
                Line line;

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqp.Data.NODENO);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null)
                {
                    err = string.Format("CAN'T FIND LINE_INFORMATION=[{0}] IN LINEENTITY!", eqp.Data.LINEID);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT IN EQUIPMENT=[{0}]!", eqp.Data.NODENO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                if (cst == null)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}], CAN'T FIND CASSETTE_DATA FROM CST_SEQNO=[{2}].", eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteSequenceNo);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID, 
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    throw new Exception(err);
                }
                #endregion

                #region Data and Status Check
                // CIM MODE OFF 不能改
                if (eqp.File.CIMMode == eBitResult.OFF)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CIM_MODE=[OFF], CAN NOT DOWNLOAD \"Cassette Map Download\" COMMAND!",
                        eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteID);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (port.File.Status != ePortStatus.LC)
                {
                    err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] PORT_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Map Download\" COMMAND!",
                        eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteID, (int)port.File.Status, port.File.Status.ToString());
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.CHANGER_MODE)
                {
                    if (port.File.CassetteStatus != eCassetteStatus.WAITING_FOR_CASSETTE_DATA &&
                        port.File.CassetteStatus != eCassetteStatus.CASSETTE_REMAP)
                    {
                        err = string.Format("EQUIPMENT=[{0}] PORT=[{1}] CSTID=[{2}] CST_STATUS=[{3}]({4}), CAN NOT DOWNLOAD \"Cassette Map Download\" COMMAND!",
                            eqp.Data.NODENO, port.Data.PORTNO, port.File.CassetteID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                        return;
                    }
                }
                #endregion

                #region [FCREP_THROUGH_MODE]
                //ADD FOR T3 20151216
                //FCREP在THROUGH_MODE下,OK玻璃ROBOT不抽,當如果全部都是OK玻璃,會造成CST狀態一直卡在WAIT_FOR_PROCESSING,導致BC無法退CST
                bool thcst = true;
                if (port.File.Type == ePortType.LoadingPort)
                {
                    if ((line.Data.LINETYPE == eLineType.CF.FCREP_TYPE1 ||
                        line.Data.LINETYPE == eLineType.CF.FCREP_TYPE2 ||
                        line.Data.LINETYPE == eLineType.CF.FCREP_TYPE3) &&
                        line.File.IndexOperMode == eINDEXER_OPERATION_MODE.THROUGH_MODE)
                    {
                        foreach (Job thj in slotData)
                        {
                            if (thj.JobJudge != "1")//有非OK的玻璃就成功下帳
                            {
                                thcst = false;
                            }
                        }
                        if (thcst == true)
                        {
                            int delayTime = ParameterManager["FILEMOVEDELAYTIME"].GetInteger();//20161115 sy add  
                            ObjectManager.JobManager.DeleteJobs(slotData);
                            ObjectManager.JobManager.RecordJobsHistory(slotData, eqp.Data.NODEID, eqp.Data.NODENO, port.Data.PORTNO, eJobEvent.Delete_CST_Complete.ToString(), trxId);
                            MoveFileData(line, slotData, port, delayTime);//Move File to  temp Path =“D:\UnicomLog\FileData\”
                            err = string.Format("LINE_NAME=[{0}],INDEXER_OPERATION_MODE=[{1}],ALL_JOB_JUDGE=[OK],ROBOT_CAN'T_FETCHOUT_JOB",
                                line.Data.LINETYPE, line.File.IndexOperMode);
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { UtilityMethod.GetAgentTrackKey(), eqp.Data.LINEID,
                        string.Format("[{0}] {1}", MethodBase.GetCurrentMethod().Name, err) });
                            Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                            return;
                        }
                    }
                }
                #endregion

                lock (cst) cst.CassetteControlCommand = eCstControlCmd.MapDownload;

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", port.Data.NODENO, port.Data.PORTNO);

                #region CassetteControlCommandForUPK Special Function
                //20160127 Add by Frank
                if (Workbench.LineType == eLineType.CF.FCUPK_TYPE1 && eqp.Data.NODEATTRIBUTE == "UPK")
                    trxName = string.Format("{0}_Port#{1}CassetteControlCommandForUPK", port.Data.NODENO, port.Data.PORTNO);
                #endregion

                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                outputData.ClearTrxWith0();

                string slotLog = string.Empty;
                string LDCassetteSettingCode = string.Empty;
                StringBuilder slotExist = new StringBuilder(new string('0', port.Data.MAXCOUNT));

                #region Watson Add 20150314 For CELL Virtual Port
                if (line.Data.FABTYPE == eFabType.CELL.ToString())
                {
                    if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.VirtualPort)
                    {
                        if (outputData.EventGroups[0].Events.Count < port.Data.MAXCOUNT)
                        {
                            err = string.Format("CELL VIRTUAL PORT=[{0}] CSTID=[{1}] MAX COUNT ERROR!!DB Port Maxcount=[{2}] AND TRX=[{3}] MISMATCH.",
                                 port.Data.PORTNO, port.File.CassetteID, port.Data.MAXCOUNT, outputData.EventGroups[0].Events.Count);
                            Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                        }

                        Job result = slotData.FirstOrDefault(j => (j.JobSequenceNo != "0") && (j.JobSequenceNo.Trim() != ""));

                        if (result != null)
                        {
                            if (outputData.EventGroups[0].Events.Count < int.Parse(result.JobSequenceNo))
                            {
                                err = string.Format("CELL VIRTUAL PORT=[{0}] CSTID=[{1}] MAX COUNT ERROR!! ValidateCassetteReply Job data Position=[{2}] AND TRX[{4}] Count =[{3}] MISMATCH.",
                                     port.Data.PORTNO, port.File.CassetteID, result.JobSequenceNo, outputData.EventGroups[0].Events.Count, outputData.EventGroups[0].Events[0].Name);
                                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                            }
                        }
                    }

                }

                #endregion

                if (line.Data.FABTYPE == eFabType.CELL.ToString())//sy modify 20160810 Use Xml format 
                    port.File.CellCst = outputData.EventGroups[0].Events[0].Items.Count < 4 ? eBitResult.ON : eBitResult.OFF;


                //20160330 sy add Update port.File.ProductType // 20160330 modify all shop
                //20160721 Moddify bt Frank For FCUPK Unloader
                if (line.Data.LINETYPE != eLineType.CF.FCUPK_TYPE1)
                {
                    if (port.File.Type == ePortType.UnloadingPort)
                    {
                        Job jobLast = slotData.LastOrDefault(j => j.CassetteSequenceNo != "0");
                        if (jobLast == null)
                            port.File.ProductType = "0";
                        else
                            port.File.ProductType = jobLast.ProductType.Value.ToString();
                    }
                }

                port.File.SamplingCount = 0;// 20161019 sy modify APC LastGlass

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK && Workbench.LineType != eLineType.CELL.CCCLN)//sy add & modify CLN Line 20160725
                {
                    outputData.EventGroups[0].IsMergeEvent = true;  //先組成寫入的Raw Data再一次性寫入，可以減少很多時間。 20150304 Tom 移动到此

                    #region Write ZR Area
                    foreach (Job j in slotData)
                    {
                        #region [APC LastGlass]
                        // 20161019 sy modify APC LastGlass
                        if (j.SamplingSlotFlag == "1")
                            port.File.SamplingCount++;
                        #endregion

                        if (!string.IsNullOrEmpty(slotLog)) slotLog += "\r\n";
                        slotLog += string.Format("\t\t\tCREATE JOB DATA: CST_SEQNO=[{0}] JOB_SEQNO=[{1}] GROUP_INDEX=[{2}] PRODUCT_TYPE=[{3}] GlassChipMaskBlockID=[{4}] SAMPLING_SLOG_FLAG=[{5}];",
                            j.CassetteSequenceNo, j.JobSequenceNo, j.GroupIndex, j.ProductType.Value, j.GlassChipMaskBlockID, j.SamplingSlotFlag);
                        if (port.File.CellCst == eBitResult.ON)//shihyang add For CELL CST 20151019
                        {
                            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[0].Value = j.CassetteSequenceNo;
                            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[1].Value = j.JobSequenceNo;
                            if (outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["SamplingSlotFlag"] != null)
                                outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["SamplingSlotFlag"].Value = j.SamplingSlotFlag;
                        }
                        else
                        {
                            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[0].Value = j.CassetteSequenceNo;
                            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[1].Value = j.JobSequenceNo;
                            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[2].Value = j.GroupIndex;
                            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[3].Value = j.ProductType.Value.ToString();
                            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[4].Value = ((int)j.CSTOperationMode).ToString();
                            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[5].Value = ((int)j.SubstrateType).ToString();
                            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[6].Value = ((int)j.CIMMode).ToString();
                            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[7].Value = ((int)j.JobType).ToString();
                            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[8].Value = j.JobJudge;
                            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[9].Value = j.SamplingSlotFlag;
                            //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[10].Value = j.OXRInformationRequestFlag; t3 common job data has't this item cc.kuang 20150702
                            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[10].Value = j.FirstRunFlag;
                            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[11].Value = j.JobGrade;
                            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[12].Value = j.GlassChipMaskBlockID;
                            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items[13].Value = j.PPID;
                        }
                        slotExist.Replace('0', '1', int.Parse(j.JobSequenceNo) - 1, 1);
                    }
                    #endregion

                    ObjectManager.PortManager.EnqueueSave(port.File);// 20161019 sy modify APC LastGlass
                }
                else
                {
                    if (outputData.EventGroups.Count == 2)
                    {
                        outputData.EventGroups[0].IsDisable = true;
                    }

                    //t3 CAC PLC not define CSTPPID item cc.kuang 2015/07/02
                    if (outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items.Count > 3)
                    {
                        string pPIDOfLinetype = line.Data.LINETYPE == eLineType.ARRAY.CAC_MYTEK ? "CAC_PPID" : "CLN_PPID";//sy add & modify CLN Line 20160725
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                            outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items["PPID"].Value = ParameterManager[pPIDOfLinetype].GetString().PadLeft(2).Substring(0, 2);//sy add & modify CLN Line 20160725
                        }
                        else //if mes not download cst clean PPID, use BC define PPID 2016/01/15 cc.kuang
                        {
                            if (port.File.CassetteCleanPPID.Trim().Length == 0)
                                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items["PPID"].Value = ParameterManager[pPIDOfLinetype].GetString().PadLeft(2).Substring(0, 2);//sy add & modify CLN Line 20160725
                            else
                                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items["PPID"].Value = port.File.CassetteCleanPPID; //ppid 改由MES download, 2016/01/14 cc.kuang//sy add & modify CLN Line 20160725
                        }
                    }
                    else
                    {
                        outputData.EventGroups[outputData.EventGroups.Count - 1].IsDisable = true;
                    }
                }

                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[0].Value = ((int)eCstControlCmd.MapDownload).ToString();
                if (slotData.Count() > 0)
                {
                    outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[1].Value = slotExist.ToString();// slotData[0].MesCstBody.SELECTEDPOSITIONMAP;
                }
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[2].Value = slotData.Count().ToString();// cc.kuang 2015/07/08 add
                if (Workbench.LineType == eLineType.CF.FCUPK_TYPE1 && port.File.Type == ePortType.UnloadingPort)
                    outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[4].Value = port.File.ProductType;
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                outputData.EventGroups[outputData.EventGroups.Count - 1].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();

                outputData.TrackKey = string.IsNullOrEmpty(trxId) == true ? UtilityMethod.GetAgentTrackKey() : trxId;

                string coolLog = string.Empty;
                if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.COOL_RUN_MODE)
                {
                    coolLog = string.Format(" COOLSTART=[{0}]", cst.CoolRunStart.ToString());
                }

                string log = string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM_MODE=[{2}] PORT=[{3}] PORT_TYPE=[{4}] CSTID=[{5}] \"Cassette Map Download\" COMMAND JOB_EXISTENCE=[{6}] JOB_COUNT=[0]{7} CST_SETTINGCODE=[{8}], SET BIT=[ON].\r\n",
                    eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString(), port.Data.PORTNO, port.File.Type, port.File.CassetteID, slotExist, coolLog, cst.LDCassetteSettingCode);

                //2015/9/8 modify by Frank 先判斷是否有這個TRX，有的話再讓PLCAgent去找這個TRX。
                bool trxExist = Convert.ToBoolean(Invoke(eAgentName.PLCAgent, "ContainsTransaction", new object[] { "L2_IndexerOperationModeChangeReport" }));
                Trx idxTrx = null;
                if (trxExist)
                    idxTrx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { "L2_IndexerOperationModeChangeReport", false }) as Trx;

                if (idxTrx != null)
                {
                    // 增加Log 來判斷是什麼模式所下的資料, 方便解析問題
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("** INDEXER OPERATION MODE=[{0}] **", line.File.IndexOperMode.ToString()));
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", log + slotLog);

                eFabType fabType;
                Enum.TryParse<eFabType>(line.Data.FABTYPE, out fabType);

                switch (fabType)
                {
                    case eFabType.ARRAY:
                        {
                            Invoke(eServiceName.ArraySpecialService, "CassetteMapDownload_Array",
                                        new object[] { line, eqp, port, slotData, outputData });
                        }
                        break;
                    case eFabType.CF:
                        {
                            object[] _obj = new object[]
                            {
                                eqp,
                                port,
                                slotData,
                                outputData
                            };
                            switch (line.Data.JOBDATALINETYPE)
                            {
                                case eJobDataLineType.CF.PHOTO_BMPS: Invoke(eServiceName.CFSpecialService, "CassetteMapDownload_PHOTO", _obj); break;
                                case eJobDataLineType.CF.PHOTO_GRB: Invoke(eServiceName.CFSpecialService, "CassetteMapDownload_PHOTO", _obj); break;
                                case eJobDataLineType.CF.REWORK: Invoke(eServiceName.CFSpecialService, "CassetteMapDownload_REWORK", _obj); break;
                                case eJobDataLineType.CF.UNPACK: Invoke(eServiceName.CFSpecialService, "CassetteMapDownload_UNPACK", _obj); break;
                                case eJobDataLineType.CF.MASK: Invoke(eServiceName.CFSpecialService, "CassetteMapDownload_MASK", _obj); break;
                                case eJobDataLineType.CF.REPAIR: Invoke(eServiceName.CFSpecialService, "CassetteMapDownload_REPAIR", _obj); break;
                                case eJobDataLineType.CF.MQC_1: Invoke(eServiceName.CFSpecialService, "CassetteMapDownload_MQC_1", _obj); break;
                                case eJobDataLineType.CF.MQC_2: Invoke(eServiceName.CFSpecialService, "CassetteMapDownload_MQC_2", _obj); break;
                                //case eJobDataLineType.CF.MQC_3: Invoke(eServiceName.CFSpecialService, "CassetteMapDownload_MQC_3", _obj); break;
                                case eJobDataLineType.CF.FCMAC: Invoke(eServiceName.CFSpecialService, "CassetteMapDownload_FCMAC", _obj); break;
                                case eJobDataLineType.CF.FCSRT: Invoke(eServiceName.CFSpecialService, "CassetteMapDownload_FCSRT", _obj); break;
                                case eJobDataLineType.CF.FCPSH: Invoke(eServiceName.CFSpecialService, "CassetteMapDownload_FCPSH", _obj); break;
                                case eJobDataLineType.CF.FCAOI: Invoke(eServiceName.CFSpecialService, "CassetteMapDownload_FCAOI", _obj); break;
                            }
                        }
                        break;
                    case eFabType.CELL:
                        {
                            //Jun Add 20150205 For Cell Loader Cassette Setting Code
                            outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[3].Value = cst.LDCassetteSettingCode;
                            object[] _obj = new object[]
                            {
                                eqp,
                                port,
                                slotData,
                                outputData
                            };
                            Invoke(eServiceName.CELLSpecialService, "CassetteMapDownload_CELL", _obj);
                        }
                        break;
                    case eFabType.MODULE:
                        {
                            //Jun Add 20150205 For Cell Loader Cassette Setting Code
                            outputData.EventGroups[outputData.EventGroups.Count - 1].Events[0].Items[3].Value = cst.LDCassetteSettingCode;
                            object[] _obj = new object[]
                            {
                                eqp,
                                port,
                                slotData,
                                outputData
                            };
                            Invoke(eServiceName.MODULESpecialService, "CassetteMapDownload_MODULE", _obj);
                        }
                        break;
                }

                if (port.File.Type != ePortType.UnloadingPort) // 20160307 Modify by Frank Unloading Port don't send this command
                    FileInfoNotifyCommand(trxId, ObjectManager.EquipmentManager.GetEQPs(), cst.CassetteSequenceNo); //add by bruce 2015/11/5 download inspection eq use

                RecordCassetteHistory(outputData.TrackKey, eqp, port, cst, eCstControlCmd.MapDownload.ToString(), string.Empty, string.Empty);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteControlCommandReply(Trx inputData)
        {
            try
            {
                eCstControlCmd cmd = eCstControlCmd.None; // t2,t3 sync 2016/04/14 cc.kuang

                if (inputData.IsInitTrigger) return;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                eCstCmdRetCode retCode = (eCstCmdRetCode)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                #region [取得EQP及Port資訊]
                int startpos = inputData.Metadata.Name.IndexOf('#') + 1;

                if (startpos.Equals(0)) throw new Exception(string.Format("CAN'T FIND PORT_NO IN TRX_NAME=[{0}]!", inputData.Metadata.Name));

                string portNo = inputData.Metadata.Name.Substring(startpos, 2);

                string err = string.Empty;
                string replyMsg = string.Empty;
                Equipment eqp; Port port;
                Line line;

                eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                if (eqp == null)
                {
                    err = string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY! SKIP RECORD CASSETTE HISTORY.", inputData.Metadata.NodeNo);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { inputData.TrackKey, eqp.Data.LINEID, "[CassetteControlCommandReply] " + err });
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                port = ObjectManager.PortManager.GetPort(eqp.Data.LINEID, eqp.Data.NODENO, portNo);

                if (port == null)
                {
                    err = string.Format("CAN'T FIND PORT_NO=[{0}] IN EQUIPMENT=[{1}]! SKIP RECORD CASSETTE HISTORY", portNo, eqp.Data.NODENO);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { inputData.TrackKey, eqp.Data.LINEID, "[CassetteControlCommandReply] " + err });
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                //for t2,t3 sync 2016/04/25 cc.kuang
                line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null)
                {
                    err = string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTENTITY!", eqp.Data.LINEID);

                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                        new object[] { inputData.TrackKey, eqp.Data.LINEID, "[CassetteControlCommandReply] " + err });
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                    return;
                }

                Cassette cst = ObjectManager.CassetteManager.GetCassette(int.Parse(port.File.CassetteSequenceNo));

                replyMsg = MethodBase.GetCurrentMethod().Name;
                string methodName = MethodBase.GetCurrentMethod().Name + "()";

                if (cst != null)
                {
                    if (cst.CassetteControlCommand != eCstControlCmd.None)
                    {
                        methodName = string.Format("Cassette{0}Reply()", cst.CassetteControlCommand.ToString());
                    }

                    if (cst.CassetteControlCommand == eCstControlCmd.MapDownload && triggerBit == eBitResult.ON)
                    {
                        Invoke(eServiceName.UIService, "CassetteMapDownloadResultReport", new object[] { port, (int)retCode });
                    }
                }
                #endregion

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, methodName,
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] PORT=[{3}] CSTID=[{4}] RETURNCODE=[{5}]({6}).",
                    inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString(), portNo, port.File.CassetteID, (int)retCode, retCode.ToString()));

                if (triggerBit == eBitResult.OFF) return;

                string timeName = string.Format("{0}_{1}_{2}", inputData.Metadata.NodeNo, portNo, CstControlCommandTimeout);

                UserTimer timer = null;
                if (_timerManager.IsAliveTimer(timeName))
                {
                    timer = _timerManager.GetAliveTimer(timeName);
                    _timerManager.TerminateTimer(timeName);
                }

                if (timer != null)
                {
                    string[] obj = timer.State.ToString().Split('_');
                    //eCstControlCmd cmd = (eCstControlCmd)int.Parse(obj[1]); 2016/04/14 cc.kuang
                    cmd = (eCstControlCmd)int.Parse(obj[1]);
                    if (cmd != eCstControlCmd.None)
                        replyMsg = string.Format("Cassette{0}Reply", cmd.ToString());

                    //t2&t3 sync 2016/04/25 cc.kuang
                    if (line.File.HostMode != eHostMode.REMOTE || (retCode != eCstCmdRetCode.COMMAND_OK && retCode != eCstCmdRetCode.ALREADY_RECEIVED))
                    {
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID, 
                        string.Format("{0} - EQUIPMENT=[{1}] PORT=[{2}] RETURN CODE=[{3}]({4})", 
                        replyMsg, eqp.Data.NODENO, port.Data.PORTNO, (int)retCode, retCode) });
                    }

                    // 2015.2.5 依照CSOT 登京 , 在Cst Map Downlad NG時要Cancel CST
                    /*
                    if (ParameterManager[eREPORT_SWITCH.DATA_DOWNLOAD_REPLY_ERROR_AUTO_CANCEL].GetBoolean() &&
                        cmd == eCstControlCmd.MapDownload)
                    {
                        switch (retCode)
                        {
                            case eCstCmdRetCode.ALREADY_RECEIVED:
                            case eCstCmdRetCode.COMMAND_OK:
                            case eCstCmdRetCode.COMMAND_ERROR: break;
                            default:
                                if (port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_CASSETTE_DATA)
                                {
                                    CassetteProcessCancel(eqp.Data.NODENO, port.Data.PORTNO);
                                }
                                break;
                        }
                    }
                    */
                }

                string trxName = string.Format("{0}_Port#{1}CassetteControlCommand", inputData.Metadata.NodeNo, portNo);

                #region CassetteControlCommandForUPK Special Function
                //20160127 Add by Frank
                if (Workbench.LineType == eLineType.CF.FCUPK_TYPE1 && eqp.Data.NODEATTRIBUTE == "UPK")
                    trxName = string.Format("{0}_Port#{1}CassetteControlCommandForUPK", port.Data.NODENO, port.Data.PORTNO);
                #endregion

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK)    //modify by bruce 2015/9/6 for CAC line use
                {
                    outputdata.EventGroups[0].IsDisable = true;
                }

                outputdata.EventGroups[outputdata.EventGroups.Count - 1].Events[0].IsDisable = true;
                outputdata.EventGroups[outputdata.EventGroups.Count - 1].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;
                SendPLCData(outputdata);

                Logger.LogInfoWrite(this.LogName, this.GetType().Name, methodName,
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}] CSTID=[{3}], SET BIT=[OFF].",
                    inputData.Metadata.NodeNo, inputData.TrackKey, portNo, port.File.CassetteID));

                RecordCassetteHistory(inputData.TrackKey, eqp, port, cst, string.Empty, retCode.ToString(), string.Empty);

                // 2015.2.5 依照CSOT 登京 , 在Cst Map Downlad NG時要Cancel CST, 2016/04/25 add line remote check cc.kuang
                if ((ParameterManager[eREPORT_SWITCH.DATA_DOWNLOAD_REPLY_ERROR_AUTO_CANCEL].GetBoolean() || line.File.HostMode == eHostMode.REMOTE) &&
                    cmd == eCstControlCmd.MapDownload)
                {
                    switch (retCode)
                    {
                        case eCstCmdRetCode.ALREADY_RECEIVED:
                        case eCstCmdRetCode.COMMAND_OK: break;
                        case eCstCmdRetCode.COMMAND_ERROR:
                            if (line.File.HostMode == eHostMode.REMOTE)
                            {
                                Thread.Sleep(2000);
                                CassetteProcessCancel(eqp.Data.NODENO, port.Data.PORTNO);
                            }
                            break;
                        default:
                            if (port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_CASSETTE_DATA && ParameterManager[eREPORT_SWITCH.DATA_DOWNLOAD_REPLY_ERROR_AUTO_CANCEL].GetBoolean())
                            {
                                Thread.Sleep(2000);
                                CassetteProcessCancel(eqp.Data.NODENO, port.Data.PORTNO);
                            }
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void CassetteControlCommandTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string[] obj = timer.State.ToString().Split('_');
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], CstControlCommandTimeout);
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

                if (Workbench.LineType != eLineType.ARRAY.CAC_MYTEK)    //modify by bruce 2015/9/6 for CAC line use
                {
                    outputdata.EventGroups[0].IsDisable = true;
                    outputdata.EventGroups[1].Events[0].IsDisable = true;
                    outputdata.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                    outputdata.TrackKey = obj[0];
                }
                else
                {
                    outputdata.EventGroups[0].Events[0].IsDisable = true;
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                    outputdata.TrackKey = obj[0];
                }
                SendPLCData(outputdata);
                eCstControlCmd cmd = (eCstControlCmd)int.Parse(obj[1]);
                Port port = ObjectManager.PortManager.GetPort(sArray[1]);

                if (cmd == eCstControlCmd.MapDownload)
                {
                    if (port != null)
                    {
                        Invoke(eServiceName.UIService, "CassetteMapDownloadResultReport", new object[] { port, 99 });
                    }
                }

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] PORT=[{2}] CASSETTE_CONTROL_COMMAND=[{3}] REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], obj[0], sArray[1], cmd.ToString()));

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                if (eqp == null) return;
                if (port == null) return;

                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { obj[0], eqp.Data.LINEID, 
                        string.Format("Cassette{0}Reply - EQUIPMENT=[{1}] PORT=[{2}] \"T1 TIMEOUT\"", 
                        cmd, eqp.Data.NODENO, port.Data.PORTNO)});
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [CST Operation Mode Change Report]
        /// <summary>
        /// 更新記億體資料
        /// </summary>
        /// <param name="inputData">同CSTOperationModeChangeReport(Trx inputData)</param>
        /// <param name="log">記錄動作者</param>
        public void CSTOperationModeChangeReportUpdate(Trx inputData, string sourceMethod)
        {
            try
            {
                #region [取得EQP]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion
                eCSTOperationMode cstOM;
                int value = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.CSTOperationMode].Value);

                //目前EQP IO Report "1：Kind To Kind mode  2：Cassette To Cassette mode 3：Lot To Lot Mode"
                //但是JOB DATA IO is " 0: Kind to Kind   1: CST to CST 2：Lot To Lot"

                switch (value)
                {
                    case 1:
                        cstOM = eCSTOperationMode.KTOK; //bruce Modify 20150707
                        break;
                    case 2:
                        cstOM = eCSTOperationMode.CTOC; //bruce Modify 20150707
                        break;
                    case 3:
                        cstOM = eCSTOperationMode.LTOL; //bruce Modify 20150707
                        break;
                    default:
                        cstOM = eCSTOperationMode.CTOC; //bruce Modify 20150707
                        break;
                }

                eCSTOperationMode oldMode = eqp.File.CSTOperationMode;

                if (oldMode != cstOM)
                {
                    lock (eqp)
                    {
                        eqp.File.CSTOperationMode = cstOM;
                    }
                    ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                    // Report to MES
                    Invoke(eServiceName.MESService, "CassetteOperModeChanged",
                        new object[] { inputData.TrackKey, eqp.Data.LINEID, cstOM.ToString() });
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [{1}][{2}] CASSETTE_OPERATION_MODE=[{3}]({4}).",
                    eqp.Data.NODENO, sourceMethod, inputData.TrackKey, (int)cstOM, cstOM.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CSTOperationModeChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger)
                {
                    CSTOperationModeChangeReportUpdate(inputData, "CSTOperationModeChangeReport_Initial");
                    return;
                }

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].",
                        inputData.Metadata.NodeNo, inputData.TrackKey));
                    CSTOperationModeChangeReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [取得EQP]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion
                eCSTOperationMode cstOM;
                //目前EQP IO Report "1：Kind To Kind mode  2：Cassette To Cassette mode 3：Lot To Lot Mode"
                //但是JOB DATA IO is " 0: Kind to Kind   1: CST to CST 2：Lot To Lot"
                int value = int.Parse(inputData.EventGroups[0].Events[0].Items[ePLC.CSTOperationMode].Value);
                //
                switch (value)
                {
                    case 1:
                        cstOM = eCSTOperationMode.KTOK; //bruce Modify 20150707
                        break;
                    case 2:
                        cstOM = eCSTOperationMode.CTOC; //bruce Modify 20150707
                        break;
                    case 3:
                        cstOM = eCSTOperationMode.LTOL; //bruce Modify 20150707
                        break;
                    default:
                        cstOM = eCSTOperationMode.CTOC; //bruce Modify 20150707
                        break;
                }

                lock (eqp)
                {
                    eqp.File.CSTOperationMode = cstOM;
                }
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });  //add by bruce 2015/7/24

                // Report to MES
                Invoke(eServiceName.MESService, "CassetteOperModeChanged",
                    new object[] { inputData.TrackKey, eqp.Data.LINEID, cstOM.ToString() });

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CASSETTE_OPERATION_MODE=[{2}]({3}).",
                    eqp.Data.NODENO, inputData.TrackKey, (int)cstOM, cstOM.ToString()));

                CSTOperationModeChangeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    CSTOperationModeChangeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void CSTOperationModeChangeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_CSTOperationModeChangeReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                if (outputdata != null)
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    string timeoutName = string.Format("{0}_{1}", eqpNo, "CSTOperationModeChangeReportReplyTimeout");
                    if (_timerManager.IsAliveTimer(timeoutName))
                    {
                        _timerManager.TerminateTimer(timeoutName);
                    }

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(CSTOperationModeChangeReportReplyTimeout), trackKey);
                    }

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CASSETTE OPERATION MODE CHANGE REPORT REPLY, SET BIT=[{2}].",
                        eqpNo, trackKey, value));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void CSTOperationModeChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CASSETTE OPERATION MODE CHANGE REPORT TIMEOUT, SET BIT=[OFF].",
                    sArray[0], sArray[1], trackKey));

                CSTOperationModeChangeReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [PartialFullMode]
        public void PartialFullMode(Trx inputData)
        {
            try
            {
                #region [取得EQP]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                eEnableDisable partialFM = (eEnableDisable)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                if (eqp.File.PartialFullMode != partialFM)
                {
                    lock (eqp)
                    {
                        eqp.File.PartialFullMode = partialFM;
                        ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                    }
                }

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [EQP -> EQP][{1}] PARTIAL_FULL_MODE=[{2}]({3})",
                    eqp.Data.NODENO, inputData.TrackKey, (int)partialFM, partialFM.ToString()));

                if (inputData.IsInitTrigger) return;
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Cassette on Port Q Time Change Report]
        public void CassetteonPortQTimeChangeReport(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;

                #region [拆出PLCAgent Data]
                eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                #endregion
                if (bitResult == eBitResult.OFF)
                {
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].",
                        inputData.Metadata.NodeNo, inputData.TrackKey));
                    CassetteonPortQTimeChangeReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                    return;
                }

                #region [取得EQP]
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));
                #endregion

                #region[取得QTime]
                int cstonPortQTime = int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                #endregion

                lock (eqp)
                {
                    eqp.File.CassetteonPortQTime = cstonPortQTime;
                }
                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CASSETTE_ON_PORT_Q_TIME=[{2}].",
                    eqp.Data.NODENO, inputData.TrackKey, cstonPortQTime));

                CassetteonPortQTimeChangeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                //CassetteOnPort QTime时也通知OPI 20150520 Tom
                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { inputData.TrackKey, eqp });
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                // 避免中間發生Exception BCS不把BIT ON起來
                if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                {
                    CassetteonPortQTimeChangeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                }
            }
        }

        private void CassetteonPortQTimeChangeReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                string trxName = string.Format("{0}_CassetteonPortQTimeChangeReportReply", eqpNo);
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;

                if (outputdata != null)
                {
                    outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    string timeoutName = string.Format("{0}_{1}", eqpNo, "CassetteonPortQTimeChangeReportReplyTimeout");
                    if (_timerManager.IsAliveTimer(timeoutName))
                    {
                        _timerManager.TerminateTimer(timeoutName);
                    }

                    if (value.Equals(eBitResult.ON))
                    {
                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["T2"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(CassetteonPortQTimeChangeReportReplyTimeout), trackKey);
                    }

                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CASSETTE ON PORT Q-TIME CHANGE REPORT REPLY, SET BIT=[{2}].",
                        eqpNo, trackKey, value));
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void CassetteonPortQTimeChangeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CASSETTE ON PORT Q-TIME CHANGE REPORT REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                CassetteonPortQTimeChangeReportReply(sArray[0], eBitResult.OFF, trackKey);
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        #region [Cassette on Port Q Time Change Command]

        public bool CassetteonPortQTimeChangeCommand(string trxid, string currentEqpNo, int cstonPortQTime)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(currentEqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", currentEqpNo));

                Trx trx = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format("{0}_CassetteonPortQTimeChangeCommand", eqp.Data.NODENO)) as Trx;
                if (trx != null)
                {
                    if (cstonPortQTime < 0 || cstonPortQTime > 65535)
                    {
                        Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}]  CASSETTE ON PORT Q-TIME=[{2}] MUST BE BETWEEN \"0~65535\"!", eqp.Data.NODENO, trx.TrackKey, cstonPortQTime));
                        return false;
                    }

                    trx.EventGroups[0].Events[0].Items[0].Value = cstonPortQTime.ToString();
                    trx.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                    trx.EventGroups[0].Events[1].Items[0].Value = "1";
                    trx.TrackKey = trxid;
                    SendPLCData(trx);
                    string timerId = string.Format("{0}_CassetteonPortQTimeChangeCommandTimeOut", eqp.Data.NODENO);
                    if (Timermanager.IsAliveTimer(timerId))
                    {
                        Timermanager.TerminateTimer(timerId);
                    }
                    Timermanager.CreateTimer(timerId, false, ParameterManager["T1"].GetInteger(), new ElapsedEventHandler(CassetteonPortQTimeChangeCommandTimeOut), trxid);

                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CASSETTE_ON_PORT_Q-TIME=[{2}], SET BIT=[ON]", eqp.Data.NODENO, trx.TrackKey, cstonPortQTime));
                }

                return true;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        private void CassetteonPortQTimeChangeCommandTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');
                string timeName = string.Format("{0}_CassetteonPortQTimeChangeCommandTimeOut", sArray[0]);
                //终止Timer
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                //清除Bit
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_CassetteonPortQTimeChangeCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CASSETTE ON PORT Q-TIME CHANGE COMMAND REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void CassetteonPortQTimeChangeCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                eReturnCode1 returnCode = (eReturnCode1)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                   string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] CASSETTE ON PORT Q-TIME CHANGE COMMAND REPLY RESULT=[{3}]({4}).",
                   eqpNo, inputData.TrackKey, triggerBit.ToString(), (int)returnCode, returnCode.ToString()));

                if (triggerBit == eBitResult.OFF) return;
                //终止Timer
                string timeName = string.Format("{0}_CassetteonPortQTimeChangeCommandTimeOut", eqpNo);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                //清除Bit
                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqpNo + "_CassetteonPortQTimeChangeCommand") as Trx;
                outputdata.EventGroups[0].Events[0].IsDisable = true;
                outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputData.TrackKey;

                SendPLCData(outputdata);

                Logger.LogInfoWrite(this.LogName, GetType().Name, "CassetteonPortQTimeChangeCommand()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CASSETTE ON PORT Q-TIME CHANGE COMMAND, SET BIT=[OFF]", eqpNo, inputData.TrackKey));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        /// <summary>
        /// 通知檢測機台Download File
        /// </summary>
        /// <param name="trackKey"></param>
        /// <param name="eqps"></param>
        /// <param name="cstSeqNo"></param>
        #region[File Info Notify Command]
        public void FileInfoNotifyCommand(string trackKey, IList<Equipment> eqps, string cstSeqNo)
        {
            try
            {
                foreach (Equipment eqp in eqps)
                {
                    if (eqp.Data.NODEATTRIBUTE != "IN")
                        continue;
                    Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqp.Data.NODENO + "_FileInfoNotifyCommand") as Trx;
                    if (outputdata == null)
                        continue;
                    if (eqp.File.CIMMode == eBitResult.OFF)
                    {
                        string err = string.Format("[{0}] EQUIPMENT=[{1}] CIM_MODE=[OFF], CAN NOT SEND FILE INFO NOTIFY COMMAND!",
                            MethodBase.GetCurrentMethod().Name, eqp.Data.NODENO);
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID, err });
                        LogWarn(MethodBase.GetCurrentMethod().Name + "()", err);
                        continue;
                    }
                    outputdata[0][0][0].Value = cstSeqNo;
                    outputdata[0][1][0].Value = ((int)eBitResult.ON).ToString();
                    outputdata[0][1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);

                    string timeName = string.Format("{0}_{1}", eqp.Data.NODENO, FileInfoNotifyCommandTimeout);

                    if (_timerManager.IsAliveTimer(timeName))
                    {
                        _timerManager.TerminateTimer(timeName);
                    }
                    _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(),
                        new System.Timers.ElapsedEventHandler(FileInfoNotifyCommandReplyTimeout), outputdata.TrackKey);

                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] FILE INFO NOTIFY COMMAND SET BIT=[ON], CST SEQUENCE NO.=[{2}]",
                            eqp.Data.NODENO, outputdata.TrackKey, cstSeqNo));
                }
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        public void FileInfoNotifyCommandReply(Trx inputdata)
        {
            try
            {
                if (inputdata.IsInitTrigger) return;
                string eqpNo = inputdata.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputdata[0][0][0].Value);
                if (triggerBit == eBitResult.OFF)
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [EQP -> BCS][{1}] FILE INFO NOTIFY COMMAND REPLY BIT=[{2}].",
                    eqpNo, inputdata.TrackKey, triggerBit.ToString()));
                    return;
                }

                LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[EQUIPMENT={0}] [EQP -> BCS][{1}] FILE INFO NOTIFY COMMAND REPLY BIT=[{2}].",
                eqpNo, inputdata.TrackKey, triggerBit.ToString()));

                string timeName = string.Format("{0}_{1}", inputdata.Metadata.NodeNo, FileInfoNotifyCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(inputdata.Metadata.NodeNo + "_FileInfoNotifyCommand") as Trx;
                outputdata[0][1][0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = inputdata.TrackKey;
                SendPLCData(outputdata);
                LogInfo("FileInfoNotifyCommand()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] FILE INFO NOTIFY COMMAND SET BIT=[OFF].",
                    inputdata.Metadata.NodeNo, inputdata.TrackKey));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void FileInfoNotifyCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format("{0}_{1}", sArray[0], FileInfoNotifyCommandTimeout);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] FILE INFO NOTIFY COMMAND REPLY TIMEOUT, SET BIT=[OFF].",
                    sArray[0], trackKey));

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(sArray[0] + "_FileInfoNotifyCommand") as Trx;
                outputdata[0][1][0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        #endregion

        /// <summary>
        /// Robot Scan which CST should be Quit
        /// </summary>
        /// <param name="cmdPortNo"></param>
        #region[Cassette Process End Scan]
        public void CassetteProcessEndScan(IList<string> cmdPortNos)
        {
            Line line;
            Equipment eqp;
            List<Port> lstPort;
            Cassette cst;
            IList<Job> lstJobinCST;
            IList<Job> lstJob;
            bool quitCST = true;
            string trxName;
            Trx trxCassetteSlotPosition;
            Trx trxCassetteSlotExist;
            //Trx inputdata;
            string allSlotExistInfo;
            string msg;
            string msgInfo = string.Empty;//20161116 sy add
            try
            {
                line = ObjectManager.LineManager.GetLines()[0];
                eqp = ObjectManager.EquipmentManager.GetEQP("L2");
                if (eqp.File.CIMMode != eBitResult.ON || eqp.File.EquipmentOperationMode != eEQPOperationMode.AUTO)
                    return;

                if (CassetteProcessEndScanCount < 10)
                {
                    CassetteProcessEndScanCount++;
                    return;
                }
                else
                {
                    CassetteProcessEndScanCount = 0;
                }

                lstPort = ObjectManager.PortManager.GetPorts(eqp.Data.NODEID);
                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("CassettePorcessEndScan is run! lstPort=[{0}]",lstPort));
                //check Processing cst
                foreach (Port port in lstPort)
                {
                    if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE || line.File.LineOperMode == eMES_LINEOPERMODE.EXCHANGE) //modify for cst quit at changer mode | exchange mode 2016/05/06 cc.kuang
                        break;

                    quitCST = true;
                    bool skip = cmdPortNos.Contains(port.Data.PORTNO);
                    if (skip)
                        continue;

                    if (port.File.Status != ePortStatus.LC || (port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING && port.File.CassetteStatus != eCassetteStatus.IN_ABORTING))
                        continue;

                    cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID);
                    if (cst == null //|| cst.CassetteControlCommand == eCstControlCmd.ProcessCancel, delete cancel constrain for robot cmd timing issue cc.kuang 2016/02/29
                        || cst.CassetteControlCommand == eCstControlCmd.ProcessAbort || cst.CassetteControlCommand == eCstControlCmd.ProcessEnd)
                        continue;

                    trxName = string.Format("{0}_Port#{1}JobEachCassetteSlotPositionBlock", eqp.Data.NODENO, port.Data.PORTNO);
                    trxCassetteSlotPosition = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                    if (trxCassetteSlotPosition == null)
                        continue;

                    if (line.Data.LINETYPE == eLineType.CELL.CCCRP || line.Data.LINETYPE == eLineType.CELL.CCCHN
                           || line.Data.LINETYPE == eLineType.CELL.CCSOR || line.Data.LINETYPE == eLineType.CELL.CCRWT
                        || line.Data.LINETYPE == eLineType.CELL.CCCRP_2) //CELL check sy add 20160228
                    {
                    }
                    else
                    {
                        trxName = string.Format("{0}_Port#{1}JobEachCassetteSlotExistsBlock", eqp.Data.NODENO, port.Data.PORTNO);
                        trxCassetteSlotExist = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                        if (trxCassetteSlotExist == null)
                            continue;

                        allSlotExistInfo = trxCassetteSlotExist.EventGroups[0].Events[0].Items[0].Value;
                        for (int i = 0; i < port.Data.MAXCOUNT; i++) //check slot position & slot exixt 
                        {
                            int job_ExistInfo = int.Parse(allSlotExistInfo.Substring(i, 1));
                            string cst_seq = trxCassetteSlotPosition.EventGroups[0].Events[0].Items[2 * i].Value;
                            string job_seq = trxCassetteSlotPosition.EventGroups[0].Events[0].Items[2 * i + 1].Value;
                            if (job_ExistInfo == 1)
                            {
                                if (cst_seq.Equals("0") || job_seq.Equals("0"))
                                {
                                    quitCST = false;
                                    Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("quitCST=[{0}],job_ExistInfo=[{1}],cst_seq&job_seq=[0],PortNo=[{2}],CSTID=[{3}]",quitCST, job_ExistInfo,port.Data.PORTNO,port.File.CassetteID));
                                    break;
                                }
                            }
                            else
                            {
                                if (!cst_seq.Equals("0") || !job_seq.Equals("0"))
                                {
                                    quitCST = false;
                                    Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("quitCST=[{0}],job_ExistInfo=[{1}],cst_seq&job_seq!=[0],PortNo=[{2}],CSTID=[{3}]", quitCST, job_ExistInfo,port.Data.PORTNO, port.File.CassetteID));
                                    break;
                                }
                            }
                        }
                    }

                    if (!quitCST)
                        continue;

                    lstJobinCST = new List<Job>();
                    for (int i = 0; i < trxCassetteSlotPosition.EventGroups[0].Events[0].Items.Count; i += 2)
                    {
                        string cst_seq = trxCassetteSlotPosition.EventGroups[0].Events[0].Items[i].Value;
                        string job_seq = trxCassetteSlotPosition.EventGroups[0].Events[0].Items[i + 1].Value;
                        if (!cst_seq.Equals("0") || !job_seq.Equals("0"))
                        {
                            Job j = new Job(int.Parse(cst_seq), int.Parse(job_seq));
                            lstJobinCST.Add(j);
                        }
                    }
                    msgInfo += string.Format("CST Job Counts = [{0}],", lstJobinCST.Count);//20161116 sy add
                    msgInfo += string.Format("Port Type = [{0}],", port.File.Type.ToString());//20161116 sy add
                    if (port.File.Type == ePortType.BothPort)
                    {
                        if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                            continue; //Both Port in Changer mode, not Check


                        if (line.Data.LINETYPE == eLineType.CELL.CCGAP || line.Data.LINETYPE == eLineType.CELL.CCPTH
                            || line.Data.LINETYPE == eLineType.CELL.CCTAM || line.Data.LINETYPE == eLineType.CELL.CCPDR||line.Data.LINETYPE==eLineType.CF.FCREW_TYPE1) //CELL check sy add 20160223
                        {
                            #region[CELL BOTH]
                            #region [SamplingSlotFlag & TrackingData Check]
                            foreach (Job job in lstJobinCST)//CST 還有GLASSES 未做製程
                            {
                                Job jb = ObjectManager.JobManager.GetJob(job.CassetteSequenceNo, job.JobSequenceNo);
                                if (jb.SamplingSlotFlag.Equals("1") && jb.TrackingData == new string('0', jb.TrackingData.Length))//20170601 huangjiayin modify
                                {
                                    quitCST = false;
                                    continue;
                                }
                            }

                            if (quitCST) msgInfo += string.Format("SamplingSlotFlag & TrackingData Check OK,");//20161116 sy add
                            #endregion
                            #region ["All Glass In CST Check]
                            lstJob = ObjectManager.JobManager.GetJobs(port.File.CassetteSequenceNo); //get all original job in cst
                            List<string> jobInCst = new List<string>();
                            int jobRemoveCount = 0;
                            int jobInCstCount = 0;
                            foreach (Job job in lstJob)
                            {
                                if (job.RemoveFlag) //Remove glass not need check
                                {
                                    jobRemoveCount++;
                                    continue;
                                }
                                List<Port> ports = ObjectManager.PortManager.GetPorts(eqp.Data.NODEID);
                                foreach (Port eachPort in ports)
                                {
                                    bool inCst = false;
                                    string trxID = string.Format("{0}_Port#{1}JobEachCassetteSlotPositionBlock", eqp.Data.NODENO, eachPort.Data.PORTNO);
                                    Trx Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;
                                    int maxSlot = Trx.EventGroups[0].Events[0].Items.Count;
                                    for (int i = 0; i < maxSlot; i += 2)
                                    {
                                        if (job.CassetteSequenceNo == Trx.EventGroups[0].Events[0].Items[i].Value && job.JobSequenceNo == Trx.EventGroups[0].Events[0].Items[i + 1].Value)
                                        {
                                            inCst = true;
                                            jobInCstCount++;
                                            break;
                                        }
                                    }
                                    if (inCst) break;
                                }
                            }
                            if (!(lstJob.Count == (jobRemoveCount + jobInCstCount)))
                            {
                                quitCST = false;
                                continue;
                                //break;Debug CFREWORK NG port偶尔不退port异常 20190611 by hujunpeng
                            }
                            if (quitCST) msgInfo += string.Format("All Glass In CST Check OK,");//20161116 sy add
                            #endregion
                            #endregion
                        }
                        else if (line.Data.LINETYPE != eLineType.ARRAY.BFG_SHUZTUNG) //not BFG check
                        {
                            lstJob = ObjectManager.JobManager.GetJobs(port.File.CassetteSequenceNo); //get all original job in cst
                            foreach (Job job in lstJob)
                            {
                                if (job.RemoveFlag) //Remove glass not need check
                                    continue;

                                if (lstJobinCST.FirstOrDefault(j => job.CassetteSequenceNo == j.CassetteSequenceNo
                                    && job.JobSequenceNo == j.JobSequenceNo) == null) //not in cst's glass not quit
                                {
                                    quitCST = false;
                                    //if (!quitCST)
                                    //{
                                    //    if (line.Data.FABTYPE == eFabType.CF.ToString())//glass store to other cst check for CF's NG glass
                                    //    {
                                    //        if (job.ToCstID.Trim().Length > 0 && job.ToSlotNo.Trim().Length > 0 && job.CurrentEQPNo == eqp.Data.NODENO)
                                    //            quitCST = true;
                                    //    }
                                    //}
                                    //if (!quitCST)
                                    //    break;
                                }
                                else //glass in cst check
                                {
                                    if (port.File.CassetteStatus == eCassetteStatus.IN_ABORTING)
                                        continue; //inaborting status not need check job data

                                    if (job.SamplingSlotFlag.Equals("1")) //just check sampling glass
                                    {
                                        if (job.RobortRTCFlag || job.RobotWIP.EQPRTCFlag) //RTC not quit
                                        {
                                            quitCST = false;
                                            break;
                                        }
                                        else
                                        {
                                            if (job.TrackingData == new string('0', job.TrackingData.Length)) //tracking data ='000..' not quit
                                            {
                                                quitCST = false;
                                                break;
                                            }
                                            else
                                            {
                                                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                    string.Format("[PORTNO={0}] [CSTID={1}] ALL JOBS TRACKING DATA=[1]."
                                                    , port.Data.PORTNO, port.File.CassetteID));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        else //BFG check
                        {
                            #region [ BFG Quit CST Check ]
                            //for avoid each position exchange miss 2016/01/05 cc.kuang
                            Robot curRobot = ObjectManager.RobotManager.GetRobot(eqp.Data.NODENO);
                            if (curRobot.File.Status != eRobotStatus.IDLE)
                                return;

                            foreach (Job job in lstJobinCST)
                            {
                                Job jb = ObjectManager.JobManager.GetJob(job.CassetteSequenceNo, job.JobSequenceNo);
                                if (jb.SamplingSlotFlag.Equals("1") && jb.TrackingData == new string('0', jb.TrackingData.Length))
                                {
                                    quitCST = false;
                                    break;
                                }
                            }

                            //add by yang 2017/6/14 要抓所有job做筛选以判断
                            //quitCST里的判断先保留,以后再整理
                            List<Job> jbs = ObjectManager.JobManager.GetJobs().Where(s => s.CassetteSequenceNo.Equals(port.File.CassetteSequenceNo)).ToList();
                            if (jbs.Where(s => !s.RemoveFlag && !s.ArraySpecial.RtcFlag.Equals("1")).Count() > 0)
                                quitCST = false;

                            if (quitCST) //if arm or eq has wip, no quit for check vcr NG RTC
                            {
                                string trxID = string.Format("{0}_Arm#{1}SingleSubstrateInfoBlock", "L2", "01");
                                Trx Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

                                if (Trx == null)
                                {
                                    this.LogError(MethodBase.GetCurrentMethod().Name + "()", trxID + " is Null Error !");
                                    return;
                                }

                                //<itemgroup name="Arm#02SingleSubstrateInfoBlock">
                                //  <item name="JobCassetteSequenceNo" woffset="0" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                                //  <item name="JobSequenceNo" woffset="1" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                                //  <item name="JobExist" woffset="2" boffset="0" wpoints="1" bpoints="16" expression="INT" />
                                //</itemgroup>

                                string trxEventGroupName = string.Format("{0}_EG_Arm#{1}SingleSubstrateInfoBlock", "L2", "01");
                                string trxEventName = string.Format("{0}_W_Arm#{1}SingleSubstrateInfoBlock", "L2", "01");
                                string trxItem_CSTSeq = "JobCassetteSequenceNo";
                                string trxItem_JobSeq = "JobSequenceNo";
                                string trxItem_JobExist = "JobExist";

                                string curRBArmCSTSeq = Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_CSTSeq].Value;
                                string curRBArmJobSeq = Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_JobSeq].Value;
                                string curRBArmJobExist = Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_JobExist].Value;
                                if (!curRBArmCSTSeq.Equals("0") || !curRBArmJobSeq.Equals("0") || !curRBArmJobExist.Equals("1"))
                                {
                                    armlastcstseq = curRBArmCSTSeq; //update newest arm job ,yang
                                    armlastjobseq = curRBArmJobSeq;
                                    return;
                                }

                                trxID = string.Format("{0}_Arm#{1}SingleSubstrateInfoBlock", "L2", "02");
                                Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

                                if (Trx == null)
                                {
                                    this.LogError(MethodBase.GetCurrentMethod().Name + "()", trxID + " is Null Error !");
                                    return;
                                }

                                trxEventGroupName = string.Format("{0}_EG_Arm#{1}SingleSubstrateInfoBlock", "L2", "02");
                                trxEventName = string.Format("{0}_W_Arm#{1}SingleSubstrateInfoBlock", "L2", "02");

                                curRBArmCSTSeq = Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_CSTSeq].Value;
                                curRBArmJobSeq = Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_JobSeq].Value;
                                curRBArmJobExist = Trx.EventGroups[trxEventGroupName].Events[trxEventName].Items[trxItem_JobExist].Value;
                                if (!curRBArmCSTSeq.Equals("0") || !curRBArmJobSeq.Equals("0") || !curRBArmJobExist.Equals("1"))
                                {
                                    armlastcstseq = curRBArmCSTSeq; //update newest arm job ,yang
                                    armlastjobseq = curRBArmJobSeq;
                                    return;
                                }

                                //get galss in Unit(0) smash
                                trxID = string.Format("{0}_JobEachPositionBlock", "L3");
                                Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;
                                if (Trx == null)
                                {
                                    this.LogError(MethodBase.GetCurrentMethod().Name + "()", trxID + " is Null Error !");
                                    return;
                                }
                                string cst_seq = Trx.EventGroups[0].Events[0].Items[0].Value;
                                string job_seq = Trx.EventGroups[0].Events[0].Items[1].Value;
                                if (!cst_seq.Equals("0") || !job_seq.Equals("0"))
                                {
                                    return;
                                }

                                //position的判断太敏感了,刚好RB清了position,而Unit还未on position,就会直接退cst
                                //Unit cut glass check ,add by yang 2017/6/8
                                string cst_seq2 = Trx.EventGroups[0].Events[0].Items[2].Value;
                                string job_seq2 = Trx.EventGroups[0].Events[0].Items[3].Value;
                                if (!cst_seq2.Equals("0") || !job_seq2.Equals("0"))
                                {
                                    Job job2 = ObjectManager.JobManager.GetJob(cst_seq2, job_seq2);
                                    if (job2 != null)
                                    {
                                        if (!job2.RemoveFlag) return;  //position未清,再去check job是否有remove
                                    }
                                }

                                //add by yang 2017/6/8
                                Job armlastjob = ObjectManager.JobManager.GetJob(armlastcstseq, armlastjobseq);
                                if (armlastjob != null && !armlastjob.ArraySpecial.RtcFlag.Equals("1") && !armlastjob.RemoveFlag) return;

                                Thread.Sleep(1000);//加个delay time for report product scarpped
                                armlastcstseq = string.Empty;
                                armlastjobseq = string.Empty;
                            }
                            #endregion
                        }
                    }
                    else if (port.File.Type == ePortType.LoadingPort)
                    {
                        lstJob = ObjectManager.JobManager.GetJobs(port.File.CassetteSequenceNo); //get all original job in cst
                        if (line.Data.FABTYPE == eFabType.CELL.ToString())
                        {
                            #region [CELL]
                            switch (line.Data.LINETYPE)
                            {
                                #region [Glass]
                                //By Sampling Flag
                                case eLineType.CELL.CCGAP:
                                case eLineType.CELL.CCTAM:
                                case eLineType.CELL.CCPDR:
                                case eLineType.CELL.CCPTH:
                                    foreach (Job job in lstJobinCST)
                                    {
                                        Job jb = lstJob.FirstOrDefault(j => job.CassetteSequenceNo == j.CassetteSequenceNo && job.JobSequenceNo == j.JobSequenceNo);
                                        if (jb.SamplingSlotFlag.Equals("1"))
                                        {
                                            quitCST = false;
                                            break;
                                        }
                                    }
                                    break;
                                #endregion
                                #region [Panel]
                                case eLineType.CELL.CCSOR://only full smapling
                                case eLineType.CELL.CCCHN:
                                case eLineType.CELL.CCRWT:
                                    if (lstJobinCST.Count > 0) //if have job in CST
                                    {
                                        quitCST = false;
                                        break;
                                    }
                                    break;
                                case eLineType.CELL.CCCRP:
                                case eLineType.CELL.CCCRP_2:
                                    if (lstJobinCST.Count == 0)
                                    {
                                    }
                                    else
                                    {
                                        foreach (Job job in lstJobinCST)
                                        {
                                            Job jb = lstJob.FirstOrDefault(j => job.CassetteSequenceNo == j.CassetteSequenceNo && job.JobSequenceNo == j.JobSequenceNo);
                                            if (jb.SamplingSlotFlag.Equals("1"))
                                            {
                                                if (!quitCST) break;
                                                //check no sampling flag before sampling flag
                                                int cstMaxRow = 6;//1~100,101~200......501~600
                                                string[] canFetch = new string[cstMaxRow];
                                                int cstMaxRowcount = 200;
                                                string trxID = string.Format("{0}_Port#{1}JobEachCassetteSlotPositionBlock", eqp.Data.NODENO, port.Data.PORTNO);
                                                Trx Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;
                                                int maxSlot = Trx.EventGroups[0].Events[0].Items.Count;
                                                int startPoint = 0;
                                                string errorLog = string.Empty;
                                                for (int i = 0; i < cstMaxRow; i++)
                                                {
                                                    if (!quitCST) break;
                                                    bool noJob = true;
                                                    canFetch[i] = "1";
                                                    #region [rowdata]
                                                    for (int j = 0; j < cstMaxRowcount; j += 4)
                                                    {
                                                        if (Trx.EventGroups[0].Events[0].Items[startPoint].Value.Trim() == "0" && Trx.EventGroups[0].Events[0].Items[startPoint + 2].Value.Trim() == "0")
                                                        {
                                                            //
                                                        }
                                                        else
                                                        {
                                                            noJob = false;
                                                            Job job1 = ObjectManager.JobManager.GetJob(Trx.EventGroups[0].Events[0].Items[startPoint].Value.Trim(), Trx.EventGroups[0].Events[0].Items[startPoint + 1].Value.Trim());
                                                            Job job2 = ObjectManager.JobManager.GetJob(Trx.EventGroups[0].Events[0].Items[startPoint + 2].Value.Trim(), Trx.EventGroups[0].Events[0].Items[startPoint + 3].Value.Trim());

                                                            if (job1 != null && job2 != null)
                                                            {
                                                                if (job1.SamplingSlotFlag.Equals("1") && job2.SamplingSlotFlag.Equals("1")) //表示還有panel 可取片 不能退
                                                                {
                                                                    quitCST = false;
                                                                    break;
                                                                }
                                                                else if (job1.SamplingSlotFlag.Equals("0") && job2.SamplingSlotFlag.Equals("1"))// 紀錄此ROW 無片抽片
                                                                {
                                                                    errorLog += string.Format("CST SeqNo [{0}] Can,t Fetch slot[{1}] Sample Flag Off & slot[{2}] Sample Flag ON.", job.CassetteSequenceNo, job1.JobSequenceNo, job2.JobSequenceNo);
                                                                }
                                                                else if (job1.SamplingSlotFlag.Equals("1") && job2.SamplingSlotFlag.Equals("0"))
                                                                {
                                                                    errorLog += string.Format("CST SeqNo [{0}] Can,t Fetch slot[{1}] Sample Flag ON & slot[{2}] Sample Flag Off.", job.CassetteSequenceNo, job1.JobSequenceNo, job2.JobSequenceNo);
                                                                }
                                                                else
                                                                {
                                                                    errorLog += string.Format("CST SeqNo [{0}] Can,t Fetch slot[{1}] Sample Flag Off & slot[{2}] Sample Flag Off.", job.CassetteSequenceNo, job1.JobSequenceNo, job2.JobSequenceNo);
                                                                }
                                                                canFetch[i] = "0";
                                                                break;
                                                            }
                                                            else if (job1 != null && job2 == null)
                                                            {
                                                                if (job1.SamplingSlotFlag.Equals("1")) //表示還有panel 可取片 不能退
                                                                {
                                                                    quitCST = false;
                                                                    break;
                                                                }
                                                                else// 紀錄此ROW 無片抽片
                                                                {
                                                                    errorLog += string.Format("CST SeqNo [{0}] Can,t Fetch slot[{1}] Sample Flag Off.", job.CassetteSequenceNo, job1.JobSequenceNo);
                                                                    canFetch[i] = "0";
                                                                    break;
                                                                }
                                                            }
                                                            else if (job1 == null && job2 != null)
                                                            {
                                                                if (job2.SamplingSlotFlag.Equals("1")) //表示還有panel 可取片 不能退
                                                                {
                                                                    quitCST = false;
                                                                    break;
                                                                }
                                                                else// 紀錄此ROW 無片抽片
                                                                {
                                                                    errorLog += string.Format("CST SeqNo [{0}] Can,t Fetch slot[{1}] Sample Flag Off.", job.CassetteSequenceNo, job2.JobSequenceNo);
                                                                    canFetch[i] = "0";
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    #endregion
                                                    if (noJob) canFetch[i] = "0";
                                                    startPoint += cstMaxRowcount;
                                                }
                                                foreach (string fetch in canFetch)
                                                {
                                                    if (fetch.Equals("1"))//預設6個都無法抽片,只要1個能抽片就不能退  
                                                    {
                                                        quitCST = false;
                                                        break;
                                                    }
                                                }
                                                if (quitCST)//先紀錄LOG&warm 但不退
                                                {
                                                    quitCST = false;
                                                    Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("No Job Can Fetch [{0}]", errorLog));

                                                }
                                            }
                                        }
                                    }
                                    break;
                                #endregion
                                default:
                                    break;
                            }
                            #endregion
                        }
                        else
                        {
                            switch (line.File.IndexOperMode)
                            {

                                case eINDEXER_OPERATION_MODE.THROUGH_MODE:
                                    {
                                        quitCST = ThroughModeSpecialRule(line, eqp, port, lstJobinCST);
                                        break;
                                    }
                                default:
                                    {
                                        foreach (Job job in lstJobinCST)
                                        {
                                            Job jb = lstJob.FirstOrDefault(j => job.CassetteSequenceNo == j.CassetteSequenceNo && job.JobSequenceNo == j.JobSequenceNo);
                                            if (jb == null) //if job not in original cst, can't quit 2015/12/23 cc.kuang
                                            {
                                                quitCST = false;
                                                break;
                                            }
                                            if (jb.SamplingSlotFlag.Equals("1"))
                                            {
                                                quitCST = false;
                                                break;
                                            }
                                        }
                                        msg = string.Format("[EQUIPMENT={0}] [PORTNO={1}] [PORTTYPE={2}] [CSTID={3}] Sampling Glass IS EMPTY."
                                            , eqp.Data.NODENO, port.Data.PORTNO, port.File.Type, port.File.CassetteID);

                                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", msg);
                                        break;
                                    }
                            }
                        }
                    }
                    else if (port.File.Type == ePortType.UnloadingPort)
                    {
                        switch (line.Data.FABTYPE)
                        {
                            case "CELL":
                                {
                                    //if (lstJobinCST.Count != port.Data.MAXCOUNT) //sy edit 20160215
                                    if (lstJobinCST.Count != int.Parse(port.File.MaxSlotCount))
                                    {
                                        quitCST = false;
                                    }
                                    break;
                                }
                            case "CF":
                                {
                                    if (lstJobinCST.Count != port.Data.MAXCOUNT)
                                    {
                                        quitCST = false;
                                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("quitCST=[{0}],lstJobinCST.Count=[{1}],port.Data.MAXCOUNT=[{2}] is not the same,PortNo=[{3}],CSTID=[{4}]", quitCST, lstJobinCST.Count, port.Data.MAXCOUNT, port.Data.PORTNO, port.File.CassetteID));
                                    }
                                    else
                                    {
                                        msg = string.Format("[EQUIPMENT={0}] [PORTNO={1}] [PORTTYPE={2}] [CSTID={3}] [JobInCST={4}] AND [PortMaxCount={5}] ARE THE SAME."
                                            , eqp.Data.NODENO, port.Data.PORTNO, port.File.Type, port.File.CassetteID, lstJobinCST.Count, port.Data.MAXCOUNT);

                                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", msg);
                                    }

                                    break;
                                }
                            default:
                                {
                                    if (lstJobinCST.Count != port.Data.MAXCOUNT)
                                    {
                                        quitCST = false;
                                    }
                                    else
                                    {
                                        msg = string.Format("[EQUIPMENT={0}] [PORTNO={1}] [PORTTYPE={2}] [CSTID={3}] [JobInCST={4}] AND [PortMaxCount={5}] ARE THE SAME."
                                                , eqp.Data.NODENO, port.Data.PORTNO, port.File.Type, port.File.CassetteID, lstJobinCST.Count, port.Data.MAXCOUNT);

                                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", msg);
                                    }
                                    break;
                                }
                        }
                    }

                    if (quitCST)
                    {
                        if (!string.IsNullOrEmpty(msgInfo)) Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", msgInfo);//20161116 sy add
                        if (port.File.CassetteStatus == eCassetteStatus.IN_ABORTING)
                        {
                            CassetteProcessAbort(eqp.Data.NODENO, port.Data.PORTNO);
                        }
                        else
                        {
                            CassetteProcessEnd(eqp.Data.NODENO, port.Data.PORTNO);
                        }
                    }
                }

                //Changer Plan Check
                if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE || line.File.LineOperMode == eMES_LINEOPERMODE.EXCHANGE)
                {
                    //   ChangerPlanEndScan(line, eqp);   //chn_old

                    if (line.File.HostMode == eHostMode.OFFLINE && !Debugger.IsAttached)
                    {
                        ChangerPlanEndScan(line, eqp);
                    }
                    else
                    {
                        ChangerPlanEndScanForOnline(line, eqp);
                    }
                }
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return;
        }
        public bool ThroughModeSpecialRule(Line line, Equipment eqp, Port port, IList<Job> jobInCST)
        {
            try
            {
                bool quitCST = true;
                string msg;
                IList<Job> jobs = ObjectManager.JobManager.GetJobs(port.File.CassetteSequenceNo); //get all original job in cst

                switch (line.Data.LINETYPE)
                {
                    #region Repair Line Loader Special Rule
                    case eLineType.CF.FCREP_TYPE1:
                    case eLineType.CF.FCREP_TYPE2:
                    case eLineType.CF.FCREP_TYPE3:
                        {
                            foreach (Job job in jobs)
                            {
                                if (job.RemoveFlag) //Remove glass not need check
                                    continue;

                                if (jobInCST.FirstOrDefault(j => job.CassetteSequenceNo == j.CassetteSequenceNo
                                    && job.JobSequenceNo == j.JobSequenceNo) == null) //not in cst's glass not quit
                                {
                                    quitCST = false;
                                    if (!quitCST)
                                    {
                                        //Glass isn't in loading CST and glass can't back to loading CST(Job Judge != 1:OK), quit loading CST
                                        if (job.ToCstID.Trim().Length > 0 && job.ToSlotNo.Trim().Length > 0
                                            && job.CurrentEQPNo == eqp.Data.NODENO && job.JobJudge != "1")

                                            quitCST = true;
                                    }
                                    if (!quitCST)
                                        break;
                                }
                                else //glass in cst check
                                {
                                    if (port.File.CassetteStatus == eCassetteStatus.IN_ABORTING)
                                        continue; //inaborting status not need check job data

                                    if (job.SamplingSlotFlag.Equals("1")) //just check sampling glass
                                    {
                                        if (job.RobortRTCFlag) //RTC can't quit
                                        {
                                            quitCST = false;
                                            break;
                                        }
                                        else
                                        {
                                            if (job.JobJudge != "1") //只要還有Judge != "1:OK"的玻璃在CST內，can't quit CST
                                            {
                                                quitCST = false;
                                                break;
                                            }
                                            else
                                            {
                                                msg = string.Format("[EQUIPMENT={0}] [INDEXEROPERATIONMODE={1}] [PORTNO={2}] [PORTTYPE={3}] [CSTID={4}] ALL JOBS JUDGE NOT OK."
                                               , eqp.Data.NODENO, line.File.IndexOperMode, port.Data.PORTNO, port.File.Type, port.File.CassetteID);

                                                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", msg);
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    #endregion

                    #region Macro/Offline AOI Line Loader Special Rule
                    case eLineType.CF.FCMAC_TYPE1:
                    case eLineType.CF.FCAOI_TYPE1:
                        {
                            foreach (Job job in jobs)
                            {
                                if (job.RemoveFlag) //Remove glass not need check
                                    continue;

                                if (jobInCST.FirstOrDefault(j => job.CassetteSequenceNo == j.CassetteSequenceNo
                                    && job.JobSequenceNo == j.JobSequenceNo) == null) //not in cst's glass not quit
                                {
                                    quitCST = false;
                                    if (!quitCST)
                                    {
                                        if (job.ToCstID.Trim().Length > 0 && job.ToSlotNo.Trim().Length > 0 && job.CurrentEQPNo == eqp.Data.NODENO && job.JobJudge != "1")
                                            quitCST = true;
                                    }
                                    if (!quitCST)
                                        break;
                                }
                                else //glass in cst check
                                {
                                    if (port.File.CassetteStatus == eCassetteStatus.IN_ABORTING)
                                        continue; //inaborting status not need check job data

                                    if (job.SamplingSlotFlag.Equals("1")) //just check sampling glass
                                    {
                                        if (job.RobortRTCFlag) //RTC not quit
                                        {
                                            quitCST = false;
                                            break;
                                        }
                                        else
                                        {
                                            if (job.TrackingData == new string('0', job.TrackingData.Length)) //還有玻璃還沒出來，can't quit CST
                                            {
                                                quitCST = false;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                            break;
                        }
                    #endregion
                }
                return quitCST;
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return false;
        }

        public void CassetteStoreQTimeProcessEnd()
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLines()[0];
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP("L2");
                List<Port> tarPorts = ObjectManager.PortManager.GetPorts(eqp.Data.NODEID).Where(
                    p => p.File.Type == ePortType.UnloadingPort && p.File.Mode != ePortMode.Mismatch).ToList();
                string quitPortNo = string.Empty;

                //計算哪個Target Port最久未被使用，並取出最久未被使用的Port
                List<CSTEnd> cstEndList = new List<CSTEnd>();
                foreach (Port p in tarPorts)
                {
                    if (p.File.StoreInDateTime.Equals(DateTime.MinValue))
                    {
                        quitPortNo = p.Data.PORTNO;
                        break;
                    }
                    TimeSpan ts = new TimeSpan(DateTime.Now.Ticks - p.File.StoreInDateTime.Ticks);

                    CSTEnd cstEnd = new CSTEnd();
                    cstEnd.PortNo = p.Data.PORTNO;
                    cstEnd.TS = ts;
                    cstEndList.Add(cstEnd);
                }
                CSTEnd max = null;
                foreach (CSTEnd cst_end in cstEndList)
                {
                    if (max == null) max = cst_end;
                    else
                    {
                        if (cst_end.TS.Ticks > max.TS.Ticks)
                            max = cst_end;
                    }
                }
                if (max != null)
                {
                    quitPortNo = max.PortNo;
                    Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                             string.Format("IN SORTER MODE, PORT_NO[{0}] IS THE LONGEST NO-USED CST.", quitPortNo));

                    CassetteProcessEnd(eqp.Data.NODENO, quitPortNo);
                }
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return;

        }

        public void SorterUnloaderCheck(Equipment eqp, Port port, string TrackKey)
        {
            try
            {
                IList<Port> souPorts = ObjectManager.PortManager.GetPorts(eqp.Data.NODEID).Where(p => p.File.Type == ePortType.LoadingPort).ToList();

                string msg = string.Empty;
                foreach (Port p in souPorts)
                {
                    for (int i = 0; i < p.File.SorterJobGrade.Count; i++)
                    {
                        if (port.File.MappingGrade == p.File.SorterJobGrade[i].SorterGrade
                            && !p.File.SorterJobGrade[i].GradeCount.Equals(0))
                        {
                            p.File.SorterJobGrade[i].ToMES = false;

                            msg = string.Format("EQUIPMENT=[{0}] [BCS <- EQP][{1}] PORTNO=[{2}], GRADE=[{3}] COUNT IS NOT 0. SO FLAG OF GRADE=[{4}] NEED TO SET FALSE."
                               , eqp.Data.NODENO, TrackKey, p.Data.NODENO, p.File.SorterJobGrade[i].SorterGrade, p.File.SorterJobGrade[i].SorterGrade);

                            Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("{0} ", msg));
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }

        public void PlanReadyStatusCheck(Line line, Equipment eqp, string TrackKey)
        {
            bool bPlanReady = true;
            string PlanID = string.Empty;
            string standbyPlanID = string.Empty;
            IList<SLOTPLAN> Plans;
            bool bPlanCancel = false;
            List<slotPosition> lstSourceCSTJobs = new List<slotPosition>(); //list for real slot
            List<slotPosition> lstTargetCSTJobs = new List<slotPosition>(); //list for empty slot
            string strIssueCassetteID = "";
            string strPlanCancelReason = "";
            string strErr = "";

            Plans = ObjectManager.PlanManager.GetProductPlans(out PlanID);
            if (PlanID.Trim().Length > 0 && line.File.PlanStatus == ePLAN_STATUS.REQUEST)
            {
                //取得source CST & target CST List for Assign to Each Port
                var lsSourceCassette = from slot in Plans group slot by slot.SOURCE_CASSETTE_ID into g select g.First().SOURCE_CASSETTE_ID;
                var lsTargetCassette = from slot in Plans group slot by slot.TARGET_CASSETTE_ID into g select g.First().TARGET_CASSETTE_ID;
                //Check each source CSTID
                foreach (string cstid in lsSourceCassette)
                {
                    if (ObjectManager.PortManager.GetPorts(eqp.Data.NODEID).FirstOrDefault(
                        p => p.File.CassetteID.Trim().Equals(cstid.Trim()) && p.File.Status == ePortStatus.LC
                        && (p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING)) == null)
                    {
                        bPlanReady = false;
                        break;
                    }
                }
                //Check each target CSTID
                foreach (string cstid in lsTargetCassette)
                {
                    if (bPlanReady == false)
                        break;
                    if (ObjectManager.PortManager.GetPorts(eqp.Data.NODEID).FirstOrDefault(
                        p => p.File.CassetteID.Trim().Equals(cstid.Trim()) && p.File.Status == ePortStatus.LC
                        && (p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING)) == null)
                    {
                        bPlanReady = false;
                        break;
                    }
                }

                if (bPlanReady == true)
                {
                    //1.check Plan Source CST
                    foreach (string cstid in lsSourceCassette)
                    {
                        Port planPort;
                        planPort = ObjectManager.PortManager.GetPorts(eqp.Data.NODEID).FirstOrDefault(
                            p => p.File.CassetteID.Trim().Equals(cstid.Trim()) && p.File.Status == ePortStatus.LC
                            && (p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING));
                        if (null == planPort)
                        {
                            strErr = string.Format("CSTID=[{0}] CAN'T FIND Match Source Port Status(WAITFORPROCESS, INPROCESS) Error !", cstid);
                            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                            bPlanCancel = true;
                            break;
                        }

                        string trxName = string.Format("{0}_Port#{1}JobEachCassetteSlotPositionBlock", eqp.Data.NODENO, planPort.Data.PORTNO);
                        Trx trxCassetteSlotPosition = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                        if (trxCassetteSlotPosition == null)
                        {
                            strErr = string.Format("{0} Read Error !", trxName);
                            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                            bPlanCancel = true;
                            break;
                        }

                        trxName = string.Format("{0}_Port#{1}JobEachCassetteSlotExistsBlock", eqp.Data.NODENO, planPort.Data.PORTNO);
                        Trx trxCassetteSlotExist = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                        if (trxCassetteSlotExist == null)
                        {
                            strErr = string.Format("{0} Read Error !", trxName);
                            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                            bPlanCancel = true;
                            break;
                        }

                        string allSlotExistInfo = trxCassetteSlotExist.EventGroups[0].Events[0].Items[0].Value;
                        for (int i = 0; i < planPort.Data.MAXCOUNT; i++) //check slot position & slot exixt 
                        {
                            int job_ExistInfo = int.Parse(allSlotExistInfo.Substring(i, 1));
                            string cst_seq = trxCassetteSlotPosition.EventGroups[0].Events[0].Items[2 * i].Value;
                            string job_seq = trxCassetteSlotPosition.EventGroups[0].Events[0].Items[2 * i + 1].Value;
                            if (job_ExistInfo == 1)
                            {
                                if (cst_seq.Equals("0") || job_seq.Equals("0"))
                                {
                                    strErr = string.Format("CSTID=[{0}] Slot=[{1}] GlassExist, But Slot Position JobData=[{2},{3}] Not Match Error !", cstid, i + 1, cst_seq, job_seq);
                                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                                    bPlanCancel = true;
                                    break;
                                }
                                Job jb = ObjectManager.JobManager.GetJob(cst_seq, job_seq);
                                slotPosition sltpos = new slotPosition();
                                if (null == jb)
                                {
                                    strErr = string.Format("CSTSEQUENCE=[{0}] JOBSEQUENCE=[{1}] Object Find Error !", cst_seq, job_seq);
                                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                                    bPlanCancel = true;
                                    break;
                                }
                                sltpos.cst_id = cstid;
                                sltpos.slotno = i + 1;
                                sltpos.cst_sequence = cst_seq;
                                sltpos.job_sequence = job_seq;
                                sltpos.glass_id = jb.GlassChipMaskBlockID;
                                lstSourceCSTJobs.Add(sltpos);
                            }
                            else
                            {
                                if (!cst_seq.Equals("0") || !job_seq.Equals("0"))
                                {
                                    strErr = string.Format("CSTID=[{0}] Slot=[{1}] GlassNotExist, But Slot Position JobData=[{2},{3}] Not Match Error !", cstid, i + 1, cst_seq, job_seq);
                                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                                    bPlanCancel = true;
                                    break;
                                }
                            }
                        }

                        if (bPlanCancel)
                            break;
                    }

                    //2.check Plan Target CST
                    foreach (string cstid in lsTargetCassette)
                    {
                        if (bPlanCancel)
                            break;

                        Port planPort;
                        planPort = ObjectManager.PortManager.GetPorts(eqp.Data.NODEID).FirstOrDefault(
                            p => p.File.CassetteID.Trim().Equals(cstid.Trim()) && p.File.Status == ePortStatus.LC
                            && (p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING));
                        if (null == planPort)
                        {
                            strErr = string.Format("CSTID=[{0}] CAN'T FIND Match Target Port Status(WAITFORPROCESS, INPROCESS) Error !", cstid);
                            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                            bPlanCancel = true;
                            break;
                        }

                        string trxName = string.Format("{0}_Port#{1}JobEachCassetteSlotPositionBlock", eqp.Data.NODENO, planPort.Data.PORTNO);
                        Trx trxCassetteSlotPosition = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                        if (trxCassetteSlotPosition == null)
                        {
                            strErr = string.Format("{0} Read Error !", trxName);
                            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                            bPlanCancel = true;
                            break;
                        }

                        trxName = string.Format("{0}_Port#{1}JobEachCassetteSlotExistsBlock", eqp.Data.NODENO, planPort.Data.PORTNO);
                        Trx trxCassetteSlotExist = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                        if (trxCassetteSlotExist == null)
                        {
                            strErr = string.Format("{0} Read Error !", trxName);
                            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                            bPlanCancel = true;
                            break;
                        }

                        string allSlotExistInfo = trxCassetteSlotExist.EventGroups[0].Events[0].Items[0].Value;
                        for (int i = 0; i < planPort.Data.MAXCOUNT; i++) //check slot position & slot exixt 
                        {
                            int job_ExistInfo = int.Parse(allSlotExistInfo.Substring(i, 1));
                            string cst_seq = trxCassetteSlotPosition.EventGroups[0].Events[0].Items[2 * i].Value;
                            string job_seq = trxCassetteSlotPosition.EventGroups[0].Events[0].Items[2 * i + 1].Value;
                            if (job_ExistInfo == 1)
                            {
                                if (cst_seq.Equals("0") || job_seq.Equals("0"))
                                {
                                    strErr = string.Format("CSTID=[{0}] Slot=[{1}] GlassExist, But Slot Position JobData=[{2},{3}] Not Match Error !", cstid, i + 1, cst_seq, job_seq);
                                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                                    bPlanCancel = true;
                                    break;
                                }
                            }
                            else
                            {
                                if (!cst_seq.Equals("0") || !job_seq.Equals("0"))
                                {
                                    strErr = string.Format("CSTID=[{0}] Slot=[{1}] GlassNotExist, But Slot Position JobData=[{2},{3}] Not Match Error !", cstid, i + 1, cst_seq, job_seq);
                                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                                    bPlanCancel = true;
                                    break;
                                }
                                slotPosition sltpos = new slotPosition();
                                sltpos.cst_id = cstid;
                                sltpos.slotno = i + 1;
                                sltpos.cst_sequence = cst_seq;
                                sltpos.job_sequence = job_seq;
                                lstTargetCSTJobs.Add(sltpos);
                            }
                        }

                        if (bPlanCancel)
                            break;
                    }

                    //3.check Plan Source&Target Glass
                    foreach (SLOTPLAN sltpln in Plans)
                    {
                        if (bPlanCancel)
                            break;

                        //check plan source glass
                        if (sltpln.SLOTNO > 0)
                        {
                            if (lstSourceCSTJobs.FirstOrDefault(cstglass => sltpln.SOURCE_CASSETTE_ID.Trim() == cstglass.cst_id.Trim() && cstglass.slotno == sltpln.SLOTNO) == null)
                            {
                                {
                                    strErr = string.Format("PLAN CSTID=[{0}] Source Slot=[{1}] Can't Find in CST Error !", sltpln.SOURCE_CASSETTE_ID, sltpln.SLOTNO);
                                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                                    bPlanCancel = true;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            if (lstSourceCSTJobs.FirstOrDefault(cstglass => sltpln.SOURCE_CASSETTE_ID.Trim() == cstglass.cst_id.Trim() && cstglass.glass_id.Trim() == sltpln.PRODUCT_NAME.Trim()) == null)
                            {
                                {
                                    strErr = string.Format("PLAN CSTID=[{0}] Source GlassID=[{1}] Can't Find in CST Error !", sltpln.SOURCE_CASSETTE_ID, sltpln.PRODUCT_NAME);
                                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                                    bPlanCancel = true;
                                    break;
                                }
                            }
                        }

                        if (bPlanCancel)
                            break;

                        //check plan target glass
                        if (sltpln.TARGET_SLOTNO.Length == 0 || int.Parse(sltpln.TARGET_SLOTNO) > 0)
                        {
                            if (lstTargetCSTJobs.FirstOrDefault(cstglass => sltpln.TARGET_CASSETTE_ID.Trim() == cstglass.cst_id.Trim() && cstglass.slotno == int.Parse(sltpln.TARGET_SLOTNO)) == null)
                            {
                                {
                                    strErr = string.Format("PLAN CSTID=[{0}] Target Slot=[{1}] in [{2}] isn't EMPT Error !", sltpln.SOURCE_CASSETTE_ID, sltpln.TARGET_SLOTNO, sltpln.TARGET_CASSETTE_ID);
                                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                                    bPlanCancel = true;
                                    break;
                                }
                            }
                        }
                    }

                    if (!bPlanCancel) //check plan target glass totoal count
                    {
                        foreach (string cst_id in lsTargetCassette)
                        {
                            IEnumerable<string> lstTarget = from slot in Plans where slot.TARGET_CASSETTE_ID.Trim() == cst_id.Trim() select slot.TARGET_CASSETTE_ID;
                            IEnumerable<string> lstCst = from slot in lstTargetCSTJobs where slot.cst_id.Trim() == cst_id.Trim() select slot.cst_id;
                            if (lstTarget.Count() > lstCst.Count())
                            {
                                strErr = string.Format("PLAN Target CSTID=[{0}] EMPT Slot Count[{1}] < Plan Slot Count[{2}] Error !", cst_id, lstCst.Count(), lstTarget.Count());
                                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                                bPlanCancel = true;
                                break;
                            }
                        }
                    }

                    if (bPlanCancel)
                    {
                        line.File.PlanStatus = ePLAN_STATUS.CANCEL;
                        ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                        if (line.File.HostMode != eHostMode.OFFLINE)
                        {
                            Invoke(eServiceName.MESService, "ChangePlanCanceled", new object[6] { TrackKey, line.Data.LINEID, PlanID, "0", strPlanCancelReason, strIssueCassetteID });
                            //offline not cancel cst
                            Invoke(eServiceName.PortService, "CassettePlanCancel", new object[4] { lsSourceCassette.ToList<string>(), lsTargetCassette.ToList<string>(), 1, TrackKey });
                        }
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "Plan[" + PlanID + "] Status Change To Cancel!");
                    }
                    else
                    {
                        line.File.PlanStatus = ePLAN_STATUS.READY;
                        ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                        this.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "Plan[" + PlanID + "] Status Change To Ready!");
                    }
                    Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
                }
            }
        }

        // add by box.zhai 2016/12/21 for Online Change Plan 不同逻辑
        // 只要有一组S-T的对应关系Plan Status 就 Ready
        // 每次去Check Current P1 是否Ready，如果没有就去Check Standby Plan2是否Ready，如果P2 Ready就切成Current Plan，P1切换成Standby Plan，Status 为Request
        // marked by yang : plan status check condition:1.new cst 2.last store event for putput  or store for put
        public void PlanReadyStatusCheckForOnline(Line line, Equipment eqp, string TrackKey, string CheckCondition)
        {
            bool bPlanReady = false;
            string planID = string.Empty;
            string standbyPlanID = string.Empty;
            IList<SLOTPLAN> Plans;
            IList<SLOTPLAN> StandbyPlans;

            Plans = ObjectManager.PlanManager.GetProductPlans(out planID);
            if (CheckCondition.ToUpper().Contains("STORE"))
            {
                if (line.File.PlanStatus == ePLAN_STATUS.START)  //add by yang 2017/5/5  update Plan Status when Plan Start
                    line.File.PlanStatus = ePLAN_STATUS.WAITING; //针对Store后做Ready Check:先把Current Plan Status改为WAITING（防止RB在判断中又去取了玻璃）
                ObjectManager.LineManager.EnqueueSave(line.File);
                ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
            }
            // 上CST、退CST以及Store的时候都需要去Check Plan的Ready情况，所以添加PlanStatus为Start   --old 
            if (planID.Trim().Length > 0 && line.File.PlanStatus != ePLAN_STATUS.READY && line.File.PlanStatus != ePLAN_STATUS.START)
            // modify by yang 2017/3/10
            {
                bPlanReady = PlanReadyStatusCheckForOnlineExecute(line, eqp, TrackKey, true);
                if (!bPlanReady)
                {
                    StandbyPlans = ObjectManager.PlanManager.GetProductPlansStandby(out standbyPlanID);
                    if (standbyPlanID.Trim().Length > 0)
                    {
                        bPlanReady = PlanReadyStatusCheckForOnlineExecute(line, eqp, TrackKey, false);
                        if (bPlanReady)
                        {
                            Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
                        }
                    }
                }
            }
        }

        public bool PlanReadyStatusCheckForOnlineExecute(Line line, Equipment eqp, string TrackKey, bool isCurrentPlan)
        {
            bool bPlanReady = false;
            string PlanID = string.Empty;
            IList<SLOTPLAN> Plans;
            bool bPlanCancel = false;
            List<slotPosition> lstSourceCSTJobs = new List<slotPosition>(); //list of real slot for check 满足的一组 S-T中的ProductList中Source CST是否有实物
            List<slotPosition> lstTargetCSTJobs = new List<slotPosition>(); //list of empty slot for check 满足的一组 S-T中的ProductList中Target CST是否没有实物
            string strIssueCassetteID = "";
            string strPlanCancelReason = "";
            string strErr = "";
            if (isCurrentPlan)
            {
                Plans = ObjectManager.PlanManager.GetProductPlans(out PlanID);
            }
            else
            {
                Plans = ObjectManager.PlanManager.GetProductPlansStandby(out PlanID);
            }
            #region Plan是否Ready
            //取得Source CST & Target CST List for Assign to Each Port
            var lsSourceCassette = from slot in Plans group slot by slot.SOURCE_CASSETTE_ID into g select g.First().SOURCE_CASSETTE_ID;
            var lsTargetCassette = from slot in Plans group slot by slot.TARGET_CASSETTE_ID into g select g.First().TARGET_CASSETTE_ID;
            //Check Plan中是否至少有一组满足条件的Source&Target CST在Port上并返回Source&Target CSTID，以便来Check 对应CST中的资料是否有误
            string sourceCstID = string.Empty;
            string targetCstID = string.Empty;
            foreach (string sCstid in lsSourceCassette)
            {
                if (ObjectManager.PortManager.GetPorts(eqp.Data.NODEID).FirstOrDefault(
                    p => p.File.CassetteID.Trim().Equals(sCstid.Trim()) && p.File.Status == ePortStatus.LC
                    && (p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING)) != null)
                {
                    sourceCstID = sCstid;
                }
                IList<SLOTPLAN> scourcePlans;
                if (isCurrentPlan)
                {
                    scourcePlans = ObjectManager.PlanManager.GetProductPlansByCstID(PlanID, sourceCstID);
                }
                else
                {
                    scourcePlans = ObjectManager.PlanManager.GetProductStandByPlansByCstID(PlanID, sourceCstID);
                }
                lsTargetCassette = from slot in scourcePlans group slot by slot.TARGET_CASSETTE_ID into g select g.First().TARGET_CASSETTE_ID;
                foreach (string tCstid in lsTargetCassette)
                {
                    if (ObjectManager.PortManager.GetPorts(eqp.Data.NODEID).FirstOrDefault(
                        p => p.File.CassetteID.Trim().Equals(tCstid.Trim()) && p.File.Status == ePortStatus.LC
                        && (p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING)) != null)
                    {
                        bPlanReady = true;
                        targetCstID = tCstid;
                        break;
                    }
                }
            }
            #endregion

            if (bPlanReady == true)
            {
                string trxName;
                string allSlotExistInfo;
                Port planPort;
                Trx trxCassetteSlotPosition;
                Trx trxCassetteSlotExist;

                #region 1.Check Plan Source CST

                planPort = ObjectManager.PortManager.GetPorts(eqp.Data.NODEID).FirstOrDefault(
                    p => p.File.CassetteID.Trim().Equals(sourceCstID.Trim()) && p.File.Status == ePortStatus.LC
                    && (p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING));
                if (null == planPort)
                {
                    strErr = string.Format("CSTID=[{0}] CAN'T FIND Match Source Port Status(WAITFORPROCESS, INPROCESS) Error !", sourceCstID);
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                    bPlanCancel = true;
                }

                trxName = string.Format("{0}_Port#{1}JobEachCassetteSlotPositionBlock", eqp.Data.NODENO, planPort.Data.PORTNO);
                trxCassetteSlotPosition = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                if (trxCassetteSlotPosition == null)
                {
                    strErr = string.Format("{0} Read Error !", trxName);
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                    bPlanCancel = true;
                }

                trxName = string.Format("{0}_Port#{1}JobEachCassetteSlotExistsBlock", eqp.Data.NODENO, planPort.Data.PORTNO);
                trxCassetteSlotExist = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                if (trxCassetteSlotExist == null)
                {
                    strErr = string.Format("{0} Read Error !", trxName);
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                    bPlanCancel = true;
                }

                allSlotExistInfo = trxCassetteSlotExist.EventGroups[0].Events[0].Items[0].Value;
                //Check slot position & slot exist 
                for (int i = 0; i < planPort.Data.MAXCOUNT; i++)
                {
                    int job_ExistInfo = int.Parse(allSlotExistInfo.Substring(i, 1));
                    string cst_seq = trxCassetteSlotPosition.EventGroups[0].Events[0].Items[2 * i].Value;
                    string job_seq = trxCassetteSlotPosition.EventGroups[0].Events[0].Items[2 * i + 1].Value;
                    if (job_ExistInfo == 1)
                    {
                        if (cst_seq.Equals("0") || job_seq.Equals("0"))
                        {
                            strErr = string.Format("CSTID=[{0}] Slot=[{1}] GlassExist, But Slot Position JobData=[{2},{3}] Not Match Error !", sourceCstID, i + 1, cst_seq, job_seq);
                            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                            bPlanCancel = true;
                            break;
                        }
                        Job jb = ObjectManager.JobManager.GetJob(cst_seq, job_seq);
                        slotPosition sltpos = new slotPosition();
                        if (null == jb)
                        {
                            strErr = string.Format("CSTSEQUENCE=[{0}] JOBSEQUENCE=[{1}] Object Find Error !", cst_seq, job_seq);
                            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                            bPlanCancel = true;
                            break;
                        }
                        sltpos.cst_id = sourceCstID;
                        sltpos.slotno = i + 1;
                        sltpos.cst_sequence = cst_seq;
                        sltpos.job_sequence = job_seq;
                        sltpos.glass_id = jb.GlassChipMaskBlockID;
                        lstSourceCSTJobs.Add(sltpos);
                    }
                    else
                    {
                        if (!cst_seq.Equals("0") || !job_seq.Equals("0"))
                        {
                            strErr = string.Format("CSTID=[{0}] Slot=[{1}] GlassNotExist, But Slot Position JobData=[{2},{3}] Not Match Error !", sourceCstID, i + 1, cst_seq, job_seq);
                            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                            bPlanCancel = true;
                            break;
                        }
                    }
                }
                #endregion

                #region 2.Check Plan Target CST
                if (!bPlanCancel)
                {
                    planPort = ObjectManager.PortManager.GetPorts(eqp.Data.NODEID).FirstOrDefault(
                        p => p.File.CassetteID.Trim().Equals(targetCstID.Trim()) && p.File.Status == ePortStatus.LC
                        && (p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING));
                    if (null == planPort)
                    {
                        strErr = string.Format("CSTID=[{0}] CAN'T FIND Match Target Port Status(WAITFORPROCESS, INPROCESS) Error !", targetCstID);
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                        bPlanCancel = true;
                    }

                    trxName = string.Format("{0}_Port#{1}JobEachCassetteSlotPositionBlock", eqp.Data.NODENO, planPort.Data.PORTNO);
                    trxCassetteSlotPosition = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                    if (trxCassetteSlotPosition == null)
                    {
                        strErr = string.Format("{0} Read Error !", trxName);
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                        bPlanCancel = true;
                    }

                    trxName = string.Format("{0}_Port#{1}JobEachCassetteSlotExistsBlock", eqp.Data.NODENO, planPort.Data.PORTNO);
                    trxCassetteSlotExist = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                    if (trxCassetteSlotExist == null)
                    {
                        strErr = string.Format("{0} Read Error !", trxName);
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                        bPlanCancel = true;
                    }

                    allSlotExistInfo = trxCassetteSlotExist.EventGroups[0].Events[0].Items[0].Value;
                    //Check slot position & slot exist 
                    for (int i = 0; i < planPort.Data.MAXCOUNT; i++)
                    {
                        int job_ExistInfo = int.Parse(allSlotExistInfo.Substring(i, 1));
                        string cst_seq = trxCassetteSlotPosition.EventGroups[0].Events[0].Items[2 * i].Value;
                        string job_seq = trxCassetteSlotPosition.EventGroups[0].Events[0].Items[2 * i + 1].Value;
                        if (job_ExistInfo == 1)
                        {
                            if (cst_seq.Equals("0") || job_seq.Equals("0"))
                            {
                                strErr = string.Format("CSTID=[{0}] Slot=[{1}] GlassExist, But Slot Position JobData=[{2},{3}] Not Match Error !", targetCstID, i + 1, cst_seq, job_seq);
                                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                                bPlanCancel = true;
                                break;
                            }
                        }
                        else
                        {
                            if (!cst_seq.Equals("0") || !job_seq.Equals("0"))
                            {
                                strErr = string.Format("CSTID=[{0}] Slot=[{1}] GlassNotExist, But Slot Position JobData=[{2},{3}] Not Match Error !", targetCstID, i + 1, cst_seq, job_seq);
                                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                                bPlanCancel = true;
                                break;
                            }
                            slotPosition sltpos = new slotPosition();
                            sltpos.cst_id = targetCstID;
                            sltpos.slotno = i + 1;
                            sltpos.cst_sequence = cst_seq;
                            sltpos.job_sequence = job_seq;
                            lstTargetCSTJobs.Add(sltpos);
                        }
                    }
                }
                #endregion

                #region 3.Check 满足的一组S-T Source&Target Glass

                #region Check plan source glass
                IList<SLOTPLAN> sourceCstPlans = ObjectManager.PlanManager.GetProductPlansByCstID(PlanID, sourceCstID);
                foreach (SLOTPLAN sltpln in sourceCstPlans)
                {
                    if (bPlanCancel)
                        break;
                    Job job1 = ObjectManager.JobManager.GetJob(sltpln.PRODUCT_NAME);
                    if (job1 == null)  //add by yang 2017/7/1 for filter sltpln not in plan
                    {
                        strErr = string.Format("PLAN Job=[{0}] Object Find Error!", sltpln.PRODUCT_NAME, sltpln.SLOTNO);
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                        bPlanCancel = true;
                        break;
                    }
                    if (sltpln.SLOTNO > 0)
                    {
                        if (lstSourceCSTJobs.FirstOrDefault(cstglass => sltpln.SOURCE_CASSETTE_ID.Trim() == cstglass.cst_id.Trim() && cstglass.slotno == sltpln.SLOTNO) == null && job1.RobotWIP.CurLocation_StageID.Substring(0, 1) != "1")
                        {
                            strErr = string.Format("PLAN CSTID=[{0}] Source Slot=[{1}] Can't Find in CST Error !", sltpln.SOURCE_CASSETTE_ID, sltpln.SLOTNO);
                            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                            bPlanCancel = true;
                            break;
                        }
                    }
                    else
                    {
                        if (lstSourceCSTJobs.FirstOrDefault(cstglass => sltpln.SOURCE_CASSETTE_ID.Trim() == cstglass.cst_id.Trim() && cstglass.glass_id.Trim() == sltpln.PRODUCT_NAME.Trim()) == null && job1.RobotWIP.CurLocation_StageID.Substring(0, 1) != "1")
                        {
                            strErr = string.Format("PLAN CSTID=[{0}] Source GlassID=[{1}] Can't Find in CST Error !", sltpln.SOURCE_CASSETTE_ID, sltpln.PRODUCT_NAME);
                            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                            bPlanCancel = true;
                            break;
                        }
                    }
                }
                #endregion

                #region Check plan target glass
                IList<SLOTPLAN> targetCstPlans = ObjectManager.PlanManager.GetProductPlansByCstID(PlanID, targetCstID);
                foreach (SLOTPLAN sltpln in targetCstPlans)
                {
                    if (bPlanCancel)
                        break;
                    if (sltpln.TARGET_SLOTNO.Length == 0 || int.Parse(sltpln.TARGET_SLOTNO) > 0)
                    {
                        if (lstTargetCSTJobs.FirstOrDefault(cstglass => sltpln.TARGET_CASSETTE_ID.Trim() == cstglass.cst_id.Trim() && cstglass.slotno == int.Parse(sltpln.TARGET_SLOTNO)) == null)
                        {
                            strErr = string.Format("PLAN CSTID=[{0}] Target Slot=[{1}] in [{2}] isn't EMPT Error !", sltpln.SOURCE_CASSETTE_ID, sltpln.TARGET_SLOTNO, sltpln.TARGET_CASSETTE_ID);
                            this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                            bPlanCancel = true;
                            break;
                        }
                    }
                }
                #endregion

                #region Check plan target glass totoal count
                if (!bPlanCancel)
                {
                    //mark by yang
                    IEnumerable<string> lstTarget = from slot in Plans where (slot.HAVE_BEEN_USED == false && slot.TARGET_CASSETTE_ID.Trim() == targetCstID.Trim()) select slot.TARGET_CASSETTE_ID;
                    IEnumerable<string> lstCst = from slot in lstTargetCSTJobs where slot.cst_id.Trim() == targetCstID.Trim() select slot.cst_id;
                    if (lstTarget.Count() > lstCst.Count())
                    {
                        strErr = string.Format("PLAN Target CSTID=[{0}] EMPT Slot Count[{1}] < Plan Slot Count[{2}] Error !", targetCstID, lstCst.Count(), lstTarget.Count());
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strErr);
                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { TrackKey, eqp.Data.LINEID, strErr });
                        bPlanCancel = true;
                    }
                }
                #endregion

                #endregion


                if (bPlanCancel)
                {
                    line.File.PlanStatus = ePLAN_STATUS.CANCEL;
                    ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                    if (line.File.HostMode != eHostMode.OFFLINE)
                    {
                        Invoke(eServiceName.MESService, "ChangePlanCanceled", new object[6] { TrackKey, line.Data.LINEID, PlanID, "0", strPlanCancelReason, strIssueCassetteID });
                        //offline not cancel cst
                        Invoke(eServiceName.PortService, "CassettePlanCancel", new object[4] { lsSourceCassette.ToList<string>(), lsTargetCassette.ToList<string>(), 1, TrackKey });
                    }
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "Plan[" + PlanID + "] Status Change To Cancel!");
                }
                else
                {
                    if (isCurrentPlan)
                    {
                        if (line.File.PlanStatus != ePLAN_STATUS.START)
                        {
                            if (line.File.PlanStatus == ePLAN_STATUS.WAITING) line.File.PlanStatus = ePLAN_STATUS.START;  //yang
                            else line.File.PlanStatus = ePLAN_STATUS.READY;
                        }
                        ObjectManager.LineManager.EnqueueSave(line.File);
                        ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                    }
                    else
                    {
                        IList<SLOTPLAN> TempPlans = ObjectManager.PlanManager.GetProductPlans(line.File.CurrentPlanID);
                        ObjectManager.PlanManager.AddOnlinePlansStandby(line.File.CurrentPlanID, TempPlans);
                        line.File.PlanStatus = ePLAN_STATUS.READY;
                        line.File.CurrentPlanID = PlanID;
                        ObjectManager.LineManager.EnqueueSave(line.File);
                        ObjectManager.PlanManager.AddOnlinePlans(PlanID, Plans);
                    }
                    if (line.Data.LINEID.Contains("TCCHN"))//add by hujunpeng 20190522 for auto change DCR status
                    {
                        eBitResult vcrMode = eBitResult.Unknown;
                        if (line.File.PlanVCR.ContainsKey(PlanID))
                        {
                            vcrMode = line.File.PlanVCR[PlanID] == "Y" ? eBitResult.ON : eBitResult.OFF;
                        }
                        else
                        {
                            Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={1}] [{0}] " + "can not found current planID[2] in PlanVCR Dictionary",
                        TrackKey, line.Data.LINEID, PlanID));
                        }
                        object[] obj = new object[]
                    {
                        "L2",
                        vcrMode,
                        "1",
                        TrackKey
                    };
                        Invoke(eServiceName.VCRService, "VCRModeChangeCommand", obj);
                    }
                    this.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "Plan[" + PlanID + "] Status Change To Ready!");
                }
                Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
            }
            return (bPlanReady && (!bPlanCancel));
        }

        public class CSTEnd
        {
            private string _portNo = string.Empty;
            private TimeSpan _ts = new TimeSpan();

            public string PortNo
            {
                get { return _portNo; }
                set { _portNo = value; }
            }

            public TimeSpan TS
            {
                get { return _ts; }
                set { _ts = value; }
            }
        }
        #endregion


        /// <summary>
        /// Changer Plan End Scan
        /// </summary>
        /// <param name="cmdPortNo"></param>
        #region [Changer Plan End Scan]
        public void ChangerPlanEndScan(Line line, Equipment eqp)
        {
            List<Job> lstJobinPlan = new List<Job>(); //Plan's job, just has cst_sequenceno, job_sequenceno
            List<Job> lstJobinEQ = new List<Job>(); //Indexer's job, job in source cst or in unit
            List<Port> lstPort = new List<Port>(); //Plan's source ports
            bool quitPlan = true;
            string trxName;
            Trx trxCassetteSlotPosition;
            Trx trxEachPosition;
            string PlanID = string.Empty;
            IList<SLOTPLAN> currPlans;
            try
            {
                //for Exchange Mode, check New Plan
                if (line.File.PlanStatus != ePLAN_STATUS.START || line.File.PlanStatus != ePLAN_STATUS.READY || line.File.PlanStatus != ePLAN_STATUS.ABORTING)
                {
                    if (line.File.LineOperMode == eMES_LINEOPERMODE.EXCHANGE)
                        ChangerPlanCreateAtExchangeMode(line, eqp);
                }

                //for avoid each position exchange miss
                Robot curRobot = ObjectManager.RobotManager.GetRobot(eqp.Data.NODENO);
                if (curRobot.File.Status != eRobotStatus.IDLE)
                    return;

                //check curr Plan, if job not in Source CST & not in Each Postion, then quit Plan
                if (line.File.PlanStatus == ePLAN_STATUS.START || line.File.PlanStatus == ePLAN_STATUS.READY || line.File.PlanStatus == ePLAN_STATUS.ABORTING)
                {
                    currPlans = ObjectManager.PlanManager.GetProductPlans(out PlanID);

                    //get glass in Plan
                    foreach (SLOTPLAN plan in currPlans)
                    {
                        Port port;
                        string cst_id;
                        int job_seq;
                        cst_id = plan.SOURCE_CASSETTE_ID.Trim();
                        if (plan.SLOTNO != 0)
                        {
                            job_seq = plan.SLOTNO;
                        }
                        else
                        {
                            Cassette cas = ObjectManager.CassetteManager.GetCassette(cst_id);
                            if (cas != null)
                            {
                                //Job jb = ObjectManager.JobManager.GetJobs(cas.CassetteSequenceNo).FirstOrDefault(j => j.GlassChipMaskBlockID.Trim().Equals(plan.PRODUCT_NAME.Trim()) || j.MesProduct.PRODUCTNAME.Trim().Equals(plan.PRODUCT_NAME.Trim()));
                                //VCR Mismatch 为Cst里其他的玻璃时候，会出现不同Plan Product 对应相同Job Modified by zhangwei 20161122
                                Job jb = ObjectManager.JobManager.GetJobs(cas.CassetteSequenceNo).FirstOrDefault(j => j.MesProduct.PRODUCTNAME.Trim().Equals(plan.PRODUCT_NAME.Trim()));
                                if (jb != null)
                                    job_seq = int.Parse(jb.JobSequenceNo);
                                else
                                    continue;
                            }
                            else
                            {
                                continue;
                            }
                        }
                        port = ObjectManager.PortManager.GetPorts().FirstOrDefault(p => p.File.CassetteID.Trim() == cst_id.Trim() && p.File.Status == ePortStatus.LC && (p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING || p.File.CassetteStatus == eCassetteStatus.PROCESS_PAUSED));
                        if (port == null)
                        {
                            continue;
                        }
                        else
                        {
                            if (lstPort.FirstOrDefault(p => p.Data.PORTNO == port.Data.PORTNO) == null)
                                lstPort.Add(port);
                            Job j = new Job(int.Parse(port.File.CassetteSequenceNo), job_seq);
                            lstJobinPlan.Add(j);
                        }
                    }

                    //get glass in CSTs
                    foreach (Port port in lstPort)
                    {
                        if (line.File.PlanStatus == ePLAN_STATUS.ABORTING) // plan's status aborting, not need care source cst's glass 
                            break;

                        trxName = string.Format("{0}_Port#{1}JobEachCassetteSlotPositionBlock", eqp.Data.NODENO, port.Data.PORTNO);
                        trxCassetteSlotPosition = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                        if (trxCassetteSlotPosition == null)
                            continue;

                        for (int i = 0; i < trxCassetteSlotPosition.EventGroups[0].Events[0].Items.Count; i += 2)
                        {
                            string cst_seq = trxCassetteSlotPosition.EventGroups[0].Events[0].Items[i].Value;
                            string job_seq = trxCassetteSlotPosition.EventGroups[0].Events[0].Items[i + 1].Value;
                            if (!cst_seq.Equals("0") || !job_seq.Equals("0"))
                            {
                                Job j = new Job(int.Parse(cst_seq), int.Parse(job_seq));
                                lstJobinEQ.Add(j);
                            }
                        }
                    }
                    //get galss in Units
                    trxName = string.Format("{0}_JobEachPositionBlock", eqp.Data.NODENO);
                    trxEachPosition = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                    if (trxEachPosition != null)
                    {
                        for (int i = 0; i < trxEachPosition.EventGroups[0].Events[0].Items.Count; i += 2)
                        {
                            string cst_seq;
                            string job_seq;

                            cst_seq = trxEachPosition.EventGroups[0].Events[0].Items[i].Value;
                            job_seq = trxEachPosition.EventGroups[0].Events[0].Items[i + 1].Value;
                            if (!cst_seq.Equals("0") || !job_seq.Equals("0"))
                            {
                                Job j = new Job(int.Parse(cst_seq), int.Parse(job_seq));
                                lstJobinEQ.Add(j);

                                //has any glass in stage or arm, can't quit plan 
                                //quitPlan = false; //delete this contraint(just check plan's source glass not in units) 2016/01/23 cc.kuang
                                //break;
                            }
                        }
                    }

                    foreach (Job job in lstJobinPlan) //if Plan's glass not in source cst or unit, quit plan
                    {
                        if (null != lstJobinEQ.FirstOrDefault(j => j.CassetteSequenceNo == job.CassetteSequenceNo && j.JobSequenceNo == job.JobSequenceNo))
                        {
                            quitPlan = false;
                            break;
                        }
                    }

                    if (quitPlan)
                    {
                        if (line.File.PlanStatus == ePLAN_STATUS.ABORTING)
                        {
                            line.File.PlanStatus = ePLAN_STATUS.ABORT;
                            ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                            if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                                Invoke(eServiceName.MESService, "ChangePlanAborted", new object[6] { UtilityMethod.GetAgentTrackKey(), line.Data.LINEID, PlanID, "0", "CASSETTE BE CANCEL or PLAN CANCEL", "" });
                        }
                        else
                        {
                            line.File.PlanStatus = ePLAN_STATUS.END;
                            ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                            lock (line.File)//add by hujunpeng 20190522 for auto change DCR status
                            {
                                if (line.File.PlanVCR.ContainsKey(PlanID))
                                    line.File.PlanVCR.Remove(PlanID);
                                ObjectManager.LineManager.EnqueueSave(line.File);
                            }
                        }

                        if (line.File.HostMode == eHostMode.OFFLINE)
                            ObjectManager.PlanManager.DeleteOffLinePlanInDB(line.File.CurrentPlanID);

                        ObjectManager.PlanManager.RemoveChangePlan();
                        Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
                        IList<SLOTPLAN> standbyPlans = ObjectManager.PlanManager.GetProductPlansStandby(out PlanID);

                        if (standbyPlans != null && standbyPlans.Count > 0)
                        {
                            IList<SLOTPLAN> plans = new List<SLOTPLAN>();
                            line.File.CurrentPlanID = PlanID;
                            line.File.PlanStatus = ePLAN_STATUS.REQUEST;
                            ObjectManager.LineManager.EnqueueSave(line.File);//add by box.zhai
                            ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                            foreach (SLOTPLAN pl in standbyPlans)
                            {
                                plans.Add(pl);
                            }
                            ObjectManager.PlanManager.AddOnlinePlans(PlanID, plans);
                            ObjectManager.PlanManager.RemoveChangePlanStandby();
                            Invoke(eServiceName.PortService, "CassetteBondToPort", new object[4] { line, standbyPlans, true, UtilityMethod.GetAgentTrackKey() });
                            Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
                        }
                        else
                        {
                            IList<Port> ports = ObjectManager.PortManager.GetPorts();
                            foreach (Port port in ports)
                            {
                                if (line.File.HostMode == eHostMode.OFFLINE) //not Quit cst at offline 2016/01/03 cc.kuang
                                    continue;

                                if (line.File.LineOperMode == eMES_LINEOPERMODE.EXCHANGE)
                                {
                                    if (port.File.CassetteStatus != eCassetteStatus.IN_ABORTING && port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING)
                                        continue;
                                }

                                Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID);
                                if (cst != null && (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel || cst.CassetteControlCommand == eCstControlCmd.ProcessAbort || cst.CassetteControlCommand == eCstControlCmd.ProcessEnd))
                                    continue;

                                switch (port.File.CassetteStatus)
                                {
                                    case eCassetteStatus.WAITING_FOR_CASSETTE_DATA:
                                    case eCassetteStatus.WAITING_FOR_START_COMMAND:
                                    case eCassetteStatus.WAITING_FOR_PROCESSING:
                                        {
                                            CassetteProcessCancel(port.Data.NODENO, port.Data.PORTNO);
                                        }
                                        break;
                                    case eCassetteStatus.IN_ABORTING:
                                        {
                                            CassetteProcessAbort(port.Data.NODENO, port.Data.PORTNO);
                                        }
                                        break;
                                    case eCassetteStatus.IN_PROCESSING:
                                        {
                                            CassetteProcessEnd(eqp.Data.NODENO, port.Data.PORTNO);
                                        }
                                        break;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return;
        }

        /// <summary>
        /// Changer Plan End Scan by Cst
        /// </summary>
        //add by box.zhai for change plan 2016/12/23
        public void ChangerPlanEndScanForOnline(Line line, Equipment eqp)
        {
            List<Job> lstJobinPlanBySourceCST = new List<Job>(); //Source CST在Plan中的ProductList，只包含cst_sequenceno, job_sequenceno
            List<Job> lstJobinPlanByTargetCST = new List<Job>(); //Target CST在Plan中的ProductList，只包含cst_sequenceno, job_sequenceno
            List<Job> lstJobinCst = new List<Job>();

            bool quitPlan = true;
            string trxName;
            Trx trxCassetteSlotPosition;
            string PlanID = string.Empty;
            string StandByPlanID = string.Empty;
            int tarcount = 0; //add by yang 2017/6/12
            IList<SLOTPLAN> PlansOfCst; //不包含HasBeenUse=true的Plan
            try
            {
                //for Exchange Mode, check New Plan
                if (line.File.PlanStatus != ePLAN_STATUS.START || line.File.PlanStatus != ePLAN_STATUS.READY || line.File.PlanStatus != ePLAN_STATUS.ABORTING)
                {
                    if (line.File.LineOperMode == eMES_LINEOPERMODE.EXCHANGE)
                        ChangerPlanCreateAtExchangeMode(line, eqp);
                }

                //for avoid each position exchange miss
                Robot curRobot = ObjectManager.RobotManager.GetRobot(eqp.Data.NODENO);
                if (curRobot.File.Status != eRobotStatus.IDLE)
                    return;

                //Check CST 是否可以退Port
                if (line.File.PlanStatus == ePLAN_STATUS.START || line.File.PlanStatus == ePLAN_STATUS.READY || line.File.PlanStatus == ePLAN_STATUS.ABORTING
                    || line.File.PlanStatus == ePLAN_STATUS.WAITING)  //yang
                {
                    List<string> lstCstID = new List<string>();      //Current Plan所有的CST 
                    List<string> lstSourceCstID = new List<string>();//Current Plan所有的Source CST 
                    List<string> lstTargetCstID = new List<string>();//Current Plan所有的Target CST 
                    List<Port> lstPort = ObjectManager.PortManager.GetPorts().FindAll(p => p.File.Status == ePortStatus.LC && (p.File.CassetteStatus == eCassetteStatus.IN_PROCESSING || p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING || p.File.CassetteStatus == eCassetteStatus.PROCESS_PAUSED));
                    IList<SLOTPLAN> Plans = ObjectManager.PlanManager.GetProductPlans(out PlanID);
                    IList<SLOTPLAN> standByPlans = ObjectManager.PlanManager.GetProductPlansStandby(out StandByPlanID);

                    #region 获得Current Plan 所有的S&T CST
                    var lsSourceCassette = from slot in Plans group slot by slot.SOURCE_CASSETTE_ID into g select g.First().SOURCE_CASSETTE_ID;
                    var lsTargetCassette = from slot in Plans group slot by slot.TARGET_CASSETTE_ID into g select g.First().TARGET_CASSETTE_ID;

                    foreach (string cstid in lsSourceCassette)
                    {
                        lstSourceCstID.Add(cstid);
                    }
                    lstCstID.AddRange(lstSourceCstID);

                    foreach (string cstid in lsTargetCassette)
                    {
                        lstTargetCstID.Add(cstid);
                    }
                    lstCstID.AddRange(lstTargetCstID);
                    lstCstID.Sort();

                    #endregion

                    Plan Plan = ObjectManager.PlanManager.GetPlan(PlanID);

                    foreach (Port port in lstPort)
                    {
                        bool quitCst = true;
                        bool completeCst = true;
                        if (lstSourceCstID.Contains(port.File.CassetteID.Trim()))
                        {
                            PlansOfCst = ObjectManager.PlanManager.GetProductPlansByCstID(PlanID, port.File.CassetteID.Trim());
                            if (PlansOfCst.Count > 0)
                            {
                                #region 获得lstJobinPlanOfSourceCST
                                foreach (SLOTPLAN plan in PlansOfCst)
                                {

                                    int job_seq;
                                    if (plan.SLOTNO != 0)
                                    {
                                        job_seq = plan.SLOTNO;
                                    }
                                    else
                                    {
                                        Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID.Trim());
                                        if (cst != null)
                                        {
                                            //Job jb = ObjectManager.JobManager.GetJobs(cas.CassetteSequenceNo).FirstOrDefault(j => j.GlassChipMaskBlockID.Trim().Equals(plan.PRODUCT_NAME.Trim()) || j.MesProduct.PRODUCTNAME.Trim().Equals(plan.PRODUCT_NAME.Trim()));
                                            //VCR Mismatch 为Cst里其他的玻璃时候，会出现不同Plan Product 对应相同Job Modified by zhangwei 20161122
                                            Job jb = ObjectManager.JobManager.GetJobs(cst.CassetteSequenceNo).FirstOrDefault(j => j.MesProduct.PRODUCTNAME.Trim().Equals(plan.PRODUCT_NAME.Trim()));
                                            if (jb != null)
                                                job_seq = int.Parse(jb.JobSequenceNo);
                                            else
                                                continue;
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    if (port == null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        Job j = new Job(int.Parse(port.File.CassetteSequenceNo), job_seq);
                                        lstJobinPlanBySourceCST.Add(j);
                                    }
                                }
                                #endregion
                            }
                            //Socurce Cst找不到对应的Plan说明这个Plan HasBeenUse=true,所以这个source Cst在Current Plan中的Glass全部做完
                            else
                            {
                                if (!Plan.PlanCompleteCst.Contains(port.File.CassetteID))
                                {
                                    lock (Plan)
                                    {
                                        Plan.PlanCompleteCst.Add(port.File.CassetteID);
                                        Plan.PlanCompleteCst.Sort();
                                        ObjectManager.PlanManager.EnqueueSave(Plan);
                                    }
                                }
                                //判断StandBy Plan 是否用到这个CST,用到不退CST
                                if (ObjectManager.PlanManager.CstExistInStandByChangerPlan(StandByPlanID, port.File.CassetteID))
                                {
                                    quitCst = false;
                                }
                            }
                        }
                        else if (lstTargetCstID.Contains(port.File.CassetteID.Trim()))
                        {

                            PlansOfCst = ObjectManager.PlanManager.GetProductPlansByTargetCstID(PlanID, port.File.CassetteID.Trim());
                            if (PlansOfCst.Count > 0)
                            {
                                tarcount = PlansOfCst.Count;  //yang

                                #region 获得lstJobinPlanOfTargetCST
                                foreach (SLOTPLAN plan in PlansOfCst)
                                {
                                    if (port == null)
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        Job j = ObjectManager.JobManager.GetJob(plan.PRODUCT_NAME);
                                        if (j != null)  //add by yang 2017/5/4,可能有的source还没有上port
                                            lstJobinPlanByTargetCST.Add(j);
                                    }
                                }
                                #endregion
                            }
                            //Target Cst找不到对应的Plan说明这个Plan HasBeenUse=true,所以这个Target Cst在Current Plan中的Glass全部做完
                            else
                            {
                                if (!Plan.PlanCompleteCst.Contains(port.File.CassetteID))
                                {
                                    lock (Plan)
                                    {
                                        Plan.PlanCompleteCst.Add(port.File.CassetteID);
                                        Plan.PlanCompleteCst.Sort();
                                        ObjectManager.PlanManager.EnqueueSave(Plan);
                                    }
                                }
                                //判断StandBy Plan 是否用到这个CST,用到不退CST
                                if (ObjectManager.PlanManager.CstExistInStandByChangerPlan(StandByPlanID, port.File.CassetteID))
                                {
                                    quitCst = false;
                                }
                            }
                        }
                        else
                        {
                            continue;
                        }

                        #region 获得对应CST里面的Job

                        if (line.File.PlanStatus == ePLAN_STATUS.ABORTING)
                            break;

                        trxName = string.Format("{0}_Port#{1}JobEachCassetteSlotPositionBlock", eqp.Data.NODENO, port.Data.PORTNO);
                        trxCassetteSlotPosition = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                        if (trxCassetteSlotPosition == null)
                            continue;

                        for (int i = 0; i < trxCassetteSlotPosition.EventGroups[0].Events[0].Items.Count; i += 2)
                        {
                            string cst_seq = trxCassetteSlotPosition.EventGroups[0].Events[0].Items[i].Value;
                            string job_seq = trxCassetteSlotPosition.EventGroups[0].Events[0].Items[i + 1].Value;
                            if (!cst_seq.Equals("0") || !job_seq.Equals("0"))
                            {
                                Job job = new Job(int.Parse(cst_seq), int.Parse(job_seq));
                                lstJobinCst.Add(job);
                            }
                        }

                        #endregion

                        if (lstJobinPlanBySourceCST.Count > 0)
                        {
                            foreach (Job job in lstJobinPlanBySourceCST) //if is Source Cst,Plan of CstID的job都不在CST,退Port
                            {
                                if (null != lstJobinCst.FirstOrDefault(j => j.CassetteSequenceNo == job.CassetteSequenceNo && j.JobSequenceNo == job.JobSequenceNo))
                                {
                                    completeCst = false;
                                    quitCst = false;
                                    break;
                                }
                            }
                            if (completeCst == true)
                            {
                                if (!Plan.PlanCompleteCst.Contains(port.File.CassetteID))
                                {
                                    lock (Plan)
                                    {
                                        Plan.PlanCompleteCst.Add(port.File.CassetteID);
                                        Plan.PlanCompleteCst.Sort();
                                        ObjectManager.PlanManager.EnqueueSave(Plan);
                                    }
                                }

                                //添加判断 当前Plan完成的CST在StandByPlan中是否用到，如果没有用到直接退CST,如果用到不能退
                                if (ObjectManager.PlanManager.CstExistInStandByChangerPlan(StandByPlanID, port.File.CassetteID))
                                {
                                    quitCst = false;
                                }
                            }
                        }

                        else if (lstJobinPlanByTargetCST.Count > 0)
                        {
                            foreach (Job job in lstJobinPlanByTargetCST) //if is Target Cst,Plan of CstID的job都在CST,退Port
                            {
                                if (null == lstJobinCst.FirstOrDefault(j => j.CassetteSequenceNo == job.CassetteSequenceNo && j.JobSequenceNo == job.JobSequenceNo))
                                {
                                    completeCst = false;
                                    quitCst = false;
                                    break;
                                }
                            }
                            if (tarcount > lstJobinPlanByTargetCST.Count || tarcount > lstJobinCst.Count) //add by yang 2017/6/12
                            {
                                completeCst = false;
                                quitCst = false;
                            }
                            if (completeCst == true)
                            {
                                if (!Plan.PlanCompleteCst.Contains(port.File.CassetteID))
                                {
                                    lock (Plan)
                                    {
                                        Plan.PlanCompleteCst.Add(port.File.CassetteID);
                                        Plan.PlanCompleteCst.Sort();
                                        ObjectManager.PlanManager.EnqueueSave(Plan);
                                    }
                                }

                                //添加判断 当前Plan完成的CST在StandByPlan中是否用到，如果没有用到直接退CST,如果用到不能退
                                if (ObjectManager.PlanManager.CstExistInStandByChangerPlan(StandByPlanID, port.File.CassetteID))
                                {
                                    quitCst = false;
                                }
                            }
                        }
                        lstJobinPlanBySourceCST.Clear();
                        lstJobinPlanByTargetCST.Clear();
                        string strCst = string.Empty;
                        foreach (Job job in lstJobinCst)
                        {
                            strCst += job.CassetteSequenceNo + "_" + job.JobSequenceNo + ";";
                        }
                        lstJobinCst.Clear();
                        tarcount = 0; //init  ,yang

                        #region 退CST
                        if (quitCst)
                        {
                            switch (port.File.CassetteStatus)
                            {
                                case eCassetteStatus.WAITING_FOR_CASSETTE_DATA:
                                case eCassetteStatus.WAITING_FOR_START_COMMAND:
                                case eCassetteStatus.WAITING_FOR_PROCESSING:
                                    {
                                        CassetteProcessCancel(port.Data.NODENO, port.Data.PORTNO);
                                    }
                                    break;
                                case eCassetteStatus.IN_ABORTING:
                                    {
                                        CassetteProcessAbort(port.Data.NODENO, port.Data.PORTNO);
                                    }
                                    break;
                                case eCassetteStatus.IN_PROCESSING:
                                    {
                                        CassetteProcessEnd(eqp.Data.NODENO, port.Data.PORTNO);
                                    }
                                    break;
                            }
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] [PORTID={2}] [CSTID={3}] QUIT CASSETTE,JOB EACH POSITION[{4}]", eqp.Data.NODENO, DateTime.Now.ToString(), port.Data.Id, port.File.CassetteID, strCst));

                        }
                        #endregion

                    }
                    //当前Plan中S&T CST完成后就退Plan add by box.zhai
                    foreach (string strCstID in lstCstID)
                    {
                        if (Plan.PlanCompleteCst.Contains(strCstID) == false)
                        {
                            quitPlan = false;
                        }
                    }

                    #region 退Plan

                    if (quitPlan)
                    {
                        if (line.File.PlanStatus == ePLAN_STATUS.ABORTING)
                        {
                            line.File.PlanStatus = ePLAN_STATUS.ABORT;
                            ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                            if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                                Invoke(eServiceName.MESService, "ChangePlanAborted", new object[6] { UtilityMethod.GetAgentTrackKey(), line.Data.LINEID, PlanID, "0", "CASSETTE BE CANCEL or PLAN CANCEL", "" });
                        }
                        else
                        {
                            line.File.PlanStatus = ePLAN_STATUS.END;
                            ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                            lock (line.File)//add by hujunpeng 20190522 for auto change DCR status
                            {
                                if (line.File.PlanVCR.ContainsKey(PlanID))
                                    line.File.PlanVCR.Remove(PlanID);
                                ObjectManager.LineManager.EnqueueSave(line.File);
                            }
                        }

                        ObjectManager.PlanManager.RemoveChangePlan();
                        Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
                        IList<SLOTPLAN> standbyPlans = ObjectManager.PlanManager.GetProductPlansStandby(out PlanID);

                        if (standbyPlans != null && standbyPlans.Count > 0)
                        {
                            IList<SLOTPLAN> plans = new List<SLOTPLAN>();
                            line.File.CurrentPlanID = PlanID;
                            line.File.PlanStatus = ePLAN_STATUS.REQUEST;
                            ObjectManager.LineManager.EnqueueSave(line.File);//add by box.zhai
                            ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                            foreach (SLOTPLAN pl in standbyPlans)
                            {
                                plans.Add(pl);
                            }
                            ObjectManager.PlanManager.AddOnlinePlans(PlanID, plans);
                            ObjectManager.PlanManager.RemoveChangePlanStandby();
                            Invoke(eServiceName.PortService, "CassetteBondToPort", new object[4] { line, standbyPlans, true, UtilityMethod.GetAgentTrackKey() });
                            Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
                        }
                        else
                        {
                            IList<Port> ports = ObjectManager.PortManager.GetPorts();
                            foreach (Port port in ports)
                            {
                                if (line.File.HostMode == eHostMode.OFFLINE) //not Quit cst at offline 2016/01/03 cc.kuang
                                    continue;

                                if (line.File.LineOperMode == eMES_LINEOPERMODE.EXCHANGE)
                                {
                                    if (port.File.CassetteStatus != eCassetteStatus.IN_ABORTING && port.File.CassetteStatus != eCassetteStatus.IN_PROCESSING)
                                        continue;
                                }

                                Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID);
                                if (cst != null && (cst.CassetteControlCommand == eCstControlCmd.ProcessCancel || cst.CassetteControlCommand == eCstControlCmd.ProcessAbort || cst.CassetteControlCommand == eCstControlCmd.ProcessEnd))
                                    continue;

                                switch (port.File.CassetteStatus)
                                {
                                    case eCassetteStatus.WAITING_FOR_CASSETTE_DATA:
                                    case eCassetteStatus.WAITING_FOR_START_COMMAND:
                                    case eCassetteStatus.WAITING_FOR_PROCESSING:
                                        {
                                            CassetteProcessCancel(port.Data.NODENO, port.Data.PORTNO);
                                        }
                                        break;
                                    case eCassetteStatus.IN_ABORTING:
                                        {
                                            CassetteProcessAbort(port.Data.NODENO, port.Data.PORTNO);
                                        }
                                        break;
                                    case eCassetteStatus.IN_PROCESSING:
                                        {
                                            CassetteProcessEnd(eqp.Data.NODENO, port.Data.PORTNO);
                                        }
                                        break;
                                }
                            }
                        }
                    }
                    #endregion

                }

            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return;
        }

        public void ChangerPlanCreateAtExchangeMode(Line line, Equipment eqp)
        {
            try
            {
                string sourceCSTSeq = "";
                string targetCSTSeq = "";
                string planID = "";
                IList<Job> lstJob;

                IList<Port> ports = ObjectManager.PortManager.GetPorts();
                foreach (Port p in ports)
                {
                    if (p.File.Status == ePortStatus.LC && p.File.CassetteStatus == eCassetteStatus.WAITING_FOR_PROCESSING)
                    {
                        Cassette cst = ObjectManager.CassetteManager.GetCassette(p.File.CassetteID);
                        if (cst != null && cst.CassetteControlCommand == eCstControlCmd.ProcessCancel)
                            continue;

                        if (sourceCSTSeq.Length == 0 && p.File.Type == ePortType.LoadingPort)
                            sourceCSTSeq = p.File.CassetteSequenceNo;

                        if (targetCSTSeq.Length == 0 && p.File.Type == ePortType.UnloadingPort && int.Parse(p.File.JobCountInCassette) == 0)
                            targetCSTSeq = p.File.CassetteSequenceNo;
                    }
                }

                if (sourceCSTSeq.Length > 0 && targetCSTSeq.Length > 0)
                {
                    Cassette sourceCST = null;
                    Cassette targetCST = null;
                    IList<SLOTPLAN> plans = new List<SLOTPLAN>();

                    lstJob = ObjectManager.JobManager.GetJobs(sourceCSTSeq);
                    if (lstJob == null || lstJob.Count == 0)
                    {
                        this.LogError(MethodBase.GetCurrentMethod().Name + "()", "Get Job List Error By Source CST Sequence (" + sourceCSTSeq + ")");
                    }

                    foreach (Job jb in lstJob)
                    {
                        if (!jb.SamplingSlotFlag.Equals("1"))
                            continue;

                        SLOTPLAN plan = new SLOTPLAN();
                        plan.SLOTNO = int.Parse(jb.JobSequenceNo);
                        plan.HAVE_BEEN_USED = false;
                        plan.PRODUCT_NAME = jb.GlassChipMaskBlockID;
                        sourceCST = ObjectManager.CassetteManager.GetCassette(int.Parse(sourceCSTSeq));
                        targetCST = ObjectManager.CassetteManager.GetCassette(int.Parse(targetCSTSeq));
                        plan.SOURCE_CASSETTE_ID = sourceCST.CassetteID;
                        plan.TARGET_CASSETTE_ID = targetCST.CassetteID;
                        plan.TARGET_SLOTNO = jb.JobSequenceNo;

                        plans.Add(plan);
                    }

                    planID = "EXCHANGE" + sourceCST.CassetteSequenceNo.Trim() + "_" + sourceCST.CassetteID.Trim() + "_To_" + targetCST.CassetteID.Trim();
                    ObjectManager.PlanManager.AddOnlinePlans(planID, plans);
                    line.File.CurrentPlanID = planID;
                    line.File.PlanStatus = ePLAN_STATUS.READY;
                    this.LogInfo(MethodBase.GetCurrentMethod().Name + "()", planID + " Plan Create!");
                    ObjectManager.LineManager.EnqueueSave(line.File);//add by box.zhai
                    ObjectManager.PlanManager.SavePlanStatusInDB(line.File.CurrentPlanID, line.File.PlanStatus);
                    Invoke(eServiceName.UIService, "CurrentChangerPlanReport", new object[] { line.Data.LINEID });
                }
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return;
        }

        #endregion

        /// <summary>
        ///  Call by Robot when the glass on thr Arm has't CST to store
        /// </summary>
        /// <param name="armToCstJobs"></param>
        #region[Cassette ForceEnd Command]
        public void CassetteForceEndCommand(IList<Job> armToCstJobs)
        {
            try
            {
                ;
            }
            catch (Exception ex)
            {
                this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
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

        private class slotPosition //for changer plan use
        {
            public int slotno = 0;
            public string cst_id = "";
            public string cst_sequence = "";
            public string job_sequence = "";
            public string glass_id = "";
        }
    }
}
