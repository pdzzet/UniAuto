using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormCassetteControlSub : FormBase
    {
        public enum HandleType
        {
            Recipe,
            ProductionType,
            JobType
        }
        private HandleType CurHandleType;

        private string CurHandleColName;

        private Dictionary<string, Recipe> dicRecipe_All;

        private DataGridViewRowCollection dgvRows;

        public DataGridViewRowCollection DGVRows
        {
            get { return dgvRows; }
            set { dgvRows = value; }
        }
        
        public FormCassetteControlSub()
        {
            InitializeComponent();
        }


        //Recipe Change
        public FormCassetteControlSub(string handColName, Dictionary<string, Recipe> dicRecipe)
        {
            InitializeComponent();
            CurHandleColName = handColName;
            CurHandleType = HandleType.Recipe;
            dicRecipe_All = dicRecipe;
        }

        private void FormRecipeSelect_Load(object sender, EventArgs e)
        {
            try
            {
                switch (CurHandleType)
                {
                    case HandleType.Recipe:

                        #region  顯示可設定的Recipe
                        dgvRecipe.Rows.Clear();
                        foreach (Recipe _recipe in dicRecipe_All.Values)
                        {
                            dgvRecipe.Rows.Add(_recipe.RecipeName, _recipe.PPID);
                        }

                        gbRecipe.Visible = true;
                        gbRecipe.Height = flpLot.Height;
                        colAssginRecipe.Visible = true;
                        #endregion

                        break;

                    default:

                        break;
                }

                foreach (DataGridViewRow dgvr in dgvRows)
                {
                    dgvCassette.Rows.Add(dgvr.Cells["colProcessFlag"].Value, dgvr.Cells["colSlotNo"].Value, dgvr.Cells["colOwnerID"].Value, dgvr.Cells["colOwnerType"].Value, false, false, false);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            try
            {
                string _colName = string.Empty;
                string _newValue = string.Empty;

                switch (CurHandleType)
                {
                    case HandleType.Recipe:

                        _colName = colAssginRecipe.Name;

                        if (dgvRecipe.Rows.Count > 0) _newValue = dgvRecipe.CurrentRow.Cells[colRecipeName.Name].Value.ToString();

                        break;

                    default:

                        break;
                }

                foreach (DataGridViewRow row in dgvRows)
                {
                    string slotNo = row.Cells["colSlotNo"].Value.ToString();

                    DataGridViewRow curRow = (dgvCassette.Rows.Cast<DataGridViewRow>().Where(d => d.Cells[colSlotNo.Name].Value.ToString().Equals(slotNo))).FirstOrDefault();

                    if (curRow == null) continue;

                    if (curRow.Cells[_colName].Value.Equals(false)) continue;

                    if (FormMainMDI.G_OPIAp.CurLine.MesControlMode.ToUpper() == "LOCAL")
                    {
                        if (FormMainMDI.G_OPIAp.CurLine.LineType != "BFG_SHUZTUNG" && FormMainMDI.G_OPIAp.CurLine.LineType != "PHL_EDGEEXP" && FormMainMDI.G_OPIAp.CurLine.LineType != "PHL_TITLE")
                        {
                            #region 但如果MES download 的是以下产品信息则不能做修改 -- OWNERTYPE: P || OWNERID: RESD
                            if (row.Cells["colOwnerType"].Value.ToString() == "P" || row.Cells["colOwnerID"].Value.ToString() == "RESD")
                            {
                                continue;
                            }
                        }
                        #endregion
                    }

                    row.Cells[CurHandleColName].Value = _newValue;
                }

                this.Close();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvCassette_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            try
            {
                if (!dgvCassette.Columns[e.ColumnIndex].Name.Equals(colAssginRecipe.Name) &&
                    !dgvCassette.Columns[e.ColumnIndex].Name.Equals(colAssignJobType.Name) &&
                    !dgvCassette.Columns[e.ColumnIndex].Name.Equals(colAssignProdType.Name)) return;

                if (dgvCassette.CurrentRow.Cells[colProcessFlag.Name].Value.Equals(false)) e.Cancel = true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void txtSelFrSlotNo_KeyPress(object sender, KeyPressEventArgs e)
        {
            UniTools.CheckTextBoxKeyPressIsInteger(sender, e);
        }

        private void chk_SelectAll_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtSelFrSlotNo.Text.ToString()) && string.IsNullOrWhiteSpace(txtSelToSlotNo.Text.ToString()))
                {
                    foreach (DataGridViewRow dr in dgvCassette.Rows)
                    {
                        if ((bool)dr.Cells["colProcessFlag"].Value)
                        {
                            dr.Cells["colAssginRecipe"].Value = chk_SelectAll.Checked;
                            dr.Cells["colAssignProdType"].Value = chk_SelectAll.Checked;
                            dr.Cells["colAssignJobType"].Value = chk_SelectAll.Checked;
                        }
                    }
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(txtSelFrSlotNo.Text.ToString()))
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please key in Start Slot No", MessageBoxIcon.Error);
                        txtSelFrSlotNo.Focus();
                        return;
                    }
                    if (string.IsNullOrWhiteSpace(txtSelToSlotNo.Text.ToString()))
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please key in End Slot No", MessageBoxIcon.Error);
                        txtSelToSlotNo.Focus();
                        return;
                    }

                    int intFrSlotNo = 0;
                    int intToSlotNo = 0;
                    int.TryParse(txtSelFrSlotNo.Text.ToString(), out intFrSlotNo);
                    int.TryParse(txtSelToSlotNo.Text.ToString(), out intToSlotNo);

                    if (intToSlotNo < intFrSlotNo)
                    {
                        ShowMessage(this, lblCaption.Text, "", "The start Slot No must be less than the end Slot No!! ", MessageBoxIcon.Error);
                        return;
                    }

                    foreach (DataGridViewRow dr in dgvCassette.Rows)
                    {
                        int slotNo = 0;
                        int.TryParse(dr.Cells["colSlotNo"].Value.ToString(), out slotNo);
                        if (slotNo >= intFrSlotNo && slotNo <= intToSlotNo)
                        {
                            if ((bool)dr.Cells["colProcessFlag"].Value)
                            {
                                dr.Cells["colAssginRecipe"].Value = chk_SelectAll.Checked;
                                dr.Cells["colAssignProdType"].Value = chk_SelectAll.Checked;
                                dr.Cells["colAssignJobType"].Value = chk_SelectAll.Checked;
                            }
                        }
                    }
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
