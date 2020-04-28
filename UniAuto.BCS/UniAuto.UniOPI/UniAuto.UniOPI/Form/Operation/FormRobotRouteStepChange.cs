using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormRobotRouteStepChange : FormBase
    {

        string CSTSeqNo = string.Empty;
        string JobSeqNo = string.Empty;
        string CurRouteID = string.Empty;
        string CurStepNo = string.Empty;
        string NextStepNo = string.Empty;
        string RobotName = string.Empty;

        public FormRobotRouteStepChange(string cstSeqNo, string jobSeqNo)
        {
            InitializeComponent();

            lblCaption.Text = "Robot Route Step Change";

            CSTSeqNo = cstSeqNo;
            JobSeqNo = jobSeqNo;

            dgvRouteStep.Rows.Clear();            
        }

        private void FormRobotRouteStepChange_Load(object sender, EventArgs e)
        {
            try
            {

                #region Send JobDataRequest

                RobotRouteCurrentStepNoRequest _trx = new RobotRouteCurrentStepNoRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.LineName;
                _trx.BODY.CASSETTESEQNO = CSTSeqNo;
                _trx.BODY.JOBSEQNO = JobSeqNo;

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #endregion

                #region RobotRouteCurrentStepInfoReply

                string _respXml = _resp.Xml;

                RobotRouteCurrentStepNoReply _reply = (RobotRouteCurrentStepNoReply)Spec.CheckXMLFormat(_respXml);

                #region Update Data
                CurRouteID = _reply.BODY.CURRENTROUTEID.ToString();
                CurStepNo = _reply.BODY.CURRENTSTEPNO.ToString();
                NextStepNo = _reply.BODY.NEXTSTEPNO.ToString();
                RobotName = _reply.BODY.ROBOTNAME.ToString();

                txtRobotName.Text = RobotName;
                txtGlassID.Text = _reply.BODY.GLASSID.ToString();
                txtCstSeqNo.Text = _reply.BODY.CASSETTESEQNO.ToString();
                txtJobSeqNo.Text = _reply.BODY.JOBSEQNO.ToString();
                txtRouteID.Text = CurRouteID;
                txtStepNo.Text = CurStepNo;
                txtNextStepNo.Text = NextStepNo;
                dgvRouteStepData();
                #endregion

                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnChange_Click(object sender, EventArgs e)
        {
            try
            {
                if (cboNewStep.SelectedValue == null)
                {
                    ShowMessage(this, MethodBase.GetCurrentMethod().Name,"", "Please Choose New Step No", MessageBoxIcon.Error);
                    return;
                }

                if (cboNextStep.SelectedValue == null)
                {
                    ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", "Please Choose New Next Step No", MessageBoxIcon.Error);
                    return;
                }

                string _newStepNo = cboNewStep.SelectedValue.ToString();
                string _nextStepNo = cboNextStep.SelectedValue.ToString();

                if (_newStepNo == _nextStepNo)
                {
                    ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", "New Step No and New Next Step No must be different", MessageBoxIcon.Error);
                    return;
                }

                #region Send JobDataRequest

                RobotRouteCurrentStepChangeRequest _trx = new RobotRouteCurrentStepChangeRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.LineName;
                _trx.BODY.CASSETTESEQNO = CSTSeqNo;
                _trx.BODY.JOBSEQNO = JobSeqNo;
                _trx.BODY.NEWSTEPNO = _newStepNo;
                _trx.BODY.NEXTSTEPNO = _nextStepNo;
                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #endregion

                #region RobotRouteCurrentStepChangeReply

                ShowMessage(this, lblCaption.Text, "", "Robot Route Step Change Send to BC Success !", MessageBoxIcon.Information);

                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void dgvRouteStepData()
        {
            try
            {
                if (CurRouteID == string.Empty || RobotName == string.Empty) return;

                string[] _routeID = CurRouteID.Split(':');

                if (_routeID.Length <= 0) return;

                UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBBRMCtx;

                var _routes = (from m in _ctx.SBRM_ROBOT_ROUTE_STEP
                             where m.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName &&
                                    m.ROBOTNAME == RobotName && m.ROUTEID == _routeID[0]
                               select new RouteData { Step = m.STEPID, UseArm = m.ROBOTUSEARM, Action = m.ROBOTACTION, RouteRule = m.ROBOTRULE, stageNoList = m.STAGEIDLIST });

                dgvRouteStep.DataSource = _routes.ToList();

                RouteData _new = new RouteData();
                _new.Step = 65535;

                List<RouteData> _lstRoute = _routes.ToList();
                _lstRoute.Add(_new);

                cboNewStep.DataSource = _lstRoute;
                cboNewStep.DisplayMember = "Step";
                cboNewStep.ValueMember = "Step";
                cboNewStep.SelectedIndex = -1;


                List<RouteData> _lstNextRoute = _routes.ToList();
                _lstNextRoute.Add(_new);

                cboNextStep.DataSource = _lstNextRoute;
                cboNextStep.DisplayMember = "Step";
                cboNextStep.ValueMember = "Step";
                cboNextStep.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        
    }

    class RouteData
    {
        public int Step { get; set; }
        public string UseArm { get; set; }
        public string Action { get; set; }
        public string RouteRule { get; set; }
        public string stageNoList { get; set; }
    }
}
