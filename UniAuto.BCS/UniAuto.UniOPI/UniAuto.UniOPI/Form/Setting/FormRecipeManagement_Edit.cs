using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormRecipeManagement_Edit : FormBase
    {
        #region Fields
        private SBRM_RECIPE ObjTable;
        #endregion

        string UseLineType = string.Empty;

        Dictionary<string, Node> dic_Node;
        SortedDictionary<int, string> dic_RecipSeq;

        public FormRecipeManagement_Edit()
        {
            InitializeComponent();
        }

        public FormRecipeManagement_Edit(string mesMode) : this()
        {
            FormMode = UniOPI.FormMode.AddNew;

            if (mesMode == "LOCAL") rdoOnlineLocal.Checked = true;
            else rdoOffline.Checked = true;
        }

        public FormRecipeManagement_Edit(string mesMode, SBRM_RECIPE objRecipe)
            : this()
        {
            FormMode = UniOPI.FormMode.Modify;

            if (mesMode == "LOCAL") rdoOnlineLocal.Checked = true;
            else rdoOffline.Checked = true;

            ObjTable = objRecipe;
        }


        private void FormRecipeManagement_Edit_Load(object sender, EventArgs e)
        {
            try
            {

                dic_Node = FormMainMDI.G_OPIAp.Dic_Node;
                dic_RecipSeq = FormMainMDI.G_OPIAp.Dic_RecipSeq;

                UseLineType = FormMainMDI.G_OPIAp.CurLine.LineType;
                
    
                InitialRecipeCheckNode();


                if (FormMode == UniOPI.FormMode.Modify )
                {
                    pnlAdd.Visible = false;
                    dgvRecipe.Visible = false;

                    this.Height = 470;

                    txtRecipeName.Enabled = false;
                    rdoOffline.Enabled = false;
                    rdoOnlineLocal.Enabled = false;

                    #region Load Data

                    txtRecipeName.Text = ObjTable.LINERECIPENAME;

                    if (ObjTable.ONLINECONTROLSTATE == "OFFLINE") rdoOffline.Checked = true;
                    else rdoOnlineLocal.Checked = true;

                    txtComment.Text = ObjTable.REMARK;

                    string[] _node = ObjTable.PPID.Split(';');
                    string _txtName = string.Empty;

                    for (int i = 0; i < _node.Length; i++)
                    {
                        string[] _data = _node[i].Split(':');

                        if (_data.Length != 2) continue;

                        //NodeNo_XX (XX表示recipe順序)
                        _txtName = string.Format("{0}_{1}", _data[0], (i + 1).ToString().PadLeft(2, '0'));

                        TextBox _txt = flpRecipeNo.Controls.OfType<TextBox>().FirstOrDefault<TextBox>(t => t.Name == _txtName);

                        if (_txt != null) _txt.Text = _data[1];
                    }
                    #endregion
                }
                else
                {
                    pnlAdd.Visible = true;
                    dgvRecipe.Visible = true;

                    this.Height = 730;

                    ActiveControl = txtRecipeName;
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

                string _ppid = string.Empty;
                //string _lineType = string.Empty;
                string _onlineControlState = string.Empty;
                string _recipeName = string.Empty;
                string _remark=string.Empty ;

                switch (_btn.Tag.ToString())
                {
                    case "ADD":

                        #region Add to DataGridView
                        _ppid = CheckData();

                        if (_ppid == string.Empty) return;

                        //_lineType = FormMainMDI.G_OPIAp.CurLine.LineType;
                        _onlineControlState = rdoOffline.Checked ? "OFFLINE" : "LOCAL";
                        _recipeName = txtRecipeName.Text.Trim();
                        _remark = txtComment.Text.ToString();

                        if (CheckUniqueKeyOK(UseLineType, _onlineControlState, _recipeName) == false) return;

                        #region 判斷是否存下gridview內

                        if (dgvRecipe.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[colMESMode.Name].Value.ToString().ToUpper().Equals(_onlineControlState) && 
                            r.Cells[colRecipeName.Name].Value.Equals(_recipeName)).Count()>0)
                        {
                            DataGridViewRow _addRow = dgvRecipe.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[colMESMode.Name].Value.ToString().ToUpper().Equals(_onlineControlState) &&
                                    r.Cells[colRecipeName.Name].Value.Equals(_recipeName)).First();

                            string _msg = string.Format("Recipe Name [{0}] is already insert. Please confirm whether you will overwite data ?", _recipeName);

                            if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return;

                            _addRow.Cells[colPPID.Name].Value = _ppid;
                            _addRow.Cells[colREMARK.Name].Value = _remark;
                        }
                        else
                        {
                            dgvRecipe.Rows.Add(_onlineControlState, _recipeName, _ppid, _remark);
                        }

                        #endregion

                        //dgvRecipe.Rows.Add(_onlineControlState, _recipeName, _recipeType, _ppid, _remark);
                        #endregion

                        #region clear data
                        
                        txtRecipeName.Text = string.Empty;
                        txtComment.Text = string.Empty;


                        foreach (TextBox _txt in flpRecipeNo.Controls.OfType<TextBox>())
                        {
                            _txt.Text = string.Empty;
                        }                            
                        

                        txtRecipeName.Focus();

                        #endregion

                        break;

                    case "OK":

                        #region OK
                        switch (FormMode)
                        {
                            case FormMode.AddNew:

                                #region Add

                                SBRM_RECIPE _recipe;

                                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                                foreach (DataGridViewRow _row in dgvRecipe.Rows)
                                {
                                    _recipe = new SBRM_RECIPE();
                                    _recipe.FABTYPE = FormMainMDI.G_OPIAp.CurLine.FabType;
                                    _recipe.LINETYPE = UseLineType; //FormMainMDI.G_OPIAp.CurLine.LineType;
                                    _recipe.ONLINECONTROLSTATE = _row.Cells["colMESMode"].Value.ToString();
                                    _recipe.LINERECIPENAME = _row.Cells["colRecipeName"].Value.ToString();
                                    _recipe.PPID = _row.Cells["colPPID"].Value.ToString();
                                    _recipe.LASTUPDATEDT = System.DateTime.Now;
                                    _recipe.UPDATEOPERATOR = FormMainMDI.G_OPIAp.LoginUserID;
                                    _recipe.UPDATELINEID = FormMainMDI.G_OPIAp.CurLine.ServerName;
                                    _recipe.UPDATEPCIP = FormMainMDI.G_OPIAp.LocalIPAddress;
                                    _recipe.REMARK = _row.Cells["colREMARK"].Value.ToString();

                                    _ctxBRM.SBRM_RECIPE.InsertOnSubmit(_recipe);
                                }

                                #endregion
                                
                                break;

                            case FormMode.Modify:

                                #region Modify

                                _ppid = CheckData();

                                if (_ppid == string.Empty) return;

                                //_lineType = FormMainMDI.G_OPIAp.CurLine.LineType;
                                _onlineControlState = rdoOffline.Checked ? "OFFLINE" : "LOCAL";
                                _recipeName = txtRecipeName.Text.Trim();

                                if (!SetModify(UseLineType, _onlineControlState, _recipeName, _ppid)) return;
                                                                                             
                                #endregion
                                
                                break;

                            default:
                                break;
                        }

                        this.DialogResult = System.Windows.Forms.DialogResult.OK;

                        this.Close();

                        #endregion

                        break;

                    case "Cancel":

                        this.DialogResult = System.Windows.Forms.DialogResult.Cancel ;
                        
                        this.Close();
                        
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

        private bool SetModify(string RecipeLineType, string OnlineControlState, string RecipeName, string PPID )
        {
            try
            {
                SBRM_RECIPE _objRecipe = null;

                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                var objSelModify = from msg in _ctxBRM.SBRM_RECIPE
                                    where msg.LINETYPE == RecipeLineType &&
                                            msg.ONLINECONTROLSTATE == OnlineControlState &&
                                            msg.LINERECIPENAME == RecipeName
                                    select msg;

                if (objSelModify.Count() > 0) _objRecipe = objSelModify.ToList()[0];


                var objAddModify = from msg in _ctxBRM.GetChangeSet().Inserts.OfType<SBRM_RECIPE>()
                                    where msg.LINETYPE == RecipeLineType &&
                                            msg.ONLINECONTROLSTATE == OnlineControlState &&
                                            msg.LINERECIPENAME == RecipeName
                                    select msg;

                if (objAddModify.Count() > 0) _objRecipe = objAddModify.ToList()[0];

                if (_objRecipe == null) return false;
                _objRecipe.PPID = PPID;
                _objRecipe.LASTUPDATEDT = System.DateTime.Now;
                _objRecipe.UPDATEOPERATOR = FormMainMDI.G_OPIAp.LoginUserID;
                _objRecipe.UPDATELINEID = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _objRecipe.UPDATEPCIP = FormMainMDI.G_OPIAp.LocalIPAddress;
                _objRecipe.REMARK = txtComment.Text.Trim();
                
                return true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                
                return false;
            }
        }

        private bool CheckUniqueKeyOK(string recipeLineType, string mesMode, string recipeName)
        {
            try
            {
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                string _errMsg = string.Empty;

                #region check 資料是否已存在SBRM_RECIPE
                int _rowCnt = (from _d in _ctxBRM.SBRM_RECIPE
                                where _d.LINETYPE == recipeLineType &&
                                        _d.ONLINECONTROLSTATE == mesMode &&
                                        _d.LINERECIPENAME == recipeName
                                select _d).Count();

                if (_rowCnt > 0)
                {
                    _errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is ({0}, {1}, {2})！", recipeLineType, mesMode, recipeName);
                    ShowMessage(this, this.lblCaption.Text, "", _errMsg, MessageBoxIcon.Warning);
                    txtRecipeName.Focus();
                    txtRecipeName.SelectAll();
                    return false;
                }
                #endregion

                #region check 資料是否已存在新增資料內 (尚未儲存至SBRM_RECIPE)
                if (FormMode == UniOPI.FormMode.AddNew)
                {
                    var _add = _ctxBRM.GetChangeSet().Inserts.OfType<SBRM_RECIPE>().Where(
                        _d => _d.LINETYPE == recipeLineType &&
                                _d.ONLINECONTROLSTATE == mesMode &&
                                _d.LINERECIPENAME == recipeName);
                    if (_add.Count() > 0)
                    {
                        _errMsg = string.Format("UNIQUE KEY constraint violated. Duplicate key is ({0}, {1}, {2})！", recipeLineType, mesMode, recipeName);
                        ShowMessage(this, this.lblCaption.Text, "", _errMsg, MessageBoxIcon.Warning);
                        txtRecipeName.Focus();
                        txtRecipeName.SelectAll();
                        return false;
                    }
                }
                #endregion
                
                return true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                
                return false;
            }
        }

        private string CheckData()
        {
            try
            {
                string _ppid = string.Empty;

                if (txtRecipeName.Text.Trim()==string.Empty)
                {
                    ShowMessage(this, this.lblCaption.Text, "", "Recipe Name required！", MessageBoxIcon.Warning);
                    txtRecipeName.Focus();
                    return string.Empty ;
                }

                #region check recipe no length
                foreach (TextBox txt in flpRecipeNo.Controls.OfType<TextBox>())
                {
                    if (txt.Text.Length != txt.MaxLength)
                    {
                        ShowMessage(this, this.lblCaption.Text, "", string.Format("{0}'s length must be {1}！", txt.Tag.ToString(), txt.MaxLength), MessageBoxIcon.Warning);
                        txt.Focus();
                        return string.Empty;
                    }
                    else
                    {
                        //L2:recipe no;L3:recipe no....
                        _ppid = _ppid + (_ppid==string.Empty ? "" :";") + string.Format("{0}:{1}", txt.Tag.ToString(), txt.Text.ToString());
                    }
                }
                #endregion


                #region PVD及ITO LINE --第一組的L3(L3_02)設定的RECIPE都是0000或沒有值的話..第二組(L3_04)就是0000 ---2015.02.10 俊成
                if (UseLineType == "PVD" || UseLineType == "ITO_I" || UseLineType == "ITO_N")
                {
                    TextBox _firstTxt = null;
                    TextBox _secondTxt = null;

                    //NodeNo_XX (XX表示recipe順序)
                    var _varFirst = flpRecipeNo.Controls.Find("L3_02", false).OfType<TextBox>();
                    var _varSec = flpRecipeNo.Controls.Find("L3_04", false).OfType<TextBox>();
                    if (_varFirst != null && _varSec!=null)
                    {
                        _firstTxt = _varFirst.First();
                        _secondTxt = _varSec.First();

                        if (_firstTxt.Text == "0".PadLeft(_firstTxt.MaxLength, '0'))
                        {
                            if (_secondTxt.Text != "0".PadLeft(_secondTxt.MaxLength, '0'))
                            {
                                ShowMessage(this, this.lblCaption.Text, "", "Second Recipe No of L3 must be 0000", MessageBoxIcon.Warning);
                                _secondTxt.Focus();
                                return string.Empty;
                            }
                        }
                    }
                    
                }
                #endregion

                return _ppid;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                
                return string.Empty;
            }
        }

        private void InitialRecipeCheckNode()
        {
            try
            {
                int ctlTabIdx = 1;
                Font font = new Font("Calibri", 12.75f, FontStyle.Regular);

                Node _node = null;

                Label _lbl = null;
                TextBox _txt = null;
                string _name = string.Empty;


                foreach (KeyValuePair<int, string> _recipe in dic_RecipSeq)
                {
                    if (dic_Node.ContainsKey(_recipe.Value) == false) continue;

                    _node = dic_Node[_recipe.Value];

                    if (_node.RecipeLen == 0) continue;

                    //NodeNo_XX (XX表示recipe順序)
                    _name = _recipe.Value + "_" + _recipe.Key.ToString().PadLeft(2, '0');
                    #region Create Label
                    _lbl = new Label();
                    _lbl.Name = "lbl" + _name;
                    _lbl.Text = _node.NodeNo + "_" + _node.NodeID;
                    _lbl.AutoSize = false;
                    _lbl.Size = new System.Drawing.Size(110, 28);
                    _lbl.Font = new Font("Calibri", 11.25f, FontStyle.Regular);
                    _lbl.ForeColor = Color.Black;
                    _lbl.TabIndex = (ctlTabIdx++);
                    _lbl.TextAlign = ContentAlignment.MiddleRight;
                    flpRecipeNo.Controls.Add(_lbl);
                    #endregion

                    #region Create TextBox
                    _txt = new TextBox();
                    _txt.Name = _name;
                    _txt.MaxLength = dic_Node[_recipe.Value].RecipeLen; //FormMainMDI.G_OPIAp.Dic_Node[_recipe.Value].RecipeLen;  
                    //_txt.CharacterCasing = CharacterCasing.Upper;
                    _txt.Size = new System.Drawing.Size(170, 28);
                    _txt.Font = font;
                    _txt.Tag = _recipe.Value;
                    _txt.TabIndex = (ctlTabIdx++);
                    _txt.KeyPress += new KeyPressEventHandler(txtRecipeNo_KeyPress);
                    flpRecipeNo.Controls.Add(_txt);
                    #endregion
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }


        //private void Set_CuttingDefaultRecipe()
        //{
        //    try
        //    {
        //        if (cmbRecipeType.SelectedValue == null) return;

        //        List<string> _lstUseNode = null;

        //        switch (cmbRecipeType.SelectedValue.ToString())
        //        {
        //            case "LOT":

        //                //只有 L2 ~ L3需要設定recipe no
        //                _lstUseNode = new List<string>(new string[] { "L2", "L3" });

        //                break;

        //            case "PROCESSLINE":

        //                //只有 L4 ~ L10 需要設定recipe no
        //                _lstUseNode = new List<string>(new string[] { "L4", "L5", "L6", "L7", "L8", "L9", "L10" });

        //                break;

        //            case "STB":

        //                //只有 L11 ~ L15 需要設定recipe no
        //                _lstUseNode = new List<string>(new string[] { "L11", "L12", "L13", "L14", "L15" });

        //                break;
        //            default:
        //                break;
        //        }

        //        if (_lstUseNode == null) return;

        //        #region 設定需輸入的recipe node
        //        foreach (TextBox _txt in flpRecipeNo.Controls.OfType<TextBox>())
        //        {
        //            if (_lstUseNode.Contains(_txt.Name))
        //            {
        //                _txt.Text = string.Empty;
        //                _txt.ReadOnly = false;
        //            }
        //            else
        //            {
        //                //if (FormMainMDI.G_OPIAp.Dic_Node.ContainsKey(_txt.Name))
        //                if (dic_Node.ContainsKey(_txt.Name))
        //                {
        //                    Node _node = FormMainMDI.G_OPIAp.Dic_Node[_txt.Name];
        //                    _txt.Text = _node.DefaultRecipeNo;
        //                    _txt.ReadOnly = true;
        //                }
        //                else
        //                {
        //                    _txt.Text = string.Empty;
        //                    _txt.ReadOnly = false;
        //                }
        //            }
        //        }
        //        #endregion
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
        //        ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
        //    }
        //}

        //0~9, A~Z, a~z (Except：I, O, i, o))
        private void txtRecipeNo_KeyPress(object sender, KeyPressEventArgs e)
        {
            int asciiCode = (int)e.KeyChar;
            if ((asciiCode >= 48 & asciiCode <= 57) |   // 0-9
                (asciiCode >= 65 & asciiCode <= 90 & asciiCode != 79 & asciiCode != 73) |   // A-Z except I、O
                (asciiCode >= 97 & asciiCode <= 122 & asciiCode != 111 & asciiCode != 105) |   // a-z except i、o
                asciiCode == 8 )    // Backspace
            {
                e.Handled = false;
            }
            else
                e.Handled = true;
        }
    }
}
