using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UniAuto.UniBCS.Entity;
using System.Collections;
using UniAuto.UniBCS.Log;
using UniAuto.UniBCS.Core;
using UniAuto.UniBCS.MISC;

namespace UniAuto.UniBCS.EntityManager
{
    public class EquipmentManager : EntityManager, IDataSource
	{
        private Dictionary<string, Equipment> _entities = new Dictionary<string, Equipment>();
        private Dictionary<string,Dictionary<string,List<SECSVARIABLEDATA>>> _secsVariableData=new Dictionary<string,Dictionary<string,List<SECSVARIABLEDATA>>>();

        public override EntityManager.FILE_TYPE GetFileType()
        {
            return FILE_TYPE.BIN;
            //return FILE_TYPE.JSON;
        }

        protected override string GetSelectHQL()
        {
            return string.Format("from EquipmentEntityData where SERVERNAME = '{0}'", BcServerName);
        }

        protected override Type GetTypeOfEntityData()
        {
            return typeof(EquipmentEntityData);
        }

        protected override void AfterSelectDB(List<EntityData> EntityDatas, string FilePath, out List<string> Filenames)
        {
            Filenames = new List<string>();
            foreach (EntityData entity_data in EntityDatas)
            {
                EquipmentEntityData eqp_entity_data = entity_data as EquipmentEntityData;
                if (eqp_entity_data != null)
                {
                    //string file_name = string.Format("{0}.bin", eqp_entity_data.NODEID);
                    string file_name = string.Format("{0}.{1}", eqp_entity_data.NODEID,GetFileExtension());
                    Filenames.Add(file_name);
                }
            }
        }

        protected override Type GetTypeOfEntityFile()
        {
            return typeof(EquipmentEntityFile);
        }

        protected override EntityFile NewEntityFile(string Filename)
        {
            return new EquipmentEntityFile();
        }

        protected override void AfterInit(List<EntityData> entityDatas, List<EntityFile> entityFiles)
        {
            foreach (EntityData entity_data in entityDatas)
            {
                EquipmentEntityData eqp_entity_data = entity_data as EquipmentEntityData;
                if (eqp_entity_data != null)
                {
                    foreach (EntityFile entity_file in entityFiles)
                    {
                        EquipmentEntityFile eqp_entity_file = entity_file as EquipmentEntityFile;
                        if (eqp_entity_file != null)
                        {
                            string fextname = eqp_entity_file.GetFilename();
                            string fname = Path.GetFileNameWithoutExtension(fextname);
                            if (string.Compare(eqp_entity_data.NODEID, fname, true) == 0)
                            {
                                if (eqp_entity_data.VCRCOUNT > 0 && eqp_entity_file.VcrMode.Count() == 0)
                                {
                                    for (int i=0; i < eqp_entity_data.VCRCOUNT; i++) eqp_entity_file.VcrMode.Add(eBitResult.OFF);
                                    EnqueueSave(eqp_entity_file);
                                }
                                _entities.Add(eqp_entity_data.NODEID, new Equipment(eqp_entity_data, eqp_entity_file));
                            }
                        }
                    }
                }
            }
            ReloadSECSVariableDataAll();
        }

        private Dictionary<string, List<SECSVARIABLEDATA>> LoadSECSVariableData(Equipment eqp)
        {
            string hql = string.Format("from SECSVARIABLEDATA where NODENO='{0}' and LINEID='{1}' order by TRID,ITEM_ID ", eqp.Data.NODENO, eqp.Data.LINEID);

            if (HibernateAdapter == null)
            {
                throw new Exception("Hibernate Adapter is Null. Cant get data from Data");
            }
            Dictionary<string,List<SECSVARIABLEDATA>> variables=new Dictionary<string,List<SECSVARIABLEDATA>>();
            IList list = HibernateAdapter.GetObjectByQuery(hql);

            if (list != null)
            {
                foreach (SECSVARIABLEDATA data in list)
                {
                    if(variables.ContainsKey(data.TRID))
                    {
                        variables[data.TRID].Add(data);
                    }
                    else
                    {
                        List<SECSVARIABLEDATA> variableList = new List<SECSVARIABLEDATA>();
                        variableList.Add(data);
                        variables.Add(data.TRID, variableList);
                    }
                }
            }
            return variables;
             
        }

        /// <summary>
        /// Load SECS Variable Data 
        /// </summary>
        public void ReloadSECSVariableDataAll()
        {
            foreach (Equipment eqp in _entities.Values)
            {
                eReportMode reportMode;
                Enum.TryParse<eReportMode>(eqp.Data.REPORTMODE, out reportMode);
                if (reportMode == eReportMode.HSMS_CSOT || reportMode == eReportMode.HSMS_NIKON)
                {
                    Dictionary<string, List<SECSVARIABLEDATA>> variableDatas = LoadSECSVariableData(eqp);
                    lock (_secsVariableData)
                    {
                        if (_secsVariableData.ContainsKey(eqp.Data.NODENO))
                        {
                            _secsVariableData[eqp.Data.NODENO] = variableDatas;
                        }
                        else
                        {
                            _secsVariableData.Add(eqp.Data.NODENO, variableDatas);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// if equipment Reportmode is not HSMS_CSOT or HSMS_NIKON 
        /// this function Throw Exception
        /// </summary>
        /// <param name="eqpNo"></param>
        public void ReloadSECSVariableDataByEqpNo(string eqpNo)
        {
            Equipment eqp = GetEQP(eqpNo);
            if (eqp == null)
                throw new Exception(string.Format("Canot found Equipment No ({0})", eqpNo));
            eReportMode reportMode;
            Enum.TryParse<eReportMode>(eqp.Data.REPORTMODE, out reportMode);
            if (reportMode == eReportMode.HSMS_CSOT || reportMode == eReportMode.HSMS_NIKON || reportMode == eReportMode.HSMS_PLC) //20150131 cy add HSMS_PLC
            {
                Dictionary<string, List<SECSVARIABLEDATA>> variables= LoadSECSVariableData(eqp);
                lock (_secsVariableData)
                {
                    if (_secsVariableData.ContainsKey(eqpNo))
                    {
                        _secsVariableData[eqpNo] = variables;
                    }
                    else
                    {
                        _secsVariableData.Add(eqpNo, variables);
                    }
                }
            }
            else
            {
                throw new Exception(string.Format("Equipment No ({0}) reportMode is {1} not Supper this function. ", eqpNo,reportMode));
            }
        }

        /// <summary>
        /// Reload Recipe Reister validation Enable
        /// </summary>
        public void ReloadRecipeCheckFlagAll()
        {
            IList entiyDatas = HibernateAdapter.GetObjectByQuery(GetSelectHQL());

            if (entiyDatas != null)
            {
                foreach (EquipmentEntityData eqp in entiyDatas)
                {
                    Equipment eq = GetEQP(eqp.NODENO);
                    if ((eq != null) && (eq.Data.RECIPEREGVALIDATIONENABLED!=eqp.RECIPEREGVALIDATIONENABLED))
                    {
                        lock (eq)
                        {
                            eq.Data.RECIPEREGVALIDATIONENABLED = eqp.RECIPEREGVALIDATIONENABLED;
                        }

                    }

                }
            }
        }

        /// <summary>
        /// Reload Recipe Parameter validation Enable
        /// </summary>
        public void ReloadRecipeParamCheckFlagAll()
        {
            IList entiyDatas = HibernateAdapter.GetObjectByQuery(GetSelectHQL());

            if (entiyDatas != null)
            {
                foreach (EquipmentEntityData eqp in entiyDatas)
                {
                    Equipment eq = GetEQP(eqp.NODENO);
                    if ((eq != null) && (eq.Data.RECIPEPARAVALIDATIONENABLED != eqp.RECIPEPARAVALIDATIONENABLED))
                    {
                        lock (eq)
                        {
                            eq.Data.RECIPEPARAVALIDATIONENABLED = eqp.RECIPEPARAVALIDATIONENABLED;
                        }

                    }

                }
            }
        }

        /// <summary>
        /// Reload Proflie Version
        /// </summary>
        public void ReloadProfileVersion()
        {
            IList entiyDatas = HibernateAdapter.GetObjectByQuery(GetSelectHQL());

            if (entiyDatas != null)
            {
                foreach (EquipmentEntityData eqp in entiyDatas)
                {
                    Equipment eq = GetEQP(eqp.NODENO);
                    if ((eq != null) && (eq.Data.EQPPROFILE != eqp.EQPPROFILE))
                    {
                        lock (eq)
                        {
                            eq.Data.EQPPROFILE = eqp.EQPPROFILE;
                        }

                    }

                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eqp"></param>
        /// <param name="trid"></param>
        /// <returns></returns>
        public List<SECSVARIABLEDATA> GetVariableData(string eqp, string trid)
        {

            if (_secsVariableData.ContainsKey(eqp))
            {
                if (_secsVariableData[eqp].ContainsKey(trid))
                {
                    return _secsVariableData[eqp][trid];
                }
            }
            return null;
        }

        /// <summary>
        /// UPK Line By Equipment No Get Other EQP ID
        /// </summary>
        /// <param name="EQPNo"></param>
        /// <returns></returns>
        public string GetEQPID(string eqpNo)
        {
            string EQPID = string.Empty;

            Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
            if (eqp == null) //Watson add 20150126 For MES可能會傳入不對的機台NO (Recipe check)
                return EQPID;

            Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);
            if (line == null)
            {
                EQPID = eqp.Data.NODEID;
                return EQPID;
            }

            if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1 ||
                line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE2)
            {
                if (eqpNo != "")
                {
                    switch (line.File.UPKEquipmentRunMode)
                    {
                        case eUPKEquipmentRunMode.TFT:
                            EQPID = "T" + eqp.Data.NODEID.Substring(1);
                            break;
                        case eUPKEquipmentRunMode.CF:
                            EQPID = eqp.Data.NODEID;
                            break;
                        default:
                            EQPID = eqp.Data.NODEID;
                            break;
                    }
                }
                else
                {
                    EQPID = eqp.Data.NODEID;
                }
            }
            //else if (line.Data.LINETYPE == eLineType.CELL.CBPMT)
            //{
            //    if (line.Data.LINEID.Contains(keyCELLPMTLINE.CBPTI))
            //    {
            //        if (eqpNo == "L2")
            //        {
            //            try
            //            {
            //                ParameterManager pm = Workbench.Instance.GetObject("ParameterManager") as ParameterManager;
            //                if (pm != null)
            //                    EQPID = pm[keyCELLPTIParameter.CELL_PTI_NODEID].GetString();
            //            }
            //            catch (Exception ex)
            //            {
            //                string err = "Parameter File not key in PTI Line Data, 'CELL_PTI_NODEID'";
            //                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex + err);
            //            }
            //        }
            //        else
            //            EQPID = eqp.Data.NODEID;
            //    }
            //    else
            //    {
            //        EQPID = eqp.Data.NODEID;
            //    }
            //}
            else
            {
                EQPID = eqp.Data.NODEID;
            }

            return EQPID;
        }

        /// <summary>
        /// By Equipment No Select Node no
        /// </summary>
        /// <param name="eqpNo"></param>
        /// <returns></returns>
        public Equipment GetEQP(string eqpNo)
		{
            Equipment ret = null;
            foreach (Equipment entity in _entities.Values)
			{
                if (entity.Data.NODENO == eqpNo)
				{
					ret = entity;
					break;
				}
			}
			return ret;
		}

        /// <summary>
        /// by Equipment ID Select Equipment object
        /// </summary>
        /// <param name="eqpID"></param>
        /// <returns></returns>
        public Equipment GetEQPByID(string eqpID)
        {
            string _eqpID = string.Empty;
            Equipment ret = null;

            foreach (Line line in ObjectManager.LineManager.GetLines())
            {
                if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1 ||
                    line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE2)
                {
                    _eqpID = "F" + eqpID.Substring(1);
                    if (_entities.ContainsKey(_eqpID))
                    {
                        ret = _entities[_eqpID];
                    }
                    return ret;
                }
            }

            if (_entities.ContainsKey(eqpID))
            {
                ret = _entities[eqpID];
            }
            return ret;
        }

        //public EquipmentEntity GetNode(string nodeID)
        //{
        //    EquipmentEntity ret = null;
        //    if (_entities.ContainsKey(nodeID))
        //        ret = _entities[nodeID];
        //    return ret;
        //}

        public List<Equipment> GetEQPs()
		{
            List<Equipment> ret = new List<Equipment>();
            foreach (Equipment entity in _entities.Values)
			{
				ret.Add(entity);
			}
			return ret;
		}

        public List<Equipment> GetEQPs_FA()
        {
            List<Equipment> ret = new List<Equipment>();
            lock (_entities)
            {
                ret = _entities.Values.Where(w => w.Data.NODEATTRIBUTE == "LD" || w.Data.NODEATTRIBUTE == "UD"||w.Data.NODEATTRIBUTE=="LU").ToList<Equipment>();
            }
            return ret;

        }

        public List<Equipment> GetEQPsByLine(string lineID)
        {
            List<Equipment> ret = new List<Equipment>();
            foreach (Equipment entity in _entities.Values)
            {
                if (entity.Data.LINEID == lineID)
                    ret.Add(entity);
            }
            return ret;
        }

        //Add By Yangzhenteng 20190508
        public List<Equipment> GetEQPsByEQPNOList(List<string> EQPNoList)
        {
            List<Equipment> ret = new List<Equipment>();
            lock (_entities)
            {
              ret = _entities.Values.Where(w => EQPNoList.Contains(w.Data.NODENO)).ToList<Equipment>();
            }
            return ret;
        }

        //private void DataTableAppendColumn(object obj, DataTable dataTable)
        //{
        //    Type type = obj.GetType();
        //    PropertyInfo[] properties = type.GetProperties();
        //    foreach (PropertyInfo prop in properties)
        //    {
        //        if (prop.PropertyType != typeof(ICollection)) ;
        //        dataTable.Columns.Add(prop.Name, typeof(string));
        //    }
        //}

        //private void DataRowAssignValue(object obj, DataRow dataRow)
        //{

        //    Type type = obj.GetType();
        //    PropertyInfo[] properties = type.GetProperties();
        //    foreach (PropertyInfo prop in properties)
        //    {
        //        if (prop.PropertyType != typeof(ICollection))
        //        {
        //            object val = prop.GetValue(obj, null);
        //            if (val != null)
        //            {
        //                dataRow[prop.Name] = val.ToString();
        //            }
        //            else
        //            {
        //                dataRow[prop.Name] = "";
        //            }
        //        }
        //    }
        //}

        /// <summary>
        /// Save Equipment History  20150427 tom 
        /// </summary>
        /// <param name="eqp"></param>
        public void RecordEquipmentHistory(string trxID, Equipment eqp)
        {
            try
            {
                // Save DB
                EQUIPMENTHISTORY his = new EQUIPMENTHISTORY();

                his.LINEID = eqp.Data.LINEID;
                his.NODEID = eqp.Data.NODEID;
                his.NODENO = eqp.Data.NODENO;
                his.NODEATTRIBUTE = eqp.Data.NODEATTRIBUTE;
                his.CIMMODE = eqp.File.CIMMode == eBitResult.ON ? "CIM ON" : "CIM OFF";
                his.UPSTREAMINLINEMODE = eqp.File.UpstreamInlineMode.ToString();
                his.DOWNSTREAMINLINEMODE = eqp.File.DownstreamInlineMode.ToString();
                his.CURRENTRECIPEID = eqp.File.CurrentRecipeID;
                his.CURRENTSTATUS = eqp.File.Status.ToString();
                his.CFJOBCOUNT = eqp.File.TotalCFProductJobCount;
                his.TFTJOBCOUNT = eqp.File.TotalTFTJobCount;
                his.ALARMEXIST = eqp.File.LocalAlarmStatus == eBitResult.ON ? "ALARM EXIST" : "NO EXIST";
                his.EQUIPMENTOPERATORMODE = eqp.File.EquipmentOperationMode.ToString();
                his.AUTORECIPECHANGEFLAG = eqp.File.AutoRecipeChangeMode.ToString();
                his.EQUIPMENTRUNMODE = eqp.File.EquipmentRunMode;
                his.JOBDATACHECKMODE = eqp.File.JobDataCheckMode.ToString();
                his.JOBDUPLICATECHECKMODE = eqp.File.JobDuplicateCheckMode.ToString();
                his.PRODUCTIDCHECKMODE = eqp.File.ProductIDCheckMode.ToString();
                his.PRODUCTTYPECHECKMODE = eqp.File.ProductTypeCheckMode.ToString();
                his.RECIPEIDCHECKMODE = eqp.File.RecipeIDCheckMode.ToString();
                his.GROUPINDEXCHECKMODE = eqp.File.GroupIndexCheckMode.ToString();
                his.SAMPLINGRULE = eqp.File.SamplingRule.ToString();
                his.SAMPLINGUNIT = eqp.File.SamplingCount.ToString();
                his.SIDEINFORMATION = eqp.File.SamplingUnit.ToString();
                his.COAVERSIONCHECKCHECKMODE = eqp.File.COAVersionCheckMode.ToString(); //2016/04/06 Add by Frank
                his.TRANSACTIONID = trxID;
                ObjectManager.EquipmentManager.InsertDB(his);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }
        }

        public System.Data.DataTable GetDataTable(string entityName)
        {
            try
            {
                DataTable dt = new DataTable();
                EquipmentEntityData data = new EquipmentEntityData();
                EquipmentEntityFile file = new EquipmentEntityFile();
                DataTableHelp.DataTableAppendColumn(data, dt);
                DataTableHelp.DataTableAppendColumn(file, dt);

                List<Equipment> eqp_entities = GetEQPs();
                foreach (Equipment entity in eqp_entities)
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

      

        public IList<string> GetEntityNames()
        {
            IList<string> entityName = new List<string>();
            entityName.Add("EquipmentManager");
            return entityName;
        }
    }
}
