using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormDailyCheckManagement : FormParamManagementBase
    {
        public FormDailyCheckManagement()
            : base(ParamForm.DailyCheckManagement)
        {
            InitializeComponent();

            base.btnAdd.Click += btnAdd_Click;
            base.btnModify.Click += btnModify_Click;
            base.btnDelete.Click += btnDelete_Click;
            base.btnSave.Click += btnSave_Click;

            dgvData.DataSourceChanged += new EventHandler(dgvData_DataSourceChanged);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                FormDailyCheckManagementEdit frmAdd = new FormDailyCheckManagementEdit(null);
                frmAdd.StartPosition = FormStartPosition.CenterScreen;
                frmAdd.ShowDialog();
                frmAdd.Dispose();

                GetGridViewData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnModify_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvData.SelectedRows.Count != 1) return;

                SBRM_DAILYCHECKDATA objSelModify = (dgvData.SelectedRows[0].DataBoundItem) as SBRM_DAILYCHECKDATA;
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                SBRM_DAILYCHECKDATA objDailyCheck = null;

                if (objSelModify.OBJECTKEY == 0)
                {
                    // 修改的是尚未Submit的新增
                    var objAddModify = (from chk in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_DAILYCHECKDATA>()
                                        where chk.LINEID == objSelModify.LINEID && chk.LINETYPE == objSelModify.LINETYPE &&
                                        chk.NODENO == objSelModify.NODENO && chk.PARAMETERNAME == objSelModify.PARAMETERNAME
                                        select chk).FirstOrDefault();
                    if (objAddModify != null)
                    {
                        objDailyCheck = objAddModify;
                    }
                    else return;
                }
                else
                {
                    objDailyCheck = objSelModify;
                }

                FormDailyCheckManagementEdit frmModify = new FormDailyCheckManagementEdit(objDailyCheck);
                frmModify.StartPosition = FormStartPosition.CenterScreen;
                frmModify.ShowDialog();
                frmModify.Dispose();

                GetGridViewData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvData.SelectedRows.Count == 0)
                    return;

                DialogResult result = this.QuectionMessage(this, this.lblCaption.Text, "Are you sure to delete selected records?");
                if (result == System.Windows.Forms.DialogResult.No)
                    return;

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                SBRM_DAILYCHECKDATA objEach = null, objToDelete = null;
                foreach (DataGridViewRow selectedRow in dgvData.SelectedRows)
                {
                    objEach = selectedRow.DataBoundItem as SBRM_DAILYCHECKDATA;
                    if (objEach.OBJECTKEY > 0)
                    {
                        ctxBRM.SBRM_DAILYCHECKDATA.DeleteOnSubmit(objEach);
                    }
                    else
                    {
                        objToDelete = (from apc in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_DAILYCHECKDATA>()
                                       where apc.LINEID == objEach.LINEID && apc.LINETYPE == objEach.LINETYPE &&
                                       apc.NODENO == objEach.NODENO && apc.PARAMETERNAME == objEach.PARAMETERNAME
                                       select apc).FirstOrDefault();

                        if (objToDelete != null)
                            ctxBRM.SBRM_DAILYCHECKDATA.DeleteOnSubmit(objToDelete);
                    }
                }

                GetGridViewData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void dgvData_DataSourceChanged(object sender, EventArgs e)
        {
            try
            {
                if (dgvData.DataSource == null || dgvData.Rows.Count == 0) return;

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                ctxBRM.GetChangeSet();
                foreach (DataGridViewRow dr in dgvData.Rows)
                {
                    SBRM_DAILYCHECKDATA obj = dr.DataBoundItem as SBRM_DAILYCHECKDATA;

                    if (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_DAILYCHECKDATA>().Any(
                        msg => msg.LINEID == obj.LINEID && msg.LINETYPE == obj.LINETYPE &&
                        msg.NODENO == obj.NODENO && msg.PARAMETERNAME == obj.PARAMETERNAME))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(214, 233, 177);
                    }
                    else if (ctxBRM.GetChangeSet().Deletes.OfType<SBRM_DAILYCHECKDATA>().Any(
                        msg => msg.LINEID == obj.LINEID && msg.LINETYPE == obj.LINETYPE &&
                        msg.NODENO == obj.NODENO && msg.PARAMETERNAME == obj.PARAMETERNAME))
                    {
                        dr.DefaultCellStyle.BackColor = Color.Red;
                    }
                    else if (ctxBRM.GetChangeSet().Updates.OfType<SBRM_DAILYCHECKDATA>().Any(
                        msg => msg.LINEID == obj.LINEID && msg.LINETYPE == obj.LINETYPE &&
                        msg.NODENO == obj.NODENO && msg.PARAMETERNAME == obj.PARAMETERNAME))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 195);
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        protected override void GetGridViewData(bool showMessage = false)
        {
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                //資料庫資料
                var selData = from msg in ctxBRM.SBRM_DAILYCHECKDATA
                              where msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName &&
                              msg.NODENO == CurNodeNo
                              select msg;

                //已修改未更新物件
                var addData = ctxBRM.GetChangeSet().Inserts.OfType<SBRM_DAILYCHECKDATA>().Where(
                    msg => msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName);

                List<SBRM_DAILYCHECKDATA> objTables = selData.ToList();

                objTables.AddRange(addData.ToList());

                dgvData.AutoGenerateColumns = false;
                dgvData.DataSource = objTables;

                //取得NodeID更新
                foreach (DataGridViewRow dr in dgvData.Rows)
                {
                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(dr.Cells["colLocalNo"].Value.ToString()))
                        dr.Cells["colLocalID"].Value = FormMainMDI.G_OPIAp.Dic_Node[dr.Cells["colLocalNo"].Value.ToString()].NodeID.ToString();
                }

                if (objTables.Count == 0 && showMessage == true)
                    ShowMessage(this, lblCaption.Text , "", "No matching data for your query！", MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        protected override void Save()
        {
            UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

            try
            {
                string _sqlDesc = string.Empty;

                string _sqlErr = string.Empty;

                try
                {
                    #region 取得更新的data

                    //新增
                    foreach (object objToInsert in _ctxBRM.GetChangeSet().Inserts)
                    {
                        if (objToInsert.GetType().Name != "SBRM_DAILYCHECKDATA") continue;

                        SBRM_DAILYCHECKDATA _updateData = (SBRM_DAILYCHECKDATA)objToInsert;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("ADD [ {0} , {1} , {2} ] ", _updateData.NODENO, _updateData.SVID, _updateData.PARAMETERNAME);
                    }

                    //delete
                    foreach (object objToDelete in _ctxBRM.GetChangeSet().Deletes)
                    {
                        if (objToDelete.GetType().Name != "SBRM_DAILYCHECKDATA") continue;

                        SBRM_DAILYCHECKDATA _updateData = (SBRM_DAILYCHECKDATA)objToDelete;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("DEL [ {0} , {1} , {2} ] ", _updateData.NODENO, _updateData.SVID, _updateData.PARAMETERNAME);
                    }

                    //modify
                    foreach (object objToUpdate in _ctxBRM.GetChangeSet().Updates)
                    {
                        if (objToUpdate.GetType().Name != "SBRM_DAILYCHECKDATA") continue;

                        SBRM_DAILYCHECKDATA _updateData = (SBRM_DAILYCHECKDATA)objToUpdate;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [ {0} , {1} , {2} ] ", _updateData.NODENO, _updateData.SVID, _updateData.PARAMETERNAME);
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
                string _err = UniTools.InsertOPIHistory_DB("SBRM_DAILYCHECKDATA", _sqlDesc, _sqlErr);

                if (_err != string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _err, MessageBoxIcon.Error);
                }
                #endregion

                Public.SendDatabaseReloadRequest("SBRM_DAILYCHECKDATA");

                if (_sqlErr == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Daily Check Data Save Success！", MessageBoxIcon.Information);
                }

                GetGridViewData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
    }
}
