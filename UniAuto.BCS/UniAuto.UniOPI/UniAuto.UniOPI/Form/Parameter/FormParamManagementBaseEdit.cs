using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormParamManagementBaseEdit : FormBase
    {
        #region Protected Fields
        protected string LineID = string.Empty;
        protected string LineType = string.Empty;
        protected string ServerName = string.Empty;
        protected string NodeNo = string.Empty;
        protected string ReportUnitNo = string.Empty;        
        protected string SvID = string.Empty;
        protected string ParameterName = string.Empty;
        protected string Item = string.Empty;
        protected string SiteNo = string.Empty;        
        protected string Unit = string.Empty;
        protected string Range = string.Empty;
        protected string Operator = string.Empty;
        protected string DotRatio = string.Empty;
        protected string ReportTo = string.Empty;
        protected string Expression = string.Empty;
        protected string Woffset = string.Empty;
        protected string WPoints = string.Empty;
        protected string BOffset = string.Empty;
        protected string BPoints = string.Empty;                
        protected string Description = string.Empty;
        protected string JobDataItem = string.Empty;
        #endregion

        ParamForm CurParamForm;

        #region Constructor
        public FormParamManagementBaseEdit()
        {
            InitializeComponent();
        }

        public FormParamManagementBaseEdit(ParamForm formType)
            : this()
        {
            switch (formType)
            {
                case ParamForm.APCDownlaodManagement:
                    this.lblCaption.Text = "APC Download Edit";
                    break;
                case ParamForm.APCReportManagement:
                    this.lblCaption.Text = "APC Report Edit";
                    break;
                case ParamForm.DailyCheckManagement:
                    this.lblCaption.Text = "Daily Check Edit";
                    break;
                case ParamForm.EnergyVisualizationManagement:
                    this.lblCaption.Text = "Energy Visualization Edit";
                    break;
                case ParamForm.ProcessDataManagement:
                    this.lblCaption.Text = "Process Data Edit";
                    break;
                case ParamForm.RecipeParameter:
                    this.lblCaption.Text = "Recipe Parameter Management";
                    break;
                default:
                    break;
            }

            CurParamForm = formType;

            ServerName = FormMainMDI.G_OPIAp.CurLine.ServerName;
            LineType = FormMainMDI.G_OPIAp.CurLine.LineType;

            InitialCombox();

            txtReportTo.Click += new EventHandler(txtReportTo_Click);
        }
        #endregion

        #region Events

        private void btnOKClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void txtReportTo_Click(object sender, EventArgs e)
        {
            try
            {
                TextBox txt = sender as TextBox;

                FormParamManagementEdit_SelectReport frmReportTo = new FormParamManagementEdit_SelectReport();
                frmReportTo.StartPosition = FormStartPosition.CenterScreen;

                frmReportTo.SelectedValue = txt.Text.Trim();

                if (frmReportTo.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    txt.Text = frmReportTo.SelectedValue;
                }

                frmReportTo.Dispose();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        protected virtual void btnADD_Click(object sender, EventArgs e)
        {
            try
            {
                #region Add to DataGridView

                if (CheckData() == false) return;

                if (AssignInputData() == false) return;

                if (CheckUniqueKeyOK(LineType, LineID, NodeNo, ParameterName) == false) return;

                #region 判斷是否存下gridview內
                if (dgvAddList.Rows.Cast<DataGridViewRow>().Where(r =>
                    r.Cells[colLocalNo.Name].Value.ToString().Equals(NodeNo) &&
                    r.Cells[colParameterName.Name].Value.ToString().Equals(ParameterName) 
                    ).Count() > 0)
                {
                    DataGridViewRow _addRow = dgvAddList.Rows.Cast<DataGridViewRow>().Where(r =>
                    r.Cells[colLocalNo.Name].Value.ToString().Equals(NodeNo) &&
                    r.Cells[colParameterName.Name].Value.ToString().Equals(ParameterName) 
                        ).First();

                    string _msg = string.Format("Local No[{0}],Parameter Name[{1}] is already insert. Please confirm whether you will overwite data ?", NodeNo, ParameterName);

                    if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return;

                    _addRow.Cells[colUnitNo.Name].Value = ReportUnitNo;
                    _addRow.Cells[colSVID.Name].Value = SvID;
                    _addRow.Cells[colItem.Name].Value = Item;
                    _addRow.Cells[colSite.Name].Value = SiteNo;
                    _addRow.Cells[colUnit.Name].Value = Unit;
                    _addRow.Cells[colRange.Name].Value = Range;
                    _addRow.Cells[colOperator.Name].Value = Operator;
                    _addRow.Cells[colDotRatio.Name].Value = DotRatio;
                    _addRow.Cells[colReportTo.Name].Value = ReportTo;
                    _addRow.Cells[colExpression.Name].Value = Expression;
                    _addRow.Cells[colWoffset.Name].Value = Woffset;
                    _addRow.Cells[colWPoints.Name].Value = WPoints;
                    _addRow.Cells[colBitOffset.Name].Value = BOffset;
                    _addRow.Cells[colBPoints.Name].Value = BPoints;
                    _addRow.Cells[colDescription.Name].Value = Description;
                    _addRow.Cells[colJobDataItem.Name].Value = JobDataItem;
                }
                else
                {
                    dgvAddList.Rows.Add(LineID, NodeNo, ReportUnitNo, SvID, ParameterName, Item, SiteNo, Unit, Range, Operator, DotRatio, ReportTo, Expression,
                    Woffset,WPoints, BOffset, BPoints, Description, JobDataItem);
                }

                #endregion

                #endregion

                #region clear data

                ClearData();

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        protected virtual void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        protected virtual void btnClear_Click(object sender, EventArgs e)
        {
            ClearData();
        }

        protected virtual void btnClose_Click(object sender, EventArgs e)
        {
            try
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
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
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow _row = dgvAddList.SelectedRows[0];

                    if (_row != null)
                    {
                        cmbNode.SelectedValue = _row.Cells[colLocalNo.Name].Value.ToString();
                        cmbUnit.SelectedValue = _row.Cells[colUnitNo.Name].Value.ToString();

                        txtSvID.Text = _row.Cells[colSVID.Name].Value.ToString();
                        txtParameterName.Text = _row.Cells[colParameterName.Name].Value.ToString();
                        txtItem.Text = _row.Cells[colItem.Name].Value.ToString();
                        txtSite.Text = _row.Cells[colSite.Name].Value.ToString();
                        txtUnit.Text = _row.Cells[colUnit.Name].Value.ToString();
                        txtRange.Text = _row.Cells[colRange.Name].Value.ToString();

                        cmbOperator.SelectedValue = _row.Cells[colOperator.Name].Value.ToString();
                        txtDotRatioOperand.Text = _row.Cells[colDotRatio.Name].Value.ToString();
                        txtReportTo.Text = _row.Cells[colReportTo.Name].Value.ToString();
                        cmbExpression.SelectedValue = _row.Cells[colExpression.Name].Value.ToString();
                        txtWordOffsetPos.Text = _row.Cells[colWoffset.Name].Value.ToString();
                        txtWordLength.Text = _row.Cells[colWPoints.Name].Value.ToString();
                        txtBitOffsetPos.Text = _row.Cells[colBitOffset.Name].Value.ToString();
                        txtBitLength.Text = _row.Cells[colBPoints.Name].Value.ToString();
                        txtDescription.Text = _row.Cells[colDescription.Name].Value.ToString();
                        txtJobData.Text = _row.Cells[colJobDataItem.Name].Value.ToString();
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

        #endregion

        #region Protected Methods

        protected virtual void InitialCombox()
        {
            cmbNode.SelectedIndexChanged -= new EventHandler(cmbNode_SelectedIndexChanged);
            
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

            cmbNode.DataSource = q;
            cmbNode.DisplayMember = "IDNAME";
            cmbNode.ValueMember = "NODENO";
            cmbNode.SelectedIndex = -1;
            #endregion

            cmbNode.SelectedIndexChanged += new EventHandler(cmbNode_SelectedIndexChanged);
        }

        protected virtual bool CheckData()
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

                if ("".Equals(txtParameterName.Text.Trim()))
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Parameter Name required！", MessageBoxIcon.Warning);
                    txtParameterName.Focus();
                    return false;
                }

                if (!"".Equals(txtRange.Text.Trim()))
                {
                    int intRange = -1;
                    bool bolCheck = Int32.TryParse(txtRange.Text.Trim(), out intRange);
                    if (bolCheck == false || (intRange < 0 || intRange > 65535))
                    {
                        ShowMessage(this, this.lblCaption.Text, "", "Range must between 0 and 65535！", MessageBoxIcon.Warning);
                        txtRange.Focus();
                        txtRange.SelectAll();
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

        protected virtual bool AssignInputData()
        {
            try
            {
                var _objNode = ((dynamic)cmbNode.SelectedItem);

                LineID =_objNode.LineID;
                NodeNo = _objNode.NodeNo;
                ReportUnitNo = ((dynamic)cmbUnit.SelectedItem).UnitNo;
                SvID = txtSvID.Text.Trim();
                ParameterName = txtParameterName.Text.Trim();
                Item = txtItem.Text.Trim();
                SiteNo = txtSite.Text.Trim();                
                Unit = txtUnit.Text.Trim();                               
                Range = txtRange.Text.Trim();
                Operator = (cmbOperator.SelectedItem == null) ? string.Empty : cmbOperator.SelectedItem.ToString();
                DotRatio = txtDotRatioOperand.Text.Trim();
                ReportTo = txtReportTo.Text.Trim();
                Expression = (cmbExpression.SelectedItem == null) ? string.Empty : cmbExpression.SelectedItem.ToString();
                Woffset = txtWordOffsetPos.Text.Trim();
                WPoints = txtWordLength.Text.Trim();
                BOffset = txtBitOffsetPos.Text.Trim();
                BPoints = txtBitLength.Text.Trim();
                Description = txtDescription.Text.Trim();
                JobDataItem= txtJobData.Text.Trim();

                return true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                return false;
            }
        }

        protected virtual bool CheckUniqueKeyOK(string lineType, string lineID, string nodeNo, string parameterName)
        {
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                int rowCnt = 0;
                string _err = string.Empty;

                switch (CurParamForm)
                {
                    case ParamForm.APCDownlaodManagement:
                        //資料庫資料
                        rowCnt = (from msg in ctxBRM.SBRM_APCDATADOWNLOAD
                                  where msg.LINEID == lineID && msg.LINETYPE == lineType &&
                                  msg.NODENO == nodeNo && msg.PARAMETERNAME == parameterName
                                  select msg).Count();

                        break;

                    case ParamForm.APCReportManagement:
                        //資料庫資料
                        rowCnt = (from msg in ctxBRM.SBRM_APCDATAREPORT
                                  where msg.LINEID == lineID && msg.LINETYPE == lineType &&
                                  msg.NODENO == nodeNo && msg.PARAMETERNAME == parameterName
                                  select msg).Count();

                        break;

                    case ParamForm.DailyCheckManagement:
                        //資料庫資料
                        rowCnt = (from msg in ctxBRM.SBRM_DAILYCHECKDATA
                                  where msg.LINEID == lineID && msg.LINETYPE == lineType &&
                                  msg.NODENO == nodeNo && msg.PARAMETERNAME == parameterName
                                  select msg).Count();

                        break;

                    case ParamForm.EnergyVisualizationManagement:
                        //資料庫資料
                        rowCnt = (from msg in ctxBRM.SBRM_ENERGYVISUALIZATIONDATA
                                  where msg.LINEID == lineID && msg.LINETYPE == lineType &&
                                  msg.NODENO == nodeNo && msg.PARAMETERNAME == parameterName
                                  select msg).Count();

                        break;

                    case ParamForm.ProcessDataManagement:
                        //資料庫資料
                        rowCnt = (from msg in ctxBRM.SBRM_PROCESSDATA
                                  where msg.LINEID == lineID && msg.LINETYPE == lineType &&
                                  msg.NODENO == nodeNo && msg.PARAMETERNAME == parameterName
                                  select msg).Count();

                        break;

                    case ParamForm.RecipeParameter:
                        //資料庫資料
                        rowCnt = (from msg in ctxBRM.SBRM_RECIPEPARAMETER
                                  where msg.LINEID == lineID && msg.LINETYPE == lineType &&
                                  msg.NODENO == nodeNo && msg.PARAMETERNAME == parameterName
                                  select msg).Count();

                        break;
                    default :
                        break;
                }

                if (rowCnt > 0)
                {
                    _err = string.Format("UNIQUE KEY constraint violated. Duplicate key is Line Type [{0}], Line ID [{1}], Node No [{2}], Parameter Name [{3}]！", lineType, lineID, nodeNo, parameterName);
                    ShowMessage(this, this.lblCaption.Text, "", _err, MessageBoxIcon.Warning);
                    txtParameterName.Focus();
                    txtParameterName.SelectAll();
                    return false;
                }

                var add = ctxBRM.GetChangeSet().Inserts.OfType<SBRM_APCDATADOWNLOAD>().Where(
                    msg => msg.LINEID == lineID && msg.LINETYPE == lineType &&
                        msg.NODENO == nodeNo && msg.PARAMETERNAME == parameterName);

                if (add.Count() > 0)
                {
                    _err = string.Format("UNIQUE KEY constraint violated. Duplicate key is Line Type [{0}], Line ID [{1}], Node No [{2}], Parameter Name [{3}]！", lineType, lineID, nodeNo, parameterName);
                    ShowMessage(this, this.lblCaption.Text, "", _err, MessageBoxIcon.Warning);
                    txtParameterName.Focus();
                    txtParameterName.SelectAll();
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

        protected virtual void ClearData()
        {
            cmbNode.SelectedIndex = -1;
            cmbUnit.SelectedIndex = -1;
            txtSvID.Clear();
            txtParameterName.Clear();
            txtItem.Clear();
            txtSite.Clear();
            txtDescription.Clear();
            txtUnit.Clear();
            cmbOperator.SelectedIndex = -1;
            cmbExpression.SelectedIndex = -1;
            txtRange.Clear();
            txtWordOffsetPos.Clear();
            txtWordLength.Clear();
            txtBitOffsetPos.Clear();
            txtBitLength.Clear();
            txtDotRatioOperand.Clear();
            txtReportTo.Clear();
            this.ActiveControl = cmbNode;
        }

        #endregion
    }
}
