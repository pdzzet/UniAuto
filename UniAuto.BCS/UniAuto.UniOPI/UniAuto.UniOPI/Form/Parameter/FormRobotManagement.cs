using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Collections.Generic;

namespace UniOPI
{
    public partial class FormRobotManagement : FormBase
    {
        public FormRobotManagement()
        {
            InitializeComponent();

            this.Load += new EventHandler(FormRobotManagement_Load);
            //畫面上按鈕、GridView按鈕事件增加
            btnSetting.Click +=new EventHandler(btnSetting_Click);
            btnRefresh.Click += new EventHandler(btnRefresh_Click);
            dgvData.CellClick +=new DataGridViewCellEventHandler(dgvData_CellClick);            
        }

        private void FormRobotManagement_Load(object sender, EventArgs e)
        {
            //顯示GridView資料
            #region 顯示GridView資料
            try
            {
                GetGridViewData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            #region  顯示GridView資料
            try
            {
                GetGridViewData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

        private void dgvData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            #region GridView按下後，將資料給Setting
            try
            {
                if (e.RowIndex != -1)
                {
                    lblRobotName.Text = dgvData.Rows[e.RowIndex].Cells[colRobotName.Name].Value.ToString();

                    cboPortFetchSEQ.SelectedItem = dgvData.Rows[e.RowIndex].Cells[colPortfetchSeq.Name].Value.ToString();
                    cboPortFetchSEQ.Tag = dgvData.Rows[e.RowIndex].Cells[colPortfetchSeq.Name].Value.ToString();

                    cboSlotFetchSEQ.SelectedItem = dgvData.Rows[e.RowIndex].Cells[colSlotfetchSeq.Name].Value.ToString();
                    cboSlotFetchSEQ.Tag = dgvData.Rows[e.RowIndex].Cells[colSlotfetchSeq.Name].Value.ToString();

                    cboPortStoreSEQ.SelectedItem = dgvData.Rows[e.RowIndex].Cells[colPortStoreSeq.Name].Value.ToString();
                    cboPortStoreSEQ.Tag = dgvData.Rows[e.RowIndex].Cells[colPortStoreSeq.Name].Value.ToString();

                    cboSlotStoreSEQ.SelectedItem = dgvData.Rows[e.RowIndex].Cells[colSlotStoreSeq.Name].Value.ToString();
                    cboSlotStoreSEQ.Tag = dgvData.Rows[e.RowIndex].Cells[colSlotStoreSeq.Name].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

        private void btnSetting_Click(object sender, EventArgs e)
        {
            #region 設定鈕處發後，先取得更新DATA，把變更的欄位寫入資料庫內，最後再記錄至OPI內，在重新顯示GridView
            UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
            try
            {
                string _sqlDesc = string.Empty;

                string _sqlErr = string.Empty;

                try
                {
                    if (CheckData() == false) return;

                    #region 取得更新的data
                    //modify
                    string _robotName = lblRobotName.Text.ToString();
                    string _portFetchSeq = cboPortFetchSEQ.SelectedItem.ToString();
                    string _portStoreSeq = cboPortStoreSEQ.SelectedItem.ToString();
                    string _slotFetchSeq = cboSlotFetchSEQ.SelectedItem.ToString();
                    string _slotStoreSeq = cboSlotStoreSEQ.SelectedItem.ToString();

                    var _modify = (from rb in _ctxBRM.SBRM_ROBOT
                                   where rb.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName && rb.ROBOTNAME == _robotName
                                   select rb).ToList();

                    foreach (SBRM_ROBOT _rb in _modify)
                    {
                        _rb.PORTFETCHSEQ = _portFetchSeq;
                        _rb.PORTSTORESEQ = _portStoreSeq;
                        _rb.SLOTFETCHSEQ = _slotFetchSeq;
                        _rb.SLOTSTORESEQ = _slotStoreSeq;
                    }

                    if (_portFetchSeq != cboPortFetchSEQ.Tag.ToString()) _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD PortFetchSEQ [ {0} ] ", _portFetchSeq);
                    if (_portStoreSeq != cboPortStoreSEQ.Tag.ToString()) _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD PortStoreSEQ [ {0} ] ", _portStoreSeq);
                    if (_slotFetchSeq != cboSlotFetchSEQ.Tag.ToString()) _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD SlotFetchSEQ [ {0} ] ", _slotFetchSeq);
                    if (_slotStoreSeq != cboSlotStoreSEQ.Tag.ToString()) _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD SlotStoreSEQ [ {0} ] ", _slotStoreSeq);
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
                string _err = UniTools.InsertOPIHistory_DB("SBRM_ROBOT", _sqlDesc, _sqlErr);

                if (_err != string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _err, MessageBoxIcon.Error);
                }
                #endregion

                Public.SendDatabaseReloadRequest("SBRM_ROBOT");

                if (_sqlErr == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Robot Data Save Success！", MessageBoxIcon.Information);
                }

                GetGridViewData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

        private bool CheckData()
        {
            #region 檢查ComboBox有無選擇值
            try
             {
                 if (lblRobotName.Text == string.Empty)
                 {
                     ShowMessage(this, lblCaption.Text, "", "Please Choose Robot", MessageBoxIcon.Error);
                     return false ;
                 }

                 if (cboPortFetchSEQ.SelectedItem == null || cboPortFetchSEQ.SelectedItem.ToString() == string.Empty)
                 {
                     ShowMessage(this, lblCaption.Text, "", "Please Choose Port Fetch Sequence", MessageBoxIcon.Error);
                     return false;
                 }

                 if (cboPortStoreSEQ.SelectedItem == null || cboPortStoreSEQ.SelectedItem.ToString() == string.Empty)
                 {
                     ShowMessage(this, lblCaption.Text, "", "Please Choose Port Store Sequence", MessageBoxIcon.Error);
                     return false;
                 }

                 if (cboSlotFetchSEQ.SelectedItem == null || cboSlotFetchSEQ.SelectedItem.ToString() == string.Empty)
                 {
                     ShowMessage(this, lblCaption.Text, "", "Please Choose Slot Fetch Sequence", MessageBoxIcon.Error);
                     return false;
                 }

                 if (cboSlotStoreSEQ.SelectedItem == null || cboSlotStoreSEQ.SelectedItem.ToString() == string.Empty)
                 {
                     ShowMessage(this, lblCaption.Text, "", "Please Choose Slot Store Sequence", MessageBoxIcon.Error);
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
            #endregion
        }

        private void GetGridViewData()
        {
            #region 顯示GridView內容，從DB ROBOT表格中提取特定內容
            try
            {
                string _nodeID = string.Empty;
                string _unitID = string.Empty;
                string _unitKey = string.Empty;

                dgvData.Rows.Clear();

                var q = (from rb in FormMainMDI.G_OPIAp.DBBRMCtx.SBRM_ROBOT where rb.LINEID.Equals(FormMainMDI.G_OPIAp.CurLine.LineID) && rb.LINETYPE.Equals(FormMainMDI.G_OPIAp.CurLine.LineType)
                         select rb).ToList();

                if (q == null || q.Count < 1)
                    return;

                foreach (var _data in q)
                {
                    if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_data.NODENO)) _nodeID = FormMainMDI.G_OPIAp.Dic_Node[_data.NODENO].NodeID;
                    else _nodeID = string.Empty;

                    // Key: NODENO(3) + UNITNO(2)
                    _unitKey = _data.NODENO.PadRight(3, ' ') + _data.UNITNO.PadLeft(2, '0');

                    if (FormMainMDI.G_OPIAp.Dic_Unit.ContainsKey(_unitKey)) _unitID = FormMainMDI.G_OPIAp.Dic_Unit[_unitKey].UnitID;
                    else _unitID = string.Empty;

                    dgvData.Rows.Add(_data.NODENO, _nodeID, _data.UNITNO, _unitID, _data.ROBOTNAME, _data.PORTFETCHSEQ, _data.SLOTFETCHSEQ, _data.PORTSTORESEQ, _data.SLOTSTORESEQ, _data.ROBOTARMQTY, _data.ARMJOBQTY, _data.REMARKS);
                }

                if (dgvData.Rows.Count > 0)
                {
                    dgvData_CellClick(dgvData.Rows[0].Cells[0], new DataGridViewCellEventArgs(0, 0));
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
}
