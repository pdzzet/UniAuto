using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniClientTools;

namespace UniOPI
{
    public partial class FormRobotRouteCondition : FormBase
    {
        string RobotName = string.Empty;

        public FormRobotRouteCondition()
        {
            InitializeComponent();
        }

        private void FormRobotRouteCondition_Load(object sender, EventArgs e)
        {
            InitialRadioBtn();
            InitializeData();
        }

        public void GetGridViewData()
        {
            try
            {
                #region 顯示GridVew資料

                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                //資料庫資料
                var _selData = from rb in _ctxBRM.SBRM_ROBOT_ROUTE_CONDITION
                              where rb.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName && 
                                    rb.LINETYPE.Equals(FormMainMDI.G_OPIAp.CurLine.LineType)
                              select rb;

                //已修改未更新物件
                var _addData = _ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_ROUTE_CONDITION>().Where(
                    add => add.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName && 
                           add.LINETYPE.Equals(FormMainMDI.G_OPIAp.CurLine.LineType));

                List<SBRM_ROBOT_ROUTE_CONDITION> _objTables = _selData.ToList();

                _objTables.AddRange(_addData.ToList());

                List<clsRobotRouteConditionTable> _lstCondition = new List<clsRobotRouteConditionTable>();

                #region copy data & Get Description & Function Key
                foreach (SBRM_ROBOT_ROUTE_CONDITION _data in _objTables)
                {
                    clsRobotRouteConditionTable _tmp = new clsRobotRouteConditionTable();

                    _tmp.SERVERNAME = _data.SERVERNAME;
                    _tmp.LINETYPE = _data.LINETYPE;
                    _tmp.CONDITIONID = _data.CONDITIONID;
                    _tmp.CONDITIONSEQ = _data.CONDITIONSEQ;
                    _tmp.DESCRIPTION = _data.DESCRIPTION;
                    _tmp.ISENABLED = _data.ISENABLED;
                    _tmp.LASTUPDATETIME = _data.LASTUPDATETIME;                    
                    _tmp.METHODNAME = _data.METHODNAME;
                    _tmp.OBJECTKEY = _data.OBJECTKEY;
                    _tmp.OBJECTNAME = _data.OBJECTNAME;
                    _tmp.REMARKS = _data.REMARKS;
                    _tmp.ROBOTNAME = _data.ROBOTNAME;
                    _tmp.ROUTEID = _data.ROUTEID;
                    _tmp.ROUTEPRIORITY = _data.ROUTEPRIORITY;
                    
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

                SortableBindingList<clsRobotRouteConditionTable> _fieldList = new SortableBindingList<clsRobotRouteConditionTable>(_lstCondition);

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

        private void InitializeData()
        {
            try
            {            
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                if ((from d in _ctxBRM.SBRM_ROBOT_ROUTE_CONDITION where d.LINETYPE.Equals(FormMainMDI.G_OPIAp.CurLine.LineType) && d.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) select d).ToList().Count > 0)
                {
                    GetGridViewData();
                }
                else
                {
                    ShowMessage(this, lblCaption.Text, "", "Can not Find Robot Route Condition Data !", MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btn_Click(object sender, EventArgs e)
        {
            try
            {
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                Button _btn = (Button)sender;

                switch (_btn.Tag.ToString())
                {
                    case "Add":

                        #region Add

                        FormRobotRouteConditionEdit _frmRobotMethodDefEdit = new FormRobotRouteConditionEdit(null);
                        _frmRobotMethodDefEdit.StartPosition = FormStartPosition.CenterScreen;
                        _frmRobotMethodDefEdit.ShowDialog();
                        _frmRobotMethodDefEdit.Dispose();
                        
                        GetGridViewData();
                        
                        break;

                        #endregion

                    case "Modify":

                        #region Modify

                        if (dgvData.SelectedRows.Count != 1) return;

                        clsRobotRouteConditionTable objSelModify = (dgvData.SelectedRows[0].DataBoundItem) as clsRobotRouteConditionTable;

                        SBRM_ROBOT_ROUTE_CONDITION objProcess = null;

                        if (objSelModify.OBJECTKEY == 0)
                        {
                            // 修改的是尚未Submit的新增
                            var objAddModify = (from d in _ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_ROUTE_CONDITION>()
                                                where d.SERVERNAME == objSelModify.SERVERNAME &&
                                                      d.ROBOTNAME == objSelModify.ROBOTNAME &&
                                                      d.ROUTEID == objSelModify.ROUTEID &&
                                                      d.CONDITIONID == objSelModify.CONDITIONID &&
                                                      d.OBJECTNAME == objSelModify.OBJECTNAME && 
                                                      d.METHODNAME == objSelModify.METHODNAME select d).FirstOrDefault();

                            if (objAddModify != null)
                            {
                                objProcess = objAddModify;
                            }
                            else  return;
                        }
                        else
                        {
                            var objModify = (from d in _ctxBRM.SBRM_ROBOT_ROUTE_CONDITION
                                             where d.OBJECTKEY == objSelModify.OBJECTKEY
                                             select d).FirstOrDefault();

                            if (objModify != null)
                            {
                                objProcess = objModify;
                            }
                            else  return; 
                        }
                            
                        FormRobotRouteConditionEdit _frmModify = new FormRobotRouteConditionEdit(objProcess);
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

                        clsRobotRouteConditionTable objEach = null;
                        SBRM_ROBOT_ROUTE_CONDITION objtoDelete = null;

                        foreach (DataGridViewRow selectedRow in dgvData.SelectedRows)
                        {
                            objEach = selectedRow.DataBoundItem as clsRobotRouteConditionTable;

                            if (objEach.OBJECTKEY > 0)
                            {
                                objtoDelete = (from proc in _ctxBRM.SBRM_ROBOT_ROUTE_CONDITION
                                               where proc.OBJECTKEY.Equals(objEach.OBJECTKEY) 
                                               select proc).FirstOrDefault();
                            }
                            else
                            {
                                objtoDelete = (from proc in _ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_ROUTE_CONDITION>()
                                               where proc.SERVERNAME.Equals(objEach.SERVERNAME) &&
                                                     proc.ROBOTNAME.Equals(objEach.ROBOTNAME) &&
                                                     proc.ROUTEID.Equals(objEach.ROUTEID) &&
                                                     proc.CONDITIONID.Equals(objEach.CONDITIONID) &&
                                                     proc.OBJECTNAME.Equals(objEach.OBJECTNAME) &&
                                                     proc.METHODNAME.Equals(objEach.METHODNAME) select proc).FirstOrDefault();
                            }

                            if (objtoDelete != null)
                                _ctxBRM.SBRM_ROBOT_ROUTE_CONDITION.DeleteOnSubmit(objtoDelete);
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
                                foreach (object objToInsert in _ctxBRM.GetChangeSet().Inserts)
                                {
                                    if (objToInsert.GetType().Name != "SBRM_ROBOT_ROUTE_CONDITION") continue;
                                    SBRM_ROBOT_ROUTE_CONDITION _updateData = (SBRM_ROBOT_ROUTE_CONDITION)objToInsert;
                                    _updateData.LASTUPDATETIME = _upDateTime;

                                    _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("ADD [{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12} ",_updateData.ROUTEID, _updateData.ROBOTNAME, _updateData.CONDITIONID, _updateData.SERVERNAME, _updateData.LINETYPE, _updateData.DESCRIPTION,_updateData.OBJECTNAME,_updateData.METHODNAME,_updateData.ISENABLED,_updateData.REMARKS,_updateData.LASTUPDATETIME,_updateData.CONDITIONSEQ,_updateData.ROUTEPRIORITY);
                                }
                                #endregion

                                #region Modify
                                foreach (object objToModify in _ctxBRM.GetChangeSet().Updates)
                                {
                                    if (objToModify.GetType().Name != "SBRM_ROBOT_ROUTE_CONDITION") continue;
                                    SBRM_ROBOT_ROUTE_CONDITION _updateData = (SBRM_ROBOT_ROUTE_CONDITION)objToModify;
                                    _updateData.LASTUPDATETIME = _upDateTime;

                                    _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11} ,{12}", _updateData.ROUTEID, _updateData.ROBOTNAME, _updateData.CONDITIONID, _updateData.SERVERNAME, _updateData.LINETYPE, _updateData.DESCRIPTION, _updateData.OBJECTNAME, _updateData.METHODNAME, _updateData.ISENABLED, _updateData.REMARKS, _updateData.LASTUPDATETIME, _updateData.CONDITIONSEQ,_updateData.ROUTEPRIORITY);
                                }
                                #endregion

                                #region Delete
                                foreach (object objToDelete in _ctxBRM.GetChangeSet().Deletes)
                                {
                                    if (objToDelete.GetType().Name != "SBRM_ROBOT_ROUTE_CONDITION") continue;
                                    SBRM_ROBOT_ROUTE_CONDITION _updateData = (SBRM_ROBOT_ROUTE_CONDITION)objToDelete;

                                    _updateData.LASTUPDATETIME = _upDateTime;

                                    _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("DEL [{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12} ", _updateData.ROUTEID, _updateData.ROBOTNAME, _updateData.CONDITIONID, _updateData.SERVERNAME, _updateData.LINETYPE, _updateData.DESCRIPTION, _updateData.OBJECTNAME, _updateData.METHODNAME, _updateData.ISENABLED, _updateData.REMARKS, _updateData.LASTUPDATETIME, _updateData.CONDITIONSEQ,_updateData.ROUTEPRIORITY);
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
                            string _err = UniTools.InsertOPIHistory_DB("SBRM_ROBOTMETHODDEF", _sqlDesc, _sqlErr);

                            if (_err != string.Empty)
                            {
                                ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _err, MessageBoxIcon.Error);
                            }
                            #endregion

                            Public.SendDatabaseReloadRequest("SBRM_ROBOT_ROUTE_CONDITION");

                            if (_sqlErr == string.Empty)
                            {
                                ShowMessage(this, lblCaption.Text, "", "Robot Route Condition Save Success!", MessageBoxIcon.Information);
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

        private void dgvData_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dgvCellBackColor();
        }

        private void InitialRadioBtn()
        {
            try
            {
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                string _nodeID = string.Empty;
                string _unitID = string.Empty;
                string _unitKey = string.Empty;

                var _robot = (from rb in _ctxBRM.SBRM_ROBOT
                              where rb.LINETYPE.Equals(FormMainMDI.G_OPIAp.CurLine.LineType) && 
                                    rb.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName)
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
            try
            {
                RadioButton _rb = (RadioButton)sender;

                if (_rb.Checked == true)
                {
                    RobotName = _rb.Name;

                    GetGridViewData();
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvCellBackColor()
        {
            try
            {
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                if (dgvData.DataSource == null || dgvData.Rows.Count == 0) return;

                _ctxBRM.GetChangeSet();

                foreach (DataGridViewRow dr in dgvData.Rows)
                {
                    clsRobotRouteConditionTable _obj = dr.DataBoundItem as clsRobotRouteConditionTable;
                    
                    if (_ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_ROUTE_CONDITION>().Any(
                        msg => msg.SERVERNAME == _obj.SERVERNAME &&
                               msg.ROBOTNAME == _obj.ROBOTNAME &&
                               msg.ROUTEID == _obj.ROUTEID &&
                               msg.CONDITIONID == _obj.CONDITIONID &&
                               msg.OBJECTNAME == _obj.OBJECTNAME &&
                               msg.METHODNAME == _obj.METHODNAME))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(214, 233, 177);
                    }
                    else if (_ctxBRM.GetChangeSet().Deletes.OfType<SBRM_ROBOT_ROUTE_CONDITION>().Any(
                        msg => msg.SERVERNAME == _obj.SERVERNAME &&
                               msg.ROBOTNAME == _obj.ROBOTNAME &&
                               msg.ROUTEID == _obj.ROUTEID &&
                               msg.CONDITIONID == _obj.CONDITIONID &&
                               msg.OBJECTNAME == _obj.OBJECTNAME &&
                               msg.METHODNAME == _obj.METHODNAME))
                    {
                        dr.DefaultCellStyle.BackColor = Color.Red;
                    }
                    else if (_ctxBRM.GetChangeSet().Updates.OfType<SBRM_ROBOT_ROUTE_CONDITION>().Any(
                        msg => msg.SERVERNAME == _obj.SERVERNAME &&
                               msg.ROBOTNAME == _obj.ROBOTNAME &&
                               msg.ROUTEID == _obj.ROUTEID &&
                               msg.CONDITIONID == _obj.CONDITIONID &&
                               msg.OBJECTNAME == _obj.OBJECTNAME &&
                               msg.METHODNAME == _obj.METHODNAME))
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
    }

    public class clsRobotRouteConditionTable : SBRM_ROBOT_ROUTE_CONDITION
    {
        public string MED_DEF_DESCRIPTION { get; set; }
        public string MED_DEF_FUNCKEY { get; set; }
    }
}
