using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;
using System.Text;

namespace UniOPI
{
    public partial class FormUnloadingPortSetting : FormBase
    {
        string CurNodeNo = string.Empty;
        string CurPortNo = string.Empty;

        public FormUnloadingPortSetting()
        {
            InitializeComponent();
        }

        private void FormUnloadingPortSetting_Load(object sender, EventArgs e)
        {
            InitialProdTypeItem();

            txtCmdyOperatorID.Text = FormMainMDI.G_OPIAp.LoginUserID;
        }

        private void btnQuery_Click(object sender, EventArgs e)
        {
            try
            {
                ClearQueryData();

                if (CurNodeNo == string.Empty && CurPortNo == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please Choose Unloader Port", MessageBoxIcon.Warning);
                    return;
                }

                SendtoBC_UnloadingPortSettingReportRequest();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        public void SetPort()
        {
            try
            {
                var _q = (from port in FormMainMDI.G_OPIAp.Dic_Port.Values
                         select new
                         {
                             IDNAME = string.Format("{0}-{1}-{2}", port.NodeNo, port.NodeID, port.PortNo),
                             port.NodeNo,
                             port.PortNo,
                             port.PortID
                         }).ToList();

                if (_q == null || _q.Count == 0)
                {
                    cboPort.DataSource = null;
                    return;
                }

                cboPort.SelectedIndexChanged -= cboPort_SelectedIndexChanged;
                cboPort.DataSource = _q;
                cboPort.DisplayMember = "IDNAME";
                cboPort.ValueMember = "PORTNO";
                cboPort.SelectedIndex = -1;
                cboPort.SelectedIndexChanged += cboPort_SelectedIndexChanged;

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void cboPort_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                if (cboPort.SelectedIndex < 0 || cboPort.SelectedValue == null)
                {
                    CurNodeNo = string.Empty;
                    CurPortNo = string.Empty;
                    return;
                }

                CurNodeNo = ((dynamic)cboPort.SelectedItem).NodeNo;
                CurPortNo = ((dynamic)cboPort.SelectedItem).PortNo;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnReloadPort_Click(object sender, EventArgs e)
        {
            SetPort();
            ClearQueryData();
            ClearCommandData();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            try
            {
                if (CheckData() == false) return;

                SendtoBC_UnloadingPortSettingCommandRequest();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            ClearCommandData();
        }

        private void txtNumber_KeyPress(object sender, KeyPressEventArgs e)
        {
            int asciiCode = (int)e.KeyChar;
            if ((asciiCode >= 48 & asciiCode <= 57) |   // 0-9
                asciiCode == 8)   // Backspace
            {
                e.Handled = false;
            }
            else
                e.Handled = true;
        }

        private void InitialProdTypeItem()
        {
            try
            {
                #region Product Type Check Mode
                List<comboxInfo> _lstItems = new List<comboxInfo>();

                _lstItems.Add(new comboxInfo { ITEM_ID = "1", ITEM_NAME = "Product Type Check" });
                _lstItems.Add(new comboxInfo { ITEM_ID = "2", ITEM_NAME = "No Product Type Check" });

                cboCmdProductType_OK.DataSource = _lstItems.ToList();
                cboCmdProductType_OK.DisplayMember = "ITEM_DESC";
                cboCmdProductType_OK.ValueMember = "ITEM_ID";
                cboCmdProductType_OK.SelectedIndex = -1;

                cboCmdProductType_PD.DataSource = _lstItems.ToList();
                cboCmdProductType_PD.DisplayMember = "ITEM_DESC";
                cboCmdProductType_PD.ValueMember = "ITEM_ID";
                cboCmdProductType_PD.SelectedIndex = -1;

                cboCmdProductType_RP.DataSource = _lstItems.ToList();
                cboCmdProductType_RP.DisplayMember = "ITEM_DESC";
                cboCmdProductType_RP.ValueMember = "ITEM_ID";
                cboCmdProductType_RP.SelectedIndex = -1;

                cboCmdProductType_IR.DataSource = _lstItems.ToList();
                cboCmdProductType_IR.DisplayMember = "ITEM_DESC";
                cboCmdProductType_IR.ValueMember = "ITEM_ID";
                cboCmdProductType_IR.SelectedIndex = -1;

                cboCmdProductType_NG.DataSource = _lstItems.ToList();
                cboCmdProductType_NG.DisplayMember = "ITEM_DESC";
                cboCmdProductType_NG.ValueMember = "ITEM_ID";
                cboCmdProductType_NG.SelectedIndex = -1;

                cboCmdProductType_MIX.DataSource = _lstItems.ToList();
                cboCmdProductType_MIX.DisplayMember = "ITEM_DESC";
                cboCmdProductType_MIX.ValueMember = "ITEM_ID";
                cboCmdProductType_MIX.SelectedIndex = -1;
                #endregion

                #region Port Judge
                _lstItems = new List<comboxInfo>();

                _lstItems.Add(new comboxInfo { ITEM_ID = "1", ITEM_NAME = "Product Type Check" });
                _lstItems.Add(new comboxInfo { ITEM_ID = "2", ITEM_NAME = "No Product Type Check" });

                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void ClearQueryData()
        {
            try
            {
                txtQueryQTime_OK.Text = string.Empty;
                txtQueryProductType_OK.Text = string.Empty;

                txtQueryQTime_PD.Text = string.Empty;
                txtQueryProductType_PD.Text = string.Empty;

                txtQueryQTime_RP.Text = string.Empty;
                txtQueryProductType_RP.Text = string.Empty;

                txtQueryQTime_NG.Text = string.Empty;
                txtQueryProductType_NG.Text = string.Empty;
                chkQueryPortJudge_NG02.Checked = false;
                chkQueryPortJudge_NG02.Checked = false;

                txtQueryQTime_IR.Text = string.Empty;
                txtQueryProductType_IR.Text = string.Empty;

                txtQueryQTime_MIX.Text = string.Empty;
                txtQueryProductType_MIX.Text = string.Empty;
                chkQueryPortJudge_MIX00.Checked = false;
                chkQueryPortJudge_MIX01.Checked = false;
                chkQueryPortJudge_MIX02.Checked = false;
                chkQueryPortJudge_MIX03.Checked = false;
                chkQueryPortJudge_MIX04.Checked = false;
                chkQueryPortJudge_MIX05.Checked = false;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void ClearCommandData()
        {
            try
            {
                txtCmdQTime_OK.Text = string.Empty;
                cboCmdProductType_OK.SelectedIndex = -1;

                txtCmdQTime_PD.Text = string.Empty;
                cboCmdProductType_PD.SelectedIndex = -1;

                txtCmdQTime_RP.Text = string.Empty;
                cboCmdProductType_RP.SelectedIndex = -1;

                txtCmdQTime_IR.Text = string.Empty;
                cboCmdProductType_IR.SelectedIndex = -1;

                txtCmdQTime_NG.Text = string.Empty;
                cboCmdProductType_NG.SelectedIndex = -1;
                chkCmdPortJudge_NG02.Checked = false;
                chkCmdPortJudge_NG03.Checked = false;

                txtCmdQTime_MIX.Text = string.Empty;
                cboCmdProductType_MIX.SelectedIndex = -1;
                chkCmdPortJudge_MIX00.Checked = false;
                chkCmdPortJudge_MIX01.Checked = false;
                chkCmdPortJudge_MIX02.Checked = false;
                chkCmdPortJudge_MIX03.Checked = false;
                chkCmdPortJudge_MIX04.Checked = false;
                chkCmdPortJudge_MIX05.Checked = false;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private bool CheckData()
        {
            try
            {
                int _num = 0;

                if (CurNodeNo == string.Empty && CurPortNo == string.Empty)
                {
                    ShowMessage(this, lblCaption.Text, "", "Please Choose Unloader Port", MessageBoxIcon.Warning);
                    return false ;
                }

                #region OK PORT
                //Store Q Time有設定=>product type check mode須設定
                if (txtCmdQTime_OK.Text.Trim() != string.Empty)
                {
                    if (cboCmdProductType_OK.SelectedValue == null)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please Choose OK Port Product Type Check Mode", MessageBoxIcon.Error);
                        txtCmdQTime_OK.Focus();
                        return false;
                    }
                }

                int.TryParse(txtCmdQTime_OK.Text.ToString(), out _num);

                if (_num > 65535)
                {
                    ShowMessage(this, lblCaption.Text, "", "OK  Port Sore Q Time <=65535 ", MessageBoxIcon.Error);
                    txtCmdQTime_OK.Focus();
                    return false;
                }

                //Product Type Check 時，Store Q Time不可為empty
                if (cboCmdProductType_OK.SelectedValue != null)
                {
                    if (cboCmdProductType_OK.SelectedValue.ToString() == "1" && txtCmdQTime_OK.Text.ToString()==string.Empty )
                    {
                        ShowMessage(this, lblCaption.Text, "", "0 <= OK  Port Sore Q Time <=65535 ", MessageBoxIcon.Error);
                        txtCmdQTime_OK.Focus();
                        return false;
                    }
                }
                #endregion

                #region PD PORT
                //Store Q Time有設定=>product type check mode須設定
                if (txtCmdQTime_PD.Text.Trim() != string.Empty)
                {
                    if (cboCmdProductType_PD.SelectedValue == null)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please Choose PD Port Product Type Check Mode", MessageBoxIcon.Error);
                        txtCmdQTime_PD.Focus();
                        return false;
                    }
                }

                int.TryParse(txtCmdQTime_PD.Text.ToString(), out _num);

                if (_num > 65535)
                {
                    ShowMessage(this, lblCaption.Text, "", "PD Port Sore Q Time <=65535 ", MessageBoxIcon.Error);
                    txtCmdQTime_PD.Focus();
                    return false;
                }

                //Product Type Check 時，Store Q Time不可為empty
                if (cboCmdProductType_PD.SelectedValue != null)
                {
                    if (cboCmdProductType_PD.SelectedValue.ToString() == "1" && txtCmdQTime_PD.Text.ToString() == string.Empty )
                    {
                        ShowMessage(this, lblCaption.Text, "", "0 <= PD  Port Sore Q Time <=65535 ", MessageBoxIcon.Error);
                        txtCmdQTime_PD.Focus();
                        return false;
                    }
                }
                #endregion

                #region RP PORT
                //Store Q Time有設定=>product type check mode須設定
                if (txtCmdQTime_RP.Text.Trim() != string.Empty)
                {
                    if (cboCmdProductType_RP.SelectedValue == null)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please Choose RP Port Product Type Check Mode", MessageBoxIcon.Error);
                        txtCmdQTime_RP.Focus();
                        return false;
                    }
                }

                int.TryParse(txtCmdQTime_RP.Text.ToString(), out _num);

                if (_num > 65535)
                {
                    ShowMessage(this, lblCaption.Text, "", "RP Port Sore Q Time <=65535 ", MessageBoxIcon.Error);
                    txtCmdQTime_RP.Focus();
                    return false;
                }

                //Product Type Check 時，Store Q Time不可為empty
                if (cboCmdProductType_RP.SelectedValue != null)
                {
                    if (cboCmdProductType_RP.SelectedValue.ToString() == "1" && txtCmdQTime_RP.Text.ToString() == string.Empty )
                    {
                        ShowMessage(this, lblCaption.Text, "", "0 <= RP  Port Sore Q Time <=65535 ", MessageBoxIcon.Error);
                        txtCmdQTime_RP.Focus();
                        return false;
                    }
                }
                #endregion

                #region IR PORT
                //Store Q Time有設定=>product type check mode須設定
                if (txtCmdQTime_IR.Text.Trim() != string.Empty)
                {
                    if (cboCmdProductType_IR.SelectedValue == null)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please Choose IR Port Product Type Check Mode", MessageBoxIcon.Error);
                        txtCmdQTime_IR.Focus();
                        return false;
                    }
                }

                int.TryParse(txtCmdQTime_IR.Text.ToString(), out _num);
                

                if (_num > 65535)
                {
                    ShowMessage(this, lblCaption.Text, "", "IR Port Sore Q Time <=65535 ", MessageBoxIcon.Error);
                    txtCmdQTime_IR.Focus();
                    return false;
                }

                //Product Type Check 時，Store Q Time不可為Empty
                if (cboCmdProductType_IR.SelectedValue != null)
                {
                    if (cboCmdProductType_IR.SelectedValue.ToString() == "1" && txtCmdQTime_IR.Text.ToString() == string.Empty )
                    {
                        ShowMessage(this, lblCaption.Text, "", "0 <= IR  Port Sore Q Time <=65535 ", MessageBoxIcon.Error);
                        txtCmdQTime_IR.Focus();
                        return false;
                    }
                }
                #endregion

                #region NG PORT
                //Store Q Time有設定=>product type check mode須設定
                if (txtCmdQTime_NG.Text.Trim() != string.Empty)
                {
                    if (cboCmdProductType_NG.SelectedValue == null)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please Choose NG Port Product Type Check Mode", MessageBoxIcon.Error);
                        txtCmdQTime_NG.Focus();
                        return false;
                    }
                }

                int.TryParse(txtCmdQTime_NG.Text.ToString(), out _num);

                if (_num > 65535)
                {
                    ShowMessage(this, lblCaption.Text, "", "NG Port Sore Q Time <=65535 ", MessageBoxIcon.Error);
                    txtCmdQTime_NG.Focus();
                    return false;
                }

                //Product Type Check 時，Store Q Time不可為0 , Port Judge至少勾選一個
                if (cboCmdProductType_NG.SelectedValue != null)
                {
                    if (cboCmdProductType_NG.SelectedValue.ToString() == "1" && txtCmdQTime_NG.Text.ToString() == string.Empty )
                    {
                        ShowMessage(this, lblCaption.Text, "", "0 <= NG  Port Sore Q Time <=65535 ", MessageBoxIcon.Error);
                        txtCmdQTime_NG.Focus();
                        return false;
                    }

                    if (chkCmdPortJudge_NG02.Checked == false && chkCmdPortJudge_NG03.Checked == false)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please Choose NG Port Judge ", MessageBoxIcon.Error);
                        return false;
                    }
                }

                if (chkCmdPortJudge_NG02.Checked == true || chkCmdPortJudge_NG03.Checked == true)
                {
                    if (cboCmdProductType_NG.SelectedValue== null)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please Choose NG Port Product Type Check Mode", MessageBoxIcon.Error);
                        return false;
                    }
                }
                #endregion

                #region MIX PORT
                //Store Q Time有設定=>product type check mode須設定
                if (txtCmdQTime_MIX.Text.Trim() != string.Empty)
                {
                    if (cboCmdProductType_MIX.SelectedValue == null)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please Choose MIX Port Product Type Check Mode", MessageBoxIcon.Error);
                        txtCmdQTime_MIX.Focus();
                        return false;
                    }
                }

                int.TryParse(txtCmdQTime_MIX.Text.ToString(), out _num);

                if (_num > 65535)
                {
                    ShowMessage(this, lblCaption.Text, "", "MIX Port Sore Q Time <=65535 ", MessageBoxIcon.Error);
                    txtCmdQTime_MIX.Focus();
                    return false;
                }

                //Product Type Check 時，Store Q Time不可為0 , Port Judge至少勾選一個
                if (cboCmdProductType_MIX.SelectedValue != null)
                {
                    if (cboCmdProductType_MIX.SelectedValue.ToString() == "1" && txtCmdQTime_MIX.Text.ToString() == string.Empty )
                    {
                        ShowMessage(this, lblCaption.Text, "", "0 <= MIX  Port Sore Q Time <=65535 ", MessageBoxIcon.Error);
                        txtCmdQTime_MIX.Focus();
                        return false;
                    }

                    if (chkCmdPortJudge_MIX00.Checked == false && chkCmdPortJudge_MIX01.Checked == false && chkCmdPortJudge_MIX02.Checked ==false &&
                        chkCmdPortJudge_MIX03.Checked == false && chkCmdPortJudge_MIX04.Checked == false && chkCmdPortJudge_MIX05.Checked == false)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please Choose MIX Port Judge ", MessageBoxIcon.Error);
                        return false;
                    }
                }

                if (chkCmdPortJudge_MIX00.Checked == true || chkCmdPortJudge_MIX01.Checked == true || chkCmdPortJudge_MIX02.Checked == true ||
                    chkCmdPortJudge_MIX03.Checked == true || chkCmdPortJudge_MIX04.Checked == true || chkCmdPortJudge_MIX05.Checked == true)
                {
                    if (cboCmdProductType_MIX.SelectedValue == null)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Please Choose MIX Port Product Type Check Mode", MessageBoxIcon.Error);
                        return false;
                    }
                }
                #endregion

                return true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                return false;
            }
        }

        private string GetProductTypeCheckMode(string CheckMode)
        {
            switch (CheckMode.Trim())
            {
                case "1":  return "1:Product Type Check";
                case "2": return "2:No Product Type Check";
                default: return CheckMode;
            }
        }

        private void SendtoBC_UnloadingPortSettingReportRequest()
        {
            try
            {
                string _bitData = string.Empty;

                #region UnloadingPortSettingReportRequest
                UnloadingPortSettingReportRequest _trx = new UnloadingPortSettingReportRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                _trx.BODY.EQUIPMENTNO = CurNodeNo;
                _trx.BODY.PORTNO = CurPortNo;

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #endregion

                #region UnloadingPortSettingReportReply

                string _respXml = _resp.Xml;

                UnloadingPortSettingReportReply _unloadingPortSettingReportReply = (UnloadingPortSettingReportReply)Spec.CheckXMLFormat(_respXml);
                txtQueryQTime_OK.Text = _unloadingPortSettingReportReply.BODY.OKSTOREQTIME;
                txtQueryProductType_OK.Text = GetProductTypeCheckMode(_unloadingPortSettingReportReply.BODY.OKPRODUCTTYPECHECKMODE);

                txtQueryQTime_PD.Text = _unloadingPortSettingReportReply.BODY.PDSTOREQTIME;
                txtQueryProductType_PD.Text = GetProductTypeCheckMode(_unloadingPortSettingReportReply.BODY.PDPRODUCTTYPECHECKMODE);

                txtQueryQTime_RP.Text = _unloadingPortSettingReportReply.BODY.RPSTOREQTIME;
                txtQueryProductType_RP.Text = GetProductTypeCheckMode(_unloadingPortSettingReportReply.BODY.RPPRODUCTTYPECHECKMODE);

                txtQueryQTime_NG.Text = _unloadingPortSettingReportReply.BODY.NGSTOREQTIME;
                txtQueryProductType_NG.Text = GetProductTypeCheckMode(_unloadingPortSettingReportReply.BODY.NGPRODUCTTYPECHECKMODE);

                _bitData = _unloadingPortSettingReportReply.BODY.NGPORTJUDGE.ToString().PadLeft(16, '0');
                //_bitData = UniTools.ReverseStr(_unloadingPortSettingReportReply.BODY.NGPORTJUDGE.ToString().PadLeft(16, '0'));

                if (_bitData.Substring(2, 1) == "1") chkQueryPortJudge_NG02.Checked = true;
                else chkQueryPortJudge_NG02.Checked = false;

                if (_bitData.Substring(3, 1) == "1") chkQueryPortJudge_NG03.Checked = true;
                else chkQueryPortJudge_NG03.Checked = false;


                txtQueryQTime_IR.Text = _unloadingPortSettingReportReply.BODY.IRSTOREQTIME;
                txtQueryProductType_IR.Text = GetProductTypeCheckMode(_unloadingPortSettingReportReply.BODY.IRPRODUCTTYPECHECKMODE);

                txtQueryQTime_MIX.Text = _unloadingPortSettingReportReply.BODY.MIXSTOREQTIME;
                txtQueryProductType_MIX.Text = GetProductTypeCheckMode(_unloadingPortSettingReportReply.BODY.MIXPRODUCTTYPECHECKMODE);

                _bitData = _unloadingPortSettingReportReply.BODY.MIXPORTJUDGE.ToString().PadLeft(16, '0');
                //_bitData = UniTools.ReverseStr(_unloadingPortSettingReportReply.BODY.MIXPORTJUDGE.ToString().PadLeft(16, '0'));

                if (_bitData.Substring(0, 1) == "1") chkQueryPortJudge_MIX00.Checked = true;
                else chkQueryPortJudge_MIX00.Checked = false;

                if (_bitData.Substring(1, 1) == "1") chkQueryPortJudge_MIX01.Checked = true;
                else chkQueryPortJudge_MIX01.Checked = false;

                if (_bitData.Substring(2, 1) == "1") chkQueryPortJudge_MIX02.Checked = true;
                else chkQueryPortJudge_MIX02.Checked = false;

                if (_bitData.Substring(3, 1) == "1") chkQueryPortJudge_MIX03.Checked = true;
                else chkQueryPortJudge_MIX03.Checked = false;

                if (_bitData.Substring(4, 1) == "1") chkQueryPortJudge_MIX04.Checked = true;
                else chkQueryPortJudge_MIX04.Checked = false;

                if (_bitData.Substring(5, 1) == "1") chkQueryPortJudge_MIX05.Checked = true;
                else chkQueryPortJudge_MIX05.Checked = false;

                txtQueryOperatorID.Text = _unloadingPortSettingReportReply.BODY.OPERATORID;
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SendtoBC_UnloadingPortSettingCommandRequest()
        {
            try
            {
                string _bitData = "0".PadLeft(16, '0');

                StringBuilder _str = new StringBuilder(_bitData);

                #region Send Unloading Port Setting Command Request

                UnloadingPortSettingCommandRequest _trx = new UnloadingPortSettingCommandRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                _trx.BODY.EQUIPMENTNO = CurNodeNo;
                _trx.BODY.PORTNO = CurPortNo;
                _trx.BODY.OKSTOREQTIME = txtCmdQTime_OK.Text.ToString().Trim() == string.Empty ? "0" : txtCmdQTime_OK.Text.ToString().Trim();
                _trx.BODY.OKPRODUCTTYPECHECKMODE = cboCmdProductType_OK.SelectedValue==null ? "0" : cboCmdProductType_OK.SelectedValue.ToString();

                _trx.BODY.PDSTOREQTIME = txtCmdQTime_PD.Text.ToString().Trim() == string.Empty ? "0" : txtCmdQTime_PD.Text.ToString().Trim();
                _trx.BODY.PDPRODUCTTYPECHECKMODE = cboCmdProductType_PD.SelectedValue==null ? "0" : cboCmdProductType_PD.SelectedValue.ToString();

                _trx.BODY.RPSTOREQTIME = txtCmdQTime_RP.Text.ToString().Trim() == string.Empty ? "0" : txtCmdQTime_RP.Text.ToString().Trim();
                _trx.BODY.RPPRODUCTTYPECHECKMODE = cboCmdProductType_RP.SelectedValue==null ? "0" : cboCmdProductType_RP.SelectedValue.ToString();

                _trx.BODY.IRSTOREQTIME = txtCmdQTime_IR.Text.ToString().Trim() == string.Empty ? "0" : txtCmdQTime_IR.Text.ToString().Trim();
                _trx.BODY.IRPRODUCTTYPECHECKMODE = cboCmdProductType_IR.SelectedValue==null ? "0" : cboCmdProductType_IR.SelectedValue.ToString();

                _trx.BODY.NGSTOREQTIME = txtCmdQTime_NG.Text.ToString().Trim() == string.Empty ? "0" : txtCmdQTime_NG.Text.ToString().Trim();
                _trx.BODY.NGPRODUCTTYPECHECKMODE = cboCmdProductType_NG.SelectedValue==null ? "0" : cboCmdProductType_NG.SelectedValue.ToString();

                if (chkCmdPortJudge_NG02.Checked == true) _str[2] = '1';
                if (chkCmdPortJudge_NG03.Checked == true) _str[3] = '1';
                _trx.BODY.NGPORTJUDGE = _str.ToString(); // UniTools.ReverseStr(_str.ToString());


                _str = new StringBuilder(_bitData);
                _trx.BODY.MIXSTOREQTIME = txtCmdQTime_MIX.Text.ToString().Trim() == string.Empty ? "0" : txtCmdQTime_MIX.Text.ToString().Trim();
                _trx.BODY.MIXPRODUCTTYPECHECKMODE = cboCmdProductType_MIX.SelectedValue==null ? "0" : cboCmdProductType_MIX.SelectedValue.ToString();
                if (chkCmdPortJudge_MIX00.Checked == true) _str[0] = '1';
                if (chkCmdPortJudge_MIX01.Checked == true) _str[1] = '1';
                if (chkCmdPortJudge_MIX02.Checked == true) _str[2] = '1';
                if (chkCmdPortJudge_MIX03.Checked == true) _str[3] = '1';
                if (chkCmdPortJudge_MIX04.Checked == true) _str[4] = '1';
                if (chkCmdPortJudge_MIX05.Checked == true) _str[5] = '1';
                _trx.BODY.MIXPORTJUDGE = _str.ToString(); //UniTools.ReverseStr(_str.ToString());

                _trx.BODY.OPERATORID = txtCmdyOperatorID.Text.ToString();

                string _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);

                if (_resp == null) return;

                #endregion

                #region UnloadingPortSettingCommandReply

                ShowMessage(this, lblCaption.Text, "", "Unloading Port Setting Command Send to BC Success !", MessageBoxIcon.Information);

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
