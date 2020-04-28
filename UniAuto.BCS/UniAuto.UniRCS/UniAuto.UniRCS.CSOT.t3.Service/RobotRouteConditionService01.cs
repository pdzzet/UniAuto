using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniRCS.Core;
using System.Reflection;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;

namespace UniAuto.UniRCS.CSOT.t3.Service
{
    public partial class RobotRouteConditionService : AbstractRobotService
    {

//RouteCondition Funckey = "RC" + XXXX(序列號)

        #region Robot Route Condition for DRY_ICD / DRY_YAC line type (CSOT t3 Array shop)  add DRY_TEL   yang 2017/5/8

        /// <summary>只进 TCDDC 机台
        /// 
        /// Glass Flow :: 1->3->4->2
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0062")]
        public bool RouteCondition_TCDDC_ONLY(IRobotContext robotConText)
        {
            try
            {
                Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
                string L3_recipe = GetNodeRecipe(job.PPID, "L3"); //Return Null is RecipeID='00'
                string L4_recipe = GetNodeRecipe(job.PPID, "L4"); //Return Null is RecipeID='00'
                if (L3_recipe != null && L4_recipe == null)  //
                {
                    //直接回覆OK表示不卡任何條件直接套用這個設定的RouteID
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
                else
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    string return_str = "Unknown";
                    if (L3_recipe == null) return_str = "L3 Recipe is by pass";
                    else if (L4_recipe != null) return_str = "L4 Recipe is not by pass";
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }
        }

        /// <summary>只进 TCDRY 机台
        /// 
        /// Glass Flow :: 1->5 or 7->6 or 8->2
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0063")]
        public bool RouteCondition_TCDRY_ONLY(IRobotContext robotConText)
        {
            try
            {
                Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
                string L3_recipe = GetNodeRecipe(job.PPID, "L3"); //Return Null is RecipeID='00'
                string L4_recipe = GetNodeRecipe(job.PPID, "L4"); //Return Null is RecipeID='00'
                if (L3_recipe == null && L4_recipe != null)  //
                {
                    //直接回覆OK表示不卡任何條件直接套用這個設定的RouteID
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
                else
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    string return_str = "Unknown";
                    if (L3_recipe == null) return_str = "L3 Recipe is not by pass";
                    else if (L4_recipe != null) return_str = "L4 Recipe is by pass";
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }
        }
        
        /// <summary>先进 TCDDC 机台, 然后再进 TCDRY 机台
        /// 
        /// Glass Flow :: 1->3->4->5 or 7->6 or 8->2
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0064")]
        public bool RouteCondition_TCDDC_TCDRY(IRobotContext robotConText)
        {
            try
            {
                Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
                string L3_recipe = GetNodeRecipe(job.PPID, "L3"); //Return Null is RecipeID='00'
                string L4_recipe = GetNodeRecipe(job.PPID, "L4"); //Return Null is RecipeID='00'
                if (L3_recipe != null && L4_recipe != null)  //
                {
                    //直接回覆OK表示不卡任何條件直接套用這個設定的RouteID
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
                else
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    string return_str = "Unknown";
                    if (L3_recipe == null) return_str = "L3 Recipe is by pass";
                    else if (L4_recipe != null) return_str = "L4 Recipe is by pass";
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);

                return false;
            }
        }

        #endregion

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0065")]
        public bool RouteCondition_Cell_PDR_Normal(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            try
            {
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

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Robot EQP Entity ]

                Equipment curEQP = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);

                //找不到 EQP 回NG
                if (curEQP == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get EQP by NODENO({3})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.NODENO);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) can not Get EQP by NODENO({2})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.NODENO);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockEQP_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }


                #endregion

                #region [ Get Line Entity ]

                Line line = ObjectManager.LineManager.GetLine(curEQP.Data.LINEID);

                if (line == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get Line by EQP LINEID({3})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.LINEID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) can not Get Line by EQP LINEID({2})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.LINEID);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockLine_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Check EQP current RunMode(desc)

                //1 : Normal Mode,  2 : Sorter Mode
                string runModeDesc = string.Empty;
                string eqpRunMode = GetRunMode(line, curEQP.Data.NODEID, "1", out runModeDesc);
                if (curEQP.File.EquipmentRunMode == eqpRunMode)
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
                else
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) EQPID({3}) curEqpRunMode({4}) is not Match Mode({5})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                                curEQP.File.EquipmentRunMode, eqpRunMode);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) EQPID({2}) curEqpRunMode({3}) is not Match Mode1({4})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                            curEQP.File.EquipmentRunMode, eqpRunMode);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockLine_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0066")]
        public bool RouteCondition_Cell_PDR_Sorter(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            try
            {
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

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Robot EQP Entity ]

                Equipment curEQP = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);

                //找不到 EQP 回NG
                if (curEQP == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get EQP by NODENO({3})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.NODENO);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) can not Get EQP by NODENO({2})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.NODENO);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockEQP_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }


                #endregion

                #region [ Get Line Entity ]

                Line line = ObjectManager.LineManager.GetLine(curEQP.Data.LINEID);

                if (line == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get Line by EQP LINEID({3})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.LINEID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) can not Get Line by EQP LINEID({2})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.LINEID);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockLine_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Check EQP current RunMode(desc)

                //1 : Normal Mode,  2 : Sorter Mode
                string runModeDesc = string.Empty;
                string eqpRunMode = GetRunMode(line, curEQP.Data.NODEID, "2", out runModeDesc);
                if (curEQP.File.EquipmentRunMode == eqpRunMode)
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
                else
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) EQPID({3}) curEqpRunMode({4}) is not Match Mode({5})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                                curEQP.File.EquipmentRunMode, eqpRunMode);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) EQPID({2}) curEqpRunMode({3}) is not Match Mode({4})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                            curEQP.File.EquipmentRunMode, eqpRunMode);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockLine_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0067")]
        public bool RouteCondition_Cell_TAM_Normal(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            try
            {
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

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Robot EQP Entity ]

                Equipment curEQP = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);

                //找不到 EQP 回NG
                if (curEQP == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get EQP by NODENO({3})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.NODENO);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) can not Get EQP by NODENO({2})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.NODENO);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockEQP_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }


                #endregion

                #region [ Get Line Entity ]

                Line line = ObjectManager.LineManager.GetLine(curEQP.Data.LINEID);

                if (line == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get Line by EQP LINEID({3})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.LINEID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) can not Get Line by EQP LINEID({2})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.LINEID);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockLine_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Check EQP current RunMode(desc)

                //1 : Inspection Mode,  2 : Sorter Mode
                string runModeDesc = string.Empty;
                string eqpRunMode = GetRunMode(line, curEQP.Data.NODEID, "1", out runModeDesc);
                if (curEQP.File.EquipmentRunMode == eqpRunMode)
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
                else
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) EQPID({3}) curEqpRunMode({4}) is not Match Mode({5})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                                curEQP.File.EquipmentRunMode, eqpRunMode);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) EQPID({2}) curEqpRunMode({3}) is not Match Mode1({4})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                            curEQP.File.EquipmentRunMode, eqpRunMode);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockLine_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0068")]
        public bool RouteCondition_Cell_TAM_Sorter(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            try
            {
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

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Robot EQP Entity ]

                Equipment curEQP = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);

                //找不到 EQP 回NG
                if (curEQP == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get EQP by NODENO({3})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.NODENO);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) can not Get EQP by NODENO({2})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.NODENO);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockEQP_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }


                #endregion

                #region [ Get Line Entity ]

                Line line = ObjectManager.LineManager.GetLine(curEQP.Data.LINEID);

                if (line == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get Line by EQP LINEID({3})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.LINEID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) can not Get Line by EQP LINEID({2})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.LINEID);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockLine_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Check EQP current RunMode(desc)

                //1 : Normal Mode,  2 : Sorter Mode
                string runModeDesc = string.Empty;
                string eqpRunMode = GetRunMode(line, curEQP.Data.NODEID, "2", out runModeDesc);
                if (curEQP.File.EquipmentRunMode == eqpRunMode)
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
                else
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) EQPID({3}) curEqpRunMode({4}) is not Match Mode({5})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                                curEQP.File.EquipmentRunMode, eqpRunMode);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) EQPID({2}) curEqpRunMode({3}) is not Match Mode({4})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                            curEQP.File.EquipmentRunMode, eqpRunMode);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockLine_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }
        #region Robot Rout Condition for CVD Line

        /// <summary> Only L3 Process
        ///
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns>True/False</returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0002")]
        public bool RouteCondition_HDC_ONLY(IRobotContext robotConText)
        {
            try
            {

                Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
                string L3_recipe = GetNodeRecipe(job.PPID, "L3"); //Return Null is RecipeID='00'
                string L4_recipe = GetNodeRecipe(job.PPID, "L4"); //Return Null is RecipeID='00'
                if (L3_recipe != null && L4_recipe == null)  //
                {
                    //直接回覆OK表示不卡任何條件直接套用這個設定的RouteID
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
                else
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    string return_str = "Unknown";
                    if (L3_recipe == null) return_str = "L3 Recipe is by pass";
                    else if (L4_recipe != null) return_str = "L4 Recipe is not by pass";
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        /// <summary> Only Array CVD L4 Process
        ///
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns>True/False</returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0003")]
        public bool RouteCondition_CVD_ONLY(IRobotContext robotConText)
        {
            try
            {

                Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
                string L3_recipe = GetNodeRecipe(job.PPID, "L3"); //Return Null is RecipeID='00'
                string L4_recipe = GetNodeRecipe(job.PPID, "L4"); //Return Null is RecipeID='00'
                if (L3_recipe == null && L4_recipe != null)  //
                {
                    //直接回覆OK表示不卡任何條件直接套用這個設定的RouteID
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
                else
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    string return_str = "Unknown";
                    if (L4_recipe != null) return_str = "L3 Recipe is not by pass";
                    else if (L4_recipe == null) return_str = "L4 Recipe is by pass";
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        /// <summary> L2-L3->L4
        ///
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns>True/False</returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0004")]
        public bool RouteCondition_HDC_CVD(IRobotContext robotConText)
        {
            try
            {
                if (IsAllLineSameIndexerMode(eINDEXER_OPERATION_MODE.CHANGER_MODE))
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage("Indexer Operation Changer Mode");
                    return false;
                }

                Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
                string L3_recipe = GetNodeRecipe(job.PPID, "L3");
                string L4_recipe = GetNodeRecipe(job.PPID, "L4");
                if (L3_recipe != null && L4_recipe != null)
                {
                    //直接回覆OK表示不卡任何條件直接套用這個設定的RouteID
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
                else
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    string return_str = "Unknown";
                    if (L3_recipe == null) return_str = "L3 Recipe is by pass";
                    else if (L4_recipe == null) return_str = "L4 Recipe is by pass";
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        #endregion

        #region Robot Route for TTP Line

        /// <summary> Only L3 Process
        ///  20151211 Modify Watson 不再需要
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns>True/False</returns>
        //20151120 add FuncKey
        //[UniAuto.UniBCS.OpiSpec.Help("RC0014")]
        //public bool RouteCondition_TTP_ONLY(IRobotContext robotConText)
        //{
        //    try
        //    {
        //        Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
        //        string return_str = string.Empty;
        //        Equipment eqp = GetTTPEQP();

        //        if (eqp == null)
        //        {
        //            robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
        //            return_str = "EQP IS Null, Node NO=[" + robot.Data.NODENO + "]";
        //            robotConText.SetReturnMessage(return_str);
        //            return false;
        //        }


        //            robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
        //            return_str = "";
        //            robotConText.SetReturnMessage(return_str);
        //            return false;

        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //        robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
        //        robotConText.SetReturnMessage(ex.Message);
        //        return false;
        //    }
        //}

        //20151120 add FuncKey

        [UniAuto.UniBCS.OpiSpec.Help("RC0013")]
        public bool RouteCondition_TTP_CHAMBER(IRobotContext robotConText)
        {
            try
            {
                Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;

                Equipment eqp = GetTTPEQP();

                if (eqp == null)
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    string return_str = "EQP IS Null, Node NO=[" + robot.Data.NODENO + "]";
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }


 
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                return true;
          

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        private Equipment GetTTPEQP()
        {
            try
            {
                foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPs())
                {
                    if (eqp.Data.NODENAME.ToUpper().IndexOf("TTP") >=0)
                        return eqp;
                }
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }

        }

        #endregion

        #region Robot route for GAP Line
        /// <summary> Only CELL GAP L3 Process
        ///
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns>True/False</returns>
        [UniAuto.UniBCS.OpiSpec.Help("RC0074")]
        public bool RouteCondition_GAP_ONLY(IRobotContext robotConText)
        {
            string return_str = string.Empty;
            try
            {

                Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;

                # region [ Get Job From Port Assignment]
                eCELLPortAssignment glassAssignment = new eCELLPortAssignment();

                try
                {
                    glassAssignment = ObjectManager.PortManager.GetPort(job.SourcePortID).File.PortAssignment;
                }
                catch (Exception ex)
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    return_str = string.Format("Can not Get Glass Source Port Entity By SourcePortID({0})",job.SourcePortID);
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }

                #region [route判斷條件2 Run Mode:]
                Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(robot.Data.NODENO);

                if (eqp == null)
                {
                    return_str = "error eqp run mode [" + eqp.File.EquipmentRunMode + "]";
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }
                #endregion

                if ((glassAssignment == eCELLPortAssignment.GAP) && (eqp.File.EquipmentRunMode != eCellGAPEQPRunMode.GMIANDSORTERMODE))
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
                #endregion

                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                return_str = "RunMode[" + eqp.File.EquipmentRunMode + "] and Assignment[GAP] is not Match!";  //Watson Modify 20160227 For 回拋錯誤訊息、簡短、易懂
                robotConText.SetReturnMessage(return_str);
                return false;

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        [UniAuto.UniBCS.OpiSpec.Help("RC0075")]
        public bool RouteCondition_GMI_ONLY(IRobotContext robotConText)
        {
            string return_str = string.Empty;
            try
            {

                Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
              
                # region [ Get Job From Port Assignment]
                eCELLPortAssignment glassAssignment = new eCELLPortAssignment();

                try
                {
                    glassAssignment = ObjectManager.PortManager.GetPort(job.SourcePortID).File.PortAssignment;
                }
                catch (Exception ex)
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    return_str = string.Format("Can not Get Glass Source Port Entity By SourcePortID({0})", job.SourcePortID);
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }

                #region [route判斷條件2 Run Mode:]
                Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(robot.Data.NODENO);

                if (eqp == null)
                {
                    return_str = "error eqp run mode [" + eqp.File.EquipmentRunMode + "]";
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }
                #endregion

                if ((glassAssignment == eCELLPortAssignment.GMI) && (eqp.File.EquipmentRunMode != eCellGAPEQPRunMode.GAPANDSORTERMODE))
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
                #endregion

                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                return_str = "RunMode["+ eqp.File.EquipmentRunMode +"] and Assignment[GMI] is not Match!";  //Watson Modify 20160227 For 回拋錯誤訊息、簡短、易懂
              
                robotConText.SetReturnMessage(return_str);
                return false;
                
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        [UniAuto.UniBCS.OpiSpec.Help("RC0084")]
        public bool RouteCondition_GAPSORT(IRobotContext robotConText)
        {
            string return_str = string.Empty;
            try
            {
                Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
               
                # region [ Get Job From Port Assignment]
                eCELLPortAssignment glassAssignment = new eCELLPortAssignment();

                try
                {
                    glassAssignment = ObjectManager.PortManager.GetPort(job.SourcePortID).File.PortAssignment;
                }
                catch (Exception ex)
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    return_str = string.Format("Can not Get Glass Source Port Entity By SourcePortID({0})", job.SourcePortID);
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }
                #endregion

                #region [route判斷條件2 Run Mode:]
                Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(robot.Data.NODENO);

                if (eqp == null)
                {
                    return_str = "error eqp run mode [" + eqp.File.EquipmentRunMode + "]";
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }
                #endregion

                //沒有 Port Assignment a
                if (glassAssignment == eCELLPortAssignment.UNKNOW)
                {
                    return_str = "No any Port Assigment[" + glassAssignment + "] Report.";
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }

                //不一樣的Port Assignment a與不同的Run Mode 得到一樣的Route 
                if (eqp.File.EquipmentRunMode == eCellGAPEQPRunMode.GAPANDGMIMODE)
                {
                    return_str = "EQP Run Mode is Not Sort.";
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }

                //不一樣的Port Assignment a與不同的Run Mode 得到一樣的Route 
                if ((glassAssignment == eCELLPortAssignment.GMI) && (eqp.File.EquipmentRunMode == eCellGAPEQPRunMode.GAPANDSORTERMODE))
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }

                if ((glassAssignment == eCELLPortAssignment.GAP) && (eqp.File.EquipmentRunMode == eCellGAPEQPRunMode.GMIANDSORTERMODE))
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }

                return_str = "EQP run mode [" + eqp.File.EquipmentRunMode + "] and Port Assigment["+ glassAssignment + "] not match ,isn't Sort Route.";
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(return_str);
                return false;
                
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        #endregion

        [UniAuto.UniBCS.OpiSpec.Help("RC0078")]
        public bool RouteCondition_Cell_PTH_Normal(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            try
            {
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

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Robot EQP Entity ]

                Equipment curEQP = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);

                //找不到 EQP 回NG
                if (curEQP == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get EQP by NODENO({3})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.NODENO);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) can not Get EQP by NODENO({2})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.NODENO);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockEQP_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }


                #endregion

                #region [ Get Line Entity ]

                Line line = ObjectManager.LineManager.GetLine(curEQP.Data.LINEID);

                if (line == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get Line by EQP LINEID({3})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.LINEID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) can not Get Line by EQP LINEID({2})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.LINEID);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockLine_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Check EQP current RunMode(desc)

                //1 : Normal Inspection Mode,  2 : Repair Mode, 3 : Sorter Mode
                string runModeDesc = string.Empty;
                string eqpRunMode = GetRunMode(line, curEQP.Data.NODEID, "1", out runModeDesc);
                if (curEQP.File.EquipmentRunMode == eqpRunMode)
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
                else
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) EQPID({3}) curEqpRunMode({4}) is not Match Mode({5})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                                curEQP.File.EquipmentRunMode, eqpRunMode);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) EQPID({2}) curEqpRunMode({3}) is not Match Mode({4})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                            curEQP.File.EquipmentRunMode, eqpRunMode);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockLine_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        [UniAuto.UniBCS.OpiSpec.Help("RC0079")]
        public bool RouteCondition_Cell_PTH_Sorter(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            try
            {
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

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Robot EQP Entity ]

                Equipment curEQP = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);

                //找不到 EQP 回NG
                if (curEQP == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get EQP by NODENO({3})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.NODENO);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) can not Get EQP by NODENO({2})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.NODENO);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockEQP_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }


                #endregion

                #region [ Get Line Entity ]

                Line line = ObjectManager.LineManager.GetLine(curEQP.Data.LINEID);

                if (line == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get Line by EQP LINEID({3})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.LINEID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) can not Get Line by EQP LINEID({2})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.LINEID);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockLine_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Check EQP current RunMode(desc)

                //1 : Normal Inspection Mode,  2 : Repair Mode, 3 : Sorter Mode
                string runModeDesc = string.Empty;
                string eqpRunMode = GetRunMode(line, curEQP.Data.NODEID, "3", out runModeDesc);
                if (curEQP.File.EquipmentRunMode == eqpRunMode)
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
                else
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) EQPID({3}) curEqpRunMode({4}) is not Match Mode({5})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                                curEQP.File.EquipmentRunMode, eqpRunMode);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) EQPID({2}) curEqpRunMode({3}) is not Match Mode({4})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                            curEQP.File.EquipmentRunMode, eqpRunMode);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockLine_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        [UniAuto.UniBCS.OpiSpec.Help("RC0080")]
        public bool RouteCondition_Cell_PTH_Repair(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            try
            {
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

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Robot EQP Entity ]

                Equipment curEQP = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);

                //找不到 EQP 回NG
                if (curEQP == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get EQP by NODENO({3})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.NODENO);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) can not Get EQP by NODENO({2})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.NODENO);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockEQP_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }


                #endregion

                #region [ Get Line Entity ]

                Line line = ObjectManager.LineManager.GetLine(curEQP.Data.LINEID);

                if (line == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get Line by EQP LINEID({3})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.LINEID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) can not Get Line by EQP LINEID({2})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.LINEID);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockLine_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Check EQP current RunMode(desc)

                //1 : Normal Inspection Mode,  2 : Repair Mode, 3 : Sorter Mode
                string runModeDesc = string.Empty;
                string eqpRunMode = GetRunMode(line, curEQP.Data.NODEID, "2", out runModeDesc);
                if (curEQP.File.EquipmentRunMode == eqpRunMode)
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
                else
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) EQPID({3}) curEqpRunMode({4}) is not Match Mode({5})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                                curEQP.File.EquipmentRunMode, eqpRunMode);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) EQPID({2}) curEqpRunMode({3}) is not Match Mode({4})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                            curEQP.File.EquipmentRunMode, eqpRunMode);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockLine_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        [UniAuto.UniBCS.OpiSpec.Help("RC0081")]
        public bool RouteCondition_Cell_CRP(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            try
            {
                #region [ Get Equipment No ]
                string eqpNo = (string)robotConText[eRobotContextParameter.EquipmentNo];
                if (eqpNo == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Equipment No!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion
                    errMsg = string.Format("[{0}] can not Get Equipment No!", MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.EquipmentNo_Is_Error);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                if (eqpNo == "L2")
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
                else
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Equipment No!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] can not Get Equipment No!", MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.EquipmentNo_Is_Error);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        [UniAuto.UniBCS.OpiSpec.Help("RC0082")]
        public bool RouteCondition_Cell_CRP_VirtualPort(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            try
            {
                #region [ Get Equipment No ]
                string eqpNo = (string)robotConText[eRobotContextParameter.EquipmentNo];
                if (eqpNo == null)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Equipment No!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion
                    errMsg = string.Format("[{0}] can not Get Equipment No!",
                                            MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.EquipmentNo_Is_Error);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                #endregion

                if (eqpNo == "L2")
                {
                    #region[DebugLog]
                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] can not Get Equipment No!",
                                                "L1", MethodBase.GetCurrentMethod().Name);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }
                    #endregion
                    errMsg = string.Format("[{0}] can not Get Equipment No!", MethodBase.GetCurrentMethod().Name);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.EquipmentNo_Is_Error);
                    robotConText.SetReturnMessage(errMsg);
                    return false;
                }
                else
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        //20160229 add for Cell RWT Line SORT Mode
        [UniAuto.UniBCS.OpiSpec.Help("RC0085")]
        public bool RouteCondition_Cell_RWT_Sorter(IRobotContext robotConText)
        {
            string strlog = string.Empty;
            string errMsg = string.Empty;
            try
            {

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

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }

                #endregion

                #region [ Get Robot EQP Entity ]

                Equipment curEQP = ObjectManager.EquipmentManager.GetEQP(curRobot.Data.NODENO);

                //找不到 EQP 回NG
                if (curEQP == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get EQP by NODENO({3})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.NODENO);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) can not Get EQP by NODENO({2})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curRobot.Data.NODENO);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockEQP_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }


                #endregion

                #region [ Get Line Entity ]

                Line line = ObjectManager.LineManager.GetLine(curEQP.Data.LINEID);

                if (line == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) can not Get Line by EQP LINEID({3})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.LINEID);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) can not Get Line by EQP LINEID({2})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.LINEID);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockLine_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;

                }

                #endregion

                #region [ Check EQP current RunMode(desc)

                //- 1：Inspection Mode   - 2：Sort Mode
                string runModeDesc = string.Empty;
                string eqpRunMode = GetRunMode(line, curEQP.Data.NODEID, "2", out runModeDesc);
                if (curEQP.File.EquipmentRunMode == eqpRunMode)
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
                else
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) EQPID({3}) curEqpRunMode({4}) is not Match Mode({5})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                                curEQP.File.EquipmentRunMode, eqpRunMode);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) EQPID({2}) curEqpRunMode({3}) is not Match Mode({4})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                            curEQP.File.EquipmentRunMode, eqpRunMode);

                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_curRobot_DockLine_Is_Null);
                    robotConText.SetReturnMessage(errMsg);

                    return false;
                }
                #endregion
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        #region Robot route for PDR+CEM Line

        [UniAuto.UniBCS.OpiSpec.Help("RC0086")]
        public bool RouteCondition_PDR_ONLY(IRobotContext robotConText)
        {
            string return_str = string.Empty;
            try
            {

                Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;

                # region [ Get Job From Port Assignment]
                eCELLPortAssignment glassAssignment = new eCELLPortAssignment();

                try
                {
                    glassAssignment = ObjectManager.PortManager.GetPort(job.SourcePortID).File.PortAssignment;
                }
                catch (Exception ex)
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    return_str = string.Format("Can not Get Glass Source Port Entity By SourcePortID({0})", job.SourcePortID);
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }

                #region [route判斷條件2 Run Mode:]
                Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(robot.Data.NODENO);

                if (eqp == null)
                {
                    return_str = "error eqp run mode [" + eqp.File.EquipmentRunMode + "]";
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }
                #endregion

                if ((glassAssignment == eCELLPortAssignment.PDR) && (eqp.File.EquipmentRunMode != eCellPDREQPRunMode.CEMANDSORTERMODE))
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
                #endregion

                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                return_str = "RunMode[" + eqp.File.EquipmentRunMode + "] and Assignment[PDR] is not Match!";  //Watson Modify 20160227 For 回拋錯誤訊息、簡短、易懂
                robotConText.SetReturnMessage(return_str);
                return false;

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        [UniAuto.UniBCS.OpiSpec.Help("RC0087")]
        public bool RouteCondition_CEM_ONLY(IRobotContext robotConText)
        {
            string return_str = string.Empty;
            try
            {

                Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;

                # region [ Get Job From Port Assignment]
                eCELLPortAssignment glassAssignment = new eCELLPortAssignment();

                try
                {
                    glassAssignment = ObjectManager.PortManager.GetPort(job.SourcePortID).File.PortAssignment;
                }
                catch (Exception ex)
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    return_str = string.Format("Can not Get Glass Source Port Entity By SourcePortID({0})", job.SourcePortID);
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }

                #region [route判斷條件2 Run Mode:]
                Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(robot.Data.NODENO);

                if (eqp == null)
                {
                    return_str = "error eqp run mode [" + eqp.File.EquipmentRunMode + "]";
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }
                #endregion

                if ((glassAssignment == eCELLPortAssignment.CEM) && (eqp.File.EquipmentRunMode != eCellPDREQPRunMode.PDRANDSORTERMODE))
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
                #endregion

                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                return_str = "RunMode[" + eqp.File.EquipmentRunMode + "] and Assignment[CEM] is not Match!";  

                robotConText.SetReturnMessage(return_str);
                return false;

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        [UniAuto.UniBCS.OpiSpec.Help("RC0088")]
        public bool RouteCondition_PDRSORT(IRobotContext robotConText)
        {
            string return_str = string.Empty;
            try
            {
                Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;

                # region [ Get Job From Port Assignment]
                eCELLPortAssignment glassAssignment = new eCELLPortAssignment();

                try
                {
                    glassAssignment = ObjectManager.PortManager.GetPort(job.SourcePortID).File.PortAssignment;
                }
                catch (Exception ex)
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    return_str = string.Format("Can not Get Glass Source Port Entity By SourcePortID({0})", job.SourcePortID);
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }
                #endregion

                #region [route判斷條件2 Run Mode:]
                Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(robot.Data.NODENO);

                if (eqp == null)
                {
                    return_str = "error eqp run mode [" + eqp.File.EquipmentRunMode + "]";
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }
                #endregion

                //沒有 Port Assignment a
                if (glassAssignment == eCELLPortAssignment.UNKNOW)
                {
                    return_str = "No any Port Assigment[" + glassAssignment + "] Report.";
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }

                //不一樣的Port Assignment a與不同的Run Mode 得到一樣的Route 
                if (eqp.File.EquipmentRunMode == eCellPDREQPRunMode.PDRANDCEMMODE)
                {
                    return_str = "EQP Run Mode is Not Sort.";
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(return_str);
                    return false;
                }

                //不一樣的Port Assignment a與不同的Run Mode 得到一樣的Route 
                if ((glassAssignment == eCELLPortAssignment.CEM) && (eqp.File.EquipmentRunMode == eCellPDREQPRunMode.PDRANDSORTERMODE))
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }

                if ((glassAssignment == eCELLPortAssignment.PDR) && (eqp.File.EquipmentRunMode == eCellPDREQPRunMode.CEMANDSORTERMODE))
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }

                return_str = "EQP run mode [" + eqp.File.EquipmentRunMode + "] and Port Assigment[" + glassAssignment + "] not match ,isn't Sort Route.";
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(return_str);
                return false;

            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                robotConText.SetReturnMessage(ex.Message);
                return false;
            }
        }

        #endregion
    }
}
