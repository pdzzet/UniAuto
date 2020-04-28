using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.Log;

namespace UniAuto.UniBCS.EntityManager
{
    public class PortManager : EntityManager, IDataSource
	{
		private Dictionary<string, Port> _entities = new Dictionary<string, Port>();

        private List<EntityData> _tempEntityDatas = null;

        public override EntityManager.FILE_TYPE GetFileType()
        {
            return FILE_TYPE.BIN;
            //return FILE_TYPE.JSON;
        }

        protected override string GetSelectHQL()
        {
            return string.Format("from PortEntityData where SERVERNAME = '{0}'", BcServerName);
        }

        protected override Type GetTypeOfEntityData()
        {
            return typeof(PortEntityData);
        }

        protected override void AfterSelectDB(List<EntityData> EntityDatas, string FilePath, out List<string> Filenames)
        {
            Filenames = new List<string>();
            _tempEntityDatas = EntityDatas;
            foreach (EntityData entity_data in EntityDatas)
            {
                PortEntityData port_entity_data = entity_data as PortEntityData;
                if (port_entity_data != null)
                {
                    //string file_name = string.Format("{0}.bin", port_entity_data.PORTID);
                    string file_name = string.Format("{0}.{1}", port_entity_data.PORTID,GetFileExtension());
                    Filenames.Add(file_name);
                }
            }
        }

        protected override Type GetTypeOfEntityFile()
        {
            return typeof(PortEntityFile);
        }

        protected override EntityFile NewEntityFile(string Filename)
        {
            int maxCount = 28;
            //string port_id = Filename.Substring(0, Filename.Length - ".bin".Length);
            int index = Filename.IndexOf('.');
            string port_id="";
            if (index != -1)
                port_id = Filename.Substring(0, index);
            else
                port_id = Filename;
            foreach (PortEntityData data in _tempEntityDatas)
            {
                if (data.PORTID == port_id)
                {
                    if (data.MAXCOUNT > 0)
                        maxCount = data.MAXCOUNT;
                    else
                        maxCount = 28;
                    break;
                }
            }
            return new PortEntityFile(maxCount);
        }

        protected override void AfterInit(List<EntityData> entityDatas, List<EntityFile> entityFiles)
        {
            foreach (EntityData entity_data in entityDatas)
            {
                PortEntityData port_entity_data = entity_data as PortEntityData;
                if (port_entity_data != null)
                {
                    foreach (EntityFile entity_file in entityFiles)
                    {
                        PortEntityFile port_entity_file = entity_file as PortEntityFile;
                        if (port_entity_file != null)
                        {
                            //port_entity_file.RedimArrayJoExistenceSlot(port_entity_data.MAXCOUNT);//  重新分配ARRAY Job Existence
                            string fextname = port_entity_file.GetFilename();
                            string fname = Path.GetFileNameWithoutExtension(fextname);
                            if (string.Compare(port_entity_data.PORTID, fname, true) == 0)
                                _entities.Add(port_entity_data.PORTID, new Port(port_entity_data, port_entity_file));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// UPK Line By Port ID Get Other Port ID
        /// </summary>
        /// <param name="PortID"></param>
        /// <returns></returns>
        public string GetPortID(string eqpNo, string portID)
        {
            string PortID = string.Empty;

            Equipment eqp = ObjectManager.EquipmentManager.GetEQP(eqpNo);
            Line line = ObjectManager.LineManager.GetLine(eqp.Data.LINEID);

            if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1 ||
                line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE2)
            {
                switch (line.File.UPKEquipmentRunMode)
                {
                    case eUPKEquipmentRunMode.TFT:
                        PortID = portID;
                        break;
                    case eUPKEquipmentRunMode.CF:
                        PortID = portID;
                        break;
                    default:
                        PortID = portID;
                        break;
                }
            }
            else
            {
                PortID = portID;
            }

            return PortID;
        }

		public Port GetPort(string lineID, string eqpNo, string portNo)
		{
			Port ret = null;
            //Sorry!! Watson Add 20150211 For CELL PMI Line 
            if (lineID.Contains(keyCELLPMTLINE.CBPMI))
            {
                foreach (Port entity in _entities.Values)
                {
                    if (entity.Data.PORTNO == portNo)
                    {
                        ret = entity;
                        break;
                    }
                }
            }
			foreach (Port entity in _entities.Values)
			{
                if (entity.Data.LINEID == lineID && entity.Data.NODENO == eqpNo && entity.Data.PORTNO == portNo)
				{
					ret = entity;
					break;
				}
			}
			return ret;
		}

        //Jun Add 20150113 Get Port By LineID, PortID
        public Port GetPortByLineIDPortID(string lineID, string portID)
        {
            Port ret = null;
            Line line = ObjectManager.LineManager.GetLine(lineID);
            if (line != null)
            {
                if (line.Data.LINETYPE == eLineType.CELL.CBCUT_3)
                {
                    foreach (Port entity in _entities.Values)
                    {
                        if (entity.Data.LINEID == lineID && entity.Data.PORTID == portID)
                        {
                            ret = entity;
                            break;
                        }
                    }
                }
                else
                {
                    if (_entities.ContainsKey(portID))
                        ret = _entities[portID];
                }
            }
            return ret;
        }

		public Port GetPort(string eqpID, string portNo)
		{
			Port ret = null;
			foreach (Port entity in _entities.Values)
			{
                if (entity.Data.NODEID == eqpID && entity.Data.PORTNO == portNo)
				{
					ret = entity;
					break;
				}
			}
			return ret;
		}

        public List<Port> GetPorts(string eqpID)
        {
            List<Port> ret = new List<Port>();
            foreach (Port entity in _entities.Values)
            {
                if (entity.Data.NODEID == eqpID)
                {
                    ret.Add(entity);
                    //break;
                }
            }
            return ret;
        }

		public Port GetPort(string portID)
		{
			Port ret = null;
            if (_entities.ContainsKey(portID))
                ret = _entities[portID];
			return ret;
		}

        /// <summary>
        /// CELL PIL Line, 
        /// 取得Normal Port ,
        /// 找不到Port時回傳NULL
        /// </summary>
        /// <param name="eqpID">Port所在的EQPID</param>
        /// <returns>Port</returns>
        public Port GetNormalPortByPIL(string eqpNo, string portNo)
        {
            Port ret = _entities.Values.FirstOrDefault(p => p.Data.NODENO == eqpNo &&
                p.Data.PORTATTRIBUTE == keyCELLPORTAtt.NORMAL && p.Data.PORTNO == portNo);
            return ret;
        }

		public List<Port> GetPorts()
		{
			List<Port> ret = new List<Port>();
			foreach (Port entity in _entities.Values)
			{
				ret.Add(entity);
			}
			return ret;
		}

        //CELL DenseBox Port
        /// <summary>
        /// 一定要在DB PORTATTRIBUTE ="DENSE"的Port才會找得到
        /// </summary>
        /// <param name="eqpNO">Equipment No</param>
        /// <param name="portNo">Port No</param>
        /// <returns>PortEntity</returns>
        public Port GetDPPort(string eqpNO,string portNo)
        {
            Port ret = null;
            foreach (Port entity in _entities.Values)
            {
                if (entity.Data.NODENO == eqpNO && entity.Data.PORTNO == portNo)
                {
                    if (entity.Data.PORTATTRIBUTE == keyCELLPORTAtt.DENSE)
                    {
                        ret = entity;
                        break;
                    }
                }
            }
            return ret;
        }

        /// <summary>
        /// 以line去取ports
        /// </summary>
        /// <param name="lineID"></param>
        /// <returns></returns>
        public List<Port> GetPortsByLine(string lineID)
        {
            List<Port> ret = new List<Port>();
            foreach (Port entity in _entities.Values)
            {
                if (entity.Data.LINEID == lineID)
                {
                    ret.Add(entity);
                    //break;
                }
            }
            return ret;
        }


        //Watson Add 2010119 For DPI 一次有個box
        public Port GetPortByDPI(string eqpNO, string portNo)
        {
            Port ret = null;
                switch (portNo)
                {
                    case "01":
                        return GetDPPort(eqpNO, "02");
                    case "02":
                        return GetDPPort(eqpNO, "01");
                    case "03":
                        return GetDPPort(eqpNO, "04");
                    case "04":
                        return GetDPPort(eqpNO, "03");
                    case "05":
                        return GetDPPort(eqpNO, "06");
                    case "06":
                        return GetDPPort(eqpNO, "05");
                }
                return ret;
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
        public IList<string> GetEntityNames()
        {
            IList<string> entityName = new List<string>();
            entityName.Add("PortManager");
            return entityName;
        }

        public System.Data.DataTable GetDataTable(string entityName)
        {
            try
            {
                DataTable dt = new DataTable();
                PortEntityData data = new PortEntityData();
                PortEntityFile file = new PortEntityFile();
                DataTableHelp.DataTableAppendColumn(data, dt);
                DataTableHelp.DataTableAppendColumn(file, dt);

                List<Port> port_entities = GetPorts();
                foreach (Port entity in port_entities)
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
	}
}
