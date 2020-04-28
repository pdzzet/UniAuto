using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.OpiSpec;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.PLCAgent.PLC;
using System.IO;



namespace UniAuto.UniBCS.CSOT.UIService
{
    public partial class UIService
    {

        /// <summary> OPI MessageSet: Robot Current Mode Report
        ///
        /// </summary>
        /// <param name="trxID"></param>
        /// <param name="lineName"></param>
        /// <param name="robot"></param>
        public void RobotCurrentModeReport(Robot robot)
        {

            try
            {

                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("RobotCurrentModeReport") as XmlDocument;
                RobotCurrentModeReport trx = Spec.XMLtoMessage(xml_doc) as RobotCurrentModeReport;

                //[ Wait_Proc_0017 ] 後續處理              
                trx.BODY.LINENAME = robot.Data.SERVERNAME;
                trx.BODY.EQUIPMENTNO = robot.Data.NODENO;
                trx.BODY.ROBOTNAME = robot.Data.ROBOTNAME;
                trx.BODY.ROBOTMODE = robot.File.curRobotRunMode;
                trx.BODY.SAMEEQFLAG = robot.File.curRobotSameEQFlag;
                trx.BODY.HAVE_ROBOT_CMD = ((int)robot.File.RobotHasCommandstatus).ToString();
                trx.BODY.ROBOTSTATUS = ((int)robot.File.Status).ToString() == "" ? "0" : ((int)robot.File.Status).ToString();
                trx.BODY.CURRENT_POSITION = robot.File.CurRobotPosition;
                trx.BODY.HOLD_STATUS = robot.File.CurRobotHoldStatus;

                //For Each Arm之前先清除預設的空白List
                trx.BODY.ARMLIST.Clear();

                #region [ By Arm Type Set ArmInfo ]

                if (robot.Data.ARMJOBQTY != 2)
                {

                    #region [ 1Arm1Job ] 20151208 add by RealTime ArmInfo

                    for (int i = 0; i < robot.CurTempArmSingleJobInfoList.Length; i++)
                    {

                        UniAuto.UniBCS.OpiSpec.RobotCurrentModeReport.ARMc curArmInfo = new OpiSpec.RobotCurrentModeReport.ARMc();

                        curArmInfo.ARMNO = (i + 1).ToString().PadLeft(2, '0');
                        curArmInfo.FORK_FRONT_CSTSEQ = robot.CurTempArmSingleJobInfoList[i].ArmCSTSeq;
                        curArmInfo.FORK_FRONT_JOBSEQ = robot.CurTempArmSingleJobInfoList[i].ArmJobSeq;
                        curArmInfo.FORK_FRONT_JOBEXIST = robot.CurTempArmSingleJobInfoList[i].CurRptArmJobExistDisableInfo.ToString();
                        string tmpStatus = eArmDisableStatus.Enable.ToString();

                        if (robot.CurTempArmSingleJobInfoList[i].ArmDisableFlag.ToString() == eArmDisableStatus.Enable.ToString())
                        {
                            curArmInfo.ARM_ENABLE = "Y";
                        }
                        else
                        {
                            curArmInfo.ARM_ENABLE = "N";
                        }

                        if (robot.CurTempArmSingleJobInfoList[i].ArmJobExist == eGlassExist.Exist)
                        {
                            Job curJob = ObjectManager.JobManager.GetJob(curArmInfo.FORK_FRONT_CSTSEQ, curArmInfo.FORK_FRONT_JOBSEQ);

                            if (curJob != null)
                            {
                                curArmInfo.FORK_FRONT_TRACKINGVALUE = curJob.TrackingData;

                            }

                        }

                        trx.BODY.ARMLIST.Add(curArmInfo);

                    }

                    #endregion

                }
                else
                {

                    #region [ Cell Special 1Arm2Job ]

                    for (int i = 0; i < robot.CurTempArmDoubleJobInfoList.Length; i++)
                    {
                        UniAuto.UniBCS.OpiSpec.RobotCurrentModeReport.ARMc curArmInfo = new OpiSpec.RobotCurrentModeReport.ARMc();

                        curArmInfo.ARMNO = (i + 1).ToString().PadLeft(2, '0');
                        curArmInfo.FORK_FRONT_CSTSEQ = robot.CurTempArmDoubleJobInfoList[i].ArmFrontCSTSeq;
                        curArmInfo.FORK_FRONT_JOBSEQ = robot.CurTempArmDoubleJobInfoList[i].ArmFrontJobSeq;
                        //20160120 modify 要送給OPI完整的資訊
                        curArmInfo.FORK_FRONT_JOBEXIST = robot.CurTempArmDoubleJobInfoList[i].CurRptArmFrontJobExistDisableInfo.ToString(); //robot.CurTempArmDoubleJobInfoList[i].ArmFrontJobExist.ToString() == "" ? "0" : ((int)robot.CurTempArmDoubleJobInfoList[i].ArmFrontJobExist).ToString();

                        if (robot.CurTempArmDoubleJobInfoList[i].ArmFrontJobExist == eGlassExist.Exist)
                        {
                            Job curfrontJob = ObjectManager.JobManager.GetJob(curArmInfo.FORK_FRONT_CSTSEQ, curArmInfo.FORK_FRONT_JOBSEQ);

                            if (curfrontJob != null)
                            {
                                curArmInfo.FORK_FRONT_TRACKINGVALUE = curfrontJob.TrackingData;

                            }

                        }

                        curArmInfo.FORK_BACK_CSTSEQ = robot.CurTempArmDoubleJobInfoList[i].ArmBackCSTSeq;
                        curArmInfo.FORK_BACK_JOBSEQ = robot.CurTempArmDoubleJobInfoList[i].ArmBackJobSeq;
                        //20160120 modify 要送給OPI完整的資訊
                        curArmInfo.FORK_BACK_JOBEXIST = robot.CurTempArmDoubleJobInfoList[i].CurRptArmBackJobExistDisableInfo.ToString(); //robot.CurTempArmDoubleJobInfoList[i].ArmBackJobExist.ToString() == "" ? "0" : ((int)robot.CurTempArmDoubleJobInfoList[i].ArmBackJobExist).ToString();

                        if (robot.CurTempArmDoubleJobInfoList[i].ArmBackJobExist == eGlassExist.Exist)
                        {
                            Job curBackJob = ObjectManager.JobManager.GetJob(curArmInfo.FORK_BACK_CSTSEQ, curArmInfo.FORK_BACK_JOBSEQ);

                            if (curBackJob != null)
                            {
                                curArmInfo.FORK_BACK_TRACKINGVALUE = curBackJob.TrackingData;

                            }

                        }

                        string tmpStatus = eArmDisableStatus.Enable.ToString();

                        if (robot.CurTempArmDoubleJobInfoList[i].ArmDisableFlag.ToString() == eArmDisableStatus.Enable.ToString())
                        {
                            curArmInfo.ARM_ENABLE = "Y";
                        }
                        else
                        {
                            curArmInfo.ARM_ENABLE = "N";
                        }

                        trx.BODY.ARMLIST.Add(curArmInfo);

                    }

                    #endregion

                }

                #endregion

                xMessage msg = SendReportToAllOPI(string.Empty, trx);

                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[ROBOT={0}] [BCS -> OPI][{1}] UIService report message to OPI.",
                                        robot.Data.ROBOTNAME, msg.TransactionID));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary> OPI MessageSet: Robot Current Mode Reqest
        ///
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_RobotCurrentModeRequest(XmlDocument xmlDoc)
        {
            IServerAgent agent = GetServerAgent();
            RobotCurrentModeRequest command = Spec.XMLtoMessage(xmlDoc) as RobotCurrentModeRequest;
            XmlDocument xml_doc = agent.GetTransactionFormat("RobotCurrentModeReply") as XmlDocument;
            RobotCurrentModeReply reply = Spec.XMLtoMessage(xml_doc) as RobotCurrentModeReply;

            try
            {

                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                reply.BODY.LINENAME = command.BODY.LINENAME;

                //先清除預設的空白List
                reply.BODY.EQUIPMENTLIST.Clear();

                foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPs())
                {
                    RobotCurrentModeReply.EQUIPMENTc _eqp = new RobotCurrentModeReply.EQUIPMENTc();
                    _eqp.EQUIPMENTNO = eqp.Data.NODENO;

                    foreach (Robot curRobot in ObjectManager.RobotManager.GetRobots())
                    {
                        if (curRobot.Data.NODENO != eqp.Data.NODENO)
                        {
                            
                            continue;
                        }

                        RobotCurrentModeReply.ROBOTc _replyRobotEntity = new RobotCurrentModeReply.ROBOTc();

                        _replyRobotEntity.ROBOTNAME = curRobot.Data.ROBOTNAME;
                        _replyRobotEntity.ROBOTMODE = curRobot.File.curRobotRunMode;
                        _replyRobotEntity.SAMEEQFLAG = curRobot.File.curRobotSameEQFlag;
                        _replyRobotEntity.ROBOTSTATUS = ((int)curRobot.File.Status).ToString() == "" ? "0" : ((int)curRobot.File.Status).ToString();
                        _replyRobotEntity.HAVE_ROBOT_CMD = ((int)curRobot.File.RobotHasCommandstatus).ToString() == "" ? "0" : ((int)curRobot.File.RobotHasCommandstatus).ToString();
                        _replyRobotEntity.CURRENT_POSITION = curRobot.File.CurRobotPosition;
                        _replyRobotEntity.HOLD_STATUS = curRobot.File.CurRobotHoldStatus;

                        _replyRobotEntity.ARMLIST = new List<RobotCurrentModeReply.ARMc>();
                        
                        //For Each Arm之前先清除預設的空白List
                        _replyRobotEntity.ARMLIST.Clear();

                        #region [ By Arm Type Set ArmInfo ]

                        if (curRobot.Data.ARMJOBQTY != 2)
                        {

                            #region [ 1Arm1Job ] 20151208 add by RealTime ArmInfo

                            for (int i = 0; i < curRobot.CurTempArmSingleJobInfoList.Length; i++)
                            {
                                UniAuto.UniBCS.OpiSpec.RobotCurrentModeReply.ARMc curArmInfo = new OpiSpec.RobotCurrentModeReply.ARMc();

                                curArmInfo.ARMNO = (i + 1).ToString().PadLeft(2, '0');
                                curArmInfo.FORK_FRONT_CSTSEQ = curRobot.CurTempArmSingleJobInfoList[i].ArmCSTSeq;
                                curArmInfo.FORK_FRONT_JOBSEQ = curRobot.CurTempArmSingleJobInfoList[i].ArmJobSeq;
                                curArmInfo.FORK_FRONT_JOBEXIST = curRobot.CurTempArmSingleJobInfoList[i].CurRptArmJobExistDisableInfo.ToString();
                                string tmpStatus = eArmDisableStatus.Enable.ToString();

                                if (curRobot.CurTempArmSingleJobInfoList[i].ArmDisableFlag.ToString() == eArmDisableStatus.Enable.ToString())
                                {
                                    curArmInfo.ARM_ENABLE = "Y";
                                }
                                else
                                {
                                    curArmInfo.ARM_ENABLE = "N";
                                }


                                if (curRobot.CurTempArmSingleJobInfoList[i].ArmJobExist == eGlassExist.Exist)
                                {
                                    Job curJob = ObjectManager.JobManager.GetJob(curArmInfo.FORK_FRONT_CSTSEQ, curArmInfo.FORK_FRONT_JOBSEQ);

                                    if (curJob != null)
                                    {
                                        curArmInfo.FORK_FRONT_TRACKINGVALUE = curJob.TrackingData;

                                    }

                                }

                                _replyRobotEntity.ARMLIST.Add(curArmInfo);

                            }

                            #endregion

                        }
                        else
                        {

                            //20160119 add for Cell Special
                            #region [ Cell Special 1Arm2Job ]

                            for (int i = 0; i < curRobot.CurTempArmDoubleJobInfoList.Length; i++)
                            {
                                UniAuto.UniBCS.OpiSpec.RobotCurrentModeReply.ARMc curArmInfo = new OpiSpec.RobotCurrentModeReply.ARMc();

                                curArmInfo.ARMNO = (i + 1).ToString().PadLeft(2, '0');

                                //20160119 modify 改為由實際PLC上報的值
                                curArmInfo.FORK_FRONT_CSTSEQ = curRobot.CurRealTimeArmDoubleJobInfoList[i].ArmFrontCSTSeq;
                                curArmInfo.FORK_FRONT_JOBSEQ = curRobot.CurRealTimeArmDoubleJobInfoList[i].ArmFrontJobSeq;
                                //20160120 modify 要送給OPI完整的資訊
                                curArmInfo.FORK_FRONT_JOBEXIST = curRobot.CurRealTimeArmDoubleJobInfoList[i].CurRptArmFrontJobExistDisableInfo.ToString();// curRobot.CurRealTimeArmDoubleJobInfoList[i].ArmFrontJobExist.ToString() == "" ? "0" : ((int)curRobot.CurRealTimeArmDoubleJobInfoList[i].ArmFrontJobExist).ToString();

                                if (curRobot.CurRealTimeArmDoubleJobInfoList[i].ArmFrontJobExist == eGlassExist.Exist)
                                {
                                    Job curfrontJob = ObjectManager.JobManager.GetJob(curArmInfo.FORK_FRONT_CSTSEQ, curArmInfo.FORK_FRONT_JOBSEQ);

                                    if (curfrontJob != null)
                                    {
                                        curArmInfo.FORK_FRONT_TRACKINGVALUE = curfrontJob.TrackingData;

                                    }

                                }

                                curArmInfo.FORK_BACK_CSTSEQ = curRobot.CurRealTimeArmDoubleJobInfoList[i].ArmBackCSTSeq;
                                curArmInfo.FORK_BACK_JOBSEQ = curRobot.CurRealTimeArmDoubleJobInfoList[i].ArmBackJobSeq;
                                //20160120 modify 要送給OPI完整的資訊
                                curArmInfo.FORK_BACK_JOBEXIST = curRobot.CurRealTimeArmDoubleJobInfoList[i].CurRptArmBackJobExistDisableInfo.ToString();// curRobot.CurRealTimeArmDoubleJobInfoList[i].ArmBackJobExist.ToString() == "" ? "0" : ((int)curRobot.CurRealTimeArmDoubleJobInfoList[i].ArmBackJobExist).ToString();

                                if (curRobot.CurRealTimeArmDoubleJobInfoList[i].ArmBackJobExist == eGlassExist.Exist)
                                {
                                    Job curBackJob = ObjectManager.JobManager.GetJob(curArmInfo.FORK_BACK_CSTSEQ, curArmInfo.FORK_BACK_JOBSEQ);

                                    if (curBackJob != null)
                                    {
                                        curArmInfo.FORK_BACK_TRACKINGVALUE = curBackJob.TrackingData;

                                    }

                                }

                                string tmpStatus = eArmDisableStatus.Enable.ToString();

                                if (curRobot.CurRealTimeArmDoubleJobInfoList[i].ArmDisableFlag.ToString() == eArmDisableStatus.Enable.ToString())
                                {
                                    curArmInfo.ARM_ENABLE = "Y";
                                }
                                else
                                {
                                    curArmInfo.ARM_ENABLE = "N";
                                }

                                _replyRobotEntity.ARMLIST.Add(curArmInfo);

                            }

                            #endregion

                        }

                        #endregion

                        //For Each Arm之後加入Robot Item
                        _eqp.ROBOTLIST.Add(_replyRobotEntity);

                    }

                    //EQP有Robot資訊才需要新增到Body
                    if (_eqp.ROBOTLIST.Count > 0)
                    {
                        reply.BODY.EQUIPMENTLIST.Add(_eqp);
                    }

                }

                //if (reply.BODY.EQUIPMENTLIST.Count > 0)
                //    reply.BODY.EQUIPMENTLIST.RemoveAt(0);

                xMessage msg = SendReplyToOPI(command, reply);

                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
            }
            catch (Exception ex)
            {

                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            }
        }

        /// <summary> OPI MessageSet: Robot Mode Change Request
        ///
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_RobotModeChangeRequest(XmlDocument xmlDoc)
        {
            IServerAgent agent = GetServerAgent();
            RobotModeChangeRequest command = Spec.XMLtoMessage(xmlDoc) as RobotModeChangeRequest;
            XmlDocument xml_doc = agent.GetTransactionFormat("RobotModeChangeReply") as XmlDocument;
            RobotModeChangeReply reply = Spec.XMLtoMessage(xml_doc) as RobotModeChangeReply;

            try
            {

                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, ROBOTNAME={4}, ROBOTMODE={5}",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                    command.BODY.EQUIPMENTNO, command.BODY.ROBOTNAME, command.BODY.ROBOTMODE));

                reply.BODY.LINENAME = command.BODY.LINENAME;
                reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                Robot robot = ObjectManager.RobotManager.GetRobot(command.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                    reply.RETURN.RETURNCODE = "0010350";
                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);
                }
                else if (robot == null)
                {
                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[ROBOT={0}] [BCS <- OPI][{1}] Can't find Robot({0}) in RobotEntity.",
                    command.BODY.ROBOTNAME, command.HEADER.TRANSACTIONID));

                    reply.RETURN.RETURNCODE = "0010351";
                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Robot[{0}] in RobotEntity", command.BODY.ROBOTNAME);
                }
                else
                {
                    reply.BODY.ROBOTNAME = command.BODY.ROBOTNAME;
                    reply.BODY.ROBOTMODE = command.BODY.ROBOTMODE;
                }

                xMessage msg = SendReplyToOPI(command, reply);

                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));

                //先回覆OPI再呼叫Robot，避免timeout
                if (reply.RETURN.RETURNCODE == "0000000")
                {
                    Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[ROBOT={0}] [BCS <- OPI][{1}] Invoke RobotService RobotModeChangeRequest.",
                    command.BODY.ROBOTNAME, command.HEADER.TRANSACTIONID));
                    //注意! Business.xml 要有設定才能InVoke
                    Invoke(eServiceName.RobotStatusService, "RobotModeChangeRequest", new object[] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.ROBOTNAME, command.BODY.ROBOTMODE });
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            }
        }

        /// <summary> OPI MessageSet: Robot Mode Change Request
        ///
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_RobotJobSendToSameEQRequest(XmlDocument xmlDoc)
        {
            IServerAgent agent = GetServerAgent();
            RobotJobSendToSameEQRequest command = Spec.XMLtoMessage(xmlDoc) as RobotJobSendToSameEQRequest;
            XmlDocument xml_doc = agent.GetTransactionFormat("RobotJobSendToSameEQReply") as XmlDocument;
            RobotJobSendToSameEQReply reply = Spec.XMLtoMessage(xml_doc) as RobotJobSendToSameEQReply;

            try
            {

                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, ROBOTNAME={4}, SAMEEQFLAG={5}",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                    command.BODY.EQUIPMENTNO, command.BODY.ROBOTNAME, command.BODY.SAMEEQFLAG));

                reply.BODY.LINENAME = command.BODY.LINENAME;
                reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                Robot robot = ObjectManager.RobotManager.GetRobot(command.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                    reply.RETURN.RETURNCODE = "0010350";
                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);
                }
                else if (robot == null)
                {
                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[ROBOT={0}] [BCS <- OPI][{1}] Can't find Robot({0}) in RobotEntity.",
                    command.BODY.ROBOTNAME, command.HEADER.TRANSACTIONID));

                    reply.RETURN.RETURNCODE = "0010351";
                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Robot[{0}] in RobotEntity", command.BODY.ROBOTNAME);
                }
                else
                {
                    reply.BODY.ROBOTNAME = command.BODY.ROBOTNAME;
                    reply.BODY.SAMEEQFLAG = command.BODY.SAMEEQFLAG;
                }

                xMessage msg = SendReplyToOPI(command, reply);

                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));

                //先回覆OPI再呼叫Robot，避免timeout
                if (reply.RETURN.RETURNCODE == "0000000")
                {
                    Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[ROBOT={0}] [BCS <- OPI][{1}] Invoke RobotService RobotModeChangeRequest.",
                    command.BODY.ROBOTNAME, command.HEADER.TRANSACTIONID));
                    //注意! Business.xml 要有設定才能InVoke
                    Invoke(eServiceName.RobotStatusService, "RobotJobSendToSameEQRequest", new object[] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.ROBOTNAME, command.BODY.SAMEEQFLAG });
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            }
        }

        /// <summary> OPI MessageSet: Robot Hold Status Change Request
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_RobotCommandHoldRequest(XmlDocument xmlDoc)
        {
            IServerAgent agent = GetServerAgent();
            RobotCommandHoldRequest command = Spec.XMLtoMessage(xmlDoc) as RobotCommandHoldRequest;
            XmlDocument xml_doc = agent.GetTransactionFormat("RobotCommandHoldReply") as XmlDocument;
            RobotCommandHoldReply reply = Spec.XMLtoMessage(xml_doc) as RobotCommandHoldReply;

            try
            {

                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, ROBOTNAME={4}, Robot HoldStatus={5}",
                                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
                                command.BODY.EQUIPMENTNO, command.BODY.ROBOTNAME, command.BODY.HOLD_STATUS));

                reply.BODY.LINENAME = command.BODY.LINENAME;
                reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                Robot robot = ObjectManager.RobotManager.GetRobot(command.BODY.EQUIPMENTNO);

                if (eqp == null)
                {
                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                                   command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                    reply.RETURN.RETURNCODE = "0010350";
                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);
                }
                else if (robot == null)
                {
                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[ROBOT={0}] [BCS <- OPI][{1}] Can't find Robot({0}) in RobotEntity.",
                                   command.BODY.ROBOTNAME, command.HEADER.TRANSACTIONID));

                    reply.RETURN.RETURNCODE = "0010351";
                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Robot[{0}] in RobotEntity", command.BODY.ROBOTNAME);
                }
                else
                {
                    reply.BODY.ROBOTNAME = command.BODY.ROBOTNAME;
                }

                xMessage msg = SendReplyToOPI(command, reply);

                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                                command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));

                //先回覆OPI再呼叫Robot，避免timeout
                if (reply.RETURN.RETURNCODE == "0000000")
                {
                    Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[ROBOT={0}] [BCS <- OPI][{1}] Invoke RobotService RobotHoldStatusChangeRequest.",
                    command.BODY.ROBOTNAME, command.HEADER.TRANSACTIONID));
                    //注意! Business.xml 要有設定才能InVoke
                    Invoke(eServiceName.RobotStatusService, "RobotHoldStatusChangeRequest", new object[] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.ROBOTNAME, command.BODY.HOLD_STATUS });
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            }
 
        }

        //20151126 add by Robot Arm Qty來區分送給OPI的狀態訊息
        /// <summary> OPI MessageSet: Robot Stage Info Report
        ///
        /// </summary>
        /// <param name="lineID"></param>
        public void RobotStageInfoReport(string lineID , Robot curRobot)
        {
            try
            {
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("RobotStageInfoReport") as XmlDocument;
                RobotStageInfoReport trx = Spec.XMLtoMessage(xml_doc) as RobotStageInfoReport;

                //trx.BODY.LINENAME = lineID;
                Line line = ObjectManager.LineManager.GetLine(lineID);
                trx.BODY.LINENAME = line == null ? lineID : line.Data.SERVERNAME;

                List<RobotStage> stages = ObjectManager.RobotStageManager.GetRobotStages();

                foreach (RobotStage stage in stages)
                {
                    //
                    if (stage.Data.STAGETYPE != "PORT")
                    {

                        #region [ Not Port Type Stage Info ]

                        RobotStageInfoReport.STAGEc _stageEntity = new RobotStageInfoReport.STAGEc();
                        _stageEntity.ROBOTNAME = stage.Data.ROBOTNAME;
                        _stageEntity.STAGEID = stage.Data.STAGEID;

                        //STAGESTATUS: 0:Unknown,1:NoExist, 2:LDRQ, 3:UDRQ
                        switch (stage.File.CurStageStatus)
                        {
                            case eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY:// "UDRQ_LDRQ":
                                _stageEntity.STAGESTATUS = "4";
                                break;
                            case eRobotStageStatus.RECEIVE_READY://"LDRQ":
                                _stageEntity.STAGESTATUS = "2";
                                break;
                            case eRobotStageStatus.SEND_OUT_READY:// "UDRQ":
                                _stageEntity.STAGESTATUS = "3";
                                break;
                            case eRobotStageStatus.NO_REQUEST://"NOREQ":
                                _stageEntity.STAGESTATUS = "1";
                                break;
                            default:
                                _stageEntity.STAGESTATUS = "0";
                                break;
                        }

                        #region [ 只有UDRQ時才會更新 ]

                        if (stage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY || stage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                        {
                            //20160113 add by Robot Arm Job Qty來區分
                            if (curRobot.Data.ARMJOBQTY == 2)
                            {
                                #region [ 由Stage UDRQ SlotBlockInfo List來取得顯示資訊 最多顯示2各SlotBlockInfo 共4片資訊 ]

                                if (stage.curUDRQ_SlotBlockInfoList.Count > 0)
                                {
                                    int tmpCount = 0;
                                    int showMaxSlotBlockJobInfo = 1;

                                    if (stage.Data.SLOTMAXCOUNT > 2)
                                    {
                                        showMaxSlotBlockJobInfo = 2; //要顯示2各SlotBlockInfo
                                    }

                                    //By Cmd SlotNo Check
                                    foreach (int tmpCmdSlotNo in stage.curUDRQ_SlotBlockInfoList.Keys)
                                    {
                                        tmpCount = tmpCount + 1;

                                        if (tmpCount <= showMaxSlotBlockJobInfo) //Check 1st CmdSlot Front
                                        {
                                            #region [ Check 1st CmdSlotInfo ]

                                            if (stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo].Count == 1)
                                            {

                                                #region [ SlotBlockInfo只有一片 Front or Back ]

                                                foreach (int tmpJobSlotNo in stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo].Keys)
                                                {

                                                    UniAuto.UniBCS.OpiSpec.RobotStageInfoReport.JOBc curExistJobEntity = new OpiSpec.RobotStageInfoReport.JOBc();
                                                    UniAuto.UniBCS.OpiSpec.RobotStageInfoReport.JOBc curNoExistJobEntity = new OpiSpec.RobotStageInfoReport.JOBc();

                                                    //CmdSlotNo都是Front SlotNo
                                                    if (tmpCmdSlotNo != tmpJobSlotNo)
                                                    {
                                                        #region [ 表示Front為空 Add Front ]

                                                        curNoExistJobEntity.SLOTNO = tmpCmdSlotNo.ToString().PadLeft(3, '0');
                                                        curNoExistJobEntity.CASSETTESEQNO = "0";
                                                        curNoExistJobEntity.JOBSEQNO = "0";
                                                        curNoExistJobEntity.JOBEXIST = "1";  //NoExist

                                                        _stageEntity.JOBLIST.Add(curNoExistJobEntity);

                                                        #endregion

                                                        #region [ Add Back Info ]

                                                        string[] cur1stJobInfo = stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo][tmpJobSlotNo].Split('_');

                                                        if (cur1stJobInfo.Length >= 2)
                                                        {
                                                            curExistJobEntity.SLOTNO = tmpJobSlotNo.ToString().PadLeft(3, '0');
                                                            curExistJobEntity.CASSETTESEQNO = cur1stJobInfo[0];
                                                            curExistJobEntity.JOBSEQNO = cur1stJobInfo[1];
                                                            curExistJobEntity.JOBEXIST = "2";  //Exist

                                                            Job curJob = ObjectManager.JobManager.GetJob(curExistJobEntity.CASSETTESEQNO, curExistJobEntity.JOBSEQNO);

                                                            if (curJob != null)
                                                            {
                                                                curExistJobEntity.TRACKINGVALUE = curJob.TrackingData;

                                                            }

                                                            _stageEntity.JOBLIST.Add(curExistJobEntity);
                                                        }

                                                        #endregion

                                                    }
                                                    else
                                                    {
                                                        #region [ Add Front Info ]

                                                        string[] cur1stJobInfo = stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo][tmpJobSlotNo].Split('_');

                                                        if (cur1stJobInfo.Length >= 2)
                                                        {
                                                            curExistJobEntity.SLOTNO = tmpJobSlotNo.ToString().PadLeft(3, '0');
                                                            curExistJobEntity.CASSETTESEQNO = cur1stJobInfo[0];
                                                            curExistJobEntity.JOBSEQNO = cur1stJobInfo[1];
                                                            curExistJobEntity.JOBEXIST = "2";  //Exist

                                                            Job curJob = ObjectManager.JobManager.GetJob(curExistJobEntity.CASSETTESEQNO, curExistJobEntity.JOBSEQNO);

                                                            if (curJob != null)
                                                            {
                                                                curExistJobEntity.TRACKINGVALUE = curJob.TrackingData;

                                                            }

                                                            _stageEntity.JOBLIST.Add(curExistJobEntity);
                                                        }

                                                        #endregion

                                                        #region  [ 表示back為空 Add Back ]

                                                        curNoExistJobEntity.SLOTNO = (tmpCmdSlotNo + 1).ToString().PadLeft(3, '0');
                                                        curNoExistJobEntity.CASSETTESEQNO = "0";
                                                        curNoExistJobEntity.JOBSEQNO = "0";
                                                        curNoExistJobEntity.JOBEXIST = "1";  //NoExist

                                                        _stageEntity.JOBLIST.Add(curNoExistJobEntity);

                                                        #endregion

                                                    }

                                                }

                                                #endregion
                                            }
                                            else
                                            {

                                                #region [ SlotBlockInfo有2片 Front & Back ]

                                                bool checkFront = false; //判斷是否已經確認過Front

                                                //by CmdSlotBlockInfo  JobList SlotNo Check
                                                foreach (int tmpJobSlotNo in stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo].Keys)
                                                {
                                                    if (checkFront == false)
                                                    {
                                                        UniAuto.UniBCS.OpiSpec.RobotStageInfoReport.JOBc curFrontJobEntity = new OpiSpec.RobotStageInfoReport.JOBc();
                                                        string[] cur1stJobInfo = stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo][tmpJobSlotNo].Split('_');

                                                        if (cur1stJobInfo.Length >= 2)
                                                        {
                                                            curFrontJobEntity.SLOTNO = tmpJobSlotNo.ToString().PadLeft(3, '0');
                                                            curFrontJobEntity.CASSETTESEQNO = cur1stJobInfo[0];
                                                            curFrontJobEntity.JOBSEQNO = cur1stJobInfo[1];
                                                            curFrontJobEntity.JOBEXIST = "2";  //Exist

                                                            Job curJob = ObjectManager.JobManager.GetJob(curFrontJobEntity.CASSETTESEQNO, curFrontJobEntity.JOBSEQNO);

                                                            if (curJob != null)
                                                            {
                                                                curFrontJobEntity.TRACKINGVALUE = curJob.TrackingData;

                                                            }

                                                            _stageEntity.JOBLIST.Add(curFrontJobEntity);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        UniAuto.UniBCS.OpiSpec.RobotStageInfoReport.JOBc curBackJobEntity = new OpiSpec.RobotStageInfoReport.JOBc();
                                                        string[] cur2ndJobInfo = stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo][tmpJobSlotNo].Split('_');

                                                        if (cur2ndJobInfo.Length >= 2)
                                                        {
                                                            curBackJobEntity.SLOTNO = tmpJobSlotNo.ToString().PadLeft(3, '0');
                                                            curBackJobEntity.CASSETTESEQNO = cur2ndJobInfo[0];
                                                            curBackJobEntity.JOBSEQNO = cur2ndJobInfo[1];
                                                            curBackJobEntity.JOBEXIST = "2";  //Exist

                                                            Job curJob = ObjectManager.JobManager.GetJob(curBackJobEntity.CASSETTESEQNO, curBackJobEntity.JOBSEQNO);

                                                            if (curJob != null)
                                                            {
                                                                curBackJobEntity.TRACKINGVALUE = curJob.TrackingData;

                                                            }

                                                            _stageEntity.JOBLIST.Add(curBackJobEntity);
                                                        }

                                                    }
                                                }

                                                #endregion

                                            }

                                            #endregion

                                        }

                                    }

                                }

                                #endregion
                            }
                            else
                            {

                                #region [ For Normal Robot 1Arm 1Job ]

                                if (!string.IsNullOrEmpty(stage.File.CurSendOut_CSTSeq) &&
                                    !string.IsNullOrEmpty(stage.File.CurSendOut_JobSeq))
                                {
                                    UniAuto.UniBCS.OpiSpec.RobotStageInfoReport.JOBc cur1stJobEntity = new OpiSpec.RobotStageInfoReport.JOBc();
                                    //針對Dual Mode  永遠會用 SendOut01 位置要特別處理
                                    //cur1stJobEntity.SLOTNO = "001";

                                    //20160519
                                    if (stage.curUDRQ_SlotList != null && stage.curUDRQ_SlotList.Keys.Count > 0 && stage.Data.SLOTMAXCOUNT > 1)
                                        cur1stJobEntity.SLOTNO = stage.curUDRQ_SlotList.Keys.ElementAt(0).ToString().PadLeft(3, '0');
                                    else
                                        cur1stJobEntity.SLOTNO = "001";

                                    cur1stJobEntity.CASSETTESEQNO = stage.File.CurSendOut_CSTSeq;
                                    cur1stJobEntity.JOBSEQNO = stage.File.CurSendOut_JobSeq;
                                    cur1stJobEntity.JOBEXIST = "2";  //Exist

                                    Job curJob = ObjectManager.JobManager.GetJob(cur1stJobEntity.CASSETTESEQNO, cur1stJobEntity.JOBSEQNO);

                                    if (curJob != null)
                                    {
                                        cur1stJobEntity.TRACKINGVALUE = curJob.TrackingData;

                                    }

                                    _stageEntity.JOBLIST.Add(cur1stJobEntity);

                                }

                                if (!string.IsNullOrEmpty(stage.File.CurSendOut_CSTSeq02) &&
                                    !string.IsNullOrEmpty(stage.File.CurSendOut_JobSeq02))
                                {
                                    UniAuto.UniBCS.OpiSpec.RobotStageInfoReport.JOBc cur2ndJobEntity = new OpiSpec.RobotStageInfoReport.JOBc();
                                    //[ Wait_Proc_00030 ]
                                    //cur2ndJobEntity.SLOTNO = "002";

                                    //20160519
                                    if (stage.curUDRQ_SlotList != null && stage.curUDRQ_SlotList.Keys.Count > 1 && stage.Data.SLOTMAXCOUNT > 1)
                                        cur2ndJobEntity.SLOTNO = stage.curUDRQ_SlotList.Keys.ElementAt(1).ToString().PadLeft(3, '0');
                                    else
                                        cur2ndJobEntity.SLOTNO = "002";

                                    cur2ndJobEntity.CASSETTESEQNO = stage.File.CurSendOut_CSTSeq02;
                                    cur2ndJobEntity.JOBSEQNO = stage.File.CurSendOut_JobSeq02;
                                    cur2ndJobEntity.JOBEXIST = "2";  //Exist

                                    Job cur2ndJob = ObjectManager.JobManager.GetJob(cur2ndJobEntity.CASSETTESEQNO, cur2ndJobEntity.JOBSEQNO);

                                    if (cur2ndJob != null)
                                    {
                                        cur2ndJobEntity.TRACKINGVALUE = cur2ndJob.TrackingData;

                                    }

                                    _stageEntity.JOBLIST.Add(cur2ndJobEntity);

                                }

                                #endregion

                            }

                        }
                        else
                        {
                            //非UDRQ 也要補上空白Job List 20150922 mark 不須送
                            //UniAuto.UniBCS.OpiSpec.RobotStageInfoReport.JOBc cur1stJobEntity = new OpiSpec.RobotStageInfoReport.JOBc();
                            ////針對Dual Mode  永遠會用 SendOut01 位置要特別處理
                            //cur1stJobEntity.SLOTNO = "01";
                            //cur1stJobEntity.CASSETTESEQNO = "0";
                            //cur1stJobEntity.JOBSEQNO = "0";
                            //cur1stJobEntity.JOBEXIST = "1";  //Exist
                            //_stageEntity.JOBLIST.Add(cur1stJobEntity);
                          
                        }

                        #endregion

                        #endregion

                        trx.BODY.STAGELIST.Add(_stageEntity);

                    }
                    else
                    {

                        #region [ Is Port Type Stage Info ]

                        //[ Wait_Proc_00038 ]送給OPI LayOut 要考慮Port 如何送出Job資訊
                        RobotStageInfoReport.STAGEc _stageEntity = new RobotStageInfoReport.STAGEc();
                        _stageEntity.ROBOTNAME = stage.Data.ROBOTNAME;
                        _stageEntity.STAGEID = stage.Data.STAGEID;

                        //STAGESTATUS: 0:Unknown,1:NoExist, 2:LDRQ, 3:UDRQ
                        switch (stage.File.CurStageStatus)
                        {
                            case eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY://"UDRQ_LDRQ":
                                _stageEntity.STAGESTATUS = "4";
                                break;
                            case eRobotStageStatus.RECEIVE_READY://"LDRQ":
                                _stageEntity.STAGESTATUS = "2";
                                break;
                            case eRobotStageStatus.SEND_OUT_READY://"UDRQ":
                                _stageEntity.STAGESTATUS = "3";
                                break;
                            case eRobotStageStatus.NO_REQUEST:// "NOREQ":
                                _stageEntity.STAGESTATUS = "1";
                                break;
                            default:
                                _stageEntity.STAGESTATUS = "0";
                                break;
                        }

                        #region [ 只有UDRQ時才會更新 ]

                        if (stage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY || stage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                        {

                            //20151126 add by Port Arm Job Qty來區分
                            if (curRobot.Data.ARMJOBQTY == 2)
                            {

                                #region [ 由Stage UDRQ SlotBlockInfo List來取得顯示資訊 最多顯示2各SlotBlockInfo 共4片資訊 ]

                                if (stage.curUDRQ_SlotBlockInfoList.Count > 0)
                                {
                                    int tmpCount = 0;

                                    //By Cmd SlotNo Check
                                    foreach (int tmpCmdSlotNo in stage.curUDRQ_SlotBlockInfoList.Keys)
                                    {
                                        tmpCount = tmpCount + 1;

                                        if (tmpCount <3) //Check 1st CmdSlot Front
                                        {
                                            #region [ Check 1st CmdSlotInfo ]

                                            if (stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo].Count == 1)
                                            {

                                                #region [ SlotBlockInfo只有一片 Front or Back ]

                                                foreach (int tmpJobSlotNo in stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo].Keys)
                                                {
                                                    
                                                    UniAuto.UniBCS.OpiSpec.RobotStageInfoReport.JOBc curExistJobEntity = new OpiSpec.RobotStageInfoReport.JOBc();
                                                    UniAuto.UniBCS.OpiSpec.RobotStageInfoReport.JOBc curNoExistJobEntity = new OpiSpec.RobotStageInfoReport.JOBc();

                                                    //CmdSlotNo都是Front SlotNo
                                                    if (tmpCmdSlotNo != tmpJobSlotNo)
                                                    {
                                                        #region [ 表示Front為空 Add Front ]

                                                        curNoExistJobEntity.SLOTNO = tmpCmdSlotNo.ToString().PadLeft(3, '0');
                                                        curNoExistJobEntity.CASSETTESEQNO = "0";
                                                        curNoExistJobEntity.JOBSEQNO = "0";
                                                        curNoExistJobEntity.JOBEXIST = "1";  //NoExist

                                                        _stageEntity.JOBLIST.Add(curNoExistJobEntity);

                                                        #endregion

                                                        #region [ Add Back Info ]

                                                        string[] cur1stJobInfo = stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo][tmpJobSlotNo].Split('_');

                                                        if (cur1stJobInfo.Length >= 2)
                                                        {
                                                            curExistJobEntity.SLOTNO = tmpJobSlotNo.ToString().PadLeft(3, '0');
                                                            curExistJobEntity.CASSETTESEQNO = cur1stJobInfo[0];
                                                            curExistJobEntity.JOBSEQNO = cur1stJobInfo[1];
                                                            curExistJobEntity.JOBEXIST = "2";  //Exist

                                                            Job curJob = ObjectManager.JobManager.GetJob(curExistJobEntity.CASSETTESEQNO, curExistJobEntity.JOBSEQNO);

                                                            if (curJob != null)
                                                            {
                                                                curExistJobEntity.TRACKINGVALUE = curJob.TrackingData;

                                                            }

                                                            _stageEntity.JOBLIST.Add(curExistJobEntity);
                                                        }

                                                        #endregion

                                                    }
                                                    else
                                                    {
                                                        #region [ Add Front Info ]

                                                        string[] cur1stJobInfo = stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo][tmpJobSlotNo].Split('_');

                                                        if (cur1stJobInfo.Length >= 2)
                                                        {
                                                            curExistJobEntity.SLOTNO = tmpJobSlotNo.ToString().PadLeft(3, '0');
                                                            curExistJobEntity.CASSETTESEQNO = cur1stJobInfo[0];
                                                            curExistJobEntity.JOBSEQNO = cur1stJobInfo[1];
                                                            curExistJobEntity.JOBEXIST = "2";  //Exist

                                                            Job curJob = ObjectManager.JobManager.GetJob(curExistJobEntity.CASSETTESEQNO, curExistJobEntity.JOBSEQNO);

                                                            if (curJob != null)
                                                            {
                                                                curExistJobEntity.TRACKINGVALUE = curJob.TrackingData;

                                                            }

                                                            _stageEntity.JOBLIST.Add(curExistJobEntity);
                                                        }

                                                        #endregion

                                                        #region  [ 表示back為空 Add Back ]

                                                        curNoExistJobEntity.SLOTNO = (tmpCmdSlotNo + 1).ToString().PadLeft(3, '0');
                                                        curNoExistJobEntity.CASSETTESEQNO = "0";
                                                        curNoExistJobEntity.JOBSEQNO = "0";
                                                        curNoExistJobEntity.JOBEXIST = "1";  //NoExist

                                                        _stageEntity.JOBLIST.Add(curNoExistJobEntity);

                                                        #endregion

                                                    }                                  
                                                    
                                                }

                                                #endregion
                                            }
                                            else
                                            {

                                                #region [ SlotBlockInfo有2片 Front & Back ]

                                                bool checkFront = false; //判斷是否已經確認過Front

                                                //by CmdSlotBlockInfo  JobList SlotNo Check
                                                foreach (int tmpJobSlotNo in stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo].Keys)
                                                {
                                                    if (checkFront == false)
                                                    {
                                                        UniAuto.UniBCS.OpiSpec.RobotStageInfoReport.JOBc curFrontJobEntity = new OpiSpec.RobotStageInfoReport.JOBc();
                                                        string[] cur1stJobInfo = stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo][tmpJobSlotNo].Split('_');

                                                        if (cur1stJobInfo.Length >= 2)
                                                        {
                                                            curFrontJobEntity.SLOTNO = tmpJobSlotNo.ToString().PadLeft(3, '0');
                                                            curFrontJobEntity.CASSETTESEQNO = cur1stJobInfo[0];
                                                            curFrontJobEntity.JOBSEQNO = cur1stJobInfo[1];
                                                            curFrontJobEntity.JOBEXIST = "2";  //Exist

                                                            Job curJob = ObjectManager.JobManager.GetJob(curFrontJobEntity.CASSETTESEQNO, curFrontJobEntity.JOBSEQNO);

                                                            if (curJob != null)
                                                            {
                                                                curFrontJobEntity.TRACKINGVALUE = curJob.TrackingData;

                                                            }

                                                            _stageEntity.JOBLIST.Add(curFrontJobEntity);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        UniAuto.UniBCS.OpiSpec.RobotStageInfoReport.JOBc curBackJobEntity = new OpiSpec.RobotStageInfoReport.JOBc();
                                                        string[] cur2ndJobInfo = stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo][tmpJobSlotNo].Split('_');

                                                        if (cur2ndJobInfo.Length >= 2)
                                                        {
                                                            curBackJobEntity.SLOTNO = tmpJobSlotNo.ToString().PadLeft(3, '0');
                                                            curBackJobEntity.CASSETTESEQNO = cur2ndJobInfo[0];
                                                            curBackJobEntity.JOBSEQNO = cur2ndJobInfo[1];
                                                            curBackJobEntity.JOBEXIST = "2";  //Exist

                                                            Job curJob = ObjectManager.JobManager.GetJob(curBackJobEntity.CASSETTESEQNO, curBackJobEntity.JOBSEQNO);

                                                            if (curJob != null)
                                                            {
                                                                curBackJobEntity.TRACKINGVALUE = curJob.TrackingData;

                                                            }

                                                            _stageEntity.JOBLIST.Add(curBackJobEntity);
                                                        }

                                                    }
                                                }

                                                #endregion

                                            }

                                            #endregion

                                        }

                                    }

                                }

                                #endregion

                            }
                            else
                            {
                                #region [ Port Type 永遠Show 2各Slot for 1Arm 1Job ]

                                if (stage.curUDRQ_SlotList.Count > 0)
                                {
                                    int tmpCount = 0;

                                    foreach (int key in stage.curUDRQ_SlotList.Keys)
                                    {
                                        tmpCount = tmpCount + 1;

                                        if (tmpCount == 1)
                                        {
                                            UniAuto.UniBCS.OpiSpec.RobotStageInfoReport.JOBc cur1stJobEntity = new OpiSpec.RobotStageInfoReport.JOBc();
                                            cur1stJobEntity.SLOTNO = key.ToString().PadLeft(3, '0');

                                            string[] cur1stJobInfo = stage.curUDRQ_SlotList[key].Split('_');

                                            if (cur1stJobInfo.Length >= 2)
                                            {
                                                cur1stJobEntity.CASSETTESEQNO = cur1stJobInfo[0];
                                                cur1stJobEntity.JOBSEQNO = cur1stJobInfo[1];
                                                cur1stJobEntity.JOBEXIST = "2";  //Exist

                                                Job curJob = ObjectManager.JobManager.GetJob(cur1stJobEntity.CASSETTESEQNO, cur1stJobEntity.JOBSEQNO);

                                                if (curJob != null)
                                                {
                                                    cur1stJobEntity.TRACKINGVALUE = curJob.TrackingData;

                                                }

                                                _stageEntity.JOBLIST.Add(cur1stJobEntity);
                                            }

                                        }
                                        else if (tmpCount == 2)
                                        {
                                            UniAuto.UniBCS.OpiSpec.RobotStageInfoReport.JOBc cur2ndJobEntity = new OpiSpec.RobotStageInfoReport.JOBc();
                                            cur2ndJobEntity.SLOTNO = key.ToString().PadLeft(3, '0');

                                            string[] cur2ndJobInfo = stage.curUDRQ_SlotList[key].Split('_');

                                            cur2ndJobEntity.CASSETTESEQNO = cur2ndJobInfo[0];
                                            cur2ndJobEntity.JOBSEQNO = cur2ndJobInfo[1];
                                            cur2ndJobEntity.JOBEXIST = "2";  //Exist

                                            Job cur2ndJob = ObjectManager.JobManager.GetJob(cur2ndJobEntity.CASSETTESEQNO, cur2ndJobEntity.JOBSEQNO);

                                            if (cur2ndJob != null)
                                            {
                                                cur2ndJobEntity.TRACKINGVALUE = cur2ndJob.TrackingData;

                                            }

                                            _stageEntity.JOBLIST.Add(cur2ndJobEntity);
                                        }
                                        else
                                        {
                                            break;
                                        }
                                    }
                                }


                                #endregion
                            }

                            #region [ 20150922 old寫法 mark ]

                            //if (!string.IsNullOrEmpty(stage.File.CurSendOut_CSTSeq) &&
                            //    !string.IsNullOrEmpty(stage.File.CurSendOut_JobSeq))
                            //{
                            //    UniAuto.UniBCS.OpiSpec.RobotStageInfoReport.JOBc cur1stJobEntity = new OpiSpec.RobotStageInfoReport.JOBc();
                            //    //針對Dual Mode  永遠會用 SendOut01 位置要特別處理,這裡是針對Port所以要以Stage的UDRQ SlotNo為主
                            //    if (stage.curUDRQ_SlotList.Count > 0)
                            //    {
                            //        foreach (int key in stage.curUDRQ_SlotList.Keys)
                            //        {
                            //            cur1stJobEntity.SLOTNO = key.ToString().PadLeft(2,'0');
                            //            break;
                            //        }
                                    
                            //    }
                            //    else
                            //    {
                            //        cur1stJobEntity.SLOTNO = "01";

                            //    }

                            //    cur1stJobEntity.CASSETTESEQNO = stage.File.CurSendOut_CSTSeq;
                            //    cur1stJobEntity.JOBSEQNO = stage.File.CurSendOut_JobSeq;
                            //    cur1stJobEntity.JOBEXIST = "2";  //Exist

                            //    Job curJob = ObjectManager.JobManager.GetJob(cur1stJobEntity.CASSETTESEQNO, cur1stJobEntity.JOBSEQNO);

                            //    if (curJob != null)
                            //    {
                            //        cur1stJobEntity.TRACKINGVALUE = curJob.TrackingData;

                            //    }

                            //    _stageEntity.JOBLIST.Add(cur1stJobEntity);

                            //}

                            //if (!string.IsNullOrEmpty(stage.File.CurSendOut_CSTSeq02) &&
                            //    !string.IsNullOrEmpty(stage.File.CurSendOut_JobSeq02))
                            //{
                            //    UniAuto.UniBCS.OpiSpec.RobotStageInfoReport.JOBc cur2ndJobEntity = new OpiSpec.RobotStageInfoReport.JOBc();
                            //    //[ Wait_Proc_00030 ]
                            //    cur2ndJobEntity.SLOTNO = "02";
                            //    cur2ndJobEntity.CASSETTESEQNO = stage.File.CurSendOut_CSTSeq02;
                            //    cur2ndJobEntity.JOBSEQNO = stage.File.CurSendOut_JobSeq02;
                            //    cur2ndJobEntity.JOBEXIST = "2";  //Exist

                            //    Job cur2ndJob = ObjectManager.JobManager.GetJob(cur2ndJobEntity.CASSETTESEQNO, cur2ndJobEntity.JOBSEQNO);

                            //    if (cur2ndJob != null)
                            //    {
                            //        cur2ndJobEntity.TRACKINGVALUE = cur2ndJob.TrackingData;

                            //    }

                            //    _stageEntity.JOBLIST.Add(cur2ndJobEntity);

                            //}

                            #endregion

                        }
                        else
                        {
                            //非UDRQ 也要補上空白Job List =>20150922 mark 不須補
                            //UniAuto.UniBCS.OpiSpec.RobotStageInfoReport.JOBc cur1stJobEntity = new OpiSpec.RobotStageInfoReport.JOBc();
                            ////針對Dual Mode  永遠會用 SendOut01 位置要特別處理
                            //cur1stJobEntity.SLOTNO = "01";
                            //cur1stJobEntity.CASSETTESEQNO = "0";
                            //cur1stJobEntity.JOBSEQNO = "0";
                            //cur1stJobEntity.JOBEXIST = "1";  //Exist
                            //_stageEntity.JOBLIST.Add(cur1stJobEntity);
                        }

                        #endregion

                        #endregion

                        trx.BODY.STAGELIST.Add(_stageEntity);
                    }


                }

                if (trx.BODY.STAGELIST.Count > 0)
                    trx.BODY.STAGELIST.RemoveAt(0);

                xMessage msg = SendReportToAllOPI(string.Empty, trx);

                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                       string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService report message to OPI.",
                       lineID, msg.TransactionID));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary> OPI MessageSet: Robot Stage Info Request
        ///
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_RobotStageInfoRequest(XmlDocument xmlDoc)
        {
            IServerAgent agent = GetServerAgent();
            RobotStageInfoRequest command = Spec.XMLtoMessage(xmlDoc) as RobotStageInfoRequest;
            XmlDocument xml_doc = agent.GetTransactionFormat("RobotStageInfoRequestReply") as XmlDocument;
            RobotStageInfoRequestReply reply = Spec.XMLtoMessage(xml_doc) as RobotStageInfoRequestReply;

            try
            {
                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

                reply.BODY.LINENAME = command.BODY.LINENAME;

                List<Robot> robots = ObjectManager.RobotManager.GetRobots();
                
                foreach (Robot curRobot in robots)
	            {

                    RobotStageInfoRequestReply.ROBOTc _robotEntity = new RobotStageInfoRequestReply.ROBOTc(); 

                    _robotEntity.ROBOTNAME = curRobot.Data.ROBOTNAME;

                    List<RobotStage> stages = ObjectManager.RobotStageManager.GetRobotStages();

                    foreach (RobotStage stage in stages)
                    {

                        #region [ by Stage Get Status ]

                        //排除不需要的PORT
                        if (stage.Data.STAGETYPE != "PORT")
                        {
                            RobotStageInfoRequestReply.STAGEc _stageEntity = new RobotStageInfoRequestReply.STAGEc();

                            _stageEntity.STAGEID = stage.Data.STAGEID;

                            //STAGESTATUS: 0:Unknown,1:NoExist, 2:LDRQ, 3:UDRQ
                            switch (stage.File.CurStageStatus)
                            {
                                //20151209 StageType add Stage
                                case eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY://"UDRQ_LDRQ":
                                    _stageEntity.STAGESTATUS = "4";
                                    break;
                                case eRobotStageStatus.RECEIVE_READY://"LDRQ":
                                    _stageEntity.STAGESTATUS = "2";
                                    break;
                                case eRobotStageStatus.SEND_OUT_READY://"UDRQ":
                                    _stageEntity.STAGESTATUS = "3";
                                    break;
                                case eRobotStageStatus.NO_REQUEST://"NOREQ":
                                    _stageEntity.STAGESTATUS = "1";
                                    break;
                                default:
                                    _stageEntity.STAGESTATUS = "0";
                                    break;
                            }

                            #region [ 只有UDRQ時才會更新 ]


                            if (stage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY || stage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                            {
                                //20160113 add by Robot Arm Job Qty來區分
                                if (curRobot.Data.ARMJOBQTY == 2)
                                {
                                    #region [ 由Stage UDRQ SlotBlockInfo List來取得顯示資訊 最多顯示2各SlotBlockInfo 共4片資訊 ]

                                    if (stage.curUDRQ_SlotBlockInfoList.Count > 0)
                                    {
                                        int tmpCount = 0;
                                        int showMaxSlotBlockJobInfo = 1;

                                        if (stage.Data.SLOTMAXCOUNT > 2)
                                        {
                                            showMaxSlotBlockJobInfo = 2; //要顯示2各SlotBlockInfo
                                        }

                                        //By Cmd SlotNo Check
                                        foreach (int tmpCmdSlotNo in stage.curUDRQ_SlotBlockInfoList.Keys)
                                        {
                                            tmpCount = tmpCount + 1;

                                            if (tmpCount <= showMaxSlotBlockJobInfo) //Check 1st CmdSlot Front
                                            {
                                                #region [ Check 1st CmdSlotInfo ]

                                                if (stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo].Count == 1)
                                                {

                                                    #region [ SlotBlockInfo只有一片 Front or Back ]

                                                    foreach (int tmpJobSlotNo in stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo].Keys)
                                                    {

                                                        UniAuto.UniBCS.OpiSpec.RobotStageInfoRequestReply.JOBc curExistJobEntity = new OpiSpec.RobotStageInfoRequestReply.JOBc();
                                                        UniAuto.UniBCS.OpiSpec.RobotStageInfoRequestReply.JOBc curNoExistJobEntity = new OpiSpec.RobotStageInfoRequestReply.JOBc();

                                                        //CmdSlotNo都是Front SlotNo
                                                        if (tmpCmdSlotNo != tmpJobSlotNo)
                                                        {
                                                            #region [ 表示Front為空 Add Front ]

                                                            curNoExistJobEntity.SLOTNO = tmpCmdSlotNo.ToString().PadLeft(3, '0');
                                                            curNoExistJobEntity.CASSETTESEQNO = "0";
                                                            curNoExistJobEntity.JOBSEQNO = "0";
                                                            curNoExistJobEntity.JOBEXIST = "1";  //NoExist

                                                            _stageEntity.JOBLIST.Add(curNoExistJobEntity);

                                                            #endregion

                                                            #region [ Add Back Info ]

                                                            string[] cur1stJobInfo = stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo][tmpJobSlotNo].Split('_');

                                                            if (cur1stJobInfo.Length >= 2)
                                                            {
                                                                curExistJobEntity.SLOTNO = tmpJobSlotNo.ToString().PadLeft(3, '0');
                                                                curExistJobEntity.CASSETTESEQNO = cur1stJobInfo[0];
                                                                curExistJobEntity.JOBSEQNO = cur1stJobInfo[1];
                                                                curExistJobEntity.JOBEXIST = "2";  //Exist

                                                                Job curJob = ObjectManager.JobManager.GetJob(curExistJobEntity.CASSETTESEQNO, curExistJobEntity.JOBSEQNO);

                                                                if (curJob != null)
                                                                {
                                                                    curExistJobEntity.TRACKINGVALUE = curJob.TrackingData;

                                                                }

                                                                _stageEntity.JOBLIST.Add(curExistJobEntity);
                                                            }

                                                            #endregion

                                                        }
                                                        else
                                                        {
                                                            #region [ Add Front Info ]

                                                            string[] cur1stJobInfo = stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo][tmpJobSlotNo].Split('_');

                                                            if (cur1stJobInfo.Length >= 2)
                                                            {
                                                                curExistJobEntity.SLOTNO = tmpJobSlotNo.ToString().PadLeft(3, '0');
                                                                curExistJobEntity.CASSETTESEQNO = cur1stJobInfo[0];
                                                                curExistJobEntity.JOBSEQNO = cur1stJobInfo[1];
                                                                curExistJobEntity.JOBEXIST = "2";  //Exist

                                                                Job curJob = ObjectManager.JobManager.GetJob(curExistJobEntity.CASSETTESEQNO, curExistJobEntity.JOBSEQNO);

                                                                if (curJob != null)
                                                                {
                                                                    curExistJobEntity.TRACKINGVALUE = curJob.TrackingData;

                                                                }

                                                                _stageEntity.JOBLIST.Add(curExistJobEntity);
                                                            }

                                                            #endregion

                                                            #region  [ 表示back為空 Add Back ]

                                                            curNoExistJobEntity.SLOTNO = (tmpCmdSlotNo + 1).ToString().PadLeft(3, '0');
                                                            curNoExistJobEntity.CASSETTESEQNO = "0";
                                                            curNoExistJobEntity.JOBSEQNO = "0";
                                                            curNoExistJobEntity.JOBEXIST = "1";  //NoExist

                                                            _stageEntity.JOBLIST.Add(curNoExistJobEntity);

                                                            #endregion

                                                        }

                                                    }

                                                    #endregion
                                                }
                                                else
                                                {

                                                    #region [ SlotBlockInfo有2片 Front & Back ]

                                                    bool checkFront = false; //判斷是否已經確認過Front

                                                    //by CmdSlotBlockInfo  JobList SlotNo Check
                                                    foreach (int tmpJobSlotNo in stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo].Keys)
                                                    {
                                                        if (checkFront == false)
                                                        {
                                                            UniAuto.UniBCS.OpiSpec.RobotStageInfoRequestReply.JOBc curFrontJobEntity = new OpiSpec.RobotStageInfoRequestReply.JOBc();
                                                            string[] cur1stJobInfo = stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo][tmpJobSlotNo].Split('_');

                                                            if (cur1stJobInfo.Length >= 2)
                                                            {
                                                                curFrontJobEntity.SLOTNO = tmpJobSlotNo.ToString().PadLeft(3, '0');
                                                                curFrontJobEntity.CASSETTESEQNO = cur1stJobInfo[0];
                                                                curFrontJobEntity.JOBSEQNO = cur1stJobInfo[1];
                                                                curFrontJobEntity.JOBEXIST = "2";  //Exist

                                                                Job curJob = ObjectManager.JobManager.GetJob(curFrontJobEntity.CASSETTESEQNO, curFrontJobEntity.JOBSEQNO);

                                                                if (curJob != null)
                                                                {
                                                                    curFrontJobEntity.TRACKINGVALUE = curJob.TrackingData;

                                                                }

                                                                _stageEntity.JOBLIST.Add(curFrontJobEntity);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            UniAuto.UniBCS.OpiSpec.RobotStageInfoRequestReply.JOBc curBackJobEntity = new OpiSpec.RobotStageInfoRequestReply.JOBc();
                                                            string[] cur2ndJobInfo = stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo][tmpJobSlotNo].Split('_');

                                                            if (cur2ndJobInfo.Length >= 2)
                                                            {
                                                                curBackJobEntity.SLOTNO = tmpJobSlotNo.ToString().PadLeft(3, '0');
                                                                curBackJobEntity.CASSETTESEQNO = cur2ndJobInfo[0];
                                                                curBackJobEntity.JOBSEQNO = cur2ndJobInfo[1];
                                                                curBackJobEntity.JOBEXIST = "2";  //Exist

                                                                Job curJob = ObjectManager.JobManager.GetJob(curBackJobEntity.CASSETTESEQNO, curBackJobEntity.JOBSEQNO);

                                                                if (curJob != null)
                                                                {
                                                                    curBackJobEntity.TRACKINGVALUE = curJob.TrackingData;

                                                                }

                                                                _stageEntity.JOBLIST.Add(curBackJobEntity);
                                                            }

                                                        }
                                                    }

                                                    #endregion

                                                }

                                                #endregion

                                            }

                                        }

                                    }

                                    #endregion
                                }
                                else
                                {
                                    #region [ for Normal Robot Type Use ]

                                    if (!string.IsNullOrEmpty(stage.File.CurSendOut_CSTSeq) &&
                                        !string.IsNullOrEmpty(stage.File.CurSendOut_JobSeq))
                                    {

                                        UniAuto.UniBCS.OpiSpec.RobotStageInfoRequestReply.JOBc cur1stJobEntity = new OpiSpec.RobotStageInfoRequestReply.JOBc();
                                        //針對Dual Mode  永遠會用 SendOut01 位置要特別處理
                                        cur1stJobEntity.SLOTNO = "001";
                                        cur1stJobEntity.CASSETTESEQNO = stage.File.CurSendOut_CSTSeq;
                                        cur1stJobEntity.JOBSEQNO = stage.File.CurSendOut_JobSeq;
                                        cur1stJobEntity.JOBEXIST = "2";  //Exist

                                        Job curJob = ObjectManager.JobManager.GetJob(cur1stJobEntity.CASSETTESEQNO, cur1stJobEntity.JOBSEQNO);

                                        if (curJob != null)
                                        {
                                            cur1stJobEntity.TRACKINGVALUE = curJob.TrackingData;

                                        }

                                        _stageEntity.JOBLIST.Add(cur1stJobEntity);

                                    }

                                    if (!string.IsNullOrEmpty(stage.File.CurSendOut_CSTSeq02) &&
                                        !string.IsNullOrEmpty(stage.File.CurSendOut_JobSeq02))
                                    {

                                        UniAuto.UniBCS.OpiSpec.RobotStageInfoRequestReply.JOBc cur2ndJobEntity = new OpiSpec.RobotStageInfoRequestReply.JOBc();
                                        cur2ndJobEntity.SLOTNO = "002";
                                        cur2ndJobEntity.CASSETTESEQNO = stage.File.CurSendOut_CSTSeq02;
                                        cur2ndJobEntity.JOBSEQNO = stage.File.CurSendOut_JobSeq02;
                                        cur2ndJobEntity.JOBEXIST = "2";  //Exist

                                        Job cur2ndJob = ObjectManager.JobManager.GetJob(cur2ndJobEntity.CASSETTESEQNO, cur2ndJobEntity.JOBSEQNO);

                                        if (cur2ndJob != null)
                                        {
                                            cur2ndJobEntity.TRACKINGVALUE = cur2ndJob.TrackingData;

                                        }

                                        _stageEntity.JOBLIST.Add(cur2ndJobEntity);

                                    }

                                    #endregion
                                }
                            }

                            #endregion
                        
                            _robotEntity.STAGELIST.Add(_stageEntity);

                        }
                        else
                        {

                            #region [ Is Port Type Stage Info ]

                            //送給OPI LayOut 要考慮Port 如何送出Job資訊
                            RobotStageInfoRequestReply.STAGEc _stageEntity = new RobotStageInfoRequestReply.STAGEc();

                            _stageEntity.STAGEID = stage.Data.STAGEID;

                            //STAGESTATUS: 0:Unknown,1:NoExist, 2:LDRQ, 3:UDRQ
                            switch (stage.File.CurStageStatus)
                            {
                                case eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY://"UDRQ_LDRQ":
                                    _stageEntity.STAGESTATUS = "4";
                                    break;
                                case eRobotStageStatus.RECEIVE_READY://"LDRQ":
                                    _stageEntity.STAGESTATUS = "2";
                                    break;
                                case eRobotStageStatus.SEND_OUT_READY://"UDRQ":
                                    _stageEntity.STAGESTATUS = "3";
                                    break;
                                case eRobotStageStatus.NO_REQUEST:// "NOREQ":
                                    _stageEntity.STAGESTATUS = "1";
                                    break;
                                default:
                                    _stageEntity.STAGESTATUS = "0";
                                    break;
                            }

                            #region [ 只有UDRQ時才會更新 ]

                            if (stage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_READY || stage.File.CurStageStatus == eRobotStageStatus.SEND_OUT_AND_RECEIVE_READY)
                            {

                                //20151126 add by Port Arm Job Qty來區分
                                if (curRobot.Data.ARMJOBQTY == 2)
                                {
                                    //20160112 modify
                                    #region [ 由Stage UDRQ SlotBlockInfo List來取得顯示資訊 最多顯示2各SlotBlockInfo 共4片資訊 ]

                                    if (stage.curUDRQ_SlotBlockInfoList.Count > 0)
                                    {
                                        int tmpCount = 0;

                                        //By Cmd SlotNo Check
                                        foreach (int tmpCmdSlotNo in stage.curUDRQ_SlotBlockInfoList.Keys)
                                        {
                                            tmpCount = tmpCount + 1;

                                            if (tmpCount < 3) //Check 1st CmdSlot Front
                                            {
                                                #region [ Check 1st CmdSlotInfo ]

                                                if (stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo].Count == 1)
                                                {

                                                    #region [ SlotBlockInfo只有一片 Front or Back ]

                                                    foreach (int tmpJobSlotNo in stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo].Keys)
                                                    {

                                                        UniAuto.UniBCS.OpiSpec.RobotStageInfoRequestReply.JOBc curExistJobEntity = new OpiSpec.RobotStageInfoRequestReply.JOBc();
                                                        UniAuto.UniBCS.OpiSpec.RobotStageInfoRequestReply.JOBc curNoExistJobEntity = new OpiSpec.RobotStageInfoRequestReply.JOBc();

                                                        //CmdSlotNo都是Front SlotNo
                                                        if (tmpCmdSlotNo != tmpJobSlotNo)
                                                        {
                                                            #region [ 表示Front為空 Add Front ]

                                                            curNoExistJobEntity.SLOTNO = tmpCmdSlotNo.ToString().PadLeft(3, '0');
                                                            curNoExistJobEntity.CASSETTESEQNO = "0";
                                                            curNoExistJobEntity.JOBSEQNO = "0";
                                                            curNoExistJobEntity.JOBEXIST = "1";  //NoExist

                                                            _stageEntity.JOBLIST.Add(curNoExistJobEntity);

                                                            #endregion

                                                            #region [ Add Back Info ]

                                                            string[] cur1stJobInfo = stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo][tmpJobSlotNo].Split('_');

                                                            if (cur1stJobInfo.Length >= 2)
                                                            {
                                                                curExistJobEntity.SLOTNO = tmpJobSlotNo.ToString().PadLeft(3, '0');
                                                                curExistJobEntity.CASSETTESEQNO = cur1stJobInfo[0];
                                                                curExistJobEntity.JOBSEQNO = cur1stJobInfo[1];
                                                                curExistJobEntity.JOBEXIST = "2";  //Exist

                                                                Job curJob = ObjectManager.JobManager.GetJob(curExistJobEntity.CASSETTESEQNO, curExistJobEntity.JOBSEQNO);

                                                                if (curJob != null)
                                                                {
                                                                    curExistJobEntity.TRACKINGVALUE = curJob.TrackingData;

                                                                }

                                                                _stageEntity.JOBLIST.Add(curExistJobEntity);
                                                            }

                                                            #endregion

                                                        }
                                                        else
                                                        {
                                                            #region [ Add Front Info ]

                                                            string[] cur1stJobInfo = stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo][tmpJobSlotNo].Split('_');

                                                            if (cur1stJobInfo.Length >= 2)
                                                            {
                                                                curExistJobEntity.SLOTNO = tmpJobSlotNo.ToString().PadLeft(3, '0');
                                                                curExistJobEntity.CASSETTESEQNO = cur1stJobInfo[0];
                                                                curExistJobEntity.JOBSEQNO = cur1stJobInfo[1];
                                                                curExistJobEntity.JOBEXIST = "2";  //Exist

                                                                Job curJob = ObjectManager.JobManager.GetJob(curExistJobEntity.CASSETTESEQNO, curExistJobEntity.JOBSEQNO);

                                                                if (curJob != null)
                                                                {
                                                                    curExistJobEntity.TRACKINGVALUE = curJob.TrackingData;

                                                                }

                                                                _stageEntity.JOBLIST.Add(curExistJobEntity);
                                                            }

                                                            #endregion

                                                            #region  [ 表示back為空 Add Back ]

                                                            curNoExistJobEntity.SLOTNO = (tmpCmdSlotNo + 1).ToString().PadLeft(3, '0');
                                                            curNoExistJobEntity.CASSETTESEQNO = "0";
                                                            curNoExistJobEntity.JOBSEQNO = "0";
                                                            curNoExistJobEntity.JOBEXIST = "1";  //NoExist

                                                            _stageEntity.JOBLIST.Add(curNoExistJobEntity);

                                                            #endregion

                                                        }

                                                    }

                                                    #endregion
                                                }
                                                else
                                                {

                                                    #region [ SlotBlockInfo有2片 Front & Back ]

                                                    bool checkFront = false; //判斷是否已經確認過Front

                                                    //by CmdSlotBlockInfo  JobList SlotNo Check
                                                    foreach (int tmpJobSlotNo in stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo].Keys)
                                                    {
                                                        if (checkFront == false)
                                                        {
                                                            UniAuto.UniBCS.OpiSpec.RobotStageInfoRequestReply.JOBc curFrontJobEntity = new OpiSpec.RobotStageInfoRequestReply.JOBc();
                                                            string[] cur1stJobInfo = stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo][tmpJobSlotNo].Split('_');

                                                            if (cur1stJobInfo.Length >= 2)
                                                            {
                                                                curFrontJobEntity.SLOTNO = tmpJobSlotNo.ToString().PadLeft(3, '0');
                                                                curFrontJobEntity.CASSETTESEQNO = cur1stJobInfo[0];
                                                                curFrontJobEntity.JOBSEQNO = cur1stJobInfo[1];
                                                                curFrontJobEntity.JOBEXIST = "2";  //Exist

                                                                Job curJob = ObjectManager.JobManager.GetJob(curFrontJobEntity.CASSETTESEQNO, curFrontJobEntity.JOBSEQNO);

                                                                if (curJob != null)
                                                                {
                                                                    curFrontJobEntity.TRACKINGVALUE = curJob.TrackingData;

                                                                }

                                                                _stageEntity.JOBLIST.Add(curFrontJobEntity);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            UniAuto.UniBCS.OpiSpec.RobotStageInfoRequestReply.JOBc curBackJobEntity = new OpiSpec.RobotStageInfoRequestReply.JOBc();
                                                            string[] cur2ndJobInfo = stage.curUDRQ_SlotBlockInfoList[tmpCmdSlotNo][tmpJobSlotNo].Split('_');

                                                            if (cur2ndJobInfo.Length >= 2)
                                                            {
                                                                curBackJobEntity.SLOTNO = tmpJobSlotNo.ToString().PadLeft(3, '0');
                                                                curBackJobEntity.CASSETTESEQNO = cur2ndJobInfo[0];
                                                                curBackJobEntity.JOBSEQNO = cur2ndJobInfo[1];
                                                                curBackJobEntity.JOBEXIST = "2";  //Exist

                                                                Job curJob = ObjectManager.JobManager.GetJob(curBackJobEntity.CASSETTESEQNO, curBackJobEntity.JOBSEQNO);

                                                                if (curJob != null)
                                                                {
                                                                    curBackJobEntity.TRACKINGVALUE = curJob.TrackingData;

                                                                }

                                                                _stageEntity.JOBLIST.Add(curBackJobEntity);
                                                            }

                                                        }
                                                    }

                                                    #endregion

                                                }

                                                #endregion

                                            }

                                        }

                                    }

                                    #endregion
                                    
                                }
                                else
                                {

                                    #region [ Port Type 永遠Show 2各Slot for 1Arm 1Job ]

                                    if (stage.curUDRQ_SlotList.Count > 0)
                                    {
                                        int tmpCount = 0;

                                        foreach (int key in stage.curUDRQ_SlotList.Keys)
                                        {
                                            tmpCount = tmpCount + 1;

                                            if (tmpCount == 1)
                                            {

                                                #region [ Check 1st SendOut Job Info ]

                                                UniAuto.UniBCS.OpiSpec.RobotStageInfoRequestReply.JOBc cur1stJobEntity = new OpiSpec.RobotStageInfoRequestReply.JOBc();

                                                cur1stJobEntity.SLOTNO = key.ToString().PadLeft(3, '0');

                                                string[] cur1stJobInfo = stage.curUDRQ_SlotList[key].Split('_');

                                                if (cur1stJobInfo.Length >= 2)
                                                {
                                                    cur1stJobEntity.CASSETTESEQNO = cur1stJobInfo[0];
                                                    cur1stJobEntity.JOBSEQNO = cur1stJobInfo[1];
                                                    cur1stJobEntity.JOBEXIST = "2";  //Exist

                                                    Job curJob = ObjectManager.JobManager.GetJob(cur1stJobEntity.CASSETTESEQNO, cur1stJobEntity.JOBSEQNO);

                                                    if (curJob != null)
                                                    {
                                                        cur1stJobEntity.TRACKINGVALUE = curJob.TrackingData;

                                                    }

                                                    _stageEntity.JOBLIST.Add(cur1stJobEntity);
                                                }

                                                #endregion

                                            }
                                            else if (tmpCount == 2)
                                            {

                                                #region [ Check 1st SendOut Job Info ]

                                                UniAuto.UniBCS.OpiSpec.RobotStageInfoRequestReply.JOBc cur2ndJobEntity = new OpiSpec.RobotStageInfoRequestReply.JOBc();

                                                cur2ndJobEntity.SLOTNO = key.ToString().PadLeft(3, '0');

                                                string[] cur2ndJobInfo = stage.curUDRQ_SlotList[key].Split('_');

                                                cur2ndJobEntity.CASSETTESEQNO = cur2ndJobInfo[0];
                                                cur2ndJobEntity.JOBSEQNO = cur2ndJobInfo[1];
                                                cur2ndJobEntity.JOBEXIST = "2";  //Exist

                                                Job cur2ndJob = ObjectManager.JobManager.GetJob(cur2ndJobEntity.CASSETTESEQNO, cur2ndJobEntity.JOBSEQNO);

                                                if (cur2ndJob != null)
                                                {
                                                    cur2ndJobEntity.TRACKINGVALUE = cur2ndJob.TrackingData;

                                                }

                                                _stageEntity.JOBLIST.Add(cur2ndJobEntity);

                                                #endregion

                                            }
                                            else
                                            {
                                                break;
                                            }
                                        }
                                    }

                                    #endregion

                                }                          

                            }

                            #endregion

                            #endregion

                            _robotEntity.STAGELIST.Add(_stageEntity);
                        }
                       
                        #endregion

                    }

                    //if (_robotEntity.STAGELIST.Count > 0)
                    //    _robotEntity.STAGELIST.RemoveAt(0);

                    reply.BODY.ROBOTLIST.Add(_robotEntity);

	            }

                if (reply.BODY.ROBOTLIST.Count > 0)
                    reply.BODY.ROBOTLIST.RemoveAt(0);

                xMessage msg = SendReplyToOPI(command, reply);

                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            }
        }

        /// <summary> OPI Message: Send To OPI Robot Command 相關Message Report.對應RobotMessageReport
        ///
        /// </summary>
        public void RobotCommandReport(Robot curRobot, string cmdInfo, string cmdMsgType)
        {
            try
            {
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("RobotMessageReport") as XmlDocument;
                RobotMessageReport trx = Spec.XMLtoMessage(xml_doc) as RobotMessageReport;
                //trx.BODY.LINENAME = rbt.Data.LINEID;
                trx.BODY.LINENAME = curRobot.Data.SERVERNAME;
                trx.BODY.EQUIPMENTNO = curRobot.Data.NODENO;
                trx.BODY.ROBOTNAME = curRobot.Data.ROBOTNAME;
                trx.BODY.MSG_DATETIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff");
                trx.BODY.MSG_DETAIL = cmdInfo;
                trx.BODY.MSG_TYPE = cmdMsgType;

                xMessage msg = SendReportToAllOPI(string.Empty, trx);

                if (dicClient.Count == 0)
                {
                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[ROBOT={0}] [BCS -> OPI][{1}] OPI client number is zero, UIService will not report message to OPI.",
                    curRobot.Data.ROBOTNAME, msg.TransactionID));
                }
                else
                {
                    Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[ROBOT={0}] [BCS -> OPI][{1}] UIService report equipment({2}) message({3}) to OPI.",
                    curRobot.Data.ROBOTNAME, msg.TransactionID, curRobot.Data.NODENO, trx.HEADER.MESSAGENAME));
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary> OPI Message: Send To OPI Robot Real Time Command Report.對應RobotCommandReport
        /// 
        /// </summary>
        /// <param name="curRobot"></param>
        public void RobotRealTimeRobotCommandReport(Robot curRobot)
        {
            try
            {
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("RobotCommandReport") as XmlDocument;
                RobotCommandReport trx = Spec.XMLtoMessage(xml_doc) as RobotCommandReport;

                trx.BODY.LINENAME = curRobot.Data.SERVERNAME;
                trx.BODY.EQUIPMENTNO = curRobot.Data.NODENO;
                trx.BODY.ROBOTNAME = curRobot.Data.ROBOTNAME;

                //For Each Arm之前先清除預設的空白List
                trx.BODY.COMMANDLIST.Clear();

                if (curRobot.Data.ARMJOBQTY != 2)
                {

                    #region [ For 1Arm 1Job Normal Robot Use ]

                    RobotCommandReport.COMMANDc _command1stEntity = new OpiSpec.RobotCommandReport.COMMANDc();          

                    _command1stEntity.COMMAND_DATETIME =curRobot.CurRealTimeSetCommandInfo.CmdCreateDateTime.ToString("yyyy-MM-dd hh:mm:ss.FFFF");
                    _command1stEntity.COMMAND_SEQ = "1";
                    _command1stEntity.CASSETTESEQNO =curRobot.CurRealTimeSetCommandInfo.Cmd01_CSTSeq.ToString();
                    _command1stEntity.JOBSEQNO =curRobot.CurRealTimeSetCommandInfo.Cmd01_JobSeq.ToString();
                    _command1stEntity.ROBOT_COMMAND = GetSendToOPICmdCode(curRobot.CurRealTimeSetCommandInfo.Cmd01_Command);
                    _command1stEntity.ARM_SELECT = GetSendToOPIUseArmCode(curRobot.CurRealTimeSetCommandInfo.Cmd01_ArmSelect, false);
                    _command1stEntity.TARGETPOSITION = curRobot.CurRealTimeSetCommandInfo.Cmd01_TargetPosition.ToString();
                    _command1stEntity.TARGETSLOTNO = curRobot.CurRealTimeSetCommandInfo.Cmd01_TargetSlotNo.ToString();

                    trx.BODY.COMMANDLIST.Add(_command1stEntity);

                    RobotCommandReport.COMMANDc _command2ndEntity = new OpiSpec.RobotCommandReport.COMMANDc();

                    _command2ndEntity.COMMAND_DATETIME = curRobot.CurRealTimeSetCommandInfo.CmdCreateDateTime.ToString("yyyy-MM-dd hh:mm:ss.FFFF");
                    _command2ndEntity.COMMAND_SEQ = "2";
                    _command2ndEntity.CASSETTESEQNO = curRobot.CurRealTimeSetCommandInfo.Cmd02_CSTSeq.ToString();
                    _command2ndEntity.JOBSEQNO = curRobot.CurRealTimeSetCommandInfo.Cmd02_JobSeq.ToString();
                    _command2ndEntity.ROBOT_COMMAND = GetSendToOPICmdCode(curRobot.CurRealTimeSetCommandInfo.Cmd02_Command);
                    _command2ndEntity.ARM_SELECT = GetSendToOPIUseArmCode(curRobot.CurRealTimeSetCommandInfo.Cmd02_ArmSelect, false);
                    _command2ndEntity.TARGETPOSITION = curRobot.CurRealTimeSetCommandInfo.Cmd02_TargetPosition.ToString();
                    _command2ndEntity.TARGETSLOTNO = curRobot.CurRealTimeSetCommandInfo.Cmd02_TargetSlotNo.ToString();

                    trx.BODY.COMMANDLIST.Add(_command2ndEntity);

                    #endregion

                }
                else
                {
                    //20151204 add for Cell Special Robot Use 
                    #region [ For 1Arm 2Job Cell Special Robot Use ]

                    RobotCommandReport.COMMANDc _command1stEntity = new OpiSpec.RobotCommandReport.COMMANDc();

                    _command1stEntity.COMMAND_DATETIME = curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdCreateDateTime.ToString("yyyy-MM-dd hh:mm:ss.FFFF");
                    _command1stEntity.COMMAND_SEQ = "1";
                    _command1stEntity.CASSETTESEQNO = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_FrontCSTSeq.ToString();
                    _command1stEntity.JOBSEQNO = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_FrontJobSeq.ToString();
                    _command1stEntity.CASSETTESEQNO_BACK = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_BackCSTSeq.ToString();
                    _command1stEntity.JOBSEQNO_BACK = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_BackJobSeq.ToString();
                    _command1stEntity.ROBOT_COMMAND = GetSendToOPICmdCode(curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_Command);
                    _command1stEntity.ARM_SELECT = GetSendToOPIUseArmCode(curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_ArmSelect, true);
                    _command1stEntity.TARGETPOSITION = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_TargetPosition.ToString();
                    _command1stEntity.TARGETSLOTNO = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd01_TargetSlotNo.ToString();

                    trx.BODY.COMMANDLIST.Add(_command1stEntity);

                    RobotCommandReport.COMMANDc _command2ndEntity = new OpiSpec.RobotCommandReport.COMMANDc();

                    _command2ndEntity.COMMAND_DATETIME = curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdCreateDateTime.ToString("yyyy-MM-dd hh:mm:ss.FFFF");
                    _command2ndEntity.COMMAND_SEQ = "2";
                    _command2ndEntity.CASSETTESEQNO = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_FrontCSTSeq.ToString();
                    _command2ndEntity.JOBSEQNO = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_FrontJobSeq.ToString();
                    _command2ndEntity.CASSETTESEQNO_BACK = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_BackCSTSeq.ToString();
                    _command2ndEntity.JOBSEQNO_BACK = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_BackJobSeq.ToString();
                    _command2ndEntity.ROBOT_COMMAND = GetSendToOPICmdCode(curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_Command);
                    _command2ndEntity.ARM_SELECT = GetSendToOPIUseArmCode(curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_ArmSelect, true);
                    _command2ndEntity.TARGETPOSITION = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_TargetPosition.ToString();
                    _command2ndEntity.TARGETSLOTNO = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd02_TargetSlotNo.ToString();

                    trx.BODY.COMMANDLIST.Add(_command2ndEntity);

                    RobotCommandReport.COMMANDc _command3rdEntity = new OpiSpec.RobotCommandReport.COMMANDc();

                    _command3rdEntity.COMMAND_DATETIME = curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdCreateDateTime.ToString("yyyy-MM-dd hh:mm:ss.FFFF");
                    _command3rdEntity.COMMAND_SEQ = "3";
                    _command3rdEntity.CASSETTESEQNO = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_FrontCSTSeq.ToString();
                    _command3rdEntity.JOBSEQNO = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_FrontJobSeq.ToString();
                    _command3rdEntity.CASSETTESEQNO_BACK = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_BackCSTSeq.ToString();
                    _command3rdEntity.JOBSEQNO_BACK = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_BackJobSeq.ToString();
                    _command3rdEntity.ROBOT_COMMAND = GetSendToOPICmdCode(curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_Command);
                    _command3rdEntity.ARM_SELECT = GetSendToOPIUseArmCode(curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_ArmSelect, true);
                    _command3rdEntity.TARGETPOSITION = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_TargetPosition.ToString();
                    _command3rdEntity.TARGETSLOTNO = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd03_TargetSlotNo.ToString();

                    trx.BODY.COMMANDLIST.Add(_command3rdEntity);

                    RobotCommandReport.COMMANDc _command4thEntity = new OpiSpec.RobotCommandReport.COMMANDc();

                    _command4thEntity.COMMAND_DATETIME = curRobot.CurCellSpecialRealTimeSetCommandInfo.CmdCreateDateTime.ToString("yyyy-MM-dd hh:mm:ss.FFFF");
                    _command4thEntity.COMMAND_SEQ = "3";
                    _command4thEntity.CASSETTESEQNO = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_FrontCSTSeq.ToString();
                    _command4thEntity.JOBSEQNO = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_FrontJobSeq.ToString();
                    _command4thEntity.CASSETTESEQNO_BACK = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_BackCSTSeq.ToString();
                    _command4thEntity.JOBSEQNO_BACK = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_BackJobSeq.ToString();
                    _command4thEntity.ROBOT_COMMAND = GetSendToOPICmdCode(curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_Command);
                    _command4thEntity.ARM_SELECT = GetSendToOPIUseArmCode(curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_ArmSelect, true);
                    _command4thEntity.TARGETPOSITION = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_TargetPosition.ToString();
                    _command4thEntity.TARGETSLOTNO = curRobot.CurCellSpecialRealTimeSetCommandInfo.Cmd04_TargetSlotNo.ToString();

                    trx.BODY.COMMANDLIST.Add(_command3rdEntity);

                    #endregion

                }


                xMessage msg = SendReportToAllOPI(string.Empty, trx);

                if (dicClient.Count == 0)
                {
                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[ROBOT={0}] [BCS -> OPI][{1}] OPI client number is zero, UIService will not report message to OPI.",
                    curRobot.Data.ROBOTNAME, msg.TransactionID));
                }
                else
                {
                    Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[ROBOT={0}] [BCS -> OPI][{1}] UIService report equipment({2}) message({3}) to OPI.",
                    curRobot.Data.ROBOTNAME, msg.TransactionID, curRobot.Data.NODENO, trx.HEADER.MESSAGENAME));
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }

        }

        private string GetSendToOPICmdCode(int commandCode)
        {
            string tmpCode = string.Empty;

            try
            {
                switch (commandCode)
                {
                    case 0:
                        //要給OPI Empty 才不會顯示
                        tmpCode= string.Empty;// "0: None";
                        break;

                        case 1:

                        tmpCode= "1: Put";
                        break;
                        case 2:

                        tmpCode= "2: Get";
                        break;
                        case 4:

                        tmpCode= "4: Exchange";
                        break;
                        case 8:

                        tmpCode= "8: Put Ready";
                        break;

                        case 16:

                        tmpCode= "16: Get Ready";
                        break;

                        case 32:

                        tmpCode = "32: Get/Put";
                        break;

                        //20151025 add Multi-Get/Put 64: Multi-Put 128:Multi-Get

                        case 64:

                        tmpCode = "64: Multi_Put";
                        break;

                        case 128:

                        tmpCode = "128: Multi_Get";
                        break;
                        
                        case 256:

                        tmpCode = "256: RTC_PUT";
                        break;

                        //20160511 add 512:RECIPEGROUPEND_PUT 1024:Multi_RECIPEGROUPEND_PUT
                        case 512:

                        tmpCode = "512: RECIPEGROUPEND_PUT";
                        break;

                        case 1024:

                        tmpCode = "1024: Multi_RECIPEGROUPEND_PUT";
                        break;

                    default:
                        break;
                }

                return tmpCode;
            }
            catch (Exception ex)
            {

                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return string.Empty;
            }

        }

        private string GetSendToOPIUseArmCode(int useArmCode, bool isCellSpecialFlag)
        {
            string tmpCode = string.Empty;

            try
            {
                //SPEC定義
                //0: None
                //1: Upper/Left Arm
                //2: Lower/Left Arm
                //3: Left Both Arm 
                //4: Upper/Right Arm
                //8: Lower/Right Arm
                //12: Right Both Arm

                //Define OPI
                //0: None
                //1: Upper Arm
                //2: Lower Arm
                //3: Both Arm

                //4 Arm
                //0: None
                //1:Upper/Left Arm
                //2:Lower/Left Arm
                //3:Left Both Arm
                //4:Upper/Right Arm
                //8:Lower/Right Arm
                //12:Right Both Arm
                //20160127 add 5 and 10
                //5: Upper Both Arm 
                //10: Lower Both Arm

                if (isCellSpecialFlag == true)
                {
                    #region [ Cell Special Robot ]

                    switch (useArmCode)
                    {
                        case 0:

                            tmpCode = "0: None";
                            break;

                        case 1:

                            tmpCode = "1:Upper/Left Arm";
                            break;
                        case 2:

                            tmpCode = "2:Lower/Left Arm";
                            break;
                        case 3:

                            tmpCode = "3:Left Both Arm";
                            break;

                        case 4:

                            tmpCode = "4:Upper/Right Arm";
                            break;
                        case 8:

                            tmpCode = "8:Lower/Right Arm";
                            break;
                        case 12:

                            tmpCode = "12:Right Both Arm";
                            break;

                        //20160127 add 5 and 10
                        //5: Upper Both Arm 
                        //10: Lower Both Arm
                        case 5:

                            tmpCode = "5:Upper Both Arm ";
                            break;

                        case 10:

                            tmpCode = "10:Lower Both Arm";
                            break;

                        default:
                            break;
                    }

                    #endregion

                }
                else
                {
                    #region [ Normal Robot ]

                    switch (useArmCode)
                    {
                        case 0:

                            tmpCode = "0: None";
                            break;

                        case 1:

                            tmpCode = "1: Upper Arm";
                            break;
                        case 2:

                            tmpCode = "2: Lower Arm";
                            break;
                        case 3:

                            tmpCode = "3: Both Arm";
                            break;

                        default:
                            break;
                    }

                    #endregion

                }

                return tmpCode;
            }
            catch (Exception ex)
            {

                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                return string.Empty;
            }

        }

        /// <summary> OPI MessageSet: Robot Unloader Dispatch Rule Request
        ///
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_RobotUnloaderDispatchRuleRequest(XmlDocument xmlDoc)
        {
            IServerAgent agent = GetServerAgent();
            RobotUnloaderDispatchRuleRequest command = Spec.XMLtoMessage(xmlDoc) as RobotUnloaderDispatchRuleRequest;
            XmlDocument xml_doc = agent.GetTransactionFormat("RobotUnloaderDispatchRuleReply") as XmlDocument;
            RobotUnloaderDispatchRuleReply reply = Spec.XMLtoMessage(xml_doc) as RobotUnloaderDispatchRuleReply;

            try
            {
                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. LINENAME={3}",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.LINENAME));

                reply.BODY.LINENAME = command.BODY.LINENAME;

                IList<Line> lines = ObjectManager.LineManager.GetLines();

                if (lines == null)
                {
                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                    reply.RETURN.RETURNCODE = "0010920";
                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
                }
                else
                {
                    reply.BODY.PORTLIST.Clear();
                    //[ Wait_Proc_00032 ] Unloader Dispatch Rule需要by T3 CF Cell (Array目前沒有) 修正
                    foreach (KeyValuePair<string, clsDispatchRule> rule in lines[0].File.UnlaoderDispatchRule)
                    {
                        Port port = ObjectManager.PortManager.GetPort(rule.Key);
                        reply.BODY.PORTLIST.Add(new RobotUnloaderDispatchRuleReply.PORTc()
                        {
                            EQUIPMENTNO = port == null ? "" : port.Data.NODENO,
                            PORTNO = port == null ? "" : port.Data.PORTNO,
                            PORTID = port == null ? "" : port.Data.PORTID,
                            GRADE_1 = rule.Value.Grade1,
                            GRADE_2 = rule.Value.Grade2,
                            GRADE_3 = rule.Value.Grade3,
                            //GRADE_4 = rule.Value.Grade4,
                            //ABNORMALCODE_1 = rule.Value.AbnormalCode1,
                            //ABNORMALCODE_2 = rule.Value.AbnormalCode2,
                            //ABNORMALCODE_3 = rule.Value.AbnormalCode3,
                            //ABNORMALCODE_4 = rule.Value.AbnormalCode4,
                            //ABNORMALFLAG = rule.Value.AbnormalFlag,
                            OPERATORID = rule.Value.OperatorID
                        });
                    }
                }

                xMessage msg = SendReplyToOPI(command, reply);

                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            }
        }

        /// <summary> OPI MessageSet: Robot Unloader Dispatch Rule Report
        ///
        /// </summary>
        public void RobotUnloaderDispatchRuleReport(string trxID)
        {
            try
            {
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("RobotUnloaderDispatchRuleReport") as XmlDocument;
                RobotUnloaderDispatchRuleReport trx = Spec.XMLtoMessage(xml_doc) as RobotUnloaderDispatchRuleReport;

                trx.BODY.LINENAME = Workbench.ServerName;
                IList<Line> lines = ObjectManager.LineManager.GetLines();

                trx.BODY.PORTLIST.Clear();
                //[ Wait_Proc_00032 ] Unloader Dispatch Rule需要by T3 CF Cell (Array目前沒有) 修正
                foreach (KeyValuePair<string, clsDispatchRule> rule in lines[0].File.UnlaoderDispatchRule)
                {
                    Port port = ObjectManager.PortManager.GetPort(rule.Key);
                    trx.BODY.PORTLIST.Add(new RobotUnloaderDispatchRuleReport.PORTc()
                    {
                        EQUIPMENTNO = port == null ? "" : port.Data.NODENO,
                        PORTNO = port == null ? "" : port.Data.PORTNO,
                        PORTID = port == null ? "" : port.Data.PORTID,
                        GRADE_1 = rule.Value.Grade1,
                        GRADE_2 = rule.Value.Grade2,
                        GRADE_3 = rule.Value.Grade3,
                        //GRADE_4 = rule.Value.Grade4,
                        //ABNORMALCODE_1 = rule.Value.AbnormalCode1,
                        //ABNORMALCODE_2 = rule.Value.AbnormalCode2,
                        //ABNORMALCODE_3 = rule.Value.AbnormalCode3,
                        //ABNORMALCODE_4 = rule.Value.AbnormalCode4,
                        //ABNORMALFLAG = rule.Value.AbnormalFlag,
                        OPERATORID = rule.Value.OperatorID
                    });
                }

                xMessage msg = SendReportToAllOPI(trxID, trx);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary> OPI MessageSet: Robot Unloader Dispatch Rule Report Reply
        ///
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_RobotUnloaderDispatchRuleReportReply(XmlDocument xmlDoc)
        {
            try
            {
                //Do Nothing
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        //public void OPI_RobotRouteCurrentStepInfoRequest(XmlDocument xmlDoc)
        //{
        //    try
        //    {
        //        //Do Nothing
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        //20151030 add
        /// <summary> OPI MessageSet: Robot Query Job Current Step Info
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_RobotRouteCurrentStepNoRequest(XmlDocument xmlDoc)
        {

            IServerAgent agent = GetServerAgent();
            RobotRouteCurrentStepNoRequest command = Spec.XMLtoMessage(xmlDoc) as RobotRouteCurrentStepNoRequest;
            RobotRouteCurrentStepNoReply reply = new RobotRouteCurrentStepNoReply();

            try
            {
                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}]", command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                reply.BODY.LINENAME = command.BODY.LINENAME;

                foreach (Robot curRobot in ObjectManager.RobotManager.GetRobots())
                {
                    reply.BODY.ROBOTNAME = curRobot.Data.ROBOTNAME;
                    break;
                }

                reply.BODY.CASSETTESEQNO = command.BODY.CASSETTESEQNO;
                reply.BODY.JOBSEQNO = command.BODY.JOBSEQNO;


                Job bcsJob = ObjectManager.JobManager.GetJob(command.BODY.CASSETTESEQNO, command.BODY.JOBSEQNO);

                if (bcsJob != null)
                {

                    reply.BODY.GLASSID = bcsJob.GlassChipMaskBlockID;
                    reply.BODY.CURRENTROUTEID = bcsJob.RobotWIP.CurRouteID;
                    reply.BODY.CURRENTSTEPNO = bcsJob.RobotWIP.CurStepNo.ToString();
                    reply.BODY.NEXTSTEPNO = bcsJob.RobotWIP.NextStepNo.ToString();

                }

                xMessage msg = SendReplyToOPI(command, reply);

                Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] {2} OK", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.GetType().Name));
            }
            catch (Exception ex)
            {
                reply.RETURN.RETURNMESSAGE = ex.Message;
                xMessage msg = SendReplyToOPI(command, reply);

                //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            }

        }

        //20151030 add
        /// <summary> OPI MessageSet: Robot Change Job Current StepNo/Next StepNo
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_RobotRouteCurrentStepChangeRequest(XmlDocument xmlDoc)
        {
            string errMsg = string.Empty;
            xMessage msg = null;

            try
            {
                IServerAgent agent = GetServerAgent();
                RobotRouteCurrentStepChangeRequest command = Spec.XMLtoMessage(xmlDoc) as RobotRouteCurrentStepChangeRequest;
                RobotRouteCurrentStepChangeReply reply = new RobotRouteCurrentStepChangeReply();

                try
                {
                    Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}]", command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                    reply.BODY.LINENAME = command.BODY.LINENAME;
                    reply.BODY.CASSETTESEQNO = command.BODY.CASSETTESEQNO;
                    reply.BODY.JOBSEQNO = command.BODY.JOBSEQNO;

                    #region [ Check Robot Mode. 必須要是Semi Mode才可以變更Step 以免對AUTO邏輯造成影響 ]

                    Robot curRobot = ObjectManager.RobotManager.GetRobotbySeverName(Workbench.ServerName);

                    if (curRobot == null)
                    {

                        reply.RETURN.RETURNCODE = eRobotUIService_RetrunErrCode.GET_ROBOT_FAIL;
                        reply.RETURN.RETURNMESSAGE = string.Format("Can Not Find Robot Entity by ServerName({0})", Workbench.ServerName);

                        msg = SendReplyToOPI(command, reply);

                        Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] {2} OK", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.GetType().Name));
                        return;
                    }

                    if (curRobot.File.curRobotRunMode != eRobot_RunMode.SEMI_MODE)
                    {
                        reply.RETURN.RETURNCODE = eRobotUIService_RetrunErrCode.RUNMODE_NOT_SEMI_STEP_CHNAGE_FAIL;
                        reply.RETURN.RETURNMESSAGE = string.Format("Robot({0}) curRunMode=({1}) is not SEMI Mode! Can not Modify Step!", curRobot.Data.ROBOTNAME, curRobot.File.curRobotRunMode);

                        msg = SendReplyToOPI(command, reply);

                        Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] {2} OK", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.GetType().Name));
                        return;

                    }

                    #endregion

                    Job bcsJob = ObjectManager.JobManager.GetJob(command.BODY.CASSETTESEQNO, command.BODY.JOBSEQNO);

                    if (bcsJob != null)
                    {
                        //Update Step
                        errMsg = (string)Invoke(eServiceName.RobotStatusService, "OPIChangeStepID", new object[] { bcsJob, command.BODY.NEWSTEPNO , command.BODY.NEXTSTEPNO });

                        if (errMsg != string.Empty)
                        {
                            reply.RETURN.RETURNCODE = eRobotUIService_RetrunErrCode.CHANGE_STEP_FAIL;
                            reply.RETURN.RETURNMESSAGE = string.Format("Job CassetteSequenceNo({0}) JobSequenceNo({1}) curStepNo({2}), NextStepNo({3}) Change NewCurStepNo({4}), NewNextStepNo({5}) Fail, ErrMsg({6})",
                                                                        reply.BODY.CASSETTESEQNO, reply.BODY.JOBSEQNO, bcsJob.RobotWIP.CurStepNo.ToString(), bcsJob.RobotWIP.NextStepNo.ToString(),
                                                                        command.BODY.NEWSTEPNO, command.BODY.NEXTSTEPNO, errMsg);
                        }

                    }
                    else
                    {
                        reply.RETURN.RETURNCODE = eRobotUIService_RetrunErrCode.GET_JOB_FAIL;
                        reply.RETURN.RETURNMESSAGE = string.Format("Can Not Find Job WIP by CassetteSequenceNo({0}) JobSequenceNo({1})", reply.BODY.CASSETTESEQNO, reply.BODY.JOBSEQNO);
                    }

                    msg = SendReplyToOPI(command, reply);

                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS -> OPI][{1}] {2} OK", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.GetType().Name));
                }
                catch (Exception ex)
                {
                    reply.RETURN.RETURNMESSAGE = ex.Message;
                    msg = SendReplyToOPI(command, reply);

                    //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void OPI_RobotWipCreateRequest(XmlDocument xmlDoc)
        {
            string errMsg = string.Empty;
            xMessage msg = null;
            try
            {
                IServerAgent agent = GetServerAgent();
                RobotWipCreateRequest command = Spec.XMLtoMessage(xmlDoc) as RobotWipCreateRequest;
                RobotWipCreateReply reply = new RobotWipCreateReply();

                try
                {
                    Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}]", command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                    reply.BODY.LINENAME = command.BODY.LINENAME;
                    reply.BODY.CASSETTESEQNO = command.BODY.CASSETTESEQNO;
                    reply.BODY.JOBSEQNO = command.BODY.JOBSEQNO;

                    #region [ Check Robot Mode. 必須要是Semi Mode才可以變更Step 以免對AUTO邏輯造成影響 ]

                    Robot curRobot = ObjectManager.RobotManager.GetRobotbySeverName(Workbench.ServerName);

                    if (curRobot == null)
                    {

                        reply.RETURN.RETURNCODE = eRobotUIService_RetrunErrCode.GET_ROBOT_FAIL;
                        reply.RETURN.RETURNMESSAGE = string.Format("Can Not Find Robot Entity by ServerName({0})", Workbench.ServerName);

                        msg = SendReplyToOPI(command, reply);

                        Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] {2} OK", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.GetType().Name));
                        return;
                    }

                    if (curRobot.File.curRobotRunMode != eRobot_RunMode.SEMI_MODE)
                    {
                        reply.RETURN.RETURNCODE = eRobotUIService_RetrunErrCode.RUNMODE_NOT_SEMI_STEP_CHNAGE_FAIL;
                        reply.RETURN.RETURNMESSAGE = string.Format("Robot({0}) curRunMode=({1}) is not SEMI Mode! Can not Modify Step!", curRobot.Data.ROBOTNAME, curRobot.File.curRobotRunMode);

                        msg = SendReplyToOPI(command, reply);

                        Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] {2} OK", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.GetType().Name));
                        return;

                    }

                    #endregion

                    Job bcsJob = ObjectManager.JobManager.GetJob(command.BODY.CASSETTESEQNO, command.BODY.JOBSEQNO);
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP("L2");

                    if (bcsJob != null)
                    {
                        //bool result = (bool)Invoke(eServiceName.RobotStatusService, "CreateJobRobotWIPInfo", new object[] { eqp.Data.NODENO, bcsJob, errMsg, command.BODY.NEWROUTEID, Int32.Parse(command.BODY.NEWSTEPNO), Int32.Parse(command.BODY.NEXTSTEPNO) });

                        object[] parameters = new object[] { eqp.Data.NODENO, bcsJob, errMsg , command.BODY.NEWROUTEID, Int32.Parse(command.BODY.NEWSTEPNO), Int32.Parse(command.BODY.NEXTSTEPNO) };
                        bool result = (bool)Invoke(eServiceName.RobotCoreService, "CreateJobRobotWIPInfo", parameters,
                                new Type[] { typeof(string), typeof(Job), typeof(string).MakeByRefType(),typeof(string),typeof(int),typeof(int) });

                        if ((!result) && (errMsg != string.Empty))
                        {
                            reply.RETURN.RETURNCODE = eRobotUIService_RetrunErrCode.ROBOTWIPCREATE_FAIL;
                            reply.RETURN.RETURNMESSAGE = string.Format("Can Not Create Robot WIP by Job CassetteSequenceNo({0}) JobSequenceNo({1})", bcsJob.CassetteSequenceNo, bcsJob.JobSequenceNo);
                        }

                    }
                    else
                    {
                        reply.RETURN.RETURNCODE = eRobotUIService_RetrunErrCode.ROBOTWIPCREATE_FAIL;
                        reply.RETURN.RETURNMESSAGE = string.Format("Can Not Find Job WIP by CassetteSequenceNo({0}) JobSequenceNo({1})", reply.BODY.CASSETTESEQNO, reply.BODY.JOBSEQNO);
                    }

                    msg = SendReplyToOPI(command, reply);

                }
                catch (Exception ex)
                {
                    reply.RETURN.RETURNMESSAGE = ex.Message;
                    msg = SendReplyToOPI(command, reply);

                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary> OPI MessageSet: Robot Semi Command Request
        ///
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_RobotCommandRequest(XmlDocument xmlDoc)
        {
            IServerAgent agent = GetServerAgent();
            RobotCommandRequest command = Spec.XMLtoMessage(xmlDoc) as RobotCommandRequest;
            XmlDocument xml_doc = agent.GetTransactionFormat("RobotCommandReply") as XmlDocument;
            RobotCommandReply reply = Spec.XMLtoMessage(xml_doc) as RobotCommandReply;

            try
            {
                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, ROBOTNAME={4}",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO, command.BODY.ROBOTNAME));

                reply.BODY.LINENAME = command.BODY.LINENAME;
                reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                reply.BODY.ROBOTNAME = command.BODY.ROBOTNAME;

                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
                Robot robot = ObjectManager.RobotManager.GetRobot(command.BODY.EQUIPMENTNO);
                RobotCmdInfo cmd = new RobotCmdInfo();
                CellSpecialRobotCmdInfo cellSpecialCmd = new CellSpecialRobotCmdInfo();

                if (eqp == null)
                {
                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
                    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

                    reply.RETURN.RETURNCODE = "0010360";
                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);
                }
                else if (robot == null)
                {
                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[ROBOT={0}] [BCS <- OPI][{1}] Can't find Robot({0}) in RobotEntity.",
                    command.BODY.ROBOTNAME, command.HEADER.TRANSACTIONID));

                    reply.RETURN.RETURNCODE = "0010361";
                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Robot[{0}] in RobotEntity", command.BODY.ROBOTNAME);
                }
                else
                {
                    //20160121 add for Cell Special 1Arm2Job Function
                    if (robot.Data.ARMJOBQTY == 2)
                    {

                        #region [ For Cell Special 1Arm 2Job Use ]

                        //For Cell SEMI Command Function
                        foreach (RobotCommandRequest.COMMANDc curCmd in command.BODY.COMMANDLIST)
                        {

                            switch (curCmd.COMMAND_SEQ)
                            {
                                case "1":

                                    cellSpecialCmd.Cmd01_Command = int.Parse(curCmd.ROBOT_COMMAND);
                                    cellSpecialCmd.Cmd01_ArmSelect = int.Parse(curCmd.ARM_SELECT);
                                    cellSpecialCmd.Cmd01_TargetPosition = int.Parse(curCmd.TARGETPOSITION);
                                    cellSpecialCmd.Cmd01_TargetSlotNo = int.Parse(curCmd.TARGETSLOTNO);

                                    break;

                                case "2":

                                    cellSpecialCmd.Cmd02_Command = int.Parse(curCmd.ROBOT_COMMAND);
                                    cellSpecialCmd.Cmd02_ArmSelect = int.Parse(curCmd.ARM_SELECT);
                                    cellSpecialCmd.Cmd02_TargetPosition = int.Parse(curCmd.TARGETPOSITION);
                                    cellSpecialCmd.Cmd02_TargetSlotNo = int.Parse(curCmd.TARGETSLOTNO);

                                    break;

                                case "3":

                                    cellSpecialCmd.Cmd03_Command = int.Parse(curCmd.ROBOT_COMMAND);
                                    cellSpecialCmd.Cmd03_ArmSelect = int.Parse(curCmd.ARM_SELECT);
                                    cellSpecialCmd.Cmd03_TargetPosition = int.Parse(curCmd.TARGETPOSITION);
                                    cellSpecialCmd.Cmd03_TargetSlotNo = int.Parse(curCmd.TARGETSLOTNO);

                                    break;

                                case "4":

                                    cellSpecialCmd.Cmd04_Command = int.Parse(curCmd.ROBOT_COMMAND);
                                    cellSpecialCmd.Cmd04_ArmSelect = int.Parse(curCmd.ARM_SELECT);
                                    cellSpecialCmd.Cmd04_TargetPosition = int.Parse(curCmd.TARGETPOSITION);
                                    cellSpecialCmd.Cmd04_TargetSlotNo = int.Parse(curCmd.TARGETSLOTNO);

                                    break;

                                default:
                                    break;
                            }

                        }

                        #endregion

                    }
                    else
                    {

                        #region [ For Normal 1Arm 1Job Use ]

                        //For Normal Command Function
                        foreach (RobotCommandRequest.COMMANDc curCmd in command.BODY.COMMANDLIST)
                        {

                            switch (curCmd.COMMAND_SEQ)
                            {
                                case "1":

                                    cmd.Cmd01_Command = int.Parse(curCmd.ROBOT_COMMAND);
                                    cmd.Cmd01_ArmSelect = int.Parse(curCmd.ARM_SELECT);
                                    cmd.Cmd01_TargetPosition = int.Parse(curCmd.TARGETPOSITION);
                                    cmd.Cmd01_TargetSlotNo = int.Parse(curCmd.TARGETSLOTNO);

                                    break;

                                case "2":

                                    cmd.Cmd02_Command = int.Parse(curCmd.ROBOT_COMMAND);
                                    cmd.Cmd02_ArmSelect = int.Parse(curCmd.ARM_SELECT);
                                    cmd.Cmd02_TargetPosition = int.Parse(curCmd.TARGETPOSITION);
                                    cmd.Cmd02_TargetSlotNo = int.Parse(curCmd.TARGETSLOTNO);

                                    break;

                                default:
                                    break;
                            }

                        }

                        #endregion

                    }

                    Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[ROBOT={0}] [BCS <- OPI][{1}] Invoke RobotService RobotSemiCommandRequest.",
                    command.BODY.ROBOTNAME, command.HEADER.TRANSACTIONID));
                    //20160121 add for Cell Special 1Arm2Job Function
                    Invoke(eServiceName.RobotCoreService, "RobotSemiCommandRequest", new object[] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.ROBOTNAME, cmd, cellSpecialCmd });
                }

                xMessage msg = SendReplyToOPI(command, reply);

                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
            }
            catch (Exception ex)
            {
                //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            }
        }

        /// <summary> OPI MessageSet: RobotRouteStepInfoRequest
        ///
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_RobotRouteStepInfoRequest(XmlDocument xmlDoc)
        {
            IServerAgent agent = GetServerAgent();
            RobotRouteStepInfoRequest command = Spec.XMLtoMessage(xmlDoc) as RobotRouteStepInfoRequest;
            RobotRouteStepInfoReply reply = new RobotRouteStepInfoReply();
            try
            {
                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}]", command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                reply.BODY.LINENAME = command.BODY.LINENAME;
                reply.BODY.CASSETTESEQNO = command.BODY.CASSETTESEQNO;
                reply.BODY.JOBSEQNO = command.BODY.JOBSEQNO;

                Job bcsJob = ObjectManager.JobManager.GetJob(command.BODY.CASSETTESEQNO, command.BODY.JOBSEQNO);

                if (bcsJob != null)
                {

                    reply.BODY.GLASSID = bcsJob.GlassChipMaskBlockID;
                    reply.BODY.REAL_STEPID = bcsJob.RobotWIP.CurStepNo.ToString();
                    reply.BODY.REAL_NEXT_STEPID = bcsJob.RobotWIP.NextStepNo.ToString();
                    //20151111 modify 將RouteID提到上層給OPI特別處理
                    reply.BODY.ROUTE_ID = bcsJob.RobotWIP.CurRouteID.ToString();

                    reply.BODY.ITEMLIST.Add(new RobotRouteStepInfoReply.ITEMc() { VNAME = "CurJobStatus", VVALUE = bcsJob.RobotWIP.RouteProcessStatus });
                    //reply.BODY.ITEMLIST.Add(new RobotRouteStepInfoReply.ITEMc() { VNAME = "CurStepID", VVALUE = bcsJob.RobotWIP.CurStepNo.ToString() });                 
                    reply.BODY.ITEMLIST.Add(new RobotRouteStepInfoReply.ITEMc() { VNAME = "CurLocation_StageType", VVALUE = bcsJob.RobotWIP.CurLocation_StageType });
                    reply.BODY.ITEMLIST.Add(new RobotRouteStepInfoReply.ITEMc() { VNAME = "CurLocation_StageID", VVALUE = bcsJob.RobotWIP.CurLocation_StageID });
                    reply.BODY.ITEMLIST.Add(new RobotRouteStepInfoReply.ITEMc() { VNAME = "CurLocation_SlotNO", VVALUE = bcsJob.RobotWIP.CurLocation_SlotNo.ToString() });                   

                    #region [ 將查詢StepID在DB中的設定參數傳給OPI顯示 ]

                    if (bcsJob.RobotWIP.RobotRouteStepList.ContainsKey(bcsJob.RobotWIP.CurStepNo))
                    {

                        RobotRouteStep routeStep = bcsJob.RobotWIP.RobotRouteStepList[bcsJob.RobotWIP.CurStepNo];

                        if (routeStep.Data != null)
                        {
                            PropertyInfo[] properties = routeStep.Data.GetType().GetProperties();
                            foreach (PropertyInfo prop in properties)
                            {
                                //20151111 modify 將RouteID提到上層給OPI特別處理
                                if (prop.Name == "Id" || prop.Name == "SERVERNAME" || prop.Name == "ROUTEID")
                                    continue;

                                RobotRouteStepInfoReply.ITEMc curItem =new RobotRouteStepInfoReply.ITEMc();
                                
                                curItem.VNAME= prop.Name;

                                if (prop.GetValue(routeStep.Data, null) == null)
                                {
                                    curItem.VVALUE = string.Empty;
                                }
                                else
                                {
                                    curItem.VVALUE = prop.GetValue(routeStep.Data, null).ToString();

                                }

                                reply.BODY.ITEMLIST.Add(curItem);
                                //reply.BODY.ITEMLIST.Add(new RobotRouteStepInfoReply.ITEMc() { VNAME = prop.Name, VVALUE = prop.GetValue(routeStep.Data, null).ToString() });

                            }
                        }
                    }

                    #endregion

                }

                xMessage msg = SendReplyToOPI(command, reply);

                Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] {2} OK", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.GetType().Name));
            }
            catch (Exception ex)
            {
                reply.RETURN.RETURNMESSAGE = ex.Message;
                xMessage msg = SendReplyToOPI(command, reply);

                //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            }
        }

        /// <summary> OPI MessageSet: RobotStopRunReasonRequest
        /// 
        /// </summary>
        /// <param name="xmlDoc"></param>
        public void OPI_RobotStopRunReasonRequest(XmlDocument xmlDoc)
        {
            IServerAgent agent = GetServerAgent();
            RobotStopRunReasonRequest command = Spec.XMLtoMessage(xmlDoc) as RobotStopRunReasonRequest;
            RobotStopRunReasonReply reply = new RobotStopRunReasonReply();

            try
            {
                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}]", command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                reply.BODY.LINENAME = command.BODY.LINENAME;
                reply.BODY.CASSETTESEQNO = command.BODY.CASSETTESEQNO;
                reply.BODY.JOBSEQNO = command.BODY.JOBSEQNO;

                #region [ Get Robot entity by ServerName and Fail Msg ]

                reply.BODY.ROBOTREASONLIST.Clear();

                Robot curRobot = ObjectManager.RobotManager.GetRobotbySeverName(Workbench.ServerName);

                if (curRobot == null)
                {
                    //找不到則回覆錯誤訊息
                    RobotStopRunReasonReply.REASONc robotStopReason = new RobotStopRunReasonReply.REASONc();

                    robotStopReason.STOP_REASON = string.Format("[LINEMAME={0}] [RCS -> OPI] can not Get Robot Entity by ServerName({1})!",
                                                                command.BODY.LINENAME, Workbench.ServerName);
                    reply.BODY.ROBOTNAME="Unknown";
                    reply.BODY.ROBOTREASONLIST.Add(robotStopReason);

                }
                else
                {
                    reply.BODY.ROBOTNAME = curRobot.Data.ROBOTNAME;

                    if (curRobot.CheckFailMessageList.Count > 0)
                    {
                        foreach (string robotErrorkey in curRobot.CheckFailMessageList.Keys)
                        {
                            RobotStopRunReasonReply.REASONc robotStopReason = new RobotStopRunReasonReply.REASONc();

                            robotStopReason.STOP_REASON = curRobot.CheckFailMessageList[robotErrorkey];
                            reply.BODY.ROBOTREASONLIST.Add(robotStopReason);
                        }
                    }
                }

                #endregion

                #region [ Get Job entity by ServerName and Fail Msg ]

                reply.BODY.JOBREASONLIST.Clear();

                Job bcsJob = ObjectManager.JobManager.GetJob(command.BODY.CASSETTESEQNO, command.BODY.JOBSEQNO);

                if (bcsJob == null)
                {
                    reply.BODY.GLASSID = "Unknown";

                    //找不到則回覆錯誤訊息
                    RobotStopRunReasonReply.REASONc jobStopReason = new RobotStopRunReasonReply.REASONc();

                    jobStopReason.STOP_REASON = string.Format("[LINEMAME={0}] [RCS -> OPI] can not Get Job Entity by CstSeq({1}) JobSeq({2})!",
                                                                command.BODY.LINENAME, command.BODY.CASSETTESEQNO, command.BODY.JOBSEQNO);

                    reply.BODY.JOBREASONLIST.Add(jobStopReason);

                }
                else
                {
                    //20151204 add RealTime CurStepID and NextStepID
                    reply.BODY.REAL_STEPID = bcsJob.RobotWIP.CurStepNo.ToString();
                    reply.BODY.REAL_NEXT_STEPID = bcsJob.RobotWIP.NextStepNo.ToString();

                    reply.BODY.GLASSID = bcsJob.GlassChipMaskBlockID;
                    
                    if (bcsJob.RobotWIP.CheckFailMessageList.Count > 0)
                    {
                        foreach (string jobErrorkey in bcsJob.RobotWIP.CheckFailMessageList.Keys)
                        {
                            RobotStopRunReasonReply.REASONc jobStopReason = new RobotStopRunReasonReply.REASONc();

                            jobStopReason.STOP_REASON = bcsJob.RobotWIP.CheckFailMessageList[jobErrorkey];
                            reply.BODY.JOBREASONLIST.Add(jobStopReason);
                        }
                    }
                }

                #endregion

                xMessage msg = SendReplyToOPI(command, reply);

                Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] {2} OK", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.GetType().Name));
            }
            catch (Exception ex)
            {
                reply.RETURN.RETURNMESSAGE = ex.Message;
                xMessage msg = SendReplyToOPI(command, reply);

                //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            }
        }
        
        //20151203 add for MQC Inspeciton Priority
        public void OPI_InspectionFlowPriorityInfoReply(XmlDocument xmlDoc)
        {
            IServerAgent agent = GetServerAgent();
            InspectionFlowPriorityInfoRequest command = Spec.XMLtoMessage(xmlDoc) as InspectionFlowPriorityInfoRequest;
            InspectionFlowPriorityInfoReply reply = new InspectionFlowPriorityInfoReply();
            xMessage msg;

            try
            {
                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}]", command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

                reply.BODY.LINENAME = command.BODY.LINENAME;
                reply.BODY.PORTNO = command.BODY.PORTNO;
                reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                //reply.BODY.PRIORITYLIST.Clear();

                #region [ Get Robot Default Inspection Priority RouteSetting ]

                #region [ Get Line Robot Entity ]

                Robot curRobot = ObjectManager.RobotManager.GetRobotbySeverName(Workbench.ServerName);

                if (curRobot == null)
                {
                    //找不到則回覆錯誤訊息
                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[ROBOT={0}] [BCS <- OPI][{1}] Can't find RobotEntity by WorkBench ServerName[{2}]!", 
                                  "Unknown", command.HEADER.TRANSACTIONID, Workbench.ServerName));

                    reply.RETURN.RETURNCODE = "0010361";
                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find RobotEntity by WorkBench ServerName[{0}]!", Workbench.ServerName);
                    msg = SendReplyToOPI(command, reply);

                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS -> OPI][{1}] {2} OK", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.GetType().Name));

                    return;
                }

                #endregion

                if (curRobot.File.CurMQCPortDefaultInspPriority.ContainsKey(command.BODY.PORTNO) == true)
                {
                        #region [ 以目前的Prioity做上報 ]
                       reply.BODY.PRIORITY = curRobot.File.CurMQCPortDefaultInspPriority[command.BODY.PORTNO];
                        #endregion
                }
                else
                {
                    #region [ 目前沒有設定Priority 則以目前的Node List做預設 ]
                    List<Equipment> curAllNodeList = ObjectManager.EquipmentManager.GetEQPsByLine(Workbench.ServerName);
                    if (curAllNodeList.Count == 0)
                    {
                        #region 找不到機台資訊，只能回覆並記錄錯誤訊息
                        //找不到則回覆錯誤訊息
                        Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                        string.Format("[ROBOT={0}] [BCS <- OPI][{1}] Can't find NodeEntity by WorkBench ServerName[{2}]!",
                                        "Unknown", command.HEADER.TRANSACTIONID, Workbench.ServerName));
                        reply.RETURN.RETURNCODE = "0010361";
                        reply.RETURN.RETURNMESSAGE = string.Format("Can't find NodeEntity by WorkBench ServerName[{0}]!", Workbench.ServerName);
                        msg = SendReplyToOPI(command, reply);
                        Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] {2} OK", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.GetType().Name));
                        return;
                        #endregion 
                    }
                    else
                    {
                        #region [ 設定All Inspeciton default Priority ]
                        string tmpPriority = string.Empty;
                        foreach (Equipment curEQP in curAllNodeList)
                        {
                            if (curEQP.Data.NODEATTRIBUTE == "IN")
                            {
                                //只需要設定所有檢測機的Priority
                                tmpPriority += curEQP.Data.NODENO.Replace('L','0');
                            }
                        }

                        Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] Robot({2}) add default Port({3}) Inspection Priority({4})",
                                        reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, curRobot.Data.ROBOTNAME, command.BODY.PORTNO,tmpPriority));
                        //更新資料
                        reply.BODY.PRIORITY = tmpPriority;
                        lock (curRobot.File)
                        {
                            curRobot.File.CurMQCPortDefaultInspPriority.Add(command.BODY.PORTNO, tmpPriority);
                        }
                        //存入Robot File
                        ObjectManager.RobotManager.EnqueueSave(curRobot.File);
                        #endregion
                    }
                    #endregion
                }
                #endregion

                msg = SendReplyToOPI(command, reply);

                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] {2} OK", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.GetType().Name));
            }
            catch (Exception ex)
            {
                reply.RETURN.RETURNMESSAGE = ex.Message;
                msg = SendReplyToOPI(command, reply);

                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            }
 
        }

        public void OPI_InspectionFlowPriorityChangeReply(XmlDocument xmlDoc)
        {

            IServerAgent agent = GetServerAgent();
            InspectionFlowPriorityChangeRequest command = Spec.XMLtoMessage(xmlDoc) as InspectionFlowPriorityChangeRequest;
            InspectionFlowPriorityChangeReply reply = new InspectionFlowPriorityChangeReply();
            xMessage msg;

            try
            {
                reply.BODY.LINENAME = Workbench.ServerName;
                reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
                reply.BODY.PORTNO = command.BODY.PORTNO;

                #region [ Get Line Robot Entity ]

                Robot curRobot = ObjectManager.RobotManager.GetRobotbySeverName(Workbench.ServerName);

                if (curRobot == null)
                {
                    //找不到則回覆錯誤訊息
                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[ROBOT={0}] [BCS <- OPI][{1}] Can't find RobotEntity by WorkBench ServerName[{2}]!",
                                  "Unknown", command.HEADER.TRANSACTIONID, Workbench.ServerName));

                    reply.RETURN.RETURNCODE = "0010361";
                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find RobotEntity by WorkBench ServerName[{0}]!", Workbench.ServerName);
                    msg = SendReplyToOPI(command, reply);

                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS -> OPI][{1}] {2} OK", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.GetType().Name));

                    return;
                }

                #endregion

                #region [ 紀錄目前Robot Keep Inspection Flow Priority ]

                #region 找不到Priority的錯誤訊息
                if (curRobot.File.CurMQCPortDefaultInspPriority.ContainsKey(command.BODY.PORTNO) == false)
                {
                    //OPI訊息異常
                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[ROBOT={0}] [BCS <- OPI][{1}] OPI Send Inspection Flow PortNo({2}) is illigal!",
                                  curRobot.Data.ROBOTNAME, command.HEADER.TRANSACTIONID, reply.BODY.PORTNO));
                    reply.RETURN.RETURNCODE = "RB00002";
                    reply.RETURN.RETURNMESSAGE = string.Format("OPI Send Inspection Flow PortNo({0}) is illigal!",reply.BODY.PORTNO);
                    msg = SendReplyToOPI(command, reply);
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS -> OPI][{1}] {2} OK", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.GetType().Name));

                    return;
                }
                #endregion

                if (curRobot.File.CurMQCPortDefaultInspPriority.Count > 0)
                {
                        Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                     string.Format("[LINENAME={0}] [BCS -> OPI][{1}] Robot({2}) Port({3}) current Inspection Priority({4})",
                                                   reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, curRobot.Data.ROBOTNAME, command.BODY.PORTNO, command.BODY.PRIORITY));
                }

                #endregion

                #region [ 根據OPI設定Update Inspeciton Flow Priority ]

                //更新資料
                lock (curRobot.File)
                {
                        if (curRobot.File.CurMQCPortDefaultInspPriority.ContainsKey(command.BODY.PORTNO.Trim()))
                            curRobot.File.CurMQCPortDefaultInspPriority.Remove(command.BODY.PORTNO.Trim());

                            curRobot.File.CurMQCPortDefaultInspPriority.Add(command.BODY.PORTNO, command.BODY.PRIORITY);

                            Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[LINENAME={0}] [BCS -> OPI][{1}] Robot({2}) Port({3}) Update Inspection Priority({4})",
                                            reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, curRobot.Data.ROBOTNAME, command.BODY.PORTNO, command.BODY.PRIORITY));
                }

                //存入Robot File
                ObjectManager.RobotManager.EnqueueSave(curRobot.File);

                CPC_InspectionFlowPriorityInfoReport(curRobot, command.BODY.PORTNO, command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID);
                
                #endregion

                msg = SendReplyToOPI(command, reply);

                Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] {2} OK", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.GetType().Name));

            }
            catch (Exception ex)
            {
                reply.RETURN.RETURNMESSAGE = ex.Message;
                msg = SendReplyToOPI(command, reply);

                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
            }

        }

        public void CPC_InspectionFlowPriorityInfoReport(Robot curRobot,string curPortNo, string curEepNo, string trxID)
        {
            try
            {
                IServerAgent agent = GetServerAgent();
                XmlDocument xml_doc = agent.GetTransactionFormat("InspectionFlowPriorityInfoReport") as XmlDocument;
                InspectionFlowPriorityInfoReport trx = Spec.XMLtoMessage(xml_doc) as InspectionFlowPriorityInfoReport;
                
                trx.BODY.LINENAME = Workbench.ServerName;
                trx.BODY.PORTNO = curPortNo;
                trx.BODY.EQUIPMENTNO = curEepNo;
                if (curRobot.File.CurMQCPortDefaultInspPriority.ContainsKey(curPortNo))
                    trx.BODY.PRIORITY = curRobot.File.CurMQCPortDefaultInspPriority[curPortNo];
                else
                {

                    Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[LINENAME={0}] [BCS -> OPI] Robot({2}) Port({3}) Inspection Priority No Data",
                            Workbench.ServerName,  curRobot.Data.ROBOTNAME, curPortNo));
           
                }

                xMessage msg = SendReportToAllOPI(trxID, trx);

            }
            catch (Exception ex)
            {
                
                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
            }

        }

        //[ Wait_Proc_00032 ] OPI與RB之間的部分Function尚未修正完畢  Robot Cmd , StageInfo , ULD Dispatch Rule, StepInfo

        #region [ T2 function List ]

//        #region Status
//        /// <summary>
//        /// OPI MessageSet: Robot Command Request
//        /// </summary>
//        /// <param name="xmlDoc"></param>
//        //public void OPI_RobotCommandRequest(XmlDocument xmlDoc)
//        //{
//        //    IServerAgent agent = GetServerAgent();
//        //    RobotCommandRequest command = Spec.XMLtoMessage(xmlDoc) as RobotCommandRequest;
//        //    XmlDocument xml_doc = agent.GetTransactionFormat("RobotCommandReply") as XmlDocument;
//        //    RobotCommandReply reply = Spec.XMLtoMessage(xml_doc) as RobotCommandReply;

//        //    try
//        //    {
//        //        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//        //            string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
//        //            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

//        //        reply.BODY.LINENAME = command.BODY.LINENAME;

//        //        foreach (Robot robot in ObjectManager.RobotManager.GetRobots())
//        //        {
//        //            RobotCommandReply.COMMANDc cmd = new RobotCommandReply.COMMANDc();
//        //            cmd.EQUIPMENTNO = robot.Data.NODENO;
//        //            cmd.ROBOTNAME = robot.Data.ROBOTNAME;
//        //            cmd.ROBOTCONTROLMODE = robot.File.curRobotCmd.CmdCreateMode;
//        //            cmd.CMDSTATUS = robot.File.curRobotCmd.rbCmd_Status;
//        //            cmd.CREATETIME = robot.File.curRobotCmd.CmdCreateDateTime.ToString("yyyy-MM-dd HH:mm:ss.FFFF");
//        //            cmd.STATUSCHANGETIME = robot.File.curRobotCmd.CmdStatusChangeDateTime.ToString("yyyy-MM-dd HH:mm:ss.FFFF");
//        //            cmd.EQPCMDREPLY = robot.File.curRobotCmd.CmdEQReply;
//        //            cmd.COMMAND1 = robot.File.curRobotCmd.rbCmd_1;
//        //            cmd.ARM1 = robot.File.curRobotCmd.rbArmSelect_1;
//        //            cmd.TARGETPOSITION1 = robot.File.curRobotCmd.rbTargetPos_1;
//        //            string targetSlotNo1 = robot.File.curRobotCmd.rbTargetSlotNo_1.PadLeft(4, '0');
//        //            cmd.TARGETSLOTNO1_1 = targetSlotNo1.Substring(0, 2);
//        //            cmd.TARGETSLOTNO1_2 = targetSlotNo1.Substring(2, 2);
//        //            cmd.CMDRESULT1 = robot.File.curRobotCmd.rbCmdResult_1;
//        //            cmd.COMMAND2 = robot.File.curRobotCmd.rbCmd_2;
//        //            cmd.ARM2 = robot.File.curRobotCmd.rbArmSelect_2;
//        //            cmd.TARGETPOSITION2 = robot.File.curRobotCmd.rbTargetPos_2;
//        //            string targetSlotNo2 = robot.File.curRobotCmd.rbTargetSlotNo_2.PadLeft(4, '0');
//        //            cmd.TARGETSLOTNO2_1 = targetSlotNo2.Substring(0, 2);
//        //            cmd.TARGETSLOTNO2_2 = targetSlotNo2.Substring(2, 2);
//        //            cmd.CMDRESULT2 = robot.File.curRobotCmd.rbCmdResult_2;
//        //            reply.BODY.COMMANDLIST.Add(cmd);
//        //        }

//        //        if (reply.BODY.COMMANDLIST.Count > 0)
//        //            reply.BODY.COMMANDLIST.RemoveAt(0);

//        //        xMessage msg = SendReplyToOPI(command, reply);

//        //        Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//        //            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
//        //            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
//        //    }
//        //    catch (Exception ex)
//        //    {
//        //        //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
//        //        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//        //            string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
//        //            command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.Message));
//        //    }
//        //}

//        /// <summary>
//        /// OPI Message: Robot Command Report
//        /// </summary>
//        //public void RobotCommandReport(Robot rbt)
//        public void RobotCommandReport(Robot rbt, string cmdInfo)
//        {
//            try
//            {
//                IServerAgent agent = GetServerAgent();
//                XmlDocument xml_doc = agent.GetTransactionFormat("RobotCommandReport") as XmlDocument;
//                RobotCommandReport trx = Spec.XMLtoMessage(xml_doc) as RobotCommandReport;
//                //trx.BODY.LINENAME = rbt.Data.LINEID;
//                trx.BODY.LINENAME = rbt.Data.SERVERNAME;
//                trx.BODY.EQUIPMENTNO = rbt.Data.NODENO;
//                trx.BODY.ROBOTNAME = rbt.Data.ROBOTNAME;
//                trx.BODY.CMDDATETIME = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.ffff");
//                trx.BODY.CMDDETAIL = cmdInfo;

//                //cmd.EQUIPMENTNO = rbt.Data.NODENO;
//                //string.cmd.ROBOTNAME = rbt.Data.ROBOTNAME;
//                //cmd.ROBOTCONTROLMODE = rbt.File.curRobotCmd.CmdCreateMode;
//                //cmd.CMDSTATUS = rbt.File.curRobotCmd.rbCmd_Status;
//                //cmd.CREATETIME = rbt.File.curRobotCmd.CmdCreateDateTime.ToString("yyyy-MM-dd HH:mm:ss.FFFF");
//                //cmd.STATUSCHANGETIME = rbt.File.curRobotCmd.CmdStatusChangeDateTime.ToString("yyyy-MM-dd HH:mm:ss.FFFF");
//                //cmd.EQPCMDREPLY = rbt.File.curRobotCmd.CmdEQReply;
//                //cmd.COMMAND1 = rbt.File.curRobotCmd.rbCmd_1;
//                //cmd.ARM1 = rbt.File.curRobotCmd.rbArmSelect_1;
//                //cmd.TARGETPOSITION1 = rbt.File.curRobotCmd.rbTargetPos_1;
//                //string targetSlotNo1 = rbt.File.curRobotCmd.rbTargetSlotNo_1.PadLeft(4, '0');
//                //cmd.TARGETSLOTNO1_1 = targetSlotNo1.Substring(0, 2);
//                //cmd.TARGETSLOTNO1_2 = targetSlotNo1.Substring(2, 2);
//                //cmd.CMDRESULT1 = rbt.File.curRobotCmd.rbCmdResult_1;
//                //cmd.COMMAND2 = rbt.File.curRobotCmd.rbCmd_2;
//                //cmd.ARM2 = rbt.File.curRobotCmd.rbArmSelect_2;
//                //cmd.TARGETPOSITION2 = rbt.File.curRobotCmd.rbTargetPos_2;
//                //string targetSlotNo2 = rbt.File.curRobotCmd.rbTargetSlotNo_2.PadLeft(4, '0');
//                //cmd.TARGETSLOTNO2_1 = targetSlotNo2.Substring(0, 2);
//                //cmd.TARGETSLOTNO2_2 = targetSlotNo2.Substring(2, 2);
//                //cmd.CMDRESULT2 = rbt.File.curRobotCmd.rbCmdResult_2;

//                xMessage msg = SendReportToAllOPI(string.Empty, trx);

//                if (dicClient.Count == 0)
//                {
//                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[ROBOT={0}] [BCS -> OPI][{1}] OPI client number is zero, UIService will not report message to OPI.",
//                    rbt.Data.ROBOTNAME, msg.TransactionID));
//                }
//                else
//                {
//                    Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[ROBOT={0}] [BCS -> OPI][{1}] UIService report equipment({2}) message({3}) to OPI.",
//                    rbt.Data.ROBOTNAME, msg.TransactionID, rbt.Data.NODENO, trx.HEADER.MESSAGENAME));
//                }
//            }
//            catch (Exception ex)
//            {
//                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
//            }
//        }

//        /// <summary>
//        /// OPI Message: Robot Command Report Reply
//        /// </summary>
//        /// <param name="xmlDoc"></param>
//        public void OPI_RobotCommandReportReply(XmlDocument xmlDoc)
//        {
//            try
//            {
//                //IServerAgent agent = GetServerAgent();
//                //{
//                //    Message rbtCommandReportReply = Spec.CheckXMLFormat(xmlDoc);
//                //}
//            }
//            catch (Exception ex)
//            {
//                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
//            }
//        }

//        /// <summary>
//        /// OPI MessageSet: Robot Current Mode Reqest
//        /// </summary>
//        /// <param name="xmlDoc"></param>
//        public void OPI_RobotCurrentModeRequest(XmlDocument xmlDoc)
//        {
//            IServerAgent agent = GetServerAgent();
//            RobotCurrentModeRequest command = Spec.XMLtoMessage(xmlDoc) as RobotCurrentModeRequest;
//            XmlDocument xml_doc = agent.GetTransactionFormat("RobotCurrentModeReply") as XmlDocument;
//            RobotCurrentModeReply reply = Spec.XMLtoMessage(xml_doc) as RobotCurrentModeReply;

//            try
//            {
//                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
//                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

//                reply.BODY.LINENAME = command.BODY.LINENAME;


//                //先清除預設的空白List
//                reply.BODY.EQUIPMENTLIST.Clear();

//                foreach (Equipment eqp in ObjectManager.EquipmentManager.GetEQPs())
//                {
//                    RobotCurrentModeReply.EQUIPMENTc _eqp = new RobotCurrentModeReply.EQUIPMENTc();
//                    _eqp.EQUIPMENTNO = eqp.Data.NODENO;

//                    foreach (Robot rbt in ObjectManager.RobotManager.GetRobots())
//                    {
//                        //詢問國基：目前應該是同一條Line只有一個機台有一個Robot
//                        reply.BODY.FORCEVCRREADINGMODE = rbt.File.ForceVCRReadingMode.ToString();
//                        reply.BODY.VIRUALPORTOPERATIONMODE = rbt.File.VirtualPortMode.ToString();

//                        RobotCurrentModeReply.ROBOTc _rbt = new RobotCurrentModeReply.ROBOTc();
//                        _rbt.ROBOTNAME = rbt.Data.ROBOTNAME;
//                        _rbt.ROBOTMODE = rbt.File.curRobotRunMode;
//                        _rbt.ROBOTSTATUS = ((int)rbt.File.Status).ToString() == "" ? "0" : ((int)rbt.File.Status).ToString();
//                        _rbt.UPPERARMSTATUS = ((int)rbt.File.curRBUpArmExist).ToString() == "" ? "0" : ((int)rbt.File.curRBUpArmExist).ToString();
//                        _rbt.UPPERARMCSTSEQ = rbt.File.curRBUpArmCSTSeq;
//                        _rbt.UPPERARMJOBSEQ = rbt.File.curRBUpArmJobSeq;
//                        _rbt.LOWERARMSTATUS = ((int)rbt.File.curRBLowArmExist).ToString() == "" ? "0" : ((int)rbt.File.curRBLowArmExist).ToString();
//                        _rbt.LOWERARMCSTSEQ = rbt.File.curRBLowArmCSTSeq;
//                        _rbt.LOWERARMJOBSEQ = rbt.File.curRBLowArmJobSeq;

//                        _eqp.ROBOTLIST.Add(_rbt);
//                    }
//                    reply.BODY.EQUIPMENTLIST.Add(_eqp);
//                }

//                xMessage msg = SendReplyToOPI(command, reply);

//                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
//                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
//            }
//            catch (Exception ex)
//            {
//                //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
//                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
//                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
//            }
//        }

//        /// <summary>
//        /// OPI MessageSet: Robot Stage Info Request
//        /// </summary>
//        /// <param name="xmlDoc"></param>
//        public void OPI_RobotStageInfoRequest(XmlDocument xmlDoc)
//        {
//            IServerAgent agent = GetServerAgent();
//            RobotStageInfoRequest command = Spec.XMLtoMessage(xmlDoc) as RobotStageInfoRequest;
//            XmlDocument xml_doc = agent.GetTransactionFormat("RobotStageInfoRequestReply") as XmlDocument;
//            RobotStageInfoRequestReply reply = Spec.XMLtoMessage(xml_doc) as RobotStageInfoRequestReply;

//            try
//            {
//                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
//                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

//                reply.BODY.LINENAME = command.BODY.LINENAME;

//                List<RobotStage> stages = ObjectManager.RobotStageManager.GetRobotStages();

//                foreach (RobotStage stage in stages)
//                {
//                    //排除不需要的PORT
//                    if (stage.Data.STAGETYPE != "PORT")
//                    {
//                        RobotStageInfoRequestReply.STAGEc _stage = new RobotStageInfoRequestReply.STAGEc();
//                        _stage.STAGENO = stage.Data.STAGENO;
//                        //_stage.STAGESTATUS = stage.File.curStageStatus;

//                        //STAGESTATUS: 0:Unknown,1:NoExist, 2:LDRQ, 3:UDRQ
//                        switch (stage.File.curStageStatus)
//                        {
//                            case "LDRQ":
//                                _stage.STAGESTATUS = "2";
//                                break;
//                            case "UDRQ":
//                                _stage.STAGESTATUS = "3";
//                                break;
//                            case "NOREQ":
//                                _stage.STAGESTATUS = "1";
//                                break;
//                            default:
//                                _stage.STAGESTATUS = "0";
//                                break;
//                        }

//                        if (stage.File.curStageStatus == "UDRQ" && !string.IsNullOrEmpty(stage.File.UDRQJobKey))
//                        {
//                            //UDRQJobKey = "CSTSeq_JobSeq"
//                            string[] seqs = stage.File.UDRQJobKey.Split(new char[] { '_' }, StringSplitOptions.None);
//                            _stage.STAGESCSTSEQ = seqs[0];
//                            _stage.STAGESJOBSEQ = seqs[1];
//                        }
//                        else
//                        {
//                            _stage.STAGESCSTSEQ = string.Empty;
//                            _stage.STAGESJOBSEQ = string.Empty;
//                        }

//                        reply.BODY.STAGELIST.Add(_stage);
//                    }
//                }

//                if (reply.BODY.STAGELIST.Count > 0)
//                    reply.BODY.STAGELIST.RemoveAt(0);

//                xMessage msg = SendReplyToOPI(command, reply);

//                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
//                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
//            }
//            catch (Exception ex)
//            {
//                //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
//                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
//                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
//            }
//        }

//        /// <summary>
//        /// OPI MessageSet: Robot Stage Info Report Reply
//        /// </summary>
//        /// <param name="xmlDoc"></param>
//        public void OPI_RobotStageInfoReportReply(XmlDocument xmlDoc)
//        {
//            try
//            {
//                //IServerAgent agent = GetServerAgent();
//                //{
//                //    Message RobotStageInfoReportReply = Spec.CheckXMLFormat(xmlDoc);
//                //}
//            }
//            catch (Exception ex)
//            {
//                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
//            }
//        }

//        /// <summary>
//        /// OPI MessageSet: Port CST Status Report Reply to BC
//        /// </summary>
//        /// <param name="xmlDoc"></param>
//        public void OPI_RobotCurrentModeReportReply(XmlDocument xmlDoc)
//        {
//            try
//            {
//                //IServerAgent agent = GetServerAgent();
//                //{
//                //    Message RobotCurrentModeReportReply = Spec.CheckXMLFormat(xmlDoc);
//                //}
//            }
//            catch (Exception ex)
//            {
//                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
//            }
//        }








//        /// <summary>
//        /// OPI MessageSet: RobotRouteStepInfoRequest
//        /// </summary>
//        /// <param name="xmlDoc"></param>
//        public void OPI_RobotRouteStepInfoRequest(XmlDocument xmlDoc)
//        {
//            IServerAgent agent = GetServerAgent();
//            RobotRouteStepInfoRequest command = Spec.XMLtoMessage(xmlDoc) as RobotRouteStepInfoRequest;
//            RobotRouteStepInfoReply reply = new RobotRouteStepInfoReply();
//            try
//            {
//                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}]", command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

//                reply.BODY.LINENAME = command.BODY.LINENAME;
//                reply.BODY.CASSETTESEQNO = command.BODY.CASSETTESEQNO;
//                reply.BODY.JOBSEQNO = command.BODY.JOBSEQNO;

//                RobotJob robot_job = ObjectManager.RobotJobManager.GetRobotJob(command.BODY.CASSETTESEQNO, command.BODY.JOBSEQNO);
//                Job job = ObjectManager.JobManager.GetJob(command.BODY.CASSETTESEQNO, command.BODY.JOBSEQNO);
//                if (job != null)
//                {
//                    reply.BODY.GLASSID = job.GlassChipMaskCutID;
//                    if (job.CellSpecial != null)
//                    {
//                        reply.BODY.ITEMLIST.Add(new RobotRouteStepInfoReply.ITEMc() { VNAME = "RunMode", VVALUE = job.CellSpecial.RunMode });
//                    }
//                }
//                if (robot_job != null)
//                {
//                    reply.BODY.ITEMLIST.Add(new RobotRouteStepInfoReply.ITEMc() { VNAME = "CurJobStatus", VVALUE = robot_job.CurJobStatus });
//                    reply.BODY.ITEMLIST.Add(new RobotRouteStepInfoReply.ITEMc() { VNAME = "CurStepNo", VVALUE = robot_job.CurStepNo.ToString() });
//                    if (robot_job.RobotRoutes.ContainsKey(robot_job.CurStepNo))
//                    {
//                        RobotRoute route = robot_job.RobotRoutes[robot_job.CurStepNo];
//                        if (route.Data != null)
//                        {
//                            PropertyInfo[] properties = route.Data.GetType().GetProperties();
//                            foreach (PropertyInfo prop in properties)
//                            {
//                                if (prop.Name == "Id" || prop.Name == "SERVERNAME")
//                                    continue;

//                                reply.BODY.ITEMLIST.Add(new RobotRouteStepInfoReply.ITEMc() { VNAME = prop.Name, VVALUE = prop.GetValue(route.Data, null).ToString() });
//                            }
//                        }
//                    }
//                }
                
//                xMessage msg = SendReplyToOPI(command, reply);

//                Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
//                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] {2} OK", reply.BODY.LINENAME, reply.HEADER.TRANSACTIONID, reply.GetType().Name));
//            }
//            catch (Exception ex)
//            {
//                reply.RETURN.RETURNMESSAGE = ex.Message;
//                xMessage msg = SendReplyToOPI(command, reply);

//                //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
//                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
//                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
//            }
//        }

//        /// <summary>
//        /// OPI MessageSet: Robot Unloader Dispatch Rule Request
//        /// </summary>
//        /// <param name="xmlDoc"></param>
//        public void OPI_RobotUnloaderDispatchRuleRequest(XmlDocument xmlDoc)
//        {
//            IServerAgent agent = GetServerAgent();
//            RobotUnloaderDispatchRuleRequest command = Spec.XMLtoMessage(xmlDoc) as RobotUnloaderDispatchRuleRequest;
//            XmlDocument xml_doc = agent.GetTransactionFormat("RobotUnloaderDispatchRuleReply") as XmlDocument;
//            RobotUnloaderDispatchRuleReply reply = Spec.XMLtoMessage(xml_doc) as RobotUnloaderDispatchRuleReply;

//            try
//            {
//                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. LINENAME={3}",
//                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.LINENAME));

//                reply.BODY.LINENAME = command.BODY.LINENAME;

//                IList<Line> lines = ObjectManager.LineManager.GetLines();

//                if (lines == null)
//                {
//                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                        string.Format("[LINENAME={0}] [BCS <- OPI][{1}] Can't find Line({0}) in LineEntity.",
//                        command.BODY.LINENAME, command.HEADER.TRANSACTIONID));

//                    reply.RETURN.RETURNCODE = "0010920";
//                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Line[{0}] in LineEntity", command.BODY.LINENAME);
//                }
//                else
//                {
//                    reply.BODY.PORTLIST.Clear();
//                    foreach (KeyValuePair<string, clsDispatchRule> rule in lines[0].File.UnlaoderDispatchRule)
//                    {
//                        Port port = ObjectManager.PortManager.GetPort(rule.Key);
//                        reply.BODY.PORTLIST.Add(new RobotUnloaderDispatchRuleReply.PORTc()
//                        {
//                            EQUIPMENTNO = port == null ? "" : port.Data.NODENO,
//                            PORTNO = port == null ? "" : port.Data.PORTNO,
//                            PORTID = port == null ? "" : port.Data.PORTID,
//                            GRADE_1 = rule.Value.Grade1,
//                            GRADE_2 = rule.Value.Grade2,
//                            GRADE_3 = rule.Value.Grade3,
//                            GRADE_4 = rule.Value.Grade4,
//                            ABNORMALCODE_1 = rule.Value.AbnormalCode1,
//                            ABNORMALCODE_2 = rule.Value.AbnormalCode2,
//                            ABNORMALCODE_3 = rule.Value.AbnormalCode3,
//                            ABNORMALCODE_4 = rule.Value.AbnormalCode4,
//                            ABNORMALFLAG = rule.Value.AbnormalFlag,
//                            OPERATORID = rule.Value.OperatorID
//                        });
//                    }
//                }

//                xMessage msg = SendReplyToOPI(command, reply);

//                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
//                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
//            }
//            catch (Exception ex)
//            {
//                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
//                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
//            }
//        }

//        /// <summary>
//        /// OPI MessageSet: Robot Unloader Dispatch Rule Report
//        /// </summary>
//        public void RobotUnloaderDispatchRuleReport(string trxID)
//        {
//            try
//            {
//                IServerAgent agent = GetServerAgent();
//                XmlDocument xml_doc = agent.GetTransactionFormat("RobotUnloaderDispatchRuleReport") as XmlDocument;
//                RobotUnloaderDispatchRuleReport trx = Spec.XMLtoMessage(xml_doc) as RobotUnloaderDispatchRuleReport;

//                trx.BODY.LINENAME = Workbench.ServerName;
//                IList<Line> lines = ObjectManager.LineManager.GetLines();

//                trx.BODY.PORTLIST.Clear();
//                foreach (KeyValuePair<string, clsDispatchRule> rule in lines[0].File.UnlaoderDispatchRule)
//                {
//                    Port port = ObjectManager.PortManager.GetPort(rule.Key);
//                    trx.BODY.PORTLIST.Add(new RobotUnloaderDispatchRuleReport.PORTc()
//                    {
//                        EQUIPMENTNO = port == null ? "" : port.Data.NODENO,
//                        PORTNO = port == null ? "" : port.Data.PORTNO,
//                        PORTID = port == null ? "" : port.Data.PORTID,
//                        GRADE_1 = rule.Value.Grade1,
//                        GRADE_2 = rule.Value.Grade2,
//                        GRADE_3 = rule.Value.Grade3,
//                        GRADE_4 = rule.Value.Grade4,
//                        ABNORMALCODE_1 = rule.Value.AbnormalCode1,
//                        ABNORMALCODE_2 = rule.Value.AbnormalCode2,
//                        ABNORMALCODE_3 = rule.Value.AbnormalCode3,
//                        ABNORMALCODE_4 = rule.Value.AbnormalCode4,
//                        ABNORMALFLAG = rule.Value.AbnormalFlag,
//                        OPERATORID = rule.Value.OperatorID
//                    });
//                }

//                xMessage msg = SendReportToAllOPI(trxID, trx);
//            }
//            catch (Exception ex)
//            {
//                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
//            }
//        }

//        /// <summary>
//        /// OPI MessageSet: Robot Unloader Dispatch Rule Report Reply
//        /// </summary>
//        /// <param name="xmlDoc"></param>
//        public void OPI_RobotUnloaderDispatchRuleReportReply(XmlDocument xmlDoc)
//        {
//            try
//            {
//                //Do Nothing
//            }
//            catch (Exception ex)
//            {
//                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
//            }
//        }

//        #endregion

//        #region Command
//        /// <summary>
//        /// OPI MessageSet: Robot Mode Change Request
//        /// </summary>
//        /// <param name="xmlDoc"></param>
//        public void OPI_RobotModeChangeRequest(XmlDocument xmlDoc)
//        {
//            IServerAgent agent = GetServerAgent();
//            RobotModeChangeRequest command = Spec.XMLtoMessage(xmlDoc) as RobotModeChangeRequest;
//            XmlDocument xml_doc = agent.GetTransactionFormat("RobotModeChangeReply") as XmlDocument;
//            RobotModeChangeReply reply = Spec.XMLtoMessage(xml_doc) as RobotModeChangeReply;

//            try
//            {
//                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, ROBOTNAME={4}, ROBOTMODE={5}",
//                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME,
//                    command.BODY.EQUIPMENTNO, command.BODY.ROBOTNAME, command.BODY.ROBOTMODE));

//                reply.BODY.LINENAME = command.BODY.LINENAME;
//                reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;

//                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
//                Robot robot = ObjectManager.RobotManager.GetRobot(command.BODY.EQUIPMENTNO);

//                if (eqp == null)
//                {
//                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
//                    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

//                    reply.RETURN.RETURNCODE = "0010350";
//                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);
//                }
//                else if (robot == null)
//                {
//                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[ROBOT={0}] [BCS <- OPI][{1}] Can't find Robot({0}) in RobotEntity.",
//                    command.BODY.ROBOTNAME, command.HEADER.TRANSACTIONID));

//                    reply.RETURN.RETURNCODE = "0010351";
//                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Robot[{0}] in RobotEntity", command.BODY.ROBOTNAME);
//                }
//                else
//                {
//                    reply.BODY.ROBOTNAME = command.BODY.ROBOTNAME;
//                    reply.BODY.ROBOTMODE = command.BODY.ROBOTMODE;
//                }

//                xMessage msg = SendReplyToOPI(command, reply);

//                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
//                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));

//                //先回覆OPI再呼叫Robot，避免timeout
//                if (reply.RETURN.RETURNCODE == "0000000")
//                {
//                    Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[ROBOT={0}] [BCS <- OPI][{1}] Invoke RobotService RobotModeChangeRequest.",
//                    command.BODY.ROBOTNAME, command.HEADER.TRANSACTIONID));

//                    Invoke("RobotService", "RobotModeChangeRequest", new object[] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.ROBOTNAME, command.BODY.ROBOTMODE });
//                }
//            }
//            catch (Exception ex)
//            {
//                //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
//                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
//                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
//            }
//        }

//        /// <summary>
//        /// OPI MessageSet: Robot Semi Command Request
//        /// </summary>
//        /// <param name="xmlDoc"></param>
//        public void OPI_RobotSemiCommandRequest(XmlDocument xmlDoc)
//        {
//            IServerAgent agent = GetServerAgent();
//            RobotSemiCommandRequest command = Spec.XMLtoMessage(xmlDoc) as RobotSemiCommandRequest;
//            XmlDocument xml_doc = agent.GetTransactionFormat("RobotSemiCommandReply") as XmlDocument;
//            RobotSemiCommandReply reply = Spec.XMLtoMessage(xml_doc) as RobotSemiCommandReply;

//            try
//            {
//                Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI. EQUIPMENTNO={3}, ROBOTNAME={4}",
//                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME, command.BODY.EQUIPMENTNO, command.BODY.ROBOTNAME));

//                reply.BODY.LINENAME = command.BODY.LINENAME;
//                reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
//                reply.BODY.ROBOTNAME = command.BODY.ROBOTNAME;

//                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(command.BODY.EQUIPMENTNO);
//                Robot robot = ObjectManager.RobotManager.GetRobot(command.BODY.EQUIPMENTNO);

//                if (eqp == null)
//                {
//                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[EQUIPMENT={0}] [BCS <- OPI][{1}] Can't find Equipment({0}) in EquipmentEntity.",
//                    command.BODY.EQUIPMENTNO, command.HEADER.TRANSACTIONID));

//                    reply.RETURN.RETURNCODE = "0010360";
//                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Equipment[{0}] in EquipmentEntity", command.BODY.EQUIPMENTNO);
//                }
//                else if (robot == null)
//                {
//                    Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[ROBOT={0}] [BCS <- OPI][{1}] Can't find Robot({0}) in RobotEntity.",
//                    command.BODY.ROBOTNAME, command.HEADER.TRANSACTIONID));

//                    reply.RETURN.RETURNCODE = "0010361";
//                    reply.RETURN.RETURNMESSAGE = string.Format("Can't find Robot[{0}] in RobotEntity", command.BODY.ROBOTNAME);
//                }
//                else
//                {
//                    RobotCmdInfo cmd = new RobotCmdInfo();
//                    cmd.rbCmd_1 = command.BODY.COMMAND1;
//                    cmd.rbArmSelect_1 = command.BODY.ARM1;
//                    cmd.rbTargetPos_1 = command.BODY.TARGETPOSITION1;
//                    cmd.rbTargetSlotNo_1 = command.BODY.TARGETSLOTNO1;
//                    cmd.rbCmd_2 = command.BODY.COMMAND2;
//                    cmd.rbArmSelect_2 = command.BODY.ARM2;
//                    cmd.rbTargetPos_2 = command.BODY.TARGETPOSITION2;
//                    cmd.rbTargetSlotNo_2 = command.BODY.TARGETSLOTNO2;

//                    Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[ROBOT={0}] [BCS <- OPI][{1}] Invoke RobotService RobotSemiCommandRequest.",
//                    command.BODY.ROBOTNAME, command.HEADER.TRANSACTIONID));

//                    Invoke("RobotService", "RobotSemiCommandRequest", new object[] { command.HEADER.TRANSACTIONID, command.BODY.EQUIPMENTNO, command.BODY.ROBOTNAME, cmd });
//                }

//                xMessage msg = SendReplyToOPI(command, reply);

//                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
//                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
//            }
//            catch (Exception ex)
//            {
//                //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
//                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
//                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.ToString()));
//            }
//        }

//        /// <summary>
//        /// OPI MessageSet: Robot Stage Info Report
//        /// </summary>
//        /// <param name="lineID"></param>
//        public void RobotStageInfoReport(string lineID)
//        {
//            try
//            {
//                IServerAgent agent = GetServerAgent();
//                XmlDocument xml_doc = agent.GetTransactionFormat("RobotStageInfoReport") as XmlDocument;
//                RobotStageInfoReport trx = Spec.XMLtoMessage(xml_doc) as RobotStageInfoReport;

//                //trx.BODY.LINENAME = lineID;
//                Line line = ObjectManager.LineManager.GetLine(lineID);
//                trx.BODY.LINENAME = line == null ? lineID : line.Data.SERVERNAME;

//                List<RobotStage> stages = ObjectManager.RobotStageManager.GetRobotStages();

//                foreach (RobotStage stage in stages)
//                {
//                    if (stage.Data.STAGETYPE != "PORT")
//                    {
//                        RobotStageInfoReport.STAGEc _stage = new RobotStageInfoReport.STAGEc();
//                        _stage.STAGENO = stage.Data.STAGENO;

//                        //STAGESTATUS: 0:Unknown,1:NoExist, 2:LDRQ, 3:UDRQ
//                        switch (stage.File.curStageStatus)
//                        {
//                            case "LDRQ":
//                                _stage.STAGESTATUS = "2";
//                                break;
//                            case "UDRQ":
//                                _stage.STAGESTATUS = "3";
//                                break;
//                            case "NOREQ":
//                                _stage.STAGESTATUS = "1";
//                                break;
//                            default:
//                                _stage.STAGESTATUS = "0";
//                                break;
//                        }

//                        //只有UDRQ時才會更新
//                        if (stage.File.curStageStatus == "UDRQ" && !string.IsNullOrEmpty(stage.File.UDRQJobKey))
//                        {
//                            //UDRQJobKey = "CSTSeq_JobSeq"
//                            string[] seqs = stage.File.UDRQJobKey.Split('_');
//                            _stage.STAGESCSTSEQ = seqs[0];
//                            _stage.STAGESJOBSEQ = seqs[1];
//                        }
//                        else
//                        {
//                            _stage.STAGESCSTSEQ = string.Empty;
//                            _stage.STAGESJOBSEQ = string.Empty;
//                        }
//                        trx.BODY.STAGELIST.Add(_stage);
//                    }
//                }

//                if (trx.BODY.STAGELIST.Count > 0)
//                    trx.BODY.STAGELIST.RemoveAt(0);

//                xMessage msg = SendReportToAllOPI(string.Empty, trx);

//                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                       string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService report message to OPI.",
//                       lineID, msg.TransactionID));
//            }
//            catch (Exception ex)
//            {
//                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
//            }
//        }

//        /// <summary>
//        /// OPI MessageSet: Robot Route Setting Request
//        /// </summary>
//        /// <param name="xmlDoc"></param>
//        public void OPI_RobotRouteSettingRequest(XmlDocument xmlDoc)
//        {
//            IServerAgent agent = GetServerAgent();
//            RobotRouteSettingRequest command = Spec.XMLtoMessage(xmlDoc) as RobotRouteSettingRequest;
//            XmlDocument xml_doc = agent.GetTransactionFormat("RobotRouteSettingReply") as XmlDocument;
//            RobotRouteSettingReply reply = Spec.XMLtoMessage(xml_doc) as RobotRouteSettingReply;

//            try
//            {
//                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[LINENAME={0}] [BCS <- OPI][{1}] UIService receive message({2}) from OPI.",
//                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, command.HEADER.MESSAGENAME));

//                reply.BODY.LINENAME = command.BODY.LINENAME;
//                reply.BODY.EQUIPMENTNO = command.BODY.EQUIPMENTNO;
//                reply.BODY.ROBOTNO = command.BODY.ROBOTNO;
//                reply.BODY.ROBOTRUNMODE = command.BODY.ROBOTRUNMODE;

//                xMessage msg = SendReplyToOPI(command, reply);

//                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService reply message({2}) to OPI.",
//                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME));
//            }
//            catch (Exception ex)
//            {
//                //NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
//                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                    string.Format("[LINENAME={0}] [BCS -> OPI][{1}] UIService catch message({2}) exception : {3} ",
//                    command.BODY.LINENAME, command.HEADER.TRANSACTIONID, reply.HEADER.MESSAGENAME, ex.Message));
//            }
//        }

//        /// <summary>
//        /// OPI MessageSet: Robot Current Mode Report
//        /// </summary>
//        /// <param name="trxID"></param>
//        /// <param name="lineName"></param>
//        /// <param name="robot"></param>
//        public void RobotCurrentModeReport(Robot robot)
//        {
//            try
//            {
//                IServerAgent agent = GetServerAgent();
//                XmlDocument xml_doc = agent.GetTransactionFormat("RobotCurrentModeReport") as XmlDocument;
//                RobotCurrentModeReport trx = Spec.XMLtoMessage(xml_doc) as RobotCurrentModeReport;
//                //trx.BODY.LINENAME = robot.Data.LINEID;
//                trx.BODY.FORCEVCRREADINGMODE = robot.File.ForceVCRReadingMode.ToString();
//                trx.BODY.VIRUALPORTOPERATIONMODE = robot.File.VirtualPortMode.ToString();
//                trx.BODY.LINENAME = robot.Data.SERVERNAME;
//                trx.BODY.EQUIPMENTNO = robot.Data.NODENO;
//                trx.BODY.ROBOTNAME = robot.Data.ROBOTNAME;
//                trx.BODY.ROBOTMODE = robot.File.curRobotRunMode;
//                trx.BODY.ROBOTSTATUS = ((int)robot.File.Status).ToString() == "" ? "0" : ((int)robot.File.Status).ToString();
//                trx.BODY.UPPERARMSTATUS = ((int)robot.File.curRBUpArmExist).ToString() == "" ? "0" : ((int)robot.File.curRBUpArmExist).ToString();
//                trx.BODY.UPPERARMCSTSEQ = robot.File.curRBUpArmCSTSeq;
//                trx.BODY.UPPERARMJOBSEQ = robot.File.curRBUpArmJobSeq;
//                trx.BODY.LOWERARMSTATUS = ((int)robot.File.curRBLowArmExist).ToString() == "" ? "0" : ((int)robot.File.curRBLowArmExist).ToString();
//                trx.BODY.LOWERARMCSTSEQ = robot.File.curRBLowArmCSTSeq;
//                trx.BODY.LOWERARMJOBSEQ = robot.File.curRBLowArmJobSeq;

//                xMessage msg = SendReportToAllOPI(string.Empty, trx);

//                Logger.LogDebugWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
//                       string.Format("[ROBOT={0}] [BCS -> OPI][{1}] UIService report message to OPI.",
//                       robot.Data.ROBOTNAME, msg.TransactionID));
//            }
//            catch (Exception ex)
//            {
//                NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
//            }
//        }



//        #endregion

        #endregion

    }
}