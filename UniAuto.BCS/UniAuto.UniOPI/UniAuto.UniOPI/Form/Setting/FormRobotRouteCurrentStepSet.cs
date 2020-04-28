using System;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormRobotRouteCurrentStepSet : FormBase
    {
        SBRM_ROBOT_ROUTE_STEP RobotRouteStep = null;

        DataGridView DgvSetp = null;

        public FormRobotRouteCurrentStepSet(SBRM_ROBOT_ROUTE_STEP routeStep, DataGridView dgvRouteStep)
        {
            InitializeComponent();

            lblCaption.Text = "Current Step ID Change";

            RobotRouteStep = routeStep;

            DgvSetp = dgvRouteStep;

            txtCurrentStepID.Text = routeStep.STEPID.ToString();
        }

        private void txtNewStepID_KeyPress(object sender, KeyPressEventArgs e)
        {
            UniTools.CheckTextBoxKeyPressIsInteger(sender, e);
        }

        private void btnSetOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtNewStepID.Text.ToString().Trim() == string.Empty )
                {
                    ShowMessage(this, lblCaption.Text, "", "Please Input New Step ID !", MessageBoxIcon.Warning);
                    return;
                }

                int _stepID = int.Parse(txtNewStepID.Text.ToString());
                int _checkStepID = 0;

                #region Check step id是否已經存在
                foreach (DataGridViewRow _row in DgvSetp.Rows)
                {
                    _checkStepID = int.Parse(_row.Cells["colStepID"].Value.ToString());

                    if (_checkStepID == _stepID)
                    {
                        ShowMessage(this, lblCaption.Text, "", string.Format("Step ID [{0}] is exist !", _checkStepID.ToString()), MessageBoxIcon.Warning);
                        return;
                    }
                }
                #endregion

                RobotRouteStep.STEPID = _stepID;
                
                #region 有使用此next step id的更新為65535

                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;

                foreach (DataGridViewRow _row in DgvSetp.Rows)
                {

                    if (_row.Cells["colNextStep"].Value.ToString() == txtCurrentStepID.Text.ToString())
                    {
                        SBRM_ROBOT_ROUTE_STEP _obj = (_row.DataBoundItem) as SBRM_ROBOT_ROUTE_STEP;

                        _obj.NEXTSTEPID = 65535;

                        _row.Cells["colNextStep"].Value = "65535";
                    }
                }
                #endregion
                //if (_nextStepID == RobotRouteStep.STEPID)
                //{
                //    ShowMessage(this, lblCaption.Text, "", "New Next Step ID must be different from Step ID", MessageBoxIcon.Warning);
                //    return;
                //}

                //RobotRouteStep.NEXTSTEPID = _nextStepID;

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
    }
}
