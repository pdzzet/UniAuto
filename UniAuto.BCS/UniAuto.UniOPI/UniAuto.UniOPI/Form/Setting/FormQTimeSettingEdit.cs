using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormQTimeSettingEdit : FormBase
    {
        private SBRM_QTIME_DEF ObjEditData;

        public FormQTimeSettingEdit()
        {
            InitializeComponent();

            lblCaption.Text = "QTime Setting Edit";
        }

        public FormQTimeSettingEdit(SBRM_QTIME_DEF objData)
            : this()
        {
            if (objData == null)
            {
                FormMode = UniOPI.FormMode.AddNew;

                pnlAdd.Visible = true;
                dgvAddList.Visible = true;

                Size = new Size(800, 700);
            }
            else
            {
                FormMode = UniOPI.FormMode.Modify;

                ObjEditData = objData;

                pnlAdd.Visible = false;
                dgvAddList.Visible = false;

                Size = new Size(800, 420);
            }
        }

        private void FormQTimeSettingEdit_Load(object sender, EventArgs e)
        {
            try
            {
                this.InitialCombox();

                if (FormMode == UniOPI.FormMode.Modify)
                {
                    this.txtQTimeID.Enabled = false;
                }
                else
                    this.ActiveControl = cmbStartNodeNo;

                if (ObjEditData != null)
                {
                    this.txtQTimeID.Text = ObjEditData.QTIMEID;
                    this.cmbStartNodeNo.SelectedValue = ObjEditData.STARTNODENO;
                    this.cmbStartUnitNo.SelectedValue = ObjEditData.STARTUNITNO;
                    this.cmbStartEvtMsg.SelectedValue = ObjEditData.STARTEVENTMSG;
                    this.cmbEndNodeNo.SelectedValue = ObjEditData.ENDNODENO;
                    this.cmbEndUnitNo.SelectedValue = ObjEditData.ENDUNITNO;
                    this.cmbEndEvtMsg.SelectedValue = ObjEditData.ENDEVENTMSG;
                    this.txtSetTimeValue.Text = ObjEditData.SETTIMEVALUE.ToString();
                    this.txtRemark.Text = ObjEditData.REMARK;
                    this.txtStartNodeRecipeID.Text = ObjEditData.STARTNODERECIPEID;
                    this.txtCFRWQTime.Text = ObjEditData.CFRWQTIME.HasValue ? ObjEditData.CFRWQTIME.ToString() : "";
                    this.chkEnabled.Checked = ObjEditData.ENABLED == "Y" ? true : false;

                    //FCBPH_TYPE1,FCMPH_TYPE1,FCSPH_TYPE1,FCRPH_TYPE1,FCGPH_TYPE1,FCOPH_TYPE1
                    if (OPIConst.LstLineType_Photo.Contains(FormMainMDI.G_OPIAp.CurLine.LineType))
                    {
                        lblCFRWQTime.Visible = true;
                        txtCFRWQTime.Visible = true;
                        colCFReworkQTime.Visible = true;
                        lblReworkDesc.Visible = true;
                    }
                    else
                    {
                        lblCFRWQTime.Visible = false;
                        txtCFRWQTime.Visible = false;
                        colCFReworkQTime.Visible = false;
                        lblReworkDesc.Visible = false;
                    }
                }
                else
                {
                    //FCBPH_TYPE1,FCMPH_TYPE1,FCSPH_TYPE1,FCRPH_TYPE1,FCGPH_TYPE1,FCOPH_TYPE1
                    if (OPIConst.LstLineType_Photo.Contains(FormMainMDI.G_OPIAp.CurLine.LineType))
                    {
                        lblCFRWQTime.Visible = true;
                        txtCFRWQTime.Visible = true;
                        colCFReworkQTime.Visible = true;
                        lblReworkDesc.Visible = true;
                    }
                    else
                    {
                        lblCFRWQTime.Visible = false;
                        txtCFRWQTime.Visible = false;
                        colCFReworkQTime.Visible = false;
                        lblReworkDesc.Visible = false;
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
            try
            {
                Button _btn = (Button)sender;
                int iSetTimeValue = 0;
                int iCFRWQTime = 0;

                switch (_btn.Tag.ToString())
                {
                    case "ADD":

                        #region Add

                        #region Add to DataGridView

                        if (CheckData() == false) return;

                        string _serverName = FormMainMDI.G_OPIAp.CurLine.ServerName;
                        string _lineID = ((dynamic)this.cmbStartNodeNo.SelectedItem).LineID;
                        string _qTimeID = txtQTimeID.Text.Trim();
                        string _startNodeNo = ((dynamic)this.cmbStartNodeNo.SelectedItem).NodeNo;
                        string _startNodeID = ((dynamic)this.cmbStartNodeNo.SelectedItem).NodeID;
                        string _startUnitNo = ((dynamic)this.cmbStartUnitNo.SelectedItem).UnitNo;
                        string _startUnitID = ((dynamic)this.cmbStartUnitNo.SelectedItem).UnitID;
                        string _startEvtMsg = ((dynamic)this.cmbStartEvtMsg.SelectedItem).Key;
                        string _endNodeNo = ((dynamic)this.cmbEndNodeNo.SelectedItem).NodeNo;
                        string _endNodeID = ((dynamic)this.cmbEndNodeNo.SelectedItem).NodeID;
                        string _endUnitNo = ((dynamic)this.cmbEndUnitNo.SelectedItem).UnitNo;
                        string _endUnitID = ((dynamic)this.cmbEndUnitNo.SelectedItem).UnitID;
                        string _endEvtMsg = ((dynamic)this.cmbEndEvtMsg.SelectedItem).Key;
                        string _enable = chkEnabled.Checked ? "Y" : "N";                                                
                        string _remark = txtRemark.Text.Trim();
                        string _sartNodeRecipeID = txtStartNodeRecipeID.Text.Trim();
                        int.TryParse(txtSetTimeValue.Text.Trim(), out iSetTimeValue);
                        int.TryParse(txtCFRWQTime.Text.Trim(), out iCFRWQTime);

                        if (CheckUniqueKeyOK(_serverName, _qTimeID) == false) return;

                        #region 判斷是否存下gridview內
                        if (dgvAddList.Rows.Cast<DataGridViewRow>().Where(r =>
                            r.Cells[colQTimeID.Name].Value.ToString().Equals(_qTimeID) 
                            ).Count() > 0)
                        {
                            DataGridViewRow _addRow = dgvAddList.Rows.Cast<DataGridViewRow>().Where(r =>
                            r.Cells[colQTimeID.Name].Value.ToString().Equals(_qTimeID) 
                                ).First();

                            string _msg = string.Format("Q Time ID [{0}] is already insert. Please confirm whether you will overwite data ?", _qTimeID);

                            if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return;

                            _addRow.Cells[colLineID.Name].Value = _lineID;
                            _addRow.Cells[colStartLocalNo.Name].Value = _startNodeNo;
                            _addRow.Cells[colStartLocalID.Name].Value = _startNodeID;
                            _addRow.Cells[colStartUnitNo.Name].Value = _startUnitNo;
                            _addRow.Cells[colStartUnitID.Name].Value = _startUnitID;
                            _addRow.Cells[colStartEventMsg.Name].Value = _startEvtMsg;
                            _addRow.Cells[colEndLocalNo.Name].Value = _endNodeNo;
                            _addRow.Cells[colEndLocalID.Name].Value = _endNodeID;
                            _addRow.Cells[colEndUnitNo.Name].Value = _endUnitNo;
                            _addRow.Cells[colEndUnitID.Name].Value = _endUnitID;
                            _addRow.Cells[colEndEventMsg.Name].Value = _endEvtMsg;
                            _addRow.Cells[colNGQTime.Name].Value = iSetTimeValue;
                            _addRow.Cells[colRemark.Name].Value = _remark;
                            _addRow.Cells[colStartLocalRecipeID.Name].Value = _sartNodeRecipeID;
                            _addRow.Cells[colCFReworkQTime.Name].Value = iCFRWQTime;
                            _addRow.Cells[colEnabled.Name].Value = _enable;
                        }
                        else
                        {
                            dgvAddList.Rows.Add(_lineID,_qTimeID, _startNodeNo, _startNodeID, _startUnitNo, _startUnitID, _startEvtMsg, _endNodeNo, _endNodeID, _endUnitNo, _endUnitID, _endEvtMsg, iSetTimeValue, _remark, _sartNodeRecipeID, iCFRWQTime, _enable);
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

                                SBRM_QTIME_DEF _objAdd = null;

                                foreach (DataGridViewRow _row in dgvAddList.Rows)
                                {
                                    _objAdd = new SBRM_QTIME_DEF();

                                    int.TryParse(_row.Cells[colNGQTime.Name].Value.ToString(), out iSetTimeValue);
                                    int.TryParse(_row.Cells[colCFReworkQTime.Name].Value.ToString(), out iCFRWQTime);

                                    _objAdd.SERVERNAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                                    _objAdd.LINEID = _row.Cells[colLineID.Name].Value.ToString();
                                    _objAdd.QTIMEID = _row.Cells[colQTimeID.Name].Value.ToString();
                                    _objAdd.STARTNODENO = _row.Cells[colStartLocalNo.Name].Value.ToString();
                                    _objAdd.STARTNODEID = _row.Cells[colStartLocalID.Name].Value.ToString();
                                    _objAdd.STARTUNITNO = _row.Cells[colStartUnitNo.Name].Value.ToString();
                                    _objAdd.STARTNUNITID = _row.Cells[colStartUnitID.Name].Value.ToString();
                                    _objAdd.STARTEVENTMSG = _row.Cells[colStartEventMsg.Name].Value.ToString();
                                    _objAdd.ENDNODENO = _row.Cells[colEndLocalNo.Name].Value.ToString();
                                    _objAdd.ENDNODEID = _row.Cells[colEndLocalID.Name].Value.ToString(); ;
                                    _objAdd.ENDUNITNO = _row.Cells[colEndUnitNo.Name].Value.ToString();
                                    _objAdd.ENDNUNITID = _row.Cells[colEndUnitID.Name].Value.ToString();
                                    _objAdd.ENDEVENTMSG = _row.Cells[colEndEventMsg.Name].Value.ToString();
                                    _objAdd.SETTIMEVALUE = iSetTimeValue;
                                    _objAdd.REMARK = _row.Cells[colRemark.Name].Value.ToString();
                                    _objAdd.STARTNODERECIPEID = _row.Cells[colStartLocalRecipeID.Name].Value.ToString();
                                    _objAdd.CFRWQTIME = iCFRWQTime;
                                    _objAdd.ENABLED = _row.Cells[colEnabled.Name].Value.ToString();


                                    _ctxBRM.SBRM_QTIME_DEF.InsertOnSubmit(_objAdd);
                                }

                                break;
                                #endregion

                            case FormMode.Modify:

                                #region Modify

                                if (ObjEditData == null) return;

                                if (CheckData() == false) return;

                                int.TryParse(txtSetTimeValue.Text.Trim(), out iSetTimeValue);
                                int.TryParse(txtCFRWQTime.Text.Trim(), out iCFRWQTime);

                                ObjEditData.LINEID = ((dynamic)this.cmbStartNodeNo.SelectedItem).LineID;
                                ObjEditData.STARTNODENO =  ((dynamic)this.cmbStartNodeNo.SelectedItem).NodeNo;
                                ObjEditData.STARTNODEID =  ((dynamic)this.cmbStartNodeNo.SelectedItem).NodeID;
                                ObjEditData.STARTUNITNO = ((dynamic)this.cmbStartUnitNo.SelectedItem).UnitNo;
                                ObjEditData.STARTNUNITID =  ((dynamic)this.cmbStartUnitNo.SelectedItem).UnitID;
                                ObjEditData.STARTEVENTMSG = ((dynamic)this.cmbStartEvtMsg.SelectedItem).Key;
                                ObjEditData.ENDNODENO = ((dynamic)this.cmbEndNodeNo.SelectedItem).NodeNo;
                                ObjEditData.ENDNODEID = ((dynamic)this.cmbEndNodeNo.SelectedItem).NodeID;
                                ObjEditData.ENDUNITNO = ((dynamic)this.cmbEndUnitNo.SelectedItem).UnitNo;
                                ObjEditData.ENDNUNITID = ((dynamic)this.cmbEndUnitNo.SelectedItem).UnitID;
                                ObjEditData.ENDEVENTMSG = ((dynamic)this.cmbEndEvtMsg.SelectedItem).Key;
                                ObjEditData.SETTIMEVALUE = iSetTimeValue;
                                ObjEditData.REMARK = txtRemark.Text.Trim();
                                ObjEditData.STARTNODERECIPEID =  txtStartNodeRecipeID.Text.Trim();
                                ObjEditData.CFRWQTIME = iCFRWQTime;
                                ObjEditData.ENABLED = chkEnabled.Checked ? "Y" : "N";       

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

        private void btnStartNodeRecipeID_Click(object sender, EventArgs e)
        {
            try
            {
                if (this.cmbStartNodeNo.SelectedIndex == -1) return;
                string strNodeNo = ((dynamic)cmbStartNodeNo.SelectedItem).NodeNo;

                List<string> lstNodeRecipeID = UniTools.GetNodeRecipeID(strNodeNo);

                if (lstNodeRecipeID == null) return;

                FormSelectItem frmSelectItem = new FormSelectItem("Recipe ID", lstNodeRecipeID);
                DialogResult dia = frmSelectItem.ShowDialog(this);
                if (dia == System.Windows.Forms.DialogResult.OK)
                    this.txtStartNodeRecipeID.Text = frmSelectItem.ItemValue;
                else
                    this.txtStartNodeRecipeID.Text = string.Empty;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cmbStartNodeNo_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.cmbStartNodeNo.SelectedIndex == -1) return;

                string strNodeNo = ((dynamic)cmbStartNodeNo.SelectedItem).NodeNo;
                SetUnit(strNodeNo, cmbStartUnitNo);
                SetNodeRecipeLen(strNodeNo);

                #region Aligner Rework Q Time Check  --可不填值 (OPIConst.LstLineType_Photo.Contains(FormMainMDI.G_OPIAp.CurLine.LineType) && _startNodeNo =="L6")
                txtCFRWQTime.Text = "0";

                if (OPIConst.LstLineType_Photo.Contains(FormMainMDI.G_OPIAp.CurLine.LineType) && strNodeNo == "L6")
                    txtCFRWQTime.ReadOnly = false;
                else txtCFRWQTime.ReadOnly = true;
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cmbEndNodeNo_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (this.cmbEndNodeNo.SelectedIndex == -1) return;

                string strNodeNo = ((dynamic)cmbEndNodeNo.SelectedItem).NodeNo;
                SetUnit(strNodeNo, cmbEndUnitNo);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void txtNum_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = (!char.IsNumber(e.KeyChar) && e.KeyChar != (char)Keys.Delete && e.KeyChar != (char)Keys.Enter && e.KeyChar != (char)Keys.Back);
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
                        this.txtQTimeID.Text = _row.Cells[colQTimeID.Name].Value.ToString(); 
                        this.cmbStartNodeNo.SelectedValue = _row.Cells[colStartLocalNo.Name].Value.ToString();
                        this.cmbStartUnitNo.SelectedValue =  _row.Cells[colStartUnitNo.Name].Value.ToString();
                        this.cmbStartEvtMsg.SelectedValue = _row.Cells[colStartEventMsg.Name].Value.ToString();
                        this.cmbEndNodeNo.SelectedValue =  _row.Cells[colEndLocalNo.Name].Value.ToString();
                        this.cmbEndUnitNo.SelectedValue = _row.Cells[colEndUnitNo.Name].Value.ToString();
                        this.cmbEndEvtMsg.SelectedValue = _row.Cells[colEndEventMsg.Name].Value.ToString();
                        this.txtSetTimeValue.Text = _row.Cells[colNGQTime.Name].Value.ToString();
                        this.txtRemark.Text = _row.Cells[colRemark.Name].Value.ToString();
                        this.txtStartNodeRecipeID.Text = _row.Cells[colStartLocalRecipeID.Name].Value.ToString();
                        this.txtCFRWQTime.Text = _row.Cells[colCFReworkQTime.Name].Value.ToString();
                        this.chkEnabled.Checked = _row.Cells[colEnabled.Name].Value.ToString()=="Y" ? true:false ;
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
            SetNode();
            SetEventMsg();
        }

        private void SetNode()
        {
            try
            {
                var q = (from node in FormMainMDI.G_OPIAp.Dic_Node.Values
                         select new
                         {
                             IDNAME = string.Format("{0}-{1}-{2}", node.NodeNo, node.NodeID, node.NodeName),
                             node.NodeNo,
                             node.NodeID,
                             node.LineID
                         }).ToList();

                if (q == null || q.Count == 0) return;

                #region Start Node
                var s = q.ToList();
                this.cmbStartNodeNo.SelectedIndexChanged -= new EventHandler(cmbStartNodeNo_SelectedIndexChanged);
                this.cmbStartNodeNo.DataSource = s;
                this.cmbStartNodeNo.DisplayMember = "IDNAME";
                this.cmbStartNodeNo.ValueMember = "NODENO";
                this.cmbStartNodeNo.SelectedIndex = -1;
                this.cmbStartNodeNo.SelectedIndexChanged += new EventHandler(cmbStartNodeNo_SelectedIndexChanged);
                #endregion

                #region End Node
                var e = q.ToList();
                this.cmbEndNodeNo.SelectedIndexChanged -= new EventHandler(cmbEndNodeNo_SelectedIndexChanged);
                this.cmbEndNodeNo.DataSource = e;
                this.cmbEndNodeNo.DisplayMember = "IDNAME";
                this.cmbEndNodeNo.ValueMember = "NODENO";
                this.cmbEndNodeNo.SelectedIndex = -1;
                this.cmbEndNodeNo.SelectedIndexChanged += new EventHandler(cmbEndNodeNo_SelectedIndexChanged);
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetUnit(string NodeNo, ComboBox cmb)
        {
            try
            {
                var q = (from unit in FormMainMDI.G_OPIAp.Dic_Unit.Values
                         where unit.NodeNo == NodeNo
                         select new
                         {
                             unit.UnitID,
                             unit.UnitNo,
                             IDNAME = string.Format("{0}-{1}", unit.UnitNo, unit.UnitID)
                         }
                    ).ToList();

                if (q == null || q.Count == 0)
                {
                    q = new[] { new 
                { 
                    UnitID = "0", 
                    UnitNo = "0", 
                    IDNAME = "" } 
                }.ToList();
                    //return;
                }
                else
                {
                    q.Insert(0, new
                    {
                        UnitID = "0",
                        UnitNo = "0",
                        IDNAME = ""
                    });
                }

                cmb.DataSource = q;
                cmb.DisplayMember = "IDNAME";
                cmb.ValueMember = "UNITNO";
                cmb.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetEventMsg()
        {
            try
            {
                var data = new[] 
                { 
                    new { Key = "RECEIVE", Value = "Receive" } ,
                    new { Key = "SEND", Value = "Send" } ,
                    new { Key = "STORE", Value = "Store" } ,
                    new { Key = "FETCH", Value = "Fetch" }                       
                };


                var _start = data.ToList();
                var _end = data.ToList();

                if (FormMainMDI.G_OPIAp.CurLine.FabType == "CELL")
                {
                    _start.Add(new { Key = "PROCSTART", Value = "Process Start" });
                    _start.Add(new { Key = "PROCCOMP", Value = "Process Complete" });

                    _end.Add(new { Key = "PROCSTART", Value = "Process Start" });
                    _end.Add(new { Key = "PROCCOMP", Value = "Process Complete" });
                }

                this.cmbStartEvtMsg.DataSource = _start;
                this.cmbStartEvtMsg.DisplayMember = "Value"; 
                this.cmbStartEvtMsg.ValueMember = "Key";
                this.cmbStartEvtMsg.SelectedIndex = -1;


                this.cmbEndEvtMsg.DataSource = _end;
                this.cmbEndEvtMsg.DisplayMember = "Value";
                this.cmbEndEvtMsg.ValueMember = "Key";
                this.cmbEndEvtMsg.SelectedIndex = -1;
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
                if (string.IsNullOrEmpty(this.txtQTimeID.Text.Trim()))
                {
                    ShowMessage(this, lblCaption.Text , "", "QTime ID required！", MessageBoxIcon.Warning);
                    this.txtQTimeID.Focus();
                    return false;
                }
                if (this.cmbStartNodeNo.SelectedIndex < 0)
                {
                    ShowMessage(this, lblCaption.Text, "", "Start Local No required！", MessageBoxIcon.Warning);
                    this.cmbStartNodeNo.DroppedDown = true;
                    return false;
                }
                if (this.cmbStartUnitNo.SelectedIndex < 0)
                {
                    ShowMessage(this, lblCaption.Text, "", "Start Unit No required！", MessageBoxIcon.Warning);
                    this.cmbStartUnitNo.DroppedDown = true;
                    return false;
                }
                if (this.cmbStartEvtMsg.SelectedIndex < 0)
                {
                    ShowMessage(this, lblCaption.Text, "", "Start Event Message required！", MessageBoxIcon.Warning);
                    this.cmbStartEvtMsg.DroppedDown = true;
                    return false;
                }
                if (this.cmbEndNodeNo.SelectedIndex < 0)
                {
                    ShowMessage(this, lblCaption.Text, "", "End Local No required！", MessageBoxIcon.Warning);
                    this.cmbEndNodeNo.DroppedDown = true;
                    return false;
                }
                if (this.cmbEndUnitNo.SelectedIndex < 0)
                {
                    ShowMessage(this, lblCaption.Text, "", "End Unit No required！", MessageBoxIcon.Warning);
                    this.cmbEndUnitNo.DroppedDown = true;
                    return false;
                }
                if (this.cmbEndEvtMsg.SelectedIndex < 0)
                {
                    ShowMessage(this, lblCaption.Text, "", "End Event Message required！", MessageBoxIcon.Warning);
                    this.cmbEndEvtMsg.DroppedDown = true;
                    return false;
                }

                #region NG Q Time Check
                if (string.IsNullOrEmpty(this.txtSetTimeValue.Text.Trim()))
                {
                    ShowMessage(this, lblCaption.Text, "", "NG Q Time Value required", MessageBoxIcon.Warning);
                    this.txtSetTimeValue.Focus();
                    return false;
                }
                int iSetTimeValue = 0;
                if (!int.TryParse(this.txtSetTimeValue.Text.Trim(), out iSetTimeValue))
                {
                    ShowMessage(this, lblCaption.Text, "", "NG Q Time Value is not Number！", MessageBoxIcon.Warning);
                    this.txtSetTimeValue.Focus();
                    return false;
                }
                if (iSetTimeValue < 0 || iSetTimeValue > 65535)
                {
                    ShowMessage(this, lblCaption.Text, "", "NG Q Time Value is not Between 0~65535！", MessageBoxIcon.Warning);
                    this.txtSetTimeValue.Focus();
                    return false;
                }
                #endregion

                #region Coater Rework Q Time Check  --可不填值 
                string _startNodeNo = ((dynamic)this.cmbStartNodeNo.SelectedItem).NodeNo;
                if (OPIConst.LstLineType_Photo.Contains(FormMainMDI.G_OPIAp.CurLine.LineType) && _startNodeNo == "L6")
                {
                    iSetTimeValue = 0;
                    if (!int.TryParse(this.txtCFRWQTime.Text.Trim(), out iSetTimeValue))
                    {
                        ShowMessage(this, lblCaption.Text, "", "Coater Rework Q Time is not Number！", MessageBoxIcon.Warning);
                        this.txtCFRWQTime.Focus();
                        return false;
                    }

                    if (iSetTimeValue < 0 || iSetTimeValue > 65535)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Coater Rework Q Time is not Between 1~65535！", MessageBoxIcon.Warning);
                        this.txtCFRWQTime.Focus();
                        return false;
                    }
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);

                return false; 
            }
        }

        private bool CheckUniqueKeyOK(string ServerName, string QTimeID)
        {
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                //資料庫資料
                int rowCnt = (from msg in ctxBRM.SBRM_QTIME_DEF
                              where msg.SERVERNAME == ServerName && msg.QTIMEID == QTimeID
                              select msg).Count();
                if (rowCnt > 0)
                {
                    string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is ({0}, {1})！", ServerName, QTimeID);
                    ShowMessage(this, this.lblCaption.Text, "", errMsg, MessageBoxIcon.Warning);
                    return false;
                }

                //已修改未更新物件
                if (FormMode == UniOPI.FormMode.AddNew)
                {
                    var add = ctxBRM.GetChangeSet().Inserts.OfType<SBRM_QTIME_DEF>().Where(msg => msg.SERVERNAME == ServerName && msg.QTIMEID == QTimeID);
                    if (add.Count() > 0)
                    {
                        string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is ({0}, {1})！", ServerName, QTimeID);
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
            this.txtQTimeID.Clear();
            this.cmbStartNodeNo.SelectedIndex = -1;
            this.cmbStartUnitNo.SelectedIndex = -1;
            this.cmbStartEvtMsg.SelectedIndex = -1;
            this.cmbEndNodeNo.SelectedIndex = -1;
            this.cmbEndUnitNo.SelectedIndex = -1;
            this.cmbEndEvtMsg.SelectedIndex = -1;
            this.txtSetTimeValue.Clear();
            this.txtRemark.Clear();
            this.txtStartNodeRecipeID.Clear();
            this.txtCFRWQTime.Clear();
            this.chkEnabled.Checked = false;
        }

        private void SetNodeRecipeLen(string NodeNo)
        {
            if (cmbStartNodeNo.SelectedIndex < 0) return;

            int iNodeRecipeLen = GetNodeRecipeLen(NodeNo);
            txtStartNodeRecipeID.MaxLength = iNodeRecipeLen;
        }

        //private List<string> GetNodeRecipeID(string NodeNo)
        //{
        //    try
        //    {
        //        List<string> lstNodeRecipeID = new List<string>();

        //        UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

        //        //int iRecipeIndex = 0, iRecepeLen = 0;
        //        //GetNodeRecipeIndexLen(NodeNo, ref iRecipeIndex, ref iRecepeLen);

        //        var c = from msg in ctxBRM.SBRM_RECIPE
        //                where msg.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType
        //                select msg.PPID;

        //        DataTable dtRecipe = DBConnect.GetDataTable(ctxBRM, "", c);

        //        foreach (DataRow drRecipe in dtRecipe.Rows)
        //        {
        //            string strPPID = drRecipe["PPID"].ToString();

        //            List<string> _ppid = strPPID.Split(';').ToList();

        //            string result = _ppid.FirstOrDefault(s => s.Contains(NodeNo + ":"));

        //            if (result == null) continue;

        //            string[] _data = result.Split(':');

        //            if (_data.Length < 2) continue;

        //            if (_data[1] == FormMainMDI.G_OPIAp.Dic_Node[NodeNo].DefaultRecipeNo) continue;

        //            if (lstNodeRecipeID.Contains(_data[1])) continue;

        //            lstNodeRecipeID.Add(_data[1]);                    
        //        }

        //        if (lstNodeRecipeID.Count > 0) lstNodeRecipeID.Sort();

        //        return lstNodeRecipeID;
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
        //        ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);

        //        return null;
        //    }
        //}

        private int GetNodeRecipeLen(string NodeNo)
        {
            try
            {
                int iRecipeIndex = 0, iRecepeLen = 0;
                GetNodeRecipeIndexLen(NodeNo, ref iRecipeIndex, ref iRecepeLen);

                return iRecepeLen;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);

                return 0;
            }
        }

        private void GetNodeRecipeIndexLen(string NodeNo, ref int RecipeIndex, ref int RecepeLen)
        {
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                var a = from msg in ctxBRM.SBRM_NODE
                        where msg.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName && msg.NODENO == NodeNo
                        select msg;
                DataTable dtNode = DBConnect.GetDataTable(ctxBRM, "", a);

                if (dtNode.Rows.Count > 0)
                {
                    int.TryParse(dtNode.Rows[0]["RECIPEIDX"].ToString(), out RecipeIndex);
                    int.TryParse(dtNode.Rows[0]["RECIPELEN"].ToString(), out RecepeLen);
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
