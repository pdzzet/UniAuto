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

namespace UniAuto.UniBCS.EntityManager
{
    public class APCDataDownloadManager
    {
        /// <summary>
        /// 各机台的APC Download设定档
        /// </summary>
        public IDictionary<string, IList<APCDataDownload>> _bcAPCDatas = new Dictionary<string, IList<APCDataDownload>>();

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
            _bcAPCDatas = Reload();
        }

        protected IDictionary<string, IList<APCDataDownload>> Reload()
        {
            try
            {
                IDictionary<string, IList<APCDataDownload>> bcAPCDatas = new Dictionary<string, IList<APCDataDownload>>();
                string hql = string.Format("from APCDataDownloadEntityData where LINETYPE = '{0}' order by NODENO, OBJECTKEY", Workbench.LineType);
                IList list = this.HibernateAdapter.GetObjectByQuery(hql);
                IList<APCDataDownload> bcAPCDataList = null;
                if (list != null)
                {
                    foreach (APCDataDownloadEntityData data in list)
                    {
                        //过滤一些特殊字符
                        //data.PARAMETERNAME = data.PARAMETERNAME.Replace(',', ';').Replace('=', '-').Replace('<', '(').Replace('>', ')').Replace('/', '-');
                        data.PARAMETERNAME = Regex.Replace(data.PARAMETERNAME, @"[/| |?|/|<|>|'|\-|,]", "_");
                        if (!bcAPCDatas.ContainsKey(data.NODENO))
                        {
                            bcAPCDataList = new List<APCDataDownload>();
                            bcAPCDatas.Add(data.NODENO, bcAPCDataList);
                        }
                        bcAPCDatas[data.NODENO].Add(new APCDataDownload(data));

                    }
                }
                return bcAPCDatas;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }

        /// <summary>
        /// 取得APC Download 设定档
        /// </summary>
        public IList<APCDataDownload> GetAPCDataDownloadProfile(string eqpNo)
        {
            try
            {
                if (_bcAPCDatas.ContainsKey(eqpNo))
                {
                    lock (_bcAPCDatas)
                    {
                        return _bcAPCDatas[eqpNo];
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
                IDictionary<string, IList<APCDataDownload>> tempDic = Reload();

                if (tempDic != null)
                {
                    lock (_bcAPCDatas)
                    {
                        _bcAPCDatas = tempDic;
                    }

                }
                NLogManager.Logger.LogInfoWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name,
                           string.Format("Reload APC Data all Equipment."));
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
                string hql = string.Format("from APCDataDownloadEntityData where LINETYPE = '{0}' and NODENO='{1}' order by SVID ", Workbench.LineType, eqpNo);
                IList list = this.HibernateAdapter.GetObjectByQuery(hql);
                IList<APCDataDownload> bcAPCDataList = new List<APCDataDownload>();
                if (list != null)
                {
                    foreach (APCDataDownloadEntityData data in list)
                    {
                        //过滤一些特殊字符
                        //data.PARAMETERNAME = data.PARAMETERNAME.Replace(',', ';').Replace('=', '-').Replace('<', '(').Replace('>', ')').Replace('/', '-');
                        data.PARAMETERNAME = Regex.Replace(data.PARAMETERNAME, @"[/| |?|/|<|>|'|\-|,]", "_");
                        bcAPCDataList.Add(new APCDataDownload(data));
                    }
                    lock (_bcAPCDatas)
                    {
                        if (_bcAPCDatas.ContainsKey(eqpNo))
                        {
                            _bcAPCDatas.Remove(eqpNo);
                        }
                        _bcAPCDatas.Add(eqpNo, bcAPCDataList);
                    }
                }
                NLogManager.Logger.LogInfoWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name,
                            string.Format("Reload APC Data EQUIPMENT=[{0}]", eqpNo));

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
        //        APCDataDownloadEntityData data = new APCDataDownloadEntityData();
        //        DataTableHelp.DataTableAppendColumn(data, dt);

        //        Dictionary<string, List<APCDataDownloadEntityData>>.Enumerator enumerator = _bcAPCDatas.GetEnumerator();
        //        while (enumerator.MoveNext())
        //        {
        //            List<APCDataDownloadEntityData> list = enumerator.Current.Value;
        //            foreach (APCDataDownloadEntityData entity in list)
        //            {
        //                DataRow dr = dt.NewRow();
        //                DataTableHelp.DataRowAssignValue(entity, dr);
        //                dt.Rows.Add(dr);
        //            }
        //        }
        //        return dt;
        //    }
        //    catch (System.Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //        return null;
        //    }
            
        //}
    }
}
