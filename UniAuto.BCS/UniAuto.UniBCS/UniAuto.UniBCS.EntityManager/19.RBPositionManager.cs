using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.DB;
using System.Collections;
using UniAuto.UniBCS.Log;
using System.Reflection;
using UniAuto.UniBCS.Core;
using System.Data;

namespace UniAuto.UniBCS.EntityManager
{
    public class RBPositionManager : IDataSource
    {
        private IDictionary<string, RobotPosition> _robotPositions = new Dictionary<string, RobotPosition>();

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
            _robotPositions = Reload();
        }

        public IDictionary<string, RobotPosition> Reload()
        {
            try
            {
                IDictionary<string, RobotPosition> rebotPostion = new Dictionary<string, RobotPosition>();
                string hql = string.Format("from RobotPositionEntityData where LINETYPE = '{0}' order by NODENO,ROBOTPOSITIONNO ", Workbench.LineType);
                IList list = this.HibernateAdapter.GetObjectByQuery(hql);
                if (list != null)
                {
                    foreach (RobotPositionEntityData position in list)
                    {
                        if (rebotPostion.ContainsKey(position.ROBOTPOSITIONNO))
                        {
                            rebotPostion.Remove(position.ROBOTPOSITIONNO);
                        }
                        rebotPostion.Add(position.ROBOTPOSITIONNO, new RobotPosition(position));
                    }
                }
                return rebotPostion;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }

        /// <summary>
        /// By Robot Position No Get RobotPosition 
        /// </summary>
        /// <param name="equipmentNo"></param>
        /// <param name="robotPositionNo"></param>
        /// <returns></returns>
        public RobotPosition GetRobotPosition(string robotPositionNo)
        {
            try
            {
                if (_robotPositions.ContainsKey(robotPositionNo))
                {
                    return _robotPositions[robotPositionNo];
                }
                return null;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
                return null;
            }
        }

        public List<RobotPosition> GetRobotPositions()
        {
            try
            {
                return _robotPositions.Values.ToList<RobotPosition>();
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
            entityNames.Add("RBPositionManager");
            return entityNames;
        }

        public System.Data.DataTable GetDataTable(string entityName)
        {
            try
            {
                DataTable dt = new DataTable();
                RobotPositionEntityData data = new RobotPositionEntityData();
                DataTableHelp.DataTableAppendColumn(data, dt);
                dt.Columns.Add("Mode");
                dt.Columns.Add("Action");

                List<RobotPosition> robotPosition = GetRobotPositions();
                foreach (RobotPosition rp in robotPosition)
                {
                    DataRow dr = dt.NewRow();
                    DataTableHelp.DataRowAssignValue(rp.Data, dr);
                    dr["Mode"] = rp.Mode.ToString();
                    dr["Action"] = rp.Action.ToString();
                    dt.Rows.Add(dr);
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
