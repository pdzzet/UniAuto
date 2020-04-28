using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormRobotMethodDefEdit : FormBase
    {
        private SBRM_ROBOT_METHOD_DEF ObjEdit;

        Assembly AsmMethod = null;

        List<RB_ObjectName> LstObject; //設定ObjectName的List
        UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

        //Assembly HelpAtt = null;


        public FormRobotMethodDefEdit()
        {
            InitializeComponent();
            this.lblCaption.Text = "Robot Method Definition Edit";
        }

        public FormRobotMethodDefEdit(SBRM_ROBOT_METHOD_DEF objProcess):this()
        {
            // TODO: Complete member initialization
            if (objProcess == null)
            {
                FormMode = UniOPI.FormMode.AddNew;
                dgvData.Visible = true;
                Size = new Size(850, 590);
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
                Size = new Size(850,340);
                btnAddNew.Tag = "Insert";
                btnAddNew.Text = "Modify";
                btnNewClose.Tag = "Close";
                btnNewClose.Text = "Close";

            }
        }

        private void FormRobotMethodDefEdit_Load(object sender, EventArgs e)
        {
            #region 讀取ObjectNam，設定ObjectName、MethodName
            try
            {
                InitialCombox_ObjectName();

                if (FormMode == UniOPI.FormMode.Modify)
                {
                    cboObjectName.Enabled = false;
                    cboMethodName.Enabled = false;

                    if (ObjEdit != null)
                    {
                        //txtObjectName.Text = ObjEdit.OBJECTNAME;
                        //txtMethodName.Text = ObjEdit.METHODNAME;
                        //cboMethodRuleType.SelectedItem = ObjEdit.METHODRULETYPE;
                        txtFuncKey.Text = ObjEdit.FUNCKEY;
                        txtDescription.Text = ObjEdit.DESCRIPTION;
                        txtRemark.Text = ObjEdit.REMARKS;
                        ckbIsEnable.Checked = ObjEdit.ISENABLED.Equals("Y") ? true : false;

                        SetObjectCombobox(ObjEdit.OBJECTNAME, ObjEdit.METHODNAME);
                    }
                }

                cboObjectName.SelectedValueChanged += new EventHandler(cboObjectName_SelectedValueChanged);
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
                            string _objectName = ((RB_ObjectName)cboObjectName.SelectedItem).ObjName;
                            string _methodName = ((RB_ObjectName)cboMethodName.SelectedItem).ObjName;
                            string _funcKey = ((RB_ObjectName)cboMethodName.SelectedItem).FunctionKey;
                            dgvData.Rows.Add("0", ckbIsEnable.Checked, cboMethodRuleType.Text, _objectName, _methodName,_funcKey, txtDescription.Text, FormMainMDI.G_OPIAp.LoginUserID, DateTime.Now, txtRemark.Text);
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

                        switch (FormMode)
                        {
                            case FormMode.AddNew:

                                #region Add

                                if (dgvData.Rows.Count < 1)
                                {
                                    ShowMessage(this, lblCaption.Text, "", "Please Add New Data !", MessageBoxIcon.Error);
                                    return;
                                }

                                SBRM_ROBOT_METHOD_DEF _objAdd = null;

                                foreach (DataGridViewRow _row in dgvData.Rows)
                                {
                                    _objAdd = new SBRM_ROBOT_METHOD_DEF();

                                    _objAdd.OBJECTKEY = 0;
                                    _objAdd.OBJECTNAME = _row.Cells[colObjectName.Name].Value.ToString();
                                    _objAdd.METHODNAME = _row.Cells[colMethodName.Name].Value.ToString();
                                    _objAdd.METHODRULETYPE = _row.Cells[colMethodRuleType.Name].Value.ToString();
                                    _objAdd.FUNCKEY = _row.Cells[colFuncKey.Name].Value.ToString();
                                    _objAdd.LASTUPDATEDATE = Convert.ToDateTime(_row.Cells[colLastUpdateDate.Name].Value.ToString());
                                    _objAdd.ISENABLED = _row.Cells[colIsEnabled.Name].Value.ToString().Equals("True") ? "Y" : "N";
                                    _objAdd.DESCRIPTION = _row.Cells[colDesciption.Name].Value.ToString();
                                    _objAdd.AUTHOR = _row.Cells[colAuthor.Name].Value.ToString();
                                    _objAdd.REMARKS = _row.Cells[colReMarks.Name].Value.ToString();
                                    _ctxBRM.SBRM_ROBOT_METHOD_DEF.InsertOnSubmit(_objAdd);
                                }

                                break;
                                #endregion

                            case FormMode.Modify:

                                #region Modify

                                if (ObjEdit == null) return;
                                    ObjEdit.LASTUPDATEDATE = DateTime.Now;
                                    ObjEdit.ISENABLED = ckbIsEnable.Checked ? "Y" : "N";
                                    ObjEdit.DESCRIPTION = txtDescription.Text;
                                    ObjEdit.AUTHOR = FormMainMDI.G_OPIAp.LoginUserID;
                                    ObjEdit.REMARKS = txtRemark.Text;

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
                #region 檢查有無資料未輸入、資料重複
                if (cboMethodRuleType.SelectedIndex == -1)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please Choose Method Rule Type !", MessageBoxIcon.Error);
                    return true;
                }

                if (cboObjectName.SelectedValue == null )
                {
                     ShowMessage(this, lblCaption.Text, "", "Please Choose Object Name !", MessageBoxIcon.Error);
                    return true;
                }

                if (cboMethodName.SelectedValue == null)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please Choose Method Name !", MessageBoxIcon.Error);
                    return true;
                }

                string _objectName = ((RB_ObjectName)cboObjectName.SelectedItem).ObjName;
                string _methodName = ((RB_ObjectName)cboMethodName.SelectedItem).ObjName;

                var rowCnt = (from msg in _ctxBRM.SBRM_ROBOT_METHOD_DEF where msg.OBJECTNAME.Equals(_objectName) && msg.METHODNAME.Equals(_methodName) select msg).Count();

                var ChangeSet = (_ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_METHOD_DEF>().Where(d => d.METHODNAME.Equals(_methodName) && d.OBJECTNAME.Equals(_objectName))).Count();

                if (rowCnt > 0 || ChangeSet>0)
                {
                    string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key isUNIQUE KEY constraint violated. Duplicate key is Object Name [{0}], Method Name [{1}]！", _objectName, _methodName);
                    ShowMessage(this, this.lblCaption.Text, "", errMsg, MessageBoxIcon.Warning);
                    return true;
                }

                var findreplace = (from d in dgvData.Rows.Cast<DataGridViewRow>().Where(d => d.Cells[colObjectName.Name].Value.ToString().Equals(_objectName) && d.Cells[colMethodName.Name].Value.ToString().Equals(_methodName)) select d).Count();

                if (findreplace > 0)
                {
                    DataGridViewRow _addNewRow = dgvData.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[colObjectName.Name].Value.ToString().Equals(_objectName) && r.Cells[colMethodName.Name].Value.ToString().Equals(_methodName)).First();

                    string _msg = string.Format("Object Name [{0}],Method Name [{1}]is already insert. Please confirm whether you will overwite data ?", _objectName, _methodName);

                    if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return true;
                    _addNewRow.Cells[colAuthor.Name].Value = FormMainMDI.G_OPIAp.LoginUserID;
                    _addNewRow.Cells[colDesciption.Name].Value = txtDescription.Text;
                    _addNewRow.Cells[colIsEnabled.Name].Value = ckbIsEnable.Checked;
                    _addNewRow.Cells[colLastUpdateDate.Name].Value = DateTime.Now;
                    _addNewRow.Cells[colReMarks.Name].Value = txtRemark.Text;
                    _addNewRow.Cells[colMethodRuleType.Name].Value = cboMethodRuleType.SelectedItem.ToString();
                    ClearData();
                    return true;
                }
                return false;
                #endregion
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
            try
            {
                foreach (Control _clear in pnlNew.Controls)
                {
                    if (_clear is TextBox)
                    {
                        _clear.Text = string.Empty;
                    }
                }
                ckbIsEnable.Checked = true;
                cboMethodRuleType.SelectedIndex = -1;
                cboObjectName.SelectedIndex = -1;
                cboMethodName.SelectedIndex = -1;
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
                    //cboMethodRuleType.SelectedItem = dgvData.Rows[e.RowIndex].Cells[colMethodRuleType.Name].Value.ToString();
                    txtDescription.Text = dgvData.Rows[e.RowIndex].Cells[colDesciption.Name].Value.ToString();
                    //txtMethodName.SelectedValue = dgvData.Rows[e.RowIndex].Cells[colMethodName.Name].Value.ToString();
                    //txtObjectName.SelectedValue = dgvData.Rows[e.RowIndex].Cells[colObjectName.Name].Value.ToString();
                    cboObjectName.SelectedItem = dgvData.Rows[e.RowIndex].Cells[colObjectName.Name].Value.ToString();
                    cboMethodName.SelectedItem = dgvData.Rows[e.RowIndex].Cells[colMethodRuleType.Name].Value.ToString()+ "_" + dgvData.Rows[e.RowIndex].Cells[colMethodName.Name].Value.ToString();
                    cboMethodRuleType.Text = dgvData.Rows[e.RowIndex].Cells[colMethodRuleType.Name].Value.ToString();
                    txtRemark.Text = dgvData.Rows[e.RowIndex].Cells[colReMarks.Name].Value.ToString();
                    ckbIsEnable.Checked = dgvData.Rows[e.RowIndex].Cells[colIsEnabled.Name].Value.ToString().Equals("True") ? true : false;
                    SetObjectCombobox(dgvData.Rows[e.RowIndex].Cells[colObjectName.Name].Value.ToString(), dgvData.Rows[e.RowIndex].Cells[colMethodName.Name].Value.ToString());
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

        private void cboObjectName_SelectedValueChanged(object sender, EventArgs e)
        {
            #region 選取ObjectName 後產生對應的MethodName
            try
            {
                ComboBox _cbo = (ComboBox)sender;

                if (_cbo.SelectedValue == null) return;

                string _objName = _cbo.SelectedValue.ToString();

                InitialCombox_MethodName(_objName);

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

        private void cboMethodName_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (cboMethodName.SelectedValue == null) return;

                string _funcKey = ((RB_ObjectName)cboMethodName.SelectedItem).FunctionKey;
                string _methodName = ((RB_ObjectName)cboMethodName.SelectedItem).ObjName;

                txtFuncKey.Text = _funcKey;

                if (_methodName.Contains("ProcResult") == false)
                {
                    if (_methodName.IndexOf("_") < 0)
                        cboMethodRuleType.Text = _methodName.Substring(0, _methodName.Length-1);
                    else
                        cboMethodRuleType.Text = _methodName.Substring(0, _methodName.IndexOf("_"));
                }
                else
                    cboMethodRuleType.Text = "ResultAction";
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private bool CheckDBRelate()
        {
            #region 檢查新增修改項目是否有重複值
            try
            {
                var condition = (from q in _ctxBRM.SBRM_ROBOT_ROUTE_CONDITION where q.METHODNAME.Equals(ObjEdit.METHODNAME) && q.OBJECTNAME.Equals(ObjEdit.OBJECTNAME) select q).Distinct().ToList();

                if (condition == null || condition.Count() == 0)
                {
                    var jobSelect = (from d in _ctxBRM.SBRM_ROBOT_RULE_JOB_SELECT where d.OBJECTNAME.Equals(ObjEdit.OBJECTNAME) && d.METHODNAME.Equals(ObjEdit.METHODNAME) select d).Distinct().ToList();

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

        private void ckbIsEnable_CheckedChanged(object sender, EventArgs e)
        {
            #region 當有表格使用中，不能取消勾選
            if (ObjEdit != null && ckbIsEnable.Checked != true && CheckDBRelate() == false)
            {
                ckbIsEnable.Checked = true;
                return;
            }
            #endregion
        }

        private void InitialCombox_ObjectName()
        {            
            try
            {
                #region 載入Method Def的ObjectName資料至CboObjectName
                LstObject = GetClassName();

                cboObjectName.DataSource = LstObject;
                cboObjectName.DisplayMember = "ObjName";
                cboObjectName.ValueMember = "ObjFullName";

                cboObjectName.SelectedIndex = -1;
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            
        }

        private void InitialCombox_MethodName(string objectFullName)
        {
            try
            {
                if (AsmMethod == null) return;

                txtFuncKey.Text = string.Empty;

                #region Method Name

                List<RB_ObjectName> _lstMethodName = new List<RB_ObjectName>();
                //List<string> _lstObject = new List<string>();

                Type _type = AsmMethod.GetType(objectFullName);

                if (_type == null) return;

                MethodInfo[] _method = _type.GetMethods();
                var DB_MathodName = (from _d in _ctxBRM.SBRM_ROBOT_METHOD_DEF select _d.METHODNAME).ToList();

                foreach (MethodInfo _info in _method)
                {
                    if (DB_MathodName.Contains(_info.Name)) continue;
                    //Filter_
                    //OrderBy_
                    //ProcResult_
                    //StageSelect_
                    //RouteCondition_
                    //Select_
                    if (_info.Name.IndexOf("Filter_", 0) >= 0 || _info.Name.IndexOf("OrderBy_", 0) >= 0 || _info.Name.IndexOf("ProcResult_", 0) >= 0 ||
                        _info.Name.IndexOf("StageSelect_", 0) >= 0 || _info.Name.IndexOf("RouteCondition_", 0) >= 0 || _info.Name.IndexOf("Select_", 0) >= 0 || _info.Name.IndexOf("RouteStepByPass_", 0) >= 0 || _info.Name.IndexOf("RouteStepJump_", 0) >= 0)
                    {
                        RB_ObjectName _obj = new RB_ObjectName();

                        _obj.ObjName = _info.Name;
                        _obj.ObjFullName = _info.Name;


                        object[] _attrs = _info.GetCustomAttributes(true);

                        if (_attrs == null || _attrs.Length == 0)
                        {
                            _obj.FunctionKey = OPIConst.Default_RobotFunKey;
                        }
                        else
                        {
                            _obj.FunctionKey = ((HelpAttribute)_attrs[0]).Description.ToString();
                        }

                        //foreach (HelpAttribute attr in _attrs)
                        //{
                        //    string _data = attr.Description.ToString();
                        //}
                        _lstMethodName.Add(_obj);
                    }
                }

                _lstMethodName = _lstMethodName.OrderBy(r => r.Description).ToList();

                cboMethodName.SelectedValueChanged -= new EventHandler(cboMethodName_SelectedValueChanged);

                cboMethodName.DataSource = _lstMethodName;
                cboMethodName.DisplayMember = "Description"; //"ObjName";
                cboMethodName.ValueMember = "ObjFullName";

                cboMethodName.SelectedIndex = -1;

                cboMethodName.SelectedValueChanged +=new EventHandler(cboMethodName_SelectedValueChanged);
                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetObjectCombobox(string objectName, string methodName)
        {           
            try
            {
                if (LstObject == null) return;

                RB_ObjectName _object = LstObject.Where(r => r.ObjName.Equals(objectName)).FirstOrDefault();

                if (_object == null) return;

                cboObjectName.SelectedValue = _object.ObjFullName;

                InitialCombox_MethodName(_object.ObjFullName);

                cboMethodName.SelectedValue = methodName;

                if (methodName.Contains("ProcResult") == false)
                    cboMethodRuleType.Text = methodName.Substring(0, methodName.IndexOf("_"));
                else
                    cboMethodRuleType.Text = "ResultAction";
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private List<RB_ObjectName> GetClassName()
        {
            
            List<RB_ObjectName> _lstClassName = new List<RB_ObjectName>();

            try
            {
                #region 取得UniAuto.UniRCS.CSOT.t3.Service.dll內的class name
                string _dllFilePath = string.Format("{0}\\UniAuto.UniRCS.CSOT.t3.Service.dll", Application.StartupPath);

                if (File.Exists(_dllFilePath))
                {
                    byte[] _buf = System.IO.File.ReadAllBytes(_dllFilePath);

                    AsmMethod = Assembly.Load(_buf);

                    Type[] types = AsmMethod.GetTypes();

                    foreach (Type type in types)
                    {
                        RB_ObjectName _obj = new RB_ObjectName();

                        object[] _attrs = type.GetCustomAttributes(true);

                        if (_attrs == null || _attrs.Length == 0) continue;

                        if (type.Name.IndexOf("Job", 0) >= 0 || type.Name.IndexOf("Robot", 0) >= 0)
                        {
                            _obj.ObjName = type.Name;
                            _obj.ObjFullName = type.FullName;
                            _lstClassName.Add(_obj);
                        }
                    }
                }

                return _lstClassName;
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);

                return _lstClassName;
            }
            
        }

        //private void cboMethodName_SelectionChangeCommitted(object sender, EventArgs e)
        //{
        //    if (cboMethodName.SelectedValue.ToString().Contains("ProcResult") == false)
        //        cboMethodRuleType.Text = cboMethodName.SelectedValue.ToString().Substring(0, cboMethodName.SelectedValue.ToString().IndexOf("_"));
        //    else
        //        cboMethodRuleType.Text = "ResultAction";
        //}
    }

    public class RB_ObjectName
    {
        private string objName = string.Empty;
        private string objFullName = string.Empty;
        private string funcKey = string.Empty;

        public string ObjName
        {
            get { return objName; }
            set { objName = value; }
        }

        public string ObjFullName
        {
            get { return objFullName; }
            set { objFullName = value; }
        }

        public string FunctionKey
        {
            get { return funcKey; }
            set { funcKey = value; }
        }

        public string Description
        {
            get { return string.Format("[{0}] {1}", funcKey, objName); }
        }
    }
}
