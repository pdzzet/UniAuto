using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;

namespace UniOPI
{
    public partial class FormUserGroupQuery : FormBase
    {
        public FormUserGroupQuery()
        {
            InitializeComponent();
        }

        private void FormUserGroupQuery_Load(object sender, EventArgs e)
        {
            try
            {
                this.lblCaption.Text = "Group User Query";

                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                var _var = (from _group in _ctxBRM.SBRM_OPI_USER_GROUP
                            select _group.GROUP_ID).Distinct();
                if (_var == null) return;

                cboGroupID.DataSource = _var.ToList();

                cboGroupID.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            try
            {
                if (cboGroupID.Text == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, string.Empty, "Please Choose Group ID！", MessageBoxIcon.Warning);
                    return;
                }

                Refresh_GroupInformation(cboGroupID.Text, this.dgvGroup);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Refresh_GroupInformation(string strGroupID, DataGridView dgv)
        {
            try
            {
                #region 找出Group中符合輸入GroupID資料，並依照Button Key 將Description填入特定欄位
                UniBCSDataContext ctx = FormMainMDI.G_OPIAp.DBCtx;

                var objUser = (from usr in ctx.SBRM_OPI_USER_GROUP
                               from rep in ctx.SBRM_OPI_BUTTON_RELATION
                               where rep.ACTIVE == "Y" && rep.BUTTON_KEY.Equals(usr.BUTTON_KEY) && System.Data.Linq.SqlClient.SqlMethods.Like(usr.GROUP_ID, "%" + strGroupID + "%")
                               select new GroupQuery
                               {
                                   GroupID = usr.GROUP_ID,
                                   GroupName = usr.GROUP_NAME,
                                   Visible = usr.VISIBLE,
                                   Enable = usr.ENABLE,
                                   ButtonKey = usr.BUTTON_KEY,
                                   ButtonDesc = rep.BUTTON_DESC

                               }).ToList<GroupQuery>();

                foreach (var ButtonDesciption in objUser)
                {
                    if (ButtonDesciption.ButtonKey.Contains("S00F00"))
                        ButtonDesciption.ButtonMain = ButtonDesciption.ButtonDesc;
                    else if (ButtonDesciption.ButtonKey.Contains("F00"))
                        ButtonDesciption.ButtonSub = ButtonDesciption.ButtonDesc;
                    else if (!(ButtonDesciption.ButtonKey.Contains("F00")))
                        ButtonDesciption.ButtonFun = ButtonDesciption.ButtonDesc;
                }

                dgvGroup.DataSource = objUser;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
                #endregion
        }

        private void dgvGroup_DataSourceChanged(object sender, System.EventArgs e)
        {
            #region 當連接資料改變時，改變列的顏色並依照顏色將+-號加入欄位中，並隱藏非MainButton的欄位
            try
            {
                foreach (DataGridViewRow _dataRowSet in dgvGroup.Rows)
                {
                    if (_dataRowSet.Cells[colMainButton.Name].Value != null)
                    {
                        _dataRowSet.DefaultCellStyle.BackColor = Color.White;
                        _dataRowSet.Cells[colExpand.Name].Value = "+";
                    }
                    else if (_dataRowSet.Cells[colSubButton.Name].Value != null)
                    {
                        _dataRowSet.DefaultCellStyle.BackColor = Color.Pink;
                        _dataRowSet.Visible = false;
                    }
                    else if (_dataRowSet.Cells[colFunButton.Name].Value != null)
                    {
                        if (dgvGroup.Rows[int.Parse(_dataRowSet.Index.ToString()) - 1].DefaultCellStyle.BackColor == Color.Pink && _dataRowSet.DefaultCellStyle.BackColor != Color.Pink)
                            dgvGroup.Rows[int.Parse(_dataRowSet.Index.ToString()) - 1].Cells[colExpand.Name].Value = "+";
                        _dataRowSet.DefaultCellStyle.BackColor = Color.LightYellow;
                        _dataRowSet.Visible = false;
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

        private void dgvGroup_CellClick(object sender, System.Windows.Forms.DataGridViewCellEventArgs e)
        {
            #region 當點選欄位時，根據+-號做顯示/隱藏欄位
            try
            {
                #region 若+-欄位有值，MainButton有值，則把到下一個有值的MainButton之前所有列展開，並把+-號設為-號
                if (e.RowIndex > -1 && dgvGroup.Rows[e.RowIndex].Cells[colExpand.Name].Value != null && dgvGroup.Rows[e.RowIndex].Cells[colExpand.Name].Value.Equals("+") && dgvGroup.Rows[e.RowIndex].Cells[colMainButton.Name].Value != null)
                {
                    int rows = e.RowIndex + 1;
                    while (rows < dgvGroup.Rows.Count)
                    {
                        if (dgvGroup.Rows[e.RowIndex].DefaultCellStyle.BackColor == Color.White && dgvGroup.Rows[rows].DefaultCellStyle.BackColor == Color.White)
                            break;
                        else if (dgvGroup.Rows[rows].Cells[colSubButton.Name].Value != null)
                            dgvGroup.Rows[rows].Visible = true;
                        rows++;
                    }
                    dgvGroup.Rows[e.RowIndex].Cells[colExpand.Name].Value = "-";
                }               
                #endregion
                #region 若+-欄位有值，MainButton有值，則把到下一個有值的MainButton之前所有列隱藏，並把+-號設為+號(包跨子列)
                else if (e.RowIndex > -1 && dgvGroup.Rows[e.RowIndex].Cells[colExpand.Name].Value != null && dgvGroup.Rows[e.RowIndex].Cells[colExpand.Name].Value.Equals("-") && dgvGroup.Rows[e.RowIndex].Cells[colMainButton.Name].Value != null)
                {
                    int rows = e.RowIndex + 1;
                    while (rows < dgvGroup.Rows.Count)
                    {
                        if (dgvGroup.Rows[e.RowIndex].DefaultCellStyle.BackColor == Color.White && dgvGroup.Rows[rows].DefaultCellStyle.BackColor == Color.White)
                            break;
                        else if (dgvGroup.Rows[rows].Cells[colSubButton.Name].Value != null)
                        {
                            dgvGroup.Rows[rows].Visible = false;
                            if (dgvGroup.Rows[rows].Cells[colExpand.Name].Value != null && dgvGroup.Rows[rows].Cells[colExpand.Name].Value.Equals("-"))
                                dgvGroup.Rows[rows].Cells[colExpand.Name].Value = "+";
                        }
                        else if (dgvGroup.Rows[rows].Cells[colFunButton.Name].Value != null)
                            dgvGroup.Rows[rows].Visible = false;
                        rows++;
                    }
                    dgvGroup.Rows[e.RowIndex].Cells[colExpand.Name].Value = "+";
                }
                #endregion
                ///////////////////////////////////////////////////////
                #region 若+-欄位有值，SubButton有值，則把到下一個有值的SubButton之前所有列展開，並把+-號設為-號
                else if (e.RowIndex > -1 && dgvGroup.Rows[e.RowIndex].Cells[colExpand.Name].Value != null && dgvGroup.Rows[e.RowIndex].Cells[colExpand.Name].Value.Equals("+") && dgvGroup.Rows[e.RowIndex].Cells[colSubButton.Name].Value != null)
                {
                    int rows = e.RowIndex + 1;
                    while (rows < dgvGroup.Rows.Count)
                    {
                        if (dgvGroup.Rows[e.RowIndex].DefaultCellStyle.BackColor == Color.LightYellow && dgvGroup.Rows[rows].DefaultCellStyle.BackColor == Color.LightYellow)
                            break;
                        else if (dgvGroup.Rows[rows].Cells[colFunButton.Name].Value != null)
                            dgvGroup.Rows[rows].Visible = true;
                        rows++;
                    }
                    dgvGroup.Rows[e.RowIndex].Cells[colExpand.Name].Value = "-";
                }
#endregion
                #region 若+-欄位有值，SubButton有值，則把到下一個有值的SubButton之前所有列隱藏，並把+-號設為+號
                else if (e.RowIndex > -1 && dgvGroup.Rows[e.RowIndex].Cells[colExpand.Name].Value != null && dgvGroup.Rows[e.RowIndex].Cells[colExpand.Name].Value.Equals("-") && dgvGroup.Rows[e.RowIndex].Cells[colSubButton.Name].Value != null)
                {
                    int rows = e.RowIndex + 1;
                    while (rows < dgvGroup.Rows.Count)
                    {
                        if (dgvGroup.Rows[e.RowIndex].DefaultCellStyle.BackColor == Color.LightYellow && dgvGroup.Rows[rows].DefaultCellStyle.BackColor == Color.LightYellow)
                            break;
                        else if (dgvGroup.Rows[rows].Cells[colFunButton.Name].Value != null)
                            dgvGroup.Rows[rows].Visible = false;

                        rows++;
                    }
                    dgvGroup.Rows[e.RowIndex].Cells[colExpand.Name].Value = "+";
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

        private void btnExpand_Click(object sender, System.EventArgs e)
        {
            #region 將所有有值的+-欄位，都改成-號及顯示其列
            try
            {
                foreach (DataGridViewRow _dgvRow in dgvGroup.Rows)
                {
                        if (_dgvRow.Cells[colExpand.Name].Value != null)
                        {
                            _dgvRow.Cells[colExpand.Name].Value = "-";
                        }
                        if (_dgvRow.Visible == false && _dgvRow.Cells[colMainButton.Name].Value == null)
                            _dgvRow.Visible = true;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

        private void btnHide_Click(object sender, System.EventArgs e)
        {
            #region 將所有有值的+-欄位，都改成+號及隱藏其列

            try
            {
                foreach (DataGridViewRow _dgvRow in dgvGroup.Rows)
                {
                        if (_dgvRow.Cells[colExpand.Name].Value != null)
                        {
                            _dgvRow.Cells[colExpand.Name].Value = "+";
                        }
                        if (_dgvRow.Visible == true && _dgvRow.Cells[colMainButton.Name].Value == null)
                        {
                            dgvGroup.CurrentCell = null;
                            _dgvRow.Visible = false;
                        }
                  }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
            #endregion
        }

    }
}

public class GroupQuery
{
    public string GroupName { get; set; }
    public string GroupID { get; set; }
    public string Expand { get; set; }
    public string ButtonKey { get; set; }
    public string ButtonMain { get; set; }
    public string ButtonSub { get; set; }
    public string ButtonFun { get; set; }
    public string ButtonDesc { get; set; }
    public string Visible { get; set; }
    public string Enable { get; set; }

}