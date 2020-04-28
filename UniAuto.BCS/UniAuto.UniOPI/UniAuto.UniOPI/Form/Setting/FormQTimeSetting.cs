using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormQTimeSetting : FormBase
    {
        public FormQTimeSetting()
        {
            InitializeComponent();
            lblCaption.Text = "Q Time Setting";
        }

        private void FormQTimeSetting_Load(object sender, EventArgs e)
        {
            this.GetGridViewData();

            if (OPIConst.LstLineType_Photo.Contains(FormMainMDI.G_OPIAp.CurLine.LineType))
                colCFRWQTime.Visible = true;
            else colCFRWQTime.Visible = false;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                FormQTimeSettingEdit frmAdd = new FormQTimeSettingEdit(null);
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
                if (this.dgvData.SelectedRows.Count != 1)
                    return;

                SBRM_QTIME_DEF objSelModify = (dgvData.SelectedRows[0].DataBoundItem) as SBRM_QTIME_DEF;
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                SBRM_QTIME_DEF objQtime = null;

                if (objSelModify.OBJECTKEY == 0)
                {
                    // 修改的是尚未Submit的新增
                    var objAddModify = (from qtime in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_QTIME_DEF>()
                                        where qtime.LINEID == objSelModify.LINEID && qtime.QTIMEID == objSelModify.QTIMEID
                                        select qtime).FirstOrDefault();
                    if (objAddModify != null)
                    {
                        objQtime = objAddModify;
                    }
                    else return;
                }
                else
                {
                    objQtime = objSelModify;
                }

                FormQTimeSettingEdit frmModify = new FormQTimeSettingEdit(objQtime);
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

            if (dgvData.SelectedRows.Count == 0)
                return;

            if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, "Are you sure to delete selected records?")) return;
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                SBRM_QTIME_DEF objEach = null, objToDelete = null;
                foreach (DataGridViewRow selectedRow in dgvData.SelectedRows)
                {
                    objEach = selectedRow.DataBoundItem as SBRM_QTIME_DEF;
                    if (objEach.OBJECTKEY > 0)
                    {
                        ctxBRM.SBRM_QTIME_DEF.DeleteOnSubmit(objEach);
                    }
                    else
                    {
                        objToDelete = (from qtime in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_QTIME_DEF>()
                                       where qtime.LINEID == objEach.LINEID && qtime.QTIMEID == objEach.QTIMEID
                                       select qtime).FirstOrDefault();

                        if (objToDelete != null)
                            ctxBRM.SBRM_QTIME_DEF.DeleteOnSubmit(objToDelete);
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

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            CheckChangeSave();
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
                    SBRM_QTIME_DEF obj = dr.DataBoundItem as SBRM_QTIME_DEF;

                    if (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_QTIME_DEF>().Any(
                        msg => msg.LINEID == obj.LINEID && msg.QTIMEID == obj.QTIMEID))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(214, 233, 177);
                    }
                    else if (ctxBRM.GetChangeSet().Deletes.OfType<SBRM_QTIME_DEF>().Any(
                        msg => msg.LINEID == obj.LINEID && msg.QTIMEID == obj.QTIMEID))
                    {
                        dr.DefaultCellStyle.BackColor = Color.Red;
                    }
                    else if (ctxBRM.GetChangeSet().Updates.OfType<SBRM_QTIME_DEF>().Any(
                        msg => msg.LINEID == obj.LINEID && msg.QTIMEID == obj.QTIMEID))
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

        private void GetGridViewData()
        {
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                //資料庫資料
                var selData = from msg in ctxBRM.SBRM_QTIME_DEF
                              where msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName
                              select msg;

                //已修改未更新物件
                var addData = ctxBRM.GetChangeSet().Inserts.OfType<SBRM_QTIME_DEF>().Where(
                    msg => msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName);

                List<SBRM_QTIME_DEF> objTables = selData.ToList();
                objTables.AddRange(addData.ToList());

                dgvData.AutoGenerateColumns = false;
                dgvData.DataSource = objTables;

                if (FormMainMDI.G_OPIAp.CurLine.FabType == "CF")
                    colCFRWQTime.Visible = true;
                else
                    colCFRWQTime.Visible = false;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void ClearModify()
        {
            FormMainMDI.G_OPIAp.RefreshDBBRMCtx();        
        }

        private IEnumerable<dynamic> SetStatus(IEnumerable<dynamic> data, string strStatus)
        {
            return (from msg in data
                    select new
                    {
                        msg.QTIMEID,
                        msg.STARTNODENO,
                        msg.STARTNODEID,
                        msg.STARTUNITNO,
                        msg.STARTNUNITID,
                        msg.STARTEVENTMSG,
                        msg.ENDNODENO,
                        msg.ENDNODEID,
                        msg.ENDUNITNO,
                        msg.ENDNUNITID,
                        msg.ENDEVENTMSG,
                        msg.SETTIMEVALUE,
                        STATUS = strStatus
                    }).ToList();
        }

        private void Save()
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
                        if (objToInsert.GetType().Name != "SBRM_QTIME_DEF") continue;

                        SBRM_QTIME_DEF _updateData = (SBRM_QTIME_DEF)objToInsert;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("ADD [ {0} , {1} , {2} ] ", _updateData.QTIMEID, _updateData.STARTNODENO, _updateData.ENDNODENO);
                    }

                    //delete
                    foreach (object objToDelete in _ctxBRM.GetChangeSet().Deletes)
                    {
                        if (objToDelete.GetType().Name != "SBRM_QTIME_DEF") continue;

                        SBRM_QTIME_DEF _updateData = (SBRM_QTIME_DEF)objToDelete;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("DEL [ {0} , {1} , {2} ] ", _updateData.QTIMEID, _updateData.STARTNODENO, _updateData.ENDNODENO);
                    }

                    //modify
                    foreach (object objToUpdate in _ctxBRM.GetChangeSet().Updates)
                    {
                        if (objToUpdate.GetType().Name != "SBRM_QTIME_DEF") continue;

                        SBRM_QTIME_DEF _updateData = (SBRM_QTIME_DEF)objToUpdate;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [ {0} , {1} , {2} ] ", _updateData.QTIMEID, _updateData.STARTNODENO, _updateData.ENDNODENO);
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
                string _err = UniTools.InsertOPIHistory_DB("SBRM_QTIME_DEF", _sqlDesc, _sqlErr);

                if (_err != string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _err, MessageBoxIcon.Error);
                }
                #endregion


                Public.SendDatabaseReloadRequest("SBRM_QTIME_DEF");

                if (_sqlErr == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Q Time Save Success！", MessageBoxIcon.Information);
                }

                GetGridViewData();
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

                GetGridViewData();

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
