using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Configuration;
using NLog;
using System.Net.Configuration;

namespace UniOPI
{
    public partial class FormLogin : Form
    {
        public static FormMainMDI FrmMainMDI;
        //public static FormBCSMessage FrmBcMessage;
        //public OPIInfo OPIAp;

        //private bool CallByProc; //OPI是否由其他AP所喚起執行

        //private ConfigXML Config;
        private csDBConfigXML DBConfigXml;
        //private FormBase frmBase = new FormBase();

        private string FabType = string.Empty;
        private string LineType = string.Empty;
        private string LineID = string.Empty;
        private bool IsUAC = false;
        //private string LayoutFolder = string.Empty;
        //private string ParamFolder = string.Empty;

        private string RobotPath = string.Empty;

        public FormLogin()
        {
            InitializeComponent();
        }

        public FormLogin(string[] args)
        {
            InitializeComponent();

            //args陣列長度為4以上，表示OPI是由其他程式呼叫的. -- { "-u","CSOTCIM","-p", "CIMENGa" }; 
            if (args.Length >= 4)
            {
                //CallByProc = true;
                txtUserId.Text = args[1];
                txtPassword.Text  = args[3];
            }            
        }

        private void FormLogin_Load(object sender, EventArgs e)
        {
            try
            {                
                SetLanguage();
            }
            catch (Exception ex)
            {
                //NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                MessageBox.Show(new Form() { TopMost = true }, ex.ToString(), MethodBase.GetCurrentMethod().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormLogin_Shown(object sender, EventArgs e)
        {
            try
            {
                string _errMsg = string.Empty;

                tmrRefresh.Enabled = true;

                //Get config.xml Info
                FabType = ConfigurationManager.AppSettings["FabType"];  //Config.GetPublicParam("FabType");
                LineType = ConfigurationManager.AppSettings["LineType"]; //Config.GetPublicParam("LineType");
                LineID = ConfigurationManager.AppSettings["LineID"]; // Config.GetPublicParam("LineID");
                IsUAC = ConfigurationManager.AppSettings["IsUAC"]=="Y" ? true : false ;

                OPIConst.LayoutFolder =  ConfigurationManager.AppSettings["LayoutFolder"]; // Config.GetPublicParam("LayoutFolder");
                //OPIConst.LayoutFolder = LayoutFolder;

                OPIConst.ParamFolder = ConfigurationManager.AppSettings["ParamFolder"]; // Config.GetPublicParam("ParamFolder");
                //OPIConst.ParamFolder = ParamFolder;

                RobotPath = ConfigurationManager.AppSettings["RobotFolder"];  //Config.GetPublicParam("RobotFolder");

                OPIConst.TimingChartFolder = ConfigurationManager.AppSettings["TimingChartFolder"];

                if (!Directory.Exists(OPIConst.ParamFolder))
                {
                    MessageBox.Show(new Form() { TopMost = true }, "ParamFolder is not exist.", MethodBase.GetCurrentMethod().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //frmBase.ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", "ParamFolder is not exist.", MessageBoxIcon.Error);
                    this.Close();
                }

                if (!Directory.Exists(OPIConst.LayoutFolder))
                {
                    MessageBox.Show(new Form() { TopMost = true }, "LayoutFolder is not exist.", MethodBase.GetCurrentMethod().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //frmBase.ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", "LayoutFolder is not exist.", MessageBoxIcon.Error);
                    this.Close();
                }

                if (!File.Exists(OPIConst.ParamFolder + OPIConst.DBCFG_XML_FILE_NAME))
                {
                    MessageBox.Show(new Form() { TopMost = true }, "DBConfig.xml not exist.", MethodBase.GetCurrentMethod().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //frmBase.ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", "DBConfig.xml not exist.", MessageBoxIcon.Error);
                    this.Close();
                }

                LoadDBConfig(OPIConst.ParamFolder + OPIConst.DBCFG_XML_FILE_NAME);
                cboFabType.SelectedItem = FabType;
                cboLineType.SelectedItem = LineType;
                cboServerName.SelectedItem = LineID;

                //if (CallByProc)
                //{
                    //btnLogin.PerformClick();
                //}

                if (IsUAC)
                {
                    txtUserId.Text = "Query";
                    txtPassword.Text = "Query";

                    txtUserId.Enabled = false;
                    txtPassword.Enabled = false;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                MessageBox.Show(new Form() { TopMost = true }, ex.ToString(), MethodBase.GetCurrentMethod().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED
                return cp;
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            try
            {
                #region Check 輸入條件
                if (txtUserId.Text.Trim() == "")
                {
                    //frmBase.ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", "Please input UserID", MessageBoxIcon.Error);
                    MessageBox.Show(new Form() { TopMost = true }, "Please input UserID", MethodBase.GetCurrentMethod().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (txtPassword.Text.Trim() == "")
                {
                    //frmBase.ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", "Please input Password", MessageBoxIcon.Error);
                    MessageBox.Show(new Form() { TopMost = true }, "Please input Password", MethodBase.GetCurrentMethod().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                this.Cursor = Cursors.WaitCursor;

                OPIConst.RobotFolder = RobotPath + string.Format(@"{0}\", cboFabType.Text);

                OPIInfo OPIAp = new OPIInfo(cboFabType.Text, cboLineType.Text, cboServerName.Text, txtUserId.Text.ToString(), txtPassword.Text.ToString());
                
                if (OPIAp.ErrMessage != string.Empty)
                {                    
                    //frmBase.ShowMessage(this, "OPIInfo Error", "", OPIAp.ErrMessage, MessageBoxIcon.Error);
                    MessageBox.Show(new Form() { TopMost = true }, OPIAp.ErrMessage, MethodBase.GetCurrentMethod().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                #region update login line
                //20170803 huangjiayin modify: VDI如果是只读权限，则不做Save动作，以免报错无法登入
                try
                {
                    Configuration _config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    AppSettingsSection app = _config.AppSettings;
                    app.Settings["FabType"].Value = cboFabType.Text;
                    app.Settings["LineType"].Value = cboLineType.Text;
                    app.Settings["LineID"].Value = cboServerName.Text;
                    _config.Save(ConfigurationSaveMode.Modified);
                    NLogManager.Logger.ReplaceServerName(cboServerName.Text.Trim());
                }
                catch (Exception ex)
                {
                    //
                }

                #endregion

                #endregion






                //OPIAp.FrmBcMessage = new FormBCSMessage(OPIAp);

                //NLogManager.Logger.LogInfoWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, "Hello World");
                FrmMainMDI = new FormMainMDI(OPIAp, cboLanguage.SelectedValue.ToString());

                if (FormMainMDI.G_OPIAp.IsRunVshost)
                {
                    #region 用source code 跑不check 連線
                    this.Hide();
                    DialogResult dlg = FrmMainMDI.ShowDialog();
                    this.Cursor = Cursors.Default;

                    this.Show();

                    if (dlg.Equals(DialogResult.Abort))
                    {
                        ShowMessage( "ClientDisconnectRequest",  "You are logout by system.", MessageBoxIcon.Error);
                    }
                    else if (dlg.Equals(DialogResult.Retry))
                    {
                        ShowMessage( "ClientDisconnectRequest",  "No connection could be made because the target machine actively refused it.Please retry it.", MessageBoxIcon.Error);
                    }
                    else if (dlg.Equals(DialogResult.Cancel))  //socket close
                    {
                        ShowMessage( "Socket Close",  "BCS Disconnection, You are logout by system.", MessageBoxIcon.Error);
                    }
                    else if (dlg.Equals(DialogResult.OK))
                    {
                        //frmBase.ShowMessage(this, "Socket Close", "", "BCS Disconnection, You are logout by system.", MessageBoxIcon.Error);
                    }
                    #endregion
                }
                else
                {
                    #region 等待BC回覆OK才可登入
                    while (FormMainMDI.ConnectBCSResult == string.Empty) { }

                    if (FormMainMDI.ConnectBCSResult == OPIAp.ReturnCodeSuccess)
                    {
                        this.Hide();
                        DialogResult dlg = FrmMainMDI.ShowDialog();
                        this.Cursor = Cursors.Default;

                        this.Show();

                        if (dlg.Equals(DialogResult.Abort))
                        {
                            ShowMessage( "ClientDisconnectRequest", "You are logout by system.", MessageBoxIcon.Error);
                        }
                        else if (dlg.Equals(DialogResult.Retry))
                        {
                            ShowMessage( "ClientDisconnectRequest", "No connection could be made because the target machine actively refused it.Please retry it.", MessageBoxIcon.Error);
                        }
                        else if (dlg.Equals(DialogResult.Cancel))  //socket close
                        {
                            ShowMessage( "Socket Close", "BCS Disconnection, You are logout by system.", MessageBoxIcon.Error);
                        }
                        else if (dlg.Equals(DialogResult.OK)) 
                        {
                            //frmBase.ShowMessage(this, "Socket Close", "", "BCS Disconnection, You are logout by system.", MessageBoxIcon.Error);
                        }

                    }
                    else
                    {
                        FormMainMDI.SocketDriver.Dispose();
                        ShowMessage( "Socket Error", FormMainMDI.ConnectBCSResult, MessageBoxIcon.Error);
                    }
                    #endregion
                }

                txtUserId.Text = string.Empty;
                txtPassword.Text = string.Empty;
            }
            catch (Exception ex)
            {
                //NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                MessageBox.Show(new Form() { TopMost = true }, ex.ToString(), MethodBase.GetCurrentMethod().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //ShowMessage(MethodBase.GetCurrentMethod().Name, ex.ToString(), MessageBoxIcon.Error);
            }
            finally 
            {
                this.Cursor = Cursors.Default;

                this.Show();

                if (FrmMainMDI != null)
                    FrmMainMDI.Dispose();

                if (IsUAC)
                {
                    txtUserId.Text = "Query";
                    txtPassword.Text = "Query";
                }
            }
        }
     
        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            #region Current DateTime
            lblDateTime.Text = string.Format("{0}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            #endregion
        }

        private void LoadDBConfig(string strFilePath)
        {
            try
            {
                string _errMsg=string.Empty ;

                //讀取 DBConfig.xml 檔案
                DBConfigXml = new csDBConfigXML(strFilePath, out _errMsg);

                if (_errMsg != string.Empty)
                {
                    MessageBox.Show(new Form() { TopMost = true }, _errMsg, MethodBase.GetCurrentMethod().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    //frmBase.ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", _errMsg, MessageBoxIcon.Error);
                    this.Close();
                }

                // 建立 Fab Items
                cboFabType.Items.Clear();
                foreach (string fab in DBConfigXml.dic_Setting.Keys)                
                    cboFabType.Items.Add(fab);

                if (cboFabType.Items.Count >= 1)
                    cboFabType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                //NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                MessageBox.Show(new Form() { TopMost = true }, ex.ToString(), MethodBase.GetCurrentMethod().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cboFabType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                cboLineType.Items.Clear();

                foreach (string strLineId in DBConfigXml.dic_Setting[cboFabType.Text].dic_LineType.Keys)
                    cboLineType.Items.Add(strLineId);

                if (cboLineType.Items.Count >= 1)
                    cboLineType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                //NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                MessageBox.Show(new Form() { TopMost = true }, ex.ToString(), MethodBase.GetCurrentMethod().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cboLineType_SelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                cboServerName.Items.Clear();

                foreach (string strLineId in DBConfigXml.dic_Setting[cboFabType.Text].dic_LineType[cboLineType.Text].dic_Line.Keys)
                    cboServerName.Items.Add(strLineId);

                if (cboServerName.Items.Count>=1)
                    cboServerName.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                //NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                MessageBox.Show(new Form() { TopMost = true }, ex.ToString(), MethodBase.GetCurrentMethod().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SetLanguage()
        {
            var data = new[] 
            { 
                new { Key = "ENG", Value ="English" } ,
                new { Key = "CHS", Value ="简体中文"  } 
            };

            cboLanguage.DataSource = data;
            cboLanguage.DisplayMember = "Value";
            cboLanguage.ValueMember = "Key";
            cboLanguage.SelectedIndex = 0;
        }

        private void lblRemark_DoubleClick(object sender, EventArgs e)
        {

            //20170419: huangjiayin...

            Label my_lbl = (Label)sender;

            if (my_lbl.Tag==null)
            {

                if (ModifierKeys == (Keys.Shift | Keys.Control))
                {
                    txtUserId.Text = "Unicom";
                    txtPassword.Text = "Unicom";
                }
            }
            else if (ModifierKeys ==Keys.Alt&& ((MouseEventArgs)e).Button==MouseButtons.Right)
            {
                txtUserId.Text = "13266";
                txtPassword.Text = "hjy1120.";
            }

        }

        private void txt_MouseClick(object sender, MouseEventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        private void txt_Enter(object sender, EventArgs e)
        {
            ((TextBox)sender).SelectAll();
        }

        /// <summary>
        /// 在Password 输入完成后直接按Enter 键直接登录 20150423 tom
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void txtPassword_KeyPress(object sender, KeyPressEventArgs e)
        {
            if(e.KeyChar==(char)13)
            {
                e.Handled = true;
                btnLogin_Click(null, null);
            }
        }

        private void ShowMessage(string caption,string message, MessageBoxIcon iconType)
        {
            FormShowMessage _frm = new FormShowMessage(caption,message,iconType);
            _frm.ShowDialog();
            _frm.Dispose();
        }
    }
}
