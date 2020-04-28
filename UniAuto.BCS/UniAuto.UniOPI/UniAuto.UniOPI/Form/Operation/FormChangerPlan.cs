using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormChangerPlan : FormBase
    {
        public FormChangerPlan()
        {
            InitializeComponent();
        }

        private void GetComboxPlanInfo(string plan = "")
        {
            try
            {
                List<comboxInfo> lstPlan = new List<comboxInfo>();
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                List<string> lstdbPlans = (from p in ctxBRM.SBRM_CHANGEPLAN
                                           where p.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName)
                                           select p.PLANID).Distinct().ToList();
                List<string> lstAddPlans = (from p in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_CHANGEPLAN>()
                                            where p.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName)
                                            select p.PLANID).Distinct().ToList();
                foreach (string dbPlan in lstdbPlans)
                    lstPlan.Add(new comboxInfo() { ITEM_ID = dbPlan, ITEM_NAME = dbPlan });
                foreach (string addPlan in lstAddPlans)
                {
                    if (lstPlan.Find(d => d.ITEM_ID.Equals(addPlan)) != null) continue;
                    lstPlan.Add(new comboxInfo() { ITEM_ID = addPlan, ITEM_NAME = addPlan });
                }
                lstPlan.Sort((d1, d2) => d1.ITEM_ID.CompareTo(d2.ITEM_ID));
                this.cmbPlanID.SelectedIndexChanged -= new System.EventHandler(this.cmbPlanID_SelectedIndexChanged);
                cmbPlanID.DataSource = lstPlan.ToList();
                cmbPlanID.DisplayMember = "ITEM_ID";
                cmbPlanID.ValueMember = "ITEM_ID";
                cmbPlanID.SelectedIndex = -1;
                this.cmbPlanID.SelectedIndexChanged += new System.EventHandler(this.cmbPlanID_SelectedIndexChanged);

                if (!string.IsNullOrWhiteSpace(plan))
                {
                    if (lstPlan.Exists(d => d.ITEM_ID.Equals(plan)))
                    {
                        cmbPlanID.SelectedValue = plan;
                    }
                    else
                    { GetChangePlan(""); }
                }
                else
                { GetChangePlan(""); }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void GetChangePlan(string planID)
        {
            try
            {
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                //資料庫資料
                var selData = from d in ctxBRM.SBRM_CHANGEPLAN
                              where d.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) && d.PLANID.Equals(planID)
                              select d;

                //已修改未更新物件
                var addData = from d in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_CHANGEPLAN>()
                              where d.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) && d.PLANID.Equals(planID)
                              select d;


                List<SBRM_CHANGEPLAN> objTables = selData.ToList();
                objTables.AddRange(addData.ToList());

                objTables.Sort((d1, d2) => d1.SOURCECASSETTEID.CompareTo(d2.SOURCECASSETTEID) != 0 ? d1.SOURCECASSETTEID.CompareTo(d2.SOURCECASSETTEID) : d1.SLOTNO.CompareTo(d2.SLOTNO));

                dgvData.AutoGenerateColumns = false;
                dgvData.DataSource = objTables;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void FormChangePlan_Load(object sender, EventArgs e)
        {
            GetComboxPlanInfo();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void btnAddPlan_Click(object sender, EventArgs e)
        {
            try
            {
                FormChangerPlanEdit frmAdd = new FormChangerPlanEdit();

                if (DialogResult.OK == frmAdd.ShowDialog())
                {
                    GetComboxPlanInfo(frmAdd.PlanID);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnModifyPlan_Click(object sender, EventArgs e)
        {
            try
            {
                if (cmbPlanID.SelectedIndex == -1)
                {
                    ShowMessage(this, lblCaption.Text , "", "Please choose Plan ID!!", MessageBoxIcon.Warning);
                    cmbPlanID.Focus();
                    return;
                }

                string _planID = cmbPlanID.SelectedValue.ToString();

                FormChangerPlanEdit frmModify = new FormChangerPlanEdit(cmbPlanID.SelectedValue.ToString());
                frmModify.StartPosition = FormStartPosition.CenterScreen;

                if (DialogResult.OK == frmModify.ShowDialog())
                {
                    GetComboxPlanInfo(frmModify.PlanID);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnDeletePlan_Click(object sender, EventArgs e)
        {
            if (cmbPlanID.SelectedIndex == -1)
            {
                ShowMessage(this, lblCaption.Text , "", "Please choose the plan id  you want to delete!!", MessageBoxIcon.Warning);
                cmbPlanID.Focus();
                return;
            }

            string _planID = cmbPlanID.SelectedValue.ToString();


            if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, "Are you sure to delete the plan id ?")) return;

            UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
            try
            {
                //string planID = cmbPlanID.SelectedValue.ToString();
                var objSelDelete = from d in ctxBRM.SBRM_CHANGEPLAN
                                   where d.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) && d.PLANID.Equals(_planID)
                                   select d;
                if (objSelDelete.Count() > 0)
                {
                    foreach (SBRM_CHANGEPLAN obj in objSelDelete)
                        ctxBRM.SBRM_CHANGEPLAN.DeleteOnSubmit(obj);
                }

                var objAddDelete = from d in ctxBRM.GetChangeSet().Inserts.OfType<SBRM_CHANGEPLAN>()
                                   where d.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) && d.PLANID.Equals(_planID)
                                   select d;
                if (objAddDelete.Count() > 0)
                {
                    foreach (SBRM_CHANGEPLAN obj in objAddDelete)
                        ctxBRM.SBRM_CHANGEPLAN.DeleteOnSubmit(obj);
                }
                GetComboxPlanInfo(_planID);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private bool ChkPlanID(string PlanID)
        {
            //取得Line Mode是否為3:changerMode且ChangePlanid是否和現在處理的Plan一樣，若是的話即為受限制的PlanID
            if (FormMainMDI.G_OPIAp.CurLine.IndexerMode== 3)
            {
                if (FormMainMDI.G_OPIAp.CurLine.ChangerPlanID == null ) return true;
                if (FormMainMDI.G_OPIAp.CurLine.ChangerPlanID.ToString().Equals(PlanID)) return true;
                else  return false;
            }

            return false;

        }

        private void dgvData_DataSourceChanged(object sender, EventArgs e)
        {
            try
            {
                if (dgvData.DataSource == null || dgvData.Rows.Count == 0) return;

                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                foreach (DataGridViewRow dr in dgvData.Rows)
                {
                    SBRM_CHANGEPLAN obj = dr.DataBoundItem as SBRM_CHANGEPLAN;

                    if (ctxBRM.GetChangeSet().Inserts.OfType<SBRM_CHANGEPLAN>().Any(
                        d => d.SERVERNAME.Equals(obj.SERVERNAME) && d.PLANID.Equals(obj.PLANID) && d.SOURCECASSETTEID.Equals(obj.SOURCECASSETTEID) && d.SLOTNO.Equals(obj.SLOTNO)))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(214, 233, 177);
                    }
                    else if (ctxBRM.GetChangeSet().Deletes.OfType<SBRM_CHANGEPLAN>().Any(
                        d => d.SERVERNAME.Equals(obj.SERVERNAME) && d.PLANID.Equals(obj.PLANID) && d.SOURCECASSETTEID.Equals(obj.SOURCECASSETTEID) && d.SLOTNO.Equals(obj.SLOTNO)))
                    {
                        dr.DefaultCellStyle.BackColor = Color.Red;
                    }
                    else if (ctxBRM.GetChangeSet().Updates.OfType<SBRM_CHANGEPLAN>().Any(
                        d => d.SERVERNAME.Equals(obj.SERVERNAME) && d.PLANID.Equals(obj.PLANID) && d.SOURCECASSETTEID.Equals(obj.SOURCECASSETTEID) && d.SLOTNO.Equals(obj.SLOTNO)))
                    {
                        dr.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 195);
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                CheckChangeSave();
                //ClearModify();

                //string planID = string.Empty;
                //if (cmbPlanID.SelectedIndex != -1) planID = cmbPlanID.SelectedValue.ToString();

                //GetComboxPlanInfo(planID);

                //GetChangePlan(planID);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cmbPlanID_SelectedIndexChanged(object sender, EventArgs e)
        {
            string planID = string.Empty;
            if (cmbPlanID.SelectedIndex != -1) planID = cmbPlanID.SelectedValue.ToString();
            GetChangePlan(planID);
        }

        private void Save()
        {
            UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
            string planID = string.Empty;
            if (cmbPlanID.SelectedIndex != -1) planID = cmbPlanID.SelectedValue.ToString();
            try
            {
                ctxBRM.SubmitChanges();
                ShowMessage(this, lblCaption.Text, "", "Save Success！", MessageBoxIcon.Information);

                GetComboxPlanInfo(planID);

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
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

        public void RefreshData()
        {
            try
            {
                FormMainMDI.G_OPIAp.RefreshDBBRMCtx();

                string planID = string.Empty;

                if (cmbPlanID.SelectedIndex != -1) planID = cmbPlanID.SelectedValue.ToString();

                GetComboxPlanInfo(planID);

                GetChangePlan(planID);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
    }
}
