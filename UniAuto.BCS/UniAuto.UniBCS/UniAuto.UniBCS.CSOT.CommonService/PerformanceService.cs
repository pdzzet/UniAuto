using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Core.Generic;
using System.Globalization;
using System.Diagnostics;
using System.Collections;
using UniAuto.UniBCS.EntityManager;
using System.Threading;
using System.Timers;
using System.Configuration;
using System.Reflection;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.MISC;

namespace UniAuto.UniBCS.CSOT.CommonService
{
    

    public class PerformanceService:AbstractService
    {
        private CultureInfo m_ValueFormat;
        private PerformanceCounter m_TotalCpuUsage;
        private ArrayList m_ProcessNameList=new ArrayList();
        private long m_CheckCount;
        private float m_AccumulatedTotalCPUUsage;
        private bool m_BatonAlarm = false;

        private System.Timers.Timer timerClock = new System.Timers.Timer();   

        private string[] Baton_pass_status={"Data linking",
                                           "Data link stop in execution",
                                           "Baton pass being executed",
                                           "Baton pass stop in execution",
                                            "Test being executed",
                                            "Offline"};
        private Dictionary<int,string> Baton_pass_Interruption = new Dictionary<int,string>{
                                                    {0x0,"Normal communication"},
                                                   {0x30,"Cable disconnection or power-on"},
                                                   {0x31,"Cable insertion error"},
                                                   {0x32,"Cable IN-OUT checking"},
                                                   {0x33,"Disconnection or reconnection processing"},
                                                   {0x40,"Offline mode"},
                                                   {0x41,"Hardware test"},
                                                   {0x42,"Self_loopback test"},
                                                   {0x50,"Self-diagnostics in execution"}};

        private Dictionary<int,string> Data_Link_Stop = new Dictionary<int,string>{
                                                  {0x0,"Normal communication"},
                                                  {0x1,"Stop directed"},
                                                  {0x2,"Monitoring timeout"},
                                                  {0x3,"Circuit test being executed"},
                                                  {0x10,"Parameter unreceived"},
                                                  {0x11,"Own station No. that is out of range"},
                                                  {0x12, "Setting where own station is reserved station"},
                                                  {0x13,"Own station No. duplication"},
                                                  {0x14,"Control station duplication"},
                                                  {0x15,"Control station or own station No. duplication"},
                                                  {0x16,"Station No. unset"},
                                                  {0x17,"Network No. illegality"}};

      

        private Dictionary<int, string> Station_Loop_Status = new Dictionary<int, string>{
                                                {0x0,"Normal"},
                                                {0x12,"IN-side loopback (OUT-side cable disconnection)"},
                                                {0x13,"IN-side loopback (OUT-side cable insertion error)"},
                                                {0x14,"IN-side loopback (OUT-side line establishing)"},
                                                {0x21,"OUT-side loopback (IN-side cable disconnection)"},
                                                {0x31,"OUT-side loopback (IN-side cable insertion error)"},
                                                {0x41,"OUT-side loopback (IN-side line establishing)"},
                                                {0x22,"Disconnecting (IN-side or OUT-side cable disconnection)"},
                                                {0x23,"Disconnecting (IN-side cable disconnection, OUT-side cable insertion error)"},
                                                {0x24,"Disconnecting (IN-side cable disconnection, OUT-side line establishing)"},
                                                 {0x32,"Disconnecting (IN-side cable insertion error, OUT-side cable disconnection)"},
                                                 {0x33,"Disconnecting (IN-side or OUT-side cable insertion error)"},
                                                  {0x34,"Disconnecting (IN-side cable insertion error, OUT-side line establishing)"},
                                                   {0x42,"Disconnecting (IN-side line establishing, OUT-side cable disconnection)"},
                                                   {0x43,"Disconnecting (IN-side line establishing, OUT-side cable insertion error)"},
                                                   {0x44,"Disconnecting (IN-side or OUT-side line establishing)"}};

        public override bool Init()
        {
            try
            {
                m_ValueFormat = new CultureInfo("en-US");
                m_TotalCpuUsage = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                m_ValueFormat.NumberFormat.NumberDecimalDigits = 1;

                string m_ProcessorName = ConfigurationManager.AppSettings["MonitorProcessorName"];
                String[] strArray = m_ProcessorName.Split(',');
                m_ProcessNameList = new ArrayList();
                for (int i = 0; i < strArray.Length; i++)
                {
                    m_ProcessNameList.Add(strArray[i]);
                }

                string m_Interval = ConfigurationManager.AppSettings["MonitorInterval"];
                if (Convert.ToInt32(m_Interval) < 600000) m_Interval = "600000";
                if (Convert.ToInt32(m_Interval) > 7200000) m_Interval = "7200000";
                timerClock.Elapsed += new ElapsedEventHandler(WritePerformance);
                timerClock.Interval = Convert.ToInt32(m_Interval);
                timerClock.Enabled = true;

                return true;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        public void WritePerformance(object subject, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                Process[] processes = Process.GetProcesses();

                if (processes != null)
                {
                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",string.Format("============================================================================================================================"));
                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("| {0,20}{1,20}{2,20}{3,20}{4,20}{5,20} |", "Process Name", "ProcessID", "Current Memory", "Peak Memory", "Virtual Memory", "Thread Count"));
                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("----------------------------------------------------------------------------------------------------------------------------"));
                    foreach (string processName in m_ProcessNameList)
                    {
                        foreach (Process process in processes)
                        {
                            try
                            {
                                //列出所有在Server跑的BCS 和OPI的 Performance
                                if (process.ProcessName == processName)
                                {
                                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("| {0,20}{1,20}{2,20}{3,20}{4,20}{5,20} |",
                                                               process.ProcessName,
                                                               process.Id.ToString(),
                                                               (process.WorkingSet64 / 1024).ToString() + "k",
                                                               (process.PeakWorkingSet64 / 1024).ToString() + "k",
                                                               (process.PrivateMemorySize64 / 1024).ToString() + "k",
                                                               process.Threads.Count.ToString()));
                                    PerformanceInfo info = new PerformanceInfo();
                                    info.processName = processName;
                                    info.PrivateMemeorySize = process.PrivateMemorySize64;
                                    info.WorkingSet = process.WorkingSet64;
                                    info.ThreadCount = process.Threads.Count;
                                    //MISC.Repository.Add(processName, info); 
                                    
                                }
                            }
                            catch { }
                        }
                    }

                    m_CheckCount++;

                    float currTotalCpuUsage = m_TotalCpuUsage.NextValue();
                    m_AccumulatedTotalCPUUsage = m_AccumulatedTotalCPUUsage + currTotalCpuUsage;
                    float avgTotalCPUUsage = m_AccumulatedTotalCPUUsage / m_CheckCount;

                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("----------------------------------------------------------------------------------------------------------------------------"));
                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("   Processor CPU Usage         = {0:00.00}%", currTotalCpuUsage));
                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("   Avg Processor CPU Usage     = {0:00.00}%", avgTotalCPUUsage));
                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("   Total Processed Glass Count = {0}", ObjectManager.JobManager.GetJobCount()));
                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("============================================================================================================================"));
                    Logger.LogDebugWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "");
                }
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 记录在线的PLC状态
        /// </summary>
        /// <param name="inputData"></param>
        public void BatonPassStatus(Trx inputData)
        {
            try
            {
                //if (inputData.IsInitTrigger) return;

                //Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                //if (eqp == null) throw new Exception(string.Format("Can't find Equipment No=[{0}) in EquipmentEntity!", inputData.Metadata.NodeNo));

                #region [拆出PLCAgent Data]  Word
                #region [且先Mark不用]
                string status = inputData.EventGroups[0].Events[0].Items["BatonpassStatus"].Value;
                string interruption = inputData.EventGroups[0].Events[0].Items["BatonpassInterruption"].Value;
                string datalinkstop = inputData.EventGroups[0].Events[0].Items["DataLinkStop"].Value;
                string stationloopstatus = inputData.EventGroups[0].Events[0].Items["StationLoopStatus"].Value;
                string eachstation = inputData.EventGroups[0].Events[0].Items["BatonpassStatusEachStation"].Value;
                string cyclictransmissionstatuseachstation = inputData.EventGroups[0].Events[0].Items["CyclicTransmissionstatusEachStation"].Value;
                #endregion
                #endregion

                //string status = inputData.EventGroups[0].Events[0].Items["BatonpassStatus"].Value;

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT=[{0})] [BCS <- EQP][{1}] RECODE A BATONPASSSTATUS =[{2}).", "L1", inputData.TrackKey, status));

                #region [紀錄Log以供查詢]
                Repository.Add(inputData.Name, inputData);
                #endregion
                //只要BatonPassStatus 发送变化后通知UI Service 20150409 Tom
                Invoke(eServiceName.UIService, "EquipmentDataLinkStatusReport",
                    new object[] {inputData.TrackKey, 
                                status,
                                interruption,
                                datalinkstop,
                                stationloopstatus,
                                eachstation,
                                cyclictransmissionstatuseachstation,
                    });
               
                StringBuilder errLog = new StringBuilder();
                #region baton Pass Status
                int batonpassStatus;
                int.TryParse(status, out batonpassStatus);
                if (batonpassStatus != 0)
                {
                    if(batonpassStatus<5)
                        errLog.AppendFormat("batonpassStatus Code [{0}],Message [{1}].", batonpassStatus, Baton_pass_status[batonpassStatus]);
                    else
                        errLog.AppendFormat("batonpassStatus Code [{0}],Message [{1}].", batonpassStatus, "Unknow Message.");
                    //Invoke(eServiceName.EvisorService,"BC_System_Alarm")
                }
                #endregion
              
                #region Baton Pass Interruption
                int batonpassInterruption=Convert.ToInt32(interruption,16);
                if (batonpassInterruption != 0)
                {
                    if(Baton_pass_Interruption.ContainsKey(batonpassInterruption))
                        errLog.AppendFormat("batonpassInterruption Code [{0}],Message [{1}].", batonpassInterruption, Baton_pass_Interruption[batonpassInterruption]);
                    else
                        errLog.AppendFormat("batonpassInterruption Code [{0}],Message [{1}].", batonpassInterruption, "Unknow Message");
                }
                #endregion

                #region data Link Stop
                int dataLinkStop_i = Convert.ToInt32(datalinkstop,16);
               
                if (dataLinkStop_i != 0)
                {
                    if (Data_Link_Stop.ContainsKey(dataLinkStop_i))
                        errLog.AppendFormat("DataLinkStop Code [{0}],Message [{1}].", dataLinkStop_i, Data_Link_Stop[dataLinkStop_i]);
                    else
                        errLog.AppendFormat("DataLinkStop Code [{0}],Message [{1}].", dataLinkStop_i, "Unknow Message");

                }
                #endregion

                #region Station Loop Status
                int stationLoopStatus = Convert.ToInt32(stationloopstatus,16);
                
                if (stationLoopStatus != 0)
                {
                    if (Station_Loop_Status.ContainsKey(stationLoopStatus))
                        errLog.AppendFormat("Station Loop Status Code [{0}],Message [{1}].", stationLoopStatus, Station_Loop_Status[stationLoopStatus]);
                    else
                        errLog.AppendFormat("Station Loop Status Code [{0}],Message [{1}].", stationLoopStatus, "Unknow Message");

                }
                #endregion

               //修改取得Station NO的方式，某些Line 空的Node 20150522 Tom
               // int eqpCount=ObjectManager.EquipmentManager.GetEQPs().Count; 
                List<Equipment> equipments=ObjectManager.EquipmentManager.GetEQPs();
                if(equipments!=null)
                {
                    
                    foreach (Equipment eq in equipments )
                    {
                        eReportMode reportMode;
                        Enum.TryParse<eReportMode>(eq.Data.REPORTMODE, out reportMode);
                        switch (reportMode)
                        {
                            case eReportMode.PLC:
                            case eReportMode.PLC_HSMS:
                            case eReportMode.HSMS_PLC:
                                int stationNo=GetStationNo(eq);
                                if(stationNo>0)
                                {
                                    if (eachstation[stationNo-1] != '0')
                                    {
                                        errLog.AppendFormat("Each Station [L{0}] disconnect.", stationNo);
                                    }

                                    if (cyclictransmissionstatuseachstation[stationNo-1] != '0')
                                    {
                                        errLog.AppendFormat("cyclic transmission status each station [L{0}] disconnect.",stationNo);
                                    }
                                }
                                break;
                            default:
                                break;
                        }

                    }
                }
                string timerID = string.Format("L1_BatonPassStatus");
                if (errLog.Length > 0)
                {
                   
                    if (Timermanager.IsAliveTimer(timerID))
                    {
                        return;
                    }
                    else
                    {
                        Timermanager.CreateTimer(timerID, false, ParameterManager["PLCBOARDSTATUSCHECK"].GetInteger(), new ElapsedEventHandler(BatonPassStatusTimeout));
                    }
                }
                else
                {
                    if (Timermanager.IsAliveTimer(timerID))
                    {
                        Timermanager.TerminateTimer(timerID);
                    }

                    if (m_BatonAlarm) //add 2016/08/04 cc.kuang
                    {
                        Invoke(eServiceName.EvisorService, "AppAlarmReport", new object[] { ServerName, "NORMAL", "cyclic transmission is change to Normal" });

                        m_BatonAlarm = false;
                    }
                }

                    //Invoke(eServiceName.EvisorService, "BC_System_Alarm", new object[] { ServerName, errLog.ToString() });
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void BatonPassStatusTimeout(object sender, ElapsedEventArgs e) {
            try
            {
                UserTimer timer = sender as UserTimer;
                string tmp = timer.TimerId;
                if(Timermanager.IsAliveTimer(tmp))
                    Timermanager.TerminateTimer(tmp);

                Trx trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { "L1_batonpassStatus",true }) as Trx;

                if (trx != null)
                {
                    string status = trx.EventGroups[0].Events[0].Items["BatonpassStatus"].Value;
                    string interruption = trx.EventGroups[0].Events[0].Items["BatonpassInterruption"].Value;
                    string datalinkstop = trx.EventGroups[0].Events[0].Items["DataLinkStop"].Value;
                    string stationloopstatus = trx.EventGroups[0].Events[0].Items["StationLoopStatus"].Value;
                    string eachstation = trx.EventGroups[0].Events[0].Items["BatonpassStatusEachStation"].Value;
                    string cyclictransmissionstatuseachstation = trx.EventGroups[0].Events[0].Items["CyclicTransmissionstatusEachStation"].Value;

                    StringBuilder errLog = new StringBuilder();
                    #region baton Pass Status
                    int batonpassStatus;
                    int.TryParse(status, out batonpassStatus);
                    if (batonpassStatus != 0)
                    {
                        if (batonpassStatus < 5)
                            errLog.AppendFormat("batonpassStatus Code [{0}],Message [{1}].", batonpassStatus, Baton_pass_status[batonpassStatus]);
                        else
                            errLog.AppendFormat("batonpassStatus Code [{0}],Message [{1}].", batonpassStatus, "Unknow Message.");
                        //Invoke(eServiceName.EvisorService,"BC_System_Alarm")
                    }
                    #endregion

                    #region Baton Pass Interruption
                    int batonpassInterruption = Convert.ToInt32(interruption, 16);
                    if (batonpassInterruption != 0)
                    {
                        if (Baton_pass_Interruption.ContainsKey(batonpassInterruption))
                            errLog.AppendFormat("batonpassInterruption Code [{0}],Message [{1}].", batonpassInterruption, Baton_pass_Interruption[batonpassInterruption]);
                        else
                            errLog.AppendFormat("batonpassInterruption Code [{0}],Message [{1}].", batonpassInterruption, "Unknow Message");
                    }
                    #endregion

                    #region data Link Stop
                    int dataLinkStop_i = Convert.ToInt32(datalinkstop, 16);

                    if (dataLinkStop_i != 0)
                    {
                        if (Data_Link_Stop.ContainsKey(dataLinkStop_i))
                            errLog.AppendFormat("DataLinkStop Code [{0}],Message [{1}].", dataLinkStop_i, Data_Link_Stop[dataLinkStop_i]);
                        else
                            errLog.AppendFormat("DataLinkStop Code [{0}],Message [{1}].", dataLinkStop_i, "Unknow Message");

                    }
                    #endregion

                    #region Station Loop Status
                    int stationLoopStatus = Convert.ToInt32(stationloopstatus, 16);

                    if (stationLoopStatus != 0)
                    {
                        if (Station_Loop_Status.ContainsKey(stationLoopStatus))
                            errLog.AppendFormat("Station Loop Status Code [{0}],Message [{1}].", stationLoopStatus, Station_Loop_Status[stationLoopStatus]);
                        else
                            errLog.AppendFormat("Station Loop Status Code [{0}],Message [{1}].", stationLoopStatus, "Unknow Message");

                    }
                    #endregion

                    #region eachstation & cyclictransmissionstatuseachstation
                    List<Equipment> equipments = ObjectManager.EquipmentManager.GetEQPs();
                    if (equipments != null)
                    {

                        foreach (Equipment eq in equipments)
                        {
                            eReportMode reportMode;
                            Enum.TryParse<eReportMode>(eq.Data.REPORTMODE, out reportMode);
                            switch (reportMode)
                            {
                                case eReportMode.PLC:
                                case eReportMode.PLC_HSMS:
                                case eReportMode.HSMS_PLC:
                                    int stationNo = GetStationNo(eq);
                                    if (stationNo > 0)
                                    {
                                        if (eachstation[stationNo - 1] != '0')
                                        {
                                            errLog.AppendFormat("Each Station [L{0}] disconnect.", stationNo);
                                        }

                                        if (cyclictransmissionstatuseachstation[stationNo - 1] != '0')
                                        {
                                            errLog.AppendFormat("cyclic transmission status each station [L{0}] disconnect.", stationNo);
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }

                        }
                    }
                    #endregion

                    if (errLog.Length > 0)
                    {
                        //Invoke(eServiceName.EvisorService, "BC_System_Alarm", new object[] { ServerName, errLog.ToString() });
                        if (!m_BatonAlarm)
                            Invoke(eServiceName.EvisorService, "AppAlarmReport", new object[] { ServerName, "ALARM", errLog.ToString() });

                        m_BatonAlarm = true;
                    }
                    else //add for BMS alarm reset cyclic transmission Alarm 2016/06/30 cc.kuang
                    {
                        if (m_BatonAlarm)
                            Invoke(eServiceName.EvisorService, "AppAlarmReport", new object[] { ServerName, "NORMAL", "cyclic transmission is change to Normal" });

                        m_BatonAlarm = false;
                    }
                }
            }
            catch (System.Exception ex)
            {
            	
            }
        }

        private int GetStationNo(Equipment eqp)
        {
            string stationNo = eqp.Data.NODENO.Split(new char[] { 'L' })[1];

            int result=0;
            bool done=int.TryParse(stationNo, out result);
            if (done)
                return result;
            else
                return -1;

        }


    }
}
