using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.Entity;
using System.IO;
using System.Data;
using UniAuto.UniBCS.Log;
using System.Reflection;
using System.Collections;
namespace UniAuto.UniBCS.EntityManager
{
    public class RobotStageManager : EntityManager, IDataSource
    {
        private Dictionary<string, RobotStage> _entities = new Dictionary<string, RobotStage>();

        private List<EntityData> _tempEntityDatas = null;

        #region 繼承EntityManager

        public override EntityManager.FILE_TYPE GetFileType()
        {
            return FILE_TYPE.BIN;
        }

        protected override string GetSelectHQL()
        {
            return string.Format("from RobotStageEntityData where SERVERNAME = '{0}' and ISENABLED = 'Y'", BcServerName);
        }

        protected override Type GetTypeOfEntityData()
        {
            return typeof(RobotStageEntityData);
        }

        //產生序列化文件
        protected override void AfterSelectDB(List<EntityData> EntityDatas, string FilePath, out List<string> Filenames)
        {
            Filenames = new List<string>();
            _tempEntityDatas = EntityDatas;

            foreach (EntityData entity_data in EntityDatas)
            {
                RobotStageEntityData robot_entity_data = entity_data as RobotStageEntityData;
                if (robot_entity_data != null)
                {
                    //以RobotName+StageNo為檔名
                    //string file_name = string.Format("{0}_{1}.bin",robot_entity_data.ROBOTNAME, robot_entity_data.STAGEID);
                    string file_name = string.Format("{0}_{1}.{2}", robot_entity_data.ROBOTNAME, robot_entity_data.STAGEID,GetFileExtension());
                    Filenames.Add(file_name);
                }
            }

        }

        protected override Type GetTypeOfEntityFile()
        {
            return typeof(RobotStageEntityFile);
        }

        //確認檔案是否存在,不存在就新建
        protected override EntityFile NewEntityFile(string Filename)
        {
            //return new RobotStageEntityFile();
            int maxCount = 1;
            string[] strTmp = Filename.Substring(0, Filename.Length - ".bin".Length).Split('_');
            string robot_id=string.Empty;
            string stage_id =string.Empty;
            //string stage_id = Filename.Substring(0, Filename.Length - ".bin".Length);

            if (strTmp.Length > 1)
             {
                robot_id = strTmp[0];
                stage_id = strTmp[1];

                foreach (RobotStageEntityData data in _tempEntityDatas)
                {
                    if (data.STAGEID == stage_id && data.ROBOTNAME == robot_id)
                    {
                        if (data.SLOTMAXCOUNT > 0)
                            maxCount = data.SLOTMAXCOUNT;
                        else
                            maxCount = 1;

                        #region [ by StageType initial Stage Signal and Multi Slot Info ]

                        return new RobotStageEntityFile(maxCount, data.STAGETYPE.ToUpper());

                        #endregion

                    }
                }
                return null;
            }
            else
            {
                return null;
            }
        }

        //以StageNo 存放在Dic
        protected override void AfterInit(List<EntityData> entityDatas, List<EntityFile> entityFiles)
        {
            string mapKey = string.Empty;

            foreach (EntityData entity_data in entityDatas)
            {
                RobotStageEntityData robotStage_entity_data = entity_data as RobotStageEntityData;

                if (robotStage_entity_data != null)
                {
                    foreach (EntityFile entity_file in entityFiles)
                    {
                        RobotStageEntityFile robotStage_entity_file = entity_file as RobotStageEntityFile;
                        if (robotStage_entity_file != null)
                        {
                            //RBName_StageNo.bin
                            string fextname = robotStage_entity_file.GetFilename();
                            string fname = Path.GetFileNameWithoutExtension(fextname);
                            //Check Dic Key
                            mapKey = string.Format("{0}_{1}",robotStage_entity_data.ROBOTNAME, robotStage_entity_data.STAGEID);
                            //if (string.Compare(robotStage_entity_data.STAGENO, fname, true) == 0)
                            if (string.Compare(mapKey, fname, true) == 0)
                                _entities.Add(robotStage_entity_data.STAGEID, new RobotStage(robotStage_entity_data, robotStage_entity_file));
                        }
                    }
                }
            }
        }

        #endregion

        /// <summary>
        /// 外部EQP 透過NodeNo ,DirectKey ,PathNo取得單一RobotStage
        /// </summary>
        /// <param name="nodeNo"></param>
        /// <param name="pathNo"></param>
        /// <param name="directKey">UPSTREAMPATH or DOWNSTREAMPATH</param>
        /// <returns></returns>
        public RobotStage GetRobotStagebyLinkSignal(string nodeNo ,string trxKey,string directKey)
        {
            RobotStage ret = null;
           
            foreach (RobotStage item in _entities.Values)
            {
                //by Path 只能針對外部Stage
                if (item.Data.STAGETYPE == "EQUIPMENT" && item.Data.NODENO == nodeNo) {
                    if (directKey == eLinekSignalDirect.UPSTREAM) {
                        if (item.Data.UPSTREAMPATHTRXNAME == trxKey)
                            ret= item;
                    }
                    else if (directKey == eLinekSignalDirect.DOWNSTREAM) {
                        if (item.Data.DOWNSTREAMPATHTRXNAME == trxKey) {
                            ret= item;
                        }
                    }
                }
            }

            return ret;
        }

        /// <summary> get stage by stage ID
        ///
        /// </summary>
        /// <param name="stageID"></param>
        /// <returns></returns>
        public RobotStage GetRobotStagebyStageID(string stageID)
        {
            RobotStage ret = null;

            foreach (RobotStage roboStagetitem in _entities.Values)
            {
                if (roboStagetitem.Data.STAGEID == stageID)
                {
                    ret = roboStagetitem;
                    break;
                }
            }

            return ret;
        }
        
        /// <summary>
        /// Get robot control port
        /// </summary>
        /// <param name="nodeNo"></param>
        /// <param name="portNo"></param>
        /// <returns></returns>
        public RobotStage GetRobotStagebyPortNo(string nodeNo, string portNo)
        {
            RobotStage ret = null;

            foreach (RobotStage roboStagetitem in _entities.Values)
            {
                
                //20150105 add for Trx上報都為0x結構 所以StageNo要補0
                if (roboStagetitem.Data.NODENO == nodeNo 
                    && roboStagetitem.Data.STAGETYPE.ToUpper()==eRobotStageType.PORT 
                    && roboStagetitem.Data.STAGEID == portNo)
                {
                    ret = roboStagetitem;
                    break;
                }
            }

            return ret;
        }

        /// <summary>
        /// Get fix buffer Stage 
        /// </summary>
        /// <param name="nodeNo"></param>
        /// <returns></returns>
        public RobotStage GetRobotBufferStagebyNodeNo(string nodeNo)
        {
            RobotStage ret = null;

            foreach (RobotStage roboStagetitem in _entities.Values)
            {

                if (roboStagetitem.Data.NODENO == nodeNo 
                    && roboStagetitem.Data.STAGETYPE.ToUpper() != eRobotStageType.FIXBUFFER 
                    && roboStagetitem.Data.ISMULTISLOT.ToUpper() == "Y")
                {
                    ret = roboStagetitem;
                    break;
                }
            }

            return ret;
        }

        /// <summary>
        /// Get Internal Stage by State No
        /// </summary>
        /// <param name="nodeNo"></param>
        /// <param name="internalStageNo"></param>
        /// <returns></returns>
        public RobotStage GetInternalRobotStagebyStageNo(string nodeNo, string internalStageNo)
        {
            RobotStage ret = null;

            foreach (RobotStage roboStagetitem in _entities.Values)
            {
                //POSITIONINDEX 是指內部Stage上的Jobdata在EQ的each Position那個位置, INTERNALPOS 是指內部Stage的編號
                if (roboStagetitem.Data.NODENO == nodeNo 
                    && roboStagetitem.Data.STAGETYPE == eRobotStageType.STAGE 
                    && roboStagetitem.Data.STAGEID ==internalStageNo)
                {
                    ret = roboStagetitem;
                    break;
                }
            }

            return ret;
        }

        public List<RobotStage> GetRobotStages()
        {
            List<RobotStage> ret = new List<RobotStage>();
            foreach (RobotStage entity in _entities.Values)
            {
                ret.Add(entity);
            }
            return ret;
        }

        //20141023 add Get RobotStages by RobotName
        public List<RobotStage> GetRobotStages(string robotName)
        {
            List<RobotStage> ret = new List<RobotStage>();
            foreach (RobotStage entity in _entities.Values)
            {
                if (entity.Data.ROBOTNAME.ToUpper().Trim() == robotName.ToUpper().Trim())
                {
                    ret.Add(entity);
                }
            }
            return ret;
        }

        public IList<string> GetEntityNames()
        {
            IList<string> entityNames = new List<string>();
            entityNames.Add("RobotStageManager");
            return entityNames;
        }

        //提供給外部取得Dt做Show在BC上
        public System.Data.DataTable GetDataTable(string entityName)
        {
            try
            {
                DataTable dt = new DataTable();
                RobotStageEntityData data = new RobotStageEntityData();
                RobotStageEntityFile file = new RobotStageEntityFile();
                DataTableHelp.DataTableAppendColumn(data, dt);
                DataTableHelp.DataTableAppendColumn(file, dt);

                List<RobotStage> robotStage_entities = GetRobotStages();
                foreach (RobotStage entity in robotStage_entities)
                {
                    DataRow dr = dt.NewRow();
                    DataTableHelp.DataRowAssignValue(entity.Data, dr);
                    DataTableHelp.DataRowAssignValue(entity.File, dr);
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

        //20141215 add for Reload RobotStageDataEntity
        protected Dictionary<string, RobotStageEntityData> LoadRobotStageDataEntity()
        {
            try
            {
                Dictionary<string, RobotStageEntityData> robotStageDataEntitys = new Dictionary<string, RobotStageEntityData>();

                string hql = string.Format("from RobotStageEntityData where SERVERNAME = '{0}'", BcServerName);

                IList list = HibernateAdapter.GetObjectByQuery(hql);

                if (list != null)
                {
                    foreach (RobotStageEntityData data in list)
                    {
                        //RobotName_StageNo
                        string key = string.Format("{0}_{1}", data.ROBOTNAME, data.STAGEID);

                        if (robotStageDataEntitys.ContainsKey(key))
                        {
                            robotStageDataEntitys.Remove(key);
                        }
                        robotStageDataEntitys.Add(key, data);
                    }
                }
                return robotStageDataEntitys;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                throw ex;
            }
        }


        public void ReloadRobotStage()
        {
            try
            {
                Dictionary<string, RobotStageEntityData> robotStageEntitys = LoadRobotStageDataEntity();

                if (robotStageEntitys != null)
                {

                    foreach (string itemkey in   robotStageEntitys.Keys)
                    {
                        RobotStageEntityData curDataEntity = robotStageEntitys[itemkey];

                        RobotStage curStage = GetRobotStagebyStageID(curDataEntity.STAGEID);

                        if (curStage != null)
                        {
                            lock (curStage)
                            {
                                curStage.Data = curDataEntity;
                            }
                        }
                        else
                        {
                            //Stage新增則BCS需要重開以免風險太大(參照Node)
                            NLogManager.Logger.LogInfoWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name,
                           string.Format("Reload all Robot Stage and Find Create New Stage! Please Reset BCS."));
                        }

                    }

                    NLogManager.Logger.LogInfoWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name,
                           string.Format("Reload all Robot Stage."));
                }
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);

            }
        }

    }
}
