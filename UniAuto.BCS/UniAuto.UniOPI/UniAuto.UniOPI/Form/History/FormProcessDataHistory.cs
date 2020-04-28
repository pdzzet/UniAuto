using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.Timers;
using System.Threading;

namespace UniOPI
{
    public partial class FormProcessDataHistory : FormBase
    {
        #region Fields
        public const int DEFAULT_COLUMN_WIDTH = 100;
        private HisTableParam _frmParam = null;
        private List<string> VisibleFields= new List<string> { "FILENAMA", "TRXID" };
        #endregion

        public Dictionary<string, ucComBox_His> timerReloadSqls = new Dictionary<string, ucComBox_His>();//add by huangjiayin 20171211
        bool isExcutingTask = false;

        string MsgCaption = "Process Data History";

        public FormProcessDataHistory(HisTableParam histParam)
        {
            InitializeComponent();

            this._frmParam = histParam;

            this.GenerateConditionUserCtl();
            this.GenerateDataGridViewCol();
        }

        private void FormProcessDataHistory_Load(object sender, EventArgs e)
        {
            try
            {
                rdoHour.Checked = true;
                Control.CheckForIllegalCrossThreadCalls = false;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        #region Events
        private void btnQuery_Click(object sender, EventArgs e)
        {
            try
            {
                lock (timerReloadSqls)
                {
                    timerReloadSqls.Clear();
                }
                #region 組查詢語法
                string _sql = string.Format("SELECT TOP {0} OBJECTKEY, {1} FROM {2}", FormMainMDI.G_OPIAp.QueryMaxCount, String.Join(",", _frmParam.QueryFields.ToArray()), _frmParam.TableName);
                string _sqlCnt = string.Format("SELECT COUNT(*) FROM {0}", _frmParam.TableName);
                StringBuilder sbSql = new StringBuilder(_sql);
                StringBuilder sbSqlCnt = new StringBuilder(_sqlCnt);
                bool bolFirstCondition = true;

                #region 組日期條件
                string result = string.Format(" WHERE {0} BETWEEN '{1}' AND '{2}'",
                    grbDateTime.Tag.ToString(),
                    string.Format("{0}:00:00", dtpStartDate.Value.ToString("yyyy-MM-dd HH")),
                    string.Format("{0}:59:59", dtpEndDate.Value.ToString("yyyy-MM-dd HH")));

                string resultCnt = string.Format(" WHERE {0} BETWEEN '{1}' AND '{2}'",
                    grbDateTime.Tag.ToString(),
                    string.Format("{0}:00:00", dtpStartDate.Value.ToString("yyyy-MM-dd HH")),
                    string.Format("{0}:59:59", dtpEndDate.Value.ToString("yyyy-MM-dd HH")));

                sbSql.Append(result);
                sbSqlCnt.Append(resultCnt);
                bolFirstCondition = false;
                #endregion

                foreach (iHistBase uc in flpnlCondition.Controls)
                {
                    sbSql.Append(uc.GetCondition(ref bolFirstCondition));
                    sbSqlCnt.Append(uc.GetCondition(ref bolFirstCondition));
                }

                sbSql.Append(string.Format(" ORDER BY {0}", _frmParam.SortExpression));
                #endregion

                UniBCSDataContext ctx = FormMainMDI.G_OPIAp.DBCtx;

                #region 檢查搜尋筆數不得大於最大值

                IEnumerable<int> _queryCnt = ctx.ExecuteQuery<int>(sbSqlCnt.ToString());
                int _resultCnt = _queryCnt.FirstOrDefault();
                if (_resultCnt > FormMainMDI.G_OPIAp.QueryMaxCount)
                {
                    ShowMessage(this, "Query Reuslt", "", string.Format("Result count [{0}] > QueryMaxCount [{1}], Please reset condition ！", _resultCnt.ToString(), FormMainMDI.G_OPIAp.QueryMaxCount.ToString()), MessageBoxIcon.Information);
                    return ;
                }
                #endregion

                string projectName = Assembly.GetExecutingAssembly().GetName().Name;
                var query = ctx.ExecuteQuery(Type.GetType(string.Format("{0}.{1}", projectName, _frmParam.TableName)), sbSql.ToString(), new object[0]);
                DataTable dt = DBConnect.ToDataTable(query); 
                int rowCount = dt.Rows.Count;

                // 轉成只留下欲查詢欄位的DataTable
                DataTable dtDisplay = null;
                if (dt.Rows.Count == 0)
                {
                    dgvData.DataSource = null;
                    dgvData.PageSize = FormMainMDI.G_OPIAp.QueryPageSixeCount;
                    this.GenerateDataGridViewCol();
                }
                else
                {
                    dtDisplay = dt.DefaultView.ToTable(false, _frmParam.QueryFields.ToArray());
                    dgvData.PageSize = FormMainMDI.G_OPIAp.QueryMaxCount;
                    dgvData.SetPagedDataSource(dtDisplay, bnavRecord);

                }
                gbHistory.Text = string.Format("Total Count: {0}", rowCount.ToString());

                if (dt.Rows.Count == 0)
                    ShowMessage(this, "Query Reuslt", "", "No matching data for your query！", MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }   
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                DataTable dtExport = dgvData.Source;

                // DataGridView 沒資料不執行匯出動作
                if (dtExport.Rows.Count == 0)
                {
                    ShowMessage(this, MsgCaption, "", "No Data to Export!", MessageBoxIcon.Information);
                    return;
                }

                string message = string.Empty;
                if (UniTools.ExportToExcel(this.lblCaption.Text, dgvData, true, out message))
                {
                    ShowMessage(this, MsgCaption, "", "Export Success！", MessageBoxIcon.Information);
                }
                else
                {
                    if (!"".Equals(message))
                    {
                        ShowMessage(this, MsgCaption, "", message, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            } 
        }

        private void dgvData_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                var senderGrid = (DataGridView)sender;
                if (senderGrid.Columns[e.ColumnIndex] is DataGridViewButtonColumn && e.RowIndex >= 0)
                {
                    if (dgvData.Rows[e.RowIndex].DataBoundItem != null)
                    {
                        DataRow dr = ((DataRowView)dgvData.Rows[e.RowIndex].DataBoundItem).Row;

                        FormProcessDataHistoryDetail _frm = new FormProcessDataHistoryDetail(dr) { TopMost = true };
                        _frm.ShowDialog();

                        _frm.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }  
        }

        private void lblCaption_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            try
            {
                string fieldWidth = string.Empty;

                // 取得各欄位寬度並組成逗點隔開的字串
                foreach (DataGridViewColumn col in dgvData.Columns)
                {
                    if (col is DataGridViewButtonColumn)
                        continue;
                    fieldWidth += string.Format(",{0}", col.Width);
                }

                UniBCSDataContext ctx = FormMainMDI.G_OPIAp.DBCtx;
                SBRM_HISTABLEFIELDS histField =
                    (from c in ctx.SBRM_HISTABLEFIELDS
                     where c.TABLENAME == _frmParam.TableName
                     select c).Single();
                histField.FIELDWIDTH = fieldWidth.Substring(1);

                ctx.SubmitChanges();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }  
        }
        #endregion

        #region Private Methods
        // 動態產生查詢條件所需的控制項
        private void GenerateConditionUserCtl()
        {
            try
            {
                List<ParamInfo> lstParamInfo = _frmParam.ConditionParamInfo;
                if (lstParamInfo.Count == 0)
                    return;

                FieldTypes enumFieldType;
                foreach (ParamInfo pi in lstParamInfo)
                {
                    enumFieldType = (Enum.TryParse<FieldTypes>(pi.FieldType, out enumFieldType) == true) ? enumFieldType : FieldTypes.Unknown;
                    switch (enumFieldType)
                    {
                        case FieldTypes.ComboBox:
                            ucComBox_His objCombox = new ucComBox_His(pi);
                            objCombox.Caption = pi.FieldCaption;
                            flpnlCondition.Controls.Add(objCombox);
                            break;
                        case FieldTypes.TextBox:
                            ucTextBox_His objTextBox = new ucTextBox_His(pi);
                            objTextBox.Caption = pi.FieldCaption;
                            flpnlCondition.Controls.Add(objTextBox);
                            break;
                        case FieldTypes.Unknown:
                        default:
                            // TODO:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            } 
        }

        //動態產生DataGridView的欄位
        private void GenerateDataGridViewCol()
        {
            try
            {
                List<ColumnProp> lstColumnInfo = _frmParam.GridViewColumnInfo;
                grbDateTime.Tag = _frmParam.DateTimeField;

                if (lstColumnInfo.Count == 0)
                    return;

                dgvData.Columns.Clear();
                int width = 0;
                DataGridViewColumn col = null;

                //20150318 cy:將Detail欄位往前提到第一欄
                col = new DataGridViewButtonColumn();
                col.HeaderText = "Detail";
                ((DataGridViewButtonColumn)col).Text = "Detail";
                ((DataGridViewButtonColumn)col).UseColumnTextForButtonValue = true;
                col.Width = 100;
                dgvData.Columns.Add(col);

                foreach (ColumnProp cp in lstColumnInfo)
                {
                    col = new DataGridViewTextBoxColumn();
                    col.DataPropertyName = cp.DataField;
                    col.HeaderText = cp.DisplayField;

                    if ("F".Equals(cp.FieldWidth))
                    {
                        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                    else
                    {
                        col.Width = (int.TryParse(cp.FieldWidth, out width) == true) ? width : DEFAULT_COLUMN_WIDTH;
                    }

                    if (VisibleFields.Contains(cp.DataField)) col.Visible = false;

                    dgvData.Columns.Add(col);
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }  
        }

     
        #endregion



        private void rdoLatest_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                RadioButton _rdo = (RadioButton)sender;
                //DateTime _
                switch (_rdo.Tag.ToString())
                {
                    case "H":
                        dtpStartDate.Value = DateTime.Now.AddHours(-1);
                        dtpEndDate.Value = DateTime.Now;
                        break;

                    case "D":
                        dtpStartDate.Value = DateTime.Now.AddDays(-1);
                        dtpEndDate.Value = DateTime.Now;
                        break;

                    case "W":
                        dtpStartDate.Value = DateTime.Now.AddDays(-7);
                        dtpEndDate.Value = DateTime.Now;
                        break;

                    case "M":
                        dtpStartDate.Value = DateTime.Now.AddMonths(-1);
                        dtpEndDate.Value = DateTime.Now;
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void dtpDateTime_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                //加载过程中，不允许再次切换时间区间，以免Dic Lock期间，界面无响应
                if (isExcutingTask) return;
                lock (timerReloadSqls)
                {
                    timerReloadSqls.Clear();
                }
                foreach (ucComBox_His _cbo in flpnlCondition.Controls.OfType<ucComBox_His>())
                {
                    if (_cbo.Param.ItemCondition != "DATETIME") continue;

                    if (dtpStartDate.Value > dtpEndDate.Value) continue;

                    string _start = string.Format("{0}:00:00", dtpStartDate.Value.ToString("yyyy-MM-dd HH"));
                    string _end = string.Format("{0}:59:59", dtpEndDate.Value.ToString("yyyy-MM-dd HH"));

                    string _sql = string.Format(_cbo.Param.FieldCboValueSQL.ToString(), _start, _end);

                    //20171211 by huangjiayin: 预加载数据改成by timer异步执行，不等待SQL执行
                    lock (timerReloadSqls)
                    {
                        timerReloadSqls.Add(_sql, _cbo);
                    }
                }

                System.Timers.Timer reloadHisSqlDatas = new System.Timers.Timer(100);
                reloadHisSqlDatas.AutoReset = false;
                reloadHisSqlDatas.Elapsed += new System.Timers.ElapsedEventHandler(do_RelaodHisSqlDatasTask);
                reloadHisSqlDatas.Start();

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        //add by huangjiayin 20171211
        public void do_RelaodHisSqlDatasTask(object obj, ElapsedEventArgs e)
        {
            string _chooseItem = string.Empty;
            try
            {

                lock (timerReloadSqls)
                {
                    isExcutingTask = true;
                    foreach (var t in timerReloadSqls)
                    {
                        ucComBox_His _cbo = t.Value;
                        string _sql = t.Key;
                        UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBCtx;

                        if (_cbo.Param.FiledAttribute == "INT")
                        {
                            List<int> _lst = _ctx.ExecuteQuery<int>(_sql).ToList();

                            _cbo.Param.FieldDisplay = string.Join(",", _lst.ToArray());
                            _cbo.Param.FieldValue = string.Join(",", _lst.ToArray());
                        }
                        else
                        {
                            List<string> _lst = _ctx.ExecuteQuery<string>(_sql).ToList();

                            _cbo.Param.FieldDisplay = string.Join(",", _lst.ToArray());
                            _cbo.Param.FieldValue = string.Join(",", _lst.ToArray());
                        }

                        _chooseItem = _cbo.cmbItem.Text;

                        _cbo.BindDataSource();

                        _cbo.cmbItem.Text = _chooseItem;
                    }

                    isExcutingTask = false;
                }
            }
            catch (Exception ex)
            {
                //防止异常isExcutingTask不结束
                isExcutingTask = false;
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }


    }
}
