using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormEnergyVisualizationManagementEdit : FormParamManagementBaseEdit
    {
        private SBRM_ENERGYVISUALIZATIONDATA ObjEdit;

        public FormEnergyVisualizationManagementEdit()
            : base(ParamForm.EnergyVisualizationManagement)
        {
            InitializeComponent();
        }

        public FormEnergyVisualizationManagementEdit(SBRM_ENERGYVISUALIZATIONDATA objData)
            : this()
        {
            if (objData == null)
            {
                FormMode = UniOPI.FormMode.AddNew;

                dgvAddList.Visible = true;
                pnlAdd.Visible = true;

                Size = new Size(830, 750);
            }
            else
            {
                FormMode = UniOPI.FormMode.Modify;

                ObjEdit = objData;

                dgvAddList.Visible = false;
                pnlAdd.Visible = false;

                Size = new Size(830, 440);
            }
        }

        private void FormEnergyVisualizationManagementEdit_Load(object sender, EventArgs e)
        {
            try
            {
                if (FormMode == UniOPI.FormMode.Modify)
                {
                    cmbNode.Enabled = false;
                    cmbUnit.Enabled = false;
                    txtSvID.ReadOnly = true;
                    txtParameterName.ReadOnly = true;

                    if (ObjEdit != null)
                    {
                        cmbNode.SelectedValue = ObjEdit.NODENO;
                        txtSvID.Text = ObjEdit.SVID;
                        txtParameterName.Text = ObjEdit.PARAMETERNAME;
                        txtItem.Text = ObjEdit.ITEM;
                        txtSite.Text = ObjEdit.SITE;
                        txtDescription.Text = ObjEdit.DESCRIPTION;
                        txtUnit.Text = ObjEdit.UNIT;
                        cmbOperator.SelectedItem = ObjEdit.OPERATOR;
                        cmbExpression.SelectedItem = ObjEdit.EXPRESSION;
                        txtRange.Text = ObjEdit.RANGE;
                        txtWordOffsetPos.Text = ObjEdit.WOFFSET;
                        txtWordLength.Text = ObjEdit.WPOINTS;
                        txtBitOffsetPos.Text = ObjEdit.BOFFSET;
                        txtBitLength.Text = ObjEdit.BPOINTS;
                        txtDotRatioOperand.Text = ObjEdit.DOTRATIO;
                        txtReportTo.Text = ObjEdit.REPORTTO;
                        txtJobData.Text = ObjEdit.JOBDATAITEMNAME;

                    }

                    this.ActiveControl = txtItem;
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

        protected override void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                int _num = 0;
                switch (FormMode)
                {
                    case FormMode.AddNew:

                        #region Add

                        SBRM_ENERGYVISUALIZATIONDATA _objData = null;

                        foreach (DataGridViewRow _row in dgvAddList.Rows)
                        {
                            _objData = new SBRM_ENERGYVISUALIZATIONDATA();

                            _objData.LINETYPE = LineType;
                            _objData.SERVERNAME = ServerName;
                            _objData.LINEID = _row.Cells["colLineID"].Value.ToString();
                            _objData.NODENO = _row.Cells["colLocalNo"].Value.ToString();
                            _objData.SVID = _row.Cells["colSVID"].Value.ToString();
                            _objData.PARAMETERNAME = _row.Cells["colParameterName"].Value.ToString();

                            int.TryParse(_row.Cells["colUnitNo"].Value.ToString(), out _num);
                            _objData.REPORTUNITNO = _num;

                            _objData.ITEM = _row.Cells["colItem"].Value.ToString();
                            _objData.SITE = _row.Cells["colSite"].Value.ToString();
                            _objData.UNIT = _row.Cells["colUnit"].Value.ToString();
                            _objData.RANGE = _row.Cells["colRange"].Value.ToString();
                            _objData.OPERATOR = _row.Cells["colOperator"].Value.ToString();
                            _objData.DOTRATIO = _row.Cells["colDotRatio"].Value.ToString();
                            _objData.REPORTTO = _row.Cells["colReportTo"].Value.ToString();
                            _objData.EXPRESSION = _row.Cells["colExpression"].Value.ToString();
                            _objData.WOFFSET = _row.Cells["colWoffset"].Value.ToString();
                            _objData.WPOINTS = _row.Cells["colWPoints"].Value.ToString();
                            _objData.BOFFSET = _row.Cells["colBitOffset"].Value.ToString();
                            _objData.BPOINTS = _row.Cells["colBPoints"].Value.ToString();
                            _objData.DESCRIPTION = _row.Cells["colDescription"].Value.ToString();
                            _objData.JOBDATAITEMNAME = _row.Cells["colJobDataItem"].Value.ToString();

                            _ctxBRM.SBRM_ENERGYVISUALIZATIONDATA.InsertOnSubmit(_objData);
                        }

                        break;
                        #endregion

                    case FormMode.Modify:

                        #region Modify

                        if (AssignInputData() == false) return;

                        ObjEdit.ITEM = Item;
                        ObjEdit.SITE = SiteNo;
                        ObjEdit.UNIT = Unit;
                        ObjEdit.RANGE = Range;
                        ObjEdit.OPERATOR = Operator;
                        ObjEdit.DOTRATIO = DotRatio;
                        ObjEdit.REPORTTO = ReportTo;
                        ObjEdit.EXPRESSION = Expression;
                        ObjEdit.WOFFSET = Woffset;
                        ObjEdit.WPOINTS = WPoints;
                        ObjEdit.BOFFSET = BOffset;
                        ObjEdit.BPOINTS = BPoints;
                        ObjEdit.DESCRIPTION = Description;
                        ObjEdit.JOBDATAITEMNAME = JobDataItem;

                        break;
                        #endregion

                    default:
                        break;
                }

                base.btnOK_Click(sender, e);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
    }
}