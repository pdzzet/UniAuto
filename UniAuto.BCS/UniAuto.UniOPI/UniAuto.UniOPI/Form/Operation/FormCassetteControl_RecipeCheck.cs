using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormCassetteControl_RecipeCheck : FormBase
    {
        Dictionary<string, Recipe> dicRecipeUse;
        //Dictionary<string, Recipe> dicRecipeUse_Cross;

        public List<string> chooseRecipe;
        //public List<string> chooseRecipe_Cross;


        public FormCassetteControl_RecipeCheck(Dictionary<string, Recipe> dicRecipe)
        {
            InitializeComponent();

            dicRecipeUse = dicRecipe;
            //dicRecipeUse_Cross = dicRecipe_Cross;

            chooseRecipe = new List<string>();
            //chooseRecipe_Cross = new List<string>();
        }

        private void FormCassetteControl_RecipeCheck_Load(object sender, EventArgs e)
        {
            try
            {
                #region Initial Recipe Check Data
                dgvRecipe.Rows.Clear();

                foreach (Recipe _recipe in dicRecipeUse.Values)
                {                    
                    string _result = _recipe.AllEQ_RecipeCheck_OK ? "OK" :"NG";

                    dgvRecipe.Rows.Add(false, _recipe.RecipeName,_recipe.ServerName , _result);

                    if (!_recipe.AllEQ_RecipeCheck_OK) dgvRecipe.Rows[dgvRecipe.Rows.Count - 1].DefaultCellStyle.BackColor = Color.Red;
                }

                if (dgvRecipe.Rows.Count > 0) dgvRecipe.ClearSelection();
                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;

            try
            {
                foreach (DataGridViewRow _row in dgvRecipe.Rows)
                {
                    if ((bool)_row.Cells[colCheck.Name].Value)
                    {
                        if (_row.Cells[colServerName.Name].Value.ToString() == FormMainMDI.G_OPIAp.CurLine.ServerName)
                            chooseRecipe.Add(_row.Cells[colRecipeName.Name].Value.ToString());
                        //else
                            //chooseRecipe_Cross.Add(_row.Cells[colRecipeName.Name].Value.ToString());  //for CBCUT_2 跨line 
                    }
                }

                this.Close();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;

            this.Close();
        }

        private void dgvRecipe_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                dgvResult.Rows.Clear();

                if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

                DataGridViewRow dgvRow = dgvRecipe.Rows[e.RowIndex];

                Show_RecipeCheck_Result(dgvRow);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Show_RecipeCheck_Result(DataGridViewRow dgvRow)
        {
            try
            {
                if (dgvRow == null) return;

                string _nodeNo = string.Empty;
                string _recipeName = dgvRow.Cells["colRecipeName"].Value.ToString();
                string _serverName = dgvRow.Cells[colServerName.Name].Value.ToString();
                string _cimMode = string.Empty;
                string _result = string.Empty;

                if (_serverName == FormMainMDI.G_OPIAp.CurLine.ServerName)
                {
                    #region 本line recipe
                    if (dicRecipeUse.ContainsKey(_recipeName))
                    {
                        foreach (RecipeCheckEQ _recipe in dicRecipeUse[_recipeName].LocalRecipeCheck)
                        {

                            if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_recipe.LocalNo)) _cimMode = FormMainMDI.G_OPIAp.Dic_Node[_recipe.LocalNo].CIMMode.ToString();
                            else _cimMode = string.Empty;

                            dgvResult.Rows.Add(_recipe.LocalNo, _recipe.RecipeNo, _recipe.RecipeCheckResult, _recipe.ResultMsg, _cimMode);

                            dgvResult.Rows[dgvResult.Rows.Count - 1].DefaultCellStyle.BackColor = GetColor(_recipe.RecipeCheckResult, _recipe.ResultMsg);

                        }

                        if (dgvResult.Rows.Count > 0) dgvResult.ClearSelection();
                    }
                    #endregion
                }
                else
                {
                    #region 跨line recipe
                    //if (dicRecipeUse_Cross.ContainsKey(_recipeName))
                    //{
                    //    foreach (RecipeCheckEQ _recipe in dicRecipeUse_Cross[_recipeName].LocalRecipeCheck)
                    //    {
                    //        dgvResult.Rows.Add(_recipe.LocalNo, _recipe.RecipeNo, _recipe.RecipeCheckResult, _recipe.ResultMsg,"");
                    //        dgvResult.Rows[dgvResult.Rows.Count - 1].DefaultCellStyle.BackColor = GetColor(_recipe.RecipeCheckResult, _recipe.ResultMsg); ;
                    //    }

                    //    if (dgvResult.Rows.Count > 0) dgvResult.ClearSelection();
                    //}
                    #endregion
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private Color GetColor(string RecipeCheckResult,string ResutlMsg)
        {
            try
            {
                //RecipeRegisterValidationReturnReport
                //1. OK:
                //2. NG: NG, MACHINENG, NOMACHINE, RECIPELENNG, TIMEOUT
                //3. NONE: ZERO, CIMOFF, NOCHECK
                switch (RecipeCheckResult)
                {
                    case "OK": return  Color.White; 

                    case "NONE": return  Color.Gold;

                    case "NG":
                        if (ResutlMsg == "TIMEOUT") return Color.Pink;
                        else return Color.Red; 

                    default:   return Color.White; 
                } 
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                return Color.White; 
            }
        }
    }
}
