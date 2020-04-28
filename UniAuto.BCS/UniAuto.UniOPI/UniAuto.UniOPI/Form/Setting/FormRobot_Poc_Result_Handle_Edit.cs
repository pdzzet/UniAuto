using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormRobot_Poc_Result_Handle_Edit : FormBase
    {
        string CurRouteID = string.Empty;

        int CurStepID = 0;//目前的Step
        Robot CurRobot = null;//Robot資訊
        ucRBMethod MethodDef = new ucRBMethod();

        SBRM_ROBOT_PROC_RESULT_HANDLE ObjEdit = null;

        public FormRobot_Poc_Result_Handle_Edit()
        {
            InitializeComponent();

            this.lblCaption.Text = "Robot Proc Result Handle Edit";
        }

        public FormRobot_Poc_Result_Handle_Edit(string routeID, int stepID, Robot robot, SBRM_ROBOT_PROC_RESULT_HANDLE objProcess)
            : this()
        {
            CurRouteID = routeID;
            CurStepID = stepID;
            CurRobot = robot;
            
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
                Size = new Size(851, 382);
            }
        }

        private void FormRobot_Rule_Filter_Load(object sender, EventArgs e)
        {           
            try
            {
                #region Method & Object Def
                MethodDef.Location = new Point(6, 103);
                pnlNew.Controls.Add(MethodDef);

                MethodDef.InitialRBMethod("ResultAction");
                MethodDef.Select_cbo += new ucRBMethod.CboClickDelegate(_methodDef_CboSelect);
                #endregion

                txtRobotName.Text = CurRobot.RobotName;
                txtRouteID.Text = CurRouteID;
                txtStepID.Text = CurStepID.ToString();
                txtItemSeq.Text = "";

                #region Load Modify Data
                if (FormMode == UniOPI.FormMode.Modify)
                {
                    //UK 不可修改 : ServerName + Robot Name + RouteID + Step ID +  + Item ID                   
                    MethodDef.SetEnable(false);

                    if (ObjEdit != null)
                    {
                        MethodDef.SetObjectName(ObjEdit.OBJECTNAME);
                        MethodDef.SetMethName(ObjEdit.METHODNAME);

                        txtItemSeq.Text = ObjEdit.ITEMSEQ.ToString();

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
                            dgvData.Rows.Add( CurRobot.ServerName, CurRobot.RobotName, CurRouteID, CurStepID, txtItemID.Text, txtItemSeq.Text, MethodDef.GetObjectName(), MethodDef.GetMethodName(), MethodDef.GetFunName(), chkIsEnable.Checked, txtDescription.Text, DateTime.Now.ToString());
                            
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

                                SBRM_ROBOT_PROC_RESULT_HANDLE _objAdd = null;

                                foreach (DataGridViewRow _row in dgvData.Rows)
                                {
                                    _objAdd = new SBRM_ROBOT_PROC_RESULT_HANDLE();

                                    _objAdd.OBJECTKEY = 0;
                                    _objAdd.SERVERNAME = CurRobot.ServerName;
                                    _objAdd.ROBOTNAME = _row.Cells[colRobotName.Name].Value.ToString();
                                    _objAdd.ITEMID = _row.Cells[colItemID.Name].Value.ToString();
                                    _objAdd.ROUTEID = _row.Cells[colRouteID.Name].Value.ToString();
                                    _objAdd.STEPID = int.Parse(txtStepID.Text);
                                    _objAdd.ITEMSEQ = int.Parse(_row.Cells[colItemSeq.Name].Value.ToString());
                                    _objAdd.DESCRIPTION = string.Empty;
                                    _objAdd.OBJECTNAME = _row.Cells[colObjectName.Name].Value.ToString();
                                    _objAdd.METHODNAME = _row.Cells[colMethodName.Name].Value.ToString();
                                    _objAdd.ISENABLED = _row.Cells[chkEnable.Name].Value.ToString().Equals("True") ? "Y" : "N";
                                    _objAdd.REMARKS = "";
                                    _objAdd.LASTUPDATETIME = Convert.ToDateTime(_row.Cells[colLastUpdateTime.Name].Value.ToString());

                                    _ctxBRM.SBRM_ROBOT_PROC_RESULT_HANDLE.InsertOnSubmit(_objAdd);
                                }

                                ClearData();

                                break;
                                #endregion

                            case FormMode.Modify:

                                #region Modify

                                if (ObjEdit == null) return;

                                ObjEdit.ITEMSEQ = string.IsNullOrEmpty(txtItemSeq.Text) ? 0 : int.Parse(txtItemSeq.Text);
                                ObjEdit.ISENABLED = chkIsEnable.Checked ? "Y" : "N";
                                ObjEdit.LASTUPDATETIME = DateTime.Now;
                                ObjEdit.REMARKS = "";

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

                foreach (Control _obj in pnlNew.Controls)
                {
                    if (_obj is TextBox && String.IsNullOrEmpty(_obj.Text) == true && (_obj.Name.Equals(txtItemID.Name) || (_obj.Name.Equals(txtItemSeq.Name))))
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please Enter Value " + _obj.Name.Replace("txt", "") + " !", MessageBoxIcon.Error);
                        _obj.Focus();
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

                //rowCnt表示現在已存在的表格
                var rowCnt = (from msg in ctxBRM.SBRM_ROBOT_PROC_RESULT_HANDLE
                              where msg.SERVERNAME.Equals(CurRobot.ServerName) && 
                                    msg.ROBOTNAME.Equals(CurRobot.RobotName) && 
                                    msg.ROUTEID.Equals(CurRouteID) && 
                                    msg.STEPID.Equals(CurStepID) && 
                                    msg.ITEMID.Equals(txtItemID.Text) select msg).Count();

                //ChangeSet表示已經準備新增及修改的表格
                var ChangeSet = (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_PROC_RESULT_HANDLE>().Where(d =>
                    d.SERVERNAME.Equals(CurRobot.ServerName) && 
                    d.ROBOTNAME.Equals(CurRobot.RobotName) && 
                    d.ROUTEID.Equals(CurRouteID) &&
                    d.STEPID.Equals(CurStepID) && 
                    d.ITEMID.Equals(txtItemID.Text))).Count();

                if (rowCnt > 0 || ChangeSet > 0)
                {
                    string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is  Item ID[{0}] !",txtItemID.Text);
                    ShowMessage(this, this.lblCaption.Text, "", errMsg, MessageBoxIcon.Warning);
                    return true;
                }

                #endregion

                #region 檢查新增在datagridview內的資料是否重複，若重複，詢問是否要覆蓋
                var _findNewRow = (from d in dgvData.Rows.Cast<DataGridViewRow>().Where(d => d.Cells[colSeverName.Name].Value.ToString().Equals(CurRobot.ServerName.ToString()) &&
                                                                                             d.Cells[colRobotName.Name].Value.ToString().Equals(CurRobot.RobotName.ToString()) && 
                                                                                             d.Cells[colRouteID.Name].Value.ToString().Equals(CurRouteID) && 
                                                                                             d.Cells[colStepID.Name].Value.ToString().Equals(CurStepID.ToString()) && 
                                                                                             d.Cells[colItemID.Name].Value.ToString().Equals(txtItemID.Text)) select d).FirstOrDefault();

                 if (_findNewRow != null)
                {
                    string _msg = string.Format("Item ID [{0}]  is already insert. Please confirm whether you will overwite data ?",  txtItemID.Text);

                    if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return true;

                    _findNewRow.Cells[colObjectName.Name].Value = MethodDef.GetObjectName();
                    _findNewRow.Cells[colMethodName.Name].Value = MethodDef.GetMethodName();
                    _findNewRow.Cells[colFuncKey.Name].Value = MethodDef.GetFunName();
                    _findNewRow.Cells[colItemSeq.Name].Value = string.IsNullOrEmpty(txtItemSeq.Text) ? "0" : txtItemSeq.Text;
                    _findNewRow.Cells[colDesciption.Name].Value = txtDescription.Text;
                    _findNewRow.Cells[chkEnable.Name].Value = chkIsEnable.Checked;
                    _findNewRow.Cells[colLastUpdateTime.Name].Value = DateTime.Now;

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
                txtItemID.Text = string.Empty;
                txtItemSeq.Text = string.Empty;
                txtDescription.Text = string.Empty;
                chkIsEnable.Checked = true;
                MethodDef.ClearMethod();
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
                    txtItemSeq.Text = dgvData.Rows[e.RowIndex].Cells[colItemSeq.Name].Value.ToString();
                    MethodDef.SetObjectName(dgvData.Rows[e.RowIndex].Cells[colObjectName.Name].Value.ToString());
                    MethodDef.SetMethName(dgvData.Rows[e.RowIndex].Cells[colMethodName.Name].Value.ToString());

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
            txtItemID.Text = MethodDef.GetMethodName();
            txtDescription.Text = MethodDef.GetDescription();
        }
    }
}
