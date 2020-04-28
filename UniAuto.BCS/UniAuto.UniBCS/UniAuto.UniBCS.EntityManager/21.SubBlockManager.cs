using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.DB;
using UniAuto.UniBCS.Log;
using System.Reflection;
using System.Collections;
using UniAuto.UniBCS.Core;
using System.Data;

namespace UniAuto.UniBCS.EntityManager
{
    public class SubBlockManager:IDataSource
    {
        /// <summary>
        /// 所有需要拆解的SubJobData
        /// </summary>
        private IDictionary<string, IList<SubBlock>> _subBlocks = new Dictionary<string, IList<SubBlock>>();

        public IDictionary<string, IList<SubBlock>> SubBlocks
        {
            get { return _subBlocks; }
            set { _subBlocks = value; }
        }

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
            _subBlocks = Reload();
        }

        /// <summary>
        /// Load SubBlocks
        /// </summary>
        /// <returns></returns>
        public IDictionary<string, IList<SubBlock>> Reload()
        {
            try
            {
                IDictionary<string, IList<SubBlock>> subBlocks = new Dictionary<string, IList<SubBlock>>();
                string hql = string.Format("from SubBlockCtrlEntityData where SERVERNAME = '{0}'", Workbench.ServerName);
                IList list = this.HibernateAdapter.GetObjectByQuery(hql);
                IList<SubBlock> subBlockList = null;
                if (list != null)
                {
                    foreach (SubBlockCtrlEntityData subBlock in list)
                    {
                        if (!subBlocks.ContainsKey(subBlock.STARTEQP))
                        {
                            subBlockList = new List<SubBlock>();
                            subBlocks.Add(subBlock.STARTEQP, subBlockList);
                        }
                        subBlocks[subBlock.STARTEQP].Add(new SubBlock(subBlock));
                        //if (subBlock.STARTEQP.Contains("P"))
                        //{
                        //    string portId = subBlock.STARTEQP.Split(new char[] { ':' })[1];
                        //    string portNo = portId.Substring(1, portId.Length - 1);
                        //    Port port = ObjectManager.PortManager.GetPort(Workbench.ServerName, subBlock.STARTEQP.Split(new char[] { ':' })[0], portNo);
                        //    lock (port.File) port.File.PortIntelockNo = subBlock.INTERLOCKNO;
                        //    ObjectManager.PortManager.EnqueueSave(port.File);
                        //}
                    }
                }
                return subBlocks;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }

        /// <summary>
        /// Reload SubBlocks by UI
        /// </summary>
        /// <returns></returns>
        public void ReloadByUI()
        {
            try
            {
                string hql = string.Format("from SubBlockCtrlEntityData where SERVERNAME = '{0}'", Workbench.ServerName);
                IList list = this.HibernateAdapter.GetObjectByQuery(hql);
                IList<SubBlock> subBlockList = null;
                if (list != null)
                {
                    lock (_subBlocks)
                    {
                        _subBlocks=new Dictionary<string, IList<SubBlock>>();
                        foreach (SubBlockCtrlEntityData subBlock in list)
                        {
                            if (!_subBlocks.ContainsKey(subBlock.STARTEQP))
                            {
                                subBlockList = new List<SubBlock>();
                                _subBlocks.Add(subBlock.STARTEQP, subBlockList);
                            }
                            _subBlocks[subBlock.STARTEQP].Add(new SubBlock(subBlock));
                            if (subBlock.STARTEQP.Contains("P"))
                            {
                                string portId = subBlock.STARTEQP.Split(new char[] { ':' })[1];
                                string portNo = portId.Substring(1, portId.Length - 1);
                                Port port = ObjectManager.PortManager.GetPort(Workbench.ServerName, subBlock.STARTEQP.Split(new char[] { ':' })[0], portNo);
                                lock (port.File) port.File.PortIntelockNo = subBlock.INTERLOCKNO;
                                ObjectManager.PortManager.EnqueueSave(port.File);
                            }
                        }
                    }
                }
                NLogManager.Logger.LogInfoWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name,
                            "Reload SubBlock by UI!");

            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        /// <summary>
        /// 获取对应node+unit的SubBlock
        /// </summary>
        /// <param name="nodeno"></param>
        /// <param name="unitno"></param>
        /// <param name="eventName"></param>
        /// <returns></returns>
        public IDictionary<string, IList<SubBlock>> GetBlock(string nodeno,string unitno,string eventName)
        {
            try
            {
                IDictionary<string, IList<SubBlock>> subBlocks = new Dictionary<string, IList<SubBlock>>();
                string startEq="";
                if (unitno == "" || unitno=="0")
                {
                    startEq = nodeno;
                }
                else
                {
                    startEq = string.Format("{0}:{1}", nodeno, unitno);
                }
                if (!_subBlocks.Keys.Contains(startEq))
                {
                    return null;
                }
                IList<SubBlock> list = _subBlocks[startEq];
                IList<SubBlock> subBlockList = null;
                foreach (SubBlock subBlock in list)
                {
                    if (subBlock.Data.STARTEVENTMSG==eventName)
                    {
                        if (!subBlocks.ContainsKey(startEq))
                        {
                            subBlockList = new List<SubBlock>();
                            subBlocks.Add(startEq, subBlockList);
                        }
                        subBlocks[startEq].Add(new SubBlock(subBlock.Data));
                    }
                }
                return subBlocks;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        /// <summary>
        /// 获取对应node+port的SubBlock
        /// </summary>
        /// <param name="nodeno"></param>
        /// <param name="portno"></param>
        /// <returns></returns>
        public IDictionary<string, IList<SubBlock>> GetBlock(string nodeno, string portno)
        {
            try
            {
                IDictionary<string, IList<SubBlock>> subBlocks = new Dictionary<string, IList<SubBlock>>();
                string startEq = "";
                if (!portno.Contains("P"))
                {
                    return subBlocks;
                }
                else
                {
                    startEq = string.Format("{0}:{1}", nodeno, portno);
                }
                if (!_subBlocks.Keys.Contains(startEq))
                {
                    return null;
                }
                IList<SubBlock> list = _subBlocks[startEq];
                IList<SubBlock> subBlockList = null;
                foreach (SubBlock subBlock in list)
                {
                    if (!subBlocks.ContainsKey(startEq))
                    {
                        subBlockList = new List<SubBlock>();
                        subBlocks.Add(startEq, subBlockList);
                    }
                    subBlocks[startEq].Add(new SubBlock(subBlock.Data));                    
                }
                return subBlocks;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        public IList<string> GetEntityNames()
        {
            IList<string> entityNames = new List<string>();
            entityNames.Add("SubBlockManager");
            return entityNames;
        }

        public DataTable GetDataTable(string entityName)
        {
            try
            {
                DataTable dt = new DataTable();
                SubBlockCtrlEntityData data = new SubBlockCtrlEntityData();

                DataTableHelp.DataTableAppendColumn(data, dt);

                foreach (string key in _subBlocks.Keys)
                {
                    IList<SubBlock> specs = _subBlocks[key];
                    foreach (SubBlock spec in specs)
                    {
                        DataRow dr = dt.NewRow();
                        DataTableHelp.DataRowAssignValue(spec.Data, dr);

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

       
    }
}
