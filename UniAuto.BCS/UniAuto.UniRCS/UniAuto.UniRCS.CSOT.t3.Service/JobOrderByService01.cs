using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniRCS.Core;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using System.Reflection;

namespace UniAuto.UniRCS.CSOT.t3.Service
{
    /// <summary>
    /// Port的UnloadDispatchRule與Job Grade相符者, 排優先
    /// </summary>
    public class JobGradePortUnloadDispatchingRule : IComparer<RobotStage>
    {
        private Job curBcsJob = null;
        private Dictionary<string, clsDispatchRule> lineUnloaderDispatchRule = new Dictionary<string, clsDispatchRule>();
        public JobGradePortUnloadDispatchingRule(Line line, Job curBcsJob)
        {
            lock (line.File.UnlaoderDispatchRule)
            {
                foreach (string port_id in line.File.UnlaoderDispatchRule.Keys)
                {
                    lineUnloaderDispatchRule.Add(port_id, line.File.UnlaoderDispatchRule[port_id]);
                }
            }
            this.curBcsJob = curBcsJob;
        }
        public int Compare(RobotStage x, RobotStage y)
        {
            // x and y are equal, return 0;
            // x is greater, return 1;
            // y is greater, return -1;
            if (x.Data.STAGETYPE == eRobotStageType.PORT && y.Data.STAGETYPE == eRobotStageType.PORT)
            {
                int i = string.Compare(x.UnloaderPortMatchSetGradePriority, y.UnloaderPortMatchSetGradePriority);
                return i;//UnloaderPortMatchSetGradePriority越小越優先
            }
            else
            {
                if (x.Data.STAGETYPE == eRobotStageType.PORT) return 1;
                else if (y.Data.STAGETYPE == eRobotStageType.PORT) return -1;
                return 0;
            }
        }
    }

    /// <summary>
    /// 根據JobJudge及PortMode排序
    /// </summary>
    public class UnloaderPortModeOrderByComparer : IComparer<Tuple<RobotStage, Port, Cassette>>
    {
        private Job curBcsJob = null;
        public UnloaderPortModeOrderByComparer(Job curBcsJob)
        {
            this.curBcsJob = curBcsJob;
        }
        public int Compare(Tuple<RobotStage, Port, Cassette> x, Tuple<RobotStage, Port, Cassette> y)
        {
            // x and y are equal, return 0;
            // x is greater, return 1;
            // y is greater, return -1;

            // PortMode與JobJudge相同, 則優先; PortMode MIX排第二; PortMode EMP排第三; 其餘PortMode不分先後
            if (x.Item2.File.Mode == y.Item2.File.Mode)
                return 0;//PortMode相同, 不分先後

            switch (curBcsJob.RobotWIP.CurSendOutJobJudge)
            {
            case "1"://1：OK
                {
                    if (x.Item2.File.Mode == ePortMode.OK) return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=OK優先, 因此return -1讓 Y排後面
                    if (y.Item2.File.Mode == ePortMode.OK) return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=OK優先, 因此return 1讓 X排後面
                    if (x.Item2.File.Mode == ePortMode.MIX) return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=MIX排第二, 因此return -1讓 Y排後面
                    if (y.Item2.File.Mode == ePortMode.MIX) return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=MIX排第二, 因此return 1讓 X排後面
                    if (x.Item2.File.Mode == ePortMode.EMPMode) return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=EMP排第三, 因此return -1讓 Y排後面
                    if (y.Item2.File.Mode == ePortMode.EMPMode) return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=EMP排第三, 因此return 1讓 X排後面
                }
                break;
            case "2"://2：NG
                {
                    if (x.Item2.File.Mode == ePortMode.NG) return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=NG優先, 因此return -1讓 Y排後面
                    if (y.Item2.File.Mode == ePortMode.NG) return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=NG優先, 因此return 1讓 X排後面
                    if (x.Item2.File.Mode == ePortMode.MIX) return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=MIX排第二, 因此return -1讓 Y排後面
                    if (y.Item2.File.Mode == ePortMode.MIX) return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=MIX排第二, 因此return 1讓 X排後面
                    if (x.Item2.File.Mode == ePortMode.EMPMode) return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=EMP排第三, 因此return -1讓 Y排後面
                    if (y.Item2.File.Mode == ePortMode.EMPMode) return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=EMP排第三, 因此return 1讓 X排後面
                }
                break;
            case "3"://3：RW
                {
                    if (x.Item2.File.Mode == ePortMode.Rework) return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=RW優先, 因此return -1讓 Y排後面
                    if (y.Item2.File.Mode == ePortMode.Rework) return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=RW優先, 因此return 1讓 X排後面
                    if (x.Item2.File.Mode == ePortMode.MIX) return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=MIX排第二, 因此return -1讓 Y排後面
                    if (y.Item2.File.Mode == ePortMode.MIX) return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=MIX排第二, 因此return 1讓 X排後面
                    if (x.Item2.File.Mode == ePortMode.NG) return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=NG排第三, 因此return -1讓 Y排後面
                    if (y.Item2.File.Mode == ePortMode.NG) return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=NG排第三, 因此return 1讓 X排後面
                    if (x.Item2.File.Mode == ePortMode.EMPMode) return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=EMP排第四, 因此return -1讓 Y排後面
                    if (y.Item2.File.Mode == ePortMode.EMPMode) return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=EMP排第四, 因此return 1讓 X排後面
                }
                break;
            case "4"://4：PD
                {
                    if (x.Item2.File.Mode == ePortMode.PD) return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=PD優先, 因此return -1讓 Y排後面
                    if (y.Item2.File.Mode == ePortMode.PD) return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=PD優先, 因此return 1讓 X排後面
                    if (x.Item2.File.Mode == ePortMode.MIX) return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=MIX排第二, 因此return -1讓 Y排後面
                    if (y.Item2.File.Mode == ePortMode.MIX) return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=MIX排第二, 因此return 1讓 X排後面
                    if (x.Item2.File.Mode == ePortMode.EMPMode) return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=EMP排第三, 因此return -1讓 Y排後面
                    if (y.Item2.File.Mode == ePortMode.EMPMode) return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=EMP排第三, 因此return 1讓 X排後面
                }
                break;
            case "5"://5：RP
                {
                    if (x.Item2.File.Mode == ePortMode.RP) return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=RP優先, 因此return -1讓 Y排後面
                    if (y.Item2.File.Mode == ePortMode.RP) return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=RP優先, 因此return 1讓 X排後面
                    if (x.Item2.File.Mode == ePortMode.MIX) return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=MIX排第二, 因此return -1讓 Y排後面
                    if (y.Item2.File.Mode == ePortMode.MIX) return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=MIX排第二, 因此return 1讓 X排後面
                    if (x.Item2.File.Mode == ePortMode.EMPMode) return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=EMP排第三, 因此return -1讓 Y排後面
                    if (y.Item2.File.Mode == ePortMode.EMPMode) return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=EMP排第三, 因此return 1讓 X排後面
                }
                break;
            case "6"://6：IR
                {
                    if (x.Item2.File.Mode == ePortMode.IR) return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=IR優先, 因此return -1讓 Y排後面
                    if (y.Item2.File.Mode == ePortMode.IR) return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=IR優先, 因此return 1讓 X排後面
                    if (x.Item2.File.Mode == ePortMode.MIX) return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=MIX排第二, 因此return -1讓 Y排後面
                    if (y.Item2.File.Mode == ePortMode.MIX) return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=MIX排第二, 因此return 1讓 X排後面
                    if (x.Item2.File.Mode == ePortMode.EMPMode) return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=EMP排第三, 因此return -1讓 Y排後面
                    if (y.Item2.File.Mode == ePortMode.EMPMode) return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=EMP排第三, 因此return 1讓 X排後面
                }
                break;
            default://7：Other, 8：RV
                {
                    if (x.Item2.File.Mode == ePortMode.MIX) return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=MIX排第二, 因此return -1讓 Y排後面
                    if (y.Item2.File.Mode == ePortMode.MIX) return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=MIX排第二, 因此return 1讓 X排後面
                    if (x.Item2.File.Mode == ePortMode.EMPMode) return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=EMP排第三, 因此return -1讓 Y排後面
                    if (y.Item2.File.Mode == ePortMode.EMPMode) return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=EMP排第三, 因此return 1讓 X排後面
                }
                break;
            }
            return 0;//其餘PortMode不分先後
        }
    }

    /// <summary>
    /// Port的Slot DCRandSorterFlag與Job DCRandSorterFlag相符者, 排優先
    /// </summary>
    public class OrderByJobDCRandSorterFlag : IComparer<RobotStage>
    {
        private string curBcsJobEqpFlag_DCRandSorterFlag = string.Empty;

        public OrderByJobDCRandSorterFlag(string curBcsJobEqpFlag_DCRandSorterFlag)
        {
            this.curBcsJobEqpFlag_DCRandSorterFlag = curBcsJobEqpFlag_DCRandSorterFlag;
        }
        public int Compare(RobotStage x, RobotStage y)
        {
            // x and y are equal, return 0;
            // x is greater, return 1;
            // y is greater, return -1;
            if (x.Data.STAGETYPE == eRobotStageType.PORT && y.Data.STAGETYPE == eRobotStageType.PORT)
            {
                //0:No Flag Glass,1:DCR Flag Glass,2:Sorter Flag Glass,3:DCR and Sorter Flag Glass
                bool x_match = false, y_match = false;
                if (((curBcsJobEqpFlag_DCRandSorterFlag == "2" || curBcsJobEqpFlag_DCRandSorterFlag == "3") && (x.UnloaderPortSlotJobDCRandSorterFlag == "2" || x.UnloaderPortSlotJobDCRandSorterFlag == "3")) ||
                    ((curBcsJobEqpFlag_DCRandSorterFlag == "0" || curBcsJobEqpFlag_DCRandSorterFlag == "1") && (x.UnloaderPortSlotJobDCRandSorterFlag == "0" || x.UnloaderPortSlotJobDCRandSorterFlag == "1")))
                {
                    x_match = true;
                }
                if (((curBcsJobEqpFlag_DCRandSorterFlag == "2" || curBcsJobEqpFlag_DCRandSorterFlag == "3") && (y.UnloaderPortSlotJobDCRandSorterFlag == "2" || y.UnloaderPortSlotJobDCRandSorterFlag == "3")) ||
                    ((curBcsJobEqpFlag_DCRandSorterFlag == "0" || curBcsJobEqpFlag_DCRandSorterFlag == "1") && (y.UnloaderPortSlotJobDCRandSorterFlag == "0" || y.UnloaderPortSlotJobDCRandSorterFlag == "1")))
                {
                    y_match = true;
                }
                if (x_match && y_match) return 0;
                else if (x_match) return -1;//使用升冪排序, x match則return -1讓y排在後面
                else if (y_match) return 1;//使用升冪排序, y match則return 1讓x排在後面
                else
                {
                    // x,y 皆 mismatch 則空 Cassette 排前面
                    if (x.UnloaderPortSlotJobDCRandSorterFlag == string.Empty && y.UnloaderPortSlotJobDCRandSorterFlag == string.Empty) return 0;
                    else if (x.UnloaderPortSlotJobDCRandSorterFlag == string.Empty) return -1;//使用升冪排序, x match則return -1讓y排在後面
                    else if (y.UnloaderPortSlotJobDCRandSorterFlag == string.Empty) return 1;//使用升冪排序, y match則return 1讓x排在後面
                    else return 0;
                }
            }
            else
            {
                if (x.Data.STAGETYPE == eRobotStageType.PORT) return 1;
                else if (y.Data.STAGETYPE == eRobotStageType.PORT) return -1;
                return 0;
            }
        }
    }

    /// <summary>
    /// 依Cassette Start Time排序
    /// </summary>
    public class OrderByCassetteStartTime : IComparer<RobotStage>
    {
        public OrderByCassetteStartTime()
        {
        }
        public int Compare(RobotStage x, RobotStage y)
        {
            // x and y are equal, return 0;
            // x is greater, return 1;
            // y is greater, return -1;
            if (x.CassetteStartTime > y.CassetteStartTime) return 0;
            else if (x.CassetteStartTime < y.CassetteStartTime) return -1;
            return 0;
        }
    }

    public partial class JobOrderByService
    {
        /// <summary>
        /// <br>CF coolrun mode</br>
        /// <br>job is from both port,return to source port cst slot</br>
        /// <br>job is from loading port,return to unloading port cst</br>
        /// </summary>
        /// <param name="robotContext">robot context object</param>
        /// <returns>true=no error</returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("OR0006")]
        public bool OrderBy_CoolRunUnloadDispatch(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;

                List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];
                List<RobotStage> stage_list = beforeOrderByStageList == null ? new List<RobotStage>() : beforeOrderByStageList.ToList();


                #region Job curBcsJob
                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];
                if (curBcsJob == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find curBcsJob Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region DefineNormalRobotCmd curDefineCmd
                DefineNormalRobotCmd curDefineCmd = (DefineNormalRobotCmd)robotConText[eRobotContextParameter.DefineNormalRobotCmd];

                //找不到任何的Stage List時回NG
                if (curDefineCmd == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find DefineNormalRobotCmd!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find DefineNormalRobotCmd Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetDefineNormalRobotCmd);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                RobotStage target_stage = null;
                int target_slot_no = 0;
                int from_slot_no = int.Parse(curBcsJob.FromSlotNo);

                #region * Decide target_stage,target_slot_no
                //判斷來源cst,若無，則要去unloading port
                #region Cassette source_cassette
                Cassette source_cassette = ObjectManager.CassetteManager.GetCassette(curBcsJob.FromCstID);
                if (source_cassette == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob Source Cassette({2})!",
                                                "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.FromCstID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion
                    goto jumpULD;
                    //找不到原cst，去unloading port
                }
                #endregion
                #region Port source_port
                Port source_port = ObjectManager.PortManager.GetPort(source_cassette.PortID);
                if (source_port == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob Source Port({2})!",
                                                "L2", MethodBase.GetCurrentMethod().Name, source_cassette.PortID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find curBcsJob Source Port({1})!",
                                            MethodBase.GetCurrentMethod().Name, source_cassette.PortID);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion
                #region Stage source_stage
                //檢查若來源是both port，回原cst原slot
                if (source_port.File.Type == ePortType.BothPort)
                {
                    RobotStage source_stage = ObjectManager.RobotStageManager.GetRobotStagebyPortNo(source_port.Data.NODENO, source_port.Data.PORTNO);
                    //找原cst原slot
                    if (source_stage != null)
                    {
                        if (source_stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST)
                        {
                            //check source slot是否為空
                            if (source_stage.PortSlotInfos[from_slot_no - 1].slotGlassExist == "0")
                            {
                                target_stage = source_stage;
                                target_slot_no = from_slot_no;
                                goto jumpRet; //回原cst原slot
                            }
                            else
                            {
                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] curBcsJob Return Port({2}) Slot({3}) is not Empty",
                                                            "L2", MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                }

                                #endregion
                                //原slot不為空，回不去原cst原slot，則要去unloading port
                                goto jumpULD;
                            }
                        }
                    }
                }
                #endregion

            jumpULD:
                #region 找出UnloadingPort
                List<Tuple<RobotStage, Port, Cassette>> unloading_ports = new List<Tuple<RobotStage, Port, Cassette>>();
                foreach (RobotStage stage in stage_list)
                {
                    Port port = ObjectManager.PortManager.GetPort(stage.Data.STAGEID);
                    Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID);
                    if (port.File.Type == ePortType.UnloadingPort && stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST && cst != null)
                    {
                        unloading_ports.Add(Tuple.Create(stage, port, cst));
                    }
                }
                unloading_ports = unloading_ports.OrderBy(p => p.Item3.LoadTime).ToList();
                #endregion
                #region 尋找是否UnloadingPort上已經有相同FromCst的Job, 且相同Slot必須為空
                if (target_stage == null)
                {
                    foreach (var unloading_port in unloading_ports)
                    {
                        bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotCSTSeq == curBcsJob.CassetteSequenceNo);
                        //check有相同FromCst的Job且相同Slot必須為空
                        if (found)
                        {
                            foreach (int curSlotNo in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                            {
                                if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[curSlotNo]))
                                {
                                    unloading_port.Item1.curLDRQ_EmptySlotList[curSlotNo] = curBcsJob.JobKey;
                                    target_stage = unloading_port.Item1;
                                    target_slot_no = curSlotNo;
                                    break;
                                }
                            }
                        }
                        if (target_stage != null) break;
                    }
                }
                #endregion
                #region UnloadingPort上沒有相同FromCst的Job, 尋找UnloadingPort是否有空Cst
                if (target_stage == null)
                {
                    foreach (var unloading_port in unloading_ports)
                    {
                        bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotGlassExist == "1");
                        if (!found)
                        {
                            foreach (int curSlotNo in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                            {
                                if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[curSlotNo]))
                                {
                                    unloading_port.Item1.curLDRQ_EmptySlotList[curSlotNo] = curBcsJob.JobKey;
                                    target_stage = unloading_port.Item1;
                                    target_slot_no = curSlotNo;
                                    break;
                                }
                            }
                        }
                        if (target_stage != null) break;
                    }
                }
                #endregion
                #region UnloadingPort上沒有空Cst, 則找有空Slot的UnloadingPort
                if (target_stage == null)
                {
                    foreach (var unloading_port in unloading_ports)
                    {
                        foreach (int curSlotNo in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                        {
                            if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[curSlotNo]))
                            {
                                unloading_port.Item1.curLDRQ_EmptySlotList[curSlotNo] = curBcsJob.JobKey;
                                target_stage = unloading_port.Item1;
                                target_slot_no = curSlotNo;
                                break;
                            }
                        }
                        if (target_stage != null) break;
                    }
                }
                #endregion
                #endregion
            jumpRet:
                if (target_stage != null)
                {
                    curDefineCmd.Cmd01_TargetPosition = int.Parse(target_stage.Data.STAGEID);
                    curDefineCmd.Cmd01_TargetSlotNo = target_slot_no;
                }
                else
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) can not find target stage!(please check 1.Both port=>Source CST slot is not empty 2.Unloading port=>can't find any cst)",
                                                "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not find target stage!(please check 1.Both port=>Source CST slot is not empty 2.Unloading port=>can't find any cst)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_UnloadDispatch_No_TargetStage);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }


        /// <summary>
        /// <br>CF through mode (CFREP/CFMAC/CFAOI)</br>
        /// <br>job is ok judge,default return to source port cst slot</br>
        /// <br>job is ok judge,source port cst not found,return to mix unloading port cst</br>
        /// <br>job is ng judge,is from both port,default return to source port cst slot</br>
        /// <br>job is ng judge,is from both port,source port cst not found,return to mix unloading port cst</br>
        /// <br>job is ng judge,is from loading port,return to mix unloading port cst</br>
        /// </summary>
        /// <param name="robotContext">robot context object</param>
        /// <returns>true=no error</returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("OR0007")]
        public bool OrderBy_ThroughUnloadDispatch(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;

                List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];
                List<RobotStage> stage_list = beforeOrderByStageList == null ? new List<RobotStage>() : beforeOrderByStageList.ToList();

                #region [ Get curRobot Entity ]

                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

                //找不到 Robot 回NG
                if (curRobot == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region Job curBcsJob
                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];
                if (curBcsJob == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find curBcsJob Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region DefineNormalRobotCmd curDefineCmd
                DefineNormalRobotCmd curDefineCmd = (DefineNormalRobotCmd)robotConText[eRobotContextParameter.DefineNormalRobotCmd];

                //找不到任何的Stage List時回NG
                if (curDefineCmd == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find DefineNormalRobotCmd!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find DefineNormalRobotCmd Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetDefineNormalRobotCmd);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region Get L2 Equipment
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);
                if (eqp == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot Node No!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot Node No!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_EQP_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                RobotStage target_stage = null;
                int target_slot_no = 0;
                int from_slot_no = int.Parse(curBcsJob.FromSlotNo);
                Cassette source_cassette = null;
                Port source_port = null;

                #region * Decide target_stage,target_slot_no
                if (curBcsJob.RobotWIP.CurSendOutJobJudge == "1")
                {
                    //OK judge回原cst原slot,若回不去原cst原slot，則要去unloading port
                    #region Cassette source_cassette
                    source_cassette = ObjectManager.CassetteManager.GetCassette(curBcsJob.FromCstID);
                    if (source_cassette == null)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find OK judge curBcsJob Source Cassette({2})!",
                                                    "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.FromCstID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion
                        goto jumpULD;
                        //找不到原cst，回不去原cst原slot，則要去unloading port
                    }
                    #endregion
                    #region Port source_port
                    source_port = ObjectManager.PortManager.GetPort(source_cassette.PortID);
                    if (source_port == null)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find OK judge curBcsJob Source Port({2})!",
                                                    "L2", MethodBase.GetCurrentMethod().Name, source_cassette.PortID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion

                        errMsg = string.Format("[{0}] can not find curBcsJob Source Port({1})!",
                                                MethodBase.GetCurrentMethod().Name, source_cassette.PortID);

                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                    #endregion
                    #region Decide target_stage,target_slot_no
                    RobotStage source_stage = ObjectManager.RobotStageManager.GetRobotStagebyPortNo(source_port.Data.NODENO, source_port.Data.PORTNO);
                    //找原cst原slot
                    if (source_stage != null)
                    {
                        if (source_stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST)
                        {
                            //check source slot是否為空
                            if (source_stage.PortSlotInfos[from_slot_no - 1].slotGlassExist == "0")
                            {
                                target_stage = source_stage;
                                target_slot_no = from_slot_no;
                                goto jumpRet; //回原cst原slot
                            }
                            else
                            {
                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] OK judge curBcsJob Return Port({2}) Slot({3}) is not Empty",
                                                            "L2", MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                }

                                #endregion
                                //原slot不為空，回不去原cst原slot，則要去unloading port
                                goto jumpULD;
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    //NG judge如果來自both port，則回原cst原slot,若回不去原cst原slot，則要去unloading port
                    #region Cassette source_cassette
                    source_cassette = ObjectManager.CassetteManager.GetCassette(curBcsJob.FromCstID);
                    if (source_cassette == null)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find OK judge curBcsJob Source Cassette({2})!",
                                                    "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.FromCstID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion
                        goto jumpULD;
                        //找不到原cst，回不去原cst原slot，則要去unloading port
                    }
                    #endregion
                    #region Port source_port
                    source_port = ObjectManager.PortManager.GetPort(source_cassette.PortID);
                    if (source_port == null)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find OK judge curBcsJob Source Port({2})!",
                                                    "L2", MethodBase.GetCurrentMethod().Name, source_cassette.PortID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion

                        errMsg = string.Format("[{0}] can not find curBcsJob Source Port({1})!",
                                                MethodBase.GetCurrentMethod().Name, source_cassette.PortID);

                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                    #endregion
                    if (source_port.File.Type == ePortType.BothPort)
                    {
                        #region Decide target_stage,target_slot_no
                        RobotStage source_stage = ObjectManager.RobotStageManager.GetRobotStagebyPortNo(source_port.Data.NODENO, source_port.Data.PORTNO);
                        //找原cst原slot
                        if (source_stage != null)
                        {
                            if (source_stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST)
                            {
                                //check source slot是否為空
                                if (source_stage.PortSlotInfos[from_slot_no - 1].slotGlassExist == "0")
                                {
                                    target_stage = source_stage;
                                    target_slot_no = from_slot_no;
                                    goto jumpRet; //回原cst原slot
                                }
                                else
                                {
                                    #region  [DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] curBcsJob Return Port({2}) Slot({3}) is not Empty",
                                                                "L2", MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    }

                                    #endregion
                                    //原slot不為空，回不去原cst原slot，則要去unloading port
                                    goto jumpULD;
                                }
                            }
                        }
                        #endregion
                    }
                }
                #endregion
            jumpULD:
                bool buffering = false;
                //其他，去mix unloading port
                #region 找出JobJudge與PortMode, UnloadingPortSetting相符的UnloadingPort
                List<Tuple<RobotStage, Port, Cassette>> unloading_ports = new List<Tuple<RobotStage, Port, Cassette>>();
                foreach (RobotStage stage in stage_list)
                {
                    Port port = ObjectManager.PortManager.GetPort(stage.Data.STAGEID);
                    Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID);
                    if (port.File.Type == ePortType.UnloadingPort && stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST && cst != null)
                    {
                        bool match = ForCF_CheckJobJudge_PortMode_UnloadingPortSetting(curBcsJob, port);
                        if (match)
                        {
                            unloading_ports.Add(Tuple.Create(stage, port, cst));
                        }
                    }
                }
                unloading_ports = unloading_ports.OrderBy(p => p, new UnloaderPortModeOrderByComparer(curBcsJob)).ToList();
                #endregion
                #region 尋找是否UnloadingPort上已經有相同FromCst的Job,找空Slot
                if (target_stage == null)
                {
                    foreach (var unloading_port in unloading_ports)
                    {
                        bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotCSTSeq == curBcsJob.CassetteSequenceNo);
                        //找到有相同FromCst的Job且相同Slot必須為空
                        if (found)
                        {
                            foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                            {
                                if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                {
                                    unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                    target_stage = unloading_port.Item1;
                                    target_slot_no = slot_no;
                                    break;
                                }
                            }
                        }
                        if (target_stage != null) break;
                    }
                }
                #endregion
                #region UnloadingPort上沒有相同FromCst的Job,找空CST
                if (target_stage == null)
                {
                    foreach (var unloading_port in unloading_ports)
                    {
                        bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotGlassExist == "1");
                        if (!found)
                        {
                            foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                            {
                                if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                {
                                    unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                    target_stage = unloading_port.Item1;
                                    target_slot_no = slot_no;
                                    break;
                                }
                            }
                        }
                        if (target_stage != null) break;
                    }
                }
                #endregion
                #region UnloadingPort上沒有空CST,找空Slot
                if (target_stage == null)
                {
                    foreach (var unloading_port in unloading_ports)
                    {
                        bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotGlassExist == "0");
                        if (found)
                        {
                            foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                            {
                                if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                {
                                    unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                    target_stage = unloading_port.Item1;
                                    target_slot_no = slot_no;
                                    break;
                                }
                            }
                        }
                        if (target_stage != null) break;
                    }
                }
                #endregion
                if (target_stage == null)
                {
                    if (Workbench.LineType != eLineType.CF.FCREW_TYPE1)
                    {
                        buffering = true;
                    }
                }
                //如果不能去unloading port,則要回原cst原slot先buffering
                if (buffering)
                {
                    #region job is return to source cst & slot,need check job is already in cst
                    if (source_port != null)
                    {
                        RobotStage source_stage = ObjectManager.RobotStageManager.GetRobotStagebyPortNo(source_port.Data.NODENO, source_port.Data.PORTNO);
                        //先找出source_port的stage
                        if (source_stage != null)
                        {
                            if (source_stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST)
                            {
                                //從stage找slot是否為空
                                if (source_stage.PortSlotInfos[from_slot_no - 1].slotGlassExist == "0")
                                {
                                    target_stage = source_stage;
                                    target_slot_no = from_slot_no;

                                    //update job rcsbufferingflag=1	
                                    lock (curBcsJob)
                                    {
                                        curBcsJob.CfSpecial.RCSBufferingFlag = "1";
                                    }
                                    ObjectManager.JobManager.EnqueueSave(curBcsJob);
                                }
                                else
                                {
                                    #region check job is already in cst
                                    if (source_stage.PortSlotInfos[from_slot_no - 1].slotCSTSeq == curBcsJob.CassetteSequenceNo &&
                                        source_stage.PortSlotInfos[from_slot_no - 1].slotJobSeq == curBcsJob.JobSequenceNo)
                                    {

                                        #region  [DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] curBcsJob do Loading_Buffer but job is already in cst,Return Port({1}) Slot({2})",
                                                                    "L2", MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] curBcsJob do Loading_Buffer but job is already in cst,Return Port({1}) Slot({2})",
                                                                MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OriginalCassetteOriginalSlotIsNotEmpty);
                                        robotConText.SetReturnMessage(errMsg);
                                        return false;

                                    }
                                    else
                                    {
                                        #region  [DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] curBcsJob do Loading_Buffer,Return Port({2}) Slot({3}) is not Empty",
                                                                    "L2", MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] curBcsJob do Loading_Buffer,Return Port({1}) Slot({2}) is not Empty",
                                                                MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OriginalCassetteOriginalSlotIsNotEmpty);
                                        robotConText.SetReturnMessage(errMsg);
                                        return false;
                                    }
                                    #endregion
                                }
                            }
                        }
                    }
                    #endregion
                }
            jumpRet:
                if (target_stage != null)
                {
                    curDefineCmd.Cmd01_TargetPosition = int.Parse(target_stage.Data.STAGEID);
                    curDefineCmd.Cmd01_TargetSlotNo = target_slot_no;
                }
                else
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) can not find target stage!(please check 1.EQP.File.ProductTypeCheckMode is Disable=>Job is OK Judge in source Loading/Both port,Job is Other Judge go to port mode:other/EMP/MIX unloading port 2.EQP.File.ProductTypeCheckMode is Enable=>port.File.ProductType is 0 or port.File.ProductType == Job.ProductType,Job is OK Judge in source Loading/Both port,Job is Other Judge go to port mode:other/EMP/MIX unloading port)",
                                                "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not find target stage!(please check 1.EQP.File.ProductTypeCheckMode is Disable=>Job is OK Judge in source Loading/Both port,Job is Other Judge go to port mode:other/EMP/MIX unloading port 2.EQP.File.ProductTypeCheckMode is Enable=>port.File.ProductType is 0 or port.File.ProductType == Job.ProductType,Job is OK Judge in source Loading/Both port,Job is Other Judge go to port mode:other/EMP/MIX unloading port)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_UnloadDispatch_No_TargetStage);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }


        /// <summary>
        /// <br>CF fix mode(CFREP/CFMAC),with buffering function</br>
        /// <br>job is ok judge,default return to ok unloading port cst</br>
        /// <br>job is ok judge,ok unloading port cst not found,buffering to source port cst slot</br>
        /// <br>job is ng judge,default return to ng unloading port cst</br>
        /// <br>job is ng judge,ng unloading port cst not found,buffering to source port cst slot</br>
        /// </summary>
        /// <param name="robotContext">robot context object</param>
        /// <returns>true=no error</returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("OR0008")]
        public bool OrderBy_FixUnloadDispatch(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;

                List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];
                List<RobotStage> stage_list = beforeOrderByStageList == null ? new List<RobotStage>() : beforeOrderByStageList.ToList();

                #region [ Get curRobot Entity ]

                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

                //找不到 Robot 回NG
                if (curRobot == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region Job curBcsJob
                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];
                if (curBcsJob == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find curBcsJob Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region DefineNormalRobotCmd curDefineCmd
                DefineNormalRobotCmd curDefineCmd = (DefineNormalRobotCmd)robotConText[eRobotContextParameter.DefineNormalRobotCmd];

                //找不到任何的Stage List時回NG
                if (curDefineCmd == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find DefineNormalRobotCmd!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find DefineNormalRobotCmd Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetDefineNormalRobotCmd);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region Get L2 Equipment
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);
                if (eqp == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot Node No!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot Node No!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_EQP_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                RobotStage target_stage = null;
                int target_slot_no = 0;
                int from_slot_no = int.Parse(curBcsJob.FromSlotNo);

                #region * Decide target_stage,target_slot_no
                bool buffering = false;
                if (curBcsJob.RobotWIP.CurSendOutJobJudge == "1")
                {
                    #region 找出JobJudge與PortMode, UnloadingPortSetting相符的UnloadingPort
                    List<Tuple<RobotStage, Port, Cassette>> unloading_ports = new List<Tuple<RobotStage, Port, Cassette>>();
                    foreach (RobotStage stage in stage_list)
                    {
                        Port port = ObjectManager.PortManager.GetPort(stage.Data.STAGEID);
                        Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID);
                        if (port.File.Type == ePortType.UnloadingPort && stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST && cst != null)
                        {
                            bool match = ForCF_CheckJobJudge_PortMode_UnloadingPortSetting(curBcsJob, port);
                            if (match)
                            {
                                unloading_ports.Add(Tuple.Create(stage, port, cst));
                            }
                        }
                    }
                    unloading_ports = unloading_ports.OrderBy(p => p, new UnloaderPortModeOrderByComparer(curBcsJob)).ToList();
                    #endregion
                    #region 尋找是否UnloadingPort上已經有相同FromCst的Job,找空Slot
                    if (target_stage == null)
                    {
                        foreach (var unloading_port in unloading_ports)
                        {
                            bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotCSTSeq == curBcsJob.CassetteSequenceNo);
                            //找到有相同FromCst的Job且相同Slot必須為空
                            if (found)
                            {
                                foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                                {
                                    if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                    {
                                        unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                        target_stage = unloading_port.Item1;
                                        target_slot_no = slot_no;
                                        break;
                                    }
                                }
                            }
                            if (target_stage != null) break;
                        }
                    }
                    #endregion
                    #region UnloadingPort上沒有相同FromCst的Job,找空CST
                    if (target_stage == null)
                    {
                        foreach (var unloading_port in unloading_ports)
                        {
                            bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotGlassExist == "1");
                            if (!found)
                            {
                                foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                                {
                                    if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                    {
                                        unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                        target_stage = unloading_port.Item1;
                                        target_slot_no = slot_no;
                                        break;
                                    }
                                }
                            }
                            if (target_stage != null) break;
                        }
                    }
                    #endregion
                    #region UnloadingPort上沒有空CST,找空Slot
                    if (target_stage == null)
                    {
                        foreach (var unloading_port in unloading_ports)
                        {
                            bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotGlassExist == "0");
                            if (found)
                            {
                                foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                                {
                                    if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                    {
                                        unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                        target_stage = unloading_port.Item1;
                                        target_slot_no = slot_no;
                                        break;
                                    }
                                }
                            }
                            if (target_stage != null) break;
                        }
                    }
                    #endregion
                    if (target_stage == null)
                    {
                        buffering = true;
                    }
                }
                else
                {
                    #region 找出JobJudge與PortMode, UnloadingPortSetting相符的UnloadingPort
                    List<Tuple<RobotStage, Port, Cassette>> unloading_ports = new List<Tuple<RobotStage, Port, Cassette>>();
                    foreach (RobotStage stage in stage_list)
                    {
                        Port port = ObjectManager.PortManager.GetPort(stage.Data.STAGEID);
                        Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID);
                        if (port.File.Type == ePortType.UnloadingPort && stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST && cst != null)
                        {
                            bool match = ForCF_CheckJobJudge_PortMode_UnloadingPortSetting(curBcsJob, port);
                            if (match)
                            {
                                unloading_ports.Add(Tuple.Create(stage, port, cst));
                            }
                        }
                    }
                    unloading_ports = unloading_ports.OrderBy(p => p, new UnloaderPortModeOrderByComparer(curBcsJob)).ToList();
                    #endregion
                    #region 尋找是否UnloadingPort上已經有相同FromCst的Job,找空Slot
                    if (target_stage == null)
                    {
                        foreach (var unloading_port in unloading_ports)
                        {
                            bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotCSTSeq == curBcsJob.CassetteSequenceNo);
                            //找到有相同FromCst的Job且相同Slot必須為空
                            if (found)
                            {
                                foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                                {
                                    if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                    {
                                        unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                        target_stage = unloading_port.Item1;
                                        target_slot_no = slot_no;
                                        break;
                                    }
                                }
                            }
                            if (target_stage != null) break;
                        }
                    }
                    #endregion
                    #region UnloadingPort上沒有相同FromCst的Job,找空CST
                    if (target_stage == null)
                    {
                        foreach (var unloading_port in unloading_ports)
                        {
                            bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotGlassExist == "1");
                            if (!found)
                            {
                                foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                                {
                                    if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                    {
                                        unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                        target_stage = unloading_port.Item1;
                                        target_slot_no = slot_no;
                                        break;
                                    }
                                }
                            }
                            if (target_stage != null) break;
                        }
                    }
                    #endregion
                    #region UnloadingPort上沒有空CST,找空Slot
                    if (target_stage == null)
                    {
                        foreach (var unloading_port in unloading_ports)
                        {
                            bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotGlassExist == "0");
                            if (found)
                            {
                                foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                                {
                                    if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                    {
                                        unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                        target_stage = unloading_port.Item1;
                                        target_slot_no = slot_no;
                                        break;
                                    }
                                }
                            }
                            if (target_stage != null) break;
                        }
                    }
                    #endregion
                    if (target_stage == null)
                    {
                        buffering = true;
                    }
                }
                //如果不能去unloading port,則要回原cst原slot先buffering
                if (buffering)
                {
                    #region Cassette source_cassette
                    Cassette source_cassette = ObjectManager.CassetteManager.GetCassette(curBcsJob.FromCstID);
                    if (source_cassette == null)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob Source Cassette({2})!",
                                                    "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.FromCstID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion

                        errMsg = string.Format("[{0}] do buffering,can not find curBcsJob Source Cassette({1})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.FromCstID);

                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                    #endregion
                    #region Port source_port
                    Port source_port = ObjectManager.PortManager.GetPort(source_cassette.PortID);
                    if (source_port == null)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob Source Port({2})!",
                                                    "L2", MethodBase.GetCurrentMethod().Name, source_cassette.PortID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion

                        errMsg = string.Format("[{0}] can not find curBcsJob Source Port({1})!",
                                                MethodBase.GetCurrentMethod().Name, source_cassette.PortID);

                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                    #endregion
                    #region job is return to source cst & slot,need check job is already in cst
                    RobotStage source_stage = ObjectManager.RobotStageManager.GetRobotStagebyPortNo(source_port.Data.NODENO, source_port.Data.PORTNO);
                    //先找出source_port的stage
                    if (source_stage != null)
                    {
                        if (source_stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST)
                        {
                            //從stage找slot是否為空
                            if (source_stage.PortSlotInfos[from_slot_no - 1].slotGlassExist == "0")
                            {
                                target_stage = source_stage;
                                target_slot_no = from_slot_no;

                                //update job rcsbufferingflag=1	
                                lock (curBcsJob)
                                {
                                    curBcsJob.CfSpecial.RCSBufferingFlag = "1";
                                }
                                ObjectManager.JobManager.EnqueueSave(curBcsJob);
                            }
                            else
                            {
                                #region check job is already in cst
                                if (source_stage.PortSlotInfos[from_slot_no - 1].slotCSTSeq == curBcsJob.CassetteSequenceNo &&
                                    source_stage.PortSlotInfos[from_slot_no - 1].slotJobSeq == curBcsJob.JobSequenceNo)
                                {

                                    #region  [DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] curBcsJob do Loading_Buffer but job is already in cst,Return Port({1}) Slot({2})",
                                                                "L2", MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    }

                                    #endregion

                                    errMsg = string.Format("[{0}] curBcsJob do Loading_Buffer but job is aready in cst,Return Port({1}) Slot({2})",
                                                            MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OriginalCassetteOriginalSlotIsNotEmpty);
                                    robotConText.SetReturnMessage(errMsg);
                                    return false;

                                }
                                else
                                {
                                    #region  [DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] curBcsJob do Loading_Buffer,Return Port({2}) Slot({3}) is not Empty",
                                                                "L2", MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    }

                                    #endregion

                                    errMsg = string.Format("[{0}] curBcsJob do Loading_Buffer,Return Port({1}) Slot({2}) is not Empty",
                                                            MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OriginalCassetteOriginalSlotIsNotEmpty);
                                    robotConText.SetReturnMessage(errMsg);
                                    return false;
                                }
                                #endregion
                            }
                        }
                    }
                    #endregion
                }
                #endregion

                if (target_stage != null)
                {
                    curDefineCmd.Cmd01_TargetPosition = int.Parse(target_stage.Data.STAGEID);
                    curDefineCmd.Cmd01_TargetSlotNo = target_slot_no;
                }
                else
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) can not find target stage!(please check 1.EQP.File.ProductTypeCheckMode is Disable=>Job is OK/NG/Other Judge,go to port mode:OK/NG/Other/EMP/MIX unloading port 2.EQP.File.ProductTypeCheckMode is Enable=>port.File.ProductType is 0 or port.File.ProductType == Job.ProductType,Job is OK/NG/Other Judge,go to port mode:OK/NG/Other/EMP/MIX unloading port)",
                                                "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not find target stage!(please check 1.EQP.File.ProductTypeCheckMode is Disable=>Job is OK/NG/Other Judge,go to port mode:OK/NG/Other/EMP/MIX unloading port 2.EQP.File.ProductTypeCheckMode is Enable=>port.File.ProductType is 0 or port.File.ProductType == Job.ProductType,Job is OK/NG/Other Judge,go to port mode:OK/NG/Other/EMP/MIX unloading port)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_UnloadDispatch_No_TargetStage);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }


        /// <summary>
        /// <br>CF random mode(CFREP/CFMAC)</br>
        /// <br>job is ok judge,return to ok unloading port cst or emp unloading port cst</br>
        /// <br>job is ng judge,return to ng unloading port cst or emp unloading port cst</br>
        /// </summary>
        /// <param name="robotContext">robot context object</param>
        /// <returns>true=no error</returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("OR0009")]
        public bool OrderBy_RandomUnloadDispatch(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;

                List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];
                List<RobotStage> stage_list = beforeOrderByStageList == null ? new List<RobotStage>() : beforeOrderByStageList.ToList();

                #region [ Get curRobot Entity ]

                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

                //找不到 Robot 回NG
                if (curRobot == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region Job curBcsJob
                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];
                if (curBcsJob == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find curBcsJob Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region DefineNormalRobotCmd curDefineCmd
                DefineNormalRobotCmd curDefineCmd = (DefineNormalRobotCmd)robotConText[eRobotContextParameter.DefineNormalRobotCmd];

                //找不到任何的Stage List時回NG
                if (curDefineCmd == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find DefineNormalRobotCmd!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find DefineNormalRobotCmd Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetDefineNormalRobotCmd);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region Get L2 Equipment
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);
                if (eqp == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot Node No!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot Node No!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_EQP_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                RobotStage target_stage = null;
                int target_slot_no = 0;
                int from_slot_no = int.Parse(curBcsJob.FromSlotNo);

                #region * Decide target_stage,target_slot_no
                bool buffering = false;
                if (curBcsJob.RobotWIP.CurSendOutJobJudge == "1")
                {
                    #region 找出JobJudge與PortMode, UnloadingPortSetting相符的UnloadingPort
                    List<Tuple<RobotStage, Port, Cassette>> unloading_ports = new List<Tuple<RobotStage, Port, Cassette>>();
                    foreach (RobotStage stage in stage_list)
                    {
                        Port port = ObjectManager.PortManager.GetPort(stage.Data.STAGEID);
                        Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID);
                        if (port.File.Type == ePortType.UnloadingPort && stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST && cst != null)
                        {
                            bool match = ForCF_CheckJobJudge_PortMode_UnloadingPortSetting(curBcsJob, port);
                            if (match)
                            {
                                unloading_ports.Add(Tuple.Create(stage, port, cst));
                            }
                        }
                    }
                    unloading_ports = unloading_ports.OrderBy(p => p, new UnloaderPortModeOrderByComparer(curBcsJob)).ToList();
                    #endregion
                    #region 尋找是否UnloadingPort上已經有相同FromCst的Job,找空Slot
                    if (target_stage == null)
                    {
                        foreach (var unloading_port in unloading_ports)
                        {
                            bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotCSTSeq == curBcsJob.CassetteSequenceNo);
                            //找到有相同FromCst的Job且相同Slot必須為空
                            if (found)
                            {
                                foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                                {
                                    if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                    {
                                        unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                        target_stage = unloading_port.Item1;
                                        target_slot_no = slot_no;
                                        break;
                                    }
                                }
                            }
                            if (target_stage != null) break;
                        }
                    }
                    #endregion
                    #region UnloadingPort上沒有相同FromCst的Job,找空CST
                    if (target_stage == null)
                    {
                        foreach (var unloading_port in unloading_ports)
                        {
                            bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotGlassExist == "1");
                            if (!found)
                            {
                                foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                                {
                                    if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                    {
                                        unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                        target_stage = unloading_port.Item1;
                                        target_slot_no = slot_no;
                                        break;
                                    }
                                }
                            }
                            if (target_stage != null) break;
                        }
                    }
                    #endregion
                    #region UnloadingPort上沒有空CST,找空Slot
                    if (target_stage == null)
                    {
                        foreach (var unloading_port in unloading_ports)
                        {
                            bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotGlassExist == "0");
                            if (found)
                            {
                                foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                                {
                                    if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                    {
                                        unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                        target_stage = unloading_port.Item1;
                                        target_slot_no = slot_no;
                                        break;
                                    }
                                }
                            }
                            if (target_stage != null) break;
                        }
                    }
                    #endregion
                    if (target_stage == null)
                    {
                        buffering = true;
                    }
                }
                else
                {
                    #region 找出JobJudge與PortMode, UnloadingPortSetting相符的UnloadingPort
                    List<Tuple<RobotStage, Port, Cassette>> unloading_ports = new List<Tuple<RobotStage, Port, Cassette>>();
                    foreach (RobotStage stage in stage_list)
                    {
                        Port port = ObjectManager.PortManager.GetPort(stage.Data.STAGEID);
                        Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID);
                        if (port.File.Type == ePortType.UnloadingPort && stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST && cst != null)
                        {
                            bool match = ForCF_CheckJobJudge_PortMode_UnloadingPortSetting(curBcsJob, port);
                            if (match)
                            {
                                unloading_ports.Add(Tuple.Create(stage, port, cst));
                            }
                        }
                    }
                    unloading_ports = unloading_ports.OrderBy(p => p, new UnloaderPortModeOrderByComparer(curBcsJob)).ToList();
                    #endregion
                    #region 尋找是否UnloadingPort上已經有相同FromCst的Job,找空Slot
                    if (target_stage == null)
                    {
                        foreach (var unloading_port in unloading_ports)
                        {
                            bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotCSTSeq == curBcsJob.CassetteSequenceNo);
                            //找到有相同FromCst的Job且相同Slot必須為空
                            if (found)
                            {
                                foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                                {
                                    if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                    {
                                        unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                        target_stage = unloading_port.Item1;
                                        target_slot_no = slot_no;
                                        break;
                                    }
                                }
                            }
                            if (target_stage != null) break;
                        }
                    }
                    #endregion
                    #region UnloadingPort上沒有相同FromCst的Job,找空CST
                    if (target_stage == null)
                    {
                        foreach (var unloading_port in unloading_ports)
                        {
                            bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotGlassExist == "1");
                            if (!found)
                            {
                                foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                                {
                                    if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                    {
                                        unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                        target_stage = unloading_port.Item1;
                                        target_slot_no = slot_no;
                                        break;
                                    }
                                }
                            }
                            if (target_stage != null) break;
                        }
                    }
                    #endregion
                    #region UnloadingPort上沒有空CST,找空Slot
                    if (target_stage == null)
                    {
                        foreach (var unloading_port in unloading_ports)
                        {
                            bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotGlassExist == "0");
                            if (found)
                            {
                                foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                                {
                                    if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                    {
                                        unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                        target_stage = unloading_port.Item1;
                                        target_slot_no = slot_no;
                                        break;
                                    }
                                }
                            }
                            if (target_stage != null) break;
                        }
                    }
                    #endregion
                    if (target_stage == null)
                    {
                        buffering = true;
                    }
                }
                //如果不能去unloading port,則要回原cst原slot先buffering
                if (buffering)
                {
                    #region Cassette source_cassette
                    Cassette source_cassette = ObjectManager.CassetteManager.GetCassette(curBcsJob.FromCstID);
                    if (source_cassette == null)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob Source Cassette({2})!",
                                                    "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.FromCstID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion

                        errMsg = string.Format("[{0}] do buffering,can not find curBcsJob Source Cassette({1})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.FromCstID);

                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                    #endregion
                    #region Port source_port
                    Port source_port = ObjectManager.PortManager.GetPort(source_cassette.PortID);
                    if (source_port == null)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob Source Port({2})!",
                                                    "L2", MethodBase.GetCurrentMethod().Name, source_cassette.PortID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion

                        errMsg = string.Format("[{0}] can not find curBcsJob Source Port({1})!",
                                                MethodBase.GetCurrentMethod().Name, source_cassette.PortID);

                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                    #endregion
                    #region job is return to source cst & slot,need check job is already in cst
                    RobotStage source_stage = ObjectManager.RobotStageManager.GetRobotStagebyPortNo(source_port.Data.NODENO, source_port.Data.PORTNO);
                    //先找出source_port的stage
                    if (source_stage != null)
                    {
                        if (source_stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST)
                        {
                            //從stage找slot是否為空
                            if (source_stage.PortSlotInfos[from_slot_no - 1].slotGlassExist == "0")
                            {
                                target_stage = source_stage;
                                target_slot_no = from_slot_no;

                                //update job rcsbufferingflag=1	
                                lock (curBcsJob)
                                {
                                    curBcsJob.CfSpecial.RCSBufferingFlag = "1";
                                }
                                ObjectManager.JobManager.EnqueueSave(curBcsJob);
                            }
                            else
                            {
                                #region check job is already in cst
                                if (source_stage.PortSlotInfos[from_slot_no - 1].slotCSTSeq == curBcsJob.CassetteSequenceNo &&
                                    source_stage.PortSlotInfos[from_slot_no - 1].slotJobSeq == curBcsJob.JobSequenceNo)
                                {

                                    #region  [DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] curBcsJob do Loading_Buffer but job is already in cst,Return Port({1}) Slot({2})",
                                                                "L2", MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    }

                                    #endregion

                                    errMsg = string.Format("[{0}] curBcsJob do Loading_Buffer but job is aready in cst,Return Port({1}) Slot({2})",
                                                            MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OriginalCassetteOriginalSlotIsNotEmpty);
                                    robotConText.SetReturnMessage(errMsg);
                                    return false;

                                }
                                else
                                {
                                    #region  [DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] curBcsJob do Loading_Buffer,Return Port({2}) Slot({3}) is not Empty",
                                                                "L2", MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    }

                                    #endregion

                                    errMsg = string.Format("[{0}] curBcsJob do Loading_Buffer,Return Port({1}) Slot({2}) is not Empty",
                                                            MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OriginalCassetteOriginalSlotIsNotEmpty);
                                    robotConText.SetReturnMessage(errMsg);
                                    return false;
                                }
                                #endregion
                            }
                        }
                    }
                    #endregion
                }
                #endregion

                if (target_stage != null)
                {
                    curDefineCmd.Cmd01_TargetPosition = int.Parse(target_stage.Data.STAGEID);
                    curDefineCmd.Cmd01_TargetSlotNo = target_slot_no;
                }
                else
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) can not find target stage!(please check 1.EQP.File.ProductTypeCheckMode is Disable=>Job is OK/NG/Other Judge,go to port mode:OK/NG/Other/EMP/MIX unloading port 2.EQP.File.ProductTypeCheckMode is Enable=>port.File.ProductType is 0 or port.File.ProductType == Job.ProductType,Job is OK/NG/Other Judge,go to port mode:OK/NG/Other/EMP/MIX unloading port)",
                                                "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not find target stage!(please check 1.EQP.File.ProductTypeCheckMode is Disable=>Job is OK/NG/Other Judge,go to port mode:OK/NG/Other/EMP/MIX unloading port 2.EQP.File.ProductTypeCheckMode is Enable=>port.File.ProductType is 0 or port.File.ProductType == Job.ProductType,Job is OK/NG/Other Judge,go to port mode:OK/NG/Other/EMP/MIX unloading port)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_UnloadDispatch_No_TargetStage);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }


        /// <summary>
        /// <br>CF normal mode,with buffering function</br>
        /// <br>job is from both port,return to source port cst slot</br>
        /// <br>job is from loading port,default return to unloading port cst</br>
        /// <br>job is from loading port,unloading port cst not found,buffering to source port cst slot</br>
        /// </summary>
        /// <param name="robotContext">robot context object</param>
        /// <returns>true=no error</returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("OR0010")]
        public bool OrderBy_NormalUnloadDispatch(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;

                List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];
                List<RobotStage> stage_list = beforeOrderByStageList == null ? new List<RobotStage>() : beforeOrderByStageList.ToList();

                #region [ Get curRobot Entity ]

                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

                //找不到 Robot 回NG
                if (curRobot == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region Job curBcsJob
                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];
                if (curBcsJob == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find curBcsJob Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region DefineNormalRobotCmd curDefineCmd
                DefineNormalRobotCmd curDefineCmd = (DefineNormalRobotCmd)robotConText[eRobotContextParameter.DefineNormalRobotCmd];

                //找不到任何的Stage List時回NG
                if (curDefineCmd == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find DefineNormalRobotCmd!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find DefineNormalRobotCmd Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetDefineNormalRobotCmd);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region Get L2 Equipment
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);
                if (eqp == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot Node No!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot Node No!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_EQP_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                RobotStage target_stage = null;
                int target_slot_no = 0;
                int from_slot_no = int.Parse(curBcsJob.FromSlotNo);
                Port source_port = null;

                #region * Decide target_stage,target_slot_no
                bool buffering = false;
                //判斷來源cst,若無，則要去unloading port
                #region Cassette source_cassette
                Cassette source_cassette = ObjectManager.CassetteManager.GetCassette(curBcsJob.FromCstID);
                if (source_cassette == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob Source Cassette({2})!",
                                                "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.FromCstID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion
                    goto jumpULD;
                    //找不到原cst，去unloading port
                }
                //add by hujunpeng 20190301 for FCWRW100共洗CELL DUMMY,CURRENTREWORKCOUNT=MAXREWORKCOUNT时进ULD NG port
                if(curRobot.Data.LINEID=="FCWRW100")
                {
                    if ( (int.Parse(curBcsJob.CellSpecial.CurrentRwkCount) >= int.Parse(curBcsJob.CellSpecial.MaxRwkCount)&&(int.Parse(curBcsJob.CellSpecial.MaxRwkCount)!=0) )|| (curBcsJob.RobotWIP.CurSendOutJobJudge == "2"))
                    {
                        strlog = string.Format("job[{0}] job judge[{1}]", curBcsJob.GlassChipMaskBlockID, curBcsJob.RobotWIP.CurSendOutJobJudge);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        goto jumpULD;
                    }
                }
                
                #endregion
                #region Port source_port
                source_port = ObjectManager.PortManager.GetPort(source_cassette.PortID);
                if (source_port == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob Source Port({2})!",
                                                "L2", MethodBase.GetCurrentMethod().Name, source_cassette.PortID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find curBcsJob Source Port({1})!",
                                            MethodBase.GetCurrentMethod().Name, source_cassette.PortID);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion
                #region Stage source_stage
                //檢查若來源是both port，回原cst原slot
                if (source_port.File.Type == ePortType.BothPort)
                {
                    RobotStage source_stage = ObjectManager.RobotStageManager.GetRobotStagebyPortNo(source_port.Data.NODENO, source_port.Data.PORTNO);
                    //找原cst原slot
                    if (source_stage != null)
                    {
                        if (source_stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST)
                        {
                            //check source slot是否為空
                            if (source_stage.PortSlotInfos[from_slot_no - 1].slotGlassExist == "0")
                            {
                                target_stage = source_stage;
                                target_slot_no = from_slot_no;
                                goto jumpRet; //回原cst原slot
                            }
                            else
                            {
                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] OK judge curBcsJob Return Port({2}) Slot({3}) is not Empty",
                                                            "L2", MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                }

                                #endregion
                                //原slot不為空，回不去原cst原slot，則要去unloading port
                                goto jumpULD;
                            }
                        }
                    }
                }
                else if (source_port.File.Type == ePortType.LoadingPort && curBcsJob.RobotWIP.CurSendOutJobJudge == "1")
                {
                    // Normal & MQC Mode, Judge OK 的 Job 回原 CST 原 Slot, 即使原 CST 是在 Loader Port 上也一樣
                    RobotStage source_stage = ObjectManager.RobotStageManager.GetRobotStagebyPortNo(source_port.Data.NODENO, source_port.Data.PORTNO);
                    if (source_stage != null)
                    {
                        if (source_stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST)
                        {
                            //check source slot是否為空
                            if (source_stage.PortSlotInfos[from_slot_no - 1].slotGlassExist == "0")
                            {
                                target_stage = source_stage;
                                target_slot_no = from_slot_no;
                                goto jumpRet; //回原cst原slot
                            }
                        }
                    }
                }
                #endregion
            jumpULD:
                #region 找出UnloadingPort
                List<Tuple<RobotStage, Port, Cassette>> unloading_ports = new List<Tuple<RobotStage, Port, Cassette>>();
                foreach (RobotStage stage in stage_list)
                {
                    Port port = ObjectManager.PortManager.GetPort(stage.Data.STAGEID);
                    Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID);
                    if (port.File.Type == ePortType.UnloadingPort && stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST && cst != null)
                    {
                        unloading_ports.Add(Tuple.Create(stage, port, cst));
                    }
                }
                unloading_ports = unloading_ports.OrderBy(p => p, new UnloaderPortModeOrderByComparer(curBcsJob)).ToList();
                #endregion
                #region 尋找是否UnloadingPort上已經有portmode與jobjude相同者且是相同producttype Cst
                if (target_stage == null)
                {
                    foreach (var unloading_port in unloading_ports)
                    {
                        bool match = false;
                        switch (unloading_port.Item2.File.Mode)
                        {
                        case ePortMode.OK: match = (curBcsJob.RobotWIP.CurSendOutJobJudge == "1"); break;//Port Mode OK, JobJudge OK
                        case ePortMode.NG: match = (curBcsJob.RobotWIP.CurSendOutJobJudge == "2"); break;//Port Mode NG, JobJudge NG
                        case ePortMode.Rework: match = (curBcsJob.RobotWIP.CurSendOutJobJudge == "3"); break;//Port Mode RW, JobJudge RW
                        case ePortMode.PD: match = (curBcsJob.RobotWIP.CurSendOutJobJudge == "4"); break;//Port Mode PD, JobJudge PD
                        case ePortMode.RP: match = (curBcsJob.RobotWIP.CurSendOutJobJudge == "5"); break;//Port Mode RP, JobJudge RP
                        case ePortMode.IR: match = (curBcsJob.RobotWIP.CurSendOutJobJudge == "6"); break;//Port Mode IR, JobJudge IR
                        case ePortMode.MIX: match = (curBcsJob.RobotWIP.CurSendOutJobJudge != "1"); break;//Port Mode MIX, JobJudge Not OK
                        }

                        if ((match && eqp.File.ProductTypeCheckMode == eEnableDisable.Disable) ||
                            (match && eqp.File.ProductTypeCheckMode == eEnableDisable.Enable && (string.IsNullOrEmpty(unloading_port.Item2.File.ProductType) || unloading_port.Item2.File.ProductType == "0" || unloading_port.Item2.File.ProductType == curBcsJob.ProductType.Value.ToString())))
                        {
                            foreach (int curSlotNo in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                            {
                                if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[curSlotNo]))
                                {
                                    unloading_port.Item1.curLDRQ_EmptySlotList[curSlotNo] = curBcsJob.JobKey;
                                    target_stage = unloading_port.Item1;
                                    target_slot_no = curSlotNo;
                                    break;
                                }
                            }
                        }
                        if (target_stage != null) break;
                    }
                }
                #endregion
                if (target_stage == null)
                {
                    if (Workbench.LineType != eLineType.CF.FCREW_TYPE1)
                    {
                        buffering = true;
                    }
                }

                //如果不能去unloading port,則要回原cst原slot先buffering
                if (buffering)
                {
                    #region job is return to source cst & slot,need check job is already in cst
                    if (source_port != null)
                    {
                        RobotStage source_stage = ObjectManager.RobotStageManager.GetRobotStagebyPortNo(source_port.Data.NODENO, source_port.Data.PORTNO);
                        //先找出source_port的stage
                        if (source_stage != null)
                        {
                            if (source_stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST)
                            {
                                //從stage找slot是否為空
                                if (source_stage.PortSlotInfos[from_slot_no - 1].slotGlassExist == "0")
                                {
                                    target_stage = source_stage;
                                    target_slot_no = from_slot_no;

                                    //update job rcsbufferingflag=1	
                                    lock (curBcsJob)
                                    {
                                        curBcsJob.CfSpecial.RCSBufferingFlag = "1";
                                    }
                                    ObjectManager.JobManager.EnqueueSave(curBcsJob);
                                }
                                else
                                {
                                    #region check job is already in cst
                                    if (source_stage.PortSlotInfos[from_slot_no - 1].slotCSTSeq == curBcsJob.CassetteSequenceNo &&
                                        source_stage.PortSlotInfos[from_slot_no - 1].slotJobSeq == curBcsJob.JobSequenceNo)
                                    {

                                        #region  [DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) Return Port({4}) Slot({5}) but job is already in cst",
                                                                    "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, source_port.Data.PORTID, from_slot_no);

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] Job({1}_{2}) Return Port({3}) Slot({4}) but job is already in cst",
                                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, source_port.Data.PORTID, from_slot_no);

                                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OriginalCassetteOriginalSlotIsNotEmpty);
                                        robotConText.SetReturnMessage(errMsg);
                                        return false;

                                    }
                                    else
                                    {
                                        #region  [DebugLog]

                                        if (IsShowDetialLog == true)
                                        {

                                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) Return source cst,but Port({4}) Slot({5}) is not Empty",
                                                                    "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, source_port.Data.PORTID, from_slot_no);

                                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                        }

                                        #endregion

                                        errMsg = string.Format("[{0}] Job({1}_{2}) Return source cst,but Port({3}) Slot({4}) is not Empty",
                                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, source_port.Data.PORTID, from_slot_no);

                                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OriginalCassetteOriginalSlotIsNotEmpty);
                                        robotConText.SetReturnMessage(errMsg);
                                        return false;
                                    }
                                    #endregion
                                }
                            }
                        }
                    }
                    #endregion
                }
                #endregion
            jumpRet:
                if (target_stage != null)
                {
                    curDefineCmd.Cmd01_TargetPosition = int.Parse(target_stage.Data.STAGEID);
                    curDefineCmd.Cmd01_TargetSlotNo = target_slot_no;
                }
                else
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) can not find target stage!(please check source port/cst or cst slot is empty)",
                                                "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not find target stage!(please check source port/cst or cst slot is empty)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_UnloadDispatch_No_TargetStage);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        //20160525
        [UniAuto.UniBCS.OpiSpec.Help("OR0024")]
        public bool OrderBy_EQPRTCLoadingPortDispatch(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;

                List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];
                List<RobotStage> stage_list = beforeOrderByStageList == null ? new List<RobotStage>() : beforeOrderByStageList.ToList();

                #region [ Get curRobot Entity ]

                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

                //找不到 Robot 回NG
                if (curRobot == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region Job curBcsJob
                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];
                if (curBcsJob == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find curBcsJob Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region DefineNormalRobotCmd curDefineCmd
                DefineNormalRobotCmd curDefineCmd = (DefineNormalRobotCmd)robotConText[eRobotContextParameter.DefineNormalRobotCmd];

                //找不到任何的Stage List時回NG
                if (curDefineCmd == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find DefineNormalRobotCmd!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find DefineNormalRobotCmd Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetDefineNormalRobotCmd);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region Get L2 Equipment
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);
                if (eqp == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot Node No!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot Node No!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_EQP_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                RobotStage target_stage = null;
                int target_slot_no = 0;
                int from_slot_no = int.Parse(curBcsJob.FromSlotNo);
                Port source_port = null;

                #region Cassette source_cassette
                Cassette source_cassette = ObjectManager.CassetteManager.GetCassette(curBcsJob.FromCstID);
                if (source_cassette == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob Source Cassette({2})!",
                                                "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.FromCstID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }
                    #endregion
                    return false; //找不到原CST可以放EQP RTC回來的片,就return false,不去unloading port了
                }
                #endregion
                #region Port source_port
                source_port = ObjectManager.PortManager.GetPort(source_cassette.PortID);
                if (source_port == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob Source Port({2})!",
                                                "L2", MethodBase.GetCurrentMethod().Name, source_cassette.PortID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find curBcsJob Source Port({1})!",
                                            MethodBase.GetCurrentMethod().Name, source_cassette.PortID);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion
                #region Stage source_stage
                //只會是Loading/Both port,CF MQC 特殊需求,當下游EQP不收片,超過等待時間,先放回原CST原slot
                if (source_port.File.Type == ePortType.BothPort || source_port.File.Type == ePortType.LoadingPort)
                {
                    RobotStage source_stage = ObjectManager.RobotStageManager.GetRobotStagebyPortNo(source_port.Data.NODENO, source_port.Data.PORTNO);
                    //找原cst原slot
                    if (source_stage != null)
                    {
                        if (source_stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST)
                        {
                            //check source slot是否為空
                            if (source_stage.PortSlotInfos[from_slot_no - 1].slotGlassExist == "0")
                            {
                                target_stage = source_stage;
                                target_slot_no = from_slot_no;
                                goto jumpRet; //回原cst原slot
                            }
                            else
                            {
                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Return Port({2}) Slot({3}) is not Empty",
                                                            "L2", MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                }

                                #endregion
                                //原slot不為空，要回去原cst原slot
                            }
                        }
                    }
                }
                #endregion

            jumpRet:
                if (target_stage != null)
                {
                    curDefineCmd.Cmd01_TargetPosition = int.Parse(target_stage.Data.STAGEID);
                    curDefineCmd.Cmd01_TargetSlotNo = target_slot_no;
                }
                else
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) can not find target stage!(Can't find source port/cst or cst slot is not empty)",
                                                "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not find target stage!(Can't find source port/cst or cst slot is not empty)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_UnloadDispatch_No_TargetStage);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// <br>CFMAC line</br>
        /// <br>select MACRO to transfer job</br>
        /// <br>Glass FIP Flag On ,transfer to MACRO with FIP Mode</br>
        /// <br>Glass MQC Flag On ,transfer to MACRO with MQC Mode</br>
        /// <br>Glass B-Marco Flag On ,transfer to MACRO with B-Marco Mode</br>
        /// </summary>
        /// <param name="robotContext">robot context object</param>
        /// <returns>true=no error</returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("OR0011")]
        public bool OrderBy_SelectMacro(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;

                List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];
                List<RobotStage> stage_list = beforeOrderByStageList == null ? new List<RobotStage>() : beforeOrderByStageList.ToList();

                #region Job curBcsJob
                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];
                if (curBcsJob == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find curBcsJob Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                if (string.IsNullOrEmpty(curBcsJob.EQPFlag) || curBcsJob.EQPFlag.Length < 3)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] curBcsJob[{2}_{3}] EQPFlag is null or empty or length less than 3",
                                                "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] curBcsJob[{1}_{2}] EQPFlag is null or empty or length less than 3",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region DefineNormalRobotCmd curDefineCmd
                DefineNormalRobotCmd curDefineCmd = (DefineNormalRobotCmd)robotConText[eRobotContextParameter.DefineNormalRobotCmd];

                //找不到任何的Stage List時回NG
                if (curDefineCmd == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find DefineNormalRobotCmd!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find DefineNormalRobotCmd Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetDefineNormalRobotCmd);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                RobotStage target_stage = null;
                int target_slot_no = 0;

                #region * Decide MACRO target_stage,target_slot_no
                string strEqqpRunModes = "";
                foreach (var stage in stage_list)
                {
                    if (stage.Data.STAGETYPE != eRobotStageType.EQUIPMENT)
                    {
                        continue;
                    }
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(stage.Data.NODENO);
                    if (eqp == null)
                    {
                        continue;
                    }
                    strEqqpRunModes += string.Format("{0} EqqpRunMode:({1}),",eqp.Data.NODENAME, eqp.File.EquipmentRunMode);
                    //check if eq recipeid is full of 0 
                    string curNodePPID = curBcsJob.PPID.Substring(eqp.Data.RECIPEIDX, eqp.Data.RECIPELEN);
                    string curByPassPPID = new string('0', eqp.Data.RECIPELEN);
                    if (curNodePPID == curByPassPPID)
                    {
                        strEqqpRunModes += string.Format("And PPID:({0}),", curByPassPPID);
                        continue;
                    }


                    //check if eq runmode match
                    bool match = false;
                    switch (eqp.File.EquipmentRunMode)
                    {
                    case "MQC": match = (curBcsJob.EQPFlag[1] == '1'); break;
                    case "BMACRO": match = (curBcsJob.EQPFlag[2] == '1'); break;
                    case "FIMACRO": match = (curBcsJob.EQPFlag[0] == '1'); break;
                    }
                    if (match)
                    {
                        target_stage = stage;
                        target_slot_no = 1;
                        break;
                    }
                }
                #endregion

                if (target_stage != null)
                {
                    curDefineCmd.Cmd01_TargetPosition = int.Parse(target_stage.Data.STAGEID);
                    curDefineCmd.Cmd01_TargetSlotNo = target_slot_no;
                }
                else
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) can not find target stage!(EQP Run Mode and Job EQPFlag is must same,please check 1.FIMACRO = Job.EQPFlag[0] 2.MQC = Job.EQPFlag[1] 3.BMACRO = EQPFlag[2])",
                                                "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) curEqpFlag({5}) But {6} can not find target stage!(EQP Run Mode Must Match With Job's EQPFlag ,please check 1.FIMACRO = Job.EQPFlag[0] 2.MQC = Job.EQPFlag[1] 3.BMACRO = EQPFlag[2])",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), curBcsJob.EQPFlag, strEqqpRunModes);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_SelectMAC_No_TargetStage);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }


        /// <summary>
        /// <br>CFREP line</br>
        /// <br>select REPAIR to transfer job</br>
        /// <br>RP Glass transfer to Repair or Ink Repair</br>
        /// <br>IR Glass transfer to Ink Repair</br>		
        /// </summary>
        /// <param name="robotContext">robot context object</param>
        /// <returns>true=no error</returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("OR0012")]
        public bool OrderBy_SelectRepair(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;
                IOrderedEnumerable<RobotStage> afterOrderByResultInfo;//add by hujunpeng
                List<RobotStage> afterOrderByStageList;//add by hujunpeng

                List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];
                List<RobotStage> stage_list = beforeOrderByStageList == null ? new List<RobotStage>() : beforeOrderByStageList.ToList();

                #region [ Get curRobot Entity ]

                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

                //找不到 Robot 回NG
                if (curRobot == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region Job curBcsJob
                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];
                if (curBcsJob == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find curBcsJob Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region DefineNormalRobotCmd curDefineCmd
                DefineNormalRobotCmd curDefineCmd = (DefineNormalRobotCmd)robotConText[eRobotContextParameter.DefineNormalRobotCmd];

                //找不到任何的Stage List時回NG
                if (curDefineCmd == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find DefineNormalRobotCmd!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find DefineNormalRobotCmd Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetDefineNormalRobotCmd);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                RobotStage target_stage = null;
                int target_slot_no = 0;

                #region * Decide Repair target_stage,target_slot_no
                //check if exists ir job
                List<Job> robotStageCanControlJobList = (List<Job>)curRobot.Context[eRobotContextParameter.StageCanControlJobList];
                bool existsIRJob = false;
                foreach (var job in robotStageCanControlJobList)
                {
                    //是IR job且要抽且尚末進eq
                    if (job.RobotWIP.CurSendOutJobJudge == "6" &&
                        job.SamplingSlotFlag == "1" &&
                        !job.RobotWIP.EqpReport_linkSignalSendOutTrackingData.Contains("1")&&job.RobotWIP.CurStepNo==1)//排除已run完还未退port的 JOB JUDGE 6的玻璃
                    {
                        RobotStage stage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(job.RobotWIP.CurLocation_StageID);
                        if (stage != null)
                        {
                            if (stage.Data.STAGETYPE != eStageType.EQUIPMENT)
                            {
                                existsIRJob = true;
                                break;
                            }
                        }
                    }
                }
                string strEqpRunMode = "But Recive Job Stage:";
                foreach (var stage in stage_list)
                {
                    if (stage.Data.STAGETYPE != eRobotStageType.EQUIPMENT)
                    {
                        continue;
                    }
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(stage.Data.NODENO);
                    if (eqp == null)
                    {
                        continue;
                    }

                    //check if eq recipeid is full of 0 
                    string curNodePPID = curBcsJob.PPID.Substring(eqp.Data.RECIPEIDX, eqp.Data.RECIPELEN);
                    string curByPassPPID = new string('0', eqp.Data.RECIPELEN);
                    if (curNodePPID == curByPassPPID)
                    {
                        continue;
                    }
                    strEqpRunMode += string.Format("stage{0} eqpRunMode:{1},", stage.Data.STAGEID, eqp.File.EquipmentRunMode);
                    //check if eq runmode match
                    bool match = false;
                    switch (eqp.File.EquipmentRunMode)
                    {
                    case "NORMAL": match = (curBcsJob.RobotWIP.CurSendOutJobJudge == "5"); break;//JobJudge "5" = RP
                    case "INK": match = (curBcsJob.RobotWIP.CurSendOutJobJudge == "6" || (!existsIRJob && curBcsJob.RobotWIP.CurSendOutJobJudge == "5")); break;//JobJudge "6" = IR
                    }
                    if (match)
                    {
                        target_stage = stage;
                        target_slot_no = 1;
                        //将已经排序成功取得的stage ID 传给robotConText
                        stage_list.Clear();
                        stage_list.Add(target_stage);
                        afterOrderByStageList=stage_list;
                        afterOrderByResultInfo = afterOrderByStageList.OrderBy(s => s, new NoCompare());
                        robotConText.AddParameter(eRobotContextParameter.AfrerOrderByResultInfo, afterOrderByResultInfo);
                        break;
                    }
                }
                #endregion

                if (target_stage != null)
                {
                    curDefineCmd.Cmd01_TargetPosition = int.Parse(target_stage.Data.STAGEID);
                    curDefineCmd.Cmd01_TargetSlotNo = target_slot_no;                             
                }
                else
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) can not find target stage!(please check 1.If JobJudge=5(RP) Can Enter into EQP (EQP Run Mode= NORMAL or INK) 2.If JobJudge=6(IR) Only Can Enter into EQP(EQP Run Mode=INK)",
                                                "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) CurSendOutJobJudge({5}) {6} can not find target stage!(please check 1.If JobJudge=5(RP) Can Enter into EQP (EQP Run Mode= NORMAL or INK) 2.If JobJudge=6(IR) Only Can Enter into EQP(EQP Run Mode=INK)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString()
                                                , curBcsJob.RobotWIP.CurSendOutJobJudge, strEqpRunMode);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Select_No_TargetStage);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }


        /// <summary>
        /// <br>CFPSH line</br>
        /// <br>select PSH to transfer job</br>	
        /// </summary>
        /// <param name="robotContext">robot context object</param>
        /// <returns>true=no error</returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("OR0013")]
        public bool OrderBy_SelectPSH(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;

                List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];
                List<RobotStage> stage_list = beforeOrderByStageList == null ? new List<RobotStage>() : beforeOrderByStageList.ToList();

                #region Job curBcsJob
                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];
                if (curBcsJob == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find curBcsJob Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region DefineNormalRobotCmd curDefineCmd
                DefineNormalRobotCmd curDefineCmd = (DefineNormalRobotCmd)robotConText[eRobotContextParameter.DefineNormalRobotCmd];

                //找不到任何的Stage List時回NG
                if (curDefineCmd == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find DefineNormalRobotCmd!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find DefineNormalRobotCmd Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetDefineNormalRobotCmd);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                RobotStage target_stage = null;
                int target_slot_no = 0;

                #region * Decide PSH target_stage,target_slot_no
                //get job trackingdata
                var Trackinglist = ObjectManager.SubJobDataManager.Decode2(curBcsJob.RobotWIP.EqpReport_linkSignalSendOutTrackingData, "TrackingData");

                foreach (var stage in stage_list)
                {
                    if (stage.Data.STAGETYPE != eRobotStageType.EQUIPMENT)
                    { 
                        continue;
                    }
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(stage.Data.NODENO);
                    if (eqp == null)
                    {
                        continue;
                    }
                    string curNodePPID = curBcsJob.PPID.Substring(eqp.Data.RECIPEIDX, eqp.Data.RECIPELEN);
                    string curByPassPPID = new string('0', eqp.Data.RECIPELEN);

                    #region check eq is not tracked
                    //get stage's tracking
                    int offset;
                    bool done = int.TryParse(stage.Data.TRACKDATASEQLIST, out offset);
                    if (!done)
                    {
                        continue;
                    }
                    var Tracking = Trackinglist.Find(t => t.Item2 == offset);
                    if (Tracking == null)
                    {
                        continue;
                    }
                   
                    #endregion
                    //排除巳在目前位置 和recipeid is not full of 0
                    if (stage.Data.STAGEID == curBcsJob.RobotWIP.CurLocation_StageID || curNodePPID == curByPassPPID || Tracking.Item4.Contains('1'))
                    {
                        beforeOrderByStageList.Remove(stage);
                        continue;
                    }
                    //有找到可進的PSH
                    target_stage = stage;
                    target_slot_no = 1;
                    break;
                }
                #endregion

                if (target_stage != null)
                {
                    curDefineCmd.Cmd01_TargetPosition = int.Parse(target_stage.Data.STAGEID);
                    curDefineCmd.Cmd01_TargetSlotNo = target_slot_no;
                }
                else
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) can not find target stage!(Please check Job Tracking Data,because PSH EQP was already going)",
                                                "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not find target stage!(Please check Job Tracking Data,because PSH EQP was already going)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Select_No_TargetStage);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        /// <summary>
        /// <br>CELL Job Grade與UnloadDispathingRule</br>
        /// <br>Port的UnloadDispatchRule與Job Grade相符者, 排優先</br>
        /// </summary>
        /// <param name="robotContext">robot context object</param>
        /// <returns>true=no error</returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("OR0017")]
        public bool OrderBy_JobGradePortUnloadDispatchingRule(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;

                IOrderedEnumerable<RobotStage> afterOrderByResultInfo;

                #region [ Get curRobot Entity ]

                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

                //找不到 Robot 回NG
                if (curRobot == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Robot 所屬Line Entity ]

                Line robotLine = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);

                if (robotLine == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) can not find Line Entity by LineID({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.LINEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) can not Get Line Entity!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get curBcsJob Entity ]

                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];

                //找不到 Job 回NG
                if (curBcsJob == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get JobInfo!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) can not Get JobInfo!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get After 1st OrderBy flag ]

                bool after1stOrderByFlag = false;

                try
                {
                    after1stOrderByFlag = (bool)robotConText[eRobotContextParameter.Afrer1stOrderByCheckFlag];
                }
                catch (Exception)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] can not Set Afrer1stOrderByCheckFlag!",
                                                "L2");

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Set Is2ndCmdCheckFlag!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetAfter1stOrderByFlag_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ 取得之前OrderBy的結果資訊 ]

                //List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                IOrderedEnumerable<RobotStage> beforeOrderByResultInfo = (IOrderedEnumerable<RobotStage>)robotConText[eRobotContextParameter.AfrerOrderByResultInfo];

                //找不到之前Orderby的結果時回NG
                if (beforeOrderByResultInfo == null)
                {
                    if (after1stOrderByFlag == true)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) can not find before OrderBy Stage List!(Can't find Unloading Dispatch Grade Match port)",
                                                    "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not find before OrderBy Stage List Info!(Can't find Unloading Dispatch Grade Match port)",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Null);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }

                }
                //StageList is empty 回NG
                else if (beforeOrderByResultInfo.ToList().Count == 0)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) before OrderBy Stage List is empty!(Can't find Unloading Dispatch Grade Match port)",
                                                "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) before OrderBy Stage List is empty!(Can't find Unloading Dispatch Grade Match port)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Empty);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get OrderBy Action ]

                string orderbyAction = (string)robotConText[eRobotContextParameter.OrderByAction];

                if (orderbyAction == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find OrderByAction!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find OrderByAction!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OrderByAction_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ 開始排序 ]
                if (orderbyAction.Trim() == eRobotCommonConst.DB_ORDER_BY_ASC)
                {
                    if (after1stOrderByFlag == false)
                    {
                        #region [ 尚未做過任何Order時則Get Want to Order by Stage List ]

                        List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                        //找不到任何的Stage List時回NG
                        if (beforeOrderByStageList == null)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) can not find before OrderBy Stage List!(Can't find Unloading Dispatch Grade Match port)",
                                                        "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not find before OrderBy Stage List Info!(Can't find Unloading Dispatch Grade Match port)",
                                                    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                            robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Null);
                            robotConText.SetReturnMessage(errMsg);

                            return false;


                        }

                        //StageList is empty 回NG
                        if (beforeOrderByStageList.ToList().Count == 0)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) before OrderBy Stage List is empty!(Can't find Unloading Dispatch Grade Match port)",
                                                        "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) before OrderBy Stage List is empty!(Can't find Unloading Dispatch Grade Match port)",
                                                    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                            robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Empty);
                            robotConText.SetReturnMessage(errMsg);

                            return false;
                        }

                        #endregion
                        afterOrderByResultInfo = beforeOrderByStageList.OrderBy(s => s, new JobGradePortUnloadDispatchingRule(robotLine, curBcsJob));
                    }
                    else
                    {
                        afterOrderByResultInfo = beforeOrderByResultInfo.ThenBy(s => s, new JobGradePortUnloadDispatchingRule(robotLine, curBcsJob));
                    }
                }
                else if (orderbyAction.Trim() == eRobotCommonConst.DB_ORDER_BY_DESC)
                {
                    if (after1stOrderByFlag == false)
                    {
                        #region [ 尚未做過任何Order時則Get Want to Order by Stage List ]

                        List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                        //找不到任何的Stage List時回NG
                        if (beforeOrderByStageList == null)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) can not find before OrderBy Stage List!(Can't find Unloading Dispatch Grade Match port)",
                                                        "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not find before OrderBy Stage List Info!(Can't find Unloading Dispatch Grade Match port)",
                                                    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                            robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Null);
                            robotConText.SetReturnMessage(errMsg);

                            return false;


                        }

                        //StageList is empty 回NG
                        if (beforeOrderByStageList.Count == 0)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) before OrderBy Stage List is empty!(Can't find Unloading Dispatch Grade Match port)",
                                                        "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) before OrderBy Stage List is empty!(Can't find Unloading Dispatch Grade Match port)",
                                                    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                            robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Empty);
                            robotConText.SetReturnMessage(errMsg);

                            return false;
                        }

                        #endregion
                        afterOrderByResultInfo = beforeOrderByStageList.OrderByDescending(s => s, new JobGradePortUnloadDispatchingRule(robotLine, curBcsJob));
                    }
                    else
                    {
                        afterOrderByResultInfo = beforeOrderByResultInfo.ThenByDescending(s => s, new JobGradePortUnloadDispatchingRule(robotLine, curBcsJob));
                    }
                }
                else
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] find OrderByAction({2}) is illgal!",
                                                "L2", MethodBase.GetCurrentMethod().Name, orderbyAction);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion
                    errMsg = string.Format("[{0}] find OrderByAction({2}) is illgal!",
                                            MethodBase.GetCurrentMethod().Name, orderbyAction);
                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OrderByAction_Is_illgal);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                #endregion

                //Update Order by結果. ConText需要回傳
                //beforeOrderByResultInfo = afterOrderByStageList;
                robotConText.AddParameter(eRobotContextParameter.AfrerOrderByResultInfo, afterOrderByResultInfo);

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        
        }
        [UniAuto.UniBCS.OpiSpec.Help("OR0018")]
        public bool OrderBy_CELL_TAM_InspectionModeUnloadDispatch(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;

                List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];
                List<RobotStage> stage_list = beforeOrderByStageList == null ? new List<RobotStage>() : beforeOrderByStageList.ToList();

                #region [ Get curRobot Entity ]

                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

                //找不到 Robot 回NG
                if (curRobot == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region Job curBcsJob
                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];
                if (curBcsJob == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find curBcsJob Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region DefineNormalRobotCmd curDefineCmd
                DefineNormalRobotCmd curDefineCmd = (DefineNormalRobotCmd)robotConText[eRobotContextParameter.DefineNormalRobotCmd];

                //找不到任何的Stage List時回NG
                if (curDefineCmd == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find DefineNormalRobotCmd!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find DefineNormalRobotCmd Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetDefineNormalRobotCmd);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region Get L2 Equipment
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);
                if (eqp == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot Node No!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot Node No!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_EQP_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                RobotStage target_stage = null;
                int target_slot_no = 0;
                int from_slot_no = int.Parse(curBcsJob.FromSlotNo);
                Cassette source_cassette = null;
                Port source_port = null;

                #region * Decide target_stage,target_slot_no
                if (curBcsJob.RobotWIP.CurSendOutJobJudge == "1")
                {
                    //OK judge回原cst原slot,若回不去原cst原slot，則要去unloading port
                    #region Cassette source_cassette
                    source_cassette = ObjectManager.CassetteManager.GetCassette(curBcsJob.FromCstID);
                    if (source_cassette == null)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find OK judge curBcsJob Source Cassette({2})!",
                                                    "L1", MethodBase.GetCurrentMethod().Name, curBcsJob.FromCstID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion
                        goto jumpULD;
                        //找不到原cst，回不去原cst原slot，則要去unloading port
                    }
                    #endregion
                    #region Port source_port
                    source_port = ObjectManager.PortManager.GetPort(source_cassette.PortID);
                    if (source_port == null)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find OK greade curBcsJob Source Port({2})!",
                                                    "L1", MethodBase.GetCurrentMethod().Name, source_cassette.PortID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion

                        errMsg = string.Format("[{0}] can not find curBcsJob Source Port({1})!",
                                                MethodBase.GetCurrentMethod().Name, source_cassette.PortID);

                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                    #endregion
                    #region Decide target_stage,target_slot_no
                    RobotStage source_stage = ObjectManager.RobotStageManager.GetRobotStagebyPortNo(source_port.Data.NODENO, source_port.Data.PORTNO);
                    //找原cst原slot
                    if (source_stage != null)
                    {
                        if (source_stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST)
                        {
                            //check source slot是否為空
                            if (source_stage.PortSlotInfos[from_slot_no - 1].slotGlassExist == "0")
                            {
                                target_stage = source_stage;
                                target_slot_no = from_slot_no;
                                goto jumpRet; //回原cst原slot
                            }
                            else
                            {
                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] OK judge curBcsJob Return Port({2}) Slot({3}) is not Empty",
                                                            "L1", MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                }

                                #endregion
                                //原slot不為空，回不去原cst原slot，則要去unloading port
                                goto jumpULD;
                            }
                        }
                    }
                    #endregion
                }
                else
                {
                    //NG judge如果來自both port，則回原cst原slot,若回不去原cst原slot，則要去unloading port
                    #region Cassette source_cassette
                    source_cassette = ObjectManager.CassetteManager.GetCassette(curBcsJob.FromCstID);
                    if (source_cassette == null)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find OK judge curBcsJob Source Cassette({2})!",
                                                    "L1", MethodBase.GetCurrentMethod().Name, curBcsJob.FromCstID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion
                        goto jumpULD;
                        //找不到原cst，回不去原cst原slot，則要去unloading port
                    }
                    #endregion
                    #region Port source_port
                    source_port = ObjectManager.PortManager.GetPort(source_cassette.PortID);
                    if (source_port == null)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find OK greade curBcsJob Source Port({2})!",
                                                    "L1", MethodBase.GetCurrentMethod().Name, source_cassette.PortID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion

                        errMsg = string.Format("[{0}] can not find curBcsJob Source Port({1})!",
                                                MethodBase.GetCurrentMethod().Name, source_cassette.PortID);

                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                    #endregion
                    if (source_port.File.Type == ePortType.BothPort)
                    {
                        #region Decide target_stage,target_slot_no
                        RobotStage source_stage = ObjectManager.RobotStageManager.GetRobotStagebyPortNo(source_port.Data.NODENO, source_port.Data.PORTNO);
                        //找原cst原slot
                        if (source_stage != null)
                        {
                            if (source_stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST)
                            {
                                //check source slot是否為空
                                //if (source_stage.PortSlotInfos[from_slot_no - 1].slotGlassExist == "0")
                                //{
                                //    target_stage = source_stage;
                                //    target_slot_no = from_slot_no;
                                //    goto jumpRet; //回原cst原slot
                                //}
                                //else
                                //{
                                    #region  [DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] curBcsJob Return Port({2}) Slot({3}) is not Empty",
                                                                "L1", MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    }

                                    #endregion
                                    //Judge=NG,不管原cst原slot是否為空,都去unloading port(port mode=NG)
                                    goto jumpULD;
                                //}
                            }
                        }
                        #endregion
                    }
                }
                #endregion
            jumpULD:
                bool buffering = false;
                //其他，去mix unloading port
                #region 找出JobJudge與PortMode, UnloadingPortSetting相符的UnloadingPort
                List<Tuple<RobotStage, Port, Cassette>> unloading_ports = new List<Tuple<RobotStage, Port, Cassette>>();
                foreach (RobotStage stage in stage_list)
                {
                    Port port = ObjectManager.PortManager.GetPort(stage.Data.STAGEID);
                    Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID);
                    if (port.File.Type == ePortType.UnloadingPort && stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST && cst != null)
                    {
                        bool match = ForCF_CheckJobJudge_PortMode_UnloadingPortSetting(curBcsJob, port);
                        if (match)
                        {
                            unloading_ports.Add(Tuple.Create(stage, port, cst));
                        }
                    }
                }
                unloading_ports = unloading_ports.OrderBy(p => p, new UnloaderPortModeOrderByComparer(curBcsJob)).ToList();
                #endregion
                #region 尋找是否UnloadingPort上已經有相同FromCst的Job,找空Slot
                if (target_stage == null)
                {
                    foreach (var unloading_port in unloading_ports)
                    {
                        bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotCSTSeq == curBcsJob.CassetteSequenceNo);
                        //找到有相同FromCst的Job且相同Slot必須為空
                        if (found)
                        {
                            foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                            {
                                if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                {
                                    unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                    target_stage = unloading_port.Item1;
                                    target_slot_no = slot_no;
                                    break;
                                }
                            }
                        }
                        if (target_stage != null) break;
                    }
                }
                #endregion
                #region UnloadingPort上沒有相同FromCst的Job,找空CST
                if (target_stage == null)
                {
                    foreach (var unloading_port in unloading_ports)
                    {
                        bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotGlassExist == "1");
                        if (!found)
                        {
                            foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                            {
                                if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                {
                                    unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                    target_stage = unloading_port.Item1;
                                    target_slot_no = slot_no;
                                    break;
                                }
                            }
                        }
                        if (target_stage != null) break;
                    }
                }
                #endregion
                #region UnloadingPort上沒有空CST,找空Slot
                if (target_stage == null)
                {
                    foreach (var unloading_port in unloading_ports)
                    {
                        bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotGlassExist == "0");
                        if (found)
                        {
                            foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                            {
                                if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                {
                                    unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                    target_stage = unloading_port.Item1;
                                    target_slot_no = slot_no;
                                    break;
                                }
                            }
                        }
                        if (target_stage != null) break;
                    }
                }
                #endregion
                if (target_stage == null)
                {
                    if (Workbench.LineType != eLineType.CF.FCREW_TYPE1)
                    {
                        buffering = true;
                    }
                }
                //如果不能去unloading port,則要回原cst原slot先buffering
                //if (buffering)
                //{
                //    #region job is return to source cst & slot,need check job is already in cst
                //    if (source_port != null)
                //    {
                //        RobotStage source_stage = ObjectManager.RobotStageManager.GetRobotStagebyPortNo(source_port.Data.NODENO, source_port.Data.PORTNO);
                //        //先找出source_port的stage
                //        if (source_stage != null)
                //        {
                //            if (source_stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST)
                //            {
                //                //從stage找slot是否為空
                //                if (source_stage.PortSlotInfos[from_slot_no - 1].slotGlassExist == "0")
                //                {
                //                    target_stage = source_stage;
                //                    target_slot_no = from_slot_no;

                //                    //update job rcsbufferingflag=1	
                //                    lock (curBcsJob)
                //                    {
                //                        curBcsJob.CfSpecial.RCSBufferingFlag = "1";
                //                    }
                //                    ObjectManager.JobManager.EnqueueSave(curBcsJob);
                //                }
                //                else
                //                {
                //                    #region check job is already in cst
                //                    if (source_stage.PortSlotInfos[from_slot_no - 1].slotCSTSeq == curBcsJob.CassetteSequenceNo &&
                //                        source_stage.PortSlotInfos[from_slot_no - 1].slotJobSeq == curBcsJob.JobSequenceNo)
                //                    {

                //                        #region  [DebugLog]

                //                        if (IsShowDetialLog == true)
                //                        {

                //                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] curBcsJob do buffering but job is already in cst,Return Port({1}) Slot({2})",
                //                                                    "L1", MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                //                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                //                        }

                //                        #endregion

                //                        errMsg = string.Format("[{0}] curBcsJob do buffering but job is aready in cst,Return Port({1}) Slot({2})",
                //                                                MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                //                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OriginalCassetteOriginalSlotIsNotEmpty);
                //                        robotConText.SetReturnMessage(errMsg);
                //                        return false;

                //                    }
                //                    else
                //                    {
                //                        #region  [DebugLog]

                //                        if (IsShowDetialLog == true)
                //                        {

                //                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] curBcsJob do buffering,Return Port({2}) Slot({3}) is not Empty",
                //                                                    "L1", MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                //                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                //                        }

                //                        #endregion

                //                        errMsg = string.Format("[{0}] curBcsJob do buffering,Return Port({1}) Slot({2}) is not Empty",
                //                                                MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                //                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OriginalCassetteOriginalSlotIsNotEmpty);
                //                        robotConText.SetReturnMessage(errMsg);
                //                        return false;
                //                    }
                //                    #endregion
                //                }
                //            }
                //        }
                //    }
                //    #endregion
                //}
            jumpRet:
                if (target_stage != null)
                {
                    curDefineCmd.Cmd01_TargetPosition = int.Parse(target_stage.Data.STAGEID);
                    curDefineCmd.Cmd01_TargetSlotNo = target_slot_no;
                }
                else
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find target stage!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find target stage!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_UnloadDispatch_No_TargetStage);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }
        [UniAuto.UniBCS.OpiSpec.Help("OR0019")]
        public bool OrderBy_CELL_PDR_NormalModeUnloadDispatch(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;

                List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];
                //List<RobotStage> stage_list = beforeOrderByStageList == null ? new List<RobotStage>() : beforeOrderByStageList.ToList();

                #region [ Get curRobot Entity ]

                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

                //找不到 Robot 回NG
                if (curRobot == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region Job curBcsJob
                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];
                if (curBcsJob == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find curBcsJob Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion
                List<RobotStage> stage_list = beforeOrderByStageList == null ? new List<RobotStage>() : beforeOrderByStageList.OrderBy(s => s, new JobJudgePortModeCompare(curBcsJob)).ToList();
                #region DefineNormalRobotCmd curDefineCmd
                DefineNormalRobotCmd curDefineCmd = (DefineNormalRobotCmd)robotConText[eRobotContextParameter.DefineNormalRobotCmd];

                //找不到任何的Stage List時回NG
                if (curDefineCmd == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find DefineNormalRobotCmd!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find DefineNormalRobotCmd Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetDefineNormalRobotCmd);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region Get L2 Equipment
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);
                if (eqp == null)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot Node No!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot Node No!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_EQP_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                RobotStage target_stage = null;
                int target_slot_no = 0;
                int from_slot_no = int.Parse(curBcsJob.FromSlotNo);

                #region * Decide target_stage,target_slot_no
                bool buffering = false;
                if (curBcsJob.RobotWIP.CurSendOutJobJudge == "1")
                {
                    #region 找出JobJudge與PortMode, UnloadingPortSetting相符的UnloadingPort
                    List<Tuple<RobotStage, Port, Cassette>> unloading_ports = new List<Tuple<RobotStage, Port, Cassette>>();
                    foreach (RobotStage stage in stage_list)
                    {
                        Port port = ObjectManager.PortManager.GetPort(stage.Data.STAGEID);
                        Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID);
                        if (port.File.Type == ePortType.UnloadingPort && stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST && cst != null)
                        {
                            bool match = ForCF_CheckJobJudge_PortMode_UnloadingPortSetting(curBcsJob, port);
                            if (match)
                            {
                                unloading_ports.Add(Tuple.Create(stage, port, cst));
                            }
                        }
                    }
                    //unloading_ports = unloading_ports.OrderBy(p => p.Item3.LoadTime).ToList();
                    #endregion
                    #region 尋找是否UnloadingPort上已經有相同FromCst的Job,找空Slot
                    if (target_stage == null)
                    {
                        foreach (var unloading_port in unloading_ports)
                        {
                            bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotCSTSeq == curBcsJob.CassetteSequenceNo);
                            //找到有相同FromCst的Job且相同Slot必須為空
                            if (found)
                            {
                                foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                                {
                                    if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                    {
                                        unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                        target_stage = unloading_port.Item1;
                                        target_slot_no = slot_no;
                                        break;
                                    }
                                }
                            }
                            if (target_stage != null) break;
                        }
                    }
                    #endregion
                    #region UnloadingPort上沒有相同FromCst的Job,找空CST
                    if (target_stage == null)
                    {
                        foreach (var unloading_port in unloading_ports)
                        {
                            bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotGlassExist == "1");
                            if (!found)
                            {
                                foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                                {
                                    if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                    {
                                        unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                        target_stage = unloading_port.Item1;
                                        target_slot_no = slot_no;
                                        break;
                                    }
                                }
                            }
                            if (target_stage != null) break;
                        }
                    }
                    #endregion
                    #region UnloadingPort上沒有空CST,找空Slot
                    if (target_stage == null)
                    {
                        foreach (var unloading_port in unloading_ports)
                        {
                            bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotGlassExist == "0");
                            if (found)
                            {
                                foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                                {
                                    if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                    {
                                        unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                        target_stage = unloading_port.Item1;
                                        target_slot_no = slot_no;
                                        break;
                                    }
                                }
                            }
                            if (target_stage != null) break;
                        }
                    }
                    #endregion
                    if (target_stage == null)
                    {
                        buffering = true;
                    }
                }
                else
                {
                    #region 找出JobJudge與PortMode, UnloadingPortSetting相符的UnloadingPort
                    List<Tuple<RobotStage, Port, Cassette>> unloading_ports = new List<Tuple<RobotStage, Port, Cassette>>();
                    foreach (RobotStage stage in stage_list)
                    {
                        Port port = ObjectManager.PortManager.GetPort(stage.Data.STAGEID);
                        Cassette cst = ObjectManager.CassetteManager.GetCassette(port.File.CassetteID);
                        if (port.File.Type == ePortType.UnloadingPort && stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST && cst != null)
                        {
                            bool match = ForCF_CheckJobJudge_PortMode_UnloadingPortSetting(curBcsJob, port);
                            if (match)
                            {
                                unloading_ports.Add(Tuple.Create(stage, port, cst));
                            }
                        }
                    }
                    //unloading_ports = unloading_ports.OrderBy(p => p.Item3.LoadTime).ToList();
                    #endregion
                    #region 尋找是否UnloadingPort上已經有相同FromCst的Job,找空Slot
                    if (target_stage == null)
                    {
                        foreach (var unloading_port in unloading_ports)
                        {
                            bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotCSTSeq == curBcsJob.CassetteSequenceNo);
                            //找到有相同FromCst的Job且相同Slot必須為空
                            if (found)
                            {
                                foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                                {
                                    if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                    {
                                        unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                        target_stage = unloading_port.Item1;
                                        target_slot_no = slot_no;
                                        break;
                                    }
                                }
                            }
                            if (target_stage != null) break;
                        }
                    }
                    #endregion
                    #region UnloadingPort上沒有相同FromCst的Job,找空CST
                    if (target_stage == null)
                    {
                        foreach (var unloading_port in unloading_ports)
                        {
                            bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotGlassExist == "1");
                            if (!found)
                            {
                                foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                                {
                                    if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                    {
                                        unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                        target_stage = unloading_port.Item1;
                                        target_slot_no = slot_no;
                                        break;
                                    }
                                }
                            }
                            if (target_stage != null) break;
                        }
                    }
                    #endregion
                    #region UnloadingPort上沒有空CST,找空Slot
                    if (target_stage == null)
                    {
                        foreach (var unloading_port in unloading_ports)
                        {
                            bool found = unloading_port.Item1.PortSlotInfos.Exists(s => s.slotGlassExist == "0");
                            if (found)
                            {
                                foreach (int slot_no in unloading_port.Item1.curLDRQ_EmptySlotList.Keys)
                                {
                                    if (string.IsNullOrEmpty(unloading_port.Item1.curLDRQ_EmptySlotList[slot_no]))
                                    {
                                        unloading_port.Item1.curLDRQ_EmptySlotList[slot_no] = curBcsJob.JobKey;
                                        target_stage = unloading_port.Item1;
                                        target_slot_no = slot_no;
                                        break;
                                    }
                                }
                            }
                            if (target_stage != null) break;
                        }
                    }
                    #endregion
                    if (target_stage == null)
                    {
                        buffering = true;
                    }
                }
                //如果不能去unloading port,則要回原cst原slot先buffering
                if (buffering)
                {
                    #region Cassette source_cassette
                    Cassette source_cassette = ObjectManager.CassetteManager.GetCassette(curBcsJob.FromCstID);
                    if (source_cassette == null)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob Source Cassette({2})!",
                                                    "L1", MethodBase.GetCurrentMethod().Name, curBcsJob.FromCstID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion

                        errMsg = string.Format("[{0}] do buffering,can not find curBcsJob Source Cassette({1})!",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.FromCstID);

                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                    #endregion
                    #region Port source_port
                    Port source_port = ObjectManager.PortManager.GetPort(source_cassette.PortID);
                    if (source_port == null)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob Source Port({2})!",
                                                    "L1", MethodBase.GetCurrentMethod().Name, source_cassette.PortID);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion

                        errMsg = string.Format("[{0}] can not find curBcsJob Source Port({1})!",
                                                MethodBase.GetCurrentMethod().Name, source_cassette.PortID);

                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }
                    #endregion
                    #region job is return to source cst & slot,need check job is already in cst
                    RobotStage source_stage = ObjectManager.RobotStageManager.GetRobotStagebyPortNo(source_port.Data.NODENO, source_port.Data.PORTNO);
                    //先找出source_port的stage
                    if (source_stage != null)
                    {
                        if (source_stage.File.CurStageStatus != eRobotStageStatus.NO_REQUEST)
                        {
                            //從stage找slot是否為空
                            if (source_stage.PortSlotInfos[from_slot_no - 1].slotGlassExist == "0")
                            {
                                target_stage = source_stage;
                                target_slot_no = from_slot_no;

                                //update job rcsbufferingflag=1	
                                lock (curBcsJob)
                                {
                                    curBcsJob.CfSpecial.RCSBufferingFlag = "1";
                                }
                                ObjectManager.JobManager.EnqueueSave(curBcsJob);
                            }
                            else
                            {
                                #region check job is already in cst
                                if (source_stage.PortSlotInfos[from_slot_no - 1].slotCSTSeq == curBcsJob.CassetteSequenceNo &&
                                    source_stage.PortSlotInfos[from_slot_no - 1].slotJobSeq == curBcsJob.JobSequenceNo)
                                {

                                    #region  [DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] curBcsJob do buffering but job is already in cst,Return Port({1}) Slot({2})",
                                                                "L1", MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    }

                                    #endregion

                                    errMsg = string.Format("[{0}] curBcsJob do buffering but job is aready in cst,Return Port({1}) Slot({2})",
                                                            MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OriginalCassetteOriginalSlotIsNotEmpty);
                                    robotConText.SetReturnMessage(errMsg);
                                    return false;

                                }
                                else
                                {
                                    #region  [DebugLog]

                                    if (IsShowDetialLog == true)
                                    {

                                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] curBcsJob do buffering,Return Port({2}) Slot({3}) is not Empty",
                                                                "L1", MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    }

                                    #endregion

                                    errMsg = string.Format("[{0}] curBcsJob do buffering,Return Port({1}) Slot({2}) is not Empty",
                                                            MethodBase.GetCurrentMethod().Name, source_port.Data.PORTID, from_slot_no);

                                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OriginalCassetteOriginalSlotIsNotEmpty);
                                    robotConText.SetReturnMessage(errMsg);
                                    return false;
                                }
                                #endregion
                            }
                        }
                    }
                    #endregion
                }
                #endregion

                if (target_stage != null)
                {
                    curDefineCmd.Cmd01_TargetPosition = int.Parse(target_stage.Data.STAGEID);
                    curDefineCmd.Cmd01_TargetSlotNo = target_slot_no;
                }
                else
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find target stage!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find target stage!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_UnloadDispatch_No_TargetStage);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        /// <summary>相同JobData EQPFlag DCRandSorterFlag收在同一Unloader Casstte
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("OR0020")]
        public bool OrderBy_ULDPortDipatchRuleByJobEQPFlag(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;
                string curBcsJobEqpFlag_DCRandSorterFlag = string.Empty;

                IOrderedEnumerable<RobotStage> afterOrderByResultInfo;

                #region [ Get curRobot Entity ]

                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

                //找不到 Robot 回NG
                if (curRobot == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Robot 所屬Line Entity ]

                Line robotLine = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);

                if (robotLine == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) can not find Line Entity by LineID({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.LINEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) can not Get Line Entity!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get curBcsJob Entity and Decode EQPFlag ]

                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];

                //找不到 Job 回NG
                if (curBcsJob == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not Get JobInfo!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) can not Get JobInfo!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                IDictionary<string, string> curBcsJobEqpFlag = ObjectManager.SubJobDataManager.Decode(curBcsJob.EQPFlag, eJOBDATA.EQPFlag);
                //0:No Flag Glass,1:DCR Flag Glass,2:Sorter Flag Glass,3:DCR and Sorter Flag Glass
                curBcsJobEqpFlag_DCRandSorterFlag = curBcsJobEqpFlag["DCRandSorterFlag"];

                #endregion

                #region [ Get After 1st OrderBy flag ]

                bool after1stOrderByFlag = false;

                try
                {
                    after1stOrderByFlag = (bool)robotConText[eRobotContextParameter.Afrer1stOrderByCheckFlag];
                }
                catch (Exception)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] can not Set Afrer1stOrderByCheckFlag!",
                                                "L2");

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Set Is2ndCmdCheckFlag!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetAfter1stOrderByFlag_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ 取得之前OrderBy的結果資訊 ]

                //List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                IOrderedEnumerable<RobotStage> beforeOrderByResultInfo = (IOrderedEnumerable<RobotStage>)robotConText[eRobotContextParameter.AfrerOrderByResultInfo];

                //找不到之前Orderby的結果時回NG
                if (beforeOrderByResultInfo == null)
                {
                    if (after1stOrderByFlag == true)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) can not find before OrderBy Stage List!(can't find Sorter Flag Match port)",
                                                    "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not find before OrderBy Stage List Info!(can't find Sorter Flag Match port)",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Null);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }

                }
                //StageList is empty 回NG
                else if (beforeOrderByResultInfo.ToList().Count == 0)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) before OrderBy Stage List is empty!(can't find Sorter Flag Match port)",
                                                "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) before OrderBy Stage List is empty!(can't find Sorter Flag Match port)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Empty);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get OrderBy Action ]

                string orderbyAction = (string)robotConText[eRobotContextParameter.OrderByAction];

                if (orderbyAction == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find OrderByAction!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find OrderByAction!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OrderByAction_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ 開始排序 ]
                if (orderbyAction.Trim() == eRobotCommonConst.DB_ORDER_BY_ASC)
                {
                    if (after1stOrderByFlag == false)
                    {
                        #region [ 尚未做過任何Order時則Get Want to Order by Stage List ]

                        List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                        //找不到任何的Stage List時回NG
                        if (beforeOrderByStageList == null)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) can not find before OrderBy Stage List!(can't find Sorter Flag Match port)",
                                                        "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not find before OrderBy Stage List Info!(can't find Sorter Flag Match port)",
                                                    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                            robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Null);
                            robotConText.SetReturnMessage(errMsg);

                            return false;


                        }

                        //StageList is empty 回NG
                        if (beforeOrderByStageList.ToList().Count == 0)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) before OrderBy Stage List is empty!(can't find Sorter Flag Match port)",
                                                        "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) before OrderBy Stage List is empty!(can't find Sorter Flag Match port)",
                                                    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                            robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Empty);
                            robotConText.SetReturnMessage(errMsg);

                            return false;
                        }

                        #endregion
                        afterOrderByResultInfo = beforeOrderByStageList.OrderBy(s => s, new OrderByJobDCRandSorterFlag(curBcsJobEqpFlag_DCRandSorterFlag));
                    }
                    else
                    {
                        afterOrderByResultInfo = beforeOrderByResultInfo.ThenBy(s => s, new OrderByJobDCRandSorterFlag(curBcsJobEqpFlag_DCRandSorterFlag));
                    }
                }
                else if (orderbyAction.Trim() == eRobotCommonConst.DB_ORDER_BY_DESC)
                {
                    if (after1stOrderByFlag == false)
                    {
                        #region [ 尚未做過任何Order時則Get Want to Order by Stage List ]

                        List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                        //找不到任何的Stage List時回NG
                        if (beforeOrderByStageList == null)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) can not find before OrderBy Stage List!(can't find Sorter Flag Match port)",
                                                        "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not find before OrderBy Stage List Info!(can't find Sorter Flag Match port)",
                                                    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                            robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Null);
                            robotConText.SetReturnMessage(errMsg);

                            return false;


                        }

                        //StageList is empty 回NG
                        if (beforeOrderByStageList.Count == 0)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) before OrderBy Stage List is empty!(can't find Sorter Flag Match port)",
                                                        "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) before OrderBy Stage List is empty!(can't find Sorter Flag Match port)",
                                                    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                            robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Empty);
                            robotConText.SetReturnMessage(errMsg);

                            return false;
                        }

                        #endregion
                        afterOrderByResultInfo = beforeOrderByStageList.OrderByDescending(s => s, new OrderByJobDCRandSorterFlag(curBcsJobEqpFlag_DCRandSorterFlag));
                    }
                    else
                    {
                        afterOrderByResultInfo = beforeOrderByResultInfo.ThenByDescending(s => s, new OrderByJobDCRandSorterFlag(curBcsJobEqpFlag_DCRandSorterFlag));
                    }
                }
                else
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] find OrderByAction({2}) is illgal!", "L2", MethodBase.GetCurrentMethod().Name, orderbyAction);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion
                    errMsg = string.Format("[{0}] find OrderByAction({2}) is illgal!", MethodBase.GetCurrentMethod().Name, orderbyAction);
                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OrderByAction_Is_illgal);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                #endregion

                //Update Order by結果. ConText需要回傳
                //beforeOrderByResultInfo = afterOrderByStageList;
                robotConText.AddParameter(eRobotContextParameter.AfrerOrderByResultInfo, afterOrderByResultInfo);

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        /// <summary>依Cassette Start Time排序
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("OR0021")]
        public bool OrderBy_PortCassetteStartTime(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;

                IOrderedEnumerable<RobotStage> afterOrderByResultInfo;

                #region [ Get curRobot Entity ]

                Robot curRobot = (Robot)robotConText[eRobotContextParameter.CurRobotEntity];

                //找不到 Robot 回NG
                if (curRobot == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Robot!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Get Robot!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Robot 所屬Line Entity ]

                Line robotLine = ObjectManager.LineManager.GetLine(curRobot.Data.LINEID);

                if (robotLine == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [BCS <- RBM] Robot({1}) can not find Line Entity by LineID({2})!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRobot.Data.LINEID);
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) can not Get Line Entity!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_Get_curRobot_Line_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get After 1st OrderBy flag ]

                bool after1stOrderByFlag = false;

                try
                {
                    after1stOrderByFlag = (bool)robotConText[eRobotContextParameter.Afrer1stOrderByCheckFlag];
                }
                catch (Exception)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] can not Set Afrer1stOrderByCheckFlag!",
                                                "L2");

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Set Is2ndCmdCheckFlag!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetAfter1stOrderByFlag_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ 取得之前OrderBy的結果資訊 ]

                //List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                IOrderedEnumerable<RobotStage> beforeOrderByResultInfo = (IOrderedEnumerable<RobotStage>)robotConText[eRobotContextParameter.AfrerOrderByResultInfo];

                //找不到之前Orderby的結果時回NG
                if (beforeOrderByResultInfo == null)
                {
                    if (after1stOrderByFlag == true)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find before OrderBy Stage List!",
                                                    "L2", MethodBase.GetCurrentMethod().Name);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion

                        errMsg = string.Format("[{0}] can not find before OrderBy Stage List Info!",
                                                MethodBase.GetCurrentMethod().Name);

                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Null);
                        robotConText.SetReturnMessage(errMsg);

                        return false;
                    }

                }
                //StageList is empty 回NG
                else if (beforeOrderByResultInfo.ToList().Count == 0)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] before OrderBy Stage List is empty!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] before OrderBy Stage List is empty!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Empty);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get OrderBy Action ]

                string orderbyAction = (string)robotConText[eRobotContextParameter.OrderByAction];

                if (orderbyAction == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find OrderByAction!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find OrderByAction!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OrderByAction_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ 開始排序 ]
                if (orderbyAction.Trim() == eRobotCommonConst.DB_ORDER_BY_ASC)
                {
                    if (after1stOrderByFlag == false)
                    {
                        #region [ 尚未做過任何Order時則Get Want to Order by Stage List ]

                        List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                        //找不到任何的Stage List時回NG
                        if (beforeOrderByStageList == null)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find before OrderBy Stage List!",
                                                        "L2", MethodBase.GetCurrentMethod().Name);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                            errMsg = string.Format("[{0}] can not find before OrderBy Stage List Info!",
                                                    MethodBase.GetCurrentMethod().Name);

                            robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Null);
                            robotConText.SetReturnMessage(errMsg);

                            return false;


                        }

                        //StageList is empty 回NG
                        if (beforeOrderByStageList.ToList().Count == 0)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] before OrderBy Stage List is empty!",
                                                        "L2", MethodBase.GetCurrentMethod().Name);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                            errMsg = string.Format("[{0}] before OrderBy Stage List is empty!",
                                                    MethodBase.GetCurrentMethod().Name);

                            robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Empty);
                            robotConText.SetReturnMessage(errMsg);

                            return false;
                        }

                        #endregion
                        afterOrderByResultInfo = beforeOrderByStageList.OrderBy(s => s, new OrderByCassetteStartTime());
                    }
                    else
                    {
                        afterOrderByResultInfo = beforeOrderByResultInfo.ThenBy(s => s, new OrderByCassetteStartTime());
                    }
                }
                else if (orderbyAction.Trim() == eRobotCommonConst.DB_ORDER_BY_DESC)
                {
                    if (after1stOrderByFlag == false)
                    {
                        #region [ 尚未做過任何Order時則Get Want to Order by Stage List ]

                        List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                        //找不到任何的Stage List時回NG
                        if (beforeOrderByStageList == null)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find before OrderBy Stage List!",
                                                        "L2", MethodBase.GetCurrentMethod().Name);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                            errMsg = string.Format("[{0}] can not find before OrderBy Stage List Info!",
                                                    MethodBase.GetCurrentMethod().Name);

                            robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Null);
                            robotConText.SetReturnMessage(errMsg);

                            return false;


                        }

                        //StageList is empty 回NG
                        if (beforeOrderByStageList.Count == 0)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] before OrderBy Stage List is empty!",
                                                        "L2", MethodBase.GetCurrentMethod().Name);

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                            errMsg = string.Format("[{0}] before OrderBy Stage List is empty!",
                                                    MethodBase.GetCurrentMethod().Name);

                            robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Empty);
                            robotConText.SetReturnMessage(errMsg);

                            return false;
                        }

                        #endregion
                        afterOrderByResultInfo = beforeOrderByStageList.OrderByDescending(s => s, new OrderByCassetteStartTime());
                    }
                    else
                    {
                        afterOrderByResultInfo = beforeOrderByResultInfo.ThenByDescending(s => s, new OrderByCassetteStartTime());
                    }
                }
                else
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] find OrderByAction({2}) is illgal!", "L2", MethodBase.GetCurrentMethod().Name, orderbyAction);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion
                    errMsg = string.Format("[{0}] find OrderByAction({2}) is illgal!", MethodBase.GetCurrentMethod().Name, orderbyAction);
                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OrderByAction_Is_illgal);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }

                #endregion

                //Update Order by結果. ConText需要回傳
                //beforeOrderByResultInfo = afterOrderByStageList;
                robotConText.AddParameter(eRobotContextParameter.AfrerOrderByResultInfo, afterOrderByResultInfo);

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);
                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }
        [UniAuto.UniBCS.OpiSpec.Help("OR0023")]
        public bool OrderBy_JobJudgePortModeRule(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;
                //List<RobotStage> afterOrderByStageList = new List<RobotStage>();
                IOrderedEnumerable<RobotStage> afterOrderByResultInfo;

                #region [ Get After 1st OrderBy flag ]

                bool after1stOrderByFlag = false;

                try
                {
                    after1stOrderByFlag = (bool)robotConText[eRobotContextParameter.Afrer1stOrderByCheckFlag];
                }
                catch (Exception)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] can not Set Afrer1stOrderByCheckFlag!",
                                                "L2");

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not Set Is2ndCmdCheckFlag!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetAfter1stOrderByFlag_Is_Fail);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion
                #region Job curBcsJob
                Job curBcsJob = (Job)robotConText[eRobotContextParameter.CurJobEntity];
                if (curBcsJob == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find curBcsJob Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region [ 取得之前OrderBy的結果資訊 ]

                //List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                IOrderedEnumerable<RobotStage> beforeOrderByResultInfo = (IOrderedEnumerable<RobotStage>)robotConText[eRobotContextParameter.AfrerOrderByResultInfo];

                //找不到之前Orderby的結果時回NG
                if (beforeOrderByResultInfo == null)
                {
                    if (after1stOrderByFlag == true)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) can not find before OrderBy Stage List!(Can't find Job Judge is Match port mode)",
                                                    "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion

                        errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not find before OrderBy Stage List!(Can't find Job Judge is Match port mode)",
                                                MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Null);
                        robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);

                        return false;
                    }

                }
                //StageList is empty 回NG
                else if (beforeOrderByResultInfo.ToList().Count == 0)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) before OrderBy Stage List is empty!(Can't find Job Judge is Match port mode)",
                                                "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) before OrderBy Stage List is empty!(Can't find Job Judge is Match port mode)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Empty);
                    robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);

                    return false;
                }

                #endregion

                #region [ Get OrderBy Action ]

                string orderbyAction = (string)robotConText[eRobotContextParameter.OrderByAction];

                if (orderbyAction == null)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find OrderByAction!",
                                                "L2", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find OrderByAction!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OrderByAction_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ 開始排序 ]

                //根據Stage的LDRQ_Cst Status Priority設定來排序(1,2,3,4 數字越小優先級越高)
                if (orderbyAction.Trim() == eRobotCommonConst.DB_ORDER_BY_ASC)
                {

                    //afterOrderByStageList = beforeOrderByStageList.OrderBy(s => s.File.LDRQ_CstStatusPriority).ToList();
                    if (after1stOrderByFlag == false)
                    {
                        #region [ 尚未做過任何Order時則Get Want to Order by Stage List ]

                        List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                        //找不到任何的Stage List時回NG
                        if (beforeOrderByStageList == null)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) can not find before OrderBy Stage List!(Can't find Job Judge is Match port mode)",
                                                        "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not find before OrderBy Stage List Info!(Can't find Job Judge is Match port mode)",
                                                    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                            robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Null);
                            robotConText.SetReturnMessage(errMsg);

                            return false;


                        }

                        //StageList is empty 回NG
                        if (beforeOrderByStageList.Count == 0)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) before OrderBy Stage List is empty!(Can't find Job Judge is Match port mode)",
                                                        "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) before OrderBy Stage List is empty!(Can't find Job Judge is Match port mode)",
                                                    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                            robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Empty);
                            robotConText.SetReturnMessage(errMsg);

                            return false;
                        }

                        #endregion

                        afterOrderByResultInfo = beforeOrderByStageList.OrderBy(s => s, new JobJudgePortModeCompare(curBcsJob));

                    }
                    else
                    {
                        afterOrderByResultInfo = beforeOrderByResultInfo.ThenBy(s => s, new JobJudgePortModeCompare(curBcsJob));
                    }

                }
                else if (orderbyAction.Trim() == eRobotCommonConst.DB_ORDER_BY_DESC)
                {

                    //afterOrderByStageList = beforeOrderByStageList.OrderByDescending(s => s.File.LDRQ_CstStatusPriority).ToList();
                    if (after1stOrderByFlag == false)
                    {
                        #region [ 尚未做過任何Order時則Get Want to Order by Stage List ]

                        List<RobotStage> beforeOrderByStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                        //找不到任何的Stage List時回NG
                        if (beforeOrderByStageList == null)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) can not find before OrderBy Stage List!(Can't find Job Judge is Match port mode)",
                                                        "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not find before OrderBy Stage List Info!(Can't find Job Judge is Match port mode)",
                                                    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                            robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Null);
                            robotConText.SetReturnMessage(errMsg);

                            return false;


                        }

                        //StageList is empty 回NG
                        if (beforeOrderByStageList.Count == 0)
                        {

                            #region  [DebugLog]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) before OrderBy Stage List is empty!(Can't find Job Judge is Match port mode)",
                                                        "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            }

                            #endregion

                            errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) before OrderBy Stage List is empty!(Can't find Job Judge is Match port mode)",
                                                    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                        curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                            robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Empty);
                            robotConText.SetReturnMessage(errMsg);

                            return false;
                        }

                        #endregion

                        afterOrderByResultInfo = beforeOrderByStageList.OrderByDescending(s => s, new JobJudgePortModeCompare(curBcsJob));

                    }
                    else
                    {
                        afterOrderByResultInfo = beforeOrderByResultInfo.ThenByDescending(s => s, new JobJudgePortModeCompare(curBcsJob));
                    }
                }
                else
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] find OrderByAction({2}) is illgal!",
                                                "L2", MethodBase.GetCurrentMethod().Name, orderbyAction);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] find OrderByAction({2}) is illgal!",
                                            MethodBase.GetCurrentMethod().Name, orderbyAction);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OrderByAction_Is_illgal);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                //Update Order by結果. ConText需要回傳
                //beforeOrderByResultInfo = afterOrderByStageList;
                robotConText.AddParameter(eRobotContextParameter.AfrerOrderByResultInfo, afterOrderByResultInfo);

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);

                return true;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }

        }
    }
}

#region [Job Judge]
//0：Inspection Skip or No Judge
//1：OK
//2：NG - Insp. Result 
//3：RW - Required Rework
//4：PD –Pending judge
//5：RP – Required Repair
//6：IR–Ink Repair
//7：Other
//8：RV –PI Reivew"
#endregion
#region [Unloading Port Mode]
//1：TFT Mode
//2：CF Mode
//3：Dummy Mode
//4：MQC Mode
//5：HT Mode
//6：LT Mode
//7：ENG Mode
//8：IGZO Mode
//9：ILC Mode
//10：FLC Mode
//11：Through Dummy Mode
//12：Thickness Dummy Mode
//13：UV Mask Mode
//14：By Grade Mode 
//15：OK Mode
//16：NG Mode
//17：MIX Mode
//18：EMP Mode (Empty Cassette)
//19：RW Mode
//20：Mismatch Mode
//21: PD Mode
//22: IR Mode
//23: RP Mode
//24: Re-Judge Mode
#endregion