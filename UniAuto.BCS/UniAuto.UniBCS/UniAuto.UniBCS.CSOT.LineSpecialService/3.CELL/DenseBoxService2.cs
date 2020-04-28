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

namespace UniAuto.UniBCS.CSOT.LineSpecialService
{
      public partial class DenseBoxService
      {
            #region Dense Box Create Report
            private const string DenseBoxCreateTimeout = "DenseBoxCreateTimeout";
            public void DenseBoxCreateReport(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;

                        string eqpNo = inputData.Metadata.NodeNo;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}) in EquipmentEntity!", inputData.Metadata.NodeNo));

                        #region [拆出PLCAgent Data] Bit
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion

                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                              DenseBoxCreateReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }

                        #region [拆出PLCAgent Data]  Word
                        string denseBoxID1 = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string denseBoxID2 = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string denseBoxCreateFlag = inputData.EventGroups[0].Events[0].Items[2].Value;
                        #endregion

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] DenseBoxCreateReport DenseBoxID1=[{2}],DenseBoxID2 =[{3}],DenseBoxCreateFlag =[{4}], Set Bit (ON)", eqp.Data.NODENO, inputData.TrackKey,
                           denseBoxID1.Trim(), denseBoxID2.Trim(), denseBoxCreateFlag));

                        DenseBoxCreateReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                        // 避免中間發生Exception BCS不把BIT ON起來
                        if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                              DenseBoxCreateReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                  }
            }
            public void DenseBoxCreateReportReply(string eqpNo, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_DenseBoxCreateReportReply") as Trx;
                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + DenseBoxCreateTimeout))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + DenseBoxCreateTimeout);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + DenseBoxCreateTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(DenseBoxCreateReportReplyTimeout), trackKey);
                        }

                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] Dense Box Create Report Reply Set Bit =[{2}).",
                            eqpNo, trackKey, value.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void DenseBoxCreateReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, Dense Box Create Report Reply Timeout Set Bit (OFF).", sArray[0], trackKey));

                        DenseBoxCreateReportReply(sArray[0], eBitResult.OFF, trackKey);

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region Dense Box Decreate Report
            private const string DenseBoxDecreateTimeout = "DenseBoxCreateTimeout";
            public void DenseBoxDecreateReport(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;

                        string eqpNo = inputData.Metadata.NodeNo;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}) in EquipmentEntity!", inputData.Metadata.NodeNo));

                        #region [拆出PLCAgent Data] Bit
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion

                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                              DenseBoxDecreateReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }

                        #region [拆出PLCAgent Data]  Word
                        string denseBoxID1 = inputData.EventGroups[0].Events[0].Items[0].Value;
                        #endregion

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] DenseBoxDecreateReport DenseBoxID1 =[{2}], Set Bit (ON)", eqp.Data.NODENO, inputData.TrackKey, denseBoxID1.Trim()));

                        DenseBoxDecreateReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                        // 避免中間發生Exception BCS不把BIT ON起來
                        if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                              DenseBoxDecreateReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                  }
            }
            public void DenseBoxDecreateReportReply(string eqpNo, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_DenseBoxDecreateReportReply") as Trx;
                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + DenseBoxDecreateTimeout))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + DenseBoxDecreateTimeout);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + DenseBoxDecreateTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(DenseBoxDecreateReportReplyTimeout), trackKey);
                        }

                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] Dense Box Decreate Report Reply Set Bit =[{2}).",
                            eqpNo, trackKey, value.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void DenseBoxDecreateReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, Dense Box Decreate Report Reply Timeout Set Bit (OFF).", sArray[0], trackKey));

                        DenseBoxDecreateReportReply(sArray[0], eBitResult.OFF, trackKey);

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region Dense Box Line In Report
            private const string DenseBoxLineInTimeout = "DenseBoxLineInTimeout";
            public void DenseBoxLineInReport(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;

                        string eqpNo = inputData.Metadata.NodeNo;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}) in EquipmentEntity!", inputData.Metadata.NodeNo));

                        #region [拆出PLCAgent Data] Bit
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion

                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                              DenseBoxLineInReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }

                        #region [拆出PLCAgent Data]  Word
                        string denseBoxCount = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string denseBoxID1 = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string denseBoxID2 = inputData.EventGroups[0].Events[0].Items[2].Value;
                        #endregion

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] DenseBoxLineInReport DenseBoxCount =[{2}],DenseBoxID1 =[{3}],DenseBoxID2 =[{4}], Set Bit (ON)", eqp.Data.NODENO, inputData.TrackKey,
                            denseBoxCount, denseBoxID1.Trim(), denseBoxID2.Trim()));

                        DenseBoxLineInReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                        // 避免中間發生Exception BCS不把BIT ON起來
                        if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                              DenseBoxLineInReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                  }
            }
            public void DenseBoxLineInReportReply(string eqpNo, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_DenseBoxLineInReportReply") as Trx;
                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + DenseBoxLineInTimeout))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + DenseBoxLineInTimeout);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + DenseBoxLineInTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(DenseBoxLineInReportReplyTimeout), trackKey);
                        }

                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] Dense Box Line In Report Reply Set Bit =[{2}).",
                            eqpNo, trackKey, value.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void DenseBoxLineInReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, Dense Box Line In Report Reply Timeout Set Bit (OFF).", sArray[0], trackKey));

                        DenseBoxLineInReportReply(sArray[0], eBitResult.OFF, trackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region Dense Box Line Out Report
            private const string DenseBoxLineOutTimeout = "DenseBoxLineOutTimeout";
            public void DenseBoxLineOutReport(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;

                        string eqpNo = inputData.Metadata.NodeNo;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}) in EquipmentEntity!", inputData.Metadata.NodeNo));

                        #region [拆出PLCAgent Data] Bit
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion

                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                              DenseBoxLineOutReportReply(eqpNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }

                        #region [拆出PLCAgent Data]  Word
                        //Jun Modif 20150106 IO結構有變
                        string carNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string denseBoxCount = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string denseBoxID1 = inputData.EventGroups[0].Events[0].Items[2].Value;
                        string denseBoxID2 = inputData.EventGroups[0].Events[0].Items[3].Value;
                        #endregion

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] DenseBoxLineOutReport DenseBoxCount =[{2}],DenseBoxID1 =[{3}],DenseBoxID2 =[{4}], Set Bit (ON)", eqp.Data.NODENO, inputData.TrackKey,
                            denseBoxCount, denseBoxID1.Trim(), denseBoxID2.Trim()));

                        DenseBoxLineOutReportReply(eqpNo, eBitResult.ON, inputData.TrackKey);

                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null) return;
                        switch (line.Data.LINETYPE)
                        {
                              case eLineType.CELL.CCQUP:

                                    List<Cassette> boxList = new List<Cassette>();
                                    Cassette cst1 = ObjectManager.CassetteManager.GetCassette(denseBoxID1);
                                    if (cst1 != null) boxList.Add(cst1);
                                    Cassette cst2 = ObjectManager.CassetteManager.GetCassette(denseBoxID2);
                                    if (cst2 != null) boxList.Add(cst2);

                                    #region [Report to MES  BoxProcessEnd]
                                    object[] obj = new object[]
                        {
                        inputData.TrackKey,
                        line,
                        boxList,
                        eqpNo
                        };
                                    // MES Data BoxProcessEnd_PPK(string trxID, Line line, List<Cassette>cstList, string nodeNO)
                                    Invoke(eServiceName.MESService, "BoxProcessEnd_PPK", obj);
                                    #endregion
                                    break;
                              default:
                                    break;
                        }
                        if (line.Data.LINETYPE == eLineType.CELL.CBPPK)
                        {
                              Cassette cst1 = ObjectManager.CassetteManager.GetCassette(denseBoxID1);
                              if (cst1 != null)
                              {
                                    object[] obj = new object[]
                        {
                        inputData.TrackKey,
                        line.Data.LINEID,
                        "",
                        cst1,
                        FindInBoxJobs(denseBoxID1)
                        };
                                    Invoke(eServiceName.MESService, "BoxProcessEnd", obj);
                              }

                              Cassette cst2 = ObjectManager.CassetteManager.GetCassette(denseBoxID2);
                              if (cst2 != null)
                              {
                                    object[] obj = new object[]
                        {
                        inputData.TrackKey,
                        line.Data.LINEID,
                        "",
                        cst2,
                        FindInBoxJobs(denseBoxID2)
                        };
                                    Invoke(eServiceName.MESService, "BoxProcessEnd", obj);
                              }

                              ObjectManager.CassetteManager.DeleteBox(denseBoxID1);
                              ObjectManager.CassetteManager.DeleteBox(denseBoxID2);
                        }

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                        // 避免中間發生Exception BCS不把BIT ON起來
                        if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                              DenseBoxLineOutReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                  }
            }
            public void DenseBoxLineOutReportReply(string eqpNo, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_DenseBoxLineOutReportReply") as Trx;
                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + DenseBoxLineOutTimeout))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + DenseBoxLineOutTimeout);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + DenseBoxLineOutTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(DenseBoxLineOutReportReplyTimeout), trackKey);
                        }

                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] Dense Box Line Out Report Reply Set Bit =[{2}).",
                            eqpNo, trackKey, value.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void DenseBoxLineOutReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, Dense Box Line Out Report Reply Timeout Set Bit (OFF).", sArray[0], trackKey));

                        DenseBoxLineOutReportReply(sArray[0], eBitResult.OFF, trackKey);

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region Dense Box Line In Report#01
            private const string DenseBoxLineInTimeout1 = "DenseBoxLineInTimeout1";
            public void DenseBoxLineInReport1(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;

                        string eqpNo = inputData.Metadata.NodeNo;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}) in EquipmentEntity!", inputData.Metadata.NodeNo));

                        #region [拆出PLCAgent Data] Bit
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion

                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                              DenseBoxLineInReportReply1(eqpNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }

                        #region [拆出PLCAgent Data]  Word
                        string denseCarNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string denseBoxCount = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string denseBoxID1 = inputData.EventGroups[0].Events[0].Items[2].Value;
                        string denseBoxID2 = inputData.EventGroups[0].Events[0].Items[3].Value;
                        #endregion

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] DenseBoxLineInReport CarNo =[{2}],DenseBoxCount =[{3}],DenseBoxID1 =[{4}],DenseBoxID2 =[{5}], Set Bit (ON)", eqp.Data.NODENO, inputData.TrackKey,
                            denseCarNo, denseBoxCount, denseBoxID1.Trim(), denseBoxID2.Trim()));

                        DenseBoxLineInReportReply1(eqpNo, eBitResult.ON, inputData.TrackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                        // 避免中間發生Exception BCS不把BIT ON起來
                        if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                              DenseBoxLineInReportReply1(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                  }
            }
            public void DenseBoxLineInReportReply1(string eqpNo, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_DenseBoxLineInReportReply#01") as Trx;
                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + DenseBoxLineInTimeout1))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + DenseBoxLineInTimeout1);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + DenseBoxLineInTimeout1, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(DenseBoxLineInReportReplyTimeout1), trackKey);
                        }

                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] Dense Box Line In Report Reply Set Bit =[{2}).",
                            eqpNo, trackKey, value.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void DenseBoxLineInReportReplyTimeout1(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, Dense Box Line In Report Reply Timeout Set Bit (OFF).", sArray[0], trackKey));

                        DenseBoxLineInReportReply1(sArray[0], eBitResult.OFF, trackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region Dense Box Line Out Report#01
            private const string DenseBoxLineOutTimeout1 = "DenseBoxLineOutTimeout1";
            public void DenseBoxLineOutReport1(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;

                        string eqpNo = inputData.Metadata.NodeNo;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}) in EquipmentEntity!", inputData.Metadata.NodeNo));

                        #region [拆出PLCAgent Data] Bit
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion

                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                              DenseBoxLineOutReportReply1(eqpNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }

                        #region [拆出PLCAgent Data]  Word
                        //Jun Modif 20150106 IO結構有變
                        string carNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string denseBoxCount = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string denseBoxID1 = inputData.EventGroups[0].Events[0].Items[2].Value;
                        string denseBoxID2 = inputData.EventGroups[0].Events[0].Items[3].Value;
                        #endregion

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] DenseBoxLineOutReport DenseBoxCount =[{2}],DenseBoxID1 =[{3}],DenseBoxID2 =[{4}], Set Bit (ON)", eqp.Data.NODENO, inputData.TrackKey,
                            denseBoxCount, denseBoxID1.Trim(), denseBoxID2.Trim()));

                        DenseBoxLineOutReportReply1(eqpNo, eBitResult.ON, inputData.TrackKey);

                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null) return;
                        if (line.Data.LINETYPE == eLineType.CELL.CBPPK)
                        {
                              Cassette cst1 = ObjectManager.CassetteManager.GetCassette(denseBoxID1);
                              if (cst1 != null)
                              {
                                    object[] obj = new object[]
                        {
                        inputData.TrackKey,
                        line.Data.LINEID,
                        "",
                        cst1,
                        FindInBoxJobs(denseBoxID1)
                        };
                                    Invoke(eServiceName.MESService, "BoxProcessEnd", obj);
                              }

                              Cassette cst2 = ObjectManager.CassetteManager.GetCassette(denseBoxID2);
                              if (cst2 != null)
                              {
                                    object[] obj = new object[]
                        {
                        inputData.TrackKey,
                        line.Data.LINEID,
                        "",
                        cst2,
                        FindInBoxJobs(denseBoxID2)
                        };
                                    Invoke(eServiceName.MESService, "BoxProcessEnd", obj);
                              }

                              ObjectManager.CassetteManager.DeleteBox(denseBoxID1);
                              ObjectManager.CassetteManager.DeleteBox(denseBoxID2);
                        }

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                        // 避免中間發生Exception BCS不把BIT ON起來
                        if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                              DenseBoxLineOutReportReply1(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                  }
            }
            public void DenseBoxLineOutReportReply1(string eqpNo, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_DenseBoxLineOutReportReply#01") as Trx;
                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + DenseBoxLineOutTimeout1))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + DenseBoxLineOutTimeout1);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + DenseBoxLineOutTimeout1, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(DenseBoxLineOutReportReplyTimeout1), trackKey);
                        }

                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] Dense Box Line Out Report Reply Set Bit =[{2}).",
                            eqpNo, trackKey, value.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void DenseBoxLineOutReportReplyTimeout1(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, Dense Box Line Out Report Reply Timeout Set Bit (OFF).", sArray[0], trackKey));

                        DenseBoxLineOutReportReply1(sArray[0], eBitResult.OFF, trackKey);

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region Dense Box Line In Report#02
            private const string DenseBoxLineInTimeout2 = "DenseBoxLineInTimeout2";
            public void DenseBoxLineInReport2(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;

                        string eqpNo = inputData.Metadata.NodeNo;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}) in EquipmentEntity!", inputData.Metadata.NodeNo));

                        #region [拆出PLCAgent Data] Bit
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion

                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                              DenseBoxLineInReportReply2(eqpNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }

                        #region [拆出PLCAgent Data]  Word
                        string denseCarNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string denseBoxCount = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string denseBoxID1 = inputData.EventGroups[0].Events[0].Items[2].Value;
                        string denseBoxID2 = inputData.EventGroups[0].Events[0].Items[3].Value;
                        #endregion

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] DenseBoxLineInReport CarNo =[{2}],DenseBoxCount =[{3}],DenseBoxID1 =[{4}],DenseBoxID2 =[{5}], Set Bit (ON)", eqp.Data.NODENO, inputData.TrackKey,
                            denseCarNo, denseBoxCount, denseBoxID1.Trim(), denseBoxID2.Trim()));

                        DenseBoxLineInReportReply2(eqpNo, eBitResult.ON, inputData.TrackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                        // 避免中間發生Exception BCS不把BIT ON起來
                        if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                              DenseBoxLineInReportReply2(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                  }
            }
            public void DenseBoxLineInReportReply2(string eqpNo, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_DenseBoxLineInReportReply#02") as Trx;
                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + DenseBoxLineInTimeout2))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + DenseBoxLineInTimeout2);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + DenseBoxLineInTimeout2, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(DenseBoxLineInReportReplyTimeout2), trackKey);
                        }

                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] Dense Box Line In Report Reply Set Bit =[{2}).",
                            eqpNo, trackKey, value.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void DenseBoxLineInReportReplyTimeout2(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, Dense Box Line In Report Reply Timeout Set Bit (OFF).", sArray[0], trackKey));

                        DenseBoxLineInReportReply2(sArray[0], eBitResult.OFF, trackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region Dense Box Line Out Report#02
            private const string DenseBoxLineOutTimeout2 = "DenseBoxLineOutTimeout2";
            public void DenseBoxLineOutReport2(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;

                        string eqpNo = inputData.Metadata.NodeNo;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}) in EquipmentEntity!", inputData.Metadata.NodeNo));

                        #region [拆出PLCAgent Data] Bit
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion

                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));

                              DenseBoxLineOutReportReply2(eqpNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }

                        #region [拆出PLCAgent Data]  Word
                        //Jun Modif 20150106 IO結構有變
                        string carNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string denseBoxCount = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string denseBoxID1 = inputData.EventGroups[0].Events[0].Items[2].Value;
                        string denseBoxID2 = inputData.EventGroups[0].Events[0].Items[3].Value;
                        #endregion

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] DenseBoxLineOutReport DenseBoxCount =[{2}],DenseBoxID1 =[{3}],DenseBoxID2 =[{4}], Set Bit (ON)", eqp.Data.NODENO, inputData.TrackKey,
                            denseBoxCount, denseBoxID1.Trim(), denseBoxID2.Trim()));

                        DenseBoxLineOutReportReply2(eqpNo, eBitResult.ON, inputData.TrackKey);

                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null) return;
                        if (line.Data.LINETYPE == eLineType.CELL.CBPPK)
                        {
                              Cassette cst1 = ObjectManager.CassetteManager.GetCassette(denseBoxID1);
                              if (cst1 != null)
                              {
                                    object[] obj = new object[]
                        {
                        inputData.TrackKey,
                        line.Data.LINEID,
                        "",
                        cst1,
                        FindInBoxJobs(denseBoxID1)
                        };
                                    Invoke(eServiceName.MESService, "BoxProcessEnd", obj);
                              }

                              Cassette cst2 = ObjectManager.CassetteManager.GetCassette(denseBoxID2);
                              if (cst2 != null)
                              {
                                    object[] obj = new object[]
                        {
                        inputData.TrackKey,
                        line.Data.LINEID,
                        "",
                        cst2,
                        FindInBoxJobs(denseBoxID2)
                        };
                                    Invoke(eServiceName.MESService, "BoxProcessEnd", obj);
                              }

                              ObjectManager.CassetteManager.DeleteBox(denseBoxID1);
                              ObjectManager.CassetteManager.DeleteBox(denseBoxID2);
                        }

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                        // 避免中間發生Exception BCS不把BIT ON起來
                        if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                              DenseBoxLineOutReportReply2(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                  }
            }
            public void DenseBoxLineOutReportReply2(string eqpNo, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_DenseBoxLineOutReportReply#02") as Trx;
                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + DenseBoxLineOutTimeout2))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + DenseBoxLineOutTimeout2);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + DenseBoxLineOutTimeout2, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(DenseBoxLineOutReportReplyTimeout2), trackKey);
                        }

                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] Dense Box Line Out Report Reply Set Bit =[{2}).",
                            eqpNo, trackKey, value.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void DenseBoxLineOutReportReplyTimeout2(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, Dense Box Line Out Report Reply Timeout Set Bit (OFF).", sArray[0], trackKey));

                        DenseBoxLineOutReportReply2(sArray[0], eBitResult.OFF, trackKey);

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region[BoxIDCheckRequest]
            private const string BoxIDCheckRequestTimeout = "BoxIDCheckRequestTimeout";
            /// <summary>
            /// Paper Box Label Information Request : Request to MES
            /// </summary>
            /// <param name="inputData">PLC Data</param>
            public void BoxIDCheckRequest(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;
                        #region[Get EQP & LINE]
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                        if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                        #endregion
                        #region [PLCAgent Data Bit]
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion
                        #region[If Bit Off->Return]
                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                              BoxIDCheckRequestReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey, eReturnCode1.Unknown, "", "", "");
                              return;
                        }
                        #endregion
                        #region [PLCAgent Data Word]
                        string carrID = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                        string boxID = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                        #endregion
                        #region[Log]
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                             string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}], Node=[{3}] ,BoxIDCheckRequest BoxID =[{4}],PalletID =[{5}]",
                                     eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, boxID, carrID));
                        #endregion
                        //依設定 回機台OK/NG
                        eReturnCode1 rtncode = ParameterManager[eCELL_SWITCH.OFFLINE_REPLY_EQP].GetBoolean() == true ? eReturnCode1.OK : eReturnCode1.NG;

                        RecordPPKEventHistory(inputData.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, "1", boxID, "", "", "", "", "", "", "", "",
                            "", "", "", "", "", "", "", "", "", "", line.File.HostMode.ToString());
                        #region[If OFFLINE -> Return]
                        if (line.File.HostMode == eHostMode.OFFLINE)
                        {
                              Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.MES_OFFLINE_SKIP, line.Data.LINEID, inputData.TrackKey, MethodBase.GetCurrentMethod().Name));

                              BoxIDCheckRequestReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey, rtncode, "", "", "");
                              return;
                        }
                        #endregion
                        #region Add Reply Key
                        string key = keyBoxReplyPLCKey.BoxIDCheckRequestReply;
                        string rep = inputData.Metadata.NodeNo;
                        if (Repository.ContainsKey(key))
                              Repository.Remove(key);
                        Repository.Add(key, rep);
                        #endregion
                        #region [MES Data]
                        object[] _data = new object[4]
                { 
                    inputData.TrackKey,  
                    eqp.Data.LINEID,   
                    boxID,
                    carrID
                };
                        Invoke(eServiceName.MESService, "CheckBoxNameRequest", _data);
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                        if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                              BoxIDCheckRequestReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey, eReturnCode1.NG, "", "", "");
                  }
            }
            public void BoxIDCheckRequestReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 rtncode, string boxId, string boxtype, string carrID)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "BoxIDCheckRequestReply") as Trx;
                        #region[Get EQP & LINE]
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                        if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, eqpNo));

                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                        #endregion
                        #region[If Bit Off->Return]
                        if (value == eBitResult.OFF)
                        {
                              outputdata.EventGroups[0].Events[0].IsDisable = true;
                              outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                              outputdata.TrackKey = trackKey;
                              SendPLCData(outputdata);
                              if (_timerManager.IsAliveTimer(eqpNo + "_" + BoxIDCheckRequestTimeout))
                              {
                                    _timerManager.TerminateTimer(eqpNo + "_" + BoxIDCheckRequestTimeout);
                              }
                              #region[Log]
                              Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,CheckBoxNameRequestReply Set Bit =[{2})",
                                  eqpNo, trackKey, value.ToString()));
                              #endregion
                              return;
                        }
                        #endregion
                        #region[MES Data ]
                        outputdata.EventGroups[0].Events[0].Items[0].Value = carrID;
                        outputdata.EventGroups[0].Events[0].Items[1].Value = boxId;
                        outputdata.EventGroups[0].Events[0].Items[2].Value = ((int)rtncode).ToString();  // returnCode(INT);
                        //outputdata.EventGroups[0].Events[0].Items[3].Value = boxtype;  
                        outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString(); //Write Word dely 200 ms then Bit On
                        outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);
                        #endregion

                        #region [CarrID & BoxName Dictionary ]
                        if (rtncode == eReturnCode1.OK)
                        {
                              //if (eqp.File.CarrAndBoxMapping.ContainsKey(carrID))
                              //{
                              //eqp.File.CarrAndBoxMapping.Add(carrID, boxId);
                              //}
                        }
                        #endregion
                        #region[Create Timeout Timer]
                        if (_timerManager.IsAliveTimer(eqpNo + "_" + BoxIDCheckRequestTimeout))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + BoxIDCheckRequestTimeout);
                        }
                        _timerManager.CreateTimer(eqpNo + "_" + BoxIDCheckRequestTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(BoxIDCheckRequestReplyTimeout), trackKey);
                        #endregion
                        #region[Log]
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,CheckBoxNameRequestReply Set Bit =[{2}). ReTurn[{3}] BoxID = [{4}]",
                            eqpNo, trackKey, value.ToString(), rtncode.ToString(), boxId));
                        #endregion
                        RecordPPKEventHistory(outputdata.TrackKey, MethodBase.GetCurrentMethod().Name, eqp, "1", boxId, "", "", "", "", "", "", "", "",
                              rtncode.ToString(), "", "", "", "", "", "", "", "", "", line.File.HostMode.ToString());
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void BoxIDCheckRequestReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] EQP Reply, PaperBoxLabelInformationRequestReply Timeout Set Bit (OFF).", sArray[0], trackKey));
                        //CheckBoxNameRequestReply(string eqpNo, eBitResult value, string trackKey, eReturnCode1 rtncode, string boxId, string boxtype, string palletID)
                        BoxIDCheckRequestReply(sArray[0], eBitResult.OFF, trackKey, eReturnCode1.Unknown, "", "", "");
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion
      }
}