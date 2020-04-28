using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using System.Xml;
using UniAuto.UniBCS.OpiSpec;
using UniClientTools;
using Unicom.UniAuto.Net.Socket;
using System.IO;

namespace UniOPI
{
    public partial class FormMainMDI : Form
    {
        internal class MessagerFilter : IMessageFilter
        {
            public bool PreFilterMessage(ref System.Windows.Forms.Message m) {
                //如果检测到有鼠标或则键盘的消息
                if (m.Msg == 0x0200 || m.Msg == 0x0201 || m.Msg == 0x0204 || m.Msg == 0x0207) {
                    
                    lastOperationTime = DateTime.Now;

                    //NLogManager.Logger.LogInfoWrite("PreFilterMessage", "PreFilterMessage", string.Format("m.Msg [{0}], lastOperationTime [{1}]", m.Msg.ToString(), lastOperationTime.ToString("yyyy-MM-dd HH:mm:ss ffff")));
                }

                return false;// 消息继续循环
            }
        }

        static DateTime lastOperationTime = DateTime.Now;
        [DllImport("USER32.DLL")]
        private static extern IntPtr GetSystemMenu(IntPtr hWnd, UInt32 bRevert);
        [DllImport("USER32.DLL")]
        private static extern UInt32 RemoveMenu(IntPtr hMenu, UInt32 nPosition, UInt32 wFlags);
        private const UInt32 SC_CLOSE = 0x0000F060;
        private const UInt32 MF_BYCOMMAND = 0x00000000;

        #region 閃爍
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [System.Runtime.InteropServices.DllImport("user32")]
        public static extern bool FlashWindow(IntPtr hWnd, bool bInvert);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            /// <summary>
            /// The size of the structure in bytes.
            /// </summary>
            public uint cbSize;
            /// <summary>
            /// A Handle to the Window to be Flashed. The window can be either opened or minimized.
            /// </summary>
            public IntPtr hwnd;
            /// <summary>
            /// The Flash Status.
            /// </summary>
            public uint dwFlags;
            /// <summary>
            /// The number of times to Flash the window.
            /// </summary>
            public uint uCount;
            /// <summary>
            /// The rate at which the Window is to be flashed, in milliseconds. If Zero, the function uses the default cursor blink rate.
            /// </summary>
            public uint dwTimeout;
        }

        /// <summary>
        /// Stop flashing. The system restores the window to its original stae.
        /// 停止閃爍, 系統將窗體恢復到初始狀態
        /// </summary>
        public const uint FLASHW_STOP = 0;

        /// <summary>
        /// Flash the window caption.
        /// 閃爍窗體的標題
        /// </summary>
        public const uint FLASHW_CAPTION = 1;

        /// <summary>
        /// Flash the taskbar button.
        /// 閃爍工作列(任務欄)按鈕
        /// </summary>
        public const uint FLASHW_TRAY = 2;

        /// <summary>
        /// Flash both the window caption and taskbar button.
        /// This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
        /// 閃動窗體標題和工作列(任務欄)按鈕
        /// </summary>
        public const uint FLASHW_ALL = 3;

        /// <summary>
        /// Flash continuously, until the FLASHW_STOP flag is set.
        /// 連續不的閃爍, 直到此參數被設置為 FLASHW_STOP
        /// </summary>
        public const uint FLASHW_TIMER = 4;

        /// <summary>
        /// Flash continuously until the window comes to the foreground.
        /// 連續不停的閃爍, 直到窗體用戶被激活, 
        /// 通常用法將參數設置為 FLASHW_ALL | FLASHW_TIMERNOFG
        /// </summary>
        public const uint FLASHW_TIMERNOFG = 12;

        private static FLASHWINFO Create_FLASHWINFO(IntPtr handle, uint flags, uint count, uint timeout)
        {
            FLASHWINFO fi = new FLASHWINFO();
            fi.cbSize = Convert.ToUInt32(Marshal.SizeOf(fi));
            fi.hwnd = handle;
            fi.dwFlags = flags;
            fi.uCount = count;
            fi.dwTimeout = timeout;
            return fi;
        }
        #endregion

        private int socketRryCount = 0; //BC連線失敗時，嘗試重新連線計數器

        private bool IsLogout = false; //紀錄是否點選logout登出按鈕

        private bool IsFlash = false;  //紀錄目前是否為閃爍狀態

        public static bool IsRun = false;  //判斷是否為執行中

        public static OPIInfo G_OPIAp;
        public static List<FormBase> G_SubForm;
        public static MessageDriver SocketDriver;

        MessagerFilter filter;

        private ToolStripMenuItem LayoutBtn = null;

        public static string ConnectBCSResult = string.Empty;

        //30秒掃一次
        System.Timers.Timer timer = new System.Timers.Timer(5 * 60 * 1000);

        #region 宣告SubForm
        public static FormBase CurForm;  //目前使用中的Form

        public static FormBCSMessage FrmBcMessage;
        public static FormOPIMessage FrmOPIMessage;

        #region Layout
        public static FormLayout FrmLayout;
        public static FormRobotLayout FrmRobotLayout;
        #endregion

        #region Monitor
        public static FormRealJobCount FrmJobCount;
        public static FormAlarmStatus FrmAlarmStatus;
        public static FormNodeStatus FrmNodeStatus;
        public static FormPortStatus FrmPortStatus;        
        public static FormMonitorEDC FrmMonitorEDC;
        public static FormMonitorRecipe FrmRecipeParam;
        public static FormRecipeRegister FrmRecipeRegister;
        public static FormMonitorDailyCheck FrmMonitorUtility;
        public static FormSlotPosition FrmSlotInformation;
        public static FormMaterialStatus FrmMaterialStatus;
        public static FormEachPosition FrmEachPosition;
        //public static FormRealTimeGlassCount FrmRealTimeGlassCount;
        public static FormIonizerFanMode FrmIonizerFanMode;
        public static FormLinkStatus FrmLinkStatus;
        public static FormMonitorPLC FrmPLCTrxData;
        public static FormMonitorEnergyVisualization FrmMonitorEnergyVisualization;
        public static FormProductTypeBlock FrmProductTypeBlock;
        public static FormDefectCodeReport FrmDefectCodeReport;
        public static FormMonitorAPC FrmMonitorAPC;
        #endregion

        #region Operation
        public static FormLineControl FrmLineControl;
        public static FormNodeControl FrmNodeControl;
        //public static FormRunModeControl_ATS FrmRunModeControl_ATS;
        public static FormPortControl FrmPortControl;
        public static FormCassetteControl_Local FrmCassetteOperation_Local;
        public static FormCassetteControl_Offline FrmCassetteOperation_Offline;
        public static FormCimMessage FrmCimMessage;
        public static FormIncompleteCSTResend FrmTrackOut;
        public static FormRobotControl FrmRobotControl;
        public static FormRobotRoute FrmRobotRoute;
        public static FormSECSControl FrmSECSFunction;
        public static FormChangerPlan FrmChangerPlan;
        public static FormBCControl FrmBCControl;
        public static FormIndexerControl FrmIndexerControl;
        //public static FormGlassGradeMapping FrmGlassGradeMapping;
        public static FormDenseControl_Local_PPK FrmDenseControl_Local_PPK;
        public static FormDenseControl_Offline_PPK FrmDenseControl_Offline_PPK;
        public static FormDenseControl_Offline FrmDenseControl_Offline;
        public static FormDenseControl_Local FrmDenseControl_Local;
        public static FormPalletControl_Local FrmPalletControl_Local;
        public static FormPalletControl_Offline FrmPalletControl_Offline;
        public static FormWIPDelete FrmWIPDelete;
        public static FormTrackTime FrmTrackTime;
        public static FormUnloadingPortSetting FrmUnloadingPortSetting;
        public static FormBufferRWJudgeCapacity FrmBufferRWJudgeCapacity;
        public static FormEquipmentFetchGlassProportional FrmEquipmentFetchGlassProportional;
        public static FormRobotOperationMode FrmRobotOperationMode;
        #endregion

        #region Setting
        public static FormRecipeManagement FrmRecipeManagement;
        public static FormSubBlock FrmSubBlock;
        public static FormLineStatusRule FrmLineStatusRule;
        //public static FormUserGroup FrmUserGroup;
        public static FormUserQuery FrmUserQuery;
        public static FormSamplingRule FrmSamplingRule;
        public static FormQTimeSetting FrmQTimeSetting;
        public static FormSkipReportSetting FrmSkipReportSetting;
        public static FormParameterSetting FrmParameterSetting;
        public static FormParameterSetting_ELA FrmParameterSetting_ELA;
        public static FormUserChangePassword FrmUserChangePassword;
        public static FormUserGroupQuery FrmUserGroupQuery;
        #endregion

        #region History Form
        public static FormHistory FrmAlarmHistory;
        public static FormHistory FrmCassetteHistory;
        public static FormHistory FrmLineHistory;
        public static FormHistory FrmNodeHistory;
        public static FormHistory FrmPortHistory;
        public static FormHistory FrmUnitHistory;
        public static FormHistory FrmJobHistory;
        public static FormProcessDataHistory FrmProcessDataHistory;
        public static FormHistory FrmOPIHistory;
        public static FormHistory FrmCIMMessageHistory;
        public static FormHistory FrmChangerPlanHistory;
        public static FormHistory FrmAssemblyHistory;
        public static FormHistory FrmMaterialHistory;
        public static FormHistory FrmRecipeTableHistory;
        public static FormHistory FrmDefectcodeHistory;
        public static FormHistory FrmPPKEventHistory;
        #endregion

        #region Param Form
        public static FormAlarmManagement FrmAlarmManagement;
        public static FormAPCDownloadManagement FrmAPCDownloadManagement;
        public static FormAPCReportManagement FrmAPCReportManagement;
        public static FormDailyCheckManagement FrmDailyCheckManagement;
        public static FormEnergyVisualizationManagement FrmEnergyVisualizationManagement;
        public static FormProcessDataManagement FrmProcessDataManagement;
        public static FormRecipeParameterManagement FrmRecipeParameterManagement;
        public static FormSECSVariableManagement FrmSECSVariableManagement;
        public static FormRobotManagement FrmRobotManagement;
        public static FormRobotMethodDef FrmRobotMethodDef;
        public static FormRobotRouteCondition FrmRobotRouteCondition;
        public static FormRobotRouteMST FrmRobotRouteMST;
        public static FormRobotRuleSelect FrmRobotRuleJobSelect;
        public static FormRobotStageManagement FrmRobotStageManamgement;
        //public static FormRobotRunModeManagement FrmRobotRunModeManagement;
        #endregion

        #endregion

        public FormMainMDI(OPIInfo OPIAp, string Language)
        {
            InitializeComponent();

            #region Event
            this.Load += new EventHandler(FormMainMDI_Load);
            this.Shown += new EventHandler(FormMainMDI_Shown);
            this.FormClosing += new FormClosingEventHandler(FormMainMDI_FormClosing);
            this.VisibleChanged += new EventHandler(FormMainMDI_VisibleChanged);

            this.tmrInitial.Tick += new EventHandler(tmrInitial_Tick);
            this.tmrRefresh.Tick += new EventHandler(tmrRefresh_Tick);
            this.bgwSocket.DoWork += new DoWorkEventHandler(bgwSocket_DoWork);

            this.LanguageToolStripMenuItem_En.Click += new EventHandler(LanguageToolStrip_Click);
            this.LanguageToolStripMenuItem_Ch.Click += new EventHandler(LanguageToolStrip_Click);
            this.logoutToolStripMenuItem.Click += new EventHandler(UserButton_Click);
            this.exitToolStripMenuItem1.Click += new EventHandler(UserButton_Click);
            this.picCSOT.Click += new EventHandler(picCSOT_Click);
            this.lblBCS.Click += new EventHandler(lblBCS_Click);
            this.lblOPI.Click += new EventHandler(lblOPI_Click);
            this.lblMPLC.Click += new EventHandler(lblMPLC_Click);
            this.tslbBCIP.DoubleClick += new EventHandler(tslbBCIP_DoubleClick);
            this.lblLineInfo.DoubleClick += new EventHandler(lblLineInfo_DoubleClick);
            
            #endregion

            #region 取消右上角關閉功能
            IntPtr hMenu = GetSystemMenu(this.Handle, 0);
            RemoveMenu(hMenu, SC_CLOSE, MF_BYCOMMAND);
            #endregion

            G_OPIAp = OPIAp;
            ConnectBCSResult = string.Empty;
            socketRryCount = 0;
            G_SubForm = new List<FormBase>();
            IsLogout = false;
            IsRun = true;

            #region 建立SubForm
            //FrmShowMessage = new FormShowMessage2();

            FrmCassetteOperation_Local = new FormCassetteControl_Local();
            FrmCassetteOperation_Offline = new FormCassetteControl_Offline();
            FrmDenseControl_Local_PPK = new FormDenseControl_Local_PPK();
            FrmDenseControl_Offline_PPK = new FormDenseControl_Offline_PPK();
            FrmDenseControl_Local = new FormDenseControl_Local();
            FrmDenseControl_Offline = new FormDenseControl_Offline();
            FrmPalletControl_Local = new FormPalletControl_Local();
            FrmPalletControl_Offline = new FormPalletControl_Offline();


            #region Layout
            FrmLayout = new FormLayout(G_OPIAp) { Tag = "M01S01F00" };
            G_SubForm.Add(FrmLayout);

            FrmRobotLayout = new FormRobotLayout(G_OPIAp) { Tag = "M01S02F00" };
            G_SubForm.Add(FrmRobotLayout);
            #endregion

            #region Monitor Form
            FrmAlarmStatus = new FormAlarmStatus() { Tag = "M02S01F00" };
            G_SubForm.Add(FrmAlarmStatus);

            FrmNodeStatus = new FormNodeStatus() { Tag = "M02S02F00" };
            G_SubForm.Add(FrmNodeStatus);

            FrmJobCount = new FormRealJobCount() { Tag = "M02S03F00" };
            G_SubForm.Add(FrmJobCount);

            FrmPortStatus = new FormPortStatus() { Tag = "M02S04F00" };
            G_SubForm.Add(FrmPortStatus);

            FrmMonitorUtility = new FormMonitorDailyCheck() { Tag = "M02S05F00" };
            G_SubForm.Add(FrmMonitorUtility);

            FrmRecipeParam = new FormMonitorRecipe() { Tag = "M02S06F00" };
            G_SubForm.Add(FrmRecipeParam);

            FrmMonitorEDC = new FormMonitorEDC() { Tag = "M02S07F00" };
            G_SubForm.Add(FrmMonitorEDC);

            FrmSlotInformation = new FormSlotPosition() { Tag = "M02S08F00" };
            G_SubForm.Add(FrmSlotInformation);

            FrmMaterialStatus = new FormMaterialStatus() { Tag = "M02S09F00" };
            G_SubForm.Add(FrmMaterialStatus);

            FrmEachPosition = new FormEachPosition() { Tag = "M02S10F00" };
            G_SubForm.Add(FrmEachPosition);

            FrmRecipeRegister = new FormRecipeRegister() { Tag = "M02S12F00" };
            G_SubForm.Add(FrmRecipeRegister);

            FrmIonizerFanMode = new FormIonizerFanMode() { Tag = "M02S13F00" };
            G_SubForm.Add(FrmIonizerFanMode);

            FrmLinkStatus = new FormLinkStatus() { Tag = "M02S14F00" };
            G_SubForm.Add(FrmLinkStatus);

            FrmPLCTrxData = new FormMonitorPLC() { Tag = "M02S15F00" };
            G_SubForm.Add(FrmPLCTrxData);

            FrmMonitorEnergyVisualization = new FormMonitorEnergyVisualization() { Tag = "M02S16F00" };
            G_SubForm.Add(FrmMonitorEnergyVisualization);

            FrmProductTypeBlock = new FormProductTypeBlock() { Tag = "M02S17F00" };
            G_SubForm.Add(FrmProductTypeBlock);

            FrmDefectCodeReport = new FormDefectCodeReport() { Tag = "M02S18F00" };
            G_SubForm.Add(FrmDefectCodeReport);

            FrmMonitorAPC = new FormMonitorAPC() { Tag = "M02S19F00" };
            G_SubForm.Add(FrmMonitorAPC);

            #endregion

            #region Operation Form
            FrmLineControl = new FormLineControl() { Tag = "M03S01F00" };
            G_SubForm.Add(FrmLineControl);

            FrmNodeControl = new FormNodeControl() { Tag = "M03S02F00" };
            G_SubForm.Add(FrmNodeControl);

            FrmPortControl = new FormPortControl() { Tag = "M03S03F00" };
            G_SubForm.Add(FrmPortControl);

            FrmTrackOut = new FormIncompleteCSTResend() { Tag = "M03S05F00" };
            G_SubForm.Add(FrmTrackOut);

            FrmCimMessage = new FormCimMessage() { Tag = "M03S06F00" };
            G_SubForm.Add(FrmCimMessage);

            FrmRobotControl = new FormRobotControl() { Tag = "M03S07F00" };
            G_SubForm.Add(FrmRobotControl);

            FrmSECSFunction = new FormSECSControl() { Tag = "M03S08F00" };
            G_SubForm.Add(FrmSECSFunction);

            FrmChangerPlan = new FormChangerPlan() { Tag = "M03S09F00" };
            G_SubForm.Add(FrmChangerPlan);

            FrmBCControl = new FormBCControl() { Tag = "M03S10F00" };
            G_SubForm.Add(FrmBCControl);

            FrmIndexerControl = new FormIndexerControl() { Tag = "M03S11F00" };
            G_SubForm.Add(FrmIndexerControl);

            FrmWIPDelete = new FormWIPDelete() { Tag = "M03S13F00" };
            G_SubForm.Add(FrmWIPDelete);

            FrmTrackTime = new FormTrackTime() { Tag = "M03S14F00" };
            G_SubForm.Add(FrmTrackTime);

            FrmUnloadingPortSetting = new FormUnloadingPortSetting() { Tag = "M03S15F00" };
            G_SubForm.Add(FrmUnloadingPortSetting);

            FrmBufferRWJudgeCapacity = new FormBufferRWJudgeCapacity() { Tag = "M03S16F00" };
            G_SubForm.Add(FrmBufferRWJudgeCapacity);

            FrmEquipmentFetchGlassProportional = new FormEquipmentFetchGlassProportional() { Tag = "M03S17F00" };
            G_SubForm.Add(FrmEquipmentFetchGlassProportional);

            FrmRobotOperationMode = new FormRobotOperationMode() { Tag = "M03S18F00" };
            G_SubForm.Add(FrmRobotOperationMode);

            #endregion

            #region Setting Form
            FrmRecipeManagement = new FormRecipeManagement() { Tag = "M04S01F00" };
            G_SubForm.Add(FrmRecipeManagement);

            FrmSubBlock = new FormSubBlock() { Tag = "M04S02F00" };
            G_SubForm.Add(FrmSubBlock);

            FrmLineStatusRule = new FormLineStatusRule() { Tag = "M04S03F00" };
            G_SubForm.Add(FrmLineStatusRule);

            FrmSamplingRule = new FormSamplingRule() { Tag = "M04S04F00" };
            G_SubForm.Add(FrmSamplingRule);

            FrmQTimeSetting = new FormQTimeSetting() { Tag = "M04S05F00" };
            G_SubForm.Add(FrmQTimeSetting);

            FrmSkipReportSetting = new FormSkipReportSetting() { Tag = "M04S06F00" };
            G_SubForm.Add(FrmSkipReportSetting);

            FrmRobotRoute = new FormRobotRoute() { Tag = "M04S07F00" };
            G_SubForm.Add(FrmRobotRoute);

            FrmParameterSetting = new FormParameterSetting() { Tag = "M04S08F00" };
            G_SubForm.Add(FrmParameterSetting);

            FrmParameterSetting_ELA = new FormParameterSetting_ELA() { Tag = "M04S09F00" };
            G_SubForm.Add(FrmParameterSetting_ELA);

            FrmUserQuery = new FormUserQuery() { Tag = "M04S10F00" };
            G_SubForm.Add(FrmUserQuery);

            FrmUserChangePassword = new FormUserChangePassword() { Tag = "M04S11F00" };
            G_SubForm.Add(FrmUserChangePassword);

            FrmUserGroupQuery = new FormUserGroupQuery() { Tag = "M04S12F00"};
            G_SubForm.Add(FrmUserGroupQuery);

            #endregion

            #region History Form

            FrmLineHistory = new FormHistory(new HisTableParam("SBCS_LINEHISTORY_TRX")) { Tag = "M06S01F00" };
            G_SubForm.Add(FrmLineHistory);

            FrmNodeHistory = new FormHistory(new HisTableParam("SBCS_NODEHISTORY_TRX")) { Tag = "M06S02F00" };
            G_SubForm.Add(FrmNodeHistory);

            FrmPortHistory = new FormHistory(new HisTableParam("SBCS_PORTHISTORY_TRX")) { Tag = "M06S03F00" };
            G_SubForm.Add(FrmPortHistory);

            FrmUnitHistory = new FormHistory(new HisTableParam("SBCS_UNITHISTORY_TRX")) { Tag = "M06S04F00" };
            G_SubForm.Add(FrmUnitHistory);

            FrmJobHistory = new FormHistory(new HisTableParam("SBCS_JOBHISTORY_TRX")) { Tag = "M06S05F00" };
            G_SubForm.Add(FrmJobHistory);

            FrmAlarmHistory = new FormHistory(new HisTableParam("SBCS_ALARMHISTORY_TRX")) { Tag = "M06S06F00" };
            G_SubForm.Add(FrmAlarmHistory);

            FrmProcessDataHistory = new FormProcessDataHistory(new HisTableParam("SBCS_PROCESS_DATA_TRX")) { Tag = "M06S07F00" };
            G_SubForm.Add(FrmProcessDataHistory);

            FrmCassetteHistory = new FormHistory(new HisTableParam("SBCS_CASSETTEHISTORY_TRX")) { Tag = "M06S08F00" };
            G_SubForm.Add(FrmCassetteHistory);

            FrmOPIHistory = new FormHistory(new HisTableParam("SBCS_OPIHISTORY_TRX")) { Tag = "M06S09F00" };
            G_SubForm.Add(FrmOPIHistory);

            FrmChangerPlanHistory = new FormHistory(new HisTableParam("SBCS_CHANGEPLANHISTORY_TRX")) { Tag = "M06S10F00" };
            G_SubForm.Add(FrmChangerPlanHistory);

            FrmMaterialHistory = new FormHistory(new HisTableParam("SBCS_MATERIAL_TRX")) { Tag = "M06S12F00" };
            G_SubForm.Add(FrmMaterialHistory);

            FrmCIMMessageHistory = new FormHistory(new HisTableParam("SBCS_CIMMESSAGE_TRX")) { Tag = "M06S13F00" };
            G_SubForm.Add(FrmCIMMessageHistory);

            FrmAssemblyHistory = new FormHistory(new HisTableParam("SBCS_ASSEMBLY_TRX")) { Tag = "M06S14F00" };
            G_SubForm.Add(FrmAssemblyHistory);

            FrmRecipeTableHistory = new FormHistory(new HisTableParam("SBCS_RECIPETABLE_TRX")) { Tag = "M06S15F00" };
            G_SubForm.Add(FrmRecipeTableHistory);

            FrmDefectcodeHistory = new FormHistory(new HisTableParam("SBCS_DEFECTCODEHISTORY_TRX")) { Tag = "M06S16F00" };
            G_SubForm.Add(FrmDefectcodeHistory);

            FrmPPKEventHistory = new FormHistory(new HisTableParam("SBCS_PPKEVENTHISTORY_TRX")) { Tag = "M06S17F00" };
            G_SubForm.Add(FrmPPKEventHistory);

            #endregion

            #region Param Form
            FrmAlarmManagement = new FormAlarmManagement() { Tag = "M05S01F00" };
            G_SubForm.Add(FrmAlarmManagement);

            FrmAPCDownloadManagement = new FormAPCDownloadManagement() { Tag = "M05S02F00" };
            G_SubForm.Add(FrmAPCDownloadManagement);

            FrmAPCReportManagement = new FormAPCReportManagement() { Tag = "M05S03F00" };
            G_SubForm.Add(FrmAPCReportManagement);

            FrmDailyCheckManagement = new FormDailyCheckManagement() { Tag = "M05S04F00" };
            G_SubForm.Add(FrmDailyCheckManagement);

            FrmEnergyVisualizationManagement = new FormEnergyVisualizationManagement() { Tag = "M05S05F00" };
            G_SubForm.Add(FrmEnergyVisualizationManagement);

            FrmProcessDataManagement = new FormProcessDataManagement() { Tag = "M05S06F00" };
            G_SubForm.Add(FrmProcessDataManagement);

            FrmRecipeParameterManagement = new FormRecipeParameterManagement() { Tag = "M05S07F00" };
            G_SubForm.Add(FrmRecipeParameterManagement);

            FrmSECSVariableManagement = new FormSECSVariableManagement() { Tag = "M05S08F00" };
            G_SubForm.Add(FrmSECSVariableManagement);

            FrmRobotManagement = new FormRobotManagement() { Tag = "M05S09F00" };
            G_SubForm.Add(FrmRobotManagement);

            FrmRobotStageManamgement = new FormRobotStageManagement() { Tag = "M05S10F00" };
            G_SubForm.Add(FrmRobotStageManamgement);

            FrmRobotRouteMST = new FormRobotRouteMST() { Tag = "M05S11F00" };
            G_SubForm.Add(FrmRobotRouteMST);

            FrmRobotMethodDef = new FormRobotMethodDef() { Tag = "M05S12F00" };
            G_SubForm.Add(FrmRobotMethodDef);

            FrmRobotRouteCondition = new FormRobotRouteCondition() { Tag = "M05S13F00" };
            G_SubForm.Add(FrmRobotRouteCondition);

            FrmRobotRuleJobSelect = new FormRobotRuleSelect() { Tag = "M05S14F00" };
            G_SubForm.Add(FrmRobotRuleJobSelect);

            #endregion

            FrmBcMessage = new FormBCSMessage(G_OPIAp);
            FrmOPIMessage = new FormOPIMessage();
            //Dic_PipeName = new Dictionary<string, bool>();

            #endregion

            #region create socket driver & start
            SocketDriver = new MessageDriver();
            SocketDriver.DataEncoding = System.Text.Encoding.UTF8;
            SocketDriver.SocketErrorMessage += new MessageDriver.SocketErrorMessage_delegate(SocketDriver_SocketErrorMessage);
            //SocketDriver.ReceiveErrorMessage += new MessageDriver.ReceiveErrorMessage_delegate(SocketDriver_ReceiveErrorMessage);
            SocketDriver.SocketClose += new MessageDriver.SocketCloseMessage_delegate(SocketDriver_SocketClose);

            bool done = SocketDriver.Start(true, G_OPIAp.SocketIp, G_OPIAp.SocketPort);
            if (!done)
            {
                return;
            }
            else
            {
                bgwSocket.RunWorkerAsync();
            }
            #endregion

            string uiREX = string.Format("UniOPI.Properties.{0}", Language);

            UniLanguage.CurResourceManager = new System.Resources.ResourceManager(uiREX, Assembly.GetExecutingAssembly());
            filter = new MessagerFilter();
            Application.AddMessageFilter(filter);
         
            timer.AutoReset = true;
            timer.Elapsed += new ElapsedEventHandler(this.OnTimedEvent);           
        }

        #region Form

        private void FormMainMDI_Load(object sender, EventArgs e)
        {
            CurForm = new FormBase();
            try
            {
                #region 預載SubForm
                
                #region 自動load 程式
                List<CustButton> lstSubForm = G_OPIAp.Lst_Button.FindAll(d => d.ButtonType.Equals(buttonType.Sub) && d.ButtonVisible == true);//取Button中取得subFunction
                //G_OPIAp.Lst_Button.FindAll(d => d.ButtonType.Equals(buttonType.Sub) && d.ButtonVisible == true);//取Button中取得subFunction

                lstSubForm.Sort((d1, d2) => d1.ButtonSequence.CompareTo(d2.ButtonSequence));//依順序排序

                foreach (CustButton b in lstSubForm)
                {
                    string fetchButtonKey = string.Empty;
                    //if (b.ButtonSequence == 0)
                    //    fetchButtonKey = b.ButtonParentButtonKey;
                    //else
                        fetchButtonKey = b.ButtonKey;
                    Form subForm = G_SubForm.Find(d => d.Tag.ToString().Equals(fetchButtonKey));
                    if (subForm != null)
                    {
                        PreLoadSubForm(ref subForm, b.ButtonCaption);
                    }
                }

                foreach (ToolStripMenuItem tsmi in msMainBtn.Items)
                {
                    string tsmiTag = tsmi.Tag.ToString();

                    #region 隱藏/disable MainButton
                    CustButton cbMin = G_OPIAp.Lst_Button.Find(d => d.ButtonKey.Equals(tsmiTag));
                    if (cbMin != null)
                    {
                        if (!cbMin.ButtonVisible) { tsmi.Visible = false; continue; }
                        if (!cbMin.ButtonEnable) { tsmi.Enabled = false; continue; }
                    }
                    else
                    {
                        tsmi.Visible = false; 
                        tsmi.Enabled = false;
                        continue;
                    }
                    #endregion

                    if (tsmiTag.Equals("M07S00F00"))
                    {
                        tsmi.Click += new EventHandler(MenuButton_BrowserClick);
                    }

                    if (string.IsNullOrWhiteSpace(tsmiTag)) continue;
                    List<CustButton> tagSubButton = G_OPIAp.Lst_Button.FindAll(d => d.ButtonParentButtonKey.Equals(tsmiTag) && d.ButtonVisible == true);
                    if (tagSubButton.Count == 0) continue;
                    tagSubButton.Sort((d1, d2) => d1.ButtonSequence.CompareTo(d2.ButtonSequence));
                    if (tagSubButton.Count == 1)
                    {
                        CustButton tagMinButton = G_OPIAp.Lst_Button.Find(d => d.ButtonKey.Equals(tsmiTag));
                        if (tagMinButton != null)
                        {
                            //tsmi.Name = tagMinButton.ButtonCaption;
                            //tsmi.Tag = tagMinButton.ButtonKey;
                            //tsmi.Enabled = tagMinButton.ButtonEnable;
                            //tsmi.Click += new EventHandler(MenuButton_Click);
                            tsmi.Name = tagSubButton[0].ButtonCaption;
                            tsmi.Text = tagSubButton[0].ButtonCaption;
                            tsmi.Tag = tagSubButton[0].ButtonKey;
                            tsmi.Enabled = tagSubButton[0].ButtonEnable;
                            tsmi.Click += new EventHandler(MenuButton_Click);
                        }
                    }
                    else
                    {
                        foreach (CustButton tb in tagSubButton)
                        {

                            ToolStripMenuItem item = new ToolStripMenuItem();
                            item.Name = tb.ButtonCaption;
                            item.Text = tb.ButtonCaption;
                            item.Tag = tb.ButtonKey;
                            item.Enabled = tb.ButtonEnable;
                            if (tb.ButtonImage != null)
                            {
                                Object objImg = Properties.Resources.ResourceManager.GetObject(tb.ButtonImage);
                                if (objImg != null) item.Image = (Bitmap)objImg;
                            }
                            item.Click += new EventHandler(MenuButton_Click);
                            tsmi.DropDownItems.Add(item);

                            if (tb.ButtonKey == "M01S01F00")   LayoutBtn = item;
                        }
                    }
                }
                #endregion

                #region Load Robot Name
                if (G_OPIAp.Dic_Robot.Count > 0)
                {
                    #region 有robot功能新增顯示robot run mode區塊
                    spcBase.SplitterDistance = 100;
                    tlpTitleBackGround.RowStyles[1].Height = 30;
                    #endregion

                    #region 新增 Robot ---只有一組RB
                    foreach (Robot _rb in G_OPIAp.Dic_Robot.Values)
                    {
                        lblRobotName.Text = _rb.RobotName;
                    }
                    #endregion
                }
                else
                {
                    spcBase.SplitterDistance = 70;
                    tlpTitleBackGround.RowStyles[1].Height = 0;
                }

                #endregion

                #endregion

                G_OPIAp.APName = "OPI";
                lblLineInfo.Text = G_OPIAp.CurLine.ServerName;

                if (LayoutBtn != null)  MenuButton_Click(LayoutBtn, null);
                else MenuButton_Click(tsmiLayoutBtn, null);

                NLogManager.Logger.LogInfoWrite(GetType().Name, MethodBase.GetCurrentMethod().Name, "OPI Start.");

                tmrRefresh.Enabled = true;

                tslbLoginName.Text = "User Name [" + G_OPIAp.LoginUserName + "]";
                tslbLoginTime.Text = "Login Time [" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "]";
                tslbGroupName.Text = "Group Name [" + G_OPIAp.LoginGroupID + "]";
                tslbBCIP.Text = string.Format("BC [ {0}:{1} ]", G_OPIAp.SocketIp, G_OPIAp.SocketPort);//家成偷改:顯示遠端IPPort
                tslbOPIIP.Text = string.Format("OPI [ {0} ]", (G_OPIAp.SessionID == null ? string.Empty : G_OPIAp.SessionID));//家成偷改:顯示本地端IPPort

                #region 判斷是否有兩條lineid
                if (G_OPIAp.CurLine.LineID2 == string.Empty) pnlMESControlMode.Height = 37;
                else pnlMESControlMode.Height = 55;
                #endregion

                timer.Start();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void FormMainMDI_Shown(object sender, EventArgs e)
        {
            try
            {
                this.Text += string.Format("[ {0} ] - V{1}", G_OPIAp.CurLine.ServerName, G_OPIAp.Version);

                //載入語系
                UniLanguage.ApplyUICulture(this, UniLanguage.CurLanguage);

                tmrInitial.Enabled = true;                
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void FormMainMDI_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                SocketDriver.Stop();

                SocketDriver.Dispose();

                IsRun = false;
                bgwSocket.CancelAsync();
                bgwSocket.Dispose();
                bgwSocket = null;

                FrmLayout.bgwRefresh.CancelAsync();
                FrmLayout.bgwRefresh.Dispose();
                FrmLayout.bgwRefresh = null;

                Application.RemoveMessageFilter(filter);
                foreach(FormBase frm in G_SubForm)
                {
                    frm.Dispose();
                }

                G_OPIAp.DisposeAp();

                if (FrmBcMessage != null) FrmBcMessage.Dispose();
                if (FrmOPIMessage != null) FrmOPIMessage.Dispose();


                FormMainMDI frmMid = (FormMainMDI)FormLogin.FrmMainMDI;

                if (frmMid.pnlMain.InvokeRequired)
                {
                    this.BeginInvoke(new MethodInvoker(
                        delegate
                        {
                            for (int i = Application.OpenForms.Count - 1; i >= 0; i--)//關閉除了MDI的Form
                            {
                                if (Application.OpenForms[i].Name == frmMid.Name) continue;
                                if (Application.OpenForms[i].Name == "FormLogin") continue;
                                if (Application.OpenForms[i].Name == "FormBCSMessage" || Application.OpenForms[i].Name == "FormOPIMessage")
                                {
                                    Application.OpenForms[i].Dispose();
                                    continue;
                                }
                                Application.OpenForms[i].Close();
                            }
                        }));
                }
                else
                {
                    for (int i = Application.OpenForms.Count - 1; i >= 0; i--)//關閉除了MDI的Form
                    {
                        if (Application.OpenForms[i].Name == frmMid.Name) continue;
                        if (Application.OpenForms[i].Name == "FormLogin") continue;
                        if (Application.OpenForms[i].Name == "FormBCSMessage" || Application.OpenForms[i].Name == "FormOPIMessage")
                        {
                            Application.OpenForms[i].Dispose();
                            continue;
                        }
                        Application.OpenForms[i].Close();
                    }
                }           
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void FormMainMDI_VisibleChanged(object sender, EventArgs e)
        {
            if ((sender as Form).Visible) UniLanguage.ApplyUICulture(sender as Form, UniLanguage.CurLanguage);
        }

        #endregion

        #region Click Event
        
        private void MenuButton_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem _item = (ToolStripMenuItem)sender;

                if (_item.Tag == null) return;

                #region 自動load 程式
                FormBase frmSubForm = G_SubForm.Find(d => d.Tag.ToString().Equals(_item.Tag.ToString()));

                if (frmSubForm == null) return;

                Control[] ctrl = pnlMain.Controls.Find(frmSubForm.Name, false);
                if (ctrl.Length > 0)
                {
                    FormBase frm = null;
                    if (ctrl.Length == 1)
                        frm = (FormBase)ctrl[0];
                    else
                    {
                        foreach (Control c in ctrl)
                        {
                            string frmTag = ((FormBase)c).Tag.ToString();
                            if (frmTag.Equals(_item.Tag.ToString()))
                            {
                                frm = (FormBase)c;
                                break;
                            }
                        }
                    }

                    if (frm == null) return;

                    if (!frm.Visible)
                    {
                        #region 設定Function Button Authority
                        List<CustButton> hideButtons = G_OPIAp.Lst_Button.FindAll(d => d.ButtonParentButtonKey.Equals(frm.Tag.ToString()));
                        if (hideButtons != null && hideButtons.Count > 0)
                        {
                            foreach (CustButton hd in hideButtons)
                            {
                                if (string.IsNullOrWhiteSpace(hd.ButtonName)) continue;

                                Control[] fetchHideCtrls = frm.Controls.Find(hd.ButtonName, true);

                                if (fetchHideCtrls.Length > 0)
                                {
                                    fetchHideCtrls[0].Visible = hd.ButtonVisible;
                                    fetchHideCtrls[0].Enabled = hd.ButtonEnable;
                                }
                            }
                        }
                        #endregion

                        frm.Visible = true;
                    }

                    frm.lblCaption.Text = _item.Text;
                    frm.BringToFront();
                    frm.tmrBaseRefresh.Enabled = true;
                    if (CurForm != null && CurForm != frm)
                    {
                        CurForm.tmrBaseRefresh.Enabled = false;

                        #region by From 判斷是否需要儲存
                        CurForm.CheckChangeSave();
                        #endregion
                    }

                    CurForm = frm;

                    #region by From 處理
                    switch (CurForm.Name)
                    {
                        case "FormLayout":
                            ((FormLayout)CurForm).tmrInterface.Enabled = true;
                            break;

                        case "FormRobotLayout":
                            ((FormRobotLayout)CurForm).tmrRefresh.Enabled = true;
                            break;

                        case "FormMonitorRecipe":
                            ((FormMonitorRecipe)CurForm).tmrRefresh.Enabled = true;
                            //((FormMonitorRecipe)CurForm).SetMesModeChoose(G_OPIAp.CurLine.MesControlMode, string.Empty);
                            break;

                        case "FormRecipeRegister":
                            ((FormRecipeRegister)CurForm).tmrRefresh.Enabled = true;
                            //((FormRecipeRegister)CurForm).SetMesModeChoose(G_OPIAp.CurLine.MesControlMode, string.Empty);
                            break;

                        case "FormAlarmStatus":
                            ((FormAlarmStatus)CurForm).tmrRefresh.Enabled = true;
                            break;

                        case "FormIonizerFanMode":
                            ((FormIonizerFanMode)CurForm).tmrRefresh.Enabled = true;
                            break;

                        case "FormIndexerControl":
                            ((FormIndexerControl)CurForm).tmrRefresh.Enabled = true;
                            break;

                        case "FormRecipeManagement":

                            #region FormRecipeManagement

                            //重新取得recipe check 設定~ 
                            UniTools.Reload_RecipeSetting();

                            ((FormRecipeManagement)CurForm).SetMesModeChoose();

                            ((FormRecipeManagement)CurForm).RefreshData();

                            break;

                            #endregion

                        case "FormRobotRoute":

                            ((FormRobotRoute)CurForm).CreateObject_Stage();
                            ((FormRobotRoute)CurForm).GetGridViewData_RobotRoute();

                            break;

                        case "FormLinkStatus":
                            ((FormLinkStatus)CurForm).Send_EquipmentDataLinkStatusRequst();

                            ((FormLinkStatus)CurForm).GetDataLinkStatus();

                            ((FormLinkStatus)CurForm).tmrRefresh.Enabled = true;
                            break;

                        case "FormSlotPosition":
                            ((FormSlotPosition)CurForm).tmrRefresh.Enabled = true;
                            break;

                        case "FormEachPosition":
                            ((FormEachPosition)CurForm).tmrRefresh.Enabled = true;
                            break;

                        case "FormPortControl":
                            ((FormPortControl)CurForm).tmrRefresh.Enabled = true;
                            break;

                        case "FormLineControl":
                            ((FormLineControl)CurForm).tmrRefresh.Enabled = true;
                            ((FormLineControl)CurForm).RefreshData();
                            
                            break;

                        case "FormBCControl":
                            ((FormBCControl)CurForm).tmrRefresh.Enabled = true;
                            break;

                        case "FormNodeControl":
                            ((FormNodeControl)CurForm).tmrRefresh.Enabled = true;
                            break;

                        case "FormHistory":
                            ((FormHistory)CurForm).ReloadComboBoxItem_DateTime();
                            break;

                        case "FormTrackTime":
                            ((FormTrackTime)CurForm).SendtoBC_ODFTrackTimeSettingRequest();
                            break;

                        case "FormProductTypeBlock":
                            ((FormProductTypeBlock)CurForm).Send_ProductTypeInfoRequest();
                            ((FormProductTypeBlock)CurForm).tmrBaseRefresh.Enabled = true;
                            break;

                        case "FormUnloadingPortSetting":
                            ((FormUnloadingPortSetting)CurForm).SetPort();
                            //((FormUnloadingPortSetting)CurForm).tmrBaseRefresh.Enabled = true;
                            break;

                        case "FormRobotRouteCondition":

                            G_OPIAp.RefreshDBBRMCtx();
                            ((FormRobotRouteCondition)CurForm).GetGridViewData();

                            break;

                        case "FormLineStatusRule":

                            #region RefreshData

                            ((FormLineStatusRule)CurForm).RefreshData();

                            break;

                            #endregion

                        case "FormQTimeSetting":

                            #region RefreshData

                            ((FormQTimeSetting)CurForm).RefreshData();

                            break;

                            #endregion

                        case "FormSkipReportSetting":

                            #region RefreshData

                            ((FormSkipReportSetting)CurForm).RefreshData();

                            break;

                            #endregion

                        case "FormSECSVariableManagement":

                            #region FormSECSVariableManagement

                            ((FormSECSVariableManagement)CurForm).RefreshData();

                            break;

                            #endregion

                        case "FormSubBlock":

                            #region FormSubBlock

                            ((FormSubBlock)CurForm).RefreshData();

                            break;

                            #endregion

                        case "FormChangerPlan":

                            #region FormChangerPlan

                            ((FormChangerPlan)CurForm).RefreshData();

                            break;

                            #endregion

                        case "FormAlarmManagement":

                            #region FormAlarmManagement

                            ((FormAlarmManagement)CurForm).RefreshData();

                            break;

                            #endregion


                        case "FormAPCDownloadManagement":

                            #region FormAPCDownloadManagement

                            ((FormAPCDownloadManagement)CurForm).RefreshData();

                            break;

                            #endregion

                        case "FormAPCReportManagement":

                            #region FormProcessDataManagement

                            ((FormAPCReportManagement)CurForm).RefreshData();

                            break;

                            #endregion

                        case "FormDailyCheckManagement":

                            #region FormProcessDataManagement

                            ((FormDailyCheckManagement)CurForm).RefreshData();

                            break;

                            #endregion

                        case "FormEnergyVisualizationManagement":

                            #region FormProcessDataManagement

                            ((FormEnergyVisualizationManagement)CurForm).RefreshData();

                            break;

                            #endregion

                        case "FormProcessDataManagement":

                            #region FormProcessDataManagement

                            ((FormProcessDataManagement)CurForm).RefreshData();

                            break;

                            #endregion

                        case "FormRecipeParameterManagement":

                            #region FormRecipeParameterManagement

                            ((FormRecipeParameterManagement)CurForm).RefreshData();

                            break;

                            #endregion

                        default:
                            break;
                    }
                    #endregion

                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void UserButton_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem _item = (ToolStripMenuItem)sender;


                if (_item.Text == null) return;

                switch (_item.Text.ToString())
                {
                    case "Logout":

                        if (DialogResult.Yes == FrmOPIMessage.QuectionMessage(this, MethodBase.GetCurrentMethod().Name, "LogOut OPI ?"))
                        {
                            #region Logout
                            IsLogout = true;
                            this.DialogResult = DialogResult.OK;
                            #endregion
                        }
                        break;

                    case "Exit":
                        if (DialogResult.Yes == FrmOPIMessage.QuectionMessage(this, MethodBase.GetCurrentMethod().Name, "Exit OPI ?"))
                        {
                            Environment.Exit(0);
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void LanguageToolStrip_Click(object sender, EventArgs e)
        {
            try
            {
                ToolStripMenuItem actItem = sender as ToolStripMenuItem;

                ChangeLanguage(actItem.Tag.ToString());
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void picCSOT_Click(object sender, EventArgs e)
        {
            FormClientStatus _frm = new FormClientStatus() { TopMost = true };
            _frm.Show(this);
        }

        private void lblBCS_Click(object sender, EventArgs e)
        {
            FrmBcMessage.ShowHistoryPageAndNewMessage();
        }

        private void lblOPI_Click(object sender, EventArgs e)
        {
            #region OPI Message
            FrmOPIMessage.TopMost = true;
            FrmOPIMessage.Visible = true;
            #endregion
        }

        private void lblMPLC_Click(object sender, EventArgs e)
        {
            try
            {
                FrmLinkStatus.Visible = true;
                FrmLinkStatus.tmrRefresh.Enabled = true;
                CurForm = FormMainMDI.FrmLinkStatus;
                FrmLinkStatus.BringToFront();
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void tslbBCIP_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                FormRemoteDevice _frm = new FormRemoteDevice() { TopMost = true };
                _frm.Show(this);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void lblLineInfo_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                FormTrxTest _frm = new FormTrxTest();

                if (_frm.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string _trxID = _frm.TrxID;
                    string _msgName = _frm.MsgNmae;
                    XmlDocument _doc = _frm.XmlDoc;

                    SocketDriver.InQueueMessage(_trxID, _msgName, _doc);
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// HELP按鈕連出HTML用
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuButton_BrowserClick(object sender, EventArgs e)
        {
            //取得DB中所需的Help HTML Address
            UniBCSDataContext ctxBRM = FormMainMDI.G_OPIAp.DBCtx;
            var _helpPath = (from _path in ctxBRM.SBRM_OPI_PARAMETER where _path.KEYWORD == "HelpPath" && _path.LINETYPE == FormMainMDI.G_OPIAp.CurLine.LineType select new { _path.SUBKEY, _path.ITEMVALUE }).ToList();
            string HtmlTarget = _helpPath[0].ITEMVALUE.ToString();

            #region 開啟Help網站
            try
            {

                if (DialogResult.Yes == FrmOPIMessage.QuectionMessage(this, MethodBase.GetCurrentMethod().Name, "Open Documentation File in Browser?\r\n" + HtmlTarget))
                {
                    #region 檢查網址是否可以連線(連線Timeout只有1秒)
                    Uri _url = new Uri(HtmlTarget);
                    System.Net.HttpWebRequest _request = System.Net.HttpWebRequest.Create(_url) as System.Net.HttpWebRequest;
                    _request.Timeout = 1000;
                    _request.Method = "Head";
                    System.Net.HttpWebResponse _response = _request.GetResponse() as System.Net.HttpWebResponse;
                    #endregion

                    #region 若網址連線失敗，則開啟本機的help網頁(防呆用)
                    if (_response.StatusCode != System.Net.HttpStatusCode.OK)
                    {
                        FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, "Html Address [{0}] Connect Fail,Will Use Local File.", "", MessageBoxIcon.Error);
                        HtmlTarget = OPIConst.ParamFolder + _helpPath[0].SUBKEY.ToString();
                        if (!File.Exists(HtmlTarget))
                        {
                            FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", string.Format("Couldn't connect to local html: {0}", HtmlTarget), MessageBoxIcon.Error);
                            return;
                        }
                    }
                    #endregion

                    System.Diagnostics.Process.Start(HtmlTarget);
                }
            }
            catch (Exception ex)
            {
                FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", string.Format("Html Address [{0}] Connect Fail,Will Use Local File. ", HtmlTarget), MessageBoxIcon.Error);

                #region 若網址連線失敗，則開啟本機的help網頁
                try
                {
                    HtmlTarget = OPIConst.ParamFolder + _helpPath[0].SUBKEY.ToString();

                    if (!File.Exists(HtmlTarget))
                    {
                        FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", string.Format("Couldn't connect to local html: {0}", HtmlTarget), MessageBoxIcon.Error);
                        return;
                    }
                    System.Diagnostics.Process.Start(HtmlTarget);
                }
                catch (Exception ex2)
                {
                    NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                    FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex2, MessageBoxIcon.Error);
                }
                #endregion
            }
            #endregion
        }
        #endregion

        #region Timer

        private void tmrInitial_Tick(object sender, EventArgs e)
        {
            try
            {
                string _xml = string.Empty;
                string _err = string.Empty;

                if (ConnectBCSResult == G_OPIAp.ReturnCodeSuccess)
                {
                    #region Send AllDataUpdateRequest
                    AllDataUpdateRequest _allDataUpdateRequest = new AllDataUpdateRequest();
                    _allDataUpdateRequest.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    _allDataUpdateRequest.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    _xml = _allDataUpdateRequest.WriteToXml();

                    SocketDriver.SendMessage(_allDataUpdateRequest.HEADER.TRANSACTIONID, _allDataUpdateRequest.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                    #endregion

                    #region Send JobDataCategoryRequest
                    //JobDataCategoryRequest _jobDataCategoryRequest = new JobDataCategoryRequest();
                    //_jobDataCategoryRequest.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                    //_jobDataCategoryRequest.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                    //_jobDataCategoryRequest.BODY.EQUIPMENTNO = "00";

                    //_xml = _jobDataCategoryRequest.WriteToXml();

                    //SocketDriver.SendMessage(_jobDataCategoryRequest.HEADER.TRANSACTIONID, _jobDataCategoryRequest.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                    #endregion

                    #region Send Robot Info Request
                    if (G_OPIAp.Dic_Robot.Count > 0)
                    {
                        #region Send RobotCurrentModeRequest
                        RobotCurrentModeRequest _robotCurrentModeRequest = new RobotCurrentModeRequest();
                        _robotCurrentModeRequest.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                        _robotCurrentModeRequest.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                        _xml = _robotCurrentModeRequest.WriteToXml();

                        SocketDriver.SendMessage(_robotCurrentModeRequest.HEADER.TRANSACTIONID, _robotCurrentModeRequest.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                        #endregion

                        #region Send RobotStageInfoRequest
                        RobotStageInfoRequest _robotStageInfoRequest = new RobotStageInfoRequest();
                        _robotStageInfoRequest.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                        _robotStageInfoRequest.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                        _xml = _robotStageInfoRequest.WriteToXml();

                        SocketDriver.SendMessage(_robotStageInfoRequest.HEADER.TRANSACTIONID, _robotStageInfoRequest.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                        #endregion

                        #region Send RobotUnloaderDispatchRuleRequest
                        RobotUnloaderDispatchRuleRequest _robotUnloaderDispatchRuleRequest = new RobotUnloaderDispatchRuleRequest();
                        _robotUnloaderDispatchRuleRequest.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                        _robotUnloaderDispatchRuleRequest.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;
                        _xml = _robotUnloaderDispatchRuleRequest.WriteToXml();

                        SocketDriver.SendMessage(_robotUnloaderDispatchRuleRequest.HEADER.TRANSACTIONID, _robotUnloaderDispatchRuleRequest.HEADER.MESSAGENAME, _xml, out _err, FormMainMDI.G_OPIAp.SessionID);
                        #endregion

                        #region Send EquipmentDataLinkStatusRequst
                        EquipmentDataLinkStatusRequst _equipmentDataLinkStatusRequst = new EquipmentDataLinkStatusRequst();

                        _equipmentDataLinkStatusRequst.HEADER.REPLYSUBJECTNAME = FormMainMDI.G_OPIAp.SessionID;
                        _equipmentDataLinkStatusRequst.BODY.LINENAME = FormMainMDI.G_OPIAp.CurLine.ServerName;

                        SocketDriver.SendMessage(_equipmentDataLinkStatusRequst.HEADER.TRANSACTIONID, _equipmentDataLinkStatusRequst.HEADER.MESSAGENAME, _equipmentDataLinkStatusRequst.WriteToXml(), out _err, FormMainMDI.G_OPIAp.SessionID);

                        G_OPIAp.BC_EquipmentDataLinkStatusReply.IsReply = false;
                        #endregion

                    }
                    #endregion

                    tmrInitial.Enabled = false;
                }
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void tmrRefresh_Tick(object sender, EventArgs e)
        {
            try
            {
                if (SocketDriver.IsConnected(G_OPIAp.SessionID))
                { if (lblBCS.BackColor != Color.Lime) lblBCS.BackColor = Color.Lime; }
                else
                    if (lblBCS.BackColor != Color.Red) lblBCS.BackColor = Color.Red;

                #region Current DateTime
                lblDateTime.Text = string.Format("[ {0} ]", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                #endregion

                #region MES Mode
                if (lblLineControl.Text != G_OPIAp.CurLine.MesControlMode)
                {
                    lblLineControl.Text = G_OPIAp.CurLine.MesControlMode;

                    switch (G_OPIAp.CurLine.MesControlMode)
                    {
                        case "OFFLINE":
                            lblLineControl.ForeColor = Color.Red;
                            break;
                        case "LOCAL":
                            lblLineControl.ForeColor = Color.YellowGreen;
                            break;
                        case "REMOTE":
                            lblLineControl.ForeColor = Color.Lime;
                            break;
                        default:
                            break;
                    }
                }

                if (lblLineControl2.Text != G_OPIAp.CurLine.MesControlMode2)
                {
                    lblLineControl2.Text = G_OPIAp.CurLine.MesControlMode2;

                    switch (G_OPIAp.CurLine.MesControlMode2)
                    {
                        case "OFFLINE":
                            lblLineControl2.ForeColor = Color.Red;
                            break;
                        case "LOCAL":
                            lblLineControl2.ForeColor = Color.YellowGreen;
                            break;
                        case "REMOTE":
                            lblLineControl2.ForeColor = Color.Lime;
                            break;
                        default:
                            break;
                    }
                }
                #endregion

                #region Line Status
                if (lblLineStatus.Text != G_OPIAp.CurLine.LineStatus)
                {
                    lblLineStatus.Text = G_OPIAp.CurLine.LineStatus;

                    switch (G_OPIAp.CurLine.LineStatus)
                    {
                        case "DOWN":
                        case "STOP":
                            lblLineStatus.ForeColor = Color.Red;
                            break;
                        case "IDLE":
                            lblLineStatus.ForeColor = Color.YellowGreen;
                            break;
                        case "RUN":
                            lblLineStatus.ForeColor = Color.Lime;
                            break;
                        default:
                            break;
                    }
                }
                #endregion

                #region MPLC Status
                if (lblMPLC.Text != G_OPIAp.CurLine.PLCStatus)
                {
                    lblMPLC.Text = G_OPIAp.CurLine.PLCStatus;

                    switch (G_OPIAp.CurLine.PLCStatus)
                    {
                        case "OFF":
                            lblMPLC.ForeColor = Color.Red;
                            break;
                        case "ON":
                            lblMPLC.ForeColor = Color.Lime;
                            break;
                        default:
                            break;
                    }
                }
                #endregion

                #region Robot Run Mode
                if (G_OPIAp.Dic_Robot.Count > 0)
                {
                    foreach (Robot _rb in G_OPIAp.Dic_Robot.Values)
                    {

                        if (lblCtrlMode.Text.ToString() != _rb.RobotControlMode) lblCtrlMode.Text = _rb.RobotControlMode ;
                        if (lblRobotStatus.Text.ToString() != _rb.RobotStatus.ToString()) lblRobotStatus.Text = _rb.RobotStatus.ToString() ;
                        if (lblHaveCmd.Text.ToString() !=  (_rb.HaveRobotCommand ? "Y" : "N")) lblHaveCmd.Text =  _rb.HaveRobotCommand ? "Y" : "N" ;
                        if (lblCurrentPosition.Text.ToString() != _rb.CurrentPosition) lblCurrentPosition.Text = _rb.CurrentPosition ;
                        if (lblHoldStatus.Text.ToString() != (_rb.HoldStatus ? "Hold" : "Release")) lblHoldStatus.Text = _rb.HoldStatus ? "Hold" : "Release";   
                    }
                }

                #endregion

                #region Link Status
                BCS_EquipmentDataLinkStatusReply _reply = G_OPIAp.BC_EquipmentDataLinkStatusReply;
                bool _isError = false;
                int _idx = 0;

                if (_reply.BatonPassStatus_W != "0".PadLeft(_reply.BatonPassStatus_W.Length, '0') ||
                    _reply.BatonPassInterruption_W != "0".PadLeft(_reply.BatonPassInterruption_W.Length, '0') ||
                    _reply.DataLinkStop_W != "0".PadLeft(_reply.DataLinkStop_W.Length, '0') ||
                    _reply.StationLoopStatus_W != "0".PadLeft(_reply.StationLoopStatus_W.Length, '0') ||
                    _reply.BatonPassStatus_B.Substring(0, 1) != "0" ||
                    _reply.CyclicTransmissionStatus_B.Substring(0, 1) != "0")
                {
                    _isError = true;
                }
                else
                {
                    foreach (Node _node in G_OPIAp.Dic_Node.Values)
                    {
                        if (_node.ReportMode.Contains("PLC")) int.TryParse(_node.NodeNo.Substring(1), out _idx);
                        else continue;

                        if (_reply.BatonPassStatus_B.Substring(_idx - 1, 1) == "1" || _reply.CyclicTransmissionStatus_B.Substring(_idx - 1, 1) == "1")
                            _isError = true;
                    }
                }

                if (_isError)
                {
                    pnlPLCStatus.BackColor = pnlPLCStatus.BackColor == Color.Black ? Color.Red : Color.Black;
                }
                else pnlPLCStatus.BackColor = Color.Black;

                #endregion

                #region 停止工具列閃爍
                if (IsFlash)
                {
                    if (FrmOPIMessage.Visible == false && OPIInfo.Q_OPIMessage.Count <= 0)
                    {
                        IsFlash = false;

                        #region Stop Flash
                        if (pnlMain.InvokeRequired)
                        {
                            this.BeginInvoke(new MethodInvoker(
                               delegate
                               {
                                   FLASHWINFO fi = Create_FLASHWINFO(this.Handle, FLASHW_STOP, uint.MaxValue, 0);
                                   FlashWindowEx(ref fi);
                               }));
                        }
                        else
                        {
                            FLASHWINFO fi = Create_FLASHWINFO(this.Handle, FLASHW_STOP, uint.MaxValue, 0);
                            FlashWindowEx(ref fi);
                        }
                        #endregion
                    }
                }
                #endregion
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        //侦测是否需要自动登出
        private void OnTimedEvent(Object sender, ElapsedEventArgs e)
        {
            TimeSpan ts = DateTime.Now - lastOperationTime;
            if (G_OPIAp.AutoLogoutTime == 0) // 时间为0时则不会登出
                return;

            //NLogManager.Logger.LogInfoWrite("OnTimedEvent", "OnTimedEvent", string.Format("ts.TotalMinutes [{0}], AutoLogoutTime [{1}]", ts.TotalMinutes.ToString(), G_OPIAp.AutoLogoutTime.ToString()));

            if (ts.TotalMinutes > G_OPIAp.AutoLogoutTime)
            {
                if (G_OPIAp.SuperUserList.Contains(G_OPIAp.LoginUserID)) return;
                //this.UserButton_Click(logoutToolStripMenuItem1, null);
                NLogManager.Logger.LogInfoWrite("Logout", "Logout", string.Format("lastOperationTime [{0}] , ts.TotalMinutes [{1}], AutoLogoutTime [{2}]", lastOperationTime.ToString("yyyy-MM-dd HH:mm:ss ffff"), ts.TotalMinutes.ToString(), G_OPIAp.AutoLogoutTime.ToString()));
                //FrmOPIMessage.ShowMessage(this, "登出訊息", "", string.Format("lastOperationTime [{0}] , ts.TotalMinutes [{1}], AutoLogoutTime [{2}]", lastOperationTime.ToString("yyyy-MM-dd HH:mm:ss ffff"), ts.TotalMinutes.ToString(), G_OPIAp.AutoLogoutTime.ToString()), MessageBoxIcon.Information);
                this.DialogResult = DialogResult.OK;
            }
        }

        #endregion

        #region Socket

        private void SocketDriver_SocketClose()
        {
            try
            {
                if (IsLogout == false)
                {
                    if (this.DialogResult == System.Windows.Forms.DialogResult.None)
                        this.DialogResult = DialogResult.Cancel;
                }
                else
                    this.DialogResult = DialogResult.OK;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SocketDriver_SocketErrorMessage(SocketLogLevelType LogLevle, string MethodName, string Message)
        {
            try
            {
                switch (LogLevle)
                {
                    case SocketLogLevelType.DEBUG:
                        break;

                    case SocketLogLevelType.TRACE:
                        break;

                    case SocketLogLevelType.EXCEPTION:
                        if (Message.Contains("10061"))//無法連線，因為目標電腦拒絕連線。 
                        {
                            if (FormMainMDI.G_OPIAp.IsRunVshost)
                            {
                                ConnectBCSResult = G_OPIAp.ReturnCodeSuccess;
                                break;
                            }
                            else
                            {
                                socketRryCount = socketRryCount + 1;
                                if (socketRryCount >= FormMainMDI.G_OPIAp.SocketRetryCount)
                                {

                                    SocketDriver.Stop();

                                    ConnectBCSResult = "Connect BCS Error";
                                    this.DialogResult = DialogResult.Retry;

                                    break;
                                }
                            }

                        }
                        else
                        {
                            FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, "", Message, MessageBoxIcon.Error);
                        }
                        break;

                    case SocketLogLevelType.ERROR:
                        //MessageBox.Show(new Form() { TopMost = true }, Message, MethodBase.GetCurrentMethod().Name, MessageBoxButtons.OK, MessageBoxIcon.Error);
                        break;
                    default:
                        break;
                }

            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void SocketDriver_ReceiveErrorMessage(string MethodName, string TrxName, string ErrCode, string ErrMsg)
        {
            try
            {
                string _errMsg = string.Format("BCS Return Error , [{0}] {1}", TrxName, ErrMsg);

                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodName, _errMsg);
                FrmOPIMessage.ShowMessage(this, MethodName, ErrCode, _errMsg, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }

        private void bgwSocket_DoWork(object sender, DoWorkEventArgs e)
        {
            XmlDocument _xmlDoc = new XmlDocument();
            string _trxID = string.Empty;
            string _msgName = string.Empty;
            string _bcsXml = string.Empty;
            string _err = string.Empty;
            string _key = string.Empty;
            string _xml = string.Empty;
            string _data = string.Empty;
            string _errMsg = string.Empty;
            int _num = 0;

            while (IsRun)
            {
                Tuple<string, string, XmlDocument> _msg = null;

                try
                {

                    if (ConnectBCSResult == string.Empty)
                    {
                        continue;
                    }
                    else if (ConnectBCSResult != G_OPIAp.ReturnCodeSuccess)
                    {
                        SocketDriver.Stop();
                        return;
                    }

                    //trxid,msgname,xmldocument
                    _msg = SocketDriver.PollMessage();

                    if (_msg == null) continue;

                    _trxID = _msg.Item1;
                    _msgName = _msg.Item2;
                    _xmlDoc = _msg.Item3;

                    _bcsXml = _xmlDoc.InnerXml;


                    UniAuto.UniBCS.OpiSpec.Message bcs_message = Spec.CheckXMLFormat(_bcsXml);

                    _errMsg = UniTools.CheckBodyLineName(bcs_message.GetBody());

                    if (_errMsg != string.Empty)
                    {
                        FrmOPIMessage.ShowMessage(this, bcs_message.HEADER.MESSAGENAME, "", _errMsg, MessageBoxIcon.Error);
                        continue;
                    }

                    if (!string.IsNullOrEmpty(bcs_message.RETURN.RETURNCODE) && bcs_message.RETURN.RETURNCODE != FormMainMDI.G_OPIAp.ReturnCodeSuccess)
                    {
                        FrmOPIMessage.ShowMessage(this, bcs_message.HEADER.MESSAGENAME, bcs_message.RETURN, MessageBoxIcon.Error);
                        continue;
                    }

                    #region switch (_msgName)
                    switch (_msgName)
                    {
                        case "BCSTerminalMessageInform":

                            #region BCSTerminalMessageInform

                            BCSTerminalMessageInform _bcsTerminalMessageInform = (BCSTerminalMessageInform)Spec.CheckXMLFormat(_bcsXml);

     
                            #region Update Data
                            OPIInfo.Q_BCMessage.Enqueue(string.Format("{0}^{1}^{2}", _bcsTerminalMessageInform.HEADER.TRANSACTIONID, _bcsTerminalMessageInform.BODY.DATETIME, _bcsTerminalMessageInform.BODY.TERMINALTEXT));
                            #endregion

                            break;
                            #endregion

                        case "AllDataUpdateReply":

                            #region AllDataUpdateReply
                            AllDataUpdateReply _allDataUpdateReply = (AllDataUpdateReply)Spec.CheckXMLFormat(_bcsXml);

                            #region Update AllDataUpdateReply
                            G_OPIAp.CurLine.SetLineInfo(_allDataUpdateReply);

                            #region Update EQUIPMENTLIST
                            foreach (AllDataUpdateReply.EQUIPMENTc _eq in _allDataUpdateReply.BODY.EQUIPMENTLIST)
                            {
                                _key = _eq.EQUIPMENTNO;

                                if (!G_OPIAp.Dic_Node.ContainsKey(_key))
                                {
                                    FrmOPIMessage.ShowMessage(this, _msgName, "", string.Format("Can't find EQUIPMENTNO[{0}]", _eq.EQUIPMENTNO), MessageBoxIcon.Error);
                                    continue;
                                }
                                G_OPIAp.Dic_Node[_key].SetNodeInfo(_eq);
                            }
                            #endregion

                            #region Update Pallet
                            foreach (AllDataUpdateReply.PALLETc _pallet in _allDataUpdateReply.BODY.PALLETLIST)
                            {
                                _key = _pallet.PALLETNO;

                                if (!G_OPIAp.Dic_Pallet.ContainsKey(_key))
                                {
                                    FrmOPIMessage.ShowMessage(this, _msgName, "", string.Format("Can't find Pallet No[{0}]", _pallet.PALLETNO), MessageBoxIcon.Error);
                                    continue;
                                }
                                G_OPIAp.Dic_Pallet[_key].SetPalletInfo(_pallet);
                            }
                            #endregion

                            #endregion

                            break;

                            #endregion

                        case "AllEquipmentStatusReply":

                            #region AllEquipmentStatusReply
                            AllEquipmentStatusReply _allEquipmentStatusReply = (AllEquipmentStatusReply)Spec.CheckXMLFormat(_bcsXml);

                            #region Update AllEquipmentStatusReply
                            foreach (AllEquipmentStatusReply.EQUIPMENTc _eq in _allEquipmentStatusReply.BODY.EQUIPMENTLIST)
                            {
                                _key = _eq.EQUIPMENTNO;

                                if (!G_OPIAp.Dic_Node.ContainsKey(_key))
                                {
                                    FrmOPIMessage.ShowMessage(this, _msgName, "", string.Format("Can't find Equipment[{0}]", _eq.EQUIPMENTNO), MessageBoxIcon.Error);
                                    continue;
                                }
                                G_OPIAp.Dic_Node[_key].SetNodeInfo(_eq);
                            }

                            #endregion

                            break;
                            #endregion

                        case "LineStatusReport":

                            #region LineStatusReport

                            LineStatusReport _lineStatusReport = (LineStatusReport)Spec.CheckXMLFormat(_bcsXml);


                            #region Update Data
                            FormMainMDI.G_OPIAp.CurLine.SetLineInfo(((LineStatusReport)Spec.CheckXMLFormat(_xmlDoc)));
                            #endregion

                            break;
                            #endregion

                        case "LineStatusReply":

                            #region LineStatusReply

                            LineStatusReply _LineStatusReply = (LineStatusReply)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            FormMainMDI.G_OPIAp.CurLine.SetLineInfo(_LineStatusReply);
                            #endregion

                            break;
                            #endregion

                        //Add By Yangzhenteng For PI OPI Display 20180904
                        case "MaterialRealWeightReport":
                            #region MaterialRealWeightReport
                            UniAuto.UniBCS.OpiSpec.MaterialRealWeightReport _MaterialRealWeight = (MaterialRealWeightReport)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            FormMainMDI.G_OPIAp.CurMaterialRealWeight.SetMaterialRealWeightInfo((MaterialRealWeightReport)Spec.CheckXMLFormat(_xmlDoc));

                            #endregion
                            break;
                            #endregion

                        //add by hujunpeng 20190723 for PIJOBCOUNT MONITOR

                        case "PIJobCountReport":
                            #region PIJobCountReport
                            UniAuto.UniBCS.OpiSpec.PIJobCountReport _PIJobCountReport = (PIJobCountReport)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            FormMainMDI.G_OPIAp.PIJobCount.SetPIJobCountInfo((PIJobCountReport)Spec.CheckXMLFormat(_xmlDoc));
                            #endregion

                            break;
                            #endregion

                        case "DefectCodeReportReply":

                            #region DefectCodeReportReply
                            DefectCodeReportReply _defectCodeReportReply = (DefectCodeReportReply)Spec.CheckXMLFormat(_bcsXml);
                            
                            #region Update Data
                            _key = _defectCodeReportReply.BODY.EQUIPMENTNO;

                            if (!G_OPIAp.Dic_Node.ContainsKey(_key))
                            {
                                FrmOPIMessage.ShowMessage(this, _msgName, "", string.Format("Can't find Equipment No[{0}]", _defectCodeReportReply.BODY.EQUIPMENTNO), MessageBoxIcon.Error);
                                continue;
                            }

                            if (G_OPIAp.Dic_Node[_key].BC_DefectCodeReportReply.LstDefect.Count > 0)
                            {
                                G_OPIAp.Dic_Node[_key].BC_DefectCodeReportReply.LstDefect.Clear();
                            }

                            for (int i = 0; i < _defectCodeReportReply.BODY.DEFECTLIST.Count; i++)
                            {
                                DEFECTc _trx = new DEFECTc();
                                _trx.CassetteSeqNo = _defectCodeReportReply.BODY.DEFECTLIST[i].CASSETTE_SEQNO;
                                _trx.DefectCode = _defectCodeReportReply.BODY.DEFECTLIST[i].DEFECT_CODE;

                                if (G_OPIAp.Dic_Unit.ContainsKey(_key.PadRight(3, ' ') + _defectCodeReportReply.BODY.DEFECTLIST[i].UNITNO.ToString().PadLeft(2, '0')))
                                    _trx.UnitID = G_OPIAp.Dic_Unit[_key.PadRight(3, ' ') + _defectCodeReportReply.BODY.DEFECTLIST[i].UNITNO.ToString().PadLeft(2, '0')].UnitID;
                                else
                                    _trx.UnitID = "0";

                                _trx.JobSeqNo = _defectCodeReportReply.BODY.DEFECTLIST[i].JOB_SEQNO;
                                _trx.UnitNo = _defectCodeReportReply.BODY.DEFECTLIST[i].UNITNO;
                                _trx.ChipPosition = _defectCodeReportReply.BODY.DEFECTLIST[i].CHIP_POSITION;

                                G_OPIAp.Dic_Node[_key].BC_DefectCodeReportReply.LstDefect.Add(_trx);
                            }
                            #endregion
                            break;
                            #endregion

                        case "EquipmentStatusReport":

                            #region EquipmentStatusReport
                            EquipmentStatusReport _equipmentStatusReport = (EquipmentStatusReport)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            _key = _equipmentStatusReport.BODY.EQUIPMENTNO;
                            if (!G_OPIAp.Dic_Node.ContainsKey(_key))
                            {
                                FrmOPIMessage.ShowMessage(this, _msgName, "", string.Format("Can't find EQUIPMENTNO[{0}]", _equipmentStatusReport.BODY.EQUIPMENTNO), MessageBoxIcon.Error);
                                continue;
                            }
                            G_OPIAp.Dic_Node[_key].SetNodeInfo(_equipmentStatusReport);
                            #endregion


                            break;
                            #endregion

                        case "EquipmentStatusReply":

                            #region EquipmentStatusReply

                            EquipmentStatusReply _equipmentStatusReply = (EquipmentStatusReply)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            _key = _equipmentStatusReply.BODY.EQUIPMENTNO;
                            if (!G_OPIAp.Dic_Node.ContainsKey(_key))
                            {
                                FrmOPIMessage.ShowMessage(this, _msgName, "", string.Format("Can't find Equipment No[{0}]", _equipmentStatusReply.BODY.EQUIPMENTNO), MessageBoxIcon.Error);
                                continue;
                            }
                            G_OPIAp.Dic_Node[_key].SetNodeInfo(_equipmentStatusReply);
                            #endregion

                            break;
                            #endregion

                        case "EquipmentAlarmStatusReply":

                            #region EquipmentAlarmStatusReply

                            EquipmentAlarmStatusReply _equipmentAlarmStatusReply = (EquipmentAlarmStatusReply)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            foreach (EquipmentAlarmStatusReply.EQUIPMENTc _eq in _equipmentAlarmStatusReply.BODY.EQUIPMENTLIST)
                            {
                                _key = _eq.EQUIPMENTNO;
                                if (!G_OPIAp.Dic_Node.ContainsKey(_key))
                                {
                                    FrmOPIMessage.ShowMessage(this, _msgName, "", string.Format("Can't find Equipment No[{0}]", _key), MessageBoxIcon.Error);
                                    continue;
                                }

                                G_OPIAp.Dic_Node[_key].BC_EquipmentAlarmStatusReply.IsReply = true;

                                G_OPIAp.Dic_Node[_key].BC_EquipmentAlarmStatusReply.Lst_RealAlarm.Clear();

                                Alarm _alarm = null;

                                foreach (EquipmentAlarmStatusReply.ALARMc _realAlarm in _eq.ALARMLIST)
                                {
                                    _alarm = new Alarm();
                                    _alarm.AlarmID = _realAlarm.ALARMID;
                                    _alarm.AlarmLevel = _realAlarm.ALARMLEVEL;
                                    _alarm.AlarmText = _realAlarm.ALARMTEXT;
                                    _alarm.AlarmUnit = _realAlarm.ALARMUNIT;
                                    _alarm.AlarmCode = _realAlarm.ALARMCODE;

                                    G_OPIAp.Dic_Node[_key].BC_EquipmentAlarmStatusReply.Lst_RealAlarm.Add(_alarm);
                                }
                            }
                            #endregion

                            break;
                            #endregion

                        case "LinkSignalDataReply":

                            #region LinkSignalDataReply
                            LinkSignalDataReply _linkSignalDataReply = (LinkSignalDataReply)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            ////I0200030001
                            //string _pipeKey = string.Format("I{0}{1}{2}{3}{4}{5}", _linkSignalDataReply.BODY.UPSTREAMEQUIPMENTNO.Substring(1).PadLeft(2, '0'), _linkSignalDataReply.BODY.UPSTREAMEQUIPMENTUNITNO.PadLeft(2, '0'),
                            //    _linkSignalDataReply.BODY.DOWNSTREAMEQUIPMENTNO.Substring(1).PadLeft(2, '0'), _linkSignalDataReply.BODY.DOWNSTREAMEQUIPMENTUNITNO.PadLeft(2, '0'),
                            //    _linkSignalDataReply.BODY.UPSTREAMSEQUENCENO.PadLeft(2, '0'), _linkSignalDataReply.BODY.DOWNSTREAMSEQUENCENO.PadLeft(2, '0'));

                            //I0200030001
                            string _upLocal = _linkSignalDataReply.BODY.UPSTREAMEQUIPMENTNO.Substring(1).PadLeft(2, '0');
                            string _dnLocal = _linkSignalDataReply.BODY.DOWNSTREAMEQUIPMENTNO.Substring(1).PadLeft(2, '0');

                            string _upUnit = _linkSignalDataReply.BODY.UPSTREAMEQUIPMENTUNITNO.PadLeft(2, '0');
                            string _downUnit = _linkSignalDataReply.BODY.DOWNSTREAMEQUIPMENTUNITNO.PadLeft(2, '0');
                            string _upSeqNo = _linkSignalDataReply.BODY.UPSTREAMSEQUENCENO.PadLeft(2, '0');
                            string _dnseqNo = _linkSignalDataReply.BODY.DOWNSTREAMSEQUENCENO.PadLeft(2, '0');

                            string _pipeKey = string.Format("I{0}{1}{2}{3}{4}{5}", _upLocal, _upUnit, _dnLocal, _downUnit, _upSeqNo, _dnseqNo);

                            if (G_OPIAp.Dic_Pipe.ContainsKey(_pipeKey))
                            {
                                Interface _if = G_OPIAp.Dic_Pipe[_pipeKey];

                                #region Upstream
                                _if.UpstreamSignal = _linkSignalDataReply.BODY.UPSTREAMSIGNAL.PadRight(32, '0');
                                _if.UpstreamBitAddress = _linkSignalDataReply.BODY.UPSTREAMBITADDRESS;

                                foreach (LinkSignalDataReply.JOBDATAc _upReply in _linkSignalDataReply.BODY.UPSTREAMJOBDATALIST)
                                {
                                    JobData _upJob = _if.UpstreamJobData.Find(r => r.JobAddress.Equals(_upReply.JOBADDRESS));

                                    if (_upJob == null)
                                    {
                                        #region Upstream New Job
                                        _upJob = new JobData();
                                        _upJob.JobAddress = _upReply.JOBADDRESS;
                                        _upJob.CassetteSeqNo = _upReply.CASSETTESEQNO;
                                        _upJob.JobSeqNo = _upReply.JOBSEQNO;
                                        _upJob.ProductType = _upReply.PRODUCTTYPE;
                                        _upJob.SubStrateType = _upReply.SUBSTRATETYPE;
                                        _upJob.JobType = _upReply.JOBTYPE;
                                        _upJob.JobJudge = _upReply.JOBJUDGE;
                                        _upJob.JobGrade = _upReply.JOBGRADE;
                                        _upJob.GlassID = _upReply.GLASSID;
                                        _upJob.PPID = _upReply.PPID;
                                        _upJob.TrackingData = _upReply.TRACKINGDATA;
                                        _upJob.EQPFlag = _upReply.EQPFLAG;

                                        _if.UpstreamJobData.Add(_upJob);
                                        #endregion
                                    }
                                    else
                                    {
                                        #region Upstream old Job
                                        _upJob.JobAddress = _upReply.JOBADDRESS;
                                        _upJob.CassetteSeqNo = _upReply.CASSETTESEQNO;
                                        _upJob.JobSeqNo = _upReply.JOBSEQNO;
                                        _upJob.ProductType = _upReply.PRODUCTTYPE;
                                        _upJob.SubStrateType = _upReply.SUBSTRATETYPE;
                                        _upJob.JobType = _upReply.JOBTYPE;
                                        _upJob.JobJudge = _upReply.JOBJUDGE;
                                        _upJob.JobGrade = _upReply.JOBGRADE;
                                        _upJob.GlassID = _upReply.GLASSID;
                                        _upJob.PPID = _upReply.PPID;
                                        _upJob.TrackingData = _upReply.TRACKINGDATA;
                                        _upJob.EQPFlag = _upReply.EQPFLAG;
                                        #endregion
                                    }
                                }
                                #endregion

                                #region Downstream
                                _if.DownstreamSignal = _linkSignalDataReply.BODY.DOWNSTREAMSIGNAL.PadRight(32, '0');
                                _if.DownstreamBitAddress = _linkSignalDataReply.BODY.DOWNSTREAMBITADDRESS;

                                foreach (LinkSignalDataReply.JOBDATAc _downReply in _linkSignalDataReply.BODY.DOWNSTREAMJOBDATALIST)
                                {
                                    JobData _downJob = _if.DownstreamJobData.Find(r => r.JobAddress.Equals(_downReply.JOBADDRESS));

                                    if (_downJob == null)
                                    {
                                        #region Upstream New Job
                                        _downJob = new JobData();
                                        _downJob.JobAddress = _downReply.JOBADDRESS;
                                        _downJob.CassetteSeqNo = _downReply.CASSETTESEQNO;
                                        _downJob.JobSeqNo = _downReply.JOBSEQNO;
                                        _downJob.ProductType = _downReply.PRODUCTTYPE;
                                        _downJob.SubStrateType = _downReply.SUBSTRATETYPE;
                                        _downJob.JobType = _downReply.JOBTYPE;
                                        _downJob.JobJudge = _downReply.JOBJUDGE;
                                        _downJob.JobGrade = _downReply.JOBGRADE;
                                        _downJob.GlassID = _downReply.GLASSID;
                                        _downJob.PPID = _downReply.PPID;
                                        _downJob.TrackingData = _downReply.TRACKINGDATA;
                                        _downJob.EQPFlag = _downReply.EQPFLAG;

                                        _if.DownstreamJobData.Add(_downJob);
                                        #endregion
                                    }
                                    else
                                    {
                                        #region Upstream old Job
                                        _downJob.JobAddress = _downReply.JOBADDRESS;
                                        _downJob.CassetteSeqNo = _downReply.CASSETTESEQNO;
                                        _downJob.JobSeqNo = _downReply.JOBSEQNO;
                                        _downJob.ProductType = _downReply.PRODUCTTYPE;
                                        _downJob.SubStrateType = _downReply.SUBSTRATETYPE;
                                        _downJob.JobType = _downReply.JOBTYPE;
                                        _downJob.JobJudge = _downReply.JOBJUDGE;
                                        _downJob.JobGrade = _downReply.JOBGRADE;
                                        _downJob.GlassID = _downReply.GLASSID;
                                        _downJob.PPID = _downReply.PPID;
                                        _downJob.TrackingData = _downReply.TRACKINGDATA;
                                        _downJob.EQPFlag = _downReply.EQPFLAG;
                                        #endregion
                                    }
                                }
                                #endregion

                                #region 更新已回覆flag
                                _if.IsReply = true;
                                _if.LastReceiveMsgDateTime = DateTime.Now;
                                #endregion
                            }

                            #endregion

                            break;
                            #endregion

                        case "StagePositionInfoReply":

                            #region StagePositionInfoReply
                            StagePositionInfoReply _stagePositionInfoReply = (StagePositionInfoReply)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            var _varPositionInfoReply = G_OPIAp.Lst_RobotStage.Where(r => r.RobotName.Equals(_stagePositionInfoReply.BODY.ROBOTNAME) && r.StageID.Equals(_stagePositionInfoReply.BODY.STAGEID));

                            if (_varPositionInfoReply.Count() == 0) continue;

                            RobotStage _stagePosition = _varPositionInfoReply.First();

                            if (_stagePosition.BC_StagePositionInfoReply == null) continue;

                            BCS_StagePositionInfoReply _bcKeep = _stagePosition.BC_StagePositionInfoReply;

                            if (_stagePositionInfoReply.BODY.EQUIPMENTNO == _bcKeep.NodeNo)
                            {
                                #region 更新已回覆flag
                                _bcKeep.IsReply = true;
                                _bcKeep.LastReceiveMsgDateTime = DateTime.Now;
                                #endregion

                                _bcKeep.SendReady = _stagePositionInfoReply.BODY.SENDYREADY == "1" ? true : false;
                                _bcKeep.ReceiveReady = _stagePositionInfoReply.BODY.RECEIVEREADY == "1" ? true : false;
                                _bcKeep.ExchangePossible = _stagePositionInfoReply.BODY.EXCHANGEPOSSIBLE == "1" ? true : false;
                                _bcKeep.DoubleGlassExist = _stagePositionInfoReply.BODY.DOUBLEGLASSEXIST == "1" ? true : false;
                            }

                            #endregion

                            break;
                            #endregion

                        case "PortCSTStatusReport":

                            #region PortCSTStatusReport

                            PortCSTStatusReport _portCSTStatusReport = (PortCSTStatusReport)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            _key = _portCSTStatusReport.BODY.EQUIPMENTNO.PadRight(3, ' ') + _portCSTStatusReport.BODY.PORTNO.PadRight(2, ' ');
                            if (!G_OPIAp.Dic_Port.ContainsKey(_key))
                            {
                                FrmOPIMessage.ShowMessage(this, _msgName, "", string.Format("Can't find Equipement No[{0}],Port No [{1}]", _portCSTStatusReport.BODY.EQUIPMENTNO, _portCSTStatusReport.BODY.PORTNO), MessageBoxIcon.Error);
                                continue;
                            }
                            G_OPIAp.Dic_Port[_key].SetPortInfo(_portCSTStatusReport);
                            #endregion

                            break;
                            #endregion

                        case "PortStatusReply":

                            #region PortStatusReply

                            PortStatusReply _portStatusReply = (PortStatusReply)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            _key = _portStatusReply.BODY.EQUIPMENTNO.PadRight(3, ' ') + _portStatusReply.BODY.PORTNO.PadRight(2, ' ');
                            if (!FormMainMDI.G_OPIAp.Dic_Port.ContainsKey(_key))
                            {
                                FrmOPIMessage.ShowMessage(this, _msgName, "", string.Format("Can't find Equipement No[{0}],Port No [{1}]", _portStatusReply.BODY.EQUIPMENTNO, _portStatusReply.BODY.PORTNO), MessageBoxIcon.Error);
                                continue;
                            }
                            FormMainMDI.G_OPIAp.Dic_Port[_key].SetPortInfo(_portStatusReply);
                            #endregion

                            break;
                            #endregion

                        case "DenseStatusReport":

                            #region DenseStatusReport
                            DenseStatusReport _denseStatusReport = (DenseStatusReport)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            _key = _denseStatusReport.BODY.EQUIPMENTNO.PadRight(3, ' ') + _denseStatusReport.BODY.PORTNO.PadRight(2, ' ');
                            if (!G_OPIAp.Dic_Dense.ContainsKey(_key))
                            {
                                FrmOPIMessage.ShowMessage(this, _msgName, "", string.Format("Can't find Equipement No[{0}],Port No [{1}]", _denseStatusReport.BODY.EQUIPMENTNO, _denseStatusReport.BODY.PORTNO), MessageBoxIcon.Error);
                                continue;
                            }
                            G_OPIAp.Dic_Dense[_key].SetDenseInfo(_denseStatusReport);
                            #endregion

                            break;
                            #endregion

                        case "PalletStatusReport":

                            #region PalletStatusReport
                            PalletStatusReport _palletStatusReport = (PalletStatusReport)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            _key = _palletStatusReport.BODY.PALLETNO;

                            if (!G_OPIAp.Dic_Pallet.ContainsKey(_key))
                            {
                                FrmOPIMessage.ShowMessage(this, _msgName, "", string.Format("Can't find Pallet No[{0}]", _palletStatusReport.BODY.PALLETNO), MessageBoxIcon.Error);
                                continue;
                            }
                            G_OPIAp.Dic_Pallet[_key].SetPalletInfo(_palletStatusReport);
                            #endregion

                            break;
                            #endregion

                        case "RobotCurrentModeReply":

                            #region RobotCurrentModeReply

                            RobotCurrentModeReply _robotCurrentModeReply = (RobotCurrentModeReply)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            foreach (RobotCurrentModeReply.EQUIPMENTc _eq in _robotCurrentModeReply.BODY.EQUIPMENTLIST)
                            {
                                foreach (RobotCurrentModeReply.ROBOTc _robot in _eq.ROBOTLIST)
                                {
                                    if (G_OPIAp.Dic_Robot.ContainsKey(_robot.ROBOTNAME))
                                    {
                                        G_OPIAp.Dic_Robot[_robot.ROBOTNAME].RobotControlMode = _robot.ROBOTMODE;
                                        G_OPIAp.Dic_Robot[_robot.ROBOTNAME].RobotStatus = (eRobotStatus)int.Parse(_robot.ROBOTSTATUS);
                                        G_OPIAp.Dic_Robot[_robot.ROBOTNAME].HaveRobotCommand = _robot.HAVE_ROBOT_CMD == "1" ? true : false;
                                        G_OPIAp.Dic_Robot[_robot.ROBOTNAME].CurrentPosition = _robot.CURRENT_POSITION;
                                        G_OPIAp.Dic_Robot[_robot.ROBOTNAME].HoldStatus = _robot.HOLD_STATUS == "1" ? true : false;
                                        G_OPIAp.Dic_Robot[_robot.ROBOTNAME].SampEQPFlag = _robot.SAMEEQFLAG == "Y" ? true : false;
                                        
                                        foreach (RobotCurrentModeReply.ARMc _arm in _robot.ARMLIST)
                                        {
                                            ArmInfo _armInfo = G_OPIAp.Dic_Robot[_robot.ROBOTNAME].LstArms.Where(r => r.ArmNo == _arm.ARMNO).First();

                                            _armInfo.ArmEnable = _arm.ARM_ENABLE == "Y" ? true : false;

                                            //Arm上面可放glass 最大數量,當max count = 1 時，只需要給front arm資料；max count = 2時fron arm & back arm都需要給資料
                                            _armInfo.JobExist_Front = _arm.FORK_FRONT_JOBEXIST == string.Empty ? eRobotJobStatus.UnKnown : (eRobotJobStatus)int.Parse(_arm.FORK_FRONT_JOBEXIST);
                                            _armInfo.CstSeqNo_Front = _arm.FORK_FRONT_CSTSEQ;
                                            _armInfo.JobSeqNo_Front = _arm.FORK_FRONT_JOBSEQ;
                                            _armInfo.TrackingData_Front = _arm.FORK_FRONT_TRACKINGVALUE;
                                            if (G_OPIAp.Dic_Robot[_robot.ROBOTNAME].ArmMaxJobCount == 2)
                                            {
                                                _armInfo.JobExist_Back = _arm.FORK_BACK_JOBEXIST == string.Empty ? eRobotJobStatus.UnKnown : (eRobotJobStatus)int.Parse(_arm.FORK_BACK_JOBEXIST);
                                                _armInfo.CstSeqNo_Back = _arm.FORK_BACK_CSTSEQ;
                                                _armInfo.JobSeqNo_Back = _arm.FORK_BACK_JOBSEQ;
                                                _armInfo.TrackingData_Back = _arm.FORK_BACK_TRACKINGVALUE;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        FrmOPIMessage.ShowMessage(this, _msgName, "", string.Format("Can't find Robot Name[{0}]", _robot.ROBOTNAME), MessageBoxIcon.Error);
                                        continue;
                                    }
                                }
                            }

                            #endregion

                            break;
                            #endregion

                        case "RobotCurrentModeReport":

                            #region RobotCurrentModeReport

                            RobotCurrentModeReport _robotCurrentModeReport = (RobotCurrentModeReport)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            //ROBOTMODE : SEMI,AUTO
                            if (G_OPIAp.Dic_Robot.ContainsKey(_robotCurrentModeReport.BODY.ROBOTNAME))
                            {
                                G_OPIAp.Dic_Robot[_robotCurrentModeReport.BODY.ROBOTNAME].RobotControlMode = _robotCurrentModeReport.BODY.ROBOTMODE;
                                G_OPIAp.Dic_Robot[_robotCurrentModeReport.BODY.ROBOTNAME].RobotStatus = _robotCurrentModeReport.BODY.ROBOTSTATUS == string.Empty ? eRobotStatus.UnKnown : (eRobotStatus)int.Parse(_robotCurrentModeReport.BODY.ROBOTSTATUS);

                                G_OPIAp.Dic_Robot[_robotCurrentModeReport.BODY.ROBOTNAME].HaveRobotCommand = _robotCurrentModeReport.BODY.HAVE_ROBOT_CMD =="1" ? true: false;
                                G_OPIAp.Dic_Robot[_robotCurrentModeReport.BODY.ROBOTNAME].CurrentPosition = _robotCurrentModeReport.BODY.CURRENT_POSITION;
                                G_OPIAp.Dic_Robot[_robotCurrentModeReport.BODY.ROBOTNAME].HoldStatus = _robotCurrentModeReport.BODY.HOLD_STATUS == "1" ? true : false;
                                G_OPIAp.Dic_Robot[_robotCurrentModeReport.BODY.ROBOTNAME].SampEQPFlag = _robotCurrentModeReport.BODY.SAMEEQFLAG == "Y" ? true : false;

                                foreach (RobotCurrentModeReport.ARMc _arm in _robotCurrentModeReport.BODY.ARMLIST)
                                {
                                    ArmInfo _armInfo = G_OPIAp.Dic_Robot[_robotCurrentModeReport.BODY.ROBOTNAME].LstArms.Where(r => r.ArmNo == _arm.ARMNO).First();

                                    _armInfo.ArmEnable = _arm.ARM_ENABLE == "Y"? true : false ;

                                    //Arm上面可放glass 最大數量,當max count = 1 時，只需要給front arm資料；max count = 2時fron arm & back arm都需要給資料
                                    _armInfo.JobExist_Front = _arm.FORK_FRONT_JOBEXIST == string.Empty ? eRobotJobStatus.UnKnown : (eRobotJobStatus)int.Parse(_arm.FORK_FRONT_JOBEXIST);
                                    _armInfo.CstSeqNo_Front = _arm.FORK_FRONT_CSTSEQ ;
                                    _armInfo.JobSeqNo_Front = _arm.FORK_FRONT_JOBSEQ;
                                    _armInfo.TrackingData_Front = _arm.FORK_FRONT_TRACKINGVALUE;

                                    if (G_OPIAp.Dic_Robot[_robotCurrentModeReport.BODY.ROBOTNAME].ArmMaxJobCount == 2)
                                    {
                                        _armInfo.JobExist_Back = _arm.FORK_BACK_JOBEXIST == string.Empty ? eRobotJobStatus.UnKnown : (eRobotJobStatus)int.Parse(_arm.FORK_BACK_JOBEXIST);
                                        _armInfo.CstSeqNo_Back = _arm.FORK_BACK_CSTSEQ;
                                        _armInfo.JobSeqNo_Back = _arm.FORK_BACK_JOBSEQ;
                                        _armInfo.TrackingData_Back = _arm.FORK_BACK_TRACKINGVALUE;
                                    }
                                }
                            }
                            else
                            {
                                FrmOPIMessage.ShowMessage(this, _msgName, "", string.Format("Can't find Robot Name[{0}]", _robotCurrentModeReport.BODY.ROBOTNAME), MessageBoxIcon.Error);
                                continue;
                            }

                            #endregion

                            break;
                            #endregion

                        case "RobotStageInfoRequestReply":

                            #region RobotStageInfoRequestReply

                            RobotStageInfoRequestReply _robotStageInfoRequestReply = (RobotStageInfoRequestReply)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            foreach (RobotStageInfoRequestReply.ROBOTc _rb in _robotStageInfoRequestReply.BODY.ROBOTLIST)
                            {
                                foreach (RobotStageInfoRequestReply.STAGEc _stage in _rb.STAGELIST)
                                {
                                    var _var = G_OPIAp.Lst_RobotStage.Where(r => r.RobotName.Equals(_rb.ROBOTNAME) && r.StageID.Equals(_stage.STAGEID));

                                    if (_var == null || _var.Count() <= 0)
                                    {
                                        FrmOPIMessage.ShowMessage(this, _msgName, "", string.Format("Can't find Stage ID[{0}]", _stage.STAGEID), MessageBoxIcon.Error);
                                        continue;
                                    }

                                    RobotStage _rbStage = _var.First();

                                    if (_rbStage != null)
                                    {
                                        _rbStage.StageStatus = (eRobotStageStatus)int.Parse(_stage.STAGESTATUS);

                                        _rbStage.Lst_JobData = new List<StageJobData>();

                                        foreach (RobotStageInfoRequestReply.JOBc _job in _stage.JOBLIST)
                                        {
                                            StageJobData _newData = new StageJobData();

                                            _newData.SlotNo = _job.SLOTNO.PadLeft(3, '0');
                                            _newData.JobExist = _job.JOBEXIST == string.Empty ? eRobotJobStatus.UnKnown : (eRobotJobStatus)int.Parse(_job.JOBEXIST);
                                            _newData.JobSeqNo = _job.JOBSEQNO;
                                            _newData.CstSeqNo = _job.CASSETTESEQNO;
                                            _newData.TrackingData = _job.TRACKINGVALUE;

                                            _rbStage.Lst_JobData.Add(_newData);
                                        }
                                    }
                                }
                            }

                            #endregion

                            break;
                            #endregion

                        case "RobotStageInfoReport":

                            #region RobotStageInfoReport

                            RobotStageInfoReport _robotStageInfoReport = (RobotStageInfoReport)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            foreach (RobotStageInfoReport.STAGEc _stage in _robotStageInfoReport.BODY.STAGELIST)
                            {

                                var _var = FormMainMDI.G_OPIAp.Lst_RobotStage.Where(r => r.RobotName.Equals(_stage.ROBOTNAME) && r.StageID.Equals(_stage.STAGEID));

                                if (_var == null || _var.Count() <= 0)
                                {
                                    FrmOPIMessage.ShowMessage(this, _msgName, "", string.Format("Can't find Stage ID[{0}]", _stage.STAGEID), MessageBoxIcon.Error);
                                    continue;
                                }

                                RobotStage _rbStage = _var.First();

                                if (_rbStage != null)
                                {
                                    _rbStage.StageStatus = (eRobotStageStatus)int.Parse(_stage.STAGESTATUS);

                                    _rbStage.Lst_JobData = new List<StageJobData>();

                                    foreach (RobotStageInfoReport.JOBc _job in _stage.JOBLIST)
                                    {
                                        StageJobData _newData = new StageJobData();
                                        _newData.SlotNo = _job.SLOTNO.PadLeft(2, '0');
                                        _newData.JobExist = _job.JOBEXIST == string.Empty ? eRobotJobStatus.UnKnown : (eRobotJobStatus)int.Parse(_job.JOBEXIST);
                                        _newData.JobSeqNo = _job.JOBSEQNO;
                                        _newData.CstSeqNo = _job.CASSETTESEQNO;
                                        _newData.TrackingData = _job.TRACKINGVALUE;

                                        _rbStage.Lst_JobData.Add(_newData);
                                    }
                                }
                            }
                            #endregion

                            break;
                            #endregion

                        case "RobotMessageReport":

                            #region RobotMessageReport
                            RobotMessageReport _robotMsgReport = (RobotMessageReport)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            _data = string.Format("[{0}] {1}^{2}", _robotMsgReport.BODY.MSG_DATETIME, _robotMsgReport.BODY.MSG_DETAIL, _robotMsgReport.BODY.MSG_TYPE);

                            lock (OPIInfo.Q_RobotMessage)
                            {
                                OPIInfo.Q_RobotMessage.Enqueue(_data);
                            }

                            NLogManager.Logger.LogInfoWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, string.Format("RobotMessageReport Enqueue [{0}]", _data));
                            #endregion

                            break;
                            #endregion

                        case "RobotCommandReport":

                            #region RobotCommandReport
                            RobotCommandReport _robotCommandReport = (RobotCommandReport)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                                                        
                            lock (OPIInfo.Lst_RobotCommand)
                            {
                                foreach (RobotCommandReport.COMMANDc _cmd in _robotCommandReport.BODY.COMMANDLIST)
                                {
                                    int.TryParse(_cmd.COMMAND_SEQ, out _num);

                                    if (OPIInfo.Lst_RobotCommand.Count < (_num)) continue;

                                    OPIInfo.Lst_RobotCommand[_num-1] = _cmd;
                                   
                                }
                            }

                            NLogManager.Logger.LogInfoWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, string.Format("RobotMessageReport Enqueue [{0}]", _data));

                            #endregion

                            break;
                            #endregion

                        case "ClientDisconnectRequest":

                            #region ClientDisconnectRequest

                            ClientDisconnectRequest _clientDisconnectReq = (ClientDisconnectRequest)Spec.CheckXMLFormat(_bcsXml);

                            if (G_OPIAp.CurLine.ServerName == _clientDisconnectReq.BODY.LINENAME && _clientDisconnectReq.RETURN.RETURNCODE.Equals(G_OPIAp.ReturnCodeSuccess)
                                && _clientDisconnectReq.BODY.USERGROUP.Equals(G_OPIAp.LoginGroupID) && _clientDisconnectReq.BODY.USERID.Equals(G_OPIAp.LoginUserID))
                            {
                                FormBase frmCurrent = CurForm;

                                #region 送出ClientDisconnectReply
                                ClientDisconnectReply _clientDisconnectReply = new ClientDisconnectReply();
                                _clientDisconnectReply.HEADER.REPLYSUBJECTNAME = G_OPIAp.SessionID;
                                _clientDisconnectReply.BODY.LINENAME = G_OPIAp.CurLine.ServerName;
                                _clientDisconnectReply.BODY.LOGINSERVERIP = _clientDisconnectReq.BODY.LOGINSERVERIP;
                                _xml = _clientDisconnectReply.WriteToXml();
                                SocketDriver.SendMessage(_clientDisconnectReply.HEADER.TRANSACTIONID, _clientDisconnectReply.HEADER.MESSAGENAME, _xml, out _err, G_OPIAp.SessionID);
                                #endregion

                                if (pnlMain.InvokeRequired)
                                {
                                    this.BeginInvoke(new MethodInvoker(
                                       delegate
                                       {
                                           IsRun = false;
                                           this.DialogResult = DialogResult.Abort;
                                           this.Close();

                                       }));
                                }
                                else
                                {
                                    IsRun = false;
                                    this.DialogResult = DialogResult.Abort;
                                    this.Close();
                                }


                                //Form frm = ((Control)frmCurrent).Parent.FindForm();

                                //if (frm is FormMainMDI)
                                //{
                                //    FormMainMDI frmMid = (FormMainMDI)frm;

                                //    frmMid.Invoke(new MethodInvoker(delegate
                                //    {
                                //        for (int i = Application.OpenForms.Count; i > 1; i--)//關閉除了MDI的Form
                                //        {
                                //            if (Application.OpenForms[i - 1].Name == frmMid.Name) continue;
                                //            Application.OpenForms[i - 1].Close();
                                //        }
                                //        frmMid.DialogResult = DialogResult.Abort;
                                //    }));
                                //}
                            }

                            break;
                            #endregion

                        case "CassetteMapDownloadResultReport":

                            #region CassetteMapDownloadResultReport

                            CassetteMapDownloadResultReport _cassetteMapDownloadResultReport = (CassetteMapDownloadResultReport)Spec.CheckXMLFormat(_bcsXml);

                            if (FrmCassetteOperation_Offline.Visible == true)
                            {
                                FrmCassetteOperation_Offline.Q_BCSReport.Enqueue(string.Format("{0}^{1}", "CassetteMapDownloadResultReport", _bcsXml));
                            }
                            else if (FrmCassetteOperation_Local.Visible == true)
                            {
                                FrmCassetteOperation_Local.Q_BCSReport.Enqueue(string.Format("{0}^{1}", "CassetteMapDownloadResultReport", _bcsXml));
                            }
                            break;
                            #endregion

                        case "CurrentChangerPlanRequestReply":

                            #region CurrentChangerPlanRequestReply

                            CurrentChangerPlanRequestReply _currentChangerPlanRequestReply = (CurrentChangerPlanRequestReply)Spec.CheckXMLFormat(_bcsXml);

                            #region Set CurrentChangerPlanRequestReply to LstChangerPlanDetail
                            G_OPIAp.CurLine.ChangerPlanID = _currentChangerPlanRequestReply.BODY.CHANGERPLAN.CURRENTPLANID;
                            G_OPIAp.CurLine.StandByChangerPlanID = _currentChangerPlanRequestReply.BODY.CHANGERPLAN.STANDBYPLANID;
                            G_OPIAp.CurLine.ChangerPlanStatus = (eChangerPlanStatus)int.Parse(_currentChangerPlanRequestReply.BODY.CHANGERPLAN.PLANSTATUS.ToString());

                            G_OPIAp.CurLine.LstChangerPlanDetail = new BindingList<PlanDetail>();
                            foreach (CurrentChangerPlanRequestReply.PRODUCTc _product in _currentChangerPlanRequestReply.BODY.CHANGERPLAN.PRODUCTLIST)
                            {
                                PlanDetail _detail = new PlanDetail();
                                _detail.SlotNo = _product.SLOTNO;
                                _detail.ProductName = _product.PRODUCTNAME;
                                _detail.SourceCSTID = _product.SOURCECSTID;
                                _detail.TargetCSTID = _product.TARGETCSTID;
                                _detail.HaveBeenUse = (_product.HAVEBEENUSE.ToUpper() == "TRUE" ? true : false);

                                G_OPIAp.CurLine.LstChangerPlanDetail.Add(_detail);
                            }

                            G_OPIAp.CurLine.LstStandByChangerPlanDetail = new BindingList<PlanDetail>();
                            foreach (CurrentChangerPlanRequestReply.PRODUCTc _product in _currentChangerPlanRequestReply.BODY.CHANGERPLAN.STANDBYPRODUCTLIST)
                            {
                                PlanDetail _detail = new PlanDetail();
                                _detail.SlotNo = _product.SLOTNO;
                                _detail.ProductName = _product.PRODUCTNAME;
                                _detail.SourceCSTID = _product.SOURCECSTID;
                                _detail.TargetCSTID = _product.TARGETCSTID;
                                _detail.HaveBeenUse = (_product.HAVEBEENUSE.ToUpper() == "TRUE" ? true : false);

                                G_OPIAp.CurLine.LstStandByChangerPlanDetail.Add(_detail);
                            }
                            #endregion

                            break;

                            #endregion

                        case "CurrentChangerPlanReport":

                            #region CurrentChangerPlanReport
                            CurrentChangerPlanReport _currentChangerPlanReport = (CurrentChangerPlanReport)Spec.CheckXMLFormat(_bcsXml);

                            #region Set _currentChangerPlanReport to LstChangerPlanDetail
                            G_OPIAp.CurLine.ChangerPlanID = _currentChangerPlanReport.BODY.CHANGERPLAN.CURRENTPLANID;
                            G_OPIAp.CurLine.StandByChangerPlanID = _currentChangerPlanReport.BODY.CHANGERPLAN.STANDBYPLANID;
                            G_OPIAp.CurLine.ChangerPlanStatus = (eChangerPlanStatus)int.Parse(_currentChangerPlanReport.BODY.CHANGERPLAN.PLANSTATUS.ToString());

                            G_OPIAp.CurLine.LstChangerPlanDetail = new BindingList<PlanDetail>();
                            foreach (CurrentChangerPlanReport.PRODUCTc _product in _currentChangerPlanReport.BODY.CHANGERPLAN.PRODUCTLIST)
                            {
                                PlanDetail _detail = new PlanDetail();
                                _detail.SlotNo = _product.SLOTNO;
                                _detail.ProductName = _product.PRODUCTNAME;
                                _detail.SourceCSTID = _product.SOURCECSTID;
                                _detail.TargetCSTID = _product.TARGETCSTID;
                                _detail.HaveBeenUse = (_product.HAVEBEENUSE.ToUpper() == "TRUE" ? true : false);

                                G_OPIAp.CurLine.LstChangerPlanDetail.Add(_detail);
                            }

                            G_OPIAp.CurLine.LstStandByChangerPlanDetail = new BindingList<PlanDetail>();
                            foreach (CurrentChangerPlanReport.PRODUCTc _product in _currentChangerPlanReport.BODY.CHANGERPLAN.STANDBYPRODUCTLIST)
                            {
                                PlanDetail _detail = new PlanDetail();
                                _detail.SlotNo = _product.SLOTNO;
                                _detail.ProductName = _product.PRODUCTNAME;
                                _detail.SourceCSTID = _product.SOURCECSTID;
                                _detail.TargetCSTID = _product.TARGETCSTID;
                                _detail.HaveBeenUse = (_product.HAVEBEENUSE.ToUpper() == "TRUE" ? true : false);

                                G_OPIAp.CurLine.LstStandByChangerPlanDetail.Add(_detail);
                            }
                            #endregion

                            break;
                            #endregion

                        //case "GlassGradeMappingChangeReportReply":

                        //    #region GlassGradeMappingChangeReportReply

                        //    GlassGradeMappingChangeReportReply _glassGradeMappingChangeReportReply = (GlassGradeMappingChangeReportReply)Spec.CheckXMLFormat(_bcsXml);

                        //    G_OPIAp.CurLine.BC_GlassGradeMappingChangeReportReply.GlassGrade_OK = _glassGradeMappingChangeReportReply.BODY.OKGLASSGRADE;
                        //    G_OPIAp.CurLine.BC_GlassGradeMappingChangeReportReply.GlassGrade_NG = _glassGradeMappingChangeReportReply.BODY.NGGLASSGRADE;

                        //    G_OPIAp.CurLine.BC_GlassGradeMappingChangeReportReply.IsReply = true;

                        //    break;
                        //    #endregion

                        case "RobotOperationModeReply":

                            #region RobotOperationModeReply
                            RobotOperationModeReply _robotOperationModeReply = (RobotOperationModeReply)Spec.CheckXMLFormat(_bcsXml);

                            G_OPIAp.CurLine.BC_RobotOperationModeReply.IsReply = true;

                            foreach (RobotOperationModeReply.ROBOTPOSITIONc _pos in _robotOperationModeReply.BODY.ROBOTPOSITIONLIST)
                            {
                                IndexerRobotStage _stage = FormMainMDI.G_OPIAp.CurLine.BC_RobotOperationModeReply.IndexerRobotStages.Find(s => s.RobotPosNo.Equals(_pos.ROBOTPOSITIONNO));

                                if (_stage == null) continue;

                                _stage.OperationMode = (eRobotOperationMode)(int.Parse(_pos.OPERATIONMODE));
                            }

                            break;

                            #endregion

                        case "EquipmentFetchGlassRuleReply":

                            #region EquipmentFetchGlassRuleReply
                            EquipmentFetchGlassRuleReply _equipmentFetchGlassRuleReply = (EquipmentFetchGlassRuleReply)Spec.CheckXMLFormat(_bcsXml);

                            G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.IsReply = true;

                            G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.RuleName_1 = _equipmentFetchGlassRuleReply.BODY.RULENAME1;
                            G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.RuleValue_1 = _equipmentFetchGlassRuleReply.BODY.RULEVALUE1;
                            G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.RuleName_2 = _equipmentFetchGlassRuleReply.BODY.RULENAME2;
                            G_OPIAp.CurLine.BC_EquipmentFetchGlassRuleReply.RuleValue_2 = _equipmentFetchGlassRuleReply.BODY.RULEVALUE2;
                            break;

                            #endregion

                        case "ProcessDataHistoryReply":

                            #region ProcessDataHistoryReply
                            ProcessDataHistoryReply _processDataHistoryReply = (ProcessDataHistoryReply)Spec.CheckXMLFormat(_bcsXml);

                            break;
                            #endregion

                        case "EachPositionReply":

                            #region EachPositionReply
                            EachPositionReply _eachPositionReply = (EachPositionReply)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            string _positionNodeNo = _eachPositionReply.BODY.EQUIPMENTNO;
                            string _positionUnitNo = _eachPositionReply.BODY.UNITNO.PadLeft(2, '0');

                            string _positionKey = string.Empty;
                            int _positionNo = 0;

                            if (G_OPIAp.Dic_Node.ContainsKey(_positionNodeNo))
                            {
                                if (G_OPIAp.Dic_Node[_positionNodeNo].Dic_Position.ContainsKey(_positionUnitNo))
                                {
                                    BCS_EachPositionReply _reply = G_OPIAp.Dic_Node[_positionNodeNo].Dic_Position[_positionUnitNo];

                                    _reply.IsReply = true;

                                    //REPORTPOSITIONNAME :  Y / N  (是否上報POSITION NAME- FOR SECS 機台, Y:表示POSITION NO內填寫的內容為POSITION NAME, N:表示填寫POSITION NO)
                                    _reply.ReportPositionName = _eachPositionReply.BODY.REPORTPOSITIONNAME == "Y" ? true : false;

                                    PositionInfo _positionInfo = null;

                                    foreach (EachPositionReply.POSITIONc _position in _eachPositionReply.BODY.POSITIONLIST)
                                    {
                                        _positionInfo = null;

                                        _positionKey = _position.POSITIONNO;

                                        #region List
                                        if (_reply.ReportPositionName)
                                        {
                                            _positionInfo = _reply.Lst_Position.Find(r => r.PositionName.Equals(_positionKey));
                                        }
                                        else
                                        {
                                            if (int.TryParse(_position.POSITIONNO, out _positionNo) == false) continue;

                                            _positionInfo = _reply.Lst_Position.Find(r => r.PositionNo.Equals(_positionNo));
                                        }


                                        if (_positionInfo != null)
                                        {
                                            _positionInfo.CassetteSeqNo = _position.CASSETTESEQNO;
                                            _positionInfo.JobSeqNo = _position.JOBSEQNO;
                                            _positionInfo.JobID = _position.JOBID;
                                        }
                                        #endregion
                                    }
                                }

                            }

                            #endregion

                            break;
                            #endregion

                        case "IonizerFanModeReportReply":

                            #region IonizerFanModeReportReply
                            IonizerFanModeReportReply _ionizerFanModeReportReply = (IonizerFanModeReportReply)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data

                            _key = _ionizerFanModeReportReply.BODY.EQUIPMENTNO;

                            if (!G_OPIAp.Dic_Node.ContainsKey(_key))
                            {
                                FrmOPIMessage.ShowMessage(this, _msgName, "", string.Format("Can't find Equipment No[{0}]", _key), MessageBoxIcon.Error);
                                continue;
                            }

                            G_OPIAp.Dic_Node[_key].BC_IonizerFanModeReportReply.IsReply = true;

                            G_OPIAp.Dic_Node[_key].BC_IonizerFanModeReportReply.EnableMode = _ionizerFanModeReportReply.BODY.ENABLEMODE1 + _ionizerFanModeReportReply.BODY.ENABLEMODE2;

                            #endregion

                            break;
                            #endregion

                        case "EquipmentDataLinkStatusReply":

                            #region EquipmentDataLinkStatusReply
                            EquipmentDataLinkStatusReply _equipmentDataLinkStatusReply = (EquipmentDataLinkStatusReply)Spec.CheckXMLFormat(_bcsXml);

                            G_OPIAp.BC_EquipmentDataLinkStatusReply.IsReply = true;

                            G_OPIAp.BC_EquipmentDataLinkStatusReply.BatonPassStatus_W = _equipmentDataLinkStatusReply.BODY.BATONPASSSTATUS;
                            G_OPIAp.BC_EquipmentDataLinkStatusReply.BatonPassInterruption_W = _equipmentDataLinkStatusReply.BODY.BATONPASSINTERRUPTION;
                            G_OPIAp.BC_EquipmentDataLinkStatusReply.DataLinkStop_W = _equipmentDataLinkStatusReply.BODY.DATALINKSTOP;
                            G_OPIAp.BC_EquipmentDataLinkStatusReply.StationLoopStatus_W = _equipmentDataLinkStatusReply.BODY.STATIONLOOPSTATUS;

                            G_OPIAp.BC_EquipmentDataLinkStatusReply.BatonPassStatus_B = _equipmentDataLinkStatusReply.BODY.BATONPASSEACHSTATION;
                            G_OPIAp.BC_EquipmentDataLinkStatusReply.CyclicTransmissionStatus_B = _equipmentDataLinkStatusReply.BODY.CYCLETRANSMISSIONSTATUS;

                            break;
                            #endregion

                        case "EquipmentDataLinkStatusReport":

                            #region EquipmentDataLinkStatusReport
                            EquipmentDataLinkStatusReport _equipmentDataLinkStatusReport = (EquipmentDataLinkStatusReport)Spec.CheckXMLFormat(_bcsXml);

                            G_OPIAp.BC_EquipmentDataLinkStatusReply.BatonPassStatus_W = _equipmentDataLinkStatusReport.BODY.BATONPASSSTATUS;
                            G_OPIAp.BC_EquipmentDataLinkStatusReply.BatonPassInterruption_W = _equipmentDataLinkStatusReport.BODY.BATONPASSINTERRUPTION;
                            G_OPIAp.BC_EquipmentDataLinkStatusReply.DataLinkStop_W = _equipmentDataLinkStatusReport.BODY.DATALINKSTOP;
                            G_OPIAp.BC_EquipmentDataLinkStatusReply.StationLoopStatus_W = _equipmentDataLinkStatusReport.BODY.STATIONLOOPSTATUS;

                            G_OPIAp.BC_EquipmentDataLinkStatusReply.BatonPassStatus_B = _equipmentDataLinkStatusReport.BODY.BATONPASSEACHSTATION;
                            G_OPIAp.BC_EquipmentDataLinkStatusReply.CyclicTransmissionStatus_B = _equipmentDataLinkStatusReport.BODY.CYCLETRANSMISSIONSTATUS;

                            break;
                            #endregion

                        case "SlotPositionReply":

                            #region SlotPositionReply
                            SlotPositionReply _slotPositionReply = (SlotPositionReply)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            string _slotNodeNo = _slotPositionReply.BODY.EQUIPMENTNO;
                            string _slotPortNo = _slotPositionReply.BODY.PORTNO;
                            int _slotPositionNo = 0;

                            _key = _slotNodeNo.PadRight(3, ' ') + _slotPortNo.PadRight(2, ' ');

                            if (G_OPIAp.Dic_Port.ContainsKey(_key))
                            {
                                foreach (SlotPositionReply.POSITIONc _slotPosition in _slotPositionReply.BODY.POSITIONLIST)
                                {
                                    #region List
                                    if (int.TryParse(_slotPosition.POSITIONNO, out _slotPositionNo) == false) continue;

                                    SlotPosition _slot = G_OPIAp.Dic_Port[_key].BC_SlotPositionReply.Lst_SlotPosition.Find(r => r.PositionNo.Equals(_slotPositionNo));

                                    if (_slot != null)
                                    {
                                        _slot.CassetteSeqNo = _slotPosition.CASSETTESEQNO;
                                        _slot.JobSeqNo = _slotPosition.JOBSEQNO;
                                        _slot.JobID = _slotPosition.JOBID;
                                        _slot.TrackingValue = _slotPosition.TRACKINGVALUE;
                                        _slot.SamplingSlotFlag = _slotPosition.SAMPLINGSLOTFLAG;
                                        _slot.RecipeName = _slotPosition.RECIPENAME;
                                        _slot.PPID = _slotPosition.PPID;
                                        _slot.EQPRTCFlag = _slotPosition.EQPRTCFLAG;
                                    }
                                    #endregion
                                }

                                G_OPIAp.Dic_Port[_key].BC_SlotPositionReply.IsReply = true;
                            }
                            #endregion

                            break;

                            #endregion

                        //case "RealTimeGlassCountReply":

                        //    #region RealTimeGlassCountReply
                        //    RealTimeGlassCountReply _realTimeGlassCountReply = (RealTimeGlassCountReply)Spec.CheckXMLFormat(_bcsXml);

                        //    #region Update Data
                        //    int _tftHasAssembluCnt = 0;
                        //    int _tftNotAssembluCnt = 0;
                        //    int _cfCnt = 0;
                        //    int _throughDummy = 0;
                        //    int _piThicknessDummy = 0;
                        //    int _uvMask = 0;

                        //    foreach (RealTimeGlassCountReply.EQUIPMENTc _eq in _realTimeGlassCountReply.BODY.EQUIPMENTLIST)
                        //    {
                        //        _key = _eq.EQUIPMENTNO;

                        //        if (!G_OPIAp.Dic_Node.ContainsKey(_key))
                        //        {
                        //            FrmOPIMessage.ShowMessage(this, _msgName, "", string.Format("Can't find EQUIPMENTNO[{0}]", _eq.EQUIPMENTNO), MessageBoxIcon.Error);
                        //            continue;
                        //        }

                        //        int.TryParse(_eq.ASSEMBLYTFTGLASSCNT, out _tftHasAssembluCnt);
                        //        int.TryParse(_eq.NOTASSEMBLYTFTGLASSCNT, out _tftNotAssembluCnt);
                        //        int.TryParse(_eq.CFGLASSCNT, out _cfCnt);
                        //        int.TryParse(_eq.THROUGHGLASSCNT, out _throughDummy);
                        //        int.TryParse(_eq.PIDUMMYGLASSCNT, out _piThicknessDummy);
                        //        int.TryParse(_eq.UVMASKGLASSCNT, out _uvMask);

                        //        if (G_OPIAp.CurLine.BC_RealTimeGlassCountReply.Dic_RealTimeGlassCount.ContainsKey(_key))
                        //        {
                        //            G_OPIAp.CurLine.BC_RealTimeGlassCountReply.Dic_RealTimeGlassCount[_key].AssemblyTFTCount = _tftHasAssembluCnt;
                        //            G_OPIAp.CurLine.BC_RealTimeGlassCountReply.Dic_RealTimeGlassCount[_key].NotAssemblyTFTCount = _tftNotAssembluCnt;
                        //            G_OPIAp.CurLine.BC_RealTimeGlassCountReply.Dic_RealTimeGlassCount[_key].CFGlassCount = _cfCnt;
                        //            G_OPIAp.CurLine.BC_RealTimeGlassCountReply.Dic_RealTimeGlassCount[_key].ThroughGlassCount = _throughDummy;
                        //            G_OPIAp.CurLine.BC_RealTimeGlassCountReply.Dic_RealTimeGlassCount[_key].PIThicknessDummy = _piThicknessDummy;
                        //            G_OPIAp.CurLine.BC_RealTimeGlassCountReply.Dic_RealTimeGlassCount[_key].UVMaskGlassCount = _uvMask;
                        //            G_OPIAp.CurLine.BC_RealTimeGlassCountReply.Dic_RealTimeGlassCount[_key].TotalCount = _tftHasAssembluCnt + _tftNotAssembluCnt + _cfCnt + _throughDummy + _piThicknessDummy + _uvMask;
                        //        }
                        //        else
                        //        {
                        //            RealTimeGlassCount _realCount = new RealTimeGlassCount();

                        //            _realCount.AssemblyTFTCount = _tftHasAssembluCnt;
                        //            _realCount.NotAssemblyTFTCount = _tftNotAssembluCnt;
                        //            _realCount.CFGlassCount = _cfCnt;
                        //            _realCount.ThroughGlassCount = _throughDummy;
                        //            _realCount.PIThicknessDummy = _piThicknessDummy;
                        //            _realCount.UVMaskGlassCount = _uvMask;
                        //            _realCount.TotalCount = _tftHasAssembluCnt + _tftNotAssembluCnt + _cfCnt + _throughDummy + _piThicknessDummy + _uvMask;

                        //            G_OPIAp.CurLine.BC_RealTimeGlassCountReply.Dic_RealTimeGlassCount.Add(_key, _realCount);
                        //        }
                        //    }
                        //    G_OPIAp.CurLine.BC_RealTimeGlassCountReply.IsReply = true;
                        //    #endregion

                        //    break;

                        //    #endregion

                        case "RecipeRegisterValidationReturnReport":

                            #region RecipeRegisterValidationReturnReport
                            RecipeRegisterValidationReturnReport _recipeRegisterValidationReturnReport = (RecipeRegisterValidationReturnReport)Spec.CheckXMLFormat(_bcsXml);

                            if (FrmCassetteOperation_Offline.Visible == true)
                            {
                                FrmCassetteOperation_Offline.Q_BCSReport.Enqueue(string.Format("{0}^{1}", "RecipeRegisterValidationReturnReport", _bcsXml));
                            }
                            else if (FrmCassetteOperation_Local.Visible == true)
                            {
                                FrmCassetteOperation_Local.Q_BCSReport.Enqueue(string.Format("{0}^{1}", "RecipeRegisterValidationReturnReport", _bcsXml));
                            }
                            //else if (FrmCassetteControl_Offline_DPI.Visible == true)
                            //{
                            //    FrmCassetteControl_Offline_DPI.Q_BCSReport.Enqueue(string.Format("{0}^{1}", "RecipeRegisterValidationReturnReport", _bcsXml));
                            //}
                            //else if (FrmCassetteControl_Offline_PRM.Visible == true)
                            //{
                            //    FrmCassetteControl_Offline_PRM.Q_BCSReport.Enqueue(string.Format("{0}^{1}", "RecipeRegisterValidationReturnReport", _bcsXml));
                            //}

                            if (FrmRecipeRegister.Q_BCSReport != null)
                            {
                                FrmRecipeRegister.Q_BCSReport.Enqueue(_bcsXml);
                            }

                            break;
                            #endregion

                        case "RecipeParameterReturnReport":

                            #region RecipeParameterReturnReport
                            //RecipeParameterReturnReport _recipeParameterReturnReport = (RecipeParameterReturnReport)Spec.CheckXMLFormat(_bcsXml);

                            if (FrmRecipeParam.Q_BCSReport != null)
                            {
                                FrmRecipeParam.Q_BCSReport.Enqueue(_bcsXml);
                            }

                            break;
                            #endregion

                        //case "OperationPermissionResultReport":

                        //    #region OperationPermissionResultReport
                        //    OperationPermissionResultReport _operationPermissionResultReport = (OperationPermissionResultReport)Spec.CheckXMLFormat(_bcsXml);

                        //    //if (FrmRunModeControl_ATS.Q_BCSReport != null)
                        //    //{
                        //    //    FrmRunModeControl_ATS.Q_BCSReport.Enqueue(string.Format("{0}^{1}", "OperationPermissionResultReport", _bcsXml));
                        //    //}

                        //    break;
                        //    #endregion

                        //case "OperationRunModeChangeResultReport":

                        //    #region OperationRunModeChangeResultReport
                        //    OperationRunModeChangeResultReport _operationRunModeChangeResultReport = (OperationRunModeChangeResultReport)Spec.CheckXMLFormat(_bcsXml);


                        //    //if (FrmRunModeControl_ATS.Q_BCSReport != null)
                        //    //{
                        //    //    FrmRunModeControl_ATS.Q_BCSReport.Enqueue(string.Format("{0}^{1}", "OperationRunModeChangeResultReport", _bcsXml));
                        //    //}

                        //    break;
                        //    #endregion

                        case "RobotUnloaderDispatchRuleReply":

                            #region RobotUnloaderDispatchRuleReply

                            RobotUnloaderDispatchRuleReply _robotUnloaderDispatchRuleReply = (RobotUnloaderDispatchRuleReply)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            foreach (RobotUnloaderDispatchRuleReply.PORTc _port in _robotUnloaderDispatchRuleReply.BODY.PORTLIST)
                            {
                                _key = _port.EQUIPMENTNO.PadRight(3, ' ') + _port.PORTNO.PadRight(2, ' ');

                                if (!G_OPIAp.Dic_Port.ContainsKey(_key))
                                {
                                    FrmOPIMessage.ShowMessage(this, _msgName, "", string.Format("Can't find Equipement No[{0}],Port No [{1}]", _port.EQUIPMENTNO, _port.PORTNO), MessageBoxIcon.Error);
                                    continue;
                                }

                                G_OPIAp.Dic_Port[_key].RobotUnloaderDispatch.Grade01 = _port.GRADE_1;
                                G_OPIAp.Dic_Port[_key].RobotUnloaderDispatch.Grade02 = _port.GRADE_2;
                                G_OPIAp.Dic_Port[_key].RobotUnloaderDispatch.Grade03 = _port.GRADE_3;
                                //G_OPIAp.Dic_Port[_key].RobotUnloaderDispatch.Grade04 = _port.GRADE_4;
                                //G_OPIAp.Dic_Port[_key].RobotUnloaderDispatch.AbnormalCode01 = _port.ABNORMALCODE_1;
                                //G_OPIAp.Dic_Port[_key].RobotUnloaderDispatch.AbnormalCode02 = _port.ABNORMALCODE_2;
                                //G_OPIAp.Dic_Port[_key].RobotUnloaderDispatch.AbnormalCode03 = _port.ABNORMALCODE_3;
                                //G_OPIAp.Dic_Port[_key].RobotUnloaderDispatch.AbnormalCode04 = _port.ABNORMALCODE_4;
                                //G_OPIAp.Dic_Port[_key].RobotUnloaderDispatch.AbnormalFlag = _port.ABNORMALFLAG;
                                G_OPIAp.Dic_Port[_key].RobotUnloaderDispatch.OperatorID = _port.OPERATORID;
                            }
                            #endregion

                            break;
                            #endregion

                        case "RobotUnloaderDispatchRuleReport":

                            #region RobotUnloaderDispatchRuleReport

                            RobotUnloaderDispatchRuleReport _robotUnloaderDispatchRuleReport = (RobotUnloaderDispatchRuleReport)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            foreach (RobotUnloaderDispatchRuleReport.PORTc _port in _robotUnloaderDispatchRuleReport.BODY.PORTLIST)
                            {
                                _key = _port.EQUIPMENTNO.PadRight(3, ' ') + _port.PORTNO.PadRight(2, ' ');

                                if (!G_OPIAp.Dic_Port.ContainsKey(_key))
                                {
                                    FrmOPIMessage.ShowMessage(this, _msgName, "", string.Format("Can't find Equipement No[{0}],Port No [{1}]", _port.EQUIPMENTNO, _port.PORTNO), MessageBoxIcon.Error);
                                    continue;
                                }

                                G_OPIAp.Dic_Port[_key].RobotUnloaderDispatch.Grade01 = _port.GRADE_1;
                                G_OPIAp.Dic_Port[_key].RobotUnloaderDispatch.Grade02 = _port.GRADE_2;
                                G_OPIAp.Dic_Port[_key].RobotUnloaderDispatch.Grade03 = _port.GRADE_3;
                                //G_OPIAp.Dic_Port[_key].RobotUnloaderDispatch.Grade04 = _port.GRADE_4;
                                //G_OPIAp.Dic_Port[_key].RobotUnloaderDispatch.AbnormalCode01 = _port.ABNORMALCODE_1;
                                //G_OPIAp.Dic_Port[_key].RobotUnloaderDispatch.AbnormalCode02 = _port.ABNORMALCODE_2;
                                //G_OPIAp.Dic_Port[_key].RobotUnloaderDispatch.AbnormalCode03 = _port.ABNORMALCODE_3;
                                //G_OPIAp.Dic_Port[_key].RobotUnloaderDispatch.AbnormalCode04 = _port.ABNORMALCODE_4;
                                //G_OPIAp.Dic_Port[_key].RobotUnloaderDispatch.AbnormalFlag = _port.ABNORMALFLAG;
                                G_OPIAp.Dic_Port[_key].RobotUnloaderDispatch.OperatorID = _port.OPERATORID;
                            }
                            #endregion

                            break;
                            #endregion

                        case "ProductTypeInfoRequestReply":

                            #region ProductTypeInfoRequestReply

                            ProductTypeInfoRequestReply _productTypeInfoRequestReply = (ProductTypeInfoRequestReply)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            Dictionary<string, ProductTypeInfo> _dicProduct = G_OPIAp.CurLine.BC_ProductTypeInfoRequestReply.Dic_ProductTypeInfo;

                            foreach (ProductTypeInfoRequestReply.EQUIPMENTc _eq in _productTypeInfoRequestReply.BODY.EQUIPMENTLIST)
                            {
                                if (_dicProduct.ContainsKey(_eq.EQUIPMENTNO))
                                {
                                    _dicProduct[_eq.EQUIPMENTNO].ProductType = _eq.PRODUCTTYPE;
                                    _dicProduct[_eq.EQUIPMENTNO].ProductType_Unit01 = _eq.UNIT01_PRODUCTTYPE;
                                    _dicProduct[_eq.EQUIPMENTNO].ProductType_Unit02 = _eq.UNIT02_PRODUCTTYPE;
                                    _dicProduct[_eq.EQUIPMENTNO].ProductType_Unit03 = _eq.UNIT03_PRODUCTTYPE;
                                    _dicProduct[_eq.EQUIPMENTNO].ProductType_Unit04 = _eq.UNIT04_PRODUCTTYPE;
                                    _dicProduct[_eq.EQUIPMENTNO].ProductType_Unit05 = _eq.UNIT05_PRODUCTTYPE;
                                    _dicProduct[_eq.EQUIPMENTNO].ProductType_Unit06 = _eq.UNIT06_PRODUCTTYPE;
                                    _dicProduct[_eq.EQUIPMENTNO].ProductType_Unit07 = _eq.UNIT07_PRODUCTTYPE;
                                    _dicProduct[_eq.EQUIPMENTNO].ProductType_Unit08 = _eq.UNIT08_PRODUCTTYPE;
                                }
                                else
                                {
                                    ProductTypeInfo _prod = new ProductTypeInfo();
                                    _prod.NodeNo = _eq.EQUIPMENTNO;
                                    _prod.ProductType = _eq.PRODUCTTYPE;
                                    _prod.ProductType_Unit01 = _eq.UNIT01_PRODUCTTYPE;
                                    _prod.ProductType_Unit02 = _eq.UNIT02_PRODUCTTYPE;
                                    _prod.ProductType_Unit03 = _eq.UNIT03_PRODUCTTYPE;
                                    _prod.ProductType_Unit04 = _eq.UNIT04_PRODUCTTYPE;
                                    _prod.ProductType_Unit05 = _eq.UNIT05_PRODUCTTYPE;
                                    _prod.ProductType_Unit06 = _eq.UNIT06_PRODUCTTYPE;
                                    _prod.ProductType_Unit07 = _eq.UNIT07_PRODUCTTYPE;
                                    _prod.ProductType_Unit08 = _eq.UNIT08_PRODUCTTYPE;

                                    _dicProduct.Add(_eq.EQUIPMENTNO, _prod);
                                }
                            }
                            G_OPIAp.CurLine.BC_ProductTypeInfoRequestReply.IsReply = true;
                            #endregion

                            break;
                            #endregion

                        case "OPIFlashMessage":

                            #region OPIFlashMessage
                            OPIFlashMessage _opiFlashMessage = (OPIFlashMessage)Spec.CheckXMLFormat(_bcsXml);

                            #region Start Flash
                            if (pnlMain.InvokeRequired)
                            {
                                this.BeginInvoke(new MethodInvoker(
                                   delegate
                                   {
                                       FLASHWINFO fi = Create_FLASHWINFO(this.Handle, FLASHW_TRAY, uint.MaxValue, 0);
                                       FlashWindowEx(ref fi);
                                   }));
                            }
                            else
                            {
                                FLASHWINFO fi = Create_FLASHWINFO(this.Handle, FLASHW_TRAY, uint.MaxValue, 0);
                                FlashWindowEx(ref fi);
                            }
                            #endregion

                            FrmOPIMessage.ShowMessage(this, "OPIFlashMessage", "", _opiFlashMessage.BODY.MESSAGE, MessageBoxIcon.Information);

                            IsFlash = true;

                            break;
                            #endregion

                        case "InspectionFlowPriorityInfoReply":

                            #region InspectionFlowPriorityInfoReply

                            InspectionFlowPriorityInfoReply _inspectionFlowPriorityInfoReply = (InspectionFlowPriorityInfoReply)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            _key = _inspectionFlowPriorityInfoReply.BODY.EQUIPMENTNO.PadRight(3, ' ') + _inspectionFlowPriorityInfoReply.BODY.PORTNO.PadRight(2, ' ');

                            if (G_OPIAp.Dic_Port.ContainsKey(_key))
                            {
                                G_OPIAp.Dic_Port[_key].FlowPriority = _inspectionFlowPriorityInfoReply.BODY.PRIORITY;
                            }
                            #endregion

                            break;
                            #endregion

                        case "InspectionFlowPriorityInfoReport":

                            #region InspectionFlowPriorityInfoReport

                            InspectionFlowPriorityInfoReport _inspectionFlowPriorityInfoReport = (InspectionFlowPriorityInfoReport)Spec.CheckXMLFormat(_bcsXml);

                            #region Update Data
                            _key = _inspectionFlowPriorityInfoReport.BODY.EQUIPMENTNO.PadRight(3, ' ') + _inspectionFlowPriorityInfoReport.BODY.PORTNO.PadRight(2, ' ');

                            if (G_OPIAp.Dic_Port.ContainsKey(_key))
                            {
                                G_OPIAp.Dic_Port[_key].FlowPriority = _inspectionFlowPriorityInfoReport.BODY.PRIORITY;
                            }
                            #endregion

                            break;
                            #endregion

                        default: break;
                    }
                    #endregion

                }
                catch (Exception ex)
                {
                    NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                    FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
                }
                finally
                {
                    if (_msg == null) Thread.Sleep(100);
                }
            }
        }

        #endregion

        //讓WinForm Pop時不閃爍。 e
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x02000000;  // Turn on WS_EX_COMPOSITED (等同 cp.ExStyle = cp.ExStyle | 0x02000000)
                return cp;
            }
        }

        private bool PreLoadSubForm(ref Form frm, string frmCaption = "")
        {
            try
            {
                if (frm is FormBase)
                {

                    FormBase _frm = frm as FormBase;
                    _frm.MdiParent = this;
                    _frm.Visible = false;
                    _frm.Dock = DockStyle.Fill;
                    if (!string.IsNullOrWhiteSpace(frmCaption)) _frm.lblCaption.Text = frmCaption;
                    _frm.FormBorderStyle = FormBorderStyle.None;

                    pnlMain.Controls.Add(_frm);
                }
                else
                {
                    Form _frm = frm as Form;
                    _frm.MdiParent = this;
                    _frm.Visible = false;
                    _frm.Dock = DockStyle.Fill;
                    _frm.FormBorderStyle = FormBorderStyle.None;

                    pnlMain.Controls.Add(_frm);
                }

                return true;
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);

                return false;
            }
        }

        private void ChangeLanguage(string LanguageName)
        {
            try
            {
                string uiREX = string.Format("UniOPI.Properties.{0}", LanguageName);

                UniLanguage.CurResourceManager = new System.Resources.ResourceManager(uiREX, Assembly.GetExecutingAssembly());
                UniLanguage.ApplyUICulture(this, UniLanguage.CurLanguage);
            }
            catch (Exception ex)
            {
                NLogManager.Logger.LogErrorWrite(this.GetType().Name, MethodBase.GetCurrentMethod().Name, ex);
                FrmOPIMessage.ShowMessage(this, MethodBase.GetCurrentMethod().Name, ex, MessageBoxIcon.Error);
            }
        }
    }
}
