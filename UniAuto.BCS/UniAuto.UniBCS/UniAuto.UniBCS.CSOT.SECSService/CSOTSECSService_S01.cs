using System;
using System.Reflection;
using System.Xml;
using System.Collections.Generic;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.MesSpec;

namespace UniAuto.UniBCS.CSOT.SECSService
{
      public partial class CSOTSECSService
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
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtnID)
                        {

                              case "S1F5_H": //若斷線期間，機台切offline，則機台可能回S1F0，此時我們也要切為CIM Off
                              case "S1F15_H":
                                    {
                                          lock (eqp)
                                          {
                                                eqp.File.HSMSControlMode = "OFF-LINE";
                                                //20150918 t3:針對report mode是HSMS_開頭的,就去更新CIM Mode. (Modify by CY)
                                                switch (eqp.Data.REPORTMODE)
                                                {
                                                      case "HSMS_PLC":
                                                            eqp.File.CIMMode = eBitResult.OFF;
                                                            _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                                string.Format("CIM Mode({0}).", eqp.File.CIMMode.ToString()));
                                                            break;
                                                }
                                                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                                _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                            string.Format("Control Mode({0}).", eqp.File.HSMSControlMode));
                                                //Report to OPI
                                                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                                                //20161204 yang:CIM Mode要报给MES
                                                if (eqp.Data.REPORTMODE.Equals("HSMS_PLC"))
                                                    Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { tid, eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });
                                          }
                                    }
                                    break;

                        }

                        //check if opi request
                        switch (rtn)
                        {
                              case "OPI":
                                    if (rtnID == "S1F15_H")
                                    {
                                          Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                              new object[4] { tid, eqp.Data.LINEID, "Reply S1F0", string.Format("[EQUIPMENT={0}] Offline request Fail. Equipment reply S1F0 Abort Transaction.", eqp.Data.NODENO) });
                                          //20150317 cy:online request timeout, terminal timer and do nothing.
                                          string timerId = string.Format("S1F15_OfflineRequest_{0}_OPI", eqpno);
                                          if (_timerManager.IsAliveTimer(timerId))
                                          {
                                                _timerManager.TerminateTimer(timerId); //remove old
                                          }
                                    }
                                    if (rtnID == "S1F17_H")
                                    {
                                          Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                              new object[4] { tid, eqp.Data.LINEID, "Reply S1F0", string.Format("[EQUIPMENT={0}] online request Fail. Equipment reply S1F0 Abort Transaction.", eqp.Data.NODENO) });
                                          //20150317 cy:online request timeout, terminal timer and do nothing.
                                          string timerId = string.Format("S1F17_OnlineRequest_{0}_OPI", eqpno);
                                          if (_timerManager.IsAliveTimer(timerId))
                                          {
                                                _timerManager.TerminateTimer(timerId); //remove old
                                          }
                                    }
                                    break;
                              case "AutoOnlineRequest":
                                    lock (eqp)
                                    {
                                          eqp.File.HSMSControlMode = "OFF-LINE";
                                          //針對CIM MODE沒PLC的機台，Off-line時，視為CIM OFF，待Online後才切為CIM ON
                                          switch (eqp.Data.REPORTMODE)
                                          {
                                                case "HSMS_PLC":
                                                      eqp.File.CIMMode = eBitResult.OFF;
                                                      _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                          string.Format("CIM Mode({0}).", eqp.File.CIMMode.ToString()));
                                                      break;
                                          }
                                          ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                          _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                      string.Format("Control Mode({0}).", eqp.File.HSMSControlMode));
                                          //Report to OPI
                                          Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                                          //20161204 yang:CIM Mode要报给MES
                                          if (eqp.Data.REPORTMODE.Equals("HSMS_PLC"))
                                              Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { tid, eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });
                                    }
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Auto Online Request", string.Format("Equipment({0}) abort transaction({1}). Auto Online Request Fail.", eqp.Data.NODENO, rtnID) });
                                    if (rtnID == "S1F17_H")
                                    {
                                          string timerId = string.Format("S1F17_OnlineRequest_{0}_AUTO", eqpno);
                                          if (_timerManager.IsAliveTimer(timerId))
                                          {
                                                _timerManager.TerminateTimer(timerId); //remove old
                                          }
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
            public void S1F1_E_AreYouThereRequest(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F1_E");
                  #region Handle Logic
                  try
                  {
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        TS1F2_H_OnlineData(eqpno, agent, tid, sysbytes);

                        //onlinescenario
                        //check if in onlinescenario process(timer exists)
                        string timerId = string.Format("{0}_{1}", eqpno, "SecsOnlineScenario");
                        if (_timerManager.IsAliveTimer(timerId))
                        {
                              return; //already in onlinescenario process
                        }
                        //start onlinescenario process(create timer)
                        _timerManager.CreateTimer(timerId, false, 30000,
                            new System.Timers.ElapsedEventHandler(OnlineScenarioTimeOut), tid);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            private void OnlineScenarioTimeOut(object subject, System.Timers.ElapsedEventArgs e)
            {
                  UserTimer timer = subject as UserTimer;
                  if (timer == null)
                  {
                        return;
                  }
                  string[] arr = timer.TimerId.Split('_');
                  if (arr.Length < 1)
                  {
                        return;
                  }
                  string eqpno = arr[0];
                  string tid = timer.State.ToString();

                  //20150113 cy:不需要再做terminate onlinescenario的動作,因為在timeout而觸發到這時,就會自己將這資料刪除						
                  //_timerManager.TerminateTimer(timer.TimerId);
            }
            public void S1F2_E_OnlineData(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F2_E");
                  #region Handle Logic
                  try
                  {
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
                        //  <MDLN name="MDLN" type="A" len="6" fixlen="False" />
                        //  <SOFTREV name="SOFTREV" type="A" len="6" fixlen="False" />
                        //</array1>
                        //body
                        string mdln = recvTrx["secs"]["message"]["body"]["array1"]["MDLN"].InnerText.Trim();
                        string softrev = recvTrx["secs"]["message"]["body"]["array1"]["SOFTREV"].InnerText.Trim();
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                string.Format("MDLN({0}), SOFTREV({1})", eqp.MDLN, eqp.SOFTREV));

                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
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
            public void S1F6_01_E_EquipmentInformationInquire(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F6_01_E");
                  #region Handle Logic
                  try
                  {
                        //get basic
                        string agent = recvTrx["secs"]["message"].Attributes["agent"].InnerText.Trim();
                        string eqpno = recvTrx["secs"]["message"].Attributes["node"].InnerText.Trim();
                        string tid = recvTrx["secs"]["message"].Attributes["tid"].InnerText.Trim();
                        string sysbytes = recvTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText.Trim();
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        //get eqp object
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpno);
                        if (eqp == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                         "Can not find Equipment Number in EquipmentEntity!");
                              return;
                        }
                        //<array1 name="List" type="L" len="10">
                        //  <SFCD name="SFCD" type="A" len="2" fixlen="False" />
                        //  <EQUIPMENTID name="EQUIPMENTID" type="A" len="16" fixlen="False" />
                        //  <CTRLMODE name="CTRLMODE" type="A" len="1" fixlen="False" />
                        //  <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //  <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //  <RECOVERY name="RECOVERY" type="A" len="1" fixlen="False" />
                        //  <INLINE name="INLINE" type="A" len="1" fixlen="False" />
                        //  <PPID name="PPID" type="A" len="16" fixlen="False" />
                        //  <GLASS_COUNT name="GLASS_COUNT" type="A" len="4" fixlen="False" />
                        //  <array2 name="List (Sub Equipment Count)" type="L" len="?">
                        //    <array3 name="List (Sub Equipment Info)" type="L" len="11">
                        //      <SUBEQUIPMENTID name="SUBEQUIPMENTID" type="A" len="16" fixlen="False" />
                        //      <SUBEQPMODE name="SUBEQPMODE" type="A" len="4" fixlen="False" />
                        //      <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //      <PPID name="PPID" type="A" len="16" fixlen="False" />
                        //      <PPVER name="PPVER" type="A" len="14" fixlen="False" />
                        //      <GLASS_COUNT name="GLASS_COUNT" type="A" len="4" fixlen="False" />
                        //      <INLINE name="INLINE" type="A" len="1" fixlen="False" />
                        //      <AUTO name="AUTO" type="A" len="1" fixlen="False" />
                        //      <MANUAL name="MANUAL" type="A" len="1" fixlen="False" />
                        //      <ALID name="ALID" type="U4" len="1" fixlen="False" />
                        //      <ALTX name="ALTX" type="A" len="80" fixlen="False" />
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body

                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              return;
                        }

                        string eqpid = recvTrx["secs"]["message"]["body"]["array1"]["EQUIPMENTID"].InnerText.Trim();
                        //get eqp object
                        if (eqpid != eqp.Data.NODEID)
                        {
                              string msg = string.Format("Received EQUIPMENTID({1}) is mismatch with SECS Config Item:<{2}><LocalNO>{0}.", eqpno, eqpid, agent);
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, msg);
                              if (rtn == "OPI")
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F6_01", string.Format("Request equipment information fail. {0}", msg) });
                              return;
                        }

                        //check if opi request
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F6_01", "Equipment replied status information." });//_common.ToFormatString(recvTrx.OuterXml) });
                                    break;
                        }

                        
                        List<Tuple<Unit, string, string>> unitMode = new List<Tuple<Unit, string, string>>();
                        bool ModeRpt = false;
                        string ctrlmode = recvTrx["secs"]["message"]["body"]["array1"]["CTRLMODE"].InnerText.Trim();
                        string eqpst = recvTrx["secs"]["message"]["body"]["array1"]["EQPST"].InnerText.Trim();
                        lock (eqp)
                        {
                              //set new ctrlmode
                              string handleMsg = string.Empty;
                              if (HandleControlMode(eqp, ctrlmode, out handleMsg))
                                    _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid, handleMsg);
                              else
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, handleMsg);

                              //set new eqpmode
                              string rcvMode = recvTrx["secs"]["message"]["body"]["array1"]["EQPMODE"].InnerText.Trim();
                              string eqpmode = ConstantManager["CSOT_SECS_RUNMODE"][rcvMode].Value;
                              string oldMode = eqp.File.EquipmentRunMode;
                              Tuple<Equipment, string, string> eqpMode = Tuple.Create(eqp, rcvMode, oldMode);
                              eqp.File.EquipmentRunMode = eqpmode;
                              _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Equipment Mode({0}).", eqpmode));

                              //check eqpmode change
                              if (eqpmode != oldMode)
                              {
                                    switch (eqp.Data.NODEATTRIBUTE)
                                    {
                                          case "DNS":
                                                //1.DNS發mode為"Abnormal Pass Mode"時，BC要ON Abnormal Force Clean out。
                                                //2.DNS發mode為"Pass Mode"時，BC要ON Force Clean out。
                                                switch (rcvMode)
                                                {
                                                      case "PASS":
                                                            if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.FORCE_CLEAN_OUT_MODE)
                                                            {
                                                                  Invoke(eServiceName.ArraySpecialService, "ForceCleanOutCommand", new object[] { eBitResult.ON, tid });
                                                            }
                                                            break;
                                                      case "APAS":
                                                            if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.FORCE_CLEAN_OUT_MODE)
                                                            {
                                                                  Invoke(eServiceName.ArraySpecialService, "AbnormalForceCleanOutCommand", new object[] { eBitResult.ON, tid });
                                                            }
                                                            break;
                                                }
                                                break;
                                          case "CVD":
                                          case "DRY":
                                                ModeRpt = true;
                                                break;
                                    }
                              }


                              string recovery = recvTrx["secs"]["message"]["body"]["array1"]["RECOVERY"].InnerText.Trim();
                              _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Equipment Recovery({0}:{1})", recovery, ConvertRecoveryMode(recovery)));

                              string inline = recvTrx["secs"]["message"]["body"]["array1"]["INLINE"].InnerText.Trim();
                              switch (inline)
                              {
                                    case "0":
                                          eqp.File.UpstreamInlineMode = eBitResult.OFF;
                                          eqp.File.DownstreamInlineMode = eBitResult.OFF;
                                          break;
                                    case "1":
                                          eqp.File.UpstreamInlineMode = eBitResult.ON;
                                          eqp.File.DownstreamInlineMode = eBitResult.ON;
                                          break;
                                    case "2":
                                          eqp.File.UpstreamInlineMode = eBitResult.OFF;
                                          eqp.File.DownstreamInlineMode = eBitResult.ON;
                                          break;
                                    case "3":
                                          eqp.File.UpstreamInlineMode = eBitResult.ON;
                                          eqp.File.DownstreamInlineMode = eBitResult.OFF;
                                          break;
                              }
                              _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                              string.Format("Upstream Inline Mode({0}), Downstream Inline Mode({1}).", eqp.File.UpstreamInlineMode.ToString(), eqp.File.DownstreamInlineMode.ToString()));

                              string ppid = recvTrx["secs"]["message"]["body"]["array1"]["PPID"].InnerText.Trim();
                              eqp.File.CurrentRecipeID = ppid;
                              _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                     string.Format("Current Recipe ID ({0})", ppid));

                              int glasscount = 0;
                              if (!int.TryParse(recvTrx["secs"]["message"]["body"]["array1"]["GLASS_COUNT"].InnerText.Trim(), out glasscount))
                              {
                                    _common.LogError(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                        string.Format("Can not convert glass count value. GLASS_COUNT({0})", recvTrx["secs"]["message"]["body"]["array1"]["GLASS_COUNT"].InnerText.Trim()));
                              }
                              eqp.File.TotalTFTJobCount = glasscount; //20141226 cy modify:Array的glass count總是報在TFT即可

                              #region 處理SubEquipment
                              XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                              string len = xNode.Attributes["len"].InnerText.Trim();
                              int loop = 0;
                              int.TryParse(len, out loop);
                              //units
                              for (int i = 0; i < loop; i++)
                              {
                                    string subeqpid = xNode.ChildNodes[i]["SUBEQUIPMENTID"].InnerText.Trim();
                                    Unit unit = ObjectManager.UnitManager.GetUnit(subeqpid);
                                    if (unit == null)
                                    {
                                          continue;
                                    }
                                    else if (unit.Data.UNITATTRIBUTE == "VIRTUAL")
                                          continue;

                                    string rcvSubMode = xNode.ChildNodes[i]["SUBEQPMODE"].InnerText.Trim();
                                    string subeqpMode = ConstantManager["CSOT_SECS_RUNMODE"][rcvSubMode].Value;
                                    string oldsubeqpmode = unit.File.RunMode;
                                    if ((eqp.Data.NODEATTRIBUTE == "CVD" || eqp.Data.NODEATTRIBUTE == "DRY") && (unit.Data.UNITTYPE != null && unit.Data.UNITTYPE.Equals("CHAMBER")))
                                    {
                                          unitMode.Add(Tuple.Create(unit, rcvSubMode, oldsubeqpmode));
                                    }
                                    //check subeqpmode change
                                    if (subeqpMode != oldsubeqpmode)
                                    {
                                          ModeRpt = true;
                                    }

                                    _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                            string.Format("SUBEQUIPMENT({0}), current SUBEQPMODE({1}).", subeqpid, subeqpMode));

                                    string subeqpst = xNode.ChildNodes[i]["EQPST"].InnerText.Trim();
                                    string subeqpppid = xNode.ChildNodes[i]["PPID"].InnerText.Trim();
                                    string subeqpppver = xNode.ChildNodes[i]["PPVER"].InnerText.Trim();
                                    int subeqpglasscount = 0;
                                    if (!int.TryParse(xNode.ChildNodes[i]["GLASS_COUNT"].InnerText.Trim(), out subeqpglasscount))
                                    {
                                          _common.LogError(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                          string.Format("Can not convert unit glass count value. UnitID({0}), GLASS_COUNT({1})", unit.Data.UNITID, xNode.ChildNodes[i]["GLASS_COUNT"].InnerText.Trim()));
                                    }
                                    string subeqpinline = xNode.ChildNodes[i]["INLINE"].InnerText.Trim();
                                    string subeqpauot = xNode.ChildNodes[i]["AUTO"].InnerText.Trim();
                                    string subeqpmanual = xNode.ChildNodes[i]["MANUAL"].InnerText.Trim();
                                    string subeqalid = xNode.ChildNodes[i]["ALID"].InnerText.Trim();
                                    string subeqpaltx = xNode.ChildNodes[i]["ALTX"].InnerText.Trim();

                                    lock (unit)
                                    {
                                          unit.File.RunMode = subeqpMode;
                                          unit.File.CurrentAlarmCode = subeqalid;
                                          unit.File.PreStatus = unit.File.Status;
                                          unit.File.Status = ConvertCsotStatus(subeqpst);
                                          unit.File.PreMesStatus = unit.File.MESStatus;
                                          unit.File.MESStatus = _common.ConvertMesStatus(unit.File.Status);
                                          unit.File.TFTProductCount = subeqpglasscount;
                                    }
                                    ObjectManager.UnitManager.EnqueueSave(unit.File);
                              }
                              #endregion

                              //呼叫LineService.CheckLineRunMode
                              Invoke(eServiceName.LineService, "CheckLineRunMode", new object[] { eqp.Data.LINEID });

                              #region [ MachineModeChangeRequest ]
                              if (ModeRpt)
                              {
                                    if (eqp.Data.NODEATTRIBUTE == "CVD" || eqp.Data.NODEATTRIBUTE == "DRY")
                                    {
                                          //CVD、DRY發的需上報MES MachineModeChangeRequest
                                          Invoke(eServiceName.MESService, "MachineModeChangeRequest", new object[] { tid, eqp.Data.LINEID });
                                    }
                              }
                              #endregion

                              ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                              #region [ MachineStateChanged ]
                              eqp.File.PreStatus = eqp.File.Status;
                              eqp.File.Status = ConvertCsotStatus(eqpst);
                              eqp.File.PreMesStatus = eqp.File.MESStatus;
                              eqp.File.MESStatus = _common.ConvertMesStatus(eqp.File.Status);
                              eqp.File.CurrentRecipeID = ppid;
                              _common.HandleEquipmentStatus(eqp, tid, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()");
                              #endregion
                        }

                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
                  #endregion
            }
            public void S1F6_02_E_APCImportantUtilityFacilityDataInquire(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F6_02_E");
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

                        //*APCImportantRepository Format=> List<Tuple<subeqpid,List<Tuple<dcname,dctype,dcvalue>>>
                        List<Tuple<string, List<Tuple<string, string, string>>>> apcImportantRepository = new List<Tuple<string, List<Tuple<string, string, string>>>>();

                        //<array1 name="List" type="L" len="5">
                        //  <SFCD name="SFCD" type="A" len="2" fixlen="False" />
                        //  <SUBCD name="SUBCD" type="U4" len="1" fixlen="False" />
                        //  <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //  <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //  <array2 name="List (Eqp Count or SubEqp Count)" type="L" len="?">
                        //    <array3 name="List" type="L" len="2">
                        //      <SUBEQUIPMENTID name="SUBEQUIPMENTID" type="A" len="16" fixlen="False" />
                        //      <array4 name="List (number of data items)" type="L" len="?">
                        //        <array5 name="List" type="L" len="3">
                        //          <DCNAME name="DCNAME" type="A" len="30" fixlen="False" />
                        //          <DCTYPE name="DCTYPE" type="A" len="3" fixlen="False" />
                        //          <DCVALUE name="DCVALUE" type="A" len="16" fixlen="False" />
                        //        </array5>
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body
                        string subcd = recvTrx["secs"]["message"]["body"]["array1"]["SUBCD"].InnerText.Trim();
                        string eqpmode = recvTrx["secs"]["message"]["body"]["array1"]["EQPMODE"].InnerText.Trim();
                        string eqpst = recvTrx["secs"]["message"]["body"]["array1"]["EQPST"].InnerText.Trim();
                        XmlNode xNode2 = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                        string len2 = xNode2.Attributes["len"].InnerText.Trim();
                        int loop2 = 0;
                        int.TryParse(len2, out loop2);
                        for (int i = 0; i < loop2; i++)
                        {
                              string subeqpid = xNode2.ChildNodes[i]["SUBEQUIPMENTID"].InnerText.Trim();
                              //add subeqp
                              List<Tuple<string, string, string>> itemList = new List<Tuple<string, string, string>>();
                              apcImportantRepository.Add(Tuple.Create(subeqpid, itemList));

                              XmlNode xNode4 = xNode2.ChildNodes[i]["array4"];
                              string len4 = xNode4.Attributes["len"].InnerText.Trim();
                              int loop4 = 0;
                              int.TryParse(len4, out loop4);
                              for (int j = 0; j < loop4; j++)
                              {
                                    string dcname = xNode4.ChildNodes[j]["DCNAME"].InnerText.Trim();
                                    string dctype = xNode4.ChildNodes[j]["DCTYPE"].InnerText.Trim();
                                    string dcvalue = xNode4.ChildNodes[j]["DCVALUE"].InnerText.Trim();
                                    //add item
                                    ///20150604 cy:增加檢查資料不能重覆
                                    int dupIndex = itemList.FindIndex(n => n.Item1 == dcname);
                                    if (dupIndex > -1)
                                    {
                                          _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                      string.Format("Item({0}) duplicate, old value({1}) will change with new value({2})", dcname, itemList[dupIndex].Item3, dcvalue));
                                          itemList.RemoveAt(dupIndex);
                                    }
                                    itemList.Add(Tuple.Create(dcname, dctype, dcvalue));

                              }
                        }
                        //*APCImportantRepository key=>lineid+'_'+nodeid+'_SecsAPCImportantDataByReq'
                        string key = string.Format("{0}_{1}_SecsAPCImportantDataByReq", eqp.Data.LINEID, eqp.Data.NODEID);
                        //20150709 cy:設定為disable時,不要加到倉庫
                        if (eqp.File.APCImportanEnableReq)
                        Repository.Add(key, apcImportantRepository);
                        else
                              Repository.Remove(key);

                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F6_02", _common.ToFormatString(recvTrx.OuterXml) });
                                    break;
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S1F6_03_E_MaterialDataInquire(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F6_03_E");
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
                        //  <SFCD name="SFCD" type="A" len="2" fixlen="False" />
                        //  <array2 name="List (number of material slots)" type="L" len="?">
                        //    <array3 name="List" type="L" len="3">
                        //      <MATERIALSL name="MATERIALSL" type="A" len="2" fixlen="False" />
                        //      <MATERIALID name="MATERIALID" type="A" len="30" fixlen="False" />
                        //      <MATERIALST name="MATERIALST" type="A" len="1" fixlen="False" />
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body		
                        List<Tuple<Unit, string, string, eMaterialStatus, string, string>> materials = new List<Tuple<Unit, string, string, eMaterialStatus, string, string>>();
                        List<Tuple<string, string, eMaterialStatus>> masks = new List<Tuple<string, string, eMaterialStatus>>();
                        XmlNode xNode = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                        string len = xNode.Attributes["len"].InnerText.Trim();
                        int loop = 0;
                        int.TryParse(len, out loop);
                        for (int i = 0; i < loop; i++)
                        {
                              string materialsl = xNode.ChildNodes[i]["MATERIALSL"].InnerText.Trim();
                              string materialid = xNode.ChildNodes[i]["MATERIALID"].InnerText.Trim();
                              string materialst = xNode.ChildNodes[i]["MATERIALST"].InnerText.Trim();
                              if (materialst == "R")
                              {
                                    continue;
                              }
                              //20141220 cy modify:canon報"*",記log, 發warning到OPI
                              if (eqp.Data.NODEATTRIBUTE == "CANON" && materialid == "*".PadRight(30, '*'))
                              {
                                    string msg = string.Format("Equipment({0}) mask id read fale. MaskSlot({1}),MaskID({2}),MaskState({3})", eqp.Data.NODEID, materialsl, materialid, materialst);
                                    _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid, msg);
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Receive S1F6_03", msg });
                                    continue;
                              }
                              if (eqp.Data.NODEATTRIBUTE == "CANON")
                              {
                                    masks.Add(Tuple.Create(materialsl, materialid, ConvertCsotMaterialStatus(materialst)));
                              }
                              else
                              {
                                    materials.Add(Tuple.Create((Unit)null, materialsl, materialid, ConvertCsotMaterialStatus(materialst), string.Empty, string.Empty));
                              }
                        }
                        if (eqp.Data.NODEATTRIBUTE == "CANON")
                        {
                              HandleMaskStatus(eqp, masks, tid, false, string.Empty);
                        }
                        else
                        {
                              HandleMaterialStatus(eqp, materials, tid, true, false, string.Empty, string.Empty);
                        }

                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F6_03", _common.ToFormatString(recvTrx.OuterXml) });
                                    break;
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S1F6_04_E_APCNormalUtilityFacilityDataInquire(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F6_04_E");
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

                        //*APCNormalRepository Format=> List<Tuple<subeqpid,List<Tuple<dcname,dctype,dcvalue>>>
                        List<Tuple<string, List<Tuple<string, string, string>>>> apcNormalRepository = new List<Tuple<string, List<Tuple<string, string, string>>>>();

                        //<array1 name="List" type="L" len="5">
                        //  <SFCD name="SFCD" type="A" len="2" fixlen="False" />
                        //  <SUBCD name="SUBCD" type="U4" len="1" fixlen="False" />
                        //  <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //  <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //  <array2 name="List (Eqp Count or SubEqp Count)" type="L" len="?">
                        //    <array3 name="List" type="L" len="2">
                        //      <SUBEQUIPMENTID name="SUBEQUIPMENTID" type="A" len="16" fixlen="False" />
                        //      <array4 name="List (number of data items)" type="L" len="?">
                        //        <array5 name="List" type="L" len="3">
                        //          <DCNAME name="DCNAME" type="A" len="30" fixlen="False" />
                        //          <DCTYPE name="DCTYPE" type="A" len="3" fixlen="False" />
                        //          <DCVALUE name="DCVALUE" type="A" len="16" fixlen="False" />
                        //        </array5>
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body
                        string subcd = recvTrx["secs"]["message"]["body"]["array1"]["SUBCD"].InnerText.Trim();
                        string eqpmode = recvTrx["secs"]["message"]["body"]["array1"]["EQPMODE"].InnerText.Trim();
                        string eqpst = recvTrx["secs"]["message"]["body"]["array1"]["EQPST"].InnerText.Trim();
                        XmlNode xNode2 = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                        string len2 = xNode2.Attributes["len"].InnerText.Trim();
                        int loop2 = 0;
                        int.TryParse(len2, out loop2);
                        for (int i = 0; i < loop2; i++)
                        {
                              string subeqpid = xNode2.ChildNodes[i]["SUBEQUIPMENTID"].InnerText.Trim();
                              //add subeqp
                              List<Tuple<string, string, string>> itemList = new List<Tuple<string, string, string>>();
                              apcNormalRepository.Add(Tuple.Create(subeqpid, itemList));

                              XmlNode xNode4 = xNode2.ChildNodes[i]["array4"];
                              string len4 = xNode4.Attributes["len"].InnerText.Trim();
                              int loop4 = 0;
                              int.TryParse(len4, out loop4);
                              for (int j = 0; j < loop4; j++)
                              {
                                    string dcname = xNode4.ChildNodes[j]["DCNAME"].InnerText.Trim();
                                    string dctype = xNode4.ChildNodes[j]["DCTYPE"].InnerText.Trim();
                                    string dcvalue = xNode4.ChildNodes[j]["DCVALUE"].InnerText.Trim();
                                    //add item
                                    //20150604 cy:增加檢查資料不能重覆
                                    int dupIndex = itemList.FindIndex(n => n.Item1 == dcname);
                                    if (dupIndex > -1)
                                    {
                                          _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                      string.Format ("Item({0}) duplicate, old value({1}) will change with new value({2})",  dcname, itemList[dupIndex].Item3, dcvalue));
                                          itemList.RemoveAt(dupIndex);
                                    }
                                    itemList.Add(Tuple.Create(dcname, dctype, dcvalue));
                              }
                        }
                        //*APCNormalRepository key=>lineid+'_'+nodeid+'_SecsAPCNormalDataByReq'
                        string key = string.Format("{0}_{1}_SecsAPCNormalDataByReq", eqp.Data.LINEID, eqp.Data.NODEID);
                        //20150709 cy:設定為disable時,不要加到倉庫
                        if (eqp.File.APCNormalEnableReq)
                        Repository.Add(key, apcNormalRepository);
                        else
                              Repository.Remove(key);

                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F6_04", _common.ToFormatString(recvTrx.OuterXml) });
                                    break;
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S1F6_05_E_UtilityDataInquire(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F6_05_E");
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

                        //*DailyCheckRepository Format=> List<Tuple<subeqpid,List<Tuple<dcname,dctype,dcvalue>>>
                        List<Tuple<string, List<Tuple<string, string, string>>>> dailycheckRepository = new List<Tuple<string, List<Tuple<string, string, string>>>>();

                        //<array1 name="List" type="L" len="5">
                        //  <SFCD name="SFCD" type="A" len="2" fixlen="False" />
                        //  <SUBCD name="SUBCD" type="U4" len="1" fixlen="False" />
                        //  <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //  <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //  <array2 name="List (Eqp Count or SubEqp Count)" type="L" len="?">
                        //    <array3 name="List" type="L" len="2">
                        //      <SUBEQUIPMENTID name="SUBEQUIPMENTID" type="A" len="16" fixlen="False" />
                        //      <array4 name="List (number of data items)" type="L" len="?">
                        //        <array5 name="List" type="L" len="3">
                        //          <DCNAME name="DCNAME" type="A" len="30" fixlen="False" />
                        //          <DCTYPE name="DCTYPE" type="A" len="3" fixlen="False" />
                        //          <DCVALUE name="DCVALUE" type="A" len="16" fixlen="False" />
                        //        </array5>
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body
                        string subcd = recvTrx["secs"]["message"]["body"]["array1"]["SUBCD"].InnerText.Trim();
                        string eqpmode = recvTrx["secs"]["message"]["body"]["array1"]["EQPMODE"].InnerText.Trim();
                        string eqpst = recvTrx["secs"]["message"]["body"]["array1"]["EQPST"].InnerText.Trim();
                        XmlNode xNode2 = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                        string len2 = xNode2.Attributes["len"].InnerText.Trim();
                        int loop2 = 0;
                        int.TryParse(len2, out loop2);
                        for (int i = 0; i < loop2; i++)
                        {
                              string subeqpid = xNode2.ChildNodes[i]["SUBEQUIPMENTID"].InnerText.Trim();
                              //add subeqp
                              List<Tuple<string, string, string>> itemList = new List<Tuple<string, string, string>>();
                              dailycheckRepository.Add(Tuple.Create(subeqpid, itemList));

                              XmlNode xNode4 = xNode2.ChildNodes[i]["array4"];
                              string len4 = xNode4.Attributes["len"].InnerText.Trim();
                              int loop4 = 0;
                              int.TryParse(len4, out loop4);
                              for (int j = 0; j < loop4; j++)
                              {
                                    string dcname = xNode4.ChildNodes[j]["DCNAME"].InnerText.Trim();
                                    string dctype = xNode4.ChildNodes[j]["DCTYPE"].InnerText.Trim();
                                    string dcvalue = xNode4.ChildNodes[j]["DCVALUE"].InnerText.Trim();
                                    //add item
                                    //20150604 cy:增加檢查資料不能重覆
                                    int dupIndex = itemList.FindIndex(n => n.Item1 == dcname);
                                    if (dupIndex > -1)
                                    {
                                          _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                      string.Format("Item({0}) duplicate, old value({1}) will change with new value({2})", dcname, itemList[dupIndex].Item3, dcvalue));
                                          itemList.RemoveAt(dupIndex);
                                    }
                                    itemList.Add(Tuple.Create(dcname, dctype, dcvalue));
                              }
                        }
                        //*DailyCheckRepository key=>lineid+'_'+nodeid+'_SecsDailyCheck'
                        string key = string.Format("{0}_{1}_SecsDailyCheck", eqp.Data.LINEID, eqp.Data.NODEID);
                        Repository.Add(key, dailycheckRepository);

                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F6_05", _common.ToFormatString(recvTrx.OuterXml) });
                                    break;
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S1F6_06_E_SpecialDataInquire(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F6_06_E");
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

                        //get line object
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null)
                        {
                              _common.LogError(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                      string.Format("Can not find Line ID({0}) in LineEntity!", eqp.Data.LINEID));
                              return;
                        }

                        //*SpecialDataRepository Format=> List<Tuple<subeqpid,List<Tuple<dcname,dctype,dcvalue>>>
                        List<Tuple<string, List<Tuple<string, string, string>>>> specialDataRepository = new List<Tuple<string, List<Tuple<string, string, string>>>>();

                        //<array1 name="List" type="L" len="5">
                        //  <SFCD name="SFCD" type="A" len="2" fixlen="False" />
                        //  <SUBCD name="SUBCD" type="U4" len="1" fixlen="False" />
                        //  <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //  <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //  <array2 name="List (Eqp Count or SubEqp Count)" type="L" len="?">
                        //    <array3 name="List" type="L" len="2">
                        //      <SUBEQUIPMENTID name="SUBEQUIPMENTID" type="A" len="16" fixlen="False" />
                        //      <array4 name="List (number of data items)" type="L" len="?">
                        //        <array5 name="List" type="L" len="3">
                        //          <DCNAME name="DCNAME" type="A" len="30" fixlen="False" />
                        //          <DCTYPE name="DCTYPE" type="A" len="3" fixlen="False" />
                        //          <DCVALUE name="DCVALUE" type="A" len="16" fixlen="False" />
                        //        </array5>
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body
                        IList<ChangeMaterialLifeReport.MATERIALc> cmlMaterials = new List<ChangeMaterialLifeReport.MATERIALc>();
                        string subcd = recvTrx["secs"]["message"]["body"]["array1"]["SUBCD"].InnerText.Trim();
                        string eqpmode = recvTrx["secs"]["message"]["body"]["array1"]["EQPMODE"].InnerText.Trim();
                        string eqpst = recvTrx["secs"]["message"]["body"]["array1"]["EQPST"].InnerText.Trim();
                        XmlNode xNode2 = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                        string len2 = xNode2.Attributes["len"].InnerText.Trim();
                        int loop2 = 0;
                        int.TryParse(len2, out loop2);
                        for (int i = 0; i < loop2; i++)
                        {
                              string subeqpid = xNode2.ChildNodes[i]["SUBEQUIPMENTID"].InnerText.Trim();
                              //add subeqp
                              List<Tuple<string, string, string>> itemList = new List<Tuple<string, string, string>>();
                              specialDataRepository.Add(Tuple.Create(subeqpid, itemList));

                              XmlNode xNode4 = xNode2.ChildNodes[i]["array4"];
                              string len4 = xNode4.Attributes["len"].InnerText.Trim();
                              int loop4 = 0;
                              int.TryParse(len4, out loop4);
                              for (int j = 0; j < loop4; j++)
                              {
                                    string dcname = xNode4.ChildNodes[j]["DCNAME"].InnerText.Trim();
                                    string dctype = xNode4.ChildNodes[j]["DCTYPE"].InnerText.Trim();
                                    string dcvalue = xNode4.ChildNodes[j]["DCVALUE"].InnerText.Trim();
                                    //add item
                                    //20150604 cy:增加檢查資料不能重覆
                                    int dupIndex = itemList.FindIndex(n => n.Item1 == dcname);
                                    if (dupIndex > -1)
                                    {
                                          _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                      string.Format("Item({0}) duplicate, old value({1}) will change with new value({2})", dcname, itemList[dupIndex].Item3, dcvalue));
                                          itemList.RemoveAt(dupIndex);
                                    }
                                    itemList.Add(Tuple.Create(dcname, dctype, dcvalue));
                                    //20150310 cy 增加特定欄位,上報特定功能
                                    #region For ChangeTargetLife
                                    if (line.Data.FABTYPE.ToUpper() == "ARRAY")
                                    {
                                          if (line.Data.LINETYPE == eLineType.ARRAY.DRY_ICD||line.Data.LINETYPE == eLineType.ARRAY.DRY_TEL)
                                          {
                                                string mkey = string.Format("{0}_SPECIAL_DATA_MATERIALLIFE", line.Data.LINETYPE);

                                                ConstantData _constantdata = ConstantManager[mkey];

                                                if (_constantdata != null && _constantdata[dcname.Trim()].Value.ToUpper() == "TRUE")
                                                {
                                                      cmlMaterials.Add(new ChangeMaterialLifeReport.MATERIALc()
                                                      {
                                                            CHAMBERID = string.Empty,
                                                            MATERIALNAME = dcname.Trim(),
                                                            MATERIALTYPE = "DRYPOLE",
                                                            QUANTITY = dcvalue.Trim(),
                                                      });
                                                }
                                          }
                                    }
                                    #endregion
                              }
                        }
                        //*SpecialDataRepository key=>lineid+'_'+nodeid+'_SecsSpecialDataReq'
                        string key = string.Format("{0}_{1}_SecsSpecialDataReq", eqp.Data.LINEID, eqp.Data.NODEID);
                        //20150709 cy:設定為disable時,不要加到倉庫
                        if (eqp.File.SpecialDataEnableReq)
                              Repository.Add(key, specialDataRepository);
                        else
                              Repository.Remove(key);

                        #region 上報ChangeMaterialLifeReport(如果需要)
                        if (cmlMaterials.Count > 0)
                        {
                            Invoke(eServiceName.MESService, "ChangeMaterialLife", new object[] { tid, eqp.Data.LINEID, eqp.Data.NODEID, string.Empty, cmlMaterials }); 
                        }
                        #endregion
                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F6_06", _common.ToFormatString(recvTrx.OuterXml) });
                                    break;
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S1F6_07_E_APCImportantUtilityFacilityDataInquireforID(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F6_07_E");
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

                        //*apcImportant4IDRepository Format=> List<Tuple<subeqpid,List<Tuple<dcname,dctype,dcvalue>>>
                        List<Tuple<string, List<Tuple<string, string, string>>>> apcImportant4IDRepository = new List<Tuple<string, List<Tuple<string, string, string>>>>();

                        //<array1 name="List" type="L" len="5">
                        //  <SFCD name="SFCD" type="A" len="2" fixlen="False" />
                        //  <SUBCD name="SUBCD" type="U4" len="1" fixlen="False" />
                        //  <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //  <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //  <array2 name="List (Eqp Count or SubEqp Count)" type="L" len="?">
                        //    <array3 name="List" type="L" len="2">
                        //      <SUBEQUIPMENTID name="SUBEQUIPMENTID" type="A" len="16" fixlen="False" />
                        //      <array4 name="List (number of data items)" type="L" len="?">
                        //        <array5 name="List" type="L" len="3">
                        //          <DCID name="DCID" type="A" len="30" fixlen="False" />
                        //          <DCTYPE name="DCTYPE" type="A" len="3" fixlen="False" />
                        //          <DCVALUE name="DCVALUE" type="A" len="16" fixlen="False" />
                        //        </array5>
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body
                        string subcd = recvTrx["secs"]["message"]["body"]["array1"]["SUBCD"].InnerText.Trim();
                        string eqpmode = recvTrx["secs"]["message"]["body"]["array1"]["EQPMODE"].InnerText.Trim();
                        string eqpst = recvTrx["secs"]["message"]["body"]["array1"]["EQPST"].InnerText.Trim();
                        XmlNode xNode2 = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                        string len2 = xNode2.Attributes["len"].InnerText.Trim();
                        int loop2 = 0;
                        int.TryParse(len2, out loop2);
                        // modify by box.zhai 修改为抓取APC Data Report 表里面的格式
                        //IList<DailyCheckData> dataFormats = ObjectManager.DailyCheckManager.GetDailyCheckProfile(eqpno);
                        IList<APCDataReport> dataFormats = ObjectManager.APCDataReportManager.GetAPCDataReportProfile(eqpno);
                        for (int i = 0; i < loop2; i++)
                        {
                              string subeqpid = xNode2.ChildNodes[i]["SUBEQUIPMENTID"].InnerText.Trim();
                              //add subeqp
                              List<Tuple<string, string, string>> itemList = new List<Tuple<string, string, string>>();
                              apcImportant4IDRepository.Add(Tuple.Create(subeqpid, itemList));

                              XmlNode xNode4 = xNode2.ChildNodes[i]["array4"];
                              string len4 = xNode4.Attributes["len"].InnerText.Trim();
                              int loop4 = 0;
                              int.TryParse(len4, out loop4);
                              for (int j = 0; j < loop4; j++)
                              {
                                    string dcid = xNode4.ChildNodes[j]["DCID"].InnerText.Trim();
                                    string dctype = xNode4.ChildNodes[j]["DCTYPE"].InnerText.Trim();
                                    string dcvalue = xNode4.ChildNodes[j]["DCVALUE"].InnerText.Trim();
                                    string dcname = GetDCNAMEForAPC(dataFormats, dcid);
                                    //add item
                                    //20150604 cy:增加檢查資料不能重覆
                                    int dupIndex = itemList.FindIndex(n => n.Item1 == dcname);
                                    if (dupIndex > -1)
                                    {
                                          _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                      string.Format("Item({0},{1}) duplicate, old value({2}) will change with new value({3})", dcid, dcname, itemList[dupIndex].Item3, dcvalue));
                                          itemList.RemoveAt(dupIndex);
                                    }
                                    //itemList.Add(Tuple.Create(dcid, dctype, dcvalue));
                                    itemList.Add(Tuple.Create(dcname, dctype, dcvalue));
                              }
                        }
                        //*APCImportantData4IDRepository key=>lineid+'_'+nodeid+'_SecsAPCImportantData4IDByReq'
                        string key = string.Format("{0}_{1}_SecsAPCImportantDataByReq", eqp.Data.LINEID, eqp.Data.NODEID);
                        if (eqp.File.APCImportanEnableForID)
                              Repository.Add(key, apcImportant4IDRepository);
                        else
                              Repository.Remove(key);

                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F6_07", _common.ToFormatString(recvTrx.OuterXml) });
                                    break;
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S1F6_08_E_APCNormalUtilityFacilityDataInquireforID(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F6_08_E");
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

                        //*apcNormal4IDRepository Format=> List<Tuple<subeqpid,List<Tuple<dcname,dctype,dcvalue>>>
                        List<Tuple<string, List<Tuple<string, string, string>>>> apcNormal4IDRepository = new List<Tuple<string, List<Tuple<string, string, string>>>>();

                        //<array1 name="List" type="L" len="5">
                        //  <SFCD name="SFCD" type="A" len="2" fixlen="False" />
                        //  <SUBCD name="SUBCD" type="U4" len="1" fixlen="False" />
                        //  <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //  <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //  <array2 name="List (Eqp Count or SubEqp Count)" type="L" len="?">
                        //    <array3 name="List" type="L" len="2">
                        //      <SUBEQUIPMENTID name="SUBEQUIPMENTID" type="A" len="16" fixlen="False" />
                        //      <array4 name="List (number of data items)" type="L" len="?">
                        //        <array5 name="List" type="L" len="3">
                        //          <DCID name="DCID" type="A" len="30" fixlen="False" />
                        //          <DCTYPE name="DCTYPE" type="A" len="3" fixlen="False" />
                        //          <DCVALUE name="DCVALUE" type="A" len="16" fixlen="False" />
                        //        </array5>
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body
                        string subcd = recvTrx["secs"]["message"]["body"]["array1"]["SUBCD"].InnerText.Trim();
                        string eqpmode = recvTrx["secs"]["message"]["body"]["array1"]["EQPMODE"].InnerText.Trim();
                        string eqpst = recvTrx["secs"]["message"]["body"]["array1"]["EQPST"].InnerText.Trim();
                        XmlNode xNode2 = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                        string len2 = xNode2.Attributes["len"].InnerText.Trim();
                        int loop2 = 0;
                        int.TryParse(len2, out loop2);
                        // modify by box.zhai 修改为抓取APC Data Report 表里面的格式
                        //IList<DailyCheckData> dataFormats = ObjectManager.DailyCheckManager.GetDailyCheckProfile(eqpno);
                        IList<APCDataReport> dataFormats = ObjectManager.APCDataReportManager.GetAPCDataReportProfile(eqpno);
                        for (int i = 0; i < loop2; i++)
                        {
                              string subeqpid = xNode2.ChildNodes[i]["SUBEQUIPMENTID"].InnerText.Trim();
                              //add subeqp
                              List<Tuple<string, string, string>> itemList = new List<Tuple<string, string, string>>();
                              apcNormal4IDRepository.Add(Tuple.Create(subeqpid, itemList));

                              XmlNode xNode4 = xNode2.ChildNodes[i]["array4"];
                              string len4 = xNode4.Attributes["len"].InnerText.Trim();
                              int loop4 = 0;
                              int.TryParse(len4, out loop4);
                              for (int j = 0; j < loop4; j++)
                              {
                                    string dcid = xNode4.ChildNodes[j]["DCID"].InnerText.Trim();
                                    string dctype = xNode4.ChildNodes[j]["DCTYPE"].InnerText.Trim();
                                    string dcvalue = xNode4.ChildNodes[j]["DCVALUE"].InnerText.Trim();
                                    string dcname = GetDCNAMEForAPC(dataFormats, dcid);
                                    //add item
                                    //20150604 cy:增加檢查資料不能重覆
                                    int dupIndex = itemList.FindIndex(n => n.Item1 == dcname);
                                    if (dupIndex > -1)
                                    {
                                          _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                      string.Format("Item({0},{1}) duplicate, old value({2}) will change with new value({3})", dcid, dcname, itemList[dupIndex].Item3, dcvalue));
                                          itemList.RemoveAt(dupIndex);
                                    }
                                    //itemList.Add(Tuple.Create(dcid, dctype, dcvalue));
                                    itemList.Add(Tuple.Create(dcname, dctype, dcvalue));
                              }
                        }
                        //*APCNormalData4IDRepository key=>lineid+'_'+nodeid+'_SecsAPCNormalData4IDByReq'
                        string key = string.Format("{0}_{1}_SecsAPCNormalDataByReq", eqp.Data.LINEID, eqp.Data.NODEID);
                        if (eqp.File.APCNormalEnableForID)
                              Repository.Add(key, apcNormal4IDRepository);
                        else
                              Repository.Remove(key);

                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F6_08", _common.ToFormatString(recvTrx.OuterXml) });
                                    break;
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S1F6_09_E_UtilityDataInquireforID(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F6_09_E");
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

                        //*DailyCheck4IDRepository Format=> List<Tuple<subeqpid,List<Tuple<dcname,dctype,dcvalue>>>
                        List<Tuple<string, List<Tuple<string, string, string>>>> dailycheck4IDRepository = new List<Tuple<string, List<Tuple<string, string, string>>>>();

                        //<array1 name="List" type="L" len="5">
                        //  <SFCD name="SFCD" type="A" len="2" fixlen="False" />
                        //  <SUBCD name="SUBCD" type="U4" len="1" fixlen="False" />
                        //  <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //  <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //  <array2 name="List (Eqp Count or SubEqp Count)" type="L" len="?">
                        //    <array3 name="List" type="L" len="2">
                        //      <SUBEQUIPMENTID name="SUBEQUIPMENTID" type="A" len="16" fixlen="False" />
                        //      <array4 name="List (number of data items)" type="L" len="?">
                        //        <array5 name="List" type="L" len="3">
                        //          <DCID name="DCID" type="A" len="30" fixlen="False" />
                        //          <DCTYPE name="DCTYPE" type="A" len="3" fixlen="False" />
                        //          <DCVALUE name="DCVALUE" type="A" len="16" fixlen="False" />
                        //        </array5>
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body
                        string subcd = recvTrx["secs"]["message"]["body"]["array1"]["SUBCD"].InnerText.Trim();
                        string eqpmode = recvTrx["secs"]["message"]["body"]["array1"]["EQPMODE"].InnerText.Trim();
                        string eqpst = recvTrx["secs"]["message"]["body"]["array1"]["EQPST"].InnerText.Trim();
                        XmlNode xNode2 = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                        string len2 = xNode2.Attributes["len"].InnerText.Trim();
                        int loop2 = 0;
                        int.TryParse(len2, out loop2);
                        IList<DailyCheckData> dataFormats = ObjectManager.DailyCheckManager.GetDailyCheckProfile(eqpno);
                        for (int i = 0; i < loop2; i++)
                        {
                              string subeqpid = xNode2.ChildNodes[i]["SUBEQUIPMENTID"].InnerText.Trim();
                              //add subeqp
                              List<Tuple<string, string, string>> itemList = new List<Tuple<string, string, string>>();
                              dailycheck4IDRepository.Add(Tuple.Create(subeqpid, itemList));

                              XmlNode xNode4 = xNode2.ChildNodes[i]["array4"];
                              string len4 = xNode4.Attributes["len"].InnerText.Trim();
                              int loop4 = 0;
                              int.TryParse(len4, out loop4);
                              for (int j = 0; j < loop4; j++)
                              {
                                    string dcid = xNode4.ChildNodes[j]["DCID"].InnerText.Trim();
                                    string dctype = xNode4.ChildNodes[j]["DCTYPE"].InnerText.Trim();
                                    string dcvalue = xNode4.ChildNodes[j]["DCVALUE"].InnerText.Trim();
                                    string dcname = GetDCNAME(dataFormats, dcid);
                                    //add item
                                    //20150604 cy:增加檢查資料不能重覆
                                    int dupIndex = itemList.FindIndex(n => n.Item1 == dcname);
                                    if (dupIndex > -1)
                                    {
                                          _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                      string.Format("Item({0},{1}) duplicate, old value({2}) will change with new value({3})", dcid, dcname, itemList[dupIndex].Item3, dcvalue));
                                          itemList.RemoveAt(dupIndex);
                                    }
                                    //itemList.Add(Tuple.Create(dcid, dctype, dcvalue));
                                    itemList.Add(Tuple.Create(dcname, dctype, dcvalue));
                              }
                        }
                        //*DailyCheck4IDRepository key=>lineid+'_'+nodeid+'_SecsDailyCheck4ID'
                        string key = string.Format("{0}_{1}_SecsDailyCheck", eqp.Data.LINEID, eqp.Data.NODEID);
                        Repository.Add(key, dailycheck4IDRepository);

                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F6_09", _common.ToFormatString(recvTrx.OuterXml) });
                                    break;
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S1F6_10_E_SpecialDataInquireforID(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F6_10_E");
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

                        //*specialData4IDRepository Format=> List<Tuple<subeqpid,List<Tuple<dcname,dctype,dcvalue>>>
                        List<Tuple<string, List<Tuple<string, string, string>>>> specialData4IDRepository = new List<Tuple<string, List<Tuple<string, string, string>>>>();

                        //<array1 name="List" type="L" len="5">
                        //  <SFCD name="SFCD" type="A" len="2" fixlen="False" />
                        //  <SUBCD name="SUBCD" type="U4" len="1" fixlen="False" />
                        //  <EQPMODE name="EQPMODE" type="A" len="4" fixlen="False" />
                        //  <EQPST name="EQPST" type="A" len="1" fixlen="False" />
                        //  <array2 name="List (Eqp Count or SubEqp Count)" type="L" len="?">
                        //    <array3 name="List" type="L" len="2">
                        //      <SUBEQUIPMENTID name="SUBEQUIPMENTID" type="A" len="16" fixlen="False" />
                        //      <array4 name="List (number of data items)" type="L" len="?">
                        //        <array5 name="List" type="L" len="3">
                        //          <DCID name="DCID" type="A" len="30" fixlen="False" />
                        //          <DCTYPE name="DCTYPE" type="A" len="3" fixlen="False" />
                        //          <DCVALUE name="DCVALUE" type="A" len="16" fixlen="False" />
                        //        </array5>
                        //      </array4>
                        //    </array3>
                        //  </array2>
                        //</array1>
                        //body
                        string subcd = recvTrx["secs"]["message"]["body"]["array1"]["SUBCD"].InnerText.Trim();
                        string eqpmode = recvTrx["secs"]["message"]["body"]["array1"]["EQPMODE"].InnerText.Trim();
                        string eqpst = recvTrx["secs"]["message"]["body"]["array1"]["EQPST"].InnerText.Trim();
                        XmlNode xNode2 = recvTrx["secs"]["message"]["body"]["array1"]["array2"];
                        string len2 = xNode2.Attributes["len"].InnerText.Trim();
                        int loop2 = 0;
                        int.TryParse(len2, out loop2);
                      //  IList<DailyCheckData> dataFormats = ObjectManager.DailyCheckManager.GetDailyCheckProfile(eqpno);
                        IList<EnergyVisualizationData> dataFormats = ObjectManager.EnergyVisualizationManager.GetEnergyVisualizationProfile(eqpno);
                        for (int i = 0; i < loop2; i++)
                        {
                              string subeqpid = xNode2.ChildNodes[i]["SUBEQUIPMENTID"].InnerText.Trim();
                              //add subeqp
                              List<Tuple<string, string, string>> itemList = new List<Tuple<string, string, string>>();
                              specialData4IDRepository.Add(Tuple.Create(subeqpid, itemList));

                              XmlNode xNode4 = xNode2.ChildNodes[i]["array4"];
                              string len4 = xNode4.Attributes["len"].InnerText.Trim();
                              int loop4 = 0;
                              int.TryParse(len4, out loop4);
                              for (int j = 0; j < loop4; j++)
                              {
                                    string dcid = xNode4.ChildNodes[j]["DCID"].InnerText.Trim();
                                    string dctype = xNode4.ChildNodes[j]["DCTYPE"].InnerText.Trim();
                                    string dcvalue = xNode4.ChildNodes[j]["DCVALUE"].InnerText.Trim();
                                    string dcname = GetDCNAMEForEnergy(dataFormats, dcid);
                                    //add item
                                    //20150604 cy:增加檢查資料不能重覆
                                    int dupIndex = itemList.FindIndex(n => n.Item1 == dcname);
                                    if (dupIndex > -1)
                                    {
                                          _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                      string.Format("Item({0},{1}) duplicate, old value({2}) will change with new value({3})", dcid, dcname, itemList[dupIndex].Item3, dcvalue));
                                          itemList.RemoveAt(dupIndex);
                                    }
                                    //itemList.Add(Tuple.Create(dcid, dctype, dcvalue));
                                    itemList.Add(Tuple.Create(dcname, dctype, dcvalue));
                              }
                        }
                        //*SpecialData4IDRepository key=>lineid+'_'+nodeid+'_SecsSpecialData4IDByReq'
                        string key = string.Format("{0}_{1}_SecsSpecialDataReq", eqp.Data.LINEID, eqp.Data.NODEID);
                        if (eqp.File.SpecialDataEnableForID)
                              Repository.Add(key, specialData4IDRepository);
                        else
                              Repository.Remove(key);

                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F6_10", _common.ToFormatString(recvTrx.OuterXml) });
                                    break;
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S1F13_E_EstablishCommunicationsRequest(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F13_E");
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

                        //reply secondary
                        TS1F14_H_EstablishCommunicationsRequestAcknowledge(eqpno, agent, tid, sysbytes);

                        //20150209 cy:增加通訊狀態
                        eqp.SecsCommunicated = true;
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S1F14_E_EstablishCommunicationsRequestAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F14_E");
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

                        //body
                        string commack = recvTrx["secs"]["message"]["body"]["array1"]["COMMACK"].InnerText.Trim();

                        if (commack == "0")
                              eqp.SecsCommunicated = true;
                        else
                        {
                              eqp.SecsCommunicated = false;
                              _common.LogWarn(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                         "Equipment deny to establish communication.");
                              Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Auto Online Request", string.Format("Equipment({0}) deny to establish communication.", eqp.Data.NODENO) });
                        }
                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F14", _common.ToFormatString(recvTrx.OuterXml) });
                                    break;
                              case "ConnectedCheckContorl":
                                    if (eqp.SecsCommunicated == true)
                                          TS1F5_H_FormattedStatusRequest(eqp.Data.NODENO, eqp.Data.NODEID, "01", "ConnectedCheckContorl", tid);
                                    break;
                              case "AutoOnlineRequest":
                                    if (eqp.SecsCommunicated == true)
                                          TS1F17_H_OnlineRequest(eqp.Data.NODENO, eqp.Data.NODEID, "AutoOnlineRequest", tid);
                                    break;
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void S1F16_E_OfflineAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F16_E");
                  #region Handle Logic
                  try
                  {
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

                        //body
                        string oflack = recvTrx["secs"]["message"]["body"]["OFLACK"].InnerText.Trim();
                        string ackmsg = string.Empty;
                        switch (oflack)
                        {
                              case "0": //accept
                                    ackmsg = "0:OK, Accepted.";
                                    break;
                              case "1":
                                    ackmsg = "1:Denied, at least one constant does not exist.";
                                    break;
                              case "2":
                                    ackmsg = "2:Denied, busy.";
                                    break;
                              case "3":
                                    ackmsg = "3:Denied, at least one constant is outside the range.";
                                    break;
                              default:
                                    ackmsg = string.Format("{0}:Denied, Other Error.", oflack);
                                    break;
                        }
                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                    string.Format("Offline Ack({0})", ackmsg));

                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F16", ackmsg });
                                    break;
                        }

                        if (oflack != "0") //Denied offline,就不會offline scenario,所以刪除
                        {
                              //20150317 cy:online request but already online, terminal timer and do nothing.
                              string timerId = string.Format("S1F15_OfflineRequest_{0}_OPI", eqpno);
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
            public void S1F18_E_OnlineAcknowledge(XmlDocument recvTrx)
            {
                  _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F18_E");
                  #region Handle Logic
                  try
                  {
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
                        string timerId = string.Empty;
                        //body
                        string onlack = recvTrx["secs"]["message"]["body"]["ONLACK"].InnerText.Trim();
                        string ackmsg = string.Empty;
                        switch (onlack)
                        {
                              case "0": //accept
                                    ackmsg = "0:OK, Accepted.";
                                    break;
                              case "1":
                                    ackmsg = "1:NG, Not permit.";
                                    break;
                              case "2": //already online
                                    //query   SFCD 01 : Equipment Information Inquire                    
                                    //TS1F5_H_FormattedStatusRequest(eqpno, eqp.Data.NODEID, "01", string.Empty, tid);
                                    ackmsg = "2:NG, Already Online, Not Accepted.";
                                    //20150611 cy:不管由誰要求online,只要是回2,就都去問資料
                                    TS1F5_H_FormattedStatusRequest(eqp.Data.NODENO, eqp.Data.NODEID, "01", "ConnectedCheckContorl", tid);
                                    break;
                              default:
                                    ackmsg = string.Format("{0}:Other Error, Not Accepted.", onlack);
                                    break;
                        }

                        _common.LogInfo(GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                    string.Format("Online Ack({0})", ackmsg));

                        //check if opi request
                        string rtn = recvTrx["secs"]["message"]["return"].InnerText.Trim();
                        switch (rtn)
                        {
                              case "OPI":
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[4] { tid, eqp.Data.LINEID, "Reply S1F18", ackmsg });
                                    break;
                              case "AutoOnlineRequest":
                                    if (onlack != "0" && onlack != "2")
                                    {
                                          lock (eqp)
                                          {
                                                eqp.File.HSMSControlMode = "OFF-LINE";
                                                //20141217 CY:針對CIM MODE沒PLC的機台，Off-line時，視為CIM OFF，待Online後才切為CIM ON
                                                switch (eqp.Data.REPORTMODE)
                                                {
                                                      case "HSMS_PLC":
                                                            eqp.File.CIMMode = eBitResult.OFF;
                                                            _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                                string.Format("CIM Mode({0}).", eqp.File.CIMMode.ToString()));
                                                            break;
                                                }
                                                ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                                _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                            string.Format("Control Mode({0}).", eqp.File.HSMSControlMode));
                                                //Report to OPI
                                                Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                                                //20161204 yang:CIM Mode要报给MES
                                                if (eqp.Data.REPORTMODE.Equals("HSMS_PLC"))
                                                    Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { tid, eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });
                                          }
                                          Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                                new object[4] { tid, eqp.Data.LINEID, "Auto Online Request", string.Format("Equipment({0}) deny to change online.({1})", eqp.Data.NODENO, ackmsg) });
                                    }
                                    break;
                        }                       
                        if (onlack == "0") //OK會做online scenario
                        {
                              //onlinescenario
                              //check if in onlinescenario process(timer exists)
                              timerId = string.Format("{0}_{1}", eqpno, "SecsOnlineScenario");
                              if (_timerManager.IsAliveTimer(timerId))
                              {
                                    return; //already in onlinescenario process
                              }
                              //start onlinescenario process(create timer)
                              _timerManager.CreateTimer(timerId, false, 30000,
                                  new System.Timers.ElapsedEventHandler(OnlineScenarioTimeOut), tid);
                        }
                        else
                        {
                              //20150317 cy:online request but already online, terminal timer and do nothing.
                              timerId = string.Format("S1F17_OnlineRequest_{0}_OPI", eqpno);
                              if (_timerManager.IsAliveTimer(timerId))
                              {
                                    _timerManager.TerminateTimer(timerId); //remove
                              }
                              timerId = string.Format("S1F17_OnlineRequest_{0}_AUTO", eqpno);
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
            public void TS1F0_H_AbortTransaction(string eqpno, string eqpid, string tid, string sysbytes)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S1F0_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S1F0_H)");
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
            public void TS1F1_H_AreYouThereRequest(string eqpno, string eqpid, string tag, string trxid)
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
            public void TS1F2_H_OnlineData(string eqpno, string eqpid, string tid, string sysbytes)
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
            public void TS1F5_H_FormattedStatusRequest(string eqpno, string eqpid, string sfcd, string tag, string trxid)
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
                        XmlDocument sendTrx = agent.GetTransactionFormat("S1F5_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S1F5_H)");
                              return;
                        }
                        //Set Data
                        sendTrx["secs"]["message"].Attributes["agent"].InnerText = agent.Name;
                        sendTrx["secs"]["message"].Attributes["node"].InnerText = eqpno;
                        sendTrx["secs"]["message"].Attributes["tid"].InnerText = string.IsNullOrEmpty(trxid) ? base.CreateTrxID() : trxid;
                        sendTrx["secs"]["message"]["header"].Attributes["systembytes"].InnerText = string.Empty;
                        sendTrx["secs"]["message"]["return"].Attributes["id"].InnerText = "S1F5_H";
                        sendTrx["secs"]["message"]["return"].InnerText = tag;

                        //body
                        _common.SetItemData(sendTrx["secs"]["message"]["body"]["SFCD"],
                            sfcd.PadLeft(int.Parse(sendTrx["secs"]["message"]["body"]["SFCD"].Attributes["len"].InnerText.Trim()), '0'));

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
                                  string.Format("Can not get agent object with name ({0})", eqpid));
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
            public void TS1F14_H_EstablishCommunicationsRequestAcknowledge(string eqpno, string eqpid, string tid, string sysbytes)
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

                        //<array1 name="List" type="L" len="2">
                        //  <COMMACK name="COMMACK" type="B" len="1" fixlen="False" />
                        //  <array2 name="List" type="L" len="0" />
                        //</array1>
                        //body
                        sendTrx["secs"]["message"]["body"]["array1"].Attributes["len"].InnerText = "2";
                        sendTrx["secs"]["message"]["body"]["array1"]["COMMACK"].InnerText = "0";
                        sendTrx["secs"]["message"]["body"]["array1"]["array2"].Attributes["len"].InnerText = "0";

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
            public void TS1F15_H_OfflineRequest(string eqpno, string eqpid, string tag, string trxid)
            {
                  try
                  {
                        //Get Agent Object
                        IServerAgent agent = GetServerAgent(eqpid);
                        if (agent == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("Can not get agent object with name ({0})", eqpid));
                              if (tag == "OPI")
                              {
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[] { trxid, string.Empty, "Send S1F15", string.Format("SECSAgent name({0}) is not correct.", eqpid) });
                              }
                              return;
                        }
                        //Get Transaction Format
                        XmlDocument sendTrx = agent.GetTransactionFormat("S1F15_H") as XmlDocument;
                        if (sendTrx == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  "Can not get transaction object with name (S1F15_H)");
                              if (tag == "OPI")
                              {
                                    Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                        new object[] { trxid, string.Empty, "Send S1F15", "Transaction name(S1F15_H) is not exist." });
                              }
                              return;
                        }
                        //20150317 cy:若是由OPI要求的,設定計時器,時間內有任何異常,丟訊息給OPI
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
                              _timerManager.CreateTimer(timerId, false, _CSOTSECSDATATIMEOUT,
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
            public void TS1F17_H_OnlineRequest(string eqpno, string eqpid, string tag, string trxid)
            {
                  try
                  {
                        //Get Agent Object
                        IServerAgent agent = GetServerAgent(eqpid);
                        if (agent == null)
                        {
                              NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()",
                                  string.Format("Can not get agent object with name ({0})", eqpid));
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
                        //20150317 cy:若是由OPI要求的,設定計時器,時間內有任何異常,丟訊息給OPI
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
                              _timerManager.CreateTimer(timerId, false, _CSOTSECSDATATIMEOUT,
                                  new System.Timers.ElapsedEventHandler(OpiOnlineRequestTimeOut), trxid);
                        }
                        if (tag == "AutoOnlineRequest")
                        {
                              string timerId = string.Format("S1F17_OnlineRequest_{0}_AUTO", eqpno);
                              if (_timerManager.IsAliveTimer(timerId))
                              {
                                    _timerManager.TerminateTimer(timerId); //remove old
                              }
                              //create wait timer (50秒,使之大於T3)
                              _timerManager.CreateTimer(timerId, false, 50000,
                                  new System.Timers.ElapsedEventHandler(AutoOnlineRequestTimeOut), trxid);
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
            public void S1F0_H_AbortTransaction(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F0_H T3-Timeout", false);
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F0_H", false);
                  }
                  catch (Exception ex)
                  {
                        NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }
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
            public void S1F2_H_OnlineData(XmlDocument recvTrx, bool timeout)
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
            public void S1F5_H_FormattedStatusRequest(XmlDocument recvTrx, bool timeout)
            {
                  try
                  {
                        if (timeout)
                        {
                              _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F5_H T3-Timeout", false);
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
                                          eqp.File.HSMSControlMode = "OFF-LINE";
                                          //20150918 t3:針對report mode是HSMS_開頭的,就去更新CIM Mode. (Modify by CY)
                                          switch (eqp.Data.REPORTMODE)
                                          {
                                                case "HSMS_PLC":
                                                      eqp.File.CIMMode = eBitResult.OFF;
                                                      break;
                                          }
                                          Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                                          //20161204 yang:CIM Mode要报给MES
                                          if (eqp.Data.REPORTMODE.Equals("HSMS_PLC"))
                                              Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { tid, eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });
                                          break;
                                    case "OPI":
                                          Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S1F5", "T3 Timeout" });
                                          break;
                              }
                              return;
                        }
                        _common.LogAgentRaiseEvent(recvTrx, GetType().Name, MethodInfo.GetCurrentMethod().Name, "S1F5_H", false);
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
                                    case "OPI":
                                          Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S1F13", "T3 Timeout" });
                                          break;
                                    case "AutoOnlineRequest":
                                          
                                          Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                                new object[4] { tid, eqp.Data.LINEID, "Auto Online Request", string.Format("Host establish communication with Equipment({0}) T3 timeout.", eqp.Data.NODENO) });
                                          break;
                              }
                              lock (eqp)
                              {
                                    eqp.File.HSMSControlMode = "OFF-LINE";
                                    //20141217 CY:針對CIM MODE沒PLC的機台，Off-line時，視為CIM OFF，待Online後才切為CIM ON
                                    switch (eqp.Data.REPORTMODE)
                                    {
                                          case "HSMS_PLC":
                                                eqp.File.CIMMode = eBitResult.OFF;
                                                _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                    string.Format("CIM Mode({0}).", eqp.File.CIMMode.ToString()));
                                                break;
                                    }
                                    ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                    _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                string.Format("Control Mode({0}).", eqp.File.HSMSControlMode));
                                    //Report to OPI
                                    Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                                    //20161204 yang:CIM Mode要报给MES
                                    if (eqp.Data.REPORTMODE.Equals("HSMS_PLC"))
                                        Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { tid, eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });
                              }
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
            public void S1F15_H_OfflineRequest(XmlDocument recvTrx, bool timeout)
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
                                    _timerManager.TerminateTimer(timerId); //remove old
                              }

                              switch (rtn)
                              {
                                    case "OPI":
                                          Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                              new object[4] { tid, eqp.Data.LINEID, "Rqeuest S1F15", string.Format("[EQUIPMENT={0}] Offline request Fail. S1F15 T3 Timeout.", eqp.Data.NODENO) });
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
            public void S1F17_H_OnlineRequest(XmlDocument recvTrx, bool timeout)
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

                              switch (rtn)
                              {
                                    case "OPI":
                                          {
                                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                                    new object[4] { tid, eqp.Data.LINEID, "Rqeuest S1F17", string.Format("[EQUIPMENT={0}] Online request Fail. S1F17 T3 Timeout.", eqp.Data.NODENO) });
                                                break;
                                          }
                                    case "AutoOnlineRequest":
                                          {
                                                Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                                      new object[4] { tid, eqp.Data.LINEID, "Auto Online Request", string.Format("Host request Equipment({0}) online T3 timeout.", eqp.Data.NODENO) });
                                                string timerId = string.Format("S1F17_OnlineRequest_{0}_AUTO", eqpno);
                                                if (_timerManager.IsAliveTimer(timerId))
                                                {
                                                      _timerManager.TerminateTimer(timerId); //remove old
                                                }
                                                break;
                                          }
                              }
                              lock (eqp)
                              {
                                    eqp.File.HSMSControlMode = "OFF-LINE";
                                    //20141217 CY:針對CIM MODE沒PLC的機台，Off-line時，視為CIM OFF，待Online後才切為CIM ON
                                    switch (eqp.Data.REPORTMODE)
                                    {
                                          case "HSMS_PLC":
                                                eqp.File.CIMMode = eBitResult.OFF;
                                                _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                    string.Format("CIM Mode({0}).", eqp.File.CIMMode.ToString()));
                                                break;
                                    }
                                    ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                                    _common.LogInfo(GetType().Name, MethodBase.GetCurrentMethod().Name + "()", eqpno, true, tid,
                                                string.Format("Control Mode({0}).", eqp.File.HSMSControlMode));
                                    //Report to OPI
                                    Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[2] { tid, eqp });
                                    //20161204 yang:CIM Mode要报给MES
                                    if (eqp.Data.REPORTMODE.Equals("HSMS_PLC"))
                                        Invoke(eServiceName.MESMessageService, "CIMModeChangeReport", new object[] { tid, eqp.Data.LINEID, eqp.Data.NODEID, eqp.File.CIMMode });
                              }
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

            private void AutoOnlineRequestTimeOut(object subject, System.Timers.ElapsedEventArgs e)
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

                  string msg = string.Format("[EQUIPMENT={0}] [{1}] Online request from BCS do not action. Check if request transaction sent and equipment responted.", eqpno, tid);
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