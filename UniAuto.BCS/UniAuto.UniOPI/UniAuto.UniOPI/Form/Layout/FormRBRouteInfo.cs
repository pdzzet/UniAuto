using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;
using System.Linq;

namespace UniOPI
{
    public partial class FormRBRouteInfo : FormBase
    {
        string CSTSeqNo = string.Empty;
        string JobSeqNo = string.Empty;

        public FormRBRouteInfo(string cstSeqNo, string jobSeqNo)
        {
            InitializeComponent();

            CSTSeqNo = cstSeqNo;
            JobSeqNo = jobSeqNo;
        }

        private void FormRBRouteInfo_Load(object sender, EventArgs e)
        {
            try
            {
                #region Send RobotRouteStepInfoRequest

                RobotRouteStepInfoRequest _trx = new RobotRouteStepInfoRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.LineName;
                _trx.BODY.CASSETTESEQNO = CSTSeqNo;
                _trx.BODY.JOBSEQNO = JobSeqNo;

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #endregion

                #region RobotRouteStepInfoReply

                string _respXml = _resp.Xml;

                RobotRouteStepInfoReply _reply = (RobotRouteStepInfoReply)Spec.CheckXMLFormat(_respXml);

                #region Update Data
                SetData(_reply);
                #endregion

                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetData(RobotRouteStepInfoReply robotRouteStepInfoReply)
        {
            try
            {
                UniBCSDataContext _ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                txtCstSeqNo.Text = robotRouteStepInfoReply.BODY.CASSETTESEQNO;
                txtJobSeqNo.Text = robotRouteStepInfoReply.BODY.JOBSEQNO;
                txtGlassID.Text = robotRouteStepInfoReply.BODY.GLASSID;
                txtRealStepID.Text = robotRouteStepInfoReply.BODY.REAL_STEPID;
                txtRealNextStepID.Text = robotRouteStepInfoReply.BODY.REAL_NEXT_STEPID;
                txtRouteID.Text = robotRouteStepInfoReply.BODY.ROUTE_ID;
                var _description = (from _ds in _ctxBRM.SBRM_ROBOT_ROUTE_MST where _ds.SERVERNAME.Equals(FormMainMDI.G_OPIAp.CurLine.ServerName) && _ds.ROUTEID.Equals(robotRouteStepInfoReply.BODY.ROUTE_ID.ToString()) select _ds.DESCRIPTION).ToList();
                txtDescription.Text = _description.Count().Equals(0) ? "" : _description[0].ToString(); 

                List<RobotRouteStepInfoReply.ITEMc> _lstItems = robotRouteStepInfoReply.BODY.ITEMLIST;

                dgvData.Rows.Clear();

                foreach (RobotRouteStepInfoReply.ITEMc _item in _lstItems)
                {
                    dgvData.Rows.Add(_item.VNAME, _item.VVALUE);
                }
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
    }
}
