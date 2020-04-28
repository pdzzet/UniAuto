using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormAlarmManagementEdit : FormBase
    {
        private SBRM_ALARM ObjAlarmToEdit;

        public FormAlarmManagementEdit()
        {
            InitializeComponent();
            lblCaption.Text = "Alarm Management Edit";
        }

        public FormAlarmManagementEdit(SBRM_ALARM objAlarm)
            : this()
        {
            if (objAlarm == null)
            {
                FormMode = UniOPI.FormMode.AddNew;
               
                pnlAdd.Visible = true;
                dgvAddList.Visible = true;

                Size = new Size(851, 600);
            }
            else
            {
                FormMode = UniOPI.FormMode.Modify;
                                
                ObjAlarmToEdit = objAlarm;

                pnlAdd.Visible = false;
                dgvAddList.Visible = false;

                Size = new Size(851, 300);
            }
        }

        private void FormAlarmManagementEdit_Load(object sender, EventArgs e)
        {
            try
            {
                this.InitialCombox();

                if (FormMode == UniOPI.FormMode.Modify)
                {
                    cmbNode.Enabled = false;
                    cmbUnit.Enabled = false;
                    cmbAlarmLevel.Enabled = false;
                    txtAlarmID.Enabled = false;

                    this.ActiveControl = txtAlarmText;

                    if (ObjAlarmToEdit != null)
                    {
                        cmbNode.SelectedValue = ObjAlarmToEdit.NODENO;
                        cmbUnit.SelectedValue = ObjAlarmToEdit.UNITNO;
                        cmbAlarmLevel.SelectedValue = ObjAlarmToEdit.ALARMLEVEL;
                        txtAlarmID.Text = ObjAlarmToEdit.ALARMID;
                        //txtAlarmCode.Text = ObjAlarmToEdit.ALARMCODE;
                        txtAlarmText.Text = ObjAlarmToEdit.ALARMTEXT;
                    }
                }
                else
                    this.ActiveControl = cmbNode;
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

                        var objNode = ((dynamic)cmbNode.SelectedItem);
                        string _lineID = objNode.LineID;
                        string _serverName = objNode.ServerName;
                        string _nodeNo = objNode.NodeNo;
                        string _unitNo = ((dynamic)cmbUnit.SelectedItem).UnitNo;
                        string _alarmLevel = cmbAlarmLevel.SelectedValue.ToString();
                        string _alarmID = txtAlarmID.Text.Trim();
                        string _alarmText = txtAlarmText.Text.Trim();

                        if (CheckUniqueKeyOK(_lineID, _nodeNo, _unitNo,_alarmLevel, _alarmID) == false) return;

                        #region 判斷是否存下gridview內
                        if (dgvAddList.Rows.Cast<DataGridViewRow>().Where(r => 
                            r.Cells[colLocalNo.Name].Value.ToString().Equals(_nodeNo) &&
                            r.Cells[colUnitNo.Name].Value.ToString().Equals(_unitNo) &&
                            r.Cells[colAlarmLevel.Name].Value.ToString().Equals(_alarmLevel) &&
                            r.Cells[colAlarmID.Name].Value.ToString().Equals(_alarmID)
                            ).Count() > 0)
                        {
                            DataGridViewRow _addRow = dgvAddList.Rows.Cast<DataGridViewRow>().Where(r =>
                            r.Cells[colLocalNo.Name].Value.ToString().Equals(_nodeNo) &&
                            r.Cells[colUnitNo.Name].Value.ToString().Equals(_unitNo) &&
                            r.Cells[colAlarmLevel.Name].Value.ToString().Equals(_alarmLevel) &&
                            r.Cells[colAlarmID.Name].Value.ToString().Equals(_alarmID)                                
                                ).First();

                            string _msg = string.Format("Local No[{0}],Unit No[{1}], Alarm Level[{2}], Alarm ID[{3}] is already insert. Please confirm whether you will overwite data ?", _nodeNo, _unitNo, _alarmLevel, _alarmID);

                            if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return;

                            _addRow.Cells[colAlarmText.Name].Value = _alarmText;
                        }
                        else
                        {
                            dgvAddList.Rows.Add(_nodeNo, _unitNo, _alarmLevel, _alarmID, _alarmText);
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

                                SBRM_ALARM _objAlarm = null;

                                foreach (DataGridViewRow _row in dgvAddList.Rows)
                                {
                                    _objAlarm = new SBRM_ALARM();

                                    _objAlarm.LINEID = FormMainMDI.G_OPIAp.CurLine.LineID;
                                    _objAlarm.SERVERNAME = FormMainMDI.G_OPIAp.CurLine.ServerName;     
                                    _objAlarm.NODENO = _row.Cells[colLocalNo.Name].Value.ToString();
                                    _objAlarm.UNITNO = _row.Cells[colUnitNo.Name].Value.ToString();
                                    _objAlarm.ALARMLEVEL = _row.Cells[colAlarmLevel.Name].Value.ToString();
                                    _objAlarm.ALARMID = _row.Cells[colAlarmID.Name].Value.ToString();
                                    _objAlarm.ALARMTEXT = _row.Cells[colAlarmText.Name].Value.ToString();
                                    _objAlarm.ALARMCODE = string.Empty; //保留暫不使用

                                    _ctxBRM.SBRM_ALARM.InsertOnSubmit(_objAlarm);
                                }

                                break;
                                #endregion

                            case FormMode.Modify:

                                #region Modify

                                ObjAlarmToEdit.ALARMTEXT = txtAlarmText.Text.Trim();

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

        private void cmbNode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbNode.SelectedIndex < 0)  return;

                string nodeID = ((dynamic)cmbNode.SelectedItem).NodeID;
                var q = (from unit in FormMainMDI.G_OPIAp.Dic_Unit.Values
                         where unit.NodeID == nodeID
                         select unit
                    ).ToList();

                q.Insert(0, new UniOPI.Unit()
                {
                    UnitID = "",
                    UnitNo = "0",
                    UnitType = ""
                });

                cmbUnit.DataSource = q;
                cmbUnit.DisplayMember = "UNITID";
                cmbUnit.ValueMember = "UNITNO";
                cmbUnit.SelectedIndex = 0;
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
                        cmbNode.SelectedValue = _row.Cells[colLocalNo.Name].Value.ToString();
                        cmbUnit.SelectedValue = _row.Cells[colUnitNo.Name].Value.ToString();
                        cmbAlarmLevel.SelectedValue = _row.Cells[colAlarmLevel.Name].Value.ToString();
                        txtAlarmID.Text = _row.Cells[colAlarmID.Name].Value.ToString();
                        txtAlarmText.Text = _row.Cells[colAlarmText.Name].Value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void txtNumber_KeyPress(object sender, KeyPressEventArgs e)
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

        private void InitialCombox()
        {
            try
            {
                #region Node
                var q = (from node in FormMainMDI.G_OPIAp.Dic_Node.Values
                         select new
                         {
                             IDNAME = string.Format("{0}-{1}", node.NodeNo, node.NodeID),
                             node.NodeNo,
                             node.NodeID,
                             node.LineID,
                             node.ServerName
                         }).ToList();

                if (q == null || q.Count == 0)
                    return;

                cmbNode.SelectedIndexChanged -= cmbNode_SelectedIndexChanged;
                cmbNode.DataSource = q;
                cmbNode.DisplayMember = "IDNAME";
                cmbNode.ValueMember = "NODENO";
                cmbNode.SelectedIndex = -1;
                cmbNode.SelectedIndexChanged += cmbNode_SelectedIndexChanged;
                #endregion

                #region Alarm Level
                DataTable dtAlarmLevel = UniTools.InitDt(new string[] { "DisplayField", "ValueField" });
                DataRow drNew;

                drNew = dtAlarmLevel.NewRow();
                drNew["DisplayField"] = "Alarm";
                drNew["ValueField"] = "A";
                dtAlarmLevel.Rows.Add(drNew);

                drNew = dtAlarmLevel.NewRow();
                drNew["DisplayField"] = "Warning";
                drNew["ValueField"] = "W";
                dtAlarmLevel.Rows.Add(drNew);

                this.cmbAlarmLevel.DataSource = dtAlarmLevel;
                this.cmbAlarmLevel.DisplayMember = "DisplayField";
                this.cmbAlarmLevel.ValueMember = "ValueField";
                #endregion
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
                if (cmbNode.SelectedIndex < 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Local ID required！", MessageBoxIcon.Warning);
                    cmbNode.DroppedDown = true;
                    return false;
                }

                if (cmbUnit.SelectedIndex < 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Unit ID required！", MessageBoxIcon.Warning);
                    cmbUnit.DroppedDown = true;
                    return false;
                }

                if ("".Equals(txtAlarmID.Text.Trim()))
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Alarm ID required！", MessageBoxIcon.Warning);
                    txtAlarmID.Focus();
                    return false;
                }

                int intAlarmID = -1;
                bool bolCheck = Int32.TryParse(txtAlarmID.Text.Trim(), out intAlarmID);
                if (bolCheck == false || (intAlarmID < 1 || intAlarmID > 65535))
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Alarm ID must between 1 and 65535！", MessageBoxIcon.Warning);
                    txtAlarmID.Focus();
                    txtAlarmID.SelectAll();
                    return false;
                }

                if ("".Equals(txtAlarmText.Text.Trim()))
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Alarm Text required！", MessageBoxIcon.Warning);
                    txtAlarmText.Focus();
                    return false;
                }

                if (txtAlarmText.Text.Trim().Length > 80)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "The content length of AlarmText must less than or equal to 80！", MessageBoxIcon.Warning);
                    txtAlarmText.Focus();
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

        private void ClearData()
        {
            try
            {
                cmbNode.SelectedIndex = -1;
                cmbUnit.SelectedIndex = -1;
                cmbAlarmLevel.SelectedIndex = 0;
                txtAlarmID.Text = string.Empty;
                txtAlarmText.Text = string.Empty;

                this.ActiveControl = cmbNode;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private bool CheckUniqueKeyOK(string LineID, string NodeNo, string UnitNo, string AlarmLevel,string AlarmID)
        {
            try
            {
                string errMsg =string.Empty ;

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                //資料庫資料
                int rowCnt = (from msg in ctxBRM.SBRM_ALARM
                              where msg.LINEID == LineID && msg.NODENO == NodeNo &&
                                           msg.UNITNO == UnitNo && msg.ALARMLEVEL == AlarmLevel && msg.ALARMID == AlarmID
                              select msg).Count();

                if (rowCnt > 0)
                {
                    errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is LineID [{0}], Local No [{1}], UnitNo [{2}], AlarmLevel[{3}], AlarmID[{4}]！", LineID, NodeNo, UnitNo,AlarmLevel ,AlarmID);
                    ShowMessage(this, lblCaption.Text, "", errMsg, MessageBoxIcon.Warning);
                    return false;
                }

                var add = ctxBRM.GetChangeSet().Inserts.OfType<SBRM_ALARM>().Where(
                    msg => msg.LINEID == LineID && msg.NODENO == NodeNo &&
                        msg.UNITNO == UnitNo && msg.ALARMLEVEL == AlarmLevel && msg.ALARMID == AlarmID);
                if (add.Count() > 0)
                {
                    errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is LineID [{0}], Local No [{1}], UnitNo [{2}], AlarmLevel[{3}], AlarmID[{4}]！", LineID, NodeNo, UnitNo,AlarmLevel ,AlarmID);
                    ShowMessage(this, lblCaption.Text, "", errMsg, MessageBoxIcon.Warning);
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
    }
}
