using System;
using System.Reflection;
using System.Windows.Forms;
using UniClientTools;
using System.Configuration;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace UniOPI
{
    static class Program
    {
        [DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Auto)]
        private extern static IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern int PostMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        public const int WM_CLOSE = 0x10;
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //預設使用English的語系資源
            UniLanguage.CurResourceManager = new System.Resources.ResourceManager("UniOPI.Properties.ENG", Assembly.GetExecutingAssembly());

            //args:若OPI是由其他程式呼叫，args參數陣列為[UserID,UserPassword]
            //args = new string[] { "-u", "CSOTCIM", "-p", "CIMENGa" }; 

            //防止OPI一次大量開啟
            //若一次開啟數量大於設定值則會值接關閉OPI並跳出提示訊息紀錄LOG
            int OPIWindowsMaxCount = int.Parse(ConfigurationManager.AppSettings["OPIMaxCount"]);  //取得最大開啟OPI視窗數量
            Process _process = Process.GetCurrentProcess();
            string ProcName = _process.ProcessName;
            if (Process.GetProcessesByName(ProcName).Length < OPIWindowsMaxCount + 1)
                Application.Run(new FormLogin(args));
            else
            {
                NLogManager.Logger.LogErrorWrite("OPI WINDOWS COUNT", MethodBase.GetCurrentMethod().Name, string.Format("[OPI Process Number]:[{0}]", Process.GetProcessesByName(ProcName).Length.ToString()));
                IntPtr ptr = FindWindow(null, "OPI Windows Check.");
                if (ptr == IntPtr.Zero)
                {
                    //StartAutoClose();
                    MessageBox.Show(null, "Can't open OPI  windows number over " + OPIWindowsMaxCount + " !", "OPI Windows Check.", MessageBoxButtons.OK);
                }
                //else
                //    PostMessage(ptr, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            }
        }

        //private static void StartAutoClose()
        //{
        //    Timer _timer = new Timer();
        //    _timer.Interval = 3000;
        //    _timer.Tick += new EventHandler(Timer_Tick);
        //    _timer.Start();
        //}

        //private static void Timer_Tick(object sender, EventArgs e)
        //{
        //    //StopMessageBox();
        //    ((Timer)sender).Stop();
        //}

        //private static void StopMessageBox()
        //{
        //    IntPtr ptrMessageBox = FindWindow(null, "OPI Windows Check.");
        //    if (ptrMessageBox != IntPtr.Zero)
        //    {
        //        PostMessage(ptrMessageBox, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
        //    }
        //}
    }
}
