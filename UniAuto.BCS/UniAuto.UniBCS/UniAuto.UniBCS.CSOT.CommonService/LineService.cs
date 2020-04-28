using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniBCS.Core.Message;
using System.Reflection;
using System.Threading;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.Core;
using System.Timers;

namespace UniAuto.UniBCS.CSOT.CommonService
{
    public class LineService : AbstractService
    {
        private string _aliveValue = "1";
        private DateTime _plcStatusChangeTime;
        private Thread  _aliveThread;
        private Thread _calculationTactTime;
        private bool _isRuning=false;
        private Thread _dummyRequest;


        public override bool Init()
        {
            _aliveThread = new Thread(new ThreadStart(BCStatus)) { IsBackground = true };
            _aliveThread.Start();

            // 程序初始化後, 太早判斷會造成資料尚未Reload到BCS, 
            // 設定30秒再去判斷是否要開始一個線程去計算 Array Tact Time
            _timerManager.CreateTimer("StartArrayTactTime", false, 30000, 
                new System.Timers.ElapsedEventHandler(StartArrayTactTime));

            _timerManager.CreateTimer("StartDummyRequest", false, 30000,
                new System.Timers.ElapsedEventHandler(StartDummyRequest));
            return true;
        }

        /// <summary>
        /// PLC連線資訊
        /// </summary>
        public void PLCStatusChange(string StationNo, string Status)
        {
            try
            {
                if (eAGENT_STATE.CONNECTED == Status)
                {
                    Logger.LogInfoWrite(LogName,this.GetType().Name,MethodBase.GetCurrentMethod().Name+"()",
                        string.Format("PLC_STATION_NO=[{0}] IS CONNECTED.",StationNo));
                    if (StationNo == "255" || StationNo == "254")
                    {
                        #region [Clear All BIT]
                        IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();

                        foreach (Equipment n in eqps)
                        {
                            if (!n.Data.REPORTMODE.Contains("PLC")) //2015/09/16 cc.kuang
                                continue;
                            Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(n.Data.NODENO + "_AllBitOff") as Trx;
                            if (outputdata != null)
                            {
                                for (int i = 0; i < outputdata.EventGroups[0].Events.Count; i++)
                                {
                                    outputdata.EventGroups[0].Events[i].Items[0].Value = "0";
                                }
                                outputdata.TrackKey = UtilityMethod.GetAgentTrackKey();
                                SendPLCData(outputdata);
                            }
                        }
                        #endregion

                        Line line = ObjectManager.LineManager.GetLine(Workbench.ServerName);

                        #region Watson Add For PMT100
                        if (Workbench.ServerName.Contains(eLineType.CELL.CBPMT))
                        {
                            List<Line> linelist = ObjectManager.LineManager.GetLines();
                            if (linelist[0] != null)
                            {
                                line = linelist[0];
                            }
                        }
                        #endregion

                        if (line == null)
                        {
                            this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[LINENAME={0}] [BCS -> MES] BCS NOT FOUND LINE OBJECT.", Workbench.ServerName));
                            return;
                        }
                        if (!_isRuning && line.Data.FABTYPE == eFabType.CF.ToString())
                        {
                            Invoke(eServiceName.SubBlockService, "StartThread", new object[] { });
                        }
                        _isRuning = true;

                        bool IsSpecialHandle = false;
                        switch (Workbench.LineType)
                        {
                            /* t3 not use cc.kuang 2015/07/03
                            case eLineType.ARRAY.ILC:
                                foreach (Equipment n in eqps)
                                {
                                    if (n.Data.NODENO == "L2") continue;
                                    if (string.IsNullOrEmpty(n.File.EquipmentRunMode.Trim()))
                                    {
                                        n.File.EquipmentRunMode = "ILC";
                                        ObjectManager.EquipmentManager.EnqueueSave(n.File);
                                    }
                                    string no = n.Data.NODENO.Remove(0, 1).PadLeft(2, '0');
                                    Invoke(eServiceName.ArraySpecialService, "FLCModeInUseLocal", new object[]{
                                        no, !n.File.EquipmentRunMode.Equals("ILC"), UtilityMethod.GetAgentTrackKey()
                                    });
                                }
                                IsSpecialHandle = true;
                                break; */
                            case eLineType.ARRAY.CHN_SEEC:
                            case eLineType.CF.FCSRT_TYPE1: IsSpecialHandle = true; break;

                        }
                        if (IsSpecialHandle)
                        {
                            CheckLineRunMode(line.Data.LINEID);
                        }
                    
                        line.File.PLCStatus = Status;
                        Thread.Sleep(3000);//此处等1秒 防止PLC Agent 报错
                        Invoke(eServiceName.UIService, "LineStatusReport", new object[] { this.CreateTrxID(), line });
                    }
                }
                else
                {
                    if (StationNo == "255" || StationNo == "254") //MPLC
                    {
                        Line line = ObjectManager.LineManager.GetLine(Workbench.ServerName);
                        if (line == null)
                        {
                            this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[LINENAME={0}] [BCS -> MES] BCS NOT FOUND LINE OBJECT.", Workbench.ServerName));
                            return;
                        }

                        line.File.PLCStatus = Status;
                        Invoke(eServiceName.UIService, "LineStatusReport", new object[] { this.CreateTrxID(), line });
                    }
                    Logger.LogWarnWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("PLC STATION NO =[{0}] IS DISCONNECTED.", StationNo));
                    _isRuning = false;
                }
                if (StationNo == "255" || StationNo == "254")
                {

                    if (_plcStatusChangeTime != null)
                    {
                        //连线与断线时间超过十秒需要检查PC卡可能有问题了  20150315 Tom
                        if (new TimeSpan(DateTime.Now.Ticks - _plcStatusChangeTime.Ticks).TotalMilliseconds < 10 * 1000)
                        {
                            Line line = ObjectManager.LineManager.GetLine(Workbench.ServerName);
                            #region  Add For PMT100
                            if (Workbench.ServerName.Contains(eLineType.CELL.CBPMT))
                            {
                                List<Line> linelist = ObjectManager.LineManager.GetLines();
                                if (linelist[0] != null)
                                {
                                    line = linelist[0];
                                }
                            }
                            #endregion

                            if (line != null)
                            {
                                if (line.File.OnBoradCardAlive) //将卡标记为不稳定的
                                    lock(line)
                                        line.File.OnBoradCardAlive = false;
                            }
                        }
                    }
                    _plcStatusChangeTime = DateTime.Now;
                }
                
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// BC Status
        /// </summary>
        public void BCStatus()
        {

            List<string> eqpEvents = new List<string>();
            while (true)
            {
                try
                {
                    if (_isRuning)
                    {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat("L1_BCStatus") as Trx;
                        if (outputdata != null)//在BC關閉的過程會取不到trx
                        {                            
                            outputdata.EventGroups[0].Events[0].Items[0].Value = _aliveValue;
                            outputdata.TrackKey = UtilityMethod.GetAgentTrackKey();
                            xMessage msg = new xMessage();
                            msg.ToAgent = eAgentName.PLCAgent;
                            msg.Data = outputdata;
                            PutMessage(msg);

                            _aliveValue = _aliveValue.Equals("1") ? "0" : "1";
                        }
                    }
                    int param = ParameterManager["BCALIVE"].GetInteger();

                    Thread.Sleep(param);
                    #region [CheckBitTimeOut]
                    List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                    if (_isRuning)
                    {

                        foreach (Equipment eqp in eqps)
                        {
                            if (eqp.Data.REPORTMODE.Split('_')[0] != "PLC")
                            {
                                // 非PLC 不用去檢查
                            }
                            else
                            {
                                string trxName = eqp.Data.NODENO + "_AllBitOff";
                                Trx data = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as Trx;
                                if (data != null)
                                {
                                    for (int i = 0; i < data.EventGroups[0].Events.Count; i++)
                                    {
                                        string eqpEvent = string.Empty;
                                        eqpEvent = eqpEvents.FirstOrDefault(j => j.Contains(eqp.Data.NODENO + ";" + data.EventGroups[0].Events[i].Name.ToString() + ";"));//sy add modify event & event#01 搜尋問題 用event; & event#01; 來區分

                                        if (data.EventGroups[0].Events[i].Name.ToString().Contains("MPLCInterlockCommand"))
                                        {
                                            
                                        }
                                        else if (data.EventGroups[0].Events[i].Items[0].Value == "0")
                                        {
                                            if (String.IsNullOrEmpty(eqpEvent))
                                            {
                                            }
                                            else
                                            {
                                                eqpEvents.Remove(eqpEvent);
                                            }
                                        }
                                        else if (data.EventGroups[0].Events[i].Items[0].Value == "1")
                                        {
                                            if (String.IsNullOrEmpty(eqpEvent))
                                            {
                                                eqpEvents.Add(eqp.Data.NODENO + ";" + data.EventGroups[0].Events[i].Name.ToString() + ";" + DateTime.Now.ToString("yyyyMMddHHmmssfff", System.Globalization.DateTimeFormatInfo.InvariantInfo));
                                            }
                                            else
                                            {
                                                string diffDate = (long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssfff", System.Globalization.DateTimeFormatInfo.InvariantInfo)) - long.Parse(eqpEvent.Split(';')[2])).ToString().PadLeft(17, '0');
                                                long diffDateInt = int.Parse(diffDate.Substring(12, 5)) + int.Parse(diffDate.Substring(10, 2)) * 1000 * 60 + int.Parse(diffDate.Substring(8, 2)) * 1000 * 60 * 60 + int.Parse(diffDate.Substring(6, 2)) * 1000 * 60 * 60 * 24
                                                   + int.Parse(diffDate.Substring(4, 2)) * 1000 * 60 * 60 * 24 * 30 + int.Parse(diffDate.Substring(0, 4)) * 1000 * 60 * 60 * 24 * 30 * 365;
                                                //if (long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssfff", System.Globalization.DateTimeFormatInfo.InvariantInfo)) - long.Parse(eqpEvent.Split(';')[2]) > ParameterManager["BCBITTIMEOUT"].GetInteger())
                                                if (diffDateInt > ParameterManager["BCBITTIMEOUT"].GetInteger())
                                                {
                                                    eqpEvents.Remove(eqpEvent);
                                                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", data.EventGroups[0].Events[i].Name.ToString() + "_Bit error BC Auto off");
                                                    ControlOneOfAllBitOff(eqp.Data.NODENO, i, eBitResult.OFF);
                                                }
                                                else
                                                {
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    #endregion
                    
                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }                
            }
        }
        public void ControlOneOfAllBitOff(string nodeNo,int index ,eBitResult bit )
        {
            Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(nodeNo + "_AllBitOff") as Trx;
            if (outputdata != null)
            {
                for (int i = 0; i < outputdata.EventGroups[0].Events.Count; i++)
                {
                    if (i == index)
                        outputdata.EventGroups[0].Events[i].Items[0].Value = "0";
                    else
                        outputdata.EventGroups[0].Events[i].IsDisable = true;
                }
                outputdata.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputdata);
            }
        }

        /// <summary>
        /// Check Control Mode Change Condition
        /// </summary>
        /// <param name="trxid"></param>
        /// <param name="lineName"></param>
        /// <param name="controlMode">REMOTE/LOCAL/OFFLINE</param>
        /// <returns>
        /// Y’ – Accepted
        ///‘N’ – Not accepted
        ///‘A’ – Already in requested state
        /// </returns>
        public string CheckControlCondition(string trxid,string lineName,string controlMode, ref string retMessage)
        {
            retMessage = "";
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineName);
                if (line == null)
                {
                    retMessage = string.Format("[LINENAME={0}] [BCS -> MES][{1}] BCS IS NOT FOUND LINE OBJECT.", ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxid);
                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", retMessage);
                    return "N";//Node Foud Line 
                }
                if (line.File.HostMode.ToString() == controlMode)
                {
                    retMessage = string.Format("[LINENAME={0}] [BCS -> MES][{1}] CURRENT CONTROL=[{2}] EQUAL TO=[{2}].", ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxid, controlMode);
                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", retMessage);
                    return "A";
                }
                else //if (line.File.HostMode.ToString() !=controlMode )
                {

                    if (controlMode == eHostMode.OFFLINE.ToString())
                    {
                        retMessage = string.Format("[LINENAME={0}] [BCS -> MES][{1}] CHANGE CONTROL MODE TO OFFLINE.", ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxid);
                        this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", retMessage);
                        return "Y";
                    }

                    switch (line.Data.LINETYPE)
                    {
                        case eLineType.ARRAY.CVD_AKT:
                        //case eLineType.ARRAY.CVD_ULVAC:
                        case eLineType.ARRAY.DRY_ICD:
                        case eLineType.ARRAY.DRY_TEL:  //add by yang 
                        // case eLineType.ARRAY.DRY_YAC:
                            Equipment eqp = ObjectManager.EquipmentManager.GetEQP("L4");
                            if (eqp != null)
                            {
                                if (eqp.HsmsConnected == false)
                                {
                                    retMessage = string.Format("[LINENAME={0}] EQUIPMENT=[{1}] HSMS IS DISCONNECTED!", 
                                        line.Data.LINENAME, eqp.Data.NODENO);
                                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", 
                                        "CHANG_CONTROL_MODE NG, " + retMessage);
                                    return "N";
                                }
                                else if (string.IsNullOrEmpty(eqp.File.EquipmentRunMode))
                                {
                                    retMessage = string.Format("[LINENAME={0}] EQUIPMENT=[{1}] BCS didn't receive the message(S6F11 CEID=02 Equipment Mode Change)!",
                                        line.Data.LINENAME, eqp.Data.NODENO);
                                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        "CHANG_CONTROL_MODE NG, " + retMessage);
                                    return "N";
                                }
                            }
                            break;
                        //20160718 add by Frank For Check "ARRAY Dummy Request" Special Rule
                        case eLineType.ARRAY.MSP_ULVAC:
                        case eLineType.ARRAY.ITO_ULVAC:
                            {
                                if (line.File.DummyRequestFlag && controlMode == "REMOTE")
                                {
                                    retMessage = string.Format("[LINENAME={0}] DUMMY REQUEST BIT (ON)!",line.Data.LINENAME);
                                    this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        "CHANG_CONTROL_MODE NG, " + retMessage);
                                    return "N";
                                }                               
                            }
                            break;
                    }

                    IList<Port> ports = ObjectManager.PortManager.GetPortsByLine(line.Data.LINEID) ;
                   
                    if (ports != null)
                    {
                        foreach (Port port in ports)
                        {
                            if(port.File.CassetteStatus==eCassetteStatus.WAITING_FOR_CASSETTE_DATA||
                                port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_START_COMMAND)
                            {
                                retMessage = string.Format("[LINENAME={0}] [BCS -> MES][{1}] CHANGE_CONTROL_MODE=[{2}] EQUIPMENT=[{3}] PORT=[{4}] CST_STATUS=[{5}]({6}).",
                                    ObjectManager.LineManager.GetLineID(line.Data.LINEID), trxid, controlMode, port.Data.NODENO, port.Data.PORTID, (int)port.File.CassetteStatus,port.File.CassetteStatus.ToString());
                               this.Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", retMessage);

                               string err = string.Format("CHANG_CONTROL_MODE NG LINENAME =[{0}],CHANGE_CONTROL_MODE=[{1}] EQUIPMENT=[{2}] PORT=[{3}] CST_STATUS=[{4}]({5}).",
                                   ObjectManager.LineManager.GetLineID(line.Data.LINEID), controlMode, port.Data.NODENO, port.Data.PORTID, (int)port.File.CassetteStatus, port.File.CassetteStatus.ToString());
                               Invoke(eServiceName.UIService, "BCSTerminalMessageInform",
                                   new object[] { trxid, line.Data.LINEID, "[" + MethodBase.GetCurrentMethod().Name + "] " + err });

                                return "N";
                            }
                        }
                    }
                    return "Y";
                }
               // return "N";
            }
            catch (System.Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return "N";
            }
        }

        /// <summary>
        /// 啟動 Array 計算Receive Glass Tact Time 的線程
        /// </summary>
        private void StartArrayTactTime(object sender, ElapsedEventArgs e)
        {
            if (_timerManager.IsAliveTimer("StartArrayTactTime"))
                _timerManager.TerminateTimer("StartArrayTactTime");
            Line line = ObjectManager.LineManager.GetLine(Workbench.ServerName);
            if (line != null && (line.Data.FABTYPE == eFabType.ARRAY.ToString()))
            {
                _calculationTactTime = new Thread(new ThreadStart(CalculationTactTime)) { IsBackground = true };
                _calculationTactTime.Start();
            }
        }

        /// <summary>
        /// 啟動 Array 計算Receive Glass Tact Time 的線程
        /// </summary>
        private void StartDummyRequest(object sender, ElapsedEventArgs e)
        {
            if (_timerManager.IsAliveTimer("StartDummyRequest"))
                _timerManager.TerminateTimer("StartDummyRequest");
            Line line = ObjectManager.LineManager.GetLine(Workbench.ServerName);
            if (line != null && (line.Data.LINETYPE == eLineType.ARRAY.MSP_ULVAC || line.Data.LINETYPE == eLineType.ARRAY.ITO_ULVAC))
            {
                _dummyRequest = new Thread(new ThreadStart(DummyRequestMonotor)) { IsBackground = true };
                _dummyRequest.Start();
            }
        }

        /// <summary>
        /// Array 計算Receive Glass Tact Time的線程
        /// </summary>
        private void CalculationTactTime()
        {
            while (true)
            {
                try
                {
                    List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs().Where(e => e.Data.NODENO != "L2").ToList();

                    foreach (Equipment eqp in eqps)
                    {
                        if (eqp.File.TactTimeOut) continue;

                        if (string.IsNullOrEmpty(eqp.File.FinalReceiveGlassTime))
                        {
                            lock (eqp.File) eqp.File.FinalReceiveGlassTime = string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now);
                            ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                        }

                        DateTime eqpRcvTime = DateTime.Parse(eqp.File.FinalReceiveGlassTime);

                        if (DateTime.Now.Subtract(eqpRcvTime).TotalMinutes > ParameterManager["TACTTIME"].GetInteger())
                        {
                            lock (eqp.File) eqp.File.TactTimeOut = true;
                            Invoke(eServiceName.UIService, "EquipmentStatusReport", new object[] { UtilityMethod.GetAgentTrackKey(), eqp });
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
                Thread.Sleep(5000);
            }
        }
        //==========

        /// <summary>
        /// 啟動 Array Dummy Request的線程
        /// </summary>
        private void DummyRequestMonotor()
        {
            while (true)
            {
                try
                {
                    Line line = ObjectManager.LineManager.GetLine(Workbench.ServerName);
                    Trx _trxData1 = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { "L4_DownstreamPath#01", false }) as Trx;
                    Trx _trxData2 = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { "L4_DownstreamPath#02", false }) as Trx;

                    if (_trxData1 == null || _trxData2 == null)
                        continue;

                    if (_trxData1.EventGroups[0].Events[0].Items["DownstreamPath#01DummyGlassRequest"].Value == "1" ||
                        _trxData2.EventGroups[0].Events[0].Items["DownstreamPath#02DummyGlassRequest"].Value == "1")
                    {
                        if (line.File.DummyRequestFlag)
                            continue;
                        else 
                        {
                            if (line.File.HostMode == eHostMode.REMOTE)
                            {
                                lock (line)
                                    line.File.DummyRequestFlag = true;
                                Invoke(eServiceName.MESService, "MachineControlStateChanged", new object[] { UtilityMethod.GetAgentTrackKey(), line.Data.LINEID, "LOCAL" });
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                string.Format("[EQUIPMENT=L4] DUMMY REQUEST BIT (ON), CHANGE TO LOCAL MODE."));
                            }
                            else
                            {
                                lock (line)
                                    line.File.DummyRequestFlag = true;
                            }
                        } 
                    }

                    if (_trxData1.EventGroups[0].Events[0].Items["DownstreamPath#01DummyGlassRequest"].Value == "0" &&
                        _trxData2.EventGroups[0].Events[0].Items["DownstreamPath#02DummyGlassRequest"].Value == "0")
                    {
                        if (line.File.DummyRequestFlag)
                        {
                            lock (line)
                                line.File.DummyRequestFlag = false;
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                    string.Format("[EQUIPMENT=L4] ALL DUMMY REQUEST BIT (OFF)."));
                        }
                    }

                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
                Thread.Sleep(5000);
            }
 
        }

        private void SendPLCData(Trx outputData)
        {
            xMessage msg = new xMessage();
            msg.Data = outputData;
            msg.ToAgent = eAgentName.PLCAgent;
            PutMessage(msg);
        }

        /// <summary>
        /// Check Line Stete
        /// Report MES  LineStateChanged
        /// </summary>
        /// <param name="trxId">Trx ID</param>
        /// <param name="lineId"> Line ID </param>
        public void CheckLineState(string trxId,string lineId)
        {
            try
            {
                Line line=ObjectManager.LineManager.GetLine(lineId);
                string lineState = ObjectManager.LineManager.GetLineState(lineId);
                if ((string.IsNullOrEmpty(lineState)==false )&& (lineState.ToUpper() != line.File.Status.ToString().ToUpper()))
                {
                    LogInfo(MethodBase.GetCurrentMethod().Name, string.Format("[LINENAME={0}] [BCS -> MES][{1}] STATUS FROM [{2}] TO [{3}].",
                        lineId, trxId, line.File.Status.ToString(), lineState));
                    lock(line)line.File.Status = lineState;//(eEQPStatus)Enum.Parse(typeof(eEQPStatus), lineState);
                    Invoke(eServiceName.MESService, "LineStateChanged", new object[] { trxId, line.Data.LINEID, lineState});
                    ObjectManager.LineManager.EnqueueSave(line.File);   //add by bruce 20160120 將line state存入檔案
                    ObjectManager.LineManager.RecordLineHistory(trxId,line);//记录LineHistory 20150428 tom
                }
            }
            catch (System.Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name+"()", ex.ToString());
            }
        }

        /// <summary>
        /// 檢查並設定該LINE的LINEOPERMODE
        /// </summary>
        /// <param name="lineID"></param>
        public void CheckLineRunMode(string lineID)
        {
            try
            {
                Line line = ObjectManager.LineManager.GetLine(lineID);
                if (line == null) new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", lineID));

                if (line.Data.FABTYPE == eFabType.ARRAY.ToString())
                {
                    if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                    {
                        line.File.LineOperMode = eMES_LINEOPERMODE.CHANGER;
                        ObjectManager.LineManager.EnqueueSave(line.File);
                        return;
                    }
                }

                switch (line.Data.LINETYPE)
                {
                    #region[ARRAY CHECK RUN MODE]
                    case eLineType.ARRAY.MSP_ULVAC:
                    case eLineType.ARRAY.ITO_ULVAC:
                    case eLineType.ARRAY.CVD_AKT:
                    case eLineType.ARRAY.CVD_ULVAC:
                    case eLineType.ARRAY.DRY_YAC:
                    case eLineType.ARRAY.DRY_ICD:
                    case eLineType.ARRAY.DRY_TEL:
                        {
                            Equipment eqp = ObjectManager.EquipmentManager.GetEQPsByLine(lineID).FirstOrDefault(
                                L => L.Data.NODENO == "L4");
                            if (eqp != null)
                            {
                                lock (line) line.File.LineOperMode = eqp.File.EquipmentRunMode;
                            }
                        }
                        break;
                    case eLineType.ARRAY.CHN_SEEC:
                        {
                            if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.SAMPLING_MODE)
                                lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.EXCHANGE;
                        }
                        break;
                    case eLineType.ARRAY.ELA_JSW:
                        {
                            Equipment eqp1 = ObjectManager.EquipmentManager.GetEQPsByLine(lineID).FirstOrDefault(
                                L => L.Data.NODENO == "L4");
                            Equipment eqp2 = ObjectManager.EquipmentManager.GetEQPsByLine(lineID).FirstOrDefault(
                                L => L.Data.NODENO == "L5");
                            if (eqp1 != null && eqp2 != null)
                            {
                                if (eqp1.File.EquipmentRunMode.Trim().Length > 0 && eqp2.File.EquipmentRunMode.Trim().Length > 0)
                                {
                                    if (eqp1.File.EquipmentRunMode.Trim() == eqp2.File.EquipmentRunMode.Trim())
                                        lock (line) line.File.LineOperMode = "NORMAL";
                                    else
                                        lock (line) line.File.LineOperMode = "MIX";
                                }
                                else
                                {
                                    lock (line) line.File.LineOperMode = "NORMAL";
                                }
                            }
                        }
                        break;
                    #endregion

                    #region[CF CHECK RUN MODE]
                    case eLineType.CF.FCSRT_TYPE1:
                        {
                            lock (line)
                            {
                                switch (line.File.IndexOperMode)
                                {
                                    case eINDEXER_OPERATION_MODE.SORTER_MODE: lock(line) line.File.LineOperMode = eMES_LINEOPERMODE.SORTER; break;
                                    case eINDEXER_OPERATION_MODE.CHANGER_MODE: lock(line) line.File.LineOperMode = eMES_LINEOPERMODE.CHANGER; break;
                                    default: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.NORMAL; break;
                                }
                            }
                        }
                        break;
                    case eLineType.CF.FCUPK_TYPE1:
                        {
                            Equipment eqp = ObjectManager.EquipmentManager.GetEQP("L2");
                            if (eqp != null)
                            {
                                lock (line) line.File.LineOperMode = eqp.File.EquipmentRunMode;
                            }
                        }
                        break;
                    case eLineType.CF.FCMAC_TYPE1:
                        {
                            switch (line.File.IndexOperMode)
                            {
                                case eINDEXER_OPERATION_MODE.CHANGER_MODE: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.CHANGER; break;
                                case eINDEXER_OPERATION_MODE.SORTER_MODE: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.SORTER; break;
                                case eINDEXER_OPERATION_MODE.MQC_MODE: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.MQC; break;
                                case eINDEXER_OPERATION_MODE.THROUGH_MODE: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.CF_THROUGH; break;
                                case eINDEXER_OPERATION_MODE.SAMPLING_MODE:
                                    {
                                        List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPsByLine(lineID).Where(e => e.Data.NODENO != "L2").ToList();
                                        if (eqps.Select(e => e.File.EquipmentRunMode).Distinct().Count() > 1)
                                        {
                                            lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.MIX;
                                        }
                                        else
                                        {
                                            lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.NORMAL;
                                        }
                                    }
                                    break;
                                default: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.NORMAL; break;
                            }
                        }
                        break;
                    case eLineType.CF.FCREP_TYPE1:
                    case eLineType.CF.FCREP_TYPE2:
                    case eLineType.CF.FCREP_TYPE3:
                        {
                            //if (line.File.IndexOperMode != eINDEXER_OPERATION_MODE.CHANGER_MODE && line.File.IndexOperMode != eINDEXER_OPERATION_MODE.SORTER_MODE)
                            //{
                            //    List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPsByLine(lineID).Where(e => e.Data.NODENO != "L2" && e.File.EquipmentRunMode != "0").ToList();
                            //    if (eqps.Select(e => e.File.EquipmentRunMode).Distinct().Count() > 1)
                            //    {
                            //        lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.MIX;
                            //    }
                            //    else
                            //    {
                            //        lock (line) line.File.LineOperMode = eqps[0].File.EquipmentRunMode;
                            //    }
                            //}
                            //else if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.CHANGER_MODE)
                            //{
                            //    lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.CHANGER;
                            //}
                            //else if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.SORTER_MODE)
                            //{
                            //    lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.SORTER;
                            //}
                            //else if (line.File.IndexOperMode == eINDEXER_OPERATION_MODE.THROUGH_MODE)
                            //{
                            //    //20150128 Add by Frank T3經李泉和MES鄒良棟確認後，Repair需增加THROUGH Mode
                            //    lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.CF_THROUGH;
                            //}
                            lock (line)
                            {
                                switch (line.File.IndexOperMode)
                                {
                                    case eINDEXER_OPERATION_MODE.SORTER_MODE: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.SORTER; break;
                                    case eINDEXER_OPERATION_MODE.CHANGER_MODE: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.CHANGER; break;
                                    case eINDEXER_OPERATION_MODE.THROUGH_MODE: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.CF_THROUGH; break;//20150128 Add by Frank T3經李泉和MES鄒良棟確認後，Repair需增加THROUGH Mode
                                    default:
                                        List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPsByLine(lineID).Where(e => e.Data.NODENO != "L2" && e.File.EquipmentRunMode != "0").ToList();
                                        if (eqps.Select(e => e.File.EquipmentRunMode).Distinct().Count() > 1)
                                        {
                                            lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.MIX;
                                        }
                                        else
                                        {
                                            lock (line) line.File.LineOperMode = eqps[0].File.EquipmentRunMode;
                                        }
                                        break;
                                }
                            }
                        }
                        break;
                    case eLineType.CF.FCREW_TYPE1:
                        {
                            lock (line)
                            {
                                switch (line.File.IndexOperMode)
                                {
                                    case eINDEXER_OPERATION_MODE.SORTER_MODE: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.SORTER; break;
                                    case eINDEXER_OPERATION_MODE.CHANGER_MODE: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.CHANGER; break;
                                    default: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.NORMAL; break;
                                }
                            }
                            break;
                        }
                    case eLineType.CF.FCPSH_TYPE1:                  
                        {
                            lock (line)
                            {
                                switch (line.File.IndexOperMode)
                                {
                                    case eINDEXER_OPERATION_MODE.MQC_MODE: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.MQC; break;
                                    default: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.NORMAL; break;
                                }
                            }
                            break;
                        }
                    case eLineType.CF.FCAOI_TYPE1:
                        {
                            lock (line)
                            {
                                switch (line.File.IndexOperMode)
                                {
                                    case eINDEXER_OPERATION_MODE.MQC_MODE: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.MQC; break;
                                    case eINDEXER_OPERATION_MODE.THROUGH_MODE: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.CF_THROUGH; break;
                                    default: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.NORMAL; break;
                                }
                            }
                            break;
                        }
                    case eLineType.CF.FCMQC_TYPE1:
                    case eLineType.CF.FCMQC_TYPE2:
                        {
                            lock (line)
                            {
                                switch (line.File.IndexOperMode)
                                {
                                    case eINDEXER_OPERATION_MODE.SORTER_MODE: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.SORTER; break;
                                    case eINDEXER_OPERATION_MODE.CHANGER_MODE: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.CHANGER; break;
                                    case eINDEXER_OPERATION_MODE.THROUGH_MODE: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.CF_THROUGH; break;
                                    case eINDEXER_OPERATION_MODE.MQC_MODE: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.MQC; break;
                                    default: lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.NORMAL; break;
                                }
                            }
                            break;
                        }

                    #endregion

                    default:
                        if (line.Data.FABTYPE == eFabType.ARRAY.ToString())
                        {
                            lock (line) line.File.LineOperMode = eMES_LINEOPERMODE.NORMAL;
                        }
                        break;
                }
                ObjectManager.LineManager.EnqueueSave(line.File);
            }
            catch (Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex.ToString());
            }
        }
    }
}