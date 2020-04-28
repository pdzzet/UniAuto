using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormRobotRouteMSTEdit : FormBase
    {
        private SBRM_ROBOT_ROUTE_MST ObjEdit;

        public FormRobotRouteMSTEdit()
        {
            InitializeComponent();
            this.lblCaption.Text = "Robot Route Master Edit";

        }

        public FormRobotRouteMSTEdit(SBRM_ROBOT_ROUTE_MST objProcess)
            : this()
        {
            if (objProcess == null)
            {
                FormMode = UniOPI.FormMode.AddNew;
                dgvData.Visible = true;
                Size = new Size(851, 615);
                btnAddNew.Tag = "Add";
                btnAddNew.Text = "Add";
                btnNewClose.Tag = "Clear";
                btnNewClose.Text = "Clear";
            }
            else
            {
                FormMode = UniOPI.FormMode.Modify;
                ObjEdit = objProcess;
                dgvData.Visible = false;
                Size = new Size(851, 370);
                btnAddNew.Tag = "Insert";
                btnAddNew.Text = "Modify";
                btnNewClose.Tag = "Close";
                btnNewClose.Text = "Close";

            }
        }

        private void FormRobotRouteMSTEdit_Load(object sender, EventArgs e)
        {
            #region Robot Table必須要先有相關資料，才能進行接下來的動作
            try
            {
                var _robotName =( from _name in FormMainMDI.G_OPIAp.DBBRMCtx.SBRM_ROBOT where _name.LINEID.Equals(FormMainMDI.G_OPIAp.CurLine.LineID) && _name.LINETYPE.Equals(FormMainMDI.G_OPIAp.CurLine.LineType) select _name.ROBOTNAME).ToList();
                if (_robotName == null || _robotName.Count() == 0)
                {
                    ShowMessage(this, lblCaption.Text, "", "Can't find the Robot Name data ! ", MessageBoxIcon.Error);
                    return;
                }
                else
                {
                    cboRobotName.DataSource = _robotName;
                    cboRobotName.SelectedIndex = -1;
                }

                if (FormMode == UniOPI.FormMode.Modify)
                {
                    cboRobotName.Enabled = false;
                    txtRoubeID.Enabled = false;
                    if (ObjEdit != null)
                    {
                        cboRobotName.SelectedItem = ObjEdit.ROBOTNAME;
                        txtRoubeID.Text = ObjEdit.ROUTEID;
                        txtRouteName.Text = ObjEdit.ROUTENAME;
                        txtRemark.Text = ObjEdit.REMARKS;
                        txtDescription.Text = ObjEdit.DESCRIPTION;
                        txtRemark.Text = ObjEdit.REMARKS;
                        txtRoutePriority.Text = ObjEdit.ROUTEPRIORITY.ToString();
                        chkIsEnable.Checked = ObjEdit.ISENABLED.Equals("Y") ? true : false;
                        chkRTCModeFlag.Checked = ObjEdit.RTCMODEFLAG.Equals("Y") ? true : false;
                        chkRTCForceReturnFlag.Checked = ObjEdit.RTCFORCERETURNFLAG.Equals("Y") ? true : false;
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

        private void btn_Click(object sender, EventArgs e)
        {
            #region 按鈕功能
            try
            {
                Button _btn = (Button)sender;

                switch (_btn.Tag.ToString())
                {
                    case "Add":
                        #region Insert to datagridview
                        if (CheckData() == false)
                        {
                            dgvData.Rows.Add(0, FormMainMDI.G_OPIAp.CurLine.ServerName, cboRobotName.SelectedItem.ToString(), txtRoubeID.Text, txtRouteName.Text, FormMainMDI.G_OPIAp.CurLine.LineType, chkIsEnable.Checked, chkRTCModeFlag.Checked, txtDescription.Text, txtRemark.Text, DateTime.Now, txtRoutePriority.Text, chkRTCForceReturnFlag.Checked);
                            ClearData();
                        }
                        #endregion
                        break;
                    case "Close":
                        this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                        break;
                    case "Clear":
                        ClearData();
                        break;
                    case "Insert":

                        UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                        switch (FormMode)
                        {
                            case FormMode.AddNew:

                                #region Add

                                if (dgvData.Rows.Count < 1)
                                {
                                    ShowMessage(this, lblCaption.Text, "", "Please Add New Data !", MessageBoxIcon.Error);
                                    return;
                                }

                                SBRM_ROBOT_ROUTE_MST _objAdd = null;

                                foreach (DataGridViewRow _row in dgvData.Rows)
                                {
                                    _objAdd = new SBRM_ROBOT_ROUTE_MST();

                                    _objAdd.OBJECTKEY = 0;
                                    _objAdd.ROUTEID = _row.Cells[colRouteID.Name].Value.ToString();
                                    _objAdd.ROBOTNAME = _row.Cells[colRobotName.Name].Value.ToString();
                                    _objAdd.ROUTENAME = _row.Cells[colRouteName.Name].Value.ToString();
                                    _objAdd.SERVERNAME = _row.Cells[colServerName.Name].Value.ToString();
                                    _objAdd.LINETYPE = _row.Cells[colLineType.Name].Value.ToString();
                                    _objAdd.DESCRIPTION = _row.Cells[colDesciption.Name].Value.ToString();
                                    _objAdd.ISENABLED = _row.Cells[colIsEnabled.Name].Value.ToString().Equals("True") ? "Y" : "N";
                                    _objAdd.RTCMODEFLAG = _row.Cells[colRTCModeFlag.Name].Value.ToString().Equals("True") ? "Y" : "N";
                                    _objAdd.REMARKS = _row.Cells[colRemarks.Name].Value.ToString();
                                    _objAdd.LASTUPDATETIME = Convert.ToDateTime(_row.Cells[colLastUpdateTime.Name].Value.ToString());
                                    _objAdd.ROUTEPRIORITY = int.Parse(_row.Cells[colRoutePriority.Name].Value.ToString());
                                    _objAdd.RTCFORCERETURNFLAG = _row.Cells[colRTCForceReturnFlag.Name].Value.ToString().Equals("True") ? "Y" : "N";
                                    _ctxBRM.SBRM_ROBOT_ROUTE_MST.InsertOnSubmit(_objAdd);                                    
                                }

                                break;
                                #endregion

                            case FormMode.Modify:

                                #region Modify

                                if (ObjEdit == null) return;
                           
                                    ObjEdit.ROUTENAME = txtRouteName.Text;
                                    ObjEdit.DESCRIPTION = txtDescription.Text;
                                    ObjEdit.ISENABLED = chkIsEnable.Checked ? "Y" : "N";
                                    ObjEdit.RTCMODEFLAG = chkRTCModeFlag.Checked ? "Y" : "N";
                                    ObjEdit.LASTUPDATETIME = DateTime.Now;
                                    ObjEdit.REMARKS = txtRemark.Text;
                                    ObjEdit.ROUTEPRIORITY = string.IsNullOrEmpty(txtRoutePriority.Text) ? 0 : int.Parse(txtRoutePriority.Text);
                                    ObjEdit.RTCFORCERETURNFLAG = chkRTCForceReturnFlag.Checked ? "Y" : "N";
                                break;
                                #endregion

                            default:
                                break;
                        }

                        this.DialogResult = System.Windows.Forms.DialogResult.OK;

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

        private bool CheckData()
        {
            #region 檢查有無資料未輸入、資料重複
            try
            {
                if (cboRobotName.SelectedIndex == -1)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please choose Robot Name !", MessageBoxIcon.Error);
                    cboRobotName.Focus();
                    return true;
                }

                foreach (Control _txt in pnlNewObject.Controls)
                {
                    if (_txt is TextBox && String.IsNullOrEmpty(_txt.Text) == true && (_txt.Name.Equals(txtRoubeID.Name) || _txt.Name.Equals(txtRouteName.Name)))
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please Enter Value " + _txt.Name.Replace("txt", "") + " !", MessageBoxIcon.Error);
                        _txt.Focus();
                        return true;
                    }
                }

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                var rowCnt = (from msg in ctxBRM.SBRM_ROBOT_ROUTE_MST where msg.ROBOTNAME.Equals(cboRobotName.SelectedItem.ToString()) && msg.ROUTEID.Equals(txtRoubeID.Text) select msg).Count();

                var ChangeSet = (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_ROUTE_MST>().Where(d => d.ROBOTNAME.Equals(cboRobotName.SelectedItem.ToString()) && d.ROUTEID.Equals(txtRoubeID.Text))).Count();

                if (rowCnt > 0 || ChangeSet > 0)
                {
                    string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is Robot Name[{0}], Route ID[{1}] !", cboRobotName.SelectedItem.ToString(), txtRoubeID.Text);
                    ShowMessage(this, this.lblCaption.Text, "", errMsg, MessageBoxIcon.Warning);
                    return true;
                }

                var findreplace = (from d in dgvData.Rows.Cast<DataGridViewRow>().Where(d => d.Cells[colRobotName.Name].Value.ToString().Equals(cboRobotName.SelectedItem.ToString()) && d.Cells[colRouteID.Name].Value.ToString().Equals(txtRoubeID.Text)) select d).Count();

                if (findreplace > 0)
                {
                    DataGridViewRow _addNewRow = dgvData.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[colRobotName.Name].Value.ToString().Equals(cboRobotName.SelectedItem.ToString())).First();

                    string _msg = string.Format("Robot Name [{0}], Route ID [{1}]  is already insert. Please confirm whether you will overwite data ?", cboRobotName.SelectedItem.ToString(), txtRoubeID.Text);

                    if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return true;
                    _addNewRow.Cells[colRouteName.Name].Value = txtRouteName.Text;
                    _addNewRow.Cells[colDesciption.Name].Value = txtDescription.Text;
                    _addNewRow.Cells[colIsEnabled.Name].Value = chkIsEnable.Checked;
                    _addNewRow.Cells[colLastUpdateTime.Name].Value = DateTime.Now;
                    _addNewRow.Cells[colRemarks.Name].Value = txtRemark.Text;
                    _addNewRow.Cells[colRTCModeFlag.Name].Value = chkRTCModeFlag.Checked;
                    _addNewRow.Cells[colRoutePriority.Name].Value = string.IsNullOrEmpty(txtRoutePriority.Text) ? 0 : int.Parse(txtRoutePriority.Text);
                    ClearData();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                return true;
            }
            #endregion
        }

        private void ClearData()
        {
            try
            {
                foreach (Control _clear in pnlNewObject.Controls)
                {
                    if (_clear is TextBox)
                    {
                        _clear.Text = string.Empty;
                    }
                }
                chkIsEnable.Checked = true;
                chkRTCModeFlag.Checked = false;
                chkRTCForceReturnFlag.Checked = false;
                cboRobotName.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            #region 點選GridView資料讓資料移至編輯區讓人修改
            try
            {
                if (e.RowIndex != -1)
                {
                    txtRoubeID.Text = dgvData.Rows[e.RowIndex].Cells[colRouteID.Name].Value.ToString();
                    txtRemark.Text = dgvData.Rows[e.RowIndex].Cells[colRemarks.Name].Value.ToString();
                    txtDescription.Text = dgvData.Rows[e.RowIndex].Cells[colDesciption.Name].Value.ToString();
                    txtRouteName.Text = dgvData.Rows[e.RowIndex].Cells[colRouteName.Name].Value.ToString();
                    txtRoutePriority.Text = dgvData.Rows[e.RowIndex].Cells[colRoutePriority.Name].Value.ToString();
                    cboRobotName.SelectedItem = dgvData.Rows[e.RowIndex].Cells[colRobotName.Name].Value.ToString();
                    chkIsEnable.Checked = dgvData.Rows[e.RowIndex].Cells[colIsEnabled.Name].Value.ToString().Equals("True") ? true : false;
                    chkRTCModeFlag.Checked = dgvData.Rows[e.RowIndex].Cells[colRTCModeFlag.Name].Value.ToString().Equals("True") ? true : false;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

        private bool CheckDBRelate()
        {
            #region 檢查新增修改項目是否有重複值
            try
            {
                var condition = (from q in FormMainMDI.G_OPIAp.DBBRMCtx.SBRM_ROBOT_ROUTE_CONDITION where q.ROBOTNAME.Equals(ObjEdit.ROBOTNAME) && q.ROUTEID.Equals(ObjEdit.ROUTEID) && q.SERVERNAME.Equals(ObjEdit.SERVERNAME) && q.LINETYPE.Equals(ObjEdit.LINETYPE) select q).ToList();

                if (condition == null || condition.Count() == 0)
                {
                    var jobSelect = (from d in FormMainMDI.G_OPIAp.DBBRMCtx.SBRM_ROBOT_RULE_JOB_SELECT where d.ROBOTNAME.Equals(ObjEdit.ROBOTNAME) && d.SELECTTYPE.Equals(ObjEdit.ROUTEID) && d.SERVERNAME.Equals(ObjEdit.SERVERNAME) && d.LINETYPE.Equals(ObjEdit.LINETYPE) select d).ToList();

                    if (jobSelect == null || jobSelect.Count == 0)
                    {
                        return true;
                    }
                    else
                    {
                        ShowMessage(this, lblCaption.Text, "", "This data in ROBOT RULE JOB SELECT has been use !, it can't  change Enable state", MessageBoxIcon.Error);
                        return false;
                    }
                }
                else
                {
                    ShowMessage(this, lblCaption.Text, "", "This data in ROBOT ROUTE CONDITION has been use !, it can't  change Enable state", MessageBoxIcon.Error);
                    return false;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                return false;
            }
            #endregion
        }

        private void chkIsEnable_CheckedChanged(object sender, EventArgs e)
        {
            #region 當有表格使用中，不能取消勾選
            if (ObjEdit!= null && chkIsEnable.Checked != true&& CheckDBRelate() == false )
            {
                chkIsEnable.Checked = true;
                return;
            }
            #endregion
        }

        private void txtRoutePriority_KeyPress(object sender, KeyPressEventArgs e)
        {
            int asciiCode = (int)e.KeyChar;
            if ((asciiCode >= 48 & asciiCode <= 57) |   // 0-9
                asciiCode == 8)   // Backspace
            {
                e.Handled = false;
            }
            else
                e.Handled = true;
        }

    }
}
