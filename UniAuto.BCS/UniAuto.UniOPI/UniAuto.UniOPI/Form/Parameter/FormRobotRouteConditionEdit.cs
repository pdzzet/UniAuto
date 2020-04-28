using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormRobotRouteConditionEdit : FormBase
    {

        private SBRM_ROBOT_ROUTE_CONDITION ObjEdit;
        List<int> lstRoutPriority = new List<int>();
        UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
        ucRBMethod MethodDef = new ucRBMethod();

        public FormRobotRouteConditionEdit()
        {
            InitializeComponent();
            this.lblCaption.Text = "Robot Route Condition Edit";
        }

        public FormRobotRouteConditionEdit(SBRM_ROBOT_ROUTE_CONDITION objProcess)
            : this()
        {
            if (objProcess == null)
            {
                FormMode = UniOPI.FormMode.AddNew;
                dgvData.Visible = true;
                pnlAdd.Visible = true;
                Size = new Size(851, 627);
            }
            else
            {
                FormMode = UniOPI.FormMode.Modify;
                ObjEdit = objProcess;
                dgvData.Visible = false;
                pnlAdd.Visible = false;
                Size = new Size(851, 395);
            }
        }

        private void FormRobotRouteConditionEdit_Load(object sender, EventArgs e)
        {            
            try
            {
                #region Load Robot Combobox , Object Name & Method Name

                cboRobotName.DataSource = FormMainMDI.G_OPIAp.Dic_Robot.Values.ToList();
                cboRobotName.DisplayMember = "RobotName";
                cboRobotName.ValueMember = "RobotName";
                cboRobotName.SelectedIndex = -1;

                cboRobotName.SelectedIndexChanged += new EventHandler(cboRobotName_SelectedIndexChanged);

                MethodDef.Location = new Point(0, 124);
                pnlNew.Controls.Add(MethodDef);

                MethodDef.InitialRBMethod("ROUTECONDITION");
                MethodDef.Select_cbo += new ucRBMethod.CboClickDelegate(_methodDef_CboSelect);

                #endregion

                #region Load Modify Data
                if (FormMode == UniOPI.FormMode.Modify)
                {
                    //UK 不可修改 : Server Name + Robot Name + RouteID + Condition ID +  + Object Name + Method Name
                    cboRobotName.Enabled = false;
                    cboRouteID.Enabled = false;
                    txtConditionID.Enabled = false;
                    MethodDef.SetEnable (false);

                    if (ObjEdit != null)
                    {
                        cboRobotName.SelectedValue = ObjEdit.ROBOTNAME;
                        cboRouteID.SelectedItem = ObjEdit.ROUTEID;
                        txtConditionID.Text = ObjEdit.CONDITIONID;
                        txtDescription.Text = ObjEdit.DESCRIPTION;
                        MethodDef.SetObjectName(ObjEdit.OBJECTNAME);
                        MethodDef.SetMethName(ObjEdit.METHODNAME);
                        txtRemark.Text = ObjEdit.REMARKS;
                        txtRoutePriority.Text = ObjEdit.ROUTEPRIORITY.ToString();
                        txtConditionSeq.Text = ObjEdit.CONDITIONSEQ.ToString();
                        chkIsEnable.Checked = ObjEdit.ISENABLED.Equals("Y") ? true : false;
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

        private void btn_Click(object sender, EventArgs e)
        {            
            try
            {
                Button _btn = (Button)sender;

                switch (_btn.Tag.ToString())
                {
                    case "Add":

                        #region Insert to datagridview

                        if (CheckData() == false)
                        {
                            Robot _rb = ((Robot)cboRobotName.SelectedItem);

                            dgvData.Rows.Add(0, _rb.RobotName, cboRouteID.SelectedItem.ToString(), txtConditionID.Text, _rb.ServerName, _rb.LineType, txtDescription.Text, MethodDef.GetObjectName(), MethodDef.GetMethodName(), MethodDef.GetFunName(), chkIsEnable.Checked, txtRemark.Text, DateTime.Now, string.IsNullOrEmpty(txtConditionSeq.Text) ? "0" : txtConditionSeq.Text, txtRoutePriority.Text);
                            
                            ClearData();
                        }

                        break;

                        #endregion
                       
                    case "Close":

                        this.DialogResult = System.Windows.Forms.DialogResult.Cancel;

                        break;

                    case "Clear":

                        ClearData();

                        break;

                    case "OK":

                        switch (FormMode)
                        {
                            case FormMode.AddNew:

                                #region Add

                                if (dgvData.Rows.Count < 1)
                                {
                                    ShowMessage(this, lblCaption.Text, "", "Please Add New Data !", MessageBoxIcon.Error);
                                    return;
                                }

                                SBRM_ROBOT_ROUTE_CONDITION _objAdd = null;

                                foreach (DataGridViewRow _row in dgvData.Rows)
                                {
                                    _objAdd = new SBRM_ROBOT_ROUTE_CONDITION();

                                    _objAdd.OBJECTKEY = 0;
                                    _objAdd.ROUTEID = _row.Cells[colRouteID.Name].Value.ToString();
                                    _objAdd.ROBOTNAME = _row.Cells[colRobotName.Name].Value.ToString();
                                    _objAdd.CONDITIONID = _row.Cells[colConditionId.Name].Value.ToString();
                                    _objAdd.SERVERNAME = _row.Cells[colServerName.Name].Value.ToString();
                                    _objAdd.LINETYPE = _row.Cells[colLineType.Name].Value.ToString();
                                    _objAdd.OBJECTNAME = _row.Cells[colObjectName.Name].Value.ToString();
                                    _objAdd.METHODNAME = _row.Cells[colMethodName.Name].Value.ToString();
                                    _objAdd.ISENABLED = _row.Cells[chkIsEnabled.Name].Value.ToString().Equals("True") ? "Y" : "N";
                                    _objAdd.REMARKS = _row.Cells[colRemarks.Name].Value.ToString();
                                    _objAdd.LASTUPDATETIME = Convert.ToDateTime(_row.Cells[colLastUpdateTime.Name].Value.ToString());
                                    _objAdd.CONDITIONSEQ = int.Parse(_row.Cells[colConditionSeq.Name].Value.ToString());
                                    _objAdd.ROUTEPRIORITY = int.Parse(_row.Cells[colRoutePriority.Name].Value.ToString());
                                    _objAdd.DESCRIPTION = string.Empty ;

                                    ctxBRM.SBRM_ROBOT_ROUTE_CONDITION.InsertOnSubmit(_objAdd);

                                }

                                break;
                                #endregion

                            case FormMode.Modify:

                                #region Modify -- UK 不可修改 : Server Name + Robot Name + RouteID + Condition ID +  + Object Name + Method Name

                                if (ObjEdit == null) return;

                                ObjEdit.ISENABLED = chkIsEnable.Checked ? "Y" : "N";
                                ObjEdit.LASTUPDATETIME = DateTime.Now;
                                ObjEdit.REMARKS = txtRemark.Text;                                    
                                ObjEdit.CONDITIONSEQ = string.IsNullOrEmpty(txtConditionSeq.Text) ? 0 : int.Parse(txtConditionSeq.Text);
                                ObjEdit.DESCRIPTION = string.Empty;
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
        }

        private bool CheckData()
        {            
            try
            {
                #region Check 必填欄位
                if (cboRobotName.SelectedValue == null)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please Choose Robot Name ", MessageBoxIcon.Error);
                    return true;
                }

                Robot _rb = ((Robot)cboRobotName.SelectedItem);
 
                foreach (Control _ctrl in pnlNew.Controls)
                {
                    if (_ctrl is TextBox && String.IsNullOrEmpty(_ctrl.Text) == true && _ctrl.Name.Equals(txtConditionID.Name))
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please Enter Value " + _ctrl.Name.Replace("txt", "") + " !", MessageBoxIcon.Error);
                        _ctrl.Focus();
                        return true;
                    }
                    else if (_ctrl is ComboBox && _ctrl.Name.Equals(cboRouteID.Name) && cboRouteID.SelectedIndex == -1) 
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please Choose " + _ctrl.Tag.ToString() + " !", MessageBoxIcon.Error);
                        _ctrl.Focus();
                        return true;
                    }
                }

                if (MethodDef.GetObjectName() == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please Choose ObjectName !", MessageBoxIcon.Error);
                    return true;
                }

                else if (MethodDef.GetMethodName() == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please Choose MethodName !", MessageBoxIcon.Error);
                    return true;
                }

                #endregion

                #region 檢查資料已存在 or 已新增但尚未儲存

                //rowCnt表示現在已存在的表格 -- UK: Server Name + Robot Name + RouteID+ Condition ID +  + Object Name + Method Name
                var rowCnt = (from msg in ctxBRM.SBRM_ROBOT_ROUTE_CONDITION
                              where msg.SERVERNAME.Equals(_rb.ServerName) &&
                                    msg.ROBOTNAME.Equals(_rb.RobotName) && 
                                    msg.ROUTEID.Equals(cboRouteID.SelectedItem.ToString()) &&
                                    msg.METHODNAME.Equals(MethodDef.GetMethodName()) &&
                                    msg.OBJECTNAME.Equals(MethodDef.GetObjectName()) && 
                                    msg.CONDITIONID.Equals(txtConditionID.Text) select msg).ToList();

                //ChangeSet表示已經準備新增及修改的表格  -- UK: Server Name + Robot Name + RouteID+ Condition ID +  + Object Name + Method Name
                var ChangeSet = (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_ROUTE_CONDITION>().Where(d =>
                    d.ROBOTNAME.Equals(_rb.RobotName) &&
                    d.SERVERNAME.Equals(_rb.ServerName) &&
                    d.CONDITIONID.Equals(txtConditionID.Text) &&
                    d.METHODNAME.Equals(MethodDef.GetMethodName()) &&
                    d.OBJECTNAME.Equals(MethodDef.GetObjectName()) && 
                    d.ROUTEID.Equals(cboRouteID.SelectedItem.ToString()))).ToList();

                //1.當Mode為新增時，rowCnt和ChangeSet若有值則為新增到重複數值，則報錯 
                if (FormMode == UniOPI.FormMode.AddNew && (rowCnt.Count() > 0 || ChangeSet.Count() > 0))
                {
                    string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is Robot Name [{0}] ,Route ID [{1}] ,Condition ID [{2}]！", _rb.RobotName, cboRouteID.SelectedItem.ToString(), txtConditionID.Text);
                    ShowMessage(this, this.lblCaption.Text, "", errMsg, MessageBoxIcon.Warning);
                    return true;
                }

                #endregion

                #region 檢查新增在datagridview內的資料是否重複，若重複，詢問是否要覆蓋
                var _findNewRow = (from d in dgvData.Rows.Cast<DataGridViewRow>().Where(d => d.Cells[colRobotName.Name].Value.ToString().Equals(_rb.RobotName) &&
                                                                                        d.Cells[colRouteID.Name].Value.ToString().Equals(cboRouteID.SelectedItem.ToString()) &&
                                                                                        d.Cells[colConditionId.Name].Value.ToString().Equals(txtConditionID.Text))
                                    select d).FirstOrDefault();

                if (_findNewRow != null)
                {
                    string _msg = string.Format("Robot Name [{0}] , Robot ID [{1}],Condition ID[{2}] is already insert. Please confirm whether you will overwite data ?", _rb.RobotName, cboRouteID.SelectedItem.ToString(), txtConditionID.Text);

                    if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return true;
                    _findNewRow.Cells[colDesciption.Name].Value = txtDescription.Text;
                    _findNewRow.Cells[colObjectName.Name].Value = MethodDef.GetObjectName();
                    _findNewRow.Cells[colMethodName.Name].Value = MethodDef.GetMethodName();
                    _findNewRow.Cells[colFuncKey.Name].Value = MethodDef.GetFunName();
                    _findNewRow.Cells[chkIsEnabled.Name].Value = chkIsEnable.Checked;
                    _findNewRow.Cells[colLastUpdateTime.Name].Value = DateTime.Now;
                    _findNewRow.Cells[colRemarks.Name].Value = txtRemark.Text;
                    _findNewRow.Cells[colRoutePriority.Name].Value = string.IsNullOrEmpty(txtRoutePriority.Text) ? 0 : int.Parse(txtRoutePriority.Text);
                    _findNewRow.Cells[colConditionSeq.Name].Value = string.IsNullOrEmpty(txtConditionSeq.Text) ? 0 : int.Parse(txtConditionSeq.Text);

                    ClearData();
                    
                    return true;
                }
                #endregion

                return false;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                return true;
            }           
        }

        private void ClearData()
        {
            #region 資料清除
            try
            {
                foreach (Control _clear in pnlNew.Controls)
                {
                    if (_clear is TextBox)
                    {
                        _clear.Text = string.Empty;
                    }
                }
                chkIsEnable.Checked = true;
                cboRobotName.SelectedIndex = -1;
                cboRouteID.SelectedIndex = -1;
                MethodDef.ClearMethod();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

        private void txtConditionSeq_KeyPress(object sender, KeyPressEventArgs e)
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

        private void dgvData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            #region 點選表格讓資料回填欄位
            try
            {
                if (e.RowIndex != -1)
                {
                    cboRobotName.SelectedValue = dgvData.Rows[e.RowIndex].Cells[colRobotName.Name].Value.ToString();
                    MethodDef.SetObjectName(dgvData.Rows[e.RowIndex].Cells[colObjectName.Name].Value.ToString());
                    MethodDef.SetMethName(dgvData.Rows[e.RowIndex].Cells[colMethodName.Name].Value.ToString());
                    txtConditionID.Text = dgvData.Rows[e.RowIndex].Cells[colConditionId.Name].Value.ToString();
                    txtConditionSeq.Text = dgvData.Rows[e.RowIndex].Cells[colConditionSeq.Name].Value.ToString();
                    txtDescription.Text = dgvData.Rows[e.RowIndex].Cells[colDesciption.Name].Value.ToString();
                    txtRemark.Text = dgvData.Rows[e.RowIndex].Cells[colRemarks.Name].Value.ToString();
                    txtRoutePriority.Text = dgvData.Rows[e.RowIndex].Cells[colRoutePriority.Name].Value.ToString();
                    cboRouteID.SelectedItem = dgvData.Rows[e.RowIndex].Cells[colRouteID.Name].Value.ToString();
                    chkIsEnable.Checked = dgvData.Rows[e.RowIndex].Cells[chkIsEnabled.Name].Value.ToString().Equals("True") ? true : false;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

        private void cboRobotName_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cboRobotName.SelectedIndex != -1)
                {
                    string _robotName = ((Robot)cboRobotName.SelectedItem).RobotName;
                   
                    var _routeId = (from routeID in ctxBRM.SBRM_ROBOT_ROUTE_MST 
                                    where routeID.ISENABLED.Equals("Y") && routeID.ROBOTNAME.Equals(_robotName) 
                                    select new { routeID.ROUTEID, routeID.ROUTEPRIORITY }).Distinct().ToList();

                    if (_routeId.Count == 0)
                    {
                        cboRouteID.DataSource = null;
                        ShowMessage(this, lblCaption.Text, "", "Can't find the Route ID data ! ", MessageBoxIcon.Error);
                        return;
                    }
                    else
                    {
                        cboRouteID.DataSource = _routeId.Select(r=>r.ROUTEID).ToList();
                        cboRouteID.SelectedIndex = -1;
                        lstRoutPriority.AddRange(_routeId.Select(r => r.ROUTEPRIORITY).ToList());
                    }
                }
                else
                {
                    cboRouteID.DataSource = null;
                    lstRoutPriority = new List<int>();
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cboRouteID_SelectionChangeCommitted(object sender, EventArgs e)
        {
            try
            {
                txtRoutePriority.Text = lstRoutPriority[cboRouteID.SelectedIndex].ToString();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void _methodDef_CboSelect(object sender, EventArgs e)
        {
            try
            {
                txtConditionID.Text = MethodDef.GetMethodName();
                txtDescription.Text = MethodDef.GetDescription();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
    }
}
