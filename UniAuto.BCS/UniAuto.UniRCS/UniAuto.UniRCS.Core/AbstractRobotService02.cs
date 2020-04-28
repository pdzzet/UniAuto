using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Core.Generic;
using System.Reflection;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.Core;

namespace UniAuto.UniRCS.Core
{
    public partial class AbstractRobotService
    {
        /// <summary>
        /// <br>For CF, 檢查 JobJudge 與 PortMode 與 UnloadingPortSetting 與 ProductTypeCheck</br>
        /// <br>Judge OK 可放 EMP Mode, OK Mode 與 Mix Mode(OK bit on)</br>
        /// <br>Judge NG 可放 EMP Mode, NG Mode(NG bit on) 與 Mix Mode(NG bit on)</br>
        /// <br>Judge RW 可放 EMP Mode, NG Mode(RW bit on) 與 Mix Mode(RW bit on)</br>
        /// <br>Judge 其他 可放 EMP Mode, Mix Mode(不管Bit)</br>
        /// <br>若 ProductTypeCheckMode 為 Enable 則會檢查 Job 與 Port 的 ProductType 是否相同</br>
        /// </summary>
        /// <param name="curBcsJob"></param>
        /// <param name="port"></param>
        /// <returns>true:表示curBcsJob可以放port</returns>
        protected bool ForCF_CheckJobJudge_PortMode_UnloadingPortSetting(Job curBcsJob, Port port)
        {
            bool match = false;

            try
            {
                #region Job Judge
                //0：Inspection Skip or No Judge
                //1：OK
                //2：NG - Insp. Result 
                //3：RW - Required Rework
                //4：PD –Pending judge
                //5：RP – Required Repair
                //6：IR–Ink Repair
                //7：Other
                //8：RV –PI Reivew
                #endregion

                #region UnloadingPortSetting
                //Bit 00 :OK
                //Bit 01 :PD
                //Bit 02 :NG
                //Bit 03 :RW
                //Bit 04 :IR
                //Bit 05 :RP
                #endregion

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(port.Data.NODENO);
                if (eqp == null)
                {
                    throw new Exception(string.Format("CAN'T FIND EQUIPMENT NO[{0}] IN EQUIPMENTENTITY!", port.Data.NODENO));
                }
                Line line = ObjectManager.LineManager.GetLine(port.Data.LINEID);
                if (line == null)
                {
                    throw new Exception(string.Format("CAN'T FIND LINE[{0}] IN LineManager!", port.Data.LINEID));
                }

                string ng_port_judge = string.Empty.PadLeft(16, '1'), mix_port_judge = string.Empty.PadLeft(16, '1');
                if (line.Data.FABTYPE == eFabType.CF.ToString())
                {
                    if (eqp.File.NGPortJudge.Length >= 16) ng_port_judge = eqp.File.NGPortJudge;
                    else ng_port_judge = string.Empty.PadLeft(16, '0');

                    if (eqp.File.MIXPortJudge.Length >= 16) mix_port_judge = eqp.File.MIXPortJudge;
                    else mix_port_judge = string.Empty.PadLeft(16, '0');
                }

                #region  [DebugLog]

                if (IsShowDetialLog == true)
                {
                    string strlog = string.Format("Job({0}) Judge({1}) Port({2}) NGPortJudge({3}) MIXPortJudge({4})", curBcsJob.JobKey, curBcsJob.RobotWIP.CurSendOutJobJudge, port.Data.PORTNO, ng_port_judge, mix_port_judge);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }

                #endregion

                #region Job Judge 與 Port Mode
                switch (curBcsJob.RobotWIP.CurSendOutJobJudge)
                {
                case "1"://1：OK
                    {
                        // Judge OK 可放 EMP Mode, OK Mode 與 Mix Mode(OK bit on)
                        if (port.File.Mode == ePortMode.EMPMode ||
                            port.File.Mode == ePortMode.OK ||
                            (port.File.Mode == ePortMode.MIX && mix_port_judge[0] == '1'))
                        {
                            match = true;
                        }
                    }
                    break;
                case "2"://2：NG
                    {
                        // Judge NG 可放 EMP Mode, NG Mode(NG bit on) 與 Mix Mode(NG bit on)
                        if (port.File.Mode == ePortMode.EMPMode ||
                            (port.File.Mode == ePortMode.NG && ng_port_judge[2] == '1') ||
                            (port.File.Mode == ePortMode.MIX && mix_port_judge[2] == '1'))
                        {
                            match = true;
                        }
                    }
                    break;
                case "3"://3：RW
                    {
                        // Judge RW 可放 EMP Mode, NG Mode(RW bit on) 與 Mix Mode(RW bit on)
                        if (port.File.Mode == ePortMode.EMPMode ||
                            port.File.Mode == ePortMode.Rework ||
                            (port.File.Mode == ePortMode.NG && ng_port_judge[3] == '1') ||
                            (port.File.Mode == ePortMode.MIX && mix_port_judge[3] == '1'))
                        {
                            match = true;
                        }
                    }
                    break;
                case "4"://4：PD
                    {
                        if (port.File.Mode == ePortMode.EMPMode ||
                            port.File.Mode == ePortMode.PD ||
                            (port.File.Mode == ePortMode.MIX && mix_port_judge[1] == '1'))
                        {
                            match = true;
                        }
                    }
                    break;
                case "5"://5：RP
                    {
                        if (port.File.Mode == ePortMode.EMPMode ||
                            port.File.Mode == ePortMode.RP ||
                            (port.File.Mode == ePortMode.MIX && mix_port_judge[5] == '1'))
                        {
                            match = true;
                        }
                    }
                    break;
                case "6"://6：IR
                    {
                        if (port.File.Mode == ePortMode.EMPMode ||
                            port.File.Mode == ePortMode.IR ||
                            (port.File.Mode == ePortMode.MIX && mix_port_judge[4] == '1'))
                        {
                            match = true;
                        }
                    }
                    break;
                default://7：Other, 8：RV
                    {
                        if (port.File.Mode == ePortMode.EMPMode ||
                            port.File.Mode == ePortMode.MIX)
                        {
                            match = true;
                        }
                    }
                    break;
                }
                #endregion

                #region ProductTypeCheckMode
                if (match && eqp.File.ProductTypeCheckMode == eEnableDisable.Enable)
                {
                    match = (string.IsNullOrEmpty(port.File.ProductType) || port.File.ProductType == "0" || port.File.ProductType == curBcsJob.ProductType.Value.ToString());
                    string strlog = string.Format("Port Product:{0},Job[{1},{2}] ProductType:{3},match:{4}", port.File.ProductType, curBcsJob.CassetteSequenceNo, curBcsJob.JobSequenceNo, curBcsJob.ProductType.Value.ToString(),match);
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                }
                #endregion
            }
            catch (Exception ex)
            {
                match = false;
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return match;
        }
    }
}
