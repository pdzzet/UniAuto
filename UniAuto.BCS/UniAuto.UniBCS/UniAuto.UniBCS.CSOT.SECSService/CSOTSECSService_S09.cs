using System;
using System.Reflection;
using System.Xml;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Log;
using System.Linq;

namespace UniAuto.UniBCS.CSOT.SECSService
{
      public partial class CSOTSECSService
      {
            #region Recipte form equipment-S9
            public void S9F1_E_UnrecognizedDeviceID(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S9F1_E");
                  try
                  {
                        #region Handle Logic
                        //Logic handle.
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = HeaderToSystemBytes(recvTrx["secs"]["message"]["body"]["MHEAD"].InnerText.Trim());
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                    string.Format("Equipment indicate an unrecognized device ID. Systembytes of error message is ({0}) From agent({1}).",
                                        sysbytes, recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim()));
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S9F3_E_UnrecognizedStreamType(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S9F3_E");
                  try
                  {
                        #region Handle Logic
                        //Logic handle.
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = HeaderToSystemBytes(recvTrx["secs"]["message"]["body"]["MHEAD"].InnerText.Trim());
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                    string.Format("Equipment indicate an unrecognized stream type. Systembytes of error message is ({0}) From agent.({1}]",
                                        sysbytes, recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim()));
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S9F5_E_UnrecognizedFunctionType(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S9F5_E");
                  try
                  {
                        #region Handle Logic
                        //Logic handle.
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = HeaderToSystemBytes(recvTrx["secs"]["message"]["body"]["MHEAD"].InnerText.Trim());
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                    string.Format("Equipment indicate an unrecognized function type. Systembytes of error message is ({0}) From agent ({1}).",
                                        sysbytes, recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim()));
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S9F7_E_IllegalData(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S9F7_E");
                  try
                  {
                        #region Handle Logic
                        //Logic handle.
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = HeaderToSystemBytes(recvTrx["secs"]["message"]["body"]["MHEAD"].InnerText.Trim());
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                    string.Format("Equipment indicate an illegal data. Systembytes of error message is ({0}) From agent ({1}).",
                                        sysbytes, recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim()));
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S9F9_E_TransactionTimerTimeout(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S9F9_E");
                  try
                  {
                        #region Handle Logic
                        //Logic handle.
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = HeaderToSystemBytes(recvTrx["secs"]["message"]["body"]["SHEAD"].InnerText.Trim());
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                    string.Format("Equipment indicate a transaction time-out. Systembytes of error message is ({0}) From agent ({1}).",
                                        sysbytes, recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim()));
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S9F11_E_DataTooLong(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S9F11_E");
                  try
                  {
                        #region Handle Logic
                        //Logic handle.
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = HeaderToSystemBytes(recvTrx["secs"]["message"]["body"]["MHEAD"].InnerText.Trim());
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                    string.Format("Equipment indicate a data too long. Systembytes of error message is ({0}) From agent ({1}).",
                                        sysbytes, recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim()));
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region Recipte form Host-S9
            public void S9F1_H_UnrecognizedDeviceID(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "UnrecognizedDeviceID");
                  try
                  {
                        #region Handle Logic
                        //Logic handle.
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = HeaderToSystemBytes(recvTrx["secs"]["message"]["header"].InnerText.Trim());
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                    string.Format("Host check an unrecognized device ID. Systembytes of error message is ({0}) From agent({1}).",
                                        sysbytes, recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim()));
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S9F3_H_UnrecognizedStreamType(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "UnrecognizedStreamType");
                  try
                  {
                        #region Handle Logic
                        //Logic handle.
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = HeaderToSystemBytes(recvTrx["secs"]["message"]["header"].InnerText.Trim());
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                    string.Format("Host check an unrecognized stream type. Systembytes of error message is ({0}) From agent.({1}]",
                                        sysbytes, recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim()));
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S9F5_H_UnrecognizedFunctionType(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "UnrecognizedFunctionType");
                  try
                  {
                        #region Handle Logic
                        //Logic handle.
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = HeaderToSystemBytes(recvTrx["secs"]["message"]["header"].InnerText.Trim());
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                    string.Format("Host check an unrecognized function type. Systembytes of error message is ({0}) From agent ({1}).",
                                        sysbytes, recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim()));
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S9F7_H_IllegalData(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "IllegalData");
                  try
                  {
                        #region Handle Logic
                        //Logic handle.
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = HeaderToSystemBytes(recvTrx["secs"]["message"]["header"].InnerText.Trim());
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                    string.Format("Host check an illegal data. Systembytes of error message is ({0}) From agent ({1}).",
                                        sysbytes, recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim()));
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S9F9_H_TransactionTimerTimeout(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "TransactionTimerTimeout");
                  try
                  {
                        #region Handle Logic
                        //Logic handle.
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = HeaderToSystemBytes(recvTrx["secs"]["message"]["header"].InnerText.Trim());
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                    string.Format("Host check a transaction time-out. Systembytes of error message is ({0}) From agent ({1}).",
                                        sysbytes, recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim()));
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S9F11_H_DataTooLong(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "DataTooLong");
                  try
                  {
                        #region Handle Logic
                        //Logic handle.
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = HeaderToSystemBytes(recvTrx["secs"]["message"]["header"].InnerText.Trim());
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                    string.Format("Host check a data too long. Systembytes of error message is ({0}) From agent ({1}).",
                                        sysbytes, recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim()));
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            private string HeaderToSystemBytes(string head)
            {
                  if (string.IsNullOrEmpty(head))
                        return string.Empty;
                  try
                  {
                        byte[] ba = head.Split(' ').Select(x => Convert.ToByte(x, 16)).ToArray();
                        if (ba.Length < 10)
                              return string.Empty;
                        string binary = Convert.ToString(ba[6], 2).PadLeft(8, '0') +
                                        Convert.ToString(ba[7], 2).PadLeft(8, '0') +
                                        Convert.ToString(ba[8], 2).PadLeft(8, '0') +
                                        Convert.ToString(ba[9], 2).PadLeft(8, '0');
                        return Convert.ToUInt32(binary, 2).ToString();
                  }
                  catch { return string.Empty; }
            }
      }
}