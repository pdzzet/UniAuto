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
using System.Linq;

namespace UniAuto.UniBCS.EntityManager
{
    public class DailyCheckManager : IDataSource
    {
        /// <summary>
        /// 各机台的Daily Check设定档
        /// </summary>
        public IDictionary<string, IList<DailyCheckData>> _eqpDailyCheckDatas = new Dictionary<string, IList<DailyCheckData>>();

        /// <summary>
        /// NLog Logger Name
        /// </summary>
        public string LoggerName { get; set; }

        /// <summary>
        /// 用來讀取DB
        /// </summary>
        public HibernateAdapter HibernateAdapter { get; set; }

        public void Init()
        {
            _eqpDailyCheckDatas = Reload();
        }

        protected IDictionary<string, IList<DailyCheckData>> Reload()
        {
            try
            {
                IDictionary<string, IList<DailyCheckData>> eqpDailyCheckDatas = new Dictionary<string, IList<DailyCheckData>>();
                string hql = string.Format("from DailyCheckEntityData where LINETYPE = '{0}' order by NODENO, OBJECTKEY", Workbench.LineType);
                IList list = this.HibernateAdapter.GetObjectByQuery(hql);
                IList<DailyCheckData> eqpDailyCheckDataList = null;
                if (list != null)
                {
                    foreach (DailyCheckEntityData data in list)
                    {
                        //过滤一些特殊字符
                        //data.PARAMETERNAME = data.PARAMETERNAME.Replace(',', ';').Replace('=', '-').Replace('<', '(').Replace('>', ')').Replace('/', '-');
                        data.PARAMETERNAME = Regex.Replace(data.PARAMETERNAME, @"[/| |?|/|<|>|'|\-|,]", "_");
                        if (!eqpDailyCheckDatas.ContainsKey(data.NODENO))
                        {
                            eqpDailyCheckDataList = new List<DailyCheckData>();
                            eqpDailyCheckDatas.Add(data.NODENO, eqpDailyCheckDataList);
                        }
                        eqpDailyCheckDatas[data.NODENO].Add(new DailyCheckData(data));

                    }
                }
                return eqpDailyCheckDatas;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }

        /// <summary>
        /// 取得Daily Check 设定档
        /// </summary>
        public IList<DailyCheckData> GetDailyCheckProfile(string eqpNo)
        {
            try
            {
                if (_eqpDailyCheckDatas.ContainsKey(eqpNo))
                {
                    lock (_eqpDailyCheckDatas)
                    {
                        return _eqpDailyCheckDatas[eqpNo];
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
        /// Reload DB Profile by All
        /// </summary>
        /// <returns></returns>
        public void ReloadAll()
        {
            try
            {
                IDictionary<string, IList<DailyCheckData>> tempDic = Reload();

                if (tempDic != null)
                {
                    lock (_eqpDailyCheckDatas)
                    {
                        _eqpDailyCheckDatas = tempDic;
                    }

                }
                NLogManager.Logger.LogInfoWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name,
                           string.Format("Reload Daily Check Data all Equipment."));
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
                string hql = string.Format("from DailyCheckEntityData where LINETYPE = '{0}' and NODENO='{1}' order by SVID ", Workbench.LineType, eqpNo);
                IList list = this.HibernateAdapter.GetObjectByQuery(hql);
                IList<DailyCheckData> eqpDailyCheckDataList = new List<DailyCheckData>();
                if (list != null)
                {
                    foreach (DailyCheckEntityData data in list)
                    {
                        //过滤一些特殊字符
                        //data.PARAMETERNAME = data.PARAMETERNAME.Replace(',', ';').Replace('=', '-').Replace('<', '(').Replace('>', ')').Replace('/', '-');
                        data.PARAMETERNAME = Regex.Replace(data.PARAMETERNAME, @"[/| |?|/|<|>|'|\-|,]", "_");
                        eqpDailyCheckDataList.Add(new DailyCheckData(data));
                    }
                    lock (_eqpDailyCheckDatas)
                    {
                        if (_eqpDailyCheckDatas.ContainsKey(eqpNo))
                        {
                            _eqpDailyCheckDatas.Remove(eqpNo);
                        }
                        _eqpDailyCheckDatas.Add(eqpNo, eqpDailyCheckDataList);
                    }
                }
                NLogManager.Logger.LogInfoWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name,
                            string.Format("Reload Daily Check Data EQUIPMENT=[{0}]", eqpNo));

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        public void DeleteDB(IList<DailyCheckData> data)
        {
              try
              {
                    if (data != null)
                    {
                          IList<DailyCheckEntityData> entityData = new List<DailyCheckEntityData>();
                          foreach (DailyCheckData dc in data)
                          {
                                entityData.Add(dc.Data);
                          }
                          if (entityData.Count > 0)
                          {
                                NLogManager.Logger.LogDebugWrite(LoggerName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", entityData[0].GetType().Name);
                                HibernateAdapter.DeleteObject(entityData.ToArray<DailyCheckEntityData>());
                          }
                    }
              }
              catch (Exception ex)
              {
                    NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
              }
        }
        public void InsertDB(IList<DailyCheckData> data)
        {
              try
              {
                    if (data != null)
                    {
                          IList<DailyCheckEntityData> entityData = new List<DailyCheckEntityData>();
                          foreach (DailyCheckData dc in data)
                          {
                                entityData.Add(dc.Data);
                          }
                          if (entityData.Count > 0)
                          {
                                NLogManager.Logger.LogDebugWrite(LoggerName, GetType().Name, MethodInfo.GetCurrentMethod().Name + "()", entityData[0].GetType().Name);
                                HibernateAdapter.SaveObjectAll(entityData.ToArray<DailyCheckEntityData>());
                          }
                    }
              }
              catch (Exception ex)
              {
                    NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
              }
        }
        //private void DataTableAppendColumn(object obj, DataTable dataTable)
        //{
        //    Type type = obj.GetType();
        //    PropertyInfo[] properties = type.GetProperties();
        //    foreach (PropertyInfo prop in properties)
        //    {
        //        dataTable.Columns.Add(prop.Name, typeof(string));
        //    }
        //}

        //private void DataRowAssignValue(object obj, DataRow dataRow)
        //{
        //    Type type = obj.GetType();
        //    PropertyInfo[] properties = type.GetProperties();
        //    foreach (PropertyInfo prop in properties)
        //    {
        //        object val = prop.GetValue(obj, null);
        //        dataRow[prop.Name] = val.ToString();
        //    }
        //}

        //public System.Data.DataTable GetDataTable()
        //{
        //    try
        //    {
        //        DataTable dt = new DataTable();
        //        DailyCheckEntityData data = new DailyCheckEntityData();
        //        DataTableHelp.DataTableAppendColumn(data, dt);

        //        Dictionary<string, List<DailyCheckEntityData>>.Enumerator enumerator = _eqpDailyCheckDatas.GetEnumerator();
        //        while (enumerator.MoveNext())
        //        {
        //            List<DailyCheckEntityData> list = enumerator.Current.Value;
        //            foreach (DailyCheckEntityData entity in list)
        //            {
        //                DataRow dr = dt.NewRow();
        //                DataTableHelp.DataRowAssignValue(entity, dr);
        //                dt.Rows.Add(dr);
        //            }
        //        }
        //        return dt;
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //        return null;
        //    }
            
        //}

        public System.Data.DataTable GetDataTable(string entityName)
        {
            try
            {
                DataTable dt = new DataTable();

                DailyCheckEntityData file = new DailyCheckEntityData();
                DataTableHelp.DataTableAppendColumn(file, dt);


                foreach (IList<DailyCheckData> list in _eqpDailyCheckDatas.Values)
                {
                    foreach (DailyCheckData pd in list)
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
            entityNames.Add("DailyCheckManager");
            return entityNames;
        }
    }
}
