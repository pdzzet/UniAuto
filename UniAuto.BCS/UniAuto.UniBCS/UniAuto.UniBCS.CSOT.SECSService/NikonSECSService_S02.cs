using System;
using System.Reflection;
using System.Xml;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Log;
using System.Collections.Generic;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using System.Text.RegularExpressions;
using UniAuto.UniBCS.MISC;

namespace UniAuto.UniBCS.CSOT.SECSService
{
      public partial class NikonSECSService
      {
            #region Recipte form equipment-S2
            public void S2F0_E_AbortTransaction(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F0_E");
                  #region Handle Logic
                  //TODO:Check control mode and abort setting and reply SnF0 or not.
                  //TODO:Logic handle.
                  #endregion
            }
            private void S2F14_EquipmentConstantData(XmlDocument recvTrx)
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

                  string ecid = string.Format(" ECID({0})", recvTrx["secs"]["message"]["body"].Attributes["keyvalue"].InnerText.Trim());
                  ecid = string.Format("{0} ECV({1})", ecid, recvTrx["secs"]["message"]["body"]["ECV"].InnerText.Trim());

                  _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                              string.Format("Equipment constant data.{0}", ecid));

                  //check if opi request
                  switch (rtn)
                  {
                        case "OPI":
                              Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                  new object[4] { tid, eqp.Data.LINEID, "Reply S2F14_EquipmentConstantData", string.Format("Equipment constant data.{0}", ecid) });
                              break;
                  }

            }
            public void S2F14_501_E_ECID501TimeFormat(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F14_501_E");
                  #region Handle Logic
                  try
                  {
                        S2F14_EquipmentConstantData(recvTrx);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F14_503_E_ECID503TimeOutvalueofcommunicationestablishment(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F14_503_E");
                  #region Handle Logic
                  try
                  {
                        S2F14_EquipmentConstantData(recvTrx);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F14_505_E_ECID505MaximumSpoolSize(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F14_505_E");
                  #region Handle Logic
                  try
                  {
                        S2F14_EquipmentConstantData(recvTrx);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F14_506_E_ECID506MaximumSpoolTransmissionMessageSize(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F14_506_E");
                  #region Handle Logic
                  try
                  {
                        S2F14_EquipmentConstantData(recvTrx);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F14_507_E_ECID507SpecificationWhenetheSpoolAreaBecomesFull(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F14_507_E");
                  #region Handle Logic
                  try
                  {
                        S2F14_EquipmentConstantData(recvTrx);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F14_508_E_ECID508DefaultControlMode(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F14_508_E");
                  #region Handle Logic
                  try
                  {
                        S2F14_EquipmentConstantData(recvTrx);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F14_509_E_ECID509DefaultOnLinemode(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F14_509_E");
                  #region Handle Logic
                  try
                  {
                        S2F14_EquipmentConstantData(recvTrx);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F14_510_E_ECID510OnLineFailureState(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F14_510_E");
                  #region Handle Logic
                  try
                  {
                        S2F14_EquipmentConstantData(recvTrx);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F16_E_NewEquipmentConstantAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F16_E");
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
                        string eac = recvTrx["secs"]["message"]["body"]["EAC"].InnerText.Trim();
                        string ecid = recvTrx["secs"]["message"]["return"].Attributes["id"].InnerText.Trim();
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                    string.Format("New equipment constant acknowledge for ({0}). ({1}:{2})", ecid, eac, ConvertDescriptionEAC(eac)));
                        //check if opi request
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S2F16_NewEquipmentConstantAcknowledge", string.Format("New equipment constant acknowledge for ({0}). ({1}:{2})", ecid, eac, ConvertDescriptionEAC(eac)) });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F17_E_DateandTimeRequest(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F17_E");
                  #region Handle Logic
                  try
                  {
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        TS2F18_H_DateandTimeData(eqpno, agent, tid, sysbytes);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F18_E_DateandTimeData(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F18_E");
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
                        string time = recvTrx["secs"]["message"]["body"]["TIME"].InnerText.Trim();
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                            string.Format("Receive current date time : {0}", time));
                        //check if opi request
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S2F18", string.Format("Equipment current date time : {0}", time) });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F24_E_TraceInitializeSendAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F24_E");
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
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        string tiaack = string.Empty;
                        switch (recvTrx["secs"]["message"]["body"]["TIAACK"].InnerText.Trim())
                        {
                              case "0":
                                    tiaack = "0:Everything correct";
                                    break;
                              case "1":
                                    tiaack = "1:Too many SVIDs";
                                    break;
                              case "2":
                                    tiaack = "2:No more traces allowed";
                                    break;
                              case "3":
                                    tiaack = "3:Invalid period";
                                    break;
                              case "4":
                                    tiaack = "4:Equipment-specified error";
                                    break;
                        }
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Trace Initialize Send Acknowledge. TIAACK({0})", tiaack));
                        //check if opi request
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S2F24", string.Format("Trace Initialize Send Acknowledge. TIAACK({0})", tiaack) });
                                    break;
                              case "S2F23_APCIM":
                                    if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "APCIM") != null)
                                          TS2F23_H_TraceInitializeSend(eqpno, agent, "APCIM", eqp.File.APCImportanIntervalNK, "-1", "1", "S2F23_APCNO", tid);
                                    else if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "APCNO") != null)
                                          TS2F23_H_TraceInitializeSend(eqpno, agent, "APCNO", eqp.File.APCNormalIntervalNK, "-1", "1", "S2F23_SPCAL", tid);
                                    else if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "SPCAL") != null)
                                          TS2F23_H_TraceInitializeSend(eqpno, agent, "SPCAL", eqp.File.SpecialDataIntervalNK, "-1", "1", string.Empty, tid);
                                    break;
                              case "S2F23_APCNO":
                                    if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "APCNO") != null)
                                          TS2F23_H_TraceInitializeSend(eqpno, agent, "APCNO", eqp.File.APCNormalIntervalNK, "-1", "1", "S2F23_SPCAL", tid);
                                    else if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "SPCAL") != null)
                                          TS2F23_H_TraceInitializeSend(eqpno, agent, "SPCAL", eqp.File.SpecialDataIntervalNK, "-1", "1", string.Empty, tid);
                                    break;
                              case "S2F23_SPCAL":
                                    if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "SPCAL") != null)
                                          TS2F23_H_TraceInitializeSend(eqpno, agent, "SPCAL", eqp.File.SpecialDataIntervalNK, "-1", "1", string.Empty, tid);
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F25_E_LoopbackDiagnosticRequest(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F25_E");
                  #region Handle Logic
                  try
                  {
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string abs = recvTrx["secs"]["message"]["body"]["ABS"].InnerText.Trim();
                        TS2F26_H_LoopbackDiagnosticData(eqpno, agent, tid, sysbytes, abs);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F26_E_LoopbackDiagnosticData(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F26_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                  "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }

                        //<ABS name="ABS" type="B" len="10" fixlen="False" />
                        //body
                        string abs = recvTrx["secs"]["message"]["body"]["ABS"].InnerText.Trim();

                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S2F26", abs });
                                    break;
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void S2F30_EquipmentConstantNamelist(XmlDocument recvTrx)
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
                        return;
                  }
                  string ecid = string.Format(" ECID({0})", recvTrx["secs"]["message"]["body"]["array1"]["array2"]["ECID"].InnerText.Trim());
                  ecid = string.Format("{0} ECNAME({1})", ecid, recvTrx["secs"]["message"]["body"]["array1"]["array2"]["ECNAME"].InnerText.Trim());
                  ecid = string.Format("{0} ECMIN({1})", ecid, recvTrx["secs"]["message"]["body"]["array1"]["array2"]["ECMIN"].InnerText.Trim());
                  ecid = string.Format("{0} ECMAX({1})", ecid, recvTrx["secs"]["message"]["body"]["array1"]["array2"]["ECMAX"].InnerText.Trim());
                  ecid = string.Format("{0} ECDEF({1})", ecid, recvTrx["secs"]["message"]["body"]["array1"]["array2"]["ECDEF"].InnerText.Trim());
                  ecid = string.Format("{0} UNITS({1})", ecid, recvTrx["secs"]["message"]["body"]["array1"]["array2"]["UNITS"].InnerText.Trim());

                  _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                              string.Format("Equipment constant name.{0}", ecid));
                  //check if opi request
                  string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                  switch (rtn)
                  {
                        case "OPI":
                              Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                  new object[4] { tid, eqp.Data.LINEID, "Reply S2F30", string.Format("Equipment constant name.{0}", ecid) });
                              break;
                  }
            }
            public void S2F30_501_E_ECID501EquipmentConstantNamelist(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F30_501_E");
                  #region Handle Logic
                  try
                  {
                        S2F30_EquipmentConstantNamelist(recvTrx);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F30_503_E_ECID503EquipmentConstantNamelist(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F30_503_E");
                  #region Handle Logic
                  try
                  {
                        S2F30_EquipmentConstantNamelist(recvTrx);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F30_505_E_ECID505EquipmentConstantNamelist(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F30_505_E");
                  #region Handle Logic
                  try
                  {
                        S2F30_EquipmentConstantNamelist(recvTrx);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F30_506_E_ECID506EquipmentConstantNamelist(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F30_506_E");
                  #region Handle Logic
                  try
                  {
                        S2F30_EquipmentConstantNamelist(recvTrx);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F30_507_E_ECID507EquipmentConstantNamelist(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F30_507_E");
                  #region Handle Logic
                  try
                  {
                        S2F30_EquipmentConstantNamelist(recvTrx);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F30_508_E_ECID508EquipmentConstantNamelist(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F30_508_E");
                  #region Handle Logic
                  try
                  {
                        S2F30_EquipmentConstantNamelist(recvTrx);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F30_509_E_ECID509EquipmentConstantNamelist(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F30_509_E");
                  #region Handle Logic
                  try
                  {
                        S2F30_EquipmentConstantNamelist(recvTrx);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F30_510_E_ECID510EquipmentConstantNamelist(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F30_510_E");
                  #region Handle Logic
                  try
                  {
                        S2F30_EquipmentConstantNamelist(recvTrx);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F32_E_DateandTimeSetAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F32_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        string tiack = recvTrx["secs"]["message"]["body"]["TIACK"].InnerText.Trim();
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                            string.Format("Set date time acknowledge({0}:{1})", tiack, tiack == "0" ? "Accepted" : "Error"));
                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S2F32", string.Format("Set date time acknowledge({0}:{1})", tiack, tiack == "0" ? "Accepted" : "Error") });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F34_E_DefineReportAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F34_E");
                  #region Handle Logic
                  try
                  {
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        string ack = ConvertDescriptionDRACK(recvTrx["secs"]["message"]["body"]["DRACK"].InnerText.Trim());

                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                            string.Format("Acknowledge define report result : {0}", ack));
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "DefineReport":
                              case "DefineReportAndOnline":
                                    if (_defineReport != null && _defineReport.Count != 0)
                                    {
                                          TS2F33_H_DefineReport(eqpno, agent, _defineReport[0].Item1, _defineReport[0].Item2, rtn, tid);
                                          _defineReport.RemoveAt(0);
                                    }
                                    else if (_linkEvent != null && _linkEvent.Count != 0)
                                    {
                                          TS2F35_H_LinkEventReport(eqpno, agent, _linkEvent[0].Item1, _linkEvent[0].Item2, rtn, tid);
                                          _linkEvent.RemoveAt(0);
                                    }
                                    else
                                    {
                                          //20160128 cy:改為Online成功才去定義S2F23
                                          //TS2F37_H_EnableDisableEventReport(eqpno, agent, true, _eventReport, "S2F23_UTILY", tid);
                                          if (rtn == "DefineReportAndOnline")
                                                TS2F37_H_EnableDisableEventReport(eqpno, agent, true, _eventReport, "AutoOnlineRequest", tid);
                                          else
                                                TS2F37_H_EnableDisableEventReport(eqpno, agent, true, _eventReport, "", tid);
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
            public void S2F36_E_LinkEvnetReportAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F36_E");
                  #region Handle Logic
                  try
                  {
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (node == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        string ack = ConvertDescriptionLRACK(recvTrx["secs"]["message"]["body"]["LRACK"].InnerText.Trim());

                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                            string.Format("Acknowledge link event report result : {0}", ack));
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "DefineReport":
                              case "DefineReportAndOnline":
                                    if (_defineReport != null && _defineReport.Count != 0)
                                    {
                                          TS2F33_H_DefineReport(eqpno, agent, _defineReport[0].Item1, _defineReport[0].Item2, rtn, tid);
                                          _defineReport.RemoveAt(0);
                                    }
                                    else if (_linkEvent != null && _linkEvent.Count != 0)
                                    {
                                          TS2F35_H_LinkEventReport(eqpno, agent, _linkEvent[0].Item1, _linkEvent[0].Item2, rtn, tid);
                                          _linkEvent.RemoveAt(0);
                                    }
                                    else
                                    {
                                          //20160128 cy:改為Online成功才去定義S2F23
                                          //TS2F37_H_EnableDisableEventReport(eqpno, agent, true, _eventReport, "S2F23_UTILY", tid);
                                          if (rtn == "DefineReportAndOnline")
                                                TS2F37_H_EnableDisableEventReport(eqpno, agent, true, _eventReport, "AutoOnlineRequest", tid);
                                          else
                                                TS2F37_H_EnableDisableEventReport(eqpno, agent, true, _eventReport, "", tid);
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
            public void S2F38_E_EnableDisableEvnetReportAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F38_E");
                  #region Handle Logic
                  try
                  {
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        string ack = ConvertDescriptionERACK(recvTrx["secs"]["message"]["body"]["ERACK"].InnerText.Trim());

                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                            string.Format("Acknowledge enable/disable event report result : {0}", ack));
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "DefineReport":
                              case "DefineReportAndOnline":
                                    if (_defineReport != null && _defineReport.Count != 0)
                                    {
                                          TS2F33_H_DefineReport(eqpno, agent, _defineReport[0].Item1, _defineReport[0].Item2, rtn, tid);
                                          _defineReport.RemoveAt(0);
                                    }
                                    else if (_linkEvent != null && _linkEvent.Count != 0)
                                    {
                                          TS2F35_H_LinkEventReport(eqpno, agent, _linkEvent[0].Item1, _linkEvent[0].Item2, rtn, tid);
                                          _linkEvent.RemoveAt(0);
                                    }
                                    else
                                    {
                                          //20160128 cy:改到Online後再做S2F23
                                          //TS2F37_H_EnableDisableEventReport(eqpno, agent, true, _eventReport, "S2F23_UTILY", tid);
                                          if(rtn ==  "DefineReportAndOnline")
                                                TS2F37_H_EnableDisableEventReport(eqpno, agent, true, _eventReport, "AutoOnlineRequest", tid);
                                          else
                                                TS2F37_H_EnableDisableEventReport(eqpno, agent, true, _eventReport, "", tid);
                                    }
                                    break;
                              case "AutoOnlineRequest":
                                    TS1F17_H_RequestOnLine(eqpno, agent, "AutoOnlineRequest", string.Empty);
                                    break;
                              case "S2F23_UTILY":
                                    if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "UTILY") != null)
                                          TS2F23_H_TraceInitializeSend(eqpno, agent, "UTILY", eqp.File.UtilityIntervalNK, "-1", "1", "S2F23_APCIM", tid);
                                    else if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "APCIM") != null)
                                          TS2F23_H_TraceInitializeSend(eqpno, agent, "APCIM", eqp.File.APCImportanIntervalNK, "-1", "1", "S2F23_APCNO", tid);
                                    else if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "APCNO") != null)
                                          TS2F23_H_TraceInitializeSend(eqpno, agent, "APCNO", eqp.File.APCNormalIntervalNK, "-1", "1", "S2F23_SPCAL", tid);
                                    else if (ObjectManager.EquipmentManager.GetVariableData(eqpno, "SPCAL") != null)
                                          TS2F23_H_TraceInitializeSend(eqpno, agent, "SPCAL", eqp.File.SpecialDataIntervalNK, "-1", "1", string.Empty, tid);

                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F40_E_MultiblockGrant(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F40_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        Equipment node = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (node == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        string grant = recvTrx["secs"]["message"]["body"]["GRANT"].InnerText.Trim();
                        switch (grant)
                        {
                              case "0":
                                    _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                        string.Format("Multi-block acknowledge : {0}:Permission granted.", grant));
                                    break;
                              case "1":
                                    _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                        string.Format("Multi-block acknowledge : {0}:Busy, retry.", grant));
                                    break;
                              case "2":
                                    _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                        string.Format("Multi-block acknowledge : {0}:Space insufficient.", grant));
                                    break;
                              case "3":
                                    _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                        string.Format("Multi-block acknowledge : {0}:DATAID duplication.", grant));
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F42_E_HostCommandAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F42_E");
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

                        string hcack = recvTrx["secs"]["message"]["body"]["array1"]["HCACK"].InnerText.Trim();
                        string hcackDes = ConvertDescriptionHCACK(hcack);

                        string ack = string.Format("HCACK({0}:{1})", hcack, hcackDes);
                        if (recvTrx["secs"]["message"]["body"]["array1"]["array2"].Attributes["len"].InnerText.Trim() != "0")
                        {
                              XmlNode ackNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                              string cpname = string.Empty;
                              string cpack = string.Empty;
                              string cpackDes = string.Empty;
                              while (ackNode != null)
                              {
                                    cpname = ackNode["CPNAME"].InnerText.Trim();
                                    cpack = ackNode["CPACK"].InnerText.Trim();
                                    cpackDes = "Other error";
                                    try
                                    {
                                          cpackDes = ConvertDescriptionCPACK(cpack);
                                    }
                                    catch { }
                                    ack = string.Format("{0} CPACK[{1}-{2}:{3}]", ack, cpname, cpack, cpackDes);
                                    ackNode = ackNode.NextSibling;
                              }
                        }
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, ack);

                        //check if opi request
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S2F42", string.Format("Host command send acknowledge. {0}", ack) });
                                    break;
                        }

                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S2F44_E_ResetSpoolingAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F44_E");
                  #region Handle Logic
                  try
                  {
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        string rtnid = recvTrx["secs"]["message"]["return"].Attributes["id"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        string rspack = recvTrx["secs"]["message"]["body"]["array1"]["RSPACK"].InnerText.Trim();
                        string msg = string.Format("Reset spooling stream and function acknowledgement.({0}:{1})", rspack, ConvertResetSpoolingRspAck(rspack));

                        if (rspack != "0" && recvTrx["secs"]["message"]["body"]["array1"]["array2"].Attributes["len"].InnerText.Trim() != "0")
                        {
                              string log = string.Empty;
                              XmlNode xArray2 = recvTrx["secs"]["message"]["body"]["array1"]["array2"].FirstChild;
                              while (xArray2 != null)
                              {
                                    string strid = xArray2["STRID"].InnerText.Trim();
                                    string strack = xArray2["STRACK"].InnerText.Trim();
                                    log = log + string.Format("|{0}:{1}", ConvertResetSpoolingStrAck(strack), strid);
                                    if (xArray2["array4"].Attributes["len"].InnerText.Trim() != "0")
                                    {
                                          List<string> fcn = new List<string>();
                                          XmlNode xArray4 = xArray2["array4"].FirstChild;
                                          while (xArray4 != null)
                                          {
                                                string fcnid = xArray4["FCNID"].InnerText.Trim();
                                                fcn.Add(fcnid);
                                                xArray4 = xArray4.NextSibling;
                                          }
                                          if (fcn.Count > 0)
                                                log = log + string.Format(":{0}", string.Join(",", fcn.ToArray()));
                                    }
                                    xArray2 = xArray2.NextSibling;
                              }
                              msg = msg + log;
                        }

                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, msg);

                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S6F24", msg });
                                    break;
                        }
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }

            #endregion

            #region Send by host-S2
            public void TS2F13_H_EquipmentConstantRequest(string eqpno, string eqpid, uint ecid, string tag, string trxid)
            {
                  //*NOTE:發這個時,要在body的keyvalue填入問的是哪一個ECID
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F13_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F13_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"].Attributes["keyvalue"].InnerText = ecid.ToString();
                        sendTrx["secs"]["message"]["body"]["array1"]["ECID"].InnerText = ecid.ToString();
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F13_H";
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
            public void TS2F15_501_H_ECID501NewEquipmentConstantSend(string eqpno, string eqpid, uint ecv, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F15_501_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F15_501_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        //此method特殊處理，因S2F16不會有多種，所以把key清空
                        sendTrx["secs"]["message"]["body"].Attributes["keyname"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"].Attributes["keyvalue"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"]["array1"]["array2"]["ECID"].InnerText = "501";
                        sendTrx["secs"]["message"]["body"]["array1"]["array2"]["ECV"].InnerText = ecv.ToString();
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F15_501_H";
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
            public void TS2F15_503_H_ECID503NewEquipmentConstantSend(string eqpno, string eqpid, ushort ecv, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F15_503_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F15_503_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        //此method特殊處理，因S2F16不會有多種，所以把key清空
                        sendTrx["secs"]["message"]["body"].Attributes["keyname"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"].Attributes["keyvalue"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"]["array1"]["array2"]["ECID"].InnerText = "503";
                        sendTrx["secs"]["message"]["body"]["array1"]["array2"]["ECV"].InnerText = ecv.ToString();
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F15_503_H";
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
            public void TS2F15_505_H_ECID505NewEquipmentConstantSend(string eqpno, string eqpid, uint ecv, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F15_505_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F15_505_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        //此method特殊處理，因S2F16不會有多種，所以把key清空
                        sendTrx["secs"]["message"]["body"].Attributes["keyname"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"].Attributes["keyvalue"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"]["array1"]["array2"]["ECID"].InnerText = "505";
                        sendTrx["secs"]["message"]["body"]["array1"]["array2"]["ECV"].InnerText = ecv.ToString();
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F15_505_H";
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
            public void TS2F15_506_H_ECID506NewEquipmentConstantSend(string eqpno, string eqpid, uint ecv, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F15_506_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F15_506_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        //此method特殊處理，因S2F16不會有多種，所以把key清空
                        sendTrx["secs"]["message"]["body"].Attributes["keyname"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"].Attributes["keyvalue"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"]["array1"]["array2"]["ECID"].InnerText = "506";
                        sendTrx["secs"]["message"]["body"]["array1"]["array2"]["ECV"].InnerText = ecv.ToString();
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F15_506_H";
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
            public void TS2F15_507_H_ECID507NewEquipmentConstantSend(string eqpno, string eqpid, bool ecv, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F15_507_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F15_507_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        //此method特殊處理，因S2F16不會有多種，所以把key清空
                        sendTrx["secs"]["message"]["body"].Attributes["keyname"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"].Attributes["keyvalue"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"]["array1"]["array2"]["ECID"].InnerText = "507";
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["array2"]["ECV"], ecv.ToString());
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F15_507_H";
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
            public void TS2F15_508_H_ECID508NewEquipmentConstantSend(string eqpno, string eqpid, uint ecv, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F15_508_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F15_508_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        //此method特殊處理，因S2F16不會有多種，所以把key清空
                        sendTrx["secs"]["message"]["body"].Attributes["keyname"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"].Attributes["keyvalue"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"]["array1"]["array2"]["ECID"].InnerText = "508";
                        sendTrx["secs"]["message"]["body"]["array1"]["array2"]["ECV"].InnerText = ecv.ToString();
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F15_508_H";
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
            public void TS2F15_509_H_ECID509NewEquipmentConstantSend(string eqpno, string eqpid, uint ecv, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F15_509_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F15_509_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        //此method特殊處理，因S2F16不會有多種，所以把key清空
                        sendTrx["secs"]["message"]["body"].Attributes["keyname"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"].Attributes["keyvalue"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"]["array1"]["array2"]["ECID"].InnerText = "509";
                        sendTrx["secs"]["message"]["body"]["array1"]["array2"]["ECV"].InnerText = ecv.ToString();
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F15_509_H";
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
            public void TS2F15_510_H_ECID510NewEquipmentConstantSend(string eqpno, string eqpid, uint ecv, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F15_510_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F15_510_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        //此method特殊處理，因S2F16不會有多種，所以把key清空
                        sendTrx["secs"]["message"]["body"].Attributes["keyname"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"].Attributes["keyvalue"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"]["array1"]["array2"]["ECID"].InnerText = "510";
                        sendTrx["secs"]["message"]["body"]["array1"]["array2"]["ECV"].InnerText = ecv.ToString();
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F15_510_H";
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
            public void TS2F17_H_DateandTimeRequest(string eqpno, string eqpid, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F17_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F17_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F17_H";
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
            public void TS2F18_H_DateandTimeData(string eqpno, string eqpid, string tid, string sysbytes)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F18_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F18_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = tid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = sysbytes;
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["TIME"], DateTime.Now.ToString("yyyyMMddHHmmss"));
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
            public void TS2F23_H_TraceInitializeSend(string eqpno, string eqpid,
                string trid, string dsper, string totsmp, string repgsz, string tag, string trxid)
            {
                  try
                  {
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, trxid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        if (dsper == null)
                              dsper = "000030";
                        //檢查DSPER格式為hhmmss
                        if (dsper.Length != 6 || (dsper.Substring(0, 1) == "2" ? !Regex.IsMatch(dsper, @"[0-2][0-4][0-5]\d[0-5]\d") : !Regex.IsMatch(dsper, @"[0-2]\d[0-5]\d[0-5]\d")))
                        {
                              string msg = string.Format("Parameter of DSPER({0}) format error. Format:hhmmss", dsper);
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, trxid, msg);
                              if (tag == "OPI")
                              {
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { trxid, eqp.Data.LINEID, "Send S2F23 Error", msg });
                              }
                              return;
                        }

                        List<string> svid = new List<string>();

                        if (totsmp != "0") //TOTSMP = 0 means terminate trace.
                        {
                            ObjectManager.EquipmentManager.ReloadSECSVariableDataByEqpNo(eqpno);
                            List<SECSVARIABLEDATA> data = ObjectManager.EquipmentManager.GetVariableData(eqpno, trid);
                            List<Tuple<string, string, string>> items = new List<Tuple<string, string, string>>();

                            // add by box.zhai 如果DB没有维护，会报Exception，先判断Data 是否为空，如SVID给空，即跟EQ要所有SVID的Value
                            if (data == null)
                            {
                                data = new List<SECSVARIABLEDATA>();
                            }
                            
                            foreach (SECSVARIABLEDATA sv in data)
                            {
                                if (sv.ITEMSET == "1")
                                {
                                    svid.Add(sv.ITEMID);
                                    items.Add(Tuple.Create(sv.ITEMID, sv.ITEMNAME, sv.ITEMTYPE));
                                }
                            }

                            _traceData.AddOrUpdate(trid, items,
                                            (key, existingVal) =>
                                            {
                                                return items;
                                            });
                              //重新設置間隔時間
                              switch (trid)
                              {
                                    case "UTILY":
                                          eqp.File.UtilityIntervalNK = dsper;
                                          break;
                                    case "APCIM":
                                          eqp.File.APCImportanIntervalNK = dsper;
                                          break;
                                    case "APCNO":
                                          eqp.File.APCNormalIntervalNK = dsper;
                                          break;
                                    case "SPCAL":
                                          eqp.File.SpecialDataIntervalNK = dsper;
                                          break;
                              }
                        }

                        TS2F23_H_TraceInitializeSend(eqpno, eqpid, trid, dsper, totsmp, repgsz, svid, tag, trxid);

                        //20150710 cy:重新設置倉庫
                        switch (trid)
                        {
                              case "UTILY":
                                    eqp.File.UtilityEnableNK = (!totsmp.Equals("0"));
                                    if (!eqp.File.UtilityEnableNK)
                                    {
                                          string key = string.Format("{0}_{1}_SecsDailyCheck", eqp.Data.LINEID, eqp.Data.NODEID);
                                          Repository.Remove(key);
                                    }
                                    break;
                              case "APCIM":
                                    eqp.File.APCImportanEnable = (!totsmp.Equals("0"));
                                    if (!eqp.File.APCImportanEnable)
                                    {
                                          string key = string.Format("{0}_{1}_SecsAPCImportantDataByReq", eqp.Data.LINEID, eqp.Data.NODEID);
                                          Repository.Remove(key);
                                    }
                                    break;
                              case "APCNO":
                                    eqp.File.APCNormalEnable = (!totsmp.Equals("0"));
                                    if (!eqp.File.APCNormalEnable)
                                    {
                                          string key = string.Format("{0}_{1}_SecsAPCNormalDataByReq", eqp.Data.LINEID, eqp.Data.NODEID);
                                          Repository.Remove(key);
                                    }
                                    break;
                              case "SPCAL":
                                    eqp.File.SpecialDataEnable = (!totsmp.Equals("0"));
                                    if (!eqp.File.SpecialDataEnable)
                                    {
                                          string key = string.Format("{0}_{1}_SecsSpecialDataReq", eqp.Data.LINEID, eqp.Data.NODEID);
                                          Repository.Remove(key);
                                    }
                                    break;
                        }

                        ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
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
            public void TS2F23_H_TraceInitializeSend(string eqpno, string eqpid,
                string trid, string dsper, string totsmp, string repgsz, List<string> svid, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F23_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F23_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["TRID"], trid);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["DSPER"], dsper);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["TOTSMP"], totsmp);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["REPGSZ"], repgsz);
                        XmlNode xNode = sendTrx["secs"]["message"]["body"]["array1"]["array2"];
                        int loop = (svid != null && svid.Count > 0) ? svid.Count : 0;
                        xNode.Attributes["len"].InnerText = loop.ToString();
                        if (loop == 0)
                              xNode.RemoveChild(xNode.FirstChild);
                        for (int i0 = 0; i0 < loop; i0++)
                        {
                              _common.CloneChildNode(xNode, i0);
                        }
                        for (int i1 = 0; i1 < loop; i1++)
                        {
                              _common.SetItemData(xNode.ChildNodes[i1], svid[i1]);
                        }
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = string.Format("S2F23_{0}_H", trid);
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
            public void TS2F25_H_LoopbackDiagnosticRequest(string eqpno, string eqpid, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F25_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F25_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        string abs = string.Format("{0:X} {1:X} {2:X} {3:X} {4:X} {5:X} {6:X} {7:X} {8:X} {9:X}",
                                "2", "25", _rnd.Next(0, 254), _rnd.Next(0, 254), _rnd.Next(0, 254),
                                _rnd.Next(0, 254), _rnd.Next(0, 254), _rnd.Next(0, 254), _rnd.Next(0, 254), _rnd.Next(0, 254));
                        sendTrx["secs"]["message"]["body"]["ABS"].InnerText = abs;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F25_H";
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
            public void TS2F26_H_LoopbackDiagnosticData(string eqpno, string eqpid, string tid, string sysbytes, string abs)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F26_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F26_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = tid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = sysbytes;
                        sendTrx["secs"]["message"]["body"]["ABS"].InnerText = abs;
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
            public void TS2F29_H_EquipmentConstantNamelistRequest(string eqpno, string eqpid, uint ecid, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F29_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F29_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"].Attributes["keyname"].InnerText = "ECID";
                        sendTrx["secs"]["message"]["body"].Attributes["keyvalue"].InnerText = ecid.ToString();
                        sendTrx["secs"]["message"]["body"]["array1"]["ECID"].InnerText = ecid.ToString();
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F29_H";
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
            public void TS2F31_H_DateandTimeSetRequest(string eqpno, string eqpid, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F31_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F31_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["TIME"], DateTime.Now.ToString("yyyyMMddHHmmss"));
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F31_H";
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
            public void TS2F33_H_DefineReport(string eqpno, string eqpid, uint rptid, List<uint> vid, string tag, string trxid)
            {
                  try
                  {
                        if (vid == null || vid.Count <= 0)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "There is no data in VID collection.");
                              return;
                        }
                        //Get Agent Object
                        IServerAgent agent = GetServerAgent(eqpid);
                        if (agent == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("Can not get agent object with name ({0}]", eqpid));
                              return;
                        }
                        //Get Transaction Format
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F33_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F33_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"]["array1"]["DATAID"].InnerText = GetDataID().ToString();
                        sendTrx["secs"]["message"]["body"]["array1"]["array2"].Attributes["len"].InnerText = "1";
                        sendTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["RPTID"].InnerText = rptid.ToString();
                        XmlNode xNode = sendTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"];
                        xNode.Attributes["len"].InnerText = vid.Count.ToString();
                        for (int i0 = 0; i0 < vid.Count; i0++)
                        {
                              _common.CloneChildNode(xNode, i0);
                              xNode.ChildNodes[i0].InnerText = vid[i0].ToString();
                        }
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F33_H";
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
            public void TS2F35_H_LinkEventReport(string eqpno, string eqpid, uint ceid, List<uint> rptid, string tag, string trxid)
            {
                  try
                  {
                        if (rptid == null || rptid.Count <= 0)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "There is no data in RPTID collection.");
                              return;
                        }
                        //Get Agent Object
                        IServerAgent agent = GetServerAgent(eqpid);
                        if (agent == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("Can not get agent object with name ({0}]", eqpid));
                              return;
                        }
                        //Get Transaction Format
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F35_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F35_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"]["array1"]["DATAID"].InnerText = GetDataID().ToString();
                        sendTrx["secs"]["message"]["body"]["array1"]["array2"].Attributes["len"].InnerText = "1";
                        sendTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["CEID"].InnerText = ceid.ToString();
                        XmlNode xNode = sendTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["array4"];
                        xNode.Attributes["len"].InnerText = rptid.Count.ToString();
                        for (int i0 = 0; i0 < rptid.Count; i0++)
                        {
                              _common.CloneChildNode(xNode, i0);
                              xNode.ChildNodes[i0].InnerText = rptid[i0].ToString();
                        }
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F35_H";
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
            public void TS2F37_H_EnableDisableEventReport(string eqpno, string eqpid, bool ceed, List<uint> ceid, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F37_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F37_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"]["array1"]["CEED"].InnerText = ceed ? "1" : "0";
                        XmlNode xNode = sendTrx["secs"]["message"]["body"]["array1"]["array2"];
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F37_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;
                        if (ceid == null || ceid.Count == 0)
                        {
                              xNode.Attributes["len"].InnerText = "0";
                              xNode.RemoveChild(xNode.FirstChild);
                        }
                        else
                        {
                              xNode.Attributes["len"].InnerText = ceid.Count.ToString();
                              for (int i0 = 0; i0 < ceid.Count; i0++)
                              {
                                    _common.CloneChildNode(xNode, i0);
                                    xNode.ChildNodes[i0].InnerText = ceid[i0].ToString();
                              }
                        }
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
            public void TS2F39_H_MultiblockInquire(string eqpno, string eqpid, uint dataid, uint datalen, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F39_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F39_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"]["array1"]["DATAID"].InnerText = dataid.ToString();
                        sendTrx["secs"]["message"]["body"]["array1"]["DATALENGTH"].InnerText = datalen.ToString();
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F39_H";
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
            public void TS2F41_STOP_H_STOPHostCommandSend(string eqpno, string eqpid, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F41_STOP_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F41_STOP_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        //此method特殊處理，因S2F42不會有多種，所以把key清空
                        sendTrx["secs"]["message"]["body"].Attributes["keyname"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"].Attributes["keyvalue"].InnerText = string.Empty;
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["RCMD"], "STOP");
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F41_H_STOP";
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
            public void TS2F41_ABORT_H_ABORTHostCommandSend(string eqpno, string eqpid, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F41_ABORT_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F41_ABORT_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        //此method特殊處理，因S2F42不會有多種，所以把key清空
                        sendTrx["secs"]["message"]["body"].Attributes["keyname"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"].Attributes["keyvalue"].InnerText = string.Empty;
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["RCMD"], "ABORT");
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F41_H_ABORT";
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
            public void TS2F43_H_ResetSpoolingStreamsandFunctions(string eqpno, string eqpid, Dictionary<byte, List<byte>> rssf, string tag, string trxid)
            {
                  try
                  {
                        //Get Agent Object
                        IServerAgent agent = GetServerAgent(eqpid);
                        if (agent == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("Can not get agent object with name ({0})", eqpid));
                              return;
                        }
                        //Get Transaction Format
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F43_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F43_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = base.CreateTrxID();
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F43_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;
                        //<body keyname="" keyvalue="" keyposition="0">
                        //  <array1 name="List" type="L" len="?">
                        //    <array2 name="List" type="L" len="2">
                        //      <STRID name="STRID" type="U1" len="1" fixlen="False" />
                        //      <array3 name="List" type="L" len="?">
                        //        <FCNID name="FCNID" type="U1" len="1" fixlen="False" />
                        //      </array3>
                        //    </array2>
                        //  </array1>
                        //</body>
                        XmlNode xNode1 = sendTrx["secs"]["message"]["body"]["array1"];
                        int loop1 = 0;
                        if (rssf != null)
                        {
                              loop1 = rssf.Count;
                        }
                        xNode1.Attributes["len"].InnerText = loop1.ToString();
                        if (loop1 == 0)
                        {
                              xNode1.RemoveChild(xNode1["array2"]);
                        }
                        for (int i = 0; i < loop1; i++)
                        {
                              _common.CloneChildNode(xNode1, i);
                        }
                        int index = 0;
                        foreach (byte st in rssf.Keys)
                        {
                              _common.SetItemData(xNode1.ChildNodes[index]["STRID"], st.ToString());
                              XmlNode xNode3 = xNode1.ChildNodes[index]["array3"];
                              int loop3 = 0;
                              List<byte> items = rssf[st];
                              if (items != null)
                              {
                                    loop3 = items.Count;
                              }
                              xNode3.Attributes["len"].InnerText = loop3.ToString();
                              if (loop3 == 0)
                              {
                                    xNode3.RemoveChild(xNode3["FCNID"]);
                              }
                              for (int j = 0; j < loop3; j++)
                              {
                                    _common.CloneChildNode(xNode3, j);
                              }
                              for (int j = 0; j < loop3; j++)
                              {
                                    //add dataname
                                    _common.SetItemData(xNode3.ChildNodes[j], items[j].ToString());
                              }
                              index++;
                        }

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

            #region Sent form host-S2
            public void S2F13_H_EquipmentConstantRequest(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F13_H T3-Timeout", false);
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
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S2F13", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F13_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F15_501_H_ECID501NewEquipmentConstantSend(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F15_501_H T3-Timeout", false);
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
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S2F15_501", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F15_501_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F15_503_H_ECID503NewEquipmentConstantSend(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F15_503_H T3-Timeout", false);
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
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S2F15_503", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F15_503_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F15_505_H_ECID505NewEquipmentConstantSend(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F15_505_H T3-Timeout", false);
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
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S2F15_505", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F15_505_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F15_506_H_ECID506NewEquipmentConstantSend(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F15_506_H T3-Timeout", false);
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
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S2F15_506", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F15_506_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F15_507_H_ECID507NewEquipmentConstantSend(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F15_507_H T3-Timeout", false);
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
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S2F15_507", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F15_507_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F15_508_H_ECID508NewEquipmentConstantSend(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F15_508_H T3-Timeout", false);
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
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S2F15_508", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F15_508_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F15_509_H_ECID509NewEquipmentConstantSend(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F15_509_H T3-Timeout", false);
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
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S2F15_509", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F15_509_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F15_510_H_ECID510NewEquipmentConstantSend(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F15_510_H T3-Timeout", false);
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
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S2F15_510", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F15_510_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F17_H_DateandTimeRequest(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F17_H T3-Timeout", false);
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
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S2F17", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F17_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F18_H_DateandTimeData(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F18_H T3-Timeout", false);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F18_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F23_H_TraceInitializeSend(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F23_H T3-Timeout", false);
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
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S2F23", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F23_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F25_H_LoopbackDiagnosticRequest(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F25_H T3-Timeout", false);
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
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S2F25", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F25_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F26_H_LoopbackDiagnosticData(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F26_H T3-Timeout", false);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F26_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F29_H_EquipmentConstantNamelistRequest(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F29_H T3-Timeout", false);
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
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S2F29", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F29_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F31_H_DateandTimeSetRequest(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F31_H T3-Timeout", false);
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
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S2F31", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F31_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F33_H_DefineReport(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F33_H T3-Timeout", false);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F33_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F35_H_LinkEventReport(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F35_H T3-Timeout", false);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F35_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F37_H_EnableDisableEventReport(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F37_H T3-Timeout", false);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F37_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F39_H_MultiblockInquire(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F39_H T3-Timeout", false);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F39_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F41_STOP_H_STOPHostCommandSend(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F41_STOP_H T3-Timeout", false);
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
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S2F41", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F41_STOP_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F41_ABORT_H_ABORTHostCommandSend(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F41_ABORT_H T3-Timeout", false);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F41_ABORT_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F43_H_ResetSpoolingStreamsandFunctions(XmlDocument recvTrx, bool timeout)
            {
                  if (timeout)
                  {
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F43_H T3-Timeout");
                        return;
                  }
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F43_H");
            }

            #endregion
      }
}