using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniRCS.Core;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;
using System.Reflection;

namespace UniAuto.UniRCS.CSOT.t3.Service
{
    /// <summary>
    /// 根據Port Mode排序. Job Judge為OK時Port Mode OK優先, Port Mode EMP次之; Job Judge為NG時Port Mode NG優先, Port Mode EMP次之; 
    /// </summary>
    public class JobJudgePortModeCompare : IComparer<RobotStage>
    {
        private Job curBcsJob = null;
        public JobJudgePortModeCompare(Job curBcsJob)
        {
            this.curBcsJob = curBcsJob;
        }
        public int Compare(RobotStage x, RobotStage y)
        {
            // x and y are equal, return 0;
            // x is greater, return 1;
            // y is greater, return -1;
            Port port_x = ObjectManager.PortManager.GetPort(x.Data.LINEID, x.Data.NODENO, x.Data.STAGEIDBYNODE);
            Port port_y = ObjectManager.PortManager.GetPort(y.Data.LINEID, y.Data.NODENO, y.Data.STAGEIDBYNODE);
            if (port_x == null && port_y == null)
                return 0;
            else if (port_x == null)
                return 1;// OrderBy遞增排序, 較小者排優先, 故 null 比較大排後面
            else if (port_y == null)
                return -1;// OrderBy遞增排序, 較小者排優先, 故 null 比較大排後面

            string job_judge_ok = "1";
            ePortMode priority_port_mode = ePortMode.Unknown;
            if (curBcsJob.RobotWIP.CurSendOutJobJudge == job_judge_ok)
                priority_port_mode = ePortMode.OK;
            else
                priority_port_mode = ePortMode.NG;

            if ((port_x.File.Mode == priority_port_mode && port_y.File.Mode == priority_port_mode) ||
                    (port_x.File.Mode == ePortMode.EMPMode && port_y.File.Mode == ePortMode.EMPMode))
                return 0;
            else if (port_x.File.Mode == priority_port_mode)
                return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=OK優先, 因此return -1讓 Y排後面
            else if (port_y.File.Mode == priority_port_mode)
                return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=OK優先, 因此return 1讓 X排後面
            else if (port_x.File.Mode == ePortMode.EMPMode)
                return -1;// OrderBy遞增排序, 較小者排優先, X PortMode=EMP優先, 因此return -1讓 Y排後面
            else if (port_y.File.Mode == ePortMode.EMPMode)
                return 1;// OrderBy遞增排序, 較小者排優先, Y PortMode=EMP優先, 因此return 1讓 X排後面
            else
                return 0;
        }
    }

    /// <summary>
    /// 不排序, 只為了取得IOrderedEnumerable
    /// </summary>
    public class NoCompare : IComparer<RobotStage>
    {
        public NoCompare()
        {
        }
        public int Compare(RobotStage x, RobotStage y)
        {
            // x and y are equal, return 0;
            // x is greater, return 1;
            // y is greater, return -1;
            return 0;
        }
    }

    [UniAuto.UniBCS.OpiSpec.Help("JobOrderByService")]
    public partial class JobOrderByService : AbstractRobotService
    {
        public override bool Init()
        {
            return true;
        }

        //All Job Route Step OrderBy Function List [ Method Name = "OrderBy" + "_" + "Condition" EX:"OrderBy_CSTOnPortTimeASC" ]==============================================================
        //OrderBy Funckey = "OR" + XXXX(序列號)

        /// <summary> 根據Stage的Priority設定來排序(1,2,3,4 數字越大優先級越高) 
        ///
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("OR0001")]
        public bool OrderBy_StagePriority(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;
                //List<RobotStage> afterOrderByStageList= new List<RobotStage>();
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
                                                    "L1", MethodBase.GetCurrentMethod().Name);

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
                                                "L1", MethodBase.GetCurrentMethod().Name);

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
                                                "L1", MethodBase.GetCurrentMethod().Name);

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

                //根據Stage的Priority設定來排序(1,2,3,4 數字越大優先級越高)
                if (orderbyAction.Trim() == eRobotCommonConst.DB_ORDER_BY_ASC)
                {
                    //afterOrderByStageList = beforeOrderByStageInfo.OrderBy(s => s.Data.PRIORITY).ToList();
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
                                                        "L1", MethodBase.GetCurrentMethod().Name);

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
                                                        "L1", MethodBase.GetCurrentMethod().Name);

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

                        afterOrderByResultInfo = beforeOrderByStageList.OrderBy(s => s.Data.PRIORITY);

                    }
                    else
                    {
                        afterOrderByResultInfo = beforeOrderByResultInfo.ThenBy(s => s.Data.PRIORITY);
                    }

                }
                else if (orderbyAction.Trim() == eRobotCommonConst.DB_ORDER_BY_DESC)
                {

                    //afterOrderByStageList = beforeOrderByStageInfo.OrderByDescending(s => s.Data.PRIORITY).ToList();
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
                                                        "L1", MethodBase.GetCurrentMethod().Name);

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
                                                        "L1", MethodBase.GetCurrentMethod().Name);

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

                        afterOrderByResultInfo = beforeOrderByStageList.OrderByDescending(s => s.Data.PRIORITY);

                    }
                    else
                    {
                        afterOrderByResultInfo = beforeOrderByResultInfo.ThenByDescending(s => s.Data.PRIORITY);
                    }

                }
                else
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] find OrderByAction({2}) is illgal!",
                                                "L1", MethodBase.GetCurrentMethod().Name, orderbyAction);

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

        /// <summary> 只有Stage Type Port的Status狀態需要重新排序 InProcess > WaitForProcess, Other Stage Type 則不需重新排序(1,2,3,4 數字越小優先級越高)
        ///
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("OR0002")]
        public bool OrderBy_StageLDRQ_CSTStatusPriority(IRobotContext robotConText)
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
                                                    "L1", MethodBase.GetCurrentMethod().Name);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        }

                        #endregion

                        errMsg = string.Format("[{0}] can not find before OrderBy Stage List!",
                                                MethodBase.GetCurrentMethod().Name);

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

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] before OrderBy Stage List is empty!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] before OrderBy Stage List is empty!",
                                            MethodBase.GetCurrentMethod().Name);

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
                                                "L1", MethodBase.GetCurrentMethod().Name);

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

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find before OrderBy Stage List!",
                                                        "L1", MethodBase.GetCurrentMethod().Name);

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
                                                        "L1", MethodBase.GetCurrentMethod().Name);

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

                        afterOrderByResultInfo = beforeOrderByStageList.OrderBy(s => s.File.LDRQ_CstStatusPriority);

                    }
                    else
                    {
                        afterOrderByResultInfo = beforeOrderByResultInfo.ThenBy(s => s.File.LDRQ_CstStatusPriority);
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

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find before OrderBy Stage List!",
                                                        "L1", MethodBase.GetCurrentMethod().Name);

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
                                                        "L1", MethodBase.GetCurrentMethod().Name);

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

                        afterOrderByResultInfo = beforeOrderByStageList.OrderByDescending(s => s.File.LDRQ_CstStatusPriority);

                    }
                    else
                    {
                        afterOrderByResultInfo = beforeOrderByResultInfo.ThenByDescending(s => s.File.LDRQ_CstStatusPriority);
                    }
                }
                else
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] find OrderByAction({2}) is illgal!",
                                                "L1", MethodBase.GetCurrentMethod().Name, orderbyAction);

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

        /// <summary>Changer Plan時放回Plan指定的Port Slot, 只能在PUT to Port時使用
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("OR0003")]
        public bool OrderBy_ChangerPlanTargetSlot(IRobotContext robotConText)
        {
            try
            {
                if (!IsAllLineSameIndexerMode(eINDEXER_OPERATION_MODE.CHANGER_MODE))
                {
                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);
                    return true;
                }

                string errMsg = string.Empty;
                string strlog = string.Empty;

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

                #region List<RobotStage> stepCanUseStageList

                List<RobotStage> stepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                //找不到任何的Stage List時回NG
                if (stepCanUseStageList == null || stepCanUseStageList.Count <= 0)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find before OrderBy Stage List!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find Step Can Use Stage List Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Null);
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

                #region IList<SLOTPLAN> slot_plans
                string plan_id = string.Empty;
                IList<SLOTPLAN> slot_plans = ObjectManager.PlanManager.GetProductPlans(out plan_id);
                if (slot_plans == null || slot_plans.Count <= 0)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) can not find Changer Plan!(please check plan has setting)",
                                                "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not find Changer Plan Info!(please check plan has setting)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetChangerPlan);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region SLOTPLAN job_slot_plan
                SLOTPLAN job_slot_plan = GetSlotPlanByJob(slot_plans, curBcsJob);
                if (job_slot_plan == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNo({5}) can not find Changer Plan!(please check 1.Job.FromCstID = plan.SOURCE_CASSETTE_ID 2.Offline =>plan.SLOTNO > 0 & Job.FromSlotNo = plan.SLOTNO 3.Online =>plan.SLOTNO = 0 & plan.PRODUCT_NAME = Job.MesProduct.PRODUCTNAME)",
                                                "L1", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                                curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not find Changer Plan Info!(please check 1.Job.FromCstID = plan.SOURCE_CASSETTE_ID 2.Offline =>plan.SLOTNO > 0 & Job.FromSlotNo = plan.SLOTNO 3.Online =>plan.SLOTNO = 0 & plan.PRODUCT_NAME = Job.MesProduct.PRODUCTNAME)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.RobotWIP.CurRouteID,
                                            curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetChangerPlan);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region Port target_port
                Port target_port = null;
                RobotStage target_stage = null;
                int target_slot_no = 0;
                if (!int.TryParse(job_slot_plan.TARGET_SLOTNO, out target_slot_no)) target_slot_no = 0;

                foreach (RobotStage stage in stepCanUseStageList)
                {
                    if (stage.Data.STAGETYPE != eStageType.PORT)
                        continue;

                    Port port = ObjectManager.PortManager.GetPort(stage.Data.LINEID, stage.Data.NODENO, stage.Data.STAGEIDBYNODE);
                    if (port != null && port.File.CassetteID == job_slot_plan.TARGET_CASSETTE_ID)
                    {
                        target_port = port;
                        target_stage = stage;
                        break;
                    }
                }

                if (target_port == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) Changer Plan Target CassetteID({6}), cannot find Target Port.(please check plan target CassetteID is Exist)",
                                                "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), job_slot_plan.TARGET_CASSETTE_ID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) Changer Plan Target CassetteID({5}), cannot find Target Port.(please check plan target CassetteID is Exist)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), job_slot_plan.TARGET_CASSETTE_ID);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_ChangerPlanCassetteID);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                curDefineCmd.Cmd01_TargetPosition = int.Parse(target_stage.Data.STAGEID);
                if (target_slot_no > 0)
                {
                    if (target_port.File.ArrayJobExistenceSlot == null || target_port.File.ArrayJobExistenceSlot.Length < target_slot_no)
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            if (target_port.File.ArrayJobExistenceSlot == null)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Stage({2}) Port.File.ArrayJobExistenceSlot is null",
                                                        "L1", MethodBase.GetCurrentMethod().Name, target_stage.Data.STAGEIDBYNODE);
                            }
                            else
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Stage({2}) Port.File.ArrayJobExistenceSlot.Length({3}) Target Slot No({4})!",
                                                        "L1", MethodBase.GetCurrentMethod().Name, target_stage.Data.STAGEIDBYNODE, target_port.File.ArrayJobExistenceSlot.Length, target_slot_no);
                            }

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_JobExistenceSlotMismatch);
                        robotConText.SetReturnMessage(strlog);
                        return false;
                    }
                    if (target_port.File.ArrayJobExistenceSlot[target_slot_no - 1])
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Stage({2}) Slot({3}) Job Already Exist",
                                                    "L1", MethodBase.GetCurrentMethod().Name, target_stage.Data.STAGEIDBYNODE, target_slot_no);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Stage({1}) Port({2}) CassetteID({3}) Slot({4}) Job Already Exist in Target Slot!",
                                                MethodBase.GetCurrentMethod().Name, target_stage.Data.STAGEIDBYNODE, target_port.Data.PORTNO, target_port.File.CassetteID, target_slot_no);

                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_JobAlreadyExistInTargetSlot);
                        robotConText.SetReturnMessage(errMsg);
                        return false;
                    }
                    curDefineCmd.Cmd01_TargetSlotNo = target_slot_no;
                }
                else
                {
                    //任意空Slot
                    foreach (int curSlotNo in target_stage.curLDRQ_EmptySlotList.Keys)
                    {
                        if (string.IsNullOrEmpty(target_stage.curLDRQ_EmptySlotList[curSlotNo]))
                        {
                            target_stage.curLDRQ_EmptySlotList[curSlotNo] = curBcsJob.JobKey;
                            target_slot_no = curSlotNo;
                            break;
                        }
                    }
                    curDefineCmd.Cmd01_TargetSlotNo = target_slot_no;
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

        /// <summary> Job的來源Port為BothPort時, Job需放回原Cassette原Slot
        ///
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("OR0004")]
        public bool OrderBy_CassetteToCassette(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;

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

                #region List<RobotStage> stage_list
                IOrderedEnumerable<RobotStage> afterOrderByResultInfo = (IOrderedEnumerable<RobotStage>)robotConText[eRobotContextParameter.AfrerOrderByResultInfo];

                //找不到之前Orderby的結果時回NG
                if (afterOrderByResultInfo == null)
                {
                    if (after1stOrderByFlag == true)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find before OrderBy Stage List!",
                                                    "L1", MethodBase.GetCurrentMethod().Name);

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
                else if (afterOrderByResultInfo.ToList().Count == 0)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] before OrderBy Stage List is empty!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] before OrderBy Stage List is empty!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Empty);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                List<RobotStage> stage_list = new List<RobotStage>();
                if (after1stOrderByFlag == true && afterOrderByResultInfo != null && afterOrderByResultInfo.ToList().Count > 0)
                {
                    stage_list = afterOrderByResultInfo.ToList();
                }
                else
                    stage_list = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];
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

                #region Cassette return_cassette
                Cassette return_cassette = ObjectManager.CassetteManager.GetCassette(curBcsJob.FromCstID);
                if (return_cassette == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob Source Cassette({2})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curBcsJob.FromCstID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find curBcsJob Return Cassette({1})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.FromCstID);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region Port return_port
                Port return_port = ObjectManager.PortManager.GetPort(return_cassette.PortID);
                if (return_port == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob Return Port({2})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, return_cassette.PortID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find curBcsJob Return Port({1})!",
                                            MethodBase.GetCurrentMethod().Name, return_cassette.PortID);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                RobotStage target_stage = null;
                int target_slot_no = 0;
                int from_slot_no = int.Parse(curBcsJob.FromSlotNo);
                //return_port是BothPort, 找出return_port的stage並檢查Slot是否為空
                if (return_port.File.Type == ePortType.BothPort)
                {
                    foreach (RobotStage stage in stage_list)
                    {
                        //先找出source_port的stage
                        if (stage.Data.NODENO == return_port.Data.NODENO && stage.Data.STAGEIDBYNODE == return_port.Data.PORTNO)
                        {
                            //從stage找slot是否為空
                            if (stage.PortSlotInfos[from_slot_no - 1].slotGlassExist == "0")
                            {
                                target_stage = stage;
                                target_slot_no = from_slot_no;
                            }
                            else
                            {
                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] curBcsJob Return Port({2}) Slot({3}) is not Empty(Glass is Exist in Source Slot)",
                                                            "L1", MethodBase.GetCurrentMethod().Name, return_port.Data.PORTID, from_slot_no);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                }

                                #endregion

                                errMsg = string.Format("[{0}] curBcsJob Return Port({1}) Slot({2}) is not Empty(Glass is Exist in Source Slot)",
                                                        MethodBase.GetCurrentMethod().Name, return_port.Data.PORTID, from_slot_no);

                                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OriginalCassetteOriginalSlotIsNotEmpty);
                                robotConText.SetReturnMessage(errMsg);
                                return false;
                            }
                            break;
                        }
                    }
                }
                else
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] curBcsJob Source Port({2}) is not a Both Port",
                                                "L1", MethodBase.GetCurrentMethod().Name, curBcsJob.SourcePortID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] curBcsJob Source Port({1}) is not a Both Port",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.SourcePortID);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_SourcePortNotBothPort);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #region 暫時保留, 留待日後需求確定
                ////source_port是LoadingPort或者source_port是BothPort但相同Slot已經被佔用, 則會尋找UnloadingPort
                //if (source_port.File.Type == ePortType.LoadingPort && target_stage == null)
                //{
                //    List<RobotStage> unloading_ports = new List<RobotStage>();
                //    #region 找出UnloadingPort
                //    foreach (RobotStage stage in stage_list)
                //    {
                //        if (stage.Data.NODENO == source_port.Data.NODENO && stage.Data.STAGEIDBYNODE == source_port.Data.PORTNO)
                //            continue;

                //        Port port = ObjectManager.PortManager.GetPort(stage.Data.STAGEID);
                //        if (port.File.Type == ePortType.UnloadingPort)
                //        {
                //            unloading_ports.Add(stage);
                //        }
                //    }
                //    #endregion
                //    #region 尋找是否UnloadingPort上已經有相同FromCst的Job, 且相同Slot必須為空
                //    foreach (RobotStage unloading_port in unloading_ports)
                //    {
                //        foreach (RobotStage_PortSlotInfo slot in unloading_port.PortSlotInfos)
                //        {
                //            if (slot.slotCSTSeq == curBcsJob.CassetteSequenceNo &&//有相同FromCst的Job
                //                unloading_port.PortSlotInfos[from_slot_no - 1].slotGlassExist == "0")//相同Slot必須為空
                //            {
                //                target_stage = unloading_port;
                //                target_slot_no = from_slot_no;
                //                break;
                //            }
                //        }
                //        if (target_stage != null) break;
                //    }
                //    #endregion
                //    #region UnloadingPort上沒有相同FromCst的Job, 尋找UnloadingPort是否有空Cst
                //    if (target_stage == null)
                //    {
                //        foreach (RobotStage unloading_port in unloading_ports)
                //        {
                //            bool cassette_empty = true;
                //            for (int slot_no = unloading_port.PortSlotInfos.Count; slot_no > 0; slot_no--)
                //            {
                //                if (unloading_port.PortSlotInfos[slot_no - 1].slotGlassExist == "1")
                //                {
                //                    cassette_empty = false;
                //                    break;
                //                }
                //            }
                //            if (cassette_empty)
                //            {
                //                target_stage = unloading_port;
                //                target_slot_no = from_slot_no;//在空CST下, 相同Slot必為空
                //                break;
                //            }
                //        }
                //    }
                //    #endregion
                //    #region UnloadingPort上沒有空Cst, 則找有空Slot的UnloadingPort
                //    if (target_stage == null)
                //    {
                //        foreach (RobotStage unloading_port in unloading_ports)
                //        {
                //            for (int slot_no = unloading_port.PortSlotInfos.Count; slot_no > 0; slot_no--)
                //            {
                //                if (unloading_port.PortSlotInfos[slot_no - 1].slotGlassExist == "0")
                //                {
                //                    target_stage = unloading_port;
                //                    target_slot_no = slot_no;//從後面數來第一個空Slot
                //                    break;
                //                }
                //            }
                //        }
                //    }
                //    #endregion
                //}
                #endregion
                if (target_stage != null)
                {
                    curDefineCmd.Cmd01_TargetPosition = int.Parse(target_stage.Data.STAGEID);
                    curDefineCmd.Cmd01_TargetSlotNo = target_slot_no;
                }
                else
                {
                    //任意空Slot, 不在此指定, 交由RobotMainProcess指定
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


        [UniAuto.UniBCS.OpiSpec.Help("OR0025")]
        public bool OrderBy_RtcToOrigionCassette(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;

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

                #region List<RobotStage> stage_list
                IOrderedEnumerable<RobotStage> afterOrderByResultInfo = (IOrderedEnumerable<RobotStage>)robotConText[eRobotContextParameter.AfrerOrderByResultInfo];

                //找不到之前Orderby的結果時回NG
                if (afterOrderByResultInfo == null)
                {
                    if (after1stOrderByFlag == true)
                    {
                        #region  [DebugLog]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find before OrderBy Stage List!",
                                                    "L1", MethodBase.GetCurrentMethod().Name);

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
                else if (afterOrderByResultInfo.ToList().Count == 0)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] before OrderBy Stage List is empty!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] before OrderBy Stage List is empty!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Empty);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                List<RobotStage> stage_list = new List<RobotStage>();
                if (after1stOrderByFlag == true && afterOrderByResultInfo != null && afterOrderByResultInfo.ToList().Count > 0)
                {
                    stage_list = afterOrderByResultInfo.ToList();
                }
                else
                    stage_list = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

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

                #region Rtc To Origion Cassette
                Cassette return_cassette = ObjectManager.CassetteManager.GetCassette(curBcsJob.FromCstID);
                if (return_cassette == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob Source Cassette({2})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curBcsJob.FromCstID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find curBcsJob Return Cassette({1})!",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.FromCstID);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region Port return_port
                Port return_port = ObjectManager.PortManager.GetPort(return_cassette.PortID);
                if (return_port == null)
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find curBcsJob Return Port({2})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, return_cassette.PortID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find curBcsJob Return Port({1})!",
                                            MethodBase.GetCurrentMethod().Name, return_cassette.PortID);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetCurBcsJob);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                RobotStage target_stage = null;
                int target_slot_no = 0;
                int from_slot_no = int.Parse(curBcsJob.FromSlotNo);
                //return_port是BothPort, 找出return_port的stage並檢查Slot是否為空
                if (return_port.File.Type == ePortType.BothPort || return_port.File.Type == ePortType.LoadingPort)
                {
                    foreach (RobotStage stage in stage_list)
                    {
                        //先找出source_port的stage
                        if (stage.Data.NODENO == return_port.Data.NODENO && stage.Data.STAGEIDBYNODE == return_port.Data.PORTNO)
                        {
                            //從stage找slot是否為空
                            if (stage.PortSlotInfos[from_slot_no - 1].slotGlassExist == "0")
                            {
                                target_stage = stage;
                                target_slot_no = from_slot_no;
                            }
                            else
                            {
                                #region  [DebugLog]

                                if (IsShowDetialLog == true)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] curBcsJob Return Port({2}) Slot({3}) is not Empty(Glass is Exist in Source Slot)",
                                                            "L1", MethodBase.GetCurrentMethod().Name, return_port.Data.PORTID, from_slot_no);

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                }

                                #endregion

                                errMsg = string.Format("[{0}] curBcsJob Return Port({1}) Slot({2}) is not Empty(Glass is Exist in Source Slot)",
                                                        MethodBase.GetCurrentMethod().Name, return_port.Data.PORTID, from_slot_no);

                                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_OriginalCassetteOriginalSlotIsNotEmpty);
                                robotConText.SetReturnMessage(errMsg);
                                return false;
                            }
                            break;
                        }
                    }
                }
                else
                {
                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] curBcsJob Source Port({2}) is not a Both Port",
                                                "L1", MethodBase.GetCurrentMethod().Name, curBcsJob.SourcePortID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] curBcsJob Source Port({1}) is not a Both Port",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.SourcePortID);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_SourcePortNotBothPort);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                if (target_stage != null)
                {
                    curDefineCmd.Cmd01_TargetPosition = int.Parse(target_stage.Data.STAGEID);
                    curDefineCmd.Cmd01_TargetSlotNo = target_slot_no;
                }
                else
                {
                    //任意空Slot, 不在此指定, 交由RobotMainProcess指定
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
        /// <summary> 根據Stage的收片時間Input Time先後來排序
        ///
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("OR0005")]
        public bool OrderBy_StageInputTime(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] can not Set Afrer1stOrderByCheckFlag!", "L2");
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
                                                    "L1", MethodBase.GetCurrentMethod().Name);

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
                                                "L1", MethodBase.GetCurrentMethod().Name);

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
                                                "L1", MethodBase.GetCurrentMethod().Name);

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

                    errMsg = string.Format("[{0}] can not Get JobInfo!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobFilter_ReturnCode.NG_curBcsJob_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ 開始排序 ]
                //根據Stage的收片時間設定來排序
                if (orderbyAction.Trim() == eRobotCommonConst.DB_ORDER_BY_ASC)
                {
                    //afterOrderByStageList = beforeOrderByStageInfo.OrderBy(s => s.File.InputDateTime).ToList();
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
                                                        "L1", MethodBase.GetCurrentMethod().Name);

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
                                                        "L1", MethodBase.GetCurrentMethod().Name);

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
                        afterOrderByResultInfo = beforeOrderByStageList.OrderBy(s => s.File.InputDateTime);
                        //add by hujunpeng 20190417 for OVNITO提前开门逻辑优化，如果已经开的门和order by之后的target stage不一致，就将order by之后的结果做成已经开门的一样
                        if (curRobot.Data.LINETYPE == "OVNITO_CSUN")
                        {
                            int curstage = 0;
                            switch (curBcsJob.RobotWIP.OvnOpenTheDoorPriority)
                            { 
                              case 1:
                                  curstage = 11;
                                  break;
                              case 2:
                                  curstage = 13;
                                  break;
                              default:
                                  break;
                            }
                            if(int.Parse(afterOrderByResultInfo.FirstOrDefault<RobotStage>().Data.STAGEID)!=curstage)
                            {
                              switch (curstage)
                              { 
                                  case 11:
                                    afterOrderByResultInfo = beforeOrderByStageList.OrderBy(s=>s.Data.STAGEID);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("Current open the door is OVN[{0}],Current orderby result is stage[{1}],Force change the orderby result from stage[{2}] to stage[{3}]", curstage, afterOrderByResultInfo.FirstOrDefault<RobotStage>().Data.STAGEID, afterOrderByResultInfo.FirstOrDefault<RobotStage>().Data.STAGEID,curstage));
                                    break;
                                  case 13:
                                    afterOrderByResultInfo = beforeOrderByStageList.OrderByDescending(s=>s.Data.STAGEID);
                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                      string.Format("Current open the door is OVN[{0}],Current orderby result is stage[{1}],Force change the orderby result from stage[{2}] to stage[{3}]", curstage, afterOrderByResultInfo.FirstOrDefault<RobotStage>().Data.STAGEID, afterOrderByResultInfo.FirstOrDefault<RobotStage>().Data.STAGEID, curstage));
                                    break;
                                  default:
                                    break;
                              }
                            }
                        }
                        
                    }
                    else
                    {
                        afterOrderByResultInfo = beforeOrderByResultInfo.ThenBy(s => s.File.InputDateTime);
                        //add by hujunpeng 20190417 for OVNITO提前开门逻辑优化，如果已经开的门和order by之后的target stage不一致，就将order by之后的结果做成已经开门的一样
                        if (curRobot.Data.LINETYPE == "OVNITO_CSUN")
                        {
                            int curstage = 0;
                            switch (curBcsJob.RobotWIP.OvnOpenTheDoorPriority)
                            {
                                case 1:
                                    curstage = 11;
                                    break;
                                case 2:
                                    curstage = 13;
                                    break;
                                default:
                                    break;
                            }
                            if (int.Parse(afterOrderByResultInfo.FirstOrDefault<RobotStage>().Data.STAGEID) != curstage)
                            {
                                switch (curstage)
                                {
                                    case 11:
                                        afterOrderByResultInfo = beforeOrderByResultInfo.OrderBy(s => s.Data.STAGEID);
                                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("Current open the door is OVN[{0}],Current orderby result is stage[{1}],Force change the orderby result from stage[{2}] to stage[{3}]", curstage, afterOrderByResultInfo.FirstOrDefault<RobotStage>().Data.STAGEID, afterOrderByResultInfo.FirstOrDefault<RobotStage>().Data.STAGEID, curstage));
                                        break;
                                    case 13:
                                        afterOrderByResultInfo = beforeOrderByResultInfo.OrderByDescending(s => s.Data.STAGEID);
                                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format("Current open the door is OVN[{0}],Current orderby result is stage[{1}],Force change the orderby result from stage[{2}] to stage[{3}]", curstage, afterOrderByResultInfo.FirstOrDefault<RobotStage>().Data.STAGEID, afterOrderByResultInfo.FirstOrDefault<RobotStage>().Data.STAGEID, curstage));
                                        break;
                                    default:
                                        break;
                                }
                            }
                        }
                    }

                }
                else if (orderbyAction.Trim() == eRobotCommonConst.DB_ORDER_BY_DESC)
                {
                    //afterOrderByStageList = beforeOrderByStageInfo.OrderByDescending(s => s.File.InputDateTime).ToList();
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
                                                        "L1", MethodBase.GetCurrentMethod().Name);

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
                                                        "L1", MethodBase.GetCurrentMethod().Name);

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
                        afterOrderByResultInfo = beforeOrderByStageList.OrderByDescending(s => s.File.InputDateTime);
                    }
                    else
                    {
                        afterOrderByResultInfo = beforeOrderByResultInfo.ThenByDescending(s => s.File.InputDateTime);
                    }

                }
                else
                {
                    #region  [DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] find OrderByAction({2}) is illgal!",
                                                "L1", MethodBase.GetCurrentMethod().Name, orderbyAction);

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


        /// <summary> 根據PortMode排序, PortMode符合JobJudge為優先, PortMode為EMP次之
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("OR0014")]
        public bool OrderBy_PortMode(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] can not Set Afrer1stOrderByCheckFlag!", "L2");
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
                                                    "L1", MethodBase.GetCurrentMethod().Name);

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
                                                "L1", MethodBase.GetCurrentMethod().Name);

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
                                                "L1", MethodBase.GetCurrentMethod().Name);

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

                #region [ 開始排序 ]
                //根據PortMode排序
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
                                                        "L1", MethodBase.GetCurrentMethod().Name);

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
                                                        "L1", MethodBase.GetCurrentMethod().Name);

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
                        afterOrderByResultInfo = beforeOrderByStageList.OrderBy(s => s, new JobJudgePortModeCompare(curBcsJob));
                    }
                    else
                    {
                        afterOrderByResultInfo = beforeOrderByResultInfo.ThenBy(s => s, new JobJudgePortModeCompare(curBcsJob));
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
                                                        "L1", MethodBase.GetCurrentMethod().Name);

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
                                                        "L1", MethodBase.GetCurrentMethod().Name);

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
                                                "L1", MethodBase.GetCurrentMethod().Name, orderbyAction);

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

        /// <summary> Sorter Mode時根據Job Grade決定TargetSlot
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("OR0015")]
        public bool OrderBy_MappingGrade(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;

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

                #region List<RobotStage> stepCanUseStageList

                List<RobotStage> stepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                //找不到任何的Stage List時回NG
                if (stepCanUseStageList == null || stepCanUseStageList.Count <= 0)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find before OrderBy Stage List!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find Step Can Use Stage List Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Null);
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

                bool unloader_ready = false;
                List<PortStage> mapping_ports = new List<PortStage>();
                List<Port> ports = ObjectManager.PortManager.GetPortsByLine(curRobot.Data.LINEID);
                string fail_ReasonCode = string.Empty;
                if (curBcsJob.VCR_Result == eVCR_EVENT_RESULT.NOUSE ||
                    curBcsJob.VCR_Result == eVCR_EVENT_RESULT.READING_OK_MATCH_JOB ||
                    curBcsJob.VCR_Result == eVCR_EVENT_RESULT.READING_FAIL_KEY_IN_MATCH_JOB ||
                    curBcsJob.VCR_Result == eVCR_EVENT_RESULT.READING_FAIL_PASS)
                {
                    mapping_ports = SorterMode_JobGradeUnloaderGrade(eqp, ports, stepCanUseStageList, curBcsJob, ref unloader_ready);
                    fail_ReasonCode = string.Format("JobOrderByService_OrderBy_MappingGrade_VCRNotMismatch");
                    if (mapping_ports.Count <= 0)
                    {
                        if (!curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job({2}_{3}) curRouteID({4}) curStepNo({5}) JobVCRResult({6}) JobGrade({7}) JobProductType({8}) EqpProductTypeCheckMode({9}), please check 1.Unloading port must is ByGrade port mode 2.L2.File.ProductTypeCheckMode = Disable,Job JobGrade = port.File.MappingGrade 3.L2.File.ProductTypeCheckMode = Enable,port.File.ProductType=0 or job.ProductType=port.File.ProductType,Job JobGrade = port.File.MappingGrade",
                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), (int)curBcsJob.VCR_Result, curBcsJob.JobGrade,
                                curBcsJob.ProductType, (int)eqp.File.ProductTypeCheckMode);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                            //后面有记录job stop Reason ，所以此处不需要再记
                            //errMsg = string.Format("Job({0}_{1}) curRouteID({2}) curStepNo({3}) JobVCRResult({4}) JobGrade({5}) JobProductType({6}) EqpProductTypeCheckMode({7}), please check 1.Unloading port must is ByGrade port mode 2.L2.File.ProductTypeCheckMode = Disable,Job JobGrade = port.File.MappingGrade 3.L2.File.ProductTypeCheckMode = Enable,port.File.ProductType=0 or job.ProductType=port.File.ProductType,Job JobGrade = port.File.MappingGrade",
                            //curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                            //curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), (int)curBcsJob.VCR_Result, curBcsJob.JobGrade,
                            //curBcsJob.ProductType, (int)eqp.File.ProductTypeCheckMode);
                            //AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, errMsg);
                            //SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, errMsg, eSendToOPIMsgType.AlarmType);
                            #endregion
                        }
                    }
                    else
                    {
                        RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);
                    }
                }
                else
                {
                    fail_ReasonCode = string.Format("JobOrderByService_OrderBy_MappingGrade_VCRMismatch");
                    foreach (Port port in ports)
                    {
                        if (port.File.Mode == ePortMode.Mismatch)
                        {
                            RobotStage stage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(port.Data.PORTNO);
                            if (stage.curLDRQ_EmptySlotList.Count > 0)
                                mapping_ports.Add(new PortStage(port, stage));
                        }
                    }
                    if (mapping_ports.Count <= 0)
                    {
                        if (!curBcsJob.RobotWIP.CheckFailMessageList.ContainsKey(fail_ReasonCode))
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Job({2}_{3}) curRouteID({4}) curStepNo({5}) JobVCRResult({6}) JobGrade({7}) JobProductType({8}) EqpProductTypeCheckMode({9}), please check Job VCR Read MisMatch,must go to port mode:Mismatch port",
                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), (int)curBcsJob.VCR_Result, curBcsJob.JobGrade,
                                curBcsJob.ProductType, (int)eqp.File.ProductTypeCheckMode);
                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                            #region [ 記錄Fail Msg To OPI and Robot FailMsg ]
                            //后面有记录job stop Reason ，所以此处不需要再记
                            //errMsg = string.Format("Job({0}_{1}) curRouteID({2}) curStepNo({3}) JobVCRResult({4}) JobGrade({5}) JobProductType({6}) EqpProductTypeCheckMode({7}), please check Job VCR Read MisMatch,must go to port mode:Mismatch port",
                            //curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                            //curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), (int)curBcsJob.VCR_Result, curBcsJob.JobGrade,
                            //curBcsJob.ProductType, (int)eqp.File.ProductTypeCheckMode);
                            //AddJobCheckFailMsg(curBcsJob, fail_ReasonCode, errMsg);
                            //SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, errMsg, eSendToOPIMsgType.AlarmType);
                            #endregion
                        }
                    }
                    else
                    {
                        RemoveJobCheckFailMsg(curBcsJob, fail_ReasonCode);
                    }
                }

                if (mapping_ports.Count <= 0)
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[{0}] There is no Port mapping with CassetteSeqNo[{1}] JobSeqNo[{2}] JobVCRResult[{3}({4})] JobGrade[{5}] JobProductType[{6}] EqpProductTypeCheckMode(Indexer)[{7}](please check:1.If JobVCRResult Not MisMatch(0,1,3,5),Job MUST Go to Unloadingport(PortMode:ByGrade Mode) And Job's JobGrade Must Equals To Unloadingport's  MappingGrade AND IF Indexer's ProductTypeCheckMode=Enable,Job's ProductType Must Equals To Unloadingport's ProductType2.IF JobVCRResult MisMatch(2,4,6,7,8,9),Job MUST Go to Unloadingport(PortMode:Mismatch Mode))",
                        MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.VCR_Result.ToString(), (int)curBcsJob.VCR_Result, curBcsJob.JobGrade, curBcsJob.ProductType, eqp.File.ProductTypeCheckMode.ToString());
                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion

                    errMsg = string.Format("[{0}] There is no Port mapping with CassetteSeqNo[{1}] JobSeqNo[{2}] JobVCRResult[{3}({4})] JobGrade[{5}] JobProductType[{6}] EqpProductTypeCheckMode(Indexer)[{7}](please check:1.If JobVCRResult Not MisMatch(0,1,3,5),Job MUST Go to Unloadingport(PortMode:ByGrade Mode) And Job's JobGrade Must Equals To Unloadingport's  MappingGrade AND IF Indexer's ProductTypeCheckMode=Enable,Job's ProductType Must Equals To Unloadingport's ProductType2.IF JobVCRResult MisMatch(2,4,6,7,8,9),Job MUST Go to Unloadingport(PortMode:Mismatch Mode))",
                    MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.VCR_Result.ToString(), (int)curBcsJob.VCR_Result, curBcsJob.JobGrade, curBcsJob.ProductType, eqp.File.ProductTypeCheckMode.ToString());
                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_No_Match_Port_Grade);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                else
                {
                    RemoveJobAllCheckFailMsg(curBcsJob);
                }

                PortStage target_port = mapping_ports[0];
                foreach (int slot_no in target_port.Stage.curLDRQ_EmptySlotList.Keys)
                {
                    if (target_port.Stage.curLDRQ_EmptySlotList[slot_no].Trim() == string.Empty)
                    {
                        curDefineCmd.Cmd01_TargetPosition = int.Parse(target_port.Stage.Data.STAGEID);
                        curDefineCmd.Cmd01_TargetSlotNo = slot_no;

                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                        robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);

                        return true;
                    }
                }

                #region 找不到空SLOT
                #region[DebugLog]
                if (IsShowDetialLog == true)
                {
                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] JobGrade[{2}] JobProductType[{3}] EqpCheckMode[{4}], Target Stage[{5}] no empty slot(can't find empty slot)",
                                            "L1", MethodBase.GetCurrentMethod().Name, curBcsJob.JobGrade, curBcsJob.ProductType, (int)eqp.File.ProductTypeCheckMode, target_port.Stage.Data.STAGEID);

                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }
                #endregion
                errMsg = string.Format("[{0}] JobGrade[{1}] JobProductType[{2}] EqpCheckMode[{3}], Target Stage[{4}] is not empty slot(can't find empty slot)",
                                        MethodBase.GetCurrentMethod().Name, curBcsJob.JobGrade, curBcsJob.ProductType, (int)eqp.File.ProductTypeCheckMode, target_port.Stage.Data.STAGEID);
                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_TargetStageNotEmptySlot);
                robotConText.SetReturnMessage(errMsg);
                #endregion
                return false;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        [UniAuto.UniBCS.OpiSpec.Help("OR0016")]
        public bool OrderBy_FixTargetStage(IRobotContext robotConText)
        {
            try
            {
                string errMsg = string.Empty;
                string strlog = string.Empty;
                IOrderedEnumerable<RobotStage> afterOrderByResultInfo = null;
                #region bool after1stOrderByFlag

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

                #region IOrderedEnumerable<RobotStage> beforeOrderByResultInfo

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
                                                    "L1", MethodBase.GetCurrentMethod().Name);

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
                else if (beforeOrderByResultInfo.ToList().Count == 0)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] before OrderBy Stage List is empty!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

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

                #region Robot curRobot

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

                #region List<RobotStage> stepCanUseStageList

                List<RobotStage> stepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                //找不到任何的Stage List時回NG
                if (stepCanUseStageList == null || stepCanUseStageList.Count <= 0)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find before OrderBy Stage List!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find Step Can Use Stage List Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region FixTargetStage fix_param
                FixTargetStage_RobotParam fix_param = null;
                if (StaticContext.ContainsKey(eRobotContextParameter.FixTargetStage_RobotParam) &&
                    StaticContext[eRobotContextParameter.FixTargetStage_RobotParam] is FixTargetStage_RobotParam)
                {
                    fix_param = (FixTargetStage_RobotParam)StaticContext[eRobotContextParameter.FixTargetStage_RobotParam];
                }

                #endregion

                List<RobotStage> list = null;
                if (after1stOrderByFlag == false)
                    list = stepCanUseStageList.ToList();
                else
                    list = beforeOrderByResultInfo.ToList();

                #region Check fix_target_stage
                if (fix_param == null || string.IsNullOrEmpty(fix_param.STAGEID))
                {
                    #region DebugLog
                    if (fix_param == null)
                    {
                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] StaticContext has no FixTargetStage",
                                                    "L2", MethodBase.GetCurrentMethod().Name);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                    }
                    else
                    {
                        #region[DebugLog]
                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] FixTargetStage is empty",
                                                    "L2", MethodBase.GetCurrentMethod().Name);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }
                        #endregion
                    }
                    #endregion
                    afterOrderByResultInfo = list.OrderBy(s => s, new NoCompare());
                    robotConText.AddParameter(eRobotContextParameter.AfrerOrderByResultInfo, afterOrderByResultInfo);
                    robotConText.SetReturnCode(eJobFilter_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobFilter_ReturnMessage.OK_Message);
                    return true;
                }
                #endregion

                if (fix_param.TimeoutState == FixTargetStage_RobotParam.TIMEOUT_STATE.TIMEOUT)
                {
                    // Fix Stage 截至 Timeout 前未發出 Receive Able
                    SerializableDictionary<int, RobotRouteStep> steps = ObjectManager.RobotManager.GetRouteStepList(curRobot.Data.ROBOTNAME, curBcsJob.RobotWIP.CurRouteID);
                    string[] stage_list = steps[2].Data.STAGEIDLIST.Split(',');//STAGEIDLIST="11,12,13"
                    // 當 Fix Stage 是 11, tmp 則是 12,13
                    // 當 Fix Stage 是 12, tmp 則是 13,11
                    // 當 Fix Stage 是 13, tmp 則是 11,12
                    List<string> tmp = new List<string>();
                    #region tmp
                    {
                        int find = -1;
                        for (int i = 0; i < stage_list.Length; i++)
                        {
                            if (find >= 0)
                                tmp.Add(stage_list[i]);
                            else if (stage_list[i] == fix_param.STAGEID)
                            {
                                find = i;
                                tmp.Add(stage_list[i]);
                            }
                        }
                        for (int i = 0; i < find; i++)
                        {
                            tmp.Add(stage_list[i]);
                        }
                    }
                    #endregion
                    list.Clear();
                    foreach (string stage_id in tmp)
                    {
                        RobotStage stage = stepCanUseStageList.Find(s => s.Data.STAGEID == stage_id);
                        if (stage != null)
                            list.Add(stage);
                    }
                }
                else if (fix_param.TimeoutState == FixTargetStage_RobotParam.TIMEOUT_STATE.RECEIVE_ABLE)
                {
                    // Fix Stage 發出 Receive Able, 將 Fix Stage 移到第一順位
                    RobotStage fix_stage = list.Find(s => s.Data.STAGEID == fix_param.STAGEID);
                    if (fix_stage != null && list.Count > 1)
                    {
                        list.Remove(fix_stage);
                        list.Insert(0, fix_stage);
                    }
                }

                afterOrderByResultInfo = list.OrderBy(s => s, new NoCompare());
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

        //20160218 add for Array Changer SamplingMode Use Only
        /// <summary>Changer Plan時放回Plan指定的Port Slot, 只能在PUT to Port時使用 for Array Changer SamplingMode Use Only
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("OR0022")]
        public bool OrderBy_ChangerPlanTargetSlot_ForArrayChangerSamplingMode(IRobotContext robotConText)
        {
            try
            {
                if (!IsAllLineSameIndexerMode(eINDEXER_OPERATION_MODE.SAMPLING_MODE))
                {
                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eJobOrderBy_ReturnMessage.OK_Message);
                    return true;
                }

                string errMsg = string.Empty;
                string strlog = string.Empty;

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

                #region List<RobotStage> stepCanUseStageList

                List<RobotStage> stepCanUseStageList = (List<RobotStage>)robotConText[eRobotContextParameter.StepCanUseStageList];

                //找不到任何的Stage List時回NG
                if (stepCanUseStageList == null || stepCanUseStageList.Count <= 0)
                {

                    #region  [DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find before OrderBy Stage List!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find Step Can Use Stage List Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_BeforeOrderStageList_Is_Null);
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

                #region IList<SLOTPLAN> slot_plans
                string plan_id = string.Empty;
                IList<SLOTPLAN> slot_plans = ObjectManager.PlanManager.GetProductPlans(out plan_id);
                if (slot_plans == null || slot_plans.Count <= 0)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not find Changer Plan!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] can not find Changer Plan Info!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetChangerPlan);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion

                #region SLOTPLAN job_slot_plan
                SLOTPLAN job_slot_plan = GetSlotPlanByJob(slot_plans, curBcsJob);
                if (job_slot_plan == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job CassetteSequenceNo({2}) JobSequenceNo({3}) curRouteID({4}) curStepNo({5}) can not find Changer Plan!(please check 1.Job.FromCstID = plan.SOURCE_CASSETTE_ID 2.Offline =>plan.SLOTNO > 0 & Job.FromSlotNo = plan.SLOTNO 3.Online =>plan.SLOTNO = 0 & plan.PRODUCT_NAME = Job.MesProduct.PRODUCTNAME)",
                                                "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                                curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) can not find Changer Plan Info!(please check 1.Job.FromCstID = plan.SOURCE_CASSETTE_ID 2.Offline =>plan.SLOTNO > 0 & Job.FromSlotNo = plan.SLOTNO 3.Online =>plan.SLOTNO = 0 & plan.PRODUCT_NAME = Job.MesProduct.PRODUCTNAME)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString());

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_GetChangerPlan);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                #region Port target_port
                Port target_port = null;
                RobotStage target_stage = null;
                int target_slot_no = 0;
                if (!int.TryParse(job_slot_plan.TARGET_SLOTNO, out target_slot_no)) target_slot_no = 0;

                foreach (RobotStage stage in stepCanUseStageList)
                {
                    if (stage.Data.STAGETYPE != eStageType.PORT)
                        continue;

                    Port port = ObjectManager.PortManager.GetPort(stage.Data.LINEID, stage.Data.NODENO, stage.Data.STAGEIDBYNODE);
                    if (port != null && port.File.CassetteID == job_slot_plan.TARGET_CASSETTE_ID)
                    {
                        target_port = port;
                        target_stage = stage;
                        break;
                    }
                }

                if (target_port == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Job({2}_{3}) curRouteID({4}) curStepNo({5}) Changer Plan Target CassetteID({6}), cannot find Target Port.(please check plan target CassetteID is Exist)",
                                                "L2", MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), job_slot_plan.TARGET_CASSETTE_ID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Job({1}_{2}) curRouteID({3}) curStepNo({4}) Changer Plan Target CassetteID({5}), cannot find Target Port.(please check plan target CassetteID is Exist)",
                                            MethodBase.GetCurrentMethod().Name, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo,
                                            curBcsJob.RobotWIP.CurRouteID, curBcsJob.RobotWIP.CurStepNo.ToString(), job_slot_plan.TARGET_CASSETTE_ID);

                    robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_ChangerPlanCassetteID);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                curDefineCmd.Cmd01_TargetPosition = int.Parse(target_stage.Data.STAGEID);
                if (target_slot_no > 0)
                {
                    if (target_port.File.ArrayJobExistenceSlot == null || target_port.File.ArrayJobExistenceSlot.Length < target_slot_no)
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            if (target_port.File.ArrayJobExistenceSlot == null)
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Stage({2}) Port.File.ArrayJobExistenceSlot is null",
                                                        "L1", MethodBase.GetCurrentMethod().Name, target_stage.Data.STAGEIDBYNODE);
                            }
                            else
                            {
                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Stage({2}) Port.File.ArrayJobExistenceSlot.Length({3}) Target Slot No({4})!",
                                                        "L1", MethodBase.GetCurrentMethod().Name, target_stage.Data.STAGEIDBYNODE, target_port.File.ArrayJobExistenceSlot.Length, target_slot_no);
                            }

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_JobExistenceSlotMismatch);
                        robotConText.SetReturnMessage(strlog);
                        return false;
                    }
                    if (target_port.File.ArrayJobExistenceSlot[target_slot_no - 1])
                    {
                        #region[DebugLog]

                        if (IsShowDetialLog == true)
                        {
                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Stage({2}) Slot({3}) Job Already Exist",
                                                    "L1", MethodBase.GetCurrentMethod().Name, target_stage.Data.STAGEIDBYNODE, target_slot_no);

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        errMsg = string.Format("[{0}] Stage({1}) Port({2}) CassetteID({3}) Slot({4}) Job Already Exist!",
                                                MethodBase.GetCurrentMethod().Name, target_stage.Data.STAGEIDBYNODE, target_port.Data.PORTNO, target_port.File.CassetteID, target_slot_no);

                        robotConText.SetReturnCode(eJobOrderBy_ReturnCode.NG_JobAlreadyExistInTargetSlot);
                        robotConText.SetReturnMessage(errMsg);
                        return false;
                    }
                    curDefineCmd.Cmd01_TargetSlotNo = target_slot_no;
                }
                else
                {
                    //任意空Slot
                    foreach (int curSlotNo in target_stage.curLDRQ_EmptySlotList.Keys)
                    {
                        if (string.IsNullOrEmpty(target_stage.curLDRQ_EmptySlotList[curSlotNo]))
                        {
                            target_stage.curLDRQ_EmptySlotList[curSlotNo] = curBcsJob.JobKey;
                            target_slot_no = curSlotNo;
                            break;
                        }
                    }
                    curDefineCmd.Cmd01_TargetSlotNo = target_slot_no;
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

    }
}
