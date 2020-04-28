using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniRCS.Core;
using System.Reflection;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;

namespace UniAuto.UniRCS.CSOT.t3.Service
{
	public partial class RobotRouteConditionService : AbstractRobotService
	{

//RouteCondition Funckey = "RC" + XXXX(序列號)

		#region [CF Common , 判斷條件是indexer operation mode ]

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0034")]
		public bool RouteCondition_CF_Normal(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.NORMAL_MODE) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0035")]
		public bool RouteCondition_CF_MQC(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;				
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.MQC_MODE) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode+"]";
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;									
				}
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0036")]
		public bool RouteCondition_CF_CoolRun(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.COOL_RUN_MODE) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0037")]
		public bool RouteCondition_CF_Sorter(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.SORTER_MODE) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0038")]
		public bool RouteCondition_CF_Through(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.THROUGH_MODE) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0039")]
		public bool RouteCondition_CF_Fix(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.FIX_MODE) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0040")]
		public bool RouteCondition_CF_Random(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.RANDOM_MODE) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151229 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0069")]
        public bool RouteCondition_CELL_QSR_FLAG(IRobotContext robotConText)
        {
            try
            {
                bool ng = false;
                string retmsg = string.Empty;

                #region [route判斷條件:]
                Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(robot.Data.NODENO);

                if (eqp.File.EquipmentRunMode != eCellQCREqpRunMode.FLAGMODE)
                {
                    ng = true;
                    retmsg = "error eqp run mode [" + eqp.File.EquipmentRunMode + "]";
                }
                #endregion

                if (ng)
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(retmsg);
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

          [UniAuto.UniBCS.OpiSpec.Help("RC0070")]
        public bool RouteCondition_CELL_QSR_NORMAL(IRobotContext robotConText)
        {
            try
            {
                bool ng = false;
                string retmsg = string.Empty;

                #region [route判斷條件:]
                Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(robot.Data.NODENO);

                if (eqp.File.EquipmentRunMode != eCellQCREqpRunMode.NORMAL)
                {
                    ng = true;
                    retmsg = "error eqp run mode [" + eqp.File.EquipmentRunMode + "]";
                }
                #endregion

                if (ng)
                {
                    robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
                    robotConText.SetReturnMessage(retmsg);
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
		#endregion		
		
		#region [FCMQC-CD , 判斷條件是job FlowPriorityInfo ]

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0041")]
		public bool RouteCondition_MQCCD_MCPD_ONLY(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (!(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE ||
					line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				} else {
					Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
					int flowprio = 0;
					int.TryParse(job.CfSpecial.FlowPriorityInfo, out flowprio);
					int prio1st = (flowprio >> 0) & 15;
					int prio2nd = (flowprio >> 4) & 15;
					//CD=L3,MCPD=L4
					if (!(prio1st == 4 && prio2nd == 0)) {
						retmsg = string.Format("error FlowPriorityInfo=[{0}],1st[{1},2nd[{2}]", job.CfSpecial.FlowPriorityInfo, prio1st, prio2nd);
						ng = true;
					}
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0042")]
		public bool RouteCondition_MQCCD_CD_ONLY(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (!(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE ||
					line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				} else {
					Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
					int flowprio = 0;
					int.TryParse(job.CfSpecial.FlowPriorityInfo, out flowprio);
					int prio1st = (flowprio >> 0) & 15;
					int prio2nd = (flowprio >> 4) & 15;
					//CD=L3,MCPD=L4
					if (!(prio1st == 3 && prio2nd == 0)) {
						retmsg = string.Format("error FlowPriorityInfo=[{0}],1st[{1},2nd[{2}]", job.CfSpecial.FlowPriorityInfo, prio1st, prio2nd);
						ng = true;
					}
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0043")]
		public bool RouteCondition_MQCCD_Default(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (!(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE ||
					line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				} else {
					Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
					int flowprio = 0;
					int.TryParse(job.CfSpecial.FlowPriorityInfo, out flowprio);
					int prio1st = (flowprio >> 0) & 15;
					int prio2nd = (flowprio >> 4) & 15;
					//CD=L3,MCPD=L4
					if (!(prio1st == 0 && prio2nd == 0)) {
						retmsg = string.Format("error FlowPriorityInfo=[{0}],1st[{1},2nd[{2}]", job.CfSpecial.FlowPriorityInfo, prio1st, prio2nd);
						ng = true;
					}
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0044")]
		public bool RouteCondition_MCPD_CD(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (!(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE ||
					line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				} else {
					Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
					int flowprio = 0;
					int.TryParse(job.CfSpecial.FlowPriorityInfo, out flowprio);
					int prio1st = (flowprio >> 0) & 15;
					int prio2nd = (flowprio >> 4) & 15;
					//CD=L3,MCPD=L4
					if (!(prio1st == 4 && prio2nd == 3)) {
						retmsg = string.Format("error FlowPriorityInfo=[{0}],1st[{1},2nd[{2}]", job.CfSpecial.FlowPriorityInfo, prio1st, prio2nd);
						ng = true;
					}
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0045")]
		public bool RouteCondition_CD_MCPD(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (!(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE ||
					line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				} else {
					Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
					int flowprio = 0;
					int.TryParse(job.CfSpecial.FlowPriorityInfo, out flowprio);
					int prio1st = (flowprio >> 0) & 15;
					int prio2nd = (flowprio >> 4) & 15;
					//CD=L3,MCPD=L4
					if (!(prio1st == 3 && prio2nd == 4)) {
						retmsg = string.Format("error FlowPriorityInfo=[{0}],1st[{1},2nd[{2}]", job.CfSpecial.FlowPriorityInfo, prio1st, prio2nd);
						ng = true;
					}
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

		#endregion

		#region [FCMQC-TTP , 判斷條件是job FlowPriorityInfo]

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0046")]
		public bool RouteCondition_MQCTTP_TTP_ONLY(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (!(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE ||
					line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				} else {
					Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
					int flowprio = 0;
					int.TryParse(job.CfSpecial.FlowPriorityInfo, out flowprio);
					int prio1st = (flowprio >> 0) & 15;
					int prio2nd = (flowprio >> 4) & 15;
					int prio3rd = (flowprio >> 8) & 15;
					//TTP=L3,MCPD=L4,SP=L5
					if (!(prio1st == 3 && prio2nd == 0 && prio3rd == 0)) {
						retmsg = string.Format("error FlowPriorityInfo=[{0}],1st[{1},2nd[{2}],3rd[{3}]", job.CfSpecial.FlowPriorityInfo, prio1st, prio2nd, prio3rd);
						ng = true;
					}
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0047")]
		public bool RouteCondition_MQCTTP_MCPD_ONLY(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (!(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE ||
					line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				} else {
					Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
					int flowprio = 0;
					int.TryParse(job.CfSpecial.FlowPriorityInfo, out flowprio);
					int prio1st = (flowprio >> 0) & 15;
					int prio2nd = (flowprio >> 4) & 15;
					int prio3rd = (flowprio >> 8) & 15;
					//TTP=L3,MCPD=L4,SP=L5
					if (!(prio1st == 4 && prio2nd == 0 && prio3rd == 0)) {
						retmsg = string.Format("error FlowPriorityInfo=[{0}],1st[{1},2nd[{2}],3rd[{3}]", job.CfSpecial.FlowPriorityInfo, prio1st, prio2nd, prio3rd);
						ng = true;
					}
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}

			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0048")]
		public bool RouteCondition_MQCTTP_SP_ONLY(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (!(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE ||
					line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				} else {
					Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
					int flowprio = 0;
					int.TryParse(job.CfSpecial.FlowPriorityInfo, out flowprio);
					int prio1st = (flowprio >> 0) & 15;
					int prio2nd = (flowprio >> 4) & 15;
					int prio3rd = (flowprio >> 8) & 15;
					//TTP=L3,MCPD=L4,SP=L5
					if (!(prio1st == 5 && prio2nd == 0 && prio3rd == 0)) {
						retmsg = string.Format("error FlowPriorityInfo=[{0}],1st[{1},2nd[{2}],3rd[{3}]", job.CfSpecial.FlowPriorityInfo, prio1st, prio2nd, prio3rd);
						ng = true;
					}
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}

			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0049")]
		public bool RouteCondition_MQCTTP_Default(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (!(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE ||
					line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				} else {
					Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
					int flowprio = 0;
					int.TryParse(job.CfSpecial.FlowPriorityInfo, out flowprio);
					int prio1st = (flowprio >> 0) & 15;
					int prio2nd = (flowprio >> 4) & 15;
					int prio3rd = (flowprio >> 8) & 15;
					//TTP=L3,MCPD=L4,SP=L5
					if (!(prio1st == 0 && prio2nd == 0 && prio3rd == 0)) {
						retmsg = string.Format("error FlowPriorityInfo=[{0}],1st[{1},2nd[{2}],3rd[{3}]", job.CfSpecial.FlowPriorityInfo, prio1st, prio2nd, prio3rd);
						ng = true;
					}
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}
			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0050")]
		public bool RouteCondition_TTP_SP(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (!(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE ||
					line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				} else {
					Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
					int flowprio = 0;
					int.TryParse(job.CfSpecial.FlowPriorityInfo, out flowprio);
					int prio1st = (flowprio >> 0) & 15;
					int prio2nd = (flowprio >> 4) & 15;
					int prio3rd = (flowprio >> 8) & 15;
					//TTP=L3,MCPD=L4,SP=L5
					if (!(prio1st == 3 && prio2nd == 5 && prio3rd == 0)) {
						retmsg = string.Format("error FlowPriorityInfo=[{0}],1st[{1},2nd[{2}],3rd[{3}]", job.CfSpecial.FlowPriorityInfo, prio1st, prio2nd, prio3rd);
						ng = true;
					}
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}

			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0051")]
		public bool RouteCondition_TTP_MCPD(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (!(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE ||
					line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				} else {
					Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
					int flowprio = 0;
					int.TryParse(job.CfSpecial.FlowPriorityInfo, out flowprio);
					int prio1st = (flowprio >> 0) & 15;
					int prio2nd = (flowprio >> 4) & 15;
					int prio3rd = (flowprio >> 8) & 15;
					//TTP=L3,MCPD=L4,SP=L5
					if (!(prio1st == 3 && prio2nd == 4 && prio3rd == 0)) {
						retmsg = string.Format("error FlowPriorityInfo=[{0}],1st[{1},2nd[{2}],3rd[{3}]", job.CfSpecial.FlowPriorityInfo, prio1st, prio2nd, prio3rd);
						ng = true;
					}
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}

			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0052")]
		public bool RouteCondition_MCPD_SP(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (!(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE ||
					line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				} else {
					Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
					int flowprio = 0;
					int.TryParse(job.CfSpecial.FlowPriorityInfo, out flowprio);
					int prio1st = (flowprio >> 0) & 15;
					int prio2nd = (flowprio >> 4) & 15;
					int prio3rd = (flowprio >> 8) & 15;
					//TTP=L3,MCPD=L4,SP=L5
					if (!(prio1st == 4 && prio2nd == 5 && prio3rd == 0)) {
						retmsg = string.Format("error FlowPriorityInfo=[{0}],1st[{1},2nd[{2}],3rd[{3}]", job.CfSpecial.FlowPriorityInfo, prio1st, prio2nd, prio3rd);
						ng = true;
					}
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}

			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0053")]
		public bool RouteCondition_MCPD_TTP(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (!(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE ||
					line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				} else {
					Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
					int flowprio = 0;
					int.TryParse(job.CfSpecial.FlowPriorityInfo, out flowprio);
					int prio1st = (flowprio >> 0) & 15;
					int prio2nd = (flowprio >> 4) & 15;
					int prio3rd = (flowprio >> 8) & 15;
					//TTP=L3,MCPD=L4,SP=L5
					if (!(prio1st == 4 && prio2nd == 3 && prio3rd == 0)) {
						retmsg = string.Format("error FlowPriorityInfo=[{0}],1st[{1},2nd[{2}],3rd[{3}]", job.CfSpecial.FlowPriorityInfo, prio1st, prio2nd, prio3rd);
						ng = true;
					}
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}

			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0054")]
		public bool RouteCondition_SP_MCPD(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (!(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE ||
					line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				} else {
					Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
					int flowprio = 0;
					int.TryParse(job.CfSpecial.FlowPriorityInfo, out flowprio);
					int prio1st = (flowprio >> 0) & 15;
					int prio2nd = (flowprio >> 4) & 15;
					int prio3rd = (flowprio >> 8) & 15;
					//TTP=L3,MCPD=L4,SP=L5
					if (!(prio1st == 5 && prio2nd == 4 && prio3rd == 0)) {
						retmsg = string.Format("error FlowPriorityInfo=[{0}],1st[{1},2nd[{2}],3rd[{3}]", job.CfSpecial.FlowPriorityInfo, prio1st, prio2nd, prio3rd);
						ng = true;
					}
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}

			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0055")]
		public bool RouteCondition_SP_TTP(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (!(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE ||
					line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				} else {
					Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
					int flowprio = 0;
					int.TryParse(job.CfSpecial.FlowPriorityInfo, out flowprio);
					int prio1st = (flowprio >> 0) & 15;
					int prio2nd = (flowprio >> 4) & 15;
					int prio3rd = (flowprio >> 8) & 15;
					//TTP=L3,MCPD=L4,SP=L5
					if (!(prio1st == 5 && prio2nd == 3 && prio3rd == 0)) {
						retmsg = string.Format("error FlowPriorityInfo=[{0}],1st[{1},2nd[{2}],3rd[{3}]", job.CfSpecial.FlowPriorityInfo, prio1st, prio2nd, prio3rd);
						ng = true;
					}
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}

			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0056")]
		public bool RouteCondition_TTP_SP_MCPD(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (!(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE ||
					line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				} else {
					Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
					int flowprio = 0;
					int.TryParse(job.CfSpecial.FlowPriorityInfo, out flowprio);
					int prio1st = (flowprio >> 0) & 15;
					int prio2nd = (flowprio >> 4) & 15;
					int prio3rd = (flowprio >> 8) & 15;
					//TTP=L3,MCPD=L4,SP=L5
					if (!(prio1st == 3 && prio2nd == 5 && prio3rd == 4)) {
						retmsg = string.Format("error FlowPriorityInfo=[{0}],1st[{1},2nd[{2}],3rd[{3}]", job.CfSpecial.FlowPriorityInfo, prio1st, prio2nd, prio3rd);
						ng = true;
					}
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}

			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0057")]
		public bool RouteCondition_TTP_MCPD_SP(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (!(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE ||
					line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				} else {
					Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
					int flowprio = 0;
					int.TryParse(job.CfSpecial.FlowPriorityInfo, out flowprio);
					int prio1st = (flowprio >> 0) & 15;
					int prio2nd = (flowprio >> 4) & 15;
					int prio3rd = (flowprio >> 8) & 15;
					//TTP=L3,MCPD=L4,SP=L5
					if (!(prio1st == 3 && prio2nd == 4 && prio3rd == 5)) {
						retmsg = string.Format("error FlowPriorityInfo=[{0}],1st[{1},2nd[{2}],3rd[{3}]", job.CfSpecial.FlowPriorityInfo, prio1st, prio2nd, prio3rd);
						ng = true;
					}
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}

			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0058")]
		public bool RouteCondition_MCPD_SP_TTP(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (!(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE ||
					line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				} else {
					Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
					int flowprio = 0;
					int.TryParse(job.CfSpecial.FlowPriorityInfo, out flowprio);
					int prio1st = (flowprio >> 0) & 15;
					int prio2nd = (flowprio >> 4) & 15;
					int prio3rd = (flowprio >> 8) & 15;
					//TTP=L3,MCPD=L4,SP=L5
					if (!(prio1st == 4 && prio2nd == 5 && prio3rd == 3)) {
						retmsg = string.Format("error FlowPriorityInfo=[{0}],1st[{1},2nd[{2}],3rd[{3}]", job.CfSpecial.FlowPriorityInfo, prio1st, prio2nd, prio3rd);
						ng = true;
					}
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}

			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0059")]
		public bool RouteCondition_MCPD_TTP_SP(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (!(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE ||
					line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				} else {
					Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
					int flowprio = 0;
					int.TryParse(job.CfSpecial.FlowPriorityInfo, out flowprio);
					int prio1st = (flowprio >> 0) & 15;
					int prio2nd = (flowprio >> 4) & 15;
					int prio3rd = (flowprio >> 8) & 15;
					//TTP=L3,MCPD=L4,SP=L5
					if (!(prio1st == 4 && prio2nd == 3 && prio3rd == 5)) {
						retmsg = string.Format("error FlowPriorityInfo=[{0}],1st[{1},2nd[{2}],3rd[{3}]", job.CfSpecial.FlowPriorityInfo, prio1st, prio2nd, prio3rd);
						ng = true;
					}
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}

			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0060")]
		public bool RouteCondition_SP_MCPD_TTP(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (!(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE ||
					line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				} else {
					Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
					int flowprio = 0;
					int.TryParse(job.CfSpecial.FlowPriorityInfo, out flowprio);
					int prio1st = (flowprio >> 0) & 15;
					int prio2nd = (flowprio >> 4) & 15;
					int prio3rd = (flowprio >> 8) & 15;
					//TTP=L3,MCPD=L4,SP=L5
					if (!(prio1st == 5 && prio2nd == 4 && prio3rd == 3)) {
						retmsg = string.Format("error FlowPriorityInfo=[{0}],1st[{1},2nd[{2}],3rd[{3}]", job.CfSpecial.FlowPriorityInfo, prio1st, prio2nd, prio3rd);
						ng = true;
					}
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}

			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

        //20151120 add FuncKey
        [UniAuto.UniBCS.OpiSpec.Help("RC0061")]
		public bool RouteCondition_SP_TTP_MCPD(IRobotContext robotConText) {
			try {
				bool ng = false;
				string retmsg = string.Empty;

				#region [route判斷條件:]
				Robot robot = robotConText[eRobotContextParameter.CurRobotEntity] as Robot;
				Line line = ObjectManager.LineManager.GetLine(robot.Data.LINEID);
				if (!(line.File.IndexOperMode == eINDEXER_OPERATION_MODE.NORMAL_MODE ||
					line.File.IndexOperMode == eINDEXER_OPERATION_MODE.MQC_MODE)) {
					ng = true;
					retmsg = "error indexer operation mode [" + line.File.IndexOperMode + "]";
				} else {
					Job job = robotConText[eRobotContextParameter.CurJobEntity] as Job;
					int flowprio = 0;
					int.TryParse(job.CfSpecial.FlowPriorityInfo, out flowprio);
					int prio1st = (flowprio >> 0) & 15;
					int prio2nd = (flowprio >> 4) & 15;
					int prio3rd = (flowprio >> 8) & 15;
					//TTP=L3,MCPD=L4,SP=L5
					if (!(prio1st == 5 && prio2nd == 3 && prio3rd == 4)) {
						retmsg = string.Format("error FlowPriorityInfo=[{0}],1st[{1},2nd[{2}],3rd[{3}]", job.CfSpecial.FlowPriorityInfo, prio1st, prio2nd, prio3rd);
						ng = true;
					}
				}
				#endregion

				if (ng) {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
					robotConText.SetReturnMessage(retmsg);
					return false;
				} else {
					robotConText.SetReturnCode(eRouteCondition_ReturnCode.Result_Is_OK);
					robotConText.SetReturnMessage(eRouteCondition_ReturnMessage.OK_Message);
					return true;
				}

			} catch (Exception ex) {
				Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
				robotConText.SetReturnCode(eRouteCondition_ReturnCode.NG_Exception);
				robotConText.SetReturnMessage(ex.Message);
				return false;
			}
		}

		#endregion		

        #region [ Cell Special - CHN ]

        //20160111 add

        /// <summary> For Cell Change Line Use Check EQP Run Mode is 1:CST To Tray(MIX) or 2:CST(MIX) to Tray
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("RC0071")]
        public bool RouteCondition_CtoT_Mix(IRobotContext robotConText)
        {
            string strlog=string.Empty;
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

                //1 : CST to Tray (Mix) ,  2 : CST (Mix) to Tray  ,  3 : CST to Tray(All)
                //4 : CST to Tray(NG)   ,  5 : Tray(Mix) to CST   ,  6 : Tray to CST(All)
                string runModeDesc = string.Empty;
                string eqpRunMode1 = GetRunMode(line, curEQP.Data.NODEID, "1", out runModeDesc);
                string eqpRunMode2 = GetRunMode(line, curEQP.Data.NODEID, "2", out runModeDesc);

                if (curEQP.File.EquipmentRunMode == eqpRunMode1 || curEQP.File.EquipmentRunMode == eqpRunMode2)
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) EQPID({3}) curEqpRunMode({4}) is not Match Mode1({5}) or Mode2({6})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                                curEQP.File.EquipmentRunMode, eqpRunMode1, eqpRunMode2);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) EQPID({2}) curEqpRunMode({3}) is not Match Mode1({4}) or Mode2({5})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                            curEQP.File.EquipmentRunMode, eqpRunMode1, eqpRunMode2);

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

        /// <summary> For Cell Change Line Use Check EQP Run Mode is 3 : CST to Tray(All) or 4 : CST to Tray(NG)
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("RC0072")]
        public bool RouteCondition_CtoT_All_NG(IRobotContext robotConText)
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

                //1 : CST to Tray (Mix) ,  2 : CST (Mix) to Tray  ,  3 : CST to Tray(All)
                //4 : CST to Tray(NG)   ,  5 : Tray(Mix) to CST   ,  6 : Tray to CST(All)
                string runModeDesc = string.Empty;
                string eqpRunMode3 = GetRunMode(line, curEQP.Data.NODEID, "3", out runModeDesc);
                string eqpRunMode4 = GetRunMode(line, curEQP.Data.NODEID, "4", out runModeDesc);

                if (curEQP.File.EquipmentRunMode == eqpRunMode3 || curEQP.File.EquipmentRunMode == eqpRunMode4)
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) EQPID({3}) curEqpRunMode({4}) is not Match Mode3({5}) or Mode4({6})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                                curEQP.File.EquipmentRunMode, eqpRunMode3, eqpRunMode4);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) EQPID({2}) curEqpRunMode({3}) is not Match Mode3({4}) or Mode4({5})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                            curEQP.File.EquipmentRunMode, eqpRunMode3, eqpRunMode4);

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

        /// <summary> For Cell Change Line Use Check EQP Run Mode is 5 : Tray(Mix) to CST or 6 : Tray to CST(All)
        /// 
        /// </summary>
        /// <param name="robotConText"></param>
        /// <returns></returns>
        [UniAuto.UniBCS.OpiSpec.Help("RC0073")]
        public bool RouteCondition_TtoC(IRobotContext robotConText)
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

                //1 : CST to Tray (Mix) ,  2 : CST (Mix) to Tray  ,  3 : CST to Tray(All)
                //4 : CST to Tray(NG)   ,  5 : Tray(Mix) to CST   ,  6 : Tray to CST(All)
                string runModeDesc = string.Empty;
                string eqpRunMode5 = GetRunMode(line, curEQP.Data.NODEID, "5", out runModeDesc);
                string eqpRunMode6 = GetRunMode(line, curEQP.Data.NODEID, "6", out runModeDesc);

                if (curEQP.File.EquipmentRunMode == eqpRunMode5 || curEQP.File.EquipmentRunMode == eqpRunMode6)
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
                        strlog = string.Format("[EQUIPMENT={0}] [RCS <- RCS][{1}] Robot({2}) EQPID({3}) curEqpRunMode({4}) is not Match Mode5({5}) or Mode6({6})!",
                                                "L1", MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                                curEQP.File.EquipmentRunMode, eqpRunMode5, eqpRunMode6);

                        Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", strlog);
                    }

                    #endregion

                    errMsg = string.Format("[{0}] Robot({1}) EQPID({2}) curEqpRunMode({3}) is not Match Mode5({4}) or Mode6({5})!",
                                            MethodBase.GetCurrentMethod().Name, curRobot.Data.ROBOTNAME, curEQP.Data.NODEID,
                                            curEQP.File.EquipmentRunMode, eqpRunMode5, eqpRunMode6);

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

        #endregion





        //Get Cell Run Desc (目前EQP是記錄成Desc)
        private string GetRunMode(Line line, string eqpNo, string value, out string description)
        {
            description = string.Empty;
            ConstantItem item = null;

            try
            {
                #region[CELL Rum Mode]
                item = ConstantManager["CELL_RUNMODE_" + line.Data.LINETYPE][value];
                #endregion

                description = item.Discription;
                return item.Value;
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return "UnKnown";
            }
            
        }

    }
}
