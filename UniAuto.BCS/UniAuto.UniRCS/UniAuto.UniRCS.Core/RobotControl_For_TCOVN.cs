using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using System.Threading;
using UniAuto.UniBCS.PLCAgent.PLC;

namespace UniAuto.UniRCS.Core
{
    public partial class RobotCoreService
    {
        /// <summary>
        /// FOR TCOVN ITO PL, 檢查Cmd是否可以轉成Get Get Put Put或是Both Arm Get Both Arm Put
        /// </summary>
        /// <param name="cmd01">第一筆 Job 的 Robot Command 01</param>
        /// <param name="cmd02">第一筆 Job 的 Robot Command 02</param>
        /// <param name="cmd01_2nd">第二筆 Job 的 Robot Command 01</param>
        /// <param name="cmd02_2nd">第二筆 Job 的 Robot Command 02</param>
        /// <returns></returns>
        private bool CheckTCOVNGetGetPutPutRobotCommand(DefineNormalRobotCmd job01_cmd01, DefineNormalRobotCmd job01_cmd02, DefineNormalRobotCmd job02_cmd01, DefineNormalRobotCmd job02_cmd02)
        {
            RobotCmdInfo ret = NewRobotCmdInfo(job01_cmd01, job01_cmd02, job02_cmd01, job02_cmd02);
            return ret != null;
        }

        private bool TargetSlotCanBothGetBothPut(DefineNormalRobotCmd job01_cmd, DefineNormalRobotCmd job02_cmd)
        {
            bool ret = false;
            if ((job01_cmd.Cmd01_Command == eRobot_ControlCommand.GET || job01_cmd.Cmd01_Command == eRobot_ControlCommand.PUT) &&
                job01_cmd.Cmd01_Command == job02_cmd.Cmd01_Command &&//兩個Job的命令相同是Get或Put
                job01_cmd.Cmd01_TargetPosition == job02_cmd.Cmd01_TargetPosition &&//兩個Job的目的Postiion相同
                Math.Abs(job01_cmd.Cmd01_TargetSlotNo - job02_cmd.Cmd01_TargetSlotNo) == 1)//兩個Job的目的Slot相差一
            {
                DefineNormalRobotCmd low_slot = (job01_cmd.Cmd01_TargetSlotNo <= job02_cmd.Cmd01_TargetSlotNo) ? job01_cmd : job02_cmd;
                ret = (low_slot.Cmd01_TargetSlotNo % 2 == 1);//Both Arm Get 或 Both Arm Put 時, 下層Slot必須是單數
            }
            return ret;
        }

        private bool TargetPositionIsPort(DefineNormalRobotCmd cmd)
        {
            bool ret = false;
            if (cmd.Cmd01_TargetPosition >= 1 && cmd.Cmd01_TargetPosition <= 10)
            {
                ret = true;
            }
            return ret;
        }

        private bool TargetPositionIsOven(DefineNormalRobotCmd cmd)
        {
            bool ret = false;
            if ((Workbench.LineType == eLineType.ARRAY.OVNPL_YAC && cmd.Cmd01_TargetPosition >= 11 && cmd.Cmd01_TargetPosition <= 13) ||// OVNPL 的 Oven Stage 是 11,12,13
                (Workbench.LineType == eLineType.ARRAY.OVNITO_CSUN && (cmd.Cmd01_TargetPosition == 11 || cmd.Cmd01_TargetPosition == 13)))// OVNITO 的 Oven Stage 是 11,13
            {
                ret = true;
            }
            return ret;
        }

        private bool TargetPositionIsCooler(DefineNormalRobotCmd cmd)
        {
            bool ret = false;
            if ((Workbench.LineType == eLineType.ARRAY.OVNPL_YAC && cmd.Cmd01_TargetPosition == 14) ||// OVNPL 的 Cooler Stage 是 14
                (Workbench.LineType == eLineType.ARRAY.OVNITO_CSUN && cmd.Cmd01_TargetPosition == 12))// OVNITO 的 Cooler Stage 是 12
            {
                ret = true;
            }
            return ret;
        }

        /// <summary>
        /// 根據RobotWIP.CurLocation_StageID, 更新RobotWIP.CurLocation_StagePriority
        /// 注意! RobotWIP.CurLocation_StagePriority的初始值是null
        /// </summary>
        /// <param name="CanControlJobList"></param>
        private void UpdateStagePriority(List<Job> CanControlJobList)
        {
            foreach (Job job in CanControlJobList)
            {
                if (job.RobotWIP.CurLocation_StageID == eRobotCommonConst.ROBOT_HOME_STAGEID)
                    job.RobotWIP.CurLocation_StagePriority = eRobotCommonConst.ROBOT_STAGE_HIGTEST_PRIORITY;
                else
                {
                    RobotStage stage = ObjectManager.RobotStageManager.GetRobotStagebyStageID(job.RobotWIP.CurLocation_StageID);
                    if (stage != null)
                        job.RobotWIP.CurLocation_StagePriority = stage.Data.PRIORITY.ToString().PadLeft(2, '0');
                    else
                        job.RobotWIP.CurLocation_StagePriority = eRobotCommonConst.ROBOT_STAGE_LOWEST_PRIORITY;
                }
            }
        }

        /// <summary>
        /// FOR TCOVN ITO PL, 轉換成Get Get 或是 Put Put或是 Both Arm Get Both Arm Put
        /// </summary>
        /// <param name="job01_cmd01">第一筆 Job 的 Robot Command 01</param>
        /// <param name="job01_cmd02">第一筆 Job 的 Robot Command 02</param>
        /// <param name="job02_cmd01">第二筆 Job 的 Robot Command 01</param>
        /// <param name="job02_cmd02">第二筆 Job 的 Robot Command 02</param>
        /// <returns></returns>
        private RobotCmdInfo NewRobotCmdInfo(DefineNormalRobotCmd job01_cmd01, DefineNormalRobotCmd job01_cmd02, DefineNormalRobotCmd job02_cmd01, DefineNormalRobotCmd job02_cmd02)
        {
            // OVN ITO PL 的 Robot 動作
            // 1. 從Port出, 要進Oven; 組成一筆 Get, Get; OVN PL 不能跨 Cassette 取片
            // 2. 在Arm上, 要進Oven; 組成一筆 Both Arm Put
            // 3. 從Oven出, 要進Cooler; 組成一筆 Both Arm Get, Both Arm Put
            // 4. 從Cooler出, 要進Port; 組成一筆 Both Arm Get
            // 5. 在Arm上, 要進Port; 組成一筆 Put, Put
            RobotCmdInfo ret = null;
            if (job01_cmd01 != null && job01_cmd02 != null && job02_cmd01 != null && job02_cmd02 != null)
            {
                if (job01_cmd01.Cmd01_Command == eRobot_ControlCommand.GET && TargetPositionIsPort(job01_cmd01) &&
                    job02_cmd01.Cmd01_Command == eRobot_ControlCommand.GET && TargetPositionIsPort(job02_cmd01))
                {
                    // 1. 從Port出, 要進Oven; 組成一筆 Get, Get
                    DefineNormalRobotCmd low_slot = null, upper_slot = null;
                    if (job01_cmd01.Cmd01_TargetPosition == job02_cmd01.Cmd01_TargetPosition)
                    {
                        // 兩個Job來自同一個Port, 下手臂取下層Slot, 上手臂取上層Slot
                        low_slot = (job01_cmd01.Cmd01_TargetSlotNo <= job02_cmd01.Cmd01_TargetSlotNo) ? job01_cmd01 : job02_cmd01;
                        upper_slot = (job01_cmd01.Cmd01_TargetSlotNo <= job02_cmd01.Cmd01_TargetSlotNo) ? job02_cmd01 : job01_cmd01;
                    }
                    else
                    {
                        // 兩個Job來自不同Port
                        if (Workbench.LineType == eLineType.ARRAY.OVNPL_YAC)
                        {
                            // 兩個Job來自不同Port, OVN PL 不跨 Cassette 取片
                            // do nothing
                        }
                        else
                        {
                            // 兩個Job來自不同Port, 下手臂取job01, 上手臂取job02
                            low_slot = job01_cmd01;
                            upper_slot = job02_cmd01;
                        }
                    }

                    if (low_slot != null && upper_slot != null)
                    {
                        ret = new RobotCmdInfo();
                        ret.Cmd01_ArmSelect = eRobot_ArmSelect.LOWER;
                        ret.Cmd01_Command = eRobot_ControlCommand.GET;
                        ret.Cmd01_TargetPosition = low_slot.Cmd01_TargetPosition;
                        ret.Cmd01_TargetSlotNo = low_slot.Cmd01_TargetSlotNo;
                        ret.Cmd01_CSTSeq = string.IsNullOrEmpty(low_slot.Cmd01_CstSeq) ? 0 : int.Parse(low_slot.Cmd01_CstSeq);
                        ret.Cmd01_JobSeq = string.IsNullOrEmpty(low_slot.Cmd01_JobSeq) ? 0 : int.Parse(low_slot.Cmd01_JobSeq);

                        ret.Cmd02_ArmSelect = eRobot_ArmSelect.UPPER;
                        ret.Cmd02_Command = eRobot_ControlCommand.GET;
                        ret.Cmd02_TargetPosition = upper_slot.Cmd01_TargetPosition;
                        ret.Cmd02_TargetSlotNo = upper_slot.Cmd01_TargetSlotNo;
                        ret.Cmd02_CSTSeq = string.IsNullOrEmpty(upper_slot.Cmd01_CstSeq) ? 0 : int.Parse(upper_slot.Cmd01_CstSeq);
                        ret.Cmd02_JobSeq = string.IsNullOrEmpty(upper_slot.Cmd01_JobSeq) ? 0 : int.Parse(upper_slot.Cmd01_JobSeq);
                    }
                    else
                    {
                        // do nothing
                    }
                }
                else if (job01_cmd01.Cmd01_Command == eRobot_ControlCommand.GET && TargetPositionIsOven(job01_cmd01) &&
                         job02_cmd01.Cmd01_Command == eRobot_ControlCommand.GET && TargetPositionIsOven(job02_cmd01))
                {
                    // 3. 從Oven出
                    if (TargetSlotCanBothGetBothPut(job01_cmd01, job02_cmd01))
                    {
                        // 3. 從同一Oven出, 且 TargetSlot 緊鄰; 組成一筆 Both Arm Get
                        DefineNormalRobotCmd oven_low_slot = (job01_cmd01.Cmd01_TargetSlotNo <= job02_cmd01.Cmd01_TargetSlotNo) ? job01_cmd01 : job02_cmd01;
                        ret = new RobotCmdInfo();
                        ret.Cmd01_ArmSelect = eRobot_ArmSelect.BOTH;
                        ret.Cmd01_Command = eRobot_ControlCommand.GET;
                        ret.Cmd01_TargetPosition = oven_low_slot.Cmd01_TargetPosition;
                        ret.Cmd01_TargetSlotNo = oven_low_slot.Cmd01_TargetSlotNo;
                        ret.Cmd01_CSTSeq = string.IsNullOrEmpty(oven_low_slot.Cmd01_CstSeq) ? 0 : int.Parse(oven_low_slot.Cmd01_CstSeq);
                        ret.Cmd01_JobSeq = string.IsNullOrEmpty(oven_low_slot.Cmd01_JobSeq) ? 0 : int.Parse(oven_low_slot.Cmd01_JobSeq);

                        if (job01_cmd02.Cmd01_Command == eRobot_ControlCommand.PUT && TargetPositionIsCooler(job01_cmd02) &&
                            job02_cmd02.Cmd01_Command == eRobot_ControlCommand.PUT && TargetPositionIsCooler(job02_cmd02) &&
                            TargetSlotCanBothGetBothPut(job01_cmd02, job02_cmd02))
                        {
                            // 3. 從同一Oven出 且 可 Both Arm Get, 要進同一Cooler 且可 Both Arm Put
                            DefineNormalRobotCmd cooler_low_slot = (job01_cmd02.Cmd01_TargetSlotNo <= job02_cmd02.Cmd01_TargetSlotNo) ? job01_cmd02 : job02_cmd02;
                            ret.Cmd02_ArmSelect = eRobot_ArmSelect.BOTH;
                            ret.Cmd02_Command = eRobot_ControlCommand.PUT;
                            ret.Cmd02_TargetPosition = cooler_low_slot.Cmd01_TargetPosition;
                            ret.Cmd02_TargetSlotNo = cooler_low_slot.Cmd01_TargetSlotNo;
                            ret.Cmd02_CSTSeq = string.IsNullOrEmpty(cooler_low_slot.Cmd01_CstSeq) ? 0 : int.Parse(cooler_low_slot.Cmd01_CstSeq);
                            ret.Cmd02_JobSeq = string.IsNullOrEmpty(cooler_low_slot.Cmd01_JobSeq) ? 0 : int.Parse(cooler_low_slot.Cmd01_JobSeq);
                        }
                        else
                        {
                            // 從 OVN 出但不能 Both Arm Put 到 Cooler, do nothing
                        }
                    }
                }
                else if (job01_cmd01.Cmd01_Command == eRobot_ControlCommand.GET && TargetPositionIsCooler(job01_cmd01) &&
                         job02_cmd01.Cmd01_Command == eRobot_ControlCommand.GET && TargetPositionIsCooler(job02_cmd01))
                {
                    // 4. 從Cooler出
                    if (TargetSlotCanBothGetBothPut(job01_cmd01, job02_cmd01))
                    {
                        // 4. 從Cooler出, 要進Port; 組成一筆 Both Arm Get
                        DefineNormalRobotCmd cooler_low_slot = (job01_cmd01.Cmd01_TargetSlotNo <= job02_cmd01.Cmd01_TargetSlotNo) ? job01_cmd01 : job02_cmd01;
                        ret = new RobotCmdInfo();
                        ret.Cmd01_ArmSelect = eRobot_ArmSelect.BOTH;
                        ret.Cmd01_Command = eRobot_ControlCommand.GET;
                        ret.Cmd01_TargetPosition = cooler_low_slot.Cmd01_TargetPosition;
                        ret.Cmd01_TargetSlotNo = cooler_low_slot.Cmd01_TargetSlotNo;
                        ret.Cmd01_CSTSeq = string.IsNullOrEmpty(cooler_low_slot.Cmd01_CstSeq) ? 0 : int.Parse(cooler_low_slot.Cmd01_CstSeq);
                        ret.Cmd01_JobSeq = string.IsNullOrEmpty(cooler_low_slot.Cmd01_JobSeq) ? 0 : int.Parse(cooler_low_slot.Cmd01_JobSeq);
                    }
                    else
                    {
                        // 從 Cooler 出但不能 Both Arm Get
                        ret = new RobotCmdInfo();
                        ret.Cmd01_Command = job01_cmd01.Cmd01_Command;
                        ret.Cmd01_ArmSelect = job01_cmd01.Cmd01_ArmSelect;
                        ret.Cmd01_TargetPosition = job01_cmd01.Cmd01_TargetPosition;
                        ret.Cmd01_TargetSlotNo = job01_cmd01.Cmd01_TargetSlotNo;
                        ret.Cmd01_CSTSeq = string.IsNullOrEmpty(job01_cmd01.Cmd01_CstSeq) ? 0 : int.Parse(job01_cmd01.Cmd01_CstSeq);
                        ret.Cmd01_JobSeq = string.IsNullOrEmpty(job01_cmd01.Cmd01_JobSeq) ? 0 : int.Parse(job01_cmd01.Cmd01_JobSeq);

                        ret.Cmd02_Command = job02_cmd01.Cmd01_Command;
                        ret.Cmd02_ArmSelect = job02_cmd01.Cmd01_ArmSelect;
                        ret.Cmd02_TargetPosition = job02_cmd01.Cmd01_TargetPosition;
                        ret.Cmd02_TargetSlotNo = job02_cmd01.Cmd01_TargetSlotNo;
                        ret.Cmd02_CSTSeq = string.IsNullOrEmpty(job02_cmd01.Cmd01_CstSeq) ? 0 : int.Parse(job02_cmd01.Cmd01_CstSeq);
                        ret.Cmd02_JobSeq = string.IsNullOrEmpty(job02_cmd01.Cmd01_JobSeq) ? 0 : int.Parse(job02_cmd01.Cmd01_JobSeq);
                    }
                }
                else
                {
                    // 不是從 Port/Oven/Cooler 取, do nothing
                }
            }
            if (ret == null && job01_cmd01 != null && job02_cmd01 != null &&
                job01_cmd01.Cmd01_Command == eRobot_ControlCommand.PUT && job02_cmd01.Cmd01_Command == eRobot_ControlCommand.PUT)
            {
                // 兩個Job的命令都是Put, 表示Job在Arm上
                if (TargetPositionIsPort(job01_cmd01) && TargetPositionIsPort(job02_cmd01))
                {
                    // 5. 在Arm上, 要進Port; 組成一筆 Put, Put
                    ret = new RobotCmdInfo();
                    ret.Cmd01_Command = job01_cmd01.Cmd01_Command;
                    ret.Cmd01_ArmSelect = job01_cmd01.Cmd01_ArmSelect;
                    ret.Cmd01_TargetPosition = job01_cmd01.Cmd01_TargetPosition;
                    ret.Cmd01_TargetSlotNo = job01_cmd01.Cmd01_TargetSlotNo;
                    ret.Cmd01_CSTSeq = string.IsNullOrEmpty(job01_cmd01.Cmd01_CstSeq) ? 0 : int.Parse(job01_cmd01.Cmd01_CstSeq);
                    ret.Cmd01_JobSeq = string.IsNullOrEmpty(job01_cmd01.Cmd01_JobSeq) ? 0 : int.Parse(job01_cmd01.Cmd01_JobSeq);

                    ret.Cmd02_Command = job02_cmd01.Cmd01_Command;
                    ret.Cmd02_ArmSelect = job02_cmd01.Cmd01_ArmSelect;
                    ret.Cmd02_TargetPosition = job02_cmd01.Cmd01_TargetPosition;
                    ret.Cmd02_TargetSlotNo = job02_cmd01.Cmd01_TargetSlotNo;
                    ret.Cmd02_CSTSeq = string.IsNullOrEmpty(job02_cmd01.Cmd01_CstSeq) ? 0 : int.Parse(job02_cmd01.Cmd01_CstSeq);
                    ret.Cmd02_JobSeq = string.IsNullOrEmpty(job02_cmd01.Cmd01_JobSeq) ? 0 : int.Parse(job02_cmd01.Cmd01_JobSeq);
                }
                else if ((TargetPositionIsOven(job01_cmd01) && TargetPositionIsOven(job02_cmd01)) ||
                         (TargetPositionIsCooler(job01_cmd01) && TargetPositionIsCooler(job02_cmd01)))
                {
                    // 2. 在Arm上, 不進 Port, 那麼就是要進 Oven 或 Cooler
                    if (TargetSlotCanBothGetBothPut(job01_cmd01, job02_cmd01))
                    {
                        // 2. 在Arm上, 要進同一Oven/Cooler 且 TargetSlot 緊鄰; 組成一筆 Both Arm Put
                        DefineNormalRobotCmd low_slot = (job01_cmd01.Cmd01_TargetSlotNo <= job02_cmd01.Cmd01_TargetSlotNo) ? job01_cmd01 : job02_cmd01;

                        ret = new RobotCmdInfo();
                        ret.Cmd01_ArmSelect = eRobot_ArmSelect.BOTH;
                        ret.Cmd01_Command = eRobot_ControlCommand.PUT;
                        ret.Cmd01_TargetPosition = low_slot.Cmd01_TargetPosition;
                        ret.Cmd01_TargetSlotNo = low_slot.Cmd01_TargetSlotNo;
                        ret.Cmd01_CSTSeq = string.IsNullOrEmpty(low_slot.Cmd01_CstSeq) ? 0 : int.Parse(low_slot.Cmd01_CstSeq);
                        ret.Cmd01_JobSeq = string.IsNullOrEmpty(low_slot.Cmd01_JobSeq) ? 0 : int.Parse(low_slot.Cmd01_JobSeq);
                    }
                    else
                    {
                        // 2. 在Arm上, 但不進同一Oven/Cooler 或 進同一Oven/Cooler但TargetSlot不緊鄰; 組成一筆 Put, Put
                        ret = new RobotCmdInfo();
                        ret.Cmd01_Command = job01_cmd01.Cmd01_Command;
                        ret.Cmd01_ArmSelect = job01_cmd01.Cmd01_ArmSelect;
                        ret.Cmd01_TargetPosition = job01_cmd01.Cmd01_TargetPosition;
                        ret.Cmd01_TargetSlotNo = job01_cmd01.Cmd01_TargetSlotNo;
                        ret.Cmd01_CSTSeq = string.IsNullOrEmpty(job01_cmd01.Cmd01_CstSeq) ? 0 : int.Parse(job01_cmd01.Cmd01_CstSeq);
                        ret.Cmd01_JobSeq = string.IsNullOrEmpty(job01_cmd01.Cmd01_JobSeq) ? 0 : int.Parse(job01_cmd01.Cmd01_JobSeq);

                        ret.Cmd02_Command = job02_cmd01.Cmd01_Command;
                        ret.Cmd02_ArmSelect = job02_cmd01.Cmd01_ArmSelect;
                        ret.Cmd02_TargetPosition = job02_cmd01.Cmd01_TargetPosition;
                        ret.Cmd02_TargetSlotNo = job02_cmd01.Cmd01_TargetSlotNo;
                        ret.Cmd02_CSTSeq = string.IsNullOrEmpty(job02_cmd01.Cmd01_CstSeq) ? 0 : int.Parse(job02_cmd01.Cmd01_CstSeq);
                        ret.Cmd02_JobSeq = string.IsNullOrEmpty(job02_cmd01.Cmd01_JobSeq) ? 0 : int.Parse(job02_cmd01.Cmd01_JobSeq);
                    }
                }
                else
                {
                    // 兩個Job在Arm上, 但不是都要進 Port/OVen/Cooler, 單純Put, Put
                    ret = new RobotCmdInfo();
                    ret.Cmd01_Command = job01_cmd01.Cmd01_Command;
                    ret.Cmd01_ArmSelect = job01_cmd01.Cmd01_ArmSelect;
                    ret.Cmd01_TargetPosition = job01_cmd01.Cmd01_TargetPosition;
                    ret.Cmd01_TargetSlotNo = job01_cmd01.Cmd01_TargetSlotNo;
                    ret.Cmd01_CSTSeq = string.IsNullOrEmpty(job01_cmd01.Cmd01_CstSeq) ? 0 : int.Parse(job01_cmd01.Cmd01_CstSeq);
                    ret.Cmd01_JobSeq = string.IsNullOrEmpty(job01_cmd01.Cmd01_JobSeq) ? 0 : int.Parse(job01_cmd01.Cmd01_JobSeq);

                    ret.Cmd02_Command = job02_cmd01.Cmd01_Command;
                    ret.Cmd02_ArmSelect = job02_cmd01.Cmd01_ArmSelect;
                    ret.Cmd02_TargetPosition = job02_cmd01.Cmd01_TargetPosition;
                    ret.Cmd02_TargetSlotNo = job02_cmd01.Cmd01_TargetSlotNo;
                    ret.Cmd02_CSTSeq = string.IsNullOrEmpty(job02_cmd01.Cmd01_CstSeq) ? 0 : int.Parse(job02_cmd01.Cmd01_CstSeq);
                    ret.Cmd02_JobSeq = string.IsNullOrEmpty(job02_cmd01.Cmd01_JobSeq) ? 0 : int.Parse(job02_cmd01.Cmd01_JobSeq);
                }
            }
            return ret;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="job01_cmd01">第一筆 Job 的 Robot Command 01</param>
        /// <param name="job01_cmd02">第一筆 Job 的 Robot Command 02</param>
        /// <returns></returns>
        private RobotCmdInfo NewRobotCmdInfo(DefineNormalRobotCmd job01_cmd01, DefineNormalRobotCmd job01_cmd02)
        {
            RobotCmdInfo ret = null;
            if (job01_cmd01 != null && job01_cmd02 != null)
            {
                ret = new RobotCmdInfo();
                ret.Cmd01_Command = job01_cmd01.Cmd01_Command;
                ret.Cmd01_ArmSelect = job01_cmd01.Cmd01_ArmSelect;
                ret.Cmd01_TargetPosition = job01_cmd01.Cmd01_TargetPosition;
                ret.Cmd01_TargetSlotNo = job01_cmd01.Cmd01_TargetSlotNo;
                ret.Cmd01_CSTSeq = string.IsNullOrEmpty(job01_cmd01.Cmd01_CstSeq) ? 0 : int.Parse(job01_cmd01.Cmd01_CstSeq);
                ret.Cmd01_JobSeq = string.IsNullOrEmpty(job01_cmd01.Cmd01_JobSeq) ? 0 : int.Parse(job01_cmd01.Cmd01_JobSeq);

                ret.Cmd02_Command = job01_cmd02.Cmd01_Command;
                ret.Cmd02_ArmSelect = job01_cmd02.Cmd01_ArmSelect;
                ret.Cmd02_TargetPosition = job01_cmd02.Cmd01_TargetPosition;
                ret.Cmd02_TargetSlotNo = job01_cmd02.Cmd01_TargetSlotNo;
                ret.Cmd02_CSTSeq = string.IsNullOrEmpty(job01_cmd02.Cmd01_CstSeq) ? 0 : int.Parse(job01_cmd02.Cmd01_CstSeq);
                ret.Cmd02_JobSeq = string.IsNullOrEmpty(job01_cmd02.Cmd01_CstSeq) ? 0 : int.Parse(job01_cmd02.Cmd01_JobSeq);
            }
            return ret;
        }

        /// <summary> for Robot TypeI[ One Robot has 2 Arm,Arm#01(Upper),Arm#02(Lower) ,One Arm has One Job Position.
        ///
        /// </summary>
        /// <param name="curRobot"></param>
        /// <param name="curRobotAllStageList"></param>
        private void CheckRobotControlCommand_For_TCOVN(Robot curRobot, List<RobotStage> curRobotAllStageList)
        {
            string strlog = string.Empty;
            List<Job> robotArmCanControlJobList_OrderBy = new List<Job>();
            List<Job> robotStageCanControlJobList_OrderBy = new List<Job>();
            try
            {
                curRobot.Context = new RobotContext();
                curRobot.Context.AddParameter(eRobotContextParameter.TCOVN_PL_ITO_RobotParam, new TCOVN_PL_ITO_RobotParam());

                #region [ 1. Check Can Issue Command ]

                if (CheckCanIssueRobotCommand(curRobot) == false)
                {
                    return;
                }

                #endregion

                if (StaticContext.ContainsKey(eRobotContextParameter.FixTargetStage_RobotParam))
                {
                    FixTargetStage_RobotParam fix_param = (FixTargetStage_RobotParam)StaticContext[eRobotContextParameter.FixTargetStage_RobotParam];
                    if (fix_param.SetFixDateTime)
                    {
                        fix_param.FixDateTime = DateTime.Now;
                        fix_param.SetFixDateTime = false;
                    }
                }

                #region [ 2. Get Arm Can Control Job List, Stage Can Control Job List and Update StageInfo ][ Wait_Proc_0003 ]

                //One Robot Only One Select Rule,如有MIX Route則在Check FetchOut與Filter後 先照Route Priority排序再照STEP排序 以達到優先處理XX Route.如有其他特殊選片邏輯在特別處理
                #region [ Handle Robot Current Rule Job Select Function List ]

                #region [ Clear All Stage UDRQ And LDRQ Stage SlotNoList Info ]

                foreach (RobotStage stageItem in curRobotAllStageList)
                {
                    lock (stageItem)
                    {
                        stageItem.curLDRQ_EmptySlotList.Clear();
                        stageItem.curUDRQ_SlotList.Clear();
                        //20160302 add for Array Only
                        stageItem.CurRecipeGroupNoList.Clear();
                        stageItem.AllJobRecipeGroupNoList.Clear();
                    }
                }

                #endregion

                Dictionary<string, List<RobotRuleSelect>> curRuleJobSelectList = ObjectManager.RobotManager.GetRouteSelect(curRobot.Data.ROBOTNAME);

                bool checkFlag = false;
                IRobotContext robotConText = new RobotContext();
                string fail_ReasonCode = string.Empty;
                string failMsg = string.Empty;

                #region [ Check Select Rule Exist ]

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00010 ] 
                fail_ReasonCode = eRobot_CheckFail_Reason.CAN_ISSUE_CMD_ROBOT_SELECTRULE_IS_NULL;

                if (curRuleJobSelectList == null)
                {

                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not get any Select Rule!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00010 ]

                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not get any cSelect Rule!",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        failMsg = string.Format("Can not get any Select Rule!");

                        AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion

                    }

                    #endregion
                    return;
                }
                else
                {
                    //Clear[ Robot_Fail_Case_00010 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                }

                #endregion

                //此時Robot無法得知要跑哪種Route,所以只會有一筆[ Wait_For_Proc_00026 ] 之後Table要拿掉RouteID以免誤解的相關處理
                foreach (string routeID in curRuleJobSelectList.Keys)
                {

                    #region [ 根據RuleJobSelect選出Can Control Job List ]

                    #region [ Initial Select Rule List RobotConText Info. 搭配針對Select Rule 會用到的相關元件給參數!注意Add只能是Obj 不可以是int or String value Type! ] =====================================================================

                    robotConText.AddParameter(eRobotContextParameter.CurRobotEntity, curRobot);
                    robotConText.AddParameter(eRobotContextParameter.CurRobotAllStageListEntity, curRobotAllStageList);

                    #endregion =======================================================================================================================================================

                    foreach (RobotRuleSelect curRuleJobSelect in curRuleJobSelectList[routeID])
                    {
                        //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_E0001 ] ,以Rule Job Select 的ObjectName與MethodName為Key來決定是否紀錄Log
                        fail_ReasonCode = string.Format("{0}_{1}", curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME);

                        #region[DebugLog][ Start Rule Job Select Function ]

                        if (IsShowDetialLog == true)
                        {

                            strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Rule Job Select object({2}) MethodName({3}) IsEnable({4}) Start {5}",
                                                    curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME,
                                                    curRuleJobSelect.Data.ISENABLED, new string(eRobotCommonConst.RULE_SELECT_START_CHAR, eRobotCommonConst.RULE_SELECT_START_CHAR_LENGTH));

                            Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                        }

                        #endregion

                        if (curRuleJobSelect.Data.ISENABLED == eRobotCommonConst.DB_FUNCTION_IS_ENABLE)
                        {
                            checkFlag = (bool)Invoke(curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME, new object[] { robotConText });

                            if (checkFlag == false)
                            {
                                #region[DebugLog][ End Rule Job Select Function ]

                                if (IsShowDetialLog == true)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Rule Job Select Fail, object({2}) MethodName({3}) RtnCode({4})  RtnMsg({5}]!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME,
                                                            robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);


                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Rule Job Select object({2}) MethodName({3}) IsEnable({4}) End {5}",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME,
                                                            curRuleJobSelect.Data.ISENABLED, new string(eRobotCommonConst.RULE_SELECT_END_CHAR, eRobotCommonConst.RULE_SELECT_END_CHAR_LENGTH));

                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion

                                #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_E0001 ]

                                if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Rule Job Select Fail, object({2}) MethodName({3}) RtnCode({4})  RtnMsg({5}]!",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME,
                                                            robotConText.GetReturnCode(), robotConText.GetReturnMessage());

                                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                                    #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                                    //failMsg = string.Format("Robot({0}) object({1}) MethodName({2}) RtnCode({3})  RtnMsg({4}]!",
                                    //                        curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME, robotConText.GetReturnCode(),
                                    //                        robotConText.GetReturnMessage());

                                    failMsg = string.Format("RtnCode({0})  RtnMsg({1})!",
                                                            robotConText.GetReturnCode(),
                                                            robotConText.GetReturnMessage());

                                    AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                                    SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                                    #endregion

                                }

                                #endregion

                                //有重大異常直接結束配片邏輯要求人員介入處理
                                //20160114 modify SEMI Mode 還是要可以執行下一個Select 條件.不須結束配片邏輯
                                if (curRobot.File.curRobotRunMode == eRobot_RunMode.AUTO_MODE)
                                {
                                    return;
                                }
                            }
                            else
                            {
                                //Clear[ Robot_Fail_Case_E0001 ]
                                RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);

                                #region[DebugLog][ End Rule Job Select Function ]

                                if (IsShowDetialLog == true)
                                {

                                    strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Rule Job Select object({2}) MethodName({3}) IsEnable({4}) End {5}",
                                                            curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME,
                                                            curRuleJobSelect.Data.ISENABLED, new string(eRobotCommonConst.RULE_SELECT_END_CHAR, eRobotCommonConst.RULE_SELECT_END_CHAR_LENGTH));

                                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                                }

                                #endregion
                            }
                        }
                        else
                        {
                            #region[DebugLog][ End Rule Job Select Function ]

                            if (IsShowDetialLog == true)
                            {

                                strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) Rule Job Select object({2}) MethodName({3}) IsEnable({4}) End {5}",
                                                        curRobot.Data.NODENO, curRobot.Data.ROBOTNAME, curRuleJobSelect.Data.OBJECTNAME, curRuleJobSelect.Data.METHODNAME,
                                                        curRuleJobSelect.Data.ISENABLED, new string(eRobotCommonConst.RULE_SELECT_END_CHAR, eRobotCommonConst.RULE_SELECT_END_CHAR_LENGTH));

                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                            }

                            #endregion
                        }

                    }

                    #endregion

                    //目前只處理第一筆
                    break;

                }

                #endregion

                #region [ Get Arm Can Control Job List ]

                List<Job> robotArmCanControlJobList;

                robotArmCanControlJobList = (List<Job>)robotConText[eRobotContextParameter.ArmCanControlJobList];

                //當沒有設定參數時會回傳NULL,需防呆
                if (robotArmCanControlJobList == null)
                {
                    robotArmCanControlJobList = new List<Job>();
                }

                #endregion

                #region [ Get Stage Can Control Job List ]

                List<Job> robotStageCanControlJobList;

                robotStageCanControlJobList = (List<Job>)robotConText[eRobotContextParameter.StageCanControlJobList];

                //當沒有設定參數時會回傳NULL,需防呆
                if (robotStageCanControlJobList == null)
                {
                    robotStageCanControlJobList = new List<Job>();
                }

                #endregion

                #endregion

                #region [ 3. Update OPI Stage Display Info ][ Wait_Proc_0005 ]

                bool sendToOPI = false;

                foreach (RobotStage stage_entity in curRobotAllStageList)
                {
                    if (stage_entity.File.StatusChangeFlag == true)
                    {
                        sendToOPI = true;

                        lock (stage_entity.File)
                        {
                            stage_entity.File.StatusChangeFlag = false;
                        }
                    }

                }

                if (sendToOPI == true)
                {
                    //通知OPI更新LayOut畫面 //20151126 add by Robot Arm Qty來區分送給OPI的狀態訊息
                    Invoke(eServiceName.UIService, "RobotStageInfoReport", new object[] { curRobot.Data.LINEID , curRobot });
                }

                #endregion

                #region [ 如果是SEMI Mode只需做到取得目前可控制Job並更新資訊即可 ]

                if (curRobot.File.curRobotRunMode == eRobot_RunMode.SEMI_MODE)
                {
                    return;
                }

                #endregion

                #region [ 更新OPI畫面後 Check Can Control Job Exist ]

                //Set want To Check Function Fail_ReasonCode[ Robot_Fail_Case_00006 ] 
                fail_ReasonCode = eRobot_CheckFail_Reason.GET_CAN_CONTROL_JOB_FAIL;

                if (robotArmCanControlJobList.Count == 0 && robotStageCanControlJobList.Count == 0)
                {
                    #region[DebugLog]

                    if (IsShowDetialLog == true)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not get any can control Job!(Please check 1.Robot Arm would have Job 2.CST(Load or Both port,wait for process or In process) is Ready and Exist(JobEachCassetteSlotExists and JobEachCassetteSlotPosition) 3.Upstream EQP would SendOut Job)",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                    }

                    #endregion

                    #region [ Add To Check Fail Message To Robot ][ Robot_Fail_Case_00006 ]

                    if (curRobot.CheckFailMessageList.ContainsKey(fail_ReasonCode) == false)
                    {

                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Robot({1}) can not get any can control Job!(Please check 1.Robot Arm would have Job 2.CST(Load or Both port,wait for process or In process) is Ready and Exist(JobEachCassetteSlotExists and JobEachCassetteSlotPosition) 3.Upstream EQP would SendOut Job)",
                                                curRobot.Data.NODENO, curRobot.Data.ROBOTNAME);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);

                        #region [ 記錄Fail Msg To OPI and Robot FailMsg ]

                        //failMsg = string.Format("Robot({0}) can not get any can control Job!",
                        //                         curRobot.Data.ROBOTNAME);

                        failMsg = string.Format("RtnCode({0}) RtnMsg({1})", fail_ReasonCode, "Can not get any can control Job!(Please check 1.Robot Arm would have Job 2.CST(Load or Both port,wait for process or In process) is Ready and Exist(JobEachCassetteSlotExists and JobEachCassetteSlotPosition) 3.Upstream EQP would SendOut Job)");

                        AddRobotCheckFailMsg(curRobot, fail_ReasonCode, failMsg);
                        SendCanNotIssueCmdMsgToOPI(curRobot, fail_ReasonCode, failMsg, eSendToOPIMsgType.AlarmType);

                        #endregion

                    }

                    #endregion

                    return;

                }
                else
                {
                    //Clear[ Robot_Fail_Case_00006 ]
                    RemoveRobotCheckFailMsg(curRobot, fail_ReasonCode);
                }

                #endregion

                int count_of_enable_arm = 0;
                foreach (RobotArmSignalSubstrateInfo info in curRobot.CurTempArmSingleJobInfoList)
                {
                    if (info.ArmDisableFlag == eArmDisableStatus.Enable)
                        count_of_enable_arm++;
                }
                DefineNormalRobotCmd job01_cmd01 = null, job01_cmd02 = null, job02_cmd01 = null, job02_cmd02 = null;
                #region [ Handle Robot Arm Job List First ]
                if (count_of_enable_arm > 0 && robotArmCanControlJobList.Count > 0)
                {
                    UpdateStagePriority(robotArmCanControlJobList);
                    //排序 以Step越小, PortStatus In_Prcess為優先處理
                    robotArmCanControlJobList_OrderBy = robotArmCanControlJobList.OrderByDescending(s => s.RobotWIP.CurStepNo).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ToList();
                    for (int i = 0; i < robotArmCanControlJobList_OrderBy.Count; i++)
                    {
                        Job curRobotArmJob = robotArmCanControlJobList_OrderBy[i];
                        if (job01_cmd01 == null && job01_cmd02 == null)
                        {
                            //找第一片
                            if (CheckRobotArmJobRouteCondition(curRobot, curRobotArmJob, out job01_cmd01, out job01_cmd02))
                            {
                                ((TCOVN_PL_ITO_RobotParam)curRobot.Context[eRobotContextParameter.TCOVN_PL_ITO_RobotParam]).Job01 = curRobotArmJob;
                                ((TCOVN_PL_ITO_RobotParam)curRobot.Context[eRobotContextParameter.TCOVN_PL_ITO_RobotParam]).Job01_Cmd01 = job01_cmd01;
                                ((TCOVN_PL_ITO_RobotParam)curRobot.Context[eRobotContextParameter.TCOVN_PL_ITO_RobotParam]).Job01_Cmd02 = job01_cmd02;
                                if (count_of_enable_arm == 1) break;
                            }
                            else
                                job01_cmd01 = job01_cmd02 = null;
                        }
                        else
                        {
                            //找第二片
                            if (CheckRobotArmJobRouteCondition(curRobot, curRobotArmJob, out job02_cmd01, out job02_cmd02))
                            {
                                //檢查第二片是否能與第一片匹配
                                if (CheckTCOVNGetGetPutPutRobotCommand(job01_cmd01, job01_cmd02, job02_cmd01, job02_cmd02))
                                {
                                    ((TCOVN_PL_ITO_RobotParam)curRobot.Context[eRobotContextParameter.TCOVN_PL_ITO_RobotParam]).Job02 = curRobotArmJob;
                                    ((TCOVN_PL_ITO_RobotParam)curRobot.Context[eRobotContextParameter.TCOVN_PL_ITO_RobotParam]).Job02_Cmd01 = job02_cmd01;
                                    ((TCOVN_PL_ITO_RobotParam)curRobot.Context[eRobotContextParameter.TCOVN_PL_ITO_RobotParam]).Job02_Cmd02 = job02_cmd02;
                                    break;
                                }
                                else
                                    job02_cmd01 = job02_cmd02 = null;
                            }
                            else
                            {
                                job02_cmd01 = job02_cmd02 = null;
                            }
                        }
                    }
                }
                else if(count_of_enable_arm > 0 && robotStageCanControlJobList.Count > 0)
                {
                    UpdateStagePriority(robotStageCanControlJobList);
                    //先依Stage Priority排序越大越優先, 再依Step排序越小越優先, 最後依CurPortCstStatusPriority排序越小越優先
                    //robotStageCanControlJobList_OrderBy = robotStageCanControlJobList.OrderByDescending(s => s.RobotWIP.CurLocation_StagePriority).ThenBy(s => s.RobotWIP.CurStepNo).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ToList();
                    //add sort by cst waitforstarttime 2016/03/29 cc.kuang
                    robotStageCanControlJobList_OrderBy = robotStageCanControlJobList.OrderByDescending(s => s.RobotWIP.CurLocation_StagePriority).ThenBy(s => s.RobotWIP.CurStepNo).ThenBy(s => s.RobotWIP.CurPortCstStatusPriority).ThenBy(s => s.WaitForProcessTime).ToList();
                    for (int i = 0; i < robotStageCanControlJobList_OrderBy.Count; i++)
                    {
                        Job curRobotStageJob = robotStageCanControlJobList_OrderBy[i];
                        if (job01_cmd01 == null && job01_cmd02 == null)
                        {
                            //找第一片
                            if (CheckRobotStageJobRouteCondition(curRobot, curRobotStageJob, out job01_cmd01, out job01_cmd02))
                            {
                                ((TCOVN_PL_ITO_RobotParam)curRobot.Context[eRobotContextParameter.TCOVN_PL_ITO_RobotParam]).Job01 = curRobotStageJob;
                                ((TCOVN_PL_ITO_RobotParam)curRobot.Context[eRobotContextParameter.TCOVN_PL_ITO_RobotParam]).Job01_Cmd01 = job01_cmd01;
                                ((TCOVN_PL_ITO_RobotParam)curRobot.Context[eRobotContextParameter.TCOVN_PL_ITO_RobotParam]).Job01_Cmd02 = job01_cmd02;
                                if (count_of_enable_arm == 1) break;
                            }
                            else
                                job01_cmd01 = job01_cmd02 = null;
                        }
                        else
                        {
                            //找第二片
                            if (CheckRobotStageJobRouteCondition(curRobot, curRobotStageJob, out job02_cmd01, out job02_cmd02))
                            {
                                //檢查第二片是否與第一片匹配
                                if (CheckTCOVNGetGetPutPutRobotCommand(job01_cmd01, job01_cmd02, job02_cmd01, job02_cmd02))
                                {
                                    ((TCOVN_PL_ITO_RobotParam)curRobot.Context[eRobotContextParameter.TCOVN_PL_ITO_RobotParam]).Job02 = curRobotStageJob;
                                    ((TCOVN_PL_ITO_RobotParam)curRobot.Context[eRobotContextParameter.TCOVN_PL_ITO_RobotParam]).Job02_Cmd01 = job02_cmd01;
                                    ((TCOVN_PL_ITO_RobotParam)curRobot.Context[eRobotContextParameter.TCOVN_PL_ITO_RobotParam]).Job02_Cmd02 = job02_cmd02;
                                    break;
                                }
                                else
                                    job02_cmd01 = job02_cmd02 = null;
                            }
                            else
                            {
                                job02_cmd01 = job02_cmd02 = null;
                            }
                        }
                    }
                }
                #endregion

                if (curRobot.Data.ARMJOBQTY == 1)
                {
                    RobotCmdInfo curRobotCommand = NewRobotCmdInfo(job01_cmd01, job01_cmd02, job02_cmd01, job02_cmd02);
                    if (curRobotCommand == null)
                        curRobotCommand = NewRobotCmdInfo(job01_cmd01, job01_cmd02);
                    if (curRobotCommand != null)                       
                        Invoke(eServiceName.RobotCommandService, "RobotControlCommandSend", new object[] { curRobot, curRobotCommand });

                    //add by yang 2017/2/23
                    if (curRobot.CheckErrorList.Where(s => s.Value.Item3.Equals("0")).Count() != 0)
                        Invoke(eServiceName.EvisorService, "AppErrorSet", new object[] { curRobot.Data.LINEID, curRobot.CheckErrorList });
                       // Invoke(eServiceName.EvisorService, "AppErrorSet", new object[] { curRobot.Data.LINEID, curRobot.CheckErrorList.Where(s=>s.Value.Item3.Equals("0")) });
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            finally
            {
                curRobot.Context = null;
            }
        }

        //-----------------------------------------

        public void RemoveSameEQMap(string eqpNo, string CSTSeqNo)
        {
            try
            {
                #region [ Get Robot by EQPNO ]

                Robot curRobot = ObjectManager.RobotManager.GetRobot(eqpNo);
                if (curRobot == null)
                {
                    string strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS] Can't find Robot by EqpNo({1}) in RobotEntity!",
                                            eqpNo, eqpNo);
                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    return;
                }

                #endregion

                if (curRobot.File.RemoveFromMap(CSTSeqNo))
                    ObjectManager.RobotManager.EnqueueSave(curRobot.File);
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
    }
}
