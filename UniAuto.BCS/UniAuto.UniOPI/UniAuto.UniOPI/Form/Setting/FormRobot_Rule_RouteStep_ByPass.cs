﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniClientTools;

namespace UniOPI
{
    public partial class FormRobot_Route_Step_ByPass : FormBase
    {
        string CurRouteID = string.Empty;
        int CurStepID = 0;//現在step值
        Robot CurRobot = null;//Robot資訊

        List<int> LstStepID = null;//記錄各Step的數值

        public FormRobot_Route_Step_ByPass()
        {
            InitializeComponent();

            this.lblCaption.Text = "Robot Step ByPass";
        }

        public FormRobot_Route_Step_ByPass(string routeID, int stepID, Robot robot, List<int> lstStepID)
            : this()
        {
            CurRouteID = routeID;
            CurStepID = stepID;
            CurRobot = robot;
            LstStepID = lstStepID;
        }

        private void FormRobot_Route_Step_ByPass_Load(object sender, EventArgs e)
        {
            txtRobotName.Text = CurRobot.RobotName;
            txtRouteID.Text = CurRouteID;

            InitialCombox();
        }

        private void InitialCombox()
        {
            #region 產生Step的ComboBox資料、觸發事件。
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
            #endregion
        }

        private void GetDataGridView()
        {            
            try
            {
                #region 顯示GridVew資料

                //objTables = new List<SBRM_ROBOT_RULE_ROUTESTEP_BYPASS>();
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                //資料庫資料
                var _selData = from rb in _ctxBRM.SBRM_ROBOT_RULE_ROUTESTEP_BYPASS
                               where rb.SERVERNAME == CurRobot.ServerName  &&
                                     rb.ROBOTNAME == CurRobot.RobotName &&
                                     rb.ROUTEID == CurRouteID &&
                                     rb.STEPID == CurStepID
                               select rb;

                //已修改未更新物件
                var _addData = _ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_RULE_ROUTESTEP_BYPASS>().Where( add => 
                    add.SERVERNAME == CurRobot.ServerName &&
                    add.ROBOTNAME == CurRobot.RobotName &&
                    add.ROUTEID == CurRouteID &&
                    add.STEPID == CurStepID);

                List<SBRM_ROBOT_RULE_ROUTESTEP_BYPASS> _objTables = _selData.ToList();

                _objTables.AddRange(_addData.ToList());

                List<clsRobotRuleRouteStepByPassTable> _lstCondition = new List<clsRobotRuleRouteStepByPassTable>();

                #region Get Description & Function Key
                foreach (SBRM_ROBOT_RULE_ROUTESTEP_BYPASS _data in _objTables)
                {
                    clsRobotRuleRouteStepByPassTable _tmp = new clsRobotRuleRouteStepByPassTable();

                    _tmp.OBJECTKEY = _data.OBJECTKEY;
                    _tmp.SERVERNAME = _data.SERVERNAME;
                    _tmp.ROBOTNAME = _data.ROBOTNAME;
                    _tmp.ROUTEID = _data.ROUTEID;
                    _tmp.STEPID = _data.STEPID;
                    _tmp.BYPASSCONDITIONID = _data.BYPASSCONDITIONID;
                    _tmp.GOTOSTEPID = _data.GOTOSTEPID;
                    _tmp.BYPASSITEMSEQ = _data.BYPASSITEMSEQ;
                    _tmp.DESCRIPTION = _data.DESCRIPTION;
                    _tmp.OBJECTNAME = _data.OBJECTNAME;
                    _tmp.METHODNAME = _data.METHODNAME;
                    _tmp.REMARKS = _data.REMARKS;
                    _tmp.ISENABLED = _data.ISENABLED;
                    _tmp.LASTUPDATETIME = _data.LASTUPDATETIME;


                    var _var = _ctxBRM.SBRM_ROBOT_METHOD_DEF.Where(r => r.METHODNAME.Equals(_data.METHODNAME) &&
                        r.OBJECTNAME.Equals(_data.OBJECTNAME)).FirstOrDefault();

                    if (_var != null)
                    {
                        _tmp.MED_DEF_FUNCKEY = _var.FUNCKEY.ToString();
                        _tmp.MED_DEF_DESCRIPTION = _var.DESCRIPTION.ToString();
                    }

                    _lstCondition.Add(_tmp);
                }
                #endregion

                SortableBindingList<clsRobotRuleRouteStepByPassTable> _fieldList = new SortableBindingList<clsRobotRuleRouteStepByPassTable>(_lstCondition);

                BindingSource _source = new BindingSource();

                _source.DataSource = _fieldList;

                dgvData.AutoGenerateColumns = false;

                dgvData.DataSource = _source;

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cboStepID_SelectedIndexChanged(object sender, EventArgs e)
        {
            #region 當選取Step選項時，顯示特定GridView資料
            if (cboStepID.SelectedValue == null) return;

            CurStepID = int.Parse(cboStepID.SelectedValue.ToString());

            GetDataGridView();
            #endregion
        }

        private void dgvData_DataSourceChanged(object sender, EventArgs e)
        {
            dgvCellBackColor();
        }

        private void dgvData_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dgvCellBackColor();          
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btn_Click(object sender, EventArgs e)
        {
            #region 按鈕功能-新增、修改、刪除、儲存、重新整理
            UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

            try
            {
                Button _btn = (Button)sender;

                switch (_btn.Tag.ToString())
                {
                    case "Add":

                        #region Add
                        FormRobot_Route_Step_ByPass_Edit _frmAdd = new FormRobot_Route_Step_ByPass_Edit(
                       CurRouteID, CurStepID,CurRobot, null, LstStepID);

                        _frmAdd.StartPosition = FormStartPosition.CenterScreen;

                        if (_frmAdd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            GetDataGridView();
                        }

                        _frmAdd.Dispose();

                        break;

                        #endregion                       

                    case "Modify":

                        #region Modify
                        if (dgvData.SelectedRows.Count != 1) return;

                        clsRobotRuleRouteStepByPassTable objSelModify = (dgvData.SelectedRows[0].DataBoundItem) as clsRobotRuleRouteStepByPassTable;

                        SBRM_ROBOT_RULE_ROUTESTEP_BYPASS objProcess = null;

                        if (objSelModify.OBJECTKEY == 0)
                        {
                            // 修改的是尚未Submit的新增 -- //UK : ServerName + Robot Name + RouteID + Step ID +  + Item ID     
                            var objAddModify = (from d in _ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_RULE_ROUTESTEP_BYPASS>()
                                                where d.SERVERNAME == objSelModify.SERVERNAME && d.ROBOTNAME == objSelModify.ROBOTNAME && d.ROUTEID == objSelModify.ROUTEID &&
                                                      d.STEPID == objSelModify.STEPID && d.BYPASSCONDITIONID == objSelModify.BYPASSCONDITIONID
                                                select d).FirstOrDefault();

                            if (objAddModify != null)
                            {
                                objProcess = objAddModify;
                            }
                            else  return;
                        }
                        else
                        {
                            // 修改的是尚未Submit的新增
                            var objModify = (from d in _ctxBRM.SBRM_ROBOT_RULE_ROUTESTEP_BYPASS
                                             where d.OBJECTKEY == objSelModify.OBJECTKEY
                                             select d).FirstOrDefault();

                            if (objModify != null)
                            {
                                objProcess = objModify;
                            }
                            else  return; 
                        }

                        FormRobot_Route_Step_ByPass_Edit _frmModify = new FormRobot_Route_Step_ByPass_Edit(
                        CurRouteID, CurStepID,CurRobot,objProcess,LstStepID);

                        if (_frmModify.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        {
                            GetDataGridView();
                        }
                        break;

                        #endregion
                        
                    case "Delete":

                        #region Delete
                        if (dgvData.SelectedRows.Count == 0) return;

                        DialogResult result = this.QuectionMessage(this, this.lblCaption.Text, "Are you sure to delete selected records?");
                        if (result == System.Windows.Forms.DialogResult.No)
                            return;

                        clsRobotRuleRouteStepByPassTable objEach = null;

                        SBRM_ROBOT_RULE_ROUTESTEP_BYPASS  objtoDelete = null;

                        foreach (DataGridViewRow selectedRow in dgvData.SelectedRows)
                        {
                            objEach = selectedRow.DataBoundItem as clsRobotRuleRouteStepByPassTable;

                            if (objEach.OBJECTKEY > 0)
                            {
                                objtoDelete = (from proc in _ctxBRM.SBRM_ROBOT_RULE_ROUTESTEP_BYPASS
                                               where proc.OBJECTKEY.Equals(objEach.OBJECTKEY)
                                               select proc).FirstOrDefault();
                            }
                            else
                            {
                                objtoDelete = (from proc in _ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_RULE_ROUTESTEP_BYPASS>()
                                               where proc.SERVERNAME.Equals(objEach.SERVERNAME) &&
                                                     proc.ROBOTNAME.Equals(objEach.ROBOTNAME) &&
                                                     proc.ROUTEID.Equals(objEach.ROUTEID) &&
                                                     proc.STEPID.Equals(objEach.STEPID) &&
                                                     proc.BYPASSCONDITIONID.Equals(objEach.BYPASSCONDITIONID)
                                               select proc).FirstOrDefault();
                            }

                            if (objtoDelete != null)
                                _ctxBRM.SBRM_ROBOT_RULE_ROUTESTEP_BYPASS.DeleteOnSubmit(objtoDelete);
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
                            DateTime _upDateTime = DateTime.Now;

                            try
                            {
                                #region 取得更新的data
                                foreach (object objToInsert in _ctxBRM.GetChangeSet().Inserts)
                                {
                                    if (objToInsert.GetType().Name != "SBRM_ROBOT_RULE_ROUTESTEP_BYPASS") continue;
                                    SBRM_ROBOT_RULE_ROUTESTEP_BYPASS _updateData = (SBRM_ROBOT_RULE_ROUTESTEP_BYPASS)objToInsert;
                                    _updateData.LASTUPDATETIME = _upDateTime;

                                    _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("ADD [{0},{1},{2},{3},{4},{5},{6}]", _updateData.ROBOTNAME, _updateData.ROUTEID, _updateData.STEPID, _updateData.BYPASSCONDITIONID, _updateData.GOTOSTEPID, _updateData.ISENABLED, _updateData.DESCRIPTION);
                                }
                                #endregion

                                #region Modify
                                foreach (object objToModify in _ctxBRM.GetChangeSet().Updates)
                                {
                                    if (objToModify.GetType().Name != "SBRM_ROBOT_RULE_ROUTESTEP_BYPASS") continue;
                                    SBRM_ROBOT_RULE_ROUTESTEP_BYPASS _updateData = (SBRM_ROBOT_RULE_ROUTESTEP_BYPASS)objToModify;
                                    _updateData.LASTUPDATETIME = _upDateTime;

                                    _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("ADD [{0},{1},{2},{3},{4},{5},{6}]", _updateData.ROBOTNAME, _updateData.ROUTEID, _updateData.STEPID, _updateData.BYPASSCONDITIONID, _updateData.GOTOSTEPID, _updateData.ISENABLED, _updateData.DESCRIPTION);
                                }
                                #endregion

                                #region DELETE
                                foreach (object objToDelete in _ctxBRM.GetChangeSet().Deletes)
                                {
                                    if (objToDelete.GetType().Name != "SBRM_ROBOT_RULE_ROUTESTEP_BYPASS") continue;
                                    SBRM_ROBOT_RULE_ROUTESTEP_BYPASS _updateData = (SBRM_ROBOT_RULE_ROUTESTEP_BYPASS)objToDelete;

                                    _updateData.LASTUPDATETIME = _upDateTime;

                                    _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("ADD [{0},{1},{2},{3},{4},{5},{6}]", _updateData.ROBOTNAME, _updateData.ROUTEID, _updateData.STEPID, _updateData.BYPASSCONDITIONID, _updateData.GOTOSTEPID, _updateData.ISENABLED, _updateData.DESCRIPTION);
                                }
                                #endregion

                                if (_sqlDesc == string.Empty)
                                {
                                    ShowMessage(this, lblCaption.Text, string.Empty, "No Data for Update !", MessageBoxIcon.Warning);
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
                                            _sqlErr = _sqlErr + string.Format("\r\n Change Conflicts : Property '{0}': Database value : {1} , Original value {2} ,Current Value :{3}", _data.Member.Name, _data.DatabaseValue, _data.OriginalValue, _data.CurrentValue);
                                        }
                                    }
                                    NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                                    ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                                }
                            }

                            #region 紀錄opi history
                            string _err = UniTools.InsertOPIHistory_DB("SBRM_ROBOT_RULE_ROUTESTEP_BYPASS", _sqlDesc, _sqlErr);

                            if (_err != string.Empty)
                            {
                                ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _err, MessageBoxIcon.Error);
                            }
                            #endregion

                            Public.SendDatabaseReloadRequest("SBRM_ROBOT_RULE_ROUTESTEP_BYPASS");

                            if (_sqlErr == string.Empty)
                            {
                                ShowMessage(this, lblCaption.Text, "", "Robot Rule Step ByPass Save Success!", MessageBoxIcon.Information);
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

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

        private void dgvCellBackColor()
        {
            #region 當GridView資料來源有改變時，改變新資料的儲存格顏色
            try
            {
                if (dgvData.DataSource == null || dgvData.Rows.Count == 0) return;

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                ctxBRM.GetChangeSet();
                foreach (DataGridViewRow dr in dgvData.Rows)
                {
                    SBRM_ROBOT_RULE_ROUTESTEP_BYPASS obj = dr.DataBoundItem as SBRM_ROBOT_RULE_ROUTESTEP_BYPASS;

                    if (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_RULE_ROUTESTEP_BYPASS>().Any(
                        msg => msg.ROBOTNAME == CurRobot.RobotName && msg.ROUTEID == obj.ROUTEID && msg.STEPID == obj.STEPID && msg.BYPASSCONDITIONID.Equals(obj.BYPASSCONDITIONID)))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(214, 233, 177);
                    }
                    else if (ctxBRM.GetChangeSet().Deletes.OfType<SBRM_ROBOT_RULE_ROUTESTEP_BYPASS>().Any(
                        msg => msg.ROBOTNAME == CurRobot.RobotName && msg.ROUTEID == obj.ROUTEID && msg.STEPID == obj.STEPID && msg.BYPASSCONDITIONID.Equals(obj.BYPASSCONDITIONID)))
                    {
                        dr.DefaultCellStyle.BackColor = Color.Red;
                    }
                    else if (ctxBRM.GetChangeSet().Updates.OfType<SBRM_ROBOT_RULE_ROUTESTEP_BYPASS>().Any(
                        msg => msg.ROBOTNAME == CurRobot.RobotName && msg.ROUTEID == obj.ROUTEID && msg.STEPID == obj.STEPID && msg.BYPASSCONDITIONID.Equals(obj.BYPASSCONDITIONID)))
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
            #endregion
        }
    }

    public class clsRobotRuleRouteStepByPassTable : SBRM_ROBOT_RULE_ROUTESTEP_BYPASS
    {
        public string MED_DEF_DESCRIPTION { get; set; }
        public string MED_DEF_FUNCKEY { get; set; }
    }
}
