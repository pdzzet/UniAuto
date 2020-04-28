using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormSECSVariableManagementEdit : FormBase
    {
        private SBRM_SECS_VARIABLEDATA ObjEditData;

        public FormSECSVariableManagementEdit()
        {
            InitializeComponent();
            FormMode = UniOPI.FormMode.AddNew;
            lblCaption.Text = "SECS Variable Management Edit";           
        }

        public FormSECSVariableManagementEdit(SBRM_SECS_VARIABLEDATA objData)
            : this()
        {
            if (objData == null)
            {
                FormMode = UniOPI.FormMode.AddNew;

                pnlAdd.Visible = true;
                dgvAddList.Visible = true;

                Size = new Size(855, 630);
            }
            else
            {
                FormMode = UniOPI.FormMode.Modify;

                ObjEditData = objData;

                pnlAdd.Visible = false;
                dgvAddList.Visible = false;

                Size = new Size(850, 360);  
            }
        }

        private void FormSECSVariableManagementEdit_Load(object sender, EventArgs e)
        {
            this.InitialCombox();

            if (FormMode == UniOPI.FormMode.Modify)
            {
                this.cmbNode.Enabled = false;
                this.cmbDataType.Enabled = false;
                this.cmbTrID.Enabled = false;
                this.txtItemName.ReadOnly = true;

                if (ObjEditData != null)
                {
                    this.cmbNode.SelectedValue = FormMainMDI.G_OPIAp.Dic_Node[ObjEditData.NODENO].NodeID;
                    this.cmbDataType.SelectedValue = ObjEditData.DATATYPE;
                    this.cmbTrID.SelectedValue = ObjEditData.TRID;
                    this.txtItemID.Text = ObjEditData.ITEM_ID;
                    this.txtItemName.Text = ObjEditData.ITEM_NAME;
                    this.txtItemType.Text = ObjEditData.ITEM_TYPE;
                    if (ObjEditData.ITEM_SET == '0')
                        this.rbtnNoUse.Checked = true;
                    else
                        this.rbtnSet.Checked = true;
                    this.txtSp1.Text = ObjEditData.SP1;
                    this.txtSp2.Text = ObjEditData.SP2;
                    this.txtSp3.Text = ObjEditData.SP3;
                    this.txtDescription.Text = ObjEditData.DESCRIPTION;
                }
            }
            else
                this.ActiveControl = cmbTrID;
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

                        string _lineID = ((dynamic)cmbNode.SelectedItem).LineID;
                        string _localNo = ((dynamic)cmbNode.SelectedItem).NodeNo;
                        string _dataType = this.cmbDataType.SelectedValue.ToString();
                        string _trxID = this.cmbTrID.SelectedValue.ToString();
                        string _itemID = this.txtItemID.Text.Trim();
                        string _itemName = this.txtItemName.Text.Trim();
                        string _itemType = this.txtItemType.Text.Trim();
                        char _itemSet = rbtnNoUse.Checked ? '0' : '1';
                        string _sp1 = this.txtSp1.Text.Trim();
                        string _sp2 = this.txtSp2.Text.Trim();
                        string _sp3 = this.txtSp3.Text.Trim();
                        string _desc = this.txtDescription.Text.Trim();


                        if (CheckUniqueKeyOK( _lineID, _localNo, _dataType, _trxID, _itemName) == false) return;

                        #region 判斷是否存下gridview內
                        if (dgvAddList.Rows.Cast<DataGridViewRow>().Where(r =>
                            r.Cells[colLocalNo.Name].Value.ToString().Equals(_localNo) &&
                            r.Cells[colDataType.Name].Value.ToString().Equals(_dataType) &&
                            r.Cells[colTransferID.Name].Value.ToString().Equals(_trxID) &&
                            r.Cells[colItemName.Name].Value.ToString().Equals(_itemName)
                            ).Count() > 0)
                        {
                            DataGridViewRow _addRow = dgvAddList.Rows.Cast<DataGridViewRow>().Where(r =>
                            r.Cells[colLocalNo.Name].Value.ToString().Equals(_localNo) &&
                            r.Cells[colDataType.Name].Value.ToString().Equals(_dataType) &&
                            r.Cells[colTransferID.Name].Value.ToString().Equals(_trxID) &&
                            r.Cells[colItemName.Name].Value.ToString().Equals(_itemName)
                                ).First();

                            string _msg = string.Format("Local No [{0}],Data Type[{1}],Transfer ID [{2}],Item Name [{3}] is already insert. Please confirm whether you will overwite data ?", _localNo, _dataType, _trxID, _itemName);

                            if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return;

                            _addRow.Cells[colItemID.Name].Value = _itemID;
                            _addRow.Cells[colItemType.Name].Value = _itemType;
                            _addRow.Cells[colItemSet.Name].Value = _itemSet;
                            _addRow.Cells[colDescription.Name].Value = _desc;
                            _addRow.Cells[colSP1.Name].Value = _sp1;
                            _addRow.Cells[colSP2.Name].Value = _sp2;
                            _addRow.Cells[colSP3.Name].Value = _sp3;
                        }
                        else
                        {
                            dgvAddList.Rows.Add(_lineID, _localNo, _dataType, _trxID, _itemID, _itemName, _itemType, _itemSet, _desc, _sp1,_sp2,_sp3);                                
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

                                SBRM_SECS_VARIABLEDATA _objAdd = null;

                                foreach (DataGridViewRow _row in dgvAddList.Rows)
                                {
                                    _objAdd = new SBRM_SECS_VARIABLEDATA();

                                    _objAdd.LINEID = _row.Cells[colLineID.Name].Value.ToString();
                                    _objAdd.NODENO = _row.Cells[colLocalNo.Name].Value.ToString();
                                    _objAdd.DATATYPE = _row.Cells[colDataType.Name].Value.ToString();
                                    _objAdd.TRID = _row.Cells[colTransferID.Name].Value.ToString();
                                    _objAdd.ITEM_NAME = _row.Cells[colItemName.Name].Value.ToString();

                                    _objAdd.ITEM_ID = _row.Cells[colItemID.Name].Value.ToString();                                    
                                    _objAdd.ITEM_TYPE = _row.Cells[colItemType.Name].Value.ToString();
                                    _objAdd.ITEM_SET = _row.Cells[colItemSet.Name].Value.ToString()=="1" ? '1':'0';
                                    _objAdd.DESCRIPTION = _row.Cells[colDescription.Name].Value.ToString();
                                    _objAdd.SP1 = _row.Cells[colSP1.Name].Value.ToString();
                                    _objAdd.SP2 = _row.Cells[colSP2.Name].Value.ToString();
                                    _objAdd.SP3 = _row.Cells[colSP3.Name].Value.ToString();

                                    _ctxBRM.SBRM_SECS_VARIABLEDATA.InsertOnSubmit(_objAdd);
                                }

                                break;
                                #endregion

                            case FormMode.Modify:

                                #region Modify

                                if (ObjEditData == null) return;

                                ObjEditData.ITEM_ID = this.txtItemID.Text.Trim();
                                ObjEditData.ITEM_TYPE = this.txtItemType.Text.Trim();
                                ObjEditData.ITEM_SET = rbtnNoUse.Checked ? '0' : '1';
                                ObjEditData.DESCRIPTION = this.txtDescription.Text.Trim();
                                ObjEditData.SP1 = this.txtSp1.Text.Trim();
                                ObjEditData.SP2 = this.txtSp2.Text.Trim();
                                ObjEditData.SP3 = this.txtSp3.Text.Trim();

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

        #region Event
        private void dgvAddList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 )
            {
                DataGridViewRow _row = dgvAddList.SelectedRows[0];

                if (_row != null)
                {
                    this.cmbNode.SelectedValue = _row.Cells[colLocalNo.Name].Value.ToString();
                    this.cmbDataType.SelectedValue = _row.Cells[colDataType.Name].Value.ToString();
                    this.cmbTrID.SelectedValue = _row.Cells[colTransferID.Name].Value.ToString();
                    this.txtItemID.Text = _row.Cells[colItemID.Name].Value.ToString();
                    this.txtItemName.Text = _row.Cells[colItemName.Name].Value.ToString();
                    this.txtItemType.Text = _row.Cells[colItemType.Name].Value.ToString();
                    if (_row.Cells[colItemSet.Name].Value.ToString() == "0")
                        this.rbtnNoUse.Checked = true;
                    else
                        this.rbtnSet.Checked = true;
                    this.txtSp1.Text = _row.Cells[colSP1.Name].Value.ToString();
                    this.txtSp2.Text = _row.Cells[colSP2.Name].Value.ToString();
                    this.txtSp3.Text = _row.Cells[colSP3.Name].Value.ToString();
                    this.txtDescription.Text = _row.Cells[colDescription.Name].Value.ToString();
                }
            }
        }
        #endregion

        #region Method

        private void InitialCombox()
        {
            SetNode();
            SetDataType();
            SetTrID();
        }

        private void SetNode()
        {
            var q = (from node in FormMainMDI.G_OPIAp.Dic_Node.Values
                     select new
                     {
                         IDNAME = string.Format("{0}-{1}-{2}", node.NodeNo, node.NodeID, node.NodeName),
                         node.NodeNo,
                         node.NodeID
                     }).ToList();

            if (q == null || q.Count == 0) return;

            cmbNode.DataSource = q;
            cmbNode.DisplayMember = "IDNAME";
            cmbNode.ValueMember = "NODEID";
            cmbNode.SelectedIndex = -1;
        }

        private void SetDataType()
        {
            var data = new[]             
            { 
                new CmbItem{ ID = "APC_IMPORTANT", NAME = "APC Important" } ,
                new CmbItem{ ID = "APC_NORMAL", NAME = "ACP Normal" } ,
                new CmbItem{ ID = "SPECIAL_DATA", NAME = "Special Data" },
                new CmbItem{ ID = "UTILITY", NAME = "Utility" }
            };
            this.cmbDataType.DataSource = data;
            this.cmbDataType.DisplayMember = "NAME";
            this.cmbDataType.ValueMember = "ID";
            this.cmbDataType.SelectedIndex = -1;
        }

        private void SetTrID()
        {
            var data = new[]             
            { 
                new CmbItem{ ID = "S1F5_02", NAME = "S1F5 APC Important for name" },
                new CmbItem{ ID = "S1F5_04", NAME = "S1F5 APC Normal for name" },
                new CmbItem{ ID = "S1F5_06", NAME = "S1F5 Special Data for name" },
                new CmbItem{ ID = "S1F5_07", NAME = "S1F5 APC Important for ID" },
                new CmbItem{ ID = "S1F5_08", NAME = "S1F5 APC Normal for ID" },
                new CmbItem{ ID = "S1F5_10", NAME = "S1F5 Special Data for ID" },
                new CmbItem{ ID = "S6F3_04", NAME = "S6F3 APC Important for name" },
                new CmbItem{ ID = "S6F3_05", NAME = "S6F3 APC Normal for name" },
                new CmbItem{ ID = "S6F3_08", NAME = "S6F3 Special Data for name" },
                new CmbItem{ ID = "S6F3_09", NAME = "S6F3 APC Important for ID" },
                new CmbItem{ ID = "S6F3_10", NAME = "S6F3 APC Normal for ID" },
                new CmbItem{ ID = "S6F3_11", NAME = "S6F3 Special Data for ID" },
                new CmbItem{ ID = "APCIM", NAME = "Nikon APC Important" } ,
                new CmbItem{ ID = "APCNO", NAME = "Nikon APC Normal" } ,
                new CmbItem{ ID = "SPCAL", NAME = "Nikon Special Data" },
                new CmbItem{ ID = "UTILY", NAME = "Nikon Utility" }
            };
            this.cmbTrID.DataSource = data;
            this.cmbTrID.DisplayMember = "NAME";
            this.cmbTrID.ValueMember = "ID";
            this.cmbTrID.SelectedIndex = -1;
        }

        private bool CheckData()
        {
            if (this.cmbNode.SelectedIndex < 0)
            {
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", "Local No required！", MessageBoxIcon.Warning);
                this.cmbNode.Focus();
                return false;
            }

            if (this.cmbDataType.SelectedIndex < 0)
            {
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", "Data Type required！", MessageBoxIcon.Warning);
                this.cmbDataType.Focus();
                return false;
            }
            
            if (this.cmbTrID.SelectedIndex < 0)
            {
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", "Transfer ID required！", MessageBoxIcon.Warning);
                this.cmbTrID.Focus();
                return false;
            }

            if (string.IsNullOrEmpty(this.txtItemName.Text))
            {
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", "Item Name required！", MessageBoxIcon.Warning);
                this.txtItemName.Focus();
                return false;
            }

            return true;
        }

        private bool CheckUniqueKeyOK(string LineID, string NodeNo, string DataType, string TrID, string strItemName)
        {
            UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

            string _lineType = FormMainMDI.G_OPIAp.CurLine.LineType;

            //資料庫資料
            int rowCnt = (from msg in ctxBRM.SBRM_SECS_VARIABLEDATA
                          where msg.LINEID == LineID && msg.LINETYPE == _lineType && msg.NODENO == NodeNo && msg.DATATYPE == DataType
                          && msg.TRID == TrID && msg.ITEM_NAME == strItemName
                          select msg).Count();
            if (rowCnt > 0)
            {
                string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is ({0}, {1}, {2}, {3}, {4}, {5})！",
                    LineID, _lineType, NodeNo, DataType, TrID, strItemName);
                ShowMessage(this, this.lblCaption.Text, "", errMsg, MessageBoxIcon.Warning);
                return false;
            }

            //已修改未更新物件
            if (FormMode == UniOPI.FormMode.AddNew)
            {
                var add = ctxBRM.GetChangeSet().Inserts.OfType<SBRM_SECS_VARIABLEDATA>().Where(
                    msg => msg.LINEID == LineID && msg.LINETYPE == _lineType && msg.NODENO == NodeNo && msg.DATATYPE == DataType
                          && msg.TRID == TrID && msg.ITEM_NAME == strItemName);
                if (add.Count() > 0)
                {
                    string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is ({0}, {1}, {2}, {3}, {4}, {5})！",
                    LineID, _lineType, NodeNo, DataType, TrID, strItemName);
                    ShowMessage(this, this.lblCaption.Text, "", errMsg, MessageBoxIcon.Warning);
                    return false;
                }
            }

            return true;
        }

        private void ClearData()
        {
            this.cmbNode.SelectedIndex = -1;
            this.cmbDataType.SelectedIndex = -1;
            this.cmbTrID.SelectedIndex = -1;
            this.txtItemID.Clear();
            this.txtItemName.Clear();
            this.txtItemType.Clear();
            this.rbtnNoUse.Checked = true;
            this.txtSp1.Clear();
            this.txtSp2.Clear();
            this.txtSp3.Clear();
            this.txtDescription.Clear();
        }

        #endregion

    }
}
