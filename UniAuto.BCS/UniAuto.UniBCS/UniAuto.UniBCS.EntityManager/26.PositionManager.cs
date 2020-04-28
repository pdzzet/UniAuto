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

namespace UniAuto.UniBCS.EntityManager
{
    public class PositionManager
    {
        /// <summary>
        /// 各机台的Daily Check设定档
        /// </summary>
        public IDictionary<string, IList<PositionData>> _eqpEVData = new Dictionary<string, IList<PositionData>>();

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
            _eqpEVData = Reload();
        }

        protected IDictionary<string, IList<PositionData>> Reload()
        {
            try
            {
                IDictionary<string, IList<PositionData>> eqpEVDatas = new Dictionary<string, IList<PositionData>>();
                string hql = string.Format("from PositionEntityData where LINEID = '{0}' order by UNITNO, OBJECTKEY", Workbench.ServerName);//LINEID 
                IList list = this.HibernateAdapter.GetObjectByQuery(hql);
                IList<PositionData> eqpEVDataList = null;
                if (list != null)
                {
                    foreach (PositionEntityData data in list)
                    {
                        //过滤一些特殊字符
                        //data.PARAMETERNAME = data.PARAMETERNAME.Replace(',', ';').Replace('=', '-').Replace('<', '(').Replace('>', ')').Replace('/', '-');
                        data.POSITIONNAME = Regex.Replace(data.POSITIONNAME, @"[/|?|/|<|>|'|\|-|,]", "_"); //空白不用濾掉
                        if (!eqpEVDatas.ContainsKey(data.NODENO))
                        {
                            eqpEVDataList = new List<PositionData>();
                            eqpEVDatas.Add(data.NODENO, eqpEVDataList);
                        }
                        eqpEVDatas[data.NODENO].Add(new PositionData(data));

                    }
                }
                return eqpEVDatas;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }
                
        /// <summary>
        /// 取得Energy Visualization 设定档
        /// </summary>
        public IList<PositionData> GetPositionProfile(string eqpNo)
        {
            try
            {
                if (_eqpEVData.ContainsKey(eqpNo))
                {
                    return _eqpEVData[eqpNo];
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
                IDictionary<string, IList<PositionData>> tempDic = Reload();

                if (tempDic != null)
                {
                    lock (_eqpEVData)
                    {
                        _eqpEVData = tempDic;
                    }

                }
                NLogManager.Logger.LogInfoWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name,
                           string.Format("Reload Position Data all Equipment."));
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
                string hql = string.Format("from PositionEntityData where LINEID = '{0}' and NODENO='{1}' order by UNITNO ", Workbench.ServerName, eqpNo);
                IList list = this.HibernateAdapter.GetObjectByQuery(hql);
                IList<PositionData> eqpEVDataList = new List<PositionData>();
                if (list != null)
                {
                    foreach (PositionEntityData data in list)
                    {
                        //过滤一些特殊字符
                        //data.PARAMETERNAME = data.PARAMETERNAME.Replace(',', ';').Replace('=', '-').Replace('<', '(').Replace('>', ')').Replace('/', '-');
                        data.POSITIONNAME = Regex.Replace(data.POSITIONNAME, @"[/| |?|/|<|>|'|\-|,]", "_");
                        eqpEVDataList.Add(new PositionData(data));
                    }
                    lock (_eqpEVData)
                    {
                        if (_eqpEVData.ContainsKey(eqpNo))
                        {
                            _eqpEVData.Remove(eqpNo);
                        }
                        _eqpEVData.Add(eqpNo, eqpEVDataList);
                    }
                }
                NLogManager.Logger.LogInfoWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name,
                            string.Format("Reload Position Data EQUIPMENT=[{0}]", eqpNo));

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        public System.Data.DataTable GetDataTable(string entityName)
        {
            try
            {
                DataTable dt = new DataTable();

                PositionEntityData file = new PositionEntityData();
                DataTableHelp.DataTableAppendColumn(file, dt);


                foreach (IList<PositionData> list in _eqpEVData.Values)
                {
                    foreach (PositionData pd in list)
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
        //        EnergyVisualizationEntityData data = new EnergyVisualizationEntityData();
        //        DataTableHelp.DataTableAppendColumn(data, dt);

        //        Dictionary<string, List<EnergyVisualizationEntityData>>.Enumerator enumerator = _eqpEVData.GetEnumerator();
        //        while (enumerator.MoveNext())
        //        {
        //            List<EnergyVisualizationEntityData> list = enumerator.Current.Value;
        //            foreach (EnergyVisualizationEntityData entity in list)
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
