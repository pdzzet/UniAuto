using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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

namespace UniAuto.UniBCS.CSOT.CommonService
{
    public class ProcessDataService : AbstractService
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
        private class EDCFile
        {
            public class EDCItem
            {
                public string Name { get; set; }
                public string Value { get; set; }
                public EDCItem()
                {
                    Name = Value = string.Empty;
                }
                public EDCItem(string name, string value)
                {
                    Name = name;
                    Value = value;
                }
            }
            /// <summary>
            /// EDC File Format中, UNIT的個數
            /// </summary>
            public static readonly int EDC_UNIT_COUNT = 15;
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
            /// Dictionary[key, value], 檔案拆解後的全部Item與Value, 包括Head與EDC item
            /// </summary>
            public Dictionary<string, string> Data { get; private set; }
            public string CassetteSeqNo
            {
                get
                {
                    if (Data.ContainsKey("Cassette_Sequence_No"))
                        return Data["Cassette_Sequence_No"].TrimStart('0');
                    return string.Empty;
                }
            }
            public string JobSeqNo
            {
                get
                {
                    if (Data.ContainsKey("Job_Sequence_No"))
                        return Data["Job_Sequence_No"].TrimStart('0');
                    return string.Empty;
                }
            }
            public int LocalProcessingTime
            {
                get
                {
                    if (Data.ContainsKey("Local_Processing_Time"))
                    {
                        int tmp = 0;
                        if (int.TryParse(Data["Local_Processing_Time"], out tmp))
                            return tmp;
                    }
                    return 0;
                }
            }
            public string LocalProcessStartTime
            {
                get
                {
                    if (Data.ContainsKey("Local_Process_Start_Time"))
                    {
                        if (Data["Local_Process_Start_Time"].Length == "yyyyMMddHHmmss".Length)
                            return Data["Local_Process_Start_Time"].Substring(2);
                        else
                            return Data["Local_Process_Start_Time"];
                    }
                    return string.Empty;
                }
            }
            public string LocalProcessEndTime
            {
                get
                {
                    if (Data.ContainsKey("Local_Process_End_Time"))
                    {
                        if (Data["Local_Process_End_Time"].Length == "yyyyMMddHHmmss".Length)
                            return Data["Local_Process_End_Time"].Substring(2);
                        else
                            return Data["Local_Process_End_Time"];
                    }
                    return string.Empty;
                }
            }
            public int EDCItemCount
            {
                get
                {
                    if (Data.ContainsKey("EDC_Item_Count"))
                    {
                        int count = 0;
                        if (int.TryParse(Data["EDC_Item_Count"], out count))
                        {
                            if (count < 0) count = 0;
                            return count;
                        }
                    }
                    return 0;
                }
            }
            /// <summary>
            /// 檔案拆解後的全部EDC item
            /// </summary>
            public List<EDCItem> EDCItemList { get; private set; }
            public EDCFile(string nodeNo, string filename, string glassID, string dateTimeStr)
            {
                NodeNo = nodeNo;
                Filename = filename;
                GlassID = glassID;
                DateTimeStr = dateTimeStr;
                Data = new Dictionary<string, string>();
                EDCItemList = new List<EDCItem>();
            }
            public string GetParaList()
            {
                StringBuilder ret = new StringBuilder();//key=value,key=value,key=value
                foreach (EDCItem edc_item in EDCItemList)
                {
                    ret.AppendFormat("{0}={1},", edc_item.Name, edc_item.Value);
                }
                if (ret.Length > 0)
                    ret.Remove(ret.Length - 1, 1);//移除最後一個逗號
                return ret.ToString();
            }
            public int GetUnitProcessingTime(string unitNo)
            {
                string key = string.Format("Unit#{0}_Processing_Time", unitNo.PadLeft(2, '0'));
                if (Data.ContainsKey(key))
                {
                    int tmp = 0;
                    if (int.TryParse(Data[key], out tmp))
                        return tmp;
                }
                return 0;
            }
            public string GetUnitProcessStartTime(string unitNo)
            {
                string key = string.Format("Unit#{0}_Process_Start_Time", unitNo.PadLeft(2, '0'));
                if (Data.ContainsKey(key))
                    return Data[key];
                return string.Empty;
            }
            public string GetUnitProcessEndTime(string unitNo)
            {
                string key = string.Format("Unit#{0}_Process_End_Time", unitNo.PadLeft(2, '0'));
                if (Data.ContainsKey(key))
                    return Data[key];
                return string.Empty;
            }
            public static EDCFile Load(Log.ILogManager logger, string logname, string nodeNo, string filename)
            {
                EDCFile ret = null;
                StreamReader sr = null;
                string fname = string.Empty;
                try
                {
                    fname = Path.GetFileNameWithoutExtension(filename);

                    logger.LogInfoWrite(logname, "ProcessDataService", "EDCFile.Load()", string.Format("[EQUIPMENT={0}] [FileName={1}]", nodeNo, fname));

                    string ext = Path.GetExtension(filename);
                    string[] tmp = fname.Split('_');
                    if (tmp != null && tmp.Length == 3 && string.Compare(tmp[1], "EDC", true) == 0 && string.Compare(ext, ".dat", true) == 0)
                    {
                        sr = new StreamReader(filename);
                        ret = new EDCFile(nodeNo, filename, tmp[0], tmp[2]);
                        #region read file
                        {
                            while (!sr.EndOfStream)
                            {
                                // File Data Example
                                //Cassette_Sequence_No=01001
                                //Job_Sequence_No=00028
                                //Glass_ID=TA080001AA
                                //Job_Data=03E9 001C 006F 00DE A028 2041 4154 3830 3030
                                //Local_Processing_Time=180
                                //Local_Process_Start_Time=20140922092930
                                //Local_Process_End_Time=20140922101045
                                //Unit#01_Processing_Time=180
                                //Unit#01_Process_Start_Time=20140922092930
                                //Unit#01_Process_End_Time=20140922101045
                                //EDC_Item_Count=2
                                //Temp1_Site4_EdgeName=CD4
                                //Temp1_Site4_USL=999.000

                                string str = sr.ReadLine();
                                tmp = str.Split('=');
                                if (tmp != null && tmp.Length == 2)
                                {
                                    tmp[0] = tmp[0].Trim();
                                    tmp[1] = tmp[1].Trim();
                                    if (ret.Data.ContainsKey(tmp[0]))
                                        throw new Exception(string.Format("File[{0}] format Error, Key[{1}] is duplicated", fname, tmp[0]));

                                    ret.Data.Add(tmp[0], tmp[1]);
                                    if (tmp[0].IndexOf("EDC_Item#") == 0)
                                    {
                                        string[] t = tmp[1].Split(',');
                                        if (t != null && t.Length == 2)
                                        {
                                            ret.EDCItemList.Add(new EDCItem(t[0], t[1]));
                                        }
                                        else
                                            throw new Exception(string.Format("File[{0}] EDC data format Error, [{1}]", fname, str));
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
                                key_in_header.Add("Job_Data");
                                key_in_header.Add("Local_Process_Start_Time");
                                key_in_header.Add("Local_Process_End_Time");
                                for (int i = 0; i < EDCFile.EDC_UNIT_COUNT; i++)
                                {
                                    key_in_header.Add(string.Format("Unit#{0}_Process_Start_Time", (i + 1).ToString().PadLeft(2, '0')));
                                    key_in_header.Add(string.Format("Unit#{0}_Process_End_Time", (i + 1).ToString().PadLeft(2, '0')));
                                }
                            }
                            #endregion

                            List<string> key_in_header_int = new List<string>();
                            #region key_in_header_int
                            {
                                key_in_header_int.Add("Cassette_Sequence_No");
                                key_in_header_int.Add("Job_Sequence_No");
                                key_in_header_int.Add("Local_Processing_Time");
                                for (int i = 0; i < EDCFile.EDC_UNIT_COUNT; i++)
                                {
                                    key_in_header.Add(string.Format("Unit#{0}_Processing_Time", (i + 1).ToString().PadLeft(2, '0')));
                                }
                                key_in_header_int.Add("EDC_Item_Count");
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
                                if (ret.EDCItemCount != ret.EDCItemList.Count)
                                    throw new Exception(string.Format("EDC_Item_Count[{0}] is different from EDC Body[{1}] in file.", ret.EDCItemCount, ret.EDCItemList.Count));
                            }
                            #endregion
                        }
                        #endregion
                    }
                }
                catch(Exception ex)
                {
                    ret = null;
                    logger.LogErrorWrite(logname, "ProcessDataService", string.Format("EDCFile.Load({0})", fname), ex);
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
            /// <summary>
            /// 檔名格式是否為EDC File
            /// </summary>
            /// <param name="glassID"></param>
            /// <param name="filename">完整路徑檔名</param>
            /// <returns>true:表示檔名格式為EDC File</returns>
            public static bool IsEDCFile(string glassID, string filename)
            {
                bool ret = false;
                string file_name = Path.GetFileName(filename);
                if (string.Compare(Path.GetExtension(file_name), ".dat", true) == 0)
                {
                    if ((string.IsNullOrEmpty(glassID) && file_name.IndexOf("_EDC_") > 0) ||
                        file_name.IndexOf(string.Format("{0}_EDC_", glassID)) == 0)
                    {
                        ret = true;
                    }
                }
                return ret;
            }
            /// <summary>
            /// 取檔名中的日期, yyyyMMdd
            /// </summary>
            /// <param name="filename">完整路徑檔名或僅只檔名</param>
            /// <returns>檔名中的日期, yyyyMMdd</returns>
            public static string GetDateStrFromFilename(string filename)
            {
                string file_name = Path.GetFileNameWithoutExtension(filename);
                int idx = file_name.IndexOf("_EDC_");
                return file_name.Substring(idx + "_EDC_".Length).Substring(0, "yyyyMMdd".Length);
            }
        }

        private const string Key_ProcessDataReportReply = "{0}_ProcessDataReportReply";
        private const string Key_ProcessDataRequestCommand = "{0}_ProcessDataRequestCommand";
        private const string Key_ProcessDataReportTimeout = "{0}_ProcessDataReportTimeout";
        private const string Key_ProcessDataRequestCommandTimeout = "{0}_ProcessDataRequestCommandTimeout";

        /// <summary>
        /// Dictionary[NodeNo, FTPInfo]
        /// </summary>
        private SortedDictionary<string, BaseInfo> dicReportBy = new SortedDictionary<string, BaseInfo>();

        private bool _runThread = false;
        private FABTYPE _fabType = FABTYPE.UNKNOWN;

        /// <summary>
        /// Config File 
        /// </summary>
        public string ConfigFileName { get; set; }

        public override bool Init()
        {
            try
            {
                bool ftp = false, share_folder = false;
                #region 讀取 \Config\Agent\IO\EDCbyFTPSetting.xml
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(ConfigFileName);
                    XmlNodeList server_list = doc.SelectNodes("//ROOT/ProcessData/ServerName");
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
                if (Workbench.ServerName.Length > 2)
                {
                    string prefix = Workbench.ServerName.Substring(0, 2).ToUpper();
                    switch(prefix)
                    {
                    case "TB": _fabType = FABTYPE.ARRAY; break;
                    case "CB": _fabType = FABTYPE.CELL; break;
                    case "FB": _fabType = FABTYPE.CF; break;
                            //shihyang 20150823 add
                    case "TC": _fabType = FABTYPE.ARRAY; break;
                    case "CC": _fabType = FABTYPE.CELL; break;
                    case "FC": _fabType = FABTYPE.CF; break;
                    }
                }
                if(_fabType == FABTYPE.CELL)
                {
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
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(LogName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", ex);
                throw ex;
            }
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

        protected string Convert_DateTimeFormat_BCD(string aPreYear, string aODateTime)
        {
            string strDateTime = "";
            //"MMyyHHddssmm";
            if (aODateTime == "") return strDateTime;
            string strMonth = aODateTime.Substring(0, 2);
            string strYear = aODateTime.Substring(2, 2);
            string strHour = aODateTime.Substring(4, 2);
            string strDay = aODateTime.Substring(6, 2);
            string strSecond = aODateTime.Substring(8, 2);
            string strMinute = aODateTime.Substring(10, 2);
            
            strDateTime = aPreYear + strYear + strMonth + strDay + strHour + strMinute + strSecond;

            return strDateTime;
        }

        ////TODO:提供方法，是否有参数?
        //public void ProcessDataReload()
        //{
        //    try
        //    {
        //        bool done = ObjectManager.ProcessDataManager.ReloadAll();
        //        if (done)
        //        {
        //            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "[OPI -> BCS] Process Data Reload OK.");
        //        }
        //        else
        //        {
        //            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "[OPI -> BCS] Process Data Reload NG.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //    finally
        //    {
        //    }
        //}
        public void ProcessDataReport(Trx inputData)
        {
            if (inputData == null || inputData.IsInitTrigger)
                return;

            try
            {
                //Jun Add 20150228 因為Process 結構不同，所以需要判斷Bit所在的Events
                eBitResult triggerBit = eBitResult.OFF;
                if (inputData.EventGroups[0].Events.Count == 2)
                    triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);
                else if (inputData.EventGroups[0].Events.Count == 3)
                    triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[2].Items[0].Value);
                else
                    triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[inputData.EventGroups[0].Events.Count-1].Items[0].Value); // sy add 20160525

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}] BIT =[{2}]", inputData.Metadata.NodeNo, inputData.TrackKey, triggerBit.ToString()));

                eBitResult reply = (triggerBit == eBitResult.ON) ? eBitResult.ON : eBitResult.OFF;
                ProcessDataReportReply(inputData.Metadata.NodeNo, reply, inputData.TrackKey);

                if (triggerBit == eBitResult.OFF)
                    return;
                
                try
                {
                    if (dicReportBy.ContainsKey(inputData.Metadata.NodeNo))
                    {
                        if (_fabType == FABTYPE.ARRAY || _fabType == FABTYPE.CF)
                        {
                            if (dicReportBy[inputData.Metadata.NodeNo] is FTPInfo)
                            {
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [TrackKey={1}] ProcessDataReport_ByFTP", inputData.Metadata.NodeNo, inputData.TrackKey));

                                ProcessDataReport_ByFTP(inputData, (FTPInfo)dicReportBy[inputData.Metadata.NodeNo]);
                            }
                            else if (dicReportBy[inputData.Metadata.NodeNo] is ShareFolderInfo)
                            {
                                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [TrackKey={1}] ProcessDataReport_ByShareFolder", inputData.Metadata.NodeNo, inputData.TrackKey));

                                ProcessDataReport_ByShareFolder(inputData, (ShareFolderInfo)dicReportBy[inputData.Metadata.NodeNo]);
                            }
                            else
                            {
                                //Config錯誤, 不知是以何種方式上報, 不做事
                                Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                    string.Format("[EQUIPMENT={0}] [TrackKey={1}] Unrecognized Report type in ProcessDataService Config file", inputData.Metadata.NodeNo, inputData.TrackKey));
                            }
                        }
                        else if (_fabType == FABTYPE.CELL)
                        {
                            // 在CELL廠, 是由BC定時去資料夾抓EDC檔並上報MES,OEE
                            // 機台有設定以EDC File上報ProcessData, 且當機台上報ProcessDataReport BitOn, 一樣做ProcessDataReport_ByPLC
                            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [TrackKey={1}] ProcessDataReport_ByPLC", inputData.Metadata.NodeNo, inputData.TrackKey));

                            ProcessDataReport_ByPLC(inputData);
                        }
                    }
                    else
                    {
                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                string.Format("[EQUIPMENT={0}] [TrackKey={1}] ProcessDataReport_ByPLC", inputData.Metadata.NodeNo, inputData.TrackKey));

                        ProcessDataReport_ByPLC(inputData);
                    }
                }
                catch (Exception exception)
                {
                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", exception);
                }
            }
            catch (Exception ex)
            {
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ProcessDataReport_ByFTP(Trx inputData, FTPInfo info)
        {
            string node_no = inputData.Metadata.NodeNo;
            string glassID = inputData.EventGroups[0].Events[0].Items[2].Value.Trim();
            List<string> local_files = DownloadGlassFileFromFtp(info, glassID);
            List<EDCFile> edc_files = new List<EDCFile>();
            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Download [{0}] files from FTP, FTP=[{1}], DeleteFile=[{2}]", local_files.Count, info.SourcePath, info.DeleteRemoteFile));

            foreach (string local_fname in local_files)
            {
                try
                {
                    EDCFile edc_file = EDCFile.Load(Logger, this.LogName, node_no, local_fname);
                    if (edc_file != null)
                    {
                        edc_files.Add(edc_file);
                        ProcessDataReport_ByEDCFile(inputData.Metadata.NodeNo, inputData.TrackKey, edc_file);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }

            #region 找出最新的 EDC File 存入Repository, 以供 OPI 查詢 Process Data
            {
                EDCFile newest_edc_file = null;
                foreach (EDCFile edc_file in edc_files)
                {
                    if (newest_edc_file == null)
                        newest_edc_file = edc_file;
                    else
                    {
                        if (string.Compare(edc_file.DateTimeStr, newest_edc_file.DateTimeStr) > 0)
                            newest_edc_file = edc_file;
                    }
                }
                if (newest_edc_file != null)
                {
                    Repository.Add(inputData.Name, newest_edc_file);
                }
            }
            #endregion
        }

        private void ProcessDataReport_ByShareFolder(Trx inputData, ShareFolderInfo info)
        {
            string node_no = inputData.Metadata.NodeNo;
            string glassID = inputData.EventGroups[0].Events[0].Items[2].Value.Trim();
            List<string> local_files = DownloadGlassFileFromShareFolder(info, glassID);
            List<EDCFile> edc_files = new List<EDCFile>();
            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Download [{0}] files from ShareFolder, ShareFolder=[{1}], DeleteFile=[{2}]", local_files.Count, info.SourcePath, info.DeleteRemoteFile));

            foreach (string local_fname in local_files)
            {
                try
                {
                    EDCFile edc_file = EDCFile.Load(Logger, this.LogName, node_no, local_fname);
                    if (edc_file != null)
                    {
                        edc_files.Add(edc_file);
                        ProcessDataReport_ByEDCFile(inputData.Metadata.NodeNo, inputData.TrackKey, edc_file);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                }
            }

            #region 找出最新的 EDC File 存入Repository, 以供 OPI 查詢 Process Data
            {
                EDCFile newest_edc_file = null;
                foreach (EDCFile edc_file in edc_files)
                {
                    if (newest_edc_file == null)
                        newest_edc_file = edc_file;
                    else
                    {
                        if (string.Compare(edc_file.DateTimeStr, newest_edc_file.DateTimeStr) > 0)
                            newest_edc_file = edc_file;
                    }
                }
                if (newest_edc_file != null)
                {
                    Repository.Add(inputData.Name, newest_edc_file);
                }
            }
            #endregion
        }

        private void ProcessDataReport_ByEDCFile(string nodeNo, string trackKey, EDCFile edcFile)
        {
            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("[EQUIPMENT={0}] [TrackKey={1}] [CstSeqNo={2}] [JobSeqNo={3}] [GlsID={4}]", nodeNo, trackKey, edcFile.CassetteSeqNo, edcFile.JobSeqNo, edcFile.GlassID));

            Equipment eqp = ObjectManager.EquipmentManager.GetEQP(edcFile.NodeNo);
            if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", edcFile.NodeNo));

            Job job = ObjectManager.JobManager.GetJob(edcFile.CassetteSeqNo, edcFile.JobSeqNo);
            if (job == null)
            {
                //throw new Exception(string.Format("CAN'T FIND JOB, CASSETTE SEQUENCENO=[{0}],JOB SEQUENCE NO=[{1}] IN JOBENTITY!", edcFile.CassetteSeqNo, edcFile.JobSeqNo));
                Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                       string.Format("EQUIPMENT=[{0}] CAN'T FIND JOB DATA, CST_SEQNO=[{1}] JOB_SEQNO=[{2}] CAN'T REPORT PROCESS DATA!!",
                           eqp.Data.NODENO, edcFile.CassetteSeqNo, edcFile.JobSeqNo));
                return;
            }

            IList<ProductProcessData.ITEMc> mes_item_list = new List<ProductProcessData.ITEMc>();
            Dictionary<string, List<string>> eda_item_list = new Dictionary<string, List<string>>();
            object[] _dataMES = new object[12]
            #region _dataMES
            { 
                trackKey, /*0 TrackKey*/
                eqp.Data.LINEID,    /*1 LineName*/
                eqp.Data.NODEID,    /*2 MachineName*/
                "",     /*3 UnitName*/
                (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].LOTNAME),   /*4 LotName*/
                (job.FromCstID == null ? "" : job.FromCstID),     /*5 CarrierName*/
                (job.GlassChipMaskBlockID == null ? "" : job.GlassChipMaskBlockID),     /*6 ProductName*/
                (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME),   /*7 ProductSpecName*/
                (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].PRODUCTSPECVER),   /*8 ProductSpecVer*/
                (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME),     /*9 ProcessOperationName*/
                (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].LINERECIPENAME),   /*10 LineRecipeName*/
                mes_item_list   /*11 ItemList*/
            };
            #endregion

            object[] _dataEDA = new object[8]
            #region _dataEDA
            {
                trackKey,  /*0 TrackKey*/
                eqp.Data.LINEID,    /*1 LineName*/
                eqp.Data.NODEID,    /*2 MachineName*/
                edcFile.LocalProcessStartTime,    /*3 EQP Start Time*/
                edcFile.LocalProcessEndTime,    /*4 EQP End Time*/
                DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),    /*5 Report Time*/
                job,                 /*6 no Job*/
                eda_item_list             
            };
            #endregion

            IList<ProcessData> processDatas = ObjectManager.ProcessDataManager.GetProcessData(edcFile.NodeNo);
            foreach (EDCFile.EDCItem edc_item in edcFile.EDCItemList)
            {
                bool add = (processDatas == null || processDatas.Count == 0);
                #region checking edc item report to mes
                {
                    if (processDatas != null)
                    {
                        foreach (ProcessData pd in processDatas)
                        {
                            if (string.Compare(pd.Data.PARAMETERNAME, edc_item.Name, true) == 0 &&
                                pd.Data.REPORTTO.IndexOf("MES") >= 0)
                            {
                                add = true;
                                break;
                            }
                        }
                    }
                }
                #endregion
                if (!add && _fabType == FABTYPE.CELL && Workbench.LineType == "CBODF")
                    add = true;//CELL ODF的ProcessDataByFile, 上報時不看SBRM_PROCESSDATA, File內的Item全部上報至EDA及MES
                if (add)
                {
                    ProductProcessData.ITEMc item = new ProductProcessData.ITEMc();
                    item.ITEMNAME = edc_item.Name;
                    item.SITELIST.Add(new ProductProcessData.SITEc());
                    item.SITELIST[0].SITENAME = "DEFAULT";
                    item.SITELIST[0].SITEVALUE = edc_item.Value;
                    mes_item_list.Add(item);
                }
            }

            foreach (EDCFile.EDCItem edc_item in edcFile.EDCItemList)
            {
                bool add = (processDatas == null || processDatas.Count == 0);
                #region checking edc item report to eda
                {
                    if (processDatas != null)
                    {
                        foreach (ProcessData pd in processDatas)
                        {
                            if (string.Compare(pd.Data.PARAMETERNAME, edc_item.Name, true) == 0 &&
                                pd.Data.REPORTTO.IndexOf("EDA") >= 0)
                            {
                                add = true;
                                break;
                            }
                        }
                    }
                }
                #endregion
                if (!add && _fabType == FABTYPE.CELL && Workbench.LineType == "CBODF")
                    add = true;//CELL ODF的ProcessDataByFile, 上報時不看SBRM_PROCESSDATA, File內的Item全部上報至EDA及MES
                if (add)
                {
                    if (!eda_item_list.ContainsKey(edc_item.Name))
                    {
                        List<string> tmp = new List<string>();
                        tmp.Add(string.Format("{0};{1}", edc_item.Name, edc_item.Value));
                        eda_item_list.Add(edc_item.Name, tmp);
                    }
                    else
                    {
                        eda_item_list[edc_item.Name].Add(string.Format("{0};{1}", edc_item.Name, edc_item.Value));
                    }
                }
            }

            if (mes_item_list.Count > 0)
                Invoke(eServiceName.MESService, "ProductProcessData", _dataMES);

            if (eda_item_list.Count > 0)
                Invoke(eServiceName.EDAService, "EDAReport", _dataEDA);

            string paraList = edcFile.GetParaList();
            ObjectManager.ProcessDataManager.MakeProcessDataValuesToFile(eqp.Data.NODEID, edcFile.CassetteSeqNo, edcFile.JobSeqNo, trackKey, paraList);

            PROCESSDATAHISTORY paraHistory = new PROCESSDATAHISTORY();
            paraHistory.CASSETTESEQNO = int.Parse(edcFile.CassetteSeqNo);
            paraHistory.JOBSEQNO = int.Parse(edcFile.JobSeqNo);
            paraHistory.JOBID = edcFile.GlassID;
            paraHistory.TRXID = trackKey;
            paraHistory.NODEID = eqp.Data.NODEID;
            paraHistory.UPDATETIME = DateTime.Now;
            paraHistory.FILENAMA = string.Format("{0}_{1}_{2}_{3}", edcFile.CassetteSeqNo, edcFile.JobSeqNo, eqp.Data.NODEID, trackKey);
            paraHistory.PROCESSTIME = edcFile.LocalProcessingTime;
            paraHistory.LOCALPROCESSSTARTTIME = edcFile.LocalProcessStartTime;
            paraHistory.LOCALPROCSSSENDTIME = edcFile.LocalProcessEndTime;
            IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(eqp.Data.NODENO);
            if (units != null && units.Count > 0)
            {
                GetUnitProcessData(paraHistory, edcFile, units);
            }

            ObjectManager.ProcessDataManager.SaveProcessDataHistory(paraHistory);

            Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                string.Format("SaveProcessDataHistory [EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] CST_SEQNO=[{3}] JOB_SEQNO=[{4}] GLASS_ID=[{5}].",
                eqp.Data.NODENO, trackKey, eqp.File.CIMMode, edcFile.CassetteSeqNo, edcFile.JobSeqNo, edcFile.GlassID));
        }

        #region [EDC Report]
        private void ProcessDataReport_ByPLC(Trx inputData)
        {
            string eqpNo = inputData.Metadata.NodeNo;
            string currentPreYear = DateTime.Now.Year.ToString().Substring(0, 2);
            string cassetteSequenceNo = inputData.EventGroups[0].Events[0].Items[0].Value;
            string jobSequenceNo = inputData.EventGroups[0].Events[0].Items[1].Value;
            string glassID = inputData.EventGroups[0].Events[0].Items[2].Value;
            int localProcessingTime = Convert.ToInt32(inputData.EventGroups[0].Events[0].Items[3].Value);
            string localProcessStartTime = currentPreYear + inputData.EventGroups[0].Events[0].Items[4].Value;
            string localProcessEndTime = currentPreYear + inputData.EventGroups[0].Events[0].Items[5].Value;
            short[] processData = inputData.EventGroups[0].Events[1].RawData;
            #region [For More EventBlock]
            //針對 TRX多 EventBlock 特殊處理 超過3個EVENT ，1.BIT 2.Block 3 Item ....  sy add 20160525
            if (inputData.EventGroups[0].Events.Count > 3)
            {
                int itemEventCount = 0;//先算出總共有多少
                List<int> startIndex = new List<int>();
                for (int i = 1; i < inputData.EventGroups[0].Events.Count - 1; i++)
                {
                    startIndex.Add(itemEventCount);
                    itemEventCount += inputData.EventGroups[0].Events[i].RawData.Length;
                }
                short[] processDataTmp = new short[itemEventCount];
                int EventNo = 1;
                for (int j = 0; j < processDataTmp.Length; j++)//將對應的數值 重新排序
                {                    
                    processDataTmp[j] = inputData.EventGroups[0].Events[EventNo].RawData[j - startIndex[EventNo - 1]];
                    if (startIndex.Count > EventNo)                    
                        if (startIndex[EventNo] - 1 == j)
                            EventNo++;
                }

                processData = processDataTmp;
            }
            #endregion
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO=[{0}] IN EQUIPMENTENTITY!", eqpNo));
                Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
                if (line == null) throw new Exception(string.Format("CAN'T FIND LINE_ID=[{0}] IN LINEENTITY!", eqp.Data.LINEID));
                //sy add 20160719 
                if (line.Data.LINETYPE == eLineType.ARRAY.CAC_MYTEK ||line.Data.LINETYPE == eLineType.CELL.CCOVP)
                    return;
                //huangjiayin 20170710 CCCLN User要求上报ProcessDataReport

                Job job = ObjectManager.JobManager.GetJob(cassetteSequenceNo, jobSequenceNo);
                //if (job == null) throw new Exception(string.Format("EQUIPMENT_NO=[{2}] CAN'T FIND JOB, CASSETTE SEQUENCENO=[{0}],JOB SEQUENCE NO=[{1}] IN JOBENTITY!", cassetteSequenceNo, jobSequenceNo, eqpNo)); //增加一个Node 以方便Log查询
                if (job == null)
                {
                    //huangjiayin 20170710 CCCLN User要求上报ProcessDataReport
                    #region[CCCLN]
                    if (line.Data.LINETYPE == eLineType.CELL.CCCLN)
                    {
                        job = new Job(int.Parse(cassetteSequenceNo), 1);
                        job.GlassChipMaskBlockID =new StringBuilder().AppendFormat("CST{0}", glassID.Trim()).ToString();
                        job.JobType = eJobType.TFT;

                        IList<ProductProcessData.ITEMc> itemList_cln = new List<ProductProcessData.ITEMc>();

                        object[] _dataMES_cln = new object[12]
                { 
                    inputData.TrackKey, /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODEID,    /*2 MachineName*/
                    "",     /*3 UnitName*/
                    "",   /*4 LotName*/
                    glassID.Trim(),     /*5 CarrierName*/
                    "",     /*6 ProductName*/
                    "",   /*7 ProductSpecName*/
                    "",   /*8 ProductSpecVer*/
                    "",     /*9 ProcessOperationName*/
                    "",   /*10 LineRecipeName*/
                    itemList_cln,   /*11 ItemList*/
                };

                        object[] _dataEDA_cln = new object[8]
                {
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODEID,    /*2 MachineName*/
                    inputData.EventGroups[0].Events[0].Items[4].Value,    /*3 EQP Start Time*/
                    inputData.EventGroups[0].Events[0].Items[5].Value,    /*4 EQP End Time*/
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),    /*5 Report Time*/
                    job,                 /*6 no Job*/
                    null,             
                };

                        string paraList_cln;
                        HandleProcessData(eqpNo, eqp.Data.LINEID, eqp, job, processData, _dataMES_cln, _dataEDA_cln, out paraList_cln, inputData.TrackKey);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("[EQUIPMENT={0}] [TrackKey={1}] [CstSeqNo={2}] [JobSeqNo={3}] [CSTID={4}] [LocalProcessingTime={5}] [LocalProcessStartTime={6}] [LocalProcessEndTime={7}]",
                            inputData.Metadata.NodeNo, inputData.TrackKey, cassetteSequenceNo, "1", glassID, localProcessingTime, localProcessStartTime, localProcessEndTime));

                        Logger.LogTrxWrite(this.LogName,
                            string.Format("[EQUIPMENT={0}] [TrackKey={1}] [CstSeqNo={2}] [JobSeqNo={3}] [CSTID={4}] [LocalProcessingTime={5}] [LocalProcessStartTime={6}] [LocalProcessEndTime={7}]\r\n[RAWDATA={8}]",
                            inputData.Metadata.NodeNo, inputData.TrackKey, cassetteSequenceNo, "1", glassID, localProcessingTime, localProcessStartTime, localProcessEndTime, paraList_cln.Replace(',', '\n')));

                        ObjectManager.ProcessDataManager.MakeProcessDataValuesToFile(eqp.Data.NODEID, cassetteSequenceNo, "1", inputData.TrackKey, paraList_cln);

                        PROCESSDATAHISTORY paraHistory_cln = new PROCESSDATAHISTORY();
                        paraHistory_cln.CASSETTESEQNO = int.Parse(cassetteSequenceNo);
                        paraHistory_cln.JOBSEQNO = 1;
                        paraHistory_cln.JOBID = glassID;
                        paraHistory_cln.TRXID = inputData.TrackKey;
                        paraHistory_cln.NODEID = eqp.Data.NODEID;
                        paraHistory_cln.UPDATETIME = DateTime.Now;
                        paraHistory_cln.FILENAMA = string.Format("{0}_{1}_{2}_{3}", cassetteSequenceNo, "1", eqp.Data.NODEID, inputData.TrackKey);
                        paraHistory_cln.PROCESSTIME = localProcessingTime;
                        paraHistory_cln.LOCALPROCESSSTARTTIME = localProcessStartTime;
                        paraHistory_cln.LOCALPROCSSSENDTIME = localProcessEndTime;
                        IList<Unit> units_cln = ObjectManager.UnitManager.GetUnitsByEQPNo(eqp.Data.NODENO);
                        if (units_cln != null && units_cln.Count > 0)
                        {
                            GetUnitProcessData(paraHistory_cln, inputData.EventGroups[0].Events[0], units_cln);

                        }

                        ObjectManager.ProcessDataManager.SaveProcessDataHistory(paraHistory_cln);

                        Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("SaveProcessDataHistory [EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] CST_SEQNO=[{3}] JOB_SEQNO=[{4}] GLASS_ID=[{5}].",
                            eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, cassetteSequenceNo, jobSequenceNo, glassID));


                    }
                    #endregion
                    else
                    {
                        Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                             string.Format("EQUIPMENT=[{0}] CAN'T FIND JOB DATA, CST_SEQNO=[{1}] JOB_SEQNO=[{2}] CAN'T REPORT PROCESS DATA!!",
                                 eqp.Data.NODENO, cassetteSequenceNo, jobSequenceNo));

                    }
                    return;



                }
                //Update Job Process Flow  Tom 20150321
                GetProcessFlow(job, inputData.EventGroups[0].Events[0], eqp);

                IList<ProductProcessData.ITEMc> itemList = new List<ProductProcessData.ITEMc>();
                object[] _dataMES = new object[12]
                { 
                    inputData.TrackKey, /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODEID,    /*2 MachineName*/
                    "",     /*3 UnitName*/
                    (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].LOTNAME),   /*4 LotName*/
                    (job.FromCstID == null ? "" : job.FromCstID),     /*5 CarrierName*/
                    (job.GlassChipMaskBlockID == null ? "" : job.GlassChipMaskBlockID),     /*6 ProductName*/
                    (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME),   /*7 ProductSpecName*/
                    (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].PRODUCTSPECVER),   /*8 ProductSpecVer*/
                    (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME),     /*9 ProcessOperationName*/
                    (job.MesCstBody.LOTLIST.Count == 0 ? "" : job.MesCstBody.LOTLIST[0].LINERECIPENAME),   /*10 LineRecipeName*/
                    itemList,   /*11 ItemList*/
                };

                object[] _dataEDA = new object[8]
                {
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODEID,    /*2 MachineName*/
                    inputData.EventGroups[0].Events[0].Items[4].Value,    /*3 EQP Start Time*/
                    inputData.EventGroups[0].Events[0].Items[5].Value,    /*4 EQP End Time*/
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),    /*5 Report Time*/
                    job,                 /*6 no Job*/
                    null,             
                };
              

                string paraList;
                HandleProcessData(eqpNo, eqp.Data.LINEID, eqp, job, processData, _dataMES, _dataEDA, out paraList, inputData.TrackKey);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [TrackKey={1}] [CstSeqNo={2}] [JobSeqNo={3}] [GlsID={4}] [LocalProcessingTime={5}] [LocalProcessStartTime={6}] [LocalProcessEndTime={7}]",
                    inputData.Metadata.NodeNo, inputData.TrackKey, cassetteSequenceNo, jobSequenceNo, glassID, localProcessingTime, localProcessStartTime, localProcessEndTime));

                Logger.LogTrxWrite(this.LogName,
                    string.Format("[EQUIPMENT={0}] [TrackKey={1}] [CstSeqNo={2}] [JobSeqNo={3}] [GlsID={4}] [LocalProcessingTime={5}] [LocalProcessStartTime={6}] [LocalProcessEndTime={7}]\r\n[RAWDATA={8}]",
                    inputData.Metadata.NodeNo, inputData.TrackKey, cassetteSequenceNo, jobSequenceNo, glassID, localProcessingTime, localProcessStartTime, localProcessEndTime, paraList.Replace(',','\n')));              

                ObjectManager.ProcessDataManager.MakeProcessDataValuesToFile(eqp.Data.NODEID, cassetteSequenceNo, jobSequenceNo, inputData.TrackKey, paraList);

                PROCESSDATAHISTORY paraHistory = new PROCESSDATAHISTORY();
                paraHistory.CASSETTESEQNO = int.Parse(cassetteSequenceNo);
                paraHistory.JOBSEQNO = int.Parse(jobSequenceNo);
                paraHistory.JOBID = glassID;
                paraHistory.TRXID = inputData.TrackKey;
                paraHistory.NODEID = eqp.Data.NODEID;
                paraHistory.UPDATETIME = DateTime.Now;
                paraHistory.FILENAMA = string.Format("{0}_{1}_{2}_{3}", cassetteSequenceNo, jobSequenceNo, eqp.Data.NODEID, inputData.TrackKey);
                paraHistory.PROCESSTIME = localProcessingTime;
                paraHistory.LOCALPROCESSSTARTTIME = localProcessStartTime;
                paraHistory.LOCALPROCSSSENDTIME = localProcessEndTime;
                IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(eqp.Data.NODENO);
                if (units != null && units.Count > 0)
                {
                    GetUnitProcessData(paraHistory, inputData.EventGroups[0].Events[0], units);

                }

                ObjectManager.ProcessDataManager.SaveProcessDataHistory(paraHistory);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("SaveProcessDataHistory [EQUIPMENT={0}] [BCS <- EQP][{1}] BIT=[ON] CIM_MODE=[{2}] CST_SEQNO=[{3}] JOB_SEQNO=[{4}] GLASS_ID=[{5}].",
                    eqp.Data.NODENO, inputData.TrackKey, eqp.File.CIMMode, cassetteSequenceNo, jobSequenceNo, glassID));
            }
            catch (Exception ex)
            {
                Exception tmp = ex;
                StringBuilder sb = new StringBuilder();
                while (tmp != null)
                {
                    sb.AppendLine(tmp.Message);
                    sb.AppendLine(tmp.StackTrace);
                    tmp = tmp.InnerException;
                }

                Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [TrackKey={1}] [CstSeqNo={2}] [JobSeqNo={3}] [GlsID={4}] [LocalProcessingTime={5}] [LocalProcessStartTime={6}] [LocalProcessEndTime={7}]\r\n[Exception={8}]",
                    inputData.Metadata.NodeNo, inputData.TrackKey, cassetteSequenceNo, jobSequenceNo, glassID, localProcessingTime, localProcessStartTime, localProcessEndTime, sb.ToString()));
            }
        }

        private void GetUnitProcessData(PROCESSDATAHISTORY processHis, EDCFile edcFile, IList<Unit> units)
        {
            foreach (Unit unit in units)
            {
                switch (unit.Data.UNITNO)
                {
                case "1":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT1PROCESSTIME = edcFile.GetUnitProcessingTime(unit.Data.UNITNO);
                        processHis.UNIT1PROCESSSTARTTIME = edcFile.GetUnitProcessStartTime(unit.Data.UNITNO);
                        processHis.UNIT1PROCESSENDTIME = edcFile.GetUnitProcessEndTime(unit.Data.UNITNO);
                    }
                    break;
                case "2":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT2PROCESSTIME = edcFile.GetUnitProcessingTime(unit.Data.UNITNO);
                        processHis.UNIT2PROCESSSTARTTIME = edcFile.GetUnitProcessStartTime(unit.Data.UNITNO);
                        processHis.UNIT2PROCESSENDTIME = edcFile.GetUnitProcessEndTime(unit.Data.UNITNO);
                    }
                    break;
                case "3":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT3PROCESSTIME = edcFile.GetUnitProcessingTime(unit.Data.UNITNO);
                        processHis.UNIT3PROCESSSTARTTIME = edcFile.GetUnitProcessStartTime(unit.Data.UNITNO);
                        processHis.UNIT3PROCESSENDTIME = edcFile.GetUnitProcessEndTime(unit.Data.UNITNO);
                    }
                    break;
                case "4":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT4PROCESSTIME = edcFile.GetUnitProcessingTime(unit.Data.UNITNO);
                        processHis.UNIT4PROCESSSTARTTIME = edcFile.GetUnitProcessStartTime(unit.Data.UNITNO);
                        processHis.UNIT4PROCESSENDTIME = edcFile.GetUnitProcessEndTime(unit.Data.UNITNO);
                    }
                    break;
                case "5":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT5PROCESSTIME = edcFile.GetUnitProcessingTime(unit.Data.UNITNO);
                        processHis.UNIT5PROCESSSTARTTIME = edcFile.GetUnitProcessStartTime(unit.Data.UNITNO);
                        processHis.UNIT5PROCESSENDTIME = edcFile.GetUnitProcessEndTime(unit.Data.UNITNO);
                    }
                    break;
                case "6":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT6PROCESSTIME = edcFile.GetUnitProcessingTime(unit.Data.UNITNO);
                        processHis.UNIT6PROCESSSTARTTIME = edcFile.GetUnitProcessStartTime(unit.Data.UNITNO);
                        processHis.UNIT6PROCESSENDTIME = edcFile.GetUnitProcessEndTime(unit.Data.UNITNO);
                    }
                    break;
                case "7":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT7PROCESSTIME = edcFile.GetUnitProcessingTime(unit.Data.UNITNO);
                        processHis.UNIT7PROCESSSTARTTIME = edcFile.GetUnitProcessStartTime(unit.Data.UNITNO);
                        processHis.UNIT7PROCESSENDTIME = edcFile.GetUnitProcessEndTime(unit.Data.UNITNO);
                    }
                    break;
                case "8":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT8PROCESSTIME = edcFile.GetUnitProcessingTime(unit.Data.UNITNO);
                        processHis.UNIT8PROCESSSTARTTIME = edcFile.GetUnitProcessStartTime(unit.Data.UNITNO);
                        processHis.UNIT8PROCESSENDTIME = edcFile.GetUnitProcessEndTime(unit.Data.UNITNO);
                    }
                    break;
                case "9":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT9PROCESSTIME = edcFile.GetUnitProcessingTime(unit.Data.UNITNO);
                        processHis.UNIT9PROCESSSTARTTIME = edcFile.GetUnitProcessStartTime(unit.Data.UNITNO);
                        processHis.UNIT9PROCESSENDTIME = edcFile.GetUnitProcessEndTime(unit.Data.UNITNO);
                    }
                    break;
                case "10":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT10PROCESSTIME = edcFile.GetUnitProcessingTime(unit.Data.UNITNO);
                        processHis.UNIT10PROCESSSTARTTIME = edcFile.GetUnitProcessStartTime(unit.Data.UNITNO);
                        processHis.UNIT10PROCESSENDTIME = edcFile.GetUnitProcessEndTime(unit.Data.UNITNO);
                    }
                    break;
                case "11":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT11PROCESSTIME = edcFile.GetUnitProcessingTime(unit.Data.UNITNO);
                        processHis.UNIT11PROCESSSTARTTIME = edcFile.GetUnitProcessStartTime(unit.Data.UNITNO);
                        processHis.UNIT11PROCESSENDTIME = edcFile.GetUnitProcessEndTime(unit.Data.UNITNO);
                    }
                    break;
                case "12":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT12PROCESSTIME = edcFile.GetUnitProcessingTime(unit.Data.UNITNO);
                        processHis.UNIT12PROCESSSTARTTIME = edcFile.GetUnitProcessStartTime(unit.Data.UNITNO);
                        processHis.UNIT12PROCESSENDTIME = edcFile.GetUnitProcessEndTime(unit.Data.UNITNO);
                    }
                    break;
                case "13":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT13PROCESSTIME = edcFile.GetUnitProcessingTime(unit.Data.UNITNO);
                        processHis.UNIT13PROCESSSTARTTIME = edcFile.GetUnitProcessStartTime(unit.Data.UNITNO);
                        processHis.UNIT13PROCESSENDTIME = edcFile.GetUnitProcessEndTime(unit.Data.UNITNO);
                    }
                    break;
                case "14":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT14PROCESSTIME = edcFile.GetUnitProcessingTime(unit.Data.UNITNO);
                        processHis.UNIT14PROCESSSTARTTIME = edcFile.GetUnitProcessStartTime(unit.Data.UNITNO);
                        processHis.UNIT14PROCESSENDTIME = edcFile.GetUnitProcessEndTime(unit.Data.UNITNO);
                    }
                    break;
                case "15":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT15PROCESSTIME = edcFile.GetUnitProcessingTime(unit.Data.UNITNO);
                        processHis.UNIT15PROCESSSTARTTIME = edcFile.GetUnitProcessStartTime(unit.Data.UNITNO);
                        processHis.UNIT15PROCESSENDTIME = edcFile.GetUnitProcessEndTime(unit.Data.UNITNO);
                    }
                    break;
                }
            }
        }

        private void GetUnitProcessData(PROCESSDATAHISTORY processHis, Event inputEvent, IList<Unit> units)
        {
            string currentPreYear = DateTime.Now.Year.ToString().Substring(0, 2);
            foreach (Unit unit in units)
            {
                switch (unit.Data.UNITNO)
                {
                case "1":
                        if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT1PROCESSTIME = Convert.ToInt32(inputEvent.Items[6].Value);
                        processHis.UNIT1PROCESSSTARTTIME = currentPreYear + inputEvent.Items[7].Value;
                        processHis.UNIT1PROCESSENDTIME = currentPreYear + inputEvent.Items[8].Value;
                    }
                    break;
                case "2":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT2PROCESSTIME = Convert.ToInt32(inputEvent.Items[9].Value);
                        processHis.UNIT2PROCESSSTARTTIME = currentPreYear + inputEvent.Items[10].Value;
                        processHis.UNIT2PROCESSENDTIME = currentPreYear + inputEvent.Items[11].Value;
                    }
                    break;
                case "3":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT3PROCESSTIME = Convert.ToInt32(inputEvent.Items[12].Value);
                        processHis.UNIT3PROCESSSTARTTIME = currentPreYear + inputEvent.Items[13].Value;
                        processHis.UNIT3PROCESSENDTIME = currentPreYear + inputEvent.Items[14].Value;
                    }
                    break;
                case "4":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT4PROCESSTIME = Convert.ToInt32(inputEvent.Items[15].Value);
                        processHis.UNIT4PROCESSSTARTTIME = currentPreYear + inputEvent.Items[16].Value;
                        processHis.UNIT4PROCESSENDTIME = currentPreYear + inputEvent.Items[17].Value;
                    }
                    break;
                case "5":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT5PROCESSTIME = Convert.ToInt32(inputEvent.Items[18].Value);
                        processHis.UNIT5PROCESSSTARTTIME = currentPreYear + inputEvent.Items[19].Value;
                        processHis.UNIT5PROCESSENDTIME = currentPreYear + inputEvent.Items[20].Value;
                    }
                    break;
                case "6":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT6PROCESSTIME = Convert.ToInt32(inputEvent.Items[21].Value);
                        processHis.UNIT6PROCESSSTARTTIME = currentPreYear + inputEvent.Items[22].Value;
                        processHis.UNIT6PROCESSENDTIME = currentPreYear + inputEvent.Items[23].Value;
                    }
                    break;
                case "7":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT7PROCESSTIME = Convert.ToInt32(inputEvent.Items[24].Value);
                        processHis.UNIT7PROCESSSTARTTIME = currentPreYear + inputEvent.Items[25].Value;
                        processHis.UNIT7PROCESSENDTIME = currentPreYear + inputEvent.Items[26].Value;
                    }
                    break;
                case "8":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT8PROCESSTIME = Convert.ToInt32(inputEvent.Items[27].Value);
                        processHis.UNIT8PROCESSSTARTTIME = currentPreYear + inputEvent.Items[28].Value;
                        processHis.UNIT8PROCESSENDTIME = currentPreYear + inputEvent.Items[29].Value;
                    }
                    break;
                case "9":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT9PROCESSTIME = Convert.ToInt32(inputEvent.Items[30].Value);
                        processHis.UNIT9PROCESSSTARTTIME = currentPreYear + inputEvent.Items[31].Value;
                        processHis.UNIT9PROCESSENDTIME = currentPreYear + inputEvent.Items[32].Value;
                    }
                    break;
                case "10":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT10PROCESSTIME = Convert.ToInt32(inputEvent.Items[33].Value);
                        processHis.UNIT10PROCESSSTARTTIME = currentPreYear + inputEvent.Items[34].Value;
                        processHis.UNIT10PROCESSENDTIME = currentPreYear + inputEvent.Items[35].Value;
                    }
                    break;
                case "11":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT11PROCESSTIME = Convert.ToInt32(inputEvent.Items[36].Value);
                        processHis.UNIT11PROCESSSTARTTIME = currentPreYear + inputEvent.Items[37].Value;
                        processHis.UNIT11PROCESSENDTIME = currentPreYear + inputEvent.Items[38].Value;
                    }
                    break;
                case "12":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT12PROCESSTIME = Convert.ToInt32(inputEvent.Items[39].Value);
                        processHis.UNIT12PROCESSSTARTTIME = currentPreYear + inputEvent.Items[40].Value;
                        processHis.UNIT12PROCESSENDTIME = currentPreYear + inputEvent.Items[41].Value;
                    }
                    break;
                case "13":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT13PROCESSTIME = Convert.ToInt32(inputEvent.Items[42].Value);
                        processHis.UNIT13PROCESSSTARTTIME = currentPreYear + inputEvent.Items[43].Value;
                        processHis.UNIT13PROCESSENDTIME = currentPreYear + inputEvent.Items[44].Value;
                    }
                    break;
                case "14":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT14PROCESSTIME = Convert.ToInt32(inputEvent.Items[45].Value);
                        processHis.UNIT14PROCESSSTARTTIME = currentPreYear + inputEvent.Items[46].Value;
                        processHis.UNIT14PROCESSENDTIME = currentPreYear + inputEvent.Items[47].Value;
                    }
                    break;
                case "15":
                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT"))
                    {
                        processHis.UNIT15PROCESSTIME = Convert.ToInt32(inputEvent.Items[48].Value);
                        processHis.UNIT15PROCESSSTARTTIME = currentPreYear + inputEvent.Items[49].Value;
                        processHis.UNIT15PROCESSENDTIME = currentPreYear + inputEvent.Items[50].Value;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 从PROCESS Data 中取出 Job PROCESS Flow
        /// </summary>
        /// <param name="job"></param>
        /// <param name="inputEvent"></param>
        /// <param name="eqp"></param>
        public void GetProcessFlow(Job job,Event inputEvent,Equipment eqp)
        {
            try
            {
                string currentPreYear = DateTime.Now.Year.ToString().Substring(0, 2);
                lock (job)
                {
                    if (!job.JobProcessFlows.ContainsKey(eqp.Data.NODEID))
                    {
                        ProcessFlow pcf = new ProcessFlow();
                        pcf.MachineName = eqp.Data.NODEID;
                        pcf.StartTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[4].Value);
                        pcf.EndTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[5].Value);
                        job.JobProcessFlows.Add(eqp.Data.NODEID, pcf);
                    }
                    else
                    {
                        job.JobProcessFlows[eqp.Data.NODEID].StartTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[4].Value);
                        job.JobProcessFlows[eqp.Data.NODEID].EndTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[5].Value);
                    }
                    IList<Unit> units = ObjectManager.UnitManager.GetUnitsByEQPNo(eqp.Data.NODENO);
                    if (units != null && units.Count > 0)
                    {
                        foreach (Unit unit in units)
                        {
                            bool bNewpcf = false; //add for check if store event has create 2016/06/23 cc.kuang

                            if (unit.Data.UNITATTRIBUTE == "VIRTUAL")//过滤掉Virtual
                            {
                                continue;
                            }
                            if (!job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows.ContainsKey(unit.Data.UNITID))
                            {
                                ProcessFlow pcf = new ProcessFlow();
                                pcf.MachineName = unit.Data.UNITID; //修改成UNIT ID Tom  20150421  
                                job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows.Add(unit.Data.UNITID, pcf);
                                bNewpcf = true;
                            }
                            #region Unit
                            switch (unit.Data.UNITNO)
                            {
                                case "1":
                                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT") && bNewpcf)
                                    {
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].StartTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[7].Value);
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[8].Value);
                                    }
                                    break;
                                case "2":
                                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT") && bNewpcf)
                                    {
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].StartTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[10].Value);
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[11].Value);
                                    }
                                    break;
                                case "3":
                                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT") && bNewpcf)
                                    {
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].StartTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[13].Value);
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[14].Value);
                                    }
                                    break;
                                case "4":
                                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT") && bNewpcf)
                                    {
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].StartTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[16].Value);
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[17].Value);
                                    }
                                    break;
                                case "5":
                                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT") && bNewpcf)
                                    {
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].StartTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[19].Value);
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[20].Value);
                                    }
                                    break;
                                case "6":
                                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT") && bNewpcf)
                                    {
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].StartTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[22].Value);
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[23].Value);
                                    }
                                    break;
                                case "7":
                                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT") && bNewpcf)
                                    {
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].StartTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[25].Value);
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[26].Value);
                                    }
                                    break;
                                case "8":
                                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT") && bNewpcf)
                                    {
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].StartTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[28].Value);
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[29].Value);
                                    }
                                    break;
                                case "9":
                                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT") && bNewpcf)
                                    {
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].StartTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[31].Value);
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[32].Value);
                                    }
                                    break;
                                case "10":
                                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT") && bNewpcf)
                                    {
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].StartTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[34].Value);
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[35].Value);
                                    }
                                    break;
                                case "11":
                                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT") && bNewpcf)
                                    {
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].StartTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[37].Value);
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[38].Value);
                                    }
                                    break;
                                case "12":
                                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT") && bNewpcf)
                                    {
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].StartTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[40].Value);
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[41].Value);
                                    }
                                    break;
                                case "13":
                                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT") && bNewpcf)
                                    {
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].StartTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[43].Value);
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[44].Value);
                                    }
                                    break;
                                case "14":
                                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT") && bNewpcf)
                                    {
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].StartTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[46].Value);
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[47].Value);
                                    }
                                    break;
                                case "15":
                                    if (!unit.Data.UNITATTRIBUTE.Trim().Equals("ATTACHMENT") && bNewpcf)
                                    {
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].StartTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[49].Value);
                                        job.JobProcessFlows[eqp.Data.NODEID].UnitProcessFlows[unit.Data.UNITID].EndTime = DateTime14ToDateTime(currentPreYear + inputEvent.Items[50].Value);
                                    }
                                    break;
                            }
                            #endregion
                        }
                    }
                }
                ObjectManager.JobManager.EnqueueSave(job);
               
            }
            catch (System.Exception ex)
            {
            	  Logger.LogErrorWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",ex);
            }
        }

        private DateTime DateTime14ToDateTime(string str)
        {
            try
            {
                if (str.Length == "yyyyMMddHHmmss".Length)
                {
                    string dtstr = string.Format("{0}-{1}-{2} {3}:{4}:{5}",
                    str.Substring(0, 4),
                    str.Substring(4, 2),
                    str.Substring(6, 2),
                    str.Substring(8, 2),
                    str.Substring(10, 2),
                    str.Substring(12, 2));
                    return DateTime.Parse(dtstr);
                }
                return Convert.ToDateTime(str);
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        private void ProcessDataReportReply(string eqpNo, eBitResult value, string trackKey)
        {
            try
            {
                Trx outputdata = GetServerAgent("PLCAgent").GetTransactionFormat(string.Format(Key_ProcessDataReportReply, eqpNo)) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)value).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                if (_timerManager.IsAliveTimer(string.Format(Key_ProcessDataReportTimeout, eqpNo)))
                {
                    _timerManager.TerminateTimer(string.Format(Key_ProcessDataReportTimeout, eqpNo));
                }

                if (value.Equals(eBitResult.ON))
                {
                    _timerManager.CreateTimer(string.Format(Key_ProcessDataReportTimeout, eqpNo), false, ParameterManager["T2"].GetInteger(), new System.Timers.ElapsedEventHandler(ProcessDataReportReplyTimeout), trackKey);
                }
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}], SET BIT =[{2}].", eqpNo, trackKey, value.ToString()));
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ProcessDataReportReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format(Key_ProcessDataReportTimeout, sArray[0]);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format(Key_ProcessDataReportReply, sArray[0])) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] BCS REPLY, PROCESS DATA REPORT REPLY TIMEOUT SET VALUE [OFF].",
                    sArray[0], trackKey));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }
        #endregion

        #region [EDC Request]
        public void ProcessDataRequestCommand(string eqpNo, eBitResult command)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", eqpNo));

                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format(Key_ProcessDataRequestCommand, eqpNo)) as Trx;
                outputData.EventGroups[0].Events[1].Items[0].Value = ((int)eBitResult.ON).ToString();
                //Watson Add 20141113 For Write Word dely 200 ms then Bit On
                outputData.EventGroups[0].Events[1].OpDelayTimeMS = ParameterManager["EVENTDELAYTIME"].GetInteger();
                outputData.TrackKey = UtilityMethod.GetAgentTrackKey();
                SendPLCData(outputData);

                string timeName = string.Format(Key_ProcessDataRequestCommandTimeout, eqpNo);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }
                _timerManager.CreateTimer(timeName, false, ParameterManager["T1"].GetInteger(), new System.Timers.ElapsedEventHandler(ProcessDataRequestCommandReplyTimeout), outputData.TrackKey);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] PROCESS DATA REQUEST COMMAND, SET BIT [ON]", eqp.Data.NODENO,
                        outputData.TrackKey));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void ProcessDataRequestCommandReply(Trx inputData)
        {
            try
            {
                if (inputData.IsInitTrigger) return;
                string eqpNo = inputData.Metadata.NodeNo;
                eBitResult triggerBit = (eBitResult)int.Parse(inputData.EventGroups[0].Events[1].Items[0].Value);

                Logger.LogInfoWrite(this.LogName, GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS <- EQP]=[{1}]  PROCESS DATA REQUEST COMMAND REPLY, SET BIT [{2}]",
                    eqpNo, inputData.TrackKey, triggerBit.ToString()));

                string timeName = string.Format(Key_ProcessDataRequestCommandTimeout, inputData.Metadata.NodeNo);
                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                if (triggerBit == eBitResult.OFF) return;
                Equipment eqp= ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null) throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", eqpNo));

                short[] processData = inputData.EventGroups[0].Events[1].RawData;

                IList<ProductProcessData.ITEMc> itemList = new List<ProductProcessData.ITEMc>();
                object[] _dataMES = new object[12]
                { 
                    inputData.TrackKey, /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODEID,    /*2 MachineName*/
                    "",     /*3 UnitName*/
                    "", //(job.MesCstBody.LOTLIST[0].LOTNAME == null ? "" : job.MesCstBody.LOTLIST[0].LOTNAME),   /*4 LotName*/
                    "", //(job.FromCstID == null ? "" : job.FromCstID),     /*5 CarrierName*/
                    "", //(job.GlassChipMaskBlockID == null ? "" : job.GlassChipMaskBlockID),     /*6 ProductName*/
                    "", //(job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME == null ? "" : job.MesCstBody.LOTLIST[0].PRODUCTSPECNAME),   /*7 ProductSpecName*/
                    "", //(job.MesCstBody.LOTLIST[0].PRODUCTSPECVER == null ? "" : job.MesCstBody.LOTLIST[0].PRODUCTSPECVER),   /*8 ProductSpecVer*/
                    "", //(job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME == null ? "" : job.MesCstBody.LOTLIST[0].PROCESSOPERATIONNAME),     /*9 ProcessOperationName*/
                    "", //(job.MesCstBody.LOTLIST[0].LINERECIPENAME == null ? "" : job.MesCstBody.LOTLIST[0].LINERECIPENAME),   /*10 LineRecipeName*/
                    itemList,   /*11 ItemList*/
                };

                Job job = new Job();
                object[] _dataEDA = new object[8]
                {
                    inputData.TrackKey,  /*0 TrackKey*/
                    eqp.Data.LINEID,    /*1 LineName*/
                    eqp.Data.NODEID,    /*2 MachineName*/
                    "",    /*3 EQP Start Time*/
                    "",    /*4 EQP End Time*/
                    DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),    /*5 Report Time*/
                    job,                 /*6 no Job*/
                    null,             
                };

                string paraList;
                HandleProcessData(eqpNo, eqp.Data.LINEID, null, null, processData, _dataMES, _dataEDA, out paraList, inputData.TrackKey);

                #region [Command Off]
                Trx outputData = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format(Key_ProcessDataRequestCommand, inputData.Metadata.NodeNo)) as Trx;
                outputData.EventGroups[0].Events[0].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputData.TrackKey = inputData.TrackKey;
                SendPLCData(outputData);
                Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] BCS REPLY, PROCCESS DATA REQUEST COMMAND REPLY SET VALUE [OFF].", eqpNo, inputData.TrackKey));
                #endregion
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        private void ProcessDataRequestCommandReplyTimeout(object subjet, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                UserTimer timer = subjet as UserTimer;
                string tmp = timer.TimerId;
                string trackKey = timer.State.ToString();
                string[] sArray = tmp.Split('_');

                string timeName = string.Format(Key_ProcessDataRequestCommandTimeout, sArray[0]);

                if (_timerManager.IsAliveTimer(timeName))
                {
                    _timerManager.TerminateTimer(timeName);
                }

                Trx outputdata = GetServerAgent(eAgentName.PLCAgent).GetTransactionFormat(string.Format(Key_ProcessDataRequestCommand, sArray[0])) as Trx;
                outputdata.EventGroups[0].Events[0].Items[0].Value = ((int)eBitResult.OFF).ToString();
                outputdata.TrackKey = trackKey;
                SendPLCData(outputdata);

                Logger.LogWarnWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("[EQUIPMENT={0}] [BCS -> EQP]=[{1}] BCS REPLY, PROCESS DATA REQUEST COMMAND REPLY TIMEOUT SET VALUE [OFF].",
                    sArray[0], trackKey));

            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

        }
        #endregion

        private void HandleProcessData(string eqpNo, string lineID, Equipment eqp, Job job, short[] processData, object[] _dataMES, object[] _dataEDA, out string paraList, string trackKey)
        {
            string decodeItemName = "";
            
            try
            {
                string tmpList = "";
                paraList = "";

                //取得EDC Format
                IList<ProcessData> processDatas=null;
                //20170810 huangjiayin: smo sepcial
                if (eqp.Data.NODEID.Contains("CCSMO"))
                {
                    foreach (ProcessFlow pf in job.JobProcessFlows[eqp.Data.NODEID].ExtendUnitProcessFlows)
                    {
                        if (pf.MachineName == "CCSMO1H2" || pf.MachineName == "CCSMO2H2")
                        {
                            processDatas = ObjectManager.ProcessDataManager.GetSMOProcessData(eqpNo, eSMOEDCUnitType.COOL);
                            break;
                        }
                    }
                    if (processDatas == null)
                    {
                        processDatas = ObjectManager.ProcessDataManager.GetSMOProcessData(eqpNo, eSMOEDCUnitType.OVEN);
                    }
                }
                else
                {
                    processDatas = ObjectManager.ProcessDataManager.GetProcessData(eqpNo);
                }
                if (processDatas == null) return;

                string value = string.Empty;
                int startaddress10 = 0;

                IList<ProductProcessData.ITEMc> itemList = new List<ProductProcessData.ITEMc>();

                Dictionary<string, List<string>> edalis = new Dictionary<string, List<string>>();

                // Array 
                IList<ChangeTargetLife.CHAMBERc> chamberList = new List<ChangeTargetLife.CHAMBERc>();
                IList<ChangePVDMaterialLife.CHAMBERc> PVDchamberList = new List<ChangePVDMaterialLife.CHAMBERc>();

                foreach (ProcessData pd in processDatas)
                {
                    decodeItemName = pd.Data.PARAMETERNAME;

                    ItemExpressionEnum ie;
                    if (!Enum.TryParse(pd.Data.EXPRESSION.ToUpper(), out ie))
                    {
                        continue;
                    }
                    #region decode by expression
                    bool isOutRang = false;
                    switch (ie)
                    {
                        case ItemExpressionEnum.BIT:
                            value = ExpressionBIT.Decode(startaddress10, int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), processData);
                            break;
                        case ItemExpressionEnum.ASCII:
                            value = ExpressionASCII.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), processData);
                            value = Regex.Replace(value, @"[^\x21-\x7E]|<|>|'", " ");//过滤不可显示的字符 20150211 tom
                            break;
                        case ItemExpressionEnum.BIN:
                            value = ExpressionBIN.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), processData);
                            break;
                        case ItemExpressionEnum.EXP:
                            //add for EXP Err - just get process data[0],[1] 2016/05/30
                            /*
                            short[] processDataEXP = new short[2];
                            Array.Copy(processData, int.Parse(pd.Data.WOFFSET), processDataEXP, 0, 2);
                            value = ExpressionEXP.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), processDataEXP).ToString();
                            */
                            value = ExpressionEXP.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), processData).ToString();
                            break;
                        case ItemExpressionEnum.HEX:
                            value = ExpressionHEX.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), processData);
                            break;
                        case ItemExpressionEnum.INT:
                            value = ExpressionINT.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), processData).ToString();
                            if (value == "65535")
                                isOutRang = true;
                            break;
                        case ItemExpressionEnum.LONG:
                            value = ExpressionLONG.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), processData).ToString();
                            if (value == "4294967295")
                                isOutRang = true;
                            break;
						case ItemExpressionEnum.SINT:
                            value = ExpressionSINT.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), processData).ToString();
                            if(value=="32767")
                                isOutRang = true;
                            break;
                        case ItemExpressionEnum.SLONG:
                            value = ExpressionSLONG.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), processData).ToString();
                            if (value == "2147483647")
                                isOutRang = true;
                            break;
						case ItemExpressionEnum.BCD:
							value = ExpressionBCD.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), processData).ToString();
							break;
                        default:
                            break;
                    }
                    #endregion

                    #region 算法
                    string itemValue = string.Empty;
                    if (false == isOutRang)
                    {
                        switch (pd.Data.OPERATOR)
                        {
                            case ArithmeticOperator.PlusSign:
                                itemValue = (double.Parse(value) + double.Parse(pd.Data.DOTRATIO)).ToString();
                                break;
                            case ArithmeticOperator.MinusSign:
                                itemValue = (double.Parse(value) - double.Parse(pd.Data.DOTRATIO)).ToString();
                                break;
                            case ArithmeticOperator.TimesSign:
                                itemValue = (double.Parse(value) * double.Parse(pd.Data.DOTRATIO)).ToString();
                                break;
                            case ArithmeticOperator.DivisionSign:
                                itemValue = (double.Parse(value) / double.Parse(pd.Data.DOTRATIO)).ToString();
                                break;
                            default:
                                itemValue = value;
                                break;
                        }
                    }
                    else
                    {
                        itemValue = "NA";
                    }

                    //Add By Yangzhenteng For ODF MAI ProcesData Special Bypass20191011
                    bool ODFMAISPECIALPROCESSDATABYPASSFLAG = ParameterManager.ContainsKey("ODFMAISPECIALPROCESSDATABYPASSFLAG") ? ParameterManager["ODFMAISPECIALPROCESSDATABYPASSFLAG"].GetBoolean() : false; 
                    if (lineID.Contains("CCODF") && eqp.Data.NODENO == "L15")
                    {
                        if (pd.Data.PARAMETERNAME.Contains("GLASS_") || pd.Data.PARAMETERNAME.Contains("BLOCK_") && ODFMAISPECIALPROCESSDATABYPASSFLAG)
                        {
                            if (itemValue.Contains("99.99"))
                            {
                                this.Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("CCODF Line MAI  Report Process Data Item=[{0}], Values=[{1}],TrackKey=[{2}],Values Need Change To 0 !!!", pd.Data.PARAMETERNAME, itemValue, trackKey));
                                itemValue = "0";
                            }
                        }
                    }
                    else
                    { }
                    #endregion

                    #region Report Host List
                    if (pd.Data.REPORTTO != null)
                    {
                        string[] hostReportList = pd.Data.REPORTTO.Split(',');
                        if (hostReportList.Length > 0)
                        {
                            foreach (string report in hostReportList)
                            {
                                switch (report.ToUpper())
                                {
                                    case "MES":
                                        #region MES List
                                        ProductProcessData.ITEMc item = new ProductProcessData.ITEMc();
                                        ProductProcessData.SITEc site = new ProductProcessData.SITEc();
                                        bool duplicFlag = false;

                                        item.ITEMNAME = (pd.Data.ITEM == "" ? pd.Data.PARAMETERNAME : pd.Data.ITEM);

                                        foreach (ProductProcessData.ITEMc itemName in itemList)
                                        {
                                            if (itemName.ITEMNAME == item.ITEMNAME)
                                            {
                                                site.SITENAME = pd.Data.SITE.Trim() == "" ? "DEFAULT" : pd.Data.SITE.Trim();
                                                site.SITEVALUE = itemValue;

                                                itemName.SITELIST.Add(site);

                                                duplicFlag = true;
                                                break;
                                            }
                                        }
                                        if (!duplicFlag)
                                        {
                                            site.SITENAME = pd.Data.SITE.Trim() == "" ? "DEFAULT" : pd.Data.SITE.Trim();
                                            site.SITEVALUE = itemValue;

                                            item.SITELIST.Add(site);
                                            Line line = ObjectManager.LineManager.GetLine(lineID);
                                            if (line != null)
                                            {
                                                if (line.Data.FABTYPE.ToUpper() == "CF" &&(((eqpNo == "L6" || eqpNo == "L7" || eqpNo == "L20" || eqpNo == "L10")&&itemValue =="0")||job.JobJudge=="3"))
                                                {
                                                    break;
                                                }
                                            } //modify by qiumin 20180320  ,CF Process date report to MES(rSPC),Coater,HPCP,ALN,OVEN 侧别数据等于0时不上报避免误报警
                                            itemList.Add(item);
                                        }
                                        #endregion

                                        break;

                                    case "EDA":
                                        #region EDA List
                                        string param = pd.Data.PARAMETERNAME + ";" + itemValue.ToString();
                                        string paraGroup = (pd.Data.ITEM == "" ? pd.Data.PARAMETERNAME : pd.Data.ITEM);

                                        if (edalis.ContainsKey(paraGroup))
                                        {
                                            List<string> list = new List<string>();
                                            list = edalis[paraGroup];
                                            list.Add(param);

                                            edalis[paraGroup] = list;
                                        }
                                        else
                                        {
                                            List<string> list = new List<string>();
                                            list.Add(param);

                                            edalis.Add(paraGroup, list);
                                        }
                                        #endregion

                                        break;

                                    case "OEE":
                                        break;

                                    default:
                                        break;
                                }
                            }
                        }
                    }
                    #endregion

                    #region Updata Job Data By Fab Type
                    if (job != null)
                    {
                        Line line = ObjectManager.LineManager.GetLine(lineID);
                        if (line != null)
                        {
                            switch (line.Data.FABTYPE.ToUpper())
                            {
                                case "ARRAY":
                                    break;

                                case "CF":
                                    break;

                                case "CELL":
                                    if(eqp.Data.NODEID.Contains("CCPIN")&&job.OWNERTYPE.ToUpper()=="P")
                                    {
                                        if (decodeItemName=="Particle_Sum")
                                        {
                                            if(int.Parse(itemValue)>=ParameterManager["PINPARTICLECOUNTMONITOR"].GetInteger())
                                            {
                                                HoldInfo _hd = new HoldInfo();
                                                _hd.NodeID = eqp.Data.NODEID;
                                                _hd.OperatorID = "BCAuto";
                                                _hd.HoldReason = string.Format("CCPIN Particle_Sum[{0}]>=PINPARTICLECOUNTMONITOR[{1}]", itemValue, ParameterManager["PINPARTICLECOUNTMONITOR"].GetInteger());
                                                job.HoldInforList.Add(_hd);
                                            }
                                        }
                                    }
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                    #endregion

                    #region Updata Equipment Data By Fab Type
                    if (eqp != null)
                    {
                        Line line = ObjectManager.LineManager.GetLine(lineID);
                        if (line != null)
                        {
                            switch (line.Data.FABTYPE.ToUpper())
                            {
                                case "ARRAY":
                                    if (line.Data.LINETYPE != eLineType.ARRAY.MSP_ULVAC && 
                                        line.Data.LINETYPE != eLineType.ARRAY.ITO_ULVAC) break;
                                    #region Array Material 
                                    /*
                                        *    MES : ChangeTargetLife
                                        *    MaterialType => target life
                                        */
                                    string key = string.Format("{0}_PROCESS_DATA_TARGETLIFE", line.Data.LINETYPE);

                                    ConstantData _constantdata = ConstantManager[key];

                                    if (_constantdata != null && _constantdata[decodeItemName.Trim()].Value.ToUpper() == "TRUE")
                                    {
                                        ChangeTargetLife.CHAMBERc obj = new ChangeTargetLife.CHAMBERc()
                                        {
                                            CHAMBERID = job.ChamberName,
                                            QUANTITY = itemValue
                                        };

                                        chamberList.Add(obj);
                                    }

                                    /*
                                        *    MES : ChangePVDMaterialLife
                                        *    MaterialType => target life,
                                        *    PVDTRAY, PVDCARRIER, PVDMASK
                                        */

                                    key = string.Format("{0}_PROCESS_DATA_PVDTRAY", line.Data.LINETYPE);
                                    _constantdata = ConstantManager[key];
                                    if (_constantdata != null && _constantdata[decodeItemName.Trim()].Value.ToUpper() == "TRUE")
                                    {
                                        ChangePVDMaterialLife.CHAMBERc obj = new ChangePVDMaterialLife.CHAMBERc()
                                        {
                                            MATERIALTYPE = "PVDTRAY",
                                            CHAMBERID = job.ChamberName,
                                            QUANTITY = itemValue
                                        };

                                        PVDchamberList.Add(obj);
                                    }

                                    key = string.Format("{0}_PROCESS_DATA_PVDCARRIER", line.Data.LINETYPE);
                                    _constantdata = ConstantManager[key];
                                    if (_constantdata != null && _constantdata[decodeItemName.Trim()].Value.ToUpper() == "TRUE")
                                    {
                                        ChangePVDMaterialLife.CHAMBERc obj = new ChangePVDMaterialLife.CHAMBERc()
                                        {
                                            MATERIALTYPE = "PVDCARRIER",
                                            CHAMBERID = job.ChamberName,
                                            QUANTITY = itemValue
                                        };

                                        PVDchamberList.Add(obj);
                                    }

                                    key = string.Format("{0}_PROCESS_DATA_PVDMASK", line.Data.LINETYPE);
                                    _constantdata = ConstantManager[key];
                                    if (_constantdata != null && _constantdata[decodeItemName.Trim()].Value.ToUpper() == "TRUE")
                                    {
                                        ChangePVDMaterialLife.CHAMBERc obj = new ChangePVDMaterialLife.CHAMBERc()
                                        {
                                            MATERIALTYPE = "PVDMASK",
                                            CHAMBERID = job.ChamberName,
                                            QUANTITY = itemValue
                                        };

                                        PVDchamberList.Add(obj);
                                    }

                                    
                                    break;
                                    #endregion
                                case "CF":
                                    if (eqp.Data.NODEATTRIBUTE.ToUpper() == "EXPOSURE")
                                    {
                                        // MaskUsedCountReport
                                        if (pd.Data.PARAMETERNAME == "MSK_SHOT_CNT")
                                        {
                                            lock (job)
                                            {
                                                job.CfSpecial.MaskUseCount = itemValue.Trim();
                                            }
                                            ObjectManager.JobManager.EnqueueSave(job);
                                        }

                                        if (pd.Data.PARAMETERNAME == "ALGA_MASK_ID")
                                        {
                                            lock (job)
                                            {
                                                job.CfSpecial.MaskID = itemValue.Trim();
                                            }
                                            ObjectManager.JobManager.EnqueueSave(job);
                                        }

                                    }

                                    if (eqp.Data.NODEATTRIBUTE.ToUpper() == "COATER")
                                    {
                                        if (pd.Data.PARAMETERNAME == "Total_Discharge")
                                        {
                                            lock (job)
                                            {
                                                job.CfSpecial.TotalDischarge = itemValue.Trim();
                                            }
                                            ObjectManager.JobManager.EnqueueSave(job);
                                        }

                                        if (pd.Data.PARAMETERNAME == "Initial_dis_volume")
                                        {
                                            lock (job)
                                            {
                                                job.CfSpecial.InitialDisVolume = itemValue.Trim();
                                            }
                                            ObjectManager.JobManager.EnqueueSave(job);
                                        }

                                        if (pd.Data.PARAMETERNAME == "PR_ID")
                                        {
                                            lock (job)
                                            {
                                                job.CfSpecial.PRID = itemValue.Trim();
                                                job.CfSpecial.AbnormalCode.PRLOT = itemValue.Trim();
                                            }
                                            ObjectManager.JobManager.EnqueueSave(job);
                                        }

                                        if (pd.Data.PARAMETERNAME == "Dispense_speed")
                                        {
                                            lock (job)
                                            {
                                                job.CfSpecial.AbnormalCode.DISPENSESPEED = itemValue.Trim();
                                            }
                                            ObjectManager.JobManager.EnqueueSave(job);
                                        }
                                    }
                                    break;

                                case "CELL":
                                    break;

                                default:
                                    break;
                            }
                        }
                    }
                    #endregion

                    tmpList = tmpList + pd.Data.PARAMETERNAME + "=" + itemValue.ToString() + ",";
                }

                decodeItemName = "";

                #region CF Report to MES
                if (eqp != null)
                {
                    if (eqp.Data.NODEATTRIBUTE.ToUpper() == "EXPOSURE")
                    {
                        IList<MaskUsedCountReport.MASKc> maskList = new List<MaskUsedCountReport.MASKc>();
                        MaskUsedCountReport.MASKc msc = new MaskUsedCountReport.MASKc();
                        msc.MASKNAME = job.CfSpecial.MaskID;
                        msc.MASKUSECOUNT = job.CfSpecial.MaskUseCount;
                        maskList.Add(msc);
                        object[] _data = new object[4]
                    { 
                        trackKey,             /*0  TrackKey*/
                        eqp.Data.LINEID,      /*1  LineName*/
                        eqp.Data.NODEID,      /*2  machineName*/
                        maskList,             /*3  maskList*/
                    };
                        //呼叫MES方法
                       // Invoke(eServiceName.MESService, "MaskUsedCountReport", _data);
                    }

                    if (eqp.Data.NODEATTRIBUTE.ToUpper() == "COATER")
                    {

                        string productName = string.Empty;
                        string materialName = job.CfSpecial.PRID.Trim();
                        string materialtype = "PR";
                        double initial_Dis_Volume = double.Parse(job.CfSpecial.InitialDisVolume);
                        double total_Discharge = double.Parse(job.CfSpecial.TotalDischarge);

                        //計算Coater上報的數值 (Initial_dis_volume + Total_Discharge)/1000
                        double accumulativevalue = (initial_Dis_Volume + total_Discharge) / 1000;

                        //取出機台內任一片玻璃的 Job ID 上報，若則無須上報。
                        productName = ObjectManager.JobManager.GetJobIDbyEQPNO(eqpNo);

                        #region [Report MES]
                        object[] _data = new object[7]
                        { 
                        trackKey,                      /*0  TrackKey*/
                        eqp.Data.LINEID,               /*1  LineName*/
                        eqp.Data.NODEID,               /*2  EQPID*/
                        materialName.Trim(),           /*3  MaterialName*/
                        materialtype,                  /*4  MaterialType*/
                        productName,                   /*5  pnlID*/
                        accumulativevalue.ToString(),  /*6  materialqty*/
                        };
                        //呼叫MES方法
                        // Invoke(eServiceName.MESService, "AutoDecreaseMaterialQuantity", _data);  --CSOT MES  WANGFANG Confrim Not Report Coaster
                        #endregion
                    }
                }
                #endregion

                if (chamberList.Count > 0)
                    Invoke(eServiceName.MESService, "ChangeTargetLife", new object[] { trackKey, lineID, eqp.Data.NODEID, chamberList });

                if (PVDchamberList.Count > 0)
                    Invoke(eServiceName.MESService, "ChangePVDMaterialLife", new object[] { trackKey, lineID, eqp.Data.NODEID, job.GlassChipMaskBlockID, chamberList });

                foreach (ProductProcessData.ITEMc itemName in itemList)
                {
                    if (itemName.SITELIST.Count == 1)
                    {
                        itemName.SITELIST[0].SITENAME = "DEFAULT";
                    }
                }

                if (tmpList.Length > 0)
                    paraList = tmpList.Substring(0, tmpList.Length - 1);
                
                if (_dataMES != null)
                {
                    //to MES format: Dictionary<item,Dictionary<site,value>>
                    if (itemList.Count > 0)
                    {
                        _dataMES[11] = itemList;
                        Invoke(eServiceName.MESService, "ProductProcessData", _dataMES);
                    }
                }

                if (_dataEDA != null)
                {
                    //to do EDA format: List<item;site;value>
                    if (edalis.Count > 0)
                    {
                        _dataEDA[7] = edalis;
                        Invoke(eServiceName.EDAService, "EDAReport", _dataEDA);
                    }
                }
            }
            catch (Exception ex)
            {
                paraList = "";
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", 
                    string.Format("DECODE_ITEM=[{0}] ERR_MSG=[{1}].", decodeItemName, ex.ToString()));//"DECODE_ITEM (" + decodeItemName + "), " + ex);
            }
        }

        //Jun Add 20150328 For OPI Query Process Data By No
        private void HandleProcessDatabyOPIQuery(string eqpNo, string lineID, short[] processData, out string paraList)
        {
            string decodeItemName = "";

            try
            {
                string tmpList = "";
                paraList = "";

                //取得EDC Format
                IList<ProcessData> processDatas = ObjectManager.ProcessDataManager.GetProcessData(eqpNo);
                if (processDatas == null) return;

                string value = string.Empty;
                int startaddress10 = 0;

                foreach (ProcessData pd in processDatas)
                {
                    decodeItemName = pd.Data.PARAMETERNAME;

                    ItemExpressionEnum ie;
                    if (!Enum.TryParse(pd.Data.EXPRESSION.ToUpper(), out ie))
                    {
                        continue;
                    }
                    #region decode by expression
                    switch (ie)
                    {
                        case ItemExpressionEnum.BIT:
                            value = ExpressionBIT.Decode(startaddress10, int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), processData);
                            break;
                        case ItemExpressionEnum.ASCII:
                            value = ExpressionASCII.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), processData);
                            value = Regex.Replace(value, @"[^\x21-\x7E]|<|>|'", " ");//过滤不可显示的字符 20150211 tom
                            break;
                        case ItemExpressionEnum.BIN:
                            value = ExpressionBIN.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), processData);
                            break;
                        case ItemExpressionEnum.EXP:
                            //add for EXP Err - just get process data[0],[1] 2016/05/30
                            /*
                            short[] processDataEXP = new short[2];
                            Array.Copy(processData, int.Parse(pd.Data.WOFFSET), processDataEXP, 0, 2);
                            value = ExpressionEXP.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), processDataEXP).ToString();
                            */
                            value = ExpressionEXP.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), processData).ToString();
                            break;
                        case ItemExpressionEnum.HEX:
                            value = ExpressionHEX.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), processData);
                            break;
                        case ItemExpressionEnum.INT:
                            value = ExpressionINT.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), processData).ToString();
                            break;
                        case ItemExpressionEnum.LONG:
                            value = ExpressionLONG.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), processData).ToString();
                            break;
                        case ItemExpressionEnum.SINT:
                            value = ExpressionSINT.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), processData).ToString();
                            break;
                        case ItemExpressionEnum.SLONG:
                            value = ExpressionSLONG.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), processData).ToString();
                            break;
                        case ItemExpressionEnum.BCD:
                            value = ExpressionBCD.Decode(startaddress10, int.Parse(pd.Data.WOFFSET), int.Parse(pd.Data.WPOINTS), int.Parse(pd.Data.BOFFSET), int.Parse(pd.Data.BPOINTS), processData).ToString();
                            break;
                        default:
                            break;
                    }
                    #endregion

                    #region 算法
                    string itemValue = string.Empty;
                    switch (pd.Data.OPERATOR)
                    {
                        case ArithmeticOperator.PlusSign:
                            itemValue = (double.Parse(value) + double.Parse(pd.Data.DOTRATIO)).ToString();
                            break;
                        case ArithmeticOperator.MinusSign:
                            itemValue = (double.Parse(value) - double.Parse(pd.Data.DOTRATIO)).ToString();
                            break;
                        case ArithmeticOperator.TimesSign:
                            itemValue = (double.Parse(value) * double.Parse(pd.Data.DOTRATIO)).ToString();
                            break;
                        case ArithmeticOperator.DivisionSign:
                            itemValue = (double.Parse(value) / double.Parse(pd.Data.DOTRATIO)).ToString();
                            break;
                        default:
                            itemValue = value;
                            break;
                    }
                    #endregion

                    tmpList = tmpList + pd.Data.PARAMETERNAME + "=" + itemValue.ToString() + ",";
                }

                decodeItemName = "";

                if (tmpList.Length > 0)
                    paraList = tmpList.Substring(0, tmpList.Length - 1);
            }
            catch (Exception ex)
            {
                paraList = "";
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                    string.Format("DECODE_ITEM=[{0}] ERR_MSG=[{1}].", decodeItemName, ex.ToString()));//"DECODE_ITEM (" + decodeItemName + "), " + ex);
            }
        }

        /// <summary>
        /// Process Data Request By Node No
        /// </summary>
        public bool ProcessDataRequestByNO(string eqpNo, out List<string> parameter, out string desc)
        {
            parameter = new List<string>();
            desc = "";

            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
                if (eqp == null)
                    throw new Exception(string.Format("CAN'T FIND EQUIPMENT_NO =[{0}] IN EQUIPMENTENTITY!", eqpNo));

                string plc_trx_name = string.Format("{0}_ProcessDataReport", eqpNo);
                string paraList = string.Empty;
                if (dicReportBy.ContainsKey(eqpNo))
                {
                    // 機台以 EDC File 上報 Process Data, OPI 查詢此機台的 Process Data 時, 要從倉庫讀取
                    EDCFile edc_file = Repository.Get(plc_trx_name) as EDCFile;
                    if (edc_file != null)
                        paraList = edc_file.GetParaList();
                    else
                        throw new Exception(string.Format("Cannot Find EQUIPMENT_NO=[{0}] EDC File In BC Repository", eqpNo));
                }
                else
                {
                    // 機台以 PLC 上報 Process Data, OPI 查詢此機台的 Process Data 時, 直接從 PLC 讀取
                    //modify by edison20150119:follow新的PLC Agent SyncReadTrx方法，多加一个参数
                    Trx trx = Invoke(eAgentName.PLCAgent, "SyncReadTrx", new object[] { plc_trx_name, false }) as Trx;
                    if (trx == null)
                        throw new Exception(string.Format("CAN'T GET EQUIPMENT_NO =[{0}] DATA FROM PLC!", eqpNo));

                    short[] processData = trx.EventGroups[0].Events[1].RawData;
                    #region [For More EventBlock]
                    //針對 TRX多 EventBlock 特殊處理 超過3個EVENT ，1.BIT 2.Block 3 Item ....  sy add 20160525
                    if (trx.EventGroups[0].Events.Count > 3)
                    {
                        int itemEventCount = 0;//先算出總共有多少
                        List<int> startIndex = new List<int>();
                        for (int i = 1; i < trx.EventGroups[0].Events.Count - 1; i++)
                        {
                            startIndex.Add(itemEventCount);
                            itemEventCount += trx.EventGroups[0].Events[i].RawData.Length;
                        }
                        short[] processDataTmp = new short[itemEventCount];
                        int EventNo = 1;
                        for (int j = 0; j < processDataTmp.Length; j++)//將對應的數值 重新排序
                        {
                            processDataTmp[j] = trx.EventGroups[0].Events[EventNo].RawData[j - startIndex[EventNo - 1]];
                            if (startIndex.Count > EventNo)
                                if (startIndex[EventNo] - 1 == j)
                                    EventNo++;
                        }

                        processData = processDataTmp;
                    }
                    #endregion                    
                    //Jun Modify 20150328 新增Mothed For OPI Query Process Data
                    //HandleProcessData(eqpNo, eqp.Data.LINEID, eqp, null, processData, null, null, out paraList, trx.TrackKey);
                    HandleProcessDatabyOPIQuery(eqpNo, eqp.Data.LINEID, processData, out paraList);

                    if (string.IsNullOrEmpty(paraList))
                        throw new Exception(string.Format("CAN'T DECODE EQUIPMENT_NO =[{0}] PLC DATA, OR EQUIPMENT_NO =[{0}] PROCESS DATA ITEM SETTING PROBLEM IN DB!", eqpNo));
                }

                string[] tmpList = paraList.Split(',');
                foreach (string tmp in tmpList)
                {
                    parameter.Add(tmp);
                }
                return true;
            }
            catch (Exception ex)
            {
                desc = ex.Message.ToString();

                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }


        private void SendPLCData(Trx outputData)
        {
            xMessage msg = new xMessage();
            msg.Data = outputData;
            msg.ToAgent = eAgentName.PLCAgent;
            PutMessage(msg);
        }

        private void TEST_CREATE_JOBS()
        {
            try
            {
                //呼叫MES方法
                //object[] _data = new object[14]
                //{ 
                //    inputData.TrackKey,  /*0 TrackKey*/
                //    eqp.Data.LINEID,    /*1 LineName*/
                //    eqp.Data.NODEID,    /*2 EQPID*/
                //    "",          /*3 LINERECIPENAME*/
                //    "",            /*4 MATERIALMODE*/ 
                //    "",           /*5 PRODUCTNAME*/
                //    "13431423414314",           /*6 MATERIALNAME*/
                //    eMaterialStatus.MOUNT,           /*7 MATERIALSTATE*/
                //    ""            ,/*8 MATERIALTYPE*/
                //    ""            , /*9 USEDCOUNT*/
                //    "L5557788"            ,/*10 LIFEQTIME*/
                //    ""            ,/*11 GROUPID*/
                //    ""            ,/*12 UNITID*/
                //    "HH13433"            /*13 HEADID*/
                //};
                //Invoke(eServiceName.MESService, "MaterialStateChanged", _data);


                //IList<string> box = new List<string>();
                //box.Add("BB143143243");
                //box.Add("AAeq15313413A");
                //_data = new object[5]
                //{ 
                //    inputData.TrackKey,  /*0 TrackKey*/
                //    eqp.Data.LINEID,    /*1 LineName*/
                //    "3",    /*2 boxQty*/
                //    "ppid3134141",          /*3 LINERECIPENAME*/
                //    box         /*4 bOXIDList*/ 
                //};
                //Invoke(eServiceName.MESService, "BoxProcessStarted", _data);

                Random rand = new Random();

                int j = ObjectManager.JobManager.GetJobCount();

                for (int i = 0; i <= 100; i++)
                {
                    Job job = new Job(1, i + 1);
                    //ObjectManager.JobManager.NewJob(job.CassetteSequenceNo, job.JobSequenceNo);
                    //

                    if (job == null)
                        continue;
                    job.GroupIndex = rand.Next(1, 65535).ToString();
                    job.ProductType.Value = rand.Next(1, 65535);
                    job.CSTOperationMode = (eCSTOperationMode)int.Parse(rand.Next(0, 1).ToString());
                    job.SubstrateType = (eSubstrateType)int.Parse(rand.Next(0, 3).ToString());
                    job.CIMMode = (eBitResult)int.Parse(rand.Next(0, 1).ToString());
                    job.JobType = (eJobType)int.Parse(rand.Next(1, 6).ToString());
                    job.JobJudge = rand.Next(0, 8).ToString();
                    job.SamplingSlotFlag = rand.Next(0, 1).ToString();
                    job.OXRInformationRequestFlag = rand.Next(0, 1).ToString();
                    //job. = eVent.Items[11].Value.ToString(); //Reserve
                    job.FirstRunFlag = rand.Next(0, 1).ToString();
                    job.JobGrade = rand.Next(0, 1).ToString();
                    job.GlassChipMaskBlockID = "GLASS" + rand.Next(11111111, 99999999).ToString();
                    job.PPID = "PPID3143141341341";
                    //job.INSPReservations = rand.Next(0, 768).ToString();
                    //job.EQPReservations = rand.Next(0, 235).ToString();
                    job.LastGlassFlag = rand.Next(0, 1).ToString();
                    //job. = eVent.Items[19].Value.ToString();//Reserve
                    if (job.CfSpecial == null)
                        job.CfSpecial = new JobCfSpecial();
                    job.InspJudgedData = rand.Next(0, 1).ToString();
                    job.CFSpecialReserved = rand.Next(0, 1).ToString();
                    job.TrackingData = rand.Next(0, 1).ToString();
                    job.CFSpecialReserved = rand.Next(0, 1).ToString();
                    //job.EQPFlag = rand.Next(0, 1).ToString();
                    job.OXRInformation = rand.Next(0, 4).ToString();
                    job.ChipCount = rand.Next(0, 255);
                    //job.File = eVent.Items[27].Value.ToString();//Reserve
                    job.CfSpecial.COAversion = "AB";
                    job.CfSpecial.DummyUsedCount = rand.Next(0, 5).ToString();

                    LOTc lot = new LOTc();
                                        
                    lot.LOTNAME = "Lot";
                    lot.PRODUCTSPECNAME = "SpecName";
                    lot.PRODUCTSPECVER = "SpecVer";
                    lot.PROCESSOPERATIONNAME = "OperationName";
                    lot.LINERECIPENAME = "RecipeID";
                    job.MesCstBody.LOTLIST.Add(lot);

                    ObjectManager.JobManager.AddJob(job);
                    ObjectManager.JobManager.EnqueueSave(job);
                    //System.Threading.Thread.Sleep(300);
                }
                int w = ObjectManager.JobManager.GetJobs().Count;
            }
            catch (Exception ex)
            {
                this.Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
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
        /// 從 Share Folder 下載檔案, glassID可填string.Empty
        /// CF廠從info.SourcePath抓檔案, CELL廠從info.SourcePath的子資料夾抓檔案, ARRAY廠不抓檔
        /// </summary>
        /// <param name="info"></param>
        /// <param name="glassID"></param>
        /// <param name="remoteFiles"></param>
        /// <returns></returns>
        private bool GetFileListFromShareFolder(ShareFolderInfo info, string glassID, out List<string> remoteFiles)
        {
            remoteFiles = new List<string>();
            try
            {
                //CF廠, info.SourcePath 指定到Node, 如:"\\172.17.44.118\E$\Result\ProcessDataFile\FBGPH100\FBGAO1D0\EDC\"
                //EDC File會放在info.SourcePath之中
                //------------------------------------------------------------------------------------------------------
                //CELL廠, info.SourcePath 指定到Node, 如:"\\172.17.45.9\ProcessDataByFile\CBODF200\CBSDP250"
                //但是EDC File會放在Unit資料夾內,      如:"\\172.17.45.9\ProcessDataByFile\CBODF200\CBSDP250\CBSDP251"
                //因此CELL廠必須抓info.SourcePath的子資料夾中的檔案
                //------------------------------------------------------------------------------------------------------
                //ARRAY廠, Process Data Report 係依靠PLC 及 SECS, 沒有 EDC File

                if (_fabType == FABTYPE.CF || _fabType == FABTYPE.ARRAY)
                {
                    GetFileListFromShareFolder(info.SourcePath, glassID, remoteFiles);
                    //CF的Process Data Report by EDC File是由PLC Trigger, 因此不管取到多少File都要記LOG
                    Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Get Glass[{0}] file list count[{1}] from ShareFolder[{2}] Success.", glassID, remoteFiles.Count, info.SourcePath));
                }
                else if (_fabType == FABTYPE.CELL)
                {
                    string[] dirs = Directory.GetDirectories(info.SourcePath);
                    GetFileListFromShareFolder(info.SourcePath, glassID, remoteFiles);
                    foreach (string dir in dirs)
                    {
                        GetFileListFromShareFolder(dir, glassID, remoteFiles);
                    }
                    if (remoteFiles.Count > 0)
                    {
                        //CELL的Process Data Report by EDC File是由BC Thread定時去搜尋, 因此有取到File才要記LOG
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Get Glass[{0}] file list count[{1}] from ShareFolder[{2}] Success.", glassID, remoteFiles.Count, info.SourcePath));
                    }
                }
            }
            catch (Exception ex)
            {
                info.Connected = false;
                Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
            return info.Connected;
        }

        private void GetFileListFromShareFolder(string folder, string glassID, List<string> remoteFiles)
        {
            string[] remote_files = Directory.GetFiles(folder);
            if (remote_files != null && remote_files.Length > 0)
            {
                foreach (string remote_file in remote_files)
                {
                    if (EDCFile.IsEDCFile(glassID, remote_file))
                    {
                        remoteFiles.Add(remote_file);
                    }
                }
            }
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
        /// 從遠端共享資料夾下載檔案, glassID可填string.Empty
        /// CF廠從info.SourcePath抓檔案, CELL廠從info.SourcePath的子資料夾抓檔案, ARRAY廠不抓檔
        /// </summary>
        /// <param name="remoteFolder">遠端資料夾(\\remote ip\LineID\ToolID\EDC)</param>
        /// <param name="glassID">GlassID, 用來組檔名(GlassID_EDC_YYYYMMDDmmhhss.dat), 可填string.Empty</param>
        /// <param name="loginUserName">登入遠端電腦的帳號</param>
        /// <param name="password">密碼</param>
        /// <param name="localFolder">下載存檔到本地端資料夾(D:\Folder)</param>
        /// <param name="deleteRemoteFile">true:表示刪除遠端檔案, false:表示保留遠端檔案</param>
        /// <returns>下載後的本地端檔名, 不含路徑</returns>
        private List<string> DownloadGlassFileFromShareFolder(ShareFolderInfo info, string glassID)
        {
            List<string> ret = new List<string>();
            try
            {
                if (!info.Connected)
                    OpenShareFolderConnection(info);

                List<string> remote_files = null, delete_files = new List<string>();
                if (info.Connected && GetFileListFromShareFolder(info, glassID, out remote_files))
                {
                    if (remote_files != null && remote_files.Count > 0)
                    {
                        #region 從共享資料夾下載檔案至本地端
                        {
                            foreach (string remote_file in remote_files)
                            {
                                try
                                {
                                    string date_str = EDCFile.GetDateStrFromFilename(remote_file);
                                    string path = Path.Combine(info.LocalTargetPath, date_str);
                                    if (!Directory.Exists(path))
                                        Directory.CreateDirectory(path);

                                    string fname = Path.GetFileName(remote_file);
                                    string target = Path.Combine(path, fname);
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
        /// 從FTP Server取檔案清單, glassID可填string.Empty
        /// </summary>
        /// <param name="ftpFolder">FTP資料夾</param>
        /// <param name="glassID">GlassID, 用來組檔名(GlassID_EDC_YYYYMMDDmmhhss.dat)</param>
        /// <param name="loginUsername">登入FTP Server的帳號</param>
        /// <param name="password">密碼</param>
        /// <param name="subFolder">指定ftpFolder內的子資料夾</param>
        /// <returns>FTP Server上的檔名, 不包含FTP路徑</returns>
        private List<string> GetFileListFromFtp(FTPInfo ftpInfo, string ftpFolder, string glassID, string loginUsername, string password, out List<string> subFolder)
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
                            fname = fname.Substring(start_idx).Trim();
                            if (EDCFile.IsEDCFile(glassID, fname))
                                ret.Add(fname);
                        }
                        fname = sr.ReadLine();
                    }
                }
                #region LOG, 記錄從ftpFolder中找到幾個檔案
                {
                    if (_fabType == FABTYPE.CF || _fabType == FABTYPE.ARRAY)
                    {
                        //CF廠是直接從info.SourcePath抓取檔案, 所以要記錄從ftpFolder中找到幾個檔案
                        Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Get Glass[{0}] file list count[{1}] from FTP[{2}] Success.", glassID, ret.Count, ftpFolder));
                    }
                    else if (_fabType == FABTYPE.CELL)
                    {
                        //CELL廠是從info.SourcePath下的子資料夾抓取檔案, 所以只記錄從子資料夾中找到幾個檔案
                        //CELL廠是BC定時去搜尋檔案, 所以搜尋到0個檔案不記LOG
                        if (ftpInfo.SourcePath != ftpFolder && ret.Count > 0)
                        {
                            Logger.LogInfoWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", string.Format("Get Glass[{0}] file list count[{1}] from FTP[{2}] Success.", glassID, ret.Count, ftpFolder));
                        }
                    }
                }
                #endregion
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

        /// <summary>
        /// 從FTP取Glass的EDC檔名並下載, glassID可填string.Empty
        /// </summary>
        /// <param name="ftpFolder"></param>
        /// <param name="glassID"></param>
        /// <param name="loginUsername"></param>
        /// <param name="password"></param>
        /// <param name="localFolder"></param>
        /// <param name="deleteFtpFile"></param>
        /// <returns>下載到本地端的檔案</returns>
        private List<string> DownloadGlassFileFromFtp(FTPInfo info, string glassID)
        {
            List<string> ret = new List<string>();
            try
            {
                int count = 1, retry = ParameterManager.Parameters["EDC_REPORT_BY_FTP_RETRY_COUNT"].GetInteger();
                if (retry > 0) count += retry;
                Dictionary<string, List<string>> ftp_file_downloadlist = new Dictionary<string,List<string>>(), ftp_file_deletelist = new Dictionary<string, List<string>>();
                
                //取檔案清單, 下載檔案, 刪除檔案. 這三個動作至少會做一次, 若有例外則總共會重試retry次
                //取檔案清單及下載檔案時出現例外, 則這兩個動作會全部重試, 重試的次數遞減直至零
                //刪除檔案時出現例外, 已經刪掉的檔案不會復原, 尚未刪除的檔案會重試, 重試的次數遞減直至零
                #region 取檔案清單及下載檔案
                {
                    while (count > 0)//重試次數
                    {
                        try
                        {
                            if (_fabType == FABTYPE.CF || _fabType == FABTYPE.ARRAY)
                            {
                                #region CF ARRAY, 下載info.SourcePath中的檔案
                                {
                                    List<string> sub_folders = null;
                                    List<string> file_list = GetFileListFromFtp(info, info.SourcePath, glassID, info.LoginID, info.Password, out sub_folders);
                                    ftp_file_downloadlist.Add(info.SourcePath, file_list);

                                    if (file_list != null && file_list.Count > 0)
                                    {
                                        List<string> delete_list = new List<string>();
                                        foreach (string ftp_file in file_list)
                                        {
                                            string local_fname = string.Empty;
                                            string date_str = EDCFile.GetDateStrFromFilename(ftp_file);
                                            string path = Path.Combine(info.LocalTargetPath, date_str);
                                            if (!Directory.Exists(path))
                                                Directory.CreateDirectory(path);

                                            if (DownloadFileFromFtp(info.SourcePath, ftp_file, info.LoginID, info.Password, path, out local_fname))
                                            {
                                                ret.Add(local_fname);
                                                delete_list.Add(ftp_file);
                                            }
                                        }
                                        ftp_file_deletelist.Add(info.SourcePath, delete_list);
                                    }
                                }
                                #endregion
                            }
                            else if (_fabType == FABTYPE.CELL)
                            {
                                #region CELL, 下載info.SourcePath的子資料夾中的檔案
                                {
                                    List<string> sub_folders = null;
                                    List<string> file_list_tmp = GetFileListFromFtp(info, info.SourcePath, glassID, info.LoginID, info.Password, out sub_folders);
                                    ftp_file_downloadlist.Add(info.SourcePath, file_list_tmp);
                                    if (sub_folders != null && sub_folders.Count > 0)
                                    {
                                        foreach (string sub_folder in sub_folders)
                                        {
                                            List<string> tmp = null;
                                            string path = string.Format("{0}/{1}", info.SourcePath, sub_folder);
                                            List<string> file_list = GetFileListFromFtp(info, path, glassID, info.LoginID, info.Password, out tmp);
                                            ftp_file_downloadlist.Add(path, file_list);
                                        }
                                    }

                                    if (!Directory.Exists(info.LocalTargetPath))
                                        Directory.CreateDirectory(info.LocalTargetPath);

                                    foreach (string ftp_path in ftp_file_downloadlist.Keys)
                                    {
                                        List<string> delete_list = new List<string>();
                                        List<string> file_list = ftp_file_downloadlist[ftp_path];
                                        foreach (string ftp_file in file_list)
                                        {
                                            string local_fname = string.Empty;
                                            string date_str = EDCFile.GetDateStrFromFilename(ftp_file);
                                            string local_path = Path.Combine(info.LocalTargetPath, date_str);
                                            if (!Directory.Exists(local_path))
                                                Directory.CreateDirectory(local_path);

                                            if (DownloadFileFromFtp(ftp_path, ftp_file, info.LoginID, info.Password, local_path, out local_fname))
                                            {
                                                ret.Add(local_fname);
                                                delete_list.Add(ftp_file);
                                            }
                                        }
                                        ftp_file_deletelist.Add(ftp_path, delete_list);
                                    }
                                }
                                #endregion
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
                            List<EDCFile> edc_files = new List<EDCFile>();
                            List<string> local_files = DownloadGlassFileFromShareFolder(info, string.Empty);
                            info.LastScanTime = DateTime.Now;

                            foreach (string local_fname in local_files)
                            {
                                try
                                {
                                    EDCFile edc_file = EDCFile.Load(Logger, this.LogName, info.NodeNo, local_fname);
                                    if (edc_file != null)
                                    {
                                        edc_files.Add(edc_file);
                                        ProcessDataReport_ByEDCFile(info.NodeNo, edc_file.DateTimeStr, edc_file);
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()-Load EDC File and Report", exception);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()-Scan and Download EDC File", ex);
                    }
                }
                Thread.Sleep(500);
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
                            List<EDCFile> edc_files = new List<EDCFile>();
                            List<string> local_files = DownloadGlassFileFromFtp(info, string.Empty);
                            info.LastScanTime = DateTime.Now;

                            foreach (string local_fname in local_files)
                            {
                                try
                                {
                                    EDCFile edc_file = EDCFile.Load(Logger, this.LogName, info.NodeNo, local_fname);
                                    if (edc_file != null)
                                    {
                                        edc_files.Add(edc_file);
                                        ProcessDataReport_ByEDCFile(info.NodeNo, edc_file.DateTimeStr, edc_file);
                                    }
                                }
                                catch (Exception exception)
                                {
                                    Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()-Load EDC File and Report", exception);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogErrorWrite(this.LogName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()-Scan and Download EDC File", ex);
                    }
                }
                Thread.Sleep(500);
            }
        }
    }
}
