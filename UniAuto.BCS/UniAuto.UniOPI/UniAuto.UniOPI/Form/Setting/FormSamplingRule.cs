using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormSamplingRule : FormBase
    {
        public FormSamplingRule()
        {
            InitializeComponent();
            lblCaption.Text = "Sampling Rule";
        }

        private void FormSamplingRule_Load(object sender, EventArgs e)
        {
            Initial();
            InitialCombobox();
            lblNode.Text = "";
        }

        #region Btn

        private void btnQuery_Click(object sender, EventArgs e)
        {
            try
            {
                #region Check
                if (this.cmbNode.SelectedIndex == -1)
                {
                    ShowMessage(this, lblCaption.Text, "", "Local ID required！", MessageBoxIcon.Error);
                    return;
                }
                #endregion

                string error = string.Empty;
                string _xml = string.Empty;
                SamplingRuleDataRequest _trx = new SamplingRuleDataRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = cmbNode.SelectedValue.ToString();
                _xml = _trx.WriteToXml();
                
                #region Send SamplingRuleDataRequest

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #endregion

                #region SamplingRuleDataReply

                string _respXml = _resp.Xml;

                SamplingRuleDataReply _SamplingRuleDataReply = (SamplingRuleDataReply)Spec.CheckXMLFormat(_respXml);

                #region Update Data

                txtSamplingRule.Text = GetSamplingRule(_SamplingRuleDataReply.BODY.SAMPLINGRULE);
                txtSamplingUnit.Text = _SamplingRuleDataReply.BODY.SAMPLINGUNIT;
                string strSideInformation = _SamplingRuleDataReply.BODY.SIDEINFORMATION;
                chkSideInformation01.Checked = GetSideInformationValue(strSideInformation, 0);
                chkSideInformation02.Checked = GetSideInformationValue(strSideInformation, 1);
                chkSideInformation03.Checked = GetSideInformationValue(strSideInformation, 2);
                chkSideInformation04.Checked = GetSideInformationValue(strSideInformation, 3);

                #endregion

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                #region Check
                if (this.cmbNode.SelectedIndex == -1)
                {
                    ShowMessage(this, lblCaption.Text, "", "Local ID required！", MessageBoxIcon.Error);
                    return;
                }
                if (this.cmbSendSamplingRule.SelectedIndex == -1)
                {
                    ShowMessage(this, lblCaption.Text, "", "Sampling Rule required！", MessageBoxIcon.Error);
                    return;
                }
                if (string.IsNullOrEmpty(txtSendSamplingUnit.Text.Trim()))
                {
                    ShowMessage(this, lblCaption.Text, "", "Sampling Unit required！", MessageBoxIcon.Error);
                    return;
                }
                int iUnit = 0;
                if (!int.TryParse(txtSendSamplingUnit.Text.Trim(), out iUnit))
                {
                    ShowMessage(this, lblCaption.Text, "", "Sampling Unit required！", MessageBoxIcon.Error);
                    return;
                }
                if (iUnit > 65535)
                {
                    ShowMessage(this, lblCaption.Text, "", "Sampling Unit <= 65535！", MessageBoxIcon.Error);
                    return;
                }
                #endregion

                string _msg = string.Format("Please confirm whether you will Change Sampling Rule of Node:{0} ?", cmbNode.SelectedValue.ToString());

                if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text,_msg)) return;

                if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                string error = string.Empty;
                string _xml = string.Empty;
                SamplingRuleChangeCommand _trx = new SamplingRuleChangeCommand();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = cmbNode.SelectedValue.ToString();
                _trx.BODY.SAMPLINGRULE = cmbSendSamplingRule.SelectedValue.ToString();
                _trx.BODY.SAMPLINGUNIT = txtSendSamplingUnit.Text.Trim();
                _trx.BODY.SIDEINFORMATION = GetSendSideInformation();
                _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #region SamplingRuleChangeCommandReply
                ShowMessage(this, lblCaption.Text, "", "Sampling Rule Change Send to BC Success !", MessageBoxIcon.Information);
                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Event

        private void txtSendSamplingUnit_KeyPress(object sender, KeyPressEventArgs e)
        {
            e.Handled = (!char.IsNumber(e.KeyChar) && e.KeyChar != (char)Keys.Delete && e.KeyChar != (char)Keys.Enter && e.KeyChar != (char)Keys.Back);
        }
        
        #endregion

        #region Method

        private void Initial()
        {
            try
            {
                cmbNode.SelectedIndex = -1;

                #region Query
                txtSamplingRule.Clear();
                txtSamplingUnit.Clear();
                chkSideInformation01.Checked = false;
                chkSideInformation02.Checked = false;
                chkSideInformation03.Checked = false;
                chkSideInformation04.Checked = false;
                #endregion

                #region Send
                cmbSendSamplingRule.SelectedIndex = -1;
                txtSendSamplingUnit.Clear();
                chkSendSideInformation01.Checked = false;
                chkSendSideInformation02.Checked = false;
                chkSendSideInformation03.Checked = false;
                chkSendSideInformation04.Checked = false;
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void InitialCombobox()
        {
            SetNode();
            SetSamplingRule();
        }

        private void SetNode()
        {
            try
            {
                var q = (from node in FormMainMDI.G_OPIAp.Dic_Node.Values
                         where node.NodeAttribute == "IN"
                         select new
                         {
                             IDNAME = string.Format("{0}-{1}-{2}", node.NodeNo, node.NodeID, node.NodeName),
                             node.NodeNo,
                             node.NodeID
                         }).ToList();

                if (q == null || q.Count == 0) return;

                cmbNode.DataSource = q;
                cmbNode.DisplayMember = "IDNAME";
                cmbNode.ValueMember = "NODENO";
                cmbNode.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SetSamplingRule()
        {
            var data = new[] 
            { 
                new { Key = "1", Value = "By Count" } ,
                new { Key = "2", Value = "By Unit" } ,
                new { Key = "3", Value = "By Slot" } ,
                new { Key = "4", Value = "By ID" } ,
                new { Key = "5", Value = "Full Inspection" },                
                new { Key = "6", Value = "Inspection Skip" },
                new { Key = "7", Value = "Normal Inspection" }
            };

            this.cmbSendSamplingRule.DataSource = data;
            this.cmbSendSamplingRule.DisplayMember = "Value";
            this.cmbSendSamplingRule.ValueMember = "Key";
            this.cmbSendSamplingRule.SelectedIndex = -1;
        }

        private string GetSamplingRule(string key)
        {
            switch (key)
            {
                case "1": return "1:By Count";
                case "2": return "2:By Unit";
                case "3": return "3:By Slot";
                case "4": return "4:By ID";
                case "5": return "5:Full Inspection";
                case "6": return "6:Inspection Skip";
                case "7": return "7:Normal Inspection";
                default: return key;
            }
        }

        private string GetSendSideInformation()
        {
            try
            {
                string strA = "0", strB = "0", strC = "0", strD = "0", strE = "0",
                    strF = "0", strG = "0", strH = "0", strI = "0", strJ = "0",
                    strK = "0", strL = "0", strM = "0", strN = "0", strO = "0",
                    strP = "0";

                if (chkSendSideInformation01.Checked) strA = "1";
                if (chkSendSideInformation02.Checked) strB = "1";
                if (chkSendSideInformation03.Checked) strC = "1";
                if (chkSendSideInformation04.Checked) strD = "1";

                return string.Format("{0}{1}{2}{3}{4}{5}{6}{7}{8}{9}{10}{11}{12}{13}{14}{15}",
                    strA, strB, strC, strD, strE,
                    strF, strG, strH, strI, strJ,
                    strK, strL, strM, strN, strO,
                    strP);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);

                return string.Empty;
            }
        }

        private bool GetSideInformationValue(string strSideInformation, int iIndex)
        {
            try
            {
                if (strSideInformation.Length <= iIndex) return false;

                string strValue = strSideInformation.Substring(iIndex, 1);

                return strValue == "1" ? true : false;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                return false;
            }
        }

        #endregion

        private void cmbNode_SelectedValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (cmbNode.SelectedValue == null) return;

                lblNode.Text = cmbNode.Text.ToString();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
    }
}
