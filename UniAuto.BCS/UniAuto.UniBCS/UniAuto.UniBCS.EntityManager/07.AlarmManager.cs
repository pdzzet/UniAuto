using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.Log;
using System.Collections;

namespace UniAuto.UniBCS.EntityManager
{
    public class AlarmManager : EntityManager
    {
        /// <summary>
        /// 各機台的Alarm設定檔
        /// </summary>
        public Dictionary<string, Dictionary<string, AlarmEntityData>> eqpAlarms = new Dictionary<string, Dictionary<string, AlarmEntityData>>();
        /// <summary>
        /// 各機台目前正在發生的Alarm記錄
        /// </summary>
        public Dictionary<string, AlarmHistoryFile> eqpHistory = new Dictionary<string, AlarmHistoryFile>();

        public override EntityManager.FILE_TYPE GetFileType()
        {
            return FILE_TYPE.BIN;
        }

        protected override string GetSelectHQL()
        {
            return string.Format("from AlarmEntityData where SERVERNAME = '{0}'", BcServerName);
        }

        protected override Type GetTypeOfEntityData()
        {
            return typeof(AlarmEntityData);
        }

        protected override Type GetTypeOfEntityFile()
        {
            return typeof(AlarmHistoryFile);
        }

        protected override EntityFile NewEntityFile(string Filename)
        {
            return null;
        }

        protected override void AfterInit(List<EntityData> entity_Data, List<EntityFile> entity_File)
        {
            // Reload DB
            try
            {

                Dictionary<string, AlarmEntityData> _alarmNode;

                foreach (EntityData ent in entity_Data)
                {
                    AlarmEntityData alarm = ent as AlarmEntityData;
                    if (eqpAlarms.ContainsKey(alarm.NODENO))
                    {
                        _alarmNode = eqpAlarms[alarm.NODENO];
                    }
                    else
                    {
                        _alarmNode = new Dictionary<string, AlarmEntityData>();
                        eqpAlarms.Add(alarm.NODENO, _alarmNode);
                    }

                    if (!_alarmNode.ContainsKey(alarm.UNITNO + "_" + alarm.ALARMID))
                    {
                        _alarmNode.Add(alarm.UNITNO + "_" + alarm.ALARMID, alarm);
                    }
                }

                // Reload History File
                foreach (EntityFile _file in entity_File)
                {
                    try
                    {
                        AlarmHistoryFile _f = _file as AlarmHistoryFile;
                        if (eqpHistory.ContainsKey(_f.EQPNo))
                        {
                            eqpHistory.Remove(_f.EQPNo);
                        }
                        eqpHistory.Add(_f.EQPNo, _f);
                    }
                    catch (System.Exception ex)
                    {
                        Debug.Print(ex.Message);
                    }
                }
            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        protected override void AfterSelectDB(List<EntityData> EntityDatas, string FilePath, out List<string> Filenames)
        {
            Filenames = new List<string>();
            Filenames.Add("*.bin");
        }

        /// <summary>
        /// 取得Alarm 設定檔,此方法已经过期
        /// </summary>
        public AlarmEntityData GetAlarmProfile(string eqpNo, string alarmCode)
        {
            AlarmEntityData ret = null;
            try
            {
                if (eqpAlarms.ContainsKey(eqpNo))
                {
                    if (eqpAlarms[eqpNo].ContainsKey(alarmCode))
                    {
                        ret = eqpAlarms[eqpNo][alarmCode];
                    }
                }

                return ret;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        /// <summary>
        /// 取得Alarm 設定檔
        /// </summary>
        public AlarmEntityData GetAlarmProfile(string eqpNo, string unitNo, string alarmCode)
        {
            AlarmEntityData ret = null;
            try
            {
                if (eqpAlarms.ContainsKey(eqpNo))
                {
                    if (eqpAlarms[eqpNo].ContainsKey(unitNo + "_" + alarmCode))
                    {
                        ret = eqpAlarms[eqpNo][unitNo + "_" + alarmCode];
                    }
                }

                return ret;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        public HappeningAlarm GetAlarm(string eqpNo, string alarmCode)
        {
            HappeningAlarm ret = null;
            try
            {
                if (eqpHistory.ContainsKey(eqpNo))
                {
                    AlarmHistoryFile nodeAlarm = eqpHistory[eqpNo];
                    if (nodeAlarm.HappingAlarms.ContainsKey(alarmCode))
                    {
                        ret = nodeAlarm.HappingAlarms[alarmCode];
                    }
                }

                return ret;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        public AlarmHistoryFile GetEQPAlarm(string eqpNo)
        {
            AlarmHistoryFile ret = null;
            try
            {
                if (eqpHistory.ContainsKey(eqpNo))
                {
                    ret = eqpHistory[eqpNo];
                }
                return ret;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }
        
        /// <summary>
        /// 取得目前正在發生的Alarm或Warning
        /// </summary>
        public HappeningAlarm GetAlarm(string eqpNo, string unitNo, string alarmCode)
        {
            HappeningAlarm ret = null;
            try
            {
                if (eqpHistory.ContainsKey(eqpNo))
                {
                    AlarmHistoryFile eqpAlarm = eqpHistory[eqpNo];
                    if (eqpAlarm.HappingAlarms.ContainsKey(alarmCode))
                    {
                        HappeningAlarm obj = eqpAlarm.HappingAlarms[alarmCode];
                        if (obj.Alarm.UNITNO.Equals(unitNo))
                        {
                            ret = obj;
                        }
                    }
                }

                return ret;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        /// <summary>
        /// 創建新的Alarm設定檔
        /// </summary>
        public bool CreateAlarmProfile(AlarmEntityData _entity)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(_entity.NODENO);
                if (eqp == null)
                {
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", 
                        string.Format("Can't find the Equipment No [{0}]", _entity.NODENO));
                    return false;
                }
                
                // 1. 新增到BCS的nodeAlarms
                Dictionary<string, AlarmEntityData> _alarm;
                if (eqpAlarms.ContainsKey(_entity.NODENO))
                {
                    _alarm = eqpAlarms[_entity.NODENO];
                }
                else
                {
                    _alarm = new Dictionary<string, AlarmEntityData>();
                    eqpAlarms.Add(_entity.NODENO, _alarm);
                }

                //Modify By James at 2014/09/28
                _alarm.Add(_entity.UNITNO+"_"+_entity.ALARMID, _entity);
                //_alarm.Add(_entity.ALARMID, _entity);

                // 2. 儲存到資料庫
                InsertDB(_entity);
                eqp = null;
                return true;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        /// <summary>
        /// 新增目前正在發生的Alarm
        /// </summary>
        public bool AddHappeningAlarm(AlarmEntityData _entity)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(_entity.NODENO);
                if (eqp == null)
                {
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("Can't find the Equipment No [{0}]", _entity.NODENO));
                    return false;
                }

                AlarmHistoryFile his;
                if (eqpHistory.ContainsKey(_entity.NODENO))
                {
                    his = eqpHistory[_entity.NODENO];
                }
                else
                {
                    his = new AlarmHistoryFile(eqp.Data.NODENO, eqp.Data.NODEID);
                    eqpHistory.Add(his.EQPNo, his);
                }

                if (his.HappingAlarms.ContainsKey(_entity.ALARMID))
                {
                    his.HappingAlarms.Remove(_entity.ALARMID);
                }
                his.HappingAlarms.Add(_entity.ALARMID, new HappeningAlarm(_entity, DateTime.Now));
                eqp = null;
                EnqueueSave(his);
                return true;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        /// <summary>
        /// 刪除目前正在發生的Alarm
        /// </summary>
        public bool DeleteHappeningAlarm(AlarmEntityData _entity)
        {
            try
            {
                Equipment eqp = ObjectManager.EquipmentManager.GetEQP(_entity.NODENO);
                if (eqp == null)
                {
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                        string.Format("Can't find the Equipment No [{0}]", _entity.NODENO));
                    return false;
                }
                eqp = null;
                // 找不到就跳出
                if (!eqpHistory.ContainsKey(_entity.NODENO)) return true;

                AlarmHistoryFile his = eqpHistory[_entity.NODENO];

                if (his.HappingAlarms.ContainsKey(_entity.ALARMID))
                {
                    his.HappingAlarms.Remove(_entity.ALARMID);
                }
                EnqueueSave(his);
                return true;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
            }
        }

        /// <summary>
        /// 清除所有機台正在發生的Alarm
        /// </summary>
        public void ClearAllHappeningAlarm()
        {
            try
            {
                foreach (AlarmHistoryFile his in eqpHistory.Values)
                {
                    his.HappingAlarms.Clear();
                    EnqueueSave(his);
                }
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 清除指定機台正在發生的Alarm
        /// </summary>
        public void ClearAllHappeningAlarm(string eqpNo)
        {
            try
            {
                if (eqpHistory.ContainsKey(eqpNo))
                {
                    AlarmHistoryFile his = eqpHistory[eqpNo];
                    his.HappingAlarms.Clear();
                    EnqueueSave(his);
                }
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        /// <summary>
        /// 檢查指定機台是否有Alarm 發生
        /// </summary>
        public bool CheckAlarmState(string eqpNo, string alarmID)
        {
            try
            {
                if (!eqpHistory.ContainsKey(eqpNo)) return false;
                if (!eqpHistory[eqpNo].HappingAlarms.ContainsKey(alarmID)) return false;

                return true;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return false;
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

        public System.Data.DataTable GetDataTable()
        {
            try
            {
                DataTable dt = new DataTable();
                AlarmEntityData data = new AlarmEntityData();
                DataTableHelp.DataTableAppendColumn(data, dt);

                Dictionary<string, Dictionary<string, AlarmEntityData>>.Enumerator enumerator = eqpAlarms.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    Dictionary<string, AlarmEntityData> dic = enumerator.Current.Value;
                    Dictionary<string, AlarmEntityData>.Enumerator en = dic.GetEnumerator();
                    while (en.MoveNext())
                    {
                        DataRow dr = dt.NewRow();
                        DataTableHelp.DataRowAssignValue(en.Current.Value, dr);
                        dt.Rows.Add(dr);
                    }
                }
                return dt;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
           
        }

        /// <summary>
        /// Reload All  Alarm 
        /// 20150406 Modify by Frank according by T2 method
        /// </summary>
        public void ReloadAll()
        {
            try
            {
                IList alarmEntitys= HibernateAdapter.GetObjectByQuery(GetSelectHQL());

                Dictionary<string, AlarmEntityData> _alarmNode;
                Dictionary<string, Dictionary<string, AlarmEntityData>> entitys = new Dictionary<string, Dictionary<string, AlarmEntityData>>();

                if (alarmEntitys != null)
                {
                    foreach (AlarmEntityData entity in alarmEntitys)
                    {
                        if (!eqpAlarms.ContainsKey(entity.NODENO))
                        {
                            _alarmNode = new Dictionary<string, AlarmEntityData>();
                            eqpAlarms.Add(entity.NODENO, _alarmNode);
                        }
                        else
                            _alarmNode = eqpAlarms[entity.NODENO];

                        string key = entity.UNITNO + "_" + entity.ALARMID;
                        if (_alarmNode.ContainsKey(key))
                        {
                            _alarmNode[key] = entity;
                        }
                        else
                        {
                            _alarmNode.Add(key, entity);
                        }
                    }
                }
                Log.NLogManager.Logger.LogInfoWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "Reload Alarm Success.");

            }
            catch (System.Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);

            }
        }
    }
}
