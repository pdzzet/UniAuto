using System;
using System.Reflection;
using System.Xml;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Log;
using System.Collections.Generic;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;

namespace UniAuto.UniBCS.CSOT.SECSService
{
      public partial class NikonSECSService
      {
            #region Recipte form equipment-S1
            public void S1F0_E_AbortTransaction(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F0_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string rtnid = recvTrx["secs"]["message"]["return"].Attributes["id"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtnid)
                        {
                              case "S1F15_H":
                                    {
                                          NLogManager.Logger.LogInfoWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                                              string.Format("[Equipment={0}] [{1}] Equipment denies off-line. Does not process off-line request.", eqpno, tid));
                                          Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                                          if (eqp == null)
                                          {
                                                _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                    "Can not find Equipment Number in EquipmentEntity!");
                                                return;
                                          }
                                          eqp.File.HSMSControlMode = "OFF-LINE";
                                          eqp.File.CIMMode = eBitResult.OFF;
                                          Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                                          //20161204 yang:CIM Mode要报给MES
                                          Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { tid, eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });
                                          ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                                          Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                              new object[4] { tid, eqp.Data.LINEID, "Reply S1F0", string.Format("[EQUIPMENT={0}] Offline request Fail. Equipment reply S1F0 Abort Transaction.", eqp.Data.NODENO) });
                                          //20150319 cy:online request timeout, terminal timer and do nothing.
                                          string timerId = string.Format("S1F15_OfflineRequest_{0}_OPI", eqpno);
                                          if (_timerManager.IsAliveTimer(timerId))
                                          {
                                                _timerManager.TerminateTimer(timerId); //remove old
                                          }
                                          break;
                                    }
                              case "S1F3_H":
                                    {
                                          switch (rtn)
                                          {
                                                case "ConnectedCheckContorl":
                                                      Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                                                      if (eqp == null)
                                                      {
                                                            _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                                "Can not find Equipment Number in EquipmentEntity!");
                                                            return;
                                                      }
                                                      eqp.File.HSMSControlMode = "OFF-LINE";
                                                      eqp.File.CIMMode = eBitResult.OFF;
                                                      Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                                                      //20161204 yang:CIM Mode要报给MES
                                                      Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { tid, eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });
                                                      ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                                      break;
                                          }
                                          break;
                                    }
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F1_E_AreYouThereRequest(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F1_E");
                  try
                  {
                        #region Handle Logic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        TS1F2_H_OnLineData(eqpno, agent, tid, sysbytes);

                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S1F2_E_OnLineData(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F2_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        eqp.MDLN = recvTrx["secs"]["message"]["body"]["array1"]["MDLN"].InnerText.Trim();
                        eqp.SOFTREV = recvTrx["secs"]["message"]["body"]["array1"]["SOFTREV"].InnerText.Trim();
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("MDLN({0}), SOFTREV({1})", eqp.MDLN, eqp.SOFTREV));

                        //check if opi request
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F2", string.Format("MDLN({0}), SOFTREV({1})", eqp.MDLN, eqp.SOFTREV) });//_common.ToFormatString(recvTrx.OuterXml) });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F4_1001_E_SVID1001ListofEnabledAlarms(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F4_1001_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        string lstCount = recvTrx["secs"]["message"]["body"]["array1"]["array2"].Attributes["len"].InnerText.Trim();
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                string.Format("Enabled alarm total count({0}).", lstCount));
                        switch (rtn)
                        {
                              case "OPI":
                                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                                    if (eqp == null)
                                    {
                                          //找不到eqp,就不到lineID,就無法發訊息
                                          return;
                                    }
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F4_1001_ListofEnabledAlarms", string.Format("Enabled alarm total count({0}).", lstCount) });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F4_1002_E_SVID1002ListofSetAlarms(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F4_1002_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        string lstCount = recvTrx["secs"]["message"]["body"]["array1"]["array2"].Attributes["len"].InnerText.Trim();
                        XmlNode xALID = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                        string ids = string.Empty;
                        //for (int i0 = 0; i0 < xALID.Count; i0++)
                        while (xALID != null)
                        {
                              ids = string.Format("{0},{1}", ids, xALID.InnerText.Trim());
                              xALID = xALID.NextSibling;
                        }
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                string.Format("Current set alarm total count({0}). ALID({1})", lstCount, ids));
                        switch (rtn)
                        {
                              case "OPI":
                                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                                    if (eqp == null)
                                    {
                                          //找不到eqp,就不到lineID,就無法發訊息
                                          return;
                                    }
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F4_1002_ListofSetAlarms", string.Format("Current set alarm total count({0})", lstCount) });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F4_1003_E_SVID1003Time(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F4_1003_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        string clock = recvTrx["secs"]["message"]["body"]["array1"]["CLOCK"].InnerText.Trim();
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                string.Format("Current date time({0}).", clock));
                        switch (rtn)
                        {
                              case "OPI":
                                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                                    if (eqp == null)
                                    {
                                          //找不到eqp,就不到lineID,就無法發訊息
                                          return;
                                    }
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F4_1003_Time", string.Format("Current date time({0}).", clock) });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F4_1004_E_SVID1004ControlMode(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F4_1004_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        #region [Control Mode - CIM Mode]
                        string ctrlState = recvTrx["secs"]["message"]["body"]["array1"]["CONTROLSTATE"].InnerText.Trim();
                        eBitResult preCimMode = eqp.File.CIMMode;
                        lock (eqp)
                        {
                              switch (ctrlState)
                              {
                                    case "4":
                                          eqp.File.HSMSControlMode = "ON-LINE-LOCAL";
                                          eqp.File.CIMMode = eBitResult.ON;
                                          break;
                                    case "5":
                                          eqp.File.HSMSControlMode = "ON-LINE-REMOTE";
                                          eqp.File.CIMMode = eBitResult.ON;
                                          break;
                                    default:
                                          eqp.File.HSMSControlMode = "OFF-LINE";
                                          eqp.File.CIMMode = eBitResult.OFF;
                                          break;
                              }
                        }
                        ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                        //20150616 cy:增加record to history的功能
                        ObjectManager.EquipmentManager.RecordEquipmentHistory(tid, eqp);
                        //20141023 cy:Report to OPI
                        Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                        if (preCimMode != eqp.File.CIMMode)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode Status({2}).", eqp.Data.NODENO, tid, eqp.File.CIMMode.ToString()));
                              //20161204 yang:CIM Mode要报给MES
                              Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { tid, eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });
                              ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                        }

                        #endregion
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F4_1004_ControlMode", string.Format("Control Mode({0}-{1})", ctrlState, eqp.File.HSMSControlMode) });
                                    break;
                              //20150608 cy:增加連線檢查時,若為online,則去問回eqp status, Mask status
                              case "ConnectedCheckContorl":
                                    if (ctrlState == "4" || ctrlState == "5")
                                          TS1F3_H_SelectedEquipmentStatusRequest(eqp.Data.NODENO, eqp.Data.NODEID, (uint)1007, "ConnectedCheckContorl", string.Empty);
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F4_1005_E_SVID1005ListofEnabledEvent(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F4_1005_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();

                        string lstCount = recvTrx["secs"]["message"]["body"]["array1"]["array2"].Attributes["len"].InnerText.Trim();
                        XmlNodeList xlstCEID = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                        string ids = string.Empty;
                        for (int i0 = 0; i0 < xlstCEID.Count; i0++)
                        {
                              ids = string.Format("{0},{1}", ids, xlstCEID[i0].InnerText.Trim());
                        }
                        _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                string.Format("Enabled event count({0}). CEID({1})", lstCount, ids));
                        switch (rtn)
                        {
                              case "OPI":
                                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                                    if (eqp == null)
                                    {
                                          //找不到eqp,就不到lineID,就無法發訊息
                                          return;
                                    }
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F4_1005_ListofEnabledEvent", string.Format("Enabled event count({0})", lstCount) });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F4_1006_E_SVID1006PreviousProcessSate(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F4_1006_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        #region [Process State - Equipemnt status]
                        string procState = recvTrx["secs"]["message"]["body"]["array1"]["PREVIOUSPROCESSSTATE"].InnerText.Trim();
                        eEQPStatus preEqpST = ConvertCsotEquipmentStatus(procState);
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] Previous Equipment Status({2}).", eqpno, tid, preEqpST.ToString()));
                        #endregion
                        switch (rtn)
                        {
                              case "OPI":
                                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                                    if (eqp == null)
                                    {
                                          //找不到eqp,就不到lineID,就無法發訊息
                                          return;
                                    }
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F4_1006_PreviousProcessSate", string.Format("Previous Process Status({0}-{1})", procState, ConvertCsotEquipmentStatus(procState)) });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F4_1007_E_SVID1007CurrentProcessState(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F4_1007_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        #region [Process State - Equipemnt status]
                        string procState = recvTrx["secs"]["message"]["body"]["array1"]["PROCESSSTATE"].InnerText.Trim();
                        lock (eqp)
                        {
                              eqp.File.PreStatus = eqp.File.Status;
                              eqp.File.Status = ConvertCsotEquipmentStatus(procState);
                              ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                              _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                        }
                        #endregion
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F4_1007_CurrentProcessState", string.Format("Current Process Status({0}-{1})", procState, ConvertCsotEquipmentStatus(procState)) });
                                    break;
                              //20150608 cy:增加連線檢查時,若為online,則去問回eqp status, Mask status
                              case "ConnectedCheckContorl":
                                    TS1F3_H_SelectedEquipmentStatusRequest(eqp.Data.NODENO, eqp.Data.NODEID, (uint)1008, "ConnectedCheckContorl", string.Empty);
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F4_1008_E_SVID1008StausofAllMask(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F4_1008_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();

                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        string msg = "No mask status";
                        #region [Mask Status]
                        if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].Attributes["len"].InnerText.Trim() != "0")
                        {
                              msg = string.Empty;
                              XmlNode maskNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                              string maskSlot = string.Empty;
                              string maskName = string.Empty;
                              string maskState = string.Empty;
                              List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                              while (maskNode != null)
                              {
                                    maskSlot = maskNode["MASKSLOT"].InnerText.Trim();
                                    maskName = maskNode["MASKNAME"].InnerText.Trim();
                                    maskState = maskNode["MASKSTATE"].InnerText.Trim();
                                    masks.Add(Tuple.Create(maskSlot, maskName, ConvertCsotMaterialStatus(maskState, string.IsNullOrEmpty(maskName))));
                                    maskNode = maskNode.NextSibling;
                                    msg = msg + string.Format("Mask slot-name-state({0}-{1}-{2}); ", maskSlot, maskName, maskState);
                              }
                              HandleMaskStatus(eqp, masks, tid, false, string.Empty);
                        }
                        #endregion

                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F4_1008_StausofAllMask", msg });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F4_1009_E_SVID1009StatusofAllMaskBufferSlots(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F4_1009_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        string msg = "No mask buffer slot status";
                        #region [Mask Buffer Slot]
                        if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].Attributes["len"].InnerText.Trim() != "0")
                        {
                              msg = string.Empty;
                              XmlNodeList maskNodes = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                              string maskSlot = string.Empty;
                              string maskState = string.Empty;
                              for (int i1 = 0; i1 < maskNodes.Count; i1++)
                              {
                                    maskSlot = maskNodes[i1]["MASKSLOT"].InnerText.Trim();
                                    maskState = maskNodes[i1]["MASKSLOTSTATE"].InnerText.Trim();
                                    _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                        string.Format("Mask slot({0}) status({1}).", maskSlot, ConvertMaskSlotState(maskState)));
                                    msg = msg + string.Format("Mask slot-status({0}-{1}); ", maskSlot, ConvertMaskSlotState(maskState));
                              }
                        }
                        #endregion
                        switch (rtn)
                        {
                              case "OPI":
                                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                                    if (eqp == null)
                                    {
                                          //找不到eqp,就不到lineID,就無法發訊息
                                          return;
                                    }
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F4_1009_StatusofAllMaskBufferSlots", msg });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F4_1010_E_SVID1010AllocationStatusofAllMaskBuffer(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F4_1010_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        string msg = "No mask buffer allocation status";
                        #region [Mask Allocation Slot]
                        if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].Attributes["len"].InnerText.Trim() != "0")
                        {
                              msg = string.Empty;
                              XmlNodeList maskNodes = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes;
                              string maskSlot = string.Empty;
                              string maskState = string.Empty;
                              for (int i1 = 0; i1 < maskNodes.Count; i1++)
                              {
                                    maskSlot = maskNodes[i1]["MASKSLOT"].InnerText.Trim();
                                    maskState = maskNodes[i1]["MASKALLOCATESTATE"].InnerText.Trim();
                                    _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                        string.Format("Mask slot({0}), allocation status({1}).", maskSlot, ConvertMaskAllocateState(maskState)));
                                    msg = msg + string.Format("Mask slot-status({0}-{1}); ", maskSlot, ConvertMaskAllocateState(maskState));
                              }
                        }
                        #endregion
                        switch (rtn)
                        {
                              case "OPI":
                                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                                    if (eqp == null)
                                    {
                                          return;
                                    }
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F4_1010_AllocationStatusofAllMaskBuffer", msg });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F4_1017_E_SVID1017SpoolingLoadState(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F4_1017_E");
                  #region Handle Logic
                  //TODO:Check control mode and abort setting and reply SnF0 or not.
                  //TODO:Logic handle.
                  #endregion
            }
            public void S1F4_1018_E_SVID1018ProcessingRecipeName(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F4_1018_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        string rpName = recvTrx["secs"]["message"]["body"]["array1"]["PPEXECNAME"].InnerText.Trim();
                        _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                string.Format("Current processing recipe name({0}).", rpName));
                        switch (rtn)
                        {
                              case "OPI":
                                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                                    if (eqp == null)
                                    {
                                          return;
                                    }
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F4_1017", string.Format("Current processing recipe name({0}).", rpName) });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F4_1019_E_SVID1019ProcessingRecipeNumber(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F4_1019_E");
                  #region Handle Logic
                  try
                  {
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        if (eqp.File.CurrentRecipeID != recvTrx["secs"]["message"]["body"]["array1"]["PPEXECID"].InnerText.Trim())
                        {
                              lock (eqp)
                                    eqp.File.CurrentRecipeID = recvTrx["secs"]["message"]["body"]["array1"]["PPEXECID"].InnerText.Trim();
                              ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                              ObjectManager.EquipmentManager.RecordEquipmentHistory(tid, eqp);
                        }
                        _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                string.Format("Current processing recipe number({0}).", eqp.File.CurrentRecipeID));

                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F4_1019", string.Format("Current processing recipe number({0}).", eqp.File.CurrentRecipeID) });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F4_1020_E_SVID1020ProcessingLotID(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F4_1020_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        string lotid = recvTrx["secs"]["message"]["body"]["array1"]["LOTEXECID"].InnerText.Trim();
                        _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                string.Format("Current processing lot ID({0}).", lotid));
                        switch (rtn)
                        {
                              case "OPI":
                                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                                    if (eqp == null)
                                    {
                                          return;
                                    }
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F4_1020", string.Format("Current processing lot ID({0}).", lotid) });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F4_1021_E_SVID1021ProcessingMaskName(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F4_1021_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        #region [Mask Status]
                        string maskName = recvTrx["secs"]["message"]["body"]["array1"]["MASKEXECNAME"].InnerText.Trim();
                        _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                string.Format("Current processing mask name({0}]", maskName));
                        List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                        masks.Add(Tuple.Create(string.Empty, maskName, eMaterialStatus.INUSE));
                        HandleMaskStatus(eqp, masks, tid, false, string.Empty);
                        #endregion
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F4_1021", string.Format("Current processing mask name({0}]", maskName) });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F4_1022_E_SVID1022ProcessingMaskSlotNumber(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F4_1022_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        #region [Mask Status]
                        string maskSlot = recvTrx["secs"]["message"]["body"]["array1"]["MASKEXECSLOTNO"].InnerText.Trim();
                        _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Current processing mask slot({0}).", maskSlot));
                        List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                        masks.Add(Tuple.Create(maskSlot, string.Empty, eMaterialStatus.INUSE));
                        HandleMaskStatus(eqp, masks, tid, false, string.Empty);
                        #endregion
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F4_1022", string.Format("Current processing mask slot({0}).", maskSlot) });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F4_1023_E_SVID1023ChamberTemperature(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F4_1023_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        string temperature = recvTrx["secs"]["message"]["body"]["array1"]["CHAMBERTEMP"].InnerText.Trim();
                        _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Chamber Temperature({0}).", temperature));
                        switch (rtn)
                        {
                              case "OPI":
                                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                                    if (eqp == null)
                                    {
                                          return;
                                    }
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F4_1023", string.Format("Chamber Temperature({0}).", temperature) });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F4_1024_E_SVID1024ALCPTemperature(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F4_1024_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        string temperature = recvTrx["secs"]["message"]["body"]["array1"]["STAGETEMP"].InnerText.Trim();
                        _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("Stage Temperature({0}).", temperature));
                        switch (rtn)
                        {
                              case "OPI":
                                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                                    if (eqp == null)
                                    {
                                          return;
                                    }
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F4_1024_StageTemperature", string.Format("Stage Temperature({0}).", temperature) });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F13_E_EstablishCommunicationsRequest(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F13_E");
                  #region Handle Logic
                  //Logic handle.
                  try
                  {
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      "Can not find Equipment Number in EquipmentEntity!");
                              TS1F14_H_EstablishCommunicationsRequestAcknowledge(eqpno, agent, tid, sysbytes, "1");
                              return;
                        }
                        TS1F14_H_EstablishCommunicationsRequestAcknowledge(eqpno, agent, tid, sysbytes, "0");

                        eqp.SecsCommunicated = true;
                        eqp.MDLN = recvTrx["secs"]["message"]["body"]["array1"]["MDLN"].InnerText.Trim();
                        eqp.SOFTREV = recvTrx["secs"]["message"]["body"]["array1"]["SOFTREV"].InnerText.Trim();

                        if (!eqp.EventReportConfigurated)
                        {
                              eqp.EventReportConfigurated = true;
                              _defineReport.Clear();
                              _defineReport.Add(new Tuple<uint, List<uint>>(1002, new List<uint>() { 1002 }));
                              _linkEvent.Clear();
                              _linkEvent.Add(new Tuple<uint, List<uint>>(3, new List<uint>() { 1003, 1002, 1007, 1008, 1009, 1010 }));
                              _linkEvent.Add(new Tuple<uint, List<uint>>(4, new List<uint>() { 1003, 1002, 1007, 1008, 1009, 1010 }));
                              _linkEvent.Add(new Tuple<uint, List<uint>>(101, new List<uint>() { 1006, 1007, 1002 }));
                              _linkEvent.Add(new Tuple<uint, List<uint>>(102, new List<uint>() { 1006, 1007, 1002 }));
                              _linkEvent.Add(new Tuple<uint, List<uint>>(103, new List<uint>() { 1006, 1007, 1002 }));
                              _linkEvent.Add(new Tuple<uint, List<uint>>(104, new List<uint>() { 1006, 1007, 1002 }));
                              _linkEvent.Add(new Tuple<uint, List<uint>>(105, new List<uint>() { 1006, 1007, 1002 }));
                              _linkEvent.Add(new Tuple<uint, List<uint>>(106, new List<uint>() { 1006, 1007, 1002 }));
                              _linkEvent.Add(new Tuple<uint, List<uint>>(107, new List<uint>() { 1006, 1007, 1002 }));
                              _linkEvent.Add(new Tuple<uint, List<uint>>(108, new List<uint>() { 1006, 1007, 1002 }));
                              _linkEvent.Add(new Tuple<uint, List<uint>>(109, new List<uint>() { 1006, 1007, 3001, 3002, 3003, 3004, 3005, 1002 }));
                              _linkEvent.Add(new Tuple<uint, List<uint>>(110, new List<uint>() { 1006, 1007, 3006, 3007, 3008, 3009, 3010, 3011, 1002 }));
                              _linkEvent.Add(new Tuple<uint, List<uint>>(111, new List<uint>() { 1006, 1007, 1002 }));
                              _linkEvent.Add(new Tuple<uint, List<uint>>(112, new List<uint>() { 1006, 1007, 1002 }));
                              _linkEvent.Add(new Tuple<uint, List<uint>>(113, new List<uint>() { 1006, 1007, 1002 }));
                              _linkEvent.Add(new Tuple<uint, List<uint>>(114, new List<uint>() { 1006, 1007, 1002 }));
                              _linkEvent.Add(new Tuple<uint, List<uint>>(115, new List<uint>() { 1006, 1007, 1002 }));
                              _linkEvent.Add(new Tuple<uint, List<uint>>(116, new List<uint>() { 1006, 1007, 1002 }));
                              //_linkEvent.Add(new Tuple<uint, List<uint>>(210, new List<uint>() { 3032, 3020 }));
                              _linkEvent.Add(new Tuple<uint, List<uint>>(212, new List<uint>() { 3034, 1008 }));

                              TS2F37_H_EnableDisableEventReport(eqpno, agent, false, null, "DefineReport", tid);


                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F14_E_EstablishCommunicationsRequestAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F14_E");
                  #region Handle Logic
                  try
                  {
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string ack = recvTrx["secs"]["message"]["body"]["array1"]["COMMACK"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        eqp.MDLN = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["MDLN"].InnerText.Trim();
                        eqp.SOFTREV = recvTrx["secs"]["message"]["body"]["array1"]["array2"]["SOFTREV"].InnerText.Trim();
                        string msg = string.Empty;
                        if (ack == "0")
                        {
                              msg = "Accept host establish communications request";
                              _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid, msg);
                              eqp.SecsCommunicated = true;
                        }
                        else
                        {
                              msg = "Deny host establish communications request.";
                              _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid, msg);

                              //switch (rtn)
                              //{
                              //      case "AutoOnlineRequest":
                              //        eqp.File.CIMMode = eBitResult.OFF;
                              //        Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                              //        ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                              //        break;
                              //}
                        }
                        //check if opi request
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F14_EstablishCommunicationsRequestAcknowledge", string.IsNullOrEmpty(msg) ? _common.ToFormatString(recvTrx.OuterXml) : msg });
                                    break;
                              case "AutoOnlineRequest":
                                    if (ack == "0")
                                    {
                                          //20160128 cy:BCS主動要求時, Nikon不會再發S1F13, 所以要在這再確認一次
                                          if (!eqp.EventReportConfigurated)
                                          {
                                                eqp.EventReportConfigurated = true;
                                                _defineReport.Clear();
                                                _defineReport.Add(new Tuple<uint, List<uint>>(1002, new List<uint>() { 1002 }));
                                                _linkEvent.Clear();
                                                _linkEvent.Add(new Tuple<uint, List<uint>>(3, new List<uint>() { 1003, 1002, 1007, 1008, 1009, 1010 }));
                                                _linkEvent.Add(new Tuple<uint, List<uint>>(4, new List<uint>() { 1003, 1002, 1007, 1008, 1009, 1010 }));
                                                _linkEvent.Add(new Tuple<uint, List<uint>>(101, new List<uint>() { 1006, 1007, 1002 }));
                                                _linkEvent.Add(new Tuple<uint, List<uint>>(102, new List<uint>() { 1006, 1007, 1002 }));
                                                _linkEvent.Add(new Tuple<uint, List<uint>>(103, new List<uint>() { 1006, 1007, 1002 }));
                                                _linkEvent.Add(new Tuple<uint, List<uint>>(104, new List<uint>() { 1006, 1007, 1002 }));
                                                _linkEvent.Add(new Tuple<uint, List<uint>>(105, new List<uint>() { 1006, 1007, 1002 }));
                                                _linkEvent.Add(new Tuple<uint, List<uint>>(106, new List<uint>() { 1006, 1007, 1002 }));
                                                _linkEvent.Add(new Tuple<uint, List<uint>>(107, new List<uint>() { 1006, 1007, 1002 }));
                                                _linkEvent.Add(new Tuple<uint, List<uint>>(108, new List<uint>() { 1006, 1007, 1002 }));
                                                _linkEvent.Add(new Tuple<uint, List<uint>>(109, new List<uint>() { 1006, 1007, 3001, 3002, 3003, 3004, 3005, 1002 }));
                                                _linkEvent.Add(new Tuple<uint, List<uint>>(110, new List<uint>() { 1006, 1007, 3006, 3007, 3008, 3009, 3010, 3011, 1002 }));
                                                _linkEvent.Add(new Tuple<uint, List<uint>>(111, new List<uint>() { 1006, 1007, 1002 }));
                                                _linkEvent.Add(new Tuple<uint, List<uint>>(112, new List<uint>() { 1006, 1007, 1002 }));
                                                _linkEvent.Add(new Tuple<uint, List<uint>>(113, new List<uint>() { 1006, 1007, 1002 }));
                                                _linkEvent.Add(new Tuple<uint, List<uint>>(114, new List<uint>() { 1006, 1007, 1002 }));
                                                _linkEvent.Add(new Tuple<uint, List<uint>>(115, new List<uint>() { 1006, 1007, 1002 }));
                                                _linkEvent.Add(new Tuple<uint, List<uint>>(116, new List<uint>() { 1006, 1007, 1002 }));
                                                //_linkEvent.Add(new Tuple<uint, List<uint>>(210, new List<uint>() { 3032, 3020 }));
                                                _linkEvent.Add(new Tuple<uint, List<uint>>(212, new List<uint>() { 3034, 1008 }));

                                                TS2F37_H_EnableDisableEventReport(eqpno, agent, false, null, "DefineReportAndOnline", tid);
                                          }
                                          else

                                          TS1F17_H_RequestOnLine(eqpno, agent, "AutoOnlineRequest", string.Empty);   //add by yang 2017/5/16,for disconnect when bcs is running
                                    }
                                    else
                                    {
                                          eqp.File.CIMMode = eBitResult.OFF;
                                          Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                                          //20161204 yang:CIM Mode要报给MES
                                          Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { tid, eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });
                                          ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                    }
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F16_E_OffLineAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F16_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string ack = recvTrx["secs"]["message"]["body"]["OFLACK"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        string msg = string.Empty;
                        if (ack == "0")
                        {
                              msg = "0:Accept host off-line request.";
                        }
                        else
                        {
                              msg = string.Format("{0}:Deny host off-line request.", ack);
                              //20150319 cy:Denied offline,就不會offline scenario,所以刪除
                              string timerId = string.Format("S1F15_OfflineRequest_{0}_OPI", eqpno);
                              if (_timerManager.IsAliveTimer(timerId))
                              {
                                    _timerManager.TerminateTimer(timerId); //remove
                              }
                        }
                        _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid, msg);
                        //check if opi request
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F16_OffLineAcknowledge", msg });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F18_E_OnLineAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F18_E");
                  #region Handle Logic
                  try
                  {
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string ack = recvTrx["secs"]["message"]["body"]["ONLACK"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        string msg = string.Empty;
                        switch (ack)
                        {
                              case "0":
                                    msg = "0:Accept host on-line request.";
                                    break;
                              case "1":
                                    msg = "1:Not permit host on-line request.";
                                    break;
                              case "2":
                                    msg = "2:Deny host on-line request because is already on-line.";
                                    TS1F3_H_SelectedEquipmentStatusRequest(eqp.Data.NODENO, eqp.Data.NODEID, (uint)1004, "ConnectedCheckContorl", tid);
                                    //lock (eqp)
                                    //    eqp.File.CIMMode = eBitResult.ON;
                                    //ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                    ////20141023 cy:Report to OPI
                                    //Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                                    break;
                              default:
                                    msg = string.Format("Other Error, Not Accepted.", ack);
                                    break;
                        }
                        _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid, msg);
                        //check if opi request
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F18", msg });
                                    break;
                              case "AutoOnlineRequest":
                                    if (ack != "0" && ack != "2")
                                    {
                                          eqp.File.HSMSControlMode = "OFF-LINE";
                                          eqp.File.CIMMode = eBitResult.OFF;
                                          ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                          _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                      string.Format("Control Mode({0}).", eqp.File.HSMSControlMode));
                                          //Report to OPI
                                          Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                                          //20161204 yang:CIM Mode要报给MES
                                          Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { tid, eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });
                                          Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                                      new object[4] { tid, eqp.Data.LINEID, "Auto Online Request", string.Format("Equipment({0}) deny to change online.({1})", eqp.Data.NODENO, msg) });
                                    }
                                    //20160128 cy:若是already online, 重設S2F23
                                    if (ack == "2")
                                    {
                                          //20160128 cy:Online後,再發S2F23設定
                                          if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "UTILY") != null)
                                                TS2F23_H_TraceInitializeSend(eqpno, agent, "UTILY", eqp.File.UtilityIntervalNK, "-1", "1", "S2F23_APCIM", tid);
                                          else if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "APCIM") != null)
                                                TS2F23_H_TraceInitializeSend(eqpno, agent, "APCIM", eqp.File.APCImportanIntervalNK, "-1", "1", "S2F23_APCNO", tid);
                                          else if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "APCNO") != null)
                                                TS2F23_H_TraceInitializeSend(eqpno, agent, "APCNO", eqp.File.APCNormalIntervalNK, "-1", "1", "S2F23_SPCAL", tid);
                                          else if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "SPCAL") != null)
                                                TS2F23_H_TraceInitializeSend(eqpno, agent, "SPCAL", eqp.File.SpecialDataIntervalNK, "-1", "1", string.Empty, tid);
                                    }
                                    break;
                        }
                        if (ack != "0")
                        {
                              //20150317 cy:online request but already online, terminal timer and do nothing.
                              string timerId = string.Format("S1F17_OnlineRequest_{0}_OPI", eqpno);
                              if (_timerManager.IsAliveTimer(timerId))
                              {
                                    _timerManager.TerminateTimer(timerId); //remove
                              }
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            #endregion

            #region Send by host-S1
            public void TS1F1_H_AreYouThereRequest(string eqpno, string eqpid, string tag, string trxid)
            {
                  try
                  {
                        //Get Agent Object
                        IServerAgent agent = GetServerAgent(eqpid);
                        if (agent == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("Can not get agent object with name ({0}]", eqpid));
                              return;
                        }
                        //Get Transaction Format
                        XmlDocument sendTrx = agent.GetTransactionFormat("S1F1_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S1F1_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S1F1_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;
                        //Put to Queue
                        xMessage msg = new xMessage();
                        msg.Name = sendTrx["secs"]["message"].Attributes["name"].InnerText.Trim();
                        msg.FromAgent = agent.Name;
                        msg.ToAgent = agent.Name;
                        msg.Data = sendTrx;
                        PutMessage(msg);
                  }
                  catch (InvalidOperationException ioex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ioex);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void TS1F2_H_OnLineData(string eqpno, string eqpid, string tid, string sysbytes)
            {
                  try
                  {
                        //Get Agent Object
                        IServerAgent agent = GetServerAgent(eqpid);
                        if (agent == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("Can not get agent object with name ({0}]", eqpid));
                              return;
                        }
                        //Get Transaction Format
                        XmlDocument sendTrx = agent.GetTransactionFormat("S1F2_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S1F2_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = tid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = sysbytes;
                        sendTrx["secs"]["message"]["body"]["array1"].Attributes["len"].InnerText = "0";
                        //Put to Queue
                        xMessage msg = new xMessage();
                        msg.Name = sendTrx["secs"]["message"].Attributes["name"].InnerText.Trim();
                        msg.FromAgent = agent.Name;
                        msg.ToAgent = agent.Name;
                        msg.Data = sendTrx;
                        PutMessage(msg);
                  }
                  catch (InvalidOperationException ioex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ioex);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void TS1F3_H_SelectedEquipmentStatusRequest(string eqpno, string eqpid, uint svid, string tag, string trxid)
            {
                  //*NOTE:發這個時,要在body的keyvalue填入問的是哪一個SVID
                  try
                  {
                        //Get Agent Object
                        IServerAgent agent = GetServerAgent(eqpid);
                        if (agent == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("Can not get agent object with name ({0}]", eqpid));
                              return;
                        }
                        //Get Transaction Format
                        XmlDocument sendTrx = agent.GetTransactionFormat("S1F3_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S1F3_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"].Attributes["keyvalue"].InnerText = svid.ToString();
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["SVID"], svid.ToString());
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S1F3_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;
                        //Put to Queue
                        xMessage msg = new xMessage();
                        msg.Name = sendTrx["secs"]["message"].Attributes["name"].InnerText.Trim();
                        msg.FromAgent = agent.Name;
                        msg.ToAgent = agent.Name;
                        msg.Data = sendTrx;
                        PutMessage(msg);
                  }
                  catch (InvalidOperationException ioex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ioex);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void TS1F13_H_EstablishCommunicationsRequest(string eqpno, string eqpid, string tag, string trxid)
            {
                  try
                  {
                        //Get Agent Object
                        IServerAgent agent = GetServerAgent(eqpid);
                        if (agent == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("Can not get agent object with name ({0}]", eqpid));
                              return;
                        }
                        //Get Transaction Format
                        XmlDocument sendTrx = agent.GetTransactionFormat("S1F13_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S1F13_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"]["array1"].Attributes["len"].InnerText = "0";
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S1F13_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;
                        //Put to Queue
                        xMessage msg = new xMessage();
                        msg.Name = sendTrx["secs"]["message"].Attributes["name"].InnerText.Trim();
                        msg.FromAgent = agent.Name;
                        msg.ToAgent = agent.Name;
                        msg.Data = sendTrx;
                        PutMessage(msg);
                  }
                  catch (InvalidOperationException ioex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ioex);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void TS1F14_H_EstablishCommunicationsRequestAcknowledge(string eqpno, string eqpid, string tid, string sysbytes, string ack)
            {
                  try
                  {
                        //Get Agent Object
                        IServerAgent agent = GetServerAgent(eqpid);
                        if (agent == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("Can not get agent object with name ({0}]", eqpid));
                              return;
                        }
                        //Get Transaction Format
                        XmlDocument sendTrx = agent.GetTransactionFormat("S1F14_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S1F14_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = tid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = sysbytes;
                        sendTrx["secs"]["message"]["body"]["array1"].Attributes["len"].InnerText = "2";
                        sendTrx["secs"]["message"]["body"]["array1"]["COMMACK"].InnerText = ack;
                        //Put to Queue
                        xMessage msg = new xMessage();
                        msg.Name = sendTrx["secs"]["message"].Attributes["name"].InnerText.Trim();
                        msg.FromAgent = agent.Name;
                        msg.ToAgent = agent.Name;
                        msg.Data = sendTrx;
                        PutMessage(msg);
                  }
                  catch (InvalidOperationException ioex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ioex);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void TS1F15_H_RequestOffLine(string eqpno, string eqpid, string tag, string trxid)
            {
                  try
                  {
                        //Get Agent Object
                        IServerAgent agent = GetServerAgent(eqpid);
                        if (agent == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("Can not get agent object with name ({0}]", eqpid));
                              return;
                        }
                        //Get Transaction Format
                        XmlDocument sendTrx = agent.GetTransactionFormat("S1F15_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S1F15_H)");
                              return;
                        }
                        //20150319 cy:若是由OPI要求的,設定計時器,時間內有任何異常,丟訊息給OPI
                        #region 設定計時器
                        if (tag == "OPI")
                        {
                              //wait mes ack
                              string timerId = string.Format("S1F15_OfflineRequest_{0}_OPI", eqpno);
                              if (_timerManager.IsAliveTimer(timerId))
                              {
                                    _timerManager.TerminateTimer(timerId); //remove old
                              }
                              //create wait timer
                              _timerManager.CreateTimer(timerId, false, ParameterManager["NIKONSECSDATATIMEOUT"].GetInteger(),
                                  new System.Timers.ElapsedEventHandler(OpiOfflineRequestTimeOut), trxid);
                        }
                        #endregion
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S1F15_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;
                        //Put to Queue
                        xMessage msg = new xMessage();
                        msg.Name = sendTrx["secs"]["message"].Attributes["name"].InnerText.Trim();
                        msg.FromAgent = agent.Name;
                        msg.ToAgent = agent.Name;
                        msg.Data = sendTrx;
                        PutMessage(msg);
                  }
                  catch (InvalidOperationException ioex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ioex);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void TS1F17_H_RequestOnLine(string eqpno, string eqpid, string tag, string trxid)
            {
                  try
                  {
                        //Get Agent Object
                        IServerAgent agent = GetServerAgent(eqpid);
                        if (agent == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("Can not get agent object with name ({0}]", eqpid));
                              if (tag == "OPI")
                              {
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[] { trxid, string.Empty, "Send S1F17", string.Format("SECSAgent name({0}) is not correct.", eqpid) });
                              }
                              return;
                        }
                        //Get Transaction Format
                        XmlDocument sendTrx = agent.GetTransactionFormat("S1F17_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S1F17_H)");
                              if (tag == "OPI")
                              {
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[] { trxid, string.Empty, "Send S1F17", "Transaction name(S1F17_H) is not exist." });
                              }
                              return;
                        }
                        //20150319 cy:若是由OPI要求的,設定計時器,時間內有任何異常,丟訊息給OPI
                        #region 設定計時器
                        if (tag == "OPI")
                        {
                              //wait mes ack
                              string timerId = string.Format("S1F17_OnlineRequest_{0}_OPI", eqpno);
                              if (_timerManager.IsAliveTimer(timerId))
                              {
                                    _timerManager.TerminateTimer(timerId); //remove old
                              }
                              //create wait timer
                              _timerManager.CreateTimer(timerId, false, ParameterManager["NIKONSECSDATATIMEOUT"].GetInteger(),
                                  new System.Timers.ElapsedEventHandler(OpiOnlineRequestTimeOut), trxid);
                        }
                        #endregion
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S1F17_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;
                        //Put to Queue
                        xMessage msg = new xMessage();
                        msg.Name = sendTrx["secs"]["message"].Attributes["name"].InnerText.Trim();
                        msg.FromAgent = agent.Name;
                        msg.ToAgent = agent.Name;
                        msg.Data = sendTrx;
                        PutMessage(msg);
                  }
                  catch (InvalidOperationException ioex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ioex);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region Sent form host-S1
            public void S1F1_H_AreYouThereRequest(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F1_H T3-Timeout", false);
                              string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                              string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                              string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                              string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                              Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                              if (eqp == null)
                              {
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, false, tid, "Can not find Equipment Number in EquipmentEntity!");
                                    return;
                              }
                              switch (rtn)
                              {
                                    case "OPI":
                                          Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S1F1", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F1_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S1F2_H_OnLineData(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F2_H T3-Timeout", false);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F2_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S1F3_H_SelectedEquipmentStatusRequest(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F3_H T3-Timeout", false);
                              string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                              string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                              string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                              string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                              Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                              if (eqp == null)
                              {
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, false, tid, "Can not find Equipment Number in EquipmentEntity!");
                                    return;
                              }
                              switch (rtn)
                              {
                                    case "ConnectedCheckContorl":
                                          eqp.File.CIMMode = eBitResult.OFF;
                                          eqp.File.HSMSControlMode = "OFF-LINE";
                                          Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                                          //20161204 yang:CIM Mode要报给MES
                                          Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { tid, eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });
                                          break;
                                    case "OPI":
                                          Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S1F3", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F3_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S1F13_H_EstablishCommunicationsRequest(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F13_H T3-Timeout", false);
                              string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                              string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                              string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                              string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                              Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                              if (eqp == null)
                              {
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, false, tid, "Can not find Equipment Number in EquipmentEntity!");
                                    return;
                              }
                              switch (rtn)
                              {
                                    case "AutoOnlineRequest":
                                          Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                                          new object[4] { tid, eqp.Data.LINEID, "Auto Online Request", string.Format("Host establish communication with Equipment({0}) T3 timeout.", eqp.Data.NODENO) });
                                          break;
                                    case "OPI":
                                          Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S1F13", "T3 Timeout" });
                                          break;
                              }
                              eqp.File.CIMMode = eBitResult.OFF;
                              eqp.File.HSMSControlMode = "OFF-LINE";
                              Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                              //20161204 yang:CIM Mode要报给MES
                              Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { tid, eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });
                              ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F13_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S1F14_H_EstablishCommunicationsRequestAcknowledge(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F14_H T3-Timeout", false);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F14_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S1F15_H_RequestOffLine(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F15_H T3-Timeout", false);
                              string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                              string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                              string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                              string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                              Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                              if (eqp == null)
                              {
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, false, tid, "Can not find Equipment Number in EquipmentEntity!");
                                    return;
                              }
                              //20150317 cy:online request timeout, terminal timer and do nothing.
                              string timerId = string.Format("S1F15_OfflineRequest_{0}_OPI", eqpno);
                              if (_timerManager.IsAliveTimer(timerId))
                              {
                                    _timerManager.TerminateTimer(timerId); //remove
                              }
                              switch (rtn)
                              {
                                    case "OPI":
                                          Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S1F15", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F15_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S1F17_H_RequestOnLine(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F17_H T3-Timeout", false);
                              string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                              string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                              string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                              string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                              Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                              if (eqp == null)
                              {
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, false, tid, "Can not find Equipment Number in EquipmentEntity!");
                                    return;
                              }
                              //20150319 cy:online request timeout, terminal timer and do nothing.
                              string timerId = string.Format("S1F17_OnlineRequest_{0}_OPI", eqpno);
                              if (_timerManager.IsAliveTimer(timerId))
                              {
                                    _timerManager.TerminateTimer(timerId); //remove
                              }
                              switch (rtn)
                              {
                                    case "OPI":
                                          Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S1F17", "T3 Timeout" });
                                          break;
                                    case "AutoOnlineRequest":

                                          Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                                          new object[4] { tid, eqp.Data.LINEID, "Auto Online Request", string.Format("Host request Equipment({0}) online T3 timeout.", eqp.Data.NODENO) });
                                          break;
                              }
                              eqp.File.CIMMode = eBitResult.OFF;
                              eqp.File.HSMSControlMode = "OFF-LINE";
                              Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                              //20161204 yang:CIM Mode要报给MES
                              Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { tid, eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });
                              ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F17_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            //timeout handler
            private void OpiOnlineRequestTimeOut(object subject, System.Timers.ElapsedEventArgs e)
            {
                  UserTimer timer = subject as UserTimer;
                  if (timer == null)
                  {
                        return;
                  }
                  string[] arr = timer.TimerId.Split('_');
                  if (arr.Length < 3)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("Invalid TimerId=({0}) error", timer.TimerId));
                        return;
                  }
                  string eqpno = arr[2];
                  string tid = timer.State.ToString();
                  Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                  if (eqp == null)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                string.Format("Can not find Equipment No({0}) in EquipmentEntity! TrxId({1})", eqpno, tid));
                        return;
                  }

                  string msg = string.Format("[EQUIPMENT={0}] [{1}] Online request from OPI do not action. Check if request transaction sent and equipment responted.", eqpno, tid);
                  Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                  new object[] { tid, string.Empty, "Send S1F17", msg });
                  NLogManager.Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", msg);
            }

            //timeout handler
            private void OpiOfflineRequestTimeOut(object subject, System.Timers.ElapsedEventArgs e)
            {
                  UserTimer timer = subject as UserTimer;
                  if (timer == null)
                  {
                        return;
                  }
                  string[] arr = timer.TimerId.Split('_');
                  if (arr.Length < 3)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                            string.Format("Invalid TimerId=({0}) error", timer.TimerId));
                        return;
                  }
                  string eqpno = arr[2];
                  string tid = timer.State.ToString();
                  Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                  if (eqp == null)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                string.Format("Can not find Equipment No({0}) in EquipmentEntity! TrxId({1})", eqpno, tid));
                        return;
                  }

                  string msg = string.Format("[EQUIPMENT={0}] [{1}] Offline request from OPI do not action. Check if request transaction sent and equipment responted.", eqpno, tid);
                  Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                  new object[] { tid, string.Empty, "Send S1F15", msg });
                  NLogManager.Logger.LogWarnWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", msg);
            }

            #endregion
      }
}