using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormRobot_Rule_Stage_Select_Edit : FormBase
    {
        string RouteID = string.Empty;
        string ItemID = string.Empty;
        int CurStepID = 0;
        Robot CurRobot = null;

        List<string> Description = new List<string>();

        SBRM_ROBOT_RULE_STAGE_SELECT objFilter = null;

        public FormRobot_Rule_Stage_Select_Edit()
        {
            InitializeComponent();
        }
        
        public FormRobot_Rule_Stage_Select_Edit(string routeID, int stepID, Robot robot, string item)
        {
            InitializeComponent();

            RouteID = routeID;
            CurStepID = stepID;
            CurRobot = robot;

            ItemID = item;

            if (ItemID != "") // ItemID為空表示為新增資料
            {
                cboObjectName.Enabled = false;
                cboMethodName.Enabled = false;
                btnNewClose.Tag = "Close";
                btnNewClose.Text = "Close";
                btnAddNew.Tag = "Insert";
                btnAddNew.Text = "Modify";
                Size = new Size(851, 380);
            }
        }

        private void FormRobot_Rule_Stage_Select_Edit_Load(object sender, EventArgs e)
        {
            this.lblCaption.Text = "Robot Rult Stage Select Edit";

            try
            {
                InitialCombox();

                txtRobotName.Text = CurRobot.RobotName;
                txtRouteID.Text = RouteID;
                txtStepID.Text = CurStepID.ToString();
                txtItemSeq.Text = "";

                //讀取資料是否資料已存在
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                List<SBRM_ROBOT_RULE_STAGE_SELECT> objTables = null;

                var _modify = from rb in _ctxBRM.SBRM_ROBOT_RULE_STAGE_SELECT
                              where rb.SERVERNAME == CurRobot.ServerName && rb.ROBOTNAME == CurRobot.RobotName && rb.ROUTEID == RouteID && rb.STEPID == CurStepID && rb.ITEMID == ItemID
                              select rb;

                objTables = _modify.ToList();

                if (_modify.Count() == 0)
                {
                    var addInsertsData = _ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_RULE_STAGE_SELECT>().Where(rb => rb.SERVERNAME == CurRobot.ServerName && rb.ROBOTNAME == CurRobot.RobotName && rb.ROUTEID == RouteID && rb.STEPID == CurStepID && rb.ITEMID == ItemID);

                    var addUpdatesData = _ctxBRM.GetChangeSet().Updates.OfType<SBRM_ROBOT_RULE_STAGE_SELECT>().Where(rb => rb.SERVERNAME == CurRobot.ServerName && rb.ROBOTNAME == CurRobot.RobotName && rb.ROUTEID == RouteID && rb.STEPID == CurStepID && rb.ITEMID == ItemID);

                    objTables.AddRange(addInsertsData.ToList());
                    objTables.AddRange(addUpdatesData.ToList());

                }

                if (objTables.Count() > 0)
                {
                    objFilter = objTables[0];

                    if (cboObjectName.Items.Contains(objFilter.OBJECTNAME))
                        cboObjectName.Text = objFilter.OBJECTNAME;
                    if (cboMethodName.Items.Contains(objFilter.METHODNAME))
                        cboMethodName.Text = objFilter.METHODNAME;
                    txtDescription.Text = Description[cboMethodName.SelectedIndex];
                    txtItemSeq.Text = objFilter.ITEMSEQ.ToString();

                    if (objFilter.ISENABLED == "Y")
                        chkIsEnable.Checked = true;
                    else
                        chkIsEnable.Checked = false;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void InitialCombox()
        {
            try
            {
                #region Method & Object Def

                UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBBRMCtx;

                var varDEF = (from m in _ctx.SBRM_ROBOT_METHOD_DEF
                              where m.METHODRULETYPE.Equals("STAGESELECT")
                              select m);

                DataTable dtDEF = DBConnect.GetDataTable(_ctx, "SBRM_ROBOT_METHOD_DEF", varDEF);
                cboObjectName.Items.Clear();
                cboMethodName.Items.Clear();
                cboObjectName.Items.Add("");
                cboMethodName.Items.Add("");
                Description.Add("");

                foreach (DataRow dr in dtDEF.Rows)
                {
                    if (cboObjectName.Items.Contains(dr["OBJECTNAME"].ToString()) == false)
                        cboObjectName.Items.Add(dr["OBJECTNAME"].ToString());
                    cboMethodName.Items.Add(dr["METHODNAME"].ToString());
                    Description.Add(dr["DESCRIPTION"].ToString());
                }
                cboObjectName.SelectedIndex = 0;
                cboMethodName.SelectedIndex = 0;

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cboObjectName_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cboObjectName.Enabled == true && cboMethodName.Items.Count > 0)
                {
                    if (cboObjectName.SelectedItem.Equals("") == false)
                    {
                        cboMethodName.SelectedIndex = 1;
                    }
                    else
                        cboMethodName.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cboMethodName_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cboMethodName.Text != "")
                {
                    txtItemID.Text = cboMethodName.SelectedItem.ToString();
                    txtDescription.Text = Description[cboMethodName.SelectedIndex];
                }
                else
                {
                    txtItemID.Text = txtDescription.Text = string.Empty;
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
                Button _btn = (Button)sender;

                switch (_btn.Tag.ToString())
                {
                    case "Add":

                        #region Insert to datagridview
                        if (CheckData() == false)
                        {
                            dgvData.Rows.Add(CurRobot.ServerName, txtRobotName.Text, txtRouteID.Text, txtStepID.Text, txtItemID.Text, txtItemSeq.Text,  cboObjectName.SelectedItem.ToString(), cboMethodName.SelectedItem.ToString(), chkIsEnable.Checked, txtDescription.Text, DateTime.Now.ToString());
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

                        switch (_btn.Text)
                        {
                            case "OK":

                                #region Add

                                if (dgvData.Rows.Count < 1)
                                {
                                    ShowMessage(this, lblCaption.Text, "", "Please Add New Data !", MessageBoxIcon.Error);
                                    return;
                                }

                                SBRM_ROBOT_RULE_STAGE_SELECT _objAdd = null;

                                foreach (DataGridViewRow _row in dgvData.Rows)
                                {
                                    _objAdd = new SBRM_ROBOT_RULE_STAGE_SELECT();

                                    _objAdd.OBJECTKEY = 0;
                                    _objAdd.SERVERNAME = CurRobot.ServerName;
                                    _objAdd.ROBOTNAME = _row.Cells[colRobotName.Name].Value.ToString();
                                    _objAdd.ITEMID = _row.Cells[colItemID.Name].Value.ToString();
                                    _objAdd.ROUTEID = _row.Cells[colRouteID.Name].Value.ToString();
                                    _objAdd.STEPID = int.Parse(txtStepID.Text);
                                    _objAdd.ITEMSEQ = int.Parse(_row.Cells[colItemSeq.Name].Value.ToString());
                                    _objAdd.DESCRIPTION = _row.Cells[colDesciption.Name].Value.ToString();
                                    _objAdd.OBJECTNAME = _row.Cells[colObjectName.Name].Value.ToString();
                                    _objAdd.METHODNAME = _row.Cells[colMethodName.Name].Value.ToString();
                                    _objAdd.ISENABLED = _row.Cells[chkEnable.Name].Value.ToString().Equals("True") ? "Y" : "N";
                                    _objAdd.REMARKS = "";
                                    _objAdd.LASTUPDATETIME = Convert.ToDateTime(_row.Cells[colLastUpdateTime.Name].Value.ToString());

                                    _ctxBRM.SBRM_ROBOT_RULE_STAGE_SELECT.InsertOnSubmit(_objAdd);

                                }

                                break;
                                #endregion

                            case "Modify":

                                #region Modify

                                if (objFilter == null) return;

                                //if (string.IsNullOrEmpty(txtItemSeq.Text) == true)
                                //    txtItemSeq.Text = "0";
                                objFilter.ITEMSEQ = string.IsNullOrEmpty(txtItemSeq.Text) ? 0 : int.Parse(txtItemSeq.Text);
                                objFilter.OBJECTNAME = cboObjectName.SelectedItem.ToString();
                                objFilter.STEPID = int.Parse(txtStepID.Text);
                                objFilter.METHODNAME = cboMethodName.SelectedItem.ToString();
                                objFilter.DESCRIPTION = txtDescription.Text;
                                objFilter.ISENABLED = chkIsEnable.Checked ? "Y" : "N";
                                objFilter.LASTUPDATETIME = DateTime.Now;
                                objFilter.REMARKS = "";

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
                foreach (Control _txt in pnlNew.Controls)
                {
                    if (_txt is TextBox && String.IsNullOrEmpty(_txt.Text) == true && (_txt.Name.Equals(txtItemID.Name) || (_txt.Name.Equals(txtItemSeq.Name))))
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please Enter Value " + _txt.Name.Replace("txt", "") + " !", MessageBoxIcon.Error);
                        _txt.Focus();
                        return true;
                    }
                    else if (_txt is ComboBox && ((_txt.Name.Equals(cboObjectName.Name) && cboObjectName.SelectedIndex == -1)) || (_txt.Name.Equals(cboMethodName.Name) && cboMethodName.SelectedIndex == -1))
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please Choose " + _txt.Tag.ToString() + " !", MessageBoxIcon.Error);
                        _txt.Focus();
                        return true;
                    }
                }

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                var rowCnt = (from msg in ctxBRM.SBRM_ROBOT_RULE_STAGE_SELECT where msg.ROBOTNAME.Equals(CurRobot.RobotName) && msg.ROUTEID.Equals(RouteID) && msg.STEPID.Equals(CurStepID) && msg.ITEMID.Equals(txtItemID.Text) select msg).Count();

                var ChangeSet = (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ROBOT_RULE_STAGE_SELECT>().Where(msg => msg.ROBOTNAME.Equals(CurRobot.RobotName) && msg.ROUTEID.Equals(RouteID) && msg.ITEMID.Equals(txtItemID.Text))).Count();

                if (rowCnt > 0 || ChangeSet > 0)
                {
                    string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is  Item ID[{0}] !", txtItemID.Text);
                    ShowMessage(this, this.lblCaption.Text, "", errMsg, MessageBoxIcon.Warning);
                    return true;
                }

                var findreplace = (from d in dgvData.Rows.Cast<DataGridViewRow>().Where(d => d.Cells[colRobotName.Name].Value.ToString().Equals(CurRobot.RobotName.ToString()) && d.Cells[colRouteID.Name].Value.ToString().Equals(RouteID) && d.Cells[colStepID.Name].Value.ToString().Equals(CurStepID.ToString()) && d.Cells[colItemID.Name].Value.ToString().Equals(txtItemID.Text)) select d).Count();

                if (findreplace > 0)
                {
                    DataGridViewRow _addNewRow = dgvData.Rows.Cast<DataGridViewRow>().Where(d => d.Cells[colRobotName.Name].Value.ToString().Equals(CurRobot.RobotName) && d.Cells[colRouteID.Name].Value.ToString().Equals(RouteID) && d.Cells[colStepID.Name].Value.ToString().Equals(CurStepID) && d.Cells[colItemID.Name].Value.ToString().Equals(txtItemID.Text)).First();

                    string _msg = string.Format(" Item ID [{0}]  is already insert. Please confirm whether you will overwite data ?", txtItemID.Text);

                    if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return true;
                    _addNewRow.Cells[colObjectName.Name].Value = cboObjectName.SelectedItem.ToString();
                    _addNewRow.Cells[colMethodName.Name].Value = cboMethodName.SelectedItem.ToString();
                    _addNewRow.Cells[colItemSeq.Name].Value = string.IsNullOrEmpty(txtItemSeq.Text) ? "0" : txtItemSeq.Text;
                    _addNewRow.Cells[colDesciption.Name].Value = txtDescription.Text;
                    _addNewRow.Cells[chkEnable.Name].Value = chkIsEnable.Checked;
                    _addNewRow.Cells[colLastUpdateTime.Name].Value = DateTime.Now;

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
        }
        private void ClearData()
        {
            try
            {
                foreach (Control _clear in pnlNew.Controls)
                {
                    if (_clear is TextBox && ((TextBox)_clear).ReadOnly == false)
                    {
                        _clear.Text = string.Empty;
                    }
                }
                chkIsEnable.Checked = true;
                cboObjectName.SelectedIndex = 0;
                cboMethodName.SelectedIndex = 0;
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

        private void dgvData_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex != -1)
                {
                    txtRobotName.Text = dgvData.Rows[e.RowIndex].Cells[colRobotName.Name].Value.ToString();
                    txtRouteID.Text = dgvData.Rows[e.RowIndex].Cells[colRouteID.Name].Value.ToString();
                    txtStepID.Text = dgvData.Rows[e.RowIndex].Cells[colStepID.Name].Value.ToString();
                    txtItemSeq.Text = dgvData.Rows[e.RowIndex].Cells[colItemSeq.Name].Value.ToString();
                    cboObjectName.SelectedItem = dgvData.Rows[e.RowIndex].Cells[colObjectName.Name].Value.ToString();
                    cboMethodName.SelectedItem = dgvData.Rows[e.RowIndex].Cells[colMethodName.Name].Value.ToString();

                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
    }
}
