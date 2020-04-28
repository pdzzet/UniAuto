using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormRecipeManagement_NodeRecipeCheck : FormBase
    {
        public FormRecipeManagement_NodeRecipeCheck()
        {
            InitializeComponent();
        }

        private void FormRecipeManagement_NodeRecipeCheck_Load(object sender, EventArgs e)
        {
            try
            {
                int _ctlTabIdx = 1;

                Font _font = new Font("Calibri", 12.75f, FontStyle.Regular);

                CheckBox _chk = null;

                flpNode.Controls.Clear();
                               
                foreach (Node _node in FormMainMDI.G_OPIAp.Dic_Node.Values)
                {
                    if (_node.RecipeSeq.Count == 0) continue;
                    if (_node.RecipeSeq.Contains(0)) continue;

                    _chk = new CheckBox();
                    _chk.Name = _node.NodeNo;
                    _chk.Text = string.Format("{0}-{1}", _node.NodeNo, _node.NodeName);
                    _chk.AutoSize = false;
                    _chk.Width = 200;
                    _chk.Height = 30;
                    _chk.Font = _font;
                    _chk.ForeColor = Color.Black;
                    _chk.TabIndex = (_ctlTabIdx++);
                    _chk.Checked = _node.RecipeRegisterCheck;
                    flpNode.Controls.Add(_chk);
                }
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

                switch (_btn.Name)
                {
                    case "btnOK":

                        #region Update Recipe Check
                        string _check = string.Empty;
                        string _nodeNo = string.Empty;
                        string _sqlDesc = string.Empty;
                        string _sqlErr = string.Empty;

                        UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                        string _msg = string.Format("Please confirm whether you will update Recipe Check setting ?");

                        if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text,  _msg)) return;

                        if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text )) return;

                        foreach (Control _ctrl in flpNode.Controls)
                        {
                            CheckBox _chk = (CheckBox)_ctrl;

                            if (FormMainMDI.G_OPIAp.Dic_Node[_chk.Name].RecipeRegisterCheck != _chk.Checked)
                            {
                                _check = ((CheckBox)_ctrl).Checked ? "Y" : "N";

                                var _objData = from _data in _ctxBRM.SBRM_NODE
                                               where _data.SERVERNAME  == FormMainMDI.G_OPIAp.CurLine.ServerName &&
                                                    _data.NODENO == _ctrl.Name
                                               select _data;

                                if (_objData.Count() == 0) continue;

                                SBRM_NODE _change = _objData.ToList()[0];

                                _change.RECIPEREGVALIDATIONENABLED = _check;

                                _sqlDesc = _sqlDesc + (_sqlDesc == string.Empty ? string.Empty : ";") + string.Format("MOD [ {0} RecipeRegisterCheck Set {1} ] ", _ctrl.Name, _check);
                            }
                        }

                        try
                        {
                            _ctxBRM.SubmitChanges();
                        }
                        catch (System.Data.Linq.ChangeConflictException ex)
                        {
                            _sqlErr = ex.ToString();
                            NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                            ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                        }

                        #region 紀錄opi history
                        string _err = UniTools.InsertOPIHistory_DB("SBRM_NODE:RECIPEREGVALIDATIONENABLED", _sqlDesc, _sqlErr);

                        if (_err != string.Empty)
                        {
                            ShowMessage(this, lblCaption.Text, "Save OPI History Error！", _err, MessageBoxIcon.Error);
                        }
                        #endregion

                        Public.SendDatabaseReloadRequest("SBRM_NODE:RECIPEREGVALIDATIONENABLED");

                        //重新取得recipe check 設定~ 
                        UniTools.Reload_RecipeSetting();

                        #endregion

                        ShowMessage(this, this.lblCaption.Text, "", "Update Recipe Check Setting Success!", MessageBoxIcon.Information);

                        this.DialogResult = System.Windows.Forms.DialogResult.OK;
                        break;

                    case "btnCancel":

                        this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                        break;

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

        private void chkSelect_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                foreach (Control _ctrl in flpNode.Controls.OfType<CheckBox>())
                {
                    CheckBox _chk = (CheckBox)_ctrl;

                    _chk.Checked = ((CheckBox)sender).Checked;
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
