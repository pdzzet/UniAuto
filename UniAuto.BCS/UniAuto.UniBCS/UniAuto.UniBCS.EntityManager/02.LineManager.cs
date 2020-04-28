/************************************************************
 * 1.0 修改SkipAgent DB中会直接Key OEE，MES，EDA等，程序中需要补充"Agent"  20150126  Tom
 * 
 * 
 * 
 * 
 ***************************************************************/

using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using UniAuto.UniBCS.DB;
using UniAuto.UniBCS.Entity;
using UniAuto.UniBCS.Log;
using System.Collections;

namespace UniAuto.UniBCS.EntityManager
{
    public class LineManager : EntityManager, IDataSource
	{
		private Dictionary<string, Line> _entities = new Dictionary<string, Line>();

        private Dictionary<string, List<LineStatusSpec>> _LineStatusSpecs = new Dictionary<string, List<LineStatusSpec>>();

        private Dictionary<string,List<SkipReport>> _skipReports = new Dictionary<string,List<SkipReport>>();

        public override EntityManager.FILE_TYPE GetFileType()
        {
            return FILE_TYPE.BIN;
            //return FILE_TYPE.JSON;
        }

        protected override string GetSelectHQL()
        {
            return string.Format("from LineEntityData where SERVERNAME = '{0}'", BcServerName);
        }

        protected override Type GetTypeOfEntityData()
        {
            return typeof(LineEntityData);
        }

        protected override void AfterSelectDB(List<EntityData> EntityDatas, string FilePath, out List<string> Filenames)
        {
            Filenames = new List<string>();
            foreach (EntityData entity_data in EntityDatas)
            {
                LineEntityData line_entity_data = entity_data as LineEntityData;
                if (line_entity_data != null)
                {
                    //string file_name = string.Format("{0}.bin", line_entity_data.LINEID);
                    string file_name = string.Format("{0}.{1}", line_entity_data.LINEID,GetFileExtension());
                    Filenames.Add(file_name);
                }
            }
        }

        protected override Type GetTypeOfEntityFile()
        {
            return typeof(LineEntityFile);
        }

        protected override EntityFile NewEntityFile(string Filename)
        {
            return new LineEntityFile();
        }

        protected override void AfterInit(List<EntityData> entityDatas, List<EntityFile> entityFiles)
        {
            foreach (EntityData entity_data in entityDatas)
            {
                LineEntityData line_entity_data = entity_data as LineEntityData;
                if (line_entity_data != null)
                {
                    foreach (EntityFile entity_file in entityFiles)
                    {
                        LineEntityFile line_entity_file = entity_file as LineEntityFile;
                        if (line_entity_file != null)
                        {
                            string fextname = line_entity_file.GetFilename();
                            string fname = Path.GetFileNameWithoutExtension(fextname);
                            if (string.Compare(line_entity_data.LINEID, fname, true) == 0)
                                _entities.Add(line_entity_data.LINEID, new Line(line_entity_data, line_entity_file));
                        }
                    }
                }
            }
            ReloadLineStatusSpec();
            ReloadSkipReport();
        }

        /// <summary>
        /// Reload CheckCrossRecipe Enable/Disable
        /// </summary>
        public void ReloadCheckCrossRecipe()
        {
            string hql = string.Format("from LineEntityData where SERVERNAME = '{0}'", BcServerName);
            IList entiyDatas = HibernateAdapter.GetObjectByQuery(hql);

            if (entiyDatas != null)
            {
                foreach (LineEntityData line in entiyDatas)
                {
                    Line Line = GetLine(line.LINEID);
                    if ((Line != null) && (Line.Data.CHECKCROSSRECIPE != line.CHECKCROSSRECIPE))
                    {
                        lock (Line)
                        {
                            Line.Data.CHECKCROSSRECIPE = line.CHECKCROSSRECIPE;
                        }

                    }

                }
            }
        }

        public void ReloadLineStatusSpec()
        {
            Dictionary<string, List<LineStatusSpec>> dic = LoadLineStatusSpec();
            if (dic != null && dic.Count > 0)
            {
                lock (_LineStatusSpecs)
                {
                    _LineStatusSpecs = dic;
                }
            }
        }

        /// <summary>
        /// Reload Skip Report
        /// </summary>
        public void ReloadSkipReport()
        {
            Dictionary<string, List<SkipReport>> dic = LoadSkipReport();
            if (dic != null && dic.Count > 0)
            {
                lock (_skipReports)
                {
                    _skipReports = dic;
                }
            }
        }

        /// <summary>
        /// Load Line Status Spec
        /// </summary>
        protected Dictionary<string,List<LineStatusSpec>> LoadLineStatusSpec()
        {
            Dictionary<string, List<LineStatusSpec>> lineStatusSpecs = new Dictionary<string, List<LineStatusSpec>>();
            foreach (Line line in _entities.Values)
            {
                string hql = string.Format("from LineStatusSpec where LINETYPE='{0}'  ORDER BY  CONDITIONSEQNO", line.Data.LINETYPE);
                IList list=HibernateAdapter.GetObjectByQuery(hql);
                List<LineStatusSpec> lineStatusSpecsList = null;
                if(list!=null)
                {
                    lineStatusSpecsList = new List<LineStatusSpec>();

                    foreach(LineStatusSpec spec in list)
                    {
                        lineStatusSpecsList.Add(spec);
                    }
                }
                lock (lineStatusSpecs)
                {
                    if (lineStatusSpecs.ContainsKey(line.Data.LINEID))
                    {
                        lineStatusSpecs[line.Data.LINEID] = lineStatusSpecsList;
                    }
                    else
                    {
                        lineStatusSpecs.Add(line.Data.LINEID, lineStatusSpecsList);
                    }
                }

            }
            return lineStatusSpecs;
        }

        /// <summary>
        /// Load Skip Report Flag
        /// </summary>
        protected Dictionary<string, List<SkipReport>> LoadSkipReport()
        {
            Dictionary<string, List<SkipReport>> skipReports = new Dictionary<string, List<SkipReport>>();
            foreach (Line line in _entities.Values)
            {
                string hql = string.Format("from SkipReport where LINEID='{0}'  ORDER BY  NODENO", line.Data.LINEID);
                IList list = HibernateAdapter.GetObjectByQuery(hql);
                List<SkipReport> lineSkipReports = new List<SkipReport>();
                if (list != null)
                {
                    foreach (SkipReport spec in list)
                    {
                        lineSkipReports.Add(spec);
                    }
                }
                lock (skipReports)
                {
                    if (skipReports.ContainsKey(line.Data.LINEID))
                    {
                        skipReports[line.Data.LINEID] = lineSkipReports;
                    }
                    else
                    {
                        skipReports.Add(line.Data.LINEID, lineSkipReports);
                    }
                }
            }
            return skipReports;
        }

        /// <summary>
        /// UPK Line By Line ID Get Other Line ID
        /// </summary>
        /// <param name="LineID"></param>
        /// <returns></returns>
        public string GetLineID(string lineID)
        {
            string LineID = string.Empty;

            Line line = ObjectManager.LineManager.GetLine(lineID);
            if (line == null)
            {
                LineID = lineID;
                return LineID;
            }

            if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1 ||
                line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE2)
            {
                if (lineID != "")
                {
                    switch (line.File.UPKEquipmentRunMode)
                    {
                        case eUPKEquipmentRunMode.TFT:
                            LineID = "T" + line.Data.LINEID.Substring(1);
                            break;
                        case eUPKEquipmentRunMode.CF:
                            LineID = line.Data.LINEID;
                            break;
                        default:
                            LineID = line.Data.LINEID;
                            break;
                    }
                }
                else
                {
                    LineID = line.Data.LINEID;
                }
            }
            else
            {
                LineID = line.Data.LINEID;
            }

            return LineID;
        }

		public Line GetLine(string lineID)
		{
            string _lineID = string.Empty;
			Line ret = null;

            foreach (Line line in ObjectManager.LineManager.GetLines())
            {
                if (line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE1 ||
                    line.Data.LINETYPE == eLineType.CF.FCUPK_TYPE2)
                {
                    _lineID = "F" + lineID.Substring(1);
                    if (_entities.ContainsKey(_lineID))
                        ret = _entities[_lineID];
                    return ret;
                }
            }

            if (_entities.ContainsKey(lineID))
                ret = _entities[lineID];
			return ret;
		}

		public List<Line> GetLines()
		{
			List<Line> ret = new List<Line>();
            foreach (Line entity in _entities.Values)
			{
				ret.Add(entity);
			}
			return ret;
		}

        public System.Data.DataTable GetDataTable(string entityName)
        {
            switch (entityName)
            {
                case "LineManager":
                    return GetLineDataTable();
                case "LineStatusSpec":
                    return GetLineStatusSpecDataTable();
                case "SkipReport":
                    return GetSkipReportDataTable();
                default:
                    return null;

            }
            
        }

        private DataTable GetLineDataTable()
        {
            try
            {
                DataTable dt = new DataTable();
                LineEntityData data = new LineEntityData();
                LineEntityFile file = new LineEntityFile();
                DataTableHelp.DataTableAppendColumn(data, dt);
                DataTableHelp.DataTableAppendColumn(file, dt);

                List<Line> line_entities = GetLines();
                foreach (Line entity in line_entities)
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

        private DataTable GetLineStatusSpecDataTable()
        {
            try
            {
                DataTable dt = new DataTable();
                LineStatusSpec data = new LineStatusSpec();
              
                DataTableHelp.DataTableAppendColumn(data, dt);

                foreach (string key in _LineStatusSpecs.Keys)
                {
                    List<LineStatusSpec> specs= _LineStatusSpecs[key];
                    foreach (LineStatusSpec spec in specs)
                    {
                         DataRow dr = dt.NewRow();
                        DataTableHelp.DataRowAssignValue(spec, dr);
                        
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

        private DataTable GetSkipReportDataTable()
        {
            try
            {
                DataTable dt = new DataTable();
                SkipReport data = new SkipReport();

                DataTableHelp.DataTableAppendColumn(data, dt);

                foreach (string key in _skipReports.Keys)
                {
                    List<SkipReport> specs = _skipReports[key];
                    foreach (SkipReport spec in specs)
                    {
                        DataRow dr = dt.NewRow();
                        DataTableHelp.DataRowAssignValue(spec, dr);

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

        public IList<string> GetEntityNames()
        {
            IList<string> entitys=new List<string>();
            entitys.Add("LineManager");
            entitys.Add("LineStatusSpec");
            entitys.Add("SkipReport");
            return entitys;

        }


        #region Line State
        /// <summary>
        /// Get Line State
        /// </summary>
        /// <param name="lineId">line ID</param>
        /// <returns>Line State</returns>
        public string GetLineState(string lineId)
        {
            try
            {
                List<Equipment> equipmentList = ObjectManager.EquipmentManager.GetEQPsByLine(lineId);
                if (equipmentList == null || equipmentList.Count == 0)
                {
                    NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", "Can't found Equipment list");
                    return "";     
                }
                foreach (Equipment equipment in equipmentList)
                {
                    if (equipment.File.CIMMode==eBitResult.OFF)
                    {
                        if (equipment.Data.NODEATTRIBUTE.Equals("LD") || equipment.Data.NODEATTRIBUTE.Equals("UD") || equipment.Data.NODEATTRIBUTE.Equals("LU"))
                        {
                            NLogManager.Logger.LogInfoWrite(LoggerName,this.GetType().Name,MethodBase.GetCurrentMethod().Name+"()",
                                string.Format("Loader or Unloader Equipment[{0}] CIM mode is OFF.", equipment.Data.NODENO));

                            return eLINE_STATUS.DOWN;
                        }
                    }
                }
                for (int i = 1; i <= 4; i++)    // modify by bruce 新增monitor EQ Alive Down
                {
                    string conditionState = this.GetLineStatusToString(i);

                    if (!_LineStatusSpecs.ContainsKey(lineId))
                    {
                        if (conditionState != eLINE_STATUS.EQALIVEDOWN)//sy add EQALIVEDOWN 特別不記Error CELL CF 沒有用到20160623
                            NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                       string.Format("Can't found Line Status Spec list.[{0}]", conditionState));
                        return "";
                    }
                    List<LineStatusSpec> lineConditionList=_LineStatusSpecs[lineId].Where(n=> n.CONDITIONSTATUS.ToUpper().Equals(conditionState)).ToList<LineStatusSpec>();

                    if (lineConditionList == null || lineConditionList.Count == 0)
                    {
                        if (conditionState != eLINE_STATUS.EQALIVEDOWN)//sy add EQALIVEDOWN 特別不記Error CELL CF 沒有用到20160623
                            NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                                                        string.Format("Can't found Line Status Spec list.[{0}]", conditionState));
                    }
                    else
                    {
                       
                        foreach (LineStatusSpec lineStatusSpec in lineConditionList)
                        {
                            bool result = this.CheckEqpStatus(equipmentList, lineStatusSpec.EQPNOLIST, conditionState);

                            if (result)
                            {
                                if (conditionState == "EQALIVEDOWN") conditionState = "DOWN";  // add by bruce 20160331 如果出現EQ Alive Down 就改掛Down
                                return conditionState;
                            }
                        }
                    }
                }
                NLogManager.Logger.LogWarnWrite(LoggerName, this.GetType().Name, System.Reflection.MethodBase.GetCurrentMethod().Name + "()", "Can't found matching 'EQP status' in LINESTATUSSPEC.");

            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod() + "()",ex);
                return "";
            }
            return "";
        }

        private string GetLineStatusToString(int index)
        {
            string condition = "";

            switch (index)
            {
                case 1:
                    condition = eLINE_STATUS.DOWN;
                    break;
                case 2:
                    condition = eLINE_STATUS.EQALIVEDOWN;   // add by bruce 20160331 CSOT 要求新增設定 monitor EQ Alive Down
                    break;
                case 3:
                    condition = eLINE_STATUS.RUN;
                    break;
                case 4:
                    condition = eLINE_STATUS.IDLE;
                    break;
            }

            return condition;
        }

        private bool CheckEqpStatus(IList<Equipment> equipmentList, string ConditionEqpList, string conditionState)
        {
            string[] conditionEqpList = ConditionEqpList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if (conditionEqpList == null || conditionEqpList.Length == 0)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()",
                            string.Format("Can't found Line Status Spec Condition Eqp list.[{0}]", conditionState));

                return false;
            }

            try
            {
                bool allEQPCIMModeOff = true;

                foreach (string conditionLocalNo in conditionEqpList)
                {
                    foreach (Equipment equipment in equipmentList)
                    {
                        if (equipment.Data.NODENO.ToUpper().Equals(conditionLocalNo.ToUpper()))
                        {
                            if (equipment.File.CIMMode==eBitResult.OFF)
                            {
                                if (conditionEqpList.Length == 1)
                                {
                                    if (equipment.Data.REPORTMODE != "HSMS_PLC" && equipment.Data.REPORTMODE != "HSMS_NIKON")
                                    return false;
                                }
                                else
                                {
                                    continue;
                                }
                            }

                            allEQPCIMModeOff = false;

                            eEQPStatus eqpStatus = equipment.File.Status;

                            string lineStatus = ConvertEQPStatus(eqpStatus);

                            if (conditionState.ToUpper() == "EQALIVEDOWN")
                            {
                                if (equipment.Data.REPORTMODE == "HSMS_PLC" || equipment.Data.REPORTMODE == "HSMS_NIKON")   // add by bruce 20160331 根據 EQ report mode Type 處理
                                {
                                    //if (equipment.SecsCommunicated  == false)
                                    if (equipment.HsmsConnStatus == "DISCONNECTED")
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                                else
                                {
                                    if (equipment.File.AliveTimeout == true)    // modify by bruce 20160324 只要機台出現EQ Alive Time out, line state 就上報Down
                                    {
                                        continue;
                                    }
                                    else
                                    {
                                        return false;
                                    }
                                }
                            }
                            else
                            {
                                if (lineStatus.ToUpper() == conditionState.ToUpper())   
                                {
                                    continue;
                                }
                                else
                                {
                                    return false;
                                }
                            }
                        }
                    }
                }

                if (allEQPCIMModeOff)
                {
                    NLogManager.Logger.LogInfoWrite(LoggerName,this.GetType().Name,MethodBase.GetCurrentMethod().Name+"()",
                                        string.Format("All EQP CIM Mode is OFF.[{0}]", conditionState));
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName,this.GetType().Name, MethodBase.GetCurrentMethod().Name + "()", ex);
            }

            return false;
        }

        private string ConvertEQPStatus(eEQPStatus eqpStatus)
        {
            string returnValue = "";

            switch (eqpStatus)
            {
                case eEQPStatus.IDLE:
                    returnValue = eLINE_STATUS.IDLE;
                    break;

                case eEQPStatus.RUN:
                    returnValue = eLINE_STATUS.RUN;
                    break;

                case eEQPStatus.SETUP:
                case eEQPStatus.STOP:
                case eEQPStatus.PAUSE:
                case eEQPStatus.NOUNIT:
                    returnValue = eLINE_STATUS.DOWN;
                    break;

                default: break;
            }

            return returnValue;
        }

        #endregion

        /// <summary>
        /// Check Need To Report MES or OEE 
        /// False Need to Report
        /// True don’t Report 
        /// </summary>
        /// <param name="agentName">OEE,MES,EDA</param>
        /// <returns></returns>
        public bool CheckSkipReportByID(string lineName,string machineName, string unitID,string reportName, string agentName)
        {
            try
            {
                if (_skipReports.ContainsKey(lineName))
                {
                    List<SkipReport> _skiplist = _skipReports[lineName];
                    
                    foreach (SkipReport sr in _skiplist)
                    {
                        if (sr.NODEID == machineName && sr.UNITID == unitID && sr.SKIPREPORTTRX == reportName.ToUpper() && sr.SKIPAGENT+"Agent" == agentName)
                        {
                            return true;
                        }
                    }

                }
                return false;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }
            return false;
        }


        /// <summary>
        /// Check Need To Report MES or OEE 
        /// False Need to Report
        /// True don’t Report 
        /// </summary>
        /// <param name="agentName">OEE,MES,EDA</param>
        /// <returns></returns>
        public bool CheckSkipReportByNo(string lineName, string equipmentNo, string unitNo, string reportName, string agentName)
        {
            try
            {
                if (_skipReports.ContainsKey(lineName))
                {
                    List<SkipReport> _skiplist = _skipReports[lineName];

                    foreach (SkipReport sr in _skiplist)
                    {
                        if (sr.NODENO == equipmentNo && sr.UNITNO == unitNo && sr.SKIPREPORTTRX == reportName.ToUpper() && sr.SKIPAGENT+"Agent" == agentName)
                        {
                            return true;
                        }
                    }

                }
                return false;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }
            return false;
        }

        /// <summary>
        ///Check Alarm Code Need To Report MES or OEE 
        /// False Need to Report
        /// True don’t Report
        /// </summary>
        /// <param name="lineName"></param>
        /// <param name="machineName"></param>
        /// <param name="unitID"></param>
        /// <param name="alarmCode"></param>
        /// <param name="agentName"></param>
        /// <returns></returns>
        public bool CheckSkipReportAlarmByID(string lineName, string machineName, string unitID, string alarmCode, string agentName)
        {
            try
            {
                if (_skipReports.ContainsKey(lineName))
                {
                    List<SkipReport> _skiplist = _skipReports[lineName];

                    foreach (SkipReport sr in _skiplist)
                    {
                        if (sr.NODEID == machineName && sr.UNITID == unitID && sr.SKIPCONDITION == alarmCode && sr.SKIPAGENT+"Agent" == agentName)
                        {
                            return true;
                        }
                    }

                }
                return false;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }
            return false;
        }

        /// <summary>
        /// Check Alarm Code Need To Report MES or OEE 
        /// False Need to Report
        /// True don’t Report
        /// </summary>
        /// <param name="lineName"></param>
        /// <param name="equipmentNo"></param>
        /// <param name="unitNo"></param>
        /// <param name="alarmCode"></param>
        /// <param name="agentName"></param>
        /// <returns></returns>
        public bool CheckSkipReportAlarmByNo(string lineName, string equipmentNo, string unitNo, string alarmCode, string agentName)
        {
            try
            {
                if (_skipReports.ContainsKey(lineName))
                {
                    List<SkipReport> _skiplist = _skipReports[lineName];

                    foreach (SkipReport sr in _skiplist)
                    {
                        if (sr.NODENO == equipmentNo && sr.UNITNO == unitNo && sr.SKIPCONDITION == alarmCode && sr.SKIPAGENT+"Agent" == agentName)
                        {
                            return true;
                        }
                    }

                }
                return false;
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }
            return false;
        }

        public void RecordLineHistory(string trxID, Line line)
        {
            try
            {
                LINEHISTORY history = new LINEHISTORY();
                history.LINEID = line.Data.LINEID;
                history.LINESTATUS = line.File.Status;
                history.LINETYPE = line.Data.LINETYPE;
                history.FABTYPE = line.Data.FABTYPE;
                history.MESCONNECTIONSTATE = line.File.MesConnectState;
                if (line.File.DummyRequestFlag && line.File.HostMode == eHostMode.LOCAL)
                    history.ONLINECONTROLSTATE = "LOCAL_DUMMYREQUEST";
                else
                    history.ONLINECONTROLSTATE = line.File.HostMode.ToString();
                history.LINEOPERATIONMODE = line.File.LineOperMode;
                history.EQPCOUNT = ObjectManager.EquipmentManager.GetEQPsByLine(line.Data.LINEID).Count();
                history.CSTOPERATIONMODE = ObjectManager.EquipmentManager.GetEQP("L2").File.CSTOperationMode.ToString();
                history.UPDATETIME = DateTime.Now;
                history.TRANSACTIONID = trxID;
                history.INDEXEROPERATIONMODE = line.File.IndexOperMode.ToString();
                history.SHORTCUTMODE = line.File.CFShortCutMode.ToString();

                HibernateAdapter.SaveObject(history);
            }
            catch (System.Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);

            }
        }

        //public string AnalysisLineRunMode(string lineID)
        //{
        //    try
        //    {

        //    }
        //    catch (System.Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(LoggerName, this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
        //        return "0";
        //    }
        //}
    }
}
