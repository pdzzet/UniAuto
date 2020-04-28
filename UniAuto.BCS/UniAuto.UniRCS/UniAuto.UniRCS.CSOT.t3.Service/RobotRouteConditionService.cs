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
    [UniAuto.UniBCS.OpiSpec.Help("RobotRouteConditionService")]
    public partial class RobotRouteConditionService : AbstractRobotService
    {
        public override bool Init()
        {
            return true;
        }

//RouteCondition Funckey = "RC" + XXXX(序列號)

        /// <summary> Route Condition default,如果沒有其他條件符合則預設永遠回復OK
        ///
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0001")]
        public bool RouteCondition_Default(IRobotContext robotConText)
        {
            try
            {
                //直接回覆OK表示不卡任何條件直接套用這個設定的RouteID
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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0008")]
        public bool RouteCondition_Changer(IRobotContext robotConText)
        {
            try
            {
                if (IsAllLineSameIndexerMode(eINDEXER_OPERATION_MODE.CHANGER_MODE))
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
                else
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage("Indexer Operation Mode is not Changer Mode");
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

        /// <summary>
        /// 從整串PPID中, 取指定NodeNo的Recipe
        /// </summary>
        /// <param name="PPID">整串PPID</param>
        /// <param name="NodeNo">L3, L4</param>
        /// <returns>若Recipe為00則回傳null</returns>
        private string GetNodeRecipe(string PPID, string NodeNo)
        {
            int node = int.Parse(NodeNo.Substring(1));
            int start_index = 0;
            for (int i = 2; i < node; i++)
            {
                start_index += ObjectManager.EquipmentManager.GetEQP(string.Format("L{0}", i)).Data.RECIPELEN;
            }
            int len = ObjectManager.EquipmentManager.GetEQP(string.Format("L{0}", node)).Data.RECIPELEN;
            string node_pp = PPID.Substring(start_index, len);
            if (node_pp == string.Empty.PadLeft(len, '0'))
                return null;
            return node_pp;
        }

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0005")]
        public bool RouteCondition_CLS_Mode1(IRobotContext robotConText)
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
                if (L3_recipe != null && L4_recipe == null)
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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0006")]
        public bool RouteCondition_CLS_Mode2(IRobotContext robotConText)
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
                if (L3_recipe == null && L4_recipe != null)
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
                    if (L3_recipe != null) return_str = "L3 Recipe is not by pass";
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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0007")]
        public bool RouteCondition_CLS_Mode3(IRobotContext robotConText)
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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0021")]
        public bool RouteCondition_AOH_Mode1(IRobotContext robotConText)
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
                if (L3_recipe != null && L4_recipe == null)
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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0022")]
        public bool RouteCondition_AOH_Mode2(IRobotContext robotConText)
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
                if (L3_recipe == null && L4_recipe != null)
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
                    if (L3_recipe != null) return_str = "L3 Recipe is not by pass";
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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0023")]
        public bool RouteCondition_AOH_Mode3(IRobotContext robotConText)
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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0024")]
        public bool RouteCondition_CDO_Mode1(IRobotContext robotConText)
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
                if (L3_recipe != null && L4_recipe == null)
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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0025")]
        public bool RouteCondition_CDO_Mode2(IRobotContext robotConText)
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
                if (L3_recipe == null && L4_recipe != null)
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
                    if (L3_recipe != null) return_str = "L3 Recipe is not by pass";
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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0026")]
        public bool RouteCondition_CDO_Mode3(IRobotContext robotConText)
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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0011")]
        public bool RouteCondition_BFG_Mode1(IRobotContext robotConText)
        {
            try
            {
                Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
                //CST->Smash 
                if (job.EQPFlag.Substring(1, 1) == "1" && job.EQPFlag.Substring(0, 1) != "1")
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
                    return_str = "EQPFlag is Not Match [" + job.EQPFlag +"]";  //Watson 20160227 Modify 必須回拋錯誤，儘量簡短、易懂

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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0012")]
        public bool RouteCondition_BFG_Mode2(IRobotContext robotConText)
        {
            try
            {
                Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
                //CST->Smash->Cut
                if (job.EQPFlag.Substring(0, 1) == "1" && job.EQPFlag.Substring(1, 1) != "1")
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
                    return_str = "EQPFlag is Not Match [" + job.EQPFlag + "]";  //Watson 20160227 Modify 必須回拋錯誤，儘量簡短、易懂

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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0009")]
        public bool RouteCondition_TEG_Mode1(IRobotContext robotConText)
        {
            try
            {
                if (IsAllLineSameIndexerMode(eINDEXER_OPERATION_MODE.CHANGER_MODE))
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage("Indexer Operation Changer Mode");
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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0027")]
        public bool RouteCondition_NAN_Not_ChangerMode(IRobotContext robotConText)
        {
            try
            {
                if (IsAllLineSameIndexerMode(eINDEXER_OPERATION_MODE.CHANGER_MODE))
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage("Indexer Operation Changer Mode");
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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0028")]
        public bool RouteCondition_SCN_Not_ChangerMode(IRobotContext robotConText)
        {
            try
            {
                if (IsAllLineSameIndexerMode(eINDEXER_OPERATION_MODE.CHANGER_MODE))
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage("Indexer Operation Changer Mode");
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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0029")]
        public bool RouteCondition_AOH_Not_ChangerMode(IRobotContext robotConText)
        {
            try
            {
                if (IsAllLineSameIndexerMode(eINDEXER_OPERATION_MODE.CHANGER_MODE))
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage("Indexer Operation Changer Mode");
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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0010")]
        public bool RouteCondition_FLR_Mode1(IRobotContext robotConText)
        {
            try
            {
                if (IsAllLineSameIndexerMode(eINDEXER_OPERATION_MODE.CHANGER_MODE))
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage("Indexer Operation Changer Mode");
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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0019")]
        public bool RouteCondition_Only_One_ATS(IRobotContext robotConText)
        {
            try
            {
                //对于ATS100 如果不是Changer Mode 就是 Normal Mode

                if (IsAllLineSameIndexerMode(eINDEXER_OPERATION_MODE.CHANGER_MODE))
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage("Indexer Operation Changer Mode");
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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0020")]
        public bool RouteCondition_Two_ATS(IRobotContext robotConText)
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
                if (L3_recipe != null || L4_recipe != null)
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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0018")]
        public bool RouteCondition_MAC_Not_ChangerMode(IRobotContext robotConText)
        {
            try
            {
                //对于MAC700 Or 800 如果不是Changer Mode 就是 Normal Mode

                if (IsAllLineSameIndexerMode(eINDEXER_OPERATION_MODE.CHANGER_MODE))
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage("Indexer Operation Changer Mode");
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

        /// <summary>只跑RTA
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0015")]
        public bool RouteCondition_RTA_ONLY(IRobotContext robotConText)
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
                if (L3_recipe != null && L4_recipe == null)
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

        /// <summary>只跑USC
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0016")]
        public bool RouteCondition_USC_ONLY(IRobotContext robotConText)
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
                if (L3_recipe == null && L4_recipe != null)
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
                    if (L3_recipe != null) return_str = "L3 Recipe is not by pass";
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

        /// <summary>跑RTA和USC
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0017")]
        public bool RouteCondition_RTA_USC(IRobotContext robotConText)
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

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0030")]
        public bool RouteCondition_OVNPL_OVN_ANY(IRobotContext robotConText)
        {
            robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
            robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
            return true;
        }

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0031")]
        public bool RouteCondition_OVNITO_OVN_ANY(IRobotContext robotConText)
        {
            robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
            robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
            return true;
        }

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0032")]
        public bool RouteCondition_OVNSD_Mode1(IRobotContext robotConText)
        {
            robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
            robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
            return true;
        }

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0033")]
        public bool RouteCondition_RSM_Not_ChangerMode(IRobotContext robotConText)
        {
            try
            {
                if (IsAllLineSameIndexerMode(eINDEXER_OPERATION_MODE.CHANGER_MODE))
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage("Indexer Operation Changer Mode");
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

        [UniAuto.UniBCS.OpiSpec.Help("RC0076")]
        public bool RouteCondition_Cell_SOR_GradeMode(IRobotContext robotConText)
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

                //1 : Grade Mode,  2 : Flag Mode
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
                    errMsg = "RunMode is Not Grade Mode";  //Watson 20160227 Modify 必須回拋錯誤，儘量簡短、易懂

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

        [UniAuto.UniBCS.OpiSpec.Help("RC0077")]
        public bool RouteCondition_Cell_SOR_FlagMode(IRobotContext robotConText)
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

                //1 : Grade Mode,  2 : Flag Mode
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
                    errMsg = "RunMode is Not Flag Mode";  //Watson 20160227 Modify 必須回拋錯誤，儘量簡短、易懂

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

        //20160218 add for Array CHN Sampling Mode
        [UniAuto.UniBCS.OpiSpec.Help("RC0083")]
        public bool RouteCondition_Array_ChangerSamplingMode(IRobotContext robotConText)
        {
            try
            {
                if (IsAllLineSameIndexerMode(eINDEXER_OPERATION_MODE.SAMPLING_MODE))
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
                    robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
                    return true;
                }
                else
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage("Indexer Operation Mode is not Sampling Mode");
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

    }
}
