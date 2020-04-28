using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace UniOPI
{
    public partial class FormRemoteDevice : FormBase
    {
        public FormRemoteDevice()
        {
            InitializeComponent();
        }

        #region ConnectRemoteDeviceClass
        [DllImport("mpr.dll", EntryPoint = "WNetAddConnection2")]
        public static extern uint WNetAddConnection2([In] ConnentData crdConnentData, string crdPassword, string crdUserName, uint crdFlags);

        [DllImport("mpr.dll")]
        public static extern uint WNetCancelConnection2(string crdUserName, uint crdFlags, bool crdForce);

        [StructLayout(LayoutKind.Sequential)]
        public class ConnentData
        {
            public int cdScope;
            public int cdType;
            public int cdDisplayType;
            public int cdUsage;
            public string cdLocalName;
            public string cdRemoteName;
            public string cdComment;
            public string cdProvider;
        }
        #endregion

        //public static FormShowMessage2 FrmShowMessage = new FormShowMessage2();

        private void FormRemoteDevice_Load(object sender, EventArgs e)
        {
            try
            {
                lblBCIp.Text += "   " + FormMainMDI.G_OPIAp.SocketIp + "    Port    " + FormMainMDI.G_OPIAp.SocketPort;
                lblPath.Text += "   D:\\UnicomLog\\" + FormMainMDI.G_OPIAp.CurLine.ServerName + "\\";
                //產生磁碟機代碼
                for (int addListBoxItem = 0; addListBoxItem <= 25; addListBoxItem++)
                    cboMasterDevice.Items.Add(Convert.ToChar(65 + addListBoxItem).ToString() + ":");
                cboMasterDevice.SelectedIndex = 20;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }  
        }

        /// <summary>
        /// 連結網路磁碟機
        /// 若開啟磁碟機發生錯誤則openDisk會回傳錯誤代碼
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpen_Click(object sender, EventArgs e)
        {
            try
            {
                string filePath = @"\\" + FormMainMDI.G_OPIAp.SocketIp + "\\UnicomLog\\" + FormMainMDI.G_OPIAp.CurLine.ServerName;
                long openDiskMessage = AddDiskConnection(filePath, txtUserId.Text, txtPassword.Text, cboMasterDevice.SelectedItem.ToString());
                if (openDiskMessage == 0)
                {
                    System.Diagnostics.Process.Start(cboMasterDevice.SelectedItem.ToString());
                    cboMasterDevice.Enabled = false;
                }
                else
                {
                    if (openDiskMessage == 85)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Repeat Disk Number", MessageBoxIcon.Error);
                    }
                    else if (openDiskMessage == 53)
                    {
                        ShowMessage(this, lblCaption.Text, "", "Can't Find Document, maybe not start sharing?", MessageBoxIcon.Error);
                    }
                    else if (openDiskMessage == 86 || openDiskMessage == 5 || openDiskMessage == 1326)
                    {
                        ShowMessage(this, lblCaption.Text, "", "The user name or password is incorrect.", MessageBoxIcon.Error);
                    }
                    txtUserId.Text = "";
                    txtPassword.Text = "";
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
            try
            {
                CancelDiskConnection(cboMasterDevice.SelectedItem.ToString());
                this.Close();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }  
        }

        /// <summary>
        /// 遠端磁碟Class
        /// 使用而外的mpr.dll進行連線
        /// </summary>
        /// <param name="crdConnentData"></param>
        /// <param name="crdPassword"></param>
        /// <param name="crdUserName"></param>
        /// <param name="crdFlags"></param>
        /// <returns></returns>
        public long AddDiskConnection(string frdpath, string frdName, string frdpwd, string frdDiskName)
        {
            ConnentData frdConnentData = new ConnentData();
            //frdConnentData.cdScope = 2;
            //frdConnentData.cdType = 1;
            //frdConnentData.cdDisplayType = 3;
            //frdConnentData.cdUsage = 1;
            frdConnentData.cdLocalName = frdDiskName;
            frdConnentData.cdRemoteName = frdpath;
            frdConnentData.cdProvider = null;
            return WNetAddConnection2(frdConnentData, frdpwd, frdName, 1);
        }

        public long CancelDiskConnection(string frdDisk)
        {
            return WNetCancelConnection2(frdDisk, 1, true);
        }

    }
}
