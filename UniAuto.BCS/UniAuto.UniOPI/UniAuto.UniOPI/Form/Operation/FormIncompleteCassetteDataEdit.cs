using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormIncompleteCassetteDataEdit : FormBase
    {
        private TreeNode CurrentNode;

        Dictionary<string, Recipe> dicRecipe_All;

        public FormIncompleteCassetteDataEdit(TreeNode node)
        {
            InitializeComponent();

            CurrentNode = node;
        }

        private void FormIncompleteCassetteDataEdit_Load(object sender, EventArgs e)
        {
            try
            {
                dicRecipe_All = new Dictionary<string, Recipe>();

                #region Show Data

                txtPosition.Text = CurrentNode.Tag.ToString();

                foreach (TreeNode _tN in CurrentNode.Nodes)
                {
                    switch (_tN.Name)
                    {
                        case "PRODUCTNAME":
                            txtProductName.Text = _tN.Tag.ToString();
                            break;
                        case "HOSTPRODUCTNAME":
                            txtHostProductName.Text = _tN.Tag.ToString();
                            break;
                        case "DENSEBOXID":
                            txtDenseBoxID.Text = _tN.Tag.ToString();
                            break;
                        case "PRODUCTJUDGE":
                            txtProductJudge.Text = _tN.Tag.ToString();
                            break;
                        case "PRODUCTGRADE":
                            txtProductGrade.Text = _tN.Tag.ToString();
                            break;
                        //case "SUBPRODUCTGRADES":
                        //    txtSubProductGrades.Text = _tN.Tag.ToString();
                        //    break;
                        case "PAIRPRODUCTNAME":
                            txtPairProductName.Text = _tN.Tag.ToString();
                            break;
                        case "LOTNAME":
                            txtLotName.Text = _tN.Tag.ToString();
                            break;
                        case "PRODUCTRECIPENAME":
                            txtProdRecipeName.Text = _tN.Tag.ToString();
                            break;
                        case "HOSTPRODUCTRECIPENAME":
                            txtHostProdRecipeName.Text = _tN.Tag.ToString();
                            break;
                        case "PRODUCTSPECNAME":
                            txtProductSpecName.Text = _tN.Tag.ToString();
                            break;
                        case "PROCESSOPERATIONNAME":
                            txtProcessOperName.Text = _tN.Tag.ToString();
                            break;
                        case "PRODUCTOWNER":
                            txtProductSpecName.Text = _tN.Tag.ToString();
                            break;
                        case "VCRREADFLAG":
                            txtVCRReadFlag.Text = _tN.Tag.ToString();
                            break;
                        case "SHORTCUTFLAG":
                            txtShortCutFlag.Text = _tN.Tag.ToString();
                            break;
                        case "SAMPLEFLAG":
                            txtSampleFlag.Text = _tN.Tag.ToString();
                            break;
                        case "PROCESSFLAG":
                            txtProcessFlag.Text = _tN.Tag.ToString();
                            break;
                        case "PROCESSCOMMUNICATIONSTATE":
                            txtProcessCommState.Text = _tN.Tag.ToString();
                            break;
                        default:
                            break;
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void LineRecipeName_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                TextBox _txt = (TextBox)sender;

                if (dicRecipe_All.Count == 0) LoadPPIDSetting();

                FormCassetteControl_RecipeChoose frm = new FormCassetteControl_RecipeChoose(dicRecipe_All);

                if (System.Windows.Forms.DialogResult.OK == frm.ShowDialog())
                {
                    _txt.Text = frm.RecipeName;
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
                if (CheckData() == false) return;

                CurrentNode.Text = string.Format("Position [ {0} ] - [ {1} ]", txtPosition.Text, txtProductName.Text); 
                CurrentNode.Tag = txtPosition.Text;

                foreach (TreeNode _tN in CurrentNode.Nodes)
                {
                    switch (_tN.Name)
                    {
                        case "PRODUCTNAME":
                            _tN.Text = string.Format("Product Name [ {0} ]", txtProductName.Text);
                            _tN.Tag = txtProductName.Text;
                            break;
                        case "HOSTPRODUCTNAME":
                            _tN.Text = string.Format("Host Product Name [ {0} ]", txtHostProductName.Text);
                            _tN.Tag = txtHostProductName.Text;
                            break;
                        case "DENSEBOXID":
                            _tN.Text = string.Format("Dense Box ID [ {0} ]", txtDenseBoxID.Text);
                            _tN.Tag = txtDenseBoxID.Text;
                            break;
                        case "PRODUCTJUDGE":
                            _tN.Text = string.Format("Product Judge [ {0} ]", txtProductJudge.Text);
                            _tN.Tag = txtProductJudge.Text;
                            break;
                        case "PRODUCTGRADE":
                            _tN.Text = string.Format("Product Grade [ {0} ]", txtProductGrade.Text);
                            _tN.Tag = txtProductGrade.Text;
                            break;
                        //case "SUBPRODUCTGRADES":
                        //    _tN.Text = string.Format("Sub Product Grades [ {0} ]", txtSubProductGrades.Text);
                        //    _tN.Tag = txtSubProductGrades.Text;
                        //    break;
                        case "PAIRPRODUCTNAME":
                            _tN.Text = string.Format("Pair Product Name [ {0} ]", txtPairProductName.Text);
                            _tN.Tag = txtPairProductName.Text;
                            break;
                        case "LOTNAME":
                            _tN.Text = string.Format("Lot Name [ {0} ]", txtLotName.Text);
                            _tN.Tag = txtLotName.Text;
                            break;
                        case "PRODUCTRECIPENAME":
                            _tN.Text = string.Format("Product Recipe Name [ {0} ]", txtProdRecipeName.Text);
                            _tN.Tag = txtProdRecipeName.Text;
                            break;
                        case "HOSTPRODUCTRECIPENAME":
                            _tN.Text = string.Format("Host Product Recipe Name [ {0} ]", txtHostProdRecipeName.Text);
                            _tN.Tag = txtHostProdRecipeName.Text;
                            break;
                        case "PRODUCTSPECNAME":
                            _tN.Text = string.Format("Product Spec Name [ {0} ]", txtProductSpecName.Text);
                            _tN.Tag = txtProductSpecName.Text;
                            break;
                        case "PROCESSOPERATIONNAME":
                            _tN.Text = string.Format("Process Operation Name [ {0} ]", txtProcessOperName.Text);
                            _tN.Tag = txtProcessOperName.Text;
                            break;
                        case "PRODUCTOWNER":
                            _tN.Text = string.Format("Product Owner [ {0} ]", txtProductSpecName.Text);
                            _tN.Tag = txtProductSpecName.Text;
                            break;
                        case "VCRREADFLAG":
                            _tN.Text = string.Format("VCR Read Flag [ {0} ]", txtVCRReadFlag.Text);
                            _tN.Tag = txtVCRReadFlag.Text;
                            break;
                        case "SHORTCUTFLAG":
                            _tN.Text = string.Format("Short Cut Flag [ {0} ]", txtShortCutFlag.Text);
                            _tN.Tag = txtShortCutFlag.Text;
                            break;
                        case "SAMPLEFLAG":
                            _tN.Text = string.Format("Sample Flag [ {0} ]", txtSampleFlag.Text);
                            _tN.Tag = txtSampleFlag.Text;
                            break;
                        case "PROCESSFLAG":
                            _tN.Text = string.Format("PProcess Flag [ {0} ]", txtProcessFlag.Text);
                            _tN.Tag = txtProcessFlag.Text;
                            break;
                        case "PROCESSCOMMUNICATIONSTATE":
                            _tN.Text = string.Format("Process Communication State [ {0} ]", txtProcessCommState.Text);
                            _tN.Tag = txtProcessCommState.Text;
                            break;
                        default:
                            break;
                    }
                }

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
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
        }

        private bool CheckData()
        {
            try
            {
                if (txtPosition.Text.Trim() == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please input Position", MessageBoxIcon.Warning);
                    return false;
                }

                if (txtProductName.Text.Trim() == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please input Product Name", MessageBoxIcon.Warning);
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

        private void LoadPPIDSetting()
        {
            try
            {
                Recipe _new;

                var _var = (from _recipe in FormMainMDI.G_OPIAp.DBCtx.SBRM_RECIPE
                            where _recipe.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType
                            select new { RecipeName = _recipe.LINERECIPENAME, PPID = _recipe.PPID });


                dicRecipe_All.Clear();

                foreach (var _recipe in _var)
                {
                    _new = new Recipe(FormMainMDI.G_OPIAp.CurLine.ServerName, _recipe.RecipeName, _recipe.PPID);
                    dicRecipe_All.Add(_recipe.RecipeName, _new);
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
