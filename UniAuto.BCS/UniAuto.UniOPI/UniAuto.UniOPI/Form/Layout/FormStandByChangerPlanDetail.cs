using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;


namespace UniOPI
{
    public partial class FormStandByChangerPlanDetail : FormBase
    {
        public FormStandByChangerPlanDetail()
        {
            InitializeComponent();
        }
        private void FormStandByChangerPlanDetail_Load(object sender, EventArgs e)
        {
            try
            {
                dgvData.DataSource = FormMainMDI.G_OPIAp.CurLine.LstStandByChangerPlanDetail;
             
                tmrBaseRefresh.Enabled = true;

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);                
            }
        }

        private void tmrBaseRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                txtPlanID.Text = FormMainMDI.G_OPIAp.CurLine.StandByChangerPlanID;
                if (txtPlanID.Text.Length != 0)
                    txtPlanStatus.Text = "Request";
                else txtPlanStatus.Text = "NoPlan";   //yang

                dgvData.DataSource = FormMainMDI.G_OPIAp.CurLine.LstStandByChangerPlanDetail;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            string _err = "";
            try
            {
                CurrentChangerPlanRequest _trx = new CurrentChangerPlanRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                string _xml = _trx.WriteToXml();

                if (FormMainMDI.SocketDriver.SendMessage(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID) == false)
                {
                    ShowMessage(this, lblCaption.Text ,"",string.Format("Current Change Plan Request Error [{0}]",_err), MessageBoxIcon.Warning);                    
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

        private void lblCaption_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                FormTrxTest _frm = new FormTrxTest();

                if (_frm.ShowDialog() == DialogResult.OK)
                {
                    CurrentChangerPlanReport _currentChangerPlanReport = (CurrentChangerPlanReport)Spec.CheckXMLFormat(_frm.MsgData);

                    FormMainMDI.G_OPIAp.CurLine.StandByChangerPlanID = _currentChangerPlanReport.BODY.CHANGERPLAN.STANDBYPLANID;

                    FormMainMDI.G_OPIAp.CurLine.LstStandByChangerPlanDetail = new BindingList<PlanDetail>();
                    foreach (CurrentChangerPlanReport.PRODUCTc _product in _currentChangerPlanReport.BODY.CHANGERPLAN.PRODUCTLIST)
                    {
                        PlanDetail _detail = new PlanDetail();
                        _detail.SlotNo = _product.SLOTNO;
                        _detail.ProductName = _product.PRODUCTNAME;
                        _detail.SourceCSTID = _product.SOURCECSTID;
                        _detail.TargetCSTID = _product.TARGETCSTID;
                        _detail.HaveBeenUse = (_product.HAVEBEENUSE.ToUpper() == "TRUE" ? true : false);

                        FormMainMDI.G_OPIAp.CurLine.LstStandByChangerPlanDetail.Add(_detail);
                    }
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
