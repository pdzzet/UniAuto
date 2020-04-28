using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormCassetteControl_RecipeChoose : FormBase
    {
        public string RecipeName;
        public string RecipeID;

        public FormCassetteControl_RecipeChoose(Dictionary<string, Recipe> dicRecipe_All)
        {
            InitializeComponent();

            dgvRecipe.Rows.Clear();

            foreach (Recipe _recipe in dicRecipe_All.Values)
            {
                dgvRecipe.Rows.Add(_recipe.RecipeName, _recipe.PPID);
            }
        }

        public FormCassetteControl_RecipeChoose(string RecipeType, Dictionary<string, Recipe> dicRecipe_All)
        {
            InitializeComponent();

            dgvRecipe.Rows.Clear();

            foreach (var _recipe in dicRecipe_All.Values)
            {
                dgvRecipe.Rows.Add(_recipe.RecipeName, _recipe.PPID);
            }
        }

        private void Button_Click(object sender, EventArgs e)
        {
            try
            {
                Button _btn = (Button)sender;

                switch (_btn.Text.ToUpper())
                {
                    case "OK":

                        if (dgvRecipe.CurrentRow == null)
                        {
                            ShowMessage(this, lblCaption.Text , "", "Please Choose Recipe Name !", MessageBoxIcon.Error);

                            return;
                        }

                        RecipeName = dgvRecipe.CurrentRow.Cells[colRecipeName.Name].Value.ToString();
                        RecipeID = dgvRecipe.CurrentRow.Cells[colPPID.Name].Value.ToString();

                        this.DialogResult = System.Windows.Forms.DialogResult.OK;

                        break;

                    case "CANCEL":

                        this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                        break;

                    default :
                        break;
                }

                this.Close();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }
    }
}
