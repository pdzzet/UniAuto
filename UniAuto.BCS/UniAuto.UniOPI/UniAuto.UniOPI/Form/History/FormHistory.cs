using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Timers;
using System.Threading;


namespace UniOPI
{
    public partial class FormHistory : FormBase
    {
        #region Fields
        public const int DEFAULT_COLUMN_WIDTH = 100;
        private HisTableParam _frmParam = null;
        #endregion
        bool RadioFlag = false;
        public Dictionary<string, ucComBox_His> timerReloadSqls=new Dictionary<string,ucComBox_His>();//add by huangjiayin 20171211
        bool isExcutingTask = false;

        public FormHistory(HisTableParam histParam)
        {
            InitializeComponent();

            this._frmParam = histParam;

            this.GenerateConditionUserCtl();
            this.GenerateDataGridViewCol();
        }

        private void FormHistory_Load(object sender, EventArgs e)
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
                     
                     //20170209: huangjiayin Job His 超出的话，依然可以按MaxQueryCnt显示
                    if (_frmParam.TableName == "SBCS_JOBHISTORY_TRX")
                    {
                        ShowMessage(this, "Query Reuslt", "", string.Format("Result count [{0}] > QueryMaxCount [{1}], Only Show the Top{1} Results ！", _resultCnt.ToString(), FormMainMDI.G_OPIAp.QueryMaxCount.ToString()), MessageBoxIcon.Information);
                     }
                    else
                    {
                        ShowMessage(this, "Query Reuslt", "", string.Format("Result count [{0}] > QueryMaxCount [{1}], Please reset condition ！", _resultCnt.ToString(), FormMainMDI.G_OPIAp.QueryMaxCount.ToString()), MessageBoxIcon.Information);
                        return;
                    }
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

                    this.GenerateDataGridViewCol();
                }
                else
                {
                    dtDisplay = dt.DefaultView.ToTable(false, _frmParam.QueryFields.ToArray());
                    dgvData.PageSize = FormMainMDI.G_OPIAp.QueryPageSixeCount;
                    dgvData.SetPagedDataSource(dtDisplay, bnavRecord);
                }
                gbHistory.Text = string.Format("Total Count: {0}", rowCount.ToString());

                if (dt.Rows.Count == 0)
                    ShowMessage(this, "Query Result", "", "No matching data for your query！", MessageBoxIcon.Information);
               

                //OPI的Job History的Grid畫面能用顏色顯色不同的Evnet訊息內容.
                if (_frmParam.TableName == "SBCS_JOBHISTORY_TRX")
                {
                    foreach (DataGridViewRow _row in dgvData.Rows)
                    {
                        _row.DefaultCellStyle.BackColor = FindColorCode(_row.Cells["EVENTNAME"].Value.ToString());
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);               
            }  
        }

        //動態產生查詢條件所需的控制項
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

                            if (pi.RelationField.ToUpper() == "Y")
                            {
                                if (pi.FieldKey == "NODEID")
                                {
                                    objCombox.cmbItem.SelectedValueChanged += new EventHandler(CboNode_SelectedValueChanged);
                                    objCombox.cmbItem.Tag = objCombox;

                                    objCombox.chkUse.CheckedChanged +=new EventHandler(chkNodeUse_CheckedChanged);
                                    objCombox.chkUse.Tag = objCombox;
                                }
                            }
                            
                            flpnlCondition.Controls.Add(objCombox);
                            break;

                        case FieldTypes.TextBox:
                            ucTextBox_His objTextBox = new ucTextBox_His(pi);
                            objTextBox.Caption = pi.FieldCaption;
                            flpnlCondition.Controls.Add(objTextBox);
                            break;

                        case FieldTypes.Unknown:

                        default:
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
                foreach (ColumnProp cp in lstColumnInfo)
                {
                    col = new DataGridViewTextBoxColumn();
                    col.DataPropertyName = cp.DataField;
                    col.HeaderText = cp.DisplayField;
                    col.Name = cp.DataField;

                    if ("F".Equals(cp.FieldWidth))
                    {
                        col.AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill;
                    }
                    else
                    {
                        col.Width = (int.TryParse(cp.FieldWidth, out width) == true) ? width : DEFAULT_COLUMN_WIDTH;
                    }

                    if (!"".Equals(cp.FieldFormat))
                    {
                        col.DefaultCellStyle.Format = cp.FieldFormat;
                    }

                    dgvData.Columns.Add(col);

                }

                if (_frmParam.TableName == "SBCS_JOBHISTORY_TRX")
                {
                    dgvData.CellDoubleClick += new DataGridViewCellEventHandler(dgvData_CellDoubleClick_JobDataHis); 
                }
               
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }  
        }

        private void chkNodeUse_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox _chk = (CheckBox)sender;

                if (_chk.Checked == false) return;

                ucComBox_His objCombox = (ucComBox_His)_chk.Tag;

                if (objCombox.cmbItem.SelectedValue == null) return;

                string _nodeID = objCombox.cmbItem.SelectedValue.ToString();

                ReloadComboBoxItem_NodeID(_nodeID);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void CboNode_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                ComboBox _cbo = (ComboBox)sender;

                if (_cbo.Tag == null) return;

                if (_cbo.SelectedValue == null) return;

                ucComBox_His objCombox = (ucComBox_His)_cbo.Tag;

                if (objCombox.Checked == false) return;

                string _nodeID = _cbo.SelectedValue.ToString().Trim();

                ReloadComboBoxItem_NodeID(_nodeID);
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
                    ShowMessage(this, lblCaption.Text , "", "No Data to Export!", MessageBoxIcon.Information);
                    return;
                }

                string message = string.Empty;
                if (UniTools.ExportToExcel(this.lblCaption.Text, dgvData, true, out message))
                {
                    ShowMessage(this, lblCaption.Text , "", "Export Success！", MessageBoxIcon.Information);
                }
                else
                {
                    if (!"".Equals(message))
                    {
                        ShowMessage(this, lblCaption.Text , "", message, MessageBoxIcon.Error);
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

        private void rdoLatest_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                RadioButton _rdo = (RadioButton)sender;

                if (_rdo.Checked == false) return;

                //DateTime _
                switch (_rdo.Tag.ToString())
                {
                    case "H":
                        RadioFlag = false;
                        dtpStartDate.Value = DateTime.Now.AddHours(-1);
                        RadioFlag = true;
                        dtpEndDate.Value = DateTime.Now;
                        break;

                    case "D":
                        RadioFlag = false;
                        dtpStartDate.Value = DateTime.Now.AddDays(-1);
                        RadioFlag = true;
                        dtpEndDate.Value = DateTime.Now;
                        break;

                    case "W":
                        RadioFlag = false;
                        dtpStartDate.Value = DateTime.Now.AddDays(-7);
                        RadioFlag = true;
                        dtpEndDate.Value = DateTime.Now;
                        break;

                    case "M":
                        RadioFlag = false;
                        dtpStartDate.Value = DateTime.Now.AddMonths(-1);
                        RadioFlag = true;
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
            finally
            {
                RadioFlag = true;
            }
        }

        private void dtpDateTime_ValueChanged(object sender, EventArgs e)
        {
            if(RadioFlag)
                ReloadComboBoxItem_DateTime();
        }

        private void ReloadComboBoxItem_NodeID(string NodeId)
        {
            try
            {
                string _display = string.Empty;
                string _value = string.Empty;

                foreach (ucComBox_His _cbo in flpnlCondition.Controls.OfType<ucComBox_His>())
                {
                    if (_cbo.Param.ItemCondition != "NODEID") continue;

                    switch ( _cbo.Name)
                    {
                        case "PORTID":
                            foreach (Port _port in FormMainMDI.G_OPIAp.Dic_Port.Values.Where(r => r.NodeID.Equals(NodeId)))
                            {
                                _display = _display + (_display == string.Empty ? "" : ",") + string.Format("{0}-{1}-{2}", _port.NodeNo, _port.PortNo, _port.PortID);
                                _value = _value + (_value == string.Empty ? "" : ",") + _port.PortID;
                            }
                            break ;

                        case "UNITID":
                            foreach (Unit _unit in FormMainMDI.G_OPIAp.Dic_Unit.Values.Where(r => r.NodeID.Equals(NodeId)))
                            {
                                _display = _display + (_display == string.Empty ? "" : ",") + string.Format("{0}-{1}-{2}", _unit.NodeNo, _unit.UnitNo.PadLeft(2, '0'), _unit.UnitID);
                                _value = _value + (_value == string.Empty ? "" : ",") + _unit.UnitID;
                            }
                            break ;

                        default :
                            break ;
                    }

                    _cbo.Param.FieldDisplay = _display;
                    _cbo.Param.FieldValue = _value;

                    _cbo.BindDataSource();
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        //2015-05-09 REX修改
        private Color FindColorCode(string value)
        {
            try
            {
                int index = 0;
                string[] keyWord = { "SendOut ", "Receive", "Store", "FetchOut", "Remove", "Create", "Delete", "Recovery", "Hold", "OXUpdate", "Delete_CST_Complete", "EQP_NEW", "Edit", "CUT_CREATE_CHIP", "Assembly", "VCR_Report", "VCR_Mismatch", "VCR_Mismatch_Copy" };
                for (; index < keyWord.Length; index++)
                {
                    if (value.IndexOf(keyWord[index]) != -1)
                    {
                        break;
                    }
                }
                switch (index)
                {
                    //DeepSkyBlue
                    case 0:
                        return Color.FromArgb(0, 191, 255);
                    //PaleGreen1
                    case 1:
                        return Color.FromArgb(154, 255, 154);
                    //PaleGoldenrod
                    case 2:
                        return Color.FromArgb(238, 232, 170);
                    //RosyBrown1
                    case 3:
                        return Color.FromArgb(255, 193, 193);
                    //Thistle
                    case 4:
                        return Color.FromArgb(216, 191, 216);
                    //Salmon1
                    case 5:
                        return Color.FromArgb(255, 140, 105);
                    //AntiqueWhite1
                    case 6:
                        return Color.FromArgb(255, 239, 219);
                    //Azure2
                    case 7:
                        return Color.FromArgb(224, 238, 238);
                    //Plum1
                    case 8:
                        return Color.FromArgb(255, 187, 255);
                    //grey71
                    case 9:
                        return Color.FromArgb(181, 181, 181);
                    //LightGreen
                    case 10:
                        return Color.FromArgb(144, 238, 144);
                    //LightCyan3
                    case 11:
                        return Color.FromArgb(180, 205, 205);
                    //Chartreuse1
                    case 12:
                        return Color.FromArgb(127, 255, 0);
                    //DarkSeaGreen3
                    case 13:
                        return Color.FromArgb(155, 205, 155);
                    //CornflowerBlue
                    case 14:
                        return Color.FromArgb(100, 149, 237);
                    //Yellow1
                    case 15:
                        return Color.FromArgb(255, 255, 0);
                    //LtGoldenrodYello
                    case 16:
                        return Color.FromArgb(250, 250, 210);
                    default:
                        return Color.White;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                return Color.White;
            } 
        }

        public void ReloadComboBoxItem_DateTime()
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
        public  void do_RelaodHisSqlDatasTask(object obj,ElapsedEventArgs e)
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


        private void dgvData_CellDoubleClick_JobDataHis(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0) return;

                if (dgvData.CurrentRow != null)
                {

                    FormHistory_JobDetail _frm = new FormHistory_JobDetail(dgvData.CurrentRow);
                    _frm.ShowDialog();

                    if (_frm != null) _frm.Dispose();
                }                      
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvData_refreshBGC(object sender, EventArgs e)
        {
            if (_frmParam.TableName == "SBCS_JOBHISTORY_TRX")
            {
                foreach (DataGridViewRow _row in dgvData.Rows)
                {
                    _row.DefaultCellStyle.BackColor = FindColorCode(_row.Cells["EVENTNAME"].Value.ToString());
                }
            }
        }

        


    }
}
