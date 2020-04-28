using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormRobotRuleJobSelectEdit : FormBase
    {
        private SBRM_ROBOT_RULE_JOB_SELECT ObjEdit;

        UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
        ucRBMethod MethodDef = new ucRBMethod();

        public FormRobotRuleJobSelectEdit()
        {
            InitializeComponent();
            this.lblCaption.Text = "Robot Route Job Select Edit";
        }

   
        public FormRobotRuleJobSelectEdit(SBRM_ROBOT_RULE_JOB_SELECT objProcess)
            : this()
        {
            if (objProcess == null)
            {
                FormMode = UniOPI.FormMode.AddNew;
                dgvData.Visible = true;
                pnlAdd.Visible = true;
                Size = new Size(851, 650);
            }
            else
            {
                FormMode = UniOPI.FormMode.Modify;
                ObjEdit = objProcess;
                dgvData.Visible = false;
                pnlAdd.Visible = false;
                Size = new Size(851, 410);
            }
        }

        private void FormRobotRuleJobSelectEdit_Load(object sender, EventArgs e)
        {            
            try
            {
                #region Load Robot Combobox , Object Name & Method Name
                cboRobotName.DataSource = FormMainMDI.G_OPIAp.Dic_Robot.Values.ToList();
                cboRobotName.DisplayMember = "RobotName";
                cboRobotName.ValueMember = "RobotName";
                cboRobotName.SelectedIndex = -1;

                cboRobotName.SelectedIndexChanged += new EventHandler(cboRobotName_SelectedIndexChanged);

                SetStageType();

                MethodDef.Location = new Point(6, 134);
                pnlNew.Controls.Add(MethodDef);

                MethodDef.InitialRBMethod("SELECT");
                MethodDef.Select_cbo += new ucRBMethod.CboClickDelegate(_methodDef_CboSelect);

                #endregion

                if (FormMode == UniOPI.FormMode.Modify)
                {
                    //UK 不可修改 : Server Name + Robot Name + Item ID + Select Type +  + Object Name + Method Name
                    cboRobotName.Enabled = false;
                    txtSelectType.Enabled = false;
                    txtItemID.Enabled = false;
                    MethodDef.SetEnable(false);

                    if (ObjEdit != null)
                    {
                        cboRobotName.SelectedItem= ObjEdit.ROBOTNAME;
                        txtSelectType.Text = ObjEdit.SELECTTYPE;
                        txtItemID.Text = ObjEdit.ITEMID;
                        txtItemSeq.Text = ObjEdit.ITEMSEQ.ToString() ;
                        cboStageType.SelectedValue = ObjEdit.STAGETYPE;
                        txtDescription.Text = ObjEdit.DESCRIPTION;
                        MethodDef.SetObjectName(ObjEdit.OBJECTNAME);
                        MethodDef.SetMethName(ObjEdit.METHODNAME);
                        txtRemark.Text = ObjEdit.REMARKS;
                        chkIsEnable.Checked = ObjEdit.ISENABLED.Equals("Y") ? true : false;
                    }
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
                            Robot _rb = ((Robot)cboRobotName.SelectedItem);

                            dgvData.Rows.Add(chkIsEnable.Checked, _rb.ServerName, _rb.RobotName, txtItemID.Text, _rb.LineType, cboStageType.SelectedValue.ToString(), txtSelectType.Text.ToString(), string.IsNullOrEmpty(txtItemSeq.Text) ? "0" : txtItemSeq.Text, txtDescription.Text, MethodDef.GetObjectName(), MethodDef.GetMethodName(), MethodDef.GetFunName(), txtRemark.Text, DateTime.Now);
                            
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

                                SBRM_ROBOT_RULE_JOB_SELECT _objAdd = null;

                                foreach (DataGridViewRow _row in dgvData.Rows)
                                {
                                    _objAdd = new SBRM_ROBOT_RULE_JOB_SELECT();

                                    _objAdd.OBJECTKEY = 0;
                                    _objAdd.SERVERNAME = _row.Cells[colServerName.Name].Value.ToString();
                                    _objAdd.ROBOTNAME = _row.Cells[colRobotName.Name].Value.ToString();
                                    _objAdd.ITEMID = _row.Cells[colItemID.Name].Value.ToString();
                                    _objAdd.LINETYPE = _row.Cells[colLineType.Name].Value.ToString();
                                    _objAdd.SELECTTYPE = _row.Cells[colSelectType.Name].Value.ToString();
                                    _objAdd.ITEMSEQ = int.Parse(_row.Cells[colItemSeq.Name].Value.ToString());
                                    _objAdd.DESCRIPTION = string.Empty;
                                    _objAdd.OBJECTNAME = _row.Cells[colObjectName.Name].Value.ToString();
                                    _objAdd.STAGETYPE = _row.Cells[colStageType.Name].Value.ToString();
                                    _objAdd.METHODNAME = _row.Cells[colMethodName.Name].Value.ToString();
                                    _objAdd.ISENABLED = _row.Cells[chkIsEnabled.Name].Value.ToString().Equals("True") ? "Y" : "N";
                                    _objAdd.REMARKS = _row.Cells[colRemarks.Name].Value.ToString();
                                    _objAdd.LASTUPDATETIME = Convert.ToDateTime(_row.Cells[colLastUpdateTime.Name].Value.ToString());
 
                                    ctxBRM.SBRM_ROBOT_RULE_JOB_SELECT.InsertOnSubmit(_objAdd);
                                }

                                break;
                                #endregion

                            case FormMode.Modify:

                                #region Modify -- UK 不可修改 : Server Name + Robot Name + Item ID + Select Type ID +  + Object Name + Method Name

                                if (ObjEdit == null) return;

                                ObjEdit.ITEMSEQ = string.IsNullOrEmpty(txtItemSeq.Text) ? 0 : int.Parse(txtItemSeq.Text);
                                ObjEdit.STAGETYPE = cboStageType.SelectedValue.ToString();                                    
                                ObjEdit.ISENABLED = chkIsEnable.Checked ? "Y" : "N";
                                ObjEdit.LASTUPDATETIME = DateTime.Now;
                                ObjEdit.REMARKS = txtRemark.Text;
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
            #endregion
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
                
                foreach (Control _ctrl in pnlNew.Controls)
                {
                    if (_ctrl is TextBox && String.IsNullOrEmpty(_ctrl.Text) == true && (_ctrl.Name.Equals(txtItemID.Name)))
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please Enter Value " + _ctrl.Name.Replace("txt", "") + " !", MessageBoxIcon.Error);
                        _ctrl.Focus();
                        return true;
                    }
                    else if (_ctrl is ComboBox && ((_ctrl.Name.Equals(txtSelectType.Name))  || (_ctrl.Name.Equals(cboStageType.Name) && cboStageType.SelectedIndex == -1)))
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

                if (MethodDef.GetMethodName() == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please Choose MethodName !", MessageBoxIcon.Error);
                    return true;
                }

                if (cboStageType.SelectedValue == null)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please Choose Stage Type !", MessageBoxIcon.Error);
                    return true;
                }
                #endregion

                #region 檢查資料已存在 or 已新增但尚未儲存

                Robot _rb = ((Robot)cboRobotName.SelectedItem);
                string _objectName = MethodDef.GetObjectName();
                string _methodName = MethodDef.GetMethodName();
                string _itemID = txtItemID.Text.ToString();
                string _stageType = cboStageType.SelectedValue.ToString();
                string _selectType = txtSelectType.Text.ToString();

                //rowCnt表示現在已存在的表格  UK : Server Name + Robot Name + Item ID + Select Type +  + Object Name + Method Name
                var rowCnt = (from msg in ctxBRM.SBRM_ROBOT_RULE_JOB_SELECT
                              where msg.ROBOTNAME.Equals(_rb.RobotName) &&
                                    msg.OBJECTNAME.Equals(_objectName) &&
                                    msg.METHODNAME.Equals(_methodName) &&
                                    msg.ITEMID.Equals(_itemID) &&
                                    msg.STAGETYPE.Equals(_stageType) &&
                                    msg.SELECTTYPE.Equals(_selectType)
                              select msg).ToList();

                //ChangeSet表示已經準備新增及修改的表格
                var ChangeSet = (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_RULE_JOB_SELECT>().Where(msg =>
                            msg.ROBOTNAME.Equals(_rb.RobotName) &&
                            msg.METHODNAME.Equals(_methodName) &&
                            msg.OBJECTNAME.Equals(_objectName) &&
                            msg.ITEMID.Equals(_itemID) &&
                            msg.STAGETYPE.Equals(_stageType) &&
                            msg.SELECTTYPE.Equals(_selectType))).ToList();

                //當Mode為新增時，rowCnt和ChangeSet若有值則為新增到重複數值，則報錯 
                if ((FormMode == UniOPI.FormMode.AddNew && (rowCnt.Count() > 0 || ChangeSet.Count() > 0)) )
                {
                    string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is Robot Name [{0}], Route ID[{1}], Item ID[{2}] !", cboRobotName.SelectedItem.ToString(), txtSelectType.Text.ToString(), txtItemID.Text);
                    ShowMessage(this, this.lblCaption.Text, "", errMsg, MessageBoxIcon.Warning);
                    return true;
                }

                #endregion

                #region 檢查新增在datagridview內的資料是否重複，若重複，詢問是否要覆蓋

                var _findNewRow = (from d in dgvData.Rows.Cast<DataGridViewRow>().Where(d => d.Cells[colRobotName.Name].Value.ToString().Equals(_rb.RobotName) &&
                                                                                        d.Cells[colSelectType.Name].Value.ToString().Equals(_selectType) &&
                                                                                        d.Cells[colStageType.Name].Value.ToString().Equals(_stageType) &&
                                                                                        d.Cells[colItemID.Name].Value.ToString().Equals(_itemID) &&
                                                                                        d.Cells[colObjectName.Name].Value.ToString().Equals(_objectName) &&
                                                                                        d.Cells[colMethodName.Name].Value.ToString().Equals(_methodName))
                                   select d).FirstOrDefault();

                if (_findNewRow != null)
                {
                    string _msg = string.Format("Robot Name [{0}] , Route ID [{1}] , Item ID [{2}]  is already insert. Please confirm whether you will overwite data ?", _rb.RobotName, txtSelectType.Text.ToString(), txtItemID.Text);

                    if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return true;
                    _findNewRow.Cells[colSelectType.Name].Value = _selectType;
                    _findNewRow.Cells[colStageType.Name].Value = _stageType;
                    _findNewRow.Cells[colMethodName.Name].Value = _methodName;
                    _findNewRow.Cells[colObjectName.Name].Value = _objectName;
                    _findNewRow.Cells[colItemID.Name].Value = _itemID;
                    _findNewRow.Cells[colItemSeq.Name].Value = string.IsNullOrEmpty(txtItemSeq.Text) ? "0" : txtItemSeq.Text;
                    _findNewRow.Cells[colDesciption.Name].Value = txtDescription.Text;
                    _findNewRow.Cells[chkIsEnabled.Name].Value = chkIsEnable.Checked;
                    _findNewRow.Cells[colLastUpdateTime.Name].Value = DateTime.Now;
                    _findNewRow.Cells[colRemarks.Name].Value = txtRemark.Text;
                    

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
            #region 清除資料
            try
            {
                foreach (Control _clear in pnlNew.Controls)
                {
                    if (_clear is TextBox)
                    {
                        _clear.Text = string.Empty;
                    }
                }
                txtSelectType.Text = "Common";
                chkIsEnable.Checked = true;
                cboStageType.SelectedIndex = -1;
                MethodDef.ClearMethod();
                cboRobotName.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

        private void SetStageType()
        {
            try
            {
                List<comboxInfo> _lstStageType = new List<comboxInfo>();
                _lstStageType.Add(new comboxInfo { ITEM_ID = "ALL", ITEM_NAME = "All Stage" });
                _lstStageType.Add(new comboxInfo { ITEM_ID = "PORT", ITEM_NAME = "Cassette Port" });
                _lstStageType.Add(new comboxInfo { ITEM_ID = "STAGE", ITEM_NAME = "Indexer inside Stage" });
                _lstStageType.Add(new comboxInfo { ITEM_ID = "FIXBUFFER", ITEM_NAME = "Indexer Fix Buffer" });
                _lstStageType.Add(new comboxInfo { ITEM_ID = "EQUIPMENT", ITEM_NAME = "Equipment" });
                _lstStageType.Add(new comboxInfo { ITEM_ID = "ROBOTSTAGE", ITEM_NAME = "RobotStage" });

                cboStageType.DataSource = _lstStageType.ToList();
                cboStageType.DisplayMember = "ITEM_NAME";
                cboStageType.ValueMember = "ITEM_ID";
                cboStageType.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void txtItemSeq_KeyPress(object sender, KeyPressEventArgs e)
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
            #region 點選GridView資料讓資料移至編輯區讓人修改
            try
            {
                if (e.RowIndex != -1)
                {
                    cboRobotName.SelectedValue = dgvData.Rows[e.RowIndex].Cells[colRobotName.Name].Value.ToString();
                    txtItemSeq.Text = dgvData.Rows[e.RowIndex].Cells[colItemSeq.Name].Value.ToString();
                    MethodDef.SetObjectName(dgvData.Rows[e.RowIndex].Cells[colObjectName.Name].Value.ToString());
                    MethodDef.SetMethName(dgvData.Rows[e.RowIndex].Cells[colMethodName.Name].Value.ToString());
                    txtRemark.Text = dgvData.Rows[e.RowIndex].Cells[colRemarks.Name].Value.ToString();
                    txtSelectType.Text = dgvData.Rows[e.RowIndex].Cells[colSelectType.Name].Value.ToString();
                    cboStageType.SelectedValue = dgvData.Rows[e.RowIndex].Cells[colStageType.Name].Value.ToString();
                    chkIsEnable.Checked = dgvData.Rows[e.RowIndex].Cells[chkIsEnabled.Name].Value.ToString().Equals("True") ? true : false;
                    txtDescription.Text = dgvData.Rows[e.RowIndex].Cells[colDesciption.Name].Value.ToString();
                    txtItemID.Text = dgvData.Rows[e.RowIndex].Cells[colItemID.Name].Value.ToString();
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

        private void cboRobotName_SelectedIndexChanged(object sender, System.EventArgs e)
        {

            if (cboRobotName.SelectedIndex != -1)
            {
                string _robotName = ((Robot)cboRobotName.SelectedItem).RobotName;

                var _routeId = (from routeID in ctxBRM.SBRM_ROBOT_ROUTE_MST where routeID.ISENABLED.Equals("Y") && routeID.ROBOTNAME.Equals(_robotName) select routeID.ROUTEID).Distinct().ToList();
                
                if (_routeId.Count == 0)
                {
                    ShowMessage(this, lblCaption.Text, "", "Can't find the Route ID data ! ", MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void _methodDef_CboSelect(object sender, EventArgs e)
        {
            txtItemID.Text = MethodDef.GetMethodName();
            txtDescription.Text = MethodDef.GetDescription();
        }
    }
}
