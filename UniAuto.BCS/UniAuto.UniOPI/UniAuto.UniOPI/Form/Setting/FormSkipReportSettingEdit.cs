using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormSkipReportSettingEdit : FormBase
    {
        private SBRM_SKIPREPORT ObjEditData;

        public FormSkipReportSettingEdit()
        {
            InitializeComponent();

            FormMode = UniOPI.FormMode.AddNew;
            lblCaption.Text = "Skip Report Setting Edit";
            Size = new Size(740, 600);

            pnlAdd.Visible = true;
            dgvAddList.Visible = true;
        }

        public FormSkipReportSettingEdit(SBRM_SKIPREPORT objData)
            : this()
        {
            if (objData == null)
            {
                FormMode = UniOPI.FormMode.AddNew;

                pnlAdd.Visible = true;
                dgvAddList.Visible = true;

                Size = new Size(740, 600);
            }
            else
            {
                FormMode = UniOPI.FormMode.Modify;

                ObjEditData = objData;

                pnlAdd.Visible = false;
                dgvAddList.Visible = false;

                Size = new Size(740, 330);
            }
        }

        private void Form_Load(object sender, EventArgs e)
        {
            try
            {
                this.InitialCombox();

                if (FormMode == UniOPI.FormMode.Modify )
                {
                    cmbNode.Enabled = false;
                    cmbUnit.Enabled = false;

                    if (ObjEditData != null)
                    {
                        cmbNode.SelectedValue = ObjEditData.NODEID;
                        cmbUnit.SelectedValue = ObjEditData.UNITNO;
                        cmbSkipAgent.SelectedValue = ObjEditData.SKIPAGENT;
                        cmbSkipReportTrx.SelectedValue = ObjEditData.SKIPREPORTTRX;
                        txtSkipCondition.Text = ObjEditData.SKIPCONDITION;
                    }
                }
                else
                {
                    this.ActiveControl = cmbNode;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        #region Event
        private void cmbNode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbNode.SelectedIndex == -1) return;

                string strNodeID = ((dynamic)cmbNode.SelectedItem).NodeID;
                SetUnit(strNodeID);
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
                        cmbNode.SelectedValue = _row.Cells[colLocalID.Name].Value.ToString();
                        cmbUnit.SelectedValue = _row.Cells[colUnitNo.Name].Value.ToString();
                        cmbSkipAgent.SelectedValue = _row.Cells[colSkipAgent.Name].Value.ToString();
                        cmbSkipReportTrx.SelectedValue = _row.Cells[colSkipReportTrx.Name].Value.ToString();
                        txtSkipCondition.Text = _row.Cells[colSkipCondition.Name].Value.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Method
        private void InitialCombox()
        {
            SetNode();
            SetSkipAgent();
            SetSkipReportTrx();
        }

        private bool CheckData()
        {
            try
            {
                if (cmbNode.SelectedIndex < 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Local ID required！", MessageBoxIcon.Error);
                    return false;
                }

                if (this.cmbUnit.SelectedIndex < 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Unit ID required！", MessageBoxIcon.Error);
                    return false;
                }

                if (this.cmbSkipAgent.SelectedIndex < 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Skip Agent required！", MessageBoxIcon.Error);
                    return false;
                }

                if (this.cmbSkipReportTrx.SelectedIndex < 0)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Skip Report Trx required！", MessageBoxIcon.Error);
                    return false;
                }


                //PRODUCTINOUTTOTAL just for OEE
                if (cmbSkipReportTrx.SelectedValue.ToString() == "PRODUCTINOUTTOTAL" && cmbSkipAgent.SelectedValue.ToString() != "OEE")
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Production out total just for OEE！", MessageBoxIcon.Error);
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

        private bool CheckUniqueKeyOK(string LineID, string NodeNo, string UnitNo)
        {
            try
            {
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                //資料庫資料 20150310  Tom SkipReport 不要检查UK 
                //int rowCnt = (from msg in ctxBRM.SBRM_SKIPREPORT
                //              where msg.LINEID == LineID && msg.NODENO == NodeNo && msg.UNITNO == UnitNo
                //              select msg).Count();
                //if (rowCnt > 0)
                //{
                //    string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is ({0}, {1}, {2})！", LineID, NodeNo, UnitNo);
                //    ShowMessage(this, this.lblCaption.Text, "", errMsg, MessageBoxIcon.Warning);
                //    return false;
                //}

                //已修改未更新物件
                if (FormMode == UniOPI.FormMode.AddNew)
                {
                    var add = _ctxBRM.GetChangeSet().Inserts.OfType<SBRM_SKIPREPORT>().Where(
                        msg => msg.LINEID == LineID && msg.NODENO == NodeNo && msg.UNITNO == UnitNo);
                    if (add.Count() > 0)
                    {
                        string errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is ({0}, {1}, {2})！", LineID, NodeNo, UnitNo);
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
            this.cmbNode.SelectedIndex = -1;
            this.cmbUnit.SelectedIndex = -1;
            this.cmbSkipAgent.SelectedIndex = -1;
            this.cmbSkipReportTrx.SelectedIndex = -1;
            this.txtSkipCondition.Clear();

            this.ActiveControl = cmbNode;
        }

        private void SetNode()
        {
            try
            {
                cmbNode.SelectedIndexChanged -= new EventHandler(cmbNode_SelectedIndexChanged);

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

                cmbNode.SelectedIndexChanged += new EventHandler(cmbNode_SelectedIndexChanged);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetUnit(string NodeID)
        {
            try
            {
                var q = (from unit in FormMainMDI.G_OPIAp.Dic_Unit.Values
                         where unit.NodeID == NodeID
                         select new
                         {
                             unit.UnitID,
                             unit.UnitNo,
                             IDNAME = string.Format("{0}-{1}", unit.UnitNo, unit.UnitID)
                         }
                    ).ToList();

                q.Insert(0, new
                {
                    UnitID = "",
                    UnitNo = "0",
                    IDNAME = ""
                });

                cmbUnit.DataSource = q;
                cmbUnit.DisplayMember = "IDNAME";
                cmbUnit.ValueMember = "UNITNO";
                cmbUnit.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetSkipReportTrx()
        {
            try
            {
                Dictionary<string, string> dicSkipReportTrx = new Dictionary<string, string>();
                dicSkipReportTrx.Add("PRODUCTIN", "Product In");
                dicSkipReportTrx.Add("PRODUCTOUT", "Product Out");
                dicSkipReportTrx.Add("ALARMREPORT", "Alarm Report");
                dicSkipReportTrx.Add("PRODUCTINOUTTOTAL", "Production out Total");
                cmbSkipReportTrx.DataSource = new BindingSource(dicSkipReportTrx, "");
                cmbSkipReportTrx.DisplayMember = "Value";
                cmbSkipReportTrx.ValueMember = "Key";
                cmbSkipReportTrx.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetSkipAgent()
        {
            var data = new[]             
            { 
                new { Key = "OEE", Value = "OEE" } ,
                new { Key = "MES", Value = "MES" } ,
                new { Key = "EDA", Value = "EDA" }
            };
            this.cmbSkipAgent.DataSource = data;
            this.cmbSkipAgent.DisplayMember = "Value";
            this.cmbSkipAgent.ValueMember = "Key";
            this.cmbSkipAgent.SelectedIndex = -1;

        }
        #endregion

        private void btn_Click(object sender, EventArgs e)
        {
            try
            {
                Button _btn = (Button)sender;

                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;                

                string _lineID;
                string _nodeID;
                string _nodeNo;
                string _unitID;
                string _unitNo;
                string _skipAgent;
                string _skipReportTrx;
                string _skipCondition;

                switch (_btn.Tag.ToString())
                {
                    case "ADD":
                        #region Add 
                        #region Add to DataGridView

                        if (CheckData()==false) return;

                        _lineID = FormMainMDI.G_OPIAp.CurLine.LineID;
                        _nodeID = ((dynamic)cmbNode.SelectedItem).NodeID;
                        _nodeNo = ((dynamic)cmbNode.SelectedItem).NodeNo;
                        _unitID = ((dynamic)cmbUnit.SelectedItem).UnitID;
                        _unitNo = ((dynamic)cmbUnit.SelectedItem).UnitNo;
                        _skipAgent = cmbSkipAgent.SelectedValue.ToString();
                        _skipReportTrx = cmbSkipReportTrx.SelectedValue.ToString();
                        _skipCondition = txtSkipCondition.Text.Trim();

        
                        #region 判斷是否存下gridview內

                        dgvAddList.Rows.Add(_nodeID, _nodeNo,_unitNo,_unitID, _skipAgent, _skipReportTrx, _skipCondition);

                        #endregion

                        #endregion

                        #region clear data

                        cmbNode.SelectedIndex = -1;
                        cmbUnit.SelectedIndex = -1;
                        cmbSkipAgent.SelectedIndex = -1;
                        cmbSkipReportTrx.SelectedIndex = -1;
                        txtSkipCondition.Text = string.Empty;

                        #endregion

                        break;
                        #endregion
                    case "OK":
                        #region OK
                        switch (FormMode)
                        {
                            case FormMode.AddNew:

                                #region Add

                                    #region Local
                                    SBRM_SKIPREPORT _objSkipReport = null;

                                    foreach (DataGridViewRow _row in dgvAddList.Rows)
                                    {
                                        _objSkipReport = new SBRM_SKIPREPORT();

                                        _objSkipReport.LINEID = FormMainMDI.G_OPIAp.CurLine.LineID;
                                        _objSkipReport.NODEID = _row.Cells[colLocalID.Name].Value.ToString();
                                        _objSkipReport.NODENO = _row.Cells[colLocalNo.Name].Value.ToString();
                                        _objSkipReport.UNITNO = _row.Cells[colUnitNo.Name].Value.ToString();
                                        _objSkipReport.UNITID = _row.Cells[colUnitID.Name].Value.ToString();
                                        _objSkipReport.SKIPAGENT = _row.Cells[colSkipAgent.Name].Value.ToString();
                                        _objSkipReport.SKIPREPORTTRX = _row.Cells[colSkipReportTrx.Name].Value.ToString();
                                        _objSkipReport.SKIPCONDITION = _row.Cells[colSkipCondition.Name].Value.ToString();

                                        _ctxBRM.SBRM_SKIPREPORT.InsertOnSubmit(_objSkipReport);
                                    }
                                    #endregion
  
                                break;
                                #endregion

                            case FormMode.Modify:

                                #region Modify

                                _skipAgent = cmbSkipAgent.SelectedValue.ToString();
                                _skipReportTrx = cmbSkipReportTrx.SelectedValue.ToString();
                                _skipCondition = txtSkipCondition.Text.Trim();

                                ObjEditData.SKIPAGENT = _skipAgent;
                                ObjEditData.SKIPREPORTTRX = _skipReportTrx;
                                ObjEditData.SKIPCONDITION = _skipCondition;

                                //_lineID = FormMainMDI.G_OPIAp.CurLine.LineID;
                                //_nodeID = ((dynamic)cmbNode.SelectedItem).NODEID;
                                //_nodeNo = ((dynamic)cmbNode.SelectedItem).NODENO;
                                //_unitID = ((dynamic)cmbUnit.SelectedItem).UNITID;
                                //_unitNo = ((dynamic)cmbUnit.SelectedItem).UNITNO;
                                //_skipAgent = cmbSkipAgent.SelectedValue.ToString();
                                //_skipReportTrx = cmbSkipReportTrx.SelectedValue.ToString();
                                //_skipCondition = txtSkipCondition.Text.Trim();

                                //if (ObjSkipReportToEdit.OBJECTKEY == 0)
                                //{
                                //    // 修改的如果是尚未Submit的資料(已存在ChangeSet().Inserts)，直接取出物件來編輯
                                //    _objSkipReport = (from skiprpt in _ctxBRM.GetChangeSet().Inserts.OfType<SBRM_SKIPREPORT>()
                                //                      where skiprpt.LINEID == _lineID && skiprpt.NODENO == _nodeNo &&
                                //                     skiprpt.UNITNO == _unitNo
                                //                     select skiprpt).FirstOrDefault();
                                //}
                                //else
                                //{
                                //    _objSkipReport = ObjSkipReportToEdit;
                                //}

                                //if (_objSkipReport == null) return;
                                //_objSkipReport.SKIPAGENT = _skipAgent;
                                //_objSkipReport.SKIPREPORTTRX = _skipReportTrx;
                                //_objSkipReport.SKIPCONDITION = _skipCondition;

                                break;
                                #endregion

                            default:
                                break;
                        }

                        this.DialogResult = System.Windows.Forms.DialogResult.OK;

                        this.Close();
                       
                        break;
                        #endregion

                    case "Cancel":
                        #region Cancel
                        this.DialogResult = System.Windows.Forms.DialogResult.Cancel;

                        this.Close();

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
    }
}
