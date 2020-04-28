using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Core.Generic;
using System.Timers;
using System.IO;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.Entity;
using System.Xml;
using System.Diagnostics;
using UniAuto.UniBCS.Core;
using System.Reflection;
using System.Threading;


namespace UniAuto.UniBCS.CSOT.MESMessageService
{
    public class EvisorService:AbstractService
    {
        #region BC System Info
        private const string BCSYSTEMINFO = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                                               <MESSAGE>
                                                    <MESSAGE_NAME>t2_BC_SPEC_VERSION</MESSAGE_NAME>
                                                    <TIMESTAP></TIMESTAP>
                                                    <LINE_ID></LINE_ID>
                                                    <PHYSICAL_MEMORY/>
                                                    <VIRTUAL_MEMORY/>
                                                    <THREAD_COUNT/>
                                                    <PROGRAM_VERION_LIST>
                                                        <VERSION>
                                                            <PROGRAM_NAME/>
                                                            <PROGRAM_VERION/>
                                                         </VERSION>
                                                    </PROGRAM_VERION_LIST>
                                                    <CONFIG_VERION_LIST>
                                                        <CONFIG>
                                                            <CONFIG_NAME/>
                                                            <CONFIG_VERION/>
                                                        </CONFIG>
                                                    </CONFIG_VERION_LIST>
                                                </MESSAGE>";
        #endregion

        #region BC Component Status
        private const string BC_COMPONENT_STATUS = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                                                  <MESSAGE>
                                                        <MESSAGE_NAME>t2_BC_COMPONENT_STATUS</MESSAGE_NAME>
                                                        <TIMESTAP></TIMESTAP>
                                                        <LINE_ID></LINE_ID>
                                                        <COMPONENT_LIST>
	                                                        <COMPONENT>
                                                                <COMPONENT_NAME/>     
                                                                <COMPONENT_STATUS/>
                                                            </COMPONENT>
                                                        </COMPONENT_LIST>
                                                    </MESSAGE>";

        #endregion

        #region BC Queue Alarm
        public const string BC_QUEUE_ALARM = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                                                <MESSAGE>
                                                <MESSAGE_NAME>t2_BC_QUEUE_ALARM</MESSAGE_NAME>
                                                <TIMESTAP></TIMESTAP>
                                                <LINE_ID></LINE_ID>
                                                <QUEUE_LIST>
                                                        <QUEUE>
                                                               <QUEUE_NAME/>
                                                               <CURRENT_COUNT/>
                                                               <WARNING_COUNT/>
                                                               <ALARM_COUNT/>
                                                         </QUEUE>
                                                </QUEUE_LIST>
                                                </MESSAGE>";

        #endregion

        #region OPI Connect Count
        public const string OPI_CONNECT_COUNT=@"<?xml version=""1.0"" encoding=""UTF-8""?>
                                                <MESSAGE>
                                                <MESSAGE_NAME>t3_BC_OPI_CONNECT_COUNT</MESSAGE_NAME>
                                                <TIMESTAP>2013-09-21 08:36:23</TIMESTAP>
                                                <LINE_ID>CACUT700</LINE_ID>
                                                <CURRENT_CONNECT_COUNT/>
                                                <MAX_CONNECT_COUNT/>
                                                </MESSAGE>";
        #endregion

        #region Line Info Status
        public const string LINE_INFO_STATUS = @"<?xml version=""1.0"" encoding=""UTF-8""?>
                                                <MESSAGE>
                                                <MESSAGE_NAME>t2_BC_LINE_INFO</MESSAGE_NAME>
                                                <TIMESTAP>2013-09-21 08:36:23</TIMESTAP>
                                                <LINE_ID>CACUT700</LINE_ID>
                                                <SECS1_STATE/>
                                                <SECS2_STATE/>
                                                <CONTROL_STATE/>
                                                <MPLC_STATE/>
                                                <STARTTIME/>
                                                </MESSAGE>";
        #endregion

        #region System_Alarm
        public const string SYSTEM_ALARM =@"<?xml version=""1.0"" encoding=""UTF-8""?>
                                            <MESSAGE>
                                            <MESSAGE_NAME>t2_BC_SYSTEM_ALARM</MESSAGE_NAME>
                                            <TIMESTAP>2013-09-21 08:36:23</TIMESTAP>
                                            <LINE_ID>CACUT700</LINE_ID>
                                            <TEXT/>
                                            </MESSAGE>";
        #endregion

        private const string MESSAGE=@"<MESSAGE>
                                          <HEADER>
                                            <MESSAGENAME></MESSAGENAME>
                                            <TRANSACTIONID></TRANSACTIONID>
                                            <REPLYSUBJECTNAME></REPLYSUBJECTNAME>
                                            <INBOXNAME></INBOXNAME>
                                            <LISTENER></LISTENER>
                                          </HEADER>
                                          <BODY>
                                          </BODY>
                                          <RETURN>
                                            <RETURNCODE>0000000</RETURNCODE>
                                            <RETURNMESSAGE></RETURNMESSAGE>
                                          </RETURN>
                                        </MESSAGE>";

        private const string APPINFOREPORT = @"<LINEID></LINEID>
                                                <APPNAME></APPNAME>
                                                <APPVERSION></APPVERSION>
                                                <STARTTIME></STARTTIME>
                                                <MEMORY></MEMORY>
                                                <THREADCOUNT></THREADCOUNT>
                                                <CPU></CPU>
                                                <CONFIGLIST>
                                                   <CONFIG>
                                                          <CONFIGNAME></CONFIGNAME>  
                                                          <VERSION></VERSION>
                                                   </CONFIG>
                                                </CONFIGLIST>
                                                <AGENTLIST>
                                                    <AGENT>
                                                        <AGENTNAME></AGENTNAME>
                                                       <AGENTSTATUS></AGENTSTATUS>
                                                    </AGENT>
                                                </AGENTLIST>";

        //20170208 Menghui Add ERRORCODE上报
        private const string APPERRORREPORT = @"<LINEID></LINEID>
                                                <ALARMLEVEL></ALARMLEVEL>
                                                <ERRORLEVEL></ERRORLEVEL>
                                                <ERRORCODE></ERRORCODE>
                                                <ERRORTEXT></ERRORTEXT>
                                                <ERRORSTAT></ERRORSTAT>
                                                <KEYID></KEYID>
                                                <SOURCEFROM></SOURCEFROM>
                                                <TIMESTAMP></TIMESTAMP>";

        private const string APPALARMREPORT = @"<LINEID></LINEID>
                                                <ALARMLEVEL></ALARMLEVEL>
                                                <ALARMTEXT></ALARMTEXT>
                                                <TIMESTAMP></TIMESTAMP>";

        private const string OPICONNECTCOUNTREPORT = @"<LINEID></LINEID>
                                                    <CURRENTCOUNT></CURRENTCOUNT>
                                                    <MAXCOUNT></MAXCOUNT>
                                                    <TIMESTAMP></TIMESTAMP>";

        private const string AGENTRUNTIMEREPLY = @"<LINEID></LINEID>
                                                    <AGENTNAME></AGENTNAME>
                                                    <ITEMLIST>
                                                       <ITEM>
                                                        <ITEMNAME></ITEMNAME>
                                                        <ITEMVALUE></ITEMVALUE>
                                                       </ITEM>
                                                    </ITEMLIST>";

        private System.Timers.Timer _evisorTimer;
        private FileSystemWatcher _watcher = new FileSystemWatcher();
        private FileSystemWatcher _watcherBCS = new FileSystemWatcher();//20161218
        private string UpgradeFileName = "";
        private string monitornewpackage = string.Empty; //add by yang 2017/4/11,给后面做判断
        bool initflag = true;//add by yang for filter init appinfo report  2017/5/5

        public double Interval { get; set; }
        public string EvisorFolder { get; set; }
        private int _seq=1;

        public string GetSeqNo()
        {
            if (_seq >= 63335)
            {
                _seq = 1;
            }
            string retValue = _seq.ToString("00000");
            _seq++;
            return retValue;
        }

        private Dictionary<string, bool> _evisorServerFlag=new Dictionary<string, bool>();
        DateTime _startTime;
        public EvisorService()
        {
            _startTime = DateTime.Now;
        }

        public override bool Init()
        {
            try
            {              
                StartTimer();
                StartFiletWatcher();
                AfterInit();//20161208
                return true;
            }
            catch (Exception ex)
            {
                LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        private void StartTimer()
        {
            try
            {
                if (Interval == 0)
                {
                    Interval = 60;    //default 5 Minute
                }
             
                _evisorTimer = new System.Timers.Timer();
                _evisorTimer.Interval = Interval * 1000;
                _evisorTimer.Elapsed += new ElapsedEventHandler(EvisorTimer_Elapsed);
                _evisorTimer.Start();
            }
            catch (Exception ex)
            {
                LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void StartFiletWatcher()
        {
            try
            {
                if (EvisorFolder.Substring(EvisorFolder.Length - 1) != "\\")
                {
                    EvisorFolder += "\\";
                }
                if (!Directory.Exists(this.EvisorFolder))
                {
                    Directory.CreateDirectory(this.EvisorFolder);
                }

                _watcher.Path = EvisorFolder;
                _watcher.EnableRaisingEvents = true;
                _watcher.Filter = "*.bmc"; ;
                _watcher.IncludeSubdirectories = false;
                _watcher.Created += new FileSystemEventHandler(OnCreated);

                _watcherBCS.Path = EvisorFolder;
                _watcherBCS.EnableRaisingEvents = true;
                _watcherBCS.Filter = "*.xml"; ;
                _watcherBCS.IncludeSubdirectories = false;
                _watcherBCS.Created += new FileSystemEventHandler(OnCreatedNewPackage);
            }
            catch (Exception ex)
            {
                LogInfo(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex.Message);
            }
        }

        private void OnCreated(object sender, System.IO.FileSystemEventArgs e)
        {
            try
            {
                string fileName = e.Name;//此处只会有文件名

                if (fileName.Contains("AgentRuntimeRequest")) {
                    Thread.Sleep(300);
                    try
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.Load(e.FullPath);
                        AgentRuntimeReply(doc);
                    }
                    catch (System.Exception ex)
                    {
                        LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
                    }
                    finally
                    {
                        File.Delete(e.FullPath);
                    }
                } else {

                    if(initflag)  //add by yang 2017/5/5
                    {
                        initflag = false;
                        return;
                    }

                    List<Line> _lines = ObjectManager.LineManager.GetLines();
                    foreach (Line line in _lines) {
                        //收到OnCreate时间后就产生
                        if (fileName.Contains(line.Data.LINEID)) {
                            if (ParameterManager["EVISORREPORT"].GetBoolean()) {                               
                                AppInformationReport(line.Data.LINEID);
                                //BC_System_Info_Report(line.Data.LINEID);
                                //BC_Component_Status(line.Data.LINEID);
                                //BC_Line_Status(line.Data.LINEID);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void AgentRuntimeReply(XmlDocument request) {
            try
            {
                XmlDocument response = new XmlDocument();
                response.LoadXml(MESSAGE);
                string agentName=request["MESSAGE"]["BODY"]["AGENTNAME"].InnerText;
                string lineid=request["MESSAGE"]["BODY"]["LINEID"].InnerText;
                Line line = ObjectManager.LineManager.GetLine(lineid);
                response["MESSAGE"]["HEADER"]["MESSAGENAME"].InnerText = "AgentRuntimeReply";
                response["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText = request["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText;
                response["MESSAGE"]["BODY"].InnerXml = AGENTRUNTIMEREPLY;
                response["MESSAGE"]["BODY"]["AGENTNAME"].InnerText = agentName;
                response["MESSAGE"]["BODY"]["LINEID"].InnerText = lineid;
                if (line == null) {
                    response["MESSAGE"]["RETURN"]["RETURNCODE"].InnerText = "1";
                    response["MESSAGE"]["RETURN"]["RETURNMESSAGE"].InnerText = "not found Line Object!";
                } else {//Line Object Find
                    if (Workbench.Instance.AgentList.ContainsKey(agentName)) { //  Find Agent object
                        XmlNode itemList = response["MESSAGE"]["BODY"]["ITEMLIST"];
                        XmlNode itemNode = itemList["ITEM"];
                        itemList.RemoveAll();
                        IServerAgent agent = Workbench.Instance.AgentList[agentName];
                        foreach (string key in agent.RuntimeInfo.Keys) {
                            XmlNode item = itemNode.Clone();
                            item["ITEMNAME"].InnerText = key;
                            item["ITEMVALUE"].InnerText = agent.RuntimeInfo[key].ToString();
                            itemList.AppendChild(item);
                        }

                    } else {
                        response["MESSAGE"]["RETURN"]["RETURNCODE"].InnerText = "2";
                        response["MESSAGE"]["RETURN"]["RETURNMESSAGE"].InnerText = "not found Agetn Object!";
                    }
                }
                string fileName = string.Format("{0}AgentRuntimeReport{2}_{1}.xml", EvisorFolder, DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.fff"), GetSeqNo());
                response.Save(fileName);
                
            }
            catch (System.Exception ex)
            {
                LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void EvisorTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (Workbench.State != eWorkbenchState.RUN)
                    return;

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

                #region 同步 T2 所有機台出現Eq Alive Timeout時發Evisor通知
                // add by bruce 20160411
                if (EquipmentAliveCheck() == true)
                {
                    BC_System_Alarm(ServerName, string.Format("Alive of all equpment is time out."));
                }
                #endregion

                if (line != null)
                {
                    if (line.File.OnBoradCardAlive == false) //卡已经不稳定了
                    {
                        if (EquipmentAliveCheck() == true)
                        {
                           
                            BC_System_Alarm(ServerName, string.Format("PLC board Card is error. BCS will be restart this SERVER. "));

                            Thread.Sleep(10000); //10秒后启动程序
                            if (ParameterManager["RESTARTSERVER"].GetBoolean())
                            {
                                LogError(MethodBase.GetCurrentMethod().Name + "()", "PC Board Card is error ,Server will be Restart.");
                                //TODO: Restart Server;
                                System.Diagnostics.Process.Start("shutdown.exe", "-r -f -t 15 -c \"UniBCS shutdown\"");
                            }
                        }
                    }
                }

                IDictionary<string, IServerAgent> agents = Workbench.Instance.AgentList;
                foreach (string key in agents.Keys)
                {
                    string currentCount = agents[key].RuntimeInfo["QueueCount"].ToString();

                    if (string.IsNullOrEmpty(currentCount))
                        continue;
                    if (Convert.ToInt32(currentCount) > ParameterManager["MONITORQUEUECOUNT"].GetInteger())
                    {
                        
                        BC_Queue_Alarm(ServerName, key, currentCount, ParameterManager["MONITORQUEUECOUNT"].GetInteger().ToString());
                        
                        BC_System_Alarm(ServerName, string.Format("Agent {0} message queue count is {1} overload Max count {2}.", key, currentCount, ParameterManager["MONITORQUEUECOUNT"].GetInteger()));
                        Thread.Sleep(1000);
                    }
                }
                 
            }
            catch (Exception ex)
            {
                LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex); 
            }
        }

        /// <summary>
        /// Check All Equipment Alive is Time out 
        /// if All Equipment Timeout Return true;
        /// else return false;
        /// </summary>
        /// <returns></returns>
        public bool EquipmentAliveCheck()
        {
            try
            {
                // modify by bruce 20160411 同步T2 取得所有機台 Alive 是否都出現Itemout
                return ObjectManager.EquipmentManager.GetEQPs().Where(e => e.Data.REPORTMODE.IndexOf("PLC") >= 0).All(e => e.File.AliveTimeout);

                //IList<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                //foreach (Equipment eq in eqps)
                //{
                //    if (eq.File.AliveTimeout == false) //有一个机台没有Alive TIME out就Return TRUE
                //        return false;
                //}
                //return true;
            }
            catch (System.Exception ex)
            {
                LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        /// <summary>
        /// System Info Report
        /// </summary>
        /// <param name="lineID"></param>
        public void BC_System_Info_Report(string lineID)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(BCSYSTEMINFO);
                doc["MESSAGE"]["TIMESTAP"].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                doc["MESSAGE"]["LINE_ID"].InnerText = lineID;

                PerformanceInfo info = GetPerformanceInfo(Process.GetCurrentProcess().ProcessName);
                doc["MESSAGE"]["PHYSICAL_MEMORY"].InnerText = (info.WorkingSet / 1024).ToString();
                doc["MESSAGE"]["VIRTUAL_MEMORY"].InnerText = (info.PrivateMemeorySize / 1024).ToString();
                doc["MESSAGE"]["THREAD_COUNT"].InnerText = (info.ThreadCount).ToString();
                
                XmlNode verList =doc["MESSAGE"]["PROGRAM_VERION_LIST"];
                XmlNode ver=verList["VERSION"];
                verList.RemoveAll();
                XmlNode bcVer=ver.Clone();
                bcVer["PROGRAM_NAME"].InnerText="BC";
                bcVer["PROGRAM_VERION"].InnerText=Workbench.Version;
                verList.AppendChild(bcVer);
                XmlNode opiVer=ver.Clone();
                opiVer["PROGRAM_NAME"].InnerText="OPI";
                opiVer["PROGRAM_VERION"].InnerText=Workbench.Version;
                verList.AppendChild(opiVer);
                XmlNode cfgList = doc["MESSAGE"]["CONFIG_VERION_LIST"];
                XmlNode cfg = cfgList["CONFIG"];
                cfgList.RemoveAll();
                List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                IServerAgent agent = GetObject("PLCAgent") as IServerAgent;
                string plcMapVer=string.Empty;
                if (agent != null)
                {
                    plcMapVer = agent.RuntimeInfo["Version"].ToString();
                    XmlNode node = cfg.Clone();
                    node["CONFIG_NAME"].InnerText = "PLC_MAP";
                    node["CONFIG_VERION"].InnerText = plcMapVer;
                    cfgList.AppendChild(node);
                }
                if (eqps != null)
                {
                    foreach (Equipment eq in eqps)
                    {
                        XmlNode edcVer = cfg.Clone();
                        edcVer["CONFIG_NAME"].InnerText = string.Format("{0}_PROCESS_DATA", eq.Data.NODENO);
                        edcVer["CONFIG_VERION"].InnerText = eq.Data.EQPPROFILE;
                        cfgList.AppendChild(edcVer);
                        XmlNode dailyVer = cfg.Clone();
                        dailyVer["CONFIG_NAME"].InnerText = string.Format("{0}_DAILY_CHECK", eq.Data.NODENO);
                        dailyVer["CONFIG_VERION"].InnerText = eq.Data.EQPPROFILE;
                        cfgList.AppendChild(dailyVer);
                        XmlNode recipeVer = cfg.Clone();
                        recipeVer["CONFIG_NAME"].InnerText = string.Format("{0}_RECIPE_PARAMETER", eq.Data.NODENO);
                        recipeVer["CONFIG_VERION"].InnerText = eq.Data.EQPPROFILE;
                        cfgList.AppendChild(recipeVer);
                        XmlNode apcVer = cfg.Clone();
                        apcVer["CONFIG_NAME"].InnerText = string.Format("{0}_APC_DATA", eq.Data.NODENO);
                        apcVer["CONFIG_VERION"].InnerText = eq.Data.EQPPROFILE;
                        cfgList.AppendChild(apcVer);
                        XmlNode apcDVer = cfg.Clone();
                        apcDVer["CONFIG_NAME"].InnerText = string.Format("{0}_APC_DATA_DOWNLOAD", eq.Data.NODENO);
                        apcDVer["CONFIG_VERION"].InnerText = eq.Data.EQPPROFILE;
                        cfgList.AppendChild(apcDVer);
                        XmlNode engVer = cfg.Clone();
                        engVer["CONFIG_NAME"].InnerText = string.Format("{0}_ENGRGY", eq.Data.NODENO);
                        engVer["CONFIG_VERION"].InnerText = eq.Data.EQPPROFILE;
                        cfgList.AppendChild(engVer);
                    }
                }
                string fileName=string.Format("{0}t2_BC_System_Information_{2}_{1}.xml",EvisorFolder,DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.fff"),GetSeqNo());
                doc.Save(fileName);

            }
            catch (System.Exception ex)
            {
                LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex); 
            }
        }

        public void AppInformationReport(string lineId) {
            try
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(MESSAGE);
                document["MESSAGE"]["HEADER"]["MESSAGENAME"].InnerText = "AppInfoReport";
                document["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                document["MESSAGE"]["BODY"].InnerXml = APPINFOREPORT;
                document["MESSAGE"]["BODY"]["LINEID"].InnerText = lineId;
                document["MESSAGE"]["BODY"]["APPNAME"].InnerText = "UniBCS";
                document["MESSAGE"]["BODY"]["APPVERSION"].InnerText = Workbench.Version + updateStatus;
                document["MESSAGE"]["BODY"]["STARTTIME"].InnerText = Workbench.Instance.MainForm.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
                PerformanceInfo info = GetPerformanceInfo(Process.GetCurrentProcess().ProcessName);
                document["MESSAGE"]["BODY"]["MEMORY"].InnerText = (info.WorkingSet / 1024).ToString();
                document["MESSAGE"]["BODY"]["THREADCOUNT"].InnerText = info.ThreadCount.ToString();
                PerformanceCounter cpuUsage = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName);
            
                document["MESSAGE"]["BODY"]["CPU"].InnerText=cpuUsage.NextValue().ToString();
                #region ConfigList
                XmlNode configNodeList = document["MESSAGE"]["BODY"]["CONFIGLIST"];
                XmlNode cfgNode = configNodeList["CONFIG"];
                configNodeList.RemoveAll();
                
                IServerAgent agent = GetObject("PLCAgent") as IServerAgent;
                string plcMapVer = string.Empty;
                if (agent != null) {
                    plcMapVer = agent.RuntimeInfo["Version"].ToString();
                    XmlNode node = cfgNode.Clone();
                    node["CONFIGNAME"].InnerText = "PLCMAP";
                    node["VERSION"].InnerText = plcMapVer;
                    configNodeList.AppendChild(node);
                }
                List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPs();
                if (eqps != null) {
                    XmlNode n = cfgNode.Clone();
                    n["CONFIGNAME"].InnerText = "PROFILE";
                    StringBuilder sbVersion = new StringBuilder();
                    foreach (Equipment eq in eqps) {
                        sbVersion.AppendFormat("{0}={1},", eq.Data.NODENO, eq.Data.EQPPROFILE);
                    }
                    n["VERSION"].InnerText = sbVersion.ToString(0, sbVersion.Length - 1);
                    configNodeList.AppendChild(n);
                }
                #region JobCount add Config list
                //add by qiumin 20180926 
                XmlNode jobcount = cfgNode.Clone();
                jobcount["CONFIGNAME"].InnerText="JOBCOUNT";
                jobcount["VERSION"].InnerText = ObjectManager.JobManager.GetJobCount().ToString() ;
                configNodeList.AppendChild(jobcount);
                #endregion

                #endregion
                #region AgentList
                XmlNode agentNodeList = document["MESSAGE"]["BODY"]["AGENTLIST"];
                XmlNode agentNode = agentNodeList["AGENT"];
                agentNodeList.RemoveAll();
                foreach (string agentName in Workbench.Instance.AgentList.Keys) {
                    XmlNode node = agentNode.Clone();
                    node["AGENTNAME"].InnerText = agentName;
                    if (Workbench.Instance.AgentList[agentName].ConnectedState == eAGENT_STATE.CONNECTED && Workbench.Instance.AgentList[agentName].AgentStatus == eAgentStatus.RUN) {
                        node["AGENTSTATUS"].InnerText = "ALIVE";
                    } else {
                        node["AGENTSTATUS"].InnerText = "DOWN";
                    }
                    agentNodeList.AppendChild(node);
                }
                #endregion

                string fileName = string.Format("{0}AppInfoReport_{2}_{1}.xml", EvisorFolder, DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.fff"), GetSeqNo());
                document.Save(fileName);
            }
            catch (System.Exception ex)
            {
                LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex); 
            }
        }

        /// <summary>
        /// 获取Performance Info
        /// </summary>
        /// <param name="processName"></param>
        /// <returns></returns>
        private PerformanceInfo GetPerformanceInfo(string processName)
        {
            PerformanceInfo info = MISC.Repository.Get(processName) as PerformanceInfo;
            if(info==null)
            {
                info = new PerformanceInfo();
                Process currentProcess = Process.GetCurrentProcess();
                info.PrivateMemeorySize = currentProcess.PrivateMemorySize64;
                info.WorkingSet = currentProcess.WorkingSet64;
                info.ThreadCount = currentProcess.Threads.Count;
            }

            return info;
        }

        /// <summary>
        /// BC Component Status
        /// </summary>
        /// <param name="lineID"></param>
        public void BC_Component_Status(string lineID)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(BC_COMPONENT_STATUS);
                doc["MESSAGE"]["TIMESTAP"].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                doc["MESSAGE"]["LINE_ID"].InnerText = lineID;
                XmlNode componentList = doc["MESSAGE"]["COMPONENT_LIST"];
                XmlNode component = componentList["COMPONENT"];
                componentList.RemoveAll();
                foreach(string agentName in Workbench.Instance.AgentList.Keys)
                {
                     XmlNode node = component.Clone();
                     node["COMPONENT_NAME"].InnerText = agentName;
                    if (Workbench.Instance.AgentList[agentName].ConnectedState == eAGENT_STATE.CONNECTED && Workbench.Instance.AgentList[agentName].AgentStatus==eAgentStatus.RUN)
                    {
                        node["COMPONENT_STATUS"].InnerText = "ALIVE";
                    }
                    else
                    {
                        node["COMPONENT_STATUS"].InnerText = "DOWN";
                    }
                    componentList.AppendChild(node);
                }
                string fileName = string.Format("{0}t2_BC_COMPONENT_{2}_{1}.xml", EvisorFolder, DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.fff"), GetSeqNo());
                doc.Save(fileName);

            }
            catch (System.Exception ex)
            {
                LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex); 
            }
        }

        /// <summary>
        /// BC  Queue Alarm
        /// </summary>
        /// <param name="lineId"></param>
        public void BC_Queue_Alarm(string lineId,string queuName,string currentCount,string maxCount)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(BC_QUEUE_ALARM);
                doc["MESSAGE"]["TIMESTAP"].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                doc["MESSAGE"]["LINE_ID"].InnerText = lineId;
                XmlNode queueNodes = doc["MESSAGE"]["QUEUE_LIST"];
                XmlNode queueClone = queueNodes["QUEUE"];
                queueNodes.RemoveAll();

                XmlNode queue = queueClone.Clone();
                queue["QUEUE_NAME"].InnerText = queuName;
                queue["CURRENT_COUNT"].InnerText = currentCount;
                queue["WARNING_COUNT"].InnerText = maxCount;
                queueNodes.AppendChild(queue);
                
                string fileName = string.Format("{0}t2_BC_QUEUE_Alarm_{2}_{1}.xml", EvisorFolder, DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.fff"), GetSeqNo());
                doc.Save(fileName);

            }
            catch (System.Exception ex)
            {
                LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex); 
            }
        }

        /// <summary>
        /// BC Line Status
        /// </summary>
        /// <param name="lineId"></param>
        public void BC_Line_Status(string lineId)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(LINE_INFO_STATUS);
                doc["MESSAGE"]["TIMESTAP"].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                doc["MESSAGE"]["LINE_ID"].InnerText = lineId;
                IDictionary<string,IServerAgent> agents = Workbench.Instance.AgentList;
                string SECS1_STATE = string.Empty;
                string SECS2_STATE = string.Empty;

                foreach (string key in agents.Keys)
                {
                    if(key!=eAgentName.MESAgent && 
                       key!=eAgentName.OEEAgent &&
                       key!=eAgentName.OPIAgent &&
                       key!=eAgentName.PLCAgent && 
                       key!=eAgentName.SerialPortAgent &&
                       key!=eAgentName.PassiveSocketAgent &&
                       key!=eAgentName.ActiveSocketAgent &&
                       key!=eAgentName.APCAgent &&
                       key!=eAgentName.EDAAgent )
                    {
                        if (string.IsNullOrEmpty(SECS1_STATE))
                        {
                            SECS1_STATE = agents[key].ConnectedState;
                        }
                        else if(string.IsNullOrEmpty(SECS2_STATE))
                        {
                            SECS2_STATE = agents[key].ConnectedState;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                doc["MESSAGE"]["SECS1_STATE"].InnerText = SECS1_STATE;
                doc["MESSAGE"]["SECS2_STATE"].InnerText = SECS2_STATE;
                doc["MESSAGE"]["CONTROL_STATE"].InnerText = ObjectManager.LineManager.GetLine(lineId).File.HostMode.ToString();
                //doc["MESSAGE"]["STARTTIME"].InnerText = Workbench.Instance.MainForm.StartTime.ToString("yyyy-MM-dd HH:mm:ss");
                doc["MESSAGE"]["STARTTIME"].InnerText = _startTime.ToString("yyyy-MM-dd HH:mm:ss");
                string fileName = string.Format("{0}t2_BC_LINE_INFO_{2}_{1}.xml", EvisorFolder, DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.fff"), GetSeqNo());
                doc.Save(fileName);
            }
            catch (System.Exception ex)
            {
                LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex); 
            }
        }

        /// <summary>
        /// System Alarm
        /// </summary>
        /// <param name="lineId"></param>
        /// <param name="alarmText"></param>
        public void BC_System_Alarm(string lineId, string alarmText)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(SYSTEM_ALARM);
                doc["MESSAGE"]["TIMESTAP"].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                doc["MESSAGE"]["LINE_ID"].InnerText = lineId;
                doc["MESSAGE"]["TEXT"].InnerText = alarmText;
                string fileName = string.Format("{0}t2_BC_System_Alarm_{2}_{1}.xml", EvisorFolder, DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.fff"), GetSeqNo());
                doc.Save(fileName);
            }
            catch (System.Exception ex)
            {
                LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="lineid"></param>
        /// <param name="alarmLevel"> ALAEM ，Warning</param>
        /// <param name="alarmText"></param>
        public void AppAlarmReport(string lineid, string alarmLevel,string alarmText) {
            try {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(MESSAGE);
                doc["MESSAGE"]["BODY"].InnerXml = APPALARMREPORT;
                doc["MESSAGE"]["HEADER"]["MESSAGENAME"].InnerText = "AppAlarmReport";
                doc["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText = DateTime.Now.ToString("yyyyMMddHHmmssfff");

                doc["MESSAGE"]["BODY"]["TIMESTAMP"].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                doc["MESSAGE"]["BODY"]["LINEID"].InnerText = lineid;
                doc["MESSAGE"]["BODY"]["ALARMTEXT"].InnerText = alarmText;
                if (alarmLevel == "NORMAL") //add 2016/06/30 cc.kuang
                    doc["MESSAGE"]["BODY"]["ALARMLEVEL"].InnerText = "NORMAL";
                else
                    doc["MESSAGE"]["BODY"]["ALARMLEVEL"].InnerText = "ALARM";
                string fileName = string.Format("{0}AppAlarmReport_{2}_{1}.xml", EvisorFolder, DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.fff"), GetSeqNo());
                doc.Save(fileName);
            } catch (System.Exception ex) {
                LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
       
 
        /// <summary> 
        /// Error Code 写入Monitor  20170208 Menghui Add 
        /// </summary>
        /// <param name="lineid"></param>
        /// <param name="alarmLevel">  Error</param>
        /// <param name="alarmText"></param>

        public void AppErrorReport(string lineid, string errCode, string errMsg,string errStatus,string keyid,string sourcefrom)
        {
            try
            {
                //Report开关
                bool APPERRORSEND = true;
                if (ParameterManager.ContainsKey(eAPPErrorSend.APPERRORSEND))
                    APPERRORSEND = ParameterManager[eAPPErrorSend.APPERRORSEND].GetBoolean();
                if (!APPERRORSEND) return;

                XmlDocument doc = new XmlDocument();
                doc.LoadXml(MESSAGE);
                doc["MESSAGE"]["BODY"].InnerXml = APPERRORREPORT;
                doc["MESSAGE"]["HEADER"]["MESSAGENAME"].InnerText = "AppErrorReport";
                doc["MESSAGE"]["HEADER"]["TRANSACTIONID"].InnerText = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                doc["MESSAGE"]["BODY"]["TIMESTAMP"].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
                doc["MESSAGE"]["BODY"]["LINEID"].InnerText = lineid;
                doc["MESSAGE"]["BODY"]["ALARMLEVEL"].InnerText = "ERROR";
              //  if (sourcefrom.ToUpper().Equals("ROBOT"))
                    doc["MESSAGE"]["BODY"]["ERRORLEVEL"].InnerText = "3";
               // else
                //    doc["MESSAGE"]["BODY"]["ERRORLEVEL"].InnerText = "2";
                doc["MESSAGE"]["BODY"]["ERRORCODE"].InnerText = errCode;
                doc["MESSAGE"]["BODY"]["ERRORTEXT"].InnerText = errMsg;
                doc["MESSAGE"]["BODY"]["ERRORSTAT"].InnerText = errStatus;   //errorStatus  1.SET 2.CLEAR
                doc["MESSAGE"]["BODY"]["KEYID"].InnerText = keyid;   //new 2017/3/13  yang
                doc["MESSAGE"]["BODY"]["SOURCEFROM"].InnerText = sourcefrom;   //new 2017/3/13  yang

                string fileName = string.Format("{0}AppErrorReport_{2}_{1}.xml", EvisorFolder, DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.fff"), GetSeqNo());
                doc.Save(fileName);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                      fileName);
            }
            catch (System.Exception ex)
            {
                LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }
        
        /// <summary>
        /// OPI Connect Count
        /// </summary>
        /// <param name="lineId"></param>
        /// <param name="currentCount"></param>
        /// <param name="maxCount"></param>
        public void OPI_Connect_Count(string lineId,string currentCount,string maxCount)
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(OPI_CONNECT_COUNT);
                doc["MESSAGE"]["TIMESTAP"].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                doc["MESSAGE"]["LINE_ID"].InnerText = lineId;
                doc["MESSAGE"]["CURRENT_CONNECT_COUNT"].InnerText = currentCount;
                doc["MESSAGE"]["MAX_CONNECT_COUNT"].InnerText = maxCount;
                string fileName = string.Format("{0}t3_BC_OPI_CONNECT_COUNT_{2}_{1}.xml", EvisorFolder, DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.fff"), GetSeqNo());
                doc.Save(fileName);

            }
            catch (System.Exception ex)
            {
                LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void OPIConnectCountReport(string lineId, string currentCount, string maxCount) {
            try {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(MESSAGE);
                doc["MESSAGE"]["BODY"].InnerXml = OPICONNECTCOUNTREPORT;
                //2016/3/24 marine TIMESTAMP纠正拼错
                doc["MESSAGE"]["BODY"]["TIMESTAMP"].InnerText = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                doc["MESSAGE"]["BODY"]["LINEID"].InnerText = lineId;
                doc["MESSAGE"]["BODY"]["CURRENTCOUNT"].InnerText = currentCount;
                doc["MESSAGE"]["BODY"]["MAXCOUNT"].InnerText = maxCount;
                string fileName = string.Format("{0}OPIConnectCountReport_{2}_{1}.xml", EvisorFolder, DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss.fff"), GetSeqNo());
                doc.Save(fileName);

            } catch (System.Exception ex) {
                LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        #region [BC Shutdown] //20161209 sy add
        private System.Timers.Timer _timer;
        XmlDocument shutDownDoc;
        string updateStatus = string.Empty;

        //UpdateStatus updateStatus = UpdateStatus.UN;
        //private enum UpdateStatus
        //{
        //    /// <summary>
        //    /// Unknown
        //    /// </summary>
        //    A = 0,
        //    /// <summary>
        //    /// Wait for process
        //    /// </summary>
        //    Waitforprocess = 1,
        //    /// <summary>
        //    /// Fail
        //    /// </summary>
        //    Fail= 2,
        //}

        private void AfterInit()
        {
            _timer = new System.Timers.Timer(300);
            _timer.AutoReset = true;
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(BCShutDownCheck);
            //_timer.Start();
        }

        private void OnCreatedNewPackage(object sender, System.IO.FileSystemEventArgs e)
        {
            try
            {
                if (e.Name.Contains("NewPackageRelease"))
                {
                    Thread.Sleep(300);
                    try
                    {
                        UpgradeFileName = e.FullPath.Substring(e.FullPath.LastIndexOf("NewPackage"));
                        XmlReaderSettings settings = new XmlReaderSettings();
                        settings.IgnoreComments = true;
                        XmlReader reader = XmlReader.Create(e.FullPath, settings);                        
                        shutDownDoc = new XmlDocument();
                        shutDownDoc.Load(reader);
                        reader.Close();
                        //如果是要停止更新就直接retrun
                        if (shutDownDoc["VersionChangeInfo"]["UpgradeMode"].InnerText.ToLower() == "stop")
                        {
                            _timer.Stop();
                            LogWarn(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", "BC Upgrade Stop!!");
                            return;
                        }
                        _timer.Start();
                      //  Thread.Sleep(500);  先拿掉测试下,mark by yang 2017/4/1
                    }
                    catch (System.Exception ex)
                    {
                        LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
                        _timer.Stop();
                        updateStatus = "(Fail)";
                    }
                    finally
                    {
                        monitornewpackage = e.FullPath; 
                        File.Delete(e.FullPath);
                    }
                }
            }
            catch (Exception ex)
            {
                LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void BCShutDownCheck(object sender, ElapsedEventArgs e)
        {
            try
            {
                string fileName = string.Format("{0}{1}.xml", EvisorFolder, "BC_Shutdown");
                if (File.Exists(fileName)) return;
                if (shutDownDoc == null) return;
                ErrorFileLog("",true);
                updateStatus = "(Wait for upgrade)";

                //marked by yang 调整顺序
                XmlDocument Shutdown_Doc = new XmlDocument();
                //创建类型声明节点  
                XmlNode node = Shutdown_Doc.CreateXmlDeclaration("1.0", "utf-8", "");
                Shutdown_Doc.AppendChild(node);
                //创建根节点  
                XmlNode root = Shutdown_Doc.CreateElement("PackageName");
                Shutdown_Doc.AppendChild(root);
                Shutdown_Doc["PackageName"].InnerText = UpgradeFileName;
      

                if (shutDownDoc["VersionChangeInfo"]["ShutdownCheck"].InnerText == "Y") //不等Y 就不用 CHECK
                {
                    XmlNodeList shutDownList =shutDownDoc["VersionChangeInfo"]["ShutdownConditionsList"].ChildNodes;

                    if (shutDownList.Count == 0)
                    {
                        string err = "ShutdownCheck = [Y] ,But no ShutdownConditionsList";
                        LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", err);
                        ErrorFileLog(err);
                        _timer.Stop();
                        updateStatus = "(Fail)";
                        return;
                    }

                    if (File.Exists(monitornewpackage))
                    {
                        File.Delete(monitornewpackage);
                        LogWarn(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", "UnicomLog Monitor NewPackageRelease File Delete!!");  //yang 记下log
                    }
                    //这一轮check不生成shut_down file,等下一轮再生成  yang 2017/4/11  -old rule,mark掉
                    //marked by yang 2017/4/17 调整Check Flow,checkrules一定要在生成File之前的每次都要做Check（而且不可以做Sleep）
                    //modify :如果仍未删除,也要进行shutdown check,newpackagerelease file delete交由monitor client端处理
                    //if (!File.Exists(monitornewpackage))
                    //{
                        foreach (XmlNode item in shutDownList)
                        {
                            if (!shutDownCheckRules(item.InnerText)) return;
                        }
                    //}
                    //else
                    //{
                    //    File.Delete(monitornewpackage);
                    //    LogWarn(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", "UnicomLog Monitor NewPackageRelease File Delete,need Check Again!!");//yang 记下log
                    //    return;
                    //}
                }

                //XmlDocument Shutdown_Doc = new XmlDocument();
                ////创建类型声明节点  
                //XmlNode node = Shutdown_Doc.CreateXmlDeclaration("1.0", "utf-8", "");
                //Shutdown_Doc.AppendChild(node);
                ////创建根节点  
                //XmlNode root = Shutdown_Doc.CreateElement("PackageName");
                //Shutdown_Doc.AppendChild(root);
                //Shutdown_Doc["PackageName"].InnerText = UpgradeFileName;     

                    Shutdown_Doc.Save(fileName);
                    //File.WriteAllText(fileName, "Hallow World");
                    _timer.Stop();
                    updateStatus = string.Empty;
                //}
            }
            catch (Exception ex)
            {
                LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
                ErrorFileLog(ex.ToString());
                _timer.Stop();
                updateStatus = "(Fail)";
            }
        }

        private bool shutDownCheckRules(string rule)
        {
            string[] rules = rule.Split(',');//ruls,para1,para2....
            switch (rules[0])
            {
                case "PortSatusCheck":    //   <Conditions>PortSatusCheck,4,16</Conditions> para1 for ALL setting , para2 only for PCS CUT setting without L2 (L2 Use para1)
                    return CheckRule_PrtSatusCheck(rule);
                case "CuttingStartEvent":   //   <Conditions>CuttingStartEvent,10,300</Conditions> para1( sec )<= OK <=para2(sec)
                    return CheckRule_CuttingStartEvent(rule);
                case "CanShutdownBCTime":   //   <Conditions>CanShutdownBCTime,0830,1800</Conditions>
                    return CheckRule_CanShutdownBCTime(rule);
                case "RobotStatusCheck":    //   <Conditions>RobotStatusCheck,R,H</Conditions>   add by yang 20170206
                    return CheckRule_RobotStatusCheck(rule);
                default:
                    return false;
            }
        }

        #region [shutDownCheckRules]
        private bool CheckRule_PrtSatusCheck(string para)
        {
            try
            {
                string[] paras = para.Split(',');
                Line line = ObjectManager.LineManager.GetLine(Workbench.ServerName);
                foreach (Port port in ObjectManager.PortManager.GetPortsByLine(line.Data.LINEID))
                {
                    if (port.File.Status == ePortStatus.UN || port.File.CassetteStatus == eCassetteStatus.UNKNOWN) return false; //UNKNOWN
                    if (port.File.Status == ePortStatus.LC && port.File.CassetteStatus == eCassetteStatus.NO_CASSETTE_EXIST) return false; //2.1
                    if (port.File.Status == ePortStatus.LC && port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_CASSETTE_DATA) return false; //2.2
                    if (port.File.Status == ePortStatus.LC && port.File.CassetteStatus == eCassetteStatus.WAITING_FOR_START_COMMAND) return false; //2.3
                    if (port.File.Status == ePortStatus.LC && port.File.CassetteStatus == eCassetteStatus.PROCESS_COMPLETED) return false; //2.7
                    if (port.File.Status == ePortStatus.LC && port.File.CassetteStatus == eCassetteStatus.CASSETTE_REMAP) return false; //2.9
                    if (port.File.Status == ePortStatus.LC && port.File.CassetteStatus == eCassetteStatus.IN_ABORTING) return false; //2.12
                    if (port.File.Status == ePortStatus.UC && port.File.CassetteStatus == eCassetteStatus.PROCESS_COMPLETED) return false; //4.7
                    if (port.File.Status == ePortStatus.UC && port.File.CassetteStatus == eCassetteStatus.NO_CASSETTE_EXIST) return false; //4.1

                    if (port.File.Status == ePortStatus.LC && port.File.CassetteStatus == eCassetteStatus.IN_PROCESSING)
                    {
                        #region [IN_PROCESSING]
                        if (line.Data.LINETYPE == eLineType.CELL.CCPPK || line.Data.LINETYPE == eLineType.CELL.CCQPP || line.Data.LINETYPE == eLineType.CELL.CCCLN
                            || line.Data.LINETYPE == eLineType.CELL.CCOVP || line.Data.LINETYPE == eLineType.ARRAY.CAC_MYTEK)
                        {
                        }
                        else
                        {
                            if (port.File.Type == ePortType.LoadingPort || port.File.Type == ePortType.UnloadingPort || port.File.Type == ePortType.BothPort)
                            {
                                int checkJobCount;
                                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(port.Data.NODENO);
                                if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", port.Data.NODENO));
                                string trxName = string.Format("{0}_Port#{1}JobEachCassetteSlotPositionBlock", eqp.Data.NODENO, port.Data.PORTNO);
                                UniAuto.UniBCS.PLCAgent.PLC.Trx trxCassetteSlotPosition = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxName, false }) as UniAuto.UniBCS.PLCAgent.PLC.Trx;

                                #region [Job in CST Data]
                                IList<Job> lstJobinCST = new List<Job>();
                                for (int i = 0; i < trxCassetteSlotPosition.EventGroups[0].Events[0].Items.Count; i += 2)
                                {
                                    string cst_seq = trxCassetteSlotPosition.EventGroups[0].Events[0].Items[i].Value;
                                    string job_seq = trxCassetteSlotPosition.EventGroups[0].Events[0].Items[i + 1].Value;
                                    if (!cst_seq.Equals("0") || !job_seq.Equals("0"))
                                    {
                                        //Job j = new Job(int.Parse(cst_seq), int.Parse(job_seq));
                                        Job j = ObjectManager.JobManager.GetJob(cst_seq, job_seq);
                                        if (j != null)
                                        {
                                            lstJobinCST.Add(j);
                                        }
                                    }
                                }
                                #endregion
                                #region [Update check Job Count By Shop By Line]
                                checkJobCount = CheckJobCountSetting(line, eqp, port, paras.Length >= 2 ? int.Parse(paras[1]) : 0, paras.Length >= 3 ? int.Parse(paras[2]) : 0);
                                #endregion

                                #region [Check LoadingPort]
                                if (port.File.Type == ePortType.LoadingPort)
                                {
                                    List<Job> ret = null;
                                    ret = lstJobinCST.Where(j => j.SamplingSlotFlag == "1").ToList();//所有要出片的
                                    if (ret.Count < checkJobCount) return false;
                                }
                                #endregion
                                #region [Check UnloadingPort]
                                if (port.File.Type == ePortType.UnloadingPort)
                                {
                                    if ((port.Data.MAXCOUNT - lstJobinCST.Count) < checkJobCount) return false;//最大數量 - 現在的量 
                                }
                                #endregion
                                #region [Check BothPort]
                                if (port.File.Type == ePortType.BothPort)
                                {
                                    int lstJobinLineCount = ObjectManager.JobManager.GetJobs(port.File.CassetteSequenceNo).Where(j => !j.RemoveFlag).ToList().Count;//線內
                                    int lstJobinCSTCount = lstJobinCST.Where(j => !j.RemoveFlag).ToList().Count;//CST內
                                    int lstJobinCSTSamplingCount = lstJobinCST.Where(j => j.SamplingSlotFlag == "1" && j.JobProcessStartTime == j.CreateTime && !j.RemoveFlag).ToList().Count;//CST內 未出片

                                    if (((lstJobinLineCount - lstJobinCSTCount) + lstJobinCSTSamplingCount) < checkJobCount) return false;//(線內-CST內 = 正在流片 )  + 要出片的 但未出片的 數量
                                }
                                #endregion
                            }
                        }
                        #endregion
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }
        private int CheckJobCountSetting(Line line, Equipment eqp, Port port, int para1, int para2)
        {
            switch (line.Data.FABTYPE)
            {
                case "ARRAY":
                case "CF":
                    if (para1 != 0)
                        return para1;
                    return 4;
                case "CELL":
                    #region [CELL]
                    if (line.Data.LINEID.Contains(keyCellLineType.CUT) || line.Data.LINEID.Contains(keyCellLineType.PCS))//CUT PCS 有同時大小片之問題
                    {
                        if (eqp.Data.NODENO == "L2")
                        {
                            if (para1 != 0)
                                return para1;
                            return 4;
                        }
                        else
                        {
                            if (para2 != 0)
                                return para2;
                            return 16;
                        }
                    }
                    else if (line.Data.LINEID.Contains(keyCellLineType.POL))//POL TYPE        
                    {
                        if (para1 != 0)
                            return para1;
                        return 16;
                    }
                    else
                    {
                        switch (line.Data.LINETYPE)
                        {
                            case eLineType.CELL.CCPIL:
                            case eLineType.CELL.CCPIL_2:
                            case eLineType.CELL.CCODF:
                            case eLineType.CELL.CCODF_2:
                            case eLineType.CELL.CCQUP:
                            case eLineType.CELL.CCPDR:
                            case eLineType.CELL.CCTAM:
                            case eLineType.CELL.CCPTH:
                            case eLineType.CELL.CCGAP:
                            case eLineType.CELL.CCQSR:
                                if (para1 != 0)
                                    return para1;
                                return 4;
                            default:
                                if (para1 != 0)
                                    return para1;
                                return 16;
                        }
                    }
                    #endregion
                case "MODULE":
                    if (para1 != 0)
                        return para1;
                    return 16;
                default:
                    return 4;
            }
        }

        private bool CheckRule_CuttingStartEvent(string para)
        {
            string[] paras = para.Split(',');
            Line line = ObjectManager.LineManager.GetLine(Workbench.ServerName);
            if (line.Data.LINETYPE.Contains(keyCellLineType.CUT) || line.Data.LINETYPE.Contains(keyCellLineType.PCS))
            {
                TimeSpan ts = DateTime.Now.Subtract(line.File.LastCuttingStartEventTime);
                int minTime = paras.Length >= 2 ? int.Parse(paras[1]) : 10;
                int maxTime = paras.Length >= 3 ? int.Parse(paras[2]) : 300;

                if (ts.TotalSeconds < minTime) return false;
                if (ts.TotalSeconds > maxTime) return false;
            }
            return true;
        }
        private bool CheckRule_CanShutdownBCTime(string para)
        {
            string[] paras = para.Split(',');
            DateTime dt = DateTime.Now;
            int nowTime = int.Parse(string.Format("{0:HHmm}", dt));
            int startTime = int.Parse(paras[1]);
            int endTime = int.Parse(paras[2]);
            if (startTime < endTime)
            {
                //不跨日情況，例如0830~1830
                if (nowTime >= startTime && nowTime <= endTime)
                {
                    return true;
                }
            }
            else
            {
                //跨日情況，例如2200~0800
                if (nowTime >= startTime || nowTime <= endTime)
                {
                    return true;
                }
            }
            return false;
        }

        //add by yang 20170206
        private bool CheckRule_RobotStatusCheck(string para)
        {
            try
            {
                string[] paras = para.Split(',');
                Robot robot = ObjectManager.RobotManager.GetRobot("L2");
                if (robot!=null)  //CPC Check
                {
                    string RobotStatus = paras[1].ToUpper().Trim().Substring(0, 1).ToString();  //Check First character
                    string HaveCommandStatus = paras[2].ToUpper().Trim().Substring(0, 1).ToString(); //Check First character

                    if (robot.File.RobotControlCommandEQPReplyBitFlag) return false;  //这个不放在维护的xml里,hardcode by yang
                    if (robot.File.Status.ToString().Substring(0,1).ToUpper().Equals(RobotStatus)) return false;  //RB Run,Can't Replace Version
                    if (robot.File.RobotHasCommandstatus.ToString().Substring(0, 1).ToUpper().Equals(HaveCommandStatus)) return false;  //RB Have Command,Can Replace Version
                                  
                    lock (robot)   //add by yang 2017/6/6
                        robot.File.CmdSendCondition = true;  //robot cmd can't send
                }
                return true;
            }
           
            catch(Exception ex)
            {
                LogError(System.Reflection.MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }
        #endregion        

        private void ErrorFileLog(string log,bool onlyDeleteLogFile = false)
        {
            string fileName = string.Format("{0}{1}.xml", EvisorFolder, "ErrorLog");
            if (File.Exists(fileName))
                File.Delete(fileName);
            if (!onlyDeleteLogFile)           
                File.WriteAllText(fileName, log);
        }
        #endregion

        #region[mark:不使用]
        //add by yang 2017/2/17 for Enable/Disable APPErrorSendToBMS
        //Invoke Format
        /*
          Invoke(eServiceName.EvisorService,"APPErrorSendToBMS",new object[3]{lineid,errCode,errMsg});
         
        public void APPErrorSendToBMS(string lineid, string failCode, string alarmText,string alarmService)
        {

            try
            {
                bool APPERRORSEND = false;
                string errorStatus=string.Empty;
                Line line = ObjectManager.LineManager.GetLine(lineid);
                if (line == null) return;   
                Robot robot = ObjectManager.RobotManager.GetRobot("L2");

                #region[Save & Check]
                //Robot Error
                if (alarmService.ToUpper().Equals("ROBOT"))
                {

                    if (failCode.Equals(robot.RobotErrCode) && alarmText.Equals(robot.RobotErrMsg))  //Filter Robot Same Error
                        return;
                    robot.RobotErrCode = failCode;  //存最新的Error
                    robot.RobotErrMsg = alarmText;
                    if (failCode.ToUpper().Trim().Equals("CLEAR"))  robot.errorStatus = "2";  //ROBOTCLEAR
                    else robot.errorStatus = "1";  //ROBOTSET

                    errorStatus = robot.errorStatus;

                }
                 //Line Error( port + cassette and so on)
                else 
                    //if(alarmService.ToUpper().Equals("LINE"))
                {

                    if (failCode.Equals(line.File.LineErrCode) && alarmText.Equals(line.File.LineErrMsg))  //by Line Filter Same Error
                        return;

                    if (failCode.ToUpper().Trim().Equals("CLEAR")) line.File.errorStatus = "4";  //OTHERCLEAR
                    else line.File.errorStatus = "3";  //OTHERSET
                    line.File.LineErrCode = failCode;
                    line.File.LineErrMsg = alarmText;
                    lock (line)  //存最新的Error                  
                        ObjectManager.LineManager.EnqueueSave(line.File);
                    errorStatus = line.File.errorStatus;
                }
                #endregion

                //Report
                if (ParameterManager.ContainsKey(eAPPErrorSend.APPERRORSEND))
                    APPERRORSEND = ParameterManager[eAPPErrorSend.APPERRORSEND].GetBoolean();

                if (!APPERRORSEND) return;
                AppErrorReport(lineid, failCode, alarmText, errorStatus);
            }
            catch(Exception ex)
            {

            }
        }
         */
        #endregion


        /// <summary>
        /// AppErrorSet
        /// </summary>
        /// <param name="lineid"></param>
        /// <param name="ErrorList"></param>
        /// <param name="alarmService"></param>
        public void AppErrorSet(string lineid, Dictionary<string, Tuple<string, string, string, string>> CheckErrorList)
        {
            try
            {
                string errCode = string.Empty;
                string errMsg = string.Empty;
                string errStatus = string.Empty;
                Dictionary<string, Tuple<string, string, string, string>> CheckErrorRobottemp = new Dictionary<string, Tuple<string, string, string, string>>();
                Line line = ObjectManager.LineManager.GetLine(lineid);
                if (line == null) return;
                Robot robot = ObjectManager.RobotManager.GetRobot("L2");
                
                // Dic( ErrCode, (ErrMsg, GlassID, ErrStatus, OccurService) )
                // ErrStatus   0  Waiting For Send
                //             1  Have Sent
                       
                #region[Send Error & Update Error Status ->have sent]
                    foreach (KeyValuePair<string, Tuple<string, string, string, string>> err in CheckErrorList.Where(s=>s.Value.Item3.Equals("0")))
                    {
                        errCode = err.Key;
                        errMsg = err.Value.Item1;
                       // if (err.Value.Item4.ToUpper().Equals("ROBOT"))
                            errStatus = "1";
                        //Report
                        AppErrorReport(lineid, errCode, errMsg, errStatus,err.Value.Item2,err.Value.Item4);  // 1.SET, 2.CLEAR

                        if (err.Value.Item4.ToUpper().Equals("ROBOT"))
                            CheckErrorRobottemp.Add(err.Key, Tuple.Create(err.Value.Item1, err.Value.Item2, "1", err.Value.Item4));
                    } 
                    #region[Update]   //mark by yang ,先这样写,看以后能不能优化
                    for (int i = 0; i < CheckErrorRobottemp.Keys.Count(); i++)
                    {
                        string[] ar = CheckErrorRobottemp.Keys.ToArray();
                        CheckErrorList[ar[i]] = CheckErrorRobottemp[ar[i]];
                    }
                    #endregion
                #endregion
                
            }
            catch(Exception ex)
            { }
        }


        /// <summary>
        /// mark,可能以后会用到
        /// </summary>
        /// <param name="lineid"></param>
        /// <param name="CheckErrorList"></param>
        public void AppErrorClear(string lineid, Dictionary<string, Tuple<string, string, string, string>> CheckErrorList,string jobid)
        {
             try
            {
                string errCode = string.Empty;
                string errMsg = string.Empty;
                string errStatus = string.Empty;
                Dictionary<string, Tuple<string, string, string, string>> CheckErrorRobottemp = new Dictionary<string, Tuple<string, string, string, string>>();                
                Line line = ObjectManager.LineManager.GetLine(lineid);               
                if (line == null) return;
                Robot robot = ObjectManager.RobotManager.GetRobot("L2");
                
                // Dic( ErrCode, (ErrMsg, GlassID, ErrStatus, OccurService) )
                // ErrStatus   0  Waiting For Send
                //             1  Have Sent                                 

                #region[Send Error (errStatus=clear)  & Remove ]
                //传进来的是已经有cmd的glass,需要清掉have sent的error  (Report To BMS)        
              if (CheckErrorList.Where(s=>s.Value.Item3.Equals("1")&&s.Value.Item2.Equals(jobid)).Count() > 0)
                {
                    foreach (KeyValuePair<string, Tuple<string, string, string, string>> err in CheckErrorList.Where(s => s.Value.Item3.Equals("1") && s.Value.Item2.Equals(jobid)))
                    {
                        errCode = err.Key;
                        errMsg =  err.Value.Item1;
                        errStatus = "2";
                        //Report
                        AppErrorReport(lineid, errCode, errMsg, errStatus, err.Value.Item2,err.Value.Item4);  // 1.SET, 2.CLEAR

                        if (err.Value.Item4.ToUpper().Equals("ROBOT"))
                            CheckErrorRobottemp.Add(err.Key, err.Value);
                    }                                   
                }
                 //找出这一轮Wait的Error
                 foreach(KeyValuePair<string, Tuple<string, string, string, string>> err in CheckErrorList.Where(s => s.Value.Item3.Equals("0")))
                 {
                     if (!CheckErrorRobottemp.ContainsKey(err.Key))
                         CheckErrorRobottemp.Add(err.Key, err.Value);
                 }
                //每次cmd send,就进来判断,remove:  1.当前有命令的片对应的已经Set的Error（上面会置到Clear）  2.所有的Wait的Error
                 for(int i=0;i<CheckErrorRobottemp.Keys.Count();i++)
                 {
                     string[] ar=CheckErrorRobottemp.Keys.ToArray();
                     CheckErrorList.Remove(ar[i]);
                 }              
                #endregion
            }
            catch(Exception ex)
            { }
        }


    }
}
