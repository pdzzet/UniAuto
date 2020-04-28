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
    public class APCDataReportManager
    {
        /// <summary>
        /// 各机台的APC Report设定档
        /// </summary>
        public IDictionary<string, IList<APCDataReport>> _eqpAPCDatas = new Dictionary<string, IList<APCDataReport>>();

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
            _eqpAPCDatas = Reload();
        }

        protected IDictionary<string, IList<APCDataReport>> Reload()
        {
            try
            {
                IDictionary<string, IList<APCDataReport>> eqpAPCDatas = new Dictionary<string, IList<APCDataReport>>();
                string hql = string.Format("from APCDataReportEntityData where LINETYPE = '{0}' order by NODENO, OBJECTKEY", Workbench.LineType);
                IList list = this.HibernateAdapter.GetObjectByQuery(hql);
                IList<APCDataReport> eqpAPCDataList = null;
                if (list != null)
                {
                    foreach (APCDataReportEntityData data in list)
                    {
                        //过滤一些特殊字符
                       //data.PARAMETERNAME = data.PARAMETERNAME.Replace(',', ';').Replace('=', '-').Replace('<', '(').Replace('>', ')').Replace('/', '-');
                        data.PARAMETERNAME = Regex.Replace(data.PARAMETERNAME, @"[/| |?|/|<|>|'|\-|,]", "_");
                        if (!eqpAPCDatas.ContainsKey(data.NODENO))
                        {
                            eqpAPCDataList = new List<APCDataReport>();
                            eqpAPCDatas.Add(data.NODENO, eqpAPCDataList);
                        }
                        eqpAPCDatas[data.NODENO].Add(new APCDataReport(data));

                    }
                }
                return eqpAPCDatas;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }
        
        /// <summary>
        /// 取得APC Report设定档
        /// </summary>
        public IList<APCDataReport> GetAPCDataReportProfile(string eqpNo)
        {
            try
            {
                if (_eqpAPCDatas.ContainsKey(eqpNo))
                {
                    lock (_eqpAPCDatas)
                    {
                        return _eqpAPCDatas[eqpNo];
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
                IDictionary<string, IList<APCDataReport>> tempDic = Reload();

                if (tempDic != null)
                {
                    lock (_eqpAPCDatas)
                    {
                        _eqpAPCDatas = tempDic;
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
                string hql = string.Format("from APCDataReportEntityData where LINETYPE = '{0}' and NODENO='{1}' order by SVID ", Workbench.LineType, eqpNo);
                IList list = this.HibernateAdapter.GetObjectByQuery(hql);
                IList<APCDataReport> eqpAPCDataList = new List<APCDataReport>();
                if (list != null)
                {
                    foreach (APCDataReportEntityData data in list)
                    {
                        data.PARAMETERNAME = Regex.Replace(data.PARAMETERNAME, @"[/| |?|/|<|>|'|\-|,]", "_");
                        eqpAPCDataList.Add(new APCDataReport(data));
                    }
                    lock (_eqpAPCDatas)
                    {
                        if (_eqpAPCDatas.ContainsKey(eqpNo))
                        {
                            _eqpAPCDatas.Remove(eqpNo);
                        }
                        _eqpAPCDatas.Add(eqpNo, eqpAPCDataList);
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
        //        APCDataReportEntityData data = new APCDataReportEntityData();
        //        DataTableHelp.DataTableAppendColumn(data, dt);

        //        Dictionary<string, List<APCDataReportEntityData>>.Enumerator enumerator = eqpAPCData.GetEnumerator();
        //        while (enumerator.MoveNext())
        //        {
        //            List<APCDataReportEntityData> list = enumerator.Current.Value;
        //            foreach (APCDataReportEntityData entity in list)
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

        //        return null;
        //    }

        //}
    }
}
