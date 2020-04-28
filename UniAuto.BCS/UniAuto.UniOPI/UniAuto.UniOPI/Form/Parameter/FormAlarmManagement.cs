using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormAlarmManagement : FormBase
    {
        string CurNodeNo = string.Empty;
        string CurUnitNo = string.Empty;

        public FormAlarmManagement()
        {
            InitializeComponent();
            lblCaption.Text = "Alarm Management";
        }

        private void FormAlarmManagement_Load(object sender, EventArgs e)
        {
            this.InitialCombox();
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            CheckChangeSave();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                FormAlarmManagementEdit frmAdd = new FormAlarmManagementEdit(null);
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

                SBRM_ALARM objSelModify = (dgvData.SelectedRows[0].DataBoundItem) as SBRM_ALARM;
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                SBRM_ALARM objAlarm = null;

                if (objSelModify.OBJECTKEY == 0)
                {
                    // 修改的是尚未Submit的新增
                    var objAddModify = (from alm in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ALARM>()
                                        where alm.LINEID == objSelModify.LINEID && alm.NODENO == objSelModify.NODENO &&
                                        alm.UNITNO == objSelModify.UNITNO && alm.ALARMLEVEL == objSelModify.ALARMLEVEL  && alm.ALARMID == objSelModify.ALARMID
                                        select alm).FirstOrDefault();
                    if (objAddModify != null)
                    {
                        objAlarm = objAddModify;
                    }
                    else return;
                }
                else
                {
                    objAlarm = objSelModify;
                }

                FormAlarmManagementEdit frmModify = new FormAlarmManagementEdit(objAlarm);
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

                DialogResult result = this.QuectionMessage(this, this.lblCaption.Text,  "Are you sure to delete selected records?");
                if (result == System.Windows.Forms.DialogResult.No)
                    return;

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                SBRM_ALARM objEach = null, objToDelete = null;
                foreach (DataGridViewRow selectedRow in dgvData.SelectedRows)
                {
                    objEach = selectedRow.DataBoundItem as SBRM_ALARM;
                    if (objEach.OBJECTKEY > 0)
                    {
                        ctxBRM.SBRM_ALARM.DeleteOnSubmit(objEach);
                    }
                    else
                    {
                        objToDelete = (from alm in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ALARM>()
                                       where alm.LINEID == objEach.LINEID && alm.NODENO == objEach.NODENO &&
                                       alm.UNITNO == objEach.UNITNO && alm.ALARMID == objEach.ALARMID
                                       select alm).FirstOrDefault();

                        if (objToDelete != null)
                            ctxBRM.SBRM_ALARM.DeleteOnSubmit(objToDelete);
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

        private void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                var dtExport = dgvData.DataSource as List<SBRM_ALARM>;
                // DataGridView 沒資料不執行匯出動作
                if (dtExport == null || dtExport.Count == 0)
                    return;

                string message = string.Empty;
                if (UniTools.ExportToExcel("", dgvData, 0, out message))
                {
                    ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", "Export Success！", MessageBoxIcon.Information);
                }
                else
                {
                    if (!"".Equals(message))
                    {
                        ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", message, MessageBoxIcon.Error);
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

        private void dgvData_DataSourceChanged(object sender, EventArgs e)
        {
            try
            {
                if (dgvData.DataSource == null || dgvData.Rows.Count == 0) return;

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                ctxBRM.GetChangeSet();
                foreach (DataGridViewRow dr in dgvData.Rows)
                {
                    SBRM_ALARM obj = dr.DataBoundItem as SBRM_ALARM;

                    if (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ALARM>().Any(
                        msg => msg.LINEID == obj.LINEID && msg.NODENO == obj.NODENO &&
                        msg.UNITNO == obj.UNITNO && msg.ALARMLEVEL == obj.ALARMLEVEL && msg.ALARMID == obj.ALARMID))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(214, 233, 177);
                    }
                    else if (ctxBRM.GetChangeSet().Deletes.OfType<SBRM_ALARM>().Any(
                        msg => msg.LINEID == obj.LINEID && msg.NODENO == obj.NODENO &&
                        msg.UNITNO == obj.UNITNO && msg.ALARMLEVEL == obj.ALARMLEVEL && msg.ALARMID == obj.ALARMID))
                    {
                        dr.DefaultCellStyle.BackColor = Color.Red;
                    }
                    else if (ctxBRM.GetChangeSet().Updates.OfType<SBRM_ALARM>().Any(
                        msg => msg.LINEID == obj.LINEID && msg.NODENO == obj.NODENO &&
                        msg.UNITNO == obj.UNITNO && msg.ALARMLEVEL == obj.ALARMLEVEL && msg.ALARMID == obj.ALARMID))
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

        private void cmbNode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                CurUnitNo = string.Empty;

                if (cmbNode.SelectedIndex < 0 || cmbNode.SelectedValue==null)
                {
                    CurNodeNo = string.Empty;
                    
                    return;
                }

                CurNodeNo = ((dynamic)cmbNode.SelectedItem).NodeNo;

                var q = (from unit in FormMainMDI.G_OPIAp.Dic_Unit.Values
                         where unit.NodeNo == CurNodeNo
                         select unit
                    ).ToList();


                q.Insert(0, new UniOPI.Unit()
                {
                    UnitID = "",
                    UnitNo = "0",
                    UnitType = ""
                });

                cmbUnit.DataSource = q;
                cmbUnit.DisplayMember = "UNITID";
                cmbUnit.ValueMember = "UNITNO";
                cmbUnit.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cmbUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbUnit.SelectedIndex < 0 || cmbUnit.SelectedValue == null)
                {
                    CurUnitNo = string.Empty;
                    return;
                }

                CurUnitNo = ((dynamic)cmbUnit.SelectedItem).UnitNo;

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvData_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == 4) // Alarm Level
                {
                    // A: Alarm
                    // W: Warning
                    if ("A".Equals(e.Value))
                        e.Value = "Alarm";
                    else if ("W".Equals(e.Value))
                        e.Value = "Warning";
                    else
                    { }
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
                var selData = from alm in ctxBRM.SBRM_ALARM
                              where alm.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName && 
                              alm.NODENO == CurNodeNo && alm.UNITNO == CurUnitNo 
                              select alm;

                //已修改未更新物件
                var addData = ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ALARM>().Where(
                    alm => alm.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName);

                List<SBRM_ALARM> objTables = selData.ToList();

                objTables.AddRange(addData.ToList());

                dgvData.AutoGenerateColumns = false;
                dgvData.DataSource = objTables;

                //取得NodeID更新
                foreach (DataGridViewRow dr in dgvData.Rows)
                {
                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(dr.Cells[colLocalNo.Name].Value.ToString()))
                        dr.Cells[colLocalID.Name].Value = FormMainMDI.G_OPIAp.Dic_Node[dr.Cells[colLocalNo.Name].Value.ToString()].NodeID.ToString();
                }

                //if (objTables.Count == 0 && showMessage == true)
                //    ShowMessage(this, lblCaption.Text , "", "No matching data for your query！", MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void InitialCombox()
        {
            try
            {
                var q = (from node in FormMainMDI.G_OPIAp.Dic_Node.Values
                         select new
                         {
                             IDNAME = string.Format("{0}-{1}-{2}", node.NodeNo, node.NodeID, node.NodeName),
                             node.NodeNo,
                             node.NodeID
                         }).ToList();

                if (q == null || q.Count == 0)
                    return;

                cmbNode.SelectedIndexChanged -= cmbNode_SelectedIndexChanged;
                cmbNode.DataSource = q;
                cmbNode.DisplayMember = "IDNAME";
                cmbNode.ValueMember = "NODENO";
                cmbNode.SelectedIndex = -1;
                cmbNode.SelectedIndexChanged += cmbNode_SelectedIndexChanged;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private bool CheckData(UniOPI.SBRM_ALARM objAlarm, List<Unit> lstUnit)
        {
            try
            {
                string errorMsg = string.Empty;
                if (lstUnit.Select(x => x.UnitNo == objAlarm.UNITNO) == null)
                {
                    errorMsg = string.Format("AlarmUnit [{0}] not exists on Node [{1}]！", objAlarm.UNITNO, objAlarm.NODENO);

                    ShowMessage(this, lblCaption.Text, "", errorMsg, MessageBoxIcon.Error);

                    return false;
                }

                if (!"A".Equals(objAlarm.ALARMLEVEL) && !"W".Equals(objAlarm.ALARMLEVEL))
                {
                    errorMsg = string.Format("AlarmType [{0}] not in ['A', 'W']！", objAlarm.ALARMLEVEL);
                    ShowMessage(this, lblCaption.Text, "", errorMsg, MessageBoxIcon.Error);
                    return false;
                }

                int alarmID = 0;
                if (int.TryParse(objAlarm.ALARMID, out alarmID) == false ||
                    (alarmID < 1 || alarmID > 65500))
                {
                    errorMsg = string.Format("AlarmID must between 1 and 65500！");
                    //NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, errorMsg);
                    ShowMessage(this, lblCaption.Text, "", errorMsg, MessageBoxIcon.Error);
                    return false;
                }

                if (objAlarm.ALARMTEXT.Length > 80)
                {
                    errorMsg = string.Format("Content length of AlarmText must less than or equal to 80");
                    //NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, errorMsg);
                    ShowMessage(this, lblCaption.Text, "", errorMsg, MessageBoxIcon.Error);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                return false;
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
                        if (objToInsert.GetType().Name != "SBRM_ALARM") continue;

                        SBRM_ALARM _updateData = (SBRM_ALARM)objToInsert;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("ADD [ {0} , {1} , {2} ] ", _updateData.NODENO, _updateData.ALARMLEVEL, _updateData.ALARMID);
                    }

                    //delete
                    foreach (object objToDelete in _ctxBRM.GetChangeSet().Deletes)
                    {
                        if (objToDelete.GetType().Name != "SBRM_ALARM") continue;

                        SBRM_ALARM _updateData = (SBRM_ALARM)objToDelete;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("DEL [ {0} , {1} , {2} ] ", _updateData.NODENO, _updateData.ALARMLEVEL, _updateData.ALARMID);
                    }

                    //modify
                    foreach (object objToUpdate in _ctxBRM.GetChangeSet().Updates)
                    {
                        if (objToUpdate.GetType().Name != "SBRM_ALARM") continue;

                        SBRM_ALARM _updateData = (SBRM_ALARM)objToUpdate;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [ {0} , {1} , {2} ] ", _updateData.NODENO, _updateData.ALARMLEVEL, _updateData.ALARMID);
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
                string _err = UniTools.InsertOPIHistory_DB("SBRM_ALARM", _sqlDesc, _sqlErr);

                if (_err != string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _err, MessageBoxIcon.Error);
                }
                #endregion

                Public.SendDatabaseReloadRequest("SBRM_ALARM");

                if (_sqlErr == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Alarm Data Save Success！", MessageBoxIcon.Information);
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
