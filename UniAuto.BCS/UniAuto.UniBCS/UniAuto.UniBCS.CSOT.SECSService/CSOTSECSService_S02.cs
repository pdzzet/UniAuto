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
using System.Linq;

namespace UniAuto.UniBCS.CSOT.SECSService
{
      public partial class CSOTSECSService
      {
            #region Recipte form equipment-S2
            public void S2F0_E_AbortTransaction(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F0_E");
                  #region Handle Logic
                  try
                  {
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }

                        string rtnID = recvTrx["secs"]["message"]["return"].Attributes["id"].InnerText.Trim();
                        if (string.IsNullOrEmpty(rtnID))
                        {
                              return;
                        }
                        switch (rtnID)
                        {
                              case "S2F31_H":
                              case "S2F41_H":
                              case "S2F111_H":
                                    return; //skip opi request check
                        }

                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S2F0", _common.ToFormatString(recvTrx.OuterXml) });
                                    break;
                        }
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

                        //<EAC name="EAC" type="B" len="1" fixlen="False" />
                        //body
                        string eac = recvTrx["secs"]["message"]["body"]["EAC"].InnerText.Trim();

                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S2F16", _common.ToFormatString(recvTrx.OuterXml) });
                                    break;
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F17_E_DateandTimeRequest(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F17_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //reply secondary
                        TS2F18_H_DateandTimeData(eqpno, agent, tid, sysbytes);
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F20_E_DataSetCommandAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F20_E");
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

                        //<ACK name="ACK" type="B" len="1" fixlen="False" />
                        //body
                        string ack = recvTrx["secs"]["message"]["body"]["ACK"].InnerText.Trim();

                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S2F20", ack == "0" ? "Reply S2F20 OK, Accepted." : "Reply S2F20 NG, Not permit." });
                                    break;
                              case "AutoRequest":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S2F20", ack == "0" ? "Reply S2F20 OK, Accepted." : "Reply S2F20 NG, Not permit." });
                                    break;
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F22_E_DataSetCommandforIDAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F22_E");
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

                        //<ACK name="ACK" type="B" len="1" fixlen="False" />
                        //body
                        string ack = recvTrx["secs"]["message"]["body"]["ACK"].InnerText.Trim();

                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S2F22", ack == "0" ? "Reply S2F22 OK, Accepted." : "Reply S2F22 NG, Not permit." });
                                    break;
                            case"AutoRequest":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S2F22", ack == "0" ? "Reply S2F22 OK, Accepted." : "Reply S2F22 NG, Not permit." });
                                    break;
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F24_E_DataItemMappingTable(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F24_E");
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
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                  "Can not find LineID in LineEntity!");
                              return;
                        }
                        //<array1 name="List" type="L" len="?">
                        //  <array2 name="List" type="L" len="2">
                        //    <DATANAME name="DATANAME" type="A" len="30" fixlen="False" />
                        //    <DATAID name="DATAID" type="A" len="30" fixlen="False" />
                        //  </array2>
                        //</array1>
                        //body

                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S2F24", _common.ToFormatString(recvTrx.OuterXml) });
                                    break;
                              case "ONLINE":
                                    IList<DailyCheckData> dataFormats_Old = (ObjectManager.DailyCheckManager.GetDailyCheckProfile(eqp.Data.NODENO)==null)
                                        ? new List<DailyCheckData>() : ObjectManager.DailyCheckManager.GetDailyCheckProfile(eqp.Data.NODENO);
                                    IList<DailyCheckData> dataFormats_New = new List<DailyCheckData>();
                                    string log = string.Empty;
                                    string len = recvTrx["secs"]["message"]["body"]["array1"].Attributes["len"].InnerText.Trim();
                                    int loop = 0;
                                    int.TryParse(len, out loop);
                                    for (int i = 0; i < loop; i++)
                                    {
                                        XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"].ChildNodes[i];
                                        string dataname = xNode["DATANAME"].InnerText.Trim();
                                        string dataid = xNode["DATAID"].InnerText.Trim();
                                        #region add to new object
                                        DailyCheckData dcData = null;
                                        //檢查新資料中,是否有重複
                                        if (dataFormats_New != null)  //add by yang 2016/11/26
                                            dcData = dataFormats_New.FirstOrDefault(d => d.Data.SVID == dataid);
                                        if (dcData != null)
                                        {
                                            //存在重覆資料
                                            dataFormats_New.Remove(dcData);
                                            log = string.Format("{0}(Duplicate ID={1})", log, dataid);
                                        }
                                        else
                                        {
                                            if (dataFormats_Old != null)
                                                //  {                                                  
                                                dcData = dataFormats_Old.FirstOrDefault(d => d.Data.SVID == dataid);
                                            if (dcData == null)
                                            {
                                                //收到的id不存在原始table中
                                                log = string.Format("{0}(Add ID={1},Name={2})", log, dataid, dataname);
                                            }
                                            else
                                            {
                                                if (!dcData.Data.PARAMETERNAME.Equals(dataname))
                                                {
                                                    //收到的name與原始table不符
                                                    dcData.Data.PARAMETERNAME = dataname;
                                                    log = string.Format("{0}(Change ID={1},Name={2}=>{3})", log, dataid, dcData.Data.PARAMETERNAME, dataname);
                                                }

                                            }
                                            // }
                                        }
                                        //檢查新資料的name是否重覆,還是加入,但發訊息到OPI
                                        if (dataFormats_New != null)  //add by yang 2016/11/26
                                            dcData = dataFormats_New.FirstOrDefault(d => d.Data.PARAMETERNAME == dataname);
                                        if (dcData != null)
                                        {
                                            log = string.Format("{0}(Duplicate Name={1} with ID={2},{3})", log, dataname, dataid, dcData.Data.SVID);
                                            Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                                  new object[4] { tid, eqp.Data.LINEID, "Data Item Mapping Error", 
                                                            string.Format("Equipment({0}) data item mapping duplicate name({1}) with ID({2},{3})", eqp.Data.NODEID, dataname, dataid, dcData.Data.SVID) });
                                            TS10F3_H_TerminalDisplaySingle(eqpno, agent, string.Format("Equipment({0}) data item mapping duplicate name({1}) with ID({2},{3})", eqp.Data.NODEID, dataname, dataid, dcData.Data.SVID), tid, string.Empty);
                                        }

                                        DailyCheckEntityData data = new DailyCheckEntityData();
                                        {
                                            data.LINETYPE = line.Data.LINETYPE;
                                            data.LINEID = line.Data.LINEID;
                                            data.SERVERNAME = line.Data.SERVERNAME;
                                            data.NODENO = eqp.Data.NODENO;
                                            data.SVID = dataid;
                                            data.PARAMETERNAME = dataname;                                           
                                        }
                                        dcData = new DailyCheckData(data);
                                        dataFormats_New.Add(dcData);
                                        #endregion
                                    }
                                    #region check data changed
                                    if (dataFormats_Old != null && dataFormats_New != null)
                                    {
                                          for (int i = 0; i < dataFormats_Old.Count; i++)
                                          {
                                                DailyCheckData dcData = dataFormats_New.FirstOrDefault(d => d.Data.SVID == dataFormats_Old[i].Data.SVID);
                                                if (dcData == null)
                                                {
                                                      log = string.Format("{0}(Del ID={1},Name={2})", log, dataFormats_Old[i].Data.SVID, dataFormats_Old[i].Data.PARAMETERNAME);
                                                }
                                          }
                                    }
                                    //delete old setting
                                    ObjectManager.DailyCheckManager.DeleteDB(dataFormats_Old);
                                    //insert new setting
                                    ObjectManager.DailyCheckManager.InsertDB(dataFormats_New);
                                    //reload by node
                                    ObjectManager.DailyCheckManager.ReloadByNo(eqp.Data.NODENO);
                                    if(!string.IsNullOrEmpty(log))
                                          _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                "Data item mapping check error:" + log);
                                    #endregion
                                    break;
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F25_E_LoopbackDiagnosticRequest(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F25_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();

                        //reply secondary
                        TS2F26_H_LoopbackDiagnosticData(eqpno, agent, tid, sysbytes,
                                                 recvTrx["secs"]["message"]["body"]["ABS"].InnerText.Trim());
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
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
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S2F26", _common.ToFormatString(recvTrx.OuterXml) });
                                    break;
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F30_E_EquipmentConstantNamelist(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F30_E");
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

                        //<array1 name="List" type="L" len="?">
                        //  <array2 name="List" type="L" len="6">
                        //    <ECID name="ECID" type="U4" len="1" fixlen="False" />
                        //    <ECNAME name="ECNAME" type="A" len="16" fixlen="False" />
                        //    <ECMIN name="ECMIN" type="A" len="16" fixlen="False" />
                        //    <ECMAX name="ECMAX" type="A" len="16" fixlen="False" />
                        //    <ECDEF name="ECDEF" type="A" len="16" fixlen="False" />
                        //    <UNITS name="UNITS" type="A" len="16" fixlen="False" />
                        //  </array2>
                        //</array1>
                        //body
                        string len = recvTrx["secs"]["message"]["body"]["array1"].Attributes["len"].InnerText.Trim();
                        int loop = 0;
                        int.TryParse(len, out loop);
                        for (int i = 0; i < loop; i++)
                        {
                              XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"].ChildNodes[i];
                              string ecid = xNode["ECID"].InnerText.Trim();
                              string ecname = xNode["ECNAME"].InnerText.Trim();
                              string ecmin = xNode["ECMIN"].InnerText.Trim();
                              string ecmax = xNode["ECMAX"].InnerText.Trim();
                              string ecdef = xNode["ECDEF"].InnerText.Trim();
                              string units = xNode["UNITS"].InnerText.Trim();
                        }

                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S2F30", _common.ToFormatString(recvTrx.OuterXml) });
                                    break;
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F32_E_DateandTimeSetAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F32_E");
                  try
                  {
                        #region Handle Logic
                        //TODO:Check control mode and abort setting and reply SnF0 or not.
                        //TODO:Logic handle.
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F42_E_HostCommandAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F42_E");
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

                        //<array1 name="List" type="L" len="2">
                        //  <HCACK name="HCACK" type="B" len="1" fixlen="False" />
                        //  <array2 name="List" type="L" len="?">
                        //    <array3 name="List" type="L" len="2">
                        //      <CPNAME name="CPNAME" type="A" len="16" fixlen="False" />
                        //      <CPACK name="CPACK" type="B" len="1" fixlen="False" />
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body
                        string hcack = recvTrx["secs"]["message"]["body"]["array1"]["HCACK"].InnerText.Trim();
                        string len = recvTrx["secs"]["message"]["body"]["array1"]["array2"].Attributes["len"].InnerText.Trim();
                        int loop = 0;
                        int.TryParse(len, out loop);
                        for (int i = 0; i < loop; i++)
                        {
                              XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"].ChildNodes[i];
                              string dataname = xNode["CPNAME"].InnerText.Trim();
                              string dataid = xNode["CPACK"].InnerText.Trim();
                        }

                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S2F42", _common.ToFormatString(recvTrx.OuterXml) });
                                    break;
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F104_E_LotStartInformAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F104_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string tag = recvTrx["secs"]["message"]["return"].InnerText.Trim();

                        string ackc5 = recvTrx["secs"]["message"]["body"]["array1"]["ACKC5"].InnerText.Trim();
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F106_E_LotEndInformAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F106_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string tag = recvTrx["secs"]["message"]["return"].InnerText.Trim();

                        string ackc5 = recvTrx["secs"]["message"]["body"]["array1"]["ACKC5"].InnerText.Trim();
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F112_E_ForcedCleanOutCommandAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F112_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string tag = recvTrx["secs"]["message"]["return"].InnerText.Trim();

                        string ackc = recvTrx["secs"]["message"]["body"]["array1"]["ACKC"].InnerText.Trim();
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F116_E_GlassDataDownloadAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F116_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string tag = recvTrx["secs"]["message"]["return"].InnerText.Trim();

                        string ackc = recvTrx["secs"]["message"]["body"]["array1"]["ACKC"].InnerText.Trim();
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F118_E_GlassEraseRecoveryInformationAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F118_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string tag = recvTrx["secs"]["message"]["return"].InnerText.Trim();

                        string ackc = recvTrx["secs"]["message"]["body"]["array1"]["ACKC"].InnerText.Trim();
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F120_E_EquipmentModeChangeCommandAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F120_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string tag = recvTrx["secs"]["message"]["return"].InnerText.Trim();

                        string ackc = recvTrx["secs"]["message"]["body"]["array1"]["ACKC"].InnerText.Trim();
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F122_E_APCDataDownloadCommandAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F122_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string tag = recvTrx["secs"]["message"]["return"].InnerText.Trim();

                        string ackc = recvTrx["secs"]["message"]["body"]["array1"]["ACKC"].InnerText.Trim();
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F124_E_SetGlassRecipeGroupEndFlagAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F124_E");
                  try
                  {
                        #region Handle Logic
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string tag = recvTrx["secs"]["message"]["return"].InnerText.Trim();

                        string ackc = recvTrx["secs"]["message"]["body"]["array1"]["ACKC"].InnerText.Trim();
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region Send by host-S2
            public void TS2F0_H_AbortTransaction(string eqpno, string eqpid, string tid, string sysbytes)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F0_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F0_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = tid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = sysbytes;
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
            //{{ECID,ECV},...}
            public void TS2F15_H_NewEquipmentConstantSend(string eqpno, string eqpid, List<Tuple<string, string>> ecs, string tag, string trxid)
            {
                  try
                  {
                        //check argument
                        if (ecs == null || ecs.Count <= 0)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "ecs argument is null or empty.");
                              return;
                        }

                        //Get Agent Object
                        IServerAgent agent = GetServerAgent(eqpid);
                        if (agent == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("Can not get agent object with name ({0})", eqpid));
                              return;
                        }

                        //Get Transaction Format
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F15_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F15_H)");
                              return;
                        }

                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F15_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;

                        //<array1 name="List" type="L" len="?">
                        //   <array2 name="List" type="L" len="2">
                        //     <ECID name="ECID" type="U4" len="1" fixlen="False" />
                        //     <ECV name="ECV" type="A" len="16" fixlen="False" />
                        //   </array2>
                        // </array1>
                        //body
                        XmlNode xNode = sendTrx["secs"]["message"]["body"]["array1"];
                        xNode.Attributes["len"].InnerText = ecs.Count.ToString();
                        if (ecs.Count == 0)
                        {
                              xNode.RemoveChild(xNode["array2"]);
                        }
                        for (int i = 0; i < ecs.Count; i++)
                        {
                              _common.CloneChildNode(xNode, i);
                        }
                        for (int i = 0; i < ecs.Count; i++)
                        {
                              xNode.ChildNodes[i]["ECID"].InnerText = ecs[i].Item1;
                              _common.SetItemData(xNode.ChildNodes[i]["ECV"], ecs[i].Item2);
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
            public void TS2F18_H_DateandTimeData(string eqpno, string eqpid, string tid, string sysbytes)
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
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["DATETIME"], DateTime.Now.ToString("yyyyMMddHHmmss"));
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
            public void TS2F19_H_DataSetCommand(string eqpno, string eqpid, string func, string subfunc, string enable, string frequence, string tag, string trxid)
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
                        int iInterval = 0;
                        if (!int.TryParse(frequence, out iInterval))
                        {
                              string msg = string.Format("Parameter of Frequence({0}) is not number type.", frequence);
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, trxid, msg);
                              if (tag == "OPI")
                              {
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { trxid, eqp.Data.LINEID, "Send S2F19 Error", msg });
                              }
                              return;
                        }
                        List<Tuple<string, string, string, string, List<string>>> sets = new List<Tuple<string, string, string, string, List<string>>>();
                        ObjectManager.EquipmentManager.ReloadSECSVariableDataByEqpNo(eqpno);
                        Add2SetsName(eqpno, string.Format("{0}_{1}", func, subfunc), func, subfunc, enable, frequence, sets);
                        //20150421 cy:增加判斷回應OPI
                        if (sets.Count <= 0)
                        {
                              string msg = string.Format("Setting parameter of {0} is empty.", string.Format("{0}_{1}", func, subfunc));
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, trxid, msg);
                              if (tag == "OPI"||tag=="AutoRequest")
                              {
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { trxid, eqp.Data.LINEID, "Send S2F19 Error", msg + " Please check SECS_Variable." });
                              }
                              return;
                        }
                        if (sets[0].Item5.Count <= 0)
                        {
                              string msg = string.Format("Setting parameter field(ItemName) of {0} is empty.", string.Format("{0}_{1}", func, subfunc));
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, trxid, msg);
                              if (tag == "OPI"||tag=="AutoRequest")
                              {
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { trxid, eqp.Data.LINEID, "Send S2F19 Error", msg + " Please check field(ItemName)." });
                              }
                              return;
                        }

                        //重新設定間隔時間
                        switch (func)
                        {
                              case "S1F5":
                                    switch (subfunc)
                                    {
                                          case "02":
                                                eqp.File.APCImportanIntervalMS = iInterval;
                                                eqp.File.APCImportanEnableReq = enable.Equals("0");
                                                //20150709 cy:停此要求時,把倉庫清空
                                                if (!eqp.File.APCImportanEnableReq)
                                                {
                                                      string key = string.Format("{0}_{1}_SecsAPCImportantDataByReq", eqp.Data.LINEID, eqp.Data.NODEID);
                                                      Repository.Remove(key);
                                                }
                                                break;
                                          case "04":
                                                eqp.File.APCNormalIntervalMS = iInterval;
                                                eqp.File.APCNormalEnableReq = enable.Equals("0");
                                                //20150709 cy:停此要求時,把倉庫清空
                                                if (!eqp.File.APCNormalEnableReq)
                                                {
                                                      string key = string.Format("{0}_{1}_SecsAPCNormalDataByReq", eqp.Data.LINEID, eqp.Data.NODEID);
                                                      Repository.Remove(key);
                                                }
                                                break;
                                          case "06":
                                                eqp.File.SpecialDataIntervalMS = iInterval;
                                                eqp.File.SpecialDataEnableReq = enable.Equals("0");
                                                //20150709 cy:停此要求時,把倉庫清空
                                                if (!eqp.File.SpecialDataEnableReq)
                                                {
                                                      string key = string.Format("{0}_{1}_SecsSpecialDataReq", eqp.Data.LINEID, eqp.Data.NODEID);
                                                      Repository.Remove(key);
                                                }
                                                break;
                                    }
                                    break;
                              case "S6F3":
                                    switch (subfunc)
                                    {
                                          case "04":
                                                eqp.File.APCImportanIntervalMS = iInterval;
                                                eqp.File.APCImportanEnable = enable.Equals("0");
                                                //20150709 cy:停此要求時,把倉庫清空
                                                if (!eqp.File.APCImportanEnable)
                                                {
                                                      string key = string.Format("{0}_{1}_SecsAPCImportantData", eqp.Data.LINEID, eqp.Data.NODEID);
                                                      Repository.Remove(key);
                                                }
                                                break;
                                          case "05":
                                                eqp.File.APCNormalIntervalMS = iInterval;
                                                eqp.File.APCNormalEnable = enable.Equals("0");
                                                //20150709 cy:停此要求時,把倉庫清空
                                                if (!eqp.File.APCNormalEnable)
                                                {
                                                      string key = string.Format("{0}_{1}_SecsAPCNormalData", eqp.Data.LINEID, eqp.Data.NODEID);
                                                      Repository.Remove(key);
                                                }
                                                break;
                                          case "08":
                                                eqp.File.SpecialDataIntervalMS = iInterval;
                                                eqp.File.SpecialDataEnable = enable.Equals("0");
                                                //20150709 cy:停此要求時,把倉庫清空
                                                if (!eqp.File.SpecialDataEnable)
                                                {
                                                      string key = string.Format("{0}_{1}_SecsSpecialData", eqp.Data.LINEID, eqp.Data.NODEID);
                                                      Repository.Remove(key);
                                                }
                                                break;
                                    }
                                    ObjectManager.EquipmentManager.EnqueueSave(eqp.File); //20161130 yang : 这边save
                                    break;
                        }
                     //   ObjectManager.EquipmentManager.EnqueueSave(eqp.File); //20150603 cy : add save.
                        TS2F19_H_DataSetCommand(eqpno, eqpid, sets, tag, trxid);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            //{{FUNCTIONNAME,SUBFUNCTIONCODE,USEFLAG,REPORTFREQUENCY,{DATANAME,...},...}
            public void TS2F19_H_DataSetCommand(string eqpno, string eqpid, List<Tuple<string, string, string, string, List<string>>> sets, string tag, string trxid)
            {
                  try
                  {
                        //check argument
                        if (sets == null || sets.Count <= 0)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "ItemName sets argument is null or empty.");
                              return;
                        }

                        //Get Agent Object
                        IServerAgent agent = GetServerAgent(eqpid);
                        if (agent == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("Can not get agent object with name ({0})", eqpid));
                              return;
                        }

                        //Get Transaction Format
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F19_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F19_H)");
                              return;
                        }

                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["body"]["array1"].Attributes["len"].InnerText = "1";
                        sendTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"].Attributes["len"].InnerText = "1";
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F19_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;

                        //<array1 name="List" type="L" len="?">
                        //  <array2 name="List" type="L" len="5">
                        //    <FUNCTIONNAME name="FUNCTIONNAME" type="A" len="4" fixlen="False" />
                        //    <SUBFUNCTIONCODE name="SUBFUNCTIONCODE" type="A" len="2" fixlen="False" />
                        //    <USEFLAG name="USEFLAG" type="A" len="1" fixlen="False" />
                        //    <REPORTFREQUENCY name="REPORTFREQUENCY" type="A" len="1" fixlen="False" />
                        //    <array3 name="List" type="L" len="?">
                        //      <DATANAME name="DATANAME" type="A" len="30" fixlen="False" />
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body				
                        XmlNode xNode1 = sendTrx["secs"]["message"]["body"]["array1"];
                        int loop1 = 0;
                        if (sets != null)
                        {
                              loop1 = sets.Count;
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
                        for (int i = 0; i < loop1; i++)
                        {
                              _common.SetItemData(xNode1.ChildNodes[i]["FUNCTIONNAME"], sets[i].Item1);
                              _common.SetItemData(xNode1.ChildNodes[i]["SUBFUNCTIONCODE"], sets[i].Item2);
                              _common.SetItemData(xNode1.ChildNodes[i]["USEFLAG"], sets[i].Item3);
                              _common.SetItemData(xNode1.ChildNodes[i]["REPORTFREQUENCY"], sets[i].Item4);
                              XmlNode xNode3 = xNode1.ChildNodes[i]["array3"];
                              int loop3 = 0;
                              List<string> items = sets[i].Item5;
                              if (items != null)
                              {
                                    loop3 = items.Count;
                              }
                              xNode3.Attributes["len"].InnerText = loop3.ToString();
                              if (loop3 == 0)
                              {
                                    xNode3.RemoveChild(xNode3["DATANAME"]);
                              }
                              for (int j = 0; j < loop3; j++)
                              {
                                    _common.CloneChildNode(xNode3, j);
                              }
                              for (int j = 0; j < loop3; j++)
                              {
                                    //add dataname
                                    _common.SetItemData(xNode3.ChildNodes[j], items[j]);
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
            public void TS2F21_H_DataSetCommandforID(string eqpno, string eqpid, string func, string subfunc, string enable, string frequence, string tag, string trxid)
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
                        int iInterval = 0;
                        if (!int.TryParse(frequence, out iInterval))
                        {
                              string msg = string.Format("Parameter of Frequence({0}) is not number type.", frequence);
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, trxid, msg);
                              if (tag == "OPI")
                              {
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { trxid, eqp.Data.LINEID, "Send S2F21 Error", msg });
                              }
                              return;
                        }
                        List<Tuple<string, string, string, string, List<string>>> sets = new List<Tuple<string, string, string, string, List<string>>>();

                        ObjectManager.EquipmentManager.ReloadSECSVariableDataByEqpNo(eqpno);
                        Add2SetsID(eqpno, string.Format("{0}_{1}", func, subfunc), func, subfunc, enable, frequence, sets);
                        //20150421 cy:增加判斷回應OPI
                        if (sets.Count <= 0)
                        {
                              string msg = string.Format("Setting parameter of {0} is empty.", string.Format("{0}_{1}", func, subfunc));
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, trxid, msg);
                              if (tag == "OPI" || tag=="AutoRequest")
                              {
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { trxid, eqp.Data.LINEID, "Send S2F21 Error", msg + " Please check SECS_Variable." });
                              }                         
                              return;
                        }
                        if (sets[0].Item5.Count <= 0)
                        {
                              string msg = string.Format("Setting parameter field(ItemID) of {0} is empty.", string.Format("{0}_{1}", func, subfunc));
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, trxid, msg);
                              if (tag == "OPI" || tag == "AutoRequest")
                              {
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { trxid, eqp.Data.LINEID, "Send S2F21 Error", msg + " Please check field(ItemID)." });
                              }
                              return;
                        }

                        //重新設定間隔時間
                        switch (func)
                        {
                              case "S1F5":
                                    switch (subfunc)
                                    {
                                          case "07":
                                                eqp.File.APCImportanIntervalMSForID = iInterval;
                                                //  eqp.File.APCImportanEnableReq = enable.Equals("0"); 
                                                eqp.File.APCImportanEnableReqForID = enable.Equals("0");  // wucc modify 20150806
                                                //停此要求時,把倉庫清空
                                                if (!eqp.File.APCImportanEnableReq)
                                                {
                                                      string key = string.Format("{0}_{1}_SecsAPCImportantDataByReq", eqp.Data.LINEID, eqp.Data.NODEID);
                                                      Repository.Remove(key);
                                                }
                                                break;
                                          case "08":
                                                eqp.File.APCNormalIntervalMSForID = iInterval;
                                                //  eqp.File.APCNormalEnableReq = enable.Equals("0"); 
                                                eqp.File.APCNormalEnableReqForID = enable.Equals("0");  // wucc modify 20150806
                                                //停此要求時,把倉庫清空
                                                if (!eqp.File.APCNormalEnableReq)
                                                {
                                                      string key = string.Format("{0}_{1}_SecsAPCNormalDataByReq", eqp.Data.LINEID, eqp.Data.NODEID);
                                                      Repository.Remove(key);
                                                }
                                                break;
                                          case "10":
                                                eqp.File.SpecialDataIntervalMSForID = iInterval;
                                                // eqp.File.SpecialDataEnableReq = enable.Equals("0");
                                                eqp.File.SpecialDataEnableReqForID = enable.Equals("0"); // wucc modify 20150806
                                                //停此要求時,把倉庫清空
                                                if (!eqp.File.SpecialDataEnableReq)
                                                {
                                                      string key = string.Format("{0}_{1}_SecsSpecialDataReq", eqp.Data.LINEID, eqp.Data.NODEID);
                                                      Repository.Remove(key);
                                                }
                                                break;
                                    }
                                    break;
                              case "S6F3":
                                    switch (subfunc)
                                    {
                                          case "09":
                                                eqp.File.APCImportanIntervalMSForID = iInterval;
                                                // eqp.File.APCImportanEnable = enable.Equals("0");
                                                eqp.File.APCImportanEnableForID = enable.Equals("0"); // wucc modify 20150806
                                                //停此要求時,把倉庫清空
                                                if (!eqp.File.APCImportanEnable)
                                                {
                                                      string key = string.Format("{0}_{1}_SecsAPCImportantData", eqp.Data.LINEID, eqp.Data.NODEID);
                                                      Repository.Remove(key);
                                                }
                                                break;
                                          case "10":
                                                eqp.File.APCNormalIntervalMSForID = iInterval;
                                                // eqp.File.APCNormalEnable = enable.Equals("0");
                                                eqp.File.APCNormalEnableForID = enable.Equals("0");// wucc modify 20150806
                                                //停此要求時,把倉庫清空
                                                if (!eqp.File.APCNormalEnable)
                                                {
                                                      string key = string.Format("{0}_{1}_SecsAPCNormalData", eqp.Data.LINEID, eqp.Data.NODEID);
                                                      Repository.Remove(key);
                                                }
                                                break;
                                          case "11":
                                                eqp.File.SpecialDataIntervalMSForID = iInterval;
                                                // eqp.File.SpecialDataEnable = enable.Equals("0");
                                                eqp.File.SpecialDataEnableForID = enable.Equals("0");// wucc modify 20150806
                                                //停此要求時,把倉庫清空
                                                if (!eqp.File.SpecialDataEnable)
                                                {
                                                      string key = string.Format("{0}_{1}_SecsSpecialData", eqp.Data.LINEID, eqp.Data.NODEID);
                                                      Repository.Remove(key);
                                                }
                                                break;
                                    }
                                    break;
                        }
                        ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                        TS2F21_H_DataSetCommandforID(eqpno, eqpid, sets, tag, trxid);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            //{{FUNCTIONNAME,SUBFUNCTIONCODE,USEFLAG,REPORTFREQUENCY,{DATAID,...},...}
            public void TS2F21_H_DataSetCommandforID(string eqpno, string eqpid, List<Tuple<string, string, string, string, List<string>>> sets, string tag, string trxid)
            {
                  try
                  {
                        //check argument
                        if (sets == null || sets.Count <= 0)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "ItemID sets argument is null or empty.");
                              return;
                        }

                        //Get Agent Object
                        IServerAgent agent = GetServerAgent(eqpid);
                        if (agent == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("Can not get agent object with name ({0})", eqpid));
                              return;
                        }

                        //Get Transaction Format
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F21_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F21_H)");
                              return;
                        }

                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F21_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;

                        //<array1 name="List" type="L" len="?">
                        //  <array2 name="List" type="L" len="5">
                        //    <FUNCTIONNAME name="FUNCTIONNAME" type="A" len="4" fixlen="False" />
                        //    <SUBFUNCTIONCODE name="SUBFUNCTIONCODE" type="A" len="2" fixlen="False" />
                        //    <USEFLAG name="USEFLAG" type="A" len="1" fixlen="False" />
                        //    <REPORTFREQUENCY name="REPORTFREQUENCY" type="A" len="1" fixlen="False" />
                        //    <array3 name="List" type="L" len="?">
                        //      <DATAID name="DATAID" type="A" len="30" fixlen="False" />
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body				
                        XmlNode xNode1 = sendTrx["secs"]["message"]["body"]["array1"];
                        int loop1 = 0;
                        if (sets != null)
                        {
                              loop1 = sets.Count;
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
                        for (int i = 0; i < loop1; i++)
                        {
                              _common.SetItemData(xNode1.ChildNodes[i]["FUNCTIONNAME"], sets[i].Item1);
                              _common.SetItemData(xNode1.ChildNodes[i]["SUBFUNCTIONCODE"], sets[i].Item2);
                              _common.SetItemData(xNode1.ChildNodes[i]["USEFLAG"], sets[i].Item3);
                              _common.SetItemData(xNode1.ChildNodes[i]["REPORTFREQUENCY"], sets[i].Item4);
                              XmlNode xNode3 = xNode1.ChildNodes[i]["array3"];
                              int loop3 = 0;
                              List<string> items = sets[i].Item5;
                              if (items != null)
                              {
                                    loop3 = items.Count;
                              }
                              xNode3.Attributes["len"].InnerText = loop3.ToString();
                              if (loop3 == 0)
                              {
                                    xNode3.RemoveChild(xNode3["DATAID"]);
                              }
                              for (int j = 0; j < loop3; j++)
                              {
                                    _common.CloneChildNode(xNode3, j);
                              }
                              for (int j = 0; j < loop3; j++)
                              {
                                    //add dataid
                                    _common.SetItemData(xNode3.ChildNodes[j], items[j]);
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
            public void TS2F23_H_DataItemMappingTableRequest(string eqpno, string eqpid, string tag, string trxid)
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
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F23_H";
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
                                  string.Format("Can not get agent object with name ({0})", eqpid));
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
                                  string.Format("Can not get agent object with name ({0})", eqpid));
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
            //{ECID,...}
            public void TS2F29_H_EquipmentConstantNamlistRequest(string eqpno, string eqpid, List<string> ecids, string tag, string trxid)
            {
                  try
                  {
                        //check argument
                        if (ecids == null || ecids.Count <= 0)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "ecids argument is null or empty.");
                              return;
                        }

                        //Get Agent Object
                        IServerAgent agent = GetServerAgent(eqpid);
                        if (agent == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("Can not get agent object with name ({0})", eqpid));
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
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F29_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;

                        //<array1 name="List" type="L" len="?">
                        //  <ECID name="ECID" type="U4" len="1" fixlen="False" />
                        //</array1>
                        XmlNode xNode = sendTrx["secs"]["message"]["body"]["array1"];
                        int loop = 0;
                        if (ecids != null)
                        {
                              loop = ecids.Count;
                        }
                        xNode.Attributes["len"].InnerText = loop.ToString();
                        if (loop == 0)
                        {
                              xNode.RemoveChild(xNode["ECID"]);
                        }
                        for (int i = 0; i < loop; i++)
                        {
                              _common.CloneChildNode(xNode, i);
                        }
                        for (int i = 0; i < loop; i++)
                        {
                              //add ecid
                              _common.SetItemData(xNode.ChildNodes[i], ecids[i]);
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
            public void TS2F31_H_DateandTimeSetRequest(string eqpno, string eqpid, string tag, string trxid)
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
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["DATETIME"], DateTime.Now.ToString("yyyyMMddHHmmss"));
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
            //{{CPNAME,CPVAL},...}
            //cmd //PAUSE, RESUME，FORBID,RUN
            public void TS2F41_H_HostCommandSend(string eqpno, string eqpid, string cmd, List<Tuple<string, string>> cps, string tag, string trxid)
            {
                  try
                  {
                        //check argument
                        if (cps == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "cps argument is null.");
                              return;
                        }
                        //remove empty name
                        cps.RemoveAll(t => t.Item1.Trim() == "");

                        //Get Agent Object
                        IServerAgent agent = GetServerAgent(eqpid);
                        if (agent == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("Can not get agent object with name ({0})", eqpid));
                              return;
                        }

                        //Get Transaction Format
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F41_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F41_H)");
                              return;
                        }

                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F41_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;

                        //<array1 name="List" type="L" len="2">
                        //  <RCMD name="RCMD" type="A" len="10" fixlen="False" />
                        //  <array2 name="List" type="L" len="?">
                        //    <array3 name="List" type="L" len="2">
                        //      <CPNAME name="CPNAME" type="A" len="16" fixlen="False" />
                        //      <CPVAL name="CPVAL" type="A" len="10" fixlen="False" />
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["RCMD"], cmd); //PAUSE, RESUME，FORBID,RUN
                        XmlNode xNode = sendTrx["secs"]["message"]["body"]["array1"]["array2"];
                        xNode.Attributes["len"].InnerText = cps.Count.ToString();
                        if (cps.Count == 0)
                        {
                              xNode.RemoveChild(xNode["array3"]);
                        }
                        for (int i = 0; i < cps.Count; i++)
                        {
                              _common.CloneChildNode(xNode, i);
                        }
                        for (int i = 0; i < cps.Count; i++)
                        {
                              _common.SetItemData(xNode.ChildNodes[i]["CPNAME"], cps[i].Item1);
                              _common.SetItemData(xNode.ChildNodes[i]["CPVAL"], cps[i].Item2);
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
            public void TS2F103_H_LotStartInformSend(string eqpno, string eqpid, string lotid, string glassid, string ppid, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F103_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F103_H)");
                              return;
                        }

                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F103_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;

                        //<array1 name="List" type="L" len="3">
                        //  <LOTID name="LOTID" type="A" len="20" fixlen="False" />
                        //  <GLASSID name="GLASSID" type="A" len="12" fixlen="False" />
                        //  <PPID name="PPID" type="A" len="16" fixlen="False" />
                        //</array1>
                        //body
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["LOTID"], lotid);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["GLASSID"], glassid);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["PPID"], ppid);

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
            public void TS2F105_H_LotEndInformSend(string eqpno, string eqpid, string cstseq, string slot, string glassid, string ppid, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F105_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F105_H)");
                              return;
                        }

                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F105_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;

                        //<array1 name="List" type="L" len="4">
                        //  <CSTSEQ name="CSTSEQ" type="A" len="5" fixlen="False" />
                        //  <SLOT name="SLOT" type="A" len="3" fixlen="False" />
                        //  <GLASSID name="GLASSID" type="A" len="12" fixlen="False" />
                        //  <PPID name="PPID" type="A" len="16" fixlen="False" />
                        //</array1>
                        //body
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["CSTSEQ"], cstseq);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["SLOT"], slot);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["GLASSID"], glassid);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["PPID"], ppid);


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
            //cleanout //0: Forced Clean Out Reset.,1: Normal Forced Clean Out Set.,2: Abnormal Forced Clean Out Set.
            public void TS2F111_H_ForcedCleanOutCommandSend(string eqpno, string eqpid, string eqptid, string cleanout, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F111_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F111_H)");
                              return;
                        }

                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(eqpid);
                        if (eqp == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                      string.Format("Can not find Equipment No ({0}) in EquipmentEntity!", eqpid));
                              return;
                        }

                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                      string.Format("Can not find Line ID ({0}) in LineEntity!", eqp.Data.LINEID));
                              return;
                        }

                        //check DNS
                        if (cleanout != "0")
                        {
                              switch (eqp.Data.NODEATTRIBUTE)
                              {
                                    case "DNS":
                                          if ((eqp.File.EquipmentRunMode == "PASS" && cleanout == "1")
                                              ||
                                              (eqp.File.EquipmentRunMode == "APAS" && cleanout == "2"))
                                          {
                                                //already cleanout mode
                                                _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqp.Data.NODENO, false, trxid,
                                                      string.Format("Equipment run mode is alread in Force Clean Out. ({0})", eqp.File.EquipmentRunMode));
                                                return;
                                          }
                                          break;
                              }
                        }

                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F111_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;
                        //<array1 name="List" type="L" len="2">
                        //  <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //  <CLEANOUT name="CLEANOUT" type="A" len="1" fixlen="False" />
                        //</array1>
                        //body
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["EQUIPMENTID"], eqptid);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["CLEANOUT"], cleanout); //0: Forced Clean Out Reset.,1: Normal Forced Clean Out Set.,2: Abnormal Forced Clean Out Set.


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
            public void TS2F115_H_GlassDataDownload(string eqpno, string eqpid, Job job, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F115_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F115_H)");
                              return;
                        }

                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F115_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;
                        //<array1 name="List" type="L" len="5">
                        //  <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //  <CSTSEQ name="CSTSEQ" type="A" len="5" fixlen="False" />
                        //  <CSTOPERMODE name="CSTOPERMODE" type="A" len="1" fixlen="False" />
                        //  <SUBSTRATETYPE name="SUBSTRATETYPE" type="A" len="1" fixlen="False" />
                        //  <array2 name="List" type="L" len="1">
                        //    <array3 name="List" type="L" len="8">
                        //      <SLOT name="SLOT" type="A" len="3" fixlen="False" />
                        //      <GLASSID name="GLASSID" type="A" len="20" fixlen="False" />
                        //      <ALLPPID name="ALLPPID" type="A" len="40" fixlen="False" />
                        //      <PRODUCT name="PRODUCT" type="A" len="16" fixlen="False" />
                        //      <JOBTYPE name="JOBTYPE" type="A" len="1" fixlen="False" />
                        //      <DUMMYTYPE name="DUMMYTYPE" type="A" len="1" fixlen="False" />
                        //      <JOBJUDGE name="JOBJUDGE" type="A" len="1" fixlen="False" />
                        //      <JOBGRADE name="JOBGRADE" type="A" len="4" fixlen="False" />
                        //    </array3>
                        //  </array2>
                        //</array1>

                        //body
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["EQUIPMENTID"], eqpid);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["CSTSEQ"], job.CassetteSequenceNo);
                        //_common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["CSTOPERMODE"], job.CSTOperationMode.ToString());
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["CSTOPERMODE"], ((int)job.CSTOperationMode).ToString()); //modify for 1,2,.. 2016/08/29 cc.kuang
                        //_common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["SUBSTRATETYPE"], job.SubstrateType.ToString());
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["SUBSTRATETYPE"], ((int)job.SubstrateType).ToString()); //modify for 1,2,.. 2016/08/29 cc.kuang
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["SLOT"], job.JobSequenceNo);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["GLASSID"], job.EQPJobID);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["ALLPPID"], job.PPID);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["PRODUCT"], job.ProductType.Value.ToString());
                        string jobtype = string.Empty;
                        string dummytype = string.Empty;
                        //<JOBTYPE> 1: TFT Glass,2: CF Glass,3: Dummy Glass,4: UV Mask
                        //<DUMMYTYPE> 1: General Dummy,2: Through Dummy,3: Thickness Dummy,4: ITO Dummy,5: Bare Dummy
                        switch (job.JobType)
                        {
                              case eJobType.CF:
                                    jobtype = "2";
                                    break;
                              case eJobType.DM:
                                    jobtype = "3";
                                    dummytype = "1";
                                    break;
                              case eJobType.TFT:
                                    jobtype = "1";
                                    break;
                              case eJobType.TK:
                                    jobtype = "3";
                                    dummytype = "3";
                                    break;
                              case eJobType.TR:
                                    jobtype = "3";
                                    dummytype = "2";
                                    break;
                              case eJobType.UV:
                                    jobtype = "4";
                                    break;
                        }
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["JOBTYPE"], jobtype);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["DUMMYTYPE"], dummytype);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["JOBJUDGE"], job.JobJudge);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["JOBGRADE"], job.JobGrade);
                        //_common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["array2"]["array3"]["OXR"], job.OXRInformation);   //t3沒有

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
            //er //1 = Erase. ,2 = Recovery.
            //lastflag //0 = Not Last Glass. ,1 = Last Glass
            public void TS2F117_H_GlassEraseRecoveryInformationSend(string eqpno, string eqpid, string cstseq, string slot, string glassid, string er, string lastflag, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F117_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F117_H)");
                              return;
                        }

                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F117_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;

                        //<array1 name="List" type="L" len="5">
                        //  <CSTSEQ name="CSTSEQ" type="A" len="5" fixlen="False" />
                        //  <SLOT name="SLOT" type="A" len="3" fixlen="False" />
                        //  <GLASSID name="GLASSID" type="A" len="20" fixlen="False" />
                        //  <ER name="ER" type="A" len="1" fixlen="False" />
                        //  <LASTGLSFLAG name="LASTGLSFLAG" type="A" len="1" fixlen="False" />
                        //</array1>
                        //body
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["CSTSEQ"], cstseq);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["SLOT"], slot);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["GLASSID"], glassid);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["ER"], er); //1 = Erase. ,2 = Recovery.
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["LASTGLSFLAG"], lastflag); //0 = Not Last Glass. ,1 = Last Glass

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


            public void TS2F119_H_EquipmentModeChangeCommandSend(string eqpno, string eqpid, string eqpmode, List<Tuple<string, string, string>> unitlists, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F119_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F119_H)");
                              return;
                        }

                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F119_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;
                        //<array1 name="List" type="L" len="2">
                        //  <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="True" />
                        //  <EQPMODE name="EQPMODE" type="A" len="4" fixlen="True" />
                        //  <array2 name="List" type="L" len="?">
                        //    <array3 name="List" type="L" len="2">
                        //      <SUBEQPID name="SUBEQPID" type="A" len="16" fixlen="True" />
                        //      <SUBMODE name="SUBMODE" type="A" len="4" fixlen="True" />
                        //    </array3>
                        //  </array2>        
                        //</array1>
                        //body
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["EQUIPMENTID"], eqpid);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["EQPMODE"], eqpmode); //NORN/PASS/APAS/2S/2D/4P1/4P2/2O/2Q/4Q/ENG/MQC/ENG/MQC/IGZO/A/B/MIX

                        XmlNode xNode = sendTrx["secs"]["message"]["body"]["array1"]["array2"];
                        int loop = 0;
                        if (unitlists.Count != 0)
                        {
                              loop = unitlists.Count;
                        }
                        xNode.Attributes["len"].InnerText = loop.ToString();
                        if (loop == 0)
                        {
                              xNode.RemoveChild(xNode["array3"]);
                        }
                        for (int i = 0; i < loop; i++)
                        {
                              _common.CloneChildNode(xNode, i);
                        }
                        for (int i = 0; i < loop; i++)
                        {
                              _common.SetItemData(xNode.ChildNodes[i]["SUBEQPID"], unitlists[i].Item1); //UNITID
                              _common.SetItemData(xNode.ChildNodes[i]["SUBMODE"], unitlists[i].Item3); //UNIT mode
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


            //{{APCNAME,APCVALUE},...}
            public void TS2F121_H_APCDataDownloadCommandSend(string eqpno, string eqpid, List<Tuple<string, string>> APCDatas, string tag, string trxid)
            {
                  try
                  {
                        //check argument
                        if (APCDatas == null || APCDatas.Count <= 0)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", "APCDatas argument is null or empty.");
                              return;
                        }

                        //Get Agent Object
                        IServerAgent agent = GetServerAgent(eqpid);
                        if (agent == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("Can not get agent object with name ({0})", eqpid));
                              return;
                        }

                        //Get Transaction Format
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F121_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F121_H)");
                              return;
                        }

                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F121_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;

                        //<array1 name="List" type="L" len="2">
                        //  <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //  <array2 name="List" type="L" len="?">
                        //    <array3 name="List" type="L" len="2">
                        //      <APCNAME name="APCNAME" type="A" len="16" fixlen="False" />
                        //      <APCVALUE name="APCVALUE" type="A" len="16" fixlen="False" />
                        //    </array3>
                        //  </array2>
                        //</array1>

                        //body
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["EQUIPMENTID"], eqpid);
                        XmlNode xNode = sendTrx["secs"]["message"]["body"]["array1"]["array2"];
                        int loop = 0;
                        if (APCDatas != null)
                        {
                              loop = APCDatas.Count;
                        }
                        xNode.Attributes["len"].InnerText = loop.ToString();
                        if (loop == 0)
                        {
                              xNode.RemoveChild(xNode["array3"]);
                        }
                        for (int i = 0; i < loop; i++)
                        {
                              _common.CloneChildNode(xNode, i);
                        }
                        for (int i = 0; i < loop; i++)
                        {
                              _common.SetItemData(xNode.ChildNodes[i]["APCNAME"], APCDatas[i].Item1);
                              _common.SetItemData(xNode.ChildNodes[i]["APCVALUE"], APCDatas[i].Item2);
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
            //endflag //0 = Not Recipe Group End.,1 = Recipe Group End
            public void TS2F123_H_SetGlassRecipeGroupEndFlagSend(string eqpno, string eqpid, string cstseq, string slot, string glassid, string endflag, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S2F123_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S2F123_H)");
                              return;
                        }

                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S2F123_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;

                        //<array1 name="List" type="L" len="4">
                        //  <CSTSEQ name="CSTSEQ" type="A" len="5" fixlen="False" />
                        //  <SLOT name="SLOT" type="A" len="3" fixlen="False" />
                        //  <GLASSID name="GLASSID" type="A" len="20" fixlen="False" />
                        //  <RECIPEENDFLAG name="RECIPEENDFLAG" type="A" len="1" fixlen="False" />
                        //</array1>
                        //body
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["CSTSEQ"], cstseq);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["SLOT"], slot);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["GLASSID"], glassid);
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["array1"]["RECIPEENDFLAG"], endflag); //0 = Not Recipe Group End.,1 = Recipe Group End

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
            public void S2F0_H_AbortTransaction(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F0_H T3-Timeout", false);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F0_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F15_H_NewEquipmentConstantSend(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F15_H T3-Timeout", false);
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
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S2F15", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F15_H", false);
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
            public void S2F19_H_DataSetCommand(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F19_H T3-Timeout", false);
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
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S2F19", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F19_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F21_H_DataSetCommandforID(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F21_H T3-Timeout", false);
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
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S2F21", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F21_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F23_H_DataItemMappingTableRequest(XmlDocument recvTrx, bool timeout)
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
            public void S2F29_H_EquipmentConstantNamlistRequest(XmlDocument recvTrx, bool timeout)
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
            public void S2F41_H_HostCommandSend(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F41_H T3-Timeout", false);
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
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F41_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F103_H_LotStartInformSend(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F103_H T3-Timeout", false);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F103_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F105_H_LotEndInformSend(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F105_H T3-Timeout", false);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F105_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F111_H_ForcedCleanOutCommandSend(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F111_H T3-Timeout", false);
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
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S2F111", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F111_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F115_H_GlassDataDownload(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F115_H T3-Timeout", false);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F115_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F117_H_GlassEraseRecoveryInformationSend(XmlDocument recvTrx, bool timeout)
            {
                  if (timeout)
                  {
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F117_H T3-Timeout", false);
                        return;
                  }
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F117_H", false);
            }
            public void S2F119_H_EquipmentModeChangeCommandSend(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F119_H T3-Timeout", false);
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
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S2F119", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F119_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F121_H_APCDataDownloadCommandSend(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F121_H T3-Timeout", false);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F121_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S2F123_H_SetGlassRecipeGroupEndFlagSend(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F123_H T3-Timeout", false);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S2F123_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion
      }
}