using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormLineStatusRuleEdit : FormBase
    {
        private SBRM_LINESTATUSSPEC ObjEditData;

        public FormLineStatusRuleEdit()
        {
            InitializeComponent();

            lblCaption.Text = "Line Status Rule Edit";
        }

        public FormLineStatusRuleEdit(SBRM_LINESTATUSSPEC objData)
            : this()
        {
            if (objData == null)
            {
                FormMode = UniOPI.FormMode.AddNew;

                pnlAdd.Visible = true;
                dgvAddList.Visible = true;

                Size = new Size(727, 700);
            }
            else
            {
                FormMode = UniOPI.FormMode.Modify;

                ObjEditData = objData;

                pnlAdd.Visible = false;
                dgvAddList.Visible = false;

                Size = new Size(727, 440);
            }
        }

        private void FormLineStatusRuleEdit_Load(object sender, EventArgs e)
        {
            try
            {
                this.InitialCombox();
                this.InitialCheckListBox();

                if (FormMode == UniOPI.FormMode.Modify)
                {
                    txtConditionSeqNo.Enabled = false;

                    if (ObjEditData != null)
                    {
                        cmbConditionStatus.SelectedValue = ObjEditData.CONDITIONSTATUS;
                        txtConditionSeqNo.Text = ObjEditData.CONDITIONSEQNO.ToString();
                        txtEqpList.Text = ObjEditData.EQPNOLIST;
                        this.ChooseCheckListBox(txtEqpList.Text);
                    }
                                       
                    this.ActiveControl = chklstEqpNo;
                }
                else
                {
                    this.ActiveControl = cmbConditionStatus; 
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
                    case "ADD":

                        #region Add

                        #region Add to DataGridView

                        if (CheckData() == false) return;

                        string _lineType = FormMainMDI.G_OPIAp.CurLine.LineType;
                        string _conditionStatus = cmbConditionStatus.SelectedValue.ToString();
                        int _conditionSeqNo = int.Parse(txtConditionSeqNo.Text.Trim());
                        string _eqpList = txtEqpList.Text.Trim();

                        if (CheckUniqueKeyOK(_lineType, _conditionSeqNo) == false) return;

                        #region 判斷是否存下gridview內
                        if (dgvAddList.Rows.Cast<DataGridViewRow>().Where(r =>
                            r.Cells[colLineType.Name].Value.ToString().Equals(_lineType) &&
                            r.Cells[colConditionSeqNo.Name].Value.ToString().Equals(_conditionSeqNo.ToString()) 
                            ).Count() > 0)
                        {
                            DataGridViewRow _addRow = dgvAddList.Rows.Cast<DataGridViewRow>().Where(r =>
                            r.Cells[colLineType.Name].Value.ToString().Equals(_lineType) &&
                            r.Cells[colConditionSeqNo.Name].Value.ToString().Equals(_conditionSeqNo.ToString()) 
                                ).First();

                            string _msg = string.Format("Line Type [{0}],Condition Seq No[{1}]is already insert. Please confirm whether you will overwite data ?", _lineType, _conditionSeqNo);

                            if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return;

                            _addRow.Cells[colConditionStatus.Name].Value = _conditionStatus;
                            _addRow.Cells[colLocalList.Name].Value = _eqpList;
                        }
                        else
                        {
                            dgvAddList.Rows.Add(_lineType, _conditionStatus, _conditionSeqNo, _eqpList);
                        }

                        #endregion

                        #endregion

                        #region clear data

                        ClearData();

                        #endregion

                        break;
                        #endregion

                    case "OK":

                        #region OK

                        UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                        switch (FormMode)
                        {
                            case FormMode.AddNew:

                                #region Add

                                SBRM_LINESTATUSSPEC _objAdd = null;

                                int _seqNo = 0;

                                foreach (DataGridViewRow _row in dgvAddList.Rows)
                                {
                                    _objAdd = new SBRM_LINESTATUSSPEC();

                                    int.TryParse(_row.Cells[colConditionSeqNo.Name].Value.ToString(),out _seqNo);

                                    _objAdd.LINETYPE = _row.Cells[colLineType.Name].Value.ToString();
                                    _objAdd.CONDITIONSEQNO = _seqNo;
                                    _objAdd.CONDITIONSTATUS = _row.Cells[colConditionStatus.Name].Value.ToString();
                                    _objAdd.EQPNOLIST = _row.Cells[colLocalList.Name].Value.ToString();

                                    _ctxBRM.SBRM_LINESTATUSSPEC.InsertOnSubmit(_objAdd);
                                }

                                break;
                                #endregion

                            case FormMode.Modify:

                                #region Modify

                                if (ObjEditData == null) return;

                                ObjEditData.CONDITIONSTATUS = cmbConditionStatus.SelectedValue.ToString();
                                ObjEditData.EQPNOLIST = txtEqpList.Text.Trim();

                                break;
                                #endregion

                            default:
                                break;
                        }

                        this.DialogResult = System.Windows.Forms.DialogResult.OK;

                        break;
                        #endregion

                    case "Cancel":

                        #region Cancel
                        this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                        break;
                        #endregion

                    case "Clear":

                        #region Clear
                        ClearData();
                        break;
                        #endregion

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

        private void txtConditionSeqNo_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (((int)e.KeyChar < 48 | (int)e.KeyChar > 57) & (int)e.KeyChar != 8)
            {
                e.Handled = true;
            }
        }

        private void chklstEqpNo_ItemCheck(object sender, ItemCheckEventArgs e)
        {
            try
            {
                CheckedListBox clb = (CheckedListBox)sender;

                clb.ItemCheck -= chklstEqpNo_ItemCheck;
                clb.SetItemCheckState(e.Index, e.NewValue);
                clb.ItemCheck += chklstEqpNo_ItemCheck;

                string[] items = chklstEqpNo.CheckedItems.Cast<string>().ToArray();
                string[] parse = null;
                List<string> lstSelEQ = new List<string>();
                foreach (string item in items)
                {
                    parse = item.Split(new char[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
                    lstSelEQ.Add(parse[0]);
                }

                txtEqpList.Text = string.Join(",", lstSelEQ.ToArray());
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvAddList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0 )
                {
                    DataGridViewRow _row = dgvAddList.SelectedRows[0];

                    if (_row != null)
                    {
                        cmbConditionStatus.SelectedValue = _row.Cells[colConditionStatus.Name].Value.ToString();
                        txtConditionSeqNo.Text = _row.Cells[colConditionSeqNo.Name].Value.ToString();
                        txtEqpList.Text = _row.Cells[colLocalList.Name].Value.ToString();

                        this.ChooseCheckListBox(txtEqpList.Text);
                    }                    
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
                #region cmbConditionStatus
                DataTable dtStatus = UniTools.InitDt(new string[] { "STATUS" });
                DataRow drNew = dtStatus.NewRow();
                drNew["STATUS"] = "RUN";
                dtStatus.Rows.Add(drNew);
                drNew = dtStatus.NewRow();
                drNew["STATUS"] = "DOWN";
                dtStatus.Rows.Add(drNew);
                drNew = dtStatus.NewRow();
                drNew["STATUS"] = "IDLE";
                dtStatus.Rows.Add(drNew);
                drNew = dtStatus.NewRow();
                drNew["STATUS"] = "EQALIVEDOWN";
                dtStatus.Rows.Add(drNew);

                cmbConditionStatus.DataSource = dtStatus;
                cmbConditionStatus.DisplayMember = "STATUS";
                cmbConditionStatus.ValueMember = "STATUS";
                cmbConditionStatus.SelectedIndex = -1;
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void InitialCheckListBox()
        {
            try
            {
                #region chklstEqpNo
                chklstEqpNo.Items.Clear();
                string itemName = string.Empty;
                foreach (Node n in FormMainMDI.G_OPIAp.Dic_Node.Values)
                {
                    itemName = string.Format("{0}-{1}", n.NodeNo, n.NodeName);
                    chklstEqpNo.Items.Add(itemName);
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void ChooseCheckListBox(string checkedList)
        {
            try
            {
                List<string> _lstChecked = checkedList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

                string[] _itemName;

          
                for (int i = 0; i <= chklstEqpNo.Items.Count - 1; i++)
                {
                    _itemName = chklstEqpNo.Items[i].ToString().Split('-');

                    if (_itemName.Length != 2) continue;

                    if (_lstChecked.Contains(_itemName[0]))
                    {
                        chklstEqpNo.SetItemChecked(i, true);
                    }
                    else chklstEqpNo.SetItemChecked(i, false);
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
                if (cmbConditionStatus.SelectedIndex < 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Condition Status required！", MessageBoxIcon.Warning);
                    cmbConditionStatus.DroppedDown = true;
                    return false;
                }

                if ("".Equals(txtConditionSeqNo.Text.Trim()))
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Condition SeqNo required！", MessageBoxIcon.Warning);
                    txtConditionSeqNo.Focus();
                    return false;
                }

                int intSeqNo = -1;
                bool bolCheck = Int32.TryParse(txtConditionSeqNo.Text.Trim(), out intSeqNo);
                if (bolCheck == false)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Condition SeqNo must be a number！", MessageBoxIcon.Warning);
                    txtConditionSeqNo.Focus();
                    txtConditionSeqNo.SelectAll();
                    return false;
                }

                if ("".Equals(txtEqpList.Text.Trim()))
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Local List required！", MessageBoxIcon.Warning);
                    chklstEqpNo.Focus();
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
        }

        private bool CheckUniqueKeyOK(string lineType, int conditionSeqNo)
        {
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                //資料庫資料
                int rowCnt = (from msg in ctxBRM.SBRM_LINESTATUSSPEC
                              where msg.LINETYPE == lineType && msg.CONDITIONSEQNO == conditionSeqNo
                              select msg).Count();
                if (rowCnt > 0)
                {
                    string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is ({0}, {1})！", lineType, conditionSeqNo);
                    ShowMessage(this, this.lblCaption.Text, "", errMsg, MessageBoxIcon.Warning);
                    return false;
                }

                //已修改未更新物件
                if (FormMode == UniOPI.FormMode.AddNew)
                {
                    var add = ctxBRM.GetChangeSet().Inserts.OfType<SBRM_LINESTATUSSPEC>().Where(
                        msg => msg.LINETYPE == lineType && msg.CONDITIONSEQNO == conditionSeqNo);
                    if (add.Count() > 0)
                    {
                        string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is ({0}, {1})！", lineType, conditionSeqNo);
                        ShowMessage(this, this.lblCaption.Text, "", errMsg, MessageBoxIcon.Warning);
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);

                return false;
            }
        }

        private void ClearData()
        {
            try
            {
                cmbConditionStatus.SelectedIndex = -1;
                txtConditionSeqNo.Clear();
                txtEqpList.Clear();

                for (int i = 0; i <= chklstEqpNo.Items.Count - 1; i++)
                {
                    chklstEqpNo.SetItemChecked(i, false);
                }

                this.ActiveControl = cmbConditionStatus;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
    }

}
