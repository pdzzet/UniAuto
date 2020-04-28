using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormParameterSetting_ELA : FormBase
    {
        public FormParameterSetting_ELA()
        {
            InitializeComponent();
        }

        private void FormParameterSetting_ELA_Load(object sender, EventArgs e)
        {
            try
            {
                Send_BCSParameterDataInfoRequest();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void txtNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            int asciiCode = (int)e.KeyChar;
            if ((asciiCode >= 48 & asciiCode <= 57) |   // 0-9
                asciiCode == 8 | asciiCode == 45)  // Backspace 
            {
                if (txtCompensationTime_New.TextLength!=0 & asciiCode == 45)
                {
                    e.Handled = true;
                }
                if (asciiCode == 45)
                {
                    txtCompensationTime_New.MaxLength = 6;
                    e.Handled = false;
                }
                else if (txtCompensationTime_New.TextLength == 0)
                {
                    txtCompensationTime_New.MaxLength = 5;
                    e.Handled = false;
                }
                else
                    e.Handled = false;

            }
            else
                e.Handled = true;
        }

        private void txtProcessRecipe_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                TextBox _txt = (TextBox)sender;

                if (_txt.Tag == null) return;

                List<string> lstNodeRecipeID = UniTools.GetNodeRecipeID(_txt.Tag.ToString());

                if (lstNodeRecipeID == null) return;

                FormSelectItem frmSelectItem = new FormSelectItem("Recipe ID", lstNodeRecipeID);
                DialogResult dia = frmSelectItem.ShowDialog(this);
                if (dia == System.Windows.Forms.DialogResult.OK)
                    _txt.Text = frmSelectItem.ItemValue;
                else
                    _txt.Text = string.Empty;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnProcessRecipe_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtProcessRecipe_ELA01_New.Text == string.Empty && txtProcessRecipe_ELA02_New.Text == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please Choose Recipe ID for ELA01 or ELA02 ！", MessageBoxIcon.Warning);
                    return;
                }


                Send_BCSParameterDataChangeRequest("ProcessRecipe");
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnCompensationTime_Click(object sender, EventArgs e)
        {
            try
            {
                int _num = 0;

                if (int.TryParse(txtCompensationTime_New.Text, out _num) == false)
                {
                    ShowMessage(this, lblCaption.Text, "", "ELA Compenstion Time must be a number！", MessageBoxIcon.Warning);
                    return;
                }

                if (_num < -65535 || _num > 65535)
                {
                    ShowMessage(this, lblCaption.Text, "", "ELA Compenstion Time must be a number between -65535 and 65535！", MessageBoxIcon.Warning);
                    txtCompensationTime_New.Focus();
                    txtCompensationTime_New.SelectAll();
                    return;
                }

                Send_BCSParameterDataChangeRequest("CompensationTime");
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnOverQTime_Click(object sender, EventArgs e)
        {
            try
            {
                int _num = 0;

                if (txtOverQTime_Waring_New.Text == string.Empty && txtOverQTime_SendCST_New.Text == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please input Over Q-Time Data ！", MessageBoxIcon.Warning);

                    txtOverQTime_Waring_New.Focus();
                    return;
                }

                if (txtOverQTime_Waring_New.Text != string.Empty)
                {
                    if (int.TryParse(txtOverQTime_Waring_New.Text, out _num) == false)
                    {
                        ShowMessage(this, lblCaption.Text, "", "ELA Warring Over Q-Time must be a number！", MessageBoxIcon.Warning);
                        return;
                    }

                    if (_num < 0 || _num > 65535)
                    {
                        ShowMessage(this, lblCaption.Text, "", "ELA Warring Over Q-Time must be a number between 0 and 65535！", MessageBoxIcon.Warning);
                        txtOverQTime_Waring_New.Focus();
                        txtOverQTime_Waring_New.SelectAll();
                        return;
                    }
                }

                if (txtOverQTime_SendCST_New.Text != string.Empty)
                {
                    if (int.TryParse(txtOverQTime_SendCST_New.Text, out _num) == false)
                    {
                        ShowMessage(this, lblCaption.Text, "", "ELA Send To Cassette Over Q-Time must be a number！", MessageBoxIcon.Warning);
                        return;
                    }

                    if (_num < 0 || _num > 65535)
                    {
                        ShowMessage(this, lblCaption.Text, "", "ELA Send To Cassette Over Q-Time must be a number between 0 and 65535！", MessageBoxIcon.Warning);
                        txtOverQTime_SendCST_New.Focus();
                        txtOverQTime_SendCST_New.SelectAll();
                        return;
                    }
                }

                Send_BCSParameterDataChangeRequest("Over Q-Time");
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            try
            {
                Send_BCSParameterDataInfoRequest();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Send_BCSParameterDataInfoRequest()
        {
            try
            {
                #region Send BCSParameterDataInfoRequest

                BCSParameterDataInfoRequest _trx = new BCSParameterDataInfoRequest();

                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                #region Add ELA Parameter Item
                BCSParameterDataInfoRequest.PARAMETERc _param = new BCSParameterDataInfoRequest.PARAMETERc();
                _param.EQUIPMENTNO = "L4";
                _param.NAME = "ProcessRecipe";
                _trx.BODY.PARAMETERLIST.Add(_param);

                _param = new BCSParameterDataInfoRequest.PARAMETERc();
                _param.EQUIPMENTNO = "L5";
                _param.NAME = "ProcessRecipe";
                _trx.BODY.PARAMETERLIST.Add(_param);

                _param = new BCSParameterDataInfoRequest.PARAMETERc();
                _param.EQUIPMENTNO = "00";
                _param.NAME = "CompensationTime";
                _trx.BODY.PARAMETERLIST.Add(_param);

                _param = new BCSParameterDataInfoRequest.PARAMETERc();
                _param.EQUIPMENTNO = "L4";
                _param.NAME = "ProcessTime_RecipeID";
                _trx.BODY.PARAMETERLIST.Add(_param);

                _param = new BCSParameterDataInfoRequest.PARAMETERc();
                _param.EQUIPMENTNO = "L5";
                _param.NAME = "ProcessTime_RecipeID";
                _trx.BODY.PARAMETERLIST.Add(_param);

                _param = new BCSParameterDataInfoRequest.PARAMETERc();
                _param.EQUIPMENTNO = "L4";
                _param.NAME = "ProcessTime_ReplyTime";
                _trx.BODY.PARAMETERLIST.Add(_param);

                _param = new BCSParameterDataInfoRequest.PARAMETERc();
                _param.EQUIPMENTNO = "L5";
                _param.NAME = "ProcessTime_ReplyTime";
                _trx.BODY.PARAMETERLIST.Add(_param);

                _param = new BCSParameterDataInfoRequest.PARAMETERc();
                _param.EQUIPMENTNO = "00";
                _param.NAME = "WarringTime";
                _trx.BODY.PARAMETERLIST.Add(_param);

                _param = new BCSParameterDataInfoRequest.PARAMETERc();
                _param.EQUIPMENTNO = "00";
                _param.NAME = "SendToCassetteTime";
                _trx.BODY.PARAMETERLIST.Add(_param);
                #endregion

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #endregion

                #region BCSParameterDataInfoReply

                string _respXml = _resp.Xml;

                BCSParameterDataInfoReply _reply = (BCSParameterDataInfoReply)Spec.CheckXMLFormat(_respXml);

                foreach (BCSParameterDataInfoReply.PARAMETERc _replyParam in _reply.BODY.PARAMETERLIST)
                {
                    switch (_replyParam.NAME)
                    {
                        case "ProcessRecipe":

                            if (_replyParam.EQUIPMENTNO == "L4") txtProcessRecipe_ELA01_Real.Text = _replyParam.VALUE.ToString();
                            else if (_replyParam.EQUIPMENTNO == "L5") txtProcessRecipe_ELA02_Real.Text = _replyParam.VALUE.ToString();
                            break;

                        case "CompensationTime":
                            txtCompensationTime_Real.Text = _replyParam.VALUE.ToString();
                            break;

                        case "ProcessTime_RecipeID":

                            if (_replyParam.EQUIPMENTNO == "L4") txtProcessTimeRecipe_ELA01.Text = _replyParam.VALUE.ToString();
                            else if (_replyParam.EQUIPMENTNO == "L5") txtProcessTimeRecipe_ELA02.Text = _replyParam.VALUE.ToString();                            
                            break;

                        case "ProcessTime_ReplyTime":

                            if (_replyParam.EQUIPMENTNO == "L4") txtProcessTimeReplyTime_ELA01.Text = _replyParam.VALUE.ToString();
                            else if (_replyParam.EQUIPMENTNO == "L5") txtProcessTimeReplyTime_ELA02.Text = _replyParam.VALUE.ToString();                            
                            break;

                        case "WarringTime":

                            txtOverQTime_Waring_Real.Text = _replyParam.VALUE.ToString();
                            break;

                        case "SendToCassetteTime":

                            txtOverQTime_SendCST_Real.Text = _replyParam.VALUE.ToString();
                            break;


                        default :
                            break;
                    }
                }

                #endregion

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Send_BCSParameterDataChangeRequest(string ItemName)
        {
            try
            {
                #region Send BCSParameterDataChangeRequest

                BCSParameterDataChangeRequest _trx = new BCSParameterDataChangeRequest();

                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                

                switch (ItemName)
                {

                    case "ProcessRecipe":

                        #region BC Query ELA Process Recipe

                        BCSParameterDataChangeRequest.PARAMETERc _procRecipeParam;

                        if (txtProcessRecipe_ELA01_New.Text != string.Empty)
                        {
                            _procRecipeParam = new BCSParameterDataChangeRequest.PARAMETERc();

                            _procRecipeParam.EQUIPMENTNO = "L4";
                            _procRecipeParam.NAME = "ProcessRecipe";
                            _procRecipeParam.VALUE = txtProcessRecipe_ELA01_New.Text.ToString();

                            _trx.BODY.PARAMETERLIST.Add(_procRecipeParam);
                        }

                        if (txtProcessRecipe_ELA02_New.Text != string.Empty)
                        {
                            _procRecipeParam = new BCSParameterDataChangeRequest.PARAMETERc();

                            _procRecipeParam.EQUIPMENTNO = "L5";
                            _procRecipeParam.NAME = "ProcessRecipe";
                            _procRecipeParam.VALUE = txtProcessRecipe_ELA02_New.Text.ToString();

                            _trx.BODY.PARAMETERLIST.Add(_procRecipeParam);
                        }

                        break;

                        #endregion

                    case "CompensationTime":

                        #region Compensation Time

                        BCSParameterDataChangeRequest.PARAMETERc _comTimeParam = new BCSParameterDataChangeRequest.PARAMETERc();

                        _comTimeParam.EQUIPMENTNO = "00";
                        _comTimeParam.NAME = "CompensationTime";
                        _comTimeParam.VALUE = txtCompensationTime_New.Text.ToString();

                        _trx.BODY.PARAMETERLIST.Add(_comTimeParam);                        

                        break;

                        #endregion

                    case "Over Q-Time":

                        #region Over Q-Time

                        BCSParameterDataChangeRequest.PARAMETERc _qTimeParam ;

                        if (txtOverQTime_Waring_New.Text.ToString() != string.Empty)
                        {
                            _qTimeParam = new BCSParameterDataChangeRequest.PARAMETERc();

                            _qTimeParam.EQUIPMENTNO = "00";
                            _qTimeParam.NAME = "WarringTime";
                            _qTimeParam.VALUE = txtOverQTime_Waring_New.Text.ToString();

                            _trx.BODY.PARAMETERLIST.Add(_qTimeParam);   
                        }

                        if (txtOverQTime_SendCST_New.Text.ToString() != string.Empty)
                        {
                            _qTimeParam = new BCSParameterDataChangeRequest.PARAMETERc();
                            _qTimeParam.EQUIPMENTNO = "00";
                            _qTimeParam.NAME = "SendToCassetteTime";
                            _qTimeParam.VALUE = txtOverQTime_SendCST_New.Text.ToString();

                            _trx.BODY.PARAMETERLIST.Add(_qTimeParam);   
                        }              

                        break;

                        #endregion

                    default:
                        break;
                }

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;


                ShowMessage(this, lblCaption.Text, "", "BCS Parameter Data Send to BC Success !", MessageBoxIcon.Information);

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
