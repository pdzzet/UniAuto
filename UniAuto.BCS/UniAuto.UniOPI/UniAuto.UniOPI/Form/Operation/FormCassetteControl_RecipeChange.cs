using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormCassetteControl_RecipeChange : FormBase
    {
        private string MesControlMode = string.Empty;
        private Dictionary<string, Recipe> dicRecipe_All;
        private DataGridViewRowCollection dgvRows;

        public DataGridViewRowCollection DgvRows
        {
            get { return dgvRows; }
            set { dgvRows = value; }
        }

        //for local mode 多lot 設定reipce 使用
        public List<LocalCassetteDataReply.LOTDATAc> LstLots;

        public FormCassetteControl_RecipeChange(Dictionary<string, Recipe> dicRecipe, string MESMode)
        {
            InitializeComponent();

            dicRecipe_All = dicRecipe;

            MesControlMode = MESMode;
        }

        private void FormCassetteControl_RecipeChange_Load(object sender, EventArgs e)
        {
            try
            {
                #region  顯示可設定的Recipe
                dgvRecipe.Rows.Clear();
                foreach (Recipe _recipe in dicRecipe_All.Values)
                {
                    dgvRecipe.Rows.Add(_recipe.RecipeName, _recipe.PPID);
                }

                gbRecipe.Visible = true;

                #endregion

                #region  顯示可設定的Recipe
                if (MesControlMode == "LOCAL")
                {
                    dgvCassette.Columns[colOwnerID.Name].Visible = true;
                    dgvCassette.Columns[colOwnerType.Name].Visible = true;
                }
                else
                {
                    dgvCassette.Columns[colOwnerID.Name].Visible = false;
                    dgvCassette.Columns[colOwnerType.Name].Visible = false;
                }

                #endregion

                int _endSlotNo = 0;
                int _num = 0;

                if (LstLots == null)
                {
                    foreach (DataGridViewRow dgvr in dgvRows)
                    {
                        dgvCassette.Rows.Add(string.Empty,dgvr.Cells[colProcessFlag.Name].Value, dgvr.Cells["colSlotNo"].Value, dgvr.Cells["colOwnerID"].Value, dgvr.Cells["colOwnerType"].Value, dgvr.Cells["colRecipeID"].Value, dgvr.Cells["colRecipeID"].Value, dgvr.Cells["colPPID"].Value);

                        //txtSelToSlotNo.Tag = dgvr.Cells["colSlotNo"].Value.ToString();

                        int.TryParse(dgvr.Cells["colSlotNo"].Value.ToString(), out _num);

                        if (_num > _endSlotNo)
                        {
                            _endSlotNo = _num;
                            txtSelToSlotNo.Tag = dgvr.Cells["colSlotNo"].Value.ToString();
                        }
                    }

                    if (txtSelToSlotNo.Tag != null)
                        txtSelToSlotNo.Text = txtSelToSlotNo.Tag.ToString();
                }
                else
                {
                    bool _procFlag = false ;

                    foreach (LocalCassetteDataReply.LOTDATAc _lot in LstLots)
                    {
                        foreach (LocalCassetteDataReply.PRODUCTDATAc _porduct in _lot.PRODUCTLIST)
                        {
                            _procFlag = _porduct.PROCESSFLAG == "Y" ? true : false;

                            dgvCassette.Rows.Add(_lot.LOTNAME, _procFlag,_porduct.SLOTNO.ToString(),  _porduct.OWNERID.ToString(), _porduct.OWNERTYPE.ToString(), _porduct.PRODUCTRECIPENAME.ToString(), _porduct.PRODUCTRECIPENAME.ToString(), _porduct.PPID.ToString());

                            int.TryParse(_porduct.SLOTNO.ToString(),out _num);

                            if (_num > _endSlotNo)
                            {
                                _endSlotNo = _num;
                                txtSelToSlotNo.Tag = _porduct.SLOTNO.ToString();
                            }
                            
                        }
                    }

                    if (txtSelToSlotNo.Tag != null)
                        txtSelToSlotNo.Text = txtSelToSlotNo.Tag.ToString();
                }
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
                string _newRecipe = string.Empty;
                string _newPPID = string.Empty;
                string _msg = string.Empty;
                int _intFrSlotNo = 0;
                int _intToSlotNo = 0;
                int _slotNo = 0;

                #region 取得slot no
                if (string.IsNullOrWhiteSpace(txtSelFrSlotNo.Text.ToString()))
                {
                    ShowMessage(this, lblCaption.Text , "", "Please key in Start Slot No", MessageBoxIcon.Error);
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
                    ShowMessage(this, lblCaption.Text, "", "The start Slot No must be less than the end Slot No!! ", MessageBoxIcon.Error);
                    return;
                }

                #endregion

                if (dgvRecipe.Rows.Count > 0)
                {
                    _newRecipe = dgvRecipe.CurrentRow.Cells[colRecipeName.Name].Value.ToString();
                    _newPPID = dgvRecipe.CurrentRow.Cells[colPPID.Name].Value.ToString();
                }

                foreach (DataGridViewRow row in dgvCassette.Rows)
                {
                    _slotNo = 0;

                    int.TryParse(row.Cells["colSlotNo"].Value.ToString(), out _slotNo);

                    if (_slotNo > _intToSlotNo) break;

                    if (_slotNo >= _intFrSlotNo && _slotNo <= _intToSlotNo)
                    {
                        if (MesControlMode == "LOCAL")
                        {
                            //ARRAY Line 都要可以設定recipe  2016/07/21 Edit by box.zhai
                            if (FormMainMDI.G_OPIAp.CurLine.FabType != "ARRAY")
                            {
                                #region 但如果MES download 的是以下产品信息则不能做修改 -- OWNERTYPE: P || OWNERID: RESD
                                if (row.Cells[colOwnerType.Name].Value.ToString() == "P" || row.Cells[colOwnerID.Name].Value.ToString() == "RESD")
                                {
                                    _msg = _msg + string.Format("\r\n Slot [{0}] ", _slotNo);
                                    continue;
                                }
                                #endregion

                                if ((bool)row.Cells["colProcessFlag"].Value == false) continue;
                            }
                        }

                        row.Cells["colNewRecip"].Value = _newRecipe;
                        row.Cells["colNewPPID"].Value = _newPPID;
                    }
                }

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

        private void chk_SelectAll_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox _chk = (CheckBox)sender;

                if (_chk.Checked)
                {
                    if (txtSelFrSlotNo.Tag != null)
                        txtSelFrSlotNo.Text = txtSelFrSlotNo.Tag.ToString();

                    if (txtSelToSlotNo.Tag != null)
                        txtSelToSlotNo.Text = txtSelToSlotNo.Tag.ToString();
                }
                else
                {
                    txtSelFrSlotNo.Text = string.Empty;
                    txtSelToSlotNo.Text = string.Empty;
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
                #region 更新目前下貨畫面上顯示資訊
                foreach (DataGridViewRow row in dgvRows)
                {
                    string slotNo = row.Cells["colSlotNo"].Value.ToString();

                    DataGridViewRow curRow = (dgvCassette.Rows.Cast<DataGridViewRow>().Where(d => d.Cells[colSlotNo.Name].Value.ToString().Equals(slotNo))).FirstOrDefault();

                    if (curRow == null) continue;

                    if (MesControlMode == "LOCAL")
                    {
                        //ARRAY Line 都要可以設定recipe  2016/06/16 Edit by box.zhai
                        if (FormMainMDI.G_OPIAp.CurLine.FabType != "ARRAY")
                        {
                            #region 但如果MES download 的是以下产品信息则不能做修改 -- OWNERTYPE: P || OWNERID: RESD
                            if (row.Cells[colOwnerType.Name].Value.ToString() == "P" || row.Cells[colOwnerID.Name].Value.ToString() == "RESD")
                            {
                                continue;
                            }
                            #endregion

                            if ((bool)row.Cells["colProcessFlag"].Value == false) continue;
                        }
                    }

                    row.Cells["colRecipeID"].Value = curRow.Cells["colNewRecip"].Value;
                    row.Cells["colPPID"].Value = curRow.Cells["colNewPPID"].Value;
                }
                #endregion

                #region 若有多個Lot 須同步更新非current lot 對應的recipe 
                if (LstLots != null)
                {
                    string _lotName = string.Empty;
                    string _slotNo = string.Empty ;
                    string _recipeName = string.Empty ;
                    string _ppid = string.Empty ;

                    foreach (DataGridViewRow _row in dgvCassette.Rows)
                    {
                        _lotName = _row.Cells[colLotName.Name].Value.ToString();
                        _slotNo = _row.Cells[colSlotNo.Name].Value.ToString();
                        _recipeName= _row.Cells[colNewRecip.Name].Value.ToString();
                        _ppid = _row.Cells[colNewPPID.Name].Value.ToString();

                        //ARRAY Line 都要可以設定recipe  2016/07/21 Edit by box.zhai
                        if (FormMainMDI.G_OPIAp.CurLine.FabType != "ARRAY")
                        {
                            #region 但如果MES download 的是以下产品信息则不能做修改 -- OWNERTYPE: P || OWNERID: RESD
                            if (_row.Cells[colOwnerType.Name].Value.ToString() == "P" || _row.Cells[colOwnerID.Name].Value.ToString() == "RESD")
                            {
                                continue;
                            }

                            if ((bool)_row.Cells["colProcessFlag"].Value == false) continue;
                            #endregion
                        }

                        LocalCassetteDataReply.LOTDATAc _lot = LstLots.Find(r => r.LOTNAME.Equals(_lotName));

                        if (_lot == null) continue;

                        LocalCassetteDataReply.PRODUCTDATAc _product = _lot.PRODUCTLIST.Find(r => r.SLOTNO.Equals(_slotNo));

                        if (_product == null) continue;

                        _product.PRODUCTRECIPENAME = _recipeName;
                        _product.PPID = _ppid;
                    }
                }

                #endregion

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

        private void btnSetToRow_Click(object sender, EventArgs e)
        {
            try
            {
                string _msg = string.Empty;
                int _slotNo = 0;
                DataGridViewRow _recipeCurRow = dgvRecipe.CurrentRow;
                if (_recipeCurRow == null) return;

                string _recipeName = _recipeCurRow.Cells["colRecipeName"].Value.ToString();
                string _ppid = _recipeCurRow.Cells["colPPID"].Value.ToString();

                foreach (DataGridViewRow _row in dgvCassette.SelectedRows)
                {
                    if (MesControlMode == "LOCAL")
                    {
                        //ARRAY Line 都要可以設定recipe  2016/07/21 Edit by box.zhai
                        if (FormMainMDI.G_OPIAp.CurLine.FabType != "ARRAY")
                        {
                            _slotNo = 0;

                            int.TryParse(_row.Cells["colSlotNo"].Value.ToString(), out _slotNo);

                            #region 但如果MES download 的是以下产品信息则不能做修改 -- OWNERTYPE: P || OWNERID: RESD
                            if (_row.Cells[colOwnerType.Name].Value.ToString() == "P" || _row.Cells[colOwnerID.Name].Value.ToString() == "RESD")
                            {
                                _msg = _msg + string.Format("\r\n Slot [{0}] ", _slotNo);
                                continue;
                            }
                            #endregion

                            if ((bool)_row.Cells["colProcessFlag"].Value == false) continue;
                        }
                    }

                    _row.Cells["colNewRecip"].Value = _recipeName; // _recipeCurRow.Cells["colRecipeName"].Value;
                    _row.Cells["colNewPPID"].Value = _ppid; // _recipeCurRow.Cells["colPPID"].Value;
                }

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

        private void btnSetToRows_Click(object sender, EventArgs e)
        {
            try
            {
                int _slotNo = 0;
                string _msg = string.Empty;

                DataGridViewRow _recipeCurRow = dgvRecipe.CurrentRow;
                if (_recipeCurRow == null) return;

                string _recipeName = _recipeCurRow.Cells["colRecipeName"].Value.ToString();
                string _ppid = _recipeCurRow.Cells["colPPID"].Value.ToString();

                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, string.Format("Recipe Name [{0}] for all slot ?", _recipeName))) return;

                foreach (DataGridViewRow _row in dgvCassette.Rows)
                {

                    if (MesControlMode == "LOCAL")
                    {
                        //ARRAY Line 都要可以設定recipe  2016/07/21 Edit by box.zhai
                        if (FormMainMDI.G_OPIAp.CurLine.FabType != "ARRAY")
                        {
                            _slotNo = 0;

                            int.TryParse(_row.Cells["colSlotNo"].Value.ToString(), out _slotNo);

                            #region 但如果MES download 的是以下产品信息则不能做修改 -- OWNERTYPE: P || OWNERID: RESD
                            if (_row.Cells[colOwnerType.Name].Value.ToString() == "P" || _row.Cells[colOwnerID.Name].Value.ToString() == "RESD")
                            {
                                _msg = _msg + string.Format("Slot [{0}] \r\n", _slotNo);
                                continue;
                            }
                            #endregion

                            if ((bool)_row.Cells["colProcessFlag"].Value == false) continue;
                        }
                    }
                    _row.Cells["colNewRecip"].Value = _recipeName;
                    _row.Cells["colNewPPID"].Value = _ppid;
                }

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

    }
}
