using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormSkipReportSetting : FormBase
    {
        public FormSkipReportSetting()
        {
            InitializeComponent();
            lblCaption.Text = "Skip Report Setting";
        }

        private void FormSkipReportSetting_Load(object sender, EventArgs e)
        {
            this.InitialCombox();
        }


        private void btnQuery_Click(object sender, EventArgs e)
        {
            CheckChangeSave();
            //try
            //{
            //    #region Check
            //    if (chkNode.Checked && cmbNode.SelectedIndex < 0)
            //    {
            //        ShowMessage(this, lblCaption.Text, "", "Local ID required！", MessageBoxIcon.Error);
            //        return;
            //    }

            //    if (chkUnit.Checked && cmbUnit.SelectedIndex < 0)
            //    {
            //        ShowMessage(this, lblCaption.Text, "", "Unit ID required！", MessageBoxIcon.Error);
            //        return;
            //    }

            //    if (chkSkipAgent.Checked && cmbSkipAgent.SelectedIndex < 0)
            //    {
            //        ShowMessage(this, lblCaption.Text, "", "Skip Agent required！", MessageBoxIcon.Error);
            //        return;
            //    }

            //    if (chkSkipReportTrx.Checked && cmbSkipReportTrx.SelectedIndex < 0)
            //    {
            //        ShowMessage(this, lblCaption.Text, "", "Skip Report Trx required！", MessageBoxIcon.Error);
            //        return;
            //    }
            //    #endregion

            //    ClearModify();
            //    GetGridViewData();
            //}
            //catch (Exception ex)
            //{
            //    NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            //    ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            //}
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                FormSkipReportSettingEdit frmAdd = new FormSkipReportSettingEdit(null);
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

                SBRM_SKIPREPORT objSelModify = (dgvData.SelectedRows[0].DataBoundItem) as SBRM_SKIPREPORT;
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                SBRM_SKIPREPORT objSkipReport = null;

                if (objSelModify.OBJECTKEY == 0)
                {
                    // 修改的是尚未Submit的新增
                    var objAddModify = (from skiprpt in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_SKIPREPORT>()
                                        where skiprpt.LINEID == objSelModify.LINEID && skiprpt.NODENO == objSelModify.NODENO &&
                                        skiprpt.UNITNO == objSelModify.UNITNO &&
                                        skiprpt.SKIPAGENT == objSelModify.SKIPAGENT &&
                                        skiprpt.SKIPREPORTTRX == objSelModify.SKIPREPORTTRX &&
                                        skiprpt.SKIPCONDITION == objSelModify.SKIPCONDITION 
                                        select skiprpt).FirstOrDefault();


                    if (objAddModify != null)
                    {
                        objSkipReport = objAddModify;
                    }
                    else return;
                }
                else
                {
                    objSkipReport = objSelModify;
                }

                if (objSkipReport == null)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "No Data Modify", MessageBoxIcon.Warning);
                    return;
                }

                FormSkipReportSettingEdit frmModify = new FormSkipReportSettingEdit(objSkipReport);
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
                SBRM_SKIPREPORT objEach = null, objToDelete = null;
                foreach (DataGridViewRow selectedRow in dgvData.SelectedRows)
                {
                    objEach = selectedRow.DataBoundItem as SBRM_SKIPREPORT;
                    if (objEach.OBJECTKEY > 0)
                    {
                        ctxBRM.SBRM_SKIPREPORT.DeleteOnSubmit(objEach);
                    }
                    else
                    {
                        objToDelete = (from skiprpt in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_SKIPREPORT>()
                                       where skiprpt.LINEID == objEach.LINEID && skiprpt.NODENO == objEach.NODENO &&
                                       skiprpt.UNITNO == objEach.UNITNO
                                       select skiprpt).FirstOrDefault();

                        if (objToDelete != null)
                            ctxBRM.SBRM_SKIPREPORT.DeleteOnSubmit(objToDelete);
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

        private void cmbNode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbNode.SelectedIndex == -1) return;

                string strNodeID = ((dynamic)cmbNode.SelectedItem).NodeID;
                SetUnit(strNodeID);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
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
                    SBRM_SKIPREPORT obj = dr.DataBoundItem as SBRM_SKIPREPORT;

                    if (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_SKIPREPORT>().Any(
                        msg => msg.LINEID == obj.LINEID && msg.NODENO == obj.NODENO && msg.UNITNO == obj.UNITNO && msg.OBJECTKEY == obj.OBJECTKEY))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(214, 233, 177);
                    }
                    else if (ctxBRM.GetChangeSet().Deletes.OfType<SBRM_SKIPREPORT>().Any(
                        msg => msg.LINEID == obj.LINEID && msg.NODENO == obj.NODENO && msg.UNITNO == obj.UNITNO && msg.OBJECTKEY == obj.OBJECTKEY))
                    {
                        dr.DefaultCellStyle.BackColor = Color.Red;
                    }
                    else if (ctxBRM.GetChangeSet().Updates.OfType<SBRM_SKIPREPORT>().Any(
                        msg => msg.LINEID == obj.LINEID && msg.NODENO == obj.NODENO && msg.UNITNO == obj.UNITNO && msg.OBJECTKEY ==obj.OBJECTKEY))
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

                if (chkNode.Checked && cmbNode.SelectedIndex < 0) return;
                if (chkUnit.Checked && cmbUnit.SelectedIndex < 0) return;
                if (chkSkipAgent.Checked && cmbSkipAgent.SelectedIndex < 0) return;
                if (chkSkipReportTrx.Checked && cmbSkipReportTrx.SelectedIndex < 0) return;

                string strNodeID = string.Empty;
                string strUnitNo = string.Empty;
                string strSkipAgent = string.Empty;
                string strSkipReportTrx = string.Empty;
                if (this.chkNode.Checked) strNodeID = cmbNode.SelectedValue.ToString();
                if (this.chkUnit.Checked) strUnitNo = cmbUnit.SelectedValue.ToString();
                if (this.chkSkipAgent.Checked) strSkipAgent = cmbSkipAgent.SelectedValue.ToString();
                if (this.chkSkipReportTrx.Checked) strSkipReportTrx = cmbSkipReportTrx.SelectedValue.ToString();

                //資料庫資料
                var selData = from msg in ctxBRM.SBRM_SKIPREPORT
                              where msg.LINEID == FormMainMDI.G_OPIAp.CurLine.LineID
                              select msg;

                //已修改未更新物件
                var addData = ctxBRM.GetChangeSet().Inserts.OfType<SBRM_SKIPREPORT>().Where(
                    msg => msg.LINEID == FormMainMDI.G_OPIAp.CurLine.LineID);

                List<SBRM_SKIPREPORT> objTables = selData.ToList();
                objTables.AddRange(addData.ToList());
                if (this.chkNode.Checked) objTables = objTables.Where(x => x.NODEID == strNodeID).ToList();
                if (this.chkUnit.Checked) objTables = objTables.Where(x => x.UNITNO == strUnitNo).ToList();
                if (this.chkSkipAgent.Checked) objTables = objTables.Where(x => x.SKIPAGENT == strSkipAgent).ToList();
                if (this.chkSkipReportTrx.Checked) objTables = objTables.Where(x => x.SKIPREPORTTRX == strSkipReportTrx).ToList();

                dgvData.AutoGenerateColumns = false;
                dgvData.DataSource = objTables;

                //取得NodeID更新
                foreach (DataGridViewRow dr in dgvData.Rows)
                {
                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(dr.Cells[colLocalNo.Name].Value.ToString()))
                        dr.Cells[colLocalID.Name].Value = FormMainMDI.G_OPIAp.Dic_Node[dr.Cells[colLocalNo.Name].Value.ToString()].NodeID.ToString();
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void InitialCombox()
        {
            SetNode();
            SetSkipAgent();
            SetSkipReportTrx();
        }

        private void SetNode()
        {
            try
            {
                cmbNode.SelectedIndexChanged -= new EventHandler(cmbNode_SelectedIndexChanged);

                var q = (from node in FormMainMDI.G_OPIAp.Dic_Node.Values
                         select new
                         {
                             IDNAME = string.Format("{0}-{1}-{2}", node.NodeNo, node.NodeID, node.NodeName),
                             node.NodeNo,
                             node.NodeID
                         }).ToList();

                if (q == null || q.Count == 0) return;

                cmbNode.DataSource = q;
                cmbNode.DisplayMember = "IDNAME";
                cmbNode.ValueMember = "NODEID";
                cmbNode.SelectedIndex = -1;

                cmbNode.SelectedIndexChanged += new EventHandler(cmbNode_SelectedIndexChanged);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetUnit(string NodeID)
        {
            try
            {
                var q = (from unit in FormMainMDI.G_OPIAp.Dic_Unit.Values
                         where unit.NodeID == NodeID
                         select new
                         {
                             unit.UnitID,
                             unit.UnitNo,
                             IDNAME = string.Format("{0}-{1}", unit.UnitNo, unit.UnitID)
                         }
                    ).ToList();

                q.Insert(0, new
                {
                    UnitID = "",
                    UnitNo = "0",
                    IDNAME = ""
                });

                cmbUnit.DataSource = q;
                cmbUnit.DisplayMember = "IDNAME";
                cmbUnit.ValueMember = "UNITNO";
                cmbUnit.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetSkipReportTrx()
        {
            try
            {
                Dictionary<string, string> dicSkipReportTrx = new Dictionary<string, string>();
                dicSkipReportTrx.Add("PRODUCTIN", "Product In");
                dicSkipReportTrx.Add("PRODUCTOUT", "Product Out");
                dicSkipReportTrx.Add("ALARMREPORT", "Alarm Report");
                cmbSkipReportTrx.DataSource = new BindingSource(dicSkipReportTrx, "");
                cmbSkipReportTrx.DisplayMember = "Value";
                cmbSkipReportTrx.ValueMember = "Key";
                cmbSkipReportTrx.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetSkipAgent()
        {
            try
            {
                var data = new[]             
            { 
                new { Key = "OEE", Value = "OEE" } ,
                new { Key = "MES", Value = "MES" } ,
                new { Key = "EDA", Value = "EDA" }
            };
                this.cmbSkipAgent.DataSource = data;
                this.cmbSkipAgent.DisplayMember = "Value";
                this.cmbSkipAgent.ValueMember = "Key";
                this.cmbSkipAgent.SelectedIndex = -1;
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

                try
                {
                    #region 取得更新的data

                    //新增
                    foreach (object objToInsert in _ctxBRM.GetChangeSet().Inserts)
                    {
                        if (objToInsert.GetType().Name != "SBRM_SKIPREPORT") continue;

                        SBRM_SKIPREPORT _updateData = (SBRM_SKIPREPORT)objToInsert;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("ADD [ {0} , {1} , {2} ] ", _updateData.NODENO, _updateData.SKIPAGENT, _updateData.SKIPREPORTTRX);
                    }

                    //delete
                    foreach (object objToDelete in _ctxBRM.GetChangeSet().Deletes)
                    {
                        if (objToDelete.GetType().Name != "SBRM_SKIPREPORT") continue;

                        SBRM_SKIPREPORT _updateData = (SBRM_SKIPREPORT)objToDelete;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("DEL [ {0} , {1} , {2} ] ", _updateData.NODENO, _updateData.SKIPAGENT, _updateData.SKIPREPORTTRX);
                    }

                    //modify
                    foreach (object objToUpdate in _ctxBRM.GetChangeSet().Updates)
                    {
                        if (objToUpdate.GetType().Name != "SBRM_SKIPREPORT") continue;

                        SBRM_SKIPREPORT _updateData = (SBRM_SKIPREPORT)objToUpdate;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [ {0} , {1} , {2} ] ", _updateData.NODENO, _updateData.SKIPAGENT, _updateData.SKIPREPORTTRX);
                    }

                    #endregion

                    if (_sqlDesc == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, string.Empty, "No Data for Update！", MessageBoxIcon.Warning);
                        return;
                    }

                    _ctxBRM.SubmitChanges();
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
                string _err = UniTools.InsertOPIHistory_DB("SBRM_SKIPREPORT", _sqlDesc, _sqlErr);

                if (_err != string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _err, MessageBoxIcon.Error);
                }
                #endregion

                Public.SendDatabaseReloadRequest("SBRM_SKIPREPORT");

                if (_sqlErr == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Skip Report Save Success！", MessageBoxIcon.Information);
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
