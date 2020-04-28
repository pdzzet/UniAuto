using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniClientTools;

namespace UniOPI
{
    public partial class FormRobotRuleSelect : FormBase
    {
        string CurRobotName = string.Empty;
        string RobotName = string.Empty;        

        public FormRobotRuleSelect()
        {
            InitializeComponent();
        }

        private void FormRobotRuleSelect_Load(object sender, EventArgs e)
        {
            InitialRadioBtn();
            InitializeData();
        }

        private void InitializeData()
        {           
            try
            {
                #region 檢查Job Select 是否有資料，若有則顯示出來

                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                if ((from d in _ctxBRM.SBRM_ROBOT_RULE_JOB_SELECT where d.LINETYPE.Equals(FormMainMDI.G_OPIAp.CurLine.LineType) && d.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) select d).ToList().Count > 0)
                {
                    GetGridViewData();
                }
                else
                {
                    ShowMessage(this, lblCaption.Text, "", "Can't Find Robot Route Job Select Data !", MessageBoxIcon.Error);
                }

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }            
        }

        private void GetGridViewData()
        {
            #region 顯示GridVew資料
            try
            {
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                //資料庫資料
                var _selData = from rb in _ctxBRM.SBRM_ROBOT_RULE_JOB_SELECT
                               where rb.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName && rb.LINETYPE.Equals(FormMainMDI.G_OPIAp.CurLine.LineType)
                               select rb;

                //已修改未更新物件
                var _addData = _ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_RULE_JOB_SELECT>().Where(
                    add => add.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName && add.LINETYPE.Equals(FormMainMDI.G_OPIAp.CurLine.LineType));

                List<SBRM_ROBOT_RULE_JOB_SELECT> _objTables = _selData.ToList();

                _objTables.AddRange(_addData.ToList());

                List<clsRobotRuleJobSelectTable> _lstCondition = new List<clsRobotRuleJobSelectTable>();

                #region copy data & Get Description & Function Key
                foreach (SBRM_ROBOT_RULE_JOB_SELECT _data in _objTables)
                {
                    clsRobotRuleJobSelectTable _tmp = new clsRobotRuleJobSelectTable();

                    _tmp.OBJECTKEY = _data.OBJECTKEY;
                    _tmp.SERVERNAME = _data.SERVERNAME;
                    _tmp.ROBOTNAME = _data.ROBOTNAME;
                    _tmp.ITEMID = _data.ITEMID;
                    _tmp.ITEMSEQ = _data.ITEMSEQ;
                    _tmp.LINETYPE = _data.LINETYPE;
                    _tmp.DESCRIPTION = _data.DESCRIPTION;
                    _tmp.SELECTTYPE = _data.SELECTTYPE;
                    _tmp.OBJECTNAME = _data.OBJECTNAME;
                    _tmp.METHODNAME = _data.METHODNAME;
                    _tmp.STAGETYPE = _data.STAGETYPE;
                    _tmp.ISENABLED = _data.ISENABLED;
                    _tmp.LASTUPDATETIME = _data.LASTUPDATETIME;
                    _tmp.REMARKS = _data.REMARKS;

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

                SortableBindingList<clsRobotRuleJobSelectTable> _fieldList = new SortableBindingList<clsRobotRuleJobSelectTable>(_lstCondition);

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

        private void btn_Click(object sender, EventArgs e)
        {           
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                Button _btn = (Button)sender;

                switch (_btn.Tag.ToString())
                {
                    case "Add":
                        
                        #region Add
                        FormRobotRuleJobSelectEdit _frmRobotMethodDefEdit = new FormRobotRuleJobSelectEdit(null);
                        _frmRobotMethodDefEdit.StartPosition = FormStartPosition.CenterScreen;
                        _frmRobotMethodDefEdit.ShowDialog();
                        _frmRobotMethodDefEdit.Dispose();
                        GetGridViewData();
                        break;
                        #endregion

                    case "Modify":

                        #region Modify
                        if (dgvData.SelectedRows.Count != 1) return;

                        clsRobotRuleJobSelectTable objSelModify = (dgvData.SelectedRows[0].DataBoundItem) as clsRobotRuleJobSelectTable;

                        SBRM_ROBOT_RULE_JOB_SELECT objProcess = null;

                        if (objSelModify.OBJECTKEY == 0)
                        {
                            // 修改的是尚未Submit的新增
                            var objAddModify = (from d in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_RULE_JOB_SELECT>() 
                                                where d.SERVERNAME == objSelModify.SERVERNAME &&
                                                      d.ROBOTNAME == objSelModify.ROBOTNAME &&
                                                      d.ITEMID == objSelModify.ITEMID &&
                                                      d.SELECTTYPE == objSelModify.SELECTTYPE &&
                                                      d.STAGETYPE == objSelModify.STAGETYPE &&
                                                      d.OBJECTNAME == objSelModify.OBJECTNAME &&
                                                      d.METHODNAME == objSelModify.METHODNAME
                                                select d).FirstOrDefault();

                            if (objAddModify != null)
                            {
                                objProcess = objAddModify;
                            }
                            else
                                return;
                        }
                        else
                        {
                            var objAddModify = (from d in ctxBRM.SBRM_ROBOT_RULE_JOB_SELECT
                                                where d.OBJECTKEY == objSelModify.OBJECTKEY 
                                                select d).FirstOrDefault();

                            if (objAddModify != null)
                            {
                                objProcess = objAddModify;
                            }
                            else
                                return; 
                        }

                        FormRobotRuleJobSelectEdit _frmModify = new FormRobotRuleJobSelectEdit(objProcess);
                        _frmModify.StartPosition = FormStartPosition.CenterScreen;
                        _frmModify.ShowDialog();
                        _frmModify.Dispose();

                        this.GetGridViewData();
                        break;
                        #endregion

                    case "Delete":

                        #region Delete
                        if (dgvData.SelectedRows.Count == 0) return;

                        DialogResult result = this.QuectionMessage(this, this.lblCaption.Text, "Are you sure to delete selected records?");
                        if (result == System.Windows.Forms.DialogResult.No)
                            return;

                        SBRM_ROBOT_RULE_JOB_SELECT objtoDelete = null;

                        clsRobotRuleJobSelectTable objEach = null;

                        foreach (DataGridViewRow selectedRow in dgvData.SelectedRows)
                        {
                            objEach = selectedRow.DataBoundItem as clsRobotRuleJobSelectTable;

                            if (objEach.OBJECTKEY > 0)
                            {
                                objtoDelete = (from proc in ctxBRM.SBRM_ROBOT_RULE_JOB_SELECT
                                               where proc.OBJECTKEY.Equals(objEach.OBJECTKEY)
                                               select proc).FirstOrDefault();
                            }
                            else
                            {
                                objtoDelete = (from proc in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_RULE_JOB_SELECT>()
                                               where proc.SERVERNAME == objEach.SERVERNAME &&
                                                     proc.ROBOTNAME == objEach.ROBOTNAME &&
                                                     proc.ITEMID == objEach.ITEMID &&
                                                     proc.SELECTTYPE == objEach.SELECTTYPE &&
                                                     proc.STAGETYPE == objEach.STAGETYPE &&
                                                     proc.OBJECTNAME == objEach.OBJECTNAME &&
                                                     proc.METHODNAME == objEach.METHODNAME
                                               select proc).FirstOrDefault();
                            }

                            if (objtoDelete != null)
                                ctxBRM.SBRM_ROBOT_RULE_JOB_SELECT.DeleteOnSubmit(objtoDelete);
                        }

                        this.GetGridViewData();

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
                                #region Add
                                foreach (object objToInsert in ctxBRM.GetChangeSet().Inserts)
                                {
                                    if (objToInsert.GetType().Name != "SBRM_ROBOT_RULE_JOB_SELECT") continue;
                                    SBRM_ROBOT_RULE_JOB_SELECT _updateData = (SBRM_ROBOT_RULE_JOB_SELECT)objToInsert;
                                    _updateData.LASTUPDATETIME = _upDateTime;

                                    _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("ADD [{0},{1},{2},{3},{4},{5},{6} ", _updateData.SELECTTYPE, _updateData.ROBOTNAME, _updateData.ROBOTNAME, _updateData.SERVERNAME, _updateData.LINETYPE, _updateData.DESCRIPTION, _updateData.ISENABLED);
                                }
                                #endregion

                                #region Modify
                                foreach (object objToModify in ctxBRM.GetChangeSet().Updates)
                                {
                                    if (objToModify.GetType().Name != "SBRM_ROBOT_RULE_JOB_SELECT") continue;
                                    SBRM_ROBOT_RULE_JOB_SELECT _updateData = (SBRM_ROBOT_RULE_JOB_SELECT)objToModify;
                                    _updateData.LASTUPDATETIME = _upDateTime;

                                    _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [{0},{1},{2},{3},{4},{5},{6} ", _updateData.SELECTTYPE, _updateData.ROBOTNAME, _updateData.ROBOTNAME, _updateData.SERVERNAME, _updateData.LINETYPE, _updateData.DESCRIPTION, _updateData.ISENABLED);
                                }
                                #endregion

                                #region DELETE
                                foreach (object objToDelete in ctxBRM.GetChangeSet().Deletes)
                                {
                                    if (objToDelete.GetType().Name != "SBRM_ROBOT_RULE_JOB_SELECT") continue;
                                    SBRM_ROBOT_RULE_JOB_SELECT _updateData = (SBRM_ROBOT_RULE_JOB_SELECT)objToDelete;

                                    _updateData.LASTUPDATETIME = _upDateTime;

                                    _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("DEL [{0},{1},{2},{3},{4},{5},{6} ", _updateData.SELECTTYPE, _updateData.ROBOTNAME, _updateData.ROBOTNAME, _updateData.SERVERNAME, _updateData.LINETYPE, _updateData.DESCRIPTION, _updateData.ISENABLED);
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
                            string _err = UniTools.InsertOPIHistory_DB("SBRM_ROBOT_RULE_JOB_SELECT", _sqlDesc, _sqlErr);

                            if (_err != string.Empty)
                            {
                                ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _err, MessageBoxIcon.Error);
                            }
                            #endregion

                            Public.SendDatabaseReloadRequest("SBRM_ROBOT_RULE_JOB_SELECT");

                            if (_sqlErr == string.Empty)
                            {
                                ShowMessage(this, lblCaption.Text, "", "Robot Rule Job Select Save Success!", MessageBoxIcon.Information);
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
        }

        private void dgvData_DataSourceChanged(object sender, EventArgs e)
        {
            dgvCellBackColor();
        }

        private void InitialRadioBtn()
        {            
            try
            {
                string _nodeID = string.Empty;
                string _unitID = string.Empty;
                string _unitKey = string.Empty;

                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                var _robot = (from rb in _ctxBRM.SBRM_ROBOT
                              where rb.LINETYPE.Equals(FormMainMDI.G_OPIAp.CurLine.LineType) && rb.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName)
                              select rb).ToList();

                if (_robot == null || _robot.Count == 0)
                {
                    ShowMessage(this, lblCaption.Text, "", "Can't Find Robot Data !", MessageBoxIcon.Error);
                    pnlSideBarBtn.Enabled = false;
                    return;
                }
                for (int i = 0; i < _robot.Count; i++)
                {
                    RadioButton _rb = new RadioButton();
                    _rb.Name = _robot[i].ROBOTNAME;
                    _rb.Text = _robot[i].ROBOTNAME;
                    _rb.AutoSize = false;
                    _rb.Font = new Font("Calibri", 13);
                    _rb.Size = new Size(120, 30);
                    _rb.Location = new Point(10 + (120 * i), 15);
                    if (i == 0)
                    {
                        _rb.Checked = true;
                        RobotName = _robot[i].ROBOTNAME;
                    }
                    _rb.Click += new EventHandler(RadioButton_Click);
                    pnlRobot.Controls.Add(_rb);

                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void RadioButton_Click(object sender, EventArgs e)
        {
            #region 選取RobotName ，讀取資料
            RadioButton rb = (RadioButton)sender;
            if (rb.Checked == true)
            {
                rb.Checked = true;
                RobotName = rb.Name;
                GetGridViewData();
            }
            #endregion
        }

        private void dgvData_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dgvCellBackColor();
        }

        private void dgvCellBackColor()
        {            
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                if (dgvData.DataSource == null || dgvData.Rows.Count == 0) return;

                #region 若GridView資料有變更，改變變更資料的顏色

                ctxBRM.GetChangeSet();
                foreach (DataGridViewRow dr in dgvData.Rows)
                {
                    clsRobotRuleJobSelectTable obj = dr.DataBoundItem as clsRobotRuleJobSelectTable;

                    if (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_RULE_JOB_SELECT>().Any(
                        msg => msg.SERVERNAME == obj.SERVERNAME && 
                               msg.ROBOTNAME == obj.ROBOTNAME && 
                               msg.SELECTTYPE == obj.SELECTTYPE &&
                               msg.STAGETYPE == obj.STAGETYPE &&
                               msg.ITEMID == obj.ITEMID && 
                               msg.OBJECTNAME == obj.OBJECTNAME &&
                               msg.METHODNAME == obj.METHODNAME))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(214, 233, 177);
                    }
                    else if (ctxBRM.GetChangeSet().Deletes.OfType<SBRM_ROBOT_RULE_JOB_SELECT>().Any(
                         msg => msg.SERVERNAME == obj.SERVERNAME && 
                                msg.ROBOTNAME == obj.ROBOTNAME && 
                                msg.SELECTTYPE == obj.SELECTTYPE &&
                                msg.STAGETYPE == obj.STAGETYPE &&
                                msg.ITEMID == obj.ITEMID &&
                                msg.OBJECTNAME == obj.OBJECTNAME &&
                                msg.METHODNAME == obj.METHODNAME))
                    {
                        dr.DefaultCellStyle.BackColor = Color.Red;
                    }
                    else if (ctxBRM.GetChangeSet().Updates.OfType<SBRM_ROBOT_RULE_JOB_SELECT>().Any(
                        msg => msg.SERVERNAME == obj.SERVERNAME && 
                               msg.ROBOTNAME == obj.ROBOTNAME && 
                               msg.SELECTTYPE == obj.SELECTTYPE &&
                               msg.STAGETYPE == obj.STAGETYPE &&
                               msg.ITEMID == obj.ITEMID &&
                               msg.OBJECTNAME == obj.OBJECTNAME &&
                               msg.METHODNAME == obj.METHODNAME))
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

    public class clsRobotRuleJobSelectTable : SBRM_ROBOT_RULE_JOB_SELECT
    {
        public string MED_DEF_DESCRIPTION { get; set; }
        public string MED_DEF_FUNCKEY { get; set; }
    }
}
