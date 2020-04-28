using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;


namespace UniOPI
{
    public partial class FormCassetteControl_Local : FormBase
    {
        private bool IsRemap = false;

        public Port curPort = new Port();

        public string MesControlMode = string.Empty;

        private List<LocalCassetteDataReply.LOTDATAc> LstLots;
        private LocalCassetteDataReply.LOTDATAc CurLotData;

        public Queue<string> Q_BCSReport { get; set; } //BC要OPI Pop的訊息

        private Dictionary<string, Recipe> dicRecipe_All;    //目前mes mode下可使用的recipe
        //private Dictionary<string, Recipe> dicRecipe_Cross;    //目前mes mode下可使用的recipe-- for recipe check use (Cross line)

        public Dictionary<string, Recipe> dicRecipeUse;  //被使用的recipe -- for recipe check use (local line)
        //public Dictionary<string, Recipe> dicRecipeUse_Cross;  //被使用的recipe -- for recipe check use (Cross line)

        #region for cutting line
        public Recipe Recipe_Lot_Cutting;   //Cutting Line Lot Recipe Name
        public Recipe Recipe_Proc_Cutting;  //Process Line Recipe Name
        //public Recipe Recipe_STB_Cutting;   //STB Prod Recipe Name
        //public Recipe Recipe_CrossProc_Cutting;  //Process Line Recipe Name - for cross line
        //public Recipe Recipe_CrossSTB_Cutting;  //STB Prod Recipe Name - for cross line
        #endregion

        public FormCassetteControl_Local()
        {
            InitializeComponent();

            #region 初始化recipe
            dicRecipe_All = new Dictionary<string, Recipe>();
            //dicRecipe_Cross = new Dictionary<string, Recipe>();

            dicRecipeUse = new Dictionary<string, Recipe>();
            //dicRecipeUse_Cross = new Dictionary<string, Recipe>();

            #endregion

            lblCaption.Text = "Local Cassette Control";
        }

        private void FormCassetteControl_Local_Load(object sender, EventArgs e)
        {
            try
            {
                Q_BCSReport = new Queue<string>();

                if (curPort.CassetteID == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text , "", "Cassette ID is empty", MessageBoxIcon.Error);
                    return;
                }

                //取得畫面顯示物件
                GetCSTControlDef();

                //初始化Form上面的資訊
                InitialFormInfo();

                lblPortName.Text = curPort.PortID.ToString();
                txtCassetteID.Text = curPort.CassetteID;
                txtProdQty.Text = curPort.PortGlassCount.ToString();
                txtProcessQty.Text = curPort.PortGlassCount.ToString();


                //重新取得recipe check 設定~ 避免被修改過
                UniTools.Reload_RecipeSetting();

                //讀取 DB PPID 資料
                LoadPPIDSetting();

                #region 當 ControlMode 為 Local 傳送和BC要 CST Data
                SendtoBC_CassetteDataRequest();
                #endregion

                if (curPort.SubCassetteStatus == "WAREMAPEDIT") IsRemap = true;
                else IsRemap = false;

                tmrRefresh.Enabled = true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void FormCassetteControl_Local_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                tmrRefresh.Enabled = false;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvProduct_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (dgvProduct.IsCurrentCellDirty)
            {
                dgvProduct.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }    
        }

        private void dgvProduct_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

            try
            {
                //ARRAY Line 都要可以設定recipe
                if (FormMainMDI.G_OPIAp.CurLine.FabType == "ARRAY")
                    return;

                #region 如果MES download 的是以下产品信息则不能做修改  -- OWNERTYPE: P || OWNERID: RESD
                if (dgvProduct["colOwnerType", e.RowIndex].Value.ToString() == "P" || dgvProduct["colOwnerID", e.RowIndex].Value.ToString() == "RESD")
                {
                    if (dgvProduct["colProcessFlag", e.RowIndex].Value.ToString().ToUpper() != dgvProduct["colProcessFlag_MES", e.RowIndex].Value.ToString().ToUpper())
                    {
                        ShowMessage(this, lblCaption.Text, string.Empty, "OWNERTYPE = P or OWNERID = RESD can't be modify !", MessageBoxIcon.Error);
                        dgvProduct["colProcessFlag", e.RowIndex].Value = dgvProduct["colProcessFlag_MES", e.RowIndex].Value;
                    }
                    if (dgvProduct["colRecipeID", e.RowIndex].Value != dgvProduct["colRecipeID_MES", e.RowIndex].Value)
                    {
                        ShowMessage(this, lblCaption.Text, string.Empty, "OWNERTYPE = P or OWNERID = RESD can't be modify !", MessageBoxIcon.Error);
                        dgvProduct["colRecipeID", e.RowIndex].Value = dgvProduct["colRecipeID_MES", e.RowIndex].Value;
                    }
                }
                #endregion

                #region BC在 Local 模式下只允许更改recipe 和 SAMPLING FLAG
                switch (dgvProduct.Columns[e.ColumnIndex].Name)
                {
                    case "colProcessFlag":

                        #region 跳片判斷 --DENSE 從slot01開始建帳，不可跳片 EX:產生了15片帳 slot01-15~ 我只要抽10片，就要從slot01-05取消
                        if (curPort.PortAttribute == "DENSE")
                        {
                            if ((bool)dgvProduct["colProcessFlag", e.RowIndex].Value == true)
                            {
                                if (e.RowIndex > 0)
                                {
                                    //選取須由上往下勾選
                                    if ((bool)dgvProduct["colProcessFlag", e.RowIndex - 1].Value == false && dgvProduct["colGlassID", e.RowIndex - 1].Value.ToString() != string.Empty)
                                    {
                                        dgvProduct["colProcessFlag", e.RowIndex].Value = false;
                                    }
                                }
                            }
                            else if ((bool)dgvProduct["colProcessFlag", e.RowIndex].Value == false)
                            {
                                //若要取消flag,下面一片process flag不可以被勾選(跳片)--由下往上取消勾選
                                if (e.RowIndex < dgvProduct.Rows.Count - 1)
                                {
                                    if ((bool)dgvProduct["colProcessFlag", e.RowIndex + 1].Value == true)
                                    {
                                        dgvProduct["colProcessFlag", e.RowIndex].Value = true;
                                    }
                                }
                            }

                        }
                        #endregion

                        break;

                    case "colRecipeID":
                        //txtRecipeID.Text = dgvProduct["colRecipeID", e.RowIndex].Value.ToString();

                        //if (dicRecipe_All.ContainsKey(txtRecipeID.Text))
                        //{
                        //    dgvProduct["colPPID", e.RowIndex].Value = dicRecipe_All[txtRecipeID.Text].PPID;
                        //}
                        //else dgvProduct["colPPID", e.RowIndex].Value = string.Empty;

                        break;

                    default:
                        break;
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvProduct_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex < 0 || e.RowIndex < 0) return;

            try
            {
                string _colName = dgvProduct.Columns[e.ColumnIndex].Name;

                switch (_colName)
                {
                    case "colRecipeID":

                        if (dgvProduct["colRecipeID", e.RowIndex].Value == null)
                        {
                            dgvProduct["colPPID", e.RowIndex].Value = string.Empty;
                            break;
                        }

                        string _recipe = dgvProduct["colRecipeID", e.RowIndex].Value.ToString();

                        if (dicRecipe_All.ContainsKey(_recipe))
                        {
                            dgvProduct["colPPID", e.RowIndex].Value = dicRecipe_All[_recipe].PPID;
                        }
                        else
                        {
                            dgvProduct["colPPID", e.RowIndex].Value = string.Empty;
                        }
                        break;

                    case "colProcessFlag":

                        #region 跳片判斷 -- DENSE 從slot01開始建帳，不可跳片 EX:產生了15片帳 slot01-15~ 我只要抽10片，就要從slot01-05取消
                        if (curPort.PortAttribute == "DENSE")
                        {
                            if ((bool)dgvProduct["colProcessFlag", e.RowIndex].Value == true)
                            {
                                if (e.RowIndex > 0)
                                {
                                    //選取須由上往下勾選
                                    if ((bool)dgvProduct["colProcessFlag", e.RowIndex - 1].Value == false && dgvProduct["colGlassID", e.RowIndex - 1].Value.ToString() != string.Empty)
                                    {
                                        ShowMessage(this, lblCaption.Text, "", string.Format("sampling flag of slot {0} must be choose", dgvProduct.Rows[e.RowIndex - 1].Cells[colSlotNo.Name].Value), MessageBoxIcon.Error);
                                    }
                                }
                            }
                            else if ((bool)dgvProduct["colProcessFlag", e.RowIndex].Value == false)
                            {
                                //若要取消flag,下面一片process flag不可以被勾選(跳片)--由下往上取消勾選
                                if (e.RowIndex < dgvProduct.Rows.Count - 1)
                                {
                                    if ((bool)dgvProduct["colProcessFlag", e.RowIndex + 1].Value == true)
                                    {
                                        ShowMessage(this, lblCaption.Text, "", string.Format("sampling flag of slot {0} is choose", dgvProduct.Rows[e.RowIndex + 1].Cells[colSlotNo.Name].Value), MessageBoxIcon.Error);

                                    }
                                }
                            }
                        }
                        #endregion

                        #region 計算被勾選的glass count
                        int i = 0;
                        foreach (DataGridViewRow row in dgvProduct.Rows)
                        {
                            if (row.Cells["colGlassID"].Value != null)
                            {
                                if (((bool)row.Cells["colProcessFlag"].EditedFormattedValue) == true)
                                    i++;
                            }
                            else
                            {
                                row.Cells["colProcessFlag"].Value = false;
                            }
                        }
                        txtProcessQty.Text = i.ToString();
                        #endregion

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

        private void dgvProduct_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (dgvProduct.CurrentRow == null) return;

                switch (dgvProduct.Columns[e.ColumnIndex].Name)
                {

                    case "colProcessFlag":

                        #region 計算被勾選的glass count
                        int i = 0;
                        foreach (DataGridViewRow row in dgvProduct.Rows)
                        {
                            if (row.Cells[colGlassID.Name].Value != null)
                            {
                                if (((bool)row.Cells[colProcessFlag.Name].EditedFormattedValue) == true)
                                    i++;
                            }
                            else
                            {
                                row.Cells[colProcessFlag.Name].Value = false;
                            }
                        }
                        txtProcessQty.Text = i.ToString();
                        #endregion
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvProduct_DataError(object sender, DataGridViewDataErrorEventArgs anError)
        {
            try
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, anError.Exception.ToString());
                MessageBox.Show("dgvProduct Error happened " + anError.Context.ToString());

                if (anError.Context == DataGridViewDataErrorContexts.Commit)
                {
                    MessageBox.Show("Commit error");
                }
                if (anError.Context == DataGridViewDataErrorContexts.CurrentCellChange)
                {
                    MessageBox.Show("Cell change");
                }
                if (anError.Context == DataGridViewDataErrorContexts.Parsing)
                {
                    MessageBox.Show("parsing error");
                }
                if (anError.Context == DataGridViewDataErrorContexts.LeaveControl)
                {
                    MessageBox.Show("leave control error");
                }

                if ((anError.Exception) is ConstraintException)
                {
                    DataGridView view = (DataGridView)sender;
                    view.Rows[anError.RowIndex].ErrorText = "an error";
                    view.Rows[anError.RowIndex].Cells[anError.ColumnIndex].ErrorText = "an error";

                    anError.ThrowException = false;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnRecipe_Click(object sender, EventArgs e)
        {
            try
            {
                //讀取 DB PPID 資料
                if (dicRecipe_All.Count == 0) LoadPPIDSetting();

                FormCassetteControl_RecipeChange frm = new FormCassetteControl_RecipeChange(dicRecipe_All, MesControlMode);
                frm.DgvRows = dgvProduct.Rows;

                //多個Lot才需要傳送LstLots
                if (LstLots.Count > 1) frm.LstLots = LstLots;

                frm.ShowDialog();

                if (frm != null) frm.Dispose();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnLotRecipeID_Click(object sender, EventArgs e)
        {
            try
            {
                if (dicRecipe_All.Count == 0) LoadPPIDSetting();

                FormCassetteControl_RecipeChoose _frm = new FormCassetteControl_RecipeChoose(dicRecipe_All);

                if (System.Windows.Forms.DialogResult.OK == _frm.ShowDialog())
                {
                    txtLotRecipeID.Text = _frm.RecipeID;
                    txtLotRecipeID.Tag = _frm.RecipeName;
                }

                if (_frm != null) _frm.Dispose();

                //FormCassetteControl_CuttingRecipe frm = new FormCassetteControl_CuttingRecipe(MesControlMode, dicRecipe_All, curPort);

                //frm.CurrentLinePPID = txtCurrentLinePPID.Text.ToString();
                ////frm.CrossLinePPID = txtCrossLinePPID.Text.ToString();

                //#region cassette setting code
                //frm.LotCstSettingCode.Data = txtCassetteSettingCode.Text.ToString();
                //frm.ProcCstSettingCode.Data = txtCassetteSettingCode_CUT.Text.ToString();
                ////frm.STBCstSettingCode.Data = txtSTBCassetteSettingCode.Text.ToString();
                ////frm.CrossProcCstSettingCode.Data = txtCrossCassetteSettingCode.Text.ToString();
                ////frm.CrossSTBCstSettingCode.Data = txtSTBCassetteSettingCode.Text.ToString();

                //frm.LotCstSettingCode.ReadOnly = txtCassetteSettingCode.Tag.ToString() != string.Empty ? true : false;
                //frm.ProcCstSettingCode.ReadOnly = txtCassetteSettingCode_CUT.Tag.ToString() != string.Empty ? true : false;
                ////frm.STBCstSettingCode.ReadOnly = txtSTBCassetteSettingCode.Tag.ToString() != string.Empty ? true : false;
                ////frm.CrossProcCstSettingCode.ReadOnly = txtCrossCassetteSettingCode.Tag.ToString() != string.Empty ? true : false;
                ////frm.CrossSTBCstSettingCode.ReadOnly = txtSTBCassetteSettingCode.Tag.ToString() != string.Empty ? true : false;
                //#endregion

                //#region product id
                //frm.LotProductID.Data = txtProductID.Text.ToString();
                //frm.ProcProductID.Data = txtProductID_CUT.Text.ToString();
                ////frm.STBProductID.Data = txtProductID_STB.Text.ToString();
                ////frm.CrossProcProductID.Data = txtProductID_Cross.Text.ToString();
                ////frm.CrossSTBProductID.Data = txtProductID_STB.Text.ToString();

                //frm.LotProductID.ReadOnly = txtProductID.Tag.ToString() != string.Empty ? true : false;
                //frm.ProcProductID.ReadOnly = txtProductID_CUT.Tag.ToString() != string.Empty ? true : false;
                ////frm.STBProductID.ReadOnly = txtProductID_STB.Tag.ToString() != string.Empty ? true : false;
                ////frm.CrossProcProductID.ReadOnly = txtProductID_Cross.Tag.ToString() != string.Empty ? true : false;
                ////frm.CrossSTBProductID.ReadOnly = txtProductID_STB.Tag.ToString() != string.Empty ? true : false;
                //#endregion

                //#region product type
                //frm.LotProductType.Data = txtProductType.Text.ToString();
                //frm.ProcProductType.Data = txtProductType_CUT.Text.ToString();
                ////frm.STBProductType.Data = txtProductType_STB.Text.ToString();
                ////frm.CrossProcProductType.Data = txtProductType_Cross.Text.ToString();
                ////frm.CrossSTBProductType.Data = txtProductType_STB.Text.ToString();

                //frm.LotProductType.ReadOnly = txtProductType.Tag.ToString() != string.Empty ? true : false;
                //frm.ProcProductType.ReadOnly = txtProductType_CUT.Tag.ToString() != string.Empty ? true : false;
                ////frm.STBProductType.ReadOnly = txtProductType_STB.Tag.ToString() != string.Empty ? true : false;
                ////frm.CrossProcProductType.ReadOnly = txtProductType_Cross.Tag.ToString() != string.Empty ? true : false;
                ////frm.CrossSTBProductType.ReadOnly = txtProductType_STB.Tag.ToString() != string.Empty ? true : false;
                //#endregion

                //#region recipe
                //frm.Recipe_Lot_Cutting = Recipe_Lot_Cutting;
                //frm.Recipe_Proc_Cutting = Recipe_Proc_Cutting;
                ////frm.Recipe_STB_Cutting = Recipe_STB_Cutting;
                ////frm.Recipe_CrossProc_Cutting = Recipe_CrossProc_Cutting;
                ////frm.Recipe_CrossSTB_Cutting = Recipe_CrossSTB_Cutting;
                //#endregion

                //if (DialogResult.OK == frm.ShowDialog())
                //{
                //    //dicRecipe_Cross = frm.dicRecipe_Cross;

                //    txtCurrentLinePPID.Text = frm.CurrentLinePPID;
                //    //txtCrossLinePPID.Text = frm.CrossLinePPID;

                //    txtCassetteSettingCode.Text = frm.LotCstSettingCode.Data;
                //    txtCassetteSettingCode_CUT.Text = frm.ProcCstSettingCode.Data;
                //    //txtSTBCassetteSettingCode.Text = FormMainMDI.G_OPIAp.CurLine.LineType == "CBCUT_3" ? frm.STBCstSettingCode.Data : frm.CrossSTBCstSettingCode.Data;
                //    //txtCrossCassetteSettingCode.Text = frm.CrossProcCstSettingCode.Data;

                //    txtProductID.Text = frm.LotProductID.Data;
                //    txtProductID_CUT.Text = frm.ProcProductID.Data;
                //    //txtProductID_STB.Text = FormMainMDI.G_OPIAp.CurLine.LineType == "CBCUT_3" ? frm.STBProductID.Data : frm.CrossSTBProductID.Data;
                //    //txtProductID_Cross.Text = frm.CrossProcProductID.Data;

                //    txtProductType.Text = frm.LotProductType.Data;
                //    txtProductType_CUT.Text = frm.ProcProductType.Data;
                //    //txtProductType_STB.Text = FormMainMDI.G_OPIAp.CurLine.LineType == "CBCUT_3" ? frm.STBProductType.Data : frm.CrossSTBProductType.Data;
                //    //txtProductType_Cross.Text = frm.CrossProcProductType.Data;

                //    Recipe_Lot_Cutting = frm.Recipe_Lot_Cutting;
                //    Recipe_Proc_Cutting = frm.Recipe_Proc_Cutting;
                //    //Recipe_STB_Cutting = frm.Recipe_STB_Cutting;
                //    //Recipe_CrossProc_Cutting = frm.Recipe_CrossProc_Cutting;
                //    //Recipe_CrossSTB_Cutting = frm.Recipe_CrossSTB_Cutting;
                //}

                //if (frm != null) frm.Dispose();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            try
            {
                int _slotNo = 0;
                int _intFrSlotNo = 0;
                int _intToSlotNo = 0;
                string _msg = string.Empty;

                #region 取得slot no
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

                int.TryParse(txtSelFrSlotNo.Text.ToString(), out _intFrSlotNo);
                int.TryParse(txtSelToSlotNo.Text.ToString(), out _intToSlotNo);
                if (_intToSlotNo < _intFrSlotNo)
                {
                    ShowMessage(this, lblCaption.Text , "", "The start Slot No must be less than the end Slot No!! ", MessageBoxIcon.Error);
                    return;
                }
                #endregion

                string _recipeName = txtRecipeID.Text;

                if (!dicRecipe_All.ContainsKey(_recipeName))
                {
                    ShowMessage(this, lblCaption.Text , string.Empty, string.Format("Recipe Name [{0}] is not exist !", _recipeName), MessageBoxIcon.Warning);
                    txtRecipeID.Focus();
                    return;
                }

                #region 更新至dataGridView
                foreach (DataGridViewRow dr in dgvProduct.Rows)
                {
                    _slotNo = 0;

                    int.TryParse(dr.Cells["colSlotNo"].Value.ToString(), out _slotNo);

                    if (_slotNo < _intFrSlotNo) break;

                    if (_slotNo >= _intFrSlotNo && _slotNo <= _intToSlotNo)
                    {
                        //ARRAY Line 都要可以設定recipe
                        if (FormMainMDI.G_OPIAp.CurLine.FabType != "ARRAY")
                        {
                            #region
                            //BC在 Local 模式下只允许更改recipe 和 SAMPLING FLAG
                            //但如果MES download 的是以下产品信息则不能做修改
                            //OWNERTYPE: P || OWNERID: RESD
                            if (dr.Cells[colOwnerType.Name].Value.ToString() == "P" || dr.Cells[colOwnerID.Name].Value.ToString() == "RESD")
                            {
                                _msg = _msg + string.Format("\r\n Slot [{0}] ", _slotNo);
                                continue;
                            }
                            #endregion

                            if ((bool)dr.Cells[colProcessFlag.Name].Value == false) continue;
                        }

                        dr.Cells[colRecipeID.Name].Value = _recipeName;
                    }
                }
                #endregion

                #region 針對多lot的 TRX 更新 recipe
                if (LstLots.Count > 1)
                {
                    foreach (LocalCassetteDataReply.LOTDATAc _lot in LstLots)
                    {
                        foreach (LocalCassetteDataReply.PRODUCTDATAc _product in _lot.PRODUCTLIST)
                        {
                            _slotNo = 0;

                            int.TryParse(_product.SLOTNO, out _slotNo);

                            if (_slotNo >= _intFrSlotNo && _slotNo <= _intToSlotNo)
                            {
                                //ARRAY Line 都要可以設定recipe
                                if (FormMainMDI.G_OPIAp.CurLine.FabType != "ARRAY")
                                {
                                    #region OWNERTYPE: P || OWNERID: RESD 不允許修改
                                    //BC在 Local 模式下只允许更改recipe 和 SAMPLING FLAG
                                    //但如果MES download 的是以下产品信息则不能做修改
                                    //OWNERTYPE: P || OWNERID: RESD
                                    if (_product.OWNERTYPE == "P" || _product.OWNERID == "RESD")
                                    {
                                        _msg = _msg + string.Format("\r\n Slot [{0}] ", _slotNo);
                                        continue;
                                    }
                                    #endregion

                                    if (_product.PROCESSFLAG == "N") continue;
                                }

                                _product.PRODUCTRECIPENAME = _recipeName;

                                if (dicRecipe_All.ContainsKey(_recipeName))
                                {
                                    _product.PPID = dicRecipe_All[_recipeName].PPID;
                                }
                                else
                                {
                                    _product.PPID = string.Empty;
                                }
                            }
                        }                        
                    }
                }
                
                #endregion

                if (_msg != string.Empty)
                {
                    _msg = "Recipe can't be modified when OWNERTYPE is P or OWNERID is RESD. " + _msg;
                    ShowMessage(this, lblCaption.Text, string.Empty, _msg, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnRecipeCheck_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvProduct.Rows.Count <= 0) return;

                string _key = string.Empty;
                string _err = string.Empty;

                dicRecipeUse.Clear();

                if (flpLotRecipeID.Visible)
                {
                    #region Lot Recipe Chech

                    #region 判斷Lot PPID
                    if (txtLotRecipeID.Text == string.Empty || txtLotRecipeID.Tag == null)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please Choose Recipe Name", MessageBoxIcon.Error);

                        return;
                    }
                    #endregion

                    _key = txtLotRecipeID.Tag.ToString();

                    if (!dicRecipe_All.ContainsKey(_key))
                    {
                        _err = string.Format("{0} is not exist", _key);

                        ShowMessage(this, lblCaption.Text , "", _err, MessageBoxIcon.Error);

                        return;
                    }

                    dicRecipeUse.Add(_key, dicRecipe_All[_key]);

                    #endregion
                }
                else
                {
                    #region 取得有使用的recipe
                    foreach (DataGridViewRow _row in dgvProduct.Rows)
                    {
                        if (!(bool)_row.Cells["colProcessFlag"].Value) continue;

                        _key = _row.Cells["colRecipeID"].Value.ToString();

                        if (!dicRecipe_All.ContainsKey(_key))
                        {
                            _err = string.Format("{0} is not exist", _key);

                            ShowMessage(this, lblCaption.Text , "", _err, MessageBoxIcon.Error);

                            return;
                        }

                        //已經存在於被使用的recipe check dic內，不需要再新增
                        if (dicRecipeUse.ContainsKey(_key))
                        {
                            continue;
                        }

                        dicRecipeUse.Add(_key, dicRecipe_All[_key]);
                    }
                    #endregion
                }

                #region Recipe Check
                FormCassetteControl_RecipeCheck _frm = new FormCassetteControl_RecipeCheck(dicRecipeUse);

                if (DialogResult.OK == _frm.ShowDialog())
                {

                    RecipeRegisterValidationCommandRequest _trx = new RecipeRegisterValidationCommandRequest();
                    _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    _trx.BODY.PORTNO = curPort.PortNo;

                    #region 本line recipe

                    List<string> _choose = _frm.chooseRecipe;

                    if (_choose.Count > 0)
                    {
                        RecipeRegisterValidationCommandRequest.LINEc _line = new RecipeRegisterValidationCommandRequest.LINEc();
                        _line.RECIPELINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                        foreach (string _recipeName in _choose)
                        {
                            RecipeRegisterValidationCommandRequest.RECIPECHECKc _recipeCheck = new RecipeRegisterValidationCommandRequest.RECIPECHECKc();
                            _recipeCheck.RECIPENAME = _recipeName;

                            foreach (RecipeCheckEQ _checkEQ in dicRecipeUse[_recipeName].LocalRecipeCheck)
                            {
                                if (_checkEQ.NeedRecipeCheck)
                                {
                                    RecipeRegisterValidationCommandRequest.EQUIPMENTc _eqc = new RecipeRegisterValidationCommandRequest.EQUIPMENTc();
                                    _eqc.EQUIPMENTNO = _checkEQ.LocalNo;
                                    _eqc.RECIPENO = _checkEQ.RecipeNo;

                                    _recipeCheck.EQUIPMENTLIST.Add(_eqc);
                                }
                            }

                            _line.RECIPECHECKLIST.Add(_recipeCheck);
                        }

                        _trx.BODY.LINELIST.Add(_line);
                    }
                    #endregion

                    #region 跨line recipe -CBCUT400 跨 CBCUT500
                    //_choose = _frm.chooseRecipe_Cross;

                    //if (_choose.Count > 0)
                    //{
                    //    RecipeRegisterValidationCommandRequest.LINEc _crossLine = new RecipeRegisterValidationCommandRequest.LINEc();
                    //    _crossLine.RECIPELINENAME = FormMainMDI.G_OPIAp.CrossServerName_CUT;

                    //    foreach (string _recipeName in _choose)
                    //    {
                    //        RecipeRegisterValidationCommandRequest.RECIPECHECKc _recipeCheck = new RecipeRegisterValidationCommandRequest.RECIPECHECKc();
                    //        _recipeCheck.RECIPENAME = _recipeName;

                    //        foreach (RecipeCheckEQ _checkEQ in dicRecipeUse_Cross[_recipeName].LocalRecipeCheck)
                    //        {
                    //            if (_checkEQ.NeedRecipeCheck)
                    //            {
                    //                RecipeRegisterValidationCommandRequest.EQUIPMENTc _eqc = new RecipeRegisterValidationCommandRequest.EQUIPMENTc();
                    //                _eqc.EQUIPMENTNO = _checkEQ.LocalNo;
                    //                _eqc.RECIPENO = _checkEQ.RecipeNo;

                    //                _recipeCheck.EQUIPMENTLIST.Add(_eqc);
                    //            }
                    //        }

                    //        _crossLine.RECIPECHECKLIST.Add(_recipeCheck);
                    //    }

                    //    _trx.BODY.LINELIST.Add(_crossLine);
                    //}
                    #endregion

                    if (_trx.BODY.LINELIST.Count <= 0) return;

                    string _xml = _trx.WriteToXml();

                    MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, FormMainMDI.G_OPIAp.SocketResponseTime_Query);

                    if (_resp != null)
                    {
                        #region RecipeRegisterValidationCommandReply
                        ShowMessage(this, lblCaption.Text, "", "Send Recipe Register Validation Command to BC Sucess", MessageBoxIcon.Information);
                        #endregion
                    }    
                }
                #endregion

                if (_frm != null) _frm.Dispose();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnMapDownload_Click(object sender, EventArgs e)
        {
            try
            {
                Button _btn = (Button)sender;
                string _msg = string.Empty;
                string _recipeMsg = string.Empty;

                string _recipeName = string.Empty;
                string _ppid = string.Empty;
                string _slotNo = string.Empty;


                #region PORT 層Cassette Setting Code不管是否空CST / LD UD都需要填 -- CELL
                if (pnlCassetteSettingCode_Port.Visible)
                {
                    if (txtCassetteSettingCode_Port.Text.ToString().Trim() == string.Empty)
                    {
                        ShowMessage(this, lblCaption.Text, "", string.Format("Please input Cassette Setting Code for port [{0}]", curPort.PortID), MessageBoxIcon.Error);
                        txtCassetteSettingCode_Port.Focus();
                        return;
                    }
                }
                #endregion

                //讀取 DB PPID 資料
                if (dicRecipe_All.Count == 0) LoadPPIDSetting();

                SetLotData();  //當下修改的lot data也需要更新至LstLots內

                if (flpLotRecipeID.Visible)
                {
                    #region 判斷Lot PPID
                    if (txtLotRecipeID.Text == string.Empty || txtLotRecipeID.Tag == null)
                    {
                        _msg = "Please Choose Recipe Name";

                        ShowMessage(this, lblCaption.Text, "", _msg, MessageBoxIcon.Error);

                        return;
                    }
                    #endregion
                }

                if (pnlSetting.Visible == true)
                {
                    #region  判斷 & Get slot PPID
                    foreach (LocalCassetteDataReply.LOTDATAc _lot in LstLots)
                    {
                        foreach (LocalCassetteDataReply.PRODUCTDATAc _product in _lot.PRODUCTLIST)
                        {
                            _recipeName = _product.PRODUCTRECIPENAME;
                            _ppid = _product.PPID;

                            //被修改的recipe name才需要重取ppid
                            if (_ppid != string.Empty) continue;

                            if (dicRecipe_All.ContainsKey(_recipeName))
                            {
                                _ppid = dicRecipe_All[_recipeName].PPID;
                            }
                            else
                            {
                                if (_product.PROCESSFLAG == "N") _ppid = string.Empty;
                                else
                                {
                                    _msg = string.Format("{0} is not exist", _recipeName.ToString());

                                    ShowMessage(this, lblCaption.Text , "", _msg, MessageBoxIcon.Error);

                                    return;
                                }
                            }
                            _product.PPID = _ppid;
                        }
                    }
                    #endregion
                }

                #region Cell_Local_Mode PPID Check
                //20161128 huangjiayin: For Cell Shop, do not support Slot Recipe
                _ppid = string.Empty;
                if (FormMainMDI.G_OPIAp.CurLine.FabType.ToUpper() == "CELL")
                {
                    foreach (LocalCassetteDataReply.LOTDATAc _lot in LstLots)
                    {
                        foreach (LocalCassetteDataReply.PRODUCTDATAc _product in _lot.PRODUCTLIST)
                        {
                            if (string.IsNullOrEmpty(_product.PRODUCTRECIPENAME)) continue;
                            else if (string.IsNullOrEmpty(_ppid))
                            {
                                _ppid = _product.PRODUCTRECIPENAME;
                            }
                            else
                            {
                                if (_ppid != _product.PRODUCTRECIPENAME)
                                {
                                    _msg = "For Cell Shop: 1 CST Can Only Process 1 PPID, Please Confirm it!";
                                    _msg += "\r\n Slot:" + _product.SLOTNO + " RecipeName is Different![" + _product.PRODUCTRECIPENAME+"] from ["
                                        + _ppid+"]";
                                    ShowMessage(this, lblCaption.Text, "", _msg, MessageBoxIcon.Error);
                                    return;
                                }
 
                            }
 
                        }
                    }
                }
                #endregion


                _msg = string.Format("Please confirm whether you will process the Cassette Command [{0}] ?", _btn.Text.ToString());
                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, _msg)) return;
                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text )) return;

                SendtoBC_LocalModeCassetteDataSend();

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void txtRecipeID_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                FormCassetteControl_RecipeChoose frm = new FormCassetteControl_RecipeChoose(dicRecipe_All);

                if (System.Windows.Forms.DialogResult.OK == frm.ShowDialog())
                {
                    txtRecipeID.Text = frm.RecipeName;
                }

                if (frm != null) frm.Dispose();
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

        private void txtSelToSlotNo_KeyPress(object sender, KeyPressEventArgs e)
        {
            UniTools.CheckTextBoxKeyPressIsInteger(sender, e);
        }

        private void lblLot_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                RadioButton _rdo = (RadioButton)sender;

                if (_rdo.Checked)
                {
                    LocalCassetteDataReply.LOTDATAc _lotData = LstLots.Find(d => d.LOTNAME == _rdo.Name);

                    //把目前已修改的資訊更新至lot data
                    if (CurLotData != null) SetLotData();

                    ShowLotData(_lotData);

                    CurLotData = _lotData;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void ShowLotData(LocalCassetteDataReply.LOTDATAc lotData)
        {
            try
            {
                bool _processFlag = false;
                string _productPPID = string.Empty;
                string _crossPPID = string.Empty;

                #region Lot
                txtLotID.Text = lotData.LOTNAME;
                txtProcOperName.Text = lotData.PROCESSOPERATIONNAME;
                txtProdOwner.Text = lotData.PRODUCTOWNER;
                txtProdSpecName.Text = lotData.PRODUCTSPECNAME;

                txtReworkMaxCount.Text = lotData.CFREWORKCOUNT;
                txtTargetCSTID_CF.Text = lotData.TARGETCSTID_CF;

                #region product id
                txtProductID.Text = lotData.PRODUCTID;
                txtProductID.Tag = lotData.PRODUCTID;
                #endregion

                #region product type
                txtProductType.Text = lotData.BCPRODUCTTYPE;
                txtProductType.Tag = lotData.BCPRODUCTTYPE;
                #endregion

                #region cassette setting code
                txtCassetteSettingCode.Text = lotData.CSTSETTINGCODE;
                txtCassetteSettingCode.Tag = lotData.CSTSETTINGCODE;
                #endregion

                #region product id - CUT
                txtProductID_CUT.Text = lotData.PRODUCTID_CUT;
                txtProductID_CUT.Tag = lotData.PRODUCTID_CUT;
                #endregion

                #region product type - CUT
                txtProductType_CUT.Text = lotData.BCPRODUCTTYPE_CUT;
                txtProductType_CUT.Tag = lotData.BCPRODUCTTYPE_CUT;
                #endregion

                #region cassette setting code - CUT
                txtCassetteSettingCode_CUT.Text = lotData.CSTSETTINGCODE_CUT;
                txtCassetteSettingCode_CUT.Tag = lotData.CSTSETTINGCODE_CUT;
                #endregion

                #region Lot Recipe 
                txtLotRecipeID.Text = lotData.PPID;
                txtLotRecipeID.Tag = lotData.LINERECIPENAME;
                #endregion

                //LineType == "CUT" 
                //if (OPIConst.LstLineType_Cutting.Contains(FormMainMDI.G_OPIAp.CurLine.LineType))
                //{
                    //// Lot Recipe
                    //Recipe_Lot_Cutting = new Recipe(FormMainMDI.G_OPIAp.CurLine.ServerName, lotData.LINERECIPENAME, lotData.PPID);

                    //#region Process Line List --- 最多兩組,一組本Line & 被跨Line的
                    //foreach (LocalCassetteDataReply.PROCESSLINEc _product in lotData.PROCESSLINELIST)
                    //{
                    //    if (_product.LINENAME == FormMainMDI.G_OPIAp.CurLine.ServerName)
                    //    {
                    //        Recipe_Proc_Cutting = new Recipe(FormMainMDI.G_OPIAp.CurLine.ServerName, _product.LINERECIPENAME, _product.PPID);
                    //        txtCassetteSettingCode_CUT.Text = _product.CSTSETTINGCODE;
                    //        txtCassetteSettingCode_CUT.Tag = _product.CSTSETTINGCODE;

                    //        txtProductID_CUT.Text = _product.PRODUCTID;
                    //        txtProductID_CUT.Tag = _product.PRODUCTID;

                    //        txtProductType_CUT.Text = _product.BCPRODUCTTYPE;
                    //        txtProductType_CUT.Tag = _product.BCPRODUCTTYPE;
                    //    }

                    //    //if (FormMainMDI.G_OPIAp.CurLine.ServerName != _product.LINENAME )
                    //    //{
                    //    //    Recipe_CrossProc_Cutting = new Recipe(_product.LINENAME, _product.LINERECIPENAME, _product.PPID, "PROCESSLINE");
                    //    //    txtCrossCassetteSettingCode.Text = _product.CSTSETTINGCODE;
                    //    //    txtCrossCassetteSettingCode.Tag = _product.CSTSETTINGCODE;

                    //    //    txtProductID_Cross.Text = _product.PRODUCTID;
                    //    //    txtProductID_Cross.Tag = _product.PRODUCTID;

                    //    //    txtProductType_Cross.Text = _product.BCPRODUCTTYPE;
                    //    //    txtProductType_Cross.Tag = _product.BCPRODUCTTYPE;
                    //    //}
                    //}
                    //#endregion

                    //#region STB Prod Spec List --- STB 只能有一組 針對CUT 跨Line - for CBCUT_2(CBCUT400)跨CBCUT_3(CBCUT500) 或 CBCUT500本身
                    ////foreach (LocalCassetteDataReply.STBPRODUCTSPECc _stb in lotData.STBPRODUCTSPECLIST)
                    ////{
                    ////    if (FormMainMDI.G_OPIAp.CurLine.LineType == "CBCUT_2" && _stb.LINENAME == "CBPOL400")
                    ////    {
                    ////        Recipe_CrossSTB_Cutting = new Recipe("CBPOL400", _stb.LINERECIPENAME, _stb.PPID, "STB");
                    ////        txtSTBCassetteSettingCode.Text = _stb.CSTSETTINGCODE;
                    ////        txtSTBCassetteSettingCode.Tag = _stb.CSTSETTINGCODE;

                    ////        txtProductID_STB.Text = _stb.PRODUCTID;
                    ////        txtProductID_STB.Tag = _stb.PRODUCTID;

                    ////        txtProductType_STB.Text = _stb.BCPRODUCTTYPE;
                    ////        txtProductType_STB.Tag = _stb.BCPRODUCTTYPE;
                    ////        break;
                    ////    }

                    ////    if (FormMainMDI.G_OPIAp.CurLine.LineType == "CBCUT_3" && _stb.LINENAME == "CBPOL400")
                    ////    {
                    ////        Recipe_STB_Cutting = new Recipe("CBPOL400", _stb.LINERECIPENAME, _stb.PPID, "STB");
                    ////        txtSTBCassetteSettingCode.Text = _stb.CSTSETTINGCODE;
                    ////        txtSTBCassetteSettingCode.Tag = _stb.CSTSETTINGCODE;

                    ////        txtProductID_STB.Text = _stb.PRODUCTID;
                    ////        txtProductID_STB.Tag = _stb.PRODUCTID;

                    ////        txtProductType_STB.Text = _stb.BCPRODUCTTYPE;
                    ////        txtProductType_STB.Tag = _stb.BCPRODUCTTYPE;
                    ////        break;
                    ////    }
                    ////}
                    //#endregion

                    //#region Current Line PPID
                    //RecipeCheckEQ _recipe = null;
                    //string _L2RecipeNo ="00";
                    //string _L3RecipeNo = "00";
                    //string _L4RecipeNo = "00";
                    //string _L5RecipeNo = "00";
                    //string _L6RecipeNo = "00";
                    //string _L7RecipeNo = "00";
                    //string _L8RecipeNo = "00";
                    ////string _L9RecipeNo = "00";
                    ////string _L10RecipeNo = "00";
                    ////string _L11RecipeNo = "00";
                    ////string _L12RecipeNo = "00";
                    ////string _L13RecipeNo = "00";
                    ////string _L14RecipeNo = "00";
                    ////string _L15RecipeNo = "00";

                    //#region Get Recipe No
                    //if (Recipe_Lot_Cutting != null)
                    //{
                    //    _recipe = Recipe_Lot_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L2"));
                    //    if (_recipe != null) _L2RecipeNo = _recipe.RecipeNo;

                    //    _recipe = Recipe_Lot_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L3"));
                    //    if (_recipe != null) _L3RecipeNo = _recipe.RecipeNo;
                    //}

                    //if (Recipe_Proc_Cutting != null)
                    //{
                    //    _recipe = Recipe_Proc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L4"));
                    //    if (_recipe != null) _L4RecipeNo = _recipe.RecipeNo;

                    //    _recipe = Recipe_Proc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L5"));
                    //    if (_recipe != null) _L5RecipeNo = _recipe.RecipeNo;

                    //    _recipe = Recipe_Proc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L6"));
                    //    if (_recipe != null) _L6RecipeNo = _recipe.RecipeNo;

                    //    _recipe = Recipe_Proc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L7"));
                    //    if (_recipe != null) _L7RecipeNo = _recipe.RecipeNo;

                    //    _recipe = Recipe_Proc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L8"));
                    //    if (_recipe != null) _L8RecipeNo = _recipe.RecipeNo;
                    //}

                    //_productPPID = string.Format("L2:{0};L3:{1};L4:{2};L5:{3};L6:{4};L7:{5};L8:{6}",
                    //                _L2RecipeNo, _L3RecipeNo, _L4RecipeNo, _L5RecipeNo, _L6RecipeNo, _L7RecipeNo, _L8RecipeNo); 

                    //#endregion

                    ////if (FormMainMDI.G_OPIAp.CurLine.LineType == "CBCUT_3")
                    ////{
                    ////    #region Get Recipe No
                    ////    if (Recipe_Proc_Cutting != null)
                    ////    {
                    ////        _recipe = Recipe_Proc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L9"));
                    ////        if (_recipe != null) _L9RecipeNo = _recipe.RecipeNo;

                    ////        _recipe = Recipe_Proc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L10"));
                    ////        if (_recipe != null) _L10RecipeNo = _recipe.RecipeNo;
                    ////    }

                    ////    if (Recipe_STB_Cutting != null)
                    ////    {
                    ////        _recipe = Recipe_STB_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L11"));
                    ////        if (_recipe != null) _L11RecipeNo = _recipe.RecipeNo;

                    ////        _recipe = Recipe_STB_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L12"));
                    ////        if (_recipe != null) _L12RecipeNo = _recipe.RecipeNo;

                    ////        _recipe = Recipe_STB_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L13"));
                    ////        if (_recipe != null) _L13RecipeNo = _recipe.RecipeNo;

                    ////        _recipe = Recipe_STB_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L14"));
                    ////        if (_recipe != null) _L14RecipeNo = _recipe.RecipeNo;

                    ////        _recipe = Recipe_STB_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L15"));
                    ////        if (_recipe != null) _L15RecipeNo = _recipe.RecipeNo;
                    ////    }

                    ////    _productPPID = _productPPID + string.Format(";L9:{0};L10:{1};L11:{2};L12:{3};L13:{4};L14:{5};L15:{6}",
                    ////                                    _L9RecipeNo, _L10RecipeNo, _L11RecipeNo, _L12RecipeNo, _L13RecipeNo, _L14RecipeNo, _L15RecipeNo); 


                    ////    #endregion
                    ////}
                    //txtCurrentLinePPID.Text = _productPPID;

                    //#endregion

                    //#region Cross Line PPID
                    ////if (FormMainMDI.G_OPIAp.CurLine.LineType == "CBCUT_2")
                    ////{
                    ////    #region Get Recipe No
                    ////    _L2RecipeNo = "00";
                    ////    _L3RecipeNo = "00";
                    ////    _L4RecipeNo = "00";
                    ////    _L5RecipeNo = "00";
                    ////    _L6RecipeNo = "00";
                    ////    _L7RecipeNo = "00";
                    ////    _L8RecipeNo = "00";
                    ////    _L9RecipeNo = "00";
                    ////    _L10RecipeNo = "00";
                    ////    _L11RecipeNo = "00";
                    ////    _L12RecipeNo = "00";
                    ////    _L13RecipeNo = "00";
                    ////    _L14RecipeNo = "00";
                    ////    _L15RecipeNo = "00";

                    ////    if (Recipe_CrossProc_Cutting != null)
                    ////    {
                    ////        _recipe = Recipe_CrossProc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L2"));
                    ////        if (_recipe != null) _L2RecipeNo = _recipe.RecipeNo;

                    ////        _recipe = Recipe_CrossProc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L3"));
                    ////        if (_recipe != null) _L3RecipeNo = _recipe.RecipeNo;

                    ////        _recipe = Recipe_CrossProc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L4"));
                    ////        if (_recipe != null) _L4RecipeNo = _recipe.RecipeNo;

                    ////        _recipe = Recipe_CrossProc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L5"));
                    ////        if (_recipe != null) _L5RecipeNo = _recipe.RecipeNo;

                    ////        _recipe = Recipe_CrossProc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L6"));
                    ////        if (_recipe != null) _L6RecipeNo = _recipe.RecipeNo;

                    ////        _recipe = Recipe_CrossProc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L7"));
                    ////        if (_recipe != null) _L7RecipeNo = _recipe.RecipeNo;

                    ////        _recipe = Recipe_CrossProc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L8"));
                    ////        if (_recipe != null) _L8RecipeNo = _recipe.RecipeNo;

                    ////        _recipe = Recipe_CrossProc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L9"));
                    ////        if (_recipe != null) _L9RecipeNo = _recipe.RecipeNo;

                    ////        _recipe = Recipe_CrossProc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L10"));
                    ////        if (_recipe != null) _L10RecipeNo = _recipe.RecipeNo;
                    ////    }

                    ////    if (Recipe_CrossSTB_Cutting != null)
                    ////    {
                    ////        _recipe = Recipe_CrossSTB_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L11"));
                    ////        if (_recipe != null) _L11RecipeNo = _recipe.RecipeNo;

                    ////        _recipe = Recipe_CrossSTB_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L12"));
                    ////        if (_recipe != null) _L12RecipeNo = _recipe.RecipeNo;

                    ////        _recipe = Recipe_CrossSTB_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L13"));
                    ////        if (_recipe != null) _L13RecipeNo = _recipe.RecipeNo;

                    ////        _recipe = Recipe_CrossSTB_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L14"));
                    ////        if (_recipe != null) _L14RecipeNo = _recipe.RecipeNo;

                    ////        _recipe = Recipe_CrossProc_Cutting.LocalRecipeCheck.Find(r => r.LocalNo.Equals("L15"));
                    ////        if (_recipe != null) _L15RecipeNo = _recipe.RecipeNo;
                    ////    }
                    ////    _crossPPID = string.Format("L2:{0};L3:{1};L4:{2};L5:{3};L6:{4};L7:{5};L8:{6};L9:{7};L10:{8};L11:{9};L12:{10};L13:{11};L14:{12};L15:{13}",
                    ////                                _L2RecipeNo, _L3RecipeNo, _L4RecipeNo, _L5RecipeNo, _L6RecipeNo, _L7RecipeNo, _L8RecipeNo, _L9RecipeNo, _L10RecipeNo,
                    ////                                _L11RecipeNo,_L12RecipeNo,_L13RecipeNo,_L14RecipeNo,_L15RecipeNo);

                    ////    #endregion

                    ////    txtCrossLinePPID.Text = _crossPPID;
                    ////}
                    //#endregion
                //}

                #endregion

                #region Product
                dgvProduct.Rows.Clear();

                foreach (LocalCassetteDataReply.PRODUCTDATAc product in lotData.PRODUCTLIST)
                {
                    _processFlag = product.PROCESSFLAG == "Y" ? true : false;

                    //colProcessFlag     , colProcessFlag_MES , colSlotNo         , colGlassID          , colRecipeID      
                    //colPPID            , colRecipeID_MES    , colJobType        , colJobGrade         , colJobJudge 
                    //colGroupIndex      , colProcessType      ,colTargetCSTID    , colNetworkNo     
                    //colOwnerID       , colOwnerType        ,colRevProcOperName  ,colInlineReworkCount
                    dgvProduct.Rows.Add(
                        _processFlag, _processFlag, product.SLOTNO.ToString(), product.PRODUCTNAME.ToString(), product.PRODUCTRECIPENAME.ToString(),
                        product.PPID.ToString(), product.PRODUCTRECIPENAME.ToString(), product.PRODUCTTYPE.ToString(), product.PRODUCTGRADE.ToString(), product.PRODUCTJUDGE.ToString(),
                        product.GROUPID.ToString(),   product.PROCESSTYPE.ToString(), product.TARGETCSTID.ToString(),  product.NETWORKNO.ToString(), 
                        product.OWNERID.ToString(), product.OWNERTYPE.ToString(), product.REVPROCESSOPERATIONNAME.ToString(),product.CFINLINEREWORKMAXCOUNT);


                    //BC在 Local 模式下只允许更改recipe 和 SAMPLING FLAG
                    //但如果MES download 的是以下产品信息则不能做修改
                    //OWNERTYPE: P || OWNERID: RESD
                    if (flpLotRecipeID.Visible)
                    {
                        if (product.OWNERTYPE.ToString() == "P" || product.OWNERID.ToString() == "RESD")
                        {
                            btnLotRecipeID.Enabled = false;
                        }
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

        private void SetLotData()
        {
            try
            {
                //local mode可能會有對應多個lot,須把資料回存至 LstLots 內,避免切換時資料遺失
                foreach (DataGridViewRow row in dgvProduct.Rows)
                {
                    LocalCassetteDataReply.PRODUCTDATAc _product = CurLotData.PRODUCTLIST.Find(d => d.SLOTNO == row.Cells["colSlotNo"].Value.ToString());

                    _product.PROCESSFLAG = (bool)row.Cells[colProcessFlag.Name].Value ? "Y" : "N";
                    _product.PRODUCTRECIPENAME = row.Cells[colRecipeID.Name].Value.ToString() == string.Empty ? string.Empty : row.Cells["colRecipeID"].Value.ToString();
                    _product.PPID = row.Cells[colPPID.Name].Value.ToString() == string.Empty ? string.Empty : row.Cells["colPPID"].Value.ToString();
                }
                
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void GetCSTControlDef()
        {
            try
            {
                #region 取得下貨畫面要顯示的物件資訊 (SBRM_CST_CONTROL_DEF)
                UniBCSDataContext ctx = FormMainMDI.G_OPIAp.DBCtx;
                List<SBRM_CST_CONTROL_DEF> _lstCstControl = (from row in ctx.SBRM_CST_CONTROL_DEF.Where(d =>
                    d.LINETYPE.Equals(FormMainMDI.G_OPIAp.CurLine.LineType) &&
                    (d.MESCONTROLMODE.Contains(MesControlMode)) &&
                    (d.PORTIDLIST.Contains(curPort.PortID) || d.PORTIDLIST.Equals("ALL")))
                                                             select row).ToList();

                #region 設定Port層物件
                var _portObj = _lstCstControl.FindAll(r => r.OBJECTLEVEL.Equals("Port"));
                foreach (Control _ctrl in flpCassetteInfo.Controls)
                {
                    if (_portObj.Find(r => r.OBJECTNAME.Equals(_ctrl.Name)) != null) _ctrl.Visible = true;
                    else _ctrl.Visible = false;
                }
                #endregion

                #region 設定Cassette層物件
                var _cassetteObj = _lstCstControl.FindAll(r => r.OBJECTLEVEL.Equals("Cassette"));
                foreach (Control _ctrl in flpLotInfo.Controls)
                {
                    if (_ctrl.Name == "flpLotList") continue;
                    if (_cassetteObj.Find(r => r.OBJECTNAME.Equals(_ctrl.Name)) != null) _ctrl.Visible = true;
                    else _ctrl.Visible = false;
                }
                #endregion

                #region 設定Slot層物件 -- dataGridView
                var _slotObj = _lstCstControl.FindAll(r => r.OBJECTLEVEL.Equals("Slot"));
                foreach (DataGridViewColumn _colName in dgvProduct.Columns)
                {
                    if (_slotObj.Find(r => r.OBJECTNAME.Equals(_colName.Name)) != null)
                    {
                        _colName.Visible = true;
                    }
                    else
                    {
                        _colName.Visible = false;
                    }
                }
                #endregion

                //local mode不提供recipe check --暫時mark
                btnRecipeCheck.Visible = false;
                
                #endregion

                if (OPIConst.LstLineType_Cutting.Contains(FormMainMDI.G_OPIAp.CurLine.LineType))
                {
                    // Cutting 無須slot recipe setting button 設定按鈕
                    btnRecipe.Visible = false;

                    txtCassetteSettingCode.ReadOnly = true;
                    txtProductType.ReadOnly = true;
                    txtProductID.ReadOnly = true;

                    #region 判斷是否有跨line設定,非L2上port的不需要選跨Line recipe
                    //if (FormMainMDI.G_OPIAp.CrossServerName_CUT != string.Empty && curPort.NodeNo == "L2")
                    //{
                    //    //IsCrossUse = true;
                    //    pnlCrossCassetteSettingCode.Visible = true;
                    //    pnlProductID_Cross.Visible = true;
                    //    pnlProductType_Cross.Visible = true;
                    //    lblCrossLinePPID.Visible = true;
                    //    txtCrossLinePPID.Visible = true;
                    //}
                    //else
                    //{
                    //    pnlCrossCassetteSettingCode.Visible = false;
                    //    pnlProductID_Cross.Visible = false;
                    //    pnlProductType_Cross.Visible = false;
                    //    lblCrossLinePPID.Visible = false;
                    //    txtCrossLinePPID.Visible = false;
                    //}
                    #endregion

                }
                else btnRecipe.Visible = true;

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void InitialFormInfo()
        {
            try
            {
                #region 清除基本畫面

                #region CASSETTE
                txtCassetteID.Text = string.Empty;
                txtProdQty.Text = "0";
                txtProcessQty.Text = "0";
                txtCassetteSettingCode_Port.Text = string.Empty;

                txtCassetteID.ReadOnly = true;
                txtProdQty.ReadOnly = true;
                txtProcessQty.ReadOnly = true; ;
                #endregion

                #region LOT
                foreach (Panel _pnl in flpLotInfo.Controls.OfType<Panel>())
                {
                    foreach (TextBox _txt in _pnl.Controls.OfType<TextBox>())
                    {
                        _txt.Text = string.Empty;
                        _txt.Tag = string.Empty;
                        _txt.ReadOnly = true;
                    }
                }
                #endregion

                #region Slot
                txtSelFrSlotNo.Text = string.Empty;
                txtSelToSlotNo.Text = string.Empty;
                dgvProduct.Rows.Clear();
                #endregion

                btnMapDownload.Enabled = false;
                btnRecipeCheck.Enabled = false;
                #endregion

                #region 僅開放process flag && recipe 可修改 --BFG line僅可修改recipe
                foreach (DataGridViewColumn _col in dgvProduct.Columns)
                {
                    if (_col.Name == "colRecipeID") _col.ReadOnly = false;
                    else if (_col.Name == "colProcessFlag")
                    {
                        if (FormMainMDI.G_OPIAp.CurLine.LineType == "BFG_SHUZTUNG") _col.ReadOnly = true;
                         else _col.ReadOnly = false;
                    }
                    else _col.ReadOnly = true;
                }
                #endregion
                
                //if (OPIConst.LstLineType_Cutting.Contains(FormMainMDI.G_OPIAp.CurLine.LineType))
                if (colRecipeID.Visible == false)
                {
                    pnlSetting.Visible = false;
                    tlpBase.RowStyles[3].Height = 0; 
                    tlpBase.RowStyles[2].Height = 160;
                }
                else
                {
                    pnlSetting.Visible = true;
                    tlpBase.RowStyles[3].Height = 40;
                    tlpBase.RowStyles[2].Height = 110;
                }

                #region by line 修改顯示名稱

                if (FormMainMDI.G_OPIAp.CurLine.LineType.Equals("PCS"))
                {
                    lblProductType_CUT.Text = "PCS Product Type";
                    lblProductID_CUT.Text = "PCS Product ID";
                }

                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void LoadPPIDSetting()
        {
            try
            {
                Recipe _new;

                var _var = (from _recipe in FormMainMDI.G_OPIAp.DBCtx.SBRM_RECIPE
                            where _recipe.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType &&
                                  _recipe.ONLINECONTROLSTATE == MesControlMode
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

        #region Socket Process

        private void SendtoBC_CassetteDataRequest()
        {
            try
            {
                LocalCassetteDataRequest _trx = new LocalCassetteDataRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.PORTNO = curPort.PortNo;
                _trx.BODY.EQUIPMENTNO = curPort.NodeNo;
                _trx.BODY.PORTID = curPort.PortID;
                _trx.BODY.CASSETTEID = curPort.CassetteID;
                
                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, 0);

                if (_resp == null) return;

                #region LocalCassetteDataReply

                string _respXml = _resp.Xml;

                LocalCassetteDataReply _localCassetteDataReply = (LocalCassetteDataReply)Spec.CheckXMLFormat(_respXml);

                RecvMessage_CassetteDataReply(_respXml);

                #endregion
                
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SendtoBC_LocalModeCassetteDataSend()
        {
            try
            {
                string _err = string.Empty;
                string _ppid = string.Empty;

                LocalModeCassetteDataSend _trx = new LocalModeCassetteDataSend();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.PORTNO = curPort.PortNo;
                _trx.BODY.EQUIPMENTNO = curPort.NodeNo;
                _trx.BODY.PORTID = curPort.PortID;
                _trx.BODY.CASSETTEID = curPort.CassetteID;
                _trx.BODY.REMAPFLAG = IsRemap ? "Y" : "N";
                _trx.BODY.CSTSETTINGCODE = txtCassetteSettingCode_Port.Text.ToString();

                foreach (LocalCassetteDataReply.LOTDATAc _lotData in LstLots)
                {
                    LocalModeCassetteDataSend.LOTDATAc _lot = new LocalModeCassetteDataSend.LOTDATAc();
                    _lot.LOTNAME = _lotData.LOTNAME;
                    _lot.CSTSETTINGCODE = _lotData.CSTSETTINGCODE;
                    _lot.CSTSETTINGCODE_CUT = _lotData.CSTSETTINGCODE_CUT;
                    _lot.PRODUCTID = _lotData.PRODUCTID==string.Empty ? "0": _lotData.PRODUCTID;
                    _lot.PRODUCTID_CUT = _lotData.PRODUCTID_CUT == string.Empty ? "0" : _lotData.PRODUCTID_CUT;
                    _lot.BCPRODUCTTYPE = _lotData.BCPRODUCTTYPE == string.Empty ? "0" :_lotData.BCPRODUCTTYPE;
                    _lot.BCPRODUCTTYPE_CUT = _lotData.BCPRODUCTTYPE_CUT == string.Empty ? "0" : _lotData.BCPRODUCTTYPE_CUT;

                    _lot.LINERECIPENAME = txtLotRecipeID.Tag == null ? string.Empty : txtLotRecipeID.Tag.ToString();
                    _lot.PPID = txtLotRecipeID.Text.ToString();

                    ////LineType == "CUT" 
                    //if (OPIConst.LstLineType_Cutting.Contains(FormMainMDI.G_OPIAp.CurLine.LineType))
                    //{
                        //_lot.CURRENTLINEPPID = txtCurrentLinePPID.Text.ToString();
                        ////_lot.CROSSLINEPPID = txtCrossLinePPID.Text.ToString();

                        //#region cutting recipe
                        //if (Recipe_Lot_Cutting != null)
                        //{
                        //    _lot.LINERECIPENAME = Recipe_Lot_Cutting.RecipeName;
                        //    _lot.PPID = Recipe_Lot_Cutting.PPID;
                        //}

                        //if (Recipe_Proc_Cutting != null)
                        //{
                        //    LocalModeCassetteDataSend.PROCESSLINEc _proc = new LocalModeCassetteDataSend.PROCESSLINEc();
                        //    _proc.LINENAME = Recipe_Proc_Cutting.ServerName;
                        //    _proc.LINERECIPENAME = Recipe_Proc_Cutting.RecipeName;
                        //    _proc.PPID = Recipe_Proc_Cutting.PPID;
                        //    _proc.CSTSETTINGCODE = txtCassetteSettingCode_CUT.Text.ToString();
                        //    _proc.PRODUCTID = txtProductID_CUT.Text.ToString().Trim()==string.Empty ? "0" : txtProductID_CUT.Text.ToString();
                        //    _proc.BCPRODUCTTYPE = txtProductType_CUT.Text.ToString().Trim() == string.Empty ? "0" : txtProductType_CUT.Text.ToString();
                        //    _lot.PROCESSLINELIST.Add(_proc);
                        //}

                        //if (Recipe_STB_Cutting != null)
                        //{
                        //    LocalModeCassetteDataSend.STBPRODUCTSPECc _stb = new LocalModeCassetteDataSend.STBPRODUCTSPECc();
                        //    _stb.LINENAME = "CBPOL400"; //Recipe_STB_Cutting.ServerName;
                        //    _stb.LINERECIPENAME = Recipe_STB_Cutting.RecipeName;
                        //    _stb.PPID = Recipe_STB_Cutting.PPID;
                        //    _stb.CSTSETTINGCODE = txtSTBCassetteSettingCode.Text.ToString();
                        //    _stb.PRODUCTID = txtProductID_STB.Text.ToString().Trim() == string.Empty ? "0" : txtProductID_STB.Text.ToString();
                        //    _stb.BCPRODUCTTYPE = txtProductType_STB.Text.ToString().Trim() == string.Empty ? "0" : txtProductType_STB.Text.ToString();

                        //    _lot.STBPRODUCTSPECLIST.Add(_stb);
                        //}

                        //if (Recipe_CrossProc_Cutting != null)
                        //{
                        //    LocalModeCassetteDataSend.PROCESSLINEc _proc = new LocalModeCassetteDataSend.PROCESSLINEc();
                        //    _proc.LINENAME = Recipe_CrossProc_Cutting.ServerName;
                        //    _proc.LINERECIPENAME = Recipe_CrossProc_Cutting.RecipeName;
                        //    _proc.PPID = Recipe_CrossProc_Cutting.PPID;
                        //    _proc.CSTSETTINGCODE = txtCrossCassetteSettingCode.Text.ToString();
                        //    _proc.PRODUCTID = txtProductID_Cross.Text.ToString().Trim() == string.Empty ? "0" : txtProductID_Cross.Text.ToString();
                        //    _proc.BCPRODUCTTYPE = txtProductType_Cross.Text.ToString().Trim() == string.Empty ? "0" : txtProductType_Cross.Text.ToString();

                        //    _lot.PROCESSLINELIST.Add(_proc);
                        //}

                        //if (Recipe_CrossSTB_Cutting != null)
                        //{
                        //    LocalModeCassetteDataSend.STBPRODUCTSPECc _stb = new LocalModeCassetteDataSend.STBPRODUCTSPECc();
                        //    _stb.LINENAME = "CBPOL400"; //Recipe_CrossSTB_Cutting.ServerName;
                        //    _stb.LINERECIPENAME = Recipe_CrossSTB_Cutting.RecipeName;
                        //    _stb.PPID = Recipe_CrossSTB_Cutting.PPID;
                        //    _stb.CSTSETTINGCODE = txtSTBCassetteSettingCode.Text.ToString();
                        //    _stb.PRODUCTID = txtProductID_STB.Text.ToString().Trim() == string.Empty ? "0" : txtProductID_STB.Text.ToString();
                        //    _stb.BCPRODUCTTYPE = txtProductType_STB.Text.ToString().Trim() == string.Empty ? "0" : txtProductType_STB.Text.ToString();

                        //    _lot.STBPRODUCTSPECLIST.Add(_stb);
                        //}
                        //#endregion
                    //}

                    foreach (LocalCassetteDataReply.PRODUCTDATAc _productData in _lotData.PRODUCTLIST)
                    {
                        LocalModeCassetteDataSend.PRODUCTDATAc _product = new LocalModeCassetteDataSend.PRODUCTDATAc();

                        _product.PROCESSFLAG = _productData.PROCESSFLAG;
                        _product.SLOTNO = _productData.SLOTNO;
                        _product.PRODUCTRECIPENAME = _productData.PRODUCTRECIPENAME;

                        _product.PPID = _productData.PPID;

                        _lot.PRODUCTLIST.Add(_product);
                    }

                    _trx.BODY.LOTLIST.Add(_lot);
                }

                string _xml = _trx.WriteToXml();

                //modify by yang 20161021
                if (curPort.CassetteStatus != eCassetteStatus.WaitingforCassetteData && curPort.CassetteStatus != eCassetteStatus.CassetteReMap)
                {
                    #region LocalModeCassetteDataSendReply
                    ShowMessage(this, lblCaption.Text, "", "Local Cassette Command NG ,cur CassetteStatus not permit Mapdownload !", MessageBoxIcon.Information);
                    #endregion
                }
                else
                {
                    MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, FormMainMDI.G_OPIAp.SocketResponseTime_MapDownload);

                    if (_resp != null)
                    {
                        #region LocalModeCassetteDataSendReply
                        ShowMessage(this, lblCaption.Text, "", "Local Cassette Command Send to BC Success !", MessageBoxIcon.Information);
                        #endregion
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void RecvMessage_CassetteMapDownloadResultReport( string xml)
        {
            try
            {
                CassetteMapDownloadResultReport trxData = (CassetteMapDownloadResultReport)Spec.CheckXMLFormat(xml);

                if (trxData.BODY.EQUIPMENTNO == curPort.NodeNo && trxData.BODY.PORTID == curPort.PortID && trxData.BODY.CASSETTEID == curPort.CassetteID)
                {
                    if (tlpBase.InvokeRequired)
                    {
                        this.BeginInvoke(new MethodInvoker(delegate
                        {
                            switch (trxData.BODY.RESULT)
                            {
                                case "0": txtReturnCode.Text = "0:Unknown"; break;
                                case "1": txtReturnCode.Text = "1:Command OK"; btnMapDownload.Enabled = false; break;
                                case "2": txtReturnCode.Text = "2:Command Error"; break;
                                case "3": txtReturnCode.Text = "3:CST Id Is Invalid"; break;
                                case "4": txtReturnCode.Text = "4:Recipe Mismatch"; break;
                                case "5": txtReturnCode.Text = "5:Slot Information Mismatch"; break;
                                case "6": txtReturnCode.Text = "6:Job Type Mismatch"; break;
                                case "7": txtReturnCode.Text = "7:Cassette Setting Code Mismatch"; break;
                                case "8": txtReturnCode.Text = "8:Alaready Received"; break;
                                case "9": txtReturnCode.Text = "9:Other Error"; break;
                                case "10": txtReturnCode.Text = "10:Product Type Mismatch"; break;
                                case "11": txtReturnCode.Text = "11:Group Index Mismatch"; break;
                                case "12": txtReturnCode.Text = "12:Product ID Mismatch"; break;
                                case "99": txtReturnCode.Text = "99:Equipment response timeout"; break;
                                default: txtReturnCode.Text = string.Empty; break;
                            }

                            //Cassette Control 功能理的Map Download成功的狀況下，自動關閉此畫面，若失敗請告知原因 -- 2014-12-23 by登京
                            if (trxData.BODY.RESULT == "1")
                            {
                                this.DialogResult = System.Windows.Forms.DialogResult.OK;
                                this.Close();
                            }
                        }));
                    }
                    else
                    {
                        switch (trxData.BODY.RESULT)
                        {
                            case "0": txtReturnCode.Text = "0:Unknown"; break;
                            case "1": txtReturnCode.Text = "1:Command OK"; btnMapDownload.Enabled = false; break;
                            case "2": txtReturnCode.Text = "2:Command Error"; break;
                            case "3": txtReturnCode.Text = "3:CST Id Is Invalid"; break;
                            case "4": txtReturnCode.Text = "4:Recipe Mismatch"; break;
                            case "5": txtReturnCode.Text = "5:Slot Information Mismatch"; break;
                            case "6": txtReturnCode.Text = "6:Job Type Mismatch"; break;
                            case "7": txtReturnCode.Text = "7:Cassette Setting Code Mismatch"; break;
                            case "8": txtReturnCode.Text = "8:Alaready Received"; break;
                            case "9": txtReturnCode.Text = "9:Other Error"; break;
                            case "10": txtReturnCode.Text = "10:Product Type Mismatch"; break;
                            case "11": txtReturnCode.Text = "11:Group Index Mismatch"; break;
                            case "12": txtReturnCode.Text = "12:Product ID Mismatch"; break;
                            case "99": txtReturnCode.Text = "99:Equipment response timeout"; break;
                            default: txtReturnCode.Text = string.Empty; break;
                        }

                        //Cassette Control 功能理的Map Download成功的狀況下，自動關閉此畫面，若失敗請告知原因 -- 2014-12-23 by登京
                        if (trxData.BODY.RESULT == "1")
                        {
                            this.DialogResult = System.Windows.Forms.DialogResult.OK;
                            this.Close();
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

        private void RecvMessage_CassetteDataReply(string xml)
        {
            try
            {
                RadioButton _firstLot = null;

                string _slotNo = string.Empty;
                string _jobType_Desc = string.Empty;
                string _jobJudge_Desc = string.Empty;
                string _processType_Desc = string.Empty;
                string _preInlineID_Desc = string.Empty;
                string _cuttingFlag_Desc = string.Empty;

                LocalCassetteDataReply trxData = (LocalCassetteDataReply)Spec.CheckXMLFormat(xml);

                #region Cassette
                txtCassetteID.Text = trxData.BODY.CASSETTEID;
                //txtLineOperMode.Text = trxData.BODY.LINEOPERMODE;
                txtProdQty.Text = trxData.BODY.PRODUCTQUANTITY;
                txtProcessQty.Text = trxData.BODY.PRODUCTQUANTITY;
                //txtCleanFlag.Text = trxData.BODY.CLEANFLAG;
                txtCassetteSettingCode_Port.Text = trxData.BODY.CSTSETTINGCODE;
                #endregion

                LstLots = new List<LocalCassetteDataReply.LOTDATAc>();

                foreach (LocalCassetteDataReply.LOTDATAc lot in trxData.BODY.LOTLIST)
                {
                    LstLots.Add(lot);

                    RadioButton _lot = new RadioButton();
                    _lot.Name = lot.LOTNAME;
                    _lot.Font = new Font("Calibri", 10, FontStyle.Regular);
                    _lot.Text = lot.LOTNAME;
                    _lot.CheckedChanged += new EventHandler(lblLot_CheckedChanged);
                    _lot.Visible = true;
                    _lot.Checked = false;
                    _lot.Appearance = Appearance.Button;
                    _lot.Margin = new System.Windows.Forms.Padding(1, 0, 1, 1);
                    _lot.Size = new System.Drawing.Size(100,23);
                    flpLotList.Controls.Add(_lot);

                    if (_firstLot == null) _firstLot = _lot;
                }

                if (LstLots.Count > 0)
                {
                    if (LstLots.Count == 1) flpLotList.Visible = false;
                    else
                    {
                        flpLotList.Visible = true;

                        if (LstLots.Count <= 9)
                        {
                            flpLotList.Size = new Size(1054, 25);
                        }
                        else
                        {
                            flpLotList.Size = new Size(1054, 50);
                        }
                    }

                    _firstLot.Checked = true;
                }

                btnMapDownload.Enabled = true;

                btnRecipeCheck.Enabled = true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void RecvMessage_RecipeRegisterValidationReturnReport(string xml)
        {
            try
            {
                string _recipeName = string.Empty;
                string _jobType_Desc = string.Empty;
                string _jobJudge_Desc = string.Empty;
                string _processType_Desc = string.Empty;
                string _preInlineID_Desc = string.Empty;
                string _cuttingFlag_Desc = string.Empty;

                RecipeRegisterValidationReturnReport trxData = (RecipeRegisterValidationReturnReport)Spec.CheckXMLFormat(xml);

                foreach (RecipeRegisterValidationReturnReport.LINEc _line in trxData.BODY.LINELIST)
                {
                    if (FormMainMDI.G_OPIAp.CurLine.ServerName == _line.RECIPELINENAME)
                    {
                        foreach (RecipeRegisterValidationReturnReport.RECIPECHECKc _recipeCheck in _line.RECIPECHECKLIST)
                        {
                            _recipeName = _recipeCheck.RECIPENAME;

                            if (dicRecipeUse.ContainsKey(_recipeName))
                            {
                                foreach (RecipeRegisterValidationReturnReport.EQUIPMENTc _eq in _recipeCheck.EQUIPMENTLIST)
                                {
                                    dicRecipeUse[_recipeName].SetRecipeCheckResult(_eq.EQUIPMENTNO, _eq.RECIPENO, _eq.RETURN, _eq.NGMESSAGE);
                                }
                            }
                        }
                    }
                    //else
                    //{
                    //    foreach (RecipeRegisterValidationReturnReport.RECIPECHECKc _recipeCheck in _line.RECIPECHECKLIST)
                    //    {
                    //        _recipeName = _recipeCheck.RECIPENAME;

                    //        if (dicRecipeUse_Cross.ContainsKey(_recipeName))
                    //        {
                    //            foreach (RecipeRegisterValidationReturnReport.EQUIPMENTc _eq in _recipeCheck.EQUIPMENTLIST)
                    //            {
                    //                dicRecipeUse_Cross[_recipeName].SetRecipeCheckResult(_eq.EQUIPMENTNO, _eq.RECIPENO, _eq.RETURN, _eq.NGMESSAGE);
                    //            }
                    //        }
                    //    }
                    //}
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        #endregion

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                if (Q_BCSReport.Count > 0)
                {
                    string _msg = Q_BCSReport.Dequeue();
                    string[] _msgItem = _msg.Split('^');

                    if (_msgItem.Length != 2) return;

                    switch (_msgItem[0])
                    {
                        case "CassetteMapDownloadResultReport":

                            #region CassetteMapDownloadResultReport
                            RecvMessage_CassetteMapDownloadResultReport(_msgItem[1]);
                            #endregion

                            break;

                        case "RecipeRegisterValidationReturnReport":

                            #region RecipeRegisterValidationReturnReport
                            RecvMessage_RecipeRegisterValidationReturnReport(_msgItem[1]);
                            #endregion

                            break;

                        default:
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        //yang
        private void dgvProduct_ColumnHeaderMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            try
            {
             
                switch (dgvProduct.Columns[e.ColumnIndex].Name)
                {

                    case "colProcessFlag":

                        dgvProduct.Columns[e.ColumnIndex].ReadOnly = true;

                        #region select all & anti select all

                        if (!txtProcessQty.Text.Equals(txtProdQty.Text))
                        {
                            foreach (DataGridViewRow row in dgvProduct.Rows)
                            {
                                if (row.Cells[colGlassID.Name].Value != null)
                                {

                                    row.Cells[colProcessFlag.Name].Value = true;



                                }

                            }
                            txtProcessQty.Text = txtProdQty.Text;
                        }
                        else
                        {
                            foreach (DataGridViewRow row in dgvProduct.Rows)
                            {
                                if (row.Cells[colGlassID.Name].Value != null)
                                {

                                    row.Cells[colProcessFlag.Name].Value = false;


                                }

                            }
                            txtProcessQty.Text = '0'.ToString();
                        }

                        #endregion
                        break;
                }
                dgvProduct.Columns[e.ColumnIndex].ReadOnly = false;

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

       
    }
}
