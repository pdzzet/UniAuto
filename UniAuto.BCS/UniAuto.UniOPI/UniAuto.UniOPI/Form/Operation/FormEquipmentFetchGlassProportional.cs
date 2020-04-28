using System;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using UniAuto.UniBCS.OpiSpec;

namespace UniOPI
{
    public partial class FormEquipmentFetchGlassProportional : FormBase
    {
        Node IndexerNode = null;

        public FormEquipmentFetchGlassProportional()
        {
            InitializeComponent();
        }

        private void FormEquipmentFetchGlassProportional_Load(object sender, EventArgs e)
        {
            try
            {
                //找尋IndexerNode，若為null則停止timer
                if (FormMainMDI.G_OPIAp.CurLine.IndexerNode == null)
                {
                    tmrBaseRefresh.Enabled = false;
                    ShowMessage(this, lblCaption.Text, "", "Can't find indexer equipment", MessageBoxIcon.Error);
                    return;
                }

                IndexerNode = FormMainMDI.G_OPIAp.CurLine.IndexerNode;

                //取得Radio Button所需要的資料(來自OPI Parameter)
                UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBBRMCtx;
                var rbtnProcessType = (from p in ctxBRM.SBRM_OPI_PARAMETER where p.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType && p.KEYWORD == "FetchProportionalName" select p).ToList();

                string[] rbtnAttibute = null;
                if (rbtnProcessType.Count > 0)
                {
                    rbtnAttibute = rbtnProcessType[0].ITEMVALUE.Split(',');

                    //找尋畫面中特定FlowLayoutPanel，動態產生Radio Button 
                    foreach (Control TLP in this.tlpBase.Controls)
                    {
                        if (TLP is TableLayoutPanel)
                        {
                            foreach (Control TLPContent in TLP.Controls)
                            {
                                if (TLPContent is GroupBox)
                                {
                                    foreach (Control _tlp in TLPContent.Controls)
                                    {
                                        if (_tlp is FlowLayoutPanel)
                                        {
                                            for (int i = 0; i < rbtnAttibute.Length; i++)
                                            {
                                                string[] radioContent = rbtnAttibute[i].Split(':');
                                                RadioButton newRadioButton = new RadioButton();
                                                newRadioButton.Name = "rbtn" + radioContent[1] + radioContent[0].PadLeft(2, '0');
                                                newRadioButton.Text = radioContent[1];
                                                newRadioButton.Tag = i;
                                                newRadioButton.Font = new System.Drawing.Font("Calibri", 12.75f);
                                                newRadioButton.AutoSize = false;
                                                newRadioButton.Size = new System.Drawing.Size(120, 30);
                                                _tlp.Controls.Add(newRadioButton);

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void btn_Click(object sender, EventArgs e)
        {
            
            int loadLock01 = -1, loadLock02 = -1;
            try
            {
                Button _btn = (Button)sender;
                switch (_btn.Tag.ToString())
                {
                        //向BC取得資料
                    case "Query":
                        Send_EquipmentFetchGlassRuleRequest();
                        break;

                    case "Setup":
                        {
                            //
                            getLoadLockNumber(ref loadLock01, ref loadLock02, "search");

                            if (loadLock01 == -1 || loadLock02 == -1)
                            {
                                ShowMessage(this, lblCaption.Text, "", "Please choose Process Type", MessageBoxIcon.Error);
                                return;
                            }
                            if (String.IsNullOrEmpty(txtPortionalValue03.Text)|| String.IsNullOrEmpty(txtPortionalValue04.Text))
                            {
                                ShowMessage(this, lblCaption.Text, "", "Please Enter Portional Value", MessageBoxIcon.Error);
                                if (txtPortionalValue03.Text.IsNormalized())
                                    txtPortionalValue03.Focus();
                                if (txtPortionalValue04.Text.IsNormalized() && txtPortionalValue03.Text.IsNormalized() == false)
                                    txtPortionalValue04.Focus();
                                return;
                            }

                            string msg = string.Format("Please confirm whether you will process the Equipment Fetch Glass Proportional ?");
                            if (DialogResult.No == this.QuectionMessage(this, lblCaption.Text, msg)) return;
                            if (DialogResult.Cancel == this.ConfirmPassword(this, lblCaption.Text)) return;

                            EquipmentFetchGlassCommand _trx = new EquipmentFetchGlassCommand();
                            _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                            _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                            _trx.BODY.EQUIPMENTNO = IndexerNode.NodeNo;
                            _trx.BODY.RULENAME1 = loadLock01.ToString();
                            _trx.BODY.RULENAME2 = loadLock02.ToString();
                            _trx.BODY.RULEVALUE1 = txtPortionalValue03.Text;
                            _trx.BODY.RULEVALUE2 = txtPortionalValue04.Text;

                            MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID, _trx.HEADER.MESSAGENAME, _trx.WriteToXml(), 0);
                            if (_resp == null)
                            {
                                return;
                            }

                            string _respXml = _resp.Xml;

                            EquipmentFetchGlassCommandReply _equipmentFetchGlassCommandReply = (EquipmentFetchGlassCommandReply)Spec.CheckXMLFormat(_respXml);

                            ShowMessage(this, lblCaption.Text, "", string.Format("Equipment Fetch Glass Command Send to BC Success"), MessageBoxIcon.Information);
                            Send_EquipmentFetchGlassRuleRequest();

                        }
                        break;

                    case "Clear":
                        getLoadLockNumber(ref loadLock01, ref loadLock02, "Clear");
                        txtPortionalValue03.Text = string.Empty;
                        txtPortionalValue04.Text = string.Empty;
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            if (FormMainMDI.G_OPIAp.CurLine.IndexerNode == null)
            {
                tmrBaseRefresh.Enabled = false;
                return;
            }
            try
            {
                BCS_EquipmentFetchGlassRuleReply _reply = 
                    FormMainMDI.G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply;

                if (_reply.IsReply)
                {
                    DateTime _now = DateTime.Now;
                    TimeSpan _ts = _now.Subtract(_reply.LastRequestDate).Duration();

                    if (_ts.Seconds > FormMainMDI.G_OPIAp.SocketMonitorWaitTime)
                    {
                        _reply.LastRequestDate = DateTime.Now;
                        Send_EquipmentFetchGlassRuleRequest();
                    }
                 
                }
                 RefreshData();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void Send_EquipmentFetchGlassRuleRequest()
        {
            try
            {
                string _err = string.Empty;
                string _xml = string.Empty;

                FormMainMDI.G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.IsReply = false;

                EquipmentFetchGlassRuleRequest _trx = new EquipmentFetchGlassRuleRequest();
                _trx.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                _trx.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                if (FormMainMDI.G_OPIAp.CurLine.IndexerNode != null)
                    _trx.BODY.EQUIPMENTNO = FormMainMDI.G_OPIAp.CurLine.IndexerNode.NodeNo;
                else
                    return;
                _xml = _trx.WriteToXml();

                MessageResponse _resp = this.SendRequestResponse(_trx.HEADER.TRANSACTIONID,_trx.HEADER.MESSAGENAME,_trx.WriteToXml(),0);

                if (_resp == null) return;

                string _respXml = _resp.Xml;

                EquipmentFetchGlassRuleReply _equipmentFetchGlassRuleReply = (EquipmentFetchGlassRuleReply)Spec.CheckXMLFormat(_respXml);
               
                try
                {
                    FormMainMDI.G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.RuleName_1 = _equipmentFetchGlassRuleReply.BODY.RULENAME1;
                    FormMainMDI.G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.RuleName_2 = _equipmentFetchGlassRuleReply.BODY.RULENAME2;
                    FormMainMDI.G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.RuleValue_1 = _equipmentFetchGlassRuleReply.BODY.RULEVALUE1;
                    FormMainMDI.G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.RuleValue_2 = _equipmentFetchGlassRuleReply.BODY.RULEVALUE2;
                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                    ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                }
                
            }
            catch (Exception ex)
            {
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void RefreshData()
        {
            try
            {
                if (FormMainMDI.G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.RuleName_1 == "" || FormMainMDI.G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.RuleName_2 == "")
                    return;

                int loadlock01 = -1, loadlock02 = -1;
                loadlock01 = int.Parse(FormMainMDI.G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.RuleName_1);
                loadlock02 = int.Parse(FormMainMDI.G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.RuleName_2);

                getLoadLockNumber(ref loadlock01, ref loadlock02, "find");
            }
            catch (Exception ex)
            {
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        public void getLoadLockNumber(ref int loadLock01, ref int loadLock02, string action)
        {
            try
            {
                //找尋相對應的radio button 位置
                if (action == "find")
                {
                    foreach (Control controlRadioButton01 in this.flpQuery01.Controls)
                    {
                        RadioButton radioSetup01 = controlRadioButton01 as RadioButton;
                        if (int.Parse(radioSetup01.Tag.ToString()) == loadLock01)
                        {
                            foreach (Control controlRadioButton02 in this.flpQuery02.Controls)
                            {
                                RadioButton radioSetup02 = controlRadioButton02 as RadioButton;
                                if (int.Parse(radioSetup02.Tag.ToString()) == loadLock02)
                                {
                                    radioSetup01.Checked = true;
                                    radioSetup02.Checked = true;
                                    txtPortionalValue01.Text = FormMainMDI.G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.RuleValue_1;
                                    txtPortionalValue02.Text = FormMainMDI.G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.RuleValue_2;
                                    return;
                                }
                            }
                        }
                    }
                }
                else
                {
                    //search 取得已勾選的radio button位置 clear 清除已勾選的選項
                    foreach (Control controlRadioButton in this.flpSetup01.Controls)
                    {
                        RadioButton radioSetup01 = controlRadioButton as RadioButton;
                        if (radioSetup01.Checked == true)
                        {

                            foreach (Control controlRadioButton02 in this.flpSetup02.Controls)
                            {
                                RadioButton radioSetup02 = controlRadioButton02 as RadioButton;
                                if (radioSetup02.Checked == true)
                                {
                                    if (action == "search")
                                    {
                                        loadLock01 = int.Parse(radioSetup01.Tag.ToString());
                                        loadLock02 = int.Parse(radioSetup02.Tag.ToString());
                                        //MessageBox.Show("選到一二 "+radioSetup01.Text+": "+loadLock01+"\n"+radioSetup02.Text+": "+loadLock02);
                                    }
                                    else if (action == "Clear")
                                    {
                                        radioSetup01.Checked = false;
                                        radioSetup02.Checked = false;
                                        loadLock01 = -1;
                                        loadLock02 = -1;
                                        //MessageBox.Show("清除一 二");
                                    }
                                    return;
                                }

                            }
                            radioSetup01.Checked = false;
                            loadLock01 = -1;
                            //MessageBox.Show("清除一");
                            return;
                        }
                    }
                    foreach (Control controlRadioButton02 in this.grbSetup02.Controls)
                    {
                        RadioButton radioSetup02 = controlRadioButton02 as RadioButton;
                        if (radioSetup02.Checked == true)
                        {
                            radioSetup02.Checked = false;
                            loadLock02 = -1;
                            //MessageBox.Show("清除二");
                        }
                    }
                    //MessageBox.Show("都沒選");
                }
            }
            catch (Exception ex)
            {
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
            }
        }

        private void txtKeyPress(object sender, KeyPressEventArgs e)
        {
            UniTools.CheckTextBoxKeyPressIsInteger(sender, e);
        }
    }
}
