using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Xml;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.Core.Generic;
using UniAuto.UniBCS.Core.Message;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.EntityManager;
using UniAuto.UniBCS.MesSpec;
using UniAuto.UniBCS.MISC;
using UniAuto.UniBCS.PLCAgent.PLC;
using UniAuto.UniBCS.Log;
using System.Timers;// Add By Yangzhenteng20180420;

namespace UniAuto.UniBCS.CSOT.LineSpecialService
{
      public partial class CELLSpecialService : AbstractService
      {
            private enum FABTYPE
            {
                  UNKNOWN = 0,
                  ARRAY = 1,
                  CELL = 2,
                  CF = 3
            }
            private class BaseInfo
            {
                  public string NodeNo { get; set; }
                  public string LoginID { get; set; }
                  public string Password { get; set; }
                  public string SourcePath { get; set; }
                  public string LocalTargetPath { get; set; }
                  public bool DeleteRemoteFile { get; set; }
                  public int ScanSecond { get; set; }
                  public DateTime LastScanTime { get; set; }
                  public BaseInfo()
                  {
                        NodeNo = LoginID = Password = SourcePath = LocalTargetPath = string.Empty;
                        DeleteRemoteFile = true;
                        ScanSecond = 10;// ten seconds
                        LastScanTime = DateTime.MinValue;
                  }
            }
            private class FTPInfo : BaseInfo
            {
                  public FTPInfo()
                  {
                  }
            }
            private class ShareFolderInfo : BaseInfo
            {
                  public bool Connected { get; set; }
                  public ShareFolderInfo()
                  {
                        Connected = false;
                  }
            }
            private class DefectFile
            {
                  public class DefectItem
                  {
                        public string DefectCode { get; set; }
                        public string CoordinateX { get; set; }
                        public string CoordinateY { get; set; }
                        public string ChipNo { get; set; }
                        public DefectItem()
                        {
                              DefectCode = CoordinateX = CoordinateY = ChipNo = string.Empty;
                        }
                        public DefectItem(string defectCode, string coordinateX, string coordinateY, string chipNo)
                        {
                              DefectCode = defectCode;
                              CoordinateX = coordinateX;
                              CoordinateY = coordinateY;
                              ChipNo = chipNo;
                        }
                  }

                  /// <summary>
                  /// Equipment Node No
                  /// </summary>
                  public string NodeNo { get; private set; }
                  /// <summary>
                  /// 完整檔名含路徑
                  /// </summary>
                  public string Filename { get; private set; }
                  /// <summary>
                  /// 檔名中的GlassID
                  /// </summary>
                  public string GlassID { get; private set; }
                  /// <summary>
                  /// 檔名中的日期時間, yyyyMMddHHmmss
                  /// </summary>
                  public string DateTimeStr { get; private set; }
                  /// <summary>
                  /// Dictionary[key, value], 檔案拆解後的全部Item與Value, 包括Head與Defect item
                  /// </summary>
                  public Dictionary<string, string> Data { get; private set; }
                  /// <summary>
                  /// 檔案內容中的CassetteSeqNo
                  /// </summary>
                  public string CassetteSeqNo
                  {
                        get
                        {
                              int tmp = 0;
                              if (Data.ContainsKey("Cassette_Sequence_No") && int.TryParse(Data["Cassette_Sequence_No"], out tmp))
                              {
                                    return tmp.ToString();
                              }
                              return string.Empty;
                        }
                  }
                  /// <summary>
                  /// 檔案內容中的JobSeqNo
                  /// </summary>
                  public string JobSeqNo
                  {
                        get
                        {
                              int tmp = 0;
                              if (Data.ContainsKey("Job_Sequence_No") && int.TryParse(Data["Job_Sequence_No"], out tmp))
                              {
                                    return tmp.ToString();
                              }
                              return string.Empty;
                        }
                  }
                  /// <summary>
                  /// 檔案內容中的Defect_Count
                  /// </summary>
                  public int Defect_Count
                  {
                        get
                        {
                              if (Data.ContainsKey("Defect_Count"))
                              {
                                    int count = 0;
                                    if (int.TryParse(Data["Defect_Count"], out count))
                                    {
                                          if (count < 0) count = 0;
                                          return count;
                                    }
                              }
                              return 0;
                        }
                  }
                  /// <summary>
                  /// 檔案拆解後的全部DefectItem
                  /// </summary>
                  public List<DefectItem> DefectItemItemList { get; private set; }
                  public DefectFile(string nodeNo, string filename, string glassID, string dateTimeStr)
                  {
                        NodeNo = nodeNo;
                        Filename = filename;
                        GlassID = glassID;
                        DateTimeStr = dateTimeStr;
                        Data = new Dictionary<string, string>();
                        DefectItemItemList = new List<DefectItem>();
                  }
                  public string GetDefectCodes()
                  {
                        StringBuilder ret = new StringBuilder();//DefectCode,DefectCode,DefectCode
                        foreach (DefectItem defect_item in DefectItemItemList)
                        {
                              ret.AppendFormat("{0},", defect_item.DefectCode);
                        }
                        if (ret.Length > 0)
                              ret.Remove(ret.Length - 1, 1);//移除最後一個逗號
                        return ret.ToString();
                  }
                  public static DefectFile Load(ILogManager logger, string logname, string nodeNo, string filename)
                  {
                        DefectFile ret = null;
                        StreamReader sr = null;
                        string fname = string.Empty;
                        try
                        {
                              fname = Path.GetFileNameWithoutExtension(filename);

                              logger.LogInfoWrite(logname, "CELLSpecialService", "DefectFile.Load()", string.Format("[EQUIPMENT={0}] [FileName={1}]", nodeNo, fname));

                              string ext = Path.GetExtension(filename);
                              string[] tmp = fname.Split('_');
                              if (tmp != null && tmp.Length == 3 && string.Compare(tmp[1], "DefectData", true) == 0 && string.Compare(ext, ".dat", true) == 0)
                              {
                                    sr = new StreamReader(filename);
                                    ret = new DefectFile(nodeNo, filename, tmp[0], tmp[2]);
                                    #region read file
                                    {
                                          while (!sr.EndOfStream)
                                          {
                                                // File Data Example
                                                //Cassette_Sequence_No=01001
                                                //Job_Sequence_No=00028
                                                //Glass_ID=TA080001AA
                                                //Defect_Count=10
                                                //Defect_Code#00001=AAA,123.456,234.567,1
                                                //Defect_Code#00002=BBB,123.456,234.567,1
                                                //Defect_Code#00003=CCC,123.456,234.567,2
                                                //Defect_Code#00004=DDD,123.456,234.567,2
                                                //Defect_Code#00005=EEE,123.456,234.567,2
                                                //Defect_Code#00006=FFF,123.456,234.567,3
                                                //Defect_Code#00007=GGG,123.456,234.567,3
                                                //Defect_Code#00008=HHH,123.456,234.567,4
                                                //Defect_Code#00009=UUU,123.456,234.567,5
                                                //Defect_Code#00010=RRR,123.456,234.567,6

                                                string str = sr.ReadLine();
                                                tmp = str.Split('=');
                                                if (tmp != null && tmp.Length == 2)
                                                {
                                                      if (ret.Data.ContainsKey(tmp[0]))
                                                            throw new Exception(string.Format("File[{0}] format Error, Key[{1}] is duplicated", fname, tmp[0]));

                                                      ret.Data.Add(tmp[0], tmp[1]);
                                                      if (tmp[0].IndexOf("Defect_Code#") == 0)
                                                      {
                                                            string[] t = tmp[1].Split(',');
                                                            if (t != null && t.Length == 4)
                                                            {
                                                                  ret.DefectItemItemList.Add(new DefectItem(t[0], t[1], t[2], t[3]));
                                                            }
                                                            else
                                                                  throw new Exception(string.Format("File[{0}] Defect data format Error, [{1}]", fname, str));
                                                      }
                                                }
                                          }
                                    }
                                    #endregion
                                    #region check header info
                                    {
                                          List<string> key_in_header = new List<string>();
                                          #region key_in_header
                                          {
                                                key_in_header.Add("Glass_ID");
                                          }
                                          #endregion

                                          List<string> key_in_header_int = new List<string>();
                                          #region key_in_header_int
                                          {
                                                key_in_header_int.Add("Cassette_Sequence_No");
                                                key_in_header_int.Add("Job_Sequence_No");
                                                key_in_header_int.Add("Defect_Count");
                                          }
                                          #endregion

                                          #region check header item
                                          {
                                                foreach (string key in key_in_header)
                                                {
                                                      if (!ret.Data.ContainsKey(key))
                                                            throw new Exception(string.Format("File[{0}] format Error, Item [{1}] is missing in file header", fname, key));
                                                }
                                                foreach (string key in key_in_header_int)
                                                {
                                                      if (!ret.Data.ContainsKey(key))
                                                            throw new Exception(string.Format("File[{0}] format Error, Item [{1}] is missing in file header", fname, key));
                                                      if (ret.Data[key] == string.Empty)
                                                            ret.Data[key] = "0";
                                                      int i = 0;
                                                      if (!int.TryParse(ret.Data[key], out i))
                                                            throw new Exception(string.Format("File[{0}] format Error, Item [{1}] Value [{2}] must be an integer", fname, key, ret.Data[key]));
                                                }
                                          }
                                          #endregion
                                          #region check glass id
                                          {
                                                if (ret.GlassID != ret.Data["Glass_ID"])
                                                      throw new Exception(string.Format("File[{0}] format Error, GlassID[{1}] is mismatch between file name and file header", fname, ret.Data["Glass_ID"]));
                                          }
                                          #endregion
                                          #region check edc item count
                                          {
                                                if (ret.Defect_Count != ret.DefectItemItemList.Count)
                                                      throw new Exception(string.Format("Defect_Count[{0}] is different from Defect File Body[{1}] in file.", ret.Defect_Count, ret.DefectItemItemList.Count));
                                          }
                                          #endregion
                                    }
                                    #endregion
                              }
                        }
                        catch (Exception ex)
                        {
                              ret = null;
                              logger.LogErrorWrite(logname, "CELLSpecialService", string.Format("DefectFile.Load({0})", fname), ex);
                        }
                        finally
                        {
                              if (sr != null)
                              {
                                    sr.Close();
                                    sr.Dispose();
                              }
                        }
                        return ret;
                  }
            }

            public class keyEQPEvent
            {
                public const string SendOutJobDataReport = "SendOutJobDataReport";
                public const string ReceiveJobDataReport = "ReceiveJobDataReport";
                public const string FetchOutJobDataReport = "FetchOutJobDataReport";
                public const string StoreJobDataReport = "StoreJobDataReport";
                public const string RemoveJobDataReport = "RemoveJobDataReport";
                public const string JobDataEditReport = "JobDataEditReport";
            }
            IDictionary<string, Queue<RealGlassCount>> _glassCountListQ = new Dictionary<string, Queue<RealGlassCount>>();

            private int _odfLastDelayTime = 0;//ODF Line Loader Last Delay Time 20150513 tom
            /// <summary>
            /// Dictionary[NodeNo, FTPInfo]
            /// </summary>
            private SortedDictionary<string, BaseInfo> dicReportBy = new SortedDictionary<string, BaseInfo>();
            private bool _runThread = false;

            public FileFormatManager FileFormatManager { get; set; }

            /// <summary>
            /// Config File 
            /// </summary>
            public string ConfigFileName { get; set; }

            IServerAgent _plcAgent = null;
            private IServerAgent PLCAgent
            {
                  get
                  {
                        if (_plcAgent == null)
                        {
                              _plcAgent = GetServerAgent(eAgentName.PLCAgent);
                        }
                        return _plcAgent;
                  }

            }
            IServerAgent _mesAgent = null;
            private IServerAgent MESAgent
            {
                get
                {
                    if (_mesAgent == null)
                    {
                        _mesAgent = GetServerAgent(eAgentName.MESAgent);
                    }
                    return _mesAgent;
                }

            }
            #region[For BUR Remote Check]
            private bool _ExecuteFlag = false;
            private Dictionary<string, RemoteRejudgePanel> GlassRejudgeSetCommand = new Dictionary<string, RemoteRejudgePanel>();
            private System.Timers.Timer CommandSetTimer;
            private void InitSetTimer()
            {
                CommandSetTimer = new System.Timers.Timer();
                CommandSetTimer.AutoReset = true;
                CommandSetTimer.Elapsed += new System.Timers.ElapsedEventHandler(OnSetTimedCommandEvent);
                CommandSetTimer.Interval = 2000; //每2S执行一次
                CommandSetTimer.Start();
            }
            private void OnSetTimedCommandEvent(object source, System.Timers.ElapsedEventArgs e)
            {
                try
                {
                    if (GlassRejudgeSetCommand.Count > 0)
                    {
                        lock (GlassRejudgeSetCommand)
                        {
                            var keys = GlassRejudgeSetCommand.Keys;
                            foreach (string key in keys)
                            {
                                RemoteRejudgePanel R = GlassRejudgeSetCommand[key];
                                if (R.IsSend == false && _ExecuteFlag == false)
                                {
                                    GlassRemoteRejudgeCommand(R);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }
            #endregion
            public override bool Init()
            {
                InitSetTimer();//Add By Yangzhenteng20180420;
                  #region 讀EDCbyFTPSetting.xml並啟動Thread
                  try
                  {
                        bool ftp = false, share_folder = false;
                        #region 讀取 \Config\Agent\IO\EDCbyFTPSetting.xml
                        {
                              XmlDocument doc = new XmlDocument();
                              doc.Load(ConfigFileName);
                              XmlNodeList server_list = doc.SelectNodes("//ROOT/DefectData/ServerName");
                              if (server_list != null)
                              {
                                    foreach (XmlElement server in server_list)
                                    {
                                          if (server.Attributes["name"].Value == Workbench.ServerName)
                                          {
                                                XmlNodeList node_list = server.SelectNodes("Node");
                                                foreach (XmlElement node in node_list)
                                                {
                                                      string node_no = node.Attributes["NodeNo"].Value;
                                                      if (!dicReportBy.ContainsKey(node_no))
                                                      {
                                                            BaseInfo info = null;
                                                            if (node.Attributes["Type"].Value == "FTP")
                                                            {
                                                                  info = new FTPInfo();
                                                                  ftp = true;
                                                            }
                                                            else if (node.Attributes["Type"].Value == "SHF")
                                                            {
                                                                  info = new ShareFolderInfo();
                                                                  share_folder = true;
                                                            }

                                                            if (info != null)
                                                            {
                                                                  info.NodeNo = node.Attributes["NodeNo"].Value;
                                                                  info.LoginID = node["LoginID"].InnerText;
                                                                  info.Password = node["Password"].InnerText;
                                                                  info.SourcePath = node["SourcePath"].InnerText;
                                                                  info.LocalTargetPath = node["LocalTargetPath"].InnerText.Replace("{ServerName}", Workbench.ServerName);
                                                                  if (node["ScanSecond"] != null)
                                                                  {
                                                                        int scan_sec = 0;
                                                                        if (int.TryParse(node["ScanSecond"].InnerText, out scan_sec))
                                                                              info.ScanSecond = scan_sec;
                                                                  }
                                                                  bool tmp = false;
                                                                  if (bool.TryParse(node["DeleteRemoteFile"].InnerText, out tmp))
                                                                        info.DeleteRemoteFile = tmp;

                                                                  if (info is ShareFolderInfo)
                                                                  {
                                                                        if (info.SourcePath.Length > 0 && info.SourcePath[info.SourcePath.Length - 1] == '\\')
                                                                              info.SourcePath = info.SourcePath.Remove(info.SourcePath.Length - 1);//共享資料夾連線, 網路路徑最後一碼不可以是'\', 否則會連不上. 微軟莫名其妙
                                                                  }
                                                                  dicReportBy.Add(node_no, info);
                                                            }
                                                      }
                                                }
                                          }
                                    }
                              }
                        }
                        #endregion
                        if (share_folder)
                        {
                              CloseShareFolderConnection(string.Empty);
                        }

                        if (ftp)
                        {
                              _runThread = true;
                              Thread scan_ftp_thread = new Thread(new ThreadStart(ScanFtpThreadFunc));
                              scan_ftp_thread.IsBackground = true;
                              scan_ftp_thread.Start();
                        }
                        if (share_folder)
                        {
                              _runThread = true;
                              Thread scan_share_folder_thread = new Thread(new ThreadStart(ScanShareFolderThreadFunc));
                              scan_share_folder_thread.IsBackground = true;
                              scan_share_folder_thread.Start();
                        }
                  }
                  catch (Exception ex)
                  {
                        Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                        throw ex;
                  }
                  #endregion
                  return true;
            }

            public void Destroy()
            {
                  try
                  {
                        _runThread = false;
                        foreach (object info in dicReportBy.Values)
                        {
                              if (info is ShareFolderInfo && ((ShareFolderInfo)info).Connected)
                              {
                                    CloseShareFolderConnection(((ShareFolderInfo)info).SourcePath);
                              }
                        }
                  }
                  catch (Exception ex)
                  {
                        Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                  }
            }

            #region Enum
            private enum eCELLOPLogINMode
            {
                  Login = 1,
                  OpLogout = 2,
                  AutoLogout = 3
            }

            private enum eCELLLDOperatorMode
            {
                  TFTMode_PI = 1,
                  CFMode_PI = 2,
                  TCModeByCount = 3
            }

            private enum eENGModeChangeInfo
            {
                  ENGModeEnable = 1,
                  ENGModeDisable = 2
            }

            private enum eCQLTModeChangeInfo
            {
                  CQLTModeEnable = 1,
                  CQLTModeDisable = 2
            }

            private enum eFirstRunModeChangeInfo
            {
                  FirstRunModeEnable = 1,
                  FirstRunModeDisable = 2
            }

            private enum eOperatorMode
            {
                  AutoMode = 1,
                  ManualMode = 2
            }

            private enum eENGModeChangeResult
            {
                  OK = 1,
                  NG = 2
            }

            private enum eResult
            {
                  Unknown = 0,
                  OK = 1,
                  NG = 2
            }

            private enum eCGMOResult
            {
                  Unknown = 0,
                  OK = 1,
                  NG = 2,
                  AlreadyHaveCGMOGlass = 3
            }

            private enum eVCRReadingMode
            {
                  Enable = 1,
                  Disable = 2
            }
            //Watson Add 20150307 For OPI ATS LDOperationMode/RunMode Command type 
            private class eOPIATSCmdType
            {
                  //cmdType: RUNMODE / LOADEROPERATIONMODE
                  public const string RUNMODE = "RUNMODE";
                  public const string LOADEROPERATIONMODE = "LOADEROPERATIONMODE";
            }
            #endregion

            #region [Cell Cassette Map Download]
            private const string CstMappingDownloadTimeout = "CstControlCommandTimeout";
            private const string DPCstControlCommandTimeout = "DPCstControlCommandTimeout";
            /// <summary>
            ///  NODENO_PORTNO_CstControlCommandTimeout : L2_01_CstControlCommandTimeout
            /// </summary>
            /// <param name="local"></param>
            /// <param name="port"></param>
            /// <param name="slotData"></param>
            /// <param name="outputData"></param>
            #region [T2 CassetteMapDownload]

            //public void CassetteMapDownload_PIL(Equipment local, Port port, IList<Job> slotData, Trx outputData)
            //{
            //    try
            //    {
            //        #region Special Job Data
            //        foreach (Job j in slotData)
            //        {
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LastGlassFlag"].Value = j.LastGlassFlag;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSP.JudgedData"].Value = j.InspJudgedData;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;

            //            #region [OXR Information]
            //            string oxrInfo = string.Empty;
            //            for (int i = 0; i < j.OXRInformation.Length; i++)
            //            {
            //                if (i.Equals(56)) break;
            //                oxrInfo += ConstantManager["PLC_OXRINFO_CELL"][j.OXRInformation.Substring(i, 1)].Value;
            //            }
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
            //            #endregion

            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ControlMode"].Value =((int)j.CellSpecial.ControlMode).ToString();
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TurnAngle"].Value = j.CellSpecial.TurnAngle == "" ? "1" : j.CellSpecial.TurnAngle;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ProductID"].Value = j.CellSpecial.ProductID;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CassetteSettingCode"].Value = j.CellSpecial.CassetteSettingCode;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OwnerID"].Value = j.CellSpecial.OwnerID;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["PPOSlotNo"].Value = j.CellSpecial.PPOSlotNo;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ReworkCount"].Value = j.CellSpecial.ReworkCount;
            //        }
            //        #endregion

            //        SendPLCData(outputData);

            //        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
            //        if (_timerManager.IsAliveTimer(timeoutName))
            //        {
            //            _timerManager.TerminateTimer(timeoutName);
            //        }

            //        _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
            //            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

            //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
            //            local.Data.NODENO, outputData.TrackKey, local.File.CIMMode.ToString()));
            //    }
            //    catch (Exception ex)
            //    {
            //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            //public void CassetteMapDownload_ODF(Equipment local, Port port, IList<Job> slotData, Trx outputData)
            //{
            //    try
            //    {
            //        #region Special Job Data
            //        foreach (Job j in slotData)
            //        {
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LastGlassFlag"].Value = j.LastGlassFlag;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSP.JudgedData"].Value = j.InspJudgedData;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;

            //            #region [OXR Information]
            //            string oxrInfo = string.Empty;
            //            for (int i = 0; i < j.OXRInformation.Length; i++)
            //            {
            //                if (i.Equals(56)) break;
            //                oxrInfo += ConstantManager["PLC_OXRINFO_CELL"][j.OXRInformation.Substring(i, 1)].Value;
            //            }
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
            //            #endregion

            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ControlMode"].Value = ((int)j.CellSpecial.ControlMode).ToString();
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ArrayTTPEQVersion"].Value = j.CellSpecial.ArrayTTPEQVer.ToString();
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ProductID"].Value = j.CellSpecial.ProductID;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CassetteSettingCode"].Value = j.CellSpecial.CassetteSettingCode;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OwnerID"].Value = j.CellSpecial.OwnerID;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["NetworkNo"].Value = j.CellSpecial.NetworkNo;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["RepairCount"].Value = j.CellSpecial.RepairCount;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TurnAngle"].Value = j.CellSpecial.TurnAngle == "" ? "1" : j.CellSpecial.TurnAngle;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["UVMaskAlreadyUseCount"].Value = j.CellSpecial.UVMaskAlreadyUseCount;
            //        }
            //        #endregion

            //        SendPLCData(outputData);

            //        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
            //        if (_timerManager.IsAliveTimer(timeoutName))
            //        {
            //            _timerManager.TerminateTimer(timeoutName);
            //        }

            //        _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
            //            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

            //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
            //            local.Data.NODENO, outputData.TrackKey, local.File.CIMMode.ToString()));
            //    }
            //    catch (Exception ex)
            //    {
            //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            public void CassetteMapDownload_HVA(Equipment local, Port port, IList<Job> slotData, Trx outputData)
            {
                  try
                  {
                        #region Special Job Data
                        foreach (Job j in slotData)
                        {
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LastGlassFlag"].Value = j.LastGlassFlag;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSP.JudgedData"].Value = j.InspJudgedData;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;

                              #region [OXR Information]
                              string oxrInfo = string.Empty;
                              for (int i = 0; i < j.OXRInformation.Length; i++)
                              {
                                    if (i.Equals(56)) break;
                                    oxrInfo += ConstantManager["PLC_OXRINFO_CELL"][j.OXRInformation.Substring(i, 1)].Value;
                              }
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
                              #endregion

                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ControlMode"].Value = ((int)j.CellSpecial.ControlMode).ToString();
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ProductID"].Value = j.CellSpecial.ProductID;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CassetteSettingCode"].Value = j.CellSpecial.CassetteSettingCode;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OwnerID"].Value = j.CellSpecial.OwnerID;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TurnAngle"].Value = j.CellSpecial.TurnAngle == "" ? "1" : j.CellSpecial.TurnAngle;
                        }
                        #endregion

                        SendPLCData(outputData);

                        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                              _timerManager.TerminateTimer(timeoutName);
                        }

                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
                            local.Data.NODENO, outputData.TrackKey, local.File.CIMMode.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            //public void CassetteMapDownload_CUT(Equipment local, Port port, IList<Job> slotData, Trx outputData)
            //{
            //    try
            //    {
            //        #region Special Job Data
            //        foreach (Job j in slotData)
            //        {
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LastGlassFlag"].Value = j.LastGlassFlag;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSP.JudgedData"].Value = j.InspJudgedData;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;

            //            #region [OXR Information]
            //            string oxrInfo = string.Empty;
            //            for (int i = 0; i < j.OXRInformation.Length; i++)
            //            {
            //                if (i.Equals(56)) break;
            //                oxrInfo += ConstantManager["PLC_OXRINFO_CELL"][j.OXRInformation.Substring(i, 1)].Value;
            //            }
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
            //            #endregion

            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["NetworkNo"].Value = j.CellSpecial.NetworkNo;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ProductID"].Value = j.CellSpecial.ProductID;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CassetteSettingCode"].Value = j.CellSpecial.CassetteSettingCode;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["AbnormalCode"].Value = j.CellSpecial.AbnormalCode;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["PanelSize"].Value = j.CellSpecial.PanelSize;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TurnAngle"].Value = j.CellSpecial.TurnAngle == "" ? "1" : j.CellSpecial.TurnAngle;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CrossLineCassetteSettingCode"].Value = j.CellSpecial.CrossLineCassetteSettingCode;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["PanelSizeFlag"].Value = j.CellSpecial.PanelSizeFlag;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["MMGFlag"].Value = j.CellSpecial.MMGFlag;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CrossLinePanelSize"].Value = j.CellSpecial.CrossLinePanelSize;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CUTProductID"].Value = j.CellSpecial.CUTProductID;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CUTCrossProductID"].Value = j.CellSpecial.CUTCrossProductID;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CUTProductType"].Value = j.CellSpecial.CUTProductType;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CUTCrossProductType"].Value = j.CellSpecial.CUTCrossProductType;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["POLProductType"].Value = j.CellSpecial.POLProductType;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["POLProductID"].Value = j.CellSpecial.POLProductID;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CrossLinePPID"].Value = j.CellSpecial.CrossLinePPID;
            //        }
            //        #endregion
            //        outputData.EventGroups[0].IsMergeEvent = true; //Watson Add for 連續資料寫入速度加快，僅適用於連續區域
            //        SendPLCData(outputData);

            //        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
            //        if (_timerManager.IsAliveTimer(timeoutName))
            //        {
            //            _timerManager.TerminateTimer(timeoutName);
            //        }

            //        _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
            //            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

            //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
            //            local.Data.NODENO, outputData.TrackKey, local.File.CIMMode.ToString()));
            //    }
            //    catch (Exception ex)
            //    {
            //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            //public void CassetteMapDownload_CUT_B(Equipment local, Port port, IList<Job> slotData, Trx outputData)
            //{
            //    try
            //    {
            //        #region Special Job Data
            //        foreach (Job j in slotData)
            //        {
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LastGlassFlag"].Value = j.LastGlassFlag;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSP.JudgedData"].Value = j.InspJudgedData;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;

            //            #region [OXR Information]
            //            string oxrInfo = string.Empty;
            //            for (int i = 0; i < j.OXRInformation.Length; i++)
            //            {
            //                if (i.Equals(56)) break;
            //                oxrInfo += ConstantManager["PLC_OXRINFO_CELL"][j.OXRInformation.Substring(i, 1)].Value;
            //            }
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
            //            #endregion

            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["NetworkNo"].Value = j.CellSpecial.NetworkNo;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ProductID"].Value = j.CellSpecial.ProductID;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CassetteSettingCodeA"].Value = j.CellSpecial.CassetteSettingCode;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["AbnormalCode"].Value = j.CellSpecial.AbnormalCode;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["PanelSize"].Value = j.CellSpecial.PanelSize;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CassetteSettingCodeB"].Value = j.CellSpecial.CassetteSettingCodeB;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CrossLineProductType"].Value = j.CellSpecial.CrossLineProductType;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CrossLineProductID"].Value = j.CellSpecial.CrossLineProductID;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CrossLinePPID"].Value = j.CellSpecial.CrossLinePPID;
            //        }
            //        #endregion

            //        SendPLCData(outputData);

            //        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
            //        if (_timerManager.IsAliveTimer(timeoutName))
            //        {
            //            _timerManager.TerminateTimer(timeoutName);
            //        }

            //        _timerManager.CreateTimer(timeoutName, false, ParameterManager["T1"].GetInteger(),
            //            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

            //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
            //            local.Data.NODENO, outputData.TrackKey, local.File.CIMMode.ToString()));
            //    }
            //    catch (Exception ex)
            //    {
            //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            //public void CassetteMapDownload_POL(Equipment local, Port port, IList<Job> slotData, Trx outputData)
            //{
            //    try
            //    {
            //        #region Special Job Data
            //        foreach (Job j in slotData)
            //        {
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LastGlassFlag"].Value = j.LastGlassFlag;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSP.JudgedData"].Value = j.InspJudgedData;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;

            //            #region [OXR Information]
            //            string oxrInfo = string.Empty;
            //            for (int i = 0; i < j.OXRInformation.Length; i++)
            //            {
            //                if (i.Equals(56)) break;
            //                oxrInfo += ConstantManager["PLC_OXRINFO_CELL"][j.OXRInformation.Substring(i, 1)].Value;
            //            }
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
            //            #endregion

            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ControlMode"].Value = ((int)j.CellSpecial.ControlMode).ToString();
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ProductID"].Value = j.CellSpecial.ProductID;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CassetteSettingCode"].Value = j.CellSpecial.CassetteSettingCode;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["AbnormalCode"].Value = j.CellSpecial.AbnormalCode;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["PanelSize"].Value = j.CellSpecial.PanelSize;
            //        }
            //        #endregion

            //        SendPLCData(outputData);

            //        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
            //        if (_timerManager.IsAliveTimer(timeoutName))
            //        {
            //            _timerManager.TerminateTimer(timeoutName);
            //        }

            //        _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
            //            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

            //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
            //            local.Data.NODENO, outputData.TrackKey, local.File.CIMMode.ToString()));
            //    }
            //    catch (Exception ex)
            //    {
            //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            //public void CassetteMapDownload_DPK(Equipment local, Port port, IList<Job> slotData, Trx outputData)
            //{
            //    try
            //    {
            //        #region Special Job Data
            //        foreach (Job j in slotData)
            //        {
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LastGlassFlag"].Value = j.LastGlassFlag;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSP.JudgedData"].Value = j.InspJudgedData;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;

            //            #region [OXR Information]
            //            string oxrInfo = string.Empty;
            //            for (int i = 0; i < j.OXRInformation.Length; i++)
            //            {
            //                if (i.Equals(56)) break;
            //                oxrInfo += ConstantManager["PLC_OXRINFO_CELL"][j.OXRInformation.Substring(i, 1)].Value;
            //            }
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
            //            #endregion

            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ControlMode"].Value = ((int)j.CellSpecial.ControlMode).ToString();
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ProductID"].Value = j.CellSpecial.ProductID;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CassetteSettingCode"].Value = j.CellSpecial.CassetteSettingCode;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["AbnormalCode"].Value = j.CellSpecial.AbnormalCode;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["PanelSize"].Value = j.CellSpecial.PanelSize;
            //        }
            //        #endregion

            //        SendPLCData(outputData);

            //        string timeOutType = string.Empty;

            //        if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
            //            timeOutType = DPCstControlCommandTimeout;
            //        else
            //            timeOutType = CstMappingDownloadTimeout;

            //        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, timeOutType);
            //        if (_timerManager.IsAliveTimer(timeoutName))
            //        {
            //            _timerManager.TerminateTimer(timeoutName);
            //        }

            //        _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
            //            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

            //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
            //            local.Data.NODENO, outputData.TrackKey, local.File.CIMMode.ToString()));
            //    }
            //    catch (Exception ex)
            //    {
            //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            //public void CassetteMapDownload_PMT(Equipment local, Port port, IList<Job> slotData, Trx outputData)
            //{
            //    try
            //    {
            //        #region Special Job Data
            //        foreach (Job j in slotData)
            //        {
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LastGlassFlag"].Value = j.LastGlassFlag;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSP.JudgedData"].Value = j.InspJudgedData;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;

            //            #region [OXR Information]
            //            string oxrInfo = string.Empty;
            //            for (int i = 0; i < j.OXRInformation.Length; i++)
            //            {
            //                if (i.Equals(56)) break;
            //                oxrInfo += ConstantManager["PLC_OXRINFO_CELL"][j.OXRInformation.Substring(i, 1)].Value;
            //            }
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
            //            #endregion

            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ControlMode"].Value = ((int)j.CellSpecial.ControlMode).ToString();
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ProductID"].Value = j.CellSpecial.ProductID;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CassetteSettingCode"].Value = j.CellSpecial.CassetteSettingCode;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OwnerID"].Value = j.CellSpecial.OwnerID;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["RunMode"].Value = j.CellSpecial.RunMode;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["NodeStack"].Value = j.CellSpecial.NodeStack;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["AbnormalCode"].Value = j.CellSpecial.AbnormalCode;
            //        }
            //        #endregion

            //        SendPLCData(outputData);

            //        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
            //        if (_timerManager.IsAliveTimer(timeoutName))
            //        {
            //            _timerManager.TerminateTimer(timeoutName);
            //        }

            //        _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
            //            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

            //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
            //            local.Data.NODENO, outputData.TrackKey, local.File.CIMMode.ToString()));
            //    }
            //    catch (Exception ex)
            //    {
            //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            //public void CassetteMapDownload_GAP(Equipment local, Port port, IList<Job> slotData, Trx outputData)
            //{
            //    try
            //    {
            //        #region Special Job Data
            //        foreach (Job j in slotData)
            //        {
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LastGlassFlag"].Value = j.LastGlassFlag;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSP.JudgedData"].Value = j.InspJudgedData;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;

            //            #region [OXR Information]
            //            string oxrInfo = string.Empty;
            //            for (int i = 0; i < j.OXRInformation.Length; i++)
            //            {
            //                if (i.Equals(56)) break;
            //                oxrInfo += ConstantManager["PLC_OXRINFO_CELL"][j.OXRInformation.Substring(i, 1)].Value;
            //            }
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
            //            #endregion

            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ControlMode"].Value = ((int)j.CellSpecial.ControlMode).ToString();
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ProductID"].Value = j.CellSpecial.ProductID;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CassetteSettingCode"].Value = j.CellSpecial.CassetteSettingCode;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OwnerID"].Value = j.CellSpecial.OwnerID;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["NodeStack"].Value = j.CellSpecial.NodeStack;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["AbnormalCode"].Value = j.CellSpecial.AbnormalCode;
            //        }
            //        #endregion

            //        SendPLCData(outputData);

            //        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
            //        if (_timerManager.IsAliveTimer(timeoutName))
            //        {
            //            _timerManager.TerminateTimer(timeoutName);
            //        }

            //        _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
            //            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

            //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
            //            local.Data.NODENO, outputData.TrackKey, local.File.CIMMode.ToString()));
            //    }
            //    catch (Exception ex)
            //    {
            //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            //public void CassetteMapDownload_PIS(Equipment local, Port port, IList<Job> slotData, Trx outputData)
            //{
            //    try
            //    {
            //        #region Special Job Data
            //        foreach (Job j in slotData)
            //        {
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LastGlassFlag"].Value = j.LastGlassFlag;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSP.JudgedData"].Value = j.InspJudgedData;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;

            //            #region [OXR Information]
            //            string oxrInfo = string.Empty;
            //            for (int i = 0; i < j.OXRInformation.Length; i++)
            //            {
            //                if (i.Equals(56)) break;
            //                oxrInfo += ConstantManager["PLC_OXRINFO_CELL"][j.OXRInformation.Substring(i, 1)].Value;
            //            }
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
            //            #endregion

            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["NetworkNo"].Value = j.CellSpecial.NetworkNo;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ProductID"].Value = j.CellSpecial.ProductID;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CassetteSettingCode"].Value = j.CellSpecial.CassetteSettingCode;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["AbnormalCode"].Value = j.CellSpecial.AbnormalCode;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["PanelSize"].Value = j.CellSpecial.PanelSize;
            //        }
            //        #endregion

            //        SendPLCData(outputData);

            //        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
            //        if (_timerManager.IsAliveTimer(timeoutName))
            //        {
            //            _timerManager.TerminateTimer(timeoutName);
            //        }

            //        _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
            //            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

            //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
            //            local.Data.NODENO, outputData.TrackKey, local.File.CIMMode.ToString()));
            //    }
            //    catch (Exception ex)
            //    {
            //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            public void CassetteMapDownload_PRM(Equipment local, Port port, IList<Job> slotData, Trx outputData)
            {
                  try
                  {
                        #region Special Job Data
                        foreach (Job j in slotData)
                        {
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LastGlassFlag"].Value = j.LastGlassFlag;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSP.JudgedData"].Value = j.InspJudgedData;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;

                              #region [OXR Information]
                              string oxrInfo = string.Empty;
                              for (int i = 0; i < j.OXRInformation.Length; i++)
                              {
                                    if (i.Equals(56)) break;
                                    oxrInfo += ConstantManager["PLC_OXRINFO_CELL"][j.OXRInformation.Substring(i, 1)].Value;
                              }
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
                              #endregion

                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ControlMode"].Value = ((int)j.CellSpecial.ControlMode).ToString();
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ProductID"].Value = j.CellSpecial.ProductID;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CassetteSettingCode"].Value = j.CellSpecial.CassetteSettingCode;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["AbnormalCode"].Value = j.CellSpecial.AbnormalCode;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["PanelSize"].Value = j.CellSpecial.PanelSize;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["RunMode"].Value = j.CellSpecial.RunMode;
                        }
                        #endregion

                        SendPLCData(outputData);

                        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                              _timerManager.TerminateTimer(timeoutName);
                        }

                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
                            local.Data.NODENO, outputData.TrackKey, local.File.CIMMode.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            public void CassetteMapDownload_GMO(Equipment local, Port port, IList<Job> slotData, Trx outputData)
            {
                  try
                  {
                        #region Special Job Data
                        foreach (Job j in slotData)
                        {
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LastGlassFlag"].Value = j.LastGlassFlag;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSP.JudgedData"].Value = j.InspJudgedData;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;

                              #region [OXR Information]
                              string oxrInfo = string.Empty;
                              for (int i = 0; i < j.OXRInformation.Length; i++)
                              {
                                    if (i.Equals(56)) break;
                                    oxrInfo += ConstantManager["PLC_OXRINFO_CELL"][j.OXRInformation.Substring(i, 1)].Value;
                              }
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
                              #endregion

                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ControlMode"].Value = ((int)j.CellSpecial.ControlMode).ToString();
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ProductID"].Value = j.CellSpecial.ProductID;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CassetteSettingCode"].Value = j.CellSpecial.CassetteSettingCode;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["AbnormalCode"].Value = j.CellSpecial.AbnormalCode;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["PanelSize"].Value = j.CellSpecial.PanelSize;
                        }
                        #endregion

                        SendPLCData(outputData);

                        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                              _timerManager.TerminateTimer(timeoutName);
                        }

                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
                            local.Data.NODENO, outputData.TrackKey, local.File.CIMMode.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            public void CassetteMapDownload_LOI(Equipment local, Port port, IList<Job> slotData, Trx outputData)
            {
                  try
                  {
                        #region Special Job Data
                        foreach (Job j in slotData)
                        {
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LastGlassFlag"].Value = j.LastGlassFlag;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSP.JudgedData"].Value = j.InspJudgedData;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;

                              #region [OXR Information]
                              string oxrInfo = string.Empty;
                              for (int i = 0; i < j.OXRInformation.Length; i++)
                              {
                                    if (i.Equals(56)) break;
                                    oxrInfo += ConstantManager["PLC_OXRINFO_CELL"][j.OXRInformation.Substring(i, 1)].Value;
                              }
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
                              #endregion

                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["NetworkNo"].Value = j.CellSpecial.NetworkNo;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ProductID"].Value = j.CellSpecial.ProductID;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CassetteSettingCode"].Value = j.CellSpecial.CassetteSettingCode;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["AbnormalCode"].Value = j.CellSpecial.AbnormalCode;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["PanelSize"].Value = j.CellSpecial.PanelSize;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["RepairResult"].Value = j.CellSpecial.RepairResult == string.Empty ? "0" : j.CellSpecial.RepairResult;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["RunMode"].Value = j.CellSpecial.RunMode;
                              //outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["VirtualPortEnableMode"].Value = j.CellSpecial.VirtualPortEnableMode;
                        }
                        #endregion

                        SendPLCData(outputData);

                        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                              _timerManager.TerminateTimer(timeoutName);
                        }

                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
                            local.Data.NODENO, outputData.TrackKey, local.File.CIMMode.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            public void CassetteMapDownload_NRP(Equipment local, Port port, IList<Job> slotData, Trx outputData)
            {
                  try
                  {
                        #region Special Job Data
                        foreach (Job j in slotData)
                        {
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LastGlassFlag"].Value = j.LastGlassFlag;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSP.JudgedData"].Value = j.InspJudgedData;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;

                              #region [OXR Information]
                              string oxrInfo = string.Empty;
                              for (int i = 0; i < j.OXRInformation.Length; i++)
                              {
                                    if (i.Equals(56)) break;
                                    oxrInfo += ConstantManager["PLC_OXRINFO_CELL"][j.OXRInformation.Substring(i, 1)].Value;
                              }
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
                              #endregion

                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["NetworkNo"].Value = j.CellSpecial.NetworkNo;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ProductID"].Value = j.CellSpecial.ProductID;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CassetteSettingCode"].Value = j.CellSpecial.CassetteSettingCode;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["AbnormalCode"].Value = j.CellSpecial.AbnormalCode;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["PanelSize"].Value = j.CellSpecial.PanelSize;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["RepairResult"].Value = j.CellSpecial.RepairResult;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["RepairCount"].Value = j.CellSpecial.RepairCount;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["RunMode"].Value = j.CellSpecial.RunMode;
                        }
                        #endregion

                        SendPLCData(outputData);

                        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                              _timerManager.TerminateTimer(timeoutName);
                        }

                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
                            local.Data.NODENO, outputData.TrackKey, local.File.CIMMode.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            public void CassetteMapDownload_OLS(Equipment local, Port port, IList<Job> slotData, Trx outputData)
            {
                  try
                  {
                        #region Special Job Data
                        foreach (Job j in slotData)
                        {
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LastGlassFlag"].Value = j.LastGlassFlag;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSP.JudgedData"].Value = j.InspJudgedData;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;

                              #region [OXR Information]
                              string oxrInfo = string.Empty;
                              for (int i = 0; i < j.OXRInformation.Length; i++)
                              {
                                    if (i.Equals(56)) break;
                                    oxrInfo += ConstantManager["PLC_OXRINFO_CELL"][j.OXRInformation.Substring(i, 1)].Value;
                              }
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
                              #endregion

                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ControlMode"].Value = ((int)j.CellSpecial.ControlMode).ToString();
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ProductID"].Value = j.CellSpecial.ProductID;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CassetteSettingCode"].Value = j.CellSpecial.CassetteSettingCode;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["AbnormalCode"].Value = j.CellSpecial.AbnormalCode;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["PanelSize"].Value = j.CellSpecial.PanelSize;
                        }
                        #endregion

                        SendPLCData(outputData);

                        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                              _timerManager.TerminateTimer(timeoutName);
                        }

                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
                            local.Data.NODENO, outputData.TrackKey, local.File.CIMMode.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            //public void CassetteMapDownload_SOR(Equipment local, Port port, IList<Job> slotData, Trx outputData)
            //{
            //    try
            //    {
            //        #region Special Job Data
            //        foreach (Job j in slotData)
            //        {
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LastGlassFlag"].Value = j.LastGlassFlag;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSP.JudgedData"].Value = j.InspJudgedData;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;

            //            #region [OXR Information]
            //            string oxrInfo = string.Empty;
            //            for (int i = 0; i < j.OXRInformation.Length; i++)
            //            {
            //                if (i.Equals(56)) break;
            //                oxrInfo += ConstantManager["PLC_OXRINFO_CELL"][j.OXRInformation.Substring(i, 1)].Value;
            //            }
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
            //            #endregion

            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ControlMode"].Value = ((int)j.CellSpecial.ControlMode).ToString();
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ProductID"].Value = j.CellSpecial.ProductID;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CassetteSettingCode"].Value = j.CellSpecial.CassetteSettingCode;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["AbnormalCode"].Value = j.CellSpecial.AbnormalCode;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["RunMode"].Value = j.CellSpecial.RunMode;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["PanelSize"].Value = j.CellSpecial.PanelSize;
            //            outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["NodeStack"].Value = j.CellSpecial.NodeStack;
            //        }
            //        #endregion

            //        SendPLCData(outputData);

            //        string timeOutType = string.Empty;

            //        if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
            //            timeOutType = DPCstControlCommandTimeout;
            //        else
            //            timeOutType = CstMappingDownloadTimeout;

            //        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, timeOutType);
            //        if (_timerManager.IsAliveTimer(timeoutName))
            //        {
            //            _timerManager.TerminateTimer(timeoutName);
            //        }

            //        _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
            //            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

            //        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
            //            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
            //            local.Data.NODENO, outputData.TrackKey, local.File.CIMMode.ToString()));
            //    }
            //    catch (Exception ex)
            //    {
            //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            public void CassetteMapDownload_DPS(Equipment local, Port port, IList<Job> slotData, Trx outputData)
            {
                  try
                  {
                        #region Special Job Data
                        foreach (Job j in slotData)
                        {
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LastGlassFlag"].Value = j.LastGlassFlag;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSP.JudgedData"].Value = j.InspJudgedData;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;

                              #region [OXR Information]
                              string oxrInfo = string.Empty;
                              for (int i = 0; i < j.OXRInformation.Length; i++)
                              {
                                    if (i.Equals(56)) break;
                                    oxrInfo += ConstantManager["PLC_OXRINFO_CELL"][j.OXRInformation.Substring(i, 1)].Value;
                              }
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
                              #endregion

                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ControlMode"].Value = ((int)j.CellSpecial.ControlMode).ToString();
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ProductID"].Value = j.CellSpecial.ProductID;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CassetteSettingCode"].Value = j.CellSpecial.CassetteSettingCode;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["AbnormalCode"].Value = j.CellSpecial.AbnormalCode;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["PanelSize"].Value = j.CellSpecial.PanelSize;
                        }
                        #endregion

                        SendPLCData(outputData);

                        string timeOutType = string.Empty;

                        if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
                              timeOutType = DPCstControlCommandTimeout;
                        else
                              timeOutType = CstMappingDownloadTimeout;

                        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, timeOutType);
                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                              _timerManager.TerminateTimer(timeoutName);
                        }

                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
                            local.Data.NODENO, outputData.TrackKey, local.File.CIMMode.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            public void CassetteMapDownload_ATS(Equipment local, Port port, IList<Job> slotData, Trx outputData)
            {
                  try
                  {
                        #region Special Job Data
                        foreach (Job j in slotData)
                        {
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LastGlassFlag"].Value = j.LastGlassFlag;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSP.JudgedData"].Value = j.InspJudgedData;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;

                              #region [OXR Information]
                              string oxrInfo = string.Empty;
                              for (int i = 0; i < j.OXRInformation.Length; i++)
                              {
                                    if (i.Equals(56)) break;
                                    oxrInfo += ConstantManager["PLC_OXRINFO_CELL"][j.OXRInformation.Substring(i, 1)].Value;
                              }
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
                              #endregion

                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["NetworkNo"].Value = j.CellSpecial.NetworkNo;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ProductID"].Value = j.CellSpecial.ProductID;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CassetteSettingCode"].Value = j.CellSpecial.CassetteSettingCode;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["AbnormalCode"].Value = j.CellSpecial.AbnormalCode;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["PanelSize"].Value = j.CellSpecial.PanelSize;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["NodeStack"].Value = j.CellSpecial.NodeStack;
                        }
                        #endregion

                        SendPLCData(outputData);

                        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                              _timerManager.TerminateTimer(timeoutName);
                        }

                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
                            local.Data.NODENO, outputData.TrackKey, local.File.CIMMode.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            public void CassetteMapDownload_DPI(Equipment local, Port port, IList<Job> slotData, Trx outputData)
            {
                  try
                  {
                        #region Special Job Data
                        foreach (Job j in slotData)
                        {
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LastGlassFlag"].Value = j.LastGlassFlag;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSP.JudgedData"].Value = j.InspJudgedData;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;

                              #region [OXR Information]
                              string oxrInfo = string.Empty;
                              for (int i = 0; i < j.OXRInformation.Length; i++)
                              {
                                    if (i.Equals(56)) break;
                                    oxrInfo += ConstantManager["PLC_OXRINFO_CELL"][j.OXRInformation.Substring(i, 1)].Value;
                              }
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
                              #endregion

                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ControlMode"].Value = ((int)j.CellSpecial.ControlMode).ToString();
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ProductID"].Value = j.CellSpecial.ProductID;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CassetteSettingCode"].Value = j.CellSpecial.CassetteSettingCode;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["AbnormalCode"].Value = j.CellSpecial.AbnormalCode;
                        }
                        #endregion

                        SendPLCData(outputData);

                        string timeOutType = string.Empty;

                        if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
                              timeOutType = DPCstControlCommandTimeout;
                        else
                              timeOutType = CstMappingDownloadTimeout;

                        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, timeOutType);
                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                              _timerManager.TerminateTimer(timeoutName);
                        }

                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
                            local.Data.NODENO, outputData.TrackKey, local.File.CIMMode.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            public void CassetteMapDownload_UVA(Equipment local, Port port, IList<Job> slotData, Trx outputData)
            {
                  try
                  {
                        #region Special Job Data
                        foreach (Job j in slotData)
                        {
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["LastGlassFlag"].Value = j.LastGlassFlag;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["INSP.JudgedData"].Value = j.InspJudgedData;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["TrackingData"].Value = j.TrackingData;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;

                              #region [OXR Information]
                              string oxrInfo = string.Empty;
                              for (int i = 0; i < j.OXRInformation.Length; i++)
                              {
                                    if (i.Equals(56)) break;
                                    oxrInfo += ConstantManager["PLC_OXRINFO_CELL"][j.OXRInformation.Substring(i, 1)].Value;
                              }
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OXRInformation"].Value = oxrInfo;
                              #endregion

                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ChipCount"].Value = j.ChipCount.ToString();

                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ControlMode"].Value = ((int)j.CellSpecial.ControlMode).ToString();
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["ProductID"].Value = j.CellSpecial.ProductID;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["CassetteSettingCode"].Value = j.CellSpecial.CassetteSettingCode;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["OwnerID"].Value = j.CellSpecial.OwnerID;
                        }
                        #endregion

                        SendPLCData(outputData);

                        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                              _timerManager.TerminateTimer(timeoutName);
                        }

                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
                            local.Data.NODENO, outputData.TrackKey, local.File.CIMMode.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            //Watson add 20150122 For CELL Mask Cleaner
            public void CassetteMapDownload_MCL(Equipment local, Port port, IList<Job> slotData, Trx outputData)
            {
                  try
                  {
                        #region Special Job Data
                        foreach (Job j in slotData)
                        {
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["MaskID"].Value = j.CellSpecial.MASKID;
                              outputData.EventGroups[0].Events[int.Parse(j.JobSequenceNo) - 1].Items["EQPFlag"].Value = j.EQPFlag;
                        }
                        #endregion

                        SendPLCData(outputData);

                        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                              _timerManager.TerminateTimer(timeoutName);
                        }

                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
                            local.Data.NODENO, outputData.TrackKey, local.File.CIMMode.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            //T3 shihyang 20150911 add 將job by shop by line 整合 
            /// <summary>
            /// Cassette Map Download For CELL By Line
            /// </summary>
            /// <param name="eqp">eqp</param>
            /// <param name="port">port</param>
            /// <param name="slotData">Job List</param>
            /// <param name="outputData">Trx</param>
            public void CassetteMapDownload_CELL(Equipment eqp, Port port, IList<Job> slotData, Trx outputData)
            {
                  try
                  {
                        #region[Get  LINE]
                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                        #endregion
                        if (port.File.CellCst != eBitResult.ON)//shihyang add For CELL CST 20151019
                        {
                              foreach (Job job in slotData)
                              {
                                    #region CELL Special Job Data
                                    outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.INSPReservations].Value = job.INSPReservations;   //BIN
                                    outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.EQPReservations].Value = job.EQPReservations;   //BIN
                                    outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.LastGlassFlag].Value = job.LastGlassFlag;   ////INT
                                    outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.InspJudgedData].Value = job.InspJudgedData;   //BIN
                                    outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.TrackingData].Value = job.TrackingData;   //BIN
                                    outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.EQPFlag].Value = job.EQPFlag;   //BIN
                                    outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.ChipCount].Value = job.ChipCount.ToString();   //INT

                                    outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.ProductID].Value = job.CellSpecial.ProductID;
                                    outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.CassetteSettingCode].Value = job.CellSpecial.CassetteSettingCode;
                                    switch (line.Data.JOBDATALINETYPE)
                                    {
                                        #region [T3 rule]
                                        case eJobDataLineType.CELL.CCPIL:
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.BlockOXInformation].Value = ObjectManager.JobManager.M2P_CELL_BlockOX2Bin( job.CellSpecial.BlockOXInformation);
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle == "" ? "1" : job.CellSpecial.TurnAngle;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.GlassThickness].Value = job.CellSpecial.GlassThickness;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.OperationID].Value = job.CellSpecial.OperationID;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.OwnerID].Value = job.CellSpecial.OwnerID;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.ProductOwner].Value = job.CellSpecial.ProductOwner;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.PILiquidType].Value = job.CellSpecial.PILiquidType;
                                                break;
                                          case eJobDataLineType.CELL.CCODF:
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.BlockOXInformation].Value = ObjectManager.JobManager.M2P_CELL_BlockOX2Bin(job.CellSpecial.BlockOXInformation);
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle == "" ? "1" : job.CellSpecial.TurnAngle;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.GlassThickness].Value = job.CellSpecial.GlassThickness;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.OperationID].Value = job.CellSpecial.OperationID;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.OwnerID].Value = job.CellSpecial.OwnerID;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.ProductOwner].Value = job.CellSpecial.ProductOwner;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.AssembleSeqNo].Value = job.CellSpecial.AssembleSeqNo;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.UVMaskAlreadyUseCount].Value = job.CellSpecial.UVMaskAlreadyUseCount;
                                                break;
                                          case eJobDataLineType.CELL.CCPCS:
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.BlockOXInformation].Value = ObjectManager.JobManager.M2P_CELL_BlockOX2Bin(job.CellSpecial.BlockOXInformation);
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle == "" ? "1" : job.CellSpecial.TurnAngle;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.BlockSize].Value = job.CellSpecial.BlockSize;
                                            /* 20170714 huangjiayin modify: these itmes do not use forever
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.PCSProductID].Value = job.CellSpecial.CUTProductID;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.PCSProductType].Value = job.CellSpecial.CUTProductType;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.PCSProductID2].Value = job.CellSpecial.CUTProductID2;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.PCSProductType2].Value = job.CellSpecial.CUTProductType2;
                                             */
                                                //20170714 huangjiayin add: PCSCassetteSettingCodeList
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.PCSCassetteSettingCodeList].Value = job.CellSpecial.PCSCassetteSettingCodeList;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.PCSCassetteSettingCode].Value = job.CellSpecial.CUTCassetteSettingCode;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.PCSCassetteSettingCode2].Value = job.CellSpecial.CUTCassetteSettingCode2;
                                                //20170724 huangjiayin add: BlockSize1&2
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.BlockSize1].Value = job.CellSpecial.BlockSize1;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.BlockSize2].Value = job.CellSpecial.BlockSize2;
                                            //20170725 huangjiayin add: PCSBlockSizeList
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.PCSBlockSizeList].Value = job.CellSpecial.PCSBlockSizeList;
                                                break;
                                          case eJobDataLineType.CELL.CCCUT:
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.PanelOXInformation].Value = ObjectManager.JobManager.M2P_CELL_PanelOX2Int(job.CellSpecial.PanelOXInformation);
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle == "" ? "1" : job.CellSpecial.TurnAngle;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.PanelSize].Value = job.CellSpecial.PanelSize;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.CUTProductID].Value = job.CellSpecial.CUTProductID;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.CUTProductType].Value = job.CellSpecial.CUTProductType;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.DefectCode].Value = job.CellSpecial.DefectCode;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.RejudgeCount].Value = job.CellSpecial.RejudgeCount;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.VendorName].Value = job.CellSpecial.VendorName;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.BURCheckCount].Value = job.CellSpecial.BURCheckCount;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.CUTCassetteSettingCode].Value = job.CellSpecial.CUTCassetteSettingCode;
                                                break;
                                          case eJobDataLineType.CELL.CCPCK:
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.PanelGroup].Value = job.CellSpecial.PanelGroup;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.OQCBank].Value = job.CellSpecial.OQCBank;
                                                break;
                                          case eJobDataLineType.CELL.CCPDR:
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle == "" ? "1" : job.CellSpecial.TurnAngle;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.GlassThickness].Value = job.CellSpecial.GlassThickness;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.OperationID].Value = job.CellSpecial.OperationID;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.OwnerID].Value = job.CellSpecial.OwnerID;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.ProductOwner].Value = job.CellSpecial.ProductOwner;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.MaxRwkCount].Value = job.CellSpecial.MaxRwkCount;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.CurrentRwkCount].Value = job.CellSpecial.CurrentRwkCount;
                                                break;
                                          case eJobDataLineType.CELL.CCTAM:
                                          case eJobDataLineType.CELL.CCPTH:
                                          case eJobDataLineType.CELL.CCGAP:
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle == "" ? "1" : job.CellSpecial.TurnAngle;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.GlassThickness].Value = job.CellSpecial.GlassThickness;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.OperationID].Value = job.CellSpecial.OperationID;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.OwnerID].Value = job.CellSpecial.OwnerID;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.ProductOwner].Value = job.CellSpecial.ProductOwner;
                                                break;
                                          case eJobDataLineType.CELL.CCRWT:
                                          case eJobDataLineType.CELL.CCCRP:
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.DotRepairCount].Value = job.CellSpecial.DotRepairCount;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.LineRepairCount].Value = job.CellSpecial.LineRepairCount;
                                                //outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.DefectCode].Value = job.CellSpecial.DefectCode;
                                                break;
                                          case eJobDataLineType.CELL.CCPOL:
                                                //outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.MainDefectCode].Value = job.CellSpecial.DefectCode;//Main Defect Code//由EQ上報
                                                break;
                                          case eJobDataLineType.CELL.CCSOR:
                                                //outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.SortFlagNo].Value = job.CellSpecial.SortFlagNo;//Main Defect Code//由EQ上報
                                                break;
                                          case eJobDataLineType.CELL.CCRWK:
                                          case eJobDataLineType.CELL.CCQUP:
                                          //case eJobDataLineType.CELL.CCQPP:
                                          //case eJobDataLineType.CELL.CCPPK://Job Data No Use Line 
                                          case eJobDataLineType.CELL.CCCHN:
                                          case eJobDataLineType.CELL.CCQSR:
                                                break;

                                            //add by huangjiayin for t3 notch
                                        case eJobDataLineType.CELL.CCNLS:
                                        case eJobDataLineType.CELL.CCNRD:
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.PanelOXInformation].Value = ObjectManager.JobManager.M2P_CELL_PanelOX2Int(job.CellSpecial.PanelOXInformation);
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle == "" ? "1" : job.CellSpecial.TurnAngle;
                                                outputData.EventGroups[0].Events[int.Parse(job.JobSequenceNo) - 1].Items[eJOBDATA.PanelSize].Value = job.CellSpecial.PanelSize;
                                                break;

                                        #endregion
                                          default://Job Data No Use Line 
                                                return;
                                    }
                                    #endregion
                              }
                        }
                        SendPLCData(outputData);
                        string timeoutName = string.Format("{0}_{1}_{2}", port.Data.NODENO, port.Data.PORTNO, CstMappingDownloadTimeout);
                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                              _timerManager.TerminateTimer(timeoutName);
                        }
                        _timerManager.CreateTimer(timeoutName, false, ParameterManager["MAPDOWNLOAD_T1"].GetInteger(),
                            new System.Timers.ElapsedEventHandler(CassetteMappingDownloadTimeout), outputData.TrackKey + "_" + ((int)eCstControlCmd.MapDownload).ToString());

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CIM Mode =[{2}], \"CASSETTE MAP DOWNLOAD\" COMMAND, SET BIT (ON)",
                            eqp.Data.NODENO, outputData.TrackKey, eqp.File.CIMMode.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            private void CassetteMappingDownloadTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string[] obj = timer.State.ToString().Split('_');
                        string[] sArray = tmp.Split('_');
                        string trxName = string.Empty;

                        string timeName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], CstMappingDownloadTimeout);
                        if (_timerManager.IsAliveTimer(timeName))
                        {
                              _timerManager.TerminateTimer(timeName);
                        }

                        //Port port = ObjectManager.PortManager.GetDPPort(sArray[0], sArray[1]);
                        //if (port != null)
                        //{
                        //    if (port.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSECST)//shihyang 20151118 add 用DB port data 管控                            
                        //        trxName = string.Format("{0}_Port#{1}CassetteControlCommand", sArray[0], sArray[1]);
                        //    else
                        //        trxName = string.Format("{0}_DP#{1}CassetteControlCommand", sArray[0], sArray[1]);
                        //}
                        //else
                        trxName = string.Format("{0}_Port#{1}CassetteControlCommand", sArray[0], sArray[1]);//shihyang 目前T3 只有CST IO 無使用DP#{1}Cassette IO

                        Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                        outputdata.EventGroups[0].IsDisable = true;
                        outputdata.EventGroups[1].Events[0].IsDisable = true;
                        outputdata.EventGroups[1].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                        outputdata.TrackKey = obj[0];
                        SendPLCData(outputdata);
                        eCstControlCmd cmd = (eCstControlCmd)int.Parse(obj[1]);

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] [Port={2}] EQP REPLY,  CASSETTE CONTROL COMMAND=[{3}] REPLY TIMEOUT SET BIT=[OFF].",
                            sArray[0], obj[0], sArray[1], cmd.ToString()));


                        Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { obj[0], ServerName, 
                        string.Format("Cassette{0}Reply - EQUIPMENT=[{1}] PORT=[{2}] \"T1 TIMEOUT\"", 
                        cmd, sArray[0], sArray[1])});
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region [Job Data Request]
            private const string JobDataRequestReportReplyTimeOut = "JobDataRequestReportReplyTimeOut";
            public void JobDataRequestReportReply_CELL(Trx outputData, Job job, Line line, string eqpNo, string commandNo )
            {
                  try
                  {
                        #region CELL Special Job Data
                        outputData.EventGroups[0].Events[0].Items[eJOBDATA.INSPReservations].Value = job.INSPReservations;   //BIN
                        outputData.EventGroups[0].Events[0].Items[eJOBDATA.EQPReservations].Value = job.EQPReservations;   //BIN
                        outputData.EventGroups[0].Events[0].Items[eJOBDATA.LastGlassFlag].Value = job.LastGlassFlag;   ////INT
                        outputData.EventGroups[0].Events[0].Items[eJOBDATA.InspJudgedData].Value = job.InspJudgedData;   //BIN
                        outputData.EventGroups[0].Events[0].Items[eJOBDATA.TrackingData].Value = job.TrackingData;   //BIN
                        outputData.EventGroups[0].Events[0].Items[eJOBDATA.EQPFlag].Value = job.EQPFlag;   //BIN
                        outputData.EventGroups[0].Events[0].Items[eJOBDATA.ChipCount].Value = job.ChipCount.ToString();   //INT

                        outputData.EventGroups[0].Events[0].Items[eJOBDATA.ProductID].Value = job.CellSpecial.ProductID;
                        outputData.EventGroups[0].Events[0].Items[eJOBDATA.CassetteSettingCode].Value = job.CellSpecial.CassetteSettingCode;
                        switch (line.Data.JOBDATALINETYPE)
                        {
                              #region [T2 rule]
                              case eJobDataLineType.CELL.CBPIL:
                                    outputData.EventGroups[0].Events[0].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                                    outputData.EventGroups[0].Events[0].Items["TurnAngle"].Value = job.CellSpecial.TurnAngle;
                                    outputData.EventGroups[0].Events[0].Items["ProductID"].Value = job.CellSpecial.ProductID;
                                    outputData.EventGroups[0].Events[0].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                                    outputData.EventGroups[0].Events[0].Items["OwnerID"].Value = job.CellSpecial.OwnerID;
                                    outputData.EventGroups[0].Events[0].Items["PPOSlotNo"].Value = job.CellSpecial.PPOSlotNo;
                                    outputData.EventGroups[0].Events[0].Items["ReworkCount"].Value = job.CellSpecial.ReworkCount;
                                    break;
                              //case eJobDataLineType.CELL.CBODF:    
                              //    outputData.EventGroups[0].Events[0].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                              //    outputData.EventGroups[0].Events[0].Items["ArrayTTPEQVersion"].Value = job.CellSpecial.ArrayTTPEQVer;
                              //    outputData.EventGroups[0].Events[0].Items["ProductID"].Value = job.CellSpecial.ProductID;
                              //    outputData.EventGroups[0].Events[0].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                              //    outputData.EventGroups[0].Events[0].Items["OwnerID"].Value = job.CellSpecial.OwnerID;
                              //    outputData.EventGroups[0].Events[0].Items["CFCasetteSeqNo"].Value = job.CellSpecial.CFCassetteSeqNo;
                              //    outputData.EventGroups[0].Events[0].Items["CFJobSeqno"].Value = job.CellSpecial.CFJobSeqNo;
                              //    outputData.EventGroups[0].Events[0].Items["ODFBoxChamberOpenTime#01"].Value = job.CellSpecial.ODFBoxChamberOpenTime01;
                              //    outputData.EventGroups[0].Events[0].Items["ODFBoxChamberOpenTime#02"].Value = job.CellSpecial.ODFBoxChamberOpenTime02;
                              //    outputData.EventGroups[0].Events[0].Items["ODFBoxChamberOpenTime#03"].Value = job.CellSpecial.ODFBoxChamberOpenTime03;
                              //    outputData.EventGroups[0].Events[0].Items["NetworkNo"].Value = job.CellSpecial.NetworkNo;
                              //    outputData.EventGroups[0].Events[0].Items["RepairCount"].Value = job.CellSpecial.RepairCount;
                              //    outputData.EventGroups[0].Events[0].Items["TurnAngle"].Value = job.CellSpecial.TurnAngle;
                              //    outputData.EventGroups[0].Events[0].Items["UVMaskAlreadyUseCount"].Value = job.CellSpecial.UVMaskAlreadyUseCount;
                              //    break;
                              case eJobDataLineType.CELL.CBHVA:
                                    outputData.EventGroups[0].Events[0].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                                    outputData.EventGroups[0].Events[0].Items["ProductID"].Value = job.CellSpecial.ProductID;
                                    outputData.EventGroups[0].Events[0].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                                    outputData.EventGroups[0].Events[0].Items["OwnerID"].Value = job.CellSpecial.OwnerID;
                                    outputData.EventGroups[0].Events[0].Items["TurnAngle"].Value = job.CellSpecial.TurnAngle;
                                    outputData.EventGroups[0].Events[0].Items["ReturnModeTurnAngle"].Value = job.CellSpecial.ReturnModeTurnAngle;
                                    break;
                              case eJobDataLineType.CELL.CBCUT:
                                    outputData.EventGroups[0].Events[0].Items["NetworkNo"].Value = job.CellSpecial.NetworkNo;
                                    outputData.EventGroups[0].Events[0].Items["ProductID"].Value = job.CellSpecial.ProductID;
                                    outputData.EventGroups[0].Events[0].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                                    outputData.EventGroups[0].Events[0].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                                    outputData.EventGroups[0].Events[0].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                                    outputData.EventGroups[0].Events[0].Items["TurnAngle"].Value = job.CellSpecial.TurnAngle;
                                    outputData.EventGroups[0].Events[0].Items["CrossLineCassetteSettingCode"].Value = job.CellSpecial.CrossLineCassetteSettingCode;
                                    outputData.EventGroups[0].Events[0].Items["PanelSizeFlag"].Value = job.CellSpecial.PanelSizeFlag;
                                    outputData.EventGroups[0].Events[0].Items["MMGFlag"].Value = job.CellSpecial.MMGFlag;
                                    outputData.EventGroups[0].Events[0].Items["CrossLinePanelSize"].Value = job.CellSpecial.CrossLinePanelSize;
                                    outputData.EventGroups[0].Events[0].Items["CUTProductID"].Value = job.CellSpecial.CUTProductID;
                                    outputData.EventGroups[0].Events[0].Items["CUTCrossProductID"].Value = job.CellSpecial.CUTCrossProductID;
                                    outputData.EventGroups[0].Events[0].Items["CUTProductType"].Value = job.CellSpecial.CUTProductType;
                                    outputData.EventGroups[0].Events[0].Items["CUTCrossProductType"].Value = job.CellSpecial.CUTCrossProductType;
                                    outputData.EventGroups[0].Events[0].Items["POLProductType"].Value = job.CellSpecial.POLProductType;
                                    outputData.EventGroups[0].Events[0].Items["POLProductID"].Value = job.CellSpecial.POLProductID;
                                    outputData.EventGroups[0].Events[0].Items["CrossLinePPID"].Value = job.CellSpecial.CrossLinePPID;
                                    break;
                              case eJobDataLineType.CELL.CBPOL:
                                    outputData.EventGroups[0].Events[0].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                                    outputData.EventGroups[0].Events[0].Items["ProductID"].Value = job.CellSpecial.ProductID;
                                    outputData.EventGroups[0].Events[0].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                                    outputData.EventGroups[0].Events[0].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                                    outputData.EventGroups[0].Events[0].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                                    break;
                              case eJobDataLineType.CELL.CBDPK:
                                    outputData.EventGroups[0].Events[0].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                                    outputData.EventGroups[0].Events[0].Items["ProductID"].Value = job.CellSpecial.ProductID;
                                    outputData.EventGroups[0].Events[0].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                                    outputData.EventGroups[0].Events[0].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                                    outputData.EventGroups[0].Events[0].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                                    break;
                              case eJobDataLineType.CELL.CBPMT:
                                    outputData.EventGroups[0].Events[0].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                                    outputData.EventGroups[0].Events[0].Items["ProductID"].Value = job.CellSpecial.ProductID;
                                    outputData.EventGroups[0].Events[0].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                                    outputData.EventGroups[0].Events[0].Items["OwnerID"].Value = job.CellSpecial.OwnerID;
                                    outputData.EventGroups[0].Events[0].Items["RunMode"].Value = job.CellSpecial.RunMode;
                                    outputData.EventGroups[0].Events[0].Items["NodeStack"].Value = job.CellSpecial.NodeStack;
                                    outputData.EventGroups[0].Events[0].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                                    break;
                              case eJobDataLineType.CELL.CBGAP:
                                    outputData.EventGroups[0].Events[0].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                                    outputData.EventGroups[0].Events[0].Items["ProductID"].Value = job.CellSpecial.ProductID;
                                    outputData.EventGroups[0].Events[0].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                                    outputData.EventGroups[0].Events[0].Items["OwnerID"].Value = job.CellSpecial.OwnerID;
                                    outputData.EventGroups[0].Events[0].Items["NodeStack"].Value = job.CellSpecial.NodeStack;
                                    outputData.EventGroups[0].Events[0].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                                    break;
                              case eJobDataLineType.CELL.CBPIS:
                                    outputData.EventGroups[0].Events[0].Items["NetworkNo"].Value = job.CellSpecial.NetworkNo;
                                    outputData.EventGroups[0].Events[0].Items["ProductID"].Value = job.CellSpecial.ProductID;
                                    outputData.EventGroups[0].Events[0].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                                    outputData.EventGroups[0].Events[0].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                                    outputData.EventGroups[0].Events[0].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                                    break;
                              case eJobDataLineType.CELL.CBPRM:
                                    outputData.EventGroups[0].Events[0].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                                    outputData.EventGroups[0].Events[0].Items["ProductID"].Value = job.CellSpecial.ProductID;
                                    outputData.EventGroups[0].Events[0].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                                    outputData.EventGroups[0].Events[0].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                                    outputData.EventGroups[0].Events[0].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                                    outputData.EventGroups[0].Events[0].Items["RunMode"].Value = job.CellSpecial.RunMode;
                                    break;
                              case eJobDataLineType.CELL.CBGMO:
                                    outputData.EventGroups[0].Events[0].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                                    outputData.EventGroups[0].Events[0].Items["ProductID"].Value = job.CellSpecial.ProductID;
                                    outputData.EventGroups[0].Events[0].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                                    outputData.EventGroups[0].Events[0].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                                    outputData.EventGroups[0].Events[0].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                                    break;
                              case eJobDataLineType.CELL.CBLOI:
                                    outputData.EventGroups[0].Events[0].Items["NetworkNo"].Value = job.CellSpecial.NetworkNo;
                                    outputData.EventGroups[0].Events[0].Items["ProductID"].Value = job.CellSpecial.ProductID;
                                    outputData.EventGroups[0].Events[0].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                                    outputData.EventGroups[0].Events[0].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                                    outputData.EventGroups[0].Events[0].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                                    outputData.EventGroups[0].Events[0].Items["RepairResult"].Value = job.CellSpecial.RepairResult;
                                    outputData.EventGroups[0].Events[0].Items["RunMode"].Value = job.CellSpecial.RunMode;
                                    //outputData.EventGroups[0].Events[0].Items["VirtualPortEnableMode"].Value = job.CellSpecial.VirtualPortEnableMode;  Jun Modify 20150107 For New IO
                                    break;
                              case eJobDataLineType.CELL.CBNRP:
                                    outputData.EventGroups[0].Events[0].Items["NetworkNo"].Value = job.CellSpecial.NetworkNo;
                                    outputData.EventGroups[0].Events[0].Items["ProductID"].Value = job.CellSpecial.ProductID;
                                    outputData.EventGroups[0].Events[0].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                                    outputData.EventGroups[0].Events[0].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                                    outputData.EventGroups[0].Events[0].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                                    outputData.EventGroups[0].Events[0].Items["RepairResult"].Value = job.CellSpecial.RepairResult;
                                    outputData.EventGroups[0].Events[0].Items["RepairCount"].Value = job.CellSpecial.RepairCount;
                                    outputData.EventGroups[0].Events[0].Items["RunMode"].Value = job.CellSpecial.RunMode;
                                    break;
                              case eJobDataLineType.CELL.CBOLS:
                                    outputData.EventGroups[0].Events[0].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                                    outputData.EventGroups[0].Events[0].Items["ProductID"].Value = job.CellSpecial.ProductID;
                                    outputData.EventGroups[0].Events[0].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                                    outputData.EventGroups[0].Events[0].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                                    outputData.EventGroups[0].Events[0].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                                    break;
                              case eJobDataLineType.CELL.CBSOR:
                                    outputData.EventGroups[0].Events[0].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                                    outputData.EventGroups[0].Events[0].Items["ProductID"].Value = job.CellSpecial.ProductID;
                                    outputData.EventGroups[0].Events[0].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                                    outputData.EventGroups[0].Events[0].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                                    outputData.EventGroups[0].Events[0].Items["RunMode"].Value = job.CellSpecial.RunMode;
                                    outputData.EventGroups[0].Events[0].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                                    outputData.EventGroups[0].Events[0].Items["NodeStack"].Value = job.CellSpecial.NodeStack;
                                    break;
                              case eJobDataLineType.CELL.CBDPS:
                                    outputData.EventGroups[0].Events[0].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                                    outputData.EventGroups[0].Events[0].Items["ProductID"].Value = job.CellSpecial.ProductID;
                                    outputData.EventGroups[0].Events[0].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                                    outputData.EventGroups[0].Events[0].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                                    outputData.EventGroups[0].Events[0].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                                    break;
                              case eJobDataLineType.CELL.CBATS:
                                    outputData.EventGroups[0].Events[0].Items["NetworkNo"].Value = job.CellSpecial.NetworkNo;
                                    outputData.EventGroups[0].Events[0].Items["ProductID"].Value = job.CellSpecial.ProductID;
                                    outputData.EventGroups[0].Events[0].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                                    outputData.EventGroups[0].Events[0].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                                    outputData.EventGroups[0].Events[0].Items["PanelSize"].Value = job.CellSpecial.PanelSize;
                                    outputData.EventGroups[0].Events[0].Items["NodeStack"].Value = job.CellSpecial.NodeStack;
                                    break;
                              case eJobDataLineType.CELL.CBDPI:
                                    outputData.EventGroups[0].Events[0].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                                    outputData.EventGroups[0].Events[0].Items["ProductID"].Value = job.CellSpecial.ProductID;
                                    outputData.EventGroups[0].Events[0].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                                    outputData.EventGroups[0].Events[0].Items["AbnormalCode"].Value = job.CellSpecial.AbnormalCode;
                                    break;
                              case eJobDataLineType.CELL.CBUVA:
                                    outputData.EventGroups[0].Events[0].Items["ControlMode"].Value = ((int)job.CellSpecial.ControlMode).ToString();
                                    outputData.EventGroups[0].Events[0].Items["ProductID"].Value = job.CellSpecial.ProductID;
                                    outputData.EventGroups[0].Events[0].Items["CassetteSettingCode"].Value = job.CellSpecial.CassetteSettingCode;
                                    outputData.EventGroups[0].Events[0].Items["OwnerID"].Value = job.CellSpecial.OwnerID;
                                    break;
                              #endregion
                              //T3 cs.chou 20150818 Add //shihyang  20150911 Edit
                              #region [T3 rule]
                              case eJobDataLineType.CELL.CCPIL:
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.BlockOXInformation].Value = ObjectManager.JobManager.M2P_CELL_BlockOX2Bin(job.CellSpecial.BlockOXInformation);
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.GlassThickness].Value = job.CellSpecial.GlassThickness;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.OperationID].Value = job.CellSpecial.OperationID;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.OwnerID].Value = job.CellSpecial.OwnerID;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.ProductOwner].Value = job.CellSpecial.ProductOwner;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.PILiquidType].Value = job.CellSpecial.PILiquidType;
                                    break;
                              case eJobDataLineType.CELL.CCODF:
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.BlockOXInformation].Value = ObjectManager.JobManager.M2P_CELL_BlockOX2Bin(job.CellSpecial.BlockOXInformation);
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.GlassThickness].Value = job.CellSpecial.GlassThickness;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.OperationID].Value = job.CellSpecial.OperationID;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.OwnerID].Value = job.CellSpecial.OwnerID;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.ProductOwner].Value = job.CellSpecial.ProductOwner;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.AssembleSeqNo].Value = job.CellSpecial.AssembleSeqNo;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.UVMaskAlreadyUseCount].Value = job.CellSpecial.UVMaskAlreadyUseCount;
                                    break;
                              case eJobDataLineType.CELL.CCPCS:
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.BlockOXInformation].Value = ObjectManager.JobManager.M2P_CELL_BlockOX2Bin(job.CellSpecial.BlockOXInformation);
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.BlockSize].Value = job.CellSpecial.BlockSize;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSCassetteSettingCodeList].Value = job.CellSpecial.PCSCassetteSettingCodeList;
                                //20170725 huangjiayin add: pcsblocksizelist
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSBlockSizeList].Value = job.CellSpecial.PCSBlockSizeList;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.BlockSize1].Value = job.CellSpecial.BlockSize1;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.BlockSize2].Value = job.CellSpecial.BlockSize2;
                                    //outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSProductID].Value = job.CellSpecial.CUTProductID;
                                   // outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSProductType].Value = job.CellSpecial.CUTProductType;
                                    //outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSProductID2].Value = job.CellSpecial.CUTProductID2;
                                    //outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSProductType2].Value = job.CellSpecial.CUTProductType2;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSCassetteSettingCode].Value = job.CellSpecial.CUTCassetteSettingCode;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.PCSCassetteSettingCode2].Value = job.CellSpecial.CUTCassetteSettingCode2;
                                    break;
                              case eJobDataLineType.CELL.CCCUT:
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.PanelOXInformation].Value = ObjectManager.JobManager.M2P_CELL_PanelOX2Int(job.CellSpecial.PanelOXInformation);
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.PanelSize].Value = job.CellSpecial.PanelSize;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.CUTProductID].Value = job.CellSpecial.CUTProductID;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.CUTProductType].Value = job.CellSpecial.CUTProductType;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.DefectCode].Value = job.CellSpecial.DefectCode;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.RejudgeCount].Value = job.CellSpecial.RejudgeCount;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.VendorName].Value = job.CellSpecial.VendorName;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.BURCheckCount].Value = job.CellSpecial.BURCheckCount;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.CUTCassetteSettingCode].Value = job.CellSpecial.CUTCassetteSettingCode;
                                    break;
                              case eJobDataLineType.CELL.CCPCK:
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.PanelGroup].Value = job.CellSpecial.PanelGroup;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.OQCBank].Value = job.CellSpecial.OQCBank;
                                    break;
                              case eJobDataLineType.CELL.CCPDR:
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.GlassThickness].Value = job.CellSpecial.GlassThickness;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.OperationID].Value = job.CellSpecial.OperationID;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.OwnerID].Value = job.CellSpecial.OwnerID;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.ProductOwner].Value = job.CellSpecial.ProductOwner;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.MaxRwkCount].Value = job.CellSpecial.MaxRwkCount;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.CurrentRwkCount].Value = job.CellSpecial.CurrentRwkCount;
                                    break;
                              case eJobDataLineType.CELL.CCTAM:
                              case eJobDataLineType.CELL.CCPTH:
                              case eJobDataLineType.CELL.CCGAP:
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.GlassThickness].Value = job.CellSpecial.GlassThickness;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.OperationID].Value = job.CellSpecial.OperationID;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.OwnerID].Value = job.CellSpecial.OwnerID;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.ProductOwner].Value = job.CellSpecial.ProductOwner;
                                    break;
                              case eJobDataLineType.CELL.CCRWT:
                              case eJobDataLineType.CELL.CCCRP:
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.DotRepairCount].Value = job.CellSpecial.DotRepairCount;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.LineRepairCount].Value = job.CellSpecial.LineRepairCount;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.DefectCode].Value = job.CellSpecial.DefectCode;
                                    break;
                              case eJobDataLineType.CELL.CCPOL:
                                outputData.EventGroups[0].Events[0].Items[eJOBDATA.MainDefectCode].Value = job.CellSpecial.DefectCode;//Main Defect Code
                                break;
                              case eJobDataLineType.CELL.CCSOR:
                                //outputData.EventGroups[0].Events[0].Items[eJOBDATA.SortFlagNo].Value = job.CellSpecial.SortFlagNo;//Main Defect Code
                                break;
                              case eJobDataLineType.CELL.CCRWK:
                              case eJobDataLineType.CELL.CCQUP:
                              //case eJobDataLineType.CELL.CCQPP:
                              //case eJobDataLineType.CELL.CCPPK:
                              case eJobDataLineType.CELL.CCCHN:
                              case eJobDataLineType.CELL.CCQSR:
                                    break;

                                //add by huangjiayin for t3 notch
                            case eJobDataLineType.CELL.CCNLS:
                            case eJobDataLineType.CELL.CCNRD:
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.PanelOXInformation].Value = ObjectManager.JobManager.M2P_CELL_PanelOX2Int(job.CellSpecial.PanelOXInformation);
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.TurnAngle].Value = job.CellSpecial.TurnAngle == "" ? "1" : job.CellSpecial.TurnAngle;
                                    outputData.EventGroups[0].Events[0].Items[eJOBDATA.PanelSize].Value = job.CellSpecial.PanelSize;
                                    break;
                              #endregion
                              default:
                                    return;
                        }
                        #endregion

                        //outputData.EventGroups[0].Events[0].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger(); //Delay Turn On Bit
                        SendPLCData(outputData);

                        //sy modify 20160911
                        #region [Timer]
                        string TimeOutName;
                        if (string.IsNullOrEmpty(commandNo))
                        {
                            TimeOutName = string.Format(eqpNo + "_" + JobDataRequestReportReplyTimeOut);
                        }
                        else
                        {
                            TimeOutName = string.Format(eqpNo + "_" + JobDataRequestReportReplyTimeOut) + "#" + commandNo;
                        }
                        if (_timerManager.IsAliveTimer(TimeOutName))
                        {
                            _timerManager.TerminateTimer(TimeOutName);
                        }
                            _timerManager.CreateTimer(TimeOutName, false, ParameterManager["T2"].GetInteger(),
                                new System.Timers.ElapsedEventHandler(JobDataRequestReportTimeoutForEQP), outputData.TrackKey);
                        #endregion

                            Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC REPLY, JOB DATA REQUEST REPORT SET BIT =[{2}], RETURN CODE=[{3}].",
                            eqpNo, outputData.TrackKey, eBitResult.ON, eReturnCode1.OK));

                      //add Scrap RuleCommand by zhuxingxing 20160818
                        if (line.Data.LINETYPE == eLineType.CELL.CCCUT_5 || !job.CellSpecial.DisCardJudges.Equals(""))
                        {
                               Invoke(eServiceName.CELLSpecialService, "ScrapRuleCommand", new object[] { "L4", eBitResult.ON, outputData.TrackKey, job.CassetteSequenceNo, job.CellSpecial.DisCardJudges, commandNo });

                                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC REPLY, JOB DATA ScrapRuleCommand SET BIT =[{2}], RETURN CODE=[{3}].",
                                eqpNo, outputData.TrackKey, eBitResult.ON, eReturnCode1.OK));
                            
                        }
                      //end 
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            public void JobDataRequestReportTimeoutForEQP(object subject, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subject as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');
                        string commandNo = string.Empty;
                        if (sArray[1].Split(new char[] { '#' }).Length == 2)
                            commandNo = sArray[1].Split(new char[] { '#' })[1];

                        string timeoutName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], JobDataRequestReportReplyTimeOut);
                        if (string.IsNullOrEmpty(commandNo))
                        {
                            timeoutName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], JobDataRequestReportReplyTimeOut);
                        }
                        else
                        {
                            timeoutName = string.Format("{0}_{1}_{2}", sArray[0], sArray[1], JobDataRequestReportReplyTimeOut)+ "#" + commandNo;
                        }

                        if (_timerManager.IsAliveTimer(timeoutName))
                        {
                              _timerManager.TerminateTimer(timeoutName);
                        }

                        string trxName = string.Format("{0}_JobDataRequestReportReply", sArray[0]);
                        if(!string.IsNullOrEmpty(commandNo))
                        {
                            trxName = string.Format("{0}_JobDataRequestReportReply", sArray[0]) + "#" +commandNo;
                        }

                        Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(trxName) as Trx;
                        outputdata.EventGroups[0].Events[0].IsDisable = true;
                        outputdata.EventGroups[0].Events[1].IsDisable = true;
                        outputdata.EventGroups[0].Events[2].Items[0].Value = ((int)eBitResult.OFF).ToString();
                        outputdata.TrackKey = sArray[1];
                        SendPLCData(outputdata);

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] EQP REPLY, JOB DATA REQUEST REPORT TIMEOUT SET BIT=[OFF].",
                            sArray[0], sArray[1]));

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region [Unload Store QTime Report]
            private const string UnloadStoreQTimeTimeout = "UnloadStoreQTimeTimeout";
            public void UnloadStoreQTimeReport(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;
                        #region[Get EQP & LINE]
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                        if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                        #endregion
                        #region [PLCAgent Data Bit]
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion
                        #region[If Bit Off->Return]
                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                              UnloadStoreQTimeReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }
                        #endregion
                        #region [PLCAgent Data Word]
                        string unloadStoreQTimeChangeInfo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string portMode1 = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string qtime1 = inputData.EventGroups[0].Events[0].Items[2].Value;
                        string portMode2 = inputData.EventGroups[0].Events[0].Items[3].Value;
                        string qtime2 = inputData.EventGroups[0].Events[0].Items[4].Value;
                        string portMode3 = inputData.EventGroups[0].Events[0].Items[5].Value;
                        string qtime3 = inputData.EventGroups[0].Events[0].Items[6].Value;
                        string portMode4 = inputData.EventGroups[0].Events[0].Items[7].Value;
                        string qtime4 = inputData.EventGroups[0].Events[0].Items[8].Value;
                        string portMode5 = inputData.EventGroups[0].Events[0].Items[9].Value;
                        string qtime5 = inputData.EventGroups[0].Events[0].Items[10].Value;
                        string portMode6 = inputData.EventGroups[0].Events[0].Items[11].Value;
                        string qtime6 = inputData.EventGroups[0].Events[0].Items[12].Value;
                        string portMode7 = inputData.EventGroups[0].Events[0].Items[13].Value;
                        string qtime7 = inputData.EventGroups[0].Events[0].Items[14].Value;
                        //T3 T2 Rule保留
                        if (line.Data.LINETYPE != eLineType.CELL.CBSOR_1 || line.Data.LINETYPE != eLineType.CELL.CBSOR_2 || line.Data.LINETYPE != eLineType.CELL.CCSOR)
                        {
                              string operatorID = inputData.EventGroups[0].Events[0].Items[15].Value;
                        }
                        else
                        {
                              string portMode8 = inputData.EventGroups[0].Events[0].Items[15].Value;
                              string qtime8 = inputData.EventGroups[0].Events[0].Items[16].Value;
                              string operatorID = inputData.EventGroups[0].Events[0].Items[17].Value;
                        }
                        #endregion
                        #region[Log]
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE=[{2}], NODE=[{3}],UNLOAD STORE QTIME REPORT UNLOADGLASSQTIMECHANGEINFO=[{4}],PORTMODE1 =[{5}]" +
                                ",QTIME1 =[{6}],PORTMODE2 =[{7}],QTIME2 =[{8}],PORTMODE3 =[{9}],QTIME3 =[{10}],PORTMODE4 =[{11}],QTIME4 =[{12}],PORTMODE5 =[{13}],QTIME5 =[{14}],PORTMODE6 =[{15}]" +
                                        ",QTIME6 =[{16}],PORTMODE7 =[{17}],QTIME7 =[{18}]",
                                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, unloadStoreQTimeChangeInfo, portMode1,
                                    qtime1, portMode2, qtime2, portMode3, qtime3, portMode4, qtime4, portMode5, qtime5, portMode6, qtime6, portMode7, qtime7));
                        #endregion
                        UnloadStoreQTimeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                        // 避免中間發生Exception BCS不把BIT ON起來
                        if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                              UnloadStoreQTimeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                  }
            }
            private void UnloadStoreQTimeReportReply(string eqpNo, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "UnloadStoreQTimeReportReply") as Trx;

                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + UnloadStoreQTimeTimeout))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + UnloadStoreQTimeTimeout);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + UnloadStoreQTimeTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(UnloadStoreQTimeReportReplyTimeout), trackKey);
                        }
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,SET BIT=[{2}].",
                            eqpNo, trackKey, value.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void UnloadStoreQTimeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] UNLOAD STORE QTIME REPORT REPLY TIMEOUT ,SET BIT=[OFF].", sArray[0], trackKey));

                        UnloadStoreQTimeReportReply(sArray[0], eBitResult.OFF, trackKey);

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region [Unload Glass QTime Report]
            private const string UnloadGlassQTimeTimeout = "UnloadGlassQTimeTimeout";
            public void UnloadGlassQTimeReport(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;
                        #region[Get EQP & LINE]
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                        if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                        #endregion
                        #region [PLCAgent Data Bit]
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion
                        #region[If Bit Off->Return]
                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                              UnloadGlassQTimeReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }
                        #endregion
                        #region [PLCAgent Data Word]
                        string unloadGlassQTimeChangeInfo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string portMode1 = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string qtime1 = inputData.EventGroups[0].Events[0].Items[2].Value;
                        string portMode2 = inputData.EventGroups[0].Events[0].Items[3].Value;
                        string qtime2 = inputData.EventGroups[0].Events[0].Items[4].Value;
                        string portMode3 = inputData.EventGroups[0].Events[0].Items[5].Value;
                        string qtime3 = inputData.EventGroups[0].Events[0].Items[6].Value;
                        string portMode4 = inputData.EventGroups[0].Events[0].Items[7].Value;
                        string qtime4 = inputData.EventGroups[0].Events[0].Items[8].Value;
                        string portMode5 = inputData.EventGroups[0].Events[0].Items[9].Value;
                        string qtime5 = inputData.EventGroups[0].Events[0].Items[10].Value;
                        string portMode6 = inputData.EventGroups[0].Events[0].Items[11].Value;
                        string qtime6 = inputData.EventGroups[0].Events[0].Items[12].Value;
                        string portMode7 = inputData.EventGroups[0].Events[0].Items[13].Value;
                        string qtime7 = inputData.EventGroups[0].Events[0].Items[14].Value;
                        //T3 保留T2規則
                        if (line.Data.LINETYPE != eLineType.CELL.CBSOR_1 || line.Data.LINETYPE != eLineType.CELL.CBSOR_2 || line.Data.LINETYPE != eLineType.CELL.CCSOR)
                        {
                              string operatorID = inputData.EventGroups[0].Events[0].Items[15].Value;
                        }
                        else
                        {
                              string portMode8 = inputData.EventGroups[0].Events[0].Items[15].Value;
                              string qtime8 = inputData.EventGroups[0].Events[0].Items[16].Value;
                              string operatorID = inputData.EventGroups[0].Events[0].Items[17].Value;
                        }
                        #endregion
                        #region[Log]
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE=[{2}], NODE=[{3}],UNLOAD GLASS QTIME REPORT UNLOADGLASSQTIMECHANGEINFO =[{4}],PORTMODE1 =[{5}]" +
                                ",QTIME1 =[{6}],PORTMODE2 =[{7}],QTIME2 =[{8}],PORTMODE3 =[{9}],QTIME3 =[{10}],PORTMODE4 =[{11}],QTIME4 =[{12}],PORTMODE5 =[{13}],QTIME5 =[{14}],PORTMODE6 =[{15}]," +
                                    "QTIME6 =[{16}],PORTMODE7 =[{17}],QTIME7 =[{18}]",
                                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, unloadGlassQTimeChangeInfo, portMode1,
                                        qtime1, portMode2, qtime2, portMode3, qtime3, portMode4, qtime4, portMode5, qtime5, portMode6, qtime6, portMode7, qtime7));
                        #endregion
                        UnloadGlassQTimeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                        // 避免中間發生Exception BCS不把BIT ON起來
                        if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                              UnloadGlassQTimeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                  }
            }
            private void UnloadGlassQTimeReportReply(string eqpNo, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "UnloadGlassQTimeReportReply") as Trx;

                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + UnloadGlassQTimeTimeout))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + UnloadGlassQTimeTimeout);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + UnloadGlassQTimeTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(UnloadGlassQTimeReportReplyTimeout), trackKey);
                        }
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,SET BIT=[{2}].",
                            eqpNo, trackKey, value.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void UnloadGlassQTimeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] UNLOAD GLASS QTIME REPORT REPLY TIMEOUT ,SET BIT=[OFF]", sArray[0], trackKey));

                        UnloadGlassQTimeReportReply(sArray[0], eBitResult.OFF, trackKey);

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region [Loader Operation Mode Change Report]
            private const string LoaderOperationModeChangeTimeout = "LoaderOperationModeChangeTimeout";
            public void LoaderOperationModeChangeReport(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;
                        #region[Get EQP & LINE]
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                        if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                        #endregion
                        #region [PLCAgent Data Bit]
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion
                        #region[If Bit Off->Return]
                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                              LoaderOperationModeChangeReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }
                        #endregion
                        #region [PLCAgent Data Word]
                        string mode = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string tftCount = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string cfCount = inputData.EventGroups[0].Events[0].Items[2].Value;
                        string operatorID = inputData.EventGroups[0].Events[0].Items[3].Value;

                        eCELLLDOperatorMode eCELLOperatorMode;
                        Enum.TryParse<eCELLLDOperatorMode>(mode, out eCELLOperatorMode);
                        #endregion
                        #region[Log]
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE=[{2}], NODE=[{3}], LOADEROPERATIONMODE=[{4}], TFTCOUNT=[{5}], CFCOUNT=[{6}], OPERATORID=[{7}]",
                                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, eCELLOperatorMode.ToString(), tftCount, cfCount, operatorID));
                        #endregion
                        LoaderOperationModeChangeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                        // 避免中間發生Exception BCS不把BIT ON起來
                        if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                              LoaderOperationModeChangeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                  }
            }
            private void LoaderOperationModeChangeReportReply(string eqpNo, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "LoaderOperationModeChangeReportReply") as Trx;

                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + LoaderOperationModeChangeTimeout))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + LoaderOperationModeChangeTimeout);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + LoaderOperationModeChangeTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(LoaderOperationModeChangeReportReplyTimeOut), trackKey);
                        }
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,SET BIT=[{2}].",
                            eqpNo, trackKey, value.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void LoaderOperationModeChangeReportReplyTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] LOADER OPERATION MODE CHANGE REPORT REPLY TIMEOUT ,SET BIT=[OFF].", sArray[0], trackKey));

                        LoaderOperationModeChangeReportReply(sArray[0], eBitResult.OFF, trackKey);

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region [ENG  EVENT]
            public void ENGMode(Trx inputData)
            {
                  try
                  {
                        #region[Get EQP]
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                        if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));
                        #endregion

                        eEnableDisable eqpENGMode = (eEnableDisable)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                        if (eqp.File.EQPENGMode != eqpENGMode)
                        {
                              lock (eqp)
                              {
                                    eqp.File.EQPENGMode = eqpENGMode;
                              }
                        }

                        if (inputData.IsInitTrigger) return;

                        #region[Log]
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [EQP -> EQP][{1}] ENG Mode =[{2}]", eqp.Data.NODENO, inputData.TrackKey, eqpENGMode.ToString()));
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            #region [ENG Mode Change Request Report]
            private const string ENGModeChangeRequestTimeOut = "ENGModeChangeRequestTimeOut";
            public void ENGModeChangeRequestReport(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;
                        #region[Get EQP & LINE]
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                        if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                        #endregion
                        #region [PLCAgent Data Bit]
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion
                        #region[If Bit Off->Return]
                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                              ENGModeChangeRequestReply(inputData.Metadata.NodeNo, eResult.Unknown, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }
                        #endregion
                        #region [PLCAgent Data Word]
                        string settingCount = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string operatorMode = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string operatorID = inputData.EventGroups[0].Events[0].Items[2].Value;

                        eOperatorMode eOperatorMode;
                        Enum.TryParse<eOperatorMode>(operatorMode, out eOperatorMode);
                        #endregion
                        #region[Log]
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE=[{2}], NODE=[{3}], SETTING COUNT=[{4}], OPERATOR_Mode=[{5}], OPERATOR_ID=[{6}].",
                                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, settingCount, eOperatorMode.ToString(), operatorID));
                        #endregion
                        eResult result = eResult.OK;

                        List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPsByLine(eqp.Data.LINEID);
                        if (eqps != null)
                        {
                              foreach (Equipment eq in eqps)
                              {
                                    if (eq.Data.NODENO == eqp.Data.NODENO) continue;

                                    if (eq.File.EQPENGMode == eEnableDisable.Enable || eq.File.UnitENGMode.Contains("1"))
                                    {
                                          result = eResult.NG;
                                          Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                              string.Format("[EQUIPMENT={0}] IS ENG MODE, [EQUIPMENT={1}] CAN NOT CHANGE TO ENG MODE.",
                                              eq.Data.NODENO, eqp.Data.NODENO));
                                    }
                              }
                        }

                        IList<Job> jobs = ObjectManager.JobManager.GetJobs();
                        if (jobs != null)
                        {
                              foreach (Job job in jobs)
                              {
                                  if (job.RemoveFlag == false) //Remove的Job不列入判斷 by tom.su 20160308
                                  {
                                      IDictionary<string, string> subItem = ObjectManager.SubJobDataManager.Decode(job.EQPFlag, "EQPFlag");

                                      if (subItem != null)
                                      {
                                          if (subItem.ContainsKey("ENGModeFlag"))
                                          {
                                              if (subItem["ENGModeFlag"] == ((int)eBitResult.ON).ToString())
                                              {
                                                  result = eResult.NG;
                                                  Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                      string.Format("CST_SEQNO=[{0}], JOB_SEQNO=[{1}] HAVE ENGFLAG, [EQUIPMENT={2}] CAN NOT CHANGE TO ENG MODE.",
                                                      job.CassetteSequenceNo, job.JobSequenceNo, eqp.Data.NODENO));
                                              }
                                          }
                                      }
                                  }
                              }
                        }

                        ENGModeChangeRequestReply(inputData.Metadata.NodeNo, result, eBitResult.ON, inputData.TrackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                        // 避免中間發生Exception BCS不把BIT ON起來
                        if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                              ENGModeChangeRequestReply(inputData.Name.Split('_')[0], eResult.NG, eBitResult.ON, inputData.TrackKey);
                  }
            }
            private void ENGModeChangeRequestReply(string eqpNo, eResult result, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "ENGModeChangeRequestReply") as Trx;

                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)result).ToString();
                        outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                        //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                        outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                        outputdata.TrackKey = trackKey;

                        #region [If Bit Off->Return] tom.su Add Bit off不更改W區資料 20160308
                        if (value == eBitResult.OFF)
                        {
                            outputdata.EventGroups[0].Events[0].IsDisable = true;
                            SendPLCData(outputdata);
                            if (_timerManager.IsAliveTimer(eqpNo + "_" + ENGModeChangeRequestTimeOut))
                                _timerManager.TerminateTimer(eqpNo + "_" + ENGModeChangeRequestTimeOut);   
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] BC Reply, ENGModeChangeRequestReply Set Bit =[OFF].", eqpNo, trackKey));
                            return;
                        }
                        #endregion
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + ENGModeChangeRequestTimeOut))                        
                              _timerManager.TerminateTimer(eqpNo + "_" + ENGModeChangeRequestTimeOut);    
                        if (value.Equals(eBitResult.ON))                        
                              _timerManager.CreateTimer(eqpNo + "_" + ENGModeChangeRequestTimeOut, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(ENGModeChangeRequestReplyTimeOut), trackKey);
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,SET BIT=[{2}], BC_RETURNCODE=[{3}].",
                            eqpNo, trackKey, value.ToString(), result.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void ENGModeChangeRequestReplyTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ENG MODE CHANGE REQUEST REPLY TIMEOUT ,SET BIT=[OFF].", sArray[0], trackKey));

                        ENGModeChangeRequestReply(sArray[0], eResult.Unknown, eBitResult.OFF, trackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region [ENG Mode Change Report]
            private const string ENGModeChangeReportTimeOut = "ENGModeChangeReportTimeOut";
            public void ENGModeChangeReport(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;
                        #region[Get EQP & LINE]
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                        if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                        #endregion
                        #region [PLCAgent Data Bit]
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion
                        #region[If Bit Off->Return]
                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                              ENGModeChangeReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }
                        #endregion
                        #region [PLCAgent Data Word]
                        string engModeInfo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string engModeResult = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string settingCount = inputData.EventGroups[0].Events[0].Items[2].Value;
                        string remainCount = inputData.EventGroups[0].Events[0].Items[3].Value;
                        string operatorMode = inputData.EventGroups[0].Events[0].Items[4].Value;
                        string operatorID = inputData.EventGroups[0].Events[0].Items[5].Value;

                        eENGModeChangeInfo eENGModeChangeInfo;
                        Enum.TryParse<eENGModeChangeInfo>(engModeInfo, out eENGModeChangeInfo);

                        eENGModeChangeResult eENGModeChangeResult;
                        Enum.TryParse<eENGModeChangeResult>(engModeResult, out eENGModeChangeResult);

                        eOperatorMode eOperatorMode;
                        Enum.TryParse<eOperatorMode>(operatorMode, out eOperatorMode);
                        #endregion
                        #region[Log]
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE=[{2}], NODE=[{3}], ENG CHANGE INFO=[{4}], " +
                                            "EGN CHANGE RESULT=[{5}], SETTING_COUNT=[{6}], REMAIN_COUNT=[{7}], OPERATOR_Mode=[{8}], OPERATOR_ID=[{9}].",
                                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, eENGModeChangeInfo.ToString(),
                                    eENGModeChangeResult.ToString(), settingCount, remainCount, eOperatorMode.ToString(), operatorID));
                        #endregion
                        ENGModeChangeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                        // 避免中間發生Exception BCS不把BIT ON起來
                        if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                              ENGModeChangeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                  }
            }
            private void ENGModeChangeReportReply(string eqpNo, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "ENGModeChangeReportReply") as Trx;

                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + ENGModeChangeReportTimeOut))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + ENGModeChangeReportTimeOut);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + ENGModeChangeReportTimeOut, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(ENGModeChangeReportReplyTimeOut), trackKey);
                        }
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,SET BIT=[{2}].",
                            eqpNo, trackKey, value.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void ENGModeChangeReportReplyTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ENG MODE CHANGE REPORT REPLY TIMEOUT ,SET BIT=[OFF].", sArray[0], trackKey));

                        ENGModeChangeReportReply(sArray[0], eBitResult.OFF, trackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion
            #endregion

            #region [CQLT EVENT]
            public void CQLTMode(Trx inputData)
            {
                  try
                  {
                        #region[Get EQP & LINE]
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                        if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));
                        #endregion

                        eBitResult cqltMode = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                        lock (eqp)
                        {
                              eqp.File.CQLTMode = (int)cqltMode == 1 ? eEnableDisable.Enable : eEnableDisable.Disable;
                        }

                        if (inputData.IsInitTrigger) return;

                        #region[Log]
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CQLT MODE STATUS=[{2}].", inputData.Metadata.NodeNo, inputData.TrackKey, cqltMode.ToString()));
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            #region [CQLT Mode Report]
            private const string CQLTModeReportTimeout = "CQLTModeReportTimeout";
            public void CQLTModeReport(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;
                        #region[Get EQP & LINE]
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                        if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                        #endregion
                        #region[PLCAgent Data Bit]
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion
                        #region[If Bit Off->Return]
                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                              CQLTModeReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }
                        #endregion
                        #region[PLCAgent Data Word]
                        string cqltModeInfo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string settingCount = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string operatorID = inputData.EventGroups[0].Events[0].Items[2].Value;
                        #endregion
                        eCQLTModeChangeInfo eCQLTModeChangeInfo;
                        Enum.TryParse<eCQLTModeChangeInfo>(cqltModeInfo, out eCQLTModeChangeInfo);
                        #region [Log]
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE=[{2}], NODE=[{3}], CQLT MODE CHANGE INFO=[{4}], SETTING COUNT=[{5}], OPERATOR_ID=[{6}].",
                                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, eCQLTModeChangeInfo.ToString(), settingCount, operatorID));
                        #endregion
                        CQLTModeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                        // 避免中間發生Exception BCS不把BIT ON起來
                        if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                              CQLTModeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                  }
            }
            private void CQLTModeReportReply(string eqpNo, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "CQLTModeReportReply") as Trx;

                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + CQLTModeReportTimeout))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + CQLTModeReportTimeout);
                        }
                        #region[If Bit on]
                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + CQLTModeReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(CQLTModeReportReplyTimeout), trackKey);
                        }
                        #endregion
                        #region[Log]
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,SET BIT=[{2}].",
                            eqpNo, trackKey, value.ToString()));
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void CQLTModeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CQLT MODE REPORT REPLY TIMEOUT ,SET BIT=[OFF].", sArray[0], trackKey));

                        CQLTModeReportReply(sArray[0], eBitResult.OFF, trackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion
            #endregion

            #region [CGMO Glass Request Report]
            private const string CGMOGlassRequestReportTimeout = "CGMOGlassRequestReportTimeout";
            public void CGMOGlassRequestReport(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;

                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", inputData.Metadata.NodeNo));

                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));

                        #region [拆出PLCAgent Data] Bit
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion

                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                              CGMOGlassRequestReportReply(inputData.Metadata.NodeNo, eCGMOResult.Unknown, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }

                        #region [拆出PLCAgent Data]  Word
                        string casSeqNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string jobSeqNo = inputData.EventGroups[0].Events[0].Items[1].Value;

                        #endregion

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE=[{2}], NODE=[{3}], CST_SEQNO=[{4}], JOB_SEQNO=[{5}].",
                                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, casSeqNo, jobSeqNo));

                        //TEST_CREATE_JOBS();

                        eCGMOResult result = eCGMOResult.OK;
                        Job currJob = ObjectManager.JobManager.GetJob(casSeqNo, jobSeqNo);

                        if (currJob == null || currJob.RemoveFlag == true)
                        {
                              result = eCGMOResult.NG;
                              Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("CST_SEQNO=[{0}], JOB_SEQNO=[{1}], CAN FIND JOB DATA OR ALREADY BE REMOVE.", casSeqNo, jobSeqNo));
                        }
                        else
                        {
                              IDictionary<string, string> jobSubItem = ObjectManager.SubJobDataManager.Decode(currJob.EQPFlag, "EQPFlag");

                              if (jobSubItem != null)
                              {
                                    if (result == eCGMOResult.OK)
                                    {
                                          if (jobSubItem.ContainsKey("ENGModeFlag"))
                                          {
                                                if (jobSubItem["ENGModeFlag"] == ((int)eBitResult.ON).ToString())
                                                {
                                                      result = eCGMOResult.NG;
                                                      Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                          string.Format("CST_SEQNO=[{0}], JOB_SEQNO=[{1}], ALREADY HAVE ENG MODE FLAG.", casSeqNo, jobSeqNo));
                                                }
                                          }
                                    }

                                    if (result == eCGMOResult.OK)
                                    {
                                          if (jobSubItem.ContainsKey("CGMOFlag"))
                                          {
                                                if (jobSubItem["CGMOFlag"] == ((int)eBitResult.ON).ToString())
                                                {
                                                      result = eCGMOResult.AlreadyHaveCGMOGlass;
                                                      Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                          string.Format("CST_SEQNO=[{0}], JOB_SEQNO=[{1}], ALREADY HAVE CGMO FLAG.", casSeqNo, jobSeqNo));
                                                }
                                          }
                                    }
                              }

                              if (result == eCGMOResult.OK)
                              {
                                    if (line.File.CGMOFlagCheck.ContainsKey(currJob.GroupIndex))
                                    {
                                          if (line.File.CGMOFlagCheck[currJob.GroupIndex] == true)
                                          {
                                                result = eCGMOResult.AlreadyHaveCGMOGlass;
                                                Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                    string.Format("BC MEMO GLASS CST_SEQNO=[{0}], JOB_SEQNO=[{1}] ALREADY HAVE CGMO FLAG.", casSeqNo, jobSeqNo));
                                          }
                                          else
                                          {
                                                line.File.CGMOFlagCheck[currJob.GroupIndex] = true;
                                                ObjectManager.LineManager.EnqueueSave(line.File);
                                          }
                                    }
                                    else
                                    {
                                          line.File.CGMOFlagCheck.Add(currJob.GroupIndex, true);
                                    }

                                    ObjectManager.LineManager.EnqueueSave(line.File);
                              }

                              //List<Job> jobs = ObjectManager.JobManager.GetJobs().Where(s => s.GroupIndex == currJob.GroupIndex).ToList();

                              //if (jobs != null)
                              //{
                              //    foreach (Job job in jobs)
                              //    {
                              //        if (job != null && job.RemoveFlag == false)
                              //        {
                              //            IDictionary<string, string> otherJobSubItem = ObjectManager.SubJobDataManager.Decode(job.EQPFlag, "EQPFlag");

                              //            if (otherJobSubItem.ContainsKey("CGMOFlag"))
                              //            {
                              //                if (otherJobSubItem["CGMOFlag"] == ((int)eBitResult.ON).ToString())
                              //                {
                              //                    result = eCGMOResult.AlreadyHaveCGMOGlass;
                              //                    Logger.LogWarnWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                              //                        string.Format("CST_SEQNO=[{0}], JOB_SEQNO=[{1}], ALREADY HAVE CGMO FLAG.", casSeqNo, jobSeqNo));
                              //                    break;
                              //                }
                              //            }
                              //        }
                              //    }
                              //}
                        }

                        CGMOGlassRequestReportReply(inputData.Metadata.NodeNo, result, eBitResult.ON, inputData.TrackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                        // 避免中間發生Exception BCS不把BIT ON起來
                        if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                              CGMOGlassRequestReportReply(inputData.Name.Split('_')[0], eCGMOResult.NG, eBitResult.ON, inputData.TrackKey);
                  }
            }
            private void CGMOGlassRequestReportReply(string eqpNo, eCGMOResult result, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "CGMOGlassRequestReportReply") as Trx;

                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)result).ToString();
                        outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                        //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                        outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + CGMOGlassRequestReportTimeout))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + CGMOGlassRequestReportTimeout);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + CGMOGlassRequestReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(CGMOGlassRequestReportReplyTimeout), trackKey);
                        }
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,SET BIT=[{2}], BC_RETURNCODE=[{3}].",
                            eqpNo, trackKey, value.ToString(), result.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void CGMOGlassRequestReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] CGMO GLASS REQUEST REPORT REPLY TIMEOUT ,SET BIT=[OFF].", sArray[0], trackKey));

                        CGMOGlassRequestReportReply(sArray[0], eCGMOResult.Unknown, eBitResult.OFF, trackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region [Realtime Glass Count Request Command]
            private const string RealtimeGlassCountCommandReplyTimeOut = "RealtimeGlassCountCommandReplyTimeOut";
            public void RealtimeGlassCountRequestCommand(string eqpNo, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_RealtimeGlassCountRequestCommand") as Trx;
                        if (outputdata == null)
                        {
                              LogWarn(MethodBase.GetCurrentMethod().Name + "()", string.Format("Can not found Trx {0}_RealtimeGlassCountRequestCommand.", eqpNo));
                              return;
                        }

                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + RealtimeGlassCountCommandReplyTimeOut))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + RealtimeGlassCountCommandReplyTimeOut);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + RealtimeGlassCountCommandReplyTimeOut, false, ParameterManager["T1"].GetInteger(), new System.Timers.ElapsedEventHandler(RealtimeGlassCountRequestCommandTimeOut), trackKey);
                        }

                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,SET BIT=[{2}].",
                            eqpNo, outputdata.TrackKey, value.ToString()));

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void RealtimeGlassCountRequestCommandReply(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;
                        string eqpNo = inputData.Metadata.NodeNo;

                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!!", inputData.Metadata.NodeNo));

                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        if (bitResult == eBitResult.ON)
                        {
                              if (Timermanager.IsAliveTimer(eqpNo + "_" + RealtimeGlassCountCommandReplyTimeOut))
                              {
                                    Timermanager.TerminateTimer(eqpNo + "_" + RealtimeGlassCountCommandReplyTimeOut);
                              }
                              string log = string.Empty;
                              log += string.Format(" ASSEMBLY_TFT_GLASS_COUNT=[{0}] , ", inputData.EventGroups[0].Events[0].Items[0].Value);
                              log += string.Format("NOT_ASSEMBLY_TFT_GLASS_COUNT=[{0}] ,", inputData.EventGroups[0].Events[0].Items[1].Value);
                              log += string.Format("CF_GLASS_COUNT=[{0}], ", inputData.EventGroups[0].Events[0].Items[2].Value);
                              log += string.Format("THROUGH_GLASS_COUNT=[{0}] , ", inputData.EventGroups[0].Events[0].Items[3].Value);
                              log += string.Format("PI_DUMMY_GLASS_COUNT=[{0}] ,", inputData.EventGroups[0].Events[0].Items[4].Value);
                              log += string.Format("UV_MASK_GLASS_COUNT=[{0}]. ", inputData.EventGroups[0].Events[0].Items[5].Value);

                              RealGlassCount gcnt = null;
                              lock (_glassCountListQ)
                              {
                                    if (_glassCountListQ.ContainsKey(eqpNo))
                                    {
                                          gcnt = _glassCountListQ[eqpNo].Dequeue();
                                          lock (gcnt)
                                          {
                                                gcnt.AssemblyTFTGlassCnt = inputData.EventGroups[0].Events[0].Items[0].Value;
                                                gcnt.NotAssemblyTFTGlassCnt = inputData.EventGroups[0].Events[0].Items[1].Value;
                                                gcnt.CFGlassCnt = inputData.EventGroups[0].Events[0].Items[2].Value;
                                                gcnt.ThroughGlassCnt = inputData.EventGroups[0].Events[0].Items[3].Value;
                                                gcnt.PIDummyGlassCnt = inputData.EventGroups[0].Events[0].Items[4].Value;
                                                gcnt.UVMaskGlassCnt = inputData.EventGroups[0].Events[0].Items[5].Value;

                                                gcnt.IsReply = true;
                                          }
                                    }
                              }

                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}], Node=[{3}], BIT=[ON] " + log,
                                          eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID));

                              RealtimeGlassCountRequestCommand(eqpNo, eBitResult.OFF, inputData.TrackKey);
                        }
                        else
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}], BIT=[OFF]",
                                  inputData.Metadata.NodeNo, inputData.TrackKey));
                              return;
                        }
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void RealtimeGlassCountRequestCommandTimeOut(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] REAL TIME GLASS COUNT COMMAND TIMEOUT ,SET BIT=[OFF].", sArray[0], trackKey));

                        RealtimeGlassCountRequestCommand(sArray[0], eBitResult.OFF, trackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region [First Run EVENT]
            public void FirstRunMode(Trx inputData)
            {
                  try
                  {
                        #region[Get EQP]
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                        if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));

                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                        if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                        #endregion

                        eBitResult firstRunMode = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);

                        lock (eqp)
                        {
                              eqp.File.FirstRunMode = (int)firstRunMode == 1 ? eEnableDisable.Enable : eEnableDisable.Disable;
                        }

                        #region[Log]
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] FIRST RUN MODE STATUS=[{2}].", inputData.Metadata.NodeNo,
                            inputData.TrackKey, firstRunMode.ToString()));
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            #region [First Run Mode Report]
            private const string FirstRunModeReportTimeout = "FirstRunModeReportTimeout";
            public void FirstRunModeReport(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;
                        #region[Get EQP & LINE]
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!!", inputData.Metadata.NodeNo));

                        Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                        if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                        #endregion
                        #region [PLCAgent Data Bit]
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion
                        #region[If Bit Off->Return]
                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                              FirstRunModeReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }
                        #endregion
                        #region [PLCAgent Data Word]
                        string firstRunModeInfo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string settingCount = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string operatorID = inputData.EventGroups[0].Events[0].Items[2].Value;

                        eFirstRunModeChangeInfo eFirstRunModeChangeInfo;
                        Enum.TryParse<eFirstRunModeChangeInfo>(firstRunModeInfo, out eFirstRunModeChangeInfo);

                        #endregion
                        #region [Log]
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE=[{2}], NODE=[{3}], FIRST RUN MODE CHANGE INFO=[{4}], SETTING COUNT=[{5}], OPERATOR_ID=[{6}].",
                                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, eFirstRunModeChangeInfo.ToString(), settingCount, operatorID));
                        #endregion
                        FirstRunModeReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                        // 避免中間發生Exception BCS不把BIT ON起來
                        if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                              FirstRunModeReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                  }
            }
            private void FirstRunModeReportReply(string eqpNo, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "FirstRunModeReportReply") as Trx;

                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + FirstRunModeReportTimeout))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + FirstRunModeReportTimeout);
                        }
                        #region[If Bit on]
                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + FirstRunModeReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(FirstRunModeReportReplyTimeout), trackKey);
                        }
                        #endregion
                        #region[Log]
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,SET BIT=[{2}].",
                            eqpNo, trackKey, value.ToString()));
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void FirstRunModeReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] FIRST RUN MODE REPORT REPLY TIMEOUT ,SET BIT=[OFF].", sArray[0], trackKey));

                        FirstRunModeReportReply(sArray[0], eBitResult.OFF, trackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion
            #endregion

            #region [Glass Assembly Report]
            private const string GlassAssemblyReportTimeout = "GlassAssemblyReportTimeout";
            public void GlassAssemblyReport(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;

                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!!", inputData.Metadata.NodeNo));

                        #region [拆出PLCAgent Data] Bit
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion

                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                              GlassAssemblyReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }

                        #region [拆出PLCAgent Data]  Word
                        string tftCasSeqNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string tftJobSeqNo = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string cfCasSeqNo = inputData.EventGroups[0].Events[0].Items[2].Value;
                        string cfJobSeqNo = inputData.EventGroups[0].Events[0].Items[3].Value;

                        #endregion

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE=[{2}], NODE=[{3}], TFT CAS_SEQ_NO=[{4}], JOB_SEQ_NO=[{5}], CF CAS_SEQ_NO=[{6}], JOB_SEQ_NO=[{7}].",
                                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, tftCasSeqNo, tftJobSeqNo, cfCasSeqNo, cfJobSeqNo));

                        GlassAssemblyReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                        AssemblyReport(inputData.TrackKey, eqp.Data.LINEID, eqp.Data.NODEID, tftCasSeqNo, tftJobSeqNo, cfCasSeqNo, cfJobSeqNo);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                        // 避免中間發生Exception BCS不把BIT ON起來
                        if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                              GlassAssemblyReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                  }
            }
            private void GlassAssemblyReportReply(string eqpNo, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "GlassAssemblyReportReply") as Trx;

                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + GlassAssemblyReportTimeout))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + GlassAssemblyReportTimeout);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + GlassAssemblyReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(GlassAssemblyReportReplyTimeout), trackKey);
                        }
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,SET BIT=[{2}].",
                            eqpNo, trackKey, value.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void GlassAssemblyReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] GLASS ASSEMBLY REPORT REPLY TIMEOUT ,SET BIT=[OFF].", sArray[0], trackKey));

                        GlassAssemblyReportReply(sArray[0], eBitResult.OFF, trackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            public void AssemblyReport(string trxID, string lineName, string nodeID, string tftCasSeqNo, string tftJobSeqNo, string cfCasSeqNo, string cfJobSeqNo, bool reReport = false)
            {
                  try
                  {
                        Job tftJob = ObjectManager.JobManager.GetJob(tftCasSeqNo, tftJobSeqNo);
                        Job cfJob = ObjectManager.JobManager.GetJob(cfCasSeqNo, cfJobSeqNo);

                        if (tftJob == null)
                        {
                              Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                      string.Format("TFT JOB DATA NOT FOUND, TFT CASSETTESEQNO=[{0}], JOBSEQNO=[{1}]!",
                                          tftCasSeqNo, tftJobSeqNo));

                              tftJob = new Job(int.Parse(tftCasSeqNo), int.Parse(tftJobSeqNo));
                              ObjectManager.JobManager.NewJobCreateMESDataEmpty(tftJob);
                        }

                        if (cfJob == null)
                        {
                              Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                      string.Format("CF JOB DATA NOT FOUND, CF CASSETTESEQNO=[{0}], JOBSEQNO=[{1}]!",
                                          cfCasSeqNo, cfJobSeqNo));

                              cfJob = new Job(int.Parse(cfCasSeqNo), int.Parse(cfJobSeqNo));
                              ObjectManager.JobManager.NewJobCreateMESDataEmpty(cfJob);
                        }

                        if (tftJob.JobType != eJobType.TFT)
                              Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                      string.Format("TFT CASSETTESEQNO=[{0}], JOBSEQNO=[{1}], BUT JOB TYPE <> TFT TYPE.", tftCasSeqNo, tftJobSeqNo));

                        if (cfJob.JobType != eJobType.CF)
                              Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                      string.Format("CF CASSETTESEQNO=[{0}], JOBSEQNO=[{1}], BUT JOB TYPE <> CF TYPE.", cfCasSeqNo, cfJobSeqNo));

                        tftJob.CellSpecial.CFCassetteSeqNo = cfCasSeqNo;
                        tftJob.CellSpecial.CFJobSeqNo = cfJobSeqNo;
                        tftJob.CellSpecial.AssemblyCompleteFlag = eBitResult.ON;
                        //T3. Keep panel OX information that befor assembly. (20151028 cy.tsai)
                        tftJob.CellSpecial.CFPanelOXInfoUnassembled = cfJob.OXRInformation;   //Keep cf glass
                        tftJob.CellSpecial.TFTPanelOXInfoUnassembled = tftJob.OXRInformation; //Keep tft glass

                        if (tftJob.GlassChipMaskBlockID.Trim() != "" && cfJob.GlassChipMaskBlockID.Trim() != "")
                        {
                              object[] _data = new object[9]//20170616 by huangjiayin: add T\C OX...
                                {
                                    trxID,
                                    lineName,
                                    nodeID,
                                    tftJob.GlassChipMaskBlockID,
                                    tftJob.FromCstID,
                                    tftJob.OXRInformation,
                                    cfJob.GlassChipMaskBlockID,
                                    cfJob.FromCstID,
                                    cfJob.OXRInformation
                                };

                              Invoke(eServiceName.MESService, "AssembleComplete", _data);

                              //Jun Modify 20150522 只要機台有上報過Event 不過BC有沒有Report MES成功 都不需要再進行補報
                              //tftJob.CellSpecial.AssemblyCompleteFlag = eBitResult.ON;

                              //modify by bruce 2015/11/19 EDA spec.change Message ID , EDCGLASSRUNDATASEND to EDCGLASSRUNEND
                              Invoke(eServiceName.EDAService, "EDCGLASSRUNEND", new object[] { trxID, lineName, nodeID, cfJob });
                        }
                        else
                        {
                              Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                      string.Format("TFT GLASSID=[{0}] OR CF GLASSID=[{1}] IS NULL, SKIP REPORT MES!", tftJob.GlassChipMaskBlockID.Trim(), cfJob.GlassChipMaskBlockID.Trim()));
                        }

                        //增加记录Assembly HISTORY 20150328 Tom
                        //ObjectManager.JobManager.RecordAssemblyHistory(nodeID, tftJob, cfJob);
                        ObjectManager.JobManager.RecordAssemblyHistory(nodeID, cfJob, tftJob);

                        //Jun Add 20150328 CSOT要求Assembly事件也要記入Job History
                        RecodeAssemblyInJobHistory(tftJob, nodeID, "L11", reReport, trxID);
                        RecodeAssemblyInJobHistory(cfJob, nodeID, "L11", reReport, trxID);

                        ObjectManager.JobManager.DeleteJob(cfJob);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region [GlassHoldReport]
            private const string GlassHoldReportTimeout = "GlassHoldReportTimeout";
            public void GlassHoldReport(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;

                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!!", inputData.Metadata.NodeNo));

                        #region [拆出PLCAgent Data] Bit
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion

                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                              GlassHoldReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }

                        #region [拆出PLCAgent Data]  Word
                        string casSeqNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string jobSeqNo = inputData.EventGroups[0].Events[0].Items[1].Value;
                        string holdCount = inputData.EventGroups[0].Events[0].Items[2].Value;
                        string operatorID = inputData.EventGroups[0].Events[0].Items[3].Value;

                        #endregion

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}], NODE=[{3}], HOLD CAS_SEQ_NO=[{4}], JOB_SEQ_NO=[{5}], HOLD_COUNT=[{6}], OPERATOR_ID=[{7}].",
                                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, casSeqNo, jobSeqNo, holdCount, operatorID));

                        GlassHoldReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                        Job job = ObjectManager.JobManager.GetJob(casSeqNo, jobSeqNo);
                        if (job == null)
                        {
                              //throw new Exception(string.Format("JOB DATA NOT FOUND, TFT CAS_SEQ_NO=[{0}], JOB_SEQ_NO=[{1}]!", casSeqNo, jobSeqNo));
                              Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format("[EQUIPMENT={0}] JOB IS NULL , CAS_SEQ_NO=[{4}], JOB_SEQ_NO=[{5}], HOLD_COUNT=[{6}], OPERATOR_ID=[{7}].",
                                  eqp.Data.NODENO, casSeqNo, jobSeqNo, holdCount, operatorID));
                              return;
                        }

                        HoldInfo info = new HoldInfo();
                        info.NodeID = eqp.Data.NODEID;
                        info.NodeNo = eqp.Data.NODENO;
                        info.UnitNo = string.Empty;
                        info.UnitID = string.Empty;
                        info.OperatorID = operatorID;
                        info.HoldReason = string.Empty;

                        ObjectManager.JobManager.HoldEventRecord(job, info);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                        // 避免中間發生Exception BCS不把BIT ON起來
                        if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                              GlassHoldReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                  }
            }
            private void GlassHoldReportReply(string eqpNo, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "GlassHoldReportReply") as Trx;

                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + GlassHoldReportTimeout))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + GlassHoldReportTimeout);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + GlassHoldReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(GlassHoldReportReplyTimeout), trackKey);
                        }
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,SET BIT=[{2}].",
                            eqpNo, trackKey, value.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void GlassHoldReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] GLASS HOLD REPORT REPLY TIMEOUT ,SET BIT=[OFF].", sArray[0], trackKey));

                        GlassHoldReportReply(sArray[0], eBitResult.OFF, trackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region [LoaderTackTimeControlCommand]
            private const string LoaderTackTimeCommandReplyTimeout = "LoaderTackTimeCommandReplyTimeout";
            public void LoaderTackTimeControlCommand(string eqpNo, string delayTime, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_LoaderTackTimeControlCommand") as Trx;

                        if (outputdata == null)
                        {
                              LogWarn(MethodBase.GetCurrentMethod().Name + "()", string.Format("Can not found Trx {0}_LoaderTackTimeControlCommand.", eqpNo));
                              return;
                        }

                        outputdata.EventGroups[0].Events[0].Items[0].Value = delayTime;
                        outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                        //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                        outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + LoaderTackTimeCommandReplyTimeout))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + LoaderTackTimeCommandReplyTimeout);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + LoaderTackTimeCommandReplyTimeout, false, ParameterManager["T1"].GetInteger(), new System.Timers.ElapsedEventHandler(LoaderTackTimeControlCommandTimeout), trackKey);
                        }

                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] DELAY TIME=[{3}]S,SET BIT=[{2}].",
                            eqpNo, outputdata.TrackKey, value.ToString(), delayTime));

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            public void LoaderTackTimeControlCommandReply(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;
                        string eqpNo = inputData.Metadata.NodeNo;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!!", inputData.Metadata.NodeNo));

                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                        if (bitResult == eBitResult.ON)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}], Node=[{3}], BIT=[ON].",
                                          eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID));

                              LoaderTackTimeControlCommand(eqpNo, "0", eBitResult.OFF, inputData.TrackKey);
                        }
                        else
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF]", inputData.Metadata.NodeNo, inputData.TrackKey));
                              return;
                        }
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void LoaderTackTimeControlCommandTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] LOADER TACK TIME CONTROL COMMAND TIMEOUT ,SET BIT=[OFF].", sArray[0], trackKey));

                        LoaderTackTimeControlCommand(sArray[0], "0", eBitResult.OFF, trackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region [UVMaskUseCountReport]
            private const string UVMaskUseCountReportTimeout = "UVMaskUseCountReportTimeout";
            public void UVMaskUseCountReport(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;

                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!!", inputData.Metadata.NodeNo));

                        #region [拆出PLCAgent Data] Bit
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion

                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                              UVMaskUseCountReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }

                        #region [拆出PLCAgent Data]  Word
                        string uvMaskID = inputData.EventGroups[0].Events[0].Items[0].Value;
                        string useCount = inputData.EventGroups[0].Events[0].Items[1].Value;

                        #endregion

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM MODE=[{2}], NODE=[{3}] ,UVMASKID=[{4}],USECOUNT=[{5}].",
                                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, uvMaskID, useCount));

                        UVMaskUseCountReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                        //To Do 是否需要更新到UV Mask Job
                        Job job = ObjectManager.JobManager.GetJob(uvMaskID);
                        if (job != null)
                              job.CellSpecial.UVMaskUseCount = useCount;


                        //trxID, lineName, eqpID, maskID, useQty
                        Invoke(eServiceName.MESService, "UVMaskUseCount", new object[] { inputData.TrackKey, eqp.Data.LINEID, eqp.Data.NODEID, uvMaskID, useCount });

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

                        // 避免中間發生Exception BCS不把BIT ON起來
                        if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                              UVMaskUseCountReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
                  }
            }
            private void UVMaskUseCountReportReply(string eqpNo, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "UVMaskUseCountReportReply") as Trx;

                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + UVMaskUseCountReportTimeout))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + UVMaskUseCountReportTimeout);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + UVMaskUseCountReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(UVMaskUseCountReportReplyTimeout), trackKey);
                        }
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,SET BIT=[{2}].",
                            eqpNo, trackKey, value.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void UVMaskUseCountReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] UVMASK USE COUNT REPORT REPLY TIMEOUT ,SET BIT=[OFF].", sArray[0], trackKey));

                        UVMaskUseCountReportReply(sArray[0], eBitResult.OFF, trackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region [GlassCuttingStartReport]
            private const string GlassCuttingStartReportTimeout = "GlassCuttingStartReportTimeout";//20150911 shihyang add
            /// <summary>
            /// (1)在Glass Cutting Start Report 第一次上報
            /// (2)在CUT Sending Event
            /// (3)在BUR Receive Event. 再檢查是否有上報過？
            /// </summary>
            /// <param name="inputData"></param>
            public void GlassCuttingStartReport(Trx inputData)
            {
                try
                {
                    if (inputData.IsInitTrigger) return;
                    #region[Get EQP & LINE]
                    Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                    if (eqp == null) throw new Exception(string.Format("Can't find Equipment No =[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                    Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

                    if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                    #endregion
                    #region [PLCAgent Data Bit]
                    eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                    #endregion
                    #region[If Bit Off->Return]
                    if (bitResult == eBitResult.OFF)
                    {
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                        GlassCuttingStartReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                        return;
                    }
                    #endregion
                    #region [PLCAgent Data Word]
                    string cstSeqNo = inputData.EventGroups[0].Events[0].Items[0].Value;
                    string jobSeqNo = inputData.EventGroups[0].Events[0].Items[1].Value;
                    string glassID = inputData.EventGroups[0].Events[0].Items[2].Value;
                    string jobSeqNoPosition1 = inputData.EventGroups[0].Events[0].Items[3].Value;
                    string panelCount = inputData.EventGroups[0].Events[0].Items[4].Value;
                    #endregion
                    #region [Log]
                    Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}], Node=[{3}],GLASSCUTTINGSTARTREPORT  CAS_SEQ_NO=[{4}], JOB_SEQ_NO=[{5}]"
                        + ",GLASSID =[{6}],JOBSEQNO_FOR_PANELPOSITION1=[{7}],PANEL_COUNT =[{8}]",
                                eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID, cstSeqNo, jobSeqNo, glassID, jobSeqNoPosition1, panelCount));
                    #endregion
                    GlassCuttingStartReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                    #region [shutDownCheckRules]
                    line.File.LastCuttingStartEventTime = DateTime.Now;//20161209 sy add for BC shutdown check
                    #endregion

                    Job job = ObjectManager.JobManager.GetJob(cstSeqNo, jobSeqNo);
                    //ObjectManager.JobManager.TEST_CREATE_OneJOBDataFile(ref job);  //Jun Modify 20141205 麻煩測試完 把測試時使用的Method注解

                    if (job == null) throw new Exception(string.Format("Can't find WIP =[{0}_{1}] in JobEntity!", cstSeqNo, jobSeqNo));
                    List<Job> JobList = new List<Job>();
                    //job.MesProduct.SUBPRODUCTPOSITION = jobSeqNoPosition1.Trim();
                    #region Create 子片 Rule 機台上報片數是否與MES 片數符合
                    int eqpjobcount = 0, createcount = 0;
                    int.TryParse(panelCount, out eqpjobcount);
                    createcount = job.ChipCount;
                    if (line.File.HostMode == eHostMode.OFFLINE)
                    {
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("Panel count[{0}]", panelCount));
                        createcount = eqpjobcount;
                    }
                    else
                    {
                        if (createcount != eqpjobcount)
                        {
                            LogError(MethodBase.GetCurrentMethod().Name, string.Format("Panel count[{0}] <> MES ChipCount[{1}]", panelCount, job.ChipCount.ToString()));
                            if (createcount < eqpjobcount)
                                createcount = eqpjobcount;
                        }
                    }
                    #endregion
                    Create_CELL_ChipPanel(inputData.TrackKey, eqp, job, createcount, ref JobList); //T3 MES 改先生成子片再報 
                    job.CellSpecial.CutCompleteFlag = eBitResult.ON;
                    lock (job)
                    {
                        ObjectManager.JobManager.EnqueueSave(job);
                    }
                    if (line.File.HostMode == eHostMode.OFFLINE) return;
                    #region MES Report CUTComplete
                    CUTLine_MES_CUTCompleteReport(inputData.TrackKey, eqp.Data.LINEID, job, JobList);
                    #endregion
                }
                catch (Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }
            private void GlassCuttingStartReportReply(string eqpNo, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "GlassCuttingStartReportReply") as Trx;

                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + GlassCuttingStartReportTimeout))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + GlassCuttingStartReportTimeout);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + GlassCuttingStartReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(GlassCuttingStartReportReplyTimeout), trackKey);
                        }
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,SET BIT=[{2}].",
                            eqpNo, trackKey, value.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void GlassCuttingStartReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] GLASS CUTTING START REPORT REPLY TIMEOUT ,SET BIT=[OFF].", sArray[0], trackKey));

                        GlassCuttingStartReportReply(sArray[0], eBitResult.OFF, trackKey);

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            public void CUTLine_MES_CUTCompleteReport(string trxId, string lineID, Job job,List<Job> subJobList)
            {
                  try
                  {
                        #region 判斷式在外面比較單純
                        //Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                        //if (eqp == null) throw new Exception(string.Format("Can't find Equipment No=[{0}] in EquipmentEntity!", inputData.Metadata.NodeNo));

                        ////不是CUT Line也不用報
                        //if ((!eqp.Data.LINEID.Contains(eJobDataLineType.CELL.CBCUT_A)) && (!eqp.Data.LINEID.Contains(eJobDataLineType.CELL.CBCUT_B)))
                        //        return;

                        ////CUT Sending Event and BUR Receive Event 才會報
                        //if (!eqp.Data.NODEID.Contains(keyCELLMachingName.CBCUT))
                        //{
                        //    if (!eqp.Data.NODEID.Contains(keyCELLMachingName.CBBUR))
                        //    {
                        //        return;
                        //    }
                        //    else
                        //    {
                        //        if (!inputData.Metadata.Name.Contains("Receive"))
                        //            return;
                        //    }
                        //}
                        //else
                        //{
                        //    if (!inputData.Metadata.Name.Contains("Send"))
                        //        return;
                        //}
                        #endregion

                        //報過就不用報 T3 改先生成 卡再生成前 By sy
                        //if (job.CellSpecial.CutCompleteFlag == eBitResult.ON)
                        //      return;

                        #region MES Report
                        object[] _data = new object[5]
                { 
                    trxId,  /*0 TrackKey*/
                    lineID,    /*1 LineName*/
                    job.FromCstID,
                    job,
                    subJobList
                };
                        Invoke(eServiceName.MESService, "CutComplete", _data);
                        #endregion

                        //會修改這個，表示已報過，不用再補報，如果沒報過還是得補報
                        //(1)在Glass Cutting Start Report 第一次上報
                        //(2)在CUT Sending Event
                        //(3)在BUR Receive Event. 再檢查是否有上報過？
                        job.CellSpecial.CutCompleteFlag = eBitResult.ON;

                        lock (job)
                        {
                              ObjectManager.JobManager.EnqueueSave(job);
                        }
                  }
                  catch (Exception ex)
                  {
                        LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
                  }

            }
            /// <summary>
            /// CELL CUT Glass Create Chip
            /// </summary>
            /// <param name="glassJob">母片JobEntity</param>
            /// <param name="chipcount">產生子片數</param>
            public void Create_CELL_ChipPanel(string trxid, Equipment eqp, Job glassJob, int chipcount,  ref List<Job> JobList, bool saveJob = true)
            {
                try
                {
                    //T3 有一切&二切機台，一切GLASS->Block，二切Block->Chip shihyang add 2015/11/01
                    if (glassJob.CellSpecial.CutCompleteFlag == eBitResult.ON) return;//改先生成 所以生成前也需確認報過了沒有 shihyang add 2015/11/25
                    Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                    if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                    if (glassJob == null) return;
                    string lineName = eqp.Data.LINEID;
                    //Create Rule: Panel Job Sequence No Create Rule
                    string[] subProdLayout = new string[chipcount];//sy add 20151202
                    string[] subProdSpec = new string[chipcount];
                    string[] subProdName = new string[chipcount];
                    string[] subProdPoints = new string[chipcount];//sy add 20151203
                    string[] subProdOriginid = new string[chipcount];//sy add 20151203
                    string[] subProdCarrieRestCodes = new string[chipcount];//sy add 20151229
                    string[] subProdSizeType = new string[chipcount];
                    string[] subProdSizes = new string[chipcount];
                    string[] subProdLine = new string[chipcount];
                    string[] jpsCode = new string[chipcount];
                    string[] jpsGrade = new string[chipcount];
                    string[] fGrade = new string[chipcount];
                    string[] jpsGradeRankGroup = new string[chipcount];//add by menghui 20161214
                    //ONLINE REMOTE MES DOWNLOAD DATA.(ValidateCassetteReply)
                    if (glassJob.MesCstBody.LOTLIST != null)
                    {
                        subProdLayout = glassJob.MesCstBody.LOTLIST[0].SUBPRODUCTSPECLAYOUT.Split(';');//sy add 20151202
                        subProdSpec = glassJob.MesCstBody.LOTLIST[0].SUBPRODUCTSPECS.Split(';'); //Spec 
                        //subProdName= glassJob.MesCstBody.LOTLIST[0].SUBPRODUCTNAMES.Split(';'); //Panel ID, t3 mes has't this field cc.kuang 20150702                    
                        subProdPoints = glassJob.MesCstBody.LOTLIST[0].SUBPRODUCTPOSITIONS.Split(';');//sy add 20151203
                        subProdOriginid = glassJob.MesCstBody.LOTLIST[0].SUBPRODUCTORIGINID.Split(';');//sy add 20151203
                        subProdCarrieRestCodes = glassJob.MesCstBody.LOTLIST[0].SUBPRODUCTCARRIERSETCODES.Split(';');//sy add 20151229 SUBPRODUCTCARRIERSETCODES
                        subProdSizeType = glassJob.MesCstBody.LOTLIST[0].SUBPRODUCTSIZETYPES.Split(';'); //Size type
                        subProdSizes = glassJob.MesCstBody.LOTLIST[0].SUBPRODUCTSIZES.Split(';'); //Size 
                        subProdLine = glassJob.MesCstBody.LOTLIST[0].SUBPRODUCTLINES.Split(';');
                        jpsCode = glassJob.MesProduct.SUBPRODUCTJPSCODE.Split(',');
                        jpsGrade = glassJob.MesProduct.SUBPRODUCTJPSGRADE.Split(',');
                        jpsGradeRankGroup = glassJob.MesCstBody.LOTLIST[0].GRADERANKGROUP.Split(';'); //add by menghui 20161214
                    }
                    int seq = 0;
                    //int seqNoPosition = int.Parse(glassJob.MesProduct.SUBPRODUCTPOSITION.Trim());
                    //20170717 huangjiayin: PCS OXRInforamtion Split....
                    if (line.Data.LINETYPE == eLineType.CELL.CCPCS)
                    {
                        OXRGenerate(glassJob.MesCstBody.LOTLIST[0].PRODUCTSPECLAYOUT.Split(','), subProdLayout, ref glassJob);
                    } 
                    
                    if (line.Data.LINETYPE.Contains("CUT_5") && (glassJob.MesCstBody.LOTLIST[0].ISMIXEDLAYOUT == "Y"))
                    {
                        #region[For CUT_5 MixCut Add By Yangzhenteng20190316]
                        List<string> Panelidlist = new List<string>();
                        List<string> Subproductspeclist = new List<string>();
                        Job _chipOrBlock = (Job)glassJob.Clone();
                        for (int j = 1; j <= glassJob.MesCstBody.LOTLIST[0].SUBPRODUCTSPECLIST.Count; j++)
                        {
                            int y = int.Parse(_chipOrBlock.MesCstBody.LOTLIST[0].SUBPRODUCTSPECLIST[j - 1].SUBPRODUCTSPECLAYOUT.Split(',')[1]);
                            int x = int.Parse(_chipOrBlock.MesCstBody.LOTLIST[0].SUBPRODUCTSPECLIST[j - 1].SUBPRODUCTSPECLAYOUT.Split(',')[0]);
                            string Cutpoint = glassJob.CellSpecial.CutPoint.Split(',')[j - 1].ToString();
                            for (int k = 1; k <= x * y; k++)
                            {
                                string chipID = string.Empty;
                                if (line.File.HostMode == eHostMode.OFFLINE)//001 002 003
                                    _chipOrBlock.MesProduct.PRODUCTNAME = glassJob.MesProduct.PRODUCTNAME + k.ToString().PadLeft(3, '0');
                                else
                                {
                                    int xPoint = 0;
                                    int yPoint = 0;
                                    int yLegth = y;
                                    if (k % yLegth == 0)
                                    {   //表示整除 X = X不用近位 + 原點位置 ,Y = Y最大值(才會整除) +原點位置
                                        xPoint = k / yLegth + PointSToI(Cutpoint.Substring(0, 1)) - 1;
                                        yPoint = yLegth + PointSToI(Cutpoint.Substring(1, 1)) - 1;
                                    }
                                    else
                                    {   //表示無法整除 X = Y欠多少整除補上使之能整除 + 原點位置 ,Y = Y餘數 +原點位置
                                        xPoint = ((k + yLegth - (k % yLegth)) / yLegth) + PointSToI(Cutpoint.Substring(0, 1)) - 1;
                                        yPoint = k % yLegth + PointSToI(Cutpoint.Substring(1, 1)) - 1;
                                    }
                                    _chipOrBlock.MesProduct.PRODUCTNAME = glassJob.MesProduct.PRODUCTNAME + PointIToS(xPoint) + PointIToS(yPoint);
                                    Panelidlist.Add(_chipOrBlock.MesProduct.PRODUCTNAME);
                                }
                            }
                        }
                        lock (Panelidlist)
                        {
                            Panelidlist.Sort();
                        }
                        for (int i = 1; i <= Panelidlist.Count; i++)
                        {
                            if (!int.TryParse(glassJob.JobSequenceNo, out seq)) continue;
                            Job chipOrBlock = (Job)glassJob.Clone();
                            chipOrBlock.MesProduct.PRODUCTNAME = Panelidlist[i - 1];
                            chipOrBlock.JobSequenceNo = ((seq * 1000) + i).ToString(); //新的Job Seq No.//T3 最多1089
                            chipOrBlock.MesCstBody.LOTLIST[0].PRODUCTSIZETYPE = subProdSizeType.Length < i ? string.Empty : subProdSizeType[i - 1];
                            chipOrBlock.MesProduct.SUBPRODUCTJPSCODE = jpsCode.Length < i ? string.Empty : jpsCode[i - 1];
                            string[] jpsCodeList = chipOrBlock.MesProduct.SUBPRODUCTJPSCODE.Split(';');
                            chipOrBlock.CellSpecial.AbnormalTFT = string.Empty;
                            chipOrBlock.CellSpecial.LcdQtapLotGroupID = string.Empty;
                            if (jpsCodeList.Length > 0 && !string.IsNullOrEmpty(jpsCodeList[0].ToString())) chipOrBlock.CellSpecial.AbnormalTFT = jpsCodeList[0].ToString();
                            if (jpsCodeList.Length > 1) chipOrBlock.CellSpecial.LcdQtapLotGroupID = jpsCodeList[1].ToString();
                            chipOrBlock.MesProduct.SUBPRODUCTJPSGRADE = jpsGrade.Length < i ? string.Empty : jpsGrade[i - 1];
                            chipOrBlock.MesProduct.DEFECTLIST = new List<DEFECTc>();
                            foreach (DEFECTc defect in glassJob.MesProduct.DEFECTLIST)
                            {
                                if (defect.SUBPRODUCTNAME == (subProdName.Length < i ? string.Empty : subProdName[i - 1]))
                                {
                                    chipOrBlock.MesProduct.DEFECTLIST.Add(defect);
                                }
                            }
                            DefectDecode(chipOrBlock);
                            //-------------------------------------------------------------------------------------------------------
                            double subProdSize = 0;
                            string subProdLWH = string.Empty;
                            if (subProdSizes.Length >= i)
                            {
                                if (double.TryParse(subProdSizes[i - 1], out subProdSize))
                                {
                                    subProdSize = subProdSize * 100;
                                }
                                else//MES如果給LWH //MES 目前不會給LWH  迴圈目前不會進入
                                {
                                    subProdLWH = subProdSizes[i - 1];
                                    double l = 0; double w = 0; double h = 0;
                                    if (subProdSizes[i - 1].Split('x').Length == 3)
                                    {
                                        if (double.TryParse(subProdSizes[i - 1].Split('x')[0], out l)) l = l * 100;
                                        if (double.TryParse(subProdSizes[i - 1].Split('x')[1], out w)) w = w * 100;
                                        if (double.TryParse(subProdSizes[i - 1].Split('x')[2], out h)) h = h * 100;
                                        subProdSize = l * w * h;
                                    }
                                }
                            }
                            chipOrBlock.CellSpecial.PanelOXInformation = glassJob.OXRInformation.Length < i ? string.Empty : glassJob.OXRInformation.Substring(i - 1, 1);           // sy modify BC OX 都用ASCII 記錄 回機台再轉INT
                            chipOrBlock.CellSpecial.TFTIdLastChar = string.IsNullOrEmpty(glassJob.MesProduct.ARRAYPRODUCTNAME) ? "B" : glassJob.MesProduct.ARRAYPRODUCTNAME.Substring(glassJob.MesProduct.ARRAYPRODUCTNAME.Length - 1, 1);//20171121 By Huangjiayin
                            chipOrBlock.MesCstBody.LOTLIST[0].PRODUCTSPECNAME = subProdSpec[0];//CUT MES 只會給一筆
                            chipOrBlock.MesCstBody.LOTLIST[0].PRODUCTSIZE = subProdSizes.Length < i ? string.Empty : subProdSizes[i - 1];
                            chipOrBlock.MesCstBody.LOTLIST[0].GRADERANKGROUP = jpsGradeRankGroup[0]; //add by menghui 20161214
                            chipOrBlock.CellSpecial.PanelSize = glassJob.CellSpecial.PanelSize;
                            chipOrBlock.CellSpecial.BlockLWH = subProdSizes.Length < i ? string.Empty : subProdLWH; //MES如果給LWH
                            chipOrBlock.MesProduct.SUBPRODUCTSPECNAEM = glassJob.CellSpecial.CutSubProductSpecs; //母片資料與子片相同一開始MES就該給
                            chipOrBlock.MesProduct.SUBPRODUCTGRADES = glassJob.MesProduct.SUBPRODUCTGRADES.Length < i ? string.Empty : glassJob.MesProduct.SUBPRODUCTGRADES.Substring(i - 1, 1); //20170110 MengHui Add
                            chipOrBlock.CellSpecial.CassetteSettingCode = glassJob.CellSpecial.CUTCassetteSettingCode;
                            chipOrBlock.SubstrateType = eSubstrateType.Chip;//Block or Chip  
                            chipOrBlock.ProductType = glassJob.CellSpecial.ProductType1;
                            chipOrBlock.ProductID = glassJob.CellSpecial.ProductID1;
                            chipOrBlock.GlassChipMaskBlockID = chipOrBlock.MesProduct.PRODUCTNAME; //新的ID
                            chipOrBlock.ChipCount = 1;
                            chipOrBlock.JobKey = chipOrBlock.CassetteSequenceNo + "_" + chipOrBlock.JobSequenceNo; //換掉job key 重要
                            chipOrBlock.CellSpecial.FGradeFlag = glassJob.CellSpecial.FGradeFlag.Length < i ? string.Empty : glassJob.CellSpecial.FGradeFlag.Substring(i - 1, 1);
                            chipOrBlock.MesProduct.ARRAYSUBPRODUCTGRADE = glassJob.MesProduct.ARRAYSUBPRODUCTGRADE.Length < i ? string.Empty : glassJob.MesProduct.ARRAYSUBPRODUCTGRADE.Substring(i - 1, 1);
                            chipOrBlock.MesProduct.CFSUBPRODUCTGRADE = glassJob.OXRInformation.Length < i ? string.Empty : glassJob.OXRInformation.Substring(i - 1, 1);
                            foreach (PROCESSLINEc procLine in glassJob.MesCstBody.LOTLIST[0].PROCESSLINELIST)
                            {
                                if (subProdLine.Length >= i)
                                {
                                    if (subProdLine[i - 1] == procLine.LINENAME)
                                    {
                                        if (procLine.LINENAME != "CBCUT500")
                                        {
                                            chipOrBlock.LineRecipeName = procLine.LINERECIPENAME;
                                            chipOrBlock.MesProduct.PPID = procLine.PPID;
                                            chipOrBlock.MES_PPID = procLine.PPID;  //TODO 待處理Local Mode的MES PPID
                                        }
                                        else
                                        {
                                            chipOrBlock.LineRecipeName = procLine.LINERECIPENAME;
                                            chipOrBlock.MesProduct.PPID = procLine.PPID + ";" + glassJob.MesCstBody.LOTLIST[0].STBPRODUCTSPECLIST[0].PPID;
                                            chipOrBlock.MES_PPID = procLine.PPID + ";" + glassJob.MesCstBody.LOTLIST[0].STBPRODUCTSPECLIST[0].PPID;  //TODO 待處理Local Mode的MES PPID
                                        }
                                    }
                                }
                            }
                            chipOrBlock.MesProduct.SUBPRODUCTPOSITION = i.ToString();
                            chipOrBlock.SubProductName = chipOrBlock.GlassChipMaskBlockID;
                            chipOrBlock.EQPJobID = chipOrBlock.GlassChipMaskBlockID;//sy add 20151124
                            chipOrBlock.MesProduct.SUBPRODUCTSIZE = subProdSizes.Length < i ? string.Empty : subProdSizes[i - 1];
                            lock (JobList)
                            {
                                JobList.Add(chipOrBlock);
                            }
                            if (saveJob) //可能不需要有WIP 只要File
                            {
                                ObjectManager.JobManager.AddJob(chipOrBlock); //存檔、新增WIP
                            }
                            //Watson Add 20150424 Save Job History
                            ObjectManager.JobManager.RecordJobHistory(chipOrBlock, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, chipOrBlock.FromSlotNo, eJobEvent.CUT_CREATE_CHIP.ToString(), trxid, string.Empty, chipOrBlock.GlassChipMaskBlockID);
                            string subPath = string.Format(@"{0}\{1}", lineName, glassJob.CassetteSequenceNo);
                            FileFormatManager.CreateFormatFile("CCLineJPS", subPath, chipOrBlock, true);
                            FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, chipOrBlock, true); //在切割后产生一次文件 20150313 Tom
                            string log = string.Format("Create Chip Panel Cassette Seq NO =[{0}], Job Seq No =[{1}],Panel ID =[{2}]", chipOrBlock.CassetteSequenceNo, chipOrBlock.JobSequenceNo, chipOrBlock.GlassChipMaskBlockID);
                            LogInfo(MethodBase.GetCurrentMethod().Name + "()", log);
                        }
                        #endregion
                    }                    
                    else
                    {
                        #region[For Normal CUT&&Mix Cut Not Use Add By Yangzhenteng20190316]
                        for (int i = 1; i <= chipcount; i++)
                        {
                            Job chipOrBlock = (Job)glassJob.Clone(); //完全複製母片資料
                            if (!int.TryParse(glassJob.JobSequenceNo, out seq))
                                continue;
                            chipOrBlock.JobSequenceNo = ((seq * 1000) + i).ToString(); //新的Job Seq No.//T3 最多1089
                            //New Rule ============================================
                            //<SUBPRODUCTSPECS></SUBPRODUCTSPECS><AA;BB;CC;AA;BB;CC>
                            //<SUBPRODUCTNAMES></SUBPRODUCTNAMES><01;02;0*******>
                            //<SUBPRODUCTLINES><SUBPRODUCTLINES><CBCUT400;CBCUT500;0;****>
                            //<SUBPRODUCTSIZETYPES></SUBPRODUCTSIZETYPES><BIG;SMALL;NORMAL>
                            //<SUBPRODUCTSIZES></SUBPRODUCTSIZES><24;27;31.5;*****>
                            string chipID = string.Empty;
                            //if (subProdName.Length >= i) t3 mes has't this field cc.kuang 20150702
                            //chipID = subProdName[i - 1] == string.Empty ? i.ToString().PadLeft(2, '0') : subProdName[i - 1]; //T2 Rule sy mark
                            #region [New GlassID]
                            if (line.Data.LINETYPE == eLineType.CELL.CCPCS)//原本的GLASSID最後一碼更換AA =>AA AB AC AD [MES data A;B;C;D]
                            {
                                if (line.File.HostMode == eHostMode.OFFLINE)//PointIToS(10)=A 由A 開始編 ABCDEFGH 目前不會超過I 且命名不會用到IO
                                    chipOrBlock.MesProduct.PRODUCTNAME = glassJob.MesProduct.PRODUCTNAME.Substring(0, glassJob.MesProduct.PRODUCTNAME.Length - 1) + PointIToS(i + 9);
                                else
                                    chipOrBlock.MesProduct.PRODUCTNAME = glassJob.MesProduct.PRODUCTNAME.Substring(0, glassJob.MesProduct.PRODUCTNAME.Length - 1) + subProdPoints[i - 1];
                            }
                            else//CUT [MES data 原點座標 & X * Y]
                            {
                                if (line.File.HostMode == eHostMode.OFFLINE)//001 002 003
                                    chipOrBlock.MesProduct.PRODUCTNAME = glassJob.MesProduct.PRODUCTNAME + i.ToString().PadLeft(3, '0');
                                else
                                {
                                    int yLegth = int.Parse(chipOrBlock.CellSpecial.CutLayout.Split(',')[1]);
                                    int xPoint = 0; int yPoint = 0;
                                    if (i % yLegth == 0)
                                    {//表示整除 X = X不用近位 + 原點位置 ,Y = Y最大值(才會整除) +原點位置
                                        xPoint = i / yLegth + PointSToI(glassJob.CellSpecial.CutPoint.Substring(0, 1)) - 1;
                                        yPoint = yLegth + PointSToI(glassJob.CellSpecial.CutPoint.Substring(1, 1)) - 1;
                                    }
                                    else
                                    {//表示無法整除 X = Y欠多少整除補上使之能整除 + 原點位置 ,Y = Y餘數 +原點位置
                                        xPoint = ((i + yLegth - (i % yLegth)) / yLegth) + PointSToI(glassJob.CellSpecial.CutPoint.Substring(0, 1)) - 1;
                                        yPoint = i % yLegth + PointSToI(glassJob.CellSpecial.CutPoint.Substring(1, 1)) - 1;
                                    }
                                    chipOrBlock.MesProduct.PRODUCTNAME = glassJob.MesProduct.PRODUCTNAME + PointIToS(xPoint) + PointIToS(yPoint);
                                }
                            }
                            #endregion
                            chipOrBlock.MesCstBody.LOTLIST[0].PRODUCTSIZETYPE = subProdSizeType.Length < i ? string.Empty : subProdSizeType[i - 1];
                            //Jun Add 20150109 For File Service----------------------------------------------------------------------
                            chipOrBlock.MesProduct.SUBPRODUCTJPSCODE = jpsCode.Length < i ? string.Empty : jpsCode[i - 1];
                            //Jun Modify 20150518 For MES邏輯
                            string[] jpsCodeList = chipOrBlock.MesProduct.SUBPRODUCTJPSCODE.Split(';');
                            //20171214 huangjiayin 子片Flag先清空，再给值；防止不需要带Flag的子片误带一样的Flag
                            chipOrBlock.CellSpecial.AbnormalTFT = string.Empty;
                            chipOrBlock.CellSpecial.LcdQtapLotGroupID = string.Empty;
                            if (jpsCodeList.Length > 0 && !string.IsNullOrEmpty(jpsCodeList[0].ToString())) chipOrBlock.CellSpecial.AbnormalTFT = jpsCodeList[0].ToString();
                            if (jpsCodeList.Length > 1) chipOrBlock.CellSpecial.LcdQtapLotGroupID = jpsCodeList[1].ToString();
                            //if (chip.MesProduct.SUBPRODUCTJPSCODE.Split(',').Length == 2)
                            //{
                            //    chip.CellSpecial.AbnormalTFT = (chip.MesProduct.SUBPRODUCTJPSCODE.Split(','))[0].ToString();
                            //    chip.CellSpecial.LcdQtapLotGroupID = (chip.MesProduct.SUBPRODUCTJPSCODE.Split(','))[1].ToString();
                            //}
                            chipOrBlock.MesProduct.SUBPRODUCTJPSGRADE = jpsGrade.Length < i ? string.Empty : jpsGrade[i - 1];
                            chipOrBlock.MesProduct.DEFECTLIST = new List<DEFECTc>();
                            foreach (DEFECTc defect in glassJob.MesProduct.DEFECTLIST)
                            {
                                if (defect.SUBPRODUCTNAME == (subProdName.Length < i ? string.Empty : subProdName[i - 1]))
                                {
                                    chipOrBlock.MesProduct.DEFECTLIST.Add(defect);
                                }
                            }
                            DefectDecode(chipOrBlock);
                            //-------------------------------------------------------------------------------------------------------
                            double subProdSize = 0;
                            string subProdLWH = string.Empty;
                            if (subProdSizes.Length >= i)
                            {
                                if (double.TryParse(subProdSizes[i - 1], out subProdSize))
                                {
                                    subProdSize = subProdSize * 100;
                                }
                                else//MES如果給LWH //MES 目前不會給LWH  迴圈目前不會進入
                                {
                                    subProdLWH = subProdSizes[i - 1];
                                    double l = 0; double w = 0; double h = 0;
                                    if (subProdSizes[i - 1].Split('x').Length == 3)
                                    {
                                        if (double.TryParse(subProdSizes[i - 1].Split('x')[0], out l)) l = l * 100;
                                        if (double.TryParse(subProdSizes[i - 1].Split('x')[1], out w)) w = w * 100;
                                        if (double.TryParse(subProdSizes[i - 1].Split('x')[2], out h)) h = h * 100;
                                        subProdSize = l * w * h;
                                    }
                                }
                            }
                            if (line.Data.LINETYPE == eLineType.CELL.CCPCS)
                            {
                                chipOrBlock.CellSpecial.BlockOXInformation = glassJob.CellSpecial.BlockOXInformation.Length < i ? string.Empty : glassJob.CellSpecial.BlockOXInformation.Substring(i - 1, 1);
                                //huangjiayin 20170714:分割Glass OX给子片
                                if (i <= glassJob.OXRInformation.Split(';').Length) 
                                chipOrBlock.OXRInformation = glassJob.OXRInformation.Split(';')[i - 1];
                                chipOrBlock.MesCstBody.LOTLIST[0].PRODUCTSPECNAME = subProdSpec.Length < i ? string.Empty : subProdSpec[i - 1];
                                chipOrBlock.MesCstBody.LOTLIST[0].PRODUCTSIZE = subProdSizes.Length < i ? string.Empty : subProdSizes[i - 1];
                                chipOrBlock.CellSpecial.BlockSize = subProdSizes.Length < i ? string.Empty : subProdSize.ToString();
                                chipOrBlock.CellSpecial.BlockLWH = subProdSizes.Length < i ? string.Empty : subProdLWH; //MES如果給LWH
                                chipOrBlock.MesProduct.SUBPRODUCTSPECNAEM = subProdSpec.Length < i ? string.Empty : subProdSpec[i - 1]; //Block 都不同
                                chipOrBlock.CellSpecial.CassetteSettingCode = subProdCarrieRestCodes.Length < i ? string.Empty : subProdCarrieRestCodes[i - 1];
                                chipOrBlock.CellSpecial.BlockCount = "1";
                                chipOrBlock.SubstrateType = eSubstrateType.Block;//Block or Chip
                                chipOrBlock.ProductType = glassJob.ProductType;//Modify huangjiayin: 20170714 Type\ID PCS copy  glassjob
                                chipOrBlock.ProductID = glassJob.ProductID;//Modify huangjiayin: 20170714 Type\ID PCS copy  glassjob
                                //ProductType & ID
                            }
                            else//CUT
                            {
                                //chipOrBlock.CellSpecial.PanelOXInformation = ObjectManager.JobManager.M2P_CELL_PanelOX2Int(glassJob.OXRInformation.Length < i ? string.Empty : glassJob.OXRInformation.Substring(i-1, 1));
                                chipOrBlock.CellSpecial.PanelOXInformation = glassJob.OXRInformation.Length < i ? string.Empty : glassJob.OXRInformation.Substring(i - 1, 1);           // sy modify BC OX 都用ASCII 記錄 回機台再轉INT
                                //chipOrBlock.ProductID.Value = int.Parse(chipOrBlock.CellSpecial.CUTProductID);//ProductID 與CUTCrossProductID交換 最後還是以機台send Out Update
                                //chipOrBlock.CellSpecial.CUTCrossProductID = glassJob.ProductID.Value.ToString();
                                //chipOrBlock.ProductType.Value = int.Parse(chipOrBlock.CellSpecial.CUTProductType);//ProductType 與CUTCrossProductType交換 最後還是以機台send Out Update
                                //chipOrBlock.CellSpecial.CUTCrossProductType = glassJob.ProductType.Value.ToString();
                                chipOrBlock.CellSpecial.TFTIdLastChar = string.IsNullOrEmpty(glassJob.MesProduct.ARRAYPRODUCTNAME) ? "B" : glassJob.MesProduct.ARRAYPRODUCTNAME.Substring(glassJob.MesProduct.ARRAYPRODUCTNAME.Length - 1, 1);//20171121 By Huangjiayin
                                chipOrBlock.MesCstBody.LOTLIST[0].PRODUCTSPECNAME = subProdSpec[0];//CUT MES 只會給一筆
                                chipOrBlock.MesCstBody.LOTLIST[0].PRODUCTSIZE = subProdSizes.Length < i ? string.Empty : subProdSizes[i - 1];
                                chipOrBlock.MesCstBody.LOTLIST[0].GRADERANKGROUP = jpsGradeRankGroup[0]; //add by menghui 20161214
                                chipOrBlock.CellSpecial.PanelSize = glassJob.CellSpecial.PanelSize;
                                chipOrBlock.CellSpecial.BlockLWH = subProdSizes.Length < i ? string.Empty : subProdLWH; //MES如果給LWH
                                chipOrBlock.MesProduct.SUBPRODUCTSPECNAEM = glassJob.CellSpecial.CutSubProductSpecs; //母片資料與子片相同一開始MES就該給
                                chipOrBlock.MesProduct.SUBPRODUCTGRADES = glassJob.MesProduct.SUBPRODUCTGRADES.Length < i ? string.Empty : glassJob.MesProduct.SUBPRODUCTGRADES.Substring(i - 1, 1); //20170110 MengHui Add
                                chipOrBlock.CellSpecial.CassetteSettingCode = glassJob.CellSpecial.CUTCassetteSettingCode;
                                chipOrBlock.SubstrateType = eSubstrateType.Chip;//Block or Chip  
                                //ProductType & ID
                                chipOrBlock.ProductType = glassJob.CellSpecial.ProductType1;
                                chipOrBlock.ProductID = glassJob.CellSpecial.ProductID1;
                            }
                            chipOrBlock.GlassChipMaskBlockID = chipOrBlock.MesProduct.PRODUCTNAME; //新的ID
                            chipOrBlock.ChipCount = 1;
                            //chipOrBlock.OXRInformation = glassJob.OXRInformation.Length < i ? string.Empty : glassJob.OXRInformation.Substring(i - 1, 1);//T3移除                        
                            chipOrBlock.JobKey = chipOrBlock.CassetteSequenceNo + "_" + chipOrBlock.JobSequenceNo; //換掉job key 重要
                            chipOrBlock.CellSpecial.FGradeFlag = glassJob.CellSpecial.FGradeFlag.Length < i ? string.Empty : glassJob.CellSpecial.FGradeFlag.Substring(i - 1, 1);
                            chipOrBlock.MesProduct.ARRAYSUBPRODUCTGRADE = glassJob.MesProduct.ARRAYSUBPRODUCTGRADE.Length < i ? string.Empty : glassJob.MesProduct.ARRAYSUBPRODUCTGRADE.Substring(i - 1, 1);
                            chipOrBlock.MesProduct.CFSUBPRODUCTGRADE = glassJob.OXRInformation.Length < i ? string.Empty : glassJob.OXRInformation.Substring(i - 1, 1);
                            foreach (PROCESSLINEc procLine in glassJob.MesCstBody.LOTLIST[0].PROCESSLINELIST)
                            {
                                if (subProdLine.Length >= i)
                                {
                                    if (subProdLine[i - 1] == procLine.LINENAME)
                                    {
                                        if (procLine.LINENAME != "CBCUT500")
                                        {
                                            chipOrBlock.LineRecipeName = procLine.LINERECIPENAME;
                                            chipOrBlock.MesProduct.PPID = procLine.PPID;
                                            chipOrBlock.MES_PPID = procLine.PPID;  //TODO 待處理Local Mode的MES PPID
                                        }
                                        else
                                        {
                                            chipOrBlock.LineRecipeName = procLine.LINERECIPENAME;
                                            chipOrBlock.MesProduct.PPID = procLine.PPID + ";" + glassJob.MesCstBody.LOTLIST[0].STBPRODUCTSPECLIST[0].PPID;
                                            chipOrBlock.MES_PPID = procLine.PPID + ";" + glassJob.MesCstBody.LOTLIST[0].STBPRODUCTSPECLIST[0].PPID;  //TODO 待處理Local Mode的MES PPID
                                        }
                                    }
                                }
                            }
                            chipOrBlock.MesProduct.SUBPRODUCTPOSITION = i.ToString();
                            //chipOrBlock.MesProduct.SUBPRODUCTPOSITION = seqNoPosition.ToString();
                            //seqNoPosition++;
                            chipOrBlock.SubProductName = chipOrBlock.GlassChipMaskBlockID;
                            chipOrBlock.EQPJobID = chipOrBlock.GlassChipMaskBlockID;//sy add 20151124
                            chipOrBlock.MesProduct.SUBPRODUCTSIZE = subProdSizes.Length < i ? string.Empty : subProdSizes[i - 1];
                            JobList.Add(chipOrBlock);
                            if (saveJob) //可能不需要有WIP 只要File
                                ObjectManager.JobManager.AddJob(chipOrBlock); //存檔、新增WIP
                            //Watson Add 20150424 Save Job History
                            ObjectManager.JobManager.RecordJobHistory(chipOrBlock, eqp.Data.NODEID, eqp.Data.NODENO, string.Empty, string.Empty, chipOrBlock.FromSlotNo, eJobEvent.CUT_CREATE_CHIP.ToString(), trxid, string.Empty, chipOrBlock.GlassChipMaskBlockID);
                            string subPath = string.Format(@"{0}\{1}", lineName, glassJob.CassetteSequenceNo);
                            FileFormatManager.CreateFormatFile("CCLineJPS", subPath, chipOrBlock, true);
                            FileFormatManager.CreateFormatFile("CELLShopBEOL", subPath, chipOrBlock, true); //在切割后产生一次文件 20150313 Tom
                            //System.Threading.Thread.Sleep(1);
                            string log = string.Format("Create Chip Panel Cassette Seq NO =[{0}], Job Seq No =[{1}],Panel ID =[{2}]", chipOrBlock.CassetteSequenceNo, chipOrBlock.JobSequenceNo, chipOrBlock.GlassChipMaskBlockID);
                            LogInfo(MethodBase.GetCurrentMethod().Name + "()", log);
                        }
                        #endregion
                    }
                }
                catch (Exception ex)
                {
                    LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }

          private void OXRGenerate(string[] layout,string[] sublayout,ref Job masterJob)//20170717 PCS OXR按母片生成Rule: huangjiayin
          {
              try
              {
                  List<string> vBlockOXR = new List<string>();
                  List<int> vChipsCount = new List<int>();
                  StringBuilder[] eachBlockOXR = new StringBuilder[sublayout.Length];
                  for (int i = 0; i < sublayout.Length; i++)
                  {
                      eachBlockOXR[i] = new StringBuilder();
 
                  }
                  //x_direticon split...
                  int chipStartPos = 0;
                  for (int x = 0; x < int.Parse(layout[0]); x++)
                  {
                      int vBlockChips = 0;
                      for (int y = 0; y < int.Parse(layout[1]); y++)
                      {
                          vBlockChips += int.Parse(sublayout[x * int.Parse(layout[1]) + y].Split(',')[0]) * int.Parse(sublayout[x * int.Parse(layout[1]) + y].Split(',')[1]);
                       }
                      vBlockOXR.Add(masterJob.OXRInformation.Substring(chipStartPos, vBlockChips));
                      chipStartPos += vBlockChips;
                  }



                  //each vBlock split....
                  for (int y = 0; y < int.Parse(layout[1]); y++)
                  {
                      vChipsCount.Add(int.Parse(sublayout[y].Split(',')[1])); 
                  }

                  for (int x = 0; x < vBlockOXR.Count; x++)
                  {
                      int chips=0;
                      while(chips<vBlockOXR[x].Length-1)
                      {
                          for (int y_idx = 0; y_idx <vChipsCount.Count; y_idx++)
                          {
                              eachBlockOXR[x * int.Parse(layout[1]) + y_idx].Append(vBlockOXR[x].Substring(chips,vChipsCount[y_idx]));
                              chips += vChipsCount[y_idx];
                           }
                      }
                     }

                  masterJob.OXRInformation = string.Empty;

                  for (int i = 0; i < eachBlockOXR.Length; i++)
                  {
                      if (i == 0)
                      { masterJob.OXRInformation += eachBlockOXR[i].ToString(); }
                      else
                      { masterJob.OXRInformation +=";"+ eachBlockOXR[i].ToString(); }
 
                  }

                   }




              catch (Exception ex)
              {
                  LogError(MethodBase.GetCurrentMethod().Name + "()", ex); 
              }
          }

            private int PointSToI(string xy)
            {
                switch (xy)
                {
                    case "1": return 1;
                    case "2": return 2;
                    case "3": return 3;
                    case "4": return 4;
                    case "5": return 5;
                    case "6": return 6;
                    case "7": return 7;
                    case "8": return 8;
                    case "9": return 9;
                    case "A": return 10;
                    case "B": return 11;
                    case "C": return 12;
                    case "D": return 13;
                    case "E": return 14;
                    case "F": return 15;
                    case "G": return 16;
                    case "H": return 17;
                    case "J": return 18;
                    case "K": return 19;
                    case "L": return 20;
                    case "M": return 21;
                    case "N": return 22;
                    case "P": return 23;
                    case "Q": return 24;
                    case "R": return 25;
                    case "S": return 26;
                    case "T": return 27;
                    case "U": return 28;
                    case "V": return 29;
                    case "W": return 30;
                    case "X": return 31;
                    case "Y": return 32;
                    case "Z": return 33;
                    default: return 0;
                }
            }
            private string PointIToS(int xy)
            {
                switch (xy)
                {
                    case 1: return "1";
                    case 2: return "2";
                    case 3: return "3";
                    case 4: return "4";
                    case 5: return "5";
                    case 6: return "6";
                    case 7: return "7";
                    case 8: return "8";
                    case 9: return "9";
                    case 10: return "A";
                    case 11: return "B";
                    case 12: return "C";
                    case 13: return "D";
                    case 14: return "E";
                    case 15: return "F";
                    case 16: return "G";
                    case 17: return "H";
                    case 18: return "J";
                    case 19: return "K";
                    case 20: return "L";
                    case 21: return "M";
                    case 22: return "N";
                    case 23: return "P";
                    case 24: return "Q";
                    case 25: return "R";
                    case 26: return "S";
                    case 27: return "T";
                    case 28: return "U";
                    case 29: return "V";
                    case 30: return "W";
                    case 31: return "X";
                    case 32: return "Y";
                    case 33: return "Z";
                    default:
                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("Over Cut Rule Need to Check Cut O point & Cut X,Y"));
                        return "0";
                }
            }
            public void CUTCompleteReportAgain(Trx inputData, Line line, Equipment eqp, Job job)
            {
                try
                {
                    eBitResult CompleteReportAgainFlag = eBitResult.OFF;

                    if (line.Data.LINETYPE.Contains(keyCellLineType.PCS) && eqp.Data.NODENO == "L3" && inputData.Name.Contains(keyEQPEvent.SendOutJobDataReport))
                        CompleteReportAgainFlag = eBitResult.ON;

                    if (line.Data.LINETYPE.Contains(keyCellLineType.CUT) && eqp.Data.NODENO == "L3" && inputData.Name.Contains(keyEQPEvent.SendOutJobDataReport))
                        CompleteReportAgainFlag = eBitResult.ON;

                    #region [CompleteReportAgainFlag ON]
                    if (CompleteReportAgainFlag == eBitResult.ON)
                    {
                        int jobSeq;
                        int.TryParse(job.JobSequenceNo, out jobSeq);
                        Job monJob = ObjectManager.JobManager.GetJob(job.CassetteSequenceNo, (jobSeq / 1000).ToString());
                        if (monJob != null && monJob.CellSpecial.CutCompleteFlag != eBitResult.ON)
                        {
                            List<Job> JobList = new List<Job>();

                            Create_CELL_ChipPanel(inputData.TrackKey, eqp, monJob, monJob.ChipCount, ref JobList); //T3 MES 改先生成子片再報 
                            job.CellSpecial.CutCompleteFlag = eBitResult.ON;
                            lock (monJob)
                            {
                                ObjectManager.JobManager.EnqueueSave(job);
                            }
                            if (line.File.HostMode == eHostMode.OFFLINE) return;
                            #region MES Report CUTComplete
                            CUTLine_MES_CUTCompleteReport(inputData.TrackKey, eqp.Data.LINEID, monJob, JobList);
                            #endregion
                        }
                    }

                    #endregion
                }
                catch (Exception ex)
                {
                    LogError(MethodBase.GetCurrentMethod().Name, ex);
                }
            }
            private void DefectDecode(Job job)
            {
                  try
                  {
                        for (int j = 0; j < job.MesProduct.DEFECTLIST.Count; j++)
                        {
                              if (!string.IsNullOrEmpty(job.MesProduct.DEFECTLIST[j].ARRAYDEFECTCODES))
                              {
                                    string[] codeList = job.MesProduct.DEFECTLIST[j].ARRAYDEFECTCODES.Split(';');
                                    string[] addressList = job.MesProduct.DEFECTLIST[j].ARRAYDEFECTADDRESS.Split(';');

                                    if (codeList.Length == addressList.Length)
                                    {
                                          for (int k = 0; k < codeList.Length; k++)
                                          {
                                                string[] address = addressList[k].Split(',');
                                                if (address.Length == 2)
                                                {
                                                      StringBuilder sb = new StringBuilder();
                                                      sb.AppendFormat("{0}{1}{2}",
                                                          codeList[k].PadRight(5, ' '), address[0].PadLeft(5, '0'), address[1].PadLeft(5, '0'));
                                                      job.CellSpecial.DefectList[k] = sb.ToString();
                                                }
                                          }
                                    }
                              }

                              if (!string.IsNullOrEmpty(job.MesProduct.DEFECTLIST[j].CFDEFECTCODES))
                              {
                                    string[] codeList = job.MesProduct.DEFECTLIST[j].CFDEFECTCODES.Split(';');
                                    string[] addressList = job.MesProduct.DEFECTLIST[j].CFDEFECTADDRESS.Split(';');

                                    if (codeList.Length == addressList.Length)
                                    {
                                          for (int k = 0; k < codeList.Length; k++)
                                          {
                                                string[] address = addressList[k].Split(',');
                                                if (address.Length == 2)
                                                {
                                                      StringBuilder sb = new StringBuilder();
                                                      sb.AppendFormat("{0}{1}{2}",
                                                          codeList[k].PadRight(5, ' '), address[0].PadLeft(5, '0'), address[1].PadLeft(5, '0'));
                                                      job.CellSpecial.DefectList[k + 8] = sb.ToString();
                                                }
                                          }
                                    }
                              }

                              /* t3 mes has't this item cc.kuang 20150702
                              if (!string.IsNullOrEmpty(job.MesProduct.DEFECTLIST[j].PIDEFECTCODES))
                              {
                                  string[] codeList = job.MesProduct.DEFECTLIST[j].PIDEFECTCODES.Split(';');
                                  string[] addressList = job.MesProduct.DEFECTLIST[j].PIDEFECTADDRESS.Split(';');

                                  if (codeList.Length == addressList.Length)
                                  {
                                      for (int k = 0; k < codeList.Length; k++)
                                      {
                                          string[] address = addressList[k].Split(',');
                                          if (address.Length == 2)
                                          {
                                              StringBuilder sb = new StringBuilder();
                                              sb.AppendFormat("{0}{1}{2}",
                                                  codeList[k].PadRight(5, ' '), address[0].PadLeft(5, '0'), address[1].PadLeft(5, '0'));
                                              job.CellSpecial.DefectList[k + 18] = sb.ToString();
                                          }
                                      }
                                  }
                              }

                              if (!string.IsNullOrEmpty(job.MesProduct.DEFECTLIST[j].ODFDEFECTCODES))
                              {
                                  string[] codeList = job.MesProduct.DEFECTLIST[j].ODFDEFECTCODES.Split(';');
                                  string[] addressList = job.MesProduct.DEFECTLIST[j].ODFDEFECTADDRESS.Split(';');

                                  if (codeList.Length == addressList.Length)
                                  {
                                      for (int k = 0; k < codeList.Length; k++)
                                      {
                                          string[] address = addressList[k].Split(',');
                                          if (address.Length == 2)
                                          {
                                              StringBuilder sb = new StringBuilder();
                                              sb.AppendFormat("{0}{1}{2}",
                                                  codeList[k].PadRight(5, ' '), address[0].PadLeft(5, '0'), address[1].PadLeft(5, '0'));
                                              job.CellSpecial.DefectList[k + 24] = sb.ToString();
                                          }
                                      }
                                  }
                              }*/
                        }
                  }
                  catch (Exception ex)
                  {
                        LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            #region UVA Line Mask Use Count Report Watson Add 20141216
            private const string MaskUseCountReportTimeout = "MaskUseCountReportTimeout"; //Watson Add 20141216 For UVA line
            public void MaskUseCountReport(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;

                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!!", inputData.Metadata.NodeNo));

                        #region [拆出PLCAgent Data] Bit
                        eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                        #endregion

                        if (bitResult == eBitResult.OFF)
                        {
                              Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                  string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                              MaskUseCountReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                              return;
                        }

                        #region [拆出PLCAgent Data]  Word
                        List<MaskUsedCountReport.MASKc> maskList = new List<MaskUsedCountReport.MASKc>();
                        string log = string.Empty;
                        for (int i = 0; i < inputData.EventGroups[0].Events[0].Items.Count - 1; i += 2)
                        {
                              MaskUsedCountReport.MASKc mask = new MaskUsedCountReport.MASKc();
                              mask.MASKNAME = inputData.EventGroups[0].Events[0].Items[i].Value.Trim();
                              mask.MASKUSECOUNT = inputData.EventGroups[0].Events[0].Items[i + 1].Value.Trim();
                              if (mask.MASKNAME.Trim() != string.Empty)
                                    maskList.Add(mask);
                              log += string.Format(" MASK_ID=[{0}] , USE_COUNT=[{1}] ", mask.MASKNAME, mask.MASKUSECOUNT);

                        }

                        #endregion

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] CIM Mode=[{2}], Node=[{3}] " + log,
                                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID));

                        MaskUseCountReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);

                        //To Do 是否需要更新到 Mask Job?


                        //trxID, string lineName, string machineName, IList<MaskUsedCountReport.MASKc> maskList
                        Invoke(eServiceName.MESService, "MaskUsedCountReport", new object[] { inputData.TrackKey, eqp.Data.LINEID, eqp.Data.NODEID, maskList });

                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void MaskUseCountReportReply(string eqpNo, eBitResult value, string trackKey)
            {
                  try
                  {
                        Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_MaskUseCountReportReply") as Trx;

                        outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);

                        if (_timerManager.IsAliveTimer(eqpNo + "_" + MaskUseCountReportTimeout))
                        {
                              _timerManager.TerminateTimer(eqpNo + "_" + MaskUseCountReportTimeout);
                        }

                        if (value.Equals(eBitResult.ON))
                        {
                              _timerManager.CreateTimer(eqpNo + "_" + MaskUseCountReportTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(MaskUseCountReportReplyTimeout), trackKey);
                        }
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,SET BIT=[{2}].",
                            eqpNo, trackKey, value.ToString()));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            private void MaskUseCountReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                  try
                  {
                        UserTimer timer = subjet as UserTimer;
                        string tmp = timer.TimerId;
                        string trackKey = timer.State.ToString();
                        string[] sArray = tmp.Split('_');

                        Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] MASK USE COUNT REPORT REPLY TIMEOUT ,SET BIT=[OFF].", sArray[0], trackKey));

                        MaskUseCountReportReply(sArray[0], eBitResult.OFF, trackKey);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }
            #endregion

            public void GroupIndexBlock(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!!", inputData.Metadata.NodeNo));

                        IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(eqp.Data.NODENO);
                        string groupIndex = inputData.EventGroups[0].Events[0].Items[0].Value;
                        lock (eqp)
                        {
                              eqp.File.GroupIndex = groupIndex;
                        }
                        ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                        int itemCount = inputData.EventGroups[0].Events[0].Items.Count;
                        Dictionary<string, string> itemValue = new Dictionary<string, string>();
                        string strLog = string.Format("{0}=[{1}]", inputData.EventGroups[0].Events[0].Items[0].Name, groupIndex);
                        string itemLog = "";
                        for (int i = 1; i < itemCount; i++)
                        {
                              itemValue.Add(inputData.EventGroups[0].Events[0].Items[i].Name, inputData.EventGroups[0].Events[0].Items[i].Value);
                              itemLog = string.Format("{0}=[{1}]", inputData.EventGroups[0].Events[0].Items[i].Name, inputData.EventGroups[0].Events[0].Items[i].Value);
                              strLog = strLog + "," + itemLog;
                        }
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                             string.Format("[EQUIPMENT={0}] [[BCS <- EQP]][{1}], Group Index Report,{2}",
                             eqp.Data.NODENO, inputData.TrackKey, strLog));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            public void ProductIDBlock(Trx inputData)
            {
                  try
                  {
                        if (inputData.IsInitTrigger) return;
                        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);

                        if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!!", inputData.Metadata.NodeNo));

                        IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(eqp.Data.NODENO);
                        string productID = inputData.EventGroups[0].Events[0].Items[0].Value;
                        lock (eqp)
                        {
                              eqp.File.ProductID = productID;
                        }
                        ObjectManager.EquipmentManager.EnqueueSave(eqp.File);

                        int itemCount = inputData.EventGroups[0].Events[0].Items.Count;
                        Dictionary<string, string> itemValue = new Dictionary<string, string>();
                        string strLog = string.Format("{0}=[{1}]", inputData.EventGroups[0].Events[0].Items[0].Name, productID);
                        string itemLog = "";
                        for (int i = 1; i < itemCount; i++)
                        {
                              itemValue.Add(inputData.EventGroups[0].Events[0].Items[i].Name, inputData.EventGroups[0].Events[0].Items[i].Value);
                              itemLog = string.Format("{0}=[{1}]", inputData.EventGroups[0].Events[0].Items[i].Name, inputData.EventGroups[0].Events[0].Items[i].Value);
                              strLog = strLog + "," + itemLog;
                        }
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                             string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}], ProductID Report,{2}",
                             eqp.Data.NODENO, inputData.TrackKey, strLog));
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            private void SendPLCData(Trx outputData)
            {
                  xMessage msg = new xMessage();
                  msg.Data = outputData;
                  msg.ToAgent = eAgentName.PLCAgent;
                  PutMessage(msg);
            }

            //Watson Modify 20150421  SAVE DB 
            //移至JobManage裏統一來做避免增、刪、修欄位時，全部都要修改
            private void RecodeAssemblyInJobHistory(Job job, string nodeId, string nodeNo, bool reReport, string trxid)
            {
                  try
                  {
                        if (reReport)
                            ObjectManager.JobManager.RecordJobHistory(job, nodeId, nodeNo, string.Empty, string.Empty, string.Empty, eJobEvent.Assembly.ToString() + "_Re-Report", trxid);
                        else if (job.GlassChipMaskBlockID.Trim() != "")
                            ObjectManager.JobManager.RecordJobHistory(job, nodeId, nodeNo, string.Empty, string.Empty, string.Empty, eJobEvent.Assembly.ToString(), trxid);
                        else
                            ObjectManager.JobManager.RecordJobHistory(job, nodeId, nodeNo, string.Empty, string.Empty, string.Empty, eJobEvent.Assembly_NG.ToString(), trxid);
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
            }

            //private void RecodeAssemblyInJobHistory(Job job, string nodeId, string nodeNo)
            //{
            //    try
            //    {
            //        // Save DB
            //        int i = 0;
            //        JOBHISTORY his = new JOBHISTORY()
            //        {
            //            UPDATETIME = DateTime.Now,
            //            EVENTNAME = "Assembly",
            //            CASSETTESEQNO = int.TryParse(job.CassetteSequenceNo, out i) == true ? i : 0,
            //            JOBSEQNO = int.TryParse(job.JobSequenceNo, out i) == true ? i : 0,
            //            JOBID = job.GlassChipMaskBlockID,
            //            GROUPINDEX = int.TryParse(job.GroupIndex, out i) == true ? i : 0,
            //            PRODUCTTYPE = job.ProductType.Value,
            //            CSTOPERATIONMODE = job.CSTOperationMode.ToString(),
            //            SUBSTRATETYPE = job.SubstrateType.ToString(),
            //            CIMMODE = job.CIMMode.ToString(),
            //            JOBTYPE = job.JobType.ToString(),
            //            JOBJUDGE = job.JobJudge.ToString(),
            //            SAMPLINGSLOTFLAG = job.SamplingSlotFlag,
            //            OXRINFORMATIONREQUESTFLAG = job.OXRInformationRequestFlag,
            //            FIRSTRUNFLAG = job.FirstRunFlag,
            //            JOBGRADE = job.JobGrade,
            //            PPID = job.PPID,
            //            INSPRESERVATIONS = job.INSPReservations,
            //            LASTGLASSFLAG = job.LastGlassFlag,
            //            INSPJUDGEDDATA = job.InspJudgedData,
            //            TRACKINGDATA = job.TrackingData,
            //            EQPFLAG = job.EQPFlag,
            //            OXRINFORMATION = job.OXRInformation,
            //            CHIPCOUNT = job.ChipCount,
            //            NODENO = nodeNo,
            //            UNITNO = "",
            //            PORTNO = "", 
            //            SLOTNO = "", 
            //            NODEID = nodeId, 
            //            SOURCECASSETTEID=job.FromCstID, //
            //            CURRENTCASSETTEID=job.ToCstID
            //         };
            //        ObjectManager.JobManager.InsertDB(his);

            //    }
            //    catch (Exception ex)
            //    {
            //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            //    }
            //}

            private void ScanShareFolderThreadFunc()
            {
                  List<ShareFolderInfo> share_folder_list = new List<ShareFolderInfo>();
                  foreach (BaseInfo info in dicReportBy.Values)
                  {
                        if (info is ShareFolderInfo)
                              share_folder_list.Add((ShareFolderInfo)info);
                  }

                  Thread.Sleep(5000);

                  while (_runThread)
                  {
                        foreach (ShareFolderInfo info in share_folder_list)
                        {
                              try
                              {
                                    if ((DateTime.Now - info.LastScanTime).TotalSeconds > info.ScanSecond)
                                    {
                                          List<DefectFile> defect_files = new List<DefectFile>();
                                          List<string> local_files = DownloadGlassFileFromShareFolder(info);
                                          info.LastScanTime = DateTime.Now;

                                          foreach (string local_fname in local_files)
                                          {
                                                try
                                                {
                                                      DefectFile defect_file = DefectFile.Load(Logger, this.LogName, info.NodeNo, local_fname);
                                                      if (defect_file != null)
                                                      {
                                                            defect_files.Add(defect_file);
                                                            DefectDataReport_ByDefectFile(info.NodeNo, defect_file.DateTimeStr, defect_file);
                                                      }
                                                }
                                                catch (Exception exception)
                                                {
                                                      Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()-Load Defect File and Save", exception);
                                                }
                                          }
                                    }
                              }
                              catch (Exception ex)
                              {
                                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()-Scan and Download Defect File", ex);
                              }
                        }
                        Thread.Sleep(500);
                  }
            }

            /// <summary>
            /// 從遠端共享資料夾下載檔案
            /// </summary>
            /// <param name="remoteFolder">遠端資料夾(\\remote ip\LineID\ToolID\DefectData)</param>
            /// <param name="loginUserName">登入遠端電腦的帳號</param>
            /// <param name="password">密碼</param>
            /// <param name="localFolder">下載存檔到本地端資料夾(D:\Folder)</param>
            /// <param name="deleteRemoteFile">true:表示刪除遠端檔案, false:表示保留遠端檔案</param>
            /// <returns>下載後的本地端檔名, 不含路徑</returns>
            private List<string> DownloadGlassFileFromShareFolder(ShareFolderInfo info)
            {
                  List<string> ret = new List<string>();
                  try
                  {
                        if (!info.Connected)
                              OpenShareFolderConnection(info);

                        List<string> remote_files = null, delete_files = new List<string>();
                        if (info.Connected && GetFileListFromShareFolder(info, out remote_files))
                        {
                              if (remote_files != null && remote_files.Count > 0)
                              {
                                    if (!Directory.Exists(info.LocalTargetPath))
                                          Directory.CreateDirectory(info.LocalTargetPath);
                                    #region 從共享資料夾下載檔案至本地端
                                    {
                                          foreach (string remote_file in remote_files)
                                          {
                                                try
                                                {
                                                      string fname = Path.GetFileName(remote_file);
                                                      string target = Path.Combine(info.LocalTargetPath, fname);
                                                      File.Copy(remote_file, target, true);
                                                      ret.Add(target);
                                                      delete_files.Add(remote_file);
                                                      Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Download File[{0}] from Share Folder[{1}] to Local Folder[{2}]", fname, info.SourcePath, info.LocalTargetPath));
                                                }
                                                catch (Exception exception)
                                                {
                                                      Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", exception);
                                                }
                                          }
                                    }
                                    #endregion
                                    #region 從共享資料夾刪除已經下載的檔案
                                    {
                                          if (info.DeleteRemoteFile)
                                          {
                                                foreach (string remote_file in delete_files)
                                                {
                                                      try
                                                      {
                                                            File.Delete(remote_file);
                                                            Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Delete File[{0}] from Share Folder[{1}]", remote_file, info.SourcePath));
                                                      }
                                                      catch (Exception exception)
                                                      {
                                                            Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", exception);
                                                      }
                                                }
                                          }
                                    }
                                    #endregion
                              }
                        }
                  }
                  catch (Exception ex)
                  {
                        Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
                  return ret;
            }

            /// <summary>
            /// 與遠端電腦建立連線
            /// </summary>
            /// <param name="remoteFolder">遠端資料夾</param>
            /// <param name="loginUsername">遠端電腦登入名稱</param>
            /// <param name="password">密碼</param>
            /// <returns>true:表示登入成功, false:表示登入失敗, 登入失敗會自行記Log</returns>
            private bool OpenShareFolderConnection(ShareFolderInfo info)
            {
                  try
                  {
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Try to Connect share folder[{0}]", info.SourcePath));
                        Process p = new Process();
                        p.StartInfo.FileName = "net.exe";
                        if (info.LoginID == string.Empty)
                              p.StartInfo.Arguments = string.Format(@"use {0}", info.SourcePath);
                        else
                              p.StartInfo.Arguments = string.Format(@"use {0} /user:{1} {2}", info.SourcePath, info.LoginID, info.Password);
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.RedirectStandardError = true;
                        p.StartInfo.CreateNoWindow = true;
                        p.Start();
                        p.WaitForExit();
                        string error = p.StandardError.ReadToEnd();
                        p.Dispose();
                        if (error != string.Empty)
                        {
                              Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Cannot connect to share folder[{0}]. Error:[{1}]", info.SourcePath, error));
                        }
                        else
                        {
                              info.Connected = true;
                              Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Connect to share folder[{0}] OK", info.SourcePath));
                        }
                  }
                  catch (Exception ex)
                  {
                        Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
                  return info.Connected;
            }

            /// <summary>
            /// 與遠端電腦斷線, remoteFolder 可填string.Empty
            /// </summary>
            /// <param name="remoteFolder">遠端資料夾</param>
            private void CloseShareFolderConnection(string remoteFolder)
            {
                  if (!string.IsNullOrEmpty(remoteFolder) && remoteFolder[remoteFolder.Length - 1] == '\\')
                        remoteFolder.Remove(remoteFolder.Length - 1);

                  if (string.IsNullOrEmpty(remoteFolder))
                        remoteFolder = "*";

                  Process p = new Process();
                  p.StartInfo.FileName = "net.exe";
                  p.StartInfo.Arguments = string.Format(@"use {0} /delete /y", remoteFolder);
                  p.StartInfo.UseShellExecute = false;
                  p.StartInfo.RedirectStandardError = true;
                  p.StartInfo.CreateNoWindow = true;
                  p.Start();
                  p.WaitForExit();
                  Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Disconnect from share folder[{0}] OK", remoteFolder));
            }

            /// <summary>
            /// 從 Share Folder 下載檔案
            /// </summary>
            /// <param name="info"></param>
            /// <param name="remoteFiles"></param>
            /// <returns></returns>
            private bool GetFileListFromShareFolder(ShareFolderInfo info, out List<string> remoteFiles)
            {
                  remoteFiles = new List<string>();
                  try
                  {
                        GetFileListFromShareFolder(info.SourcePath, remoteFiles);
                        //string[] dirs = Directory.GetDirectories(info.SourcePath);
                        //foreach (string dir in dirs)
                        //{
                        //    if (dir != "." && dir != "..")
                        //    {
                        //        GetFileListFromShareFolder(dir, remoteFiles);
                        //    }
                        //}
                        if (remoteFiles.Count > 0)
                        {
                              //有取到File才要記LOG
                              Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Get DefectData file count[{0}] from ShareFolder[{1}] Success.", remoteFiles.Count, info.SourcePath));
                        }
                  }
                  catch (Exception ex)
                  {
                        info.Connected = false;
                        Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
                  return info.Connected;
            }

            private void GetFileListFromShareFolder(string folder, List<string> remoteFiles)
            {
                  string[] remote_files = Directory.GetFiles(folder);
                  if (remote_files != null && remote_files.Length > 0)
                  {
                        foreach (string remote_file in remote_files)
                        {
                              string file_name = Path.GetFileName(remote_file);
                              if (string.Compare(Path.GetExtension(file_name), ".dat", true) == 0)
                              {
                                    if (file_name.IndexOf("_DefectData_") > 0)
                                    {
                                          remoteFiles.Add(remote_file);
                                    }
                              }
                        }
                  }
            }

            private void DefectDataReport_ByDefectFile(string nodeNo, string trackKey, DefectFile defectFile)
            {
                  Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                      string.Format("[EQUIPMENT={0}] [TrackKey={1}] [CstSeqNo={2}] [JobSeqNo={3}] [GlsID={4}]", nodeNo, trackKey, defectFile.CassetteSeqNo, defectFile.JobSeqNo, defectFile.GlassID));

                  Equipment eqp = ObjectManager.EquipmentManager.GetEQP(defectFile.NodeNo);
                  if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", defectFile.NodeNo));

                  Job job = ObjectManager.JobManager.GetJob(defectFile.CassetteSeqNo, defectFile.JobSeqNo);
                  if (job == null)
                  {
                        //throw new Exception(string.Format("CAN'T FIND JOB, CASSETTE SEQUENCENO=[{0}],JOB SEQUENCE NO=[{1}] IN JOBENTITY!", edcFile.CassetteSeqNo, edcFile.JobSeqNo));
                        Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                               string.Format("EQUIPMENT=[{0}] CAN'T FIND JOB DATA, CST_SEQNO=[{1}] JOB_SEQNO=[{2}] CAN'T REPORT DEFECT DATA!!",
                                   eqp.Data.NODENO, defectFile.CassetteSeqNo, defectFile.JobSeqNo));
                        return;
                  }

                  DefectCode defectcode = new DefectCode();
                  defectcode.EqpNo = eqp.Data.NODENO;
                  defectcode.ChipPostion = "0";
                  defectcode.UnitNo = "0";
                  defectcode.DefectCodes = defectFile.GetDefectCodes();
                  job.DefectCodes.Add(defectcode);
                  ObjectManager.JobManager.EnqueueSave(job);

                  Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                      string.Format("SaveJobDefectCodes [EQUIPMENT={0}] [File={1}] CST_SEQNO=[{2}] JOB_SEQNO=[{3}].",
                      eqp.Data.NODENO, defectFile.Filename, defectFile.CassetteSeqNo, defectFile.JobSeqNo));
            }

            /// <summary>
            /// 從FTP Server下載檔案
            /// </summary>
            /// <param name="ftpFolder">FTP資料夾</param>
            /// <param name="ftpFilename">FTP檔名</param>
            /// <param name="loginUsername">登入FTP Server的帳號</param>
            /// <param name="password">密碼</param>
            /// <param name="localFolder">檔案下載至本地資料夾</param>
            private bool DownloadFileFromFtp(string ftpFolder, string ftpFilename, string loginUsername, string password, string localFolder, out string localFilename)
            {
                  bool ret = false;
                  localFilename = string.Empty;
                  FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(string.Format("{0}/{1}", ftpFolder, ftpFilename));
                  ftpWebRequest.KeepAlive = false;
                  ftpWebRequest.Method = WebRequestMethods.Ftp.DownloadFile;
                  ftpWebRequest.Timeout = 60000;//一分钟
                  ftpWebRequest.UseBinary = true;
                  ftpWebRequest.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
                  ftpWebRequest.Credentials = new NetworkCredential(loginUsername, password);
                  FtpWebResponse response = null;
                  FileStream outputStream = null;
                  Stream ftpStream = null;

                  try
                  {
                        localFilename = Path.Combine(localFolder, ftpFilename);
                        response = ftpWebRequest.GetResponse() as FtpWebResponse;
                        outputStream = new FileStream(localFilename, FileMode.Create);
                        ftpStream = response.GetResponseStream();
                        byte[] buffer = new byte[2048];
                        int offset = 0;
                        int read_count = ftpStream.Read(buffer, 0, buffer.Length);
                        while (read_count > 0)
                        {
                              outputStream.Write(buffer, 0, read_count);
                              offset += read_count;
                              read_count = ftpStream.Read(buffer, 0, buffer.Length);
                        }
                        ret = true;
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("download file {0},from {1} to {2} Success.", ftpFilename, ftpFolder, localFolder));
                  }
                  catch (System.Exception ex)
                  {
                        Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
                  finally
                  {
                        if (ftpStream != null)
                              ftpStream.Close();
                        if (outputStream != null)
                              outputStream.Close();
                        if (response != null)
                              response.Close();
                  }
                  return ret;
            }

            /// <summary>
            /// 從FTP下載DefectFile
            /// </summary>
            /// <param name="info"></param>
            /// <returns>下載到本地端的檔案</returns>
            private List<string> DownloadGlassFileFromFtp(FTPInfo info)
            {
                  List<string> ret = new List<string>();
                  try
                  {
                        int count = 1, retry = ParameterManager.Parameters["EDC_REPORT_BY_FTP_RETRY_COUNT"].GetInteger();
                        if (retry > 0) count += retry;
                        Dictionary<string, List<string>> ftp_file_downloadlist = new Dictionary<string, List<string>>(), ftp_file_deletelist = new Dictionary<string, List<string>>();

                        //取檔案清單, 下載檔案, 刪除檔案. 這三個動作至少會做一次, 若有例外則總共會重試retry次
                        //取檔案清單及下載檔案時出現例外, 則這兩個動作會全部重試, 重試的次數遞減直至零
                        //刪除檔案時出現例外, 已經刪掉的檔案不會復原, 尚未刪除的檔案會重試, 重試的次數遞減直至零
                        #region 取檔案清單及下載檔案
                        {
                              while (count > 0)//重試次數
                              {
                                    try
                                    {
                                          List<string> sub_folders = null;
                                          List<string> file_list = GetFileListFromFtp(info, info.SourcePath, info.LoginID, info.Password, out sub_folders);
                                          ftp_file_downloadlist.Add(info.SourcePath, file_list);

                                          if (file_list != null && file_list.Count > 0)
                                          {
                                                if (!Directory.Exists(info.LocalTargetPath))
                                                      Directory.CreateDirectory(info.LocalTargetPath);

                                                List<string> delete_list = new List<string>();
                                                foreach (string ftp_file in file_list)
                                                {
                                                      string local_fname = string.Empty;
                                                      if (DownloadFileFromFtp(info.SourcePath, ftp_file, info.LoginID, info.Password, info.LocalTargetPath, out local_fname))
                                                      {
                                                            ret.Add(local_fname);
                                                            delete_list.Add(ftp_file);
                                                      }
                                                }
                                                ftp_file_deletelist.Add(info.SourcePath, delete_list);
                                          }
                                          break;
                                    }
                                    catch (Exception exception)
                                    {
                                          Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", exception);
                                          ftp_file_downloadlist.Clear();
                                          ret.Clear();
                                          count--;
                                    }
                              }
                        }
                        #endregion
                        #region 刪除已經下載的FTP檔案
                        {
                              bool loop = true;
                              while (info.DeleteRemoteFile && count > 0 && loop)
                              {
                                    foreach (string ftp_folder in ftp_file_deletelist.Keys)
                                    {
                                          List<string> delete_list = ftp_file_deletelist[ftp_folder];
                                          List<int> index = new List<int>();
                                          try
                                          {
                                                for (int i = 0; i < delete_list.Count; i++)
                                                {
                                                      DeleteFileFromFtp(ftp_folder, delete_list[i], info.LoginID, info.Password);
                                                      index.Add(i);
                                                }
                                                loop = false;
                                          }
                                          catch (Exception exception)
                                          {
                                                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", exception);
                                                for (int i = index.Count - 1; i >= 0; i--)
                                                {
                                                      delete_list.RemoveAt(i);
                                                }
                                                loop = true;
                                                count--;
                                          }
                                    }
                              }
                        }
                        #endregion
                  }
                  catch (Exception ex)
                  {
                        Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
                  return ret;
            }

            /// <summary>
            /// 從FTP Server取檔案清單, glassID可填string.Empty
            /// </summary>
            /// <param name="ftpFolder">FTP資料夾</param>
            /// <param name="glassID">GlassID, 用來組檔名(GlassID_EDC_YYYYMMDDmmhhss.dat)</param>
            /// <param name="loginUsername">登入FTP Server的帳號</param>
            /// <param name="password">密碼</param>
            /// <param name="subFolder">指定ftpFolder內的子資料夾</param>
            /// <returns>FTP Server上的檔名, 不包含FTP路徑</returns>
            private List<string> GetFileListFromFtp(FTPInfo ftpInfo, string ftpFolder, string loginUsername, string password, out List<string> subFolder)
            {
                  List<string> ret = new List<string>();
                  subFolder = new List<string>();
                  FtpWebResponse response = null;
                  try
                  {
                        FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(ftpFolder);
                        ftpWebRequest.KeepAlive = false;
                        ftpWebRequest.Method = WebRequestMethods.Ftp.ListDirectoryDetails;
                        ftpWebRequest.Timeout = 60000;//一分钟
                        ftpWebRequest.UseBinary = true;
                        ftpWebRequest.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
                        ftpWebRequest.Credentials = new NetworkCredential(loginUsername, password);
                        response = ftpWebRequest.GetResponse() as FtpWebResponse;
                        using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                        {
                              string fname = sr.ReadLine();
                              while (fname != null)
                              {
                                    //04-01-15  02:57PM       <DIR>          CBSDP251
                                    //04-01-15  03:39PM                    0 CB DP254
                                    //04-01-15  03:45PM                    0 A123434564568505141234123412341234Z.txt
                                    string[] substrs = fname.Split(new string[1] { "<DIR>" }, StringSplitOptions.RemoveEmptyEntries);
                                    if (substrs != null && substrs.Length == 2)
                                    {
                                          subFolder.Add(substrs[1].Trim());
                                    }
                                    else
                                    {
                                          int start_idx = "04-01-15  03:45PM                    0 ".Length;
                                          ret.Add(fname.Substring(start_idx).Trim());
                                    }
                                    fname = sr.ReadLine();
                              }
                        }
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Get Defect file count[{0}] from FTP[{1}] Success.", ret.Count, ftpFolder));
                  }
                  catch (Exception ex)
                  {
                        Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
                  finally
                  {
                        if (response != null)
                              response.Close();
                  }
                  return ret;
            }

            /// <summary>
            /// 從FTP Server上刪除檔案
            /// </summary>
            /// <param name="ftpFolder">FTP資料夾</param>
            /// <param name="ftpFilename">FTP檔名</param>
            /// <param name="loginUsername">登入FTP Server的帳號</param>
            /// <param name="password">密碼</param>
            private void DeleteFileFromFtp(string ftpFolder, string ftpFilename, string loginUsername, string password)
            {
                  FtpWebRequest ftpWebRequest = (FtpWebRequest)WebRequest.Create(string.Format("{0}/{1}", ftpFolder, ftpFilename));
                  ftpWebRequest.KeepAlive = false;
                  ftpWebRequest.Method = WebRequestMethods.Ftp.DeleteFile;
                  ftpWebRequest.Timeout = 60000;//一分钟
                  ftpWebRequest.AuthenticationLevel = System.Net.Security.AuthenticationLevel.MutualAuthRequested;
                  ftpWebRequest.Credentials = new NetworkCredential(loginUsername, password);

                  string result = String.Empty;
                  FtpWebResponse response = null;
                  Stream datastream = null;
                  try
                  {
                        response = (FtpWebResponse)ftpWebRequest.GetResponse();
                        datastream = response.GetResponseStream();
                        using (StreamReader sr = new StreamReader(datastream))
                        {
                              result = sr.ReadToEnd();
                        }
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Delete file={0} from {1} Success.", ftpFilename, ftpFolder));
                  }
                  catch (System.Exception ex)
                  {
                        Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  }
                  finally
                  {
                        if (datastream != null)
                              datastream.Close();
                        if (response != null)
                              response.Close();
                  }
            }

            private void ScanFtpThreadFunc()
            {
                  List<FTPInfo> ftp_list = new List<FTPInfo>();
                  foreach (BaseInfo info in dicReportBy.Values)
                  {
                        if (info is FTPInfo)
                              ftp_list.Add((FTPInfo)info);
                  }

                  Thread.Sleep(5000);

                  while (_runThread)
                  {
                        foreach (FTPInfo info in ftp_list)
                        {
                              try
                              {
                                    if ((DateTime.Now - info.LastScanTime).TotalSeconds > info.ScanSecond)
                                    {
                                          List<DefectFile> defect_files = new List<DefectFile>();
                                          List<string> local_files = DownloadGlassFileFromFtp(info);
                                          info.LastScanTime = DateTime.Now;

                                          foreach (string local_fname in local_files)
                                          {
                                                try
                                                {
                                                      DefectFile defect_file = DefectFile.Load(Logger, this.LogName, info.NodeNo, local_fname);
                                                      if (defect_file != null)
                                                      {
                                                            defect_files.Add(defect_file);
                                                            DefectDataReport_ByDefectFile(info.NodeNo, defect_file.DateTimeStr, defect_file);
                                                      }
                                                }
                                                catch (Exception exception)
                                                {
                                                      Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()-Load Defect File and Save", exception);
                                                }
                                          }
                                    }
                              }
                              catch (Exception ex)
                              {
                                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()-Scan and Download Defect File", ex);
                              }
                        }
                        Thread.Sleep(500);
                  }
            }

            //nouse
            public IList<RealGlassCount> GlassCountRequest(string trxID, string lineName)
            {
                  try
                  {
                        if (_glassCountListQ.Count > 0)
                              throw new Exception(string.Format("REALTIME GLASS COUNT REQUEST ALREADY QUERYING!!"));

                        IList<RealGlassCount> glassCountList = new List<RealGlassCount>();

                        List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPsByLine(lineName);
                        if (eqps != null)
                        {
                              foreach (Equipment eqp in eqps)
                              {
                                    RealGlassCount gcnt = new RealGlassCount();
                                    gcnt.EqpNo = eqp.Data.NODENO;

                                    glassCountList.Add(gcnt);

                                    lock (_glassCountListQ)
                                    {
                                          if (_glassCountListQ.ContainsKey(gcnt.EqpNo))
                                          {
                                                //To Do
                                          }
                                          else
                                          {
                                                Queue<RealGlassCount> gcntQ = new Queue<RealGlassCount>();
                                                gcntQ.Enqueue(gcnt);
                                                _glassCountListQ.Add(eqp.Data.NODENO, gcntQ);
                                          }
                                    }

                                    //base.Invoke(eServiceName.CELLSpecialService, "RealtimeGlassCountRequestCommand", new object[] { eqp.Data.NODENO, eBitResult.ON, this.CreateTrxID() });
                                    RealtimeGlassCountRequestCommand(eqp.Data.NODENO, eBitResult.ON, trxID);
                                    Thread.Sleep(10);
                              }
                        }

                        DateTime startTime = DateTime.Now;
                        while (new TimeSpan(DateTime.Now.Ticks - startTime.Ticks).TotalMilliseconds < ParameterManager["REALTIMEGLASSCOUNTTIME"].GetInteger())
                        {
                              Thread.Sleep(300);//阻塞呼叫的Function
                              bool finishflag = true;
                              foreach (RealGlassCount gcnt in glassCountList)
                              {
                                    finishflag = gcnt.IsReply;
                                    if (finishflag == false)//发现没有完成的，跳出不用检查，让线程休息一下。
                                          break;
                              }
                              if (finishflag)//全部检查完成。停止阻塞
                              {
                                    Logger.LogInfoWrite(LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                        string.Format("[BCS -> EQP][{0}]REAL GLASS COUNT REQUEST COMMAND IS FINISH.", ""));
                                    break;
                              }
                        }

                        lock (_glassCountListQ)
                              _glassCountListQ.Clear();

                        //To Do Reply OPI
                        return glassCountList;
                  }
                  catch (Exception ex)
                  {
                        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                        return null;
                  }
            }

            public void LoaderTackTimeCheck(string lineID, string trackKey)
            {
                  Dictionary<string, int> nodeDelayTime = new Dictionary<string, int>();
                  int delayTime = 0;
                  #region[Get LINE]
                  Line line = ObjectManager.LineManager.GetLine(lineID);
                  if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, lineID));
                  #endregion
                  string linetype = string.Empty;
                  if (lineID.Contains(keyCellLineType.PIL))
                      linetype = keyCellLineType.PIL;
                if (lineID.Contains(keyCellLineType.ODF))                  
                      linetype = keyCellLineType.ODF;                
                  List<Equipment> eqps = ObjectManager.EquipmentManager.GetEQPsByLine(lineID);
                  if (eqps == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT IN EQUIPMENTENTITY!"));

                  foreach (Equipment eqp in eqps)
                  {
                      if (ConstantManager.ContainsKey(string.Format("{0}_TACKTIME_CHECKUNIT_{1}", linetype,eqp.Data.NODENO)))
                      {
                          if (ConstantManager.ContainsKey(string.Format("{0}_TACKTIME_DELAYTIME_{1}", linetype, eqp.Data.NODENO)))
                          {
                              //string[] checkList = ConstantManager["ODF_TACKTIME_CHECKUNIT_" + eqp.Data.NODENO]["CHECKUNIT"].Value.Split(',');
                              string[] checkList = ConstantManager[string.Format("{0}_TACKTIME_CHECKUNIT_", linetype) + eqp.Data.NODENO]["CHECKUNIT"].Value.Split(',');
                              IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(eqp.Data.NODENO);
                              if (units == null)
                              {
                                  this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                      string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] UNITS IN UNITENTITY!!", eqp.Data.NODENO));
                                  continue;
                              }

                              int downUnitCnt = 0;
                              foreach (Unit unit in units)
                              {
                                  foreach (string unitNo in checkList)
                                  {
                                      if (unit.Data.UNITNO == unitNo)
                                      {
                                          if (unit.File.Status == eEQPStatus.STOP || unit.File.Status == eEQPStatus.PAUSE || unit.File.Status == eEQPStatus.SETUP)
                                              downUnitCnt++;

                                          break;
                                      }
                                  }
                              }

                              int nodeDelay;
                              if (nodeDelayTime.ContainsKey(eqp.Data.NODENO))
                              {
                                  if (ConstantManager[string.Format("{0}_TACKTIME_DELAYTIME_{1}", linetype, eqp.Data.NODENO)].Values.ContainsKey("DOWN" + downUnitCnt.ToString()))
                                  {
                                      if (int.TryParse(ConstantManager[string.Format("{0}_TACKTIME_DELAYTIME_{1}", linetype, eqp.Data.NODENO)]["DOWN" + downUnitCnt.ToString()].Value, out nodeDelay))
                                          nodeDelayTime[eqp.Data.NODENO] = nodeDelay;
                                      else
                                          nodeDelayTime[eqp.Data.NODENO] = 10;
                                  }
                                  else
                                  {
                                      nodeDelayTime[eqp.Data.NODENO] = 10;
                                  }
                              }
                              else
                              {
                                  if (ConstantManager[string.Format("{0}_TACKTIME_DELAYTIME_{1}", linetype, eqp.Data.NODENO)].Values.ContainsKey("DOWN" + downUnitCnt.ToString()))
                                  {
                                      if (int.TryParse(ConstantManager[string.Format("{0}_TACKTIME_DELAYTIME_{1}", linetype, eqp.Data.NODENO)]["DOWN" + downUnitCnt.ToString()].Value, out nodeDelay))
                                          nodeDelayTime.Add(eqp.Data.NODENO, nodeDelay);
                                      else
                                          nodeDelayTime.Add(eqp.Data.NODENO, 10);
                                  }
                                  else
                                  {
                                      nodeDelayTime.Add(eqp.Data.NODENO, 10);
                                  }
                              }
                          }
                      }
                  }

                  foreach (int value in nodeDelayTime.Values)
                  {
                        if (delayTime < value)
                              delayTime = value;
                  }

                  if (_odfLastDelayTime != delayTime) //新的DelayTime 与原Delay TIME 没有变化则 不下Command 个机台。 20150513 Tom
                  {
                        LoaderTackTimeControlCommand("L2", delayTime.ToString(), eBitResult.ON, trackKey);
                        _odfLastDelayTime = delayTime;
                  }
            }

            private string CellPanelOX(string PanelOX)
            {
                try
                {
                    switch (PanelOX.Trim())
                    {
                        case "0000": return "0";
                        case "0001": return "1";
                        case "0010": return "2";
                        case "0011": return "3";
                        case "0100": return "4";
                        case "0101": return "5";
                        case "0110": return "6";
                        case "0111": return "7";
                        case "1000": return "8";
                        case "1001": return "9";
                        default: return "0";
                    }
                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                    return "0";
                }
            }

            //add PolRWLifttimeRequest by zhuxingxing 20160825
            #region PolRWLifetimeRequest RW TimeFlag 请求；
            private const string PolRWLifetimeRequestTimeout = "PolRWLifetimeRequestTimeout";
            public void PolRWLifetimeRequest(Trx inputData)
            {
                try
                {
                      if (inputData.IsInitTrigger) return;
                       
                      #region[Get EQP & LINE]
                      Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                      if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!!", inputData.Metadata.NodeNo));
                      Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                      if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                       #endregion

                      #region [PLCAgent Data Bit]
                      eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                      #endregion
                      #region[If Bit Off->Return]
                      if (bitResult == eBitResult.OFF)
                      {
                          LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                              string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[OFF].", inputData.Metadata.NodeNo, inputData.TrackKey));

                          PolRWLifetimeRequestReply(inputData.Metadata.NodeNo, "0",eBitResult.OFF, inputData.TrackKey);
                          return;
                      }
                      #endregion

                      #region [PLCAgent Data Word]
                      string cstSeqNO = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                      string jobSeqNO = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();
                      #endregion

                      #region[Log]
                      LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                          string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CST_SEQNO=[{2}] JOB_SEQNO=[{3}]].",
                              inputData.Metadata.NodeNo, inputData.TrackKey, cstSeqNO, jobSeqNO, ""));
                      #endregion
                      Job slotData = ObjectManager.JobManager.GetJob(cstSeqNO, jobSeqNO);
                      if (slotData == null) throw new Exception(string.Format("EQUIPMENT=[{0}] CAN'T FIND JOB DATA, CST_SEQNO=[{1}] JOB_SEQNO=[{2}]!", inputData.Metadata.NodeNo, cstSeqNO, jobSeqNO));
                                               
                      PolRWLifetimeRequestReply(inputData.Metadata.NodeNo, slotData.CellSpecial.RwLiftTime, eBitResult.ON, inputData.TrackKey);
                }
                catch(Exception ex)
                {
                    this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                    // 避免中間發生Exception BCS不把BIT ON起來
                    if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                    {
                        PolRWLifetimeRequestReply(inputData.Metadata.NodeNo,
                            "0",
                            eBitResult.ON, inputData.TrackKey);
                    }
                }
            }

            public void PolRWLifetimeRequestReply(string eqpNo, string rwQtime,eBitResult value, string trackKey)
            {
                try
                {
                    Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "PolRWLifetimeRequestReply") as Trx;
                    #region[If Bit Off->Return]
                    if (value == eBitResult.OFF)
                    {
                        outputdata.EventGroups[0].Events[0].IsDisable = true;
                        outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                        outputdata.TrackKey = trackKey;
                        SendPLCData(outputdata);
                        if (_timerManager.IsAliveTimer(eqpNo + "_" + PolRWLifetimeRequestTimeout))
                        {
                            _timerManager.TerminateTimer(eqpNo + "_" + PolRWLifetimeRequestTimeout);
                        }
                        #region[Log]
                        LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,SET BIT=[{3}].",
                                eqpNo, trackKey,value));
                        #endregion
                        return;
                    }
                    #endregion

                    #region[Reply Data ] 
                    if (rwQtime == "") rwQtime = "0";
                    outputdata.EventGroups[0].Events[0].Items[0].Value = int.Parse(rwQtime).ToString();
                    outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                    outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                    outputdata.TrackKey = trackKey;
                    SendPLCData(outputdata);
                    #endregion
                    #region[Create Timeout Timer]
                    if (_timerManager.IsAliveTimer(eqpNo + "_" + PolRWLifetimeRequestTimeout))
                    {
                        _timerManager.TerminateTimer(eqpNo + "_" + PolRWLifetimeRequestTimeout);
                    }
                    _timerManager.CreateTimer(eqpNo + "_" + PolRWLifetimeRequestTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PolRWLifetimeRequestReplyTimeout), trackKey);
                    #endregion
                   
                    #region[Log]
                    LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}]  ,RETURN_CODE=[{2}],SET BIT=[{3}].",
                            eqpNo, trackKey, value));
                    #endregion

                }
                catch (Exception ex)
                {
                    this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }

            private void PolRWLifetimeRequestReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
            {
                try
                {
                    UserTimer timer = subjet as UserTimer;
                    string tmp = timer.TimerId;
                    string trackKey = timer.State.ToString();
                    string[] sArray = tmp.Split('_');
                    LogWarn(MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}]PolRWLifetimeRequestTimeout, SET BIT=[OFF].",
                        sArray[0], trackKey));

                    PolRWLifetimeRequestReply(sArray[0], "", eBitResult.OFF, trackKey);
                }
                catch (Exception ex)
                {
                    this.LogError(MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }
            #endregion  //add RW TimeReply by zhuxingxing 

           //add SamplingFlagCommand in SOR/CHN by zhuxingxing 20161019
          #region  SOR/CHN 机台增加隔层放片通知的功能
          private const string SamplingFlagCommandTimeout = "SamplingFlagCommandTimeout";
          public void SamplingFlagCommand(string trxID, string lineName, string RanMod, string eqpNo, eBitResult value, string CassetteSequenceNo)
          {
              try
              {
                  //1.Check eqpInfor
                  Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                  if(eqp == null)
                  {
                      throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));
                  }

                  Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo+ "_" + "SamplingFlagCommand") as Trx;
                  if (value.Equals(eBitResult.OFF))
                  {
                      outputdata.EventGroups[0].Events[0].IsDisable = true;
                  }
                  else
                  {
                      outputdata.EventGroups[0].Events[0].Items[0].Value = CassetteSequenceNo;
                      outputdata.EventGroups[0].Events[0].Items[1].Value = RanMod;
                  }
                  outputdata.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                  outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)value).ToString();
                  outputdata.TrackKey = trxID;
                  SendPLCData(outputdata);

                  string timeoutName;
                  timeoutName = string.Format(eqpNo + "_" + SamplingFlagCommandTimeout);
                  if (_timerManager.IsAliveTimer(timeoutName))
                  {
                      _timerManager.TerminateTimer(timeoutName);
                  }

                  if (value.Equals(eBitResult.ON))
                  {
                      _timerManager.CreateTimer(eqpNo + "_" + SamplingFlagCommandTimeout, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(SamplingFlagCommandReplyTimeout), trxID.ToString());
                  }
                  Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ,CassetteSequenceNo=[{2}],SamplingFlag=[{3}] SET BIT=[{4}].",
                      eqpNo, trxID.ToString(), CassetteSequenceNo, RanMod.ToString(),value.ToString()));

              }
              catch(Exception ex)
              {
                  this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
              }
          }
          public void SamplingFlagCommandReply(Trx inputData)
          {
              try
              {
                  if (inputData.IsInitTrigger) return;
                  #region[Get EQP & LINE]
                  Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                  if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!!", inputData.Metadata.NodeNo));
                  Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                  if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                  #endregion
                  #region [PLCAgent Data Bit]
                  eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                  #endregion
                  #region[If Bit Off->Return]
                  if (bitResult == eBitResult.OFF)
                  {
                      LogInfo(MethodBase.GetCurrentMethod().Name + "()",
                          string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                      return;
                  }
                  #endregion
                  Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] CIM MODE=[{2}], NODE=[{3}] ",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, eqp.Data.NODEID));
                  string timeoutName = string.Format(eqp.Data.NODENO + "_" + SamplingFlagCommandTimeout);
                  if (_timerManager.IsAliveTimer(timeoutName))
                  {
                      _timerManager.TerminateTimer(timeoutName);
                  }

                  SamplingFlagCommand(inputData.TrackKey, line.Data.LINEID, string.Empty, eqp.Data.NODENO, eBitResult.OFF, string.Empty);

              }
              catch(Exception ex)
              {
                  this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
              }
          }
          private void SamplingFlagCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
          {
              try
              {
                  UserTimer timer = subjet as UserTimer;
                  string tmp = timer.TimerId;
                  string trackKey = timer.State.ToString();
                  string[] sArray = tmp.Split('_');

                  Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SamplingFlagCommand REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));

                  SamplingFlagCommand(trackKey, string.Empty, string.Empty, sArray[0], eBitResult.OFF, string.Empty);
              }
              catch(Exception ex)
              {
                  this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
              }
          }

          #endregion
          //Add By Yangzhenteng20180420
          #region [Glass Remote Rejudge]
         public void GlassRemoteRejudgeRequestReport(Trx inputData)
          {
              try
              {
                  if (inputData.IsInitTrigger) return;
                  #region[Get EQP & LINE]
                  Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                  if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));
                  Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                  if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                  #endregion
                  #region [PLCAgent Data Bit]
                  eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                  #endregion
                  #region[If Bit Off->Return]
                  if (bitResult == eBitResult.OFF)
                  {
                      Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                      GlassRemoteRejudgeRequestReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                      return;
                  }
                  #endregion
                  string PanelID = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                  string PortNo = inputData.EventGroups[0].Events[0].Items[1].Value;
                  string SlotNo = inputData.EventGroups[0].Events[0].Items[2].Value;
                  string SideNo = inputData.EventGroups[0].Events[0].Items[3].Value;
                  Port port = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, PortNo);
                  Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] PanelID=[{3}] PortNO=[{4}] SlotNo=[{5}] SideNo=[{6}]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, PanelID, PortNo, SlotNo, SideNo));
                  GlassRemoteRejudgeRequestReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                  Invoke(eServiceName.MESService, "PanelRejudgeReport", new object[] { inputData.TrackKey, line.Data.LINEID, eqp.Data.NODEID, PanelID, port.Data.PORTID, SlotNo, SideNo });
              }
              catch (Exception ex)
              {
                  this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                      GlassRemoteRejudgeRequestReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
              }
          }
          public void GlassRemoteRejudgeRequestReportReply(string eqpNo, eBitResult value, string trackKey)
          {
              try
              {
                  Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "GlassRemoteRejudgeRequestReportReply") as Trx;
                  outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                  outputdata.TrackKey = trackKey;
                  SendPLCData(outputdata);
                  string timeName = string.Format(eqpNo + "_" + "GlassRemoteRejudgeRequestReportTimeout");
                  if (_timerManager.IsAliveTimer(timeName))
                  {
                      _timerManager.TerminateTimer(timeName);
                  }
                  if (value.Equals(eBitResult.ON))
                  {
                      _timerManager.CreateTimer(timeName, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(GlassRemoteRejudgeRequestReportReplyTimeout), trackKey);
                  }
                  Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,SET BIT=[{2}].",
                   eqpNo, trackKey, value.ToString()));
              }
              catch (Exception ex)
              {
                  this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
              }
          }
          private void GlassRemoteRejudgeRequestReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
          {
              try
              {
                  UserTimer timer = subjet as UserTimer;
                  string tmp = timer.TimerId;
                  string trackKey = timer.State.ToString();
                  string[] sArray = tmp.Split('_');
                  Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] Glass Remote Rejudge Request Report Reply Timeout ,SET BIT=[OFF].", sArray[0], trackKey));
                  GlassRemoteRejudgeRequestReportReply(sArray[0], eBitResult.OFF, trackKey);
              }
              catch (Exception ex)
              {
                  this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
              }
          }
          public void RejudgeCommandCheck(string eqpID, string GlassID, string PortName, string SlotNo, string SideNo, string GlassJudgeResult, string trackKey, string Productspecname, string Inboxname,string ReasonCode )
          {
              try
              {
                  string err = string.Empty;
                  Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(eqpID);
                  if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqp.Data.NODENO));
                  if (eqp.File.CIMMode == eBitResult.OFF)
                  {
                      err = string.Format("[{0}] EQUIPMENT=[{1}] CIM_MODE=[OFF], Can't Send Glass Remote Rejudge Command!", MethodBase.GetCurrentMethod().Name, eqp.Data.NODENO);
                      Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID, err });
                      LogWarn(MethodBase.GetCurrentMethod().Name + "()", err);
                      return;
                  }
                  string GLAssid = GlassID.Trim();
                  RemoteRejudgePanel Remoterejudgepanel = new RemoteRejudgePanel(eqpID, GLAssid, PortName, SlotNo, SideNo, GlassJudgeResult, trackKey, Productspecname, Inboxname, ReasonCode);

                  if (!GlassRejudgeSetCommand.ContainsKey(GLAssid))
                  {
                      lock (GlassRejudgeSetCommand)
                      {
                          GlassRejudgeSetCommand.Add(GLAssid, Remoterejudgepanel);
                      }
                  }
                  Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                  string.Format("[MES -> BC][{0}] Receive Panel Remote Rejudge Request Command ( EQPID=[{1}],GlassID =[{2}],PortNo = [{3}],SlotNO=[{4}],SideNO=[{5}],GlassJudgeResult=[{6}],Productspecname=[{7}]).",
                      trackKey, eqpID, GLAssid, PortName, SlotNo, SideNo, GlassJudgeResult, Productspecname, Inboxname));
              }
              catch (Exception ex)
              {
                  this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
              }
          }
          public void GlassRemoteRejudgeCommand(RemoteRejudgePanel R)
          {
              try
              {
                  string err = string.Empty;
                  Equipment eqp = ObjectManager.EquipmentManager.GetEQPByID(R.Eqpid);
                  #region[GlassJudge]
                  string glassjudgeresult = string.Empty;
                  if (R.Glassjudgeresult != null)
                  {
                      switch (R.Glassjudgeresult)
                      {
                          case "OK":
                              glassjudgeresult = "1";
                              break;
                          case "NG":
                              glassjudgeresult = "2";
                              break;
                          case "RW":
                              glassjudgeresult = "3";
                              break;
                          case "RJ":
                              glassjudgeresult = "9";
                              break;
                      }
                  }
                  #endregion
                  Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(eqp.Data.NODENO + "_" + "GlassRemoteRejudgeCommand") as Trx;
                  outputData.EventGroups[0].Events[0].Items[0].Value = R.Glassid.Trim();
                  outputData.EventGroups[0].Events[0].Items[1].Value = R.Portno;
                  outputData.EventGroups[0].Events[0].Items[2].Value = R.Slotno;
                  outputData.EventGroups[0].Events[0].Items[3].Value = R.Sideno;
                  outputData.EventGroups[0].Events[0].Items[4].Value = glassjudgeresult;
                  outputData.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                  outputData.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                  outputData.TrackKey = R.TrackKey;
                  SendPLCData(outputData);
                  R.IsSend = true;
                  _ExecuteFlag = true;
                  string timeName = string.Format(eqp.Data.NODENO + "_" + "GlassRemoteRejudgeCommandReplyTimeout");
                  if (_timerManager.IsAliveTimer(timeName))
                  {
                      _timerManager.TerminateTimer(timeName);
                  }
                  _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(), new System.Timers.ElapsedEventHandler(GlassRemoteRejudgeCommandReplyTimeout), outputData.TrackKey);
                  Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                 string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] Panel_ID=[{2}] Port_No=[{3}] Slot_NO=[{4}] Side_No=[{5}] GlassJudgeResult=[{6}] SET BIT=[ON].", eqp.Data.NODENO,
                 outputData.TrackKey, R.Glassid, R.Portno, R.Slotno, R.Sideno, glassjudgeresult));
              }
              catch (Exception ex)
              {
                  _ExecuteFlag = false;
                  this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
              }
          }
          public void GlassRemoteRejudgeCommandReply(Trx inputData)
          {
              try
              {
                  if (inputData.IsInitTrigger) return;
                  string eqpNo = inputData.Metadata.NodeNo;
                  string ReturnCode = int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value).ToString();
                  eReturnCode4 retCode = (eReturnCode4)int.Parse(inputData.EventGroups[0].Events[0].Items[0].Value);
                  eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                  Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                      string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[{2}] Glass Remote Rejudge Command Reply RETURN_CODE=[{3}]",
                      eqpNo, inputData.TrackKey, triggerBit.ToString(), retCode));
                  string timeName = string.Format("{0}_{1}", inputData.Metadata.NodeNo, "GlassRemoteRejudgeCommandReplyTimeout");
                  if (_timerManager.IsAliveTimer(timeName))
                  {
                      _timerManager.TerminateTimer(timeName);
                  }
                  if (triggerBit == eBitResult.OFF) return;
                  Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                  if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));
                  string err = string.Empty;
                  #region[Return Code]
                  switch (retCode)
                  {
                      case eReturnCode4.Accept: break;
                      case eReturnCode4.NotAcceppt:
                          {
                              err = string.Format("[{0}] EQUIPMENT=[{1}] RETURN_CODE=[1](NotAcceppt).", MethodBase.GetCurrentMethod().Name, eqpNo);
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                              _ExecuteFlag = false;
                              Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID, err });
                          }
                          break;
                      case eReturnCode4.Timeout:
                          {
                              err = string.Format("[{0}] EQUIPMENT=[{1}] RETURN_CODE=[2](Timeout).", MethodBase.GetCurrentMethod().Name, eqpNo);
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                              _ExecuteFlag = false;
                              Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID, err });
                          }
                          break;
                      default:
                          {
                              err = string.Format("[{0}] EQUIPMENT=[{1}] RETURN_CODE=[{2}](UNKNOWN) IS INVALID!",
                                  MethodBase.GetCurrentMethod().Name, eqpNo, inputData.EventGroups[0].Events[0].Items[0].Value);
                              Logger.LogWarnWrite(LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", err);
                              _ExecuteFlag = false;
                              Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID, err });
                          }
                          break;
                  }
                  #endregion
                  #region[Get Word Value]
                  string trxID = string.Format(inputData.Metadata.NodeNo + "_GlassRemoteRejudgeCommand");
                  Trx outputData = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { trxID, false }) as Trx;
                  string glassid = outputData.EventGroups[0].Events[0].Items[0].Value;
                  string portno = outputData.EventGroups[0].Events[0].Items[1].Value;
                  string slotno = outputData.EventGroups[0].Events[0].Items[2].Value;
                  string sideno = outputData.EventGroups[0].Events[0].Items[3].Value;
                  string judgeresult = outputData.EventGroups[0].Events[0].Items[4].Value;
                  #endregion
                  outputData.EventGroups[0].Events[0].IsDisable = true;
                  outputData.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                  outputData.TrackKey = inputData.TrackKey;//本身Log用触发时间记录;
                  #region[Judgeresult]
                  string Judgeresult = string.Empty;
                  if (judgeresult != null)
                  {
                      switch (judgeresult)
                      {

                          case "1":
                              Judgeresult = "OK";
                              break;
                          case "2":
                              Judgeresult = "NG";
                              break;
                          case "3":
                              Judgeresult = "RW";
                              break;
                          case "9":
                              Judgeresult = "RJ";
                              break;
                      }
                  }
                  #endregion
                  SendPLCData(outputData);
                  #region[Get panelinformation]
                  string GLASsID = glassid.Trim();
                  RemoteRejudgePanel Panelinfo = GlassRejudgeSetCommand[GLASsID];
                  string _Trackkey = Panelinfo.TrackKey;
                  #endregion
                  #region[移除此次处理过的]
                  if (GlassRejudgeSetCommand.ContainsKey(GLASsID))
                  {
                      if (GlassRejudgeSetCommand.Keys != null)
                      {
                          Panelinfo.IsFinish = true;
                          GlassRejudgeSetCommand.Remove(GLASsID);
                      }
                  }
                  #endregion
                  _ExecuteFlag = false;
                  GlassRejudgeSetCommand.Remove(GLASsID);
                  Port P = ObjectManager.PortManager.GetPort(eqp.Data.NODEID, portno);
                  Logger.LogInfoWrite(this.LogName, this.GetType().Name, "GlassRemoteRejudgeCommand()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] SET BIT=[OFF].", eqpNo, inputData.TrackKey));
                  string Productspecname = Panelinfo.Productspecname.Trim();
                  string InBoxName = Panelinfo.Inboxname.Trim();
                  Invoke(eServiceName.MESService, "PanelRejudgeResultReply", new object[] { inputData.TrackKey, eqp.Data.LINEID, eqp.Data.NODENO,P.Data.PORTID , GLASsID, slotno, sideno, Judgeresult, ReturnCode, Productspecname, InBoxName });
                  //Trackkey 保持与MES Request一致;
              }
              catch (Exception ex)
              {
                  _ExecuteFlag = false;
                  this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
              }
          }
          private void GlassRemoteRejudgeCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
          {
              try
              {
                  UserTimer timer = subjet as UserTimer;
                  string tmp = timer.TimerId;
                  string trackKey = timer.State.ToString();
                  //机台不回复时,将该笔Dic清除; 
                  string[] sArray = tmp.Split('_');
                  #region[Timeout CUT]
                  string TRXID = string.Format(sArray[0] + "_GlassRemoteRejudgeCommand");
                  Trx outputData = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { TRXID, false }) as Trx;
                  string Glassid = outputData.EventGroups[0].Events[0].Items[0].Value;
                  string Portno = outputData.EventGroups[0].Events[0].Items[1].Value;
                  string Slotno = outputData.EventGroups[0].Events[0].Items[2].Value;
                  string Sideno = outputData.EventGroups[0].Events[0].Items[3].Value;
                  string Judgeresult = outputData.EventGroups[0].Events[0].Items[4].Value;
                  string GLASSID = Glassid.Trim();
                  RemoteRejudgePanel Panelinformation = GlassRejudgeSetCommand[GLASSID];
                  Panelinformation.IsFinish = true;
                  GlassRejudgeSetCommand.Remove(GLASSID);
                  #endregion
                  _ExecuteFlag = false;
                  string TraCkey = Panelinformation.TrackKey;
                  string EQPNO = sArray[0];
                  string REturncode = "2";
                  Equipment EQP = ObjectManager.EquipmentManager.GetEQP(EQPNO);
                  Port PORT = ObjectManager.PortManager.GetPort(EQP.Data.NODEID, Portno);
                  string ProductSpecname = Panelinformation.Productspecname.Trim();
                  string INBOXname = Panelinformation.Inboxname.Trim();
                  Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] Glass Remote Rejudge Command Reply Timeout ,SET BIT=[OFF].", sArray[0], trackKey));
                  Invoke(eServiceName.MESService, "PanelRejudgeResultReply", new object[] { TraCkey, EQP.Data.LINEID, EQP.Data.NODENO, PORT.Data.PORTID, Glassid, Slotno, Sideno, Judgeresult, REturncode, ProductSpecname, INBOXname });
              }
              catch (Exception ex)
              {
                  _ExecuteFlag = false;
                  this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
              }
          }
          #endregion
          //Add By Yangzhenteng20180917
          #region [BUR Panel Scrap]
          public void PanelScrapReport(Trx inputData)
          {
              try
              {
                  if (inputData.IsInitTrigger) return;
                  #region[Get EQP & LINE]
                  string Error = string.Empty;
                  Equipment eqp = ObjectManager.EquipmentManager.GetEQP(inputData.Metadata.NodeNo);
                  if (eqp == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_EQP, inputData.Metadata.NodeNo));
                  Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                  if (line == null) throw new Exception(string.Format(eLOG_CONSTANT.CAN_NOT_FIND_LINE, eqp.Data.LINEID));
                  if (eqp.File.CIMMode == eBitResult.OFF)
                  {
                      Error = string.Format("[{0}] EQUIPMENT=[{1}] CIM_MODE=[OFF], Can't Send Panel Scrap Report!", MethodBase.GetCurrentMethod().Name, eqp.Data.NODENO);
                      Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { inputData.TrackKey, eqp.Data.LINEID, Error });
                      LogWarn(MethodBase.GetCurrentMethod().Name + "()", Error);
                      return;
                  }
                  #endregion
                  #region [PLCAgent Data Bit]
                  eBitResult bitResult = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                  #endregion
                  #region[If Bit Off->Return]
                  if (bitResult == eBitResult.OFF)
                  {
                      Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                          string.Format(eLOG_CONSTANT.EQP_REPORT_BIT_OFF, inputData.Metadata.NodeNo, inputData.TrackKey));
                      PanelScrapReportReply(inputData.Metadata.NodeNo, eBitResult.OFF, inputData.TrackKey);
                      return;
                  }
                  #endregion
                  string GlassID = inputData.EventGroups[0].Events[0].Items[0].Value.Trim();
                  string Position = inputData.EventGroups[0].Events[0].Items[1].Value;
                  string JobJudge = inputData.EventGroups[0].Events[0].Items[2].Value;
                  string JobGrade = inputData.EventGroups[0].Events[0].Items[3].Value;
                  string ReasonCode = inputData.EventGroups[0].Events[0].Items[4].Value;
                  Job _Job = ObjectManager.JobManager.GetJob(GlassID);
                  Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] GlassID=[{3}] Position=[{4}] JobJudge=[{5}] JobGrade=[{6}] ReasonCode=[{7}]",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, GlassID, Position, JobJudge, JobGrade, ReasonCode));
                  PanelScrapReportReply(inputData.Metadata.NodeNo, eBitResult.ON, inputData.TrackKey);
                  #region Send MES Data
                  object[] _data = new object[5]
                    { 
                        inputData.TrackKey,
                        eqp.Data.LINEID,  
                        eqp, 
                        _Job,
                        ReasonCode,
                    };
                  object retVal = base.Invoke(eServiceName.MESService, "ProductScrapped", _data);
                  #endregion
              }
              catch (Exception ex)
              {
                  this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                  if (inputData.EventGroups[0].Events[1].Items[0].Value.Equals("1"))
                      PanelScrapReportReply(inputData.Name.Split('_')[0], eBitResult.ON, inputData.TrackKey);
              }
          }
          public void PanelScrapReportReply(string eqpNo, eBitResult value, string trackKey)
          {
              try
              {
                  Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(eqpNo + "_" + "PanelScrapReportReply") as Trx;
                  outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                  outputdata.TrackKey = trackKey;
                  SendPLCData(outputdata);
                  string timeName = string.Format(eqpNo + "_" + "PanelScrapReportReplyTimeout");
                  if (_timerManager.IsAliveTimer(timeName))
                  {
                      _timerManager.TerminateTimer(timeName);
                  }
                  if (value.Equals(eBitResult.ON))
                  {
                      _timerManager.CreateTimer(timeName, false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(PanelScrapReportReplyTimeout), trackKey);
                  }
                  Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] ,SET BIT=[{2}].",
                   eqpNo, trackKey, value.ToString()));
              }
              catch (Exception ex)
              {
                  this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
              }
          }
          private void PanelScrapReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
          {
              try
              {
                  UserTimer timer = subjet as UserTimer;
                  string tmp = timer.TimerId;
                  string trackKey = timer.State.ToString();
                  string[] sArray = tmp.Split('_');
                  Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                         string.Format("[EQUIPMENT={0}] [BCS -> EQP][{1}] Panel Scrap Report Reply Timeout ,SET BIT=[OFF].", sArray[0], trackKey));
                  PanelScrapReportReply(sArray[0], eBitResult.OFF, trackKey);
              }
              catch (Exception ex)
              {
                  this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
              }
          }
          #endregion
          //Add By Yangzhenteng For BEOL Loader Check20181212
          #region[ProductCountSendCommand]
          public void ProductCountSendCommand(string eqpNo, string ProductCount, string PortNo, string trackKey)
          {
              try
              {
                  string err = string.Empty;
                  Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                  if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));
                  #region CIM MODE OFF 不能改
                  if (eqp.File.CIMMode == eBitResult.OFF)
                  {
                      err = string.Format("[{0}] EQUIPMENT=[{1}] CIM_MODE=[OFF], Can NOT Send Producndt Count Send Command!", MethodBase.GetCurrentMethod().Name, eqp.Data.NODENO);
                      Invoke(eServiceName.UIService, "BCSTerminalMessageInform", new object[] { trackKey, eqp.Data.LINEID, err });
                      LogWarn(MethodBase.GetCurrentMethod().Name +"#"+PortNo+ "()", err);
                      return;
                  }
                  #endregion
                  Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format(eqpNo + "_" + "ProductCountSendCommand"+"#"+PortNo)) as Trx;
                  outputData.EventGroups[0].Events[0].Items[0].Value = PortNo;
                  outputData.EventGroups[0].Events[0].Items[1].Value = ProductCount;
                  outputData.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                  outputData.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                  outputData.TrackKey = trackKey;
                  SendPLCData(outputData);
                  if (PortNo == "01")
                  {
                      lock (eqp.File)
                      {
                          eqp.File.Port01ProductCountCommandSendTime = DateTime.Now;
                          eqp.File.Port01ProductCountCommandReplyFlag = false;
                          eqp.File.Port01ProductCountCommandReplyJobCount = string.Empty;
                          ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                      }                  
                  }
                  if (PortNo == "02")
                  {
                      lock (eqp.File)
                      {
                          eqp.File.Port02ProductCountCommandSendTime = DateTime.Now;
                          eqp.File.Port02ProductCountCommandReplyFlag = false;
                          eqp.File.Port02ProductCountCommandReplyJobCount = string.Empty;
                          ObjectManager.EquipmentManager.EnqueueSave(eqp.File);
                      }
                  }
                  string timeName = string.Format(eqpNo + "_" + "ProductCountSendCommandReplyTimeout"+"#"+PortNo, eqpNo);

                  if (_timerManager.IsAliveTimer(timeName))
                  {
                      _timerManager.TerminateTimer(timeName);
                  }
                  _timerManager.CreateTimer(timeName, false, ParameterManager["ProductCountCheckTimer"].GetInteger(), new System.Timers.ElapsedEventHandler(ProductCountSendCommandReplyTimeout), outputData.TrackKey);
                  Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name +"#"+PortNo+ "()",
                          string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] ProductCount=[{2}],SET BIT=[ON].", eqp.Data.NODENO,
                          outputData.TrackKey, ProductCount));
              }
              catch (Exception ex)
              {
                  this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
              }
          }
          public void ProductCountSendCommandReply(Trx inputData)
          {
              try
              {
                  if (inputData.IsInitTrigger) return;
                  string eqpNo = inputData.Metadata.NodeNo;
                  string commandNo = string.Empty;
                  if (inputData.Name.Split(new char[] { '#' }).Length == 2)
                      commandNo = inputData.Name.Split(new char[] { '#' })[1];
                  eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                  string PortNo = inputData.EventGroups[0].Events[0].Items[0].Value.PadLeft(2,'0');
                  string ProductCount = inputData.EventGroups[0].Events[0].Items[1].Value.Trim();                
                  Equipment _EQP = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                  Line _Line = ObjectManager.LineManager.GetLine(_EQP.Data.LINEID);
                  switch(PortNo)
                  {
                      case "01":
                      #region[Port01]
                      Port Port = ObjectManager.PortManager.GetPort(_Line.Data.LINEID, _EQP.Data.NODENO, PortNo);
                      if (Port == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", PortNo));
                      if (triggerBit == eBitResult.ON)
                      {
                          lock (_EQP.File)
                          {
                              _EQP.File.Port01ProductCountCommandReplyFlag = true;
                              _EQP.File.Port01ProductCountCommandReplyJobCount = ProductCount;
                              ObjectManager.EquipmentManager.EnqueueSave(_EQP.File);
                          }
                          lock (Port.File)
                          {
                              Port.File.JobCountInCassette = inputData.EventGroups[0].Events[0].Items[2].Value;
                              Port.File.JobExistenceSlot = inputData.EventGroups[0].Events[0].Items[3].Value;
                              ObjectManager.PortManager.EnqueueSave(Port.File);
                          }
                      }
                      break;  
                    #endregion
                      case "02":
                      #region[Port02]
                      Port _Port = ObjectManager.PortManager.GetPort(_Line.Data.LINEID, _EQP.Data.NODENO, PortNo);
                      if (_Port == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", PortNo));
                      if (triggerBit == eBitResult.ON)
                      {
                          lock (_EQP.File)
                          {
                              _EQP.File.Port02ProductCountCommandReplyFlag = true;
                              _EQP.File.Port02ProductCountCommandReplyJobCount = ProductCount;
                              ObjectManager.EquipmentManager.EnqueueSave(_EQP.File);
                          }
                          lock (_Port.File)
                          {
                              _Port.File.JobCountInCassette = inputData.EventGroups[0].Events[0].Items[2].Value;
                              _Port.File.JobExistenceSlot = inputData.EventGroups[0].Events[0].Items[3].Value;
                              ObjectManager.PortManager.EnqueueSave(_Port.File);
                          }
                      }
                      break;
                    #endregion
                  }
                  Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name+"#"+commandNo + "()",
                      string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] BIT=[{2}] Product Count=[{3}] PortNo=[{4}].",
                      eqpNo, inputData.TrackKey, triggerBit.ToString(), ProductCount, PortNo));
                  string timeName = string.Format(eqpNo + "_" + "ProductCountSendCommandReplyTimeout" + "#" + commandNo);
                  if (_timerManager.IsAliveTimer(timeName))
                  {
                      _timerManager.TerminateTimer(timeName);
                  }
                  if (triggerBit == eBitResult.OFF) return;
                  #region [Command Off]
                  Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format(eqpNo + "_" + "ProductCountSendCommand" + "#" + commandNo)) as Trx;
                  outputData.EventGroups[0].Events[0].IsDisable = true;
                  outputData.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                  outputData.TrackKey = inputData.TrackKey;
                  SendPLCData(outputData);
                  Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name +"#"+commandNo + "()",
                      string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] SET BIT=[OFF].", eqpNo, inputData.TrackKey));
                  #endregion
              }
              catch (Exception ex)
              {
                  this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
              }
          }
          private void ProductCountSendCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
          {
              try
              {
                  UserTimer timer = subjet as UserTimer;
                  string tmp = timer.TimerId;
                  string trackKey = timer.State.ToString();
                  string[] sArray = tmp.Split('_');
                  string[] _Sarray = tmp.Split('#');
                  string PortID = _Sarray[1];
                  Equipment _eqp = ObjectManager.EquipmentManager.GetEQP(sArray[0]);
                  Line _line = ObjectManager.LineManager.GetLine(_eqp.Data.LINEID);
                  string timeName = string.Format(sArray[0] + "_" + "ProductCountSendCommandReplyTimeout"+"#"+_Sarray[1]);
                  if (_timerManager.IsAliveTimer(timeName))
                  {
                      _timerManager.TerminateTimer(timeName);
                  }
                  Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format(sArray[0] + "_" + "ProductCountSendCommand"+"#"+_Sarray[1])) as Trx;
                  outputdata.EventGroups[0].Events[0].IsDisable = true;
                  outputdata.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.OFF).ToString();
                  outputdata.TrackKey = trackKey;
                  SendPLCData(outputdata);
                  switch (PortID)
                  { 
                      case "01":                         
                      lock (_eqp.File)
                      {
                          _eqp.File.Port01ProductCountCommandReplyFlag = false;
                          _eqp.File.Port01ProductCountCommandReplyJobCount = string.Empty;
                          ObjectManager.EquipmentManager.EnqueueSave(_eqp.File);
                      }
                      break;
                      case "02":
                      lock (_eqp.File)
                      {
                          _eqp.File.Port02ProductCountCommandReplyFlag = false;
                          _eqp.File.Port02ProductCountCommandReplyJobCount = string.Empty;
                          ObjectManager.EquipmentManager.EnqueueSave(_eqp.File);
                      }
                      break;
                  }
                  Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name +"#"+_Sarray[1] + "()",
                      string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] PRODUCT COUNT SEND COMMAND REPLY TIMEOUT, SET BIT=[OFF].", sArray[0], trackKey));
              }
              catch (Exception ex)
              {
                  this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
              }
          }
          #endregion
      }
}
