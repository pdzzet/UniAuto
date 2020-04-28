using System;
using System.Collections.Generic;
using System.Reflection;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.DB;
using System.Collections;
using UniAuto.UniBCS.Core;
using System.Threading;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using System.Timers;

namespace UniAuto.UniBCS.EntityManager
{
    public class ProcessDataManager:IDataSource
    {
        /// <summary>
        /// 各机台的EDC设定档
        /// </summary>
        private IDictionary<string, IList<ProcessData>> _processDatas = new Dictionary<string, IList<ProcessData>>();

        private System.Timers.Timer _timer;//sy add 20160824 定时删除ProcessData的Timer 默认值1小时檢查一次

        /// <summary>
        /// NLog Logger Name
        /// </summary>
        public string LoggerName { get; set; }

        private string _historyPath;
        
        /// <summary>
        /// Histrory  File Path
        /// </summary>
        public string HistoryPath
        {
            get
            {
                if (string.IsNullOrEmpty(_historyPath))
                {
                    _historyPath = "..\\Data\\ProcessData\\";
                }

                return _historyPath;               
            }
          set
            {
                _historyPath = value;
                if (_historyPath[_historyPath.Length - 1] != '\\')
                {
                    _historyPath = _historyPath + "\\";
                }
            }
        }

        /// <summary>
        /// 用來讀取DB
        /// </summary>
        public HibernateAdapter HibernateAdapter { get; set; }

        public void Init()
        {
            _processDatas = Reload();
            HistoryPath = HistoryPath.Replace("{ServerName}", Workbench.ServerName);
            AfterInit();//sy modify 20160824 By小時檢查
        }

        protected IDictionary<string, IList<ProcessData>> Reload()
        {
            try
            {
                IDictionary<string, IList<ProcessData>> processDatas = new Dictionary<string, IList<ProcessData>>();
                string hql = string.Format("from ProcessDataEntityData where LINETYPE = '{0}' order by NODENO, OBJECTKEY", Workbench.LineType);
                IList list = this.HibernateAdapter.GetObjectByQuery(hql);
                IList<ProcessData> processDataList = null;
                if (list != null)
                {
                    foreach (ProcessDataEntityData data in list)
                    {
                        //data.PARAMETERNAME = data.PARAMETERNAME.Replace(',', ';').Replace('=', '-').Replace('<','(').Replace('>',')').Replace('/','-');//,及=會影響ProcessDataService.HandleProcessData()組字串
                        data.PARAMETERNAME = Regex.Replace(data.PARAMETERNAME, @"[/| |?|/|<|>|'|\-|,]", "_");
                        if (!processDatas.ContainsKey(data.NODENO))
                        {
                            processDataList = new List<ProcessData>();
                            processDatas.Add(data.NODENO, processDataList);
                        }
                        processDatas[data.NODENO].Add(new ProcessData(data));
                    }
                }
                return processDatas;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }

       
        /// <summary>
        /// 取得EDC 设定档
        /// </summary>
        public IList<ProcessData> GetProcessData(string eqpNo)
        {
            try
            {
                if (_processDatas.ContainsKey(eqpNo))
                {
                    lock (_processDatas)
                    {
                        return _processDatas[eqpNo];
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        /// <summary>
        /// 取得EDC 设定档
        /// </summary>
        public IList<ProcessData> GetSMOProcessData(string eqpNo,eSMOEDCUnitType _type)
        {
            //20170810 huangjiayin add for t3 ODF SMO
            try
            {
                IList<ProcessData> _allProcessDatas;
                List<ProcessData> _ovenProcessDatas=new List<ProcessData>();
                List<ProcessData> _coolProcessDatas=new List<ProcessData>();
                if (_processDatas.ContainsKey(eqpNo))
                {
                    lock (_processDatas)
                    {
                        _allProcessDatas= _processDatas[eqpNo];
                    }

                    foreach (ProcessData pd in _allProcessDatas)
                    {
                        switch (pd.Data.REPORTUNITNO)
                        {
                            case 0:
                                _ovenProcessDatas.Add(pd);
                                _coolProcessDatas.Add(pd);
                                break;
                            case 1:
                                _ovenProcessDatas.Add(pd);
                                break;
                            case 2:
                                _coolProcessDatas.Add(pd);
                                break;
                            default:
                                _ovenProcessDatas.Add(pd);
                                _coolProcessDatas.Add(pd);
                                break;
                        }
                    }

                    switch (_type)
                    {
                        case eSMOEDCUnitType.OVEN:
                            return _ovenProcessDatas;
                        case eSMOEDCUnitType.COOL:
                            return _coolProcessDatas;
                    }

                }

                return null;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }



        #region Mark
        ///// <summary>
        ///// 创建设备一栏新的EDC设定档
        ///// </summary>
        //public bool CreateProcessDataProfile(string eqpNo, ProcessDataEntityData entity)
        //{
        //    try
        //    {
        //        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
        //        if (eqp == null)
        //        {
        //            Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                string.Format("Can't find the Equipment No [{0}]", eqpNo));
        //            return false;
        //        }

        //        List<ProcessDataEntityData> _processData;
        //        if (eqpProcessData.ContainsKey(eqpNo))
        //        {
        //            _processData = eqpProcessData[eqpNo];
        //            _processData.Add(entity);
        //        }
        //        else
        //        {
        //            _processData = new List<ProcessDataEntityData>();
        //            _processData.Add(entity);
        //            eqpProcessData.Add(eqpNo, _processData);
        //        }

        //        InsertDB(entity);
        //        eqp = null;
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// 创建设备一组新的EDC设定档
        ///// </summary>
        //public bool CreateProcessDataProfile(string eqpNo, List<ProcessDataEntityData> entity)
        //{
        //    try
        //    {
        //        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
        //        if (eqp == null)
        //        {
        //            Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                string.Format("Can't find the Equipment No [{0}]", eqpNo));
        //            return false;
        //        }

        //        List<ProcessDataEntityData> _processData;
        //        if (eqpProcessData.ContainsKey(eqpNo))
        //        {
        //            _processData = eqpProcessData[eqpNo];
        //            _processData.Concat(entity);
        //        }
        //        else
        //        {
        //            eqpProcessData.Add(eqpNo, entity);
        //        }

        //        // 2. 儲存到資料庫
        //        InsertAllDB(entity.ToArray());
        //        eqp = null;
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// 删除设备一栏EDC设定档
        ///// </summary>
        //public bool DeleteProcessDataProfile(string eqpNo, ProcessDataEntityData entity)
        //{
        //    try
        //    {
        //        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
        //        if (eqp == null)
        //        {
        //            Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                string.Format("Can't find the Equipment No [{0}]", eqpNo));
        //            return false;
        //        }

        //        List<ProcessDataEntityData> _processData;
        //        if (eqpProcessData.ContainsKey(eqpNo))
        //        {
        //            _processData = eqpProcessData[eqpNo];
        //            _processData.Remove(entity);
        //        }
        //        else
        //        {
        //            return false;
        //        }

        //        DeleteDB(entity);
        //        eqp = null;
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// 删除设备所有EDC设定档
        ///// </summary>
        //public bool DeleteProcessDataProfile(string eqpNo)
        //{
        //    try
        //    {
        //        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
        //        if (eqp == null)
        //        {
        //            Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                string.Format("Can't find the Equipment No [{0}]", eqpNo));
        //            return false;
        //        }

        //        List<ProcessDataEntityData> _processData;
        //        if (eqpProcessData.ContainsKey(eqpNo))
        //        {
        //            _processData = eqpProcessData[eqpNo];
        //            eqpProcessData.Remove(eqpNo);
        //        }
        //        else
        //        {
        //            return false;
        //        }

        //        DeleteAllDB(_processData.ToArray());
        //        _processData = null;
        //        eqp = null;
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// 更新设备一栏EDC设定档
        ///// </summary>
        //public bool UpdataProcessDataProfile(string eqpNo, ProcessDataEntityData oldentity, ProcessDataEntityData newentity)
        //{
        //    try
        //    {
        //        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
        //        if (eqp == null)
        //        {
        //            Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                string.Format("Can't find the Equipment No [{0}]", eqpNo));
        //            return false;
        //        }

        //        List<ProcessDataEntityData> _processData;
        //        if (eqpProcessData.ContainsKey(eqpNo))
        //        {
        //            _processData = eqpProcessData[eqpNo];
        //            _processData.Remove(oldentity);
        //            _processData.Add(newentity);
        //        }
        //        else
        //        {
        //            return false;
        //        }

        //        UpdateDB(newentity);
        //        _processData = null;
        //        eqp = null;
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// 更新设备所有EDC设定档
        ///// </summary>
        //public bool UpdataProcessDataProfile(string eqpNo, List<ProcessDataEntityData> entity)
        //{
        //    try
        //    {
        //        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
        //        if (eqp == null)
        //        {
        //            Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                string.Format("Can't find the Equipment No [{0}]", eqpNo));
        //            return false;
        //        }

        //        if (eqpProcessData.ContainsKey(eqpNo))
        //        {
        //            eqpProcessData[eqpNo].Clear();
        //            eqpProcessData[eqpNo] = entity;
        //        }
        //        else
        //        {
        //            return false;
        //        }

        //        UpdateAllDB(entity.ToArray());
        //        eqp = null;
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //        return false;
        //    }
        //}
        #endregion

        /// <summary>
        /// Reload DB Profile by All
        /// </summary>
        /// <returns></returns>
        public void ReloadAll()
        {
            try
            {
                IDictionary<string, IList<ProcessData>> tempDic = Reload();

                if (tempDic != null)
                {
                    lock (_processDatas)
                    {
                        _processDatas = tempDic;
                    }

                }
                NLogManager.Logger.LogInfoWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name,
                           string.Format("Reload Process Data all Equipment."));

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

            }
        }

        /// <summary>
        /// Reload DB Profile by Node
        /// </summary>
        /// <returns></returns>
        public void ReloadByNo(string eqpNo)
        {
            try
            {
                string hql = string.Format("from ProcessDataEntityData where LINETYPE = '{0}' and NODENO='{1}' order by SVID ", Workbench.LineType, eqpNo);
                IList list = this.HibernateAdapter.GetObjectByQuery(hql);
                IList<ProcessData> processDataList = new List<ProcessData>();
                if (list != null)
                {
                    foreach (ProcessDataEntityData data in list)
                    {
                        data.PARAMETERNAME = Regex.Replace(data.PARAMETERNAME, @"[/| |?|/|<|>|'|\-|,]", "_");
                        processDataList.Add(new ProcessData(data));
                    }
                    lock (_processDatas)
                    {
                        if (_processDatas.ContainsKey(eqpNo))
                        {
                            _processDatas.Remove(eqpNo);
                        }
                        _processDatas.Add(eqpNo, processDataList);
                    }
                }
                NLogManager.Logger.LogInfoWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name,
                            string.Format("Reload Process Data EQUIPMENT=[{0}]", eqpNo));

            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        /// <summary>
        /// Save Process Data History to Database
        /// </summary>
        /// <param name="history"></param>
        public void SaveProcessDataHistory(PROCESSDATAHISTORY history)
        {
            try
            {
                HibernateAdapter.SaveObject(history);
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// write Process Data  to File
        /// </summary>
        /// <param name="eqpID"></param>
        /// <param name="cstSeqNo"></param>
        /// <param name="jobSeqNo"></param>
        /// <param name="timeKey">yyyyMMddHHmmssfff</param>
        /// <param name="value"></param>
        public void MakeProcessDataValuesToFile(string eqpID, string cstSeqNo, string jobSeqNo, string timeKey, string value)
        {
            try
            {
                string directoryPath = this.HistoryPath + timeKey.Substring(0, 10);//sy modify 20160824 目錄細分到小時
                if (Directory.Exists(directoryPath) == false)
                {
                    Directory.CreateDirectory(directoryPath);
                }

                //CASSETTESEQNO_JOBSEQNO_NODEID_TRXID.txt
                string fileName = string.Format("{0}\\{1}.txt", directoryPath, string.Format("{0}_{1}_{2}_{3}", cstSeqNo,jobSeqNo, eqpID, timeKey));

                File.WriteAllText(fileName, value);
                //DeleteProcessDataFiles();//sy modify 20160824 By小時檢查
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// Write Lot Process Data to File
        /// </summary>
        /// <param name="eqpID"></param>
        /// <param name="cstSeqNo"></param>
        /// <param name="timeKey"></param>
        /// <param name="value"></param>
        /// <param name="fileName"></param>
        public void MakeLotProcessDataValuesToFile(string eqpID, string cstSeqNo, string timeKey, string value, out string fileName)
        {
            fileName = string.Empty;
            try
            {
                string directoryPath = this.HistoryPath + timeKey.Substring(0, 8);
                if (Directory.Exists(directoryPath) == false)
                {
                    Directory.CreateDirectory(directoryPath);
                }

                //CASSETTESEQNO_NODEID_TRXID.txt
                fileName = string.Format("{0}\\{1}_{2}_{3}.txt", directoryPath, cstSeqNo, eqpID, timeKey);

                File.WriteAllText(fileName, value);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// Get Process Datav Values,
        /// 传入FileName =CSTSeq_JobSeq_EqpID_yyyyMMddHHmmssfff;
        /// Return IList<string> Ex "Recipe ID=14";
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public IList<string> ProcessDataValues(string key)
        {
            List<string> paramter = null; ;
            try
            {
                if (string.IsNullOrEmpty(key) == true)
                    return null ;

                if (key.Split('_').Length < 4)
                    throw new Exception(string.Format("File Name Format Error [{0}]!", key));

                string timeKey = key.Split('_')[3].ToString().Trim();

                string directoryPath = this.HistoryPath + timeKey.Substring(0, 10);

                if (!File.Exists(directoryPath + "\\" + string.Format("{0}.txt", key)))
                    throw new Exception(string.Format("Can't find Process Data File [{0}] in Directory!", key));

                string[] filePathArray = Directory.GetFiles(directoryPath, string.Format("{0}.txt", key), SearchOption.TopDirectoryOnly);

                if (filePathArray != null && filePathArray.Length > 0)
                {
                    string value = File.ReadAllText(filePathArray[0]);
                    string[] valueArray = value.Split(',');
                    paramter = new List<string>();
                    paramter.AddRange(valueArray);
                }
                return paramter;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
            
        }

        private void AfterInit()//sy modify 20160824 By小時檢查
        {
            _timer = new System.Timers.Timer(3600 * 1000);
            _timer.AutoReset = true;
            _timer.Elapsed += new System.Timers.ElapsedEventHandler(DeleteProcessDataFiles);
            _timer.Start();
        }
        /// <summary>
        /// Delete ProcessData Files 目前保留14天
        /// </summary>
        public void DeleteProcessDataFiles(object sender, ElapsedEventArgs e)//sy modify 20160824 By小時檢查
        {
            try
            {
                if (!Directory.Exists(_historyPath)) return; //20161217 sy add 機台沒processdata 表示沒產生 不用刪除

                string[] directoryList = Directory.GetDirectories(_historyPath);

                foreach (string directoryPath in directoryList)
                {
                    DateTime checkTime = Directory.GetCreationTime(directoryPath).AddDays(14);

                    if (checkTime < DateTime.Now)
                    {
                        if (Directory.Exists(directoryPath))
                            Directory.Delete(directoryPath, true);
                        //sy modify 20160824 log
                        Log.NLogManager.Logger.LogInfoWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "DELETE EVENT:Delete Process Data File Path :" + directoryPath);
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public System.Data.DataTable GetDataTable(string entityName)
        {
            try
            {
                DataTable dt = new DataTable();

                ProcessDataEntityData file = new ProcessDataEntityData();
                DataTableHelp.DataTableAppendColumn(file, dt);

               
                foreach (IList<ProcessData> list in _processDatas.Values)
                {
                    foreach (ProcessData pd in list)
                    {
                        DataRow dr = dt.NewRow();
                        DataTableHelp.DataRowAssignValue(pd.Data, dr);
                        dt.Rows.Add(dr);
                    }
                }
                return dt;
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        public IList<string> GetEntityNames()
        {
            IList<string> entityNames = new List<string>();
            entityNames.Add("ProcessDataManager");
            return entityNames;
        }
    }
}
