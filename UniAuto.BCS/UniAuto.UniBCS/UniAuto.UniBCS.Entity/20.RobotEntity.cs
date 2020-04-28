using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.MISC;

namespace UniAuto.UniBCS.Entity
{
    /// <summary>
    /// 對應File, 修改Property後呼叫Save(), 會序列化存檔
    /// </summary>
    [Serializable]
    public class RobotEntityFile : EntityFile
    {

        public RobotEntityFile()
        {

        }

        public RobotEntityFile(int robotArmCount, bool doubleSubstrateFlag)
        {
            //20151208 mark 改到RealTime區域不需要存File
            //if (doubleSubstrateFlag == false)
            //{
            //    //Robot has 2Arm ,One Arm has 1 Job
            //    _armSignalSubstrateInfoList = new RobotArmSignalSubstrateInfo[robotArmCount];

            //    for (int i = 0; i < robotArmCount; i++)
            //    {
            //        _armSignalSubstrateInfoList[i] = new RobotArmSignalSubstrateInfo();
            //        _armSignalSubstrateInfoList[i].ArmCSTSeq = "0";
            //        _armSignalSubstrateInfoList[i].ArmJobSeq = "0";
            //        _armSignalSubstrateInfoList[i].ArmJobExist = eGlassExist.NoExist; //1:No Exist. 2:Exist
            //    }

            //}
            //else
            //{

            //    //Robot has 4Arm, One Arm has 2 Job
            //    _armDoubleSubstrateInfoList = new RobotArmDoubleSubstrateInfo[robotArmCount];

            //    for (int i = 0; i < robotArmCount; i++)
            //    {
            //        _armDoubleSubstrateInfoList[i] = new RobotArmDoubleSubstrateInfo();
            //        _armDoubleSubstrateInfoList[i].ArmFrontCSTSeq = "0";
            //        _armDoubleSubstrateInfoList[i].ArmFrontJobSeq = "0";
            //        _armDoubleSubstrateInfoList[i].ArmFrontJobExist = eGlassExist.NoExist; //1:No Exist. 2:Exist
            //        _armDoubleSubstrateInfoList[i].ArmBackCSTSeq = "0";
            //        _armDoubleSubstrateInfoList[i].ArmBackJobSeq = "0";
            //        _armDoubleSubstrateInfoList[i].ArmBackJobExist = eGlassExist.NoExist; //1:No Exist. 2:Exist
            //    }

            //}

        }

        #region SameEQ
        private string _curRobotSameEQFlag = eRobot_SameEQFlag.NO;

        /// <summary> 
        /// 目前Robot的SameEQFlag(N/Y)
        /// </summary>
        public string curRobotSameEQFlag
        {
            get { return _curRobotSameEQFlag; }
            set { _curRobotSameEQFlag = value; }
        }

        /// <summary>
        /// Dictionary[CSTSeqNo, Dictionary[StepID,NodeNo], 記錄Cassette Job在PUT Step進了哪一個NodeNo
        /// </summary>
        private SerializableDictionary<string, SerializableDictionary<int, string>> _sameEQMap = new SerializableDictionary<string, SerializableDictionary<int, string>>();//Cassette Process Complete的時候要清Dictionary

        /// <summary>
        /// 查Job在PUT Step時是否有進入過EQ
        /// </summary>
        /// <param name="CSTSeqNo">Job的CSTSeqNo</param>
        /// <param name="StepID">Job的Put Step ID</param>
        /// <param name="NodeNo">同一CST的Job在同一Put Step時有進入過Node</param>
        /// <returns>true表示同一CST的Job在同一Put Step時有進入過Node, false表示尚未有同一CST的Job在同一Put Step時有進入過Node</returns>
        public bool CheckMap(string CSTSeqNo, int StepID, out string NodeNo)
        {
            NodeNo = string.Empty;
            bool ret = false;
            lock (_sameEQMap)
            {
                if (_sameEQMap.ContainsKey(CSTSeqNo) && _sameEQMap[CSTSeqNo].ContainsKey(StepID))
                {
                    NodeNo = _sameEQMap[CSTSeqNo][StepID];
                    ret = true;
                }
            }
            return ret;
        }

        /// <summary>
        /// 紀錄Job在Put Step時進入過Node
        /// </summary>
        /// <param name="CSTSeqNo"></param>
        /// <param name="StepID"></param>
        /// <param name="NodeNo"></param>
        public void AddToMap(string CSTSeqNo, int StepID, string NodeNo)
        {
            lock (_sameEQMap)
            {
                SerializableDictionary<int, string> tmp = null;
                if (_sameEQMap.ContainsKey(CSTSeqNo))
                    tmp = _sameEQMap[CSTSeqNo];
                else
                {
                    tmp = new SerializableDictionary<int, string>();
                    _sameEQMap.Add(CSTSeqNo, tmp);
                }

                if (tmp.ContainsKey(StepID))
                    tmp[StepID] = NodeNo;
                else
                    tmp.Add(StepID, NodeNo);
            }
        }

        /// <summary>
        /// 清除SameEQMap紀錄
        /// </summary>
        /// <param name="CSTSeqNo"></param>
        public bool RemoveFromMap(string CSTSeqNo)
        {
            bool ret = false;
            lock (_sameEQMap)
            {
                if (_sameEQMap.ContainsKey(CSTSeqNo))
                {
                    _sameEQMap.Remove(CSTSeqNo);
                    ret = true;
                }
            }
            return ret;
        }
        #endregion

        //20151208 mark 改到RealTime區域不需要存File
        /// <summary>Robot Arm Signal Substrate 目前資訊 20151208後改為紀錄Log用不列入運算且不用存檔
        /// 
        /// </summary>
        //private RobotArmSignalSubstrateInfo[] _armSignalSubstrateInfoList;

        /// <summary> Robot Arm Signal Substrate 所有Arm的目前資訊 20151208後改為紀錄Log用不列入運算且不用存檔
        ///
        /// </summary>
        //public RobotArmSignalSubstrateInfo[] ArmSignalSubstrateInfoList
        //{
        //    get { return _armSignalSubstrateInfoList; }
        //    set { _armSignalSubstrateInfoList = value; }
        //}

        //20151208 mark 改到RealTime區域不需要存File
        /// <summary>Robot Arm Double Substrate 目前資訊 20151208後改為紀錄Log用不列入運算且不用存檔
        /// 
        /// </summary>
        //private RobotArmDoubleSubstrateInfo[] _armDoubleSubstrateInfoList;

        /// <summary> Robot Arm Double Substrate 所有Arm的目前資訊 20151208後改為紀錄Log用不列入運算且不用存檔
        ///
        /// </summary>
        //public RobotArmDoubleSubstrateInfo[] ArmDoubleSubstrateInfoList
        //{
        //    get { return _armDoubleSubstrateInfoList; }
        //    set { _armDoubleSubstrateInfoList = value; }
        //}

        private eRobotStatus _status = eRobotStatus.UNKNOWN;

        /// <summary> Robot Current Status
        ///
        /// </summary>
        public eRobotStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }

        private eRobotHasCommandStatus _robotHasCommandstatus = eRobotHasCommandStatus.NO_COMMAND_ON_ROBOT;

        /// <summary> Robot has Command Status
        ///
        /// </summary>
        public eRobotHasCommandStatus RobotHasCommandstatus
        {
            get { return _robotHasCommandstatus; }
            set { _robotHasCommandstatus = value; }
        }

        private bool _robotControlCommandEQPReplyBitFlag = false;

        /// <summary> Monitor Robot Control Command EQP Reply Bit
        ///
        /// </summary>
        public bool RobotControlCommandEQPReplyBitFlag
        {
            get { return _robotControlCommandEQPReplyBitFlag; }
            set { _robotControlCommandEQPReplyBitFlag = value; }
        }

        private string _curRobotRunMode = eRobot_RunMode.SEMI_MODE;

        /// <summary> 目前Robot的Opeation Mode(SEMI/AUTO)
        ///
        /// </summary>
        public string curRobotRunMode
        {
            get { return _curRobotRunMode; }
            set { _curRobotRunMode = value; }
        }



        private string _curRobotHoldStatus = eRobot_HoldStatus.RELEASE_STATUS;

        /// <summary> 目前Robot的Hold Status(0:Release/1:Hold)
        ///
        /// </summary>
        public string CurRobotHoldStatus
        {
            get { return _curRobotHoldStatus; }
            set { _curRobotHoldStatus = value; }
        }

        string _curRobotPosition = "00"; //00:Robot Home Location

        /// <summary> 目前Robot的位置(00表示Home點位)
        ///
        /// </summary>
        public string CurRobotPosition
        {
            get { return _curRobotPosition; }
            set { _curRobotPosition = value; }
        }

        //現在正在抽片的種類及還沒抽完的數量
        private CVDProportionalRule _curCVDProportionalRule;
        public CVDProportionalRule CurCVDProportionalRule
        {
            get { return _curCVDProportionalRule; }
            set { _curCVDProportionalRule = value; }
        }
        private string _curELAEQPType="L45";
        public string CurELAEQPType
        {
            get { return _curELAEQPType; }
            set { _curELAEQPType = value; }
        }
        private string _curELAEQPChangeflag = "Y";
        public string CurELAEQPChangeflag
        {
            get { return _curELAEQPChangeflag; }
            set { _curELAEQPChangeflag = value; }
        }
        //全部的抽片比例資料
        private SerializableDictionary<eCVDIndexRunMode, int> _cVDProportionalRule = new SerializableDictionary<eCVDIndexRunMode, int>();


        public SerializableDictionary<eCVDIndexRunMode, int> CVDProportionalRule
        {
            get
            {
                if (_cVDProportionalRule == null)
                {
                    _cVDProportionalRule = new SerializableDictionary<eCVDIndexRunMode, int>();
                }
                return _cVDProportionalRule;
            }
            set { _cVDProportionalRule = value; }
        }

        //20151104 add for DRY line / DRY Equipment
        /// <summary>
        /// 紀錄目前 Robot 上次放進去的stage
        /// </summary>
        private int _dryLastEnterStageId = 0;
        public int DRYLastEnterStageID
        {
            get { return _dryLastEnterStageId; }
            set { _dryLastEnterStageId = value; }
        }

        ////20160105之後版本不適用
        //private SerializableDictionary<int, string> _curMQCDefaultInspPriority = new SerializableDictionary<int, string>();

        ///// <summary>設定MQC Default 進入Inspection EQP的優先順序. 序號, NodeNo //20160105之後版本不適用
        ///// 
        ///// </summary>
        //public SerializableDictionary<int, string> CurMQCDefaultInspPriority
        //{
        //    get
        //    {

        //        if (_curMQCDefaultInspPriority == null)
        //        {
        //            _curMQCDefaultInspPriority = new SerializableDictionary<int, string>();
        //        }

        //        return _curMQCDefaultInspPriority;

        //    }

        //    set { _curMQCDefaultInspPriority = value; }

        //}

        //20160105 add for By Port設定MQC Priority
        private SerializableDictionary<string, string> _curMQCPortDefaultInspPriority = new SerializableDictionary<string, string>();

        /// <summary>byPort設定MQC Default 進入Inspection EQP的優先順序. PortNo , (序號, NodeNo)
        /// 
        /// </summary>
        public SerializableDictionary<string, string> CurMQCPortDefaultInspPriority
        {
            get
            {

                if (_curMQCPortDefaultInspPriority == null)
                {
                    _curMQCPortDefaultInspPriority = new SerializableDictionary<string, string>();
                }

                return _curMQCPortDefaultInspPriority;

            }

            set { _curMQCPortDefaultInspPriority = value; }

        }

        private List<string> _dryProcessTypes = new List<string>();
        public List<string> DryProcessTypes
        {
            get { return _dryProcessTypes; }
            set { _dryProcessTypes = value; }
        }

        private string _dryLastProcessType = string.Empty;
        public string DryLastProcessType
        {
            get { return _dryLastProcessType; }
            set { _dryLastProcessType = value; }
        }
        private string _dryLastProcessUnitNo = string.Empty;
        public string DryLastProcessUnitNo
        {
            get { return _dryLastProcessUnitNo; }
            set { _dryLastProcessUnitNo = value; }
        }
        private string _curProcessPortNo = string.Empty;
        public string InProcessingPortNo
        {
            get { return _curProcessPortNo; }
            set { _curProcessPortNo = value; }
        }

        private int _dryCycleCnt = 0;
        public int DryCycleCnt
        {
            get { return _dryCycleCnt; }
            set { _dryCycleCnt = value; }
        }


        private DateTime _lastPreFetchReturnDateTime = DateTime.MinValue;
        public DateTime LastPreFetchReturnDateTime 
        {
            get { return _lastPreFetchReturnDateTime; }
            set { _lastPreFetchReturnDateTime = value; }
        }

        //20160301 add for Array Only 紀錄Robot最後取到的RecipeGroupNo(在Arm Load Job時更新)
        /// <summary>for Array Only 紀錄Robot最後取到的RecipeGroupNo(在Arm Load Job時更新)
        /// 
        /// </summary>
        private string _curFetchOutJobRecipeGroupNo = string.Empty;
        public string CurFetchOutJobRecipeGroupNo
        {
            get { return _curFetchOutJobRecipeGroupNo; }
            set { _curFetchOutJobRecipeGroupNo = value; }
        }

        #region [ 20150709 mark old entity ]

        ////RB Wip Info
        //private eRobotStatus _status = eRobotStatus.UNKNOWN;

        //public eRobotStatus Status
        //{
        //    get { return _status; }
        //    set { _status = value; }
        //}

        //#region RB Arm Info

        //private string _curRBUpArmCSTSeq="0";
        //private string _curRBUpArmJobSeq="0";
        //private eGlassExist _curRBUpArmExist = eGlassExist.Unknown;
        //private string _curRBLowArmCSTSeq = "0";
        //private string _curRBLowArmJobSeq = "0";
        //private eGlassExist _curRBLowArmExist = eGlassExist.Unknown;

        //public string curRBUpArmCSTSeq
        //{
        //    get { return _curRBUpArmCSTSeq; }
        //    set { _curRBUpArmCSTSeq = value; }
        //}

        //public string curRBUpArmJobSeq
        //{
        //    get { return _curRBUpArmJobSeq; }
        //    set { _curRBUpArmJobSeq = value; }

        //}

        //public eGlassExist curRBUpArmExist
        //{
        //    get { return _curRBUpArmExist; }
        //    set { _curRBUpArmExist = value; }
        //}

        //public string curRBLowArmCSTSeq
        //{
        //    get { return _curRBLowArmCSTSeq; }
        //    set { _curRBLowArmCSTSeq = value; }

        //}

        //public string curRBLowArmJobSeq
        //{
        //    get { return _curRBLowArmJobSeq; }
        //    set { _curRBLowArmJobSeq = value; }

        //}

        //public eGlassExist curRBLowArmExist
        //{
        //    get { return _curRBLowArmExist; }
        //    set { _curRBLowArmExist = value; }
        //}

        //#endregion

        ////20141021 add for Robot Command Info
        //#region Robot Cmd Info

        ////private RobotCmdInfo curRobotCmd = new RobotCmdInfo();
        //private RobotCmdInfo _curRobotCmd = new RobotCmdInfo();

        //public RobotCmdInfo curRobotCmd
        //{
        //    get { return _curRobotCmd; }
        //    set { _curRobotCmd = value; }
        //}

        //private string _curRobotRunMode = eRobot_RunMode.SEMI_MODE;

        //public string curRobotRunMode
        //{
        //    get { return _curRobotRunMode; }
        //    set { _curRobotRunMode = value; }
        //}

        ////20141201 add for Check Robot Can Get Stage Glass When ArmHasGlass
        //private bool _allowRobotMoveWithJobFlag = false;

        ///// <summary>
        ///// 允許Robot帶片去處理其他Stage 的Glass
        ///// </summary>
        //public bool AllowRobotMoveWithJobFlag
        //{
        //    get { return _allowRobotMoveWithJobFlag; }
        //    set { _allowRobotMoveWithJobFlag = value; }
        //}

        ////20141205 add for PRM Special
        //private string _curPRMProcJob_JobKey  =string.Empty;

        ///// <summary>
        ///// CELL PRM Special:Robot紀錄目前送入到PRM的JobKey ,當該JOB結束處理時清除
        ///// </summary>
        //public string CurPRMProcJob_JobKey
        //{
        //    get { return _curPRMProcJob_JobKey; }
        //    set { _curPRMProcJob_JobKey = value; }
        //}

        //private string _curPRMProcJob_CstSeq  ="0";

        ///// <summary>
        ///// CELL PRM Special:Robot紀錄目前送入到PRM的CSTSeq ,當該JOB結束處理時清除
        ///// </summary>
        //public string CurPRMProcJob_CstSeq
        //{
        //    get { return _curPRMProcJob_CstSeq; }
        //    set { _curPRMProcJob_CstSeq = value; }
        //}

        //private string _curPRMProcJob_JobSeq  ="0";

        ///// <summary>
        ///// CELL PRM Special:Robot紀錄目前送入到PRM的JobSeq ,當該JOB結束處理時清除
        ///// </summary>
        //public string CurPRMProcJob_JobSeq
        //{
        //    get { return _curPRMProcJob_JobSeq; }
        //    set { _curPRMProcJob_JobSeq = value; }
        //}


        ////20141224 add for SOR Special
        //private string _curSORServiceSubRunMode = string.Empty;

        //public string CurSORServiceSubRunMode
        //{
        //    get { return _curSORServiceSubRunMode; }
        //    set { _curSORServiceSubRunMode = value; }
        //}

        ////20150301 add for 防止Robot不會上報Result造成RobotCmd卡住不下的問題
        //private bool _idleClearRobotCmdStatus = false;

        ///// <summary>
        ///// Robot Status Idle(after Running) will can clear Robot Command Status to Send Next Robot Command
        ///// </summary>
        //public bool IdleClearRobotCmdStatus
        //{
        //    get { return _idleClearRobotCmdStatus; }
        //    set { _idleClearRobotCmdStatus = value; }
        //}

        ////20150323 add for Robot CheckFail Error Dir
        ///// <summary>
        ///// 紀錄目前Robot CheckFail的ErrorCode ,ErrorMsg
        ///// </summary>
        //private Dictionary<string, string> _robotCheckFailMsg = new Dictionary<string, string>();

        //public Dictionary<string, string> RobotCheckFailMsg
        //{
        //    get
        //    {
        //        if (_robotCheckFailMsg == null)
        //        {
        //            _robotCheckFailMsg = new Dictionary<string, string>();
        //        }
        //        return _robotCheckFailMsg;
        //    }
        //    set { _robotCheckFailMsg = value; }
        //}

        ////20150325 add for Get Virtual Port Mode by CBSOR_1
        //private int _virtualPortMode = 0;

        ///// <summary>
        ///// Virtual Port Mode for LineType CBSOR_1 NotUse = 0, NormalMode = 1, LDVirtualPortMode = 2, ULDVirtualPortMode = 3
        ///// </summary>
        //public int VirtualPortMode
        //{
        //    get { return _virtualPortMode; }
        //    set { _virtualPortMode = value; }
        //}

        ////20150325 add for Get Force VCR Mode Enable
        //private int _forceVCRReadingMode = 0;

        ///// <summary>
        ///// Force VCR Reading Enable Mode for LineType CBSOR_1 and CBSOR_2 ,Enable = 1, Disable = 2
        ///// </summary>
        //public int ForceVCRReadingMode
        //{
        //    get { return _forceVCRReadingMode; }
        //    set { _forceVCRReadingMode = value; }
        //}

        ////20150508 add for Get RobotCmdReplyBitFalg
        //private bool _robotCmdReplyBitFlag = false;

        ///// <summary>
        ///// Monitor Robot cmd Reply Bit
        ///// </summary>
        //public bool RobotCmdReplyBitFlag
        //{
        //    get { return _robotCmdReplyBitFlag; }
        //    set { _robotCmdReplyBitFlag = value; }
        //}

        //#endregion

        #endregion
        private string _dryRealTimeChamberMode=string.Empty;
        public string DryRealTimeChamberMode {
            get { return _dryRealTimeChamberMode; }
            set { _dryRealTimeChamberMode = value; }
        }
        private string dryLastChamberMode=string.Empty;
        public string DryLastChamberMode {
            get {return  dryLastChamberMode; }
            set { dryLastChamberMode = value; }
        }

        //add for auto replace BCS Version, by yang 2017/4/18
        private bool _cmdSendCondition = false;
        public bool CmdSendCondition
        {
            get { return _cmdSendCondition; }
            set { _cmdSendCondition = value; }
        }
    }

    #region [ 20150709 mark old entity ]

    //[Serializable]
    //public class RobotCmdInfo
    //{
    //    private string _rbCmd_1;
    //    private string _rbArmSelect_1;
    //    private string _rbTargetSlotNo_1;
    //    private string _rbTargetPos_1;
    //    private string _rbCmd_2;
    //    private string _rbArmSelect_2;
    //    private string _rbTargetSlotNo_2;
    //    private string _rbTargetPos_2;

    //    public string rbCmd_1
    //    {
    //        get { return _rbCmd_1; }
    //        set { _rbCmd_1 = value; }
    //    }

    //    public string rbArmSelect_1
    //    {
    //        get { return _rbArmSelect_1; }
    //        set { _rbArmSelect_1 = value; }
    //    }

    //    public string rbTargetSlotNo_1
    //    {
    //        get { return _rbTargetSlotNo_1; }
    //        set { _rbTargetSlotNo_1 = value; }
    //    }

    //    public string rbTargetPos_1
    //    {
    //        get { return _rbTargetPos_1; }
    //        set { _rbTargetPos_1 = value; }
    //    }

    //    public string rbCmd_2
    //    {
    //        get { return _rbCmd_2; }
    //        set { _rbCmd_2 = value; }
    //    }

    //    public string rbArmSelect_2
    //    {
    //        get { return _rbArmSelect_2; }
    //        set { _rbArmSelect_2 = value; }
    //    }

    //    public string rbTargetSlotNo_2
    //    {
    //        get { return _rbTargetSlotNo_2; }
    //        set { _rbTargetSlotNo_2 = value; }
    //    }

    //    public string rbTargetPos_2
    //    {
    //        get { return _rbTargetPos_2; }
    //        set { _rbTargetPos_2 = value; }
    //    }

    //    //20141022 add for Cmd Result
    //    private string _rbCmdResult_1;
    //    private string _rbCmdResult_2;
    //    private string _rbCmdResult_CurPos;

    //    public string rbCmdResult_1
    //    {
    //        get { return _rbCmdResult_1; }
    //        set { _rbCmdResult_1 = value; }
    //    }

    //    public string rbCmdResult_2
    //    {
    //        get { return _rbCmdResult_2; }
    //        set { _rbCmdResult_2 = value; }
    //    }

    //    public string rbCmdResult_CurPos
    //    {
    //        get { return _rbCmdResult_CurPos; }
    //        set { _rbCmdResult_CurPos = value; }
    //    }

    //    private string _rbCmd_Status =eRobot_CmdStatus.EMPTY;

    //    public string rbCmd_Status
    //    {
    //        get { return _rbCmd_Status; }
    //        set { _rbCmd_Status = value; }
    //    }

    //    //20141105 add for Key ArmJob Key for Mapping
    //    private string _rbCmd1_JobKey;
    //    private string _rbCmd2_JobKey;

    //    //20150121 add for Keep ArmJob CSTSeq and JobSeq
    //    private string _rbCmd1_CSTSeq;
    //    private string _rbCmd1_JobSeq;
    //    private string _rbCmd2_CSTSeq;
    //    private string _rbCmd2_JobSeq;

    //    public string RBCmd1_CSTSeq
    //    {
    //        get { return _rbCmd1_CSTSeq; }
    //        set { _rbCmd1_CSTSeq = value; }
    //    }

    //    public string RBCmd1_JobSeq
    //    {
    //        get { return _rbCmd1_JobSeq; }
    //        set { _rbCmd1_JobSeq = value; }
    //    }

    //    public string RBCmd2_CSTSeq
    //    {
    //        get { return _rbCmd2_CSTSeq; }
    //        set { _rbCmd2_CSTSeq = value; }
    //    }

    //    public string RBCmd2_JobSeq
    //    {
    //        get { return _rbCmd2_JobSeq; }
    //        set { _rbCmd2_JobSeq = value; }
    //    }
        
    //    /// <summary>
    //    /// 記錄下Robot Cmd時對應Cmd1上的JobKey
    //    /// </summary>
    //    public string RBCmd1_JobKey
    //    {
    //        get { return _rbCmd1_JobKey; }
    //        set { _rbCmd1_JobKey = value; }
    //    }

    //    /// <summary>
    //    /// 記錄下Robot Cmd時對應Cmd2上的JobKey
    //    /// </summary>
    //    public string RBCmd2_JobKey
    //    {
    //        get { return _rbCmd2_JobKey; }
    //        set { _rbCmd2_JobKey = value; }
    //    }

    //    //20141209 add CmdCreateDateTime
    //    private DateTime _cmdCreateDateTime = DateTime.Now;

    //    public DateTime CmdCreateDateTime
    //    {
    //        get { return _cmdCreateDateTime; }
    //        set { _cmdCreateDateTime = value; }
    //    }

    //    //20141210 add CmdStatusChangeDateTime
    //    private DateTime _cmdStatusChangeDateTime = DateTime.Now;

    //    public DateTime CmdStatusChangeDateTime
    //    {
    //        get { return _cmdStatusChangeDateTime; }
    //        set { _cmdStatusChangeDateTime = value; }
    //    }

    //    //20141210 add RBCmd  EQ Reply Messsage
    //    private string _cmdEQReply =string.Empty;

    //    public string CmdEQReply
    //    {
    //        get { return _cmdEQReply; }
    //        set { _cmdEQReply = value; }
    //    }

    //    //20150107 add for RBCmd Create Mode
    //    private string _cmdCreateMode = string.Empty;

    //    /// <summary>
    //    /// Robot Create時的RunMode
    //    /// </summary>
    //    public string CmdCreateMode
    //    {
    //        get { return _cmdCreateMode; }
    //        set { _cmdCreateMode = value; }
    //    }

    //    public RobotCmdInfo()
    //    {
    //        //Robot Control Command
    //        _rbCmd_1 = string.Empty;
    //        _rbArmSelect_1 = string.Empty;
    //        _rbTargetPos_1 = string.Empty;
    //        _rbTargetSlotNo_1 = string.Empty;
    //        _rbCmd_2 = string.Empty;
    //        _rbArmSelect_2 = string.Empty;
    //        _rbTargetPos_2 = string.Empty;
    //        _rbTargetSlotNo_2 = string.Empty;

    //        //Robot Command Result
    //        _rbCmdResult_1 = string.Empty;
    //        _rbCmdResult_2 = string.Empty;
    //        _rbCmdResult_CurPos = string.Empty;

    //        //Cur Robot Command Status
    //        _rbCmd_Status = string.Empty;

    //        //Cur Cmd1 &2 Jobkey
    //        _rbCmd1_JobKey = string.Empty;
    //        _rbCmd2_JobKey = string.Empty;

    //        _rbCmd1_CSTSeq = "0";
    //        _rbCmd1_JobSeq = "0";
    //        _rbCmd2_CSTSeq = "0";
    //        _rbCmd2_JobSeq = "0";
    //    }

    //}

    #endregion

    [Serializable]
    public class RobotCmdInfo
    {
        private int _cmd01_Command;
        private int _cmd01_ArmSelect;
        private int _cmd01_TargetSlotNo;
        private int _cmd01_TargetPosition;
        private int _cmd02_Command;
        private int _cmd02_ArmSelect;
        private int _cmd02_TargetSlotNo;
        private int _cmd02_TargetPosition;

        public int Cmd01_Command
        {
            get { return _cmd01_Command; }
            set { _cmd01_Command = value; }
        }

        public int Cmd01_ArmSelect
        {
            get { return _cmd01_ArmSelect; }
            set { _cmd01_ArmSelect = value; }
        }

        public int Cmd01_TargetSlotNo
        {
            get { return _cmd01_TargetSlotNo; }
            set { _cmd01_TargetSlotNo = value; }
        }

        public int Cmd01_TargetPosition
        {
            get { return _cmd01_TargetPosition; }
            set { _cmd01_TargetPosition = value; }
        }

        public int Cmd02_Command
        {
            get { return _cmd02_Command; }
            set { _cmd02_Command = value; }
        }

        public int Cmd02_ArmSelect
        {
            get { return _cmd02_ArmSelect; }
            set { _cmd02_ArmSelect = value; }
        }

        public int Cmd02_TargetSlotNo
        {
            get { return _cmd02_TargetSlotNo; }
            set { _cmd02_TargetSlotNo = value; }
        }

        public int Cmd02_TargetPosition
        {
            get { return _cmd02_TargetPosition; }
            set { _cmd02_TargetPosition = value; }
        }

        //20141022 add for Cmd Result
        private int _cmdResult01;
        private int _cmdResult02;
        private int _cmdResult_CurPosition;

        public int CmdResult01
        {
            get { return _cmdResult01; }
            set { _cmdResult01 = value; }
        }

        public int CmdResult02
        {
            get { return _cmdResult02; }
            set { _cmdResult02 = value; }
        }

        public int CmdResult_CurPosition
        {
            get { return _cmdResult_CurPosition; }
            set { _cmdResult_CurPosition = value; }
        }

        private string _curRobotCommandStatus = eRobot_ControlCommandStatus.EMPTY;

        public string CurRobotCommandStatus
        {
            get { return _curRobotCommandStatus; }
            set { _curRobotCommandStatus = value; }
        }


        private string _cmd01_JobKey;
        private string _cmd02_JobKey;

        /// <summary>
        /// 記錄下Robot Cmd時對應Cmd1上的JobKey
        /// </summary>
        public string Cmd01_JobKey
        {
            get { return _cmd01_JobKey; }
            set { _cmd01_JobKey = value; }
        }

        /// <summary>
        /// 記錄下Robot Cmd時對應Cmd2上的JobKey
        /// </summary>
        public string Cmd02_JobKey
        {
            get { return _cmd02_JobKey; }
            set { _cmd02_JobKey = value; }
        }

        //20150121 add for Keep ArmJob CSTSeq and JobSeq
        private int _cmd01_CSTSeq;
        private int _cmd01_JobSeq;
        private int _cmd02_CSTSeq;
        private int _cmd02_JobSeq;

        public int Cmd01_CSTSeq
        {
            get { return _cmd01_CSTSeq; }
            set { _cmd01_CSTSeq = value; }
        }

        public int Cmd01_JobSeq
        {
            get { return _cmd01_JobSeq; }
            set { _cmd01_JobSeq = value; }
        }

        public int Cmd02_CSTSeq
        {
            get { return _cmd02_CSTSeq; }
            set { _cmd02_CSTSeq = value; }
        }

        public int Cmd02_JobSeq
        {
            get { return _cmd02_JobSeq; }
            set { _cmd02_JobSeq = value; }
        }



        //20141209 add CmdCreateDateTime
        private DateTime _cmdCreateDateTime = DateTime.Now;

        public DateTime CmdCreateDateTime
        {
            get { return _cmdCreateDateTime; }
            set { _cmdCreateDateTime = value; }
        }

        //20141210 add CmdStatusChangeDateTime
        private DateTime _cmdStatusChangeDateTime = DateTime.Now;

        public DateTime CmdStatusChangeDateTime
        {
            get { return _cmdStatusChangeDateTime; }
            set { _cmdStatusChangeDateTime = value; }
        }

        //20141210 add RBCmd  EQ Reply Messsage
        private string _cmdEQReply = string.Empty;

        public string CmdEQReply
        {
            get { return _cmdEQReply; }
            set { _cmdEQReply = value; }
        }

        //20150107 add for RBCmd Create Mode
        private string _cmdCreateMode = string.Empty;

        /// <summary>
        /// Robot Create時的RunMode
        /// </summary>
        public string CmdCreateMode
        {
            get { return _cmdCreateMode; }
            set { _cmdCreateMode = value; }
        }

        public RobotCmdInfo()
        {
            //Robot Control Command

            _cmd01_Command = 0;
            _cmd01_ArmSelect =0;
            _cmd01_TargetSlotNo = 0;
            _cmd01_TargetPosition = 0;
            _cmd02_Command = 0;
            _cmd02_ArmSelect = 0;
            _cmd02_TargetSlotNo = 0;
            _cmd02_TargetPosition = 0;

            //Robot Command Result
            _cmdResult01 = 0;
            _cmdResult02 = 0;
            _cmdResult_CurPosition = 0;

            //Cur Robot Command Status
            _curRobotCommandStatus = string.Empty;

            //Cur Cmd1 &2 Jobkey
            _cmd01_JobKey = string.Empty;
            _cmd02_JobKey = string.Empty;

            _cmd01_CSTSeq = 0;
            _cmd01_JobSeq = 0;
            _cmd02_CSTSeq = 0;
            _cmd02_JobSeq = 0;
        }
       
    }

    [Serializable]
    public class CellSpecialRobotCmdInfo
    {
        #region Command
        private int _cmd01_Command;
        private int _cmd01_ArmSelect;
        private int _cmd01_TargetSlotNo;
        private int _cmd01_TargetPosition;
        private int _cmd02_Command;
        private int _cmd02_ArmSelect;
        private int _cmd02_TargetSlotNo;
        private int _cmd02_TargetPosition;
        private int _cmd03_Command;
        private int _cmd03_ArmSelect;
        private int _cmd03_TargetSlotNo;
        private int _cmd03_TargetPosition;
        private int _cmd04_Command;
        private int _cmd04_ArmSelect;
        private int _cmd04_TargetSlotNo;
        private int _cmd04_TargetPosition;

        public int Cmd01_Command
        {
            get { return _cmd01_Command; }
            set { _cmd01_Command = value; }
        }

        public int Cmd01_ArmSelect
        {
            get { return _cmd01_ArmSelect; }
            set { _cmd01_ArmSelect = value; }
        }

        public int Cmd01_TargetSlotNo
        {
            get { return _cmd01_TargetSlotNo; }
            set { _cmd01_TargetSlotNo = value; }
        }

        public int Cmd01_TargetPosition
        {
            get { return _cmd01_TargetPosition; }
            set { _cmd01_TargetPosition = value; }
        }

        public int Cmd02_Command
        {
            get { return _cmd02_Command; }
            set { _cmd02_Command = value; }
        }

        public int Cmd02_ArmSelect
        {
            get { return _cmd02_ArmSelect; }
            set { _cmd02_ArmSelect = value; }
        }

        public int Cmd02_TargetSlotNo
        {
            get { return _cmd02_TargetSlotNo; }
            set { _cmd02_TargetSlotNo = value; }
        }

        public int Cmd02_TargetPosition
        {
            get { return _cmd02_TargetPosition; }
            set { _cmd02_TargetPosition = value; }
        }

        public int Cmd03_Command
        {
            get { return _cmd03_Command; }
            set { _cmd03_Command = value; }
        }

        public int Cmd03_ArmSelect
        {
            get { return _cmd03_ArmSelect; }
            set { _cmd03_ArmSelect = value; }
        }

        public int Cmd03_TargetSlotNo
        {
            get { return _cmd03_TargetSlotNo; }
            set { _cmd03_TargetSlotNo = value; }
        }

        public int Cmd03_TargetPosition
        {
            get { return _cmd03_TargetPosition; }
            set { _cmd03_TargetPosition = value; }
        }

        public int Cmd04_Command
        {
            get { return _cmd04_Command; }
            set { _cmd04_Command = value; }
        }

        public int Cmd04_ArmSelect
        {
            get { return _cmd04_ArmSelect; }
            set { _cmd04_ArmSelect = value; }
        }

        public int Cmd04_TargetSlotNo
        {
            get { return _cmd04_TargetSlotNo; }
            set { _cmd04_TargetSlotNo = value; }
        }

        public int Cmd04_TargetPosition
        {
            get { return _cmd04_TargetPosition; }
            set { _cmd04_TargetPosition = value; }
        }
        #endregion

        #region Result

        private int _cmdResult01;
        private int _cmdResult02;
        private int _cmdResult03;
        private int _cmdResult04;
        private int _cmdResult_CurPosition;

        public int CmdResult01
        {
            get { return _cmdResult01; }
            set { _cmdResult01 = value; }
        }

        public int CmdResult02
        {
            get { return _cmdResult02; }
            set { _cmdResult02 = value; }
        }

        public int CmdResult03
        {
            get { return _cmdResult03; }
            set { _cmdResult03 = value; }
        }

        public int CmdResult04
        {
            get { return _cmdResult04; }
            set { _cmdResult04 = value; }
        }

        public int CmdResult_CurPosition
        {
            get { return _cmdResult_CurPosition; }
            set { _cmdResult_CurPosition = value; }
        }

        private string _curRobotCommandStatus = eRobot_ControlCommandStatus.EMPTY;

        public string CurRobotCommandStatus
        {
            get { return _curRobotCommandStatus; }
            set { _curRobotCommandStatus = value; }
        }

        #endregion

        #region JobKey
        private string _cmd01_FrontJobKey;
        private string _cmd01_BackJobKey;
        private string _cmd02_FrontJobKey;
        private string _cmd02_BackJobKey;
        private string _cmd03_FrontJobKey;
        private string _cmd03_BackJobKey;
        private string _cmd04_FrontJobKey;
        private string _cmd04_BackJobKey;

        /// <summary>
        /// 記錄下Robot Cmd時對應Cmd1上的FrontJobKey
        /// </summary>
        public string Cmd01_FrontJobKey
        {
            get { return _cmd01_FrontJobKey; }
            set { _cmd01_FrontJobKey = value; }
        }

        /// <summary>
        /// 記錄下Robot Cmd時對應Cmd1上的BackJobKey
        /// </summary>
        public string Cmd01_BackJobKey
        {
            get { return _cmd01_BackJobKey; }
            set { _cmd01_BackJobKey = value; }
        }

        /// <summary>
        /// 記錄下Robot Cmd時對應Cmd2上的FrontJobKey
        /// </summary>
        public string Cmd02_FrontJobKey
        {
            get { return _cmd02_FrontJobKey; }
            set { _cmd02_FrontJobKey = value; }
        }

        /// <summary>
        /// 記錄下Robot Cmd時對應Cmd2上的BackJobKey
        /// </summary>
        public string Cmd02_BackJobKey
        {
            get { return _cmd02_BackJobKey; }
            set { _cmd02_BackJobKey = value; }
        }

        /// <summary>
        /// 記錄下Robot Cmd時對應Cmd3上的FrontJobKey
        /// </summary>
        public string Cmd03_FrontJobKey
        {
            get { return _cmd03_FrontJobKey; }
            set { _cmd03_FrontJobKey = value; }
        }

        /// <summary>
        /// 記錄下Robot Cmd時對應Cmd3上的BackJobKey
        /// </summary>
        public string Cmd03_BackJobKey
        {
            get { return _cmd03_BackJobKey; }
            set { _cmd03_BackJobKey = value; }
        }

        /// <summary>
        /// 記錄下Robot Cmd時對應Cmd4上的FrontJobKey
        /// </summary>
        public string Cmd04_FrontJobKey
        {
            get { return _cmd04_FrontJobKey; }
            set { _cmd04_FrontJobKey = value; }
        }

        /// <summary>
        /// 記錄下Robot Cmd時對應Cmd4上的BackJobKey
        /// </summary>
        public string Cmd04_BackJobKey
        {
            get { return _cmd04_BackJobKey; }
            set { _cmd04_BackJobKey = value; }
        }
        #endregion

        #region ArmJob CSTSeq and JobSeq

        private int _cmd01_FrontCSTSeq;
        private int _cmd01_BackCSTSeq;
        private int _cmd01_FrontJobSeq;
        private int _cmd01_BackJobSeq;
        private int _cmd02_FrontCSTSeq;
        private int _cmd02_BackCSTSeq;
        private int _cmd02_FrontJobSeq;
        private int _cmd02_BackJobSeq;
        private int _cmd03_FrontCSTSeq;
        private int _cmd03_BackCSTSeq;
        private int _cmd03_FrontJobSeq;
        private int _cmd03_BackJobSeq;
        private int _cmd04_FrontCSTSeq;
        private int _cmd04_BackCSTSeq;
        private int _cmd04_FrontJobSeq;
        private int _cmd04_BackJobSeq;

        public int Cmd01_FrontCSTSeq
        {
            get { return _cmd01_FrontCSTSeq; }
            set { _cmd01_FrontCSTSeq = value; }
        }

        public int Cmd01_BackCSTSeq
        {
            get { return _cmd01_BackCSTSeq; }
            set { _cmd01_BackCSTSeq = value; }
        }

        public int Cmd01_FrontJobSeq
        {
            get { return _cmd01_FrontJobSeq; }
            set { _cmd01_FrontJobSeq = value; }
        }

        public int Cmd01_BackJobSeq
        {
            get { return _cmd01_BackJobSeq; }
            set { _cmd01_BackJobSeq = value; }
        }

        public int Cmd02_FrontCSTSeq
        {
            get { return _cmd02_FrontCSTSeq; }
            set { _cmd02_FrontCSTSeq = value; }
        }

        public int Cmd02_BackCSTSeq
        {
            get { return _cmd02_BackCSTSeq; }
            set { _cmd02_BackCSTSeq = value; }
        }

        public int Cmd02_FrontJobSeq
        {
            get { return _cmd02_FrontJobSeq; }
            set { _cmd02_FrontJobSeq = value; }
        }

        public int Cmd02_BackJobSeq
        {
            get { return _cmd02_BackJobSeq; }
            set { _cmd02_BackJobSeq = value; }
        }

        public int Cmd03_FrontCSTSeq
        {
            get { return _cmd03_FrontCSTSeq; }
            set { _cmd03_FrontCSTSeq = value; }
        }

        public int Cmd03_BackCSTSeq
        {
            get { return _cmd03_BackCSTSeq; }
            set { _cmd03_BackCSTSeq = value; }
        }

        public int Cmd03_FrontJobSeq
        {
            get { return _cmd03_FrontJobSeq; }
            set { _cmd03_FrontJobSeq = value; }
        }

        public int Cmd03_BackJobSeq
        {
            get { return _cmd03_BackJobSeq; }
            set { _cmd03_BackJobSeq = value; }
        }

        public int Cmd04_FrontCSTSeq
        {
            get { return _cmd04_FrontCSTSeq; }
            set { _cmd04_FrontCSTSeq = value; }
        }

        public int Cmd04_BackCSTSeq
        {
            get { return _cmd04_BackCSTSeq; }
            set { _cmd04_BackCSTSeq = value; }
        }

        public int Cmd04_FrontJobSeq
        {
            get { return _cmd04_FrontJobSeq; }
            set { _cmd04_FrontJobSeq = value; }
        }

        public int Cmd04_BackJobSeq
        {
            get { return _cmd04_BackJobSeq; }
            set { _cmd04_BackJobSeq = value; }
        }

        #endregion

        //CmdCreateDateTime
        private DateTime _cmdCreateDateTime = DateTime.Now;

        public DateTime CmdCreateDateTime
        {
            get { return _cmdCreateDateTime; }
            set { _cmdCreateDateTime = value; }
        }

        //CmdStatusChangeDateTime
        private DateTime _cmdStatusChangeDateTime = DateTime.Now;

        public DateTime CmdStatusChangeDateTime
        {
            get { return _cmdStatusChangeDateTime; }
            set { _cmdStatusChangeDateTime = value; }
        }

        //RBCmd  EQ Reply Messsage
        private string _cmdEQReply = string.Empty;

        public string CmdEQReply
        {
            get { return _cmdEQReply; }
            set { _cmdEQReply = value; }
        }

        //RBCmd Create Mode
        private string _cmdCreateMode = string.Empty;

        /// <summary>
        /// Robot Create時的RunMode
        /// </summary>
        public string CmdCreateMode
        {
            get { return _cmdCreateMode; }
            set { _cmdCreateMode = value; }
        }

        public CellSpecialRobotCmdInfo()
        {
            //Robot Control Command

            _cmd01_Command =0;
            _cmd01_ArmSelect = 0;
            _cmd01_TargetSlotNo = 0;
            _cmd01_TargetPosition = 0;
            _cmd02_Command = 0;
            _cmd02_ArmSelect = 0;
            _cmd02_TargetSlotNo = 0;
            _cmd02_TargetPosition = 0;
            _cmd03_Command = 0;
            _cmd03_ArmSelect = 0;
            _cmd03_TargetSlotNo = 0;
            _cmd03_TargetPosition = 0;
            _cmd04_Command = 0;
            _cmd04_ArmSelect = 0;
            _cmd04_TargetSlotNo = 0;
            _cmd04_TargetPosition = 0;
            //Robot Command Result
            _cmdResult01 = 0;
            _cmdResult02 = 0;
            _cmdResult03 = 0;
            _cmdResult04 = 0;
            _cmdResult_CurPosition = 0;

            //Cur Robot Command Status
            _curRobotCommandStatus = string.Empty;

            //Cur Cmd1 &2 Jobkey
            _cmd01_FrontJobKey = string.Empty;
            _cmd01_BackJobKey = string.Empty;
            _cmd02_FrontJobKey = string.Empty;
            _cmd02_BackJobKey = string.Empty;
            _cmd03_FrontJobKey = string.Empty;
            _cmd03_BackJobKey = string.Empty;
            _cmd04_FrontJobKey = string.Empty;
            _cmd04_BackJobKey = string.Empty;

            _cmd01_FrontCSTSeq = 0;
            _cmd01_BackCSTSeq = 0;
            _cmd01_FrontJobSeq = 0;
            _cmd01_BackJobSeq = 0;
            _cmd02_FrontCSTSeq = 0;
            _cmd02_BackCSTSeq = 0;
            _cmd02_FrontJobSeq = 0;
            _cmd02_BackJobSeq = 0;
            _cmd03_FrontCSTSeq = 0;
            _cmd03_BackCSTSeq = 0;
            _cmd03_FrontJobSeq = 0;
            _cmd03_BackJobSeq = 0;
            _cmd04_FrontCSTSeq = 0;
            _cmd04_BackCSTSeq = 0;
            _cmd04_FrontJobSeq = 0;
            _cmd04_BackJobSeq = 0;
        }
    }

    [Serializable]
    public class RobotArmSignalSubstrateInfo
    {

        private string _armCSTSeq;
        private string _armJobSeq;
        private eGlassExist _armJobExist;
        private eArmDisableStatus _armDisableFlag;
        private int _curRptArmJobExistDisableInfo;

        public string ArmCSTSeq
        {
            get { return _armCSTSeq; }
            set { _armCSTSeq = value; }
        }

        public string ArmJobSeq
        {
            get { return _armJobSeq; }
            set { _armJobSeq = value; }
        }

        public eGlassExist ArmJobExist
        {
            get { return _armJobExist; }
            set { _armJobExist = value; }
        }

        public eArmDisableStatus ArmDisableFlag
        {
            get { return _armDisableFlag; }
            set { _armDisableFlag = value; }
        }

        public int CurRptArmJobExistDisableInfo
        {
            get { return _curRptArmJobExistDisableInfo; }
            set { _curRptArmJobExistDisableInfo = value; }
        }

        public RobotArmSignalSubstrateInfo()
        {
            ArmCSTSeq = "0";
            ArmJobSeq = "0";
            ArmJobExist = eGlassExist.NoExist;
            ArmDisableFlag = eArmDisableStatus.Enable;
            CurRptArmJobExistDisableInfo = 0;
        }
    }

    [Serializable]
    public class RobotArmDoubleSubstrateInfo
    {

        private string _armFrontCSTSeq;
        private string _armFrontJobSeq;
        private eGlassExist _armFrontJobExist;
        private string _armBackCSTSeq;
        private string _armBackJobSeq;
        private eGlassExist _armBackJobExist;
        //20151208 add for Arm Enable/Disable
        private eArmDisableStatus _armDisableFlag;
        private int _curRptArmFrontJobExistDisableInfo;
        private int _curRptArmBackJobExistDisableInfo;

        public string ArmFrontCSTSeq
        {
            get { return _armFrontCSTSeq; }
            set { _armFrontCSTSeq = value; }
        }

        public string ArmFrontJobSeq
        {
            get { return _armFrontJobSeq; }
            set { _armFrontJobSeq = value; }
        }

        public eGlassExist ArmFrontJobExist
        {
            get { return _armFrontJobExist; }
            set { _armFrontJobExist = value; }
        }

        public string ArmBackCSTSeq
        {
            get { return _armBackCSTSeq; }
            set { _armBackCSTSeq = value; }
        }

        public string ArmBackJobSeq
        {
            get { return _armBackJobSeq; }
            set { _armBackJobSeq = value; }
        }

        public eGlassExist ArmBackJobExist
        {
            get { return _armBackJobExist; }
            set { _armBackJobExist = value; }
        }

        public eArmDisableStatus ArmDisableFlag
        {
            get { return _armDisableFlag; }
            set { _armDisableFlag = value; }
        }

        public int CurRptArmFrontJobExistDisableInfo
        {
            get { return _curRptArmFrontJobExistDisableInfo; }
            set { _curRptArmFrontJobExistDisableInfo = value; }
        }

        public int CurRptArmBackJobExistDisableInfo
        {
            get { return _curRptArmBackJobExistDisableInfo; }
            set { _curRptArmBackJobExistDisableInfo = value; }
        }

        public RobotArmDoubleSubstrateInfo()
        {
            ArmFrontCSTSeq = "0";
            ArmFrontJobSeq = "0";
            ArmFrontJobExist = eGlassExist.NoExist;
            CurRptArmFrontJobExistDisableInfo = 0;
            ArmBackCSTSeq = "0";
            ArmBackJobSeq = "0";
            ArmBackJobExist = eGlassExist.NoExist;
            CurRptArmBackJobExistDisableInfo = 0;

            ArmDisableFlag = eArmDisableStatus.Enable;

        }

    }

    public class Robot : Entity
    {
        public RobotEntityData Data { get; private set; }

        public RobotEntityFile File { get; private set; }

        public Robot(RobotEntityData data, RobotEntityFile file)
        {
            Data = data;
            File = file;
        }

        #region [ 即時運算變數,不需要存入檔案 ] ===========================================================================================================================================

        private Dictionary<string, string> _checkFailMessageList = new Dictionary<string, string>();

        /// <summary> 記錄目前Robot CheckFail的Message List(ErrorCode ,ErrorMsg)
        ///
        /// </summary>
        public Dictionary<string, string> CheckFailMessageList
        {
            get
            {
                if (_checkFailMessageList == null)
                {
                    _checkFailMessageList = new Dictionary<string, string>();
                }
                return _checkFailMessageList;
            }
            set { _checkFailMessageList = value; }
        }

        //private string _curControlCommand_Status = eRobot_ControlCommandStatus.EMPTY;

        ///// <summary> 記錄目前Robot Control Command的Status來決定是否可以下Command
        /////
        ///// </summary>
        //public string CurControlCommand_Status
        //{
        //    get { return _curControlCommand_Status; }
        //    set { _curControlCommand_Status = value; }
        //}

        private RobotCmdInfo _curRealTimeSetCommandInfo = new RobotCmdInfo();

        /// <summary> 紀錄經過計算之後產生的Robot Control Command, 每當判斷可下Command後會清空,以便後續邏輯來定義預期要給EQP的Command
        /// 
        /// </summary>
        public RobotCmdInfo CurRealTimeSetCommandInfo
        {
            get { return _curRealTimeSetCommandInfo; }
            set { _curRealTimeSetCommandInfo = value; }
        }

        private bool _robotStatusChangeFlag = false;

        /// <summary> Robot Status Change時的Flag.當Status Change時:detail Log 顯示
        /// 
        /// </summary>
        public bool RobotStatusChangeFlag
        {
            get { return _robotStatusChangeFlag; }
            set { _robotStatusChangeFlag = value; }
        }

        //20151016 add AutoModeStartTime
        private DateTime _autoModeStartDateTime = DateTime.Now;

        public DateTime AutoModeStartDateTime
        {
            get { return _autoModeStartDateTime; }
            set { _autoModeStartDateTime = value; }
        }


        private IRobotContext _context;

        /// <summary>
        /// Robot  Runtime Context
        /// </summary>
        public IRobotContext Context {
            get { return _context; }
            set { _context = value; }
        }

        //20151125 add for Get Eqp Report CST Fetch SeqMode
        /// <summary>Get Eqp Report CST Fetch SeqMode 1  : ASC(Lower to Upper)  2:DESC(Upper to Lower)
        /// 
        /// </summary>
        private string _eqpRptCSTFetchSeqMode = string.Empty;

        public string EqpRptCSTFetchSeqMode
        {
            get { return _eqpRptCSTFetchSeqMode; }
            set { _eqpRptCSTFetchSeqMode = value; }
        }

        //20151202 add for Cell Special Arm
        private CellSpecialRobotCmdInfo _curCellSpecialRealTimeSetCommandInfo = new CellSpecialRobotCmdInfo();

        /// <summary>for Cell Special紀錄經過計算之後產生的Robot Control Command, 每當判斷可下Command後會清空,以便後續邏輯來定義預期要給EQP的Command
        /// 
        /// </summary>
        public CellSpecialRobotCmdInfo CurCellSpecialRealTimeSetCommandInfo
        {
            get { return _curCellSpecialRealTimeSetCommandInfo; }
            set { _curCellSpecialRealTimeSetCommandInfo = value; }
        }

        //20151208 add for 紀錄即時Arm上資訊
        /// <summary>紀錄目前Robot內部運算用的Arm上資訊for Normal(1Arm1Job)
        /// 
        /// </summary>
        public RobotArmSignalSubstrateInfo[] CurTempArmSingleJobInfoList = new RobotArmSignalSubstrateInfo[2];

        /// <summary>紀錄目前Robot內部運算用的Arm上資訊for Cell Special(1Arm2Job)
        /// 
        /// </summary>
        public RobotArmDoubleSubstrateInfo[] CurTempArmDoubleJobInfoList = new RobotArmDoubleSubstrateInfo[4];

        /// <summary>紀錄目前RobotArm上即時資訊for Normal(1Arm1Job)
        /// 
        /// </summary>
        public RobotArmSignalSubstrateInfo[] CurRealTimeArmSingleJobInfoList = new RobotArmSignalSubstrateInfo[2];

        /// <summary>紀錄目前RobotArm上即時資訊for Cell Special(1Arm2Job)
        /// 
        /// </summary>
        public RobotArmDoubleSubstrateInfo[] CurRealTimeArmDoubleJobInfoList = new RobotArmDoubleSubstrateInfo[4];

        /// <summary>紀錄目前Port上即時Route資訊 (for CF MQC TTP 卡One Flow Cassette command)
        /// 
        /// </summary>
        public Dictionary<string, string> CurPortRouteIDInfo = new Dictionary<string, string>();

        public string Cur_CFMQCTTP_Flow_Route = string.Empty;

        //20160624
        private int _onArmPutReadyFlag = 0;
        /// <summary>
        /// 在Arm上時,PutReady紀錄的Flag,與curBcsJob.RobotWIP.PutReadyFlag(用在Arm沒片,Cmd02做PutReady使用)
        /// </summary>
        public int OnArmPutReadyFlag
        {
            get { return _onArmPutReadyFlag; }
            set { _onArmPutReadyFlag = value; }
        }
        //20160624
        private string _onArmPutReady_StageID = string.Empty;
        /// <summary>
        ///  在Arm上時,PutReady紀錄的StageID,與curBcsJob.RobotWIP.PutReady_StageID(用在Arm沒片,Cmd02做PutReady使用)
        /// </summary>
        public string OnArmPutReady_StageID
        {
            get { return _onArmPutReady_StageID; }
            set { _onArmPutReady_StageID = value; }
        }

        //20160629
        private string _moveToArm = string.Empty;
        /// <summary>
        /// 紀錄Robot Arm取Job是從port or EQP
        /// </summary>
        public string MoveToArm
        {
            get { return _moveToArm; }
            set { _moveToArm = value; }
        }

        public int MixNo = 1;//Yang

        public bool ReCheck = false;//Yang

        public bool CLNRTCWIP = false ;//Yang       

        public bool noSendToCLN = false; //yang

        public bool fetchforRTC = false; //yang 2017/5/25

        private string _waitglass = string.Empty;   //yang 2017/3/13

        public string WaitGlass  
        {
            get{return _waitglass;}
            set{_waitglass=value;}
        }

        private string _waitglasstimespan = string.Empty;  //yang 2017/3/13

        public string WaitGlassTimeSpan
        {
            get{return _waitglasstimespan;}
            set{_waitglasstimespan=value;}
        }
      /*  private string _roboterrcode = string.Empty;
        public string RobotErrCode       //20170217 add by yang for APPErrorSendToBMS
        {
            get { return _roboterrcode; }
            set { _roboterrcode = value; }
        }
        private string _roboterrmsg = string.Empty;
        public string RobotErrMsg       //20170217 add by yang for APPErrorSendToBMS
        {
            get { return _roboterrmsg; }
            set { _roboterrmsg = value; }
        }
        public string errorStatus = string.Empty;
        */

        private Dictionary<string, Tuple<string, string, string, string>> _checkErrorList = new Dictionary<string, Tuple<string, string, string, string>>();

        /// <summary> 存相同ErrCode的优先级最高的glassID(因为出也是要先出他)
        ///记录下一轮Scan的ErrorList  Dic( ErrCode, (ErrMsg, GlassID, ErrStatus, OccurService) ) ,  by yang
        /// </summary>
        public Dictionary<string, Tuple<string, string, string, string>> CheckErrorList
        {
            get
            {
                if (_checkErrorList == null)
                {
                    _checkErrorList = new Dictionary<string, Tuple<string, string, string, string>>();
                }
                return _checkErrorList;
            }
            set { _checkErrorList = value; }
        }
     

        #endregion ========================================================================================================================================================================

    }

    public class DefineNormalRobotCmd
    {
        private int _cmd01_Command;
        private int _cmd01_ArmSelect;
        private int _cmd01_TargetSlotNo;
        private int _cmd01_TargetPosition;
        private string _cmd01_DBRobotAction;
        private string _cmd01_DBUseArm;
        private string _cmd01_DBStageIDList;
        private string _cmd01_CstSeq;
        private string _cmd01_JobSeq;
        //private int _cmd02_Command;
        //private int _cmd02_ArmSelect;
        //private int _cmd02_TargetSlotNo;
        //private int _cmd02_TargetPosition;
        //private string _cmd02_DBRobotAction;
        //private string _cmd02_DBUseArm;
        //private string _cmd02_DBStageIDList;

        public int Cmd01_Command
        {
            get { return _cmd01_Command; }
            set { _cmd01_Command = value; }
        }

        public int Cmd01_ArmSelect
        {
            get { return _cmd01_ArmSelect; }
            set { _cmd01_ArmSelect = value; }
        }

        public int Cmd01_TargetSlotNo
        {
            get { return _cmd01_TargetSlotNo; }
            set { _cmd01_TargetSlotNo = value; }
        }

        public int Cmd01_TargetPosition
        {
            get { return _cmd01_TargetPosition; }
            set { _cmd01_TargetPosition = value; }
        }

        public string Cmd01_DBRobotAction
        {
            get { return _cmd01_DBRobotAction; }
            set { _cmd01_DBRobotAction = value; }
        }

        public string Cmd01_DBUseArm
        {
            get { return _cmd01_DBUseArm; }
            set { _cmd01_DBUseArm = value; }
        }

        public string Cmd01_DBStageIDList
        {
            get { return _cmd01_DBStageIDList; }
            set { _cmd01_DBStageIDList = value; }
        }

        public string Cmd01_CstSeq
        {
            get { return _cmd01_CstSeq; }
            set { _cmd01_CstSeq = value; }
        }

        public string Cmd01_JobSeq
        {
            get { return _cmd01_JobSeq; }
            set { _cmd01_JobSeq = value; }
        }

        //public int Cmd02_Command
        //{
        //    get { return _cmd02_Command; }
        //    set { _cmd02_Command = value; }
        //}

        //public int Cmd02_ArmSelect
        //{
        //    get { return _cmd02_ArmSelect; }
        //    set { _cmd02_ArmSelect = value; }
        //}

        //public int Cmd02_TargetSlotNo
        //{
        //    get { return _cmd02_TargetSlotNo; }
        //    set { _cmd02_TargetSlotNo = value; }
        //}

        //public int Cmd02_TargetPosition
        //{
        //    get { return _cmd02_TargetPosition; }
        //    set { _cmd02_TargetPosition = value; }
        //}

        //public string Cmd02_DBRobotAction
        //{
        //    get { return _cmd02_DBRobotAction; }
        //    set { _cmd02_DBRobotAction = value; }
        //}

        //public string Cmd02_DBUseArm
        //{
        //    get { return _cmd02_DBUseArm; }
        //    set { _cmd02_DBUseArm = value; }
        //}

        //public string Cmd02_DBStageIDList
        //{
        //    get { return _cmd02_DBStageIDList; }
        //    set { _cmd02_DBStageIDList = value; }
        //}

        public DefineNormalRobotCmd()
        {
            //Robot Control Command
            _cmd01_Command = 0;
            _cmd01_ArmSelect =0;
            _cmd01_TargetSlotNo = 0;
            _cmd01_TargetPosition = 0;
            _cmd01_DBRobotAction = string.Empty;
            _cmd01_DBUseArm = string.Empty;
            _cmd01_DBStageIDList = string.Empty;
            _cmd01_CstSeq = string.Empty;
            _cmd01_JobSeq = string.Empty;
            //_cmd02_Command = 0;
            //_cmd02_ArmSelect = 0;
            //_cmd02_TargetSlotNo = 0;
            //_cmd02_TargetPosition = 0;
            //_cmd02_DBRobotAction = string.Empty;
            //_cmd02_DBUseArm = string.Empty;
            //_cmd02_DBStageIDList = string.Empty;

        }
    }

    public class JobStageSelectInfo
    {
        private int _curStepNo;
        private int _afterStageSelect_StepNo;

        public int CurStepNo
        {
            get { return _curStepNo; }
            set { _curStepNo = value; }
        }

        public int AfterStageSelect_StepNo
        {
            get { return _afterStageSelect_StepNo; }
            set { _afterStageSelect_StepNo = value; }
        }

    }

    //20150923 add for CVD Fetch Glass Proportional Command
    /// <summary>
    /// 紀錄目前Robot Fetch Glass Proportional Rule的Type ,Count
    /// </summary>
    [Serializable]
    public class CVDProportionalRule
    {
        public eCVDIndexRunMode curProportionalType;
        public int curProportionalMQCCount;
        public int curProportionalPRODCount;
        public int curPorportionalPROD1Count;//modify by hujunpeng 20190425 for CVD700新增一个product进行混run逻辑，Deng，20190823
        public eCVDIndexRunMode FirstProportionaltype;
    }

    //20151216 Modify for Cell Special Use
    public class RobotCanControlSlotBlockInfo
    {
        private int _curBlock_RobotCmdSlotNo;

        /// <summary>
        /// 紀錄目前Block所在地的RobotCmdSlotNo
        /// </summary>
        public int CurBlock_RobotCmdSlotNo
        {
            get { return _curBlock_RobotCmdSlotNo; }
            set { _curBlock_RobotCmdSlotNo = value; }
        }

        private int _curBlock_StepID = 0;

        /// <summary>
        /// Block目前的StepID
        /// </summary>
        public int CurBlock_StepID
        {
            get { return _curBlock_StepID; }
            set { _curBlock_StepID = value; }
        }

        private string _curBlock_Location_StageID;       

        /// <summary>
        /// 紀錄目前Block所在地的StageID
        /// </summary>
        public string CurBlock_Location_StageID
        {
            get { return _curBlock_Location_StageID; }
            set { _curBlock_Location_StageID = value; }
        }

        private string _curBlock_Location_StagePriority;

        /// <summary>
        /// 紀錄目前Block所在地的StagePriority
        /// 兩碼長, "01"~"99", "99"表示ROBOT_HOME_STAGE的Priority
        /// </summary>
        public string CurBlock_Location_StagePriority
        {
            get { return _curBlock_Location_StagePriority; }
            set { _curBlock_Location_StagePriority = value; }
        }

        private string _curBlock_PortCstStatusPriority;

        /// <summary>
        /// 紀錄目前Block如果是在Port上的Status Priority . InProc > Wait For Process
        /// </summary>
        public string CurBlock_PortCstStatusPriority
        {
            get { return _curBlock_PortCstStatusPriority; }
            set { _curBlock_PortCstStatusPriority = value; }
        }

        //20151227 add 紀錄目前SlotBlock可控Job片數狀態
        private string _curBlock_JobExistStatus = eRobot_SlotBlock_JobsExistStatus.FRONT_BACK_EMPTY;

        /// <summary>紀錄目前SlotBlock可控Job片數狀態
        /// 
        /// </summary>
        public string CurBlock_JobExistStatus
        {
            get { return _curBlock_JobExistStatus; }
            set { _curBlock_JobExistStatus = value; }

        }

        //紀錄目前Block內可控制的JobList
        public List<Job> CurBlockCanControlJobList;

        public RobotCanControlSlotBlockInfo()
        {

            CurBlockCanControlJobList = new List<Job>(); 
        }

        //20160119 add for 紀錄Block Stage Type
        private string _curBlock_Location_StageType;

        /// <summary>
        /// 紀錄目前Block所在地的Stage Type
        /// </summary>
        public string CurBlock_Location_StageType
        {
            get { return _curBlock_Location_StageType; }
            set { _curBlock_Location_StageType = value; }
        }      


    }

}
