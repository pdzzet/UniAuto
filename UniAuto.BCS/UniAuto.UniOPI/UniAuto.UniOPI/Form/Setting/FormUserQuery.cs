using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Drawing;

namespace UniOPI
{
    public partial class FormUserQuery : FormBase
    {
        public FormUserQuery()
        {
            InitializeComponent();
        }

        private void FormUserQuery_Load(object sender, EventArgs e)
        {
            this.lblCaption.Text = "User Query";
   
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            try
            {
                    Refresh_UserInformation("", this.dgvUserList);
                    GetUserData(0);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);               
            }
        }

        private void Refresh_UserInformation(string strUserID, DataGridView dgv)
        {
            try
            {

                UniBCSDataContext ctx = FormMainMDI.G_OPIAp.DBCtx;
                var objUser = (from usr in ctx.SBRM_OPI_USER_ACCOUNT
                               where System.Data.Linq.SqlClient.SqlMethods.Like(usr.USER_ID, "%" + strUserID + "%")
                               select new GroupUser
                               {
                                   USER_ID = usr.USER_ID,
                                   CLIENT_KEY = usr.CLIENT_KEY,
                                   USER_NAME = usr.USER_NAME,
                                   PASSWORD = usr.PASSWORD,
                                   GROUP_ID = usr.GROUP_ID,
                                   ACTIVE = usr.ACTIVE,
                                   E_MAIL = usr.E_MAIL,
                                   LOCAL_NAME = usr.LOCAL_NAME,
                                   DEFAULT_FACTORY_NAME = usr.DEFAULT_FACTORY_NAME,
                                   DEFAULT_AREA_NAME = usr.DEFAULT_AREA_NAME,
                                   UACACTIVE = usr.UACACTIVE,
                                   TRX_DATETIME = usr.TRX_DATETIME,
                                   TRX_USER_ID = usr.TRX_USER_ID,
                                   ADD_DATETIME = usr.ADD_DATETIME,
                                   ADD_USER_ID = usr.ADD_USER_ID
                               }).ToList<GroupUser>();
                dgv.AutoGenerateColumns = false;
                dgv.DataSource = objUser;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvUserList_DataSourceChanged(object sender, EventArgs e)
        {
            foreach(DataGridViewRow _dataGridViewRos in dgvUserList.Rows)
            {
                DataGridViewCell cellLogging = dgvUserList.Rows[_dataGridViewRos.Index].Cells[colLogging.Name], cellLogout = dgvUserList.Rows[_dataGridViewRos.Index].Cells[colLogout.Name]; ;
                if (_dataGridViewRos.Cells[dgvchkACTIVE.Name].Value.Equals("Y"))
                {
                    cellLogging.Value = "Y";
                    cellLogout.Value = "N";
                }
                else
                {
                    cellLogging.Value = "N";
                    cellLogout.Value = "Y";
                }
            }
        }

        private void dgvUserList_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            GetUserData(e.RowIndex);
        }

        private void GetUserData(int row)
        {
            try
            {
                if (row != -1)
                {
                    txtActive.Text = dgvUserList.Rows[row].Cells[dgvchkACTIVE.Name].Value==null ? string.Empty  : dgvUserList.Rows[row].Cells[dgvchkACTIVE.Name].Value.ToString();
                    txtClientKey.Text = dgvUserList.Rows[row].Cells[dgvtxtCLIENT_KEY.Name].Value == null ? string.Empty : dgvUserList.Rows[row].Cells[dgvtxtCLIENT_KEY.Name].Value.ToString();
                    txtEmail.Text = dgvUserList.Rows[row].Cells[dgvtxtEmail.Name].Value == null ? string.Empty : dgvUserList.Rows[row].Cells[dgvtxtEmail.Name].Value.ToString();
                    txtLocalName.Text = dgvUserList.Rows[row].Cells[dgvtxtLocalName.Name].Value==null ? string.Empty : dgvUserList.Rows[row].Cells[dgvtxtLocalName.Name].Value.ToString();
                    txtPassword.Text = dgvUserList.Rows[row].Cells[dgvtxtPASSWORD.Name].Value==null ? string.Empty : dgvUserList.Rows[row].Cells[dgvtxtPASSWORD.Name].Value.ToString();
                    txtUserID.Text = dgvUserList.Rows[row].Cells[dgvtxtUSER_ID.Name].Value==null ? string.Empty :dgvUserList.Rows[row].Cells[dgvtxtUSER_ID.Name].Value.ToString();
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
