using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    public enum eBitResult
    {
        OFF = 0,
        ON = 1,
        Unknown=2  //yang 20161204 不要删除,某些情况初始赋值不可以为normal bitresult(用于判断,不可以真的写给PLC)
    }

    public enum eFabType
    {
        Unknown = 0,
        ARRAY = 1,
        CF = 2,
        CELL = 3,
        MODULE = 4
    }

    public enum eCIMModeCmd
    {
        UNKNOWN = 0,
        CIM_ON = 1,
        CIM_OFF = 2
    }

    public enum eEQPStatus
    {
        NOUNIT = 0,
        SETUP = 1,
        STOP = 2,
        PAUSE = 3,
        IDLE = 4,
        RUN = 5
    }

    public enum ePortStatus
    {
        /// <summary>
        /// Unknown
        /// </summary>
        UN = 0,
        /// <summary>
        /// Load Request
        /// </summary>
        LR = 1,
        /// <summary>
        /// Load Complete
        /// </summary>
        LC = 2,
        /// <summary>
        /// Unload Request
        /// </summary>
        UR = 3,
        /// <summary>
        /// Unload Complete
        /// </summary>
        UC = 4
    }

    public enum eCassetteStatus
    {
        UNKNOWN = 0,
        NO_CASSETTE_EXIST = 1,
        WAITING_FOR_CASSETTE_DATA = 2,
        WAITING_FOR_START_COMMAND = 3,
        WAITING_FOR_PROCESSING = 4,
        IN_PROCESSING = 5,
        PROCESS_PAUSED = 6,
        PROCESS_COMPLETED = 7,
        CASSETTE_REMAP = 8,
        IN_ABORTING = 9
    }

    public enum eCompletedCassetteData
    {
        Unknown = 0,
        NormalComplete = 1,
        OperatorForcedToCancel = 2,
        OperatorForcedToAbort = 3,
        BCForcedToCancel = 4,
        BCForcedToAbort = 5,
        EQAutoCancel = 6,
        EQAutoAbort = 7
    }

    public enum eCompleteCassetteReason
    {
        Normal = 0,
        OnPortQTimeOverCancel = 1,
        StoreQTimeOverAbort = 2,
        NextGlassPortModeMismatch = 3,
        NextGlassGradeMismatch = 4,
        ENGModeComplete = 5,
        GlassQTimeOverAbort = 6,
        NextGlassProductTypeMismatch = 7,
        NextGlassProductIDMismatch = 8,
        NextGlassGroupIndexMismatch = 9
    }

    public enum eLoadingCassetteType
    {
        ActualCassette = 1,
        EmptyCassette = 2
    }

    public enum eReportMode
    {
        PLC = 1,
        RS232 = 2,
        RS485 = 3,
        HSMS_PLC = 4,
        PLC_HSMS = 5,
        HSMS_NIKON = 6,
        HSMS_CSOT = 7
    }

    public enum eEQPOperationMode
    {
        Unknown = 0,
        MANUAL = 1,
        AUTO = 2,
        SEMIAUTO = 3
    }

    public enum eHostMode
    {
        OFFLINE = 0,
        REMOTE  = 2,
        LOCAL = 1
    }

    public enum ePLAN_STATUS
    {
        NO_PLAN = 0,
        START = 1,
        END = 2,
        READY = 3,
        REQUEST = 4,
        CANCEL = 5,
        ABORTING = 6,
        ABORT = 7,
        WAITING = 8
    }

    public enum eReturnCode1
    {
        Unknown = 0,
        OK = 1,
        NG = 2
    }

    public enum eReturnCode2
    {
        Unknown = 0,
        Accept = 1,
        AlreadyInDesiredStatus = 2,
        NG = 3
    }

    public enum eReturnCode3
    {
        Unknown = 0,
        Accept = 1,
        NotAcceppt = 2
    }
    public enum eReturnCode4  //Add By Yangzhenteng 20180420
    {
        Accept = 0,
        NotAcceppt = 1,
        Timeout = 2
    }
    public enum eLoadingCstType
    {
        Unknown = 0,
        Actual = 1,
        Empty = 2
    }

    public enum eQTime
    {
        Unknown = 0,
        NormalUnloading = 1,
        QTimeOver_Unloading = 2
    }

    public enum eCFQTime
    {
        OK = 1,
        NG = 2,
        RW = 3
    }

    public enum eParitalFull
    {
        Unknown = 0,
        PartialFull = 1,
        NoPartialFull = 2
    }


    /// <summary>
    ///  POL Line
    /// </summary>
    public enum eBACV_ByPass
    {
        Unknown = 0,
        NormalUnloading = 1,
        BACV_ByPass_Unloading = 2
    }

    public enum eDistortion
    {
        Unknown = 0,
        NotDistortion = 1,
        Distortion = 2
    }

    public enum eDirection
    {
        Unknown = 0,
        Normal = 1,
        Reverse = 2
    }

    public enum eGlassExist
    {
        Unknown = 0,
        NoExist = 1,
        Exist = 2
    }

    public enum ePortType
    {
        Unknown = 0,
        LoadingPort = 1,
        UnloadingPort = 2,
        BothPort = 3,
        BufferPort_BufferType = 4,
        BufferPort_LoaderInBufferType = 5,
        BufferPort_UnloaderInBufferType = 6
    }

    public enum ePortMode
    {
        // ITO QC CA ST RP RJ SK Y L K C : MES有這些資料, 但PLC對不上或沒用到
        Unknown = 0,
        TFT = 1,                /*TFT*/
        CF = 2,                 /*CF*/
        Dummy = 3,              /*DM*/
        MQC = 4,                /*MQC*/
        HT = 5,
        LT = 6,
        ENG = 7,
        IGZO = 8,
        ILC = 9,
        FLC = 10,
        ThroughDummy = 11,         /*TR*/
        ThicknessDummy = 12,        /*TK*/
        UVMask = 13,
        ByGrade = 14,
        OK = 15,                /*OK*/
        NG = 16,                /*NG*/
        MIX = 17,               /*MIX*/
        EMPMode = 18,           /*EMP*/
        Rework = 19,             /*RW*/
        Mismatch = 20,   
        PD = 21,
		IR = 22,
		RP = 23,
        ReJudge = 24,
        ByFlag = 25,
        MixFlag = 26,
    }

    //Watson Add 20141013 For CELL Port Oper Mode = Packing Unpacking Mode
    public enum ePortOperMode
    {
        Unknown = 0,
        PACK = 1,                /*Packing*/
        UNPACK = 2                 /*Un-Packing*/
    }

    //Watson Add 20141021 For CELL Unload Dispatch Rule
    public enum eCELL_AbnorFlagCheckRule
    {
        Empty = 1,
        NotEmpty = 2,
        Value = 3,
        NotCare = 4
    }

//Watson Add 20141128 For CELL TCV Dispatching 
//1: Glass received from LD/ULD-1 will be sent to NRP; glass received from NRP will be sent to LD/ULD-1.
//2: Glass receives from BCN will be sent to LD/ULD-1.
//3: Glass receives from BCN will be sent to OCV."
    public enum eCELL_TCVDispatchRule
    {
        NRP = 1,
        CUT = 2,
        POL = 3
    }

    public enum eCELL_TCVSamplingMode
    {
        Unknown = 0,
        FullMode = 1,
        PullMode = 2,
        PassMode = 3
    }

    public enum ePortTransferMode
    {
        Unknown = 0,
        MGV = 1,
        AGV = 2,
        StockerInline = 3
    }

    public enum ePortEnableMode
    {
        Unknown = 0,
        Enabled = 1,
        Disabled = 2
    }

    public enum eCstControlCmd
    {
        None = 0,
        ProcessStart = 1,
        ProcessStartByCount = 2,
        ProcessPause = 3,
        ProcessResume = 4,
        ProcessAbort = 5,
        ProcessCancel = 6,
        Reload = 7,
        Load = 8,
        ReMap = 9,
        ProcessEnd = 10,
        MapDownload = 11,
        ProcessAborting = 12,
        Unload = 13,
        DoubleRun = 14
    }

    public enum eCstCmdRetCode
    {
        Unknown = 0,
        COMMAND_OK = 1,
        COMMAND_ERROR = 2,
        CSTID_IS_INVALID = 3,
        RECIPE_MISMATCH = 4,
        SLOT_INFORMATION_MISMATCH = 5,
        JOB_TYPE_MISMATCH = 6,
        CST_SETTING_CODE_MISMATCH = 7,
        ALREADY_RECEIVED = 8,
        OTHER_ERROR = 9,
        PRODUCT_TYPE_MISMATCH = 10,
        GROUP_INDEX_MISMATCH = 11,
        PRODUCT_ID_MISMATCH = 12
    }

    public enum eOPISendType
    {
        Local = 0,
        All = 1,
        Appoint = 2
    }

    public enum eSubstrateType
    {
        Glass = 0,
        Chip = 1,
        /// <summary>
        /// POL , CST Cleaner
        /// </summary>
        Cassette = 2,
        Block = 3
    }

    public enum eEDCReportTo
    {
        Unknown = 0,
        MES = 1,
        EDA = 2,
        BOTH = 3
    }

    public enum ePortDown
    {
        Down = 0,
        Normal = 1
    }

    public enum eSamplingRule
    {
        ByCount = 1,
        ByUnit = 2,
        BySlot = 3,
        ByID = 4,
        FullInspection = 5,
        InspectionSkip = 6,
        NormalInspection = 7
    }

    public enum eSideUnit
    {
        CoaterVCD01 = 0,
        CoaterVCD02 = 1,
        ExposureCP01 = 2,
        ExposureCP02 = 3
    }
    public enum eMaterialEQtype
    {
        Normal = 0,
        MaskEQ = 1
    }

    //Add by marine for MES 2015/7/13
    public enum eMaterialMode
    { 
        NORMAL = 1,
        ABNORMAL =2,
        NONE = 3
    }

    public enum eMaterialStatus
    {
        MOUNT = 1,
        DISMOUNT = 2,
        INUSE = 3,
        PREPARE = 4,
        NONE = 5
    }

    public enum eMaterialType_SDP
    {
        AU = 1,
        GF = 2
    }

    public enum eMaterialType_LCD
    {
        TANK = 1,
        HEAD = 2
    }

    public enum eCompleteStatus
    {
        NORMALEND = 1,
        ABNORMAL = 2
    }
    //目前EQP IO Report "1：Kind To Kind mode  2：Cassette To Cassette mode 3：Lot To Lot Mode"
    //但是JOB DATA IO is " 0: Kind to Kind   1: CST to CST 2：Lot To Lot"
    public enum eCSTOperationMode
    {
        KTOK = 0,
        CTOC = 1,
        LTOL = 2
    }

    public enum eEnableDisable
    {
        Enable = 1,
        Disable = 0
    }
    //Add for T3 MES by marine 2015/8/19//sy modify 21060118
    public enum eBoxType
    {
        InBox = 1,
        OutBox = 2,
        NODE = 0
    }

    public enum eEQPMode
    {
        NORN = 0,
        PASS = 1,
        APAS = 2,
        CVD2S = 3,
        CVD2D = 4,
        CVD4P1 = 5,
        CVD4P2 = 6,
        CVD2O = 7,
        CVD2Q = 8,
        CVD4Q = 9,
        DRYENG = 10,
        DRYMQC = 11,
        DRYIGZO = 12,
        DRYA = 13,
        DRYB = 14,
        MIX = 15
    }

    public enum eRobotOperationMode
    {
        Unknown = 0,
        NormalMode = 1,
        DualMode = 2,
        SingleMode = 3
    }

    public enum eRobotOperationAction
    {
        Unknown = 0,
        Received = 1,
        Sent = 2,
        Both = 3
    }

    public enum eProportionalRuleName
    {
        Unknown = 0,
        Normal = 1,
        IGZO = 2,
        MQC = 3,
        ENG = 4,
        Reserved_A = 5,
        Reserved_B = 6,
        HT = 7,
        LT = 8
    }

    public enum eINDEXER_OPERATION_MODE
    {
         // modify by bruce 2015/7/3 update item list
        UNKNOWN = 0,
        SAMPLING_MODE = 1,
        SORTER_MODE = 2,
        CHANGER_MODE = 3,
        COOL_RUN_MODE = 4,
        FORCE_CLEAN_OUT_MODE = 5,
        ABNORMAL_FORCE_CLEAN_OUT_MODE = 6,
        MQC_MODE = 7,
        THROUGH_MODE = 8,
        FIX_MODE = 9,
        RANDOM_MODE = 10,
        NORMAL_MODE=11,
        MIX_MODE = 12
    }
    public enum eMESTraceLevel
    {
        M = 1, //‘M’ – Machine
        U = 2, //‘U’ – Unit
        P = 3  //‘P’ – Port
    }

    public enum eJobType
    {
        Unknown = 0,
        /// <summary>
        /// Normal TFT Product
        /// </summary>
        TFT = 1,
        /// <summary>
        /// Normal CF Product
        /// </summary>
        CF = 2,
        /// <summary>
        /// General Dummy
        /// </summary>
        DM = 3,
        /// <summary>
        /// Through Dummy
        /// </summary>
        TR = 4,
        /// <summary>
        /// Thickness Dummy
        /// </summary>
        TK = 5,
        /// <summary>
        /// UV Mask
        /// </summary>
        UV = 6,
        /// <summary>
        /// METAL1_DUMMY//sy add by MES 1.21 20161119
        /// </summary>
        METAL1 = 7,
        /// <summary>
        /// ITO_DUMMY//sy add by MES 1.21 20151119
        /// </summary>
        ITO = 8,
        /// <summary>
        /// NIP_DUMMY//sy add by MES 1.21 20151119
        /// </summary>
        NIP = 9
    }

    public enum eOPISubCstState
    {
        NONE = 0,
        WACSTEDIT = 1,
        WAREMAPEDIT = 2,
        WASTART = 3
    }

    public enum eJobEvent
    {
        SendOut = 0, Receive = 1, Store = 2, FetchOut = 3, Remove = 4, 
        Create = 5, Delete = 6, Recovery = 7, Hold = 8, OXUpdate = 9, 
        Delete_CST_Complete = 10, EQP_NEW = 11, Edit = 12, CUT_CREATE_CHIP = 13, 
        Assembly = 14, VCR_Report = 15, VCR_Mismatch = 16, VCR_Mismatch_Copy = 17, Assembly_NG = 18,
        Borrowed = 19 //Edit By Yangzhenteng 20190812
    }

    
    //1：VCR Reading OK & Match With Job Data Glass ID
    //2：VCR Reading OK & Miss Match With Job Data Glass ID
    //3：VCR Reading Fail & Key In & Match With Job Data Glass ID
    //4：VCR Reading Fail & Key In & Miss Match With Job Data Glass ID
    //5：VCR Reading Fail & Pass"
    public enum eVCR_EVENT_RESULT
    {
        NOUSE = 0,
        READING_OK_MATCH_JOB = 1,
        READING_OK_MISMATCH_JOB = 2,
        READING_FAIL_KEY_IN_MATCH_JOB = 3,
        READING_FAIL_KEY_IN_MISMATCH_JOB = 4,
        READING_FAIL_PASS = 5,
        //---Add tom.bian for t3 module
        READING_OK_MISMACHE_EXCHANGEJOBDATA=6,
        READING_FAIL_KEY_IN_MISMATCH_EXCHANGEJOBDATA=7,
        PANEL_OUT_OF_CLASS=8,
        EQP_REPORT_ERROR_CANT_FINDJOB=9
    }
    /*0：Not Use
       1：Job Remove
       2：Job Recovery*/
    public enum eJobCommand
    {
        NOUSE = 0,
        JOBREMOVE = 1,
        JOBRECOVERY = 2
    }

    /*- 0：Not Cutting
      - 1：OLS Cutting OK 
      - 2：LSC Cutting OK
      - 3：Cutting NG
    */
    public enum eCUTTING_FLAG
    {
        NOT_CUTTING = 0,
        OLS_CUTTING_OK = 1,
        LSC_CUTTING_OK = 2,
        CUTTING_NG = 3
    }

    /*- 1：OK, To Short Cut Equipment
      - 2：OK, To Unloader CST
      - 3：NG
    */
    public enum eGlassOutResult
    {
        OK_ToShortCut = 1,
        OK_ToUnloader = 2,
        NG = 3
    }

    public enum eShortCutMode
    {
        Enable = 1,
        Disable = 2
    }

    public enum eUPKEquipmentRunMode
    {
        TFT = 1,
        CF = 2,
        //T3 Add for UPK Unloader 2015/8/21 by Frank 
        Normal = 3,
        Re_Clean =4 
    }

    public enum ePermitFlag
    {
        Y = 1,
        N = 2,
        M = 3,
        F = 4
    }

    public enum eWaitCassetteStatus
    {
        UNKNOWN = 0,
        NotWaitCassette = 1,
        W_CST = 2
    }

    //Jun Modify 20141229 修改定義名稱
    public enum ePalletMode
    {
        UNKNOWN = 0,
        PACK = 1,                /*Packing*/
        UNPACK = 2                 /*Un-Packing*/
    }

    public enum eCASSETTE_MAP_FLAG
    {
        CASSETTE_MAP_EMPTY = 0,
        CASSETTE_MAP_EXIST = 1
    }

    public enum eMask_Status
    {
        CLEAN = 1,
        WAIT = 2,
        MS_WAIT = 3,
        AOI = 4 //20150720 Add by Frank For T3
    }

    public enum eHightCVmode
    {
        UNKNOW = 0,
        NORMAL_MODE = 1,
        ONE_BY_ONE_MODE = 2,
        BY_SIDE_MODE = 3
    }

    public enum eProduct_Type
    { 
        B1 = 1,
        B2 = 2,
        S1 = 3,
        S2 = 4
    }

    public enum eCELLATSRunMode
    {
        UNKNOW = 0,
        ArrayMode = 1,
        CFMode = 2,
        CellMode = 3
    }

    public enum eCELLATSLDOperMode
    {
        UNKNOW = 0,
        T1LoaderMode = 1,
        T2LoaderMode = 2,
        AutoChangeMode =3
    }
    public enum eCELLATSOperPermission
    {
        UNKNOW = 0,
        Request = 1,
        Complete = 2
    }
    public enum eCELLPortAssignment
    {
        UNKNOW = 0,
        GAP = 1,
        GMI = 2,
        PDR = 3,
        CEM = 4
    }
    public enum eboxReport  //Remove Report
    {
        NOReport = 0,
        NOProcess = 1,
        Processing = 2
    }
    public enum eVirtualPortMode  //VirtualPortMode For Robot
    {
        NotUse = 0,
        NormalMode = 1,
        LDVirtualPortMode = 2,
        ULDVirtualPortMode = 3
    }
    public enum eATSLoaderOperMode
    {
        UNKNOW = 0,
        T1LoaderMode = 1,
        T2LoaderMode = 2,
        AutoChangeMode = 3
    }
    //Watson Add 20150302
    public enum eRecipeCheckMode
    {
        UNKNOW = 0,
        Auto = 1,
        Manual = 2
   }
    /// <summary>
    /// For ProductInOutTotal Message 使用，
    /// 正常情况使用NORMAL，CUT Line 使用Cutting，CUT Line Unload 使用CUT Unload
    /// 20150421 tom 
    /// </summary>
    public enum eProductInOutTotalFlag
    {
        NORMAL = 0,
        CUTTING = 1,
        UNLOAD = 2
    }

    /// <summary>
    /// Add Common function  by bruce 2015/7/3 
    /// </summary>
    public enum eLogInOutMode
    {
        Login = 1,
        OpLogout = 2,
        AutoLogout = 3
    }

    public enum ePortModeProductTypeCheck //Add by jm.pan for T3
    {
        UNKNOWN = 0,
        ProductTypeCheck = 1,
        NoProductTypeCheck = 2
    }

    public enum eForceCleanOutAction
    {
        Set = 1,
        ReSet = 2
    }
    public enum eForceCleanOutType
    { 
        Normal = 1,
        Abnormal = 2
    }

    //add by bruce 2015/10/20 for Indexer robot Fetch sequence mode use
    public enum eRobotFetchSequenceMode
    { 
        FromLowertoUpper=1,
        FromUppertoLower=2
    }

    //add by yang 2017/1/11 for ELA Line Backup Mode Change Command ReturnCode 
    public enum eLineBackupReturnCode
    {
        /* 1:OK
         2:Connect Error
         3:CLN1 Backup CLN2, (CLN1 Not DOWN)
         4:CLN2 Backup CLN1, (CLN2 Not DOWN)
         5:Other Error
         */
        OK = 1,
        Connect_Error = 2,
        CLN1_Backup_CLN2_But_CLN1_Not_DOWN = 3,
        CLN2_Backup_CLN1_But_CLN2_Not_DOWN = 4,
        Other_Error = 5
    }

    //20170810 huangjiayin
    public enum eSMOEDCUnitType
    {
        OVEN=1,
        COOL=2
    }
}


