using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniClientTools;

namespace UniOPI
{
    public partial class FormRobotStageManagement : FormBase
    {
        string CurRobotName = string.Empty;
        List<SBRM_ROBOT_STAGE> objTables = null;
        List<bool> asc = new List<bool>();

        public FormRobotStageManagement()
        {
            InitializeComponent();
        }

        private void FormRobotStageManagement_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < dgvData.Columns.Count; i++)
            {
                asc.Add(false);
            }
            InitialRadioBtn();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            CheckChangeSave();
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                FormRobotStageManagementEdit frmAdd = new FormRobotStageManagementEdit(null);
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

                SBRM_ROBOT_STAGE objSelModify = (dgvData.SelectedRows[0].DataBoundItem) as SBRM_ROBOT_STAGE;
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                SBRM_ROBOT_STAGE objRobotStage = null;

                if (objSelModify.OBJECTKEY == 0)
                {
                    // 修改的是尚未Submit的新增
                    var objAddModify = (from robot in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_STAGE>()
                                        where robot.LINEID == objSelModify.LINEID && robot.STAGEID == objSelModify.STAGEID &&
                                        robot.ROBOTNAME == objSelModify.ROBOTNAME
                                        select robot).FirstOrDefault();
                    if (objAddModify != null)
                    {
                        objRobotStage = objAddModify;
                    }
                    else return;
                }
                else
                {
                    objRobotStage = objSelModify;
                }

                FormRobotStageManagementEdit frmModify = new FormRobotStageManagementEdit(objRobotStage);
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
                SBRM_ROBOT_STAGE objEach = null, objToDelete = null;
                foreach (DataGridViewRow selectedRow in dgvData.SelectedRows)
                {
                    objEach = selectedRow.DataBoundItem as SBRM_ROBOT_STAGE;
                    if (objEach.OBJECTKEY > 0)
                    {
                        ctxBRM.SBRM_ROBOT_STAGE.DeleteOnSubmit(objEach);
                    }
                    else
                    {
                        objToDelete = (from robot in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_STAGE>()
                                       where robot.SERVERNAME == objEach.SERVERNAME && robot.STAGEID == objEach.STAGEID &&
                                       robot.ROBOTNAME == objEach.ROBOTNAME
                                       select robot).FirstOrDefault();

                        if (objToDelete != null)
                            ctxBRM.SBRM_ROBOT_STAGE.DeleteOnSubmit(objToDelete);
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
            dgvCellBackColor();
        }

        private void dgvData_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            dgvCellBackColor();
            //dgvData.DataSource = null;
            //if (objTables != null)
            //{
            //    switch (dgvData.Columns[e.ColumnIndex].Name)
            //    {
            //        case "colStageID":
            //            if (asc[e.ColumnIndex] == false)
            //                objTables = objTables.OrderByDescending(o => o.STAGEID).ToList();
            //            else
            //                objTables = objTables.OrderBy(o => o.STAGEID).ToList();
            //            break;

            //        case "colStageName":
            //            if (asc[e.ColumnIndex] == false)
            //                objTables = objTables.OrderByDescending(o => o.STAGENAME).ToList();
            //            else
            //                objTables = objTables.OrderBy(o => o.STAGENAME).ToList();
            //            break;

            //        case "colReMarks":
            //            if (asc[e.ColumnIndex] == false)
            //                objTables = objTables.OrderByDescending(o => o.REMARKS).ToList();
            //            else
            //                objTables = objTables.OrderBy(o => o.REMARKS).ToList();
            //            break;

            //        case "colLocalNo":
            //            if (asc[e.ColumnIndex] == false)
            //                objTables = objTables.OrderByDescending(o => o.NODENO).ToList();
            //            else
            //                objTables = objTables.OrderBy(o => o.NODENO).ToList();
            //            break;

            //    }
            //    if (asc[e.ColumnIndex] == false)
            //        asc[e.ColumnIndex] = true;
            //    else
            //        asc[e.ColumnIndex] = false;

            //}
            //dgvData.DataSource = objTables;
        }

        private void InitialRadioBtn()
        {
            try
            {
                foreach (Robot _rb in FormMainMDI.G_OPIAp.Dic_Robot.Values)
                {
                    RadioButton _rd = new RadioButton();
                    _rd.Name = _rb.RobotName;
                    _rd.Text = _rb.RobotName;
                    _rd.AutoSize = false;
                    _rd.Font = new Font("Calibri", 12);
                    _rd.Size = new Size(150, 30);
                    _rd.Checked = false;
                    _rd.CheckedChanged += new EventHandler(RobotName_CheckedChanged);

                    flpRobot.Controls.Add(_rd);

                    if (CurRobotName == string.Empty )
                    {
                        CurRobotName = _rb.RobotName;
                        _rd.Checked = true;
                    }

                    #region 1 Fork 2Job的Line Type需要显示取片优先级DB中SBRM_ROBOT_STAGE中Remak字段），其他Line隐藏 By box.Zhai

                    if (FormMainMDI.G_OPIAp.CurLine.ServerName.ToString().Contains("CHN") ||
                                        FormMainMDI.G_OPIAp.CurLine.ServerName.ToString().Contains("SOR") ||
                                        FormMainMDI.G_OPIAp.CurLine.ServerName.ToString().Contains("CRP") ||
                                        FormMainMDI.G_OPIAp.CurLine.ServerName.ToString().Contains("RWT"))
                    {
                        pnlArmUsePriority.Visible = true;
                        pnlUseSpecificArm.Visible = true;
                    }
                    else
                    {
                        pnlArmUsePriority.Visible = false;
                        pnlUseSpecificArm.Visible = false;
                    }

                    #endregion

                }

                //Button _btn = new Button();
                //_btn.Name = "btnRefresh";
                //_btn.Text  = "Refresh";
                //_btn.Size = new Size(89, 38);
                //_btn.Font = new Font("Calibri", 13);
                //_btn.Click += new EventHandler(btnRefresh_Click);
                //flpRobot.Controls.Add(_btn);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void GetGridViewData(bool showMessage = false)
        {
            try
            {
                ClearDeatilInfo();

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                // 資料庫資料
                var selData = from msg in ctxBRM.SBRM_ROBOT_STAGE
                              where msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName && msg.ROBOTNAME == CurRobotName
                              select msg;

                // 已修改未更新物件
                var addData = ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_STAGE>().Where(
                    msg => msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName);

                objTables = selData.ToList();
                objTables.AddRange(addData.ToList());

                SortableBindingList<SBRM_ROBOT_STAGE> _fieldList = new SortableBindingList<SBRM_ROBOT_STAGE>(objTables);

                BindingSource _source = new BindingSource();

                _source.DataSource = _fieldList;

                dgvData.AutoGenerateColumns = false;

                dgvData.DataSource = _source;

                //dgvData.AutoGenerateColumns = false;
                //dgvData.DataSource = objTables;

                if (objTables.Count == 0 && showMessage == true)
                    ShowMessage(this, lblCaption.Text , "", "No matching data for your query！", MessageBoxIcon.Information);
                else
                {
                    dgvData.ClearSelection();

                    if (dgvData.Rows.Count > 0)
                    {
                        dgvData_CellClick(dgvData.Rows[0].Cells[0], new DataGridViewCellEventArgs(0, 0));
                    }
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

        private void dgvCellBackColor()
        {
            try
            {
                if (dgvData.DataSource == null || dgvData.Rows.Count == 0) return;

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                ctxBRM.GetChangeSet();
                foreach (DataGridViewRow dr in dgvData.Rows)
                {
                    SBRM_ROBOT_STAGE objData = dr.DataBoundItem as SBRM_ROBOT_STAGE;

                    if (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_STAGE>().Any(
                        msg => msg.LINEID == objData.LINEID && msg.STAGEID == objData.STAGEID &&
                                       msg.ROBOTNAME == objData.ROBOTNAME))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(214, 233, 177);
                    }
                    else if (ctxBRM.GetChangeSet().Deletes.OfType<SBRM_ROBOT_STAGE>().Any(
                        msg => msg.LINEID == objData.LINEID && msg.STAGEID == objData.STAGEID &&
                                       msg.ROBOTNAME == objData.ROBOTNAME))
                    {
                        dr.DefaultCellStyle.BackColor = Color.Red;
                    }
                    else if (ctxBRM.GetChangeSet().Updates.OfType<SBRM_ROBOT_STAGE>().Any(
                        msg => msg.LINEID == objData.LINEID && msg.STAGEID == objData.STAGEID &&
                                       msg.ROBOTNAME == objData.ROBOTNAME))
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

        private void RobotName_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                RadioButton _rdo = (RadioButton)sender;

                string _robotName = _rdo.Name;

                ClearModify();
                GetGridViewData(true);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

                DataGridViewRow _rowData = dgvData.Rows[e.RowIndex];

                if (_rowData == null) return;

                ShowDeatilInfo(_rowData);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void ClearDeatilInfo()
        {
            try
            {
                txtStageName.Text = string.Empty ;
                txtStageIDByNode.Text = string.Empty ;
                txtStageType.Text = string.Empty ;
                txtPriority.Text = string.Empty ;
                txtStageReportTrxName.Text = string.Empty ;
                txtStageJobDataTrxName.Text = string.Empty;
                txtSlotMaxCount.Text = string.Empty ;
                txtTrackDataSeqList.Text = string.Empty;
                txtCassetteType.Text = string.Empty;
                txtUseSpecificArm.Text = string.Empty;
                txtSlotFetchSeq.Text = string.Empty;
                txtSlotStoreSeq.Text = string.Empty;
                txtExchangeType.Text = string.Empty;
                txtEQRobotIfType.Text = string.Empty;
                txtUpstreamPathTrxName.Text = string.Empty;
                txtUpstreamSendPathTrxName.Text = string.Empty;
                txtDownstreamPathTrxName.Text = string.Empty;
                txtDownstreamReceivePathTrxName.Text = string.Empty;                

                chkIsMultiSlot.Checked = false;
                chkRecipeCheckFlag.Checked = false;
                chkDummyCheckFlag.Checked = false;
                chkGetReadyFlag.Checked = false;
                chkPutReadyFlag.Checked = false;
                chkPrefetchFlag.Checked = false;
                chkwaitFrontFlag.Checked = false;
                chkStageEnabled.Checked = false;
                chkRTCReworkFlag.Checked = false;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void ShowDeatilInfo(DataGridViewRow _rowData)
        {
            try
            {
                if (_rowData == null) return;

                #region 顯示 detail data 
                txtStageName.Text = _rowData.Cells[colStageName.Name].Value == null ? string.Empty : _rowData.Cells[colStageName.Name].Value.ToString();
                txtStageIDByNode.Text = _rowData.Cells[colStageIDByNode.Name].Value == null ? string.Empty : _rowData.Cells[colStageIDByNode.Name].Value.ToString();
                txtStageType.Text = _rowData.Cells[colStageType.Name].Value == null ? string.Empty : _rowData.Cells[colStageType.Name].Value.ToString();
                txtPriority.Text = _rowData.Cells[colPriority.Name].Value == null ? string.Empty : _rowData.Cells[colPriority.Name].Value.ToString();
                txtStageReportTrxName.Text = _rowData.Cells[colStageReportTrxName.Name].Value == null ? string.Empty : _rowData.Cells[colStageReportTrxName.Name].Value.ToString();
                txtStageJobDataTrxName.Text = _rowData.Cells[colStageJobDataTrxName.Name].Value == null ? string.Empty : _rowData.Cells[colStageJobDataTrxName.Name].Value.ToString();
                txtSlotMaxCount.Text = _rowData.Cells[colSlotMaxCount.Name].Value == null ? string.Empty : _rowData.Cells[colSlotMaxCount.Name].Value.ToString();
                txtTrackDataSeqList.Text = _rowData.Cells[colTrackDataSeqList.Name].Value == null ? string.Empty : _rowData.Cells[colTrackDataSeqList.Name].Value.ToString();
                txtCassetteType.Text = _rowData.Cells[colCassetteType.Name].Value == null ? string.Empty : _rowData.Cells[colCassetteType.Name].Value.ToString();
                string[] remark = _rowData.Cells[colRemark.Name].Value == null ? new string[0] : _rowData.Cells[colRemark.Name].Value.ToString().Split(',');
                if (remark.Length >= 2)
                {
                    txtArmUsePriority.Text = remark[0];
                    txtUseSpecificArm.Text = remark[1];
                }
                else if (remark.Length == 1)
                {
                    txtArmUsePriority.Text = remark[0];
                }
                else
                {
                    txtArmUsePriority.Text = string.Empty;
                    txtUseSpecificArm.Text = string.Empty;
                }
                txtSlotFetchSeq.Text = _rowData.Cells[colSlotFetchSeq.Name].Value == null ? string.Empty : _rowData.Cells[colSlotFetchSeq.Name].Value.ToString();
                txtSlotStoreSeq.Text = _rowData.Cells[colSlotStoreSeq.Name].Value == null ? string.Empty : _rowData.Cells[colSlotStoreSeq.Name].Value.ToString();
                txtExchangeType.Text = _rowData.Cells[colExchangeType.Name].Value == null ? string.Empty : _rowData.Cells[colExchangeType.Name].Value.ToString();
                txtEQRobotIfType.Text = _rowData.Cells[colEQRobotIfTypre.Name].Value == null ? string.Empty : _rowData.Cells[colEQRobotIfTypre.Name].Value.ToString();

                txtUpstreamPathTrxName.Text = _rowData.Cells[colUpstreamPathTrxName.Name].Value == null ? string.Empty : _rowData.Cells[colUpstreamPathTrxName.Name].Value.ToString();
                txtUpstreamSendPathTrxName.Text = _rowData.Cells[colUpstreamSendPathTrxName.Name].Value == null ? string.Empty : _rowData.Cells[colUpstreamSendPathTrxName.Name].Value.ToString();                
                txtDownstreamPathTrxName.Text = _rowData.Cells[colDownstreamPathTrxName.Name].Value == null ? string.Empty : _rowData.Cells[colDownstreamPathTrxName.Name].Value.ToString();
                txtDownstreamReceivePathTrxName.Text = _rowData.Cells[colDownstreamReceivePathTrxName.Name].Value == null ? string.Empty : _rowData.Cells[colDownstreamReceivePathTrxName.Name].Value.ToString();

                chkIsMultiSlot.Checked = (_rowData.Cells[colIsMultiSlot.Name].Value.ToString().ToUpper() == "Y" ? true : false);
                chkRTCReworkFlag.Checked = (_rowData.Cells[colRTCReworkFlag.Name].Value.ToString().ToUpper() == "Y" ? true : false);
                chkRecipeCheckFlag.Checked = (_rowData.Cells[colRecipeCheckFlag.Name].Value.ToString().ToUpper() == "Y" ? true : false);
                chkDummyCheckFlag.Checked = (_rowData.Cells[colDummyCheckFlag.Name].Value.ToString().ToUpper() == "Y" ? true : false);
                chkGetReadyFlag.Checked = (_rowData.Cells[colGetReadyFlag.Name].Value.ToString().ToUpper() == "Y" ? true : false);
                chkPutReadyFlag.Checked = (_rowData.Cells[colPutReadyFlag.Name].Value.ToString().ToUpper() == "Y" ? true : false);
                chkPrefetchFlag.Checked = (_rowData.Cells[colPrefetchFlag.Name].Value.ToString().ToUpper() == "Y" ? true : false);
                chkwaitFrontFlag.Checked = (_rowData.Cells[colSupportWaitFront.Name].Value.ToString().ToUpper() == "Y" ? true : false);
                chkStageEnabled.Checked = (_rowData.Cells[colStageEnabled.Name].Value.ToString().ToUpper() == "Y" ? true : false);
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

                List<string> _lstModifyStageID = new List<string>();  //記錄有修改tracking data的stage id

                try
                {
                    #region 取得更新的data

                    //新增
                    foreach (object objToInsert in _ctxBRM.GetChangeSet().Inserts)
                    {
                        if (objToInsert.GetType().Name != "SBRM_ROBOT_STAGE") continue;

                        SBRM_ROBOT_STAGE _updateData = (SBRM_ROBOT_STAGE)objToInsert;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("ADD [ {0} , {1} , {2} ] ", _updateData.NODENO, _updateData.ROBOTNAME, _updateData.STAGENAME);
                    }

                    //delete
                    foreach (object objToDelete in _ctxBRM.GetChangeSet().Deletes)
                    {
                        if (objToDelete.GetType().Name != "SBRM_ROBOT_STAGE") continue;

                        SBRM_ROBOT_STAGE _updateData = (SBRM_ROBOT_STAGE)objToDelete;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("DEL [ {0} , {1} , {2} ] ", _updateData.NODENO, _updateData.ROBOTNAME, _updateData.STAGENAME);
                    }

                    //modify
                    foreach (object objToUpdate in _ctxBRM.GetChangeSet().Updates)
                    {
                        if (objToUpdate.GetType().Name != "SBRM_ROBOT_STAGE") continue;

                        SBRM_ROBOT_STAGE _updateData = (SBRM_ROBOT_STAGE)objToUpdate;

                        _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [ {0} , {1} , {2} ] ", _updateData.NODENO, _updateData.ROBOTNAME, _updateData.STAGENAME);

                        #region 判斷tracking data定義是否有修改，若有則需將robot route內對應的tracking data清為000...並pop message 通知user重新設定route

                        System.Data.Linq.ModifiedMemberInfo[] _modifys = _ctxBRM.SBRM_ROBOT_STAGE.GetModifiedMembers(_updateData);

                        if (_modifys.Where(r => r.Member.Name.Equals("TRACKDATASEQLIST")).Count() > 0)
                        {
                            _lstModifyStageID.Add(_updateData.STAGEID);
                        }

                        #endregion
                    }

                    #endregion

                    if (_sqlDesc == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, string.Empty, "No Data for Update！", MessageBoxIcon.Warning);
                        return;
                    }


                    #region 將有變更tracking data的stage id 設定清為0 (SBRM_ROBOT_ROUTE_STEP)
                    foreach (string _stageID in _lstModifyStageID)
                    {
                        var _var = _ctxBRM.SBRM_ROBOT_ROUTE_STEP.Where(r => r.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) && r.STAGEIDLIST.IndexOf(_stageID) >= 0);

                        if (_var.Count() <= 0) continue;

                        foreach (SBRM_ROBOT_ROUTE_STEP _route in _var.ToList())
                        {
                            _route.INPUTTRACKDATA = "0".PadLeft(32, '0');
                            _route.OUTPUTTRACKDATA = "0".PadLeft(32, '0');
                        }
                    }
                    #endregion

                    _ctxBRM.SubmitChanges(System.Data.Linq.ConflictMode.ContinueOnConflict);
                }
                catch (System.Data.Linq.ChangeConflictException err)
                {
                    #region
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
                    #endregion
                }

                #region 紀錄opi history
                string _err = UniTools.InsertOPIHistory_DB("SBRM_ROBOT_STAGE", _sqlDesc, _sqlErr);

                if (_err != string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _err, MessageBoxIcon.Error);
                }
                #endregion

                Public.SendDatabaseReloadRequest("SBRM_ROBOT_STAGE");

                if (_sqlErr == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Robot Stage Save Success！", MessageBoxIcon.Information);
                }

                #region 將有變更tracking data的stage id pop message 通知user 重設route
                _sqlDesc = string.Empty;
                foreach (string _stageID in _lstModifyStageID)
                {
                    _sqlDesc = _sqlDesc + string.Format("\r\n Stage ID [{0}] ", _stageID);
                }
                if (_sqlDesc != string.Empty)
                {
                    _sqlDesc = "Please Reset Robot Route. Tracking Data of Stage ID is Modify " + _sqlDesc;
                    ShowMessage(this, lblCaption.Text, string.Empty, _sqlDesc, MessageBoxIcon.Warning);
                }
                #endregion

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
