using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormSubBlockEdit : FormBase
    {
        private SBRM_SUBBLOCK_CTRL ObjEditData;

        public FormSubBlockEdit()
        {
            
            InitializeComponent();
            lblCaption.Text = "Sub Block Edit";

            txtStartEQP.Click += txtEQP_Enter;
            txtControlEQP.Click += txtEQPNotShowUnit_Enter;
            txtNextSubBlockEQP.Click += txtEQPNotShowUnit_Enter;
            txtNextSubBlockEQPList.Click += txtEQP_Enter;
        }

        public FormSubBlockEdit(SBRM_SUBBLOCK_CTRL objTable)
            : this()
        {
            if (objTable == null)
            {
                FormMode = UniOPI.FormMode.AddNew;

                pnlAdd.Visible = true;
                dgvSubBlock.Visible = true;
                this.Height = 550;
            }
            else
            {                
                FormMode = UniOPI.FormMode.Modify;

                ObjEditData = objTable;

                pnlAdd.Visible = false;
                dgvSubBlock.Visible = false;

                this.Height = 320;
            }
        }

        private void FormQTimeSettingEdit_Load(object sender, EventArgs e)
        {
            try
            {
                this.InitialCombox();

                if (FormMode == UniOPI.FormMode.Modify)
                {                    
                    this.txtSubBlockID.Enabled = false;

                    if (ObjEditData != null)
                    {
                        this.txtSubBlockID.Text = ObjEditData.SUBBLOCKID;
                        this.txtStartEQP.Text = ObjEditData.STARTEQP;
                        this.txtControlEQP.Text = ObjEditData.CONTROLEQP;
                        this.cbobStartEventMsg.SelectedValue = ObjEditData.STARTEVENTMSG;
                        this.txtInterLockNo.Text = ObjEditData.INTERLOCKNO;
                        this.txtInterLockReplyNo.Text = ObjEditData.INTERLOCKREPLYNO;
                        this.txtRemark.Text = ObjEditData.REMARK;
                        this.txtNextSubBlockEQP.Text = ObjEditData.NEXTSUBBLOCKEQP;
                        this.txtNextSubBlockEQPList.Text = ObjEditData.NEXTSUBBLOCKEQPLIST;
                        this.chkEnabled.Checked = ObjEditData.ENABLED == "Y" ? true : false;
                    }                    
                }
                else
                {
                    this.ActiveControl = txtSubBlockID;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void txtEQP_Enter(object sender, EventArgs e)
        {
            try
            {
                TextBox txt = sender as TextBox;

                FormSubBlockEQSelect _frm = new FormSubBlockEQSelect();
                _frm.ShowPort = true;
                _frm.SelectedValue = txt.Text.Trim();
                _frm.StartPosition = FormStartPosition.CenterScreen;
                if ("txtNextSubBlockEQPList".Equals(txt.Name))
                {
                    _frm.MultipleSelect = true;
                }

                var result = _frm.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string val = _frm.ReturnValue;
                    txt.Text = val;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void txtEQPNotShowUnit_Enter(object sender, EventArgs e)
        {
            try
            {
                TextBox txt = sender as TextBox;

                FormSubBlockEQSelect frmSel = new FormSubBlockEQSelect();
                frmSel.SelectedValue = txt.Text.Trim();
                frmSel.StartPosition = FormStartPosition.CenterScreen;
                frmSel.ShowUnit = false;
                if ("txtNextSubBlockEQPList".Equals(txt.Name))
                {
                    frmSel.MultipleSelect = true;
                }

                var result = frmSel.ShowDialog();
                if (result == DialogResult.OK)
                {
                    string val = frmSel.ReturnValue;
                    txt.Text = val;
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
            SetEventMsg();
        }

        private void SetEventMsg()
        {
            try
            {
                var data = new[] 
            { 
                new { Key = "", Value = "" } ,
                new { Key = "RECEIVE", Value = "Receive" } ,
                new { Key = "SEND", Value = "Send" } ,
                new { Key = "STORE", Value = "Store" } ,
                new { Key = "FETCH", Value = "Fetch" } 
            };

                var s = data.ToList();
                this.cbobStartEventMsg.DataSource = s;
                this.cbobStartEventMsg.DisplayMember = "Value";
                this.cbobStartEventMsg.ValueMember = "Key";
                this.cbobStartEventMsg.SelectedIndex = -1;
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
                if ("".Equals(txtSubBlockID.Text.Trim()))
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Block ID required！", MessageBoxIcon.Warning);
                    txtSubBlockID.Focus();
                    return false;
                }

                if ("".Equals(txtStartEQP.Text.Trim()))
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Block Start EQP required！", MessageBoxIcon.Warning);
                    txtStartEQP.Focus();
                    return false;
                }

                if ("".Equals(txtControlEQP.Text.Trim()))
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Control Interlock Command EQP required！", MessageBoxIcon.Warning);
                    txtControlEQP.Focus();
                    return false;
                }


                //若startEQP不是port的情况，STARTEVENTMSG不能为空
                if (txtStartEQP.Text.ToString().Contains(":P") == false)
                {
                    if (cbobStartEventMsg.SelectedIndex < 0)
                    {
                        ShowMessage(this, this.lblCaption.Text, "", "Start EQP Event Message required！", MessageBoxIcon.Warning);
                        cbobStartEventMsg.DroppedDown = true;
                        return false;
                    }

                    if (cbobStartEventMsg.SelectedValue.ToString() == string.Empty)
                    {
                        ShowMessage(this, this.lblCaption.Text, "", "Start EQP Event Message required！", MessageBoxIcon.Warning);
                        cbobStartEventMsg.DroppedDown = true;
                        return false;
                    }
                }

                if ("".Equals(txtInterLockNo.Text.Trim()))
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Control EQP's InterlockNo required！", MessageBoxIcon.Warning);
                    txtInterLockNo.Focus();
                    return false;
                }

                if ("".Equals(txtNextSubBlockEQP.Text.Trim()))
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Next Sub Block Connect EQP required！", MessageBoxIcon.Warning);
                    txtNextSubBlockEQP.Focus();
                    return false;
                }

                if ("".Equals(txtNextSubBlockEQPList.Text.Trim()))
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Next Sub Block EQP List required！", MessageBoxIcon.Warning);
                    txtNextSubBlockEQPList.Focus();
                    return false;
                }

                if ("".Equals(txtInterLockReplyNo.Text.Trim()))
                {
                    ShowMessage(this, this.lblCaption.Text, "", "The MPLC InterLock Trx Reply Number required！", MessageBoxIcon.Warning);
                    txtInterLockReplyNo.Focus();
                    return false;
                }

                if ("".Equals(txtRemark.Text.Trim()))
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Remark required！", MessageBoxIcon.Warning);
                    txtRemark.Focus();
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

        private bool CheckUniqueKeyOK(string LineID, string SubBlockID)
        {
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                //資料庫資料
                int rowCnt = (from msg in ctxBRM.SBRM_SUBBLOCK_CTRL
                              where msg.LINEID == LineID && msg.SUBBLOCKID == SubBlockID
                              select msg).Count();
                if (rowCnt > 0)
                {
                    string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is ({0}, {1})！", LineID, SubBlockID);
                    ShowMessage(this, this.lblCaption.Text, "", errMsg, MessageBoxIcon.Warning);
                    return false;
                }

                //已修改未更新物件
                if (FormMode == UniOPI.FormMode.AddNew)
                {
                    var add = ctxBRM.GetChangeSet().Inserts.OfType<SBRM_SUBBLOCK_CTRL>().Where(msg => msg.LINEID == LineID && msg.SUBBLOCKID == SubBlockID);
                    if (add.Count() > 0)
                    {
                        string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is ({0}, {1})！", LineID, SubBlockID);
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
                this.txtSubBlockID.Clear();
                this.txtStartEQP.Clear();
                this.txtControlEQP.Clear();
                this.cbobStartEventMsg.SelectedIndex = -1;
                this.txtInterLockNo.Clear();
                this.txtInterLockReplyNo.Clear();
                this.txtRemark.Clear();
                this.txtNextSubBlockEQP.Clear();
                this.txtNextSubBlockEQPList.Clear();
                this.chkEnabled.Checked = false;
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

                string strLineID = FormMainMDI.G_OPIAp.CurLine.LineID;
                string strServerName = FormMainMDI.G_OPIAp.CurLine.ServerName;
                string strSubBlockID = txtSubBlockID.Text.Trim();

                switch (_btn.Tag.ToString())
                {
                    case "ADD":

                        #region Add to DataGridView
                        if (CheckData() == false) return;
                        
                        string strStartEqp = txtStartEQP.Text.Trim();
                        string strControlEqp = txtControlEQP.Text.Trim();
                        string strNextSubBlockEqp = txtNextSubBlockEQP.Text.Trim();
                        string strNextSubBlockEqpList = txtNextSubBlockEQPList.Text.Trim();
                        string strEnable = chkEnabled.Checked ? "Y" : "N";
                        string strStartEventMsg = string.Empty;
                        string strInterLockNo = txtInterLockNo.Text.Trim();
                        string strInterLockReplyNo = txtInterLockReplyNo.Text.Trim();
                        string strRemark = txtRemark.Text.Trim();

                        if (this.cbobStartEventMsg.SelectedItem != null)
                            strStartEventMsg = ((dynamic)this.cbobStartEventMsg.SelectedItem).Key;

                        if (CheckUniqueKeyOK(strLineID, strSubBlockID) == false) return;

                        #region 判斷是否存下gridview內
                        if (dgvSubBlock.Rows.Cast<DataGridViewRow>().Where(r =>
                            r.Cells[colSubBlockID.Name].Value.ToString().Equals(strSubBlockID)
                            ).Count() > 0)
                        {
                            DataGridViewRow _addRow = dgvSubBlock.Rows.Cast<DataGridViewRow>().Where(r =>
                            r.Cells[colSubBlockID.Name].Value.ToString().Equals(strSubBlockID)
                                ).First();

                            string _msg = string.Format("Block ID [{0}] is already insert. Please confirm whether you will overwite data ?", strSubBlockID);

                            if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text,  _msg)) return;

                            _addRow.Cells[colStartEQP.Name].Value = strStartEqp;
                            _addRow.Cells[colControlEQP.Name].Value = strControlEqp;
                            _addRow.Cells[colNextBlockEQP.Name].Value = strNextSubBlockEqp;
                            _addRow.Cells[colNextBlockEQPList.Name].Value = strNextSubBlockEqpList;
                            _addRow.Cells[colEnable.Name].Value = strEnable;
                            _addRow.Cells[colStartEvent.Name].Value = strStartEventMsg;        
                            _addRow.Cells[colEQPInterlockNo.Name].Value = strInterLockNo;
                            _addRow.Cells[colMPLCInterlockNo.Name].Value = strInterLockReplyNo;
                            _addRow.Cells[colRemark.Name].Value = strRemark;
                        }
                        else
                        {
                            dgvSubBlock.Rows.Add(strSubBlockID, strStartEqp, strControlEqp, strNextSubBlockEqp, strNextSubBlockEqpList, strEnable, strStartEventMsg,
                                strInterLockNo, strInterLockReplyNo, strRemark);
                        }

                        #endregion

                        #region clear data
                        this.ClearData();
                        #endregion

                        #endregion
                        break;

                    case "OK":

                        UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                        SBRM_SUBBLOCK_CTRL _sbuBlock = null;       

                        #region OK
                        switch (FormMode)
                        {
                            case FormMode.AddNew:

                                #region Add

                                foreach (DataGridViewRow _row in dgvSubBlock.Rows)
                                {
                                    _sbuBlock = new SBRM_SUBBLOCK_CTRL();
                                    _sbuBlock.LINEID = FormMainMDI.G_OPIAp.CurLine.LineID;
                                    _sbuBlock.SERVERNAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                                    _sbuBlock.SUBBLOCKID = _row.Cells[colSubBlockID.Name].Value.ToString();
                                    _sbuBlock.STARTEQP = _row.Cells[colStartEQP.Name].Value.ToString();
                                    _sbuBlock.CONTROLEQP = _row.Cells[colControlEQP.Name].Value.ToString();
                                    _sbuBlock.NEXTSUBBLOCKEQP =_row.Cells[colNextBlockEQP.Name].Value.ToString();
                                    _sbuBlock.NEXTSUBBLOCKEQPLIST = _row.Cells[colNextBlockEQPList.Name].Value.ToString();
                                    _sbuBlock.ENABLED = _row.Cells[colEnable.Name].Value.ToString();
                                    _sbuBlock.STARTEVENTMSG = _row.Cells[colStartEvent.Name].Value.ToString();                    
                                    _sbuBlock.INTERLOCKNO = _row.Cells[colEQPInterlockNo.Name].Value.ToString();                                                            
                                    _sbuBlock.INTERLOCKREPLYNO = _row.Cells[colMPLCInterlockNo.Name].Value.ToString();
                                    _sbuBlock.REMARK = _row.Cells[colRemark.Name].Value.ToString();

                                    _ctxBRM.SBRM_SUBBLOCK_CTRL.InsertOnSubmit(_sbuBlock);
                                }

                                #endregion

                                break;

                            case FormMode.Modify:

                                #region Modify

                                if (CheckData() == false) return;
                                //20150318 cy:修改條件where msg.LINEID == strLineID為where msg.SERVERNAME == strServerName
                                var objSelModify = from msg in _ctxBRM.SBRM_SUBBLOCK_CTRL
                                                   where msg.SERVERNAME == strServerName && msg.SUBBLOCKID == strSubBlockID
                                                   select msg;
                                if (objSelModify.Count() > 0) _sbuBlock = objSelModify.ToList()[0];
                                //20150318 cy:修改條件where msg.LINEID == strLineID為where msg.SERVERNAME == strServerName
                                var objAddModify = from msg in _ctxBRM.GetChangeSet().Inserts.OfType<SBRM_SUBBLOCK_CTRL>()
                                                   where msg.SERVERNAME == strServerName && msg.SUBBLOCKID == strSubBlockID
                                                   select msg;
                                if (objAddModify.Count() > 0) _sbuBlock = objAddModify.ToList()[0];


                                if (_sbuBlock == null) return;

                                string _startEQP = txtStartEQP.Text.Trim();
                                string _controlEqp = txtControlEQP.Text.Trim();
                                string _nextSubBlockEqp = txtNextSubBlockEQP.Text.Trim();
                                string _nextSubBlockEqpList = txtNextSubBlockEQPList.Text.Trim();
                                string _enable = chkEnabled.Checked ? "Y" : "N";
                                string _startEventMsg = string.Empty;
                                string _interLockNo = txtInterLockNo.Text.Trim();
                                string _interLockReplyNo = txtInterLockReplyNo.Text.Trim();
                                string _remark = txtRemark.Text.Trim();

                                if (this.cbobStartEventMsg.SelectedItem != null)
                                    _startEventMsg = ((dynamic)this.cbobStartEventMsg.SelectedItem).Key;

                                _sbuBlock.STARTEQP = _startEQP;
                                _sbuBlock.CONTROLEQP = _controlEqp;
                                _sbuBlock.STARTEVENTMSG = _startEventMsg;
                                _sbuBlock.INTERLOCKNO = _interLockNo;
                                _sbuBlock.REMARK = _remark;
                                _sbuBlock.NEXTSUBBLOCKEQP = _nextSubBlockEqp;
                                _sbuBlock.NEXTSUBBLOCKEQPLIST = _nextSubBlockEqpList;
                                _sbuBlock.INTERLOCKREPLYNO = _interLockReplyNo;
                                _sbuBlock.ENABLED = _enable;
                                _sbuBlock.STARTEVENTMSG = _startEventMsg;
                                #endregion

                                break;

                            default:
                                break;
                        }

                        this.DialogResult = System.Windows.Forms.DialogResult.OK;

                        this.Close();

                        #endregion

                        break;

                    case "Cancel":

                        this.DialogResult = System.Windows.Forms.DialogResult.Cancel;

                        this.Close();

                        break;

                    case "Clear":

                        #region clear data
                        this.ClearData();
                        #endregion

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

        private void dgvSubBlock_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow _row = dgvSubBlock.SelectedRows[0];

                if (_row != null)
                {
                    this.txtSubBlockID.Text = _row.Cells[colSubBlockID.Name].Value.ToString();
                    this.txtStartEQP.Text = _row.Cells[colStartEQP.Name].Value.ToString();
                    this.txtControlEQP.Text =  _row.Cells[colControlEQP.Name].Value.ToString();
                    this.cbobStartEventMsg.SelectedValue = _row.Cells[colStartEvent.Name].Value.ToString();
                    this.txtInterLockNo.Text = _row.Cells[colEQPInterlockNo.Name].Value.ToString();
                    this.txtInterLockReplyNo.Text = _row.Cells[colMPLCInterlockNo.Name].Value.ToString();
                    this.txtRemark.Text =_row.Cells[colRemark.Name].Value.ToString();
                    this.txtNextSubBlockEQP.Text = _row.Cells[colNextBlockEQP.Name].Value.ToString();
                    this.txtNextSubBlockEQPList.Text = _row.Cells[colNextBlockEQPList.Name].Value.ToString();
                    this.chkEnabled.Checked = _row.Cells[colEnable.Name].Value.ToString() == "Y" ? true : false;
                }
            }
        }

        private void txtInterLockNo_KeyPress(object sender, KeyPressEventArgs e)
        {
            int asciiCode = (int)e.KeyChar;
            if ((asciiCode >= 48 & asciiCode <= 57) |   // 0-9
                asciiCode == 8)    // Backspace
            {
                e.Handled = false;
            }
            else
                e.Handled = true;
        }
    }
}
