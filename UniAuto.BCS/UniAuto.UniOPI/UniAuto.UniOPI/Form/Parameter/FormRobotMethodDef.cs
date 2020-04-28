using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.ComponentModel;
using UniClientTools;

namespace UniOPI
{
    public partial class FormRobotMethodDef : FormBase
    {
        public FormRobotMethodDef()
        {
            InitializeComponent();
        }

        private void FormRobotMethodDef_Load(object sender, EventArgs e)
        {
            //讀取GridView欄位數量
            for (int i = 0; i < dgvData.Columns.Count; i++)
            {
                asc.Add(false);
            }
            InitializeData();

        }
        List<SBRM_ROBOT_METHOD_DEF> objTables = null;
        List<bool> asc=new List<bool>(); // 此List用於讓使用者點選欄位時可以作順序或是倒序的排序

        private void InitializeData()
        {
            #region 檢查Method DEF是否有資料，若有則顯示出來
            try
            {
                var _robotMethodDef = from q in FormMainMDI.G_OPIAp.DBBRMCtx.SBRM_ROBOT_METHOD_DEF select q;
                if (_robotMethodDef == null || _robotMethodDef.Count() == 0)
                {
                    ShowMessage(this, lblCaption.Text, "", "Can't Find Robot Method Def data !", MessageBoxIcon.Error);
                    return;
                }
                GetGridViewData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

        private void btn_Click(object sender, EventArgs e)
        {
            #region 按鈕功能
            try
            {
                Button _btn = (Button)sender;
                switch (_btn.Tag.ToString())
                {
                    case "Add":
                        #region Add
                        FormRobotMethodDefEdit _frmRobotMethodDefEdit = new FormRobotMethodDefEdit(null);
                        _frmRobotMethodDefEdit.StartPosition = FormStartPosition.CenterScreen;
                        _frmRobotMethodDefEdit.ShowDialog();
                        _frmRobotMethodDefEdit.Dispose();
                        GetGridViewData();
                        break;
                        #endregion

                    case "Modify":
                        #region Modify
                        if (dgvData.SelectedRows.Count != 1) return;

                        SBRM_ROBOT_METHOD_DEF objSelModify = (dgvData.SelectedRows[0].DataBoundItem) as SBRM_ROBOT_METHOD_DEF;

                        UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                        SBRM_ROBOT_METHOD_DEF objProcess = null;

                        if (objSelModify.OBJECTKEY==0)
                        {
                            // 修改的是尚未Submit的新增
                            var objAddModify = (from d in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_METHOD_DEF>() where d.OBJECTNAME == objSelModify.OBJECTNAME && d.METHODNAME == objSelModify.METHODNAME select d).FirstOrDefault();

                            if (objAddModify != null)
                            {
                                objProcess = objAddModify;
                            }
                            else
                                return;
                        }
                        else
                            objProcess = objSelModify;

                        FormRobotMethodDefEdit _frmModify = new FormRobotMethodDefEdit(objProcess);
                        _frmModify.StartPosition = FormStartPosition.CenterScreen;
                        _frmModify.ShowDialog();
                        _frmModify.Dispose();

                        this.GetGridViewData();
                        break;
                        #endregion

                    case "Delete":
                        #region Delete
                        if (dgvData.SelectedRows.Count == 0) return;

                        DialogResult result = this.QuectionMessage(this,this.lblCaption.Text,"Are you sure to delete selected records?");
                        if (result == System.Windows.Forms.DialogResult.No)
                            return;

                       ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                       SBRM_ROBOT_METHOD_DEF objEach = null, objtoDelete = null;

                       foreach (DataGridViewRow selectedRow in dgvData.SelectedRows)
                       {
                           objEach = selectedRow.DataBoundItem as SBRM_ROBOT_METHOD_DEF;

                           if (FindOtherRobotTableIndex(objEach.OBJECTNAME,objEach.METHODNAME) == true)
                           {
                               return;
                           }


                           if (objEach.OBJECTKEY > 0)
                           {
                               ctxBRM.SBRM_ROBOT_METHOD_DEF.DeleteOnSubmit(objEach);
                           }
                           else
                           { 
                                objtoDelete = (from proc in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_METHOD_DEF>() where proc.OBJECTNAME.Equals(objEach.OBJECTNAME) && proc.METHODNAME.Equals(objEach.METHODNAME) select proc).FirstOrDefault() ;

                                if (objtoDelete != null)
                                    ctxBRM.SBRM_ROBOT_METHOD_DEF.DeleteOnSubmit(objtoDelete);
                           }
                       }

                       this.GetGridViewData();

                        break;
                        #endregion

                    case "Save":

                        #region Save
                        ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                        try
                        {
                            string _sqlDesc = string.Empty;
                            string _sqlErr = string.Empty;

                            try
                            {
                                #region 取得更新的data
                                foreach (object objToInsert in ctxBRM.GetChangeSet().Inserts)
                                {
                                    if (objToInsert.GetType().Name != "SBRM_ROBOT_METHOD_DEF") continue;
                                    SBRM_ROBOT_METHOD_DEF _updateData = (SBRM_ROBOT_METHOD_DEF)objToInsert;

                                    _updateData.LASTUPDATEDATE = DateTime.Now;

                                    _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("ADD [{0},{1},{2},{3},{4},{5}]", _updateData.METHODRULETYPE, _updateData.DESCRIPTION, _updateData.AUTHOR, _updateData.LASTUPDATEDATE, _updateData.ISENABLED, _updateData.REMARKS);
                                }
                                #endregion

                                #region Modify(包跨其他表格)
                                foreach (object objToModify in ctxBRM.GetChangeSet().Updates)
                                {
                                    if (objToModify.GetType().Name != "SBRM_ROBOT_METHOD_DEF") continue;
                                    SBRM_ROBOT_METHOD_DEF _updateData = (SBRM_ROBOT_METHOD_DEF)objToModify;

                                    _updateData.LASTUPDATEDATE = DateTime.Now;

                                    _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [{0},{1},{2},{3},{4},{5}]", _updateData.METHODRULETYPE, _updateData.DESCRIPTION, _updateData.AUTHOR, _updateData.LASTUPDATEDATE, _updateData.ISENABLED, _updateData.REMARKS);

                                    string _errModify= UniTools.InsertOPIHistory_DB("SBRM_ROBOT_METHOD_DEF", _sqlDesc, _sqlErr);

                                    #region 若為修改，將會一併把condition、select job資料表內相同的objectName, MethodName的decsription數值修改(注意:是所有相同的objectName、MethodName，不看Line)

                                        objSelModify = (SBRM_ROBOT_METHOD_DEF)objToModify;

#region condition
                                        var objConditionModify = (from d in ctxBRM.SBRM_ROBOT_ROUTE_CONDITION where  d.OBJECTNAME == objSelModify.OBJECTNAME && d.METHODNAME == objSelModify.METHODNAME select d).ToList();

                                        for (int i = 0; i < objConditionModify.Count();i++ )
                                        {
                                            objConditionModify[i].DESCRIPTION = objSelModify.DESCRIPTION;
                                            foreach (object objToConditionModify in ctxBRM.GetChangeSet().Updates)
                                            {
                                                if (objToModify.GetType().Name != "SBRM_ROBOT_ROUTE_CONDITION") continue;
                                                SBRM_ROBOT_ROUTE_CONDITION _updateDataCondition = (SBRM_ROBOT_ROUTE_CONDITION)objToConditionModify;

                                                _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [{0},{1},{2},{3},{4},{5}]", _updateDataCondition.ROBOTNAME, _updateDataCondition.OBJECTNAME, _updateDataCondition.METHODNAME, _updateDataCondition.DESCRIPTION, _updateDataCondition.ISENABLED, _updateDataCondition.REMARKS);
                                            }
                                            _errModify = UniTools.InsertOPIHistory_DB("SBRM_ROBOT_ROUTE_CONDITION", _sqlDesc, _sqlErr);

                                            if (_errModify != string.Empty)
                                            {
                                                ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _errModify, MessageBoxIcon.Error);
                                            }

                                            Public.SendDatabaseReloadRequest("SBRM_ROBOT_ROUTE_CONDITION");
                                            ShowMessage(this, lblCaption.Text, "Robot Route Condition contains the same information,will modify Decsripion field ！", _errModify, MessageBoxIcon.Information);
                                        }
                                        #endregion
#region select job
                                        var objJobModify = (from p in ctxBRM.SBRM_ROBOT_RULE_JOB_SELECT where p.OBJECTNAME.Equals(objSelModify.OBJECTNAME) && p.METHODNAME.Equals(objSelModify.METHODNAME) select p).ToList();

                                        for (int i = 0; i < objJobModify.Count(); i++)
                                        {
                                            objJobModify[i].DESCRIPTION = objSelModify.DESCRIPTION;
                                            foreach (object objToJobModify in ctxBRM.GetChangeSet().Updates)
                                            {
                                                if (objToModify.GetType().Name != "SBRM_ROBOT_RULE_JOB_SELECT") continue;
                                                SBRM_ROBOT_RULE_JOB_SELECT _updateDataJob = (SBRM_ROBOT_RULE_JOB_SELECT)objToJobModify;

                                                _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [{0},{1},{2},{3},{4},{5}]", _updateDataJob.ROBOTNAME, _updateDataJob.OBJECTNAME, _updateDataJob.METHODNAME, _updateDataJob.DESCRIPTION, _updateDataJob.ISENABLED, _updateDataJob.REMARKS);
                                            }
                                            _errModify = UniTools.InsertOPIHistory_DB("SBRM_ROBOT_RULE_JOB_SELECT", _sqlDesc, _sqlErr);
                                            ShowMessage(this, lblCaption.Text, "Robot Route Rule Job Select contains the same information,will modify Decsripion field ！", _errModify, MessageBoxIcon.Information);
                                        }
                                        #endregion
#region filter
                                        var objFilterModify = (from p in ctxBRM.SBRM_ROBOT_RULE_FILTER where p.OBJECTNAME.Equals(objSelModify.OBJECTNAME) && p.METHODNAME.Equals(objSelModify.METHODNAME) select p).ToList();

                                        for (int i = 0; i < objFilterModify.Count(); i++)
                                        {
                                            objFilterModify[i].DESCRIPTION = objSelModify.DESCRIPTION;
                                            foreach (object objToModifyFilter in ctxBRM.GetChangeSet().Updates)
                                            {
                                                if (objToModify.GetType().Name != "SBRM_ROBOT_RULE_FILTER") continue;
                                                SBRM_ROBOT_RULE_FILTER _updateDataFilter = (SBRM_ROBOT_RULE_FILTER)objToModifyFilter;

                                                _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [{0},{1},{2},{3},{4},{5}]", _updateDataFilter.ROBOTNAME, _updateDataFilter.OBJECTNAME, _updateDataFilter.METHODNAME, _updateDataFilter.DESCRIPTION, _updateDataFilter.ISENABLED, _updateDataFilter.REMARKS);
                                            }
                                            _errModify = UniTools.InsertOPIHistory_DB("SBRM_ROBOT_RULE_FILTER", _sqlDesc, _sqlErr);
                                            ShowMessage(this, lblCaption.Text, "Robot Route Rule Filter Select contains the same information,will modify Decsripion field ！", _errModify, MessageBoxIcon.Information);
                                        }
#endregion
#region Orderby
                                        var objOrderbyModify = (from p in ctxBRM.SBRM_ROBOT_RULE_ORDERBY where p.OBJECTNAME.Equals(objSelModify.OBJECTNAME) && p.METHODNAME.Equals(objSelModify.METHODNAME) select p).ToList();

                                        for (int i = 0; i < objOrderbyModify.Count(); i++)
                                        {
                                            objOrderbyModify[i].DESCRIPTION = objSelModify.DESCRIPTION;
                                            foreach (object objToModifyOrderBy in ctxBRM.GetChangeSet().Updates)
                                            {
                                                if (objToModify.GetType().Name != "SBRM_ROBOT_RULE_ORDERBY") continue;
                                                SBRM_ROBOT_RULE_ORDERBY _updateDataOrderBy = (SBRM_ROBOT_RULE_ORDERBY)objToModifyOrderBy;

                                                _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [{0},{1},{2},{3},{4},{5}]", _updateDataOrderBy.ROBOTNAME, _updateDataOrderBy.OBJECTNAME, _updateDataOrderBy.METHODNAME, _updateDataOrderBy.DESCRIPTION, _updateDataOrderBy.ISENABLED, _updateDataOrderBy.REMARKS);
                                            }
                                            _errModify = UniTools.InsertOPIHistory_DB("SBRM_ROBOT_RULE_ORDERBY", _sqlDesc, _sqlErr);
                                            ShowMessage(this, lblCaption.Text, "Robot Route Rule Order by contains the same information,will modify Decsripion field ！", _errModify, MessageBoxIcon.Information);
                                        }
#endregion
#region Handle
                                        var objHandlebyModify = (from p in ctxBRM.SBRM_ROBOT_PROC_RESULT_HANDLE where p.OBJECTNAME.Equals(objSelModify.OBJECTNAME) && p.METHODNAME.Equals(objSelModify.METHODNAME) select p).ToList();

                                        for (int i = 0; i < objHandlebyModify.Count(); i++)
                                        {
                                            objHandlebyModify[i].DESCRIPTION = objSelModify.DESCRIPTION;
                                            foreach (object objToModifyHandle in ctxBRM.GetChangeSet().Updates)
                                            {
                                                if (objToModify.GetType().Name != "SBRM_ROBOT_PROC_RESULT_HANDLE") continue;
                                                SBRM_ROBOT_PROC_RESULT_HANDLE _updateDataHandle = (SBRM_ROBOT_PROC_RESULT_HANDLE)objToModifyHandle;

                                                _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [{0},{1},{2},{3},{4},{5}]", _updateDataHandle.ROBOTNAME, _updateDataHandle.OBJECTNAME, _updateDataHandle.METHODNAME, _updateDataHandle.DESCRIPTION, _updateDataHandle.ISENABLED, _updateDataHandle.REMARKS);
                                            }
                                            _errModify = UniTools.InsertOPIHistory_DB("SBRM_ROBOT_PROC_RESULT_HANDLE", _sqlDesc, _sqlErr);
                                            ShowMessage(this, lblCaption.Text, "Robot Route Rule Result Handle contains the same information,will modify Decsripion field ！", _errModify, MessageBoxIcon.Information);
                                        }
#endregion 
#region BYPASS
                                        var objByPassbyModify = (from p in ctxBRM.SBRM_ROBOT_RULE_ROUTESTEP_BYPASS where p.OBJECTNAME.Equals(objSelModify.OBJECTNAME) && p.METHODNAME.Equals(objSelModify.METHODNAME) select p).ToList();

                                        for (int i = 0; i < objByPassbyModify.Count(); i++)
                                        {
                                            objByPassbyModify[i].DESCRIPTION = objSelModify.DESCRIPTION;
                                            foreach (object objToModifyByPass in ctxBRM.GetChangeSet().Updates)
                                            {
                                                if (objToModify.GetType().Name != "SBRM_ROBOT_RULE_ROUTESTEP_BYPASS") continue;
                                                SBRM_ROBOT_RULE_ROUTESTEP_BYPASS _updateDataByPass = (SBRM_ROBOT_RULE_ROUTESTEP_BYPASS)objToModifyByPass;

                                                _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [{0},{1},{2},{3},{4},{5}]", _updateDataByPass.ROBOTNAME, _updateDataByPass.OBJECTNAME, _updateDataByPass.METHODNAME, _updateDataByPass.DESCRIPTION, _updateDataByPass.ISENABLED, _updateDataByPass.REMARKS);
                                            }
                                            _errModify = UniTools.InsertOPIHistory_DB("SBRM_ROBOT_RULE_ROUTESTEP_BYPASS", _sqlDesc, _sqlErr);
                                            ShowMessage(this, lblCaption.Text, "Robot Route Rule Step ByPass contains the same information,will modify Decsripion field ！", _errModify, MessageBoxIcon.Information);
                                        }
#endregion
#region Jump
                                        var objJumpbyModify = (from p in ctxBRM.SBRM_ROBOT_RULE_ROUTESTEP_JUMP where p.OBJECTNAME.Equals(objSelModify.OBJECTNAME) && p.METHODNAME.Equals(objSelModify.METHODNAME) select p).ToList();

                                        for (int i = 0; i < objJumpbyModify.Count(); i++)
                                        {
                                            objJumpbyModify[i].DESCRIPTION = objSelModify.DESCRIPTION;
                                            foreach (object objToModifyJump in ctxBRM.GetChangeSet().Updates)
                                            {
                                                if (objToModify.GetType().Name != "SBRM_ROBOT_RULE_ROUTESTEP_JUMP") continue;
                                                SBRM_ROBOT_RULE_ROUTESTEP_JUMP _updateDataJump = (SBRM_ROBOT_RULE_ROUTESTEP_JUMP)objToModifyJump;

                                                _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [{0},{1},{2},{3},{4},{5}]", _updateDataJump.ROBOTNAME, _updateDataJump.OBJECTNAME, _updateDataJump.METHODNAME, _updateDataJump.DESCRIPTION, _updateDataJump.ISENABLED, _updateDataJump.REMARKS);
                                            }
                                            _errModify = UniTools.InsertOPIHistory_DB("SBRM_ROBOT_RULE_ROUTESTEP_JUMP", _sqlDesc, _sqlErr);
                                            ShowMessage(this, lblCaption.Text, "Robot Route Rule Step Jump contains the same information,will modify Decsripion field ！", _errModify, MessageBoxIcon.Information);
                                        }
                                    #endregion
                                    #endregion
                                }
                                #endregion

                                #region DELETE
                                foreach (object objToDelete in ctxBRM.GetChangeSet().Deletes)
                                {
                                    if (objToDelete.GetType().Name != "SBRM_ROBOT_METHOD_DEF") continue;
                                    SBRM_ROBOT_METHOD_DEF _updateData = (SBRM_ROBOT_METHOD_DEF)objToDelete;

                                    _updateData.LASTUPDATEDATE = DateTime.Now;

                                    _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("DEL [{0},{1},{2},{3},{4},{5}]", _updateData.METHODRULETYPE, _updateData.DESCRIPTION, _updateData.AUTHOR, _updateData.LASTUPDATEDATE, _updateData.ISENABLED, _updateData.REMARKS);
                                }
                                #endregion

                                if (_sqlDesc == string.Empty)
                                {
                                    ShowMessage(this, lblCaption.Text, string.Empty, "No Data for Update !", MessageBoxIcon.Warning);
                                    return;
                                }
                                ctxBRM.SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);

                            }
                            catch (System.Data.Linq.ChangeConflictException err)
                            {
                                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, err);

                                foreach(System.Data.Linq.ObjectChangeConflict occ in ctxBRM.ChangeConflicts)
                                {
                                    // 將變更的欄位寫入資料庫（合併更新）
                                    occ.Resolve(System.Data.Linq.RefreshMode.KeepChanges);
                                }

                                try
                                {
                                    ctxBRM.SubmitChanges();
                                }
                                catch (Exception ex)
                                {
                                    _sqlErr = ex.ToString();

                                    foreach(System.Data.Linq.MemberChangeConflict _data in ctxBRM.ChangeConflicts[0].MemberConflicts)
                                    {
                                        if (_data.DatabaseValue != _data.OriginalValue)
                                        {
                                            _sqlErr = _sqlErr + string.Format("\r\n Change Conflicts : Property '{0}': Database value : {1} , Original value {2} ,Current Value :{3}" ,_data.Member.Name,_data.DatabaseValue,_data.OriginalValue,_data.CurrentValue);
                                        }
                                    }
                                    NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                                    ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                                }
                            }

                            #region 紀錄opi history 
                            string _err = UniTools.InsertOPIHistory_DB("SBRM_ROBOT_METHOD_DEF", _sqlDesc, _sqlErr);

                            if (_err != string.Empty)
                            {
                                ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _err, MessageBoxIcon.Error);
                            }
                            #endregion

                            Public.SendDatabaseReloadRequest("SBRM_ROBOT_METHOD_DEF");

                            if (_sqlErr == string.Empty)
                            {
                                ShowMessage(this, lblCaption.Text, "", "Robot Method Definition Save Success!", MessageBoxIcon.Information);
                            }
                            this.GetGridViewData();
                        }
                        catch (Exception ex)
                        {
                            NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                            ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                        }

                        break;
                        #endregion

                    case "Refresh":

                        #region Refresh
                        FormMainMDI.G_OPIAp.RefreshDBBRMCtx();
                        this.GetGridViewData();
                        break;
                        #endregion

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

        private void GetGridViewData()
        {
            #region 顯示GridView資料
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                //若cboMethodRuleType未選擇，則讀取全部資料，若有則只讀取特定欄位

                if (cboMetodRuleType.SelectedIndex > 0)
                {
                    objTables = (from q in ctxBRM.SBRM_ROBOT_METHOD_DEF where q.METHODRULETYPE.Equals(cboMetodRuleType.SelectedItem.ToString()) select q).ToList();
                }
                else
                    objTables = (from q in ctxBRM.SBRM_ROBOT_METHOD_DEF select q).ToList();

                //已修該未更新資料
                var addData = ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_METHOD_DEF>();
               
                objTables.AddRange(addData.ToList());

                SortableBindingList<SBRM_ROBOT_METHOD_DEF> _fieldList = new SortableBindingList<SBRM_ROBOT_METHOD_DEF>(objTables);

                BindingSource _source = new BindingSource();

                _source.DataSource = _fieldList;

                dgvData.AutoGenerateColumns = false;

                dgvData.DataSource = _source;
           
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

        private void dgvData_DataSourceChanged(object sender, EventArgs e)
        {
            dgvCellBackColor();
        }

        private bool FindOtherRobotTableIndex(string ObjectName ,string MethodName)
        {
            #region 刪除時先檢查相關表格有無值 (這裡會再更改)
            UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
            try
            {
                var _jobSelect = (from q in _ctxBRM.SBRM_ROBOT_RULE_JOB_SELECT where q.OBJECTNAME.Equals(ObjectName) && q.METHODNAME.Equals(MethodName) select q).Distinct().ToList();
                
                if (_jobSelect == null || _jobSelect.Count() == 0)
                {
                    var _condition = (from d in _ctxBRM.SBRM_ROBOT_ROUTE_CONDITION where d.OBJECTNAME.Equals(ObjectName) && d.METHODNAME.Equals(MethodName) select d).Distinct().ToList();

                    if (_condition == null || _condition.Count == 0)
                    {
                        var _filter = (from filter in _ctxBRM.SBRM_ROBOT_RULE_FILTER where filter.OBJECTNAME.Equals(ObjectName) && filter.METHODNAME.Equals(MethodName) select filter).Distinct().ToList();

                        if (_filter == null || _filter.Count == 0)
                        {
                            var _order = (from order in _ctxBRM.SBRM_ROBOT_RULE_ORDERBY where order.OBJECTNAME.Equals(ObjectName) && order.METHODNAME.Equals(MethodName) select order).Distinct().ToList();

                            if (_order == null || _order.Count == 0)
                            {
                                var _handle = (from handle in _ctxBRM.SBRM_ROBOT_PROC_RESULT_HANDLE where handle.OBJECTNAME.Equals(ObjectName) && handle.METHODNAME.Equals(MethodName) select handle).Distinct().ToList();

                                if (_handle == null || _handle.Count == 0)
                                {
                                    var _StageSelect = (from stageSelect in _ctxBRM.SBRM_ROBOT_RULE_STAGE_SELECT where stageSelect.OBJECTNAME.Equals(ObjectName) && stageSelect.METHODNAME.Equals(MethodName) select stageSelect).Distinct().ToList();
                                    if(_StageSelect == null || _StageSelect.Count==0)
                                    {
                                        var _ByPass = (from byPass in _ctxBRM.SBRM_ROBOT_RULE_ROUTESTEP_BYPASS where byPass.OBJECTNAME.Equals(ObjectName) && byPass.METHODNAME.Equals(MethodName) select byPass).Distinct().ToList();
                                        if (_ByPass == null || _ByPass.Count == 0)
                                        {
                                            var _Jump = (from jump in _ctxBRM.SBRM_ROBOT_RULE_ROUTESTEP_JUMP where jump.OBJECTNAME.Equals(ObjectName) && jump.METHODNAME.Equals(MethodName) select jump).Distinct().ToList();
                                            if (_Jump == null || _Jump.Count == 0)
                                            {
                                                return false;
                                            }
                                            else
                                            {
                                                ShowMessage(this, lblCaption.Text, "", "This Data has been reference in ROBOT RULE ROUTESTEP_JUMP  Data, it can not delete ", MessageBoxIcon.Error);
                                                return true;
                                            }
                                        }
                                        else
                                        {
                                            ShowMessage(this, lblCaption.Text, "", "This Data has been reference in ROBOT RULE ROUTESTEP_BYPASS  Data, it can not delete ", MessageBoxIcon.Error);
                                            return true;
                                        }
                                    }
                                    else
                                    {
                                        ShowMessage(this, lblCaption.Text, "", "This Data has been reference in ROBOT RULE STAGE SELECT  Data, it can not delete ", MessageBoxIcon.Error);
                                        return true;
                                    }
                                }
                                else
                                {
                                    ShowMessage(this, lblCaption.Text, "", "This Data has been reference in ROBOT RULE RESULT HANDLE Data, it can not delete ", MessageBoxIcon.Error);
                                    return true;
                                }
                            }
                            else
                            {
                                ShowMessage(this, lblCaption.Text, "", "This Data has been reference in ROBOT RULE ORDERBY Data, it can not delete ", MessageBoxIcon.Error);
                                return true;
                            }
                        }
                        else
                        {
                            ShowMessage(this, lblCaption.Text, "", "This Data has been reference in ROBOT RULE FILTER Data, it can not delete ", MessageBoxIcon.Error);
                            return true;
                        }
                    }
                    else
                    {
                        ShowMessage(this, lblCaption.Text, "", "This Data has been reference in ROBOT ROUTE CONDITION Data, it can not delete ", MessageBoxIcon.Error);
                        return true;
                    }
                }
                else
                {
                    ShowMessage(this, lblCaption.Text, "", "This Data has been reference in ROBOT RULE JOB SELECT Data, it can not delete ", MessageBoxIcon.Error);
                    return true;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                return true;
            }

            #endregion
        }

        private void cboMetodRuleType_SelectedIndexChanged(object sender, EventArgs e)
        {
            //當cboMetodRuleType改變時重新顯示GridView
            try
            {
                GetGridViewData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvData_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dgvCellBackColor();
        }

        private void dgvCellBackColor()
        {            
            try
            {
                if (dgvData.DataSource == null || dgvData.Rows.Count == 0) return;

                #region 若GridView資料有變更，改變變更資料的顏色

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                //ctxBRM.GetChangeSet();
                foreach (DataGridViewRow dr in dgvData.Rows)
                {
                    SBRM_ROBOT_METHOD_DEF obj = dr.DataBoundItem as SBRM_ROBOT_METHOD_DEF;

                    if (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_METHOD_DEF>().Any(
                        msg => msg.OBJECTNAME == obj.OBJECTNAME && msg.METHODNAME == obj.METHODNAME))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(214, 233, 177);
                    }
                    else if (ctxBRM.GetChangeSet().Deletes.OfType<SBRM_ROBOT_METHOD_DEF>().Any(
                        msg => msg.METHODNAME == obj.METHODNAME && msg.OBJECTNAME == obj.OBJECTNAME))
                    {
                        dr.DefaultCellStyle.BackColor = Color.Red;
                    }
                    else if (ctxBRM.GetChangeSet().Updates.OfType<SBRM_ROBOT_METHOD_DEF>().Any(
                        msg => msg.OBJECTNAME == obj.OBJECTNAME && msg.METHODNAME == obj.METHODNAME))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 195);
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }           
        }
    }
}
