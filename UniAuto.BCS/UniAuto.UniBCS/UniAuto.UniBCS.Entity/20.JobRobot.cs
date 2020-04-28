using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using UniAuto.UniBCS.MISC;

namespace UniAuto.UniBCS.Entity
{

    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class JobRobot : ICloneable
    {
        public object Clone()
        {
            JobRobot robotWip = (JobRobot)this.MemberwiseClone();
            return robotWip;
        }

        //20160615
        private JobCfSpecial _cfSpecial = new JobCfSpecial();

        public JobCfSpecial CfSpecial
        {
            get { return _cfSpecial; }
            set { _cfSpecial = value; }
        }


        private string _curLocation_StageType;

        /// <summary>
        /// 紀錄目前JOB所在地的StageType
        /// </summary>
        public string CurLocation_StageType
        {
            get { return _curLocation_StageType; }
            set { _curLocation_StageType = value; }
        }

        private string _curLocation_StageID;
        private string _curLocation_StagePriority;

        /// <summary>
        /// 紀錄目前JOB所在地的StageID
        /// </summary>
        public string CurLocation_StageID
        {
            get { return _curLocation_StageID; }
            set { _curLocation_StageID = value; }
        }

        /// <summary>
        /// 紀錄目前JOB所在地的StagePriority
        /// 兩碼長, "01"~"99", "99"表示ROBOT_HOME_STAGE的Priority
        /// </summary>
        public string CurLocation_StagePriority
        {
            get { return _curLocation_StagePriority; }
            set { _curLocation_StagePriority = value; }
        }

        private int _curLocation_SlotNo;

        /// <summary>
        /// 紀錄目前JOB所在地的SlotNo
        /// </summary>
        public int CurLocation_SlotNo
        {
            get { return _curLocation_SlotNo; }
            set { _curLocation_SlotNo = value; }
        }

        private string _curPortCstStatusPriority;

        private string _CFRepairPriority;// Added by zhangwei 20161010

        private string _curSendOutJobJudge;

        /// <summary>
        /// 記錄在Job UDRQ當下的Job Judge,在LinkSignal(Equipment)Type時要以SendOut上報的即時資訊為主,不可用WIP內資訊
        /// </summary>
        public string CurSendOutJobJudge
        {
            get { return _curSendOutJobJudge; }
            set { _curSendOutJobJudge = value; }
        }

        /// <summary>
        /// 紀錄目前JOB如果是在Port上的Status Priority .
        /// </summary>
        public string CurPortCstStatusPriority
        {
            get { return _curPortCstStatusPriority; }
            set { _curPortCstStatusPriority = value; }
        }
        public string RepairPriority
        {
            get { return _CFRepairPriority; }
            set { _CFRepairPriority = value; }
        }

        private SerializableDictionary<int, RobotRouteStep> _robotRouteStepList;
        private SerializableDictionary<int, RobotRouteStep> _dailycheckRouteStepList;
        private SerializableDictionary<int, RobotRouteStep> _normalRobotRouteStepList;

        [ReadOnly(true)]
        public SerializableDictionary<int, RobotRouteStep> RobotRouteStepList
        {
            get { return _robotRouteStepList; }
            set { _robotRouteStepList = value; }
        }

        //[ReadOnly(true)]
        ///public SerializableDictionary<int, RobotRouteStep> DailyCheckRouteStepList
        //{
        //    get { return _dailycheckRouteStepList; }
        //    set { _dailycheckRouteStepList = value; }
        //}

        //[ReadOnly(true)]
        //public SerializableDictionary<int, RobotRouteStep> NormalRobotCheckRouteStepList
        //{
        //    get { return _normalRobotRouteStepList; }
        //    set { _normalRobotRouteStepList = value; }
        //}

        private string _eqpReport_linkSignalSendOutEQPFLAG;
        /// <summary>
        /// 紀錄目前JOB 最新的EQPFlag
        /// </summary>
        public string EqpReport_linkSignalSendOutEQPFLAG
        {
            get { return _eqpReport_linkSignalSendOutEQPFLAG; }
            set { _eqpReport_linkSignalSendOutEQPFLAG = value; }
        }

        private string _eqpReport_linkSignalSendOutEQPReservations;
        /// <summary>
        /// 紀錄目前CF JOB 最新的EQPReservations
        /// </summary>
        public string EqpReport_linkSignalSendOutEQPRESERVATIONS
        {
            get { return _eqpReport_linkSignalSendOutEQPReservations; }
            set { _eqpReport_linkSignalSendOutEQPReservations = value; }
        }

        private int _curStepNo = 0;

        /// <summary>
        /// Job目前的StepNo
        /// </summary>
        public int CurStepNo
        {
            get { return _curStepNo; }
            set { _curStepNo = value; }
        }

        private int _nextStepNo = 0;

        /// <summary> Job下一步預計的StepNo
        ///
        /// </summary>
        public int NextStepNo
        {
            get { return _nextStepNo; }
            set { _nextStepNo = value; }
        }

        private string _routeProcessStatus;

        /// <summary>
        /// 目前Job Step的Process Status
        /// </summary>
        public string RouteProcessStatus
        {
            get { return _routeProcessStatus; }
            set { _routeProcessStatus = value; }
        }

        private SerializableDictionary<string, string> _CheckFailMessageList = new SerializableDictionary<string, string>();

        /// <summary>
        /// 紀錄Robot目前無法搬送的資訊
        /// </summary>
        [ReadOnly(true)]
        public SerializableDictionary<string, string> CheckFailMessageList
        {
            get
            {
                if (_CheckFailMessageList == null)
                {
                    _CheckFailMessageList = new SerializableDictionary<string, string>();
                }
                return _CheckFailMessageList;
            }
            set { _CheckFailMessageList = value; }
        }

        private string _lastInPutTrackingData;

        /// <summary>
        /// Job記錄最近已經進入Stage後的TrackingData資訊
        /// </summary>
        public string LastInPutTrackingData
        {
            get { return _lastInPutTrackingData; }
            set { _lastInPutTrackingData = value; }
        }

        private string _eqpReport_LinkSignalSendOutTrackingData;

        /// <summary>
        /// Job記錄最近準備從Stage SendOut時JobData的TrackingData資訊
        /// </summary>
        public string EqpReport_linkSignalSendOutTrackingData
        {
            get { return _eqpReport_LinkSignalSendOutTrackingData; }
            set { _eqpReport_LinkSignalSendOutTrackingData = value; }
        }

        private string _curRouteID;

        /// <summary>
        /// 下貨時所帶的RouteID
        /// </summary>
        public string CurRouteID
        {
            get { return _curRouteID; }
            set { _curRouteID = value; }
        }

        //20150923 add for CVD Fetch Glass Proportional Command
        /// <summary>
        /// 紀錄目前Robot Fetch Glass Proportional Rule的Type ,Count
        /// </summary>
        private string _cvdFetchProportionalRuleType;
        public string CVDFetchProportionalRuleType
        {
            get { return _cvdFetchProportionalRuleType; }
            set { _cvdFetchProportionalRuleType = value; }
        }

        /// <summary> 如果是因为Pre-Fetch功能从PORT上取出来的基板, 则该基板的该Flag要加1, 每次抽出来就加1... 0代表没做过预取!
        /// 
        /// </summary>
        private int _preFetchFlag = 0;
        public int PreFetchFlag
        {
            get { return _preFetchFlag; }
            set { _preFetchFlag = value; }
        }
        private int _preFetchFlag_Last = 0;
        public int LastPreFetchFlag
        {
            get { return _preFetchFlag_Last; }
            set { _preFetchFlag_Last = value; }
        }


        private DateTime _fetchOutDateTime = DateTime.MinValue;
        public DateTime FetchOutDataTime
        {
            get { return _fetchOutDateTime; }
            set { _fetchOutDateTime = value; }
        }
        //20160525 進入stage的時間
        private DateTime _StoreDateTime = DateTime.MinValue;
        public DateTime StoreDateTime
        {
            get { return _StoreDateTime; }
            set { _StoreDateTime = value; }
        }

        //20160525 紀錄因為EQP RTC Jump後,原本要Put的step
        private int _tempStepNo = 0;
        public int TempStepNo
        {
            get { return _tempStepNo; }
            set { _tempStepNo = value; }
        }

        //20160525 從EQP回插回CST的Job,EQPRTCFlag=1,當作區分在同step做Jump時,避免又下step==81又插回CST
        private bool _eqpRTCFlag = false;
        public bool EQPRTCFlag
        {
            get { return _eqpRTCFlag; }
            set { _eqpRTCFlag = value; }
        }

        /// <summary> 如果是有做RTC需要将该Flag ON起来!!
        /// 
        /// </summary>
        private bool _rtcReworkFlag = false;
        public bool RTCReworkFlag
        {
            get { return _rtcReworkFlag; }
            set { _rtcReworkFlag = value; }
        }

        private int _putReadyFlag = 0;
        public int PutReadyFlag
        {
            get { return _putReadyFlag; }
            set { _putReadyFlag = value; }
        }

        ///记录因为Put Ready而预计要进的Stage ID
        private string _putReady_StageID = string.Empty;
        ///记录因为Put Ready而预计要进的Stage ID
        public string PutReady_StageID
        {
            get { return _putReady_StageID; }
            set { _putReady_StageID = value; }
        }

        //20151124 add for Cell Special 1Arm 2Job 
        private string _curSubLocation = string.Empty;

        /// <summary> 紀錄目前JOB所在地對應RobotArm Front/Back的位置.01為1(Front) 02為2(Back)
        /// 
        /// </summary>
        public string CurSubLocation
        {
            get { return _curSubLocation; }
            set { _curSubLocation = value; }
        }

        private int _curRobotCmdSlotNo = 0;

        /// <summary> 紀錄目前JOB所在對應的Robot Command SlotNo
        /// 
        /// </summary>
        public int CurRobotCmdSlotNo
        {
            get { return _curRobotCmdSlotNo; }
            set { _curRobotCmdSlotNo = value; }
        }

        private DateTime _lastSendOnTime;
        private string _lastSendStageID = "";
        /// <summary> 紀錄目前JOB Link Signal Send on Time, stage id
        /// 
        /// </summary>
        public DateTime LastSendOnTime
        {
            get { return _lastSendOnTime; }
            set { _lastSendOnTime = value; }
        }
        public string LastSendStageID
        {
            get { return _lastSendStageID; }
            set { _lastSendStageID = value; }
        }

        //20151209 add for 是否準備好當CST出片後目的EQ久久不收片時要自動回CST功能 
        /// <summary>是否準備好當CST出片後目的EQ久久不收片時要自動回CST功能.在從CST取出片之後會為READY ,Job在Arm時為START , Other為NotCheck
        /// 
        /// </summary>
        private string _forceReturnCSTWithoutLDRQ_Status = eRobot_FroceRetrunCSTWithoutLDRQStatus.IS_NOTCHECK;

        /// <summary>是否準備好當CST出片後目的EQ久久不收片時要自動回CST功能.在從CST取出片之後會為READY ,Job在Arm時為START , Other為NotCheck
        /// 
        /// </summary>
        public string ForceReturnCSTWithoutLDRQ_Status
        {
            get { return _forceReturnCSTWithoutLDRQ_Status; }
            set { _forceReturnCSTWithoutLDRQ_Status = value; }
        }

        /// <summary>當CST出片後目的EQ久久不收片時要自動回CST功能.開始監控的時間
        /// 
        /// </summary>
        private DateTime _forceReturnCSTWithoutLDRQ_MonitorStartTime;

        /// <summary>當CST出片後目的EQ久久不收片時要自動回CST功能.開始監控的時間
        /// 
        /// </summary>
        public DateTime ForceReturnCSTWithoutLDRQ_MonitorStartTime
        {
            get { return _forceReturnCSTWithoutLDRQ_MonitorStartTime; }
            set { _forceReturnCSTWithoutLDRQ_MonitorStartTime = value; }
        }

        //Watson Add 20151211 For MQC TTP Sub Chamber Processed = (Tracking Data)
        private bool _cfMQCTTPSubChamberProcessedFlag;

        public bool CF_MQCTTP_SubChamberProcessedFlag
        {
            get { return _cfMQCTTPSubChamberProcessedFlag; }
            set { _cfMQCTTPSubChamberProcessedFlag = value; }
        }

        private string _reworkRealCount = string.Empty;
        public string ReworkRealCount
        {
            get { return _reworkRealCount; }
            set { _reworkRealCount = value; }
        }

        [NonSerialized]
        public bool SorterMode_OtherFilterOK = true;// false表示因為 Grade 以外的 Filter 而被過濾掉

        [NonSerialized]
        public bool SorterMode_GradeMatch = true;//false表示因為 Grade Mismatch 而被過濾掉, Filter_PortFetchOutMappingGrade

        //20160217 add Only To Mix Grade Unload Flag
        private bool _onlyToMixGradeULDFlag = false;

        /// <summary> 記錄目前Block因為Front and Back Grade不同只能去Mix Grade的Unload Port Stage
        /// 
        /// </summary>
        public bool OnlyToMixGradeULDFlag
        {
            get { return _onlyToMixGradeULDFlag; }
            set { _onlyToMixGradeULDFlag = value; }
        }

        private string _curSendOutJobGrade = string.Empty;
        public string CurSendOutJobGrade
        {
            get { return _curSendOutJobGrade; }
            set { _curSendOutJobGrade = value; }
        }
        //20160727 在Arm上->當PutReady開啟時,要skip filter的判斷;在stage->要Prefetch,且cmd2是PutReady才skip filter的判斷
        private bool _skipFilterCheck = false;
        public bool SkipFilterCheck
        {
            get { return _skipFilterCheck; }
            set { _skipFilterCheck = value; }
        }
        //20160801 在Arm上->當PutReady開啟時,要skip Orderby的判斷;在stage->要Prefetch,且cmd2是PutReady才skip Orderby的判斷
        private bool _skipOrderbyCheck = false;
        public bool SkipOrderbyCheck
        {
            get { return _skipOrderbyCheck; }
            set { _skipOrderbyCheck = value; }
        }

        //20160802
        private bool _runFilterCheckOK = false;
        public bool RunFilterCheckOK
        {
            get { return _runFilterCheckOK; }
            set { _runFilterCheckOK = value; }
        }
        private bool _runOrderbyCheckOK = false;
        public bool RunOrderbyCheckOK
        {
            get { return _runOrderbyCheckOK; }
            set { _runOrderbyCheckOK = value; }
        }
        private int _processtypepriority ;
        public int dryprocesstypepriority//add by Yang for DRY
        {
            get { return _processtypepriority; }
            set { _processtypepriority = value; }
        }
        private int _ovnOpenTheDoorPriority=0;//add by hujunpeng for OVNITO 20180929
        public int OvnOpenTheDoorPriority
        {
            get { return _ovnOpenTheDoorPriority; }
            set { _ovnOpenTheDoorPriority = value; }
        }


    }
}
