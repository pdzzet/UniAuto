using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Generic;
using System.Reflection;
using UniAuto.UniRCS.Core;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.PLCAgent.PLC;

namespace UniAuto.UniRCS.CSOT.t3.Service
{
    public partial class RobotSelectJobService
    {
        private void Get_EqpTypeMulitSlot_CanControlJobList_ForTCOVN_SD(Robot curRobot, RobotStage curStage, List<Job> curStageCanControlJobList)
        {
            string trxID = string.Empty;
            string strlog = string.Empty;

            #region Check Robot Context
            if (Workbench.LineType != eLineType.ARRAY.OVNSD_VIATRON)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "LineType is not OVNSD_VIATRON");
                return;
            }
            if (!StaticContext.ContainsKey(eRobotContextParameter.TCOVN_SD_RobotParam))
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "StaticContext is not Contains TCOVN_SD_RobotParam");
                return;
            }
            if (!(StaticContext[eRobotContextParameter.TCOVN_SD_RobotParam] is TCOVN_SD_RobotParam))
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "StaticContext is not Contains TCOVN_SD_RobotParam");
                return;
            }
            #endregion

            TCOVN_SD_RobotParam sd_param = (TCOVN_SD_RobotParam)StaticContext[eRobotContextParameter.TCOVN_SD_RobotParam];

            try
            {
                string bitOn = "1";
                string funcName = MethodBase.GetCurrentMethod().Name;

				#region [Get line fabtyep]
				string fabtype = eFabType.ARRAY.ToString();
				Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
				if (line != null) {
					fabtype = line.Data.FABTYPE;
				}
				#endregion

                #region [ Check Trx Setting ]

                if (curStage.Data.UPSTREAMPATHTRXNAME.Trim() == string.Empty)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find UpStream TrxID setting!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return;

                }

                #endregion

                //interface時Stage出片要看Upstream .透過LinkSignal已經得知JobData是否填好 Send Signal On =JobData Exist
                //string[] upStreamTrxList = curStage.Data.UPSTREAMPATHTRXNAME.Split(','); OVN SD 的 Stage 只看一組上下游 LinkSignal
                string strSlotNoBin = string.Empty;
                string strGlassCountBin = string.Empty;
                int slotNo = 0;

                #region  real time Get Interface Upstream

                trxID = curStage.Data.UPSTREAMPATHTRXNAME;

                Trx upStream_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

                if (upStream_Trx == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find TrxID({4})!",
                                                curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, funcName, string.Empty, string.Empty, string.Empty, string.Empty);
                    return;
                }

                #region  [DebugLog]

                if (IsShowDetialLog == true)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Get trx by TrxID({4})!",
                                            curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                            trxID);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                #endregion

                #region [拆出PLCAgent Data]

                #region [ Trx Structure ]

                //<trx name="L3_UpstreamPath#01" triggercondition="change">
                //    <eventgroup name="L3_EG_UpstreamPath#01" dir="E2B">
                //      <event name="L3_B_UpstreamPath#01" trigger="true" />
                //    </eventgroup>
                //</trx>

                //<event name="L3_B_UpstreamPath#01" devicecode="B" address="0x0000B00" points="32">
                //  <item name="UpstreamPath#01UpstreamInline" offset="0" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01UpstreamTrouble" offset="1" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01SendReady" offset="2" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01Send" offset="3" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01JobTransfer" offset="4" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01SendCancel" offset="5" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01ExchangeExecute" offset="6" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01DoubleGlass" offset="7" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01SendJobReserve" offset="8" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01SendOK" offset="9" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01PinUpRequest" offset="13" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01PinDownComplete" offset="14" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01SlotNumber#01" offset="16" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01SlotNumber#02" offset="17" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01SlotNumber#03" offset="18" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01SlotNumber#04" offset="19" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01SlotNumber#05" offset="20" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01SlotNumber#06" offset="21" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01GlassCount#01" offset="22" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01GlassCount#02" offset="23" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01GlassCount#03" offset="24" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01GlassCount#04" offset="25" points="1" expression="BIT" />
                //</event>

                #endregion
				#region [variable declare]
				string up_UpstreamInline = "0";
				string up_UpstreamTrouble = "0";
				string up_SendReady = "0";
				string up_Send = "0";
				string up_JobTransfer = "0";
				string up_SendCancel = "0";
				string up_ExchangeExecute = "0";
				string up_DoubleGlass = "0";
				string up_SendJobReserve = "0";
				string up_SendOK = "0";
				string up_ReceiveOK = "0";
				string up_PinUpRequest = "0";
				string up_PinDownComplete = "0";
				string up_SlotNumber01 = "0";
				string up_SlotNumber02 = "0";
				string up_SlotNumber03 = "0";
				string up_SlotNumber04 = "0";
				string up_SlotNumber05 = "0";
				string up_SlotNumber06 = "0";
				string up_GlassCount01 = "0";
				string up_GlassCount02 = "0";
				string up_GlassCount03 = "0";
				string up_GlassCount04 = "0";
				#endregion
				if (fabtype == eFabType.CF.ToString()) {
					#region CF
					up_UpstreamInline = upStream_Trx.EventGroups[0].Events[0].Items[0].Value;
					up_UpstreamTrouble = upStream_Trx.EventGroups[0].Events[0].Items[1].Value;
					up_SendReady = upStream_Trx.EventGroups[0].Events[0].Items[2].Value;
					up_Send = upStream_Trx.EventGroups[0].Events[0].Items[3].Value;
					up_JobTransfer = upStream_Trx.EventGroups[0].Events[0].Items[4].Value;
					up_SendCancel = upStream_Trx.EventGroups[0].Events[0].Items[5].Value;
					up_ExchangeExecute = upStream_Trx.EventGroups[0].Events[0].Items[6].Value;
					up_DoubleGlass = upStream_Trx.EventGroups[0].Events[0].Items[7].Value;
					up_SendJobReserve = upStream_Trx.EventGroups[0].Events[0].Items[8].Value;
					up_ReceiveOK = upStream_Trx.EventGroups[0].Events[0].Items[9].Value;
					up_SlotNumber01 = upStream_Trx.EventGroups[0].Events[0].Items[10].Value;
					up_SlotNumber02 = upStream_Trx.EventGroups[0].Events[0].Items[11].Value;
					up_SlotNumber03 = upStream_Trx.EventGroups[0].Events[0].Items[12].Value;
					up_SlotNumber04 = upStream_Trx.EventGroups[0].Events[0].Items[13].Value;
					up_SlotNumber05 = upStream_Trx.EventGroups[0].Events[0].Items[14].Value;
					up_SlotNumber06 = upStream_Trx.EventGroups[0].Events[0].Items[15].Value;
					#endregion
				} else {
					#region [default]
					up_UpstreamInline = upStream_Trx.EventGroups[0].Events[0].Items[0].Value;
					up_UpstreamTrouble = upStream_Trx.EventGroups[0].Events[0].Items[1].Value;
					up_SendReady = upStream_Trx.EventGroups[0].Events[0].Items[2].Value;
					up_Send = upStream_Trx.EventGroups[0].Events[0].Items[3].Value;
					up_JobTransfer = upStream_Trx.EventGroups[0].Events[0].Items[4].Value;
					up_SendCancel = upStream_Trx.EventGroups[0].Events[0].Items[5].Value;
					up_ExchangeExecute = upStream_Trx.EventGroups[0].Events[0].Items[6].Value;
					up_DoubleGlass = upStream_Trx.EventGroups[0].Events[0].Items[7].Value;
					up_SendJobReserve = upStream_Trx.EventGroups[0].Events[0].Items[8].Value;
					up_SendOK = upStream_Trx.EventGroups[0].Events[0].Items[9].Value;
					up_PinUpRequest = upStream_Trx.EventGroups[0].Events[0].Items[10].Value;
					up_PinDownComplete = upStream_Trx.EventGroups[0].Events[0].Items[11].Value;
					up_SlotNumber01 = upStream_Trx.EventGroups[0].Events[0].Items[12].Value;
					up_SlotNumber02 = upStream_Trx.EventGroups[0].Events[0].Items[13].Value;
					up_SlotNumber03 = upStream_Trx.EventGroups[0].Events[0].Items[14].Value;
					up_SlotNumber04 = upStream_Trx.EventGroups[0].Events[0].Items[15].Value;
					up_SlotNumber05 = upStream_Trx.EventGroups[0].Events[0].Items[16].Value;
					up_SlotNumber06 = upStream_Trx.EventGroups[0].Events[0].Items[17].Value;
					up_GlassCount01 = upStream_Trx.EventGroups[0].Events[0].Items[18].Value;
					up_GlassCount02 = upStream_Trx.EventGroups[0].Events[0].Items[19].Value;
					up_GlassCount03 = upStream_Trx.EventGroups[0].Events[0].Items[20].Value;
					up_GlassCount04 = upStream_Trx.EventGroups[0].Events[0].Items[21].Value;
					#endregion
				}
                #endregion

                //Stage Mulit Single表示是走ST to RB Mulit Slot Signal Mode
                if (up_UpstreamInline == bitOn && up_SendReady == bitOn && up_Send == bitOn)
                {
                    #region [ 將SlotNo Bit 轉成Int ]

                    strSlotNoBin = up_SlotNumber06 + up_SlotNumber05 + up_SlotNumber04 + up_SlotNumber03 + up_SlotNumber02 + up_SlotNumber01;

                    try
                    {
                        slotNo = Convert.ToInt32(strSlotNoBin, 2);
                    }
                    catch (Exception)
                    {
                        slotNo = 0;
                    }

                    #endregion

                    if (slotNo == 0)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TrxID({4}) report SlotNo is 0!",
                                                    curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    trxID);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //SlotNo沒填無法確認如何出片
                        UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, funcName, string.Empty, string.Empty, string.Empty, string.Empty);
                        return;
                    }

                    #region  [ 先根據根據SlotNo 取得JobData ]

                    Job curSendOutJob = new Job();

                    // 在 OVN SD, LinkSignal 與 Job Data 區塊有對應關係
                    if (Get_LinkSignalSendOutJobInfo_ForMulitSingle(curRobot, curStage, curStageCanControlJobList, slotNo, 1, out curSendOutJob) == true)
                    {
                        // 在 OVN SD, LinkSignal 的 Double Glass 永遠是 Off

                        #region [ EQP SendOut Job,  Add to curUDRQ_SlotList ]

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) UpstreamInline({5}) SendReady({6}) SendSignal({7}) ,Stage UDRQ Status change to (UDRQ)!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    trxID, eBitResult.ON.ToString(), eBitResult.ON.ToString(), eBitResult.ON.ToString());
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        if (sd_param.GetSendReadyOnTime(curStage.Data.STAGEID) == DateTime.MinValue)
                            sd_param.SetSendReadyOnTime(curStage.Data.STAGEID, DateTime.Now);

                        if ((sd_param.IsBufferStage(curStage.Data.STAGEID) && sd_param.CheckBufferGetGet()) ||
                            (sd_param.IsCoolerStage(curStage.Data.STAGEID) && sd_param.CheckCoolerGetGet()))
                        {
                            // 等到 Buffer Cooler 可以 Get Get 時, 才將 Slot 加入 UDRQ_SlotList 以及 Job 加入 StageCanControlJobList
                            lock (curStage)
                            {
                                if (curStage.curUDRQ_SlotList.ContainsKey(curSendOutJob.RobotWIP.CurLocation_SlotNo) == false)
                                {
                                    curStage.curUDRQ_SlotList.Add(curSendOutJob.RobotWIP.CurLocation_SlotNo, string.Format("{0}_{1}", curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo));
                                }
                            }
                        }
                        else
                        {
                            // Buffer Cooler 還不能 Get Get 且尚未 Timeout, StageCanControlJobList 暫時移除
                            // 等到可以 Get Get 或 Timeout 時才加入
                            if (curSendOutJob != null)
                                curStageCanControlJobList.Remove(curSendOutJob);
                        }

                        // 但是 Stage 狀態照常更新為 SEND_OUT_READY, 這樣 OPI 上才看得到即時狀態
                        UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.SEND_OUT_READY, funcName, curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo, string.Empty, string.Empty);
                        #endregion
                    }
                    else
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) can not get SendOut JobData, Stage UDRQ Status change from ({3}) to ({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.File.CurStageStatus,
                                                    eRobotStageStatus.NO_REQUEST);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //無SendOut Job Info Status UDRQ Stage Change To NOREQ
                        UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, funcName, string.Empty, string.Empty, string.Empty, string.Empty);

                    }

                    #endregion

                }
                else
                {

                    //Monitor 條件不合的狀態 Status UDRQ Stage Change To NOREQ
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) UpstreamInline({5}) SendReady({6}) SendSignal({7}), Stage UDRQ Status can not change to (UDRQ)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, (eBitResult)int.Parse(up_UpstreamInline), (eBitResult)int.Parse(up_SendReady), (eBitResult)int.Parse(up_Send));
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, funcName, string.Empty, string.Empty, string.Empty, string.Empty);
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void Get_EqpTypeMuliSlot_LDRQStauts_ForTCOVN_SD(Robot curRobot, RobotStage curStage)
        {

            string trxID = string.Empty;
            string strlog = string.Empty;

            #region Check Robot Context
            if (Workbench.LineType != eLineType.ARRAY.OVNSD_VIATRON)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "LineType is not OVNSD_VIATRON");
                return;
            }
            if (!StaticContext.ContainsKey(eRobotContextParameter.TCOVN_SD_RobotParam))
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "StaticContext is not Contains TCOVN_SD_RobotParam");
                return;
            }
            if (!(StaticContext[eRobotContextParameter.TCOVN_SD_RobotParam] is TCOVN_SD_RobotParam))
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "StaticContext is not Contains TCOVN_SD_RobotParam");
                return;
            }
            #endregion

            TCOVN_SD_RobotParam sd_param = (TCOVN_SD_RobotParam)StaticContext[eRobotContextParameter.TCOVN_SD_RobotParam];

            try
            {
                string bitOn = "1";
                string bitOff = "0";
                string funcName = MethodBase.GetCurrentMethod().Name;

				#region [Get line fabtyep]
				string fabtype = eFabType.ARRAY.ToString();
				Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
				if (line != null) {
					fabtype = line.Data.FABTYPE;
				}
				#endregion

                #region [ Check Trx Setting ]

                if (curStage.Data.DOWNSTREAMPATHTRXNAME.Trim() == string.Empty)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find DownStream TrxID setting!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME);//modify by Menghui 20170331 缺少curStage.Data.STAGENAME导致索引数量大于参数列表，程序异常
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return;

                }

                #endregion

                //string[] downStreamTrxList = curStage.Data.DOWNSTREAMPATHTRXNAME.Split(','); OVN SD 的 Stage 只看一組上下游 LinkSignal
                string strSlotNoBin = string.Empty;
                int slotNo = 0;

                #region  real time Get Interface downstream

                trxID = curStage.Data.DOWNSTREAMPATHTRXNAME;

                Trx downStream_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

                if (downStream_Trx == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find TrxID({4})!",
                                                curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return;
                }

                #region  [DebugLog]

                if (IsShowDetialLog == true)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Get trx by TrxID({4})!",
                                            curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                            trxID);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                #endregion

                #region [拆出PLCAgent Data]

                #region [ Trx Structure ]

                //<trx name="L3_DownstreamPath#01" triggercondition="change">
                //    <eventgroup name="L3_EG_DownstreamPath#01" dir="E2B">
                //      <event name="L3_B_DownstreamPath#01" trigger="true" />
                //    </eventgroup>
                //  </trx>

                //<event name="L3_B_DownstreamPath#01" devicecode="B" address="0x0000A00" points="32">
                //  <item name="DownstreamPath#01DownstreamInline" offset="0" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01DownstreamTrouble" offset="1" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ReceiveAble" offset="2" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01Receive" offset="3" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01JobTransfer" offset="4" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ReceiveCancel" offset="5" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ExchangePossible" offset="6" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01DoubleGlass" offset="7" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ReceiveJobReserve" offset="8" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ReceiveOK" offset="9" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01TransferStopRequest" offset="10" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01DummyGlassRequest" offset="11" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01GlassExist" offset="12" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01PinUpComplete" offset="13" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01PinDownRequest" offset="14" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01SlotNumber#01" offset="16" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01SlotNumber#02" offset="17" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01SlotNumber#03" offset="18" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01SlotNumber#04" offset="19" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01SlotNumber#05" offset="20" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01SlotNumber#06" offset="21" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01GlassCount#01" offset="22" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01GlassCount#02" offset="23" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01GlassCount#03" offset="24" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01GlassCount#04" offset="25" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ReceiveType#01" offset="26" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ReceiveType#02" offset="27" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ReceiveType#03" offset="28" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ReceiveType#04" offset="29" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ReceiveType#05" offset="30" points="1" expression="BIT" />
                //  <item name="DownstreamPath#01ReceiveType#06" offset="31" points="1" expression="BIT" />
                //</event>

                #endregion
				#region [variable declare]
				string down_DownstreamInline = "0";
				string down_DownstreamTrouble = "0";
				string down_ReceiveAble = "0";
				string down_Receive = "0";
				string down_JobTransfer = "0";
				string down_ReceiveCancel = "0";
				string down_ExchangePossible = "0";
				string down_DoubleGlass = "0";
				string down_ReceiveJobReserve = "0";
				string down_ReceiveOK = "0";
				string down_TransferStopRequest = "0";
				string down_DummyGlassRequest = "0";
				string down_GlassExist = "0";
				string down_PinUpComplete = "0";
				string down_PinDownRequest = "0";
				string down_SlotNumber01 = "0";
				string down_SlotNumber02 = "0";
				string down_SlotNumber03 = "0";
				string down_SlotNumber04 = "0";
				string down_SlotNumber05 = "0";
				string down_SlotNumber06 = "0";
				string down_GlassCount01 = "0";
				string down_GlassCount02 = "0";
				string down_GlassCount03 = "0";
				string down_GlassCount04 = "0";
				string down_ReceiveType01 = "0";
				string down_ReceiveType02 = "0";
				string down_ReceiveType03 = "0";
				string down_ReceiveType04 = "0";
				string down_ReceiveType05 = "0";
				string down_ReceiveType06 = "0";
				string down_GlassCount05 = "0";
				string down_PreparationPermission = "0";
				string down_InspectionResultUpdate = "0";
				string down_ReturnMode = "0";
				#endregion
				if (fabtype == eFabType.CF.ToString()) {
					#region CF
					down_DownstreamInline = downStream_Trx.EventGroups[0].Events[0].Items[0].Value;
					down_DownstreamTrouble = downStream_Trx.EventGroups[0].Events[0].Items[1].Value;
					down_ReceiveAble = downStream_Trx.EventGroups[0].Events[0].Items[2].Value;
					down_Receive = downStream_Trx.EventGroups[0].Events[0].Items[3].Value;
					down_JobTransfer = downStream_Trx.EventGroups[0].Events[0].Items[4].Value;
					down_ReceiveCancel = downStream_Trx.EventGroups[0].Events[0].Items[5].Value;
					down_ExchangePossible = downStream_Trx.EventGroups[0].Events[0].Items[6].Value;
					down_DoubleGlass = downStream_Trx.EventGroups[0].Events[0].Items[7].Value;
					down_ReceiveJobReserve = downStream_Trx.EventGroups[0].Events[0].Items[8].Value;
					down_TransferStopRequest = downStream_Trx.EventGroups[0].Events[0].Items[9].Value;
					down_DummyGlassRequest = downStream_Trx.EventGroups[0].Events[0].Items[10].Value;
					down_GlassExist = downStream_Trx.EventGroups[0].Events[0].Items[11].Value;
					down_SlotNumber01 = downStream_Trx.EventGroups[0].Events[0].Items[12].Value;
					down_SlotNumber02 = downStream_Trx.EventGroups[0].Events[0].Items[13].Value;
					down_SlotNumber03 = downStream_Trx.EventGroups[0].Events[0].Items[14].Value;
					down_SlotNumber04 = downStream_Trx.EventGroups[0].Events[0].Items[15].Value;
					down_SlotNumber05 = downStream_Trx.EventGroups[0].Events[0].Items[16].Value;
					down_SlotNumber06 = downStream_Trx.EventGroups[0].Events[0].Items[17].Value;
					down_GlassCount01 = downStream_Trx.EventGroups[0].Events[0].Items[18].Value;
					down_GlassCount02 = downStream_Trx.EventGroups[0].Events[0].Items[19].Value;
					down_GlassCount03 = downStream_Trx.EventGroups[0].Events[0].Items[20].Value;
					down_GlassCount04 = downStream_Trx.EventGroups[0].Events[0].Items[21].Value;
					down_GlassCount05 = downStream_Trx.EventGroups[0].Events[0].Items[22].Value;
					down_PreparationPermission = downStream_Trx.EventGroups[0].Events[0].Items[23].Value;
					down_InspectionResultUpdate = downStream_Trx.EventGroups[0].Events[0].Items[24].Value;
					down_ReturnMode = downStream_Trx.EventGroups[0].Events[0].Items[25].Value;

					#endregion
				} else {
					#region default
					down_DownstreamInline = downStream_Trx.EventGroups[0].Events[0].Items[0].Value;
					down_DownstreamTrouble = downStream_Trx.EventGroups[0].Events[0].Items[1].Value;
					down_ReceiveAble = downStream_Trx.EventGroups[0].Events[0].Items[2].Value;
					down_Receive = downStream_Trx.EventGroups[0].Events[0].Items[3].Value;
					down_JobTransfer = downStream_Trx.EventGroups[0].Events[0].Items[4].Value;
					down_ReceiveCancel = downStream_Trx.EventGroups[0].Events[0].Items[5].Value;
					down_ExchangePossible = downStream_Trx.EventGroups[0].Events[0].Items[6].Value;
					down_DoubleGlass = downStream_Trx.EventGroups[0].Events[0].Items[7].Value;
					down_ReceiveJobReserve = downStream_Trx.EventGroups[0].Events[0].Items[8].Value;
					down_ReceiveOK = downStream_Trx.EventGroups[0].Events[0].Items[9].Value;
					down_TransferStopRequest = downStream_Trx.EventGroups[0].Events[0].Items[10].Value;
					down_DummyGlassRequest = downStream_Trx.EventGroups[0].Events[0].Items[11].Value;
					down_GlassExist = downStream_Trx.EventGroups[0].Events[0].Items[12].Value;
					down_PinUpComplete = downStream_Trx.EventGroups[0].Events[0].Items[13].Value;
					down_PinDownRequest = downStream_Trx.EventGroups[0].Events[0].Items[14].Value;
					down_SlotNumber01 = downStream_Trx.EventGroups[0].Events[0].Items[15].Value;
					down_SlotNumber02 = downStream_Trx.EventGroups[0].Events[0].Items[16].Value;
					down_SlotNumber03 = downStream_Trx.EventGroups[0].Events[0].Items[17].Value;
					down_SlotNumber04 = downStream_Trx.EventGroups[0].Events[0].Items[18].Value;
					down_SlotNumber05 = downStream_Trx.EventGroups[0].Events[0].Items[19].Value;
					down_SlotNumber06 = downStream_Trx.EventGroups[0].Events[0].Items[20].Value;
					down_GlassCount01 = downStream_Trx.EventGroups[0].Events[0].Items[21].Value;
					down_GlassCount02 = downStream_Trx.EventGroups[0].Events[0].Items[22].Value;
					down_GlassCount03 = downStream_Trx.EventGroups[0].Events[0].Items[23].Value;
					down_GlassCount04 = downStream_Trx.EventGroups[0].Events[0].Items[24].Value;
					down_ReceiveType01 = downStream_Trx.EventGroups[0].Events[0].Items[25].Value;
					down_ReceiveType02 = downStream_Trx.EventGroups[0].Events[0].Items[26].Value;
					down_ReceiveType03 = downStream_Trx.EventGroups[0].Events[0].Items[27].Value;
					down_ReceiveType04 = downStream_Trx.EventGroups[0].Events[0].Items[28].Value;
					down_ReceiveType05 = downStream_Trx.EventGroups[0].Events[0].Items[29].Value;
					down_ReceiveType06 = downStream_Trx.EventGroups[0].Events[0].Items[30].Value;
					#endregion
				}
                #endregion

                //Stage GetGet表示是走ST to RB Mulit Slot Signal Mode
                if (down_DownstreamInline == bitOn && down_ReceiveAble == bitOn && down_Receive == bitOff)
                {
                    #region [ 將SlotNo Bit 轉成Int ]

                    strSlotNoBin = down_SlotNumber06 + down_SlotNumber05 + down_SlotNumber04 + down_SlotNumber03 + down_SlotNumber02 + down_SlotNumber01;

                    try
                    {
                        slotNo = Convert.ToInt32(strSlotNoBin, 2);
                    }
                    catch (Exception)
                    {
                        slotNo = 0;
                    }

                    #endregion

                    if (slotNo == 0)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TrxID({4}) report SlotNo is 0!",
                                                    curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    trxID);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //無法確認則視為無法收片 只須更新Stage LDRQ Status即可
                        UpdateStage_LDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, curStage.File.LDRQ_CstStatusPriority, funcName);
                        return;
                    }

                    // 在 OVN SD, LinkSignal 的 Double Glass 永遠是 Off
                    #region  [ 根據根據SlotNo取得Stage EmptySlotNo ]

                    #region 更新Current Stage LDRQ Empty Slot

                    if (sd_param.GetReceiveAbleOnTime(curStage.Data.STAGEID) == DateTime.MinValue)
                        sd_param.SetReceiveAbleOnTime(curStage.Data.STAGEID, DateTime.Now);

                    if (sd_param.IsBufferStage(curStage.Data.STAGEID) && sd_param.CheckBufferPutPut())
                    {
                        // 等到 Buffer 可以 Put Put 時, 才將 Slot 加入 LDRQ_EmptySlotList
                        lock (curStage)
                        {
                            curStage.CurLDRQ_EmptySlotNo = slotNo.ToString().PadLeft(2, '0');
                            curStage.CurLDRQ_EmptySlotNo02 = string.Empty;
                            //add Empty SlotNo To EmptySlotNoList
                            if (curStage.curLDRQ_EmptySlotList.ContainsKey(slotNo) == false)
                            {
                                curStage.curLDRQ_EmptySlotList.Add(slotNo, string.Empty);
                            }
                        }
                    }
                    #endregion

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) DownstreamInline({5}) ReceiveAble({6}) Receive({7}) DoubleGlass({8}) EmptySlotNo01({9})  EmptySlotNo02({10}), Stage LDRQ Status change to (LDRQ).",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, (eBitResult)int.Parse(down_DownstreamInline), (eBitResult)int.Parse(down_ReceiveAble), (eBitResult)int.Parse(down_Receive),
                                                (eBitResult)int.Parse(down_DoubleGlass), curStage.CurLDRQ_EmptySlotNo, curStage.CurLDRQ_EmptySlotNo02);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }


                    #endregion

                    //只須更新Stage LDRQ Status即可
                    UpdateStage_LDRQ_Status(curStage, eRobotStageStatus.RECEIVE_READY, curStage.File.LDRQ_CstStatusPriority, funcName);

                    #endregion

                }
                else
                {
                    //Monitor 條件不合的狀態 Status LDRQ Stage Change To NOREQ
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) DownstreamInline({5}) ReceiveAble({6}) Receive({7}), Stage LDRQ Status can not change to (LDRQ)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, (eBitResult)int.Parse(down_DownstreamInline), (eBitResult)int.Parse(down_ReceiveAble), (eBitResult)int.Parse(down_Receive));
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    //只須更新Stage LDRQ Status即可
                    UpdateStage_LDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, curStage.File.LDRQ_CstStatusPriority, funcName);

                }

                #region [ 20151215 add 如果Transfer Stop Request On 則要更新Stage狀態 ]

                if (down_TransferStopRequest == bitOn)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) TransferStopRequest({5}),Update RobotStage Transfer Stop Request Flag(True)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, eBitResult.ON.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    lock (curStage.File)
                    {
                        curStage.File.DownStreamTransferStopRequestFlag = true;
                    }

                }
                else
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) TransferStopRequest({5}),Update RobotStage Transfer Stop Request Flag(False)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                curStage.Data.STAGENAME, trxID, eBitResult.OFF.ToString());
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    lock (curStage.File)
                    {
                        curStage.File.DownStreamTransferStopRequestFlag = false;
                    }

                }

                #endregion

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }


        }

        private void Get_EqpTypeMulitSlot_CanControlJobList_ForDUAL(Robot curRobot, RobotStage curStage, List<Job> curStageCanControlJobList)
        {
            string trxID = string.Empty;
            string strlog = string.Empty;

            try
            {
                string bitOn = "1";
                string bitOff = "0";
                string funcName = MethodBase.GetCurrentMethod().Name;

				#region [Get line fabtyep]
				string fabtype = eFabType.ARRAY.ToString();
				Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
				if (line != null) {
					fabtype = line.Data.FABTYPE;
				}
				#endregion

                #region [ Check Trx Setting ]

                if (curStage.Data.UPSTREAMPATHTRXNAME.Trim() == string.Empty)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find UpStream TrxID setting!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return;

                }

                #endregion

                //interface時Stage出片要看Upstream .透過LinkSignal已經得知JobData是否填好 Send Signal On =JobData Exist
                string strSlotNoBin = string.Empty;
                string strGlassCountBin = string.Empty;
                int slotNo = 0;
                int anotherSlotNo = 0;

                #region  real time Get Interface Upstream

                trxID = curStage.Data.UPSTREAMPATHTRXNAME;

                Trx upStream_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

                if (upStream_Trx == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find TrxID({4})!",
                                                curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME, trxID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return;
                }

                #region  [DebugLog]

                if (IsShowDetialLog == true)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Get trx by TrxID({4})!",
                                            curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                            trxID);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                #endregion
				
                #region [拆出PLCAgent Data]
                #region [ Trx Structure ]

                //<trx name="L3_UpstreamPath#01" triggercondition="change">
                //    <eventgroup name="L3_EG_UpstreamPath#01" dir="E2B">
                //      <event name="L3_B_UpstreamPath#01" trigger="true" />
                //    </eventgroup>
                //</trx>

                //<event name="L3_B_UpstreamPath#01" devicecode="B" address="0x0000B00" points="32">
                //  <item name="UpstreamPath#01UpstreamInline" offset="0" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01UpstreamTrouble" offset="1" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01SendReady" offset="2" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01Send" offset="3" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01JobTransfer" offset="4" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01SendCancel" offset="5" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01ExchangeExecute" offset="6" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01DoubleGlass" offset="7" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01SendJobReserve" offset="8" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01SendOK" offset="9" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01PinUpRequest" offset="13" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01PinDownComplete" offset="14" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01SlotNumber#01" offset="16" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01SlotNumber#02" offset="17" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01SlotNumber#03" offset="18" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01SlotNumber#04" offset="19" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01SlotNumber#05" offset="20" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01SlotNumber#06" offset="21" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01GlassCount#01" offset="22" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01GlassCount#02" offset="23" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01GlassCount#03" offset="24" points="1" expression="BIT" />
                //  <item name="UpstreamPath#01GlassCount#04" offset="25" points="1" expression="BIT" />
                //</event>

                #endregion
				#region [variable declare]
				string up_UpstreamInline = "0";
				string up_UpstreamTrouble = "0";
				string up_SendReady = "0";
				string up_Send = "0";
				string up_JobTransfer = "0";
				string up_SendCancel = "0";
				string up_ExchangeExecute = "0";
				string up_DoubleGlass = "0";
				string up_SendJobReserve = "0";
				string up_SendOK = "0";
				string up_ReceiveOK = "0";
				string up_PinUpRequest = "0";
				string up_PinDownComplete = "0";
				string up_SlotNumber01 = "0";
				string up_SlotNumber02 = "0";
				string up_SlotNumber03 = "0";
				string up_SlotNumber04 = "0";
				string up_SlotNumber05 = "0";
				string up_SlotNumber06 = "0";
				string up_GlassCount01 = "0";
				string up_GlassCount02 = "0";
				string up_GlassCount03 = "0";
				string up_GlassCount04 = "0";
				#endregion
				if (fabtype == eFabType.CF.ToString()) {
					#region CF
					up_UpstreamInline = upStream_Trx.EventGroups[0].Events[0].Items[0].Value;
					up_UpstreamTrouble = upStream_Trx.EventGroups[0].Events[0].Items[1].Value;
					up_SendReady = upStream_Trx.EventGroups[0].Events[0].Items[2].Value;
					up_Send = upStream_Trx.EventGroups[0].Events[0].Items[3].Value;
					up_JobTransfer = upStream_Trx.EventGroups[0].Events[0].Items[4].Value;
					up_SendCancel = upStream_Trx.EventGroups[0].Events[0].Items[5].Value;
					up_ExchangeExecute = upStream_Trx.EventGroups[0].Events[0].Items[6].Value;
					up_DoubleGlass = upStream_Trx.EventGroups[0].Events[0].Items[7].Value;
					up_SendJobReserve = upStream_Trx.EventGroups[0].Events[0].Items[8].Value;
					up_ReceiveOK = upStream_Trx.EventGroups[0].Events[0].Items[9].Value;
					up_SlotNumber01 = upStream_Trx.EventGroups[0].Events[0].Items[10].Value;
					up_SlotNumber02 = upStream_Trx.EventGroups[0].Events[0].Items[11].Value;
					up_SlotNumber03 = upStream_Trx.EventGroups[0].Events[0].Items[12].Value;
					up_SlotNumber04 = upStream_Trx.EventGroups[0].Events[0].Items[13].Value;
					up_SlotNumber05 = upStream_Trx.EventGroups[0].Events[0].Items[14].Value;
					up_SlotNumber06 = upStream_Trx.EventGroups[0].Events[0].Items[15].Value;					
					#endregion
				} else {
					#region [default]
					up_UpstreamInline = upStream_Trx.EventGroups[0].Events[0].Items[0].Value;
					up_UpstreamTrouble = upStream_Trx.EventGroups[0].Events[0].Items[1].Value;
					up_SendReady = upStream_Trx.EventGroups[0].Events[0].Items[2].Value;
					up_Send = upStream_Trx.EventGroups[0].Events[0].Items[3].Value;
					up_JobTransfer = upStream_Trx.EventGroups[0].Events[0].Items[4].Value;
					up_SendCancel = upStream_Trx.EventGroups[0].Events[0].Items[5].Value;
					up_ExchangeExecute = upStream_Trx.EventGroups[0].Events[0].Items[6].Value;
					up_DoubleGlass = upStream_Trx.EventGroups[0].Events[0].Items[7].Value;
					up_SendJobReserve = upStream_Trx.EventGroups[0].Events[0].Items[8].Value;
					up_SendOK = upStream_Trx.EventGroups[0].Events[0].Items[9].Value;
					up_PinUpRequest = upStream_Trx.EventGroups[0].Events[0].Items[10].Value;
					up_PinDownComplete = upStream_Trx.EventGroups[0].Events[0].Items[11].Value;
					up_SlotNumber01 = upStream_Trx.EventGroups[0].Events[0].Items[12].Value;
					up_SlotNumber02 = upStream_Trx.EventGroups[0].Events[0].Items[13].Value;
					up_SlotNumber03 = upStream_Trx.EventGroups[0].Events[0].Items[14].Value;
					up_SlotNumber04 = upStream_Trx.EventGroups[0].Events[0].Items[15].Value;
					up_SlotNumber05 = upStream_Trx.EventGroups[0].Events[0].Items[16].Value;
					up_SlotNumber06 = upStream_Trx.EventGroups[0].Events[0].Items[17].Value;
					up_GlassCount01 = upStream_Trx.EventGroups[0].Events[0].Items[18].Value;
					up_GlassCount02 = upStream_Trx.EventGroups[0].Events[0].Items[19].Value;
					up_GlassCount03 = upStream_Trx.EventGroups[0].Events[0].Items[20].Value;
					up_GlassCount04 = upStream_Trx.EventGroups[0].Events[0].Items[21].Value;
					#endregion
				}
                #endregion

                //Stage Mulit Single表示是走ST to RB Mulit Slot Signal Mode
                if (up_UpstreamInline == bitOn && up_SendReady == bitOn && up_Send == bitOn)
                {
                    //LinkSignal Bit符合出片條件 Mulit Slot=> SlotNo= SendOutJobData No
                    //注意!!! Array  IMP Stage  SlotNo=1是指上層.
                    // TCOVN Stage SlotNo單數是指下層, 雙數指上層
                    #region [ 將SlotNo Bit 轉成Int ]

                    strSlotNoBin = up_SlotNumber06 + up_SlotNumber05 + up_SlotNumber04 + up_SlotNumber03 + up_SlotNumber02 + up_SlotNumber01;

                    try
                    {
                        slotNo = Convert.ToInt32(strSlotNoBin, 2);
                    }
                    catch (Exception)
                    {
                        slotNo = 0;
                    }

                    #endregion

                    #region 檢查 SlotNo
                    if (slotNo == 0)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TrxID({4}) report SlotNo is 0!",
                                                    curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME, trxID);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //SlotNo沒填無法確認如何出片
                        UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, funcName, string.Empty, string.Empty, string.Empty, string.Empty);
                        return;
                    }

                    if (up_DoubleGlass == bitOn && slotNo % 2 == 0)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TrxID({4}) report Double Glass on but Slot No is not odd!",
                                                    curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME, trxID);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //DoubleGlass On時, SlotNo必須是單數
                        UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, funcName, string.Empty, string.Empty, string.Empty, string.Empty);
                        return;
                    }
                    #endregion

                    #region  [ 先根據根據SlotNo 取得JobData ]

                    Job curSendOutJob = new Job();

                    //Mulit Signal 1bit 2Word
                    //不管一片還是兩片 第一片都會顯示在JobData Send#1.
                    if (Get_LinkSignalSendOutJobInfo_ForMulitSingle(curRobot, curStage, curStageCanControlJobList, slotNo, 1, out curSendOutJob) == true)
                    {
                        //找到符合SendOut的Job ,判斷是SendOut 2片還是一片
                        if (up_DoubleGlass != bitOn)
                        {

                            #region [ EQP Only SendOut 1 Job ]

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) UpstreamInline({5}) SendReady({6}) SendSignal({7}) ,Stage UDRQ Status change to (UDRQ)!",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                        trxID, eBitResult.ON.ToString(), eBitResult.ON.ToString(), eBitResult.ON.ToString());
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            //20151024 add for SendOut Job SlotNo
                            lock (curStage)
                            {
                                if (curStage.curUDRQ_SlotList.ContainsKey(curSendOutJob.RobotWIP.CurLocation_SlotNo) == false)
                                {
                                    curStage.curUDRQ_SlotList.Add(curSendOutJob.RobotWIP.CurLocation_SlotNo, string.Format("{0}_{1}", curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo));
                                }
                            }

                            //Update Status UDRQ Stage Change To UDRQ
                            UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.SEND_OUT_READY, funcName, curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo, string.Empty, string.Empty);

                            #endregion

                            return;

                        }
                        else
                        {

                            #region [ EQP SendOut 2 Job 20150819與京光討論結果. 第二片就算有問題還是要將第一片取出 ]

                            #region [ Get Another SlotNo ]

                            //在TCOVN_PL_ITO, DoubleGlass On時SlotNo必為單數下層Slot
                            //anotherSlotNo必為雙數上層Slot
                            anotherSlotNo = slotNo + 1;

                            #endregion

                            #region  [ 先根據根據 AnotherSlotNo 取得JobData02 ]

                            Job curSendOutJob02 = new Job();

                            if (Get_LinkSignalSendOutJobInfo_ForMulitSingle(curRobot, curStage, curStageCanControlJobList, anotherSlotNo, 2, out curSendOutJob02) == true)
                            {

                                #region [ EQP SendOut 2 Job ]

                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) UpstreamInline({5}) SendReady({6}) SendSignal({7}) ,Stage UDRQ Status change to (UDRQ)!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                            trxID, eBitResult.ON.ToString(), eBitResult.ON.ToString(), eBitResult.ON.ToString());
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                //20151024 add for SendOut Job SlotNo
                                lock (curStage)
                                {
                                    if (curStage.curUDRQ_SlotList.ContainsKey(curSendOutJob.RobotWIP.CurLocation_SlotNo) == false)
                                    {
                                        curStage.curUDRQ_SlotList.Add(curSendOutJob.RobotWIP.CurLocation_SlotNo, string.Format("{0}_{1}", curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo));
                                    }

                                    if (curStage.curUDRQ_SlotList.ContainsKey(curSendOutJob02.RobotWIP.CurLocation_SlotNo) == false)
                                    {
                                        curStage.curUDRQ_SlotList.Add(curSendOutJob02.RobotWIP.CurLocation_SlotNo, string.Format("{0}_{1}", curSendOutJob02.CassetteSequenceNo, curSendOutJob02.JobSequenceNo));
                                    }
                                }

                                //Update Status UDRQ Stage Change To UDRQ
                                UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.SEND_OUT_READY, funcName, curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo,
                                                        curSendOutJob02.CassetteSequenceNo, curSendOutJob02.JobSequenceNo);

                                #endregion

                                return;
                            }
                            else
                            {
                                //讀第二片的資料失敗, 仍舊要出第一片
                                #region [ EQP Only SendOut 1 Job ]

                                //Only Get Job01 UDRQ Stage Change To UDRQ
                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) can not get SendOut JobData02, Stage UDRQ Status change from ({3}) to ({4})!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.File.CurStageStatus,
                                                            eRobotStageStatus.SEND_OUT_READY);
                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                //20151024 add for SendOut Job SlotNo
                                lock (curStage)
                                {
                                    if (curStage.curUDRQ_SlotList.ContainsKey(curSendOutJob.RobotWIP.CurLocation_SlotNo) == false)
                                    {
                                        curStage.curUDRQ_SlotList.Add(curSendOutJob.RobotWIP.CurLocation_SlotNo, string.Format("{0}_{1}", curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo));
                                    }
                                }

                                //Update Status UDRQ Stage Change To UDRQ
                                UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.SEND_OUT_READY, funcName, curSendOutJob.CassetteSequenceNo, curSendOutJob.JobSequenceNo, string.Empty, string.Empty);

                                #endregion

                                return;
                            }

                            #endregion

                            #endregion
                        }
                    }
                    else
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) can not get SendOut JobData, Stage UDRQ Status change from ({3}) to ({4})!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.File.CurStageStatus,
                                                    eRobotStageStatus.NO_REQUEST);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //無SendOut Job Info Status UDRQ Stage Change To NOREQ
                        UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, funcName, string.Empty, string.Empty, string.Empty, string.Empty);

                    }

                    #endregion

                }
                else
                {

                    //Monitor 條件不合的狀態 Status UDRQ Stage Change To NOREQ
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) UpstreamInline({5}) SendReady({6}) SendSignal({7}), Stage UDRQ Status can not change to (UDRQ)!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID, (eBitResult)int.Parse(up_UpstreamInline), (eBitResult)int.Parse(up_SendReady), (eBitResult)int.Parse(up_Send));
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    UpdateStage_UDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, funcName, string.Empty, string.Empty, string.Empty, string.Empty);

                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void Get_EqpTypeMuliSlot_LDRQStauts_ForDUAL(Robot curRobot, RobotStage curStage)
        {

            string trxID = string.Empty;
            string strlog = string.Empty;

            try
            {
                string bitOn = "1";
                string bitOff = "0";
                string funcName = MethodBase.GetCurrentMethod().Name;

				#region [Get line fabtyep]
				string fabtype = eFabType.ARRAY.ToString();
				Line line = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);
				if (line != null) {
					fabtype = line.Data.FABTYPE;
				}
				#endregion

                #region [ Check Trx Setting ]

                if (curStage.Data.DOWNSTREAMPATHTRXNAME.Trim() == string.Empty)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find DownStream TrxID setting!",
                                                curStage.Data.NODENO, curStage.Data.ROBOTNAME, curStage.Data.STAGEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    return;

                }

                #endregion

                string[] downStreamTrxList = curStage.Data.DOWNSTREAMPATHTRXNAME.Split(',');
                string strSlotNoBin = string.Empty;
                int slotNo = 0;
                int anotherSlotNo = 0;

                for (int i = 0; i < downStreamTrxList.Length; i++)
                {

                    #region  real time Get Interface downstream

                    trxID = downStreamTrxList[i];

                    Trx downStream_Trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;

                    if (downStream_Trx == null)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3})can not find TrxID({4})!",
                                                    curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    trxID);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        continue;
                    }

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) Get trx by TrxID({4})!",
                                                curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                trxID);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    #endregion

                    #region [拆出PLCAgent Data]

                    #region [ Trx Structure ]

                    //<trx name="L3_DownstreamPath#01" triggercondition="change">
                    //    <eventgroup name="L3_EG_DownstreamPath#01" dir="E2B">
                    //      <event name="L3_B_DownstreamPath#01" trigger="true" />
                    //    </eventgroup>
                    //  </trx>

                    //<event name="L3_B_DownstreamPath#01" devicecode="B" address="0x0000A00" points="32">
                    //  <item name="DownstreamPath#01DownstreamInline" offset="0" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01DownstreamTrouble" offset="1" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ReceiveAble" offset="2" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01Receive" offset="3" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01JobTransfer" offset="4" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ReceiveCancel" offset="5" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ExchangePossible" offset="6" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01DoubleGlass" offset="7" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ReceiveJobReserve" offset="8" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ReceiveOK" offset="9" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01TransferStopRequest" offset="10" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01DummyGlassRequest" offset="11" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01GlassExist" offset="12" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01PinUpComplete" offset="13" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01PinDownRequest" offset="14" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01SlotNumber#01" offset="16" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01SlotNumber#02" offset="17" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01SlotNumber#03" offset="18" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01SlotNumber#04" offset="19" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01SlotNumber#05" offset="20" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01SlotNumber#06" offset="21" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01GlassCount#01" offset="22" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01GlassCount#02" offset="23" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01GlassCount#03" offset="24" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01GlassCount#04" offset="25" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ReceiveType#01" offset="26" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ReceiveType#02" offset="27" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ReceiveType#03" offset="28" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ReceiveType#04" offset="29" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ReceiveType#05" offset="30" points="1" expression="BIT" />
                    //  <item name="DownstreamPath#01ReceiveType#06" offset="31" points="1" expression="BIT" />
                    //</event>

                    #endregion
					#region [variable declare]
					string down_DownstreamInline = "0";
					string down_DownstreamTrouble = "0";
					string down_ReceiveAble = "0";
					string down_Receive = "0";
					string down_JobTransfer = "0";
					string down_ReceiveCancel = "0";
					string down_ExchangePossible = "0";
					string down_DoubleGlass = "0";
					string down_ReceiveJobReserve = "0";
					string down_ReceiveOK = "0";
					string down_TransferStopRequest = "0";
					string down_DummyGlassRequest = "0";
					string down_GlassExist = "0";
					string down_PinUpComplete = "0";
					string down_PinDownRequest = "0";
					string down_SlotNumber01 = "0";
					string down_SlotNumber02 = "0";
					string down_SlotNumber03 = "0";
					string down_SlotNumber04 = "0";
					string down_SlotNumber05 = "0";
					string down_SlotNumber06 = "0";
					string down_GlassCount01 = "0";
					string down_GlassCount02 = "0";
					string down_GlassCount03 = "0";
					string down_GlassCount04 = "0";
					string down_ReceiveType01 = "0";
					string down_ReceiveType02 = "0";
					string down_ReceiveType03 = "0";
					string down_ReceiveType04 = "0";
					string down_ReceiveType05 = "0";
					string down_ReceiveType06 = "0";
					string down_GlassCount05 = "0";
					string down_PreparationPermission = "0";
					string down_InspectionResultUpdate = "0";
					string down_ReturnMode = "0";
					#endregion
					if (fabtype == eFabType.CF.ToString()) {
						#region CF
						down_DownstreamInline = downStream_Trx.EventGroups[0].Events[0].Items[0].Value;
						down_DownstreamTrouble = downStream_Trx.EventGroups[0].Events[0].Items[1].Value;
						down_ReceiveAble = downStream_Trx.EventGroups[0].Events[0].Items[2].Value;
						down_Receive = downStream_Trx.EventGroups[0].Events[0].Items[3].Value;
						down_JobTransfer = downStream_Trx.EventGroups[0].Events[0].Items[4].Value;
						down_ReceiveCancel = downStream_Trx.EventGroups[0].Events[0].Items[5].Value;
						down_ExchangePossible = downStream_Trx.EventGroups[0].Events[0].Items[6].Value;
						down_DoubleGlass = downStream_Trx.EventGroups[0].Events[0].Items[7].Value;
						down_ReceiveJobReserve = downStream_Trx.EventGroups[0].Events[0].Items[8].Value;
						down_TransferStopRequest = downStream_Trx.EventGroups[0].Events[0].Items[9].Value;
						down_DummyGlassRequest = downStream_Trx.EventGroups[0].Events[0].Items[10].Value;
						down_GlassExist = downStream_Trx.EventGroups[0].Events[0].Items[11].Value;
						down_SlotNumber01 = downStream_Trx.EventGroups[0].Events[0].Items[12].Value;
						down_SlotNumber02 = downStream_Trx.EventGroups[0].Events[0].Items[13].Value;
						down_SlotNumber03 = downStream_Trx.EventGroups[0].Events[0].Items[14].Value;
						down_SlotNumber04 = downStream_Trx.EventGroups[0].Events[0].Items[15].Value;
						down_SlotNumber05 = downStream_Trx.EventGroups[0].Events[0].Items[16].Value;
						down_SlotNumber06 = downStream_Trx.EventGroups[0].Events[0].Items[17].Value;
						down_GlassCount01 = downStream_Trx.EventGroups[0].Events[0].Items[18].Value;
						down_GlassCount02 = downStream_Trx.EventGroups[0].Events[0].Items[19].Value;
						down_GlassCount03 = downStream_Trx.EventGroups[0].Events[0].Items[20].Value;
						down_GlassCount04 = downStream_Trx.EventGroups[0].Events[0].Items[21].Value;
						down_GlassCount05 = downStream_Trx.EventGroups[0].Events[0].Items[22].Value;
						down_PreparationPermission = downStream_Trx.EventGroups[0].Events[0].Items[23].Value;
						down_InspectionResultUpdate = downStream_Trx.EventGroups[0].Events[0].Items[24].Value;
						down_ReturnMode = downStream_Trx.EventGroups[0].Events[0].Items[25].Value;

						#endregion
					} else {
						#region default
						down_DownstreamInline = downStream_Trx.EventGroups[0].Events[0].Items[0].Value;
						down_DownstreamTrouble = downStream_Trx.EventGroups[0].Events[0].Items[1].Value;
						down_ReceiveAble = downStream_Trx.EventGroups[0].Events[0].Items[2].Value;
						down_Receive = downStream_Trx.EventGroups[0].Events[0].Items[3].Value;
						down_JobTransfer = downStream_Trx.EventGroups[0].Events[0].Items[4].Value;
						down_ReceiveCancel = downStream_Trx.EventGroups[0].Events[0].Items[5].Value;
						down_ExchangePossible = downStream_Trx.EventGroups[0].Events[0].Items[6].Value;
						down_DoubleGlass = downStream_Trx.EventGroups[0].Events[0].Items[7].Value;
						down_ReceiveJobReserve = downStream_Trx.EventGroups[0].Events[0].Items[8].Value;
						down_ReceiveOK = downStream_Trx.EventGroups[0].Events[0].Items[9].Value;
						down_TransferStopRequest = downStream_Trx.EventGroups[0].Events[0].Items[10].Value;
						down_DummyGlassRequest = downStream_Trx.EventGroups[0].Events[0].Items[11].Value;
						down_GlassExist = downStream_Trx.EventGroups[0].Events[0].Items[12].Value;
						down_PinUpComplete = downStream_Trx.EventGroups[0].Events[0].Items[13].Value;
						down_PinDownRequest = downStream_Trx.EventGroups[0].Events[0].Items[14].Value;
						down_SlotNumber01 = downStream_Trx.EventGroups[0].Events[0].Items[15].Value;
						down_SlotNumber02 = downStream_Trx.EventGroups[0].Events[0].Items[16].Value;
						down_SlotNumber03 = downStream_Trx.EventGroups[0].Events[0].Items[17].Value;
						down_SlotNumber04 = downStream_Trx.EventGroups[0].Events[0].Items[18].Value;
						down_SlotNumber05 = downStream_Trx.EventGroups[0].Events[0].Items[19].Value;
						down_SlotNumber06 = downStream_Trx.EventGroups[0].Events[0].Items[20].Value;
						down_GlassCount01 = downStream_Trx.EventGroups[0].Events[0].Items[21].Value;
						down_GlassCount02 = downStream_Trx.EventGroups[0].Events[0].Items[22].Value;
						down_GlassCount03 = downStream_Trx.EventGroups[0].Events[0].Items[23].Value;
						down_GlassCount04 = downStream_Trx.EventGroups[0].Events[0].Items[24].Value;
						down_ReceiveType01 = downStream_Trx.EventGroups[0].Events[0].Items[25].Value;
						down_ReceiveType02 = downStream_Trx.EventGroups[0].Events[0].Items[26].Value;
						down_ReceiveType03 = downStream_Trx.EventGroups[0].Events[0].Items[27].Value;
						down_ReceiveType04 = downStream_Trx.EventGroups[0].Events[0].Items[28].Value;
						down_ReceiveType05 = downStream_Trx.EventGroups[0].Events[0].Items[29].Value;
						down_ReceiveType06 = downStream_Trx.EventGroups[0].Events[0].Items[30].Value;
						#endregion
					}
                    #endregion

                    //Stage GetGet表示是走ST to RB Mulit Slot Signal Mode
                    if (down_DownstreamInline == bitOn && down_ReceiveAble == bitOn && down_Receive == bitOff)
                    {
                        //LinkSignal Bit符合收片條件 Mulit Slot=> SlotNo= 1st empty SlotNo
                        //注意!!! Array  IMP Stage  SlotNo=1是指上層. 

                        #region [ 將SlotNo Bit 轉成Int ]

                        strSlotNoBin = down_SlotNumber06 + down_SlotNumber05 + down_SlotNumber04 + down_SlotNumber03 + down_SlotNumber02 + down_SlotNumber01;

                        try
                        {
                            slotNo = Convert.ToInt32(strSlotNoBin, 2);
                        }
                        catch (Exception ex1)
                        {
                            slotNo = 0;
                        }

                        #endregion

                        #region 檢查 SlotNo
                        if (slotNo == 0)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TrxID({4}) report SlotNo is 0!",
                                                        curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME, trxID);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            //無法確認則視為無法收片 只須更新Stage LDRQ Status即可
                            UpdateStage_LDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, curStage.File.LDRQ_CstStatusPriority, funcName);

                            break;
                        }
                        if (down_DoubleGlass == bitOn && slotNo % 2 == 0)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TrxID({4}) report Double Glass on but Slot No is not odd!",
                                                        curStage.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME, trxID);
                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion

                            //DoubleGlass On時, SlotNo必須是單數
                            UpdateStage_LDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, curStage.File.LDRQ_CstStatusPriority, funcName);
                            return;
                        }
                        #endregion

                        #region  [ 根據根據SlotNo與DoubleGlass取得Stage EmptySlotNo ]

                        if (down_DoubleGlass != bitOn)
                        {

                            #region only 1 EmptySlot, 更新Current Stage LDRQ Empty Slot

                            lock (curStage)
                            {
                                curStage.CurLDRQ_EmptySlotNo = slotNo.ToString().PadLeft(2, '0');
                                curStage.CurLDRQ_EmptySlotNo02 = string.Empty;

                                //add Empty SlotNo To EmptySlotNoList
                                if (curStage.curLDRQ_EmptySlotList.ContainsKey(slotNo) == false)
                                {
                                    curStage.curLDRQ_EmptySlotList.Add(slotNo, string.Empty);
                                }
                            }

                            #endregion

                        }
                        else
                        {

                            #region [ has 2 EmptySlot, 更新Current Stage LDRQ Empty Slot ]

                            #region [ Get Another SlotNo ]

                            //Mulit Slot Signal Mode for IMP 只有2各Slot
                            //在TCOVN_PL_ITO, DoubleGlass On時SlotNo必為單數下層Slot
                            //anotherSlotNo必為雙數上層Slot
                            anotherSlotNo = slotNo + 1;

                            #endregion

                            lock (curStage)
                            {
                                curStage.CurLDRQ_EmptySlotNo = slotNo.ToString().PadLeft(2, '0');

                                //add Empty SlotNo To EmptySlotNoList
                                if (curStage.curLDRQ_EmptySlotList.ContainsKey(slotNo) == false)
                                {
                                    curStage.curLDRQ_EmptySlotList.Add(slotNo, string.Empty);
                                }

                                curStage.CurLDRQ_EmptySlotNo02 = anotherSlotNo.ToString().PadLeft(2, '0');

                                //add Empty SlotNo To EmptySlotNoList
                                if (curStage.curLDRQ_EmptySlotList.ContainsKey(anotherSlotNo) == false)
                                {
                                    curStage.curLDRQ_EmptySlotList.Add(anotherSlotNo, string.Empty);
                                }
                            }

                            #endregion

                        }

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) DownstreamInline({5}) ReceiveAble({6}) Receive({7}) DoubleGlass({8}) EmptySlotNo01({9})  EmptySlotNo02({10}), Stage LDRQ Status change to (LDRQ).",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    trxID, (eBitResult)int.Parse(down_DownstreamInline), (eBitResult)int.Parse(down_ReceiveAble), (eBitResult)int.Parse(down_Receive),
                                                    (eBitResult)int.Parse(down_DoubleGlass), curStage.CurLDRQ_EmptySlotNo, curStage.CurLDRQ_EmptySlotNo02);
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }


                        #endregion

                        //只須更新Stage LDRQ Status即可
                        UpdateStage_LDRQ_Status(curStage, eRobotStageStatus.RECEIVE_READY, curStage.File.LDRQ_CstStatusPriority, funcName);

                        #endregion

                    }
                    else
                    {
                        //Monitor 條件不合的狀態 Status LDRQ Stage Change To NOREQ
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) DownstreamInline({5}) ReceiveAble({6}) Receive({7}), Stage LDRQ Status can not change to (LDRQ)!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    trxID, (eBitResult)int.Parse(down_DownstreamInline), (eBitResult)int.Parse(down_ReceiveAble), (eBitResult)int.Parse(down_Receive));
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        //只須更新Stage LDRQ Status即可
                        UpdateStage_LDRQ_Status(curStage, eRobotStageStatus.NO_REQUEST, curStage.File.LDRQ_CstStatusPriority, funcName);

                    }

                    #region [ 20151215 add 如果Transfer Stop Request On 則要更新Stage狀態 ]

                    if (down_TransferStopRequest == bitOn)
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) TransferStopRequest({5}),Update RobotStage Transfer Stop Request Flag(True)!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID, curStage.Data.STAGENAME,
                                                    trxID, eBitResult.ON.ToString());
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        lock (curStage.File)
                        {
                            curStage.File.DownStreamTransferStopRequestFlag = true;
                        }

                    }
                    else
                    {

                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) StageID({2}) StageName({3}) TRXID=({4}) TransferStopRequest({5}),Update RobotStage Transfer Stop Request Flag(False)!",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curStage.Data.STAGEID,
                                                    curStage.Data.STAGENAME, trxID, eBitResult.OFF.ToString());
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion

                        lock (curStage.File)
                        {
                            curStage.File.DownStreamTransferStopRequestFlag = false;
                        }

                    }

                    #endregion

                }

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
    }
}
