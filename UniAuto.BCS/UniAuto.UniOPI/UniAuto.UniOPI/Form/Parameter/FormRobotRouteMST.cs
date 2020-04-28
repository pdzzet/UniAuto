using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniClientTools;

namespace UniOPI
{
    public partial class FormRobotRouteMST : FormBase
    {
        public FormRobotRouteMST()
        {
            InitializeComponent();
        }
        string _robotName = string.Empty;
        List<SBRM_ROBOT_ROUTE_MST> objTables;
        List<bool> asc = new List<bool>(); // 此List用於讓使用者點選欄位時可以作順序或是倒序的排序

        private void FormRobotRouteMST_Load(object sender, EventArgs e)
        {
            //讀取GridView欄位數量
            for (int i = 0; i < dgvData.Columns.Count; i++)
            {
                asc.Add(false);
            }
            InitialRadioBtn();
            InitializeData();
        }

        private void InitializeData()
        {            
            try
            {
                #region 檢查RouteMST是否有資料，若有則顯示出來

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                if ((from d in ctxBRM.SBRM_ROBOT_ROUTE_MST where d.LINETYPE.Equals(FormMainMDI.G_OPIAp.CurLine.LineType) && d.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) select d).ToList().Count > 0)
                {
                    GetGridViewData();
                }
                else
                {
                    ShowMessage(this, lblCaption.Text, "", "Can't Find Robot Route Master Data !", MessageBoxIcon.Error);
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

            #region 顯示GridView資料
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                objTables = (from msg in ctxBRM.SBRM_ROBOT_ROUTE_MST where msg.LINETYPE.Equals(FormMainMDI.G_OPIAp.CurLine.LineType) && msg.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) && msg.ROBOTNAME.Equals(_robotName) select msg).ToList();

                var addData = ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_ROUTE_MST>();

                objTables.AddRange(addData.ToList());

                SortableBindingList<SBRM_ROBOT_ROUTE_MST> _fieldList = new SortableBindingList<SBRM_ROBOT_ROUTE_MST>(objTables);

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
            #region 按鈕功能
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                Button _btn = (Button)sender;
                switch (_btn.Tag.ToString())
                {
                    case "Add":
                        FormRobotRouteMSTEdit _frmRobotMethodDefEdit = new FormRobotRouteMSTEdit(null);
                        _frmRobotMethodDefEdit.StartPosition = FormStartPosition.CenterScreen;
                        _frmRobotMethodDefEdit.ShowDialog();
                        _frmRobotMethodDefEdit.Dispose();
                        GetGridViewData();
                        break;
                    case "Modify":
                        if (dgvData.SelectedRows.Count != 1) return;

                        SBRM_ROBOT_ROUTE_MST objSelModify = (dgvData.SelectedRows[0].DataBoundItem) as SBRM_ROBOT_ROUTE_MST;

                        SBRM_ROBOT_ROUTE_MST objProcess = null;

                        if (objSelModify.OBJECTKEY == 0)
                        {
                            // 修改的是尚未Submit的新增
                            var objAddModify = (from d in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_ROUTE_MST>() where d.SERVERNAME == objSelModify.SERVERNAME && d.LINETYPE == objSelModify.LINETYPE && d.ROUTENAME == objSelModify.ROUTENAME  &&  d.ROUTEID== objSelModify.ROUTEID select d).FirstOrDefault();

                            if (objAddModify != null)
                            {
                                objProcess = objAddModify;
                            }
                            else
                                return;
                        }
                        else
                            objProcess = objSelModify;

                        FormRobotRouteMSTEdit _frmModify = new FormRobotRouteMSTEdit(objProcess);
                        _frmModify.StartPosition = FormStartPosition.CenterScreen;
                        _frmModify.ShowDialog();
                        _frmModify.Dispose();

                        this.GetGridViewData();
                        break;

                    case "Delete":
                        if (dgvData.SelectedRows.Count == 0) return;

                        DialogResult result = this.QuectionMessage(this, this.lblCaption.Text, "Are you sure to delete selected records?");
                        if (result == System.Windows.Forms.DialogResult.No)
                            return;

                        SBRM_ROBOT_ROUTE_MST objEach = null, objtoDelete = null;

                        foreach (DataGridViewRow selectedRow in dgvData.SelectedRows)
                        {
                            objEach = selectedRow.DataBoundItem as SBRM_ROBOT_ROUTE_MST;


                            if (FindOtherRobotTableIndex(objEach) == true)
                            {
                                return;
                            }

                            if (objEach.OBJECTKEY > 0)
                            {
                                ctxBRM.SBRM_ROBOT_ROUTE_MST.DeleteOnSubmit(objEach);
                            }
                            else
                            {
                                objtoDelete = (from proc in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_ROUTE_MST>() where proc.LINETYPE.Equals(objEach.LINETYPE) && proc.SERVERNAME.Equals(objEach.SERVERNAME) && proc.ROUTENAME.Equals(objEach.ROUTENAME) && proc.ROUTEID.Equals(objEach.ROUTEID) select proc).FirstOrDefault();

                                if (objtoDelete != null)
                                    ctxBRM.SBRM_ROBOT_ROUTE_MST.DeleteOnSubmit(objtoDelete);
                            }
                        }

                        this.GetGridViewData();

                        break;
                    case "Save":

                        try
                        {
                            string _sqlDesc = string.Empty;
                            string _sqlErr = string.Empty;

                            try
                            {
                                #region Add
                                foreach (object objToInsert in ctxBRM.GetChangeSet().Inserts)
                                {
                                    if (objToInsert.GetType().Name != "SBRM_ROBOT_ROUTE_MST") continue;
                                    SBRM_ROBOT_ROUTE_MST _updateData = (SBRM_ROBOT_ROUTE_MST)objToInsert;

                                    _updateData.LASTUPDATETIME = DateTime.Now;

                                    _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("ADD [{0},{1},{2},{3},{4},{5},{6},{7} ", _updateData.ROUTEID, _updateData.ROBOTNAME, _updateData.ROUTENAME, _updateData.SERVERNAME, _updateData.LINETYPE, _updateData.DESCRIPTION, _updateData.ISENABLED,_updateData.ROUTEPRIORITY);
                                }
                                #endregion

                                #region Modify
                                foreach (object objToModify in ctxBRM.GetChangeSet().Updates)
                                {
                                    if (objToModify.GetType().Name != "SBRM_ROBOT_ROUTE_MST") continue;
                                    SBRM_ROBOT_ROUTE_MST _updateData = (SBRM_ROBOT_ROUTE_MST)objToModify;

                                    _updateData.LASTUPDATETIME = DateTime.Now;

                                    _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [{0},{1},{2},{3},{4},{5},{6},{7} ", _updateData.ROUTEID, _updateData.ROBOTNAME, _updateData.ROUTENAME, _updateData.SERVERNAME, _updateData.LINETYPE, _updateData.DESCRIPTION, _updateData.ISENABLED,_updateData.ROUTEPRIORITY);

                                    string _errModify = UniTools.InsertOPIHistory_DB("SBRM_ROBOT_ROUTE_MST", _sqlDesc, _sqlErr);

                                    #region 若為修改，將會一併把condition資料表內相同的RoutePriority數值修改
                                    objSelModify = (SBRM_ROBOT_ROUTE_MST)objToModify;

                                    var objConditionModify = (from d in ctxBRM.SBRM_ROBOT_ROUTE_CONDITION where d.ROBOTNAME.Equals(objSelModify.ROBOTNAME) && d.ROUTEID.Equals(objSelModify.ROUTEID) select d).ToList();

                                    for (int i = 0; i < objConditionModify.Count();i++ )
                                    {
                                        objConditionModify[i].ROUTEPRIORITY = objSelModify.ROUTEPRIORITY;
                                        foreach (object objToConditionModify in ctxBRM.GetChangeSet().Updates)
                                        {
                                            if (objToModify.GetType().Name != "SBRM_ROBOT_ROUTE_CONDITION") continue;
                                            SBRM_ROBOT_ROUTE_CONDITION _updateDataCondition = (SBRM_ROBOT_ROUTE_CONDITION)objToConditionModify;

                                            _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [{0},{1},{2},{3},{4},{5}]", _updateDataCondition.ROBOTNAME, _updateDataCondition.ROUTEID, _updateDataCondition.ROUTEPRIORITY, _updateDataCondition.DESCRIPTION, _updateDataCondition.ISENABLED, _updateDataCondition.REMARKS);
                                        }
                                        _errModify = UniTools.InsertOPIHistory_DB("SBRM_ROBOT_ROUTE_CONDITION", _sqlDesc, _sqlErr);

                                        if (_errModify != string.Empty)
                                        {
                                            ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _errModify, MessageBoxIcon.Error);
                                        }

                                        Public.SendDatabaseReloadRequest("SBRM_ROBOT_ROUTE_CONDITION");
                                        ShowMessage(this, lblCaption.Text, "Robot Route Condition contains the same information,will modify Decsripion field ！", _errModify, MessageBoxIcon.Information);
                                    }

                                    #endregion

                                }
                                #endregion

                                #region DELETE
                                foreach (object objToDelete in ctxBRM.GetChangeSet().Deletes)
                                {
                                    if (objToDelete.GetType().Name != "SBRM_ROBOT_ROUTE_MST") continue;
                                    SBRM_ROBOT_ROUTE_MST _updateData = (SBRM_ROBOT_ROUTE_MST)objToDelete;

                                    _updateData.LASTUPDATETIME = DateTime.Now;

                                    _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("DEL [{0},{1},{2},{3},{4},{5},{6},{7} ", _updateData.ROUTEID, _updateData.ROBOTNAME, _updateData.ROUTENAME, _updateData.SERVERNAME, _updateData.LINETYPE, _updateData.DESCRIPTION, _updateData.ISENABLED, _updateData.ROUTEPRIORITY);
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
                            string _err = UniTools.InsertOPIHistory_DB("SBRM_ROBOT_ROUTE_MST", _sqlDesc, _sqlErr);

                            if (_err != string.Empty)
                            {
                                ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _err, MessageBoxIcon.Error);
                            }
                            #endregion

                            Public.SendDatabaseReloadRequest("SBRM_ROBOT_ROUTE_MST");

                            if (_sqlErr == string.Empty)
                            {
                                ShowMessage(this, lblCaption.Text, "", "Robot Route Master Save Success!", MessageBoxIcon.Information);
                            }
                            this.GetGridViewData();
                        }
                        catch (Exception ex)
                        {
                            NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                            ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                        }

                        break;
                    case "Refresh":
                        FormMainMDI.G_OPIAp.RefreshDBBRMCtx();
                        this.GetGridViewData();
                        break;
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
            #region 產生同TYPE上有的RobotName
            try
            {
                string _nodeID = string.Empty;
                string _unitID = string.Empty;
                string _unitKey = string.Empty;
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                var _robot = (from rb in ctxBRM.SBRM_ROBOT
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
                    _rb.Location = new Point(10+(120*i), 15);
                    if (i == 0)
                    {
                        _rb.Checked = true;
                        _robotName = _robot[i].ROBOTNAME;
                    }
                    _rb.Click +=new EventHandler(RadioButton_Click);
                    pnlRobot.Controls.Add(_rb);

                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

        private bool FindOtherRobotTableIndex(SBRM_ROBOT_ROUTE_MST ObjEdit)
        {
            #region 找尋其他Table是否有使用此Data(會再修改)
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                    var _condition = (from q in ctxBRM.SBRM_ROBOT_ROUTE_CONDITION where q.ROBOTNAME.Equals(ObjEdit.ROBOTNAME) && q.ROUTEID.Equals(ObjEdit.ROUTEID) && q.SERVERNAME.Equals(ObjEdit.SERVERNAME) && q.LINETYPE.Equals(ObjEdit.LINETYPE) select q).Distinct().ToList();

                    if (_condition == null || _condition.Count == 0)
                    {
                        var _filter = (from filter in ctxBRM.SBRM_ROBOT_RULE_FILTER where filter.ROBOTNAME.Equals(ObjEdit.ROBOTNAME) && filter.ROUTEID.Equals(ObjEdit.ROUTEID) && filter.SERVERNAME.Equals(ObjEdit.SERVERNAME) select filter).Distinct().ToList();

                        if (_filter == null || _filter.Count == 0)
                        {
                            var _order = (from order in ctxBRM.SBRM_ROBOT_RULE_ORDERBY where order.ROBOTNAME.Equals(ObjEdit.ROBOTNAME) && order.ROUTEID.Equals(ObjEdit.ROUTEID) && order.SERVERNAME.Equals(ObjEdit.SERVERNAME) select order).Distinct().ToList();

                            if (_order == null || _order.Count == 0)
                            {
                                var _handle = (from handle in ctxBRM.SBRM_ROBOT_PROC_RESULT_HANDLE where handle.ROBOTNAME.Equals(ObjEdit.ROBOTNAME) && handle.ROUTEID.Equals(ObjEdit.ROUTEID) && handle.SERVERNAME.Equals(ObjEdit.SERVERNAME) select handle).Distinct().ToList();

                                if (_handle == null || _handle.Count == 0)
                                {
                                    var _stageSelect = (from stageSelect in ctxBRM.SBRM_ROBOT_RULE_STAGE_SELECT where stageSelect.ROBOTNAME.Equals(ObjEdit.ROBOTNAME) && stageSelect.ROUTEID.Equals(ObjEdit.ROUTEID) && stageSelect.SERVERNAME.Equals(ObjEdit.SERVERNAME) select stageSelect).Distinct().ToList();

                                    if (_handle == null || _handle.Count == 0)
                                    {
                                        var _byPass = (from byPass in ctxBRM.SBRM_ROBOT_RULE_ROUTESTEP_BYPASS where byPass.ROBOTNAME.Equals(ObjEdit.ROBOTNAME) && byPass.ROUTEID.Equals(ObjEdit.ROUTEID) && byPass.SERVERNAME.Equals(ObjEdit.SERVERNAME) select byPass).Distinct().ToList();

                                        if (_byPass == null || _byPass.Count == 0)
                                        {
                                            var _jump = (from jump in ctxBRM.SBRM_ROBOT_RULE_ROUTESTEP_JUMP where jump.ROBOTNAME.Equals(ObjEdit.ROBOTNAME) && jump.ROUTEID.Equals(ObjEdit.ROUTEID) && jump.SERVERNAME.Equals(ObjEdit.SERVERNAME) select jump).Distinct().ToList();

                                            if (_jump == null || _jump.Count == 0)
                                            {
                                                return false;
                                            }
                                            else
                                            {
                                                ShowMessage(this, lblCaption.Text, "", "This Data has been reference in ROBOT RULE ROUTESTEP_JUMP  Data, it can not delete ", MessageBoxIcon.Error);
                                                return true;
                                            }
                                        }
                                        else
                                        {
                                            ShowMessage(this, lblCaption.Text, "", "This Data has been reference in ROBOT RULE ROUTESTEP_BYPASS  Data, it can not delete ", MessageBoxIcon.Error);
                                            return true;
                                        }
                                    }
                                    else
                                    {
                                        ShowMessage(this, lblCaption.Text, "", "This Data has been reference in ROBOT RULE STAGE SELECT  Data, it can not delete ", MessageBoxIcon.Error);
                                        return true;
                                    }
                                }
                                else
                                {
                                    ShowMessage(this, lblCaption.Text, "", "This Data has been reference in ROBOT RULE RESULT HANDLE Data, it can not delete ", MessageBoxIcon.Error);
                                    return true;
                                }
                            }
                            else
                            {
                                ShowMessage(this, lblCaption.Text, "", "This Data has been reference in ROBOT RULE ORDERBY Data, it can not delete ", MessageBoxIcon.Error);
                                return true;
                            }
                        }
                        else
                        {
                            ShowMessage(this, lblCaption.Text, "", "This Data has been reference in ROBOT RULE FILTER Data, it can not delete ", MessageBoxIcon.Error);
                            return true;
                        }
                    }
                    else
                    {
                        ShowMessage(this, lblCaption.Text, "", "This Data has been reference in ROBOT ROUTE CONDITION Data, it can not delete ", MessageBoxIcon.Error);
                        return true;
                    }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                return true;
            }
            #endregion
        }

        private void RadioButton_Click(object sender, EventArgs e)
        {
            #region 選取RobotName ，讀取資料
            try
            {
                RadioButton rb = (RadioButton)sender;
                if (rb.Checked == true)
                {
                    rb.Checked = true;
                    _robotName = rb.Name;
                    GetGridViewData();
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
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                if (dgvData.DataSource == null || dgvData.Rows.Count == 0) return;

                #region 若GridView資料有變更，改變變更資料的顏色

                ctxBRM.GetChangeSet();

                foreach (DataGridViewRow dr in dgvData.Rows)
                {
                    SBRM_ROBOT_ROUTE_MST obj = dr.DataBoundItem as SBRM_ROBOT_ROUTE_MST;

                    if (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_ROUTE_MST>().Any(
                        msg => msg.ROUTEID == obj.ROUTEID && msg.ROUTENAME == obj.ROUTENAME))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(214, 233, 177);
                    }
                    else if (ctxBRM.GetChangeSet().Deletes.OfType<SBRM_ROBOT_ROUTE_MST>().Any(
                        msg => msg.ROUTEID == obj.ROUTEID && msg.ROUTENAME == obj.ROUTENAME))
                    {
                        dr.DefaultCellStyle.BackColor = Color.Red;
                    }
                    else if (ctxBRM.GetChangeSet().Updates.OfType<SBRM_ROBOT_ROUTE_MST>().Any(
                        msg => msg.ROUTEID == obj.ROUTEID && msg.ROUTENAME == obj.ROUTENAME))
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
}
