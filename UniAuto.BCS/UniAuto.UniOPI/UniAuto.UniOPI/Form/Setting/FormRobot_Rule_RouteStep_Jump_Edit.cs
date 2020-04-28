using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormRobot_Route_Step_Jump_Edit : FormBase
    {
        string CurRouteID = string.Empty;//RouteID
        int CurStepID = 0;//目前的Step
        Robot CurRobot = null;//Robot資訊

        List<int> LstStepID = null;//Step欄位
        ucRBMethod MethodDef = new ucRBMethod();
        SBRM_ROBOT_RULE_ROUTESTEP_JUMP ObjEdit = null;

        public FormRobot_Route_Step_Jump_Edit()
        {
            InitializeComponent();

            this.lblCaption.Text = "Route Step Jump Edit";
        }

        public FormRobot_Route_Step_Jump_Edit(string routeID, int stepID, Robot robot, SBRM_ROBOT_RULE_ROUTESTEP_JUMP objProcess, List<int> lstStepID)
            : this()
        {
            CurRouteID = routeID;
            CurStepID = stepID;
            CurRobot = robot;
            LstStepID = new List<int>(lstStepID);

            if (objProcess == null)
            {
                FormMode = UniOPI.FormMode.AddNew;
                dgvData.Visible = true;
                pnlAdd.Visible = true;
            }
            else
            {
                FormMode = UniOPI.FormMode.Modify;
                ObjEdit = objProcess;
                dgvData.Visible = false;
                pnlAdd.Visible = false;
                Size = new Size(853, 412);
            }
        }

        private void FormRobot_Route_Step_Jump_Edit_Load(object sender, EventArgs e)
        {
            try
            {
                #region Method & Object Def
                MethodDef.Location = new Point(6, 134);
                pnlNew.Controls.Add(MethodDef);

                MethodDef.InitialRBMethod("RouteStepJump");
                MethodDef.Select_cbo += new ucRBMethod.CboClickDelegate(_methodDef_CboSelect);
                #endregion

                InitialCombox();//產生選項

                txtRobotName.Text = CurRobot.RobotName;
                txtRouteID.Text = CurRouteID;
                txtStepID.Text = CurStepID.ToString();
                txtJumpItemSeq.Text = "";

                #region Load Modify Data
                if (FormMode == UniOPI.FormMode.Modify)
                {
                    //UK 不可修改 : ServerName + Robot Name + RouteID + Step ID +  + Item ID                   
                    MethodDef.SetEnable(false);

                    if (ObjEdit != null)
                    {
                        MethodDef.SetObjectName(ObjEdit.OBJECTNAME);
                        MethodDef.SetMethName(ObjEdit.METHODNAME);

                        txtJumpItemSeq.Text = ObjEdit.JUMPITEMSEQ.ToString();
                        txtRemarks.Text = ObjEdit.REMARKS;
                        cboGotoStepID.Text = ObjEdit.GOTOSTEPID.ToString();

                        if (ObjEdit.ISENABLED == "Y")
                            chkIsEnable.Checked = true;
                        else
                            chkIsEnable.Checked = false;
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

        private void InitialCombox()
        {
            #region 產生Method  & Object Name & ItemID & Description，Step資料
            try
            {
                #region Next Step ID

                LstStepID.Remove(CurStepID);

                LstStepID.Add(65535);

                cboGotoStepID.DataSource = LstStepID;

                cboGotoStepID.SelectedItem = 65535;
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

        private void txtItemSeq_KeyPress(object sender, KeyPressEventArgs e)
        {
            UniTools.CheckTextBoxKeyPressIsInteger(sender, e);
        }

        private void btn_Click(object sender, EventArgs e)
        {
            #region 按鈕功能 新增，修改，清除，確認
            try
            {
                Button _btn = (Button)sender;

                switch (_btn.Tag.ToString())
                {
                    case "Add":

                        #region Insert to datagridview
                        if (CheckData() == false)
                        {
                            dgvData.Rows.Add(CurRobot.ServerName, txtRobotName.Text, txtRouteID.Text, txtStepID.Text, txtJumpConditionID.Text, txtDescription.Text, cboGotoStepID.SelectedItem.ToString(), MethodDef.GetObjectName(), MethodDef.GetMethodName(), MethodDef.GetFunName(),txtJumpItemSeq.Text, chkIsEnable.Checked, DateTime.Now.ToString(), txtRemarks.Text);
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

                                SBRM_ROBOT_RULE_ROUTESTEP_JUMP _objAdd = null;

                                foreach (DataGridViewRow _row in dgvData.Rows)
                                {
                                    _objAdd = new SBRM_ROBOT_RULE_ROUTESTEP_JUMP();

                                    _objAdd.OBJECTKEY = 0;
                                    _objAdd.SERVERNAME = CurRobot.ServerName;
                                    _objAdd.ROBOTNAME = _row.Cells[colRobotName.Name].Value.ToString();
                                    _objAdd.JUMPCONDITIONID = _row.Cells[colJumpConditionID.Name].Value.ToString();
                                    _objAdd.ROUTEID = _row.Cells[colRouteID.Name].Value.ToString();
                                    _objAdd.STEPID = int.Parse(_row.Cells[colStepID.Name].Value.ToString());
                                    _objAdd.JUMPITEMSEQ = int.Parse(_row.Cells[colJumpItemSeq.Name].Value.ToString());
                                    _objAdd.REMARKS = txtRemarks.Text;
                                    _objAdd.DESCRIPTION = string.Empty;
                                    _objAdd.OBJECTNAME = _row.Cells[colObjectName.Name].Value.ToString();
                                    _objAdd.METHODNAME = _row.Cells[colMethodName.Name].Value.ToString();
                                    _objAdd.ISENABLED = _row.Cells[chkEnable.Name].Value.ToString().Equals("True") ? "Y" : "N";
                                    _objAdd.REMARKS = _row.Cells[colRemarks.Name].Value.ToString();
                                    _objAdd.LASTUPDATETIME = Convert.ToDateTime(_row.Cells[colLastUpdateTime.Name].Value.ToString());
                                    _objAdd.GOTOSTEPID = int.Parse(_row.Cells[colGotoStepID.Name].Value.ToString());
                                    _ctxBRM.SBRM_ROBOT_RULE_ROUTESTEP_JUMP.InsertOnSubmit(_objAdd);
                                }

                                break;
                                #endregion

                            case FormMode.Modify:

                                #region Modify

                                if (ObjEdit == null) return;

                                ObjEdit.ISENABLED = chkIsEnable.Checked ? "Y" : "N";
                                ObjEdit.LASTUPDATETIME = DateTime.Now;
                                ObjEdit.REMARKS = txtRemarks.Text;
                                ObjEdit.JUMPITEMSEQ = int.Parse(txtJumpItemSeq.Text);
                                ObjEdit.GOTOSTEPID = int.Parse(cboGotoStepID.SelectedItem.ToString());

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

                foreach (Control _txt in pnlNew.Controls)
                {
                    if (_txt is TextBox && String.IsNullOrEmpty(_txt.Text) == true && (_txt.Name.Equals(txtJumpConditionID.Name) || (_txt.Name.Equals(txtJumpItemSeq.Name))))
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please Enter Value " + _txt.Name.Replace("txt", "") + " !", MessageBoxIcon.Error);
                        _txt.Focus();
                        return true;
                    }
                    else if (MethodDef.GetObjectName() == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please Choose ObjectName !", MessageBoxIcon.Error);
                        return true;
                    }
                    else if (MethodDef.GetMethodName() == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please Choose MethodName !", MessageBoxIcon.Error);
                        return true;
                    }
                }
                #endregion

                #region 檢查資料已存在 or 已新增但尚未儲存

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                var rowCnt = (from msg in ctxBRM.SBRM_ROBOT_RULE_ROUTESTEP_JUMP 
                              where msg.ROBOTNAME.Equals(CurRobot.RobotName) && 
                                    msg.ROUTEID.Equals(CurRouteID) && 
                                    msg.STEPID.Equals(CurStepID) && 
                                    msg.JUMPCONDITIONID.Equals(txtJumpConditionID.Text) select msg).Count();

                var ChangeSet = (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_RULE_ROUTESTEP_JUMP>().Where(d => 
                                            d.ROBOTNAME.Equals(CurRobot.RobotName) && 
                                            d.ROUTEID.Equals(CurRouteID) &&
                                            d.STEPID.Equals(CurStepID) &&
                                            d.JUMPCONDITIONID.Equals(txtJumpConditionID.Text))).Count();

                if (rowCnt > 0 || ChangeSet > 0)
                {
                    string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is  Item ID[{0}] !", txtJumpConditionID.Text);
                    ShowMessage(this, this.lblCaption.Text, "", errMsg, MessageBoxIcon.Warning);
                    return true;
                }
                #endregion

                #region 檢查新增在datagridview內的資料是否重複，若重複，詢問是否要覆蓋

                var _findNewRow = (from d in dgvData.Rows.Cast<DataGridViewRow>().Where(d => d.Cells[colRobotName.Name].Value.ToString().Equals(CurRobot.RobotName.ToString()) && 
                                                                                             d.Cells[colRouteID.Name].Value.ToString().Equals(CurRouteID) && 
                                                                                             d.Cells[colStepID.Name].Value.ToString().Equals(CurStepID.ToString()) &&
                                                                                             d.Cells[colJumpConditionID.Name].Value.ToString().Equals(txtJumpConditionID.Text))
                                   select d).FirstOrDefault();

                if (_findNewRow != null)
                {
                    string _msg = string.Format(" Item ID [{0}]  is already insert. Please confirm whether you will overwite data ?", txtJumpConditionID.Text);

                    if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return true;
                    _findNewRow.Cells[colObjectName.Name].Value = MethodDef.GetObjectName();
                    _findNewRow.Cells[colMethodName.Name].Value = MethodDef.GetMethodName();                    
                    _findNewRow.Cells[colDesciption.Name].Value = txtDescription.Text;
                    _findNewRow.Cells[chkEnable.Name].Value = chkIsEnable.Checked;
                    _findNewRow.Cells[colLastUpdateTime.Name].Value = DateTime.Now;
                    _findNewRow.Cells[colGotoStepID.Name].Value = cboGotoStepID.SelectedItem.ToString();
                    _findNewRow.Cells[colRemarks.Name].Value = txtRemarks.Text;
                    _findNewRow.Cells[colJumpItemSeq.Name].Value = txtJumpItemSeq.Text;

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
            #region 清除輸入資料
            try
            {
                txtJumpItemSeq.Text = string.Empty;
                txtJumpConditionID.Text = string.Empty;
                txtRemarks.Text = string.Empty;
                txtDescription.Text = string.Empty;

                chkIsEnable.Checked = true;
                MethodDef.ClearMethod();
                cboGotoStepID.SelectedIndex = 0;
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
            #region 點選GridView資料會將資料傳至輸入欄位，以方便修改
            try
            {
                if (e.RowIndex != -1)
                {
                    txtRobotName.Text = dgvData.Rows[e.RowIndex].Cells[colRobotName.Name].Value.ToString();
                    txtRouteID.Text = dgvData.Rows[e.RowIndex].Cells[colRouteID.Name].Value.ToString();
                    txtStepID.Text = dgvData.Rows[e.RowIndex].Cells[colStepID.Name].Value.ToString();
                    txtJumpItemSeq.Text = dgvData.Rows[e.RowIndex].Cells[colJumpItemSeq.Name].Value.ToString();
                    MethodDef.SetObjectName(dgvData.Rows[e.RowIndex].Cells[colObjectName.Name].Value.ToString());
                    MethodDef.SetMethName(dgvData.Rows[e.RowIndex].Cells[colMethodName.Name].Value.ToString());
                    cboGotoStepID.Text = dgvData.Rows[e.RowIndex].Cells[colGotoStepID.Name].Value.ToString();
                    txtRemarks.Text = dgvData.Rows[e.RowIndex].Cells[colRemarks.Name].Value.ToString();

                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

        private void _methodDef_CboSelect(object sender, EventArgs e)
        {
            txtJumpConditionID.Text = MethodDef.GetMethodName();
            txtDescription.Text = MethodDef.GetDescription();
        }
    }
}
