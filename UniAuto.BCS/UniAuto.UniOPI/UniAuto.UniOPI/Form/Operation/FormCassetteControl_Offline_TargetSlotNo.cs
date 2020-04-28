using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormCassetteControl_Offline_TargetSlotNo : FormBase
    {
        DataGridViewRowCollection DgvRows;

        public FormCassetteControl_Offline_TargetSlotNo(DataGridViewRowCollection dgvRows)
        {
            InitializeComponent();

            DgvRows = dgvRows;
        }

        private void FormCassetteControl_Offline_TargetSlotNo_Load(object sender, EventArgs e)
        {
            try
            {
                foreach (DataGridViewRow dgvr in DgvRows)
                {
                    dgvCassette.Rows.Add(dgvr.Cells[colProcessFlag.Name].Value, 
                                        dgvr.Cells[colSlotNo.Name].Value, 
                                        dgvr.Cells[colGlassID.Name].Value, 
                                        dgvr.Cells[colTargetSlotNo.Name].Value, 
                                        dgvr.Cells[colTargetSlotNo.Name].Value);
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

                #region 更新目前下貨畫面上顯示資訊
                foreach (DataGridViewRow row in DgvRows)
                {
                    string slotNo = row.Cells[colSlotNo.Name].Value.ToString();

                    DataGridViewRow curRow = (dgvCassette.Rows.Cast<DataGridViewRow>().Where(d => d.Cells[colSlotNo.Name].Value.ToString().Equals(slotNo))).FirstOrDefault();

                    if (curRow == null) continue;

                    row.Cells[colTargetSlotNo.Name].Value = curRow.Cells[colTargetSlotNo.Name].Value.ToString().PadLeft(3,'0');
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
            try
            {
                this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private bool CheckData()
        {
            try
            {
                bool _isDo = false ; //是否已經取值過
                bool _isBySeq = false;  //0：By Seuqnce

                int _num = 0;

                string _targetSlotNo = string.Empty;

                #region Check Target Slot No 是否重複
                foreach (DataGridViewRow _row in dgvCassette.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[colProcessFlag.Name].Value.ToString().ToUpper().Equals("TRUE")))
                {
                    _targetSlotNo = _row.Cells[colTargetSlotNo.Name].Value.ToString();

                    if (int.TryParse(_targetSlotNo, out _num) == false)
                    {
                        ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", string.Format("Target Slot No [{0}] must be number (1-28)", _targetSlotNo), MessageBoxIcon.Error);
                        return false;
                    }

                    #region 以gridview 取得的第一筆決定是否為by Seuqnce
                    if (_isDo == false)
                    {
                        _isDo = true;

                        if (_num == 0) _isBySeq = true; //0：By Seuqnce
                        else _isBySeq = false;
                    }
                    #endregion

                    #region 0：By Seuqnce,Range：1~28
                    if (_isBySeq)
                    {
                        if (_num != 0)
                        {
                            ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", string.Format(" By Seuqnce Target Slot No [{0}] must be 0 ", _targetSlotNo), MessageBoxIcon.Error);
                            return false;
                        }
                    }
                    else
                    {
                        if (_num > 28 || _num < 1)
                        {
                            ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", string.Format(" 1 <= Target Slot No [{0}] <= 28)", _targetSlotNo), MessageBoxIcon.Error);
                            return false;
                        }

                        if (dgvCassette.Rows.Cast<DataGridViewRow>().Where(r => r.Cells[colProcessFlag.Name].Value.ToString().ToUpper().Equals("TRUE") &&
                            r.Cells[colTargetSlotNo.Name].Value.ToString().Equals(_targetSlotNo)).Count() > 1)
                        {
                            ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", string.Format("Target Slot No [{0}] is Duplicate", _targetSlotNo), MessageBoxIcon.Error);
                            return false;
                        }
                    }
                    #endregion
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

        //private void btnBySeqNo_Click(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        foreach (DataGridViewRow _row in dgvCassette.Rows)
        //        {
        //            _row.Cells[colTargetSlotNo.Name].Value = "0";
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
        //        ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
        //    }
        //}

        private void chkBySeqNo_CheckedChanged(object sender, EventArgs e)
        {
            try
            {
                CheckBox _chk = (CheckBox)sender ;

                if (_chk.Checked)
                {
                    foreach (DataGridViewRow _row in dgvCassette.Rows)
                    {
                        _row.Cells[colTargetSlotNo.Name].Value = "0";
                    }

                    dgvCassette.Columns[colTargetSlotNo.Name].ReadOnly = true;
                }
                else
                {
                    foreach (DataGridViewRow _row in dgvCassette.Rows)
                    {
                        _row.Cells[colTargetSlotNo.Name].Value = _row.Cells[colSlotNo.Name].Value;
                    }

                    dgvCassette.Columns[colTargetSlotNo.Name].ReadOnly = false;
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
