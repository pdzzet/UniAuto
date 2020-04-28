using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.DB;

namespace UniAuto.UniBCS.EntityManager
{
    public class CIMMessageManager 
    {
        /// <summary>
        /// 各機台的CIM Message Set Command記錄,
        /// Key="EQUIPMENTNO_MESSAGEID"
        /// </summary>
        private IDictionary<string, CIMMessage> _cimMessageDatas = new Dictionary<string, CIMMessage>();

        private string _loggerName;

        private static int messageid = 0;

        public string LoggerName
        {
            get { return _loggerName; }
            set { _loggerName = value; }
        }

        /// <summary>
        /// 用來讀取DB
        /// </summary>
        public HibernateAdapter HibernateAdapter { get; set; }

        /// <summary>
        /// 将CIMMessage记录存储到DB
        /// </summary>
        /// <param name="cimmessage">CIMMESSAGEHISTORY</param>
        public void SaveCIMMessageHistoryToDB(CIMMESSAGEHISTORY cimmessage)
        {
            try
            {
                HibernateAdapter.SaveObject(cimmessage);
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public void Init()
        { }

        /// <summary>
        /// 新增目前正在發生的CIMMessage
        /// </summary>
        public void AddCIMMessageData(CIMMessage data)
        {
            try
            {
                string key = string.Format("{0}_{1}", data.NodeNo,data.MessageID);
                if (_cimMessageDatas.ContainsKey(key))
                {
                    _cimMessageDatas.Remove(key);
                }
                _cimMessageDatas.Add(key, data);
               
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
               
            }
        }

        /// <summary>
        /// Clear发生的CIMMessage
        /// </summary>
        /// <param name="eqpNo">EQUIPMENT NO</param>
        /// <param name="msgID">CIM Message ID</param>
        /// <returns></returns>
        public CIMMessage ClearCIMMessage(string eqpNo, string msgID)
        {
            try
            {
                CIMMessage data = null;
                string key = string.Format("{0}_{1}", eqpNo,msgID);
                if (_cimMessageDatas.ContainsKey(key))
                {
                    data = _cimMessageDatas[key];
                    _cimMessageDatas.Remove(key);
                }
                return data;

            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        /// <summary>
        /// 取得当前所有未Clear的CIM Message
        /// </summary>
        /// <param name="eqpNo"></param>
        /// <returns></returns>
        public IDictionary<string,CIMMessage> GetCIMMessageData()
        {
            try
            {
                return _cimMessageDatas;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        /// <summary>
        /// 取得唯一的Message ID
        /// </summary>
        /// <returns></returns>
        public static int GetMessageID()
        {
            messageid++;
            if (messageid < 60000)
            {
                return messageid;
            }
            else
            {
                messageid = 1;
                return messageid;
            }
        }

        /// <summary>
        /// 返回指定機台的所有未Clear的CIM Message
        /// 返回一個Dictionary，Key為eqpNo，Value為該機台下的List<CIMMessage>
        /// eqpNo為空值時，返回所有的機台下的所有CIM Message
        /// </summary>
        /// <param name="eqpNo">eqpNo,默認值為空，即返回所有機台下的所有CIM Message</param>
        /// <returns></returns>
        public IDictionary<string, List<CIMMessage>> CIMMessageRequest(string eqpNo = "")
        {
            IDictionary<string, CIMMessage> dicAll = new Dictionary<string, CIMMessage>();
            //取得当前所有未Clear的CIM Message
            dicAll = ObjectManager.CIMMessageManager.GetCIMMessageData();
            //用於記錄指定機台下的所有CIM Message，採用Dictionary存儲，其中Key為eqpNo,Value為List<CIM Message>
            IDictionary<string, List<CIMMessage>> dicCIMMessageByEqp = new Dictionary<string, List<CIMMessage>>();

            try
            {
                string[] sArray = new string[2];
                string strEqpKey = string.Empty;

                #region [所有機台的所有CIM Message]
                foreach (string key in dicAll.Keys)
                {
                    sArray = key.Split('_');
                    strEqpKey = sArray[0];//取機台編號
                    if (!dicCIMMessageByEqp.ContainsKey(strEqpKey))
                    {
                        //用於記錄某臺機台的所有CIM Message，採用List<>存儲
                        List<CIMMessage> lstCIMMessage = new List<CIMMessage>();
                        lstCIMMessage.Add(dicAll[key]);
                        dicCIMMessageByEqp.Add(strEqpKey, lstCIMMessage);

                    }
                    else
                    {
                        dicCIMMessageByEqp[strEqpKey].Add(dicAll[key]);
                    }
                }
                #endregion

                if (eqpNo != string.Empty)
                {
                    #region [返回指定機台的所有CIM Message]
                    //用於記錄某臺機台的所有CIM Message，採用List<>存儲
                    List<CIMMessage> lstCIMMessage = new List<CIMMessage>();
                    if (dicCIMMessageByEqp.Count == 0) return dicCIMMessageByEqp;
                    lstCIMMessage = dicCIMMessageByEqp[eqpNo];
                    dicCIMMessageByEqp.Clear();
                    dicCIMMessageByEqp.Add(eqpNo, lstCIMMessage);

                    #endregion

                }
                return dicCIMMessageByEqp;
            }
            catch (Exception ex)
            {
                Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        /// <summary>
        /// 该方法已弃用，仅为保证程式不报错，新版UI Service更新后删除
        /// </summary>
        /// <param name="eqpNo"></param>
        /// <returns></returns>
        public IDictionary<string, CIMMessage> CIMMessageRequest()
        {
            return null;
        }

        #region Mark

        ///// <summary>
        ///// 刪除目前正在發生的CIMMessage
        ///// </summary>
        //public bool DeleteCIMMessage(CIMMessageData _entity)
        //{
        //    try
        //    {
        //        Equipment eqp = ObjectManager.EquipmentManager.GetEQP(_entity.NodeNo);
        //        if (eqp == null)
        //        {
        //            Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
        //                string.Format("Can't find the Equipment No [{0}]", _entity.NodeNo));
        //            return false;
        //        }
        //        eqp = null;
        //        // 找不到就跳出
        //        if (!eqpHistory.ContainsKey(_entity.NodeNo)) return false;
        //        eqpHistory[_entity.NodeNo].Dequeue();
        //        //foreach (CIMMessageData cim in eqpHistory[_entity.NodeNo])
        //        //{
        //        //    if (cim.MessageID == _entity.MessageID)
        //        //    {
        //        //        eqpHistory.Remove(_entity.NodeNo);
        //        //        return true;
        //        //    }
        //        //}              
        //        //EnqueueSave(his);
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //        return false;
        //    }
        //}

        ///// <summary>
        ///// 清除所有機台正在發生的CIMMessage
        ///// </summary>
        //public void ClearCIMMessage()
        //{
        //    try
        //    {
        //        eqpHistory.Clear();
        //        //foreach (CIMMessageData his in eqpHistory.Values)
        //        //{
        //        //    //his.HappingCIMMessages.Clear();
                    
        //        //    //EnqueueSave(his);
        //        //}
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        ///// <summary>
        ///// 清除指定機台正在發生的CIMMessage
        ///// </summary>
        ///// <param name="eqpNo">指定機台名稱</param>
        //public void ClearCIMMessage(string eqpNo)
        //{
        //    try
        //    {
        //        if (eqpHistory.ContainsKey(eqpNo))
        //        {
        //            eqpHistory[eqpNo].Dequeue();
        //            //eqpHistory.Remove(eqpNo);
        //            //EnqueueSave(his);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
        //}

        /// <summary>
        /// 檢查指定機台是否有CIM Message發生
        /// </summary>
        //public bool CheckContainsKey(string eqpNo, string id)
        //{
        //    try
        //    {
        //        if (!eqpHistory.ContainsKey(eqpNo)) return false;
        //        //if (!eqpHistory[eqpNo].HappingCIMMessages.ContainsKey(id)) return false;

        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //        return false;
        //    }
        //}

        

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

        #endregion

        public System.Data.DataTable GetDataTable()
        {
            //try
            //{
            //    DataTable dt = new DataTable();
            //    CIMMESSAGEHISTORY data = new CIMMESSAGEHISTORY();
            //    CIMMessageHistoryFile file = new CIMMessageHistoryFile();
            //    DataTableHelp.DataTableAppendColumn(data, dt);
            //    DataTableHelp.DataTableAppendColumn(file, dt);
            //    Dictionary<string, CIMMessageHistoryFile>.Enumerator enumerator = eqpHistory.GetEnumerator();
            //    while (enumerator.MoveNext())
            //    {
            //        DataRow dr = dt.NewRow();
            //        DataTableHelp.DataRowAssignValue(enumerator.Current.Value, dr);
            //        dt.Rows.Add(dr);
            //    }
            //    return dt;
            //}
            //catch (System.Exception ex)
            //{
            //    return null;
            //}
            return null;
        }

        /// <summary>
        /// Save Terminal Message to Database notify OPI
        /// </summary>
        /// <param name="trxId"> trx ID</param>
        /// <param name="serverName"> ServerName</param>
        /// <param name="message"> text</param>
        /// <param name="level">Level default 0 (0~65535)</param>
        //public void SaveTerminalMessage(string trxId,string serverName, string message, int level = 0) {
        //    try
        //    {
        //        TERMINALMSGINFORM terminalMessage = new TERMINALMSGINFORM();
        //        terminalMessage.AGENTTRACKKEY = trxId;
        //        terminalMessage.MSGLEVEL = level;
        //        terminalMessage.SERVERNAME = serverName;
        //        terminalMessage.TERMINALTEXT = message;
        //        HibernateAdapter.SaveObject(terminalMessage);
        //    }
        //    catch (System.Exception ex)
        //    {
        //        Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
        //    }
          
        //}

        /// <summary>
        /// 将Terminal Message 存储到DB中
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="trxId"></param>
        /// <param name="lineId"></param>
        /// <param name="caption"></param>
        public void SaveTerminalMessage(string msg, string trxId, string lineId, string caption)
        {
              try
              {
                    TERMINALMESSAGEHISTORY tmh = new TERMINALMESSAGEHISTORY();
                    tmh.CAPTION = caption;
                    tmh.LINEID = lineId;
                    tmh.TERMINALTEXT = msg;
                    tmh.TRANSACTIONID = trxId;
                    tmh.UPDATETIME = DateTime.Now;
                    HibernateAdapter.SaveObject(tmh);
              }
              catch (Exception ex)
              {
                    Log.NLogManager.Logger.LogErrorWrite(this.LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
              }
        }
    }
}
