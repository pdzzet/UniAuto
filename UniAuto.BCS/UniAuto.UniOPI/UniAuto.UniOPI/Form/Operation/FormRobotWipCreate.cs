using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormRobotWipCreate : FormBase
    {

        string CSTSeqNo = string.Empty;
        string JobSeqNo = string.Empty;
        string CurRouteID = string.Empty;
        string CurStepNo = string.Empty;
        string NextStepNo = string.Empty;
        string RobotName = string.Empty;

        public FormRobotWipCreate(string cstSeqNo, string jobSeqNo)
        {
            InitializeComponent();

            lblCaption.Text = "Robot Wip Create";

            CSTSeqNo = cstSeqNo;
            JobSeqNo = jobSeqNo;

            dgvRouteStep.Rows.Clear();            
        }

        private void FormRobotWipCreate_Load(object sender, EventArgs e)
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
                if (CurRouteID == string.Empty || CurStepNo == "0" || NextStepNo == "0")
                {
                    txtRobotName.Text = RobotName;
                    txtGlassID.Text = _reply.BODY.GLASSID.ToString();
                    txtJobSeqNo.Text = _reply.BODY.JOBSEQNO.ToString();
                    txtCstSeqNo.Text = _reply.BODY.CASSETTESEQNO.ToString();
                }
                else 
                {
                    ShowMessage(this, MethodBase.GetCurrentMethod().Name,"", "Has Robot Wip,no need Create", MessageBoxIcon.Information);
                    this.Close();
                }
                #endregion

                #endregion

                #region Combox Inital

                cboRouteID_Inital();

                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void dgvRouteStepData()
        {
            try
            {
                if (CurRouteID==string.Empty||RobotName == string.Empty) return;

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

                cboCurStepID.DataSource = _lstRoute;
                cboCurStepID.DisplayMember = "Step";
                cboCurStepID.ValueMember = "Step";
                cboCurStepID.SelectedIndex = -1;


                List<RouteData> _lstNextRoute = _routes.ToList();
                _lstNextRoute.Add(_new);

                cboNextStepID.DataSource = _lstNextRoute;
                cboNextStepID.DisplayMember = "Step";
                cboNextStepID.ValueMember = "Step";
                cboNextStepID.SelectedIndex = -1;


                
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }  

        private void cboRouteID_Inital()
        {
            this.cboRouteID.SelectedIndexChanged -= new System.EventHandler(this.cboRouteID_SelectedIndexChanged);
            UniBCSDataContext _ctx = FormMainMDI.G_OPIAp.DBBRMCtx;

            var _routes = (from m in _ctx.SBRM_ROBOT_ROUTE_MST
                           where m.SERVERNAME == FormMainMDI.G_OPIAp.CurLine.ServerName &&
                                  m.ROBOTNAME == RobotName
                           select new RouteList { RouteID = m.ROUTEID,RouteName=m.ROUTENAME });
            cboRouteID.DataSource = _routes;
            cboRouteID.DisplayMember = "RouteName";
            cboRouteID.ValueMember = "RouteID";
            cboRouteID.SelectedIndex = -1;
            this.cboRouteID.SelectedIndexChanged += new System.EventHandler(this.cboRouteID_SelectedIndexChanged);
        }

        private void cboNextStepID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboNextStepID.SelectedValue != null && cboCurStepID.SelectedValue!=null)
            {
                if ( Convert.ToInt32(cboNextStepID.SelectedValue) <= Convert.ToInt32(cboCurStepID.SelectedValue))
                {
                    ShowMessage(this, MethodBase.GetCurrentMethod().Name,"", "Next Step ID need > Current Step ID!", MessageBoxIcon.Warning);
                    cboNextStepID.SelectedIndex = -1;
                }
            }
        }

        private void cboRouteID_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboRouteID.SelectedIndex!=-1)
            {
                CurRouteID = cboRouteID.SelectedValue.ToString();
                dgvRouteStepData();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (cboRouteID.SelectedValue == null)
            {
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", "Route ID is Empty!", MessageBoxIcon.Error);
                return;
            }

            if (cboCurStepID.SelectedValue == null)
            {
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", "Current Step ID is Empty!", MessageBoxIcon.Error);
                return;
            }

            if (cboNextStepID.SelectedValue == null)
            {
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", "Next Step ID is Empty!", MessageBoxIcon.Error);
                return;
            }

            CurRouteID = cboRouteID.SelectedValue.ToString().Trim();
            CurStepNo = cboCurStepID.SelectedValue.ToString().Trim();
            NextStepNo = cboNextStepID.SelectedValue.ToString().Trim();

            RobotWipCreateRequest _request = new RobotWipCreateRequest();
            _request.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
            _request.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.LineName;
            _request.BODY.CASSETTESEQNO = CSTSeqNo;
            _request.BODY.JOBSEQNO = JobSeqNo;
            _request.BODY.NEWROUTEID = CurRouteID;
            _request.BODY.NEWSTEPNO = CurStepNo;
            _request.BODY.NEXTSTEPNO = NextStepNo;
            string _xml = _request.WriteToXml();
            MessageResponse _resp = this.SendRequestResponse(_request.HEADER.TRANSACTIONID, _request.HEADER.MESSAGENAME, _xml, 0);

            if (_resp == null) return;

            #region RobotRouteCurrentStepChangeReply

            ShowMessage(this, lblCaption.Text, "", "Robot Wip Create Request Send to BC Success !", MessageBoxIcon.Information);
            this.Close();

            #endregion

        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
    class RouteList
    {
        public string RouteID { get; set; }
        public string RouteName { get; set; }
    }
}
