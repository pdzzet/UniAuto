using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace UniOPI
{
    /// <summary> 動態產生的元件類型
    /// 
    /// </summary>
    enum FieldTypes
    {
        /// <summary> 未定義
        /// 
        /// </summary>
        Unknown,
        /// <summary> 日期區間元件
        /// 
        /// </summary>
        DateTime,
        /// <summary> 下拉選單元件
        /// 
        /// </summary>
        ComboBox,
        /// <summary> 文字輸輸框元件
        /// 
        /// </summary>
        TextBox
    }

    /// <summary> FormHistoryBase 參數類別
    /// 
    /// </summary>
    public class HisTableParam
    {
        #region Fields
        private string _tableName = string.Empty;
        private List<string> _lstQueryFields = new List<string>();
        private List<string> _lstDisplayFields = new List<string>();
        private List<string> _lstFieldWidth = new List<string>();
        private string _sortFields = string.Empty;
        private string _dateTimeField = string.Empty;
        private string _cboValueSQL = string.Empty;

        private List<ColumnProp> _lstGridViewColumnInfo = new List<ColumnProp>();
        private List<ParamInfo> _lstConditionParamInfo = new List<ParamInfo>();
        private string _sqlQuery = string.Empty;
        #endregion

        #region Property
        /// <summary> 表單上DataGridView欄位資訊
        /// 
        /// </summary>
        public List<ColumnProp> GridViewColumnInfo
        {
            get { return _lstGridViewColumnInfo; }
        }

        /// <summary> 表單上查詢條件資訊
        /// 
        /// </summary>
        public List<ParamInfo> ConditionParamInfo
        {
            get { return _lstConditionParamInfo; }
        }

        /// <summary> 查詢的資料表名稱
        /// 
        /// </summary>
        public string TableName
        {
            get { return _tableName; }
        }

        /// <summary> 查詢的欄位 (A1,A2,A3,...)
        /// 
        /// </summary>
        public List<string> QueryFields
        {
            get { return _lstQueryFields; }
        }

        /// <summary> 查詢的SQL Statement (SELECT * FROM XXX)
        /// 
        /// </summary>
        public string SqlQuery
        {
            get { return _sqlQuery; }
        }

        /// <summary> 查詢SQL DateTime 條件
        /// 
        /// </summary>
        public string DateTimeField
        {
            get { return _dateTimeField; }
        }

        /// <summary> 排序的SQL Statement (ORDER BY XXX)
        /// 
        /// </summary>
        public string SortExpression
        {
            get { return _sortFields; }
        }
        #endregion

        #region Constructor
        public HisTableParam(string tableName)
        {
            this._tableName = tableName;
            this.Initial();
        }
        #endregion

        #region Private Methods
        private void Initial()
        {
            DataTable dt = this.GetFormParamFromDB();
            if (dt.Rows.Count == 0)
                return;

            DataRow dr = dt.Rows[0];
            _lstQueryFields = (dr["QUERYFIELDS"].ToString().Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ToList<string>();
            _lstDisplayFields = (dr["DISPLAYFIELDS"].ToString().Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ToList<string>();
            _lstFieldWidth = (dr["FIELDWIDTH"].ToString().Split(new Char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)).ToList<string>();
            _sortFields = dr["SORTFIELDS"].ToString();
            _dateTimeField = dr["TIMESTAMPFIELD"].ToString();
            _cboValueSQL = dr["FIELDCOMBOSQL"].ToString();

            #region Check 欄位是否存在entity
            List<string> _lstColName = new List<string>();

            switch (dr["TABLENAME"].ToString())
            {
                case "SBCS_ALARMHISTORY_TRX":
                    _lstColName = typeof(SBCS_ALARMHISTORY_TRX).GetProperties().Select(a => a.Name).ToList();                    
                    break;

                case "SBCS_CASSETTEHISTORY_TRX":
                    _lstColName = typeof(SBCS_CASSETTEHISTORY_TRX).GetProperties().Select(a => a.Name).ToList();
                    break;

                case "SBCS_LINEHISTORY_TRX":
                    _lstColName = typeof(SBCS_LINEHISTORY_TRX).GetProperties().Select(a => a.Name).ToList();
                    break;

                case "SBCS_NODEHISTORY_TRX":
                    _lstColName = typeof(SBCS_NODEHISTORY_TRX).GetProperties().Select(a => a.Name).ToList();
                    break;

                case "SBCS_PORTHISTORY_TRX":
                    _lstColName = typeof(SBCS_PORTHISTORY_TRX).GetProperties().Select(a => a.Name).ToList();
                    break;

                case "SBCS_UNITHISTORY_TRX":
                    _lstColName = typeof(SBCS_UNITHISTORY_TRX).GetProperties().Select(a => a.Name).ToList();                    
                    break;

                case "SBCS_PROCESS_DATA_TRX":
                    _lstColName = typeof(SBCS_PROCESS_DATA_TRX).GetProperties().Select(a => a.Name).ToList();
                    break;

                case "SBCS_JOBHISTORY_TRX":
                    _lstColName = typeof(SBCS_JOBHISTORY_TRX).GetProperties().Select(a => a.Name).ToList();
                    break;

                case "SBCS_CIMMESSAGE_TRX":
                    _lstColName = typeof(SBCS_CIMMESSAGE_TRX).GetProperties().Select(a => a.Name).ToList();
                    break;

                case "SBCS_CHANGEPLANHISTORY_TRX":
                    _lstColName = typeof(SBCS_CHANGEPLANHISTORY_TRX).GetProperties().Select(a => a.Name).ToList();
                    break;

                case "SBCS_ASSEMBLY_TRX":
                    _lstColName = typeof(SBCS_ASSEMBLY_TRX).GetProperties().Select(a => a.Name).ToList();
                    break;

                case "SBCS_MATERIAL_TRX":
                    _lstColName = typeof(SBCS_MATERIAL_TRX).GetProperties().Select(a => a.Name).ToList();
                    break;

                case "SBCS_OPIHISTORY_TRX":
                    _lstColName = typeof(SBCS_OPIHISTORY_TRX).GetProperties().Select(a => a.Name).ToList();
                    break;

                case "SBCS_DEFECTCODEHISTORY_TRX":
                    _lstColName = typeof(SBCS_DEFECTCODEHISTORY_TRX).GetProperties().Select(a => a.Name).ToList();
                    break;

                case "SBCS_RECIPETABLE_TRX":
                    _lstColName = typeof(SBCS_RECIPETABLE_TRX).GetProperties().Select(a => a.Name).ToList();
                    break;

                case "SBCS_PPKEVENTHISTORY_TRX":
                    _lstColName = typeof(SBCS_PPKEVENTHISTORY_TRX).GetProperties().Select(a => a.Name).ToList();
                    break;
            }

            List<int> _lstRemoveIndex = new List<int>();

            for (int i = _lstQueryFields.Count-1; i >= 0 ; i--)
            {
                if (_lstColName.Contains(_lstQueryFields[i]) == false) _lstRemoveIndex.Add(i);
            }

            foreach (int i in _lstRemoveIndex)
            {
                _lstQueryFields.RemoveAt(i);
                _lstDisplayFields.RemoveAt(i);
                _lstFieldWidth.RemoveAt(i);
            }
            #endregion

            this.SetHistField();
            this.SetHistParam(dt);
        }

        private DataTable GetFormParamFromDB()
        {
            UniBCSDataContext ctx = FormMainMDI.G_OPIAp.DBCtx;

            //for  office side 無server name時用
            //var query =
            //        (from hf in ctx.SBRM_HISTABLEFIELDS
            //         join hp in
            //             (from p in ctx.SBRM_HISTABLEPARAM
            //              group p by new { p.TABLENAME, p.FIELDKEY } into g
            //              select new { TABLENAME = g.Key.TABLENAME, FIELDKEY = g.Key.FIELDKEY, Products = g }) on hf.TABLENAME equals hp.TABLENAME into subGrp
            //         from hp in subGrp.DefaultIfEmpty()
            //         where hf.TABLENAME == _tableName
            //         orderby hp.Products.FirstOrDefault().FIELDSEQUENCE
            //         select new
            //         {
            //             hf.TABLENAME,
            //             hf.QUERYFIELDS,
            //             hf.DISPLAYFIELDS,
            //             hf.FIELDWIDTH,
            //             hf.SORTFIELDS,
            //             hp.FIELDKEY,
            //             hp.Products.FirstOrDefault().FIELDCAPTION,
            //             hp.Products.FirstOrDefault().FIELDTYPE,
            //             hp.Products.FirstOrDefault().FIELDDISPLAY,
            //             hp.Products.FirstOrDefault().FIELDVALUE,
            //             hp.Products.FirstOrDefault().FIELDDEFAULT,
            //             hp.Products.FirstOrDefault().FIELDSEQUENCE,
            //             hf.TIMESTAMPFIELD,
            //             hp.Products.FirstOrDefault().FIELDCOMBOSQL,
            //             hp.Products.FirstOrDefault().FIELDATTRIBUTE,
            //             hp.Products.FirstOrDefault().FIELDCOMBORELATION,
            //             hp.Products.FirstOrDefault().ITEMCONDITION
            //         }).Distinct();

            //var query =
            //        (from hf in ctx.SBRM_HISTABLEFIELDS
            //         join hp in ctx.SBRM_HISTABLEPARAM on hf.TABLENAME equals hp.TABLENAME into subGrp
            //         from hp in subGrp.DefaultIfEmpty()
            //         where hf.TABLENAME == _tableName && hf.SERVERNAME == hp.SERVERNAME && hf.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName
            //         orderby hp.FIELDSEQUENCE
            //         select new
            //         {
            //             hf.TABLENAME,
            //             hf.QUERYFIELDS,
            //             hf.DISPLAYFIELDS,
            //             hf.FIELDWIDTH,
            //             hf.SORTFIELDS,
            //             hp.FIELDKEY,
            //             hp.FIELDCAPTION,
            //             hp.FIELDTYPE,
            //             hp.FIELDDISPLAY,
            //             hp.FIELDVALUE,
            //             hp.FIELDDEFAULT,
            //             hp.FIELDSEQUENCE,
            //             hf.TIMESTAMPFIELD,
            //             hp.FIELDCOMBOSQL,
            //             hp.FIELDATTRIBUTE,
            //             hp.FIELDCOMBORELATION,
            //             hp.ITEMCONDITION
            //         });

            var query =
                    (from hf in ctx.SBRM_HISTABLEFIELDS
                     join hp in ctx.SBRM_HISTABLEPARAM on hf.TABLENAME equals hp.TABLENAME into subGrp
                     from hp in subGrp.DefaultIfEmpty()
                     where hf.TABLENAME == _tableName && hf.HISTORYTYPE == hp.HISTORYTYPE && hf.HISTORYTYPE == FormMainMDI.G_OPIAp.CurLine.HistoryType
                     orderby hp.FIELDSEQUENCE
                     select new
                     {
                         hf.TABLENAME,
                         hf.QUERYFIELDS,
                         hf.DISPLAYFIELDS,
                         hf.FIELDWIDTH,
                         hf.SORTFIELDS,
                         hp.FIELDKEY,
                         hp.FIELDCAPTION,
                         hp.FIELDTYPE,
                         hp.FIELDDISPLAY,
                         hp.FIELDVALUE,
                         hp.FIELDDEFAULT,
                         hp.FIELDSEQUENCE,
                         hf.TIMESTAMPFIELD,
                         hp.FIELDCOMBOSQL,
                         hp.FIELDATTRIBUTE,
                         hp.FIELDCOMBORELATION,
                         hp.ITEMCONDITION
                     });

            return DBConnect.ToDataTable(query);
        }

        private void SetHistField()
        {
            _lstGridViewColumnInfo.Clear();

            StringBuilder sbSql = new StringBuilder("SELECT * ");
            ColumnProp cp = null;
            for(int idx = 0; idx < _lstDisplayFields.Count; idx++)
            {
                cp = new ColumnProp()
                {
                    DataField = _lstQueryFields[idx],
                    DisplayField = _lstDisplayFields[idx],
                    FieldWidth = _lstFieldWidth[idx],
                    FieldFormat = _lstQueryFields[idx].EndsWith("DATETIME") ? "yyyy/MM/dd HH:mm:ss.fff" : ""    // 欄位名稱是DATETIME結尾的統一格式為「yyyy/MM/dd HH:mm:ss」
                    //FieldFormat = _lstQueryFields[idx].EndsWith("DATETIME") ? "yyyy/MM/dd tt hh:mm:ss" : ""
                };
                _lstGridViewColumnInfo.Add(cp);
            }
            sbSql.AppendFormat("FROM {0}", _tableName);

            _sqlQuery = sbSql.ToString();
        }



        private void SetHistParam(DataTable dt)
        {
            try
            {
                _lstConditionParamInfo.Clear();

                ParamInfo pi = null;
                DataRow dr = null;
                for (int idx = 0; idx < dt.Rows.Count; idx++)
                {
                    dr = dt.Rows[idx];
                    if ("".Equals(dr["FIELDKEY"].ToString()))
                        continue;

                    if (_lstQueryFields.Contains(dr["FIELDKEY"].ToString()) == false)
                        continue ;

                    pi = new ParamInfo()
                    {
                        FieldKey = dr["FIELDKEY"].ToString(),
                        FieldCaption = dr["FIELDCAPTION"].ToString(),
                        FieldType = dr["FIELDTYPE"].ToString(),
                        FieldDisplay = dr["FIELDDISPLAY"].ToString(),
                        FieldValue = dr["FIELDVALUE"].ToString(),
                        FieldDefault = dr["FIELDDEFAULT"].ToString(),
                        FieldSequence = dr["FIELDSEQUENCE"].ToString(),
                        FieldCboValueSQL = dr["FIELDCOMBOSQL"].ToString(),
                        FiledAttribute = dr["FIELDATTRIBUTE"].ToString(),
                        RelationField = dr["FIELDCOMBORELATION"].ToString(),
                        ItemCondition = dr["ITEMCONDITION"].ToString()
                    };


                    #region 查詢comboBox item 內容
                    if (pi.FieldType == "ComboBox" && pi.FieldDisplay == string.Empty && pi.FieldCboValueSQL != string.Empty)
                    {
                        UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBCtx;
                        
                        string _data = string.Empty ;
                        if (pi.ItemCondition == "SERVERNAME")
                        {
                            _data = string.Format(pi.FieldCboValueSQL, FormMainMDI.G_OPIAp.CurLine.ServerName);
                        }
                        else if (pi.ItemCondition == "DATETIME")
                        {                            
                            //改至觸發時動態取得 FormHistory.cs -> dtpDateTime_ValueChanged
                            //string _start = string.Format("{0}:00:00", DateTime.Now.AddHours(-1).ToString("yyyy-MM-dd HH"));
                            //string _end = string.Format("{0}:59:59", DateTime.Now.ToString("yyyy-MM-dd HH"));
                            //_data = string.Format(pi.FieldCboValueSQL, _start, _end);
                        }
                        else
                        {
                            _data = pi.FieldCboValueSQL;
                        }

                        if (_data != string.Empty)
                        {
                            string _display = string.Empty;
                            string _value = string.Empty;

                            //判斷FIELDCOMBOSQL欄位為SQL or 物件名稱 --SQL 則執行SQL取得comboboxitem ; 物件名稱則取程式內對應物件資料(Node,Port,Unit)
                            switch (_data)
                            {
                                case "Node":

                                    #region Node
                                    foreach (Node _node in FormMainMDI.G_OPIAp.Dic_Node.Values)
                                    {
                                        _display = _display + (_display == string.Empty ? "" : ",") + string.Format("{0}-{1}-{2}", _node.NodeNo, _node.NodeID, _node.NodeName);
                                        _value = _value + (_value == string.Empty ? "" : ",") + _node.NodeID;
                                    }
                                    pi.FieldDisplay = _display;
                                    pi.FieldValue = _value;
                                    break;
                                    #endregion

                                case "Port":

                                    #region Port
                                    foreach (Port _port in FormMainMDI.G_OPIAp.Dic_Port.Values)
                                    {
                                        _display = _display + (_display == string.Empty ? "" : ",") + string.Format("{0}-{1}-{2}", _port.NodeNo, _port.PortNo, _port.PortID);
                                        _value = _value + (_value == string.Empty ? "" : ",") + _port.PortID;
                                    }
                                    pi.FieldDisplay = _display;
                                    pi.FieldValue = _value;
                                    break;
                                    #endregion

                                case "Unit":

                                    #region Unit
                                    foreach (Unit _unit in FormMainMDI.G_OPIAp.Dic_Unit.Values)
                                    {
                                        _display = _display + (_display == string.Empty ? "" : ",") + string.Format("{0}-{1}-{2}", _unit.NodeNo, _unit.UnitNo.PadLeft(2, '0'), _unit.UnitID);
                                        _value = _value + (_value == string.Empty ? "" : ",") + _unit.UnitID;
                                    }
                                    pi.FieldDisplay = _display;
                                    pi.FieldValue = _value;
                                    break;
                                    #endregion

                                default: 

                                    #region SQL
                                    if (pi.FiledAttribute == "INT")
                                    {
                                        List<int> _lst = _ctx.ExecuteQuery<int>(_data).ToList();

                                        pi.FieldDisplay = string.Join(",", _lst.ToArray());
                                        pi.FieldValue = string.Join(",", _lst.ToArray());
                                    }
                                    else
                                    {
                                        List<string> _lst = _ctx.ExecuteQuery<string>(_data).ToList();

                                        pi.FieldDisplay = string.Join(",", _lst.ToArray());
                                        pi.FieldValue = string.Join(",", _lst.ToArray());
                                    }
                                    
                                    break;
                                    #endregion
                            }

                            //if (pi.FiledAttribute == "INT")
                            //{
                            //    List<int> _lst = _ctx.ExecuteQuery<int>(_data).ToList();

                            //    pi.FieldDisplay = string.Join(",", _lst.ToArray());
                            //    pi.FieldValue = string.Join(",", _lst.ToArray());
                            //}
                            //else
                            //{
                            //    List<string> _lst = _ctx.ExecuteQuery<string>(_data).ToList();

                            //    pi.FieldDisplay = string.Join(",", _lst.ToArray());
                            //    pi.FieldValue = string.Join(",", _lst.ToArray());
                            //}
                        }
                    }
                    #endregion

                    _lstConditionParamInfo.Add(pi);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        public DataTable ToDataTable(System.Data.Linq.DataContext ctx, object query)
        {
            if (query == null)
            {
                throw new ArgumentNullException("query");
            }
            IDbCommand cmd = ctx.GetCommand((IQueryable)query);
            System.Data.SqlClient.SqlDataAdapter adapter = new System.Data.SqlClient.SqlDataAdapter();
            adapter.SelectCommand = (System.Data.SqlClient.SqlCommand)cmd;
            DataTable dt = new DataTable("dataTbl");
            try
            {
                cmd.Connection.Open();
                adapter.FillSchema(dt, SchemaType.Source);
                adapter.Fill(dt);
            }
            finally
            {
                cmd.Connection.Close();
            }
            return dt;
        }

        //public DataTable ToDataTable(IEnumerable query)
        //{
        //    DataTable dt = new DataTable();
        //    foreach (object obj in query)
        //    {
        //        Type t = obj.GetType();
        //        PropertyInfo[] pis = t.GetProperties();
        //        if (dt.Columns.Count == 0)
        //        {
        //            foreach (PropertyInfo pi in pis)
        //            {
        //                dt.Columns.Add(pi.Name);
        //            }
        //        }
        //        DataRow dr = dt.NewRow();
        //        foreach (PropertyInfo pi in pis)
        //        {
        //            object value = pi.GetValue(obj, null);
        //            dr[pi.Name] = value;
        //        }
        //        dt.Rows.Add(dr);
        //    }
        //    return dt;
        //}
        #endregion
    }

    public class ColumnProp
    {
        public string DataField { get; set; }
        public string DisplayField { get; set; }
        public string FieldWidth { get; set; }
        public string FieldFormat { get; set; }
    }

    public class ParamInfo
    {
        public string FieldKey { get; set; }
        public string FieldCaption { get; set; }
        public string FieldType { get; set; }
        public string FieldDisplay { get; set; }
        public string FieldValue {get; set; }
        public string FieldDefault { get; set; }
        public string FieldSequence { get; set; }
        public string FieldDateTime { get; set; }
        public string FieldCboValueSQL { get; set; }
        public string FiledAttribute { get; set; }    
        public string ItemCondition { get; set; }
        public string RelationField { get; set; }
    }
}
