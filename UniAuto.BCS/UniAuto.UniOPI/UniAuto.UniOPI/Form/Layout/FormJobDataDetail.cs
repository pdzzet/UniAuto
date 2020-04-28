using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormJobDataDetail : FormBase
    {
        string GlassID = string.Empty;
        string CSTSeqNo = string.Empty;
        string JobSeqNo = string.Empty;

        public FormJobDataDetail(string glassID, string cstSeqNo, string jobSeqNo)
        {
            InitializeComponent();

            GlassID = glassID;
            CSTSeqNo = cstSeqNo;
            JobSeqNo = jobSeqNo;

            //if (FormMainMDI.G_OPIAp.CurLine.FabType == "CELL") pnlOXInfoReqFlag.Visible = true;
            //else pnlOXInfoReqFlag.Visible = false;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FillContorls(JobDataReply jobDataReply)
        {
            try
            {
                txtSourceCSTID.Text = jobDataReply.BODY.CSTCONTROL.SOURCECST;
                txtTargetCSTID.Text = jobDataReply.BODY.CSTCONTROL.TRAGETCST;
                txtProcessOperName.Text = jobDataReply.BODY.CSTCONTROL.PROCESSOPERATORNAME;
                txtProductSpecName.Text = jobDataReply.BODY.CSTCONTROL.PRODUCTSPECNAME;
                txtProductOwner.Text = jobDataReply.BODY.CSTCONTROL.PRODUCTOWER;
                txtOwnerType.Text = jobDataReply.BODY.CSTCONTROL.OWERTYPE;
                txtOwnerID.Text = jobDataReply.BODY.CSTCONTROL.OWERID;

                txtLineName.Text = jobDataReply.BODY.LINENAME;
                txtGlassID.Text = jobDataReply.BODY.GLASSID;
                txtCstSeqNo.Text = jobDataReply.BODY.CASSETTESEQUENCENO;
                txtJobSeqNo.Text = jobDataReply.BODY.JOBSEQUENCENO;
                txtGroupIndex.Text = jobDataReply.BODY.GROUPINDEX;
                txtProdType.Text = jobDataReply.BODY.PRODUCTTYPE;
                txtCstOperMode.Text = jobDataReply.BODY.CSTOPERATIONMODE;
                txtSubstrateType.Text = jobDataReply.BODY.SUBSTRATETYPE;
                txtCimMode.Text = jobDataReply.BODY.CIMMODE;
                txtJobType.Text = jobDataReply.BODY.JOBTYPE;
                txtJobJudge.Text = jobDataReply.BODY.JOBJUDGE;
                txtSampSlotFlag.Text = jobDataReply.BODY.SAMPLINGSLOTFLAG;
                //txtOXInfoReqFlag.Text = jobDataReply.BODY.OXINFORMATIONREQUESTFLAG;
                txtFirstRunFlag.Text = jobDataReply.BODY.FIRSTRUN;
                txtJobGrade.Text = jobDataReply.BODY.JOBGRADE;
                txtPPID.Text = jobDataReply.BODY.PPID;
                List<JobDataReply.ABNORMALFLAGc> lstAbnFlag = jobDataReply.BODY.ABNORMALFLAGLIST;
                dgvAbn.Rows.Clear();
                foreach (JobDataReply.ABNORMALFLAGc abnFlag in lstAbnFlag)
                {
                    dgvAbn.Rows.Add(abnFlag.VNAME, abnFlag.VVALUE);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void FormJobDataDetail_Load(object sender, EventArgs e)
        {
            try
            {
                #region CBUAM line glass id改顯示 mask id
                if (FormMainMDI.G_OPIAp.CurLine.ServerName == "CBUAM100" || FormMainMDI.G_OPIAp.CurLine.ServerName=="FCKCN100") //add by qiumin 20170724
                //if (FormMainMDI.G_OPIAp.CurLine.ServerName == "CBUAM100")
                    lblGlassID.Text  = "Mask ID";
                else
                    lblGlassID.Text = "Glass ID";
                #endregion

                #region Send JobDataRequest

                JobDataRequest _trx = new JobDataRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.LineName;
                _trx.BODY.CASSETTESEQNO = CSTSeqNo;
                _trx.BODY.JOBSEQNO = JobSeqNo;
                _trx.BODY.JOBID =GlassID ;

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;
               
                #endregion

                #region JobDataReply

                string _respXml = _resp.Xml;

                JobDataReply _jobDataReply = (JobDataReply)Spec.CheckXMLFormat(_respXml);

                #region Update Data
                FillContorls(_jobDataReply);
                #endregion

                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }
    }
}
