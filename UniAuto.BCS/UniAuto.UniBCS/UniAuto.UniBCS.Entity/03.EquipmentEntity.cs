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
    public class EquipmentEntityFile : EntityFile
    {
        #region Common Data
        private eEQPStatus _status = eEQPStatus.SETUP;
        private eBitResult _cIMMode = eBitResult.OFF;
        private eBitResult _precIMode = eBitResult.Unknown;
        private eBitResult _upstreamInlineMode = eBitResult.OFF;
        private eBitResult _downstreamInlineMode = eBitResult.OFF;
        private eBitResult _localAlarmStatus = eBitResult.OFF;
        private List<eBitResult> _vcrMode = new List<eBitResult>();
        private eEQPOperationMode _equipmentOperationMode = eEQPOperationMode.MANUAL;
        private eSamplingRule _samplingRule = eSamplingRule.ByCount;
        private eCSTOperationMode _cstOperationMode = eCSTOperationMode.CTOC;
        private eWaitCassetteStatus _waitCassetteStatus = eWaitCassetteStatus.UNKNOWN;
        private eEnableDisable _partialFullMode = eEnableDisable.Disable;
        private eEnableDisable _autoRecipeChangeMode = eEnableDisable.Disable;
        private string _unitVCREnableStatus = string.Empty;
        private eBitResult _unitCIMMode01 = eBitResult.OFF;
        private eBitResult _unitCIMMode02 = eBitResult.OFF;
        private eBitResult _unitCIMMode03 = eBitResult.OFF;
        private eBitResult _unitCIMMode04 = eBitResult.OFF;
        private eBitResult _unitCIMMode05 = eBitResult.OFF;


        private eEnableDisable _jobDataCheckMode = eEnableDisable.Disable;
        private eEnableDisable _recipeIDCheckMode = eEnableDisable.Disable;
        private eEnableDisable _productTypeCheckMode = eEnableDisable.Disable;
        private eEnableDisable _groupIndexCheckMode = eEnableDisable.Disable;
        private eEnableDisable _productIDCheckMode = eEnableDisable.Disable;
        private eEnableDisable _jobDuplicateCheckMode = eEnableDisable.Disable;
        private eEnableDisable _cOAVersionCheckMode = eEnableDisable.Disable;
        private eEnableDisable _cassetteSettingCodeCheckMode = eEnableDisable.Disable;
        private eEnableDisable _jobDataSeqNoCheckMode = eEnableDisable.Disable;
        private eEnableDisable _turnAngleCheckMode = eEnableDisable.Disable;
        private eEnableDisable _jobTypeforDummyCheckMode = eEnableDisable.Disable;
        private eRobotFetchSequenceMode _robotFetchSequenceMode = eRobotFetchSequenceMode.FromLowertoUpper;
        private int _samplingCount = 0;
        private string _samplingUnit = string.Empty;
        private string _samplingGroup = string.Empty;
        private DateTime _lastAliveTime = DateTime.Now;
        private bool _aliveTimeout = false;
        private DateTime _apcLastDT = DateTime.Now;
        private int _apcIntervalMS;
        private DateTime _dailyCheckLastDT = DateTime.Now;
        private int _dailyCheckIntervalS;
        private DateTime _energyLastDT = DateTime.Now;
        private string _currentAlarmCode = string.Empty;
        private string _currentRecipeID = string.Empty;
        private string _equipmentRunMode = string.Empty;
        private int _totalTFTCount = 0;
        private int _totalCFProductCount = 0;
        private int _totalDummyCount = 0;
        private int _throughDummyCount = 0;
        private int _thicknessDummyCount = 0;
        private int _uvMASKCount = 0;
        private int _productType = 0;
        private string _mplcInterlockState = string.Empty;
        private string _mesStatus = "DOWN";
        private int _cassetteonPortQTime = 0;
        private int _bufferWarningGlassSettingCount = 0;
        private int _bufferCurrentGlassCount = 0;
        private int _bufferWarning = 0;
        private int _bufferStoreGlassOverAliveTime = 0;
        private string _productID = string.Empty;
        private string _groupIndex = string.Empty;
        private int _inspectionIdleTime = 0;

        private string _finalReceiveGlassID = string.Empty;
        private string _finalReceiveGlassTime = string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now);
        private string _finalELAStageReceiveTime = string.Format("{0:yyyy/MM/dd HH:mm:ss}", DateTime.Now);
        private DateTime  _lastSendOutGlassTime =  DateTime.Now;   //20170721 modify by qiumin forDRY TACTTIME
        private string _finalReceiveGlassProcessType = "";
        private bool _tactTimeOut = false;

        private int  _eqpProcessTimeSetting =0 ; //addd by qiumin 20171219 for setting eq process time 
        private string _lastMaterialID = string.Empty;//20180416 by hujunpeng
        private bool _oxinfoCheckFlag = false;   //20180508 by qiumin for CF REP AND MAC OX CHECK
        private string _lastProductType = string.Empty;//add by hujunpeng 20180523
        private string _lastWarmCount;//add by hujunpeng 20180523
        private string _slot1warmcountflag = string.Empty;//add by hujunpeng 20180601
        private string _slot2warmcountflag = string.Empty;//add by hujunpeng 20180601
        private string _prid1 = string.Empty;//add by hujunpeng 20180904
        private string _prid2 = string.Empty;//add by hujunpeng 20180904
        private string _prid3 = string.Empty;//add by hujunpeng 20180904
        private string _prid4 = string.Empty;//add by hujunpeng 20180904
        private DateTime _r2rEQParameterDownloadDT = DateTime.Now;//add by hujunpeng 20181110
        private DateTime _r2rRecipeReportStartDT = DateTime.Now;//add by hujunpeng 20181110
        private string _r2rEQParameterDownloadRetrunCode = string.Empty;//add by hujunpeng 20181110
        private string _r2rRecipeReportReplyReturnCode = string.Empty;//add by hujunpeng 20181110
        private bool _isReveive = false;//add by hujunpeng 20181110
        private DateTime _Port01ProductCountCommandSendTime = DateTime.Now;    //Add by Yangzhenteng 20181212
        private string _Port01ProductCountCommandReplyJobCount = string.Empty; //Add By Yangzhenteng 20181212
        private bool _Port01ProductCountCommandReplyFlag = false;              //Add By Yangzhenteng 20181212
        private DateTime _Port02ProductCountCommandSendTime = DateTime.Now;    //Add by Yangzhenteng 20181212
        private string _Port02ProductCountCommandReplyJobCount = string.Empty; //Add By Yangzhenteng 20181212
        private bool _Port02ProductCountCommandReplyFlag = false;              //Add By Yangzhenteng 20181212
        private Dictionary<string, Tuple<int, int>> _pijobcount = new Dictionary<string, Tuple<int, int>>(); //add by hujunpeng 20190723

        public Dictionary<string, Tuple<int, int>> PIJobCount
        {
            get { return _pijobcount; }
            set { _pijobcount = value; }
        }
        public DateTime R2REQParameterDownloadDT
        {
            get { return _r2rEQParameterDownloadDT; }
            set { _r2rEQParameterDownloadDT = value; }
        }

        public DateTime R2RRecipeReportStartDT
        {
            get { return _r2rRecipeReportStartDT; }
            set { _r2rRecipeReportStartDT = value; }
        }

        public string R2RRecipeReportReplyReturnCode
        {
            get { return _r2rRecipeReportReplyReturnCode; }
            set { _r2rRecipeReportReplyReturnCode = value; }
        }

        public string R2REQParameterDownloadRetrunCode
        {
            get { return _r2rEQParameterDownloadRetrunCode; }
            set { _r2rEQParameterDownloadRetrunCode = value; }
        }

        public bool IsReveive
        {
            get { return _isReveive; }
            set { _isReveive = value; }
        }
        public bool Port01ProductCountCommandReplyFlag
        {
            get { return _Port01ProductCountCommandReplyFlag; }
            set { _Port01ProductCountCommandReplyFlag = value; }
        }
        public DateTime Port01ProductCountCommandSendTime
        {
            get { return _Port01ProductCountCommandSendTime; }
            set { _Port01ProductCountCommandSendTime = value; }
        }
        public string Port01ProductCountCommandReplyJobCount
        {
            get { return _Port01ProductCountCommandReplyJobCount; }
            set { _Port01ProductCountCommandReplyJobCount = value; }
        }
        public bool Port02ProductCountCommandReplyFlag
        {
            get { return _Port02ProductCountCommandReplyFlag; }
            set { _Port02ProductCountCommandReplyFlag = value; }
        }
        public DateTime Port02ProductCountCommandSendTime
        {
            get { return _Port02ProductCountCommandSendTime; }
            set { _Port02ProductCountCommandSendTime = value; }
        }
        public string Port02ProductCountCommandReplyJobCount
        {
            get { return _Port02ProductCountCommandReplyJobCount; }
            set { _Port02ProductCountCommandReplyJobCount = value; }
        }
        public string PrID1  //Add By Hujunpeng 20180904
        {
            get { return _prid1; }
            set { _prid1 = value; }
        }
        public string PrID2  //Add By Hujunpeng 20180904
        {
            get { return _prid2; }
            set { _prid2 = value; }
        }
        public string PrID3  //Add By Hujunpeng 20180904
        {
            get { return _prid3; }
            set { _prid3 = value; }
        }
        public string PrID4  //Add By Hujunpeng 20180904
        {
            get { return _prid4; }
            set { _prid4 = value; }
        }

        public string Slot2WarmCountFlag
        {
            get { return _slot2warmcountflag; }
            set { _slot2warmcountflag = value; }
        }
        public string Slot1WarmCountFlag
        {
            get { return _slot1warmcountflag; }
            set { _slot1warmcountflag = value; }
        }
        public string LastWarmCount
        {
            get { return _lastWarmCount; }
            set { _lastWarmCount = value; }
        }
        public string LastProductType
        {
            get { return _lastProductType; }
            set { _lastProductType = value; }
        }
        public bool OxinfoCheckFlag
        {
            get { return _oxinfoCheckFlag; }
            set { _oxinfoCheckFlag = value; }
        }

        public string LastMaterialID
        {
            get { return _lastMaterialID; }
            set { _lastMaterialID = value; }
        }
        public int InspectionIdleTime
        {
            get { return _inspectionIdleTime; }
            set { _inspectionIdleTime = value; }
        }

        public string GroupIndex
        {
            get { return _groupIndex; }
            set { _groupIndex = value; }
        }

        public string ProductID
        {
            get { return _productID; }
            set { _productID = value; }
        }

        public eBitResult UnitCIMMode01
        {
            get { return _unitCIMMode01; }
            set { _unitCIMMode01 = value; }
        }
        public eBitResult UnitCIMMode02
        {
            get { return _unitCIMMode02; }
            set { _unitCIMMode02 = value; }
        }
        public eBitResult UnitCIMMode03
        {
            get { return _unitCIMMode03; }
            set { _unitCIMMode03 = value; }
        }
        public eBitResult UnitCIMMode04
        {
            get { return _unitCIMMode04; }
            set { _unitCIMMode04 = value; }
        }
        public eBitResult UnitCIMMode05
        {
            get { return _unitCIMMode05; }
            set { _unitCIMMode05 = value; }
        }

        public string UnitVCREnableStatus
        {
            get { return _unitVCREnableStatus; }
            set { _unitVCREnableStatus = value; }
        }

        public int CassetteonPortQTime
        {
            get { return _cassetteonPortQTime; }
            set { _cassetteonPortQTime = value; }
        }

        public int BufferStoreGlassOverAliveTime
        {
            get { return _bufferStoreGlassOverAliveTime; }
            set { _bufferStoreGlassOverAliveTime = value; }
        }

        public int BufferWarning
        {
            get { return _bufferWarning; }
            set { _bufferWarning = value; }
        }

        public int BufferCurrentGlassCount
        {
            get { return _bufferCurrentGlassCount; }
            set { _bufferCurrentGlassCount = value; }
        }

        public int BufferWarningGlassSettingCount
        {
            get { return _bufferWarningGlassSettingCount; }
            set { _bufferWarningGlassSettingCount = value; }
        }

        public eBitResult CIMMode
        {
            get { return _cIMMode; }
            set { _cIMMode = value; }
        }

        public eBitResult PreCIMMode
        {
            get { return _precIMode; }
            set { _precIMode = value; }
        }

        public eBitResult UpstreamInlineMode
        {
            get { return _upstreamInlineMode; }
            set { _upstreamInlineMode = value; }
        }

        public eBitResult DownstreamInlineMode
        {
            get { return _downstreamInlineMode; }
            set { _downstreamInlineMode = value; }
        }

        public eBitResult LocalAlarmStatus
        {
            get { return _localAlarmStatus; }
            set { _localAlarmStatus = value; }
        }

        public List<eBitResult> VcrMode
        {
            get { return _vcrMode; }
            set { _vcrMode = value; }
        }

        public eEQPOperationMode EquipmentOperationMode
        {
            get { return _equipmentOperationMode; }
            set { _equipmentOperationMode = value; }
        }

        public eSamplingRule SamplingRule
        {
            get { return _samplingRule; }
            set { _samplingRule = value; }
        }

        public eEnableDisable PartialFullMode
        {
            get { return _partialFullMode; }
            set { _partialFullMode = value; }
        }

        public eEnableDisable AutoRecipeChangeMode
        {
            get { return _autoRecipeChangeMode; }
            set { _autoRecipeChangeMode = value; }
        }

        public int SamplingCount
        {
            get { return _samplingCount; }
            set { _samplingCount = value; }
        }

        public string SamplingUnit
        {
            get { return _samplingUnit; }
            set { _samplingUnit = value; }
        }

        public string SamplingGroup
        {
            get { return _samplingGroup; }
            set { _samplingGroup = value; }
        }

        public DateTime DailyCheckLastDT
        {
            get { return _dailyCheckLastDT; }
            set { _dailyCheckLastDT = value; }
        }

        public DateTime EnergyLastDT
        {
            get { return _energyLastDT; }
            set { _energyLastDT = value; }
        }
        private int _energyIntervalS;

        /// <summary>
        /// 能源可视化处理间隔时间
        /// </summary>
        public int EnergyIntervalS
        {
            get { return _energyIntervalS; }
            set { _energyIntervalS = value; }
        }

        /// <summary>
        /// Daily Check处理间隔时间
        /// </summary>
        public int DailyCheckIntervalS
        {
            get { return _dailyCheckIntervalS; }
            set { _dailyCheckIntervalS = value; }
        }

        /// <summary>
        /// APC最后一次Report时间
        /// </summary>
        public DateTime ApcLastDT
        {
            get { return _apcLastDT; }
            set { _apcLastDT = value; }
        }

        /// <summary>
        /// APC Report间隔时间(ms)
        /// </summary>
        public int ApcIntervalMS
        {
            get { return _apcIntervalMS; }
            set { _apcIntervalMS = value; }
        }

        public eEQPStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public DateTime LastAliveTime
        {
            get { return _lastAliveTime; }
            set { _lastAliveTime = value; }
        }
        public bool AliveTimeout
        {
            get { return _aliveTimeout; }
            set { _aliveTimeout = value; }
        }
        //目前EQP IO Report "1：Kind To Kind mode  2：Cassette To Cassette mode 3：Lot To Lot Mode"
        //但是JOB DATA IO is " 0: Kind to Kind   1: CST to CST 2：Lot To Lot"
        public eCSTOperationMode CSTOperationMode
        {
            get { return _cstOperationMode; }
            set { _cstOperationMode = value; }
        }

        public eWaitCassetteStatus WaitCassetteStatus
        {
            get { return _waitCassetteStatus; }
            set { _waitCassetteStatus = value; }
        }

        public string CurrentAlarmCode
        {
            get { return _currentAlarmCode; }
            set { _currentAlarmCode = value; }
        }

        public string CurrentRecipeID
        {
            get { return _currentRecipeID; }
            set { _currentRecipeID = value; }
        }

        public string EquipmentRunMode
        {
            get { return _equipmentRunMode; }
            set { _equipmentRunMode = value; }
        }

        public eEnableDisable JobDataCheckMode
        {
            get { return _jobDataCheckMode; }
            set { _jobDataCheckMode = value; }
        }

        public eEnableDisable RecipeIDCheckMode
        {
            get { return _recipeIDCheckMode; }
            set { _recipeIDCheckMode = value; }
        }

        public eEnableDisable ProductTypeCheckMode
        {
            get { return _productTypeCheckMode; }
            set { _productTypeCheckMode = value; }
        }

        public eEnableDisable GroupIndexCheckMode
        {
            get { return _groupIndexCheckMode; }
            set { _groupIndexCheckMode = value; }
        }

        public eEnableDisable ProductIDCheckMode
        {
            get { return _productIDCheckMode; }
            set { _productIDCheckMode = value; }
        }

        public eEnableDisable JobDuplicateCheckMode
        {
            get { return _jobDuplicateCheckMode; }
            set { _jobDuplicateCheckMode = value; }
        }

        public eEnableDisable COAVersionCheckMode
        {
            get { return _cOAVersionCheckMode; }
            set { _cOAVersionCheckMode = value; }
        }

        public eEnableDisable CassetteSettingCodeCheckMode
        {
            get { return _cassetteSettingCodeCheckMode; }
            set { _cassetteSettingCodeCheckMode = value; }
        }

        public eEnableDisable JobDataSeqNoCheckMode
        {
            get { return _jobDataSeqNoCheckMode; }
            set { _jobDataSeqNoCheckMode = value; }
        }

        public eEnableDisable TurnAngleCheckMode
        {
            get { return _turnAngleCheckMode; }
            set { _turnAngleCheckMode = value; }
        }

        public eEnableDisable JobTypeforDummyCheckMode
        {
            get { return _jobTypeforDummyCheckMode; }
            set { _jobTypeforDummyCheckMode = value; }
        }
        public eRobotFetchSequenceMode RobotFetchSequenceMode
        {
            get { return _robotFetchSequenceMode; }
            set { _robotFetchSequenceMode = value; }
        }

        public int TotalTFTJobCount
        {
            get { return _totalTFTCount; }
            set { _totalTFTCount = value; }
        }

        public int TotalCFProductJobCount
        {
            get { return _totalCFProductCount; }
            set { _totalCFProductCount = value; }
        }

        public int TotalDummyJobCount
        {
            get { return _totalDummyCount; }
            set { _totalDummyCount = value; }
        }

        public int ThroughDummyJobCount
        {
            get { return _throughDummyCount; }
            set { _throughDummyCount = value; }
        }

        public int ThicknessDummyJobCount
        {
            get { return _thicknessDummyCount; }
            set { _thicknessDummyCount = value; }
        }

        public int UVMASKJobCount
        {
            get { return _uvMASKCount; }
            set { _uvMASKCount = value; }
        }

        public int ProductType
        {
            get { return _productType; }
            set { _productType = value; }
        }

        public string MPLCInterlockState
        {
            get { return _mplcInterlockState; }
            set { _mplcInterlockState = value; }
        }

        public string MESStatus
        {
            get { return _mesStatus; }
            set { _mesStatus = value; }
        }

        /// <summary>
        /// 
        /// </summary>
        public string FinalReceiveGlassID
        {
            get { return _finalReceiveGlassID; }
            set { _finalReceiveGlassID = value; }
        }

        public string FinalReceiveGlassTime
        {
            get { return _finalReceiveGlassTime; }
            set { _finalReceiveGlassTime = value; }
        }

        public string FinalELAStageReceiveTime
        {
            get { return _finalELAStageReceiveTime; }
            set { _finalELAStageReceiveTime = value; }
        }

        public string FinalReceiveGlassProcessType
        {
            get { return _finalReceiveGlassProcessType; }
            set { _finalReceiveGlassProcessType = value; }
        }
        public DateTime LastSendOutGlassTime    //20170721 modify by qiumin forDRY TACTTIME
        {
            get { return _lastSendOutGlassTime; }
            set { _lastSendOutGlassTime = value; }
        }


        public bool TactTimeOut
        {
            get { return _tactTimeOut; }
            set { _tactTimeOut = value; }
        }

        public int  EqProcessTimeSetting   //addd by qiumin 20171219 for setting eq process time 
        {
            get { return _eqpProcessTimeSetting; }
            set { _eqpProcessTimeSetting = value; }
        
        }

        #endregion

        #region Array Special
        private bool _materialChange = false;
        private bool _defectReport = false;
        private int _proportionalRule01Type = 0;
        private int _proportionalRule01Value = 0;
        private int _proportionalRule02Type = 0;
        private int _proportionalRule02Value = 0;
        private eBitResult _lineBackupMode = eBitResult.OFF;    // add by bruce 2015/7/23
        private string _lastReceiveGlassCstSqNo=string.Empty ;  //add by qiumin 2018/7/11 Modify start
        private int _phlDelayCount = 0;
        private int _phlHoldCount = 0;
        private int _phlHoldReason = 0;//add by hujunpeng 20190514

        public int PhlHoldReason
        {
            get { return _phlHoldReason;}
            set { _phlHoldReason=value;}
        }

        public int PhlDelayCount
        {
            get { return _phlDelayCount;}
            set { _phlDelayCount = value; }
        }

        public int PhlHoldCount
        {
            get { return _phlHoldCount; }
            set { _phlHoldCount = value; }
        }

        public string LastReceiveGlassCstSqNo
        {
            get {return _lastReceiveGlassCstSqNo; }
            set {_lastReceiveGlassCstSqNo = value; }
        }
        //add by qiumin 2018/7/11  modify end

        /// <summary>
        /// Wet Str Material Changer flag
        /// </summary>
        public bool MaterialChange
        {
            get { return _materialChange; }
            set { _materialChange = value; }
        }

        public bool DefectReport
        {
            get { return _defectReport; }
            set { _defectReport = value; }
        }

        public int ProportionalRule01Type
        {
            get { return _proportionalRule01Type; }
            set { _proportionalRule01Type = value; }
        }

        public int ProportionalRule01Value
        {
            get { return _proportionalRule01Value; }
            set { _proportionalRule01Value = value; }
        }

        public int ProportionalRule02Type
        {
            get { return _proportionalRule02Type; }
            set { _proportionalRule02Type = value; }
        }

        public int ProportionalRule02Value
        {
            get { return _proportionalRule02Value; }
            set { _proportionalRule02Value = value; }
        }

        public eBitResult LineBackupMode    //add by bruce 2015/7/23
        {
            get { return _lineBackupMode; }
            set { _lineBackupMode = value; }
        }
        #endregion

        #region [CF Special]
        private eBitResult _bypassMode = eBitResult.OFF;
        private eBitResult _turnTableMode = eBitResult.OFF;
        private eBitResult _bypassInspectionEquipment01Mode = eBitResult.OFF;
        private eBitResult _bypassInspectionEquipment02Mode = eBitResult.OFF;
        private eHightCVmode _highCVMode = eHightCVmode.UNKNOW;
        private eBitResult _nextLineBCStatus = eBitResult.OFF;
        private eBitResult _cV07Status = eBitResult.OFF;
        private eBitResult _indexerExposureDispatchMode = eBitResult.OFF;
        private eBitResult _reworkForceToUnloaderCST = eBitResult.OFF;
        private eBitResult _vcd01 = eBitResult.OFF;
        private eBitResult _vcd02 = eBitResult.OFF;
        private eBitResult _cp01 = eBitResult.OFF;
        private eBitResult _cp02 = eBitResult.OFF;
        private eBitResult _hp01 = eBitResult.OFF;
        private eBitResult _hp02 = eBitResult.OFF;
        private double _accumulativeValue = 0;
        private int _reworkWashableCount = 0;
        private eEQPStatus _cv01status = eEQPStatus.SETUP;
        private int _cv01productType = 0;
        private eEQPStatus _Cleanerstatus = eEQPStatus.SETUP;
        private int _CleanerproductType = 0;
        private eEQPStatus _cv02status = eEQPStatus.SETUP;
        private int _cv02productType = 0;
        private string _cv06BufferInfo = "0";
        private string _buffer01RWJudgeCapacity = "0";
        private string _buffer02RWJudgeCapacity = "0";
        private string _operatorID = string.Empty;
        private int _okPortModeStoreQTime = 0;
        private ePortModeProductTypeCheck _okPort = ePortModeProductTypeCheck.UNKNOWN;
        private int _ngPortModeStoreQTime = 0;
        private ePortModeProductTypeCheck _ngPort = ePortModeProductTypeCheck.UNKNOWN;
        private string _ngPortJudge = "0";
        private int __pdPortModeStoreQTime = 0;
        private ePortModeProductTypeCheck _pdPort = ePortModeProductTypeCheck.UNKNOWN;
        private int _rpPortModeStoreQTime = 0;
        private ePortModeProductTypeCheck _rpPort = ePortModeProductTypeCheck.UNKNOWN;
        private int _irPortModeStoreQTime = 0;
        private ePortModeProductTypeCheck _irPort = ePortModeProductTypeCheck.UNKNOWN;
        private int _mixPortModeStoreQTime = 0;
        private ePortModeProductTypeCheck _mixPort = ePortModeProductTypeCheck.UNKNOWN;
        private string _mixPortJudge = "0";
        private ePortEnableMode _inlineRework = ePortEnableMode.Unknown;

        public eEQPStatus CV01Status
        {
            get { return _cv01status; }
            set { _cv01status = value; }
        }

        public int CV01ProductType
        {
            get { return _cv01productType; }
            set { _cv01productType = value; }
        }

        public eEQPStatus CleanerStatus
        {
            get { return _Cleanerstatus; }
            set { _Cleanerstatus = value; }
        }

        public int CleanerProductType
        {
            get { return _CleanerproductType; }
            set { _CleanerproductType = value; }
        }

        public eEQPStatus CV02Status
        {
            get { return _cv02status; }
            set { _cv02status = value; }
        }

        public int CV02ProductType
        {
            get { return _cv02productType; }
            set { _cv02productType = value; }
        }

        public int ReworkWashableCount
        {
            get { return _reworkWashableCount; }
            set { _reworkWashableCount = value; }
        }

        public eBitResult BypassMode
        {
            get { return _bypassMode; }
            set { _bypassMode = value; }
        }

        public eBitResult TurnTableMode
        {
            get { return _turnTableMode; }
            set { _turnTableMode = value; }
        }

        public eBitResult BypassInspectionEquipment01Mode
        {
            get { return _bypassInspectionEquipment01Mode; }
            set { _bypassInspectionEquipment01Mode = value; }
        }

        public eBitResult BypassInspectionEquipment02Mode
        {
            get { return _bypassInspectionEquipment02Mode; }
            set { _bypassInspectionEquipment02Mode = value; }
        }

        public eHightCVmode HighCVMode
        {
            get { return _highCVMode; }
            set { _highCVMode = value; }
        }

        public eBitResult NextLineBCStatus
        {
            get { return _nextLineBCStatus; }
            set { _nextLineBCStatus = value; }
        }

        public eBitResult CV07Status
        {
            get { return _cV07Status; }
            set { _cV07Status = value; }
        }

        public eBitResult IndexerExposureDispatchMode
        {
            get { return _indexerExposureDispatchMode; }
            set { _indexerExposureDispatchMode = value; }
        }

        public eBitResult ReworkForceToUnloaderCST
        {
            get { return _reworkForceToUnloaderCST; }
            set { _reworkForceToUnloaderCST = value; }
        }

        public eBitResult VCD01
        {
            get { return _vcd01; }
            set { _vcd01 = value; }
        }

        public eBitResult VCD02
        {
            get { return _vcd02; }
            set { _vcd02 = value; }
        }

        public eBitResult CP01
        {
            get { return _cp01; }
            set { _cp01 = value; }
        }

        public eBitResult CP02
        {
            get { return _cp02; }
            set { _cp02 = value; }
        }

        public eBitResult HP01
        {
            get { return _hp01; }
            set { _hp01 = value; }
        }

        public eBitResult HP02
        {
            get { return _hp02; }
            set { _hp02 = value; }
        }

        public double AccumulativeValue
        {
            get { return _accumulativeValue; }
            set { _accumulativeValue = value; }
        }

        public string CV06BufferInfo
        {
            get { return _cv06BufferInfo; }
            set { _cv06BufferInfo = value; }
        }

        public string Buffer01RWJudgeCapacity
        {
            get { return _buffer01RWJudgeCapacity; }
            set { _buffer01RWJudgeCapacity = value; }
        }

        public string Buffer02RWJudgeCapacity
        {
            get { return _buffer02RWJudgeCapacity; }
            set { _buffer02RWJudgeCapacity = value; }
        }

        public string OperatorID
        {
            get { return _operatorID; }
            set { _operatorID = value; }
        }

        public int OKPortModeStoreQTime
        {
            get { return _okPortModeStoreQTime; }
            set { _okPortModeStoreQTime = value; }
        }
        public ePortModeProductTypeCheck OKPort
        {
            get { return _okPort; }
            set { _okPort = value; }
        }
        public int NGPortModeStoreQTime
        {
            get { return _ngPortModeStoreQTime; }
            set { _ngPortModeStoreQTime = value; }
        }
        public ePortModeProductTypeCheck NGPort
        {
            get { return _ngPort; }
            set { _ngPort = value; }
        }
        public string NGPortJudge
        {
            get { return _ngPortJudge; }
            set { _ngPortJudge = value; }
        }
        public int PDPortModeStoreQTime
        {
            get { return __pdPortModeStoreQTime; }
            set { __pdPortModeStoreQTime = value; }
        }
        public ePortModeProductTypeCheck PDPort
        {
            get { return _pdPort; }
            set { _pdPort = value; }
        }
        public int RPPortModeStoreQTime
        {
            get { return _rpPortModeStoreQTime; }
            set { _rpPortModeStoreQTime = value; }
        }
        public ePortModeProductTypeCheck RPPort
        {
            get { return _rpPort; }
            set { _rpPort = value; }
        }
        public int IRPortModeStoreQTime
        {
            get { return _irPortModeStoreQTime; }
            set { _irPortModeStoreQTime = value; }
        }
        public ePortModeProductTypeCheck IRPort
        {
            get { return _irPort; }
            set { _irPort = value; }
        }
        public int MIXPortModeStoreQTime
        {
            get { return _mixPortModeStoreQTime; }
            set { _mixPortModeStoreQTime = value; }
        }
        public ePortModeProductTypeCheck MIXPort
        {
            get { return _mixPort; }
            set { _mixPort = value; }
        }
        public string MIXPortJudge
        {
            get { return _mixPortJudge; }
            set { _mixPortJudge = value; }
        }
        public ePortEnableMode InlineRework
        {
            get { return _inlineRework; }
            set { _inlineRework = value; }
        }

        #endregion

        #region [Cell Sepcial]
        private eEnableDisable _eqpENGMode = eEnableDisable.Disable;
        private string _unitENGMode = "";
        private eEnableDisable _firstRunMode = eEnableDisable.Disable;
        private eEnableDisable _cqltMode = eEnableDisable.Disable;
        private string _forceVCRReadingMode = "0";
        private eATSLoaderOperMode _atsLoaderOperMode = eATSLoaderOperMode.UNKNOW;
        private string _ptiNodeID = string.Empty;
        private int _totalUnassembledTFTCount = 0; //t3 cell add for PI
        private int _totalITODummyCount = 0; //t3 cell add for PI
        private int _totalNIPDummyCount = 0; //t3 cell add for PI
        private int _totalMetalOneDummyJobCount = 0; //t3 cell add for PI
        private Dictionary<string, string> _carrAndBoxMapping = new Dictionary<String, String>();
        private string _piMaterialTCProductType = string.Empty;//20171128 by huangjiayin


        public Dictionary<string, string> CarrAndBoxMapping
        {
            get { return _carrAndBoxMapping; }
            set { _carrAndBoxMapping = value; }
        }
        public int TotalUnassembledTFTJobCount //t3 cell add for PI
        {
            get { return _totalUnassembledTFTCount; }
            set { _totalUnassembledTFTCount = value; }
        }
        public int TotalITODummyJobCount //t3 cell add for PI
        {
            get { return _totalITODummyCount; }
            set { _totalITODummyCount = value; }
        }
        public int TotalNIPDummyJobCount //t3 cell add for PI
        {
            get { return _totalNIPDummyCount; }
            set { _totalNIPDummyCount = value; }
        }
        public int TotalMetalOneDummyJobCount //t3 cell add for PI// shihyang edit 2015/10/23
        {
            get { return _totalMetalOneDummyJobCount; }
            set { _totalMetalOneDummyJobCount = value; }
        }

        public eEnableDisable EQPENGMode
        {
            get { return _eqpENGMode; }
            set { _eqpENGMode = value; }
        }

        public string UnitENGMode
        {
            get { return _unitENGMode; }
            set { _unitENGMode = value; }
        }

        public eEnableDisable FirstRunMode
        {
            get { return _firstRunMode; }
            set { _firstRunMode = value; }
        }

        public eEnableDisable CQLTMode
        {
            get { return _cqltMode; }
            set { _cqltMode = value; }
        }

        public string ForceVCRReadingMode
        {
            get { return _forceVCRReadingMode; }
            set { _forceVCRReadingMode = value; }
        }

        public eATSLoaderOperMode ATSLoaderOperMode
        {
            get { return _atsLoaderOperMode; }
            set { _atsLoaderOperMode = value; }
        }

        public string PTINODEID
        {
            get { return _ptiNodeID; }
            set { _ptiNodeID = value; }
        }

        public string PIMaterialTCProductType
        {
            get { return _piMaterialTCProductType; }
            set { _piMaterialTCProductType = value; }
        }

        #endregion

        //MLDULE
        #region [Cell Sepcial]
        private eEnableDisable _eqpECNMode = eEnableDisable.Disable;

        public eEnableDisable EQPECNMode
        {
            get { return _eqpECNMode; }
            set { _eqpECNMode = value; }
        }
        #endregion

        #region [SECS]
        private eEQPStatus _preStatus = eEQPStatus.SETUP;
        private string _preMesStatus = "DOWN";
        private string _hSMSControlMode = "OFF-LINE";
        private int _aPCImportanIntervalMS = 0;
        private int _aPCImportanIntervalMSSForID = 0; // wucc add 20150806
        private int _aPCNormalIntervalMS = 0;
        private int _aPCNormalIntervalMSForID = 0; // wucc add 20150806
        private int _specialDataIntervalMS = 0;
        private int _specialDataIntervalMSForID = 0;// wucc add 20150806
        private bool _aPCImportanEnableReq = true;
        private bool _aPCNormalEnableReq = true;
        private bool _specialDataEnableReq = true;
        private bool _specialDataEnableReqForID = true; //wucc add 20150806
        private bool _aPCNormalEnableReqForID = true;//wucc add 20150806
        private bool _aPCImportanEnableReqForID = true;//wucc add 20150806  
        private bool _aPCImportanEnable = true;
        private bool _aPCNormalEnable = true;
        private bool _specialDataEnable = true;
        private bool _aPCImportanEnableForID = true; //wucc add 20150806
        private bool _aPCNormalEnableForID = true;//wucc add 20150806
        private bool _specialDataEnableForID = true;//wucc add 20150806
        private string _utilityIntervalNK = "000030";
        private string _aPCImportanIntervalNK = "000030";
        private string _aPCNormalIntervalNK = "000030";
        private string _specialDataIntervalNK = "000030";
        private bool _utilityEnableNK = true;

          /// <summary>
          /// For Nikon 設定是否還有上報Utility
          /// </summary>
        public bool UtilityEnableNK
        {
              get { return _utilityEnableNK; }
              set { _utilityEnableNK = value; }
        }

        /// <summary>
        /// 設定S6F3的Special Data是否上報
        /// </summary>
        public bool SpecialDataEnable
        {
            get { return _specialDataEnable; }
            set { _specialDataEnable = value; }
        }

        /// <summary>
        /// 設定S6F3的APC Normal是否上報
        /// </summary>
        public bool APCNormalEnable
        {
            get { return _aPCNormalEnable; }
            set { _aPCNormalEnable = value; }
        }

        /// <summary>
        /// 設定S6F3的APC Important是否上報
        /// </summary>
        public bool APCImportanEnable
        {
            get { return _aPCImportanEnable; }
            set { _aPCImportanEnable = value; }
        }

        /// <summary>
        /// 設定S6F3的Special Data是否上報 For ID
        /// </summary>
        public bool SpecialDataEnableForID   // wucc add 20150806
        {
            get { return _specialDataEnableForID; }
            set { _specialDataEnableForID = value; }
        }

        /// <summary>
        /// 設定S6F3的APC Normal是否上報 For ID
        /// </summary>
        public bool APCNormalEnableForID  // wucc add 20150806
        {
            get { return _aPCNormalEnableForID; }
            set { _aPCNormalEnableForID = value; }
        }

        /// <summary>
        /// 設定S6F3的APC Important是否上報 For ID
        /// </summary>
        public bool APCImportanEnableForID  // wucc add 20150806
        {
            get { return _aPCImportanEnableForID; }
            set { _aPCImportanEnableForID = value; }
        }

        /// <summary>
        ///  設定S1F5的Special Data是否request
        /// </summary>
        public bool SpecialDataEnableReq
        {
            get { return _specialDataEnableReq; }
            set { _specialDataEnableReq = value; }
        }

        /// <summary>
        /// 設定S1F5的APC Normal是否request
        /// </summary>
        public bool APCNormalEnableReq
        {
            get { return _aPCNormalEnableReq; }
            set { _aPCNormalEnableReq = value; }
        }

        /// <summary>
        ///  設定S1F5的APC Important是否request
        /// </summary>
        public bool APCImportanEnableReq
        {
            get { return _aPCImportanEnableReq; }
            set { _aPCImportanEnableReq = value; }
        }


        /// <summary>
        ///  設定S1F5的Special Data是否request for ID
        /// </summary>

        public bool SpecialDataEnableReqForID  // wucc add 20150806
        {
            get { return _specialDataEnableReqForID; }
            set { _specialDataEnableReqForID = value; }
        }

        /// <summary>
        /// 設定S1F5的APC Normal是否request for ID
        /// </summary>
        public bool APCNormalEnableReqForID // wucc add 20150806
        {
            get { return _aPCNormalEnableReqForID; }
            set { _aPCNormalEnableReqForID = value; }
        }
        /// <summary>
        ///  設定S1F5的APC Important是否request for ID
        /// </summary>
        public bool APCImportanEnableReqForID // wucc add 20150806
        {
            get { return _aPCImportanEnableReqForID; }
            set { _aPCImportanEnableReqForID = value; }
        }

        /// <summary>
        /// 設定Special Data上報間隔,for Nikon機台(hhmmss)
        /// </summary>
        public string SpecialDataIntervalNK
        {
            get { return _specialDataIntervalNK; }
            set { _specialDataIntervalNK = value; }
        }

        /// <summary>
        /// 設定APC Normal上報間隔,for Nikon機台(hhmmss)
        /// </summary>
        public string APCNormalIntervalNK
        {
            get { return _aPCNormalIntervalNK; }
            set { _aPCNormalIntervalNK = value; }
        }

        /// <summary>
        /// 設定APC Importan上報間隔,for Nikon機台(hhmmss)
        /// </summary>
        public string APCImportanIntervalNK
        {
            get { return _aPCImportanIntervalNK; }
            set { _aPCImportanIntervalNK = value; }
        }

        /// <summary>
        /// 設定Utility Data上報間隔,for Nikon機台(hhmmss)
        /// </summary>
        public string UtilityIntervalNK
        {
            get { return _utilityIntervalNK; }
            set { _utilityIntervalNK = value; }
        }

        /// <summary>
        /// 設定Specail Data上報間隔
        /// </summary>
        public int SpecialDataIntervalMS
        {
            get { return _specialDataIntervalMS; }
            set { _specialDataIntervalMS = value; }
        }

        /// 設定Specail Data上報間隔 For ID
        /// </summary>
        public int SpecialDataIntervalMSForID
        {
            get { return _specialDataIntervalMSForID; }
            set { _specialDataIntervalMSForID = value; }
        }

        /// <summary>
        /// 設定APC Normal上報間隔
        /// </summary>
        public int APCNormalIntervalMS
        {
            get { return _aPCNormalIntervalMS; }
            set { _aPCNormalIntervalMS = value; }
        }

        /// 設定APC Normal上報間隔 
        /// </summary>
        public int APCNormalIntervalMSForID  // wucc add 20150806
        {
            get { return _aPCNormalIntervalMSForID; }
            set { _aPCNormalIntervalMSForID = value; }
        }

        /// <summary>
        /// 設定APC Importan上報間隔
        /// </summary>
        public int APCImportanIntervalMS
        {
            get { return _aPCImportanIntervalMS; }
            set { _aPCImportanIntervalMS = value; }
        }
        // /// 設定APC Importan For ID上報間隔
        public int APCImportanIntervalMSForID
        {
            get { return _aPCImportanIntervalMSSForID; }
            set { _aPCImportanIntervalMSSForID = value; }
        }

        public string HSMSControlMode
        {
            get { return _hSMSControlMode; }
            set { _hSMSControlMode = value; }
        }

        public eEQPStatus PreStatus
        {
            get { return _preStatus; }
            set { _preStatus = value; }
        }
        public string PreMesStatus
        {
            get { return _preMesStatus; }
            set { _preMesStatus = value; }
        }
        #endregion

    }

    public class Equipment : Entity
    {
        public EquipmentEntityData Data { get; private set; }

        public EquipmentEntityFile File { get; private set; }

        #region[SECS Special]
        private bool _eventReportConfigurated = false;
        private bool _hsmsConnected = false;
        //private string _secsControlMode = "OFF-LINE";
        private bool _secsCommunicated = false;
        private bool _hsmsSelected = false;
        private string _mDLN = string.Empty;
        private string _sOFTREV = string.Empty;
        private string _hsmsConnStatus = "DISCONNECTED";
        private string _inUseMaskID = string.Empty;

        public string InUseMaskID
        {
            get { return _inUseMaskID; }
            set { _inUseMaskID = value; }
        }

        public string HsmsConnStatus
        {
            get { return _hsmsConnStatus; }
            set { _hsmsConnStatus = value; }
        }

        public string SOFTREV
        {
            get { return _sOFTREV; }
            set { _sOFTREV = value; }
        }

        public string MDLN
        {
            get { return _mDLN; }
            set { _mDLN = value; }
        }

        public bool HsmsSelected
        {
            get { return _hsmsSelected; }
            set { _hsmsSelected = value; }
        }

        public bool SecsCommunicated
        {
            get { return _secsCommunicated; }
            set { _secsCommunicated = value; }
        }

        //public string SecsControlMode
        //{
        //    get { return _secsControlMode; }
        //    set { _secsControlMode = value; }
        //}

        public bool HsmsConnected
        {
            get { return _hsmsConnected; }
            set { _hsmsConnected = value; }
        }

        public bool EventReportConfigurated
        {
            get { return _eventReportConfigurated; }
            set { _eventReportConfigurated = value; }
        }
        #endregion

        public Equipment(EquipmentEntityData data, EquipmentEntityFile file)
        {
            Data = data;
            File = file;
        }
    }
}
