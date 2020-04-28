using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormAPCDownloadManagement : FormParamManagementBase
    {
        public FormAPCDownloadManagement()
            : base(ParamForm.APCDownlaodManagement)
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
                FormAPCDownloadManagementEdit frmAdd = new FormAPCDownloadManagementEdit(null);
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

                SBRM_APCDATADOWNLOAD _objSelModify = (dgvData.SelectedRows[0].DataBoundItem) as SBRM_APCDATADOWNLOAD;
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                SBRM_APCDATADOWNLOAD objApcDownload = null;

                if (_objSelModify.OBJECTKEY == 0)
                {
                    // 修改的是尚未Submit的新增
                    var objAddModify = (from apc in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_APCDATADOWNLOAD>()
                                        where apc.LINEID == _objSelModify.LINEID && apc.LINETYPE == _objSelModify.LINETYPE &&
                                        apc.NODENO == _objSelModify.NODENO && apc.PARAMETERNAME == _objSelModify.PARAMETERNAME
                                        select apc).FirstOrDefault();
                    if (objAddModify != null)
                    {
                        objApcDownload = objAddModify;
                    }
                    else return;
                }
                else
                {
                    objApcDownload = _objSelModify;
                }

                FormAPCDownloadManagementEdit frmModify = new FormAPCDownloadManagementEdit(objApcDownload);
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
                if (dgvData.SelectedRows.Count == 0)  return;

                DialogResult result = this.QuectionMessage(this, this.lblCaption.Text, "Are you sure to delete selected records?");
                if (result == System.Windows.Forms.DialogResult.No)
                    return;
                
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                SBRM_APCDATADOWNLOAD objEach = null, objToDelete = null;

                foreach (DataGridViewRow selectedRow in dgvData.SelectedRows)
                {
                    objEach = selectedRow.DataBoundItem as SBRM_APCDATADOWNLOAD;
                    if (objEach.OBJECTKEY > 0)
                    {
                        ctxBRM.SBRM_APCDATADOWNLOAD.DeleteOnSubmit(objEach);
                    }
                    else
                    {
                        objToDelete = (from apc in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_APCDATADOWNLOAD>()
                                       where apc.LINEID == objEach.LINEID && apc.LINETYPE == objEach.LINETYPE &&
                                       apc.NODENO == objEach.NODENO && apc.PARAMETERNAME == objEach.PARAMETERNAME
                                       select apc).FirstOrDefault();

                        if (objToDelete != null)
                            ctxBRM.SBRM_APCDATADOWNLOAD.DeleteOnSubmit(objToDelete);
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
                    SBRM_APCDATADOWNLOAD obj = dr.DataBoundItem as SBRM_APCDATADOWNLOAD;

                    if (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_APCDATADOWNLOAD>().Any(
                        msg => msg.LINEID == obj.LINEID && msg.LINETYPE == obj.LINETYPE &&
                        msg.NODENO == obj.NODENO && msg.PARAMETERNAME == obj.PARAMETERNAME))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(214, 233, 177);
                    }
                    else if (ctxBRM.GetChangeSet().Deletes.OfType<SBRM_APCDATADOWNLOAD>().Any(
                        msg => msg.LINEID == obj.LINEID && msg.LINETYPE == obj.LINETYPE &&
                        msg.NODENO == obj.NODENO && msg.PARAMETERNAME == obj.PARAMETERNAME))
                    {
                        dr.DefaultCellStyle.BackColor = Color.Red;
                    }
                    else if (ctxBRM.GetChangeSet().Updates.OfType<SBRM_APCDATADOWNLOAD>().Any(
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
                var selData = from msg in ctxBRM.SBRM_APCDATADOWNLOAD
                              where msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName &&
                              msg.NODENO == CurNodeNo
                              select msg;

                //已修改未更新物件
                var addData = ctxBRM.GetChangeSet().Inserts.OfType<SBRM_APCDATADOWNLOAD>().Where(
                    msg => msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName);

                List<SBRM_APCDATADOWNLOAD> objTables = selData.ToList();

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
                        if (objToInsert.GetType().Name != "SBRM_APCDATADOWNLOAD") continue;

                        SBRM_APCDATADOWNLOAD _updateData = (SBRM_APCDATADOWNLOAD)objToInsert;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("ADD [ {0} , {1} , {2} ] ", _updateData.NODENO, _updateData.SVID, _updateData.PARAMETERNAME);
                    }

                    //delete
                    foreach (object objToDelete in _ctxBRM.GetChangeSet().Deletes)
                    {
                        if (objToDelete.GetType().Name != "SBRM_APCDATADOWNLOAD") continue;

                        SBRM_APCDATADOWNLOAD _updateData = (SBRM_APCDATADOWNLOAD)objToDelete;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("DEL [ {0} , {1} , {2} ] ", _updateData.NODENO, _updateData.SVID, _updateData.PARAMETERNAME);
                    }

                    //modify
                    foreach (object objToUpdate in _ctxBRM.GetChangeSet().Updates)
                    {
                        if (objToUpdate.GetType().Name != "SBRM_APCDATADOWNLOAD") continue;

                        SBRM_APCDATADOWNLOAD _updateData = (SBRM_APCDATADOWNLOAD)objToUpdate;

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
                string _err = UniTools.InsertOPIHistory_DB("SBRM_APCDATADOWNLOAD", _sqlDesc, _sqlErr);

                if (_err != string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _err, MessageBoxIcon.Error);
                }
                #endregion

                Public.SendDatabaseReloadRequest("SBRM_APCDATADOWNLOAD");

                if (_sqlErr == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "APC Data Download Save Success！", MessageBoxIcon.Information);
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
