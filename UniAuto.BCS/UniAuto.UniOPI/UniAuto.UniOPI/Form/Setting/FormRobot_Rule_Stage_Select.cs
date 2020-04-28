using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormRobot_Rule_Stage_Select : FormBase
    {
        string RouteID = string.Empty;
        int CurStepID = 0 ;
        Robot CurRobot = null;

        List<SBRM_ROBOT_RULE_STAGE_SELECT> objTables = null;

        List<bool> asc = new List<bool>();

        List<int> LstStepID = null;

        public FormRobot_Rule_Stage_Select()
        {
            InitializeComponent();
        }

        public FormRobot_Rule_Stage_Select(string routeID, int stepID, Robot robot, List<int> lstStepID)
        {
            InitializeComponent();

            RouteID = routeID;
            CurStepID = stepID;
            CurRobot = robot;
            LstStepID = lstStepID;
        }

        private void FormRobot_Rule_Stage_Select_Load(object sender, EventArgs e)
        {
            this.lblCaption.Text = "Robot Rult Stage Select";

            for (int i = 0; i < dgvData.Columns.Count; i++)
            {
                asc.Add(false);
            }

            txtRobotName.Text = CurRobot.RobotName;
            txtRouteID.Text = RouteID;

            InitialCombox();
        }

        private void InitialCombox()
        {
            try
            {
                if (cboStepID.Items.Count > 0)
                {
                    cboStepID.Items.Clear();
                }

                cboStepID.SelectedIndexChanged -= new EventHandler(cboStepID_SelectedIndexChanged);

                cboStepID.DataSource = LstStepID;

                cboStepID.SelectedIndex = -1;

                cboStepID.SelectedIndexChanged += new EventHandler(cboStepID_SelectedIndexChanged);

                cboStepID.SelectedItem = CurStepID;

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void GetDataGridView()
        {
            UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
            try
            {
                objTables = (from rb in _ctxBRM.SBRM_ROBOT_RULE_STAGE_SELECT
                           where rb.SERVERNAME == CurRobot.ServerName && rb.ROBOTNAME == CurRobot.RobotName && rb.ROUTEID == RouteID && rb.STEPID == CurStepID
                           select rb).ToList();

                var addData = _ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_RULE_STAGE_SELECT>();

                objTables.AddRange(addData.ToList());

                dgvData.AutoGenerateColumns = false;

                dgvData.DataSource = objTables;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cboStepID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboStepID.SelectedValue == null) return;

            CurStepID = int.Parse(cboStepID.SelectedValue.ToString());

            GetDataGridView();
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
                    SBRM_ROBOT_RULE_STAGE_SELECT obj = dr.DataBoundItem as SBRM_ROBOT_RULE_STAGE_SELECT;

                    if (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_RULE_STAGE_SELECT>().Any(
                        msg => msg.ROUTEID == obj.ROUTEID && msg.STEPID == obj.STEPID && msg.ITEMID.Equals(obj.ITEMID)))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(214, 233, 177);
                    }
                    else if (ctxBRM.GetChangeSet().Deletes.OfType<SBRM_ROBOT_RULE_STAGE_SELECT>().Any(
                        msg => msg.ROUTEID == obj.ROUTEID && msg.STEPID == obj.STEPID && msg.ITEMID.Equals(obj.ITEMID)))
                    {
                        dr.DefaultCellStyle.BackColor = Color.Red;
                    }
                    else if (ctxBRM.GetChangeSet().Updates.OfType<SBRM_ROBOT_RULE_STAGE_SELECT>().Any(
                        msg => msg.ROUTEID == obj.ROUTEID && msg.STEPID == obj.STEPID && msg.ITEMID.Equals(obj.ITEMID)))
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

        private void dgvData_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dgvData.DataSource = null;
            try
            {
                if (objTables != null)
                {
                    switch (dgvData.Columns[e.ColumnIndex].Name)
                    {

                        case "colRouteID":
                            if (asc[e.ColumnIndex] == false)
                                objTables = objTables.OrderByDescending(o => o.ROUTEID).ToList();
                            else
                                objTables = objTables.OrderBy(o => o.ROUTEID).ToList();
                            break;

                        case "colDescription":
                            if (asc[e.ColumnIndex] == false)
                                objTables = objTables.OrderByDescending(o => o.DESCRIPTION).ToList();
                            else
                                objTables = objTables.OrderBy(o => o.DESCRIPTION).ToList();
                            break;

                        case "colObjectName":
                            if (asc[e.ColumnIndex] == false)
                                objTables = objTables.OrderByDescending(o => o.OBJECTNAME).ToList();
                            else
                                objTables = objTables.OrderBy(o => o.OBJECTNAME).ToList();
                            break;

                        case "colMethodName":
                            if (asc[e.ColumnIndex] == false)
                                objTables = objTables.OrderByDescending(o => o.METHODNAME).ToList();
                            else
                                objTables = objTables.OrderBy(o => o.METHODNAME).ToList();
                            break;

                        case "colItemID":
                            if (asc[e.ColumnIndex] == false)
                                objTables = objTables.OrderByDescending(o => o.ITEMID).ToList();
                            else
                                objTables = objTables.OrderBy(o => o.ITEMID).ToList();
                            break;


                        case "colItemSeq":
                            if (asc[e.ColumnIndex] == false)
                                objTables = objTables.OrderByDescending(o => o.ITEMSEQ).ToList();
                            else
                                objTables = objTables.OrderBy(o => o.ITEMSEQ).ToList();
                            break;

                        case "colLastUpdatetime":
                            if (asc[e.ColumnIndex] == false)
                                objTables = objTables.OrderByDescending(o => o.LASTUPDATETIME).ToList();
                            else
                                objTables = objTables.OrderBy(o => o.LASTUPDATETIME).ToList();
                            break;
                    }
                    if (asc[e.ColumnIndex] == false)
                        asc[e.ColumnIndex] = true;
                    else
                        asc[e.ColumnIndex] = false;

                }
                dgvData.DataSource = objTables;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_Click(object sender, EventArgs e)
        {

            UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

            FormRobot_Rule_Stage_Select_Edit _frm = null ;

            try
            {
                Button _btn = (Button)sender;

                switch (_btn.Tag.ToString())
                {
                    case "Add":

                        #region Add
                        _frm = new FormRobot_Rule_Stage_Select_Edit(
                       RouteID, CurStepID,
                       CurRobot,
                       ""
                       );

                        if (_frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            GetDataGridView();
                        }

                        break;
                        #endregion

                    case "Modify":

                        #region Modify
                        if (dgvData.SelectedRows.Count != 1) return;
                        _frm = new FormRobot_Rule_Stage_Select_Edit(
                        this.dgvData.CurrentRow.Cells[colRouteID.Name].Value.ToString(),
                        int.Parse(this.dgvData.CurrentRow.Cells[colStepID.Name].Value.ToString()),
                        CurRobot,
                        this.dgvData.CurrentRow.Cells[colItemID.Name].Value.ToString()
                        );

                        if (_frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            GetDataGridView();
                        }

                        break;
                        #endregion
                        
                    case "Delete":

                        #region Delete
                        if (dgvData.SelectedRows.Count == 0) return;

                        DialogResult result = this.QuectionMessage(this, this.lblCaption.Text, "Are you sure to delete selected records?");

                        if (result == System.Windows.Forms.DialogResult.No)  return;

                        SBRM_ROBOT_RULE_STAGE_SELECT objEach = null, objtoDelete = null;

                        foreach (DataGridViewRow selectedRow in dgvData.SelectedRows)
                        {
                            objEach = selectedRow.DataBoundItem as SBRM_ROBOT_RULE_STAGE_SELECT;

                            if (objEach.OBJECTKEY > 0)
                            {
                                //ctxBRM.SBRM_ROBOT_RULE_STAGE_SELECT.DeleteOnSubmit(objEach);
                                objtoDelete = (from proc in ctxBRM.SBRM_ROBOT_RULE_STAGE_SELECT
                                               where proc.OBJECTKEY.Equals(objEach.OBJECTKEY) 
                                               select proc).FirstOrDefault();
                            }
                            else
                            {
                                objtoDelete = (from proc in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_RULE_STAGE_SELECT>() 
                                               where proc.OBJECTNAME.Equals(objEach.OBJECTNAME) && proc.METHODNAME.Equals(objEach.METHODNAME) 
                                               select proc).FirstOrDefault();

                            }


                            if (objtoDelete != null)
                                ctxBRM.SBRM_ROBOT_RULE_STAGE_SELECT.DeleteOnSubmit(objtoDelete);

                        }

                        this.GetDataGridView();

                        break;
                        #endregion

                    case "Save":

                        #region Save

                        try
                        {
                            string _sqlDesc = string.Empty;
                            string _sqlErr = string.Empty;

                            try
                            {
                                #region 取得更新的data
                                foreach (object objToInsert in ctxBRM.GetChangeSet().Inserts)
                                {
                                    if (objToInsert.GetType().Name != "SBRM_ROBOT_RULE_STAGE_SELECT") continue;
                                    SBRM_ROBOT_RULE_STAGE_SELECT _updateData = (SBRM_ROBOT_RULE_STAGE_SELECT)objToInsert;

                                    _updateData.LASTUPDATETIME = DateTime.Now; ;

                                    _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("ADD [{0},{1},{2},{3},{4},{5},{6}]", _updateData.ROBOTNAME, _updateData.ROUTEID, _updateData.STEPID, _updateData.ITEMID, _updateData.ITEMSEQ, _updateData.ISENABLED, _updateData.DESCRIPTION);
                                }
                                #endregion

                                #region Modify
                                foreach (object objToModify in ctxBRM.GetChangeSet().Updates)
                                {
                                    if (objToModify.GetType().Name != "SBRM_ROBOT_RULE_STAGE_SELECT") continue;
                                    SBRM_ROBOT_RULE_STAGE_SELECT _updateData = (SBRM_ROBOT_RULE_STAGE_SELECT)objToModify;

                                    _updateData.LASTUPDATETIME = DateTime.Now; ;

                                    _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("ADD [{0},{1},{2},{3},{4},{5},{6}]", _updateData.ROBOTNAME, _updateData.ROUTEID, _updateData.STEPID, _updateData.ITEMID, _updateData.ITEMSEQ, _updateData.ISENABLED, _updateData.DESCRIPTION);
                                }
                                #endregion

                                #region DELETE
                                foreach (object objToDelete in ctxBRM.GetChangeSet().Deletes)
                                {
                                    if (objToDelete.GetType().Name != "SBRM_ROBOT_RULE_STAGE_SELECT") continue;
                                    SBRM_ROBOT_RULE_STAGE_SELECT _updateData = (SBRM_ROBOT_RULE_STAGE_SELECT)objToDelete;

                                    _updateData.LASTUPDATETIME = DateTime.Now; ;

                                    _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("ADD [{0},{1},{2},{3},{4},{5},{6}]", _updateData.ROBOTNAME, _updateData.ROUTEID, _updateData.STEPID, _updateData.ITEMID, _updateData.ITEMSEQ, _updateData.ISENABLED, _updateData.DESCRIPTION);
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

                                foreach (System.Data.Linq.ObjectChangeConflict occ in ctxBRM.ChangeConflicts)
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

                                    foreach (System.Data.Linq.MemberChangeConflict _data in ctxBRM.ChangeConflicts[0].MemberConflicts)
                                    {
                                        if (_data.DatabaseValue != _data.OriginalValue)
                                        {
                                            _sqlErr = _sqlErr + string.Format("\r\n Change Conflicts : Property '{0}': Database value : {1} , Original value {2} ,Current Value :{3}", _data.Member.Name, _data.DatabaseValue, _data.OriginalValue, _data.CurrentValue);
                                        }
                                    }
                                    NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                                    ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                                }
                            }

                            #region 紀錄opi history
                            string _err = UniTools.InsertOPIHistory_DB("SBRM_ROBOT_RULE_STAGE_SELECT", _sqlDesc, _sqlErr);

                            if (_err != string.Empty)
                            {
                                ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _err, MessageBoxIcon.Error);
                            }
                            #endregion

                            Public.SendDatabaseReloadRequest("SBRM_ROBOT_RULE_STAGE_SELECT");

                            if (_sqlErr == string.Empty)
                            {
                                ShowMessage(this, lblCaption.Text, "", "Robot Rule Order by Save Success!", MessageBoxIcon.Information);
                            }
                            this.GetDataGridView();
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
                        this.GetDataGridView();
                        break;
                        #endregion

                    default:
                        break;
                }

                if (_frm != null) _frm.Dispose();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
    }
}
