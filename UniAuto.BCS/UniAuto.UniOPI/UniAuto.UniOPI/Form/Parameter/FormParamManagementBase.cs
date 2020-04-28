using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public enum ParamForm
    {
        APCDownlaodManagement,
        APCReportManagement,
        DailyCheckManagement,
        EnergyVisualizationManagement,
        ProcessDataManagement,
        RecipeParameter
    }
    
    public partial class FormParamManagementBase : FormBase
    {
        public string CurNodeNo = string.Empty;

        public FormParamManagementBase()
        {
            InitializeComponent();
        }

        public FormParamManagementBase(ParamForm formType)
            : this()
        {
            switch (formType)
            {
                case ParamForm.APCDownlaodManagement:
                    this.lblCaption.Text = "APC Download Management";
                    break;
                case ParamForm.APCReportManagement:
                    this.lblCaption.Text = "APC Report Management";
                    break;
                case ParamForm.DailyCheckManagement:
                    this.lblCaption.Text = "Daily Check Management";
                    break;
                case ParamForm.EnergyVisualizationManagement:
                    this.lblCaption.Text = "Energy Visualization Management";
                    break;
                case ParamForm.ProcessDataManagement:
                    this.lblCaption.Text = "Process Data Management";
                    break;
                case ParamForm.RecipeParameter:
                    this.lblCaption.Text = "Recipe Parameter Management";
                    break;
                default:
                    break;
            }

            InitialCombox();
        }

        protected virtual void cmbNode_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbNode.SelectedIndex < 0 || cmbNode.SelectedValue == null)
                {
                    CurNodeNo = string.Empty;

                    return;
                }

                CurNodeNo = ((dynamic)cmbNode.SelectedItem).NodeNo;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        protected virtual void InitialCombox()
        {
            var q = (from node in FormMainMDI.G_OPIAp.Dic_Node.Values
                     select new
                     {
                         IDNAME = string.Format("{0}-{1}-{2}", node.NodeNo, node.NodeID, node.NodeName),
                         node.NodeNo,
                         node.NodeID
                     }).ToList();

            if (q == null || q.Count == 0)
                return;

            cmbNode.SelectedIndexChanged -= cmbNode_SelectedIndexChanged;
            cmbNode.DataSource = q;
            cmbNode.DisplayMember = "IDNAME";
            cmbNode.ValueMember = "NODENO";
            cmbNode.SelectedIndex = -1;
            cmbNode.SelectedIndexChanged += cmbNode_SelectedIndexChanged;
        }

        protected virtual void GetGridViewData(bool showMessage = false)
        { }

        protected virtual void btnQuery_Click(object sender, EventArgs e)
        {
            try
            {
                CheckChangeSave();
                //ClearModify();

                //if (cmbNode.SelectedValue == null)
                //{
                //    ShowMessage(this, lblCaption.Text, "", "Please Choose Local No！", MessageBoxIcon.Warning);
                //    return;
                //}

                //GetGridViewData(true);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        //private void ClearModify()
        //{
        //    FormMainMDI.G_OPIAp.RefreshDBBRMCtx();
        //}

        protected virtual void btnExport_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvData.Rows.Count <= 0) return;

                string message = string.Empty;

                if (UniTools.ExportToExcel("", dgvData, 0, out message))
                {
                    ShowMessage(this, lblCaption.Text, "", "Export Success！", MessageBoxIcon.Information);
                }
                else
                {
                    if (!"".Equals(message))
                    {
                        ShowMessage(this, lblCaption.Text, "", message, MessageBoxIcon.Error);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        protected virtual void Save()
        {

        }

        public override void CheckChangeSave()
        {
            try
            {
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                if (_ctxBRM.GetChangeSet().Inserts.Count() > 0 ||
                     _ctxBRM.GetChangeSet().Deletes.Count() > 0 ||
                     _ctxBRM.GetChangeSet().Updates.Count() > 0)
                {
                    string _msg = string.Format("Please confirm whether you will save data before change layout ?");
                    if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg))
                    {
                        RefreshData();
                        return;
                    }
                    else
                    {
                        Save();
                    }
                }
                else
                {
                    RefreshData();
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        public virtual void RefreshData()
        {
            try
            {
                #region Refresh

                FormMainMDI.G_OPIAp.RefreshDBBRMCtx();

                GetGridViewData();

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
    }
}
