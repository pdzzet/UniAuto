using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormLineStatusRule : FormBase
    {
        public FormLineStatusRule()
        {
            InitializeComponent();
            lblCaption.Text = "Line Status Rule";
        }

        #region Events
        private void FormLineStatusRule_Load(object sender, EventArgs e)
        {
            this.InitialCombox();
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            CheckChangeSave();
            //ClearModify();
            //this.GetGridViewData();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                FormLineStatusRuleEdit frmAdd = new FormLineStatusRuleEdit(null);
                frmAdd.StartPosition = FormStartPosition.CenterScreen;
                frmAdd.ShowDialog();
                if (frmAdd != null) frmAdd.Dispose();

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
                if (dgvData.SelectedRows.Count != 1)
                    return;

                SBRM_LINESTATUSSPEC objSelModify = (dgvData.SelectedRows[0].DataBoundItem) as SBRM_LINESTATUSSPEC;
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                SBRM_LINESTATUSSPEC objLineStatusRule = null;

                if (objSelModify.OBJECTKEY == 0)
                {
                    // 修改的是尚未Submit的新增
                    var objAddModify = (from lsRule in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_LINESTATUSSPEC>()
                                        where lsRule.LINETYPE == objSelModify.LINETYPE && lsRule.CONDITIONSEQNO == objSelModify.CONDITIONSEQNO
                                        select lsRule).FirstOrDefault();
                    if (objAddModify != null)
                    {
                        objLineStatusRule = objAddModify;
                    }
                    else return;
                }
                else
                {
                    objLineStatusRule = objSelModify;
                }

                FormLineStatusRuleEdit frmModify = new FormLineStatusRuleEdit(objLineStatusRule);
                frmModify.StartPosition = FormStartPosition.CenterScreen;
                frmModify.ShowDialog();
                if (frmModify != null)  frmModify.Dispose();
                 
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

            if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, "Are you sure to delete selected records?"))
                return;

            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                SBRM_LINESTATUSSPEC objEach = null, objToDelete = null;
                foreach (DataGridViewRow selectedRow in dgvData.SelectedRows)
                {
                    objEach = selectedRow.DataBoundItem as SBRM_LINESTATUSSPEC;
                    if (objEach.OBJECTKEY > 0)
                    {
                        ctxBRM.SBRM_LINESTATUSSPEC.DeleteOnSubmit(objEach);
                    }
                    else
                    {
                        objToDelete = (from lsRule in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_LINESTATUSSPEC>()
                                       where lsRule.LINETYPE == objEach.LINETYPE && lsRule.CONDITIONSEQNO == objEach.CONDITIONSEQNO
                                       select lsRule).FirstOrDefault();

                        if (objToDelete != null)
                            ctxBRM.SBRM_LINESTATUSSPEC.DeleteOnSubmit(objToDelete);
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
            //ClearModify();
            //this.GetGridViewData();
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
                    SBRM_LINESTATUSSPEC obj = dr.DataBoundItem as SBRM_LINESTATUSSPEC;

                    if (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_LINESTATUSSPEC>().Any(
                        msg => msg.LINETYPE == obj.LINETYPE && msg.CONDITIONSEQNO == obj.CONDITIONSEQNO))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(214, 233, 177);
                    }
                    else if (ctxBRM.GetChangeSet().Deletes.OfType<SBRM_LINESTATUSSPEC>().Any(
                        msg => msg.LINETYPE == obj.LINETYPE && msg.CONDITIONSEQNO == obj.CONDITIONSEQNO))
                    {
                        dr.DefaultCellStyle.BackColor = Color.Red;
                    }
                    else if (ctxBRM.GetChangeSet().Updates.OfType<SBRM_LINESTATUSSPEC>().Any(
                        msg => msg.LINETYPE == obj.LINETYPE && msg.CONDITIONSEQNO == obj.CONDITIONSEQNO))
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
        #endregion

        private void GetGridViewData()
        {
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                string _status = string.Empty;

                List<SBRM_LINESTATUSSPEC> objTables = new List<SBRM_LINESTATUSSPEC>();

                if (cmbConditionStatus.SelectedIndex > 0)
                {
                    _status = cmbConditionStatus.SelectedValue.ToString();

                    //資料庫資料
                    var selData = from msg in ctxBRM.SBRM_LINESTATUSSPEC
                                  where msg.LINETYPE.Equals(FormMainMDI.G_OPIAp.CurLine.LineType) && msg.CONDITIONSTATUS.Equals(_status)
                                  select msg;

                    objTables = selData.ToList();
                }
                else if (cmbConditionStatus.SelectedIndex == 0)
                {
                    //資料庫資料
                    var selData = from msg in ctxBRM.SBRM_LINESTATUSSPEC
                                  where msg.LINETYPE.Equals(FormMainMDI.G_OPIAp.CurLine.LineType) 
                                  select msg;

                    objTables = selData.ToList();
                }

                //已修改未更新物件
                var addData = ctxBRM.GetChangeSet().Inserts.OfType<SBRM_LINESTATUSSPEC>();
                
                objTables.AddRange(addData.ToList());

                dgvData.AutoGenerateColumns = false;
                dgvData.DataSource = objTables;

                //取得NodeID更新
                string[] tmp;
                ArrayList aryValue = new ArrayList();
                foreach (DataGridViewRow dr in dgvData.Rows)
                {
                    tmp = dr.Cells[colLocalNoList.Name].Value.ToString().Split(',');
                    aryValue.Clear();
                    foreach (string strNodeNo in tmp)
                    {
                        if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(strNodeNo))
                        {
                            aryValue.Add(FormMainMDI.G_OPIAp.Dic_Node[strNodeNo].NodeID);
                        }
                    }
                    dr.Cells[colLocalIDList.Name].Value = string.Join(",", aryValue.ToArray());
                }
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

        private void InitialCombox()
        {
            try
            {
                DataRow drNew = null;

                #region cmbConditionStatus
                DataTable dtStatus = UniTools.InitDt(new string[] { "STATUS" });
                drNew = dtStatus.NewRow();
                drNew["STATUS"] = "";
                dtStatus.Rows.Add(drNew);
                drNew = dtStatus.NewRow();
                drNew["STATUS"] = "RUN";
                dtStatus.Rows.Add(drNew);
                drNew = dtStatus.NewRow();
                drNew["STATUS"] = "DOWN";
                dtStatus.Rows.Add(drNew);
                drNew = dtStatus.NewRow();
                drNew["STATUS"] = "IDLE";
                dtStatus.Rows.Add(drNew);
                drNew = dtStatus.NewRow();
                drNew["STATUS"] = "EQALIVEDOWN";
                dtStatus.Rows.Add(drNew);

                cmbConditionStatus.DataSource = dtStatus;
                cmbConditionStatus.DisplayMember = "STATUS";
                cmbConditionStatus.ValueMember = "STATUS";
                cmbConditionStatus.SelectedIndex = 0;
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void Save()
        {
            UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

            try
            {
                string _sqlDesc = string.Empty;

                string _sqlErr = string.Empty;

                DateTime _updatetime = System.DateTime.Now;

                try
                {
                    #region 取得更新的data

                    //新增
                    foreach (object objToInsert in _ctxBRM.GetChangeSet().Inserts)
                    {
                        if (objToInsert.GetType().Name != "SBRM_LINESTATUSSPEC") continue;

                        SBRM_LINESTATUSSPEC _updateData = (SBRM_LINESTATUSSPEC)objToInsert;

                        _updateData.UPDATETIME = _updatetime;
                        _updateData.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("ADD [ {0} , {1} , {2} ] ", _updateData.CONDITIONSTATUS, _updateData.CONDITIONSEQNO, _updateData.EQPNOLIST);
                    }

                    //delete
                    foreach (object objToDelete in _ctxBRM.GetChangeSet().Deletes)
                    {
                        if (objToDelete.GetType().Name != "SBRM_LINESTATUSSPEC") continue;

                        SBRM_LINESTATUSSPEC _updateData = (SBRM_LINESTATUSSPEC)objToDelete;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("DEL [ {0} , {1} , {2} ] ", _updateData.CONDITIONSTATUS, _updateData.CONDITIONSEQNO, _updateData.EQPNOLIST);
                    }

                    //modify
                    foreach (object objToUpdate in _ctxBRM.GetChangeSet().Updates)
                    {
                        if (objToUpdate.GetType().Name != "SBRM_LINESTATUSSPEC") continue;

                        SBRM_LINESTATUSSPEC _updateData = (SBRM_LINESTATUSSPEC)objToUpdate;

                        _updateData.UPDATETIME = _updatetime;
                        _updateData.OPERATORID = FormMainMDI.G_OPIAp.LoginUserID;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [ {0} , {1} , {2} ] ", _updateData.CONDITIONSTATUS, _updateData.CONDITIONSEQNO, _updateData.EQPNOLIST);
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
                string _err = UniTools.InsertOPIHistory_DB("SBRM_LINESTATUSSPEC", _sqlDesc, _sqlErr);

                if (_err != string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _err, MessageBoxIcon.Error);
                }
                #endregion

                Public.SendDatabaseReloadRequest("SBRM_LINESTATUSSPEC");

                if (_sqlErr == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Line Status Rule Save Success！", MessageBoxIcon.Information);
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
