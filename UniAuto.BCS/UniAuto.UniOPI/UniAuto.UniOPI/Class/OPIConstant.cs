using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.ComponentModel;

namespace UniOPI
{
    public enum eOPIMessageType
    {
        Warning = 0,
        Error = 1,
        Question = 2,
        Information = 3
    }

    //for CF Photo CV#03
    public enum eHighCVMode
    {
        [Description("UnKnown")]
        UnKnown = 0,
        [Description("NormalMode")]
        NormalMode = 1,
        [Description("One by One Mode")]
        OnebyOneMode = 2,
        [Description("By Side Mode")]
        BySideMode = 3
    }

    public enum ePalletMode
    {
        UnKnown = 0,
        PackMode = 1,
        UnpackMode = 2
    }

    public enum ePackingMode
    {
        UnKnown = 0,
        PackMode = 1,
        UnpackMode = 2
    }

    public enum eUnpackSource
    {
        UnKnown = 0,
        DPK = 1,
        DPS = 2
    }

    public enum eObjectType
    {
        TextBox,
        ComboBox,
        CheckBox
    }

    public enum UpdateResult
    {
        None,
        CheckFail,
        UpdateFail,
        UpdateSuccess
    }

    public enum eEQPStatus
    {
        UnKnown = 0,
        Setup = 1,
        Stop = 2,
        Pause = 3,
        Idle = 4,
        Run = 5
    }

    public enum eCassetteType
    {
        UnKnown = 0,
        NormalCassette = 1,
        WireCassette = 2,
        CellCassette = 3,
        DenseBox = 4,
        BufferCassette = 5,
        Scrap =6
    }

    public enum ePortStatus
    {
        UnKnown = 0,
        LoadRequest = 1,
        LoadComplete = 2,
        UnloadRequest = 3,
        UnloadComplete = 4
    }

    public enum eCassetteStatus
    {
        UnKnown =0,
        NoCassetteExist = 1,
        WaitingforCassetteData = 2,
        WaitingforStartCommand = 3,
        WaitingforProcessing = 4,
        InProcessing = 5,
        ProcessPaused = 6,
        ProcessCompleted = 7,
        CassetteReMap = 8,
        InAborting = 9
    }

    public enum eRobotFetchSequenceMode
    {
        [Description("UnKnown")]
        UnKnown = 0,
        [Description("From Lower to Upper")]
        FromLowerToUpper=1,
        [Description("From Upper to Lower")]
        FromUpperToLower=2
    }

    public enum eRobotOperMode
    {
        [Description("UnKnown")]
        UnKnown = 0,
        [Description("NormalMode")]
        NormalMode=1,
        [Description("D(Dual)Mode")]
        DualMode=2,
        [Description("S (Single)")]
        SingleMode=3
    }

    public enum eRobotStatus
    {
        [Description("UnKnown")]
        UnKnown = 0,
        [Description("Setup")]
        Setup = 1,
        [Description("Stop")]
        Stop = 2,
        [Description("Pause")]
        Pause = 3,
        [Description("Idle")]
        Idle = 4,
        [Description("Running")]
        Running = 5
    }

    public enum eRobotJobStatus
    {
        [Description("UnKnown")]
        UnKnown = 0,
        [Description("NoExist")]
        NoExist = 1,
        [Description("Exist")]
        Exist = 2,
        [Description("Arm Disabled")]
        ArmDisabled = 4,
        [Description("Arm Disabled & No Exist Job")]
        ArmDisabledNoExist = 5,
        [Description("Arm Disable & Exist Job")]
        ArmDisableExist = 6
    }

    public enum eRobotStageStatus
    {
        [Description("UnKnown")]
        UnKnown = 0,
        [Description("NoRquest")] //無RB服務
        NoRquest = 1,
        [Description("LDRQ")] //可收片
        LDRQ = 2,
        [Description("UDRQ")] //有片
        UDRQ = 3,
        [Description("LDRQ_UDRQ")]
        LDRQ_UDRQ = 4
    }

    public enum eChangerPlanStatus
    {
        NoPlan = 0,
        Start = 1,
        End = 2,
        Ready = 3,
        Request = 4,
        Cancel = 5,
        Aborting = 6,
        Abort = 7,
        Waiting = 8  
    }

    public enum eRobotOperationMode
    {
        UnKnown = 0,
        NormalMode = 1,
        DualMode = 2,
        SingleMode = 3
    }

    //public enum eIndexerMode
    //{
    //    [Description("UnKnown")]
    //    UnKnown = 0,
    //    [Description("Sampling Mode")]
    //    SamplingMode = 1,
    //    [Description("Sorter Mode")]
    //    SorterMode = 2,
    //    [Description("Changer Mode")]
    //    ChangerMode = 3,
    //    [Description("Cool Run Mode")]
    //    CoolRunMode = 4,
    //    [Description("Force Clean Out Mode")]
    //    ForceCleanOut = 5,
    //    [Description("Abnormal Force Clean Out Mode")]
    //    MixRunMode = 6,
    //    [Description("MQC Mode")]
    //    MQCMode = 7,
    //    [Description("Through Mode")]
    //    ThroughMode = 8,
    //    [Description("Fix Mode")]
    //    FixMode = 9,
    //    [Description("Random Mode")]
    //    RandomMode = 10,
    //    [Description("Normal Mode")]
    //    NormalMode = 11,
    //    [Description("Mix Mode")]
    //    MixMode = 12
    //}

    public enum ePortAssignment
    {
        [Description("UnKnown")]
        UnKnown = 0,
        [Description("for GAP")]
        GAP = 1,
        [Description("for GMI")]
        GMI = 2,
        [Description("for PDR")]
        PDR = 3,
        [Description("for CEM")]
        CEM = 4
    }

    public enum ePortType
    {
        [Description("UnKnown")]
        UnKnown = 0,
        [Description("Loading Port")]
        LoadingPort = 1,
        [Description("Unloading Port")]
        UnloadingPort = 2,
        [Description("Both Port")]
        BothPort = 3,
        [Description("Buffer Port - Buffer Type")]
        BufferType = 4,
        [Description("Buffer Port - Loader in Buffer Type")]
        LoaderinBufferType = 5,
        [Description("Buffer Port - Un-loader in Buffer Type")]
        UnloaderinBufferType = 6
    }

    public enum ePortDown
    {
        [Description("No Use")]
        NoUse = 0,
        [Description("Normal")]
        Normal = 1,
        [Description("Down")]
        Down = 2
    }

    public enum ePortMode
    {
        [Description("Unknown")]
        Unknown = 0,
        [Description("TFT Mode")]
        TFTMode = 1,
        [Description("CF Mode")]
        CFMode = 2,
        [Description("Dummy Mode")]
        DummyMode = 3,
        [Description("MQC Mode")]
        MQCMode = 4,
        [Description("HT Mode")]
        HTMode = 5,
        [Description("LT Mode")]
        LTMode = 6,
        [Description("ENG Mode")]
        ENGMode = 7,
        [Description("IGZO Mode")]
        IGZOMode = 8,
        [Description("ILC Mode")]
        ILCMode = 9,
        [Description("FLC Mode")]
        FLCMode = 10,
        [Description("Through Dummy Mode")]
        ThroughDummyMode = 11,
        [Description("Thickness Dummy Mode")]
        ThicknessDummyMode = 12,
        [Description("UV Mask Mode")]
        UVMaskMode = 13,
        [Description("By Grade Mode")]
        ByGradeMode = 14,
        [Description("OK Mode")]
        OKMode = 15,
        [Description("NG Mode")]
        NGMode = 16,
        [Description("MIX Mode")]
        MIXMode = 17,
        [Description("EMP Mode")]
        EMPMode = 18,
        [Description("RW Mode")]
        ReworkMode = 19,
        [Description("Mismatch Mode")]
        MismatchMode = 20,
        [Description("PD Mode")]
        PDMode = 21,
        [Description("IR Mode")]
        IRMode = 22,
        [Description("RP Mode")]
        RPMode = 23,
        [Description("Re-Judge Mode")]
        ReJudgeMode = 24
    }

    public enum ePartialFull
    {
        Unknown = 0,
        PartialFull = 1,
        NoPartialFull = 2
    }

    public enum eLoadingCassetteType
    {
        Unknown = 0,
        ActualCassette = 1,
        EmptyCassette = 2
    }

    public enum ePortTransfer
    {
        [Description("Unknown")]
        Unknown = 0,
        [Description("MGV Mode")]
        MGVMode = 1,
        [Description("AGV Mode")]
        AGVMode = 2,
        [Description("Stocker Inline Mode")]
        StockerInlineMode = 3
    }

    public enum ePortEnable
    {
        [Description("Unknown")]
        Unknown = 0,
        [Description("Port Enable")]
        PortEnable = 1,
        [Description("Port Disable")]
        PortDisable = 2
    }

    public enum eBoxType
    {
        [Description("InBox")]
        InBox = 1,
        [Description("OutBox")]
        OutBox = 2,
        [Description("Unknown")]
        Unknown = 0
    }

    public enum eEQPOperationMode
    {
        [Description("Unknown")]
        Unknown = 0,
        [Description("Manual")]
        Manual = 1 ,
        [Description("Auto")]
        Auto = 2,
        [Description("Semi-Auto")]
        SemiAuto = 3
    }

    ////for ATS Loader Operation Mode (CBATS )
    //public enum eLoaderOperationMode
    //{
    //    [Description("Unknown")]
    //    Unknown = 0,
    //    [Description("t1 Loader Mode")]
    //    T1LoaderMode = 1,
    //    [Description("t2 Loader Mode")]
    //    T2LoaderMode = 2,
    //    [Description("Auto Change Mode")]
    //    AutoChangeMode = 3
    //}

    public enum eCIMMode
    {
        OFF =0,
        ON=1
    }
    
    public enum eInterlockMode
    {
        OFF = 0,
        ON = 1
    }
    
    public enum eVCRMode
    {
        [Description("Disable")]
        DISABLE=0,
        [Description("Enable")]
        ENABLE=1
    }

    public enum eForceVCRMode_SOR
    {
        [Description("Unknown")]
        Unknown = 0,
        [Description("Enable")]
        ENABLE = 1,
        [Description("Disable")]
        DISABLE = 2
    }

    public enum eVirualPortOpMode_SOR
    {
        [Description("No Use")]
        NoUse = 0,
        [Description("Normal Mode")]
        NormalMode = 1,
        [Description("LD Virtual Port Mode")]
        LDVirualPortMode = 2,
        [Description("ULD Virtual Port Mode")]
        ULDVirualPortMode = 3
    }

    public enum eCSTOperationMode
    {
        [Description("Unknown")]
        Unknown = 0,
        [Description("Kind To Kind Mode")]
        KindToKind = 1,
        [Description("Cassette To Cassette Mode")]
        CassetteToCassette = 2,
        [Description("Lot to Lot Mode")]
        LotToLot = 3
    }

    struct OPIConst
    {
        #region 定義OPI會使用到的常數值
        public static string LayoutFolder = ""; //記錄Layout.xml放置資料夾
        public static string ParamFolder = ""; //記錄 DBConfig.xml放置資料夾
        public static string RobotFolder = "";  //紀錄robot image放置的資料夾
       
        public static string TimingChartFolder = string.Empty; //紀錄 timing chart 存放的資料夾

        public static string Default_RobotFunKey = "UN0000";

        public static readonly string DBCFG_XML_FILE_NAME = @"\DBConfig.xml";
        public static readonly List<string> LstLineType_Cutting = new List<string>(new string[] { "CUT_1" ,"CUT_2","CUT_3"});
        public static readonly List<string> LstLineType_Photo = new List<string>(new string[] { "FCBPH_TYPE1","FCMPH_TYPE1","FCSPH_TYPE1","FCRPH_TYPE1","FCGPH_TYPE1","FCOPH_TYPE1" });
        //public static readonly List<string> LstLineType_UPK = new List<string>(new string[] { "FCUPK_TYPE1", "FCUPK_TYPE2" });
        //public static readonly List<string> LstLineType_CVD_DRY = new List<string>(new string[] { "CVD_AKT", "CVD_ULVAC", "DRY_ICD","DRY_YAC" });

        #endregion
    }
}




