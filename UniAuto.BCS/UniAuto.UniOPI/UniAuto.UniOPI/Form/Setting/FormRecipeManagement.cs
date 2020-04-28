using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormRecipeManagement : FormBase
    {
        string MESControlMode = string.Empty;
        string RecipeKey = string.Empty;

        public FormRecipeManagement()
        {
            InitializeComponent();
        }

        private void FormRecipeManagement_Load(object sender, EventArgs e)
        {
            try
            {
                dgvRecipeNo.Rows.Clear();
                dgvRecipe.Rows.Clear();

                #region TBBFG機台沒有REMOTE MODE的模式
                if (FormMainMDI.G_OPIAp.CurLine.LineType == "BFG")
                {
                    rdoOnlineRemote.Visible = false;
                }
                #endregion
                    
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        public void SetMesModeChoose()
        {
            try
            {
                if (FormMainMDI.G_OPIAp.CurLine.MesControlMode == null) return;

                ///LOCAL,OFFLINE,REMOTE
                switch (FormMainMDI.G_OPIAp.CurLine.MesControlMode.ToUpper())
                {
                    case "OFFLINE":
                        rdoOffline.Checked = true;
                        break;

                    case "LOCAL":
                        rdoOnlineLocal.Checked = true; 
                        
                        break;

                    case "REMOTE":

                        rdoOnlineRemote.Checked = true; 

                        break;

                    default:
                        rdoOffline.Checked = false;
                        rdoOnlineLocal.Checked = false;
                        rdoOnlineRemote.Checked = false;

                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void GetRecipeDate()
        {
            try
            {
                if (MESControlMode == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, string.Empty, "Please choose MES Control Mode", MessageBoxIcon.Error);
                    return;
                }

                dgvRecipeNo.Rows.Clear();
                dgvRecipe.Rows.Clear();

                UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBBRMCtx;

                var _selData = from _d in _ctx.SBRM_RECIPE
                               where _d.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType && _d.ONLINECONTROLSTATE == MESControlMode
                               select _d;

                //資料庫資料
                if (RecipeKey.Length >1)
                {
                   _selData = from _d in _ctx.SBRM_RECIPE
                                   where _d.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType && _d.ONLINECONTROLSTATE == MESControlMode && _d.LINERECIPENAME.Contains(RecipeKey)
                                   select _d;
                }
                
                

                //已修改未更新物件
                var _modifyData = from _d in _ctx.GetChangeSet().Inserts.OfType<SBRM_RECIPE>()
                                  where _d.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType 
                                  select _d;

                List<SBRM_RECIPE> _objTables = _selData.ToList();

                _objTables.AddRange(_modifyData.ToList());

                foreach (SBRM_RECIPE _d in _objTables)
                {
                    dgvRecipe.Rows.Add(" R ", " P ", _d.OBJECTKEY,_d.ONLINECONTROLSTATE ,_d.LINERECIPENAME, _d.PPID, _d.REMARK, _d.UPDATEOPERATOR, _d.LASTUPDATEDT, _d.UPDATEPCIP);
                }

                 #region Set Color
                foreach (object objToInsert in _ctx.GetChangeSet().Inserts)
                {
                    if (objToInsert.GetType().Name != "SBRM_RECIPE") continue;

                    SBRM_RECIPE _recipe = (SBRM_RECIPE)objToInsert;

                    IEnumerable<DataGridViewRow> _rows = dgvRecipe.Rows.Cast<DataGridViewRow>()
                                                            .Where(r => r.Cells[colRecipeName.Name].Value.ToString().Equals(_recipe.LINERECIPENAME) &&
                                                                    r.Cells[colMesMode.Name].Value.ToString().Equals(_recipe.ONLINECONTROLSTATE));

                    foreach (DataGridViewRow _row in _rows)
                    {
                        dgvRecipe.Rows[_row.Index].DefaultCellStyle.BackColor = Color.Lime;
                    }
                }

                foreach (object objToDelete in _ctx.GetChangeSet().Deletes)
                {
                    if (objToDelete.GetType().Name != "SBRM_RECIPE") continue;

                    SBRM_RECIPE _recipe = (SBRM_RECIPE)objToDelete;

                    IEnumerable<DataGridViewRow> _rows = dgvRecipe.Rows.Cast<DataGridViewRow>()
                                                            .Where(r => r.Cells[colRecipeName.Name].Value.ToString().Equals(_recipe.LINERECIPENAME) &&
                                                                    r.Cells[colMesMode.Name].Value.ToString().Equals(_recipe.ONLINECONTROLSTATE));

                    foreach (DataGridViewRow _row in _rows)
                    {
                        dgvRecipe.Rows[_row.Index].DefaultCellStyle.BackColor = Color.Red;
                    }
                }

                foreach (object objToUpdate in _ctx.GetChangeSet().Updates)
                {
                    if (objToUpdate.GetType().Name != "SBRM_RECIPE") continue;

                    SBRM_RECIPE _recipe = (SBRM_RECIPE)objToUpdate;

                    IEnumerable<DataGridViewRow> _rows = dgvRecipe.Rows.Cast<DataGridViewRow>()
                                                            .Where(r => r.Cells[colRecipeName.Name].Value.ToString().Equals(_recipe.LINERECIPENAME) &&
                                                                    r.Cells[colMesMode.Name].Value.ToString().Equals(_recipe.ONLINECONTROLSTATE));

                    foreach (DataGridViewRow _row in _rows)
                    {
                        dgvRecipe.Rows[_row.Index].DefaultCellStyle.BackColor = Color.FromArgb(214, 233, 177);
                    }
                }

                #endregion

                if (dgvRecipe.Rows.Count > 0) dgvRecipe.ClearSelection();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void rdo_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                RadioButton _rdo = (RadioButton)sender;

                if (!_rdo.Checked) return;

                MESControlMode = _rdo.Tag.ToString();

                GetRecipeDate();

                if (dgvRecipe.Rows.Count > 0) dgvRecipe.ClearSelection();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            } 
        }

        private void dgvRecipe_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex < 0) return;

                #region Show Recipe No Detail

                dgvRecipeNo.Rows.Clear();

                // PPID 儲存格式為 L2:RecipeNo;L3:RecipeNo;L4:RecipeNo...
                string _recipeName = dgvRecipe.Rows[e.RowIndex].Cells[colRecipeName.Name].Value.ToString();
                string[] _ppid = dgvRecipe.Rows[e.RowIndex].Cells[colPPID.Name].Value.ToString().Split(';');
                string _cimMode = string.Empty;
                string _nodeDesc = string.Empty;

                foreach (string _Nodedata in _ppid)
                {
                    _cimMode = string.Empty;
                    string[] _data = _Nodedata.Split(':');

                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_data[0]))
                    {
                        _nodeDesc = _data[0] + "-" + FormMainMDI.G_OPIAp.Dic_Node[_data[0]].NodeID;
                        _cimMode = FormMainMDI.G_OPIAp.Dic_Node[_data[0]].CIMMode.ToString();
                    }
                    else _nodeDesc = _data[0];

                    dgvRecipeNo.Rows.Add(_nodeDesc, _data[1], _cimMode);

                }
                
                #endregion

                switch (e.ColumnIndex)
                {
                    case 0 :
                       
                        #region Recipe Register New
                        FormMainMDI.FrmRecipeRegister.SetMesModeChoose(MESControlMode, _recipeName);
                        FormMainMDI.FrmRecipeRegister.Visible = true;
                        FormMainMDI.FrmRecipeRegister.tmrRefresh.Enabled = true;
                        FormMainMDI.CurForm = FormMainMDI.FrmRecipeRegister;
                        FormMainMDI.FrmRecipeRegister.BringToFront();
                        #endregion

                        break ;

                    case 1 : 

                        #region Recipe Parameter
                        FormMainMDI.FrmRecipeParam.SetMesModeChoose(MESControlMode, _recipeName);
                        FormMainMDI.FrmRecipeParam.Visible = true;
                        FormMainMDI.FrmRecipeParam.tmrRefresh.Enabled = true;
                        FormMainMDI.CurForm = FormMainMDI.FrmRecipeParam;
                        FormMainMDI.FrmRecipeParam.BringToFront();
                        #endregion
                        
                        break ;

                    default :

                    break ;
                }             
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            } 
        }       

        private void btn_Click(object sender, EventArgs e)
        {
            try
            {
                string _err = string.Empty;
                string _sqlErr = string.Empty;

                Button _btn =(Button)sender ;

                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                switch (_btn.Tag.ToString())
                {
                    case "Add":

                        #region Add

                        FormRecipeManagement_Edit _addFrm = new FormRecipeManagement_Edit(MESControlMode);

                        if (_addFrm.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                        {
                            GetRecipeDate();
                        }

                        _addFrm.Dispose();

                        break;

                        #endregion

                    case "Modify":

                        #region Modify

                        if (dgvRecipe.SelectedRows.Count != 1) return;

                        DataGridViewRow _selectedRow = dgvRecipe.SelectedRows[0];

                        string strLineRecipeName = _selectedRow.Cells[colRecipeName.Name].Value.ToString(); 

                        if (MESControlMode == "REMOTE")
                        {
                            ShowMessage(this, this.lblCaption.Text, "", string.Format("Recipe with MES Mode [{0}] cannot be modify！", MESControlMode), MessageBoxIcon.Information);
                            return;
                        }

                        SBRM_RECIPE _objRecipe = null;

                        var _objSelModify = from _d in _ctxBRM.SBRM_RECIPE
                                            where _d.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType &&
                                                    _d.ONLINECONTROLSTATE == MESControlMode &&
                                                    _d.LINERECIPENAME == strLineRecipeName
                                            select _d;

                        if (_objSelModify.Count() > 0) _objRecipe = _objSelModify.ToList()[0];


                        var _objModify = from _d in _ctxBRM.GetChangeSet().Inserts.OfType<SBRM_RECIPE>()
                                            where _d.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType &&
                                                _d.ONLINECONTROLSTATE == MESControlMode &&
                                                _d.LINERECIPENAME == strLineRecipeName
                                            select _d;

                        if (_objModify.Count() > 0) _objRecipe = _objModify.ToList()[0];

                        if (_objRecipe == null)
                        {
                            ShowMessage(this, lblCaption.Text, string.Empty, string.Format("Can't find Recipe [{0}]！", strLineRecipeName), MessageBoxIcon.Warning);
                            GetRecipeDate();
                            return;
                        }

                        FormRecipeManagement_Edit _frmEditFrm = new FormRecipeManagement_Edit(MESControlMode, _objRecipe);

                        if (_frmEditFrm.ShowDialog() == System.Windows.Forms.DialogResult.OK) GetRecipeDate();

                        _frmEditFrm.Dispose();

                        break;

                        #endregion

                    case "Delete":

                        #region Delete

                        if (dgvRecipe.SelectedRows.Count == 0) return;

                        if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, "Are you sure to delete selected records?")) return;


                        foreach (DataGridViewRow _row in dgvRecipe.SelectedRows)
                        {
                            //DataRow objData = (selectedRow.DataBoundItem as DataRowView).Row;
                            long objectKey = long.Parse(_row.Cells[colObjectKey.Name].Value.ToString());

                            if (objectKey > 0)
                            {
                                var objSelDelete = from msg in _ctxBRM.SBRM_RECIPE
                                                    where msg.OBJECTKEY == objectKey
                                                    select msg;

                                if (objSelDelete.Count() > 0)
                                {
                                    foreach (SBRM_RECIPE obj in objSelDelete)
                                        _ctxBRM.SBRM_RECIPE.DeleteOnSubmit(obj);
                                }
                            }
                            else
                            {
                                SBRM_RECIPE objToDelete = (from msg in _ctxBRM.GetChangeSet().Inserts.OfType<SBRM_RECIPE>()
                                                            where msg.FABTYPE == FormMainMDI.G_OPIAp.CurLine.FabType &&
                                                                msg.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType &&
                                                                msg.ONLINECONTROLSTATE == MESControlMode &&
                                                                msg.LINERECIPENAME == _row.Cells[colRecipeName.Name].Value.ToString()
                                                            select msg).FirstOrDefault();

                                if (objToDelete != null)
                                    _ctxBRM.SBRM_RECIPE.DeleteOnSubmit(objToDelete);
                            }
                        }

                        GetRecipeDate();

                        break;

                        #endregion

                    case "Save":

                        #region Save

                        Save();

                        break;

                        #endregion

                    case "Refresh":

                        #region Refresh

                        CheckChangeSave();

                        //FormMainMDI.G_OPIAp.RefreshDBBRMCtx();

                        //GetRecipeDate();
                        
                        break;

                        #endregion

                    case "Sync":

                        #region 判斷recipe是否有修改，需先儲存
                        CheckChangeSave();
                        #endregion

                        #region Sync
                        FormRecipeSync _frmSync = new FormRecipeSync();
                        _frmSync.ShowDialog(this);
                        _frmSync.Dispose();

                        break;

                        #endregion

                    case "Recipe Check":

                        #region 判斷recipe是否有修改，需先儲存
                        CheckChangeSave();
                        #endregion

                        #region Recipe Check
                        FormRecipeManagement_NodeRecipeCheck _frmCheck = new FormRecipeManagement_NodeRecipeCheck();
                        _frmCheck.ShowDialog(this);
                        _frmCheck.Dispose();

                        break;
                        #endregion
                        
                    case "Parameter Enable":

                        #region 判斷recipe是否有修改，需先儲存
                        CheckChangeSave();
                        #endregion

                        #region Recipe Parameter Enable
                        FormRecipeManagement_NodeParameterCheck _frmParam = new FormRecipeManagement_NodeParameterCheck();
                        _frmParam.ShowDialog(this);
                        _frmParam.Dispose();

                        break;
                        #endregion

                    case "Export":

                        #region 判斷recipe是否有修改，需先儲存
                        CheckChangeSave();
                        #endregion

                        #region Export
                        List<string> _passColumns = new List<string> { "colRegister", "colParameter"};

                        if (UniTools.ExportToExcel(lblCaption.Text, dgvRecipe, 0,_passColumns, out _err))
                        {
                            ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", "Export Success！", MessageBoxIcon.Information);
                        }
                        else
                        {
                            ShowMessage(this, this.lblCaption.Text, "", _err, MessageBoxIcon.Error);
                        }

                        break;

                        #endregion

                    default :
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void chkCrossRecipe_CheckedChanged(object sender, EventArgs e)
        {
            try
            {

                btnParameterCheck.Visible = true;
                btnRecipeCheck.Visible = true;
                btnSync.Visible = true;

                dgvRecipe.Columns[colParameter.Name].Visible = true;
                dgvRecipeNo.Columns[colCIMMode.Name].Visible = true;

                GetRecipeDate();
                
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Save()
        {
            try
            {
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                string _err = string.Empty;
                string _sqlErr = string.Empty;
                string _sqlDesc = string.Empty;
                string _conStr = FormMainMDI.G_OPIAp.DBConnStr;
                try
                {
                    #region 取得更新的recipe name

                    //新增
                    foreach (object objToInsert in _ctxBRM.GetChangeSet().Inserts)
                    {
                        if (objToInsert.GetType().Name != "SBRM_RECIPE") continue;

                        UniTools.addRecipeTableHistory("Create", (SBRM_RECIPE)objToInsert, _ctxBRM, _conStr);

                        SBRM_RECIPE _recipe = (SBRM_RECIPE)objToInsert;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("ADD [ {0} , {1} , {2} ] ", _recipe.ONLINECONTROLSTATE, _recipe.LINERECIPENAME, _recipe.PPID);

                    }

                    //delete
                    foreach (object objToDelete in _ctxBRM.GetChangeSet().Deletes)
                    {
                        if (objToDelete.GetType().Name != "SBRM_RECIPE") continue;

                        UniTools.addRecipeTableHistory("Delete", (SBRM_RECIPE)objToDelete, _ctxBRM, _conStr);

                        SBRM_RECIPE _recipe = (SBRM_RECIPE)objToDelete;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ",") + string.Format("DEL [ {0} , {1} , {2} ] ", _recipe.ONLINECONTROLSTATE, _recipe.LINERECIPENAME, _recipe.PPID);

                    }

                    //modify
                    foreach (object objToUpdate in _ctxBRM.GetChangeSet().Updates)
                    {
                        if (objToUpdate.GetType().Name != "SBRM_RECIPE") continue;

                        UniTools.addRecipeTableHistory("Modify", (SBRM_RECIPE)objToUpdate, _ctxBRM, _conStr);

                        SBRM_RECIPE _recipe = (SBRM_RECIPE)objToUpdate;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ",") + string.Format("MOD [ {0} , {1} , {2} ] ", _recipe.ONLINECONTROLSTATE, _recipe.LINERECIPENAME, _recipe.PPID);

                    }


                    #endregion

                    if (_sqlDesc == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, string.Empty, "No Data for Update！", MessageBoxIcon.Warning);
                        return;
                    }

                    _ctxBRM.SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);
                }
                catch (System.Data.Linq.ChangeConflictException err)
                {

                    NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, err);

                    foreach (System.Data.Linq.ObjectChangeConflict occ in _ctxBRM.ChangeConflicts)
                    {
                        // 將變更的欄位寫入資料庫（合併更新）
                        occ.Resolve(System.Data.Linq.RefreshMode.KeepChanges);
                    }

                    try
                    {
                        _ctxBRM.SubmitChanges();
                    }
                    catch (Exception ex)
                    {
                        _sqlErr = ex.ToString();

                        foreach (System.Data.Linq.MemberChangeConflict _data in _ctxBRM.ChangeConflicts[0].MemberConflicts)
                        {
                            if (_data.DatabaseValue != _data.OriginalValue)
                            {
                                _sqlErr = _sqlErr + string.Format("\r\n Change Conflicts : Property '{0}': Database value: {1}, Original value {2}, Current Value:{3}", _data.Member.Name, _data.DatabaseValue, _data.OriginalValue, _data.CurrentValue);
                            }
                        }

                        NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                        ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                    }
                }

                #region 紀錄opi history
                UniTools.InsertOPIHistory_DB("SBRM_RECIPE", _sqlDesc, _sqlErr);

                if (_err != string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _err, MessageBoxIcon.Error);
                }
                #endregion

                Public.SendDatabaseReloadRequest("SBRM_RECIPE");

                GetRecipeDate();


                if (_sqlErr == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Recipe Save Success！", MessageBoxIcon.Information);
                }
                         
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        public override void CheckChangeSave()
        {
            try
            {
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                if (_ctxBRM.GetChangeSet().Inserts.Count() > 0 ||
                     _ctxBRM.GetChangeSet().Deletes.Count() > 0 ||
                     _ctxBRM.GetChangeSet().Updates.Count() > 0)
                {
                    string _msg = string.Format("Please confirm whether you will save data before change layout ?");
                    if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg))
                    {
                        RefreshData();
                        return;
                    }
                    else
                    {
                        Save();
                    }
                }
                else
                {
                    RefreshData();
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        public void RefreshData()
        {
            try
            {
                #region Refresh

                FormMainMDI.G_OPIAp.RefreshDBBRMCtx();

                GetRecipeDate();

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                if (textBox1.Text.Length ==1)
                {
                    return;
                }
                else
                {
                    RecipeKey = textBox1.Text;
                    FormMainMDI.G_OPIAp.RefreshDBBRMCtx();

                    GetRecipeDate();
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
    }
}
