using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    /// <summary>
    /// 對應File, 修改Property後呼叫Save(), 會序列化存檔
    /// </summary>
    [Serializable]
    public class RobotStageEntityFile : EntityFile
    {
        public RobotStageEntityFile()
        {

        }

        public RobotStageEntityFile(int maxCount,string stageType)
        {
            switch (stageType)
            {
                case eRobotStageType.PORT:

                    _portSlotInfoList = new RobotStage_PortSlotInfo[maxCount];

                    for (int i = 0; i < maxCount; i++)
                    {
                        _portSlotInfoList[i] = new RobotStage_PortSlotInfo();
                        _portSlotInfoList[i].slotCSTSeq = "0";
                        _portSlotInfoList[i].slotJobSeq = "0";
                        _portSlotInfoList[i].slotGlassExist = "1"; //1:No Exist. 2:Exist
                    }

                    break;

                case eRobotStageType.FIXBUFFER:

                    _bufferSlotInfoList = new RobotStage_BufferSlotInfo[maxCount];

                    for (int i = 0; i < maxCount; i++)
                    {
                        _bufferSlotInfoList[i] = new RobotStage_BufferSlotInfo();
                        _bufferSlotInfoList[i].slotCSTSeq = "0";
                        _bufferSlotInfoList[i].slotJobSeq = "0";
                        _bufferSlotInfoList[i].slotGlassExist = "1"; //1:No Exist. 2:Exist
                    }

                    break;


                case eRobotStageType.EQUIPMENT: //Linklignal Stage

                    _equipmentSlotInfoList = new RobotStage_EquipmentSlotInfo[maxCount];

                    for (int i = 0; i < maxCount; i++)
                    {
                        _equipmentSlotInfoList[i] = new RobotStage_EquipmentSlotInfo();
                        _equipmentSlotInfoList[i].slotCSTSeq = "0";
                        _equipmentSlotInfoList[i].slotJobSeq = "0";
                        _equipmentSlotInfoList[i].slotGlassExist = "1"; //1:No Exist. 2:Exist
                    }

                    break;

                case eRobotStageType.STAGE: //Internal Stage

                    _stageSlotInfoList = new RobotStage_StageSlotInfo[maxCount];

                    for (int i = 0; i < maxCount; i++)
                    {
                        _stageSlotInfoList[i] = new RobotStage_StageSlotInfo();
                        _stageSlotInfoList[i].slotCSTSeq = "0";
                        _stageSlotInfoList[i].slotJobSeq = "0";
                        _stageSlotInfoList[i].slotGlassExist = "1"; //1:No Exist. 2:Exist
                    }

                    break;

                   
                default:
                    break;
            }

        }

        //Stage Type Is Port的所有Slot目前資訊
        private RobotStage_PortSlotInfo[] _portSlotInfoList;

        /// <summary>
        /// Stage Type Is Port的所有Slot目前資訊
        /// </summary>
        public RobotStage_PortSlotInfo[] PortSlotInfoList
        {
            get { return _portSlotInfoList; }
            set { _portSlotInfoList = value; }
        }

        //Stage Type is Buffer 的所有Slot目前資訊
        private RobotStage_BufferSlotInfo[] _bufferSlotInfoList;

        /// <summary>
        /// Stage Type Is Buffer的所有Slot目前資訊
        /// </summary>
        public RobotStage_BufferSlotInfo[] BufferSlotInfoList
        {
            get { return _bufferSlotInfoList; }
            set { _bufferSlotInfoList = value; }
        }

        //Stage Type is Equipment的所有Slot目前資訊
        private RobotStage_EquipmentSlotInfo[] _equipmentSlotInfoList;

        /// <summary>
        /// Stage Type Is Equipment的所有Slot目前資訊
        /// </summary>
        public RobotStage_EquipmentSlotInfo[] EquipmentSlotInfoList
        {
            get { return _equipmentSlotInfoList; }
            set { _equipmentSlotInfoList = value; }
        }

        //Stage Type is Stage的所有Slot目前資訊
        private RobotStage_StageSlotInfo[] _stageSlotInfoList;

        /// <summary>
        /// Stage Type Is Equipment的所有Slot目前資訊
        /// </summary>
        public RobotStage_StageSlotInfo[] StageSlotInfoList
        {
            get { return _stageSlotInfoList; }
            set { _stageSlotInfoList = value; }
        }

        ////Robot Stage目前的收送片狀態
        private string _curStageStatus = eRobotStageStatus.NO_REQUEST;

        /// <summary>
        /// Stage Current Status
        /// </summary>
        public string CurStageStatus
        {
            get { return _curStageStatus; }
            set { _curStageStatus = value; }
        }

        //當Status為UDRQ時記錄當下的CST Seq以供EX and GET&PUT Case判斷
        private string _curSendOut_CSTSeq = "0";

        /// <summary>
        /// 當Status為UDRQ時記錄當下的CST Seq以供EX and GET&PUT Case判斷
        /// </summary>
        public string CurSendOut_CSTSeq
        {
            get { return _curSendOut_CSTSeq; }
            set { _curSendOut_CSTSeq = value; }
        }

        //當Status為UDRQ時記錄當下的Job Seq以供EX and GET&PUT Case判斷
        private string _curSendOut_JobSeq = "0";

        /// <summary>
        /// 當Status為UDRQ時記錄當下的CST Seq以供EX and GET&PUT Case判斷
        /// </summary>
        public string CurSendOut_JobSeq
        {
            get { return _curSendOut_JobSeq; }
            set { _curSendOut_JobSeq = value; }
        }

        //當Status為UDRQ時記錄當下的CST Seq以供EX and GET&PUT Case判斷(GetGetPutPutFunction会有两个Job)
        private string _curSendOut_CSTSeq02 = "0";

        /// <summary>
        /// 當Status為UDRQ時記錄當下的CST Seq以供EX and GET&PUT Case判斷
        /// </summary>
        public string CurSendOut_CSTSeq02
        {
            get { return _curSendOut_CSTSeq02; }
            set { _curSendOut_CSTSeq02 = value; }
        }

        //當Status為UDRQ時記錄當下的Job Seq以供EX and GET&PUT Case判斷(GetGetPutPutFunction会有两个Job)
        private string _curSendOut_JobSeq02 = "0";

        /// <summary>
        /// 當Status為UDRQ時記錄當下的CST Seq以供EX and GET&PUT Case判斷
        /// </summary>
        public string CurSendOut_JobSeq02
        {
            get { return _curSendOut_JobSeq02; }
            set { _curSendOut_JobSeq02 = value; }
        }

        //20151126 add for Cell 1Arm 2Job
        //當Status為UDRQ時記錄當下的CST Seq以供EX and GET&PUT Case判斷(Cell 1Arm 2Job会有4个Job)
        private string _curSendOut_CSTSeq03 = "0";

        /// <summary>
        /// 當Status為UDRQ時記錄當下的CST Seq以供EX and GET&PUT Case判斷
        /// </summary>
        public string CurSendOut_CSTSeq03
        {
            get { return _curSendOut_CSTSeq03; }
            set { _curSendOut_CSTSeq03 = value; }
        }

        //當Status為UDRQ時記錄當下的Job Seq以供EX and GET&PUT Case判斷(Cell 1Arm 2Job会有4个Job)
        private string _curSendOut_JobSeq03 = "0";

        /// <summary>
        /// 當Status為UDRQ時記錄當下的CST Seq以供EX and GET&PUT Case判斷
        /// </summary>
        public string CurSendOut_JobSeq03
        {
            get { return _curSendOut_JobSeq03; }
            set { _curSendOut_JobSeq03 = value; }
        }

        //當Status為UDRQ時記錄當下的CST Seq以供EX and GET&PUT Case判斷(Cell 1Arm 2Job会有4个Job)
        private string _curSendOut_CSTSeq04 = "0";

        /// <summary>
        /// 當Status為UDRQ時記錄當下的CST Seq以供EX and GET&PUT Case判斷
        /// </summary>
        public string CurSendOut_CSTSeq04
        {
            get { return _curSendOut_CSTSeq04; }
            set { _curSendOut_CSTSeq04 = value; }
        }

        //當Status為UDRQ時記錄當下的Job Seq以供EX and GET&PUT Case判斷(Cell 1Arm 2Job会有4个Job)
        private string _curSendOut_JobSeq04 = "0";

        /// <summary>
        /// 當Status為UDRQ時記錄當下的CST Seq以供EX and GET&PUT Case判斷
        /// </summary>
        public string CurSendOut_JobSeq04
        {
            get { return _curSendOut_JobSeq04; }
            set { _curSendOut_JobSeq04 = value; }
        }

        //Stage Status change flag,提供RCS判斷是否通知OPI 一次更新整個Robot LayOut資訊
        private bool _statusChangeFlag = false;

        /// <summary>
        /// Stage Status change flag,提供RCS判斷是否通知OPI 一次更新整個Robot LayOut資訊
        /// </summary>
        public bool StatusChangeFlag
        {
            get { return _statusChangeFlag; }
            set { _statusChangeFlag = value; }
        }

        private string _ldrq_CstStatusPriority = "9";

        /// <summary>
        /// 當CST Stage LDRQ時根據CST Status收片的優先次序
        /// </summary>
        public string LDRQ_CstStatusPriority
        {
            get { return _ldrq_CstStatusPriority; }
            set { _ldrq_CstStatusPriority = value; }
        }

        private string _stage_UDRQ_Status;

        /// <summary>
        /// Stage判斷UDRQ時的狀態.將會與Stage_LDRQ_status by Stage Type來決定整個Stage的Status
        /// </summary>
        public string Stage_UDRQ_Status
        {
            get { return _stage_UDRQ_Status; }
            set { _stage_UDRQ_Status = value; }
        }

        private string _stage_LDRQ_Status;

        /// <summary>
        /// Stage判斷LDRQ時的狀態.將會與Stage_LDRQ_status by Stage Type來決定整個Stage的Status
        /// </summary>
        public string Stage_LDRQ_Status
        {
            get { return _stage_LDRQ_Status; }
            set { _stage_LDRQ_Status = value; }
        }

        //LinkSignal 是否有發出ExchangePossuble
        private bool _downStreamExchangeReqFlag = false;

        public bool DownStreamExchangeReqFlag
        {
            get { return _downStreamExchangeReqFlag; }
            set { _downStreamExchangeReqFlag = value; }
        }

        //LinkSignal Receive Type [Array shop / DRY line][DRY-Chamber Run Mode] 
        // GlobalAssemblyVersion v1.0.0.26-20151015, added by dade
        private int _downStreamLoadLockReceiveType = 0; //0=NOREQ
 
        public int DownStreamLoadLockReceiveType
        {
            get { return _downStreamLoadLockReceiveType; }
            set { _downStreamLoadLockReceiveType = value; }
        }
        
        //LinkSignal Receive Type [Array shop / DRY line][DRY-Chamber Run Mode] 
        // GlobalAssemblyVersion v1.0.0.26-20151103, added by dade
        private int _dryKeptLoadLockReceiveType = 0; //0=NOREQ
 
        public int DryKeptLoadLockReceiveType
        {
            get { return _dryKeptLoadLockReceiveType; }
            set { _dryKeptLoadLockReceiveType = value; }
        }
        
        //LinkSignal Receive Able [Array shop / DRY line][DRY I/F]
        // GlobalAssemblyVersion v1.0.0.26-20151021, added by dade
        private bool _downStreamReceiveAbleSignal = false;
        
        public bool DownStreamReceiveAbleSignal
        {
            get { return _downStreamReceiveAbleSignal; }
            set { _downStreamReceiveAbleSignal = value; }
        }
        
        //LinkSignal Receive Job Reserve Signal
        // GlobalAssemblyVersion v1.0.0.26-20151021, added by dade
        private bool _downStreamReceiveJobReserveSignal = false;
        
        public bool DownStreamReceiveJobReserveSignal
        {
            get { return _downStreamReceiveJobReserveSignal; }
            set { _downStreamReceiveJobReserveSignal = value; }
        }

        //20151215 add LinkSignal Transfer Stop Request Signal
        /// <summary>紀錄EQP Type Stage Transfer Stop Request是否有On
        /// 
        /// </summary>
        private bool _downStreamTransferStopRequestFlag = false;

        /// <summary>紀錄EQP Type Stage Transfer Stop Request是否有On
        /// 
        /// </summary>
        public bool DownStreamTransferStopRequestFlag
        {
            get { return _downStreamTransferStopRequestFlag; }
            set { _downStreamTransferStopRequestFlag = value; }
        }

        //20151024 add for Keep Cst First Glass Check Info
        private string _cstFirstGlassCheckResult = string.Empty;

        public string CstFirstGlassCheckResult
        {
            get { return _cstFirstGlassCheckResult; }
            set { _cstFirstGlassCheckResult = value; }
        }

        //Watson Add 20151027 For Stage Input Time and output Time
        public DateTime InputDateTime;
        public DateTime OutputDateTime;

        //Watson Add 20160111 For CF MQC TTP 記住每個Port現行的RouteID
        private string _curRouteID = string.Empty;

        public string CurRouteID
        {
            get { return _curRouteID; }
            set { _curRouteID = value; }
        }
        #region [ 20150709 mark old Entity ]

        ////RB最後服務時間
        //private DateTime _rbLastServiceTime;

        //public DateTime rbLasteServiceTime
        //{
        //    get { return _rbLastServiceTime; }
        //    set { _rbLastServiceTime = value; }
        //}

        ////Stage Type Port
        //private RobotStagePortSlotInfo[] _portSlotInfos;

        //public RobotStagePortSlotInfo[] portSlotInfos
        //{
        //    get { return _portSlotInfos; }
        //    set { _portSlotInfos = value; }
        //}

        ////20141020 add for Slot Job Info
        //public RobotStagBufferSlotInfo[] _bufferSlotInfos;

        //public RobotStagBufferSlotInfo[] bufferSlotInfos
        //{
        //    get { return _bufferSlotInfos; }
        //    set { _bufferSlotInfos = value; }
        //}

        //private eBitResult _sendReady;
        //private eBitResult _receiveReady;
        //private eBitResult _glassExist;
        //private eBitResult _doubleGlassExist;
        //private eBitResult _exchangePossible;

        //public eBitResult sendReady
        //{
        //    get { return _sendReady; }
        //    set { _sendReady = value; }
        //}

        //public eBitResult receiveReady
        //{
        //    get { return _receiveReady; }
        //    set { _receiveReady = value; }
        //}

        //public eBitResult glassExist
        //{
        //    get { return _glassExist; }
        //    set { _glassExist = value; }
        //}

        //public eBitResult doubleGlassExist
        //{
        //    get { return _doubleGlassExist; }
        //    set { _doubleGlassExist = value; }
        //}

        //public eBitResult exchangePossible
        //{
        //    get { return _exchangePossible; }
        //    set { _exchangePossible = value; }
        //}

        ////RB Stage目前的狀態
        //private string _curStageStatus = eRobotStageStatus.NO_REQUEST;

        //public string curStageStatus
        //{
        //    get { return _curStageStatus; }
        //    set { _curStageStatus = value; }
        //}

        ////LinkSignal 是否有發出ExchangePossuble
        //private bool _downStreamExchangeReqFlag=false;

        //public bool DownStreamExchangeReqFlag
        //{
        //    get { return _downStreamExchangeReqFlag; }
        //    set { _downStreamExchangeReqFlag = value; }
        //}

        //private string _ldrqEmptySlot = string.Empty;

        ////當LDRQ時可以收片的Slot位置, 除Port之外其他都型態Stage都預設為01
        //public string LDRQEmptySlot
        //{
        //    get { return _ldrqEmptySlot; }
        //    set { _ldrqEmptySlot = value; }
        //}

        ////當CST為UDRQ時記錄當下的JobKey以供EX and GET&PUT Case判斷
        //private string _udrqJobKey = "0_0";

        //public string UDRQJobKey
        //{
        //    get { return _udrqJobKey; }
        //    set { _udrqJobKey = value; }
        //}

        ////當CST為LDRQ時記錄當下的CST Status Priority以供Robot Dispatch判斷
        ////20150128 modify 預設為9 排序數字越小越優先
        //private string _ldrqCSTStatusPriority = "9";

        //public string LDRQCSTStatusPriority
        //{
        //    get { return _ldrqCSTStatusPriority; }
        //    set { _ldrqCSTStatusPriority = value; }
        //}

        ////20150128 add ULD Dispatch Rule PortNode檢查符合之後,要跟根據PortMode來排序,預設為9 排序數字越小越優先
        //private string _uldPortModePriority = "99";

        ///// <summary>
        ///// ULD Dispatch Rule PortNode檢查符合之後,要跟根據PortMode來排序,預設為99 排序數字越小越優先
        ///// </summary>
        //public string ULDPortModePriority
        //{
        //    get { return _uldPortModePriority; }
        //    set { _uldPortModePriority = value; }
        //}

        ////20150128 add ULD Dispatch Rule Grade檢查符合之後,要跟根據Abnormal Rule來排序,預設為9 排序數字越小越優先
        //private string _uldAbnormalRulePriority = "99";

        ///// <summary>
        ///// ULD Dispatch Rule Grade檢查符合之後,要跟根據Abnormal Rule來排序,預設為99 排序數字越小越優先
        ///// </summary>
        //public string ULDAbnormalRulePriority
        //{
        //    get { return _uldAbnormalRulePriority; }
        //    set { _uldAbnormalRulePriority = value; }
        //}

        ////20150130 add Stage Status change flag
        //private bool _statusChangeFlag = false;

        //public bool StatusChangeFlag
        //{
        //    get { return _statusChangeFlag; }
        //    set { _statusChangeFlag = value; }
        //}

        ///// <summary>
        ///// Both Port Stage Check UDRQ Status NOREQ or UDRQ
        ///// </summary>
        //private string _curBothPortCheckUDRQStatus = eRobotStageStatus.NO_REQUEST;

        //public string curBothPortCheckUDRQStatus
        //{
        //    get { return _curBothPortCheckUDRQStatus; }
        //    set { _curBothPortCheckUDRQStatus = value; }
        //}

        ////20150317 add ULD Dispatch Rule PortNode檢查符合之後,要跟根據Match Condition來排序(Match > MX),預設為99 排序數字越小越優先
        //private string _uldPortMatchSetGradePriority = "99";

        ///// <summary>
        ///// ULD Dispatch Rule PortNode檢查符合之後,要跟根據Match Condition來排序(Match > MX),預設為99 排序數字越小越優先
        ///// </summary>
        //public string ULDPortMatchSetGradePriority
        //{
        //    get { return _uldPortMatchSetGradePriority; }
        //    set { _uldPortMatchSetGradePriority = value; }
        //}

        //////20150317 add Stage Node Stack 0~65535
        //private string _curPortNodeStack = string.Empty;

        ///// <summary>
        ///// 紀錄目前Robot Port Stage是哪種NodeStack
        ///// </summary>
        //public string curPortNodeStack
        //{
        //    get { return _curPortNodeStack; }
        //    set { _curPortNodeStack = value; }
        //}

        #endregion

    }

    [Serializable]
    public class RobotStage_PortSlotInfo
    {

        private string _slotCSTSeq;
        private string _slotJobSeq;
        private string _slotGlassExist;

        public string slotCSTSeq
        {
            get { return _slotCSTSeq; }
            set { _slotCSTSeq = value; }
        }

        public string slotJobSeq
        {
            get { return _slotJobSeq; }
            set { _slotJobSeq = value; }
        }

        public string slotGlassExist
        {
            get { return _slotGlassExist; }
            set { _slotGlassExist = value; }
        }

    }

    [Serializable]
    public class RobotStage_BufferSlotInfo
    {

        private string _slotCSTSeq;
        private string _slotJobSeq;
        private string _slotGlassExist;

        public string slotCSTSeq
        {
            get { return _slotCSTSeq; }
            set { _slotCSTSeq = value; }
        }

        public string slotJobSeq
        {
            get { return _slotJobSeq; }
            set { _slotJobSeq = value; }
        }

        public string slotGlassExist
        {
            get { return _slotGlassExist; }
            set { _slotGlassExist = value; }
        }

    }

    [Serializable]
    public class RobotStage_EquipmentSlotInfo
    {

        private string _slotCSTSeq;
        private string _slotJobSeq;
        private string _slotGlassExist;

        public string slotCSTSeq
        {
            get { return _slotCSTSeq; }
            set { _slotCSTSeq = value; }
        }

        public string slotJobSeq
        {
            get { return _slotJobSeq; }
            set { _slotJobSeq = value; }
        }

        public string slotGlassExist
        {
            get { return _slotGlassExist; }
            set { _slotGlassExist = value; }
        }

    }

    [Serializable]
    public class RobotStage_StageSlotInfo
    {

        private string _slotCSTSeq;
        private string _slotJobSeq;
        private string _slotGlassExist;

        public string slotCSTSeq
        {
            get { return _slotCSTSeq; }
            set { _slotCSTSeq = value; }
        }

        public string slotJobSeq
        {
            get { return _slotJobSeq; }
            set { _slotJobSeq = value; }
        }

        public string slotGlassExist
        {
            get { return _slotGlassExist; }
            set { _slotGlassExist = value; }
        }

    }

    public class CellSlotBlock
    {
        public string FrontCstSeqNo { get; set; }
        public string FrontJobSeqNo { get; set; }
        public string BackCstSeqNo { get; set; }
        public string BackJobSeqNo { get; set; }
        //20160602
        public int RowsPriority { get; set; }

        public CellSlotBlock(string FrontCstSeqNo, string FrontJobSeqNo, string BackCstSeqNo, string BackJobSeqNo, int RowsPriority)
        {
            this.FrontCstSeqNo = FrontCstSeqNo;
            this.FrontJobSeqNo = FrontJobSeqNo;
            this.BackCstSeqNo = BackCstSeqNo;
            this.BackJobSeqNo = BackJobSeqNo;
            this.RowsPriority = RowsPriority;
        }

        public bool FrontJobExist
        {
            get
            {
                int cst_seq_no = 0, job_seq_no = 0;
                bool ret = (int.TryParse(FrontCstSeqNo, out cst_seq_no) && int.TryParse(FrontJobSeqNo, out job_seq_no) && cst_seq_no > 0 && job_seq_no > 0);
                return ret;
            }
        }

        public bool BackJobExist
        {
            get
            {
                int cst_seq_no = 0, job_seq_no = 0;
                bool ret = (int.TryParse(BackCstSeqNo, out cst_seq_no) && int.TryParse(BackJobSeqNo, out job_seq_no) && cst_seq_no > 0 && job_seq_no > 0);
                return ret;
            }
        }
    }

    public class RobotStage:Entity
    {
        public RobotStageEntityData Data { get;  set; }

        public RobotStageEntityFile File { get; private set; }

        public RobotStage(RobotStageEntityData data, RobotStageEntityFile file)
        {
            Data = data;
            File = file;
        }

        #region [ 即時運算變數,不需要存入檔案 ]

        private string _curLDRQ_EmptySlotNo = string.Empty;

        //當Status=LDRQ時可以收片的SlotNo
        public string CurLDRQ_EmptySlotNo
        {
            get { return _curLDRQ_EmptySlotNo; }
            set { _curLDRQ_EmptySlotNo = value; }
        }


        private string _curLDRQ_EmptySlotNo02 = string.Empty;
        //當Status=LDRQ時可以收片的SlotNo02(For GetGetPutPut)第二个可收片的SlotNo
        public string CurLDRQ_EmptySlotNo02
        {
            get { return _curLDRQ_EmptySlotNo02; }
            set { _curLDRQ_EmptySlotNo02 = value; }
        }

        /// <summary> 紀錄LDRQ時Empty SlotNo的List. (SlotNo, CSTSeq_JobSeq)
        /// 
        /// </summary>
        public Dictionary<int, string> curLDRQ_EmptySlotList = new Dictionary<int, string>(); 

        ////當Status=LDRQ時可以收片的SlotNo
        //public string CurLDRQ_EmptySlotNo
        //{
        //    get { return _curLDRQ_EmptySlotNo; }
        //    set { _curLDRQ_EmptySlotNo = value; }
        //}

        /// <summary> 紀錄UDRQ時UDRQ Job SlotNo的List. (SlotNo, CSTSeq_JobSeq)
        /// 
        /// </summary>
        public Dictionary<int, string> curUDRQ_SlotList = new Dictionary<int, string>();

        /// <summary>
        /// Port Stage的即時在席資料, 在 RobotSelectJobService 更新, 只可以在RobotMainProcess裡使用
        /// PortSlotInfos[0]表示第1個slot, PortSlotInfos[27]表示第28個slot
        /// </summary>
        public List<RobotStage_PortSlotInfo> PortSlotInfos = new List<RobotStage_PortSlotInfo>();

        //20151218 add for Cell Special
        /// <summary> 紀錄UDRQ時UDRQ SlotBlockInfo的List. (CmdSlotNo, UDRQ(SlotNo, JobKey))
        /// 
        /// </summary>
        public Dictionary<int, Dictionary<int, string>> curUDRQ_SlotBlockInfoList = new Dictionary<int,Dictionary<int,string>>();

        //20151218 add for Cell Special
        /// <summary> 紀錄LDRQ時LDRQ SlotBlockInfo的List. (CmdSlotNo, CellSlotBlock), SortedDictionary預設升冪排序
        /// 
        /// </summary>
        public Dictionary<int, CellSlotBlock> curLDRQ_EmptySlotBlockInfoList = new Dictionary<int, CellSlotBlock>();

        //20160106 add for 新需求:MAC Recipe ID最后一码 = “1”，则为需要Turn基板，如MAC设备中有Turn的基板，MAC不能要求Exchange，BC不能下达Exchange Command给Robot;
        private bool macCanNotExchangeFlag = false;

        /// <summary>MAC Recipe ID最后一码 = “1”，则为需要Turn基板，如MAC设备中有Turn的基板，MAC不能要求Exchange，CPC不能下达Exchange Command给Robot;
        /// 
        /// </summary>
        public bool MacCanNotExchangeFlag
        {
            get { return macCanNotExchangeFlag; }
            set { macCanNotExchangeFlag = value; }
        }


        //20160108 add CELL ULD Dispatch Rule PortStage檢查符合之後,要根據Match Condition來排序(Match >EMP> MX),預設為99 排序數字越小越優先
        private string _unloaderPortMatchSetGradePriority = "99";

        /// <summary>CELL ULD Dispatch Rule PortStage檢查符合之後,要根據Match Condition來排序(Match >EMP> MX),預設為99 排序數字越小越優先
        /// 
        /// </summary>
        public string UnloaderPortMatchSetGradePriority
        {
            get { return _unloaderPortMatchSetGradePriority; }
            set { _unloaderPortMatchSetGradePriority = value; }
        }

        //20160118 add CELL ULD 收片邏輯, JobData EQP Flag中DCRandSorterFlag相同的Job 要收在同一Cassette, _unloaderPortSlotJobDCRandSorterFlag有值表示相同, 無值表示空Cassette
        private string _unloaderPortSlotJobDCRandSorterFlag = string.Empty;

        /// <summary>CELL ULD 收片邏輯, JobData EQP Flag中DCRandSorterFlag相同的Job 要收在同一Cassette, _unloaderPortSlotJobDCRandSorterFlag有值表示相同, 無值表示空Cassette
        /// 
        /// </summary>
        public string UnloaderPortSlotJobDCRandSorterFlag
        {
            get { return _unloaderPortSlotJobDCRandSorterFlag; }
            set { _unloaderPortSlotJobDCRandSorterFlag = value; }
        }

        /// <summary>
        /// Port 上 Cassette 的 Empty 狀態
        /// </summary>
        public enum PORTCSTEMPTY
        {
            UNKNOWN = 0,
            EMPTY = 1,
            NOT_EMPTY = 2,
            ERROR = 3
        }

        //20160127 Select Port 時記錄 port cassette 是否 empty, 避免 GETGET, PUTPUT 上 PortMode EMP
        private PORTCSTEMPTY _portCassetteEmpty = PORTCSTEMPTY.UNKNOWN;

        /// <summary>Select Port 時記錄 port cassette 是否 empty, 避免 GETGET, PUTPUT 上 PortMode EMP
        /// 
        /// </summary>
        public PORTCSTEMPTY PortCassetteEmpty
        {
            get { return _portCassetteEmpty; }
            set { _portCassetteEmpty = value; }
        }

        //20160201 Select Port 時記錄 Cassette Start Time
        private DateTime _cassetteStartTime = DateTime.MinValue;

        public DateTime CassetteStartTime
        {
            get { return _cassetteStartTime; }
            set { _cassetteStartTime = value; }
        }

        /// <summary>
        /// For Cell 1Arm2Job Sorter Mode, Loader 不論 Sampling Flag 皆要出片, 而不同 Sampling Flag 的 Job 要分別存放在不同 Unloader
        /// </summary>
        public enum UNLOADER_SAMPLING_FLAG
        {
            UNKOWN = 0,
            EMPTY = 1,
            SAMPLING_FLAG_ON = 2,
            SAMPLING_FLAG_OFF = 3,
            ERROR = 4
        }

        //20160201 Select Port 時記錄 Cassette Start Time
        private UNLOADER_SAMPLING_FLAG _unloaderSamplingFlag = UNLOADER_SAMPLING_FLAG.UNKOWN;

        public UNLOADER_SAMPLING_FLAG UnloaderSamplingFlag
        {
            get { return _unloaderSamplingFlag; }
            set { _unloaderSamplingFlag = value; }
        }

        //20160302 add
        /// <summary>For Array Use Only , Port Stage內所有Glass的Recipe GroupNo的集合
        /// 
        /// </summary>
        public List<string> CurRecipeGroupNoList = new List<string>();

        //20160511 add 針對每個RobotStage,分別對可控Job做RecipeGroup紀錄
        public List<Job> AllJobRecipeGroupNoList = new List<Job>();

        #endregion

    }
}
