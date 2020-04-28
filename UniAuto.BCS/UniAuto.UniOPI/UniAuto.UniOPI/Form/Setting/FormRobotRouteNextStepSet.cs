using System;
using System.Reflection;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormRobotRouteNextStepSet : FormBase
    {
        SBRM_ROBOT_ROUTE_STEP RobotRouteStep = null;

        DataGridView DgvSetp = null;

        public FormRobotRouteNextStepSet(SBRM_ROBOT_ROUTE_STEP routeStep, DataGridView dgvRouteStep)
        {
            InitializeComponent();

            lblCaption.Text = "Next Step ID Change";

            RobotRouteStep = routeStep;

            DgvSetp = dgvRouteStep;

            txtCurrentNextStepID.Text = routeStep.NEXTSTEPID.ToString();
        }

        private void FormRobotRouteNextStepSet_Load(object sender, EventArgs e)
        {
            try
            {
                foreach (DataGridViewRow _row in DgvSetp.Rows)
                {
                    cboNextStep.Items.Add(_row.Cells["colStepID"].Value.ToString());
                }

                cboNextStep.Items.Add("65535");
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

        private void btnSetOK_Click(object sender, EventArgs e)
        {
            try
            {
                if (cboNextStep.SelectedItem == null)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please Choose New Next Step ID !", MessageBoxIcon.Warning);
                    return;
                }

                int _nextStepID = int.Parse(cboNextStep.SelectedItem.ToString());

                if (_nextStepID == RobotRouteStep.STEPID)
                {
                    ShowMessage(this, lblCaption.Text, "", "New Next Step ID must be different from Step ID", MessageBoxIcon.Warning);
                    return;
                }

                RobotRouteStep.NEXTSTEPID = _nextStepID;

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }                  
        }
    }
}
