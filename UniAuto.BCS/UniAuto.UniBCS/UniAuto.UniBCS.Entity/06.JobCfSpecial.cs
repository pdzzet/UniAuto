using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.ComponentModel;

namespace UniAuto.UniBCS.Entity
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter)), Description("CF Special")]  
    public class JobCfSpecial:ICloneable
    {

        private string _totalDischarge = "0"; //INT
        private string _initialDisVolume = "0"; //INT
        private string _rtcFlag = "0"; //INT
        private string _loaderBufferingFlag = "0"; //BIN
        private string _sourcePortNo = "0"; //BIN
        private string _targetPortNo = "0"; //INT
        private string _targetSlotNo = "0"; //INT
        private string _targetCSTID = "0"; //INT
        private string _preInlineID = string.Empty;
        private string _dummyUsedCount = "0"; //INT
        private string _coaterCSPNo = "0"; //INT
        private string _ocFlag = "0"; //BIN For T3 CF Photo Line
        private string _vcrMismatchFlag = "0";  //BIN
        private string _ovenHPSlotNumber = "0"; //INT
        private string _samplingValue = "0"; //BCD
        private string _coaVersion = string.Empty;
        private string _nextLineName = string.Empty;
        private string _permitFlag = string.Empty;
        private string _nextLineJobData = string.Empty;
        private string _reworkMaxCount = "0"; //INT
        private string _reworkRealCount = "0"; //INT
        private string _flowPriorityInfo = "0"; //INT
        private string _plannedsourcepart = string.Empty;
        private string _exposuredoperation = string.Empty;
        private string _itoSideFlag = string.Empty;
        private string _uPKOwnerType = string.Empty;
        private string _maskID = string.Empty;
        private string _prID = string.Empty;
        private string _maskusecount = string.Empty;
        private string _cfCurrentProcess = string.Empty;//For T3 CF Host PPID Split Logic
        private string _marcoReserveFlag = string.Empty;//For T3 CF Marco Reserve Logic
        private string _ExposureName = string.Empty;// For T3 CF Exposure Name Logic For BM BackUp 
        private string _sourceLotName = string.Empty;//For T3 CF source Lot Name For Lot to Lot Mode
        private string _inlineReworkMaxCount = "0"; //INT For T3 CF Photo Line
        private string _inlineReworkRealCount = "0"; //INT For T3 CF Photo Line
        private string _recyclingFlag = string.Empty; //Add for T3 MES by marine

        private bool _cfShortCutRecipeIDCheckFlag = false; //CF廠 Short Cut - 當同一個Cassette的第一片玻璃做完下一條Line的Recipe ID Check時，要更新此Flag到所有同一個Cassette的Job
        private bool _cfShortCutRecipeParameterCheckFlag = false; //CF廠 Short Cut - 當同一個Cassette的第一片玻璃做完下一條Line的Recipe Parameter Check時，要更新此Flag到所有同一個Cassette的Job
        private bool _cfShortCutWIPCheck = false;
        private bool _cfShortCutTrackOut = false;
        
        private eRecipeCheckResult _cfShortCutrecipeParameterRequestResult = eRecipeCheckResult.NG;
        private eRecipeCheckResult _cfShortCutRecipeIDCheckResult = eRecipeCheckResult.NG;
        private CFCFSpecialReserved _cfspecialreserved = new CFCFSpecialReserved();

        private CFINSPReservations _inspReservations = new CFINSPReservations();
        private CFEQPReservations _eQPReservations = new CFEQPReservations();
        private CFInspJudgedData1 _inspjudgeddata1 = new CFInspJudgedData1();
        private CFInspJudgedData2 _inspjudgeddata2 = new CFInspJudgedData2();
        private CFEQPFlag1 _eqpflag1 = new CFEQPFlag1();
        private CFEQPFlag2 _eqpflg2 = new CFEQPFlag2();
        private CFTrackingData _trackingdata = new CFTrackingData();
        private CFMarcoReserveFlag _cfmarcoReserveFlag = new CFMarcoReserveFlag();
        private CFProcessBackUpFlag  _cfprocessbackup = new CFProcessBackUpFlag();
        
        private CFAbnormalCode _abnormalCode = new CFAbnormalCode();

		private string _rcsBufferingFlag = string.Empty;
        private DateTime _thicknessreceivejobtime = DateTime.Now;
        private DateTime _developerreceivejobtime = DateTime.Now;

        public DateTime ThicknessReceiveJobTime
        {
            get { return _thicknessreceivejobtime; }
            set { _thicknessreceivejobtime = value; }
        }
        public DateTime DeveloperReceiveJobTime
        {
            get { return _developerreceivejobtime; }
            set { _developerreceivejobtime = value; }
        }
        public string RTCFlag
        {
            get { return _rtcFlag; }
            set { _rtcFlag = value; }
        }
        public string VCRMismatchFlag
        {
            get { return _vcrMismatchFlag; }
            set { _vcrMismatchFlag = value; }
        }
        public string TotalDischarge
        {
            get { return _totalDischarge; }
            set { _totalDischarge = value; }
        }
        public string InitialDisVolume
        {
            get { return _initialDisVolume; }
            set { _initialDisVolume = value; }
        }
        public string LoaderBufferingFlag
        {
            get { return _loaderBufferingFlag; }
            set { _loaderBufferingFlag = value; }
        }
        public string SourcePortNo
        {
            get { return _sourcePortNo; }
            set { _sourcePortNo = value; }
        }
        public string TargetPortNo
        {
            get { return _targetPortNo; }
            set { _targetPortNo = value; }
        }
        public string TargetSlotNo
        {
            get { return _targetSlotNo; }
            set { _targetSlotNo = value; }
        }
        public string TargetCSTID
        {
            get { return _targetCSTID; }
            set { _targetCSTID = value; }
        }
        public string COAversion
        {
            get { return _coaVersion; }
            set { _coaVersion = value; }
        }
        public string PreInlineID
        {
            get { return _preInlineID; }
            set { _preInlineID = value; }
        }
        public string DummyUsedCount
        {
            get { return _dummyUsedCount; }
            set { _dummyUsedCount = value; }
        }
        public string CoaterCSPNo
        {
            get { return _coaterCSPNo; }
            set { _coaterCSPNo = value; }
        }

        public string OCFlag
        {
            get { return _ocFlag; }
            set { _ocFlag = value; }
        }
        public string OvenHPSlotNumber
        {
            get { return _ovenHPSlotNumber; }
            set { _ovenHPSlotNumber = value; }
        }
        public string SamplingValue
        {
            get { return _samplingValue; }
            set { _samplingValue = value; }
        }
        public string MaskID
        {
            get { return _maskID; }
            set { _maskID = value; }
        }
        public string PRID
        {
            get { return _prID; }
            set { _prID = value; }
        }
        public string MaskUseCount
        {
            get { return _maskusecount; }
            set { _maskusecount = value; }
        }
        public string NextLineName
        {
            get { return _nextLineName; }
            set { _nextLineName = value; }
        }
        public string PermitFlag
        {
            get { return _permitFlag; }
            set { _permitFlag = value; }
        }
        public string NextLineJobData
        {
            get { return _nextLineJobData; }
            set { _nextLineJobData = value; }
        }
        public string ReworkMaxCount
        {
            get { return _reworkMaxCount; }
            set { _reworkMaxCount = value; }
        }
        public string ReworkRealCount
        {
            get { return _reworkRealCount; }
            set { _reworkRealCount = value; }
        }
        public string FlowPriorityInfo
        {
            get { return _flowPriorityInfo; }
            set { _flowPriorityInfo = value; }
        }
        public string PlannedSourcePart
        {
            get { return _plannedsourcepart; }
            set { _plannedsourcepart = value; }
        }
        public string ExposureDoperation
        {
            get { return _exposuredoperation; }
            set { _exposuredoperation = value; }
        }
        public string ITOSIDEFLAG
        {
            get { return _itoSideFlag; }
            set { _itoSideFlag = value; }
        }
        public string UPKOWNERTYPE
        {
            get { return _uPKOwnerType; }
            set { _uPKOwnerType = value; }
        }

        // Valitation T3 Cst By Host PPID Namr Rule (8-9) is BM On True
        public string  CFCurrentProcess
        {
            get { return _cfCurrentProcess; }
            set { _cfCurrentProcess = value; }
        }
        /// <summary>
        /// Marco Reserve Flag
        /// </summary>
        public string MarcoReserveFlag
        {
            get { return _marcoReserveFlag; }
            set { _marcoReserveFlag = value; }
        }

        /// <summary>
        /// Exposure Name
        /// </summary>
        public string ExposureName
        {
            get { return _ExposureName; }
            set { _ExposureName = value; }
        }

        /// <summary>
        /// Source Lot Name
        /// </summary>
        public string  SourceLotName
        {
            get { return _sourceLotName; }
            set { _sourceLotName = value; } 
        }

        public string InlineReworkMaxCount
        {
            get { return _inlineReworkMaxCount; }
            set { _inlineReworkMaxCount = value; }
        }

        //Add for T3 MES by marine
        public string RecyclingFlag
        {
            get { return _recyclingFlag; }
            set { _recyclingFlag = value; }
        }

        public string InlineReworkRealCount
        {
            get { return _inlineReworkRealCount; }
            set { _inlineReworkRealCount = value; }
        }

        public CFCFSpecialReserved CFSpecialReserved
        {
            get { return _cfspecialreserved; }
            set { _cfspecialreserved = value; }
        }

        public CFINSPReservations InspReservations
        {
            get { return _inspReservations; }
            set { _inspReservations = value; }
        }

        public CFEQPReservations EQPReservations
        {
            get { return _eQPReservations; }
            set { _eQPReservations = value; }
        }

        public CFInspJudgedData1 InspJudgedData1
        {
            get { return _inspjudgeddata1; }
            set { _inspjudgeddata1 = value; }
        }

        public CFInspJudgedData2 InspJudgedData2
        {
            get { return _inspjudgeddata2; }
            set { _inspjudgeddata2 = value; }
        }

        public CFEQPFlag1 EQPFlag1
        {
            get { return _eqpflag1; }
            set { _eqpflag1 = value; }
        }

        public CFEQPFlag2 EQPFlag2
        {
            get { return _eqpflg2; }
            set { _eqpflg2 = value; }
        }

        public CFTrackingData TrackingData
        {
            get { return _trackingdata; }
            set { _trackingdata = value; }
        }

        public CFMarcoReserveFlag CFMarcoReserve
        {
            get { return _cfmarcoReserveFlag; }
            set { _cfmarcoReserveFlag = value; }
        }

        public CFProcessBackUpFlag CFProcessBackUp
        {
            get { return _cfprocessbackup; }
            set { _cfprocessbackup = value; }
        }

        public CFAbnormalCode AbnormalCode
        {
            get { return _abnormalCode; }
            set { _abnormalCode = value; }
        }


		public string RCSBufferingFlag {
			get { return _rcsBufferingFlag; }
			set { _rcsBufferingFlag = value; }
		}

        /// <summary>
        /// CF廠 All Line INSP Reservations
        /// </summary>
        [Serializable]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class CFINSPReservations : ICloneable
        {
            #region [Photo Line]
            private string _tHK = "0";
            private string _aOI = "0";
            private string _macro = "0";
            private string _totalPitch = "0";
            private string _cD = "0";

            public string THK
            {
                get { return _tHK; }
                set { _tHK = value; }
            }

            public string AOI //AOI 也有用到
            {
                get { return _aOI; }
                set { _aOI = value; }
            }

            public string Macro
            {
                get { return _macro; }
                set { _macro = value; }
            }

            public string TotalPitch
            {
                get { return _totalPitch; }
                set { _totalPitch = value; }
            }

            public string CD 
            {
                get { return _cD; }
                set { _cD = value; }
            }
            #endregion

            #region[PSH Line] 
            //Add for T3
            private string _psh = "0";

            public string PSH
            {
                get { return _psh; }
                set { _psh = value; }
            }
            #endregion

            #region[MQC Line]
            private string _mcpd = "0";
            private string _ttp = "0";
            private string _sp = "0";
            private string _mqccd = "0";

            public string MCPD
            {
                get { return _mcpd; }
                set { _mcpd = value; }
            }

            public string TTP
            {
                get { return _ttp; }
                set { _ttp = value; }
            }

            public string SP
            {
                get { return _sp; }
                set { _sp = value; }
            }

            public string MQCCD
            {
                get { return _mqccd; }
                set { _mqccd = value; }
            }
            #endregion        

            #region[MAC Line]
            private string _macro01 = "0";
            private string _macro02 = "0";
            private string _macro03 = "0";

            public string Macro01
            {
                get { return _macro01; }
                set { _macro01 = value; }
            }
            public string Macro02
            {
                get { return _macro02; }
                set { _macro02 = value; }
            }
            public string Macro03
            {
                get { return _macro03; }
                set { _macro03 = value; }
            }
            #endregion
           
            public object Clone()
            {
                CFINSPReservations cFInspReservations = (CFINSPReservations)this.MemberwiseClone();
                return cFInspReservations;
            }
        }

        /// <summary>
        /// CF廠 All Line EQP Reservations
        /// </summary>
        [Serializable]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class CFEQPReservations : ICloneable
        {
            #region [Photo Line]
            private string _toTotalPitch = "0";
            private string _toTotalPitchSubChamber = "0";
            private string _bmexposureprocess = "0";

            public string ToTotalPitch //MQC_2 也有用到
            {
                get { return _toTotalPitch; }
                set { _toTotalPitch = value; }
            }

            public string ToTotalPitchSubChamber //MQC_2 也有用到
            {
                get { return _toTotalPitchSubChamber; }
                set { _toTotalPitchSubChamber = value; }
            }

            public string BMExposureProcess
            {
                get { return _bmexposureprocess; }
                set { _bmexposureprocess = value; }
            }
            #endregion

            #region [Repair Line]
            private string _repair01 = "0";
            private string _inkrepair02 = "0";
            private string _inkrepair03 = "0";

            public string Repair01
            {
                get { return _repair01; }
                set { _repair01 = value; }
            }

            public string InkRepair02
            {
                get { return _inkrepair02; }
                set { _inkrepair02 = value; }
            }

            public string InkRepair03
            {
                get { return _inkrepair03; }
                set { _inkrepair03 = value; }
            }
            #endregion

            public object Clone()
            {
                CFEQPReservations cFEQPReservations = (CFEQPReservations)this.MemberwiseClone();
                return cFEQPReservations;
            }
        }

        /// <summary>
        /// CF廠 All Line Insp Judged Data1
        /// </summary>
        [Serializable]
        [TypeConverter(typeof(ExpandableObjectConverter))]  
        public class CFInspJudgedData1:ICloneable
        {
            #region [Common]          
            #endregion

            #region [Photo Line]
            private string _thk = "0";
            private string _macro = "0";
            private string _totalpitch_bm = "0";
            private string _aoi = "0";

            public string AOI //AOI 也有用到
            {
                get { return _aoi; }
                set { _aoi = value; }
            }

            public string THK
            {
                get { return _thk; }
                set { _thk = value; }
            }

            public string Macro
            {
                get { return _macro; }
                set { _macro = value; }
            }

            public string TotalPitch_BM
            {
                get { return _totalpitch_bm; }
                set { _totalpitch_bm = value; }
            }
            #endregion

            #region [Repair Line]
            private string _repair01 = "0";
            private string _inkrepair02 = "0";
            private string _inkrepair03 = "0";

            public string Repair01
            {
                get { return _repair01; }
                set { _repair01 = value; }
            }

            public string InkRepair02
            {
                get { return _inkrepair02; }
                set { _inkrepair02 = value; }
            }

            public string InkRepair03
            {
                get { return _inkrepair03; }
                set { _inkrepair03 = value; }
            }
            #endregion           

            #region [MQC Line]
            private string _cd = "0";
            private string _mcpd = "0";
            private string _ttp = "0";
            private string _sp = "0";

            public string CD
            {
                get { return _cd; }
                set { _cd = value; }
            }

            public string MCPD
            {
                get { return _mcpd; }
                set { _mcpd = value; }
            }

            public string TTP
            {
                get { return _ttp; }
                set { _ttp = value; }
            }

            public string SP
            {
                get { return _sp; }
                set { _sp = value; }
            }
            #endregion

            #region [MAC Line]
            private string _macro01 = "0";
            private string _macro02 = "0";
            private string _macro03 = "0";

            public string Macro01
            {
                get { return _macro01; }
                set { _macro01 = value; }
            }

            public string Macro02
            {
                get { return _macro02; }
                set { _macro02 = value; }
            }

            public string Macro03
            {
                get { return _macro03; }
                set { _macro03 = value; }
            }
            #endregion           

            #region[PSH Line] 
            //Add for T3
            private string _IJpsh1 = "0";
            private string _IJpsh2 = "0";

            public string IJPSH1
            {
                get { return _IJpsh1; }
                set { _IJpsh1 = value; }
            }
            public string IJPSH2
            {
                get { return _IJpsh2; }
                set { _IJpsh2 = value; }
            }
            #endregion

            public object Clone()
            {
                CFInspJudgedData1 cFInspJudgeData1 = (CFInspJudgedData1)this.MemberwiseClone();
                return cFInspJudgeData1;
            }
        }

        /// <summary>
        /// CF廠 All Line Insp Judged Data2
        /// </summary>
        [Serializable]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class CFInspJudgedData2 : ICloneable
        {
            #region [Photo Line]
            private string _cd = "0";

            public string CD
            {
                get { return _cd; }
                set { _cd = value; }
            }

            #endregion

            public object Clone()
            {
                CFInspJudgedData2 cFInspJudgeData2 = (CFInspJudgedData2)this.MemberwiseClone();
                return cFInspJudgeData2;
            }
        }

        /// <summary>
        /// CF廠 All Line Tracking Data
        /// </summary>
        [Serializable]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class CFTrackingData : ICloneable
        {
            #region [Common]

            #endregion

            #region [Photo Line]
            private string _coatervcd01 = "0";
            private string _coatervcd02 = "0";
            private string _cv03highturntable = "0";
            private string _cv03lowturntable = "0";
            private string _exposure = "0";
            private string _exposure2 = "0";
            private string _exposurecp01 = "0";
            private string _exposurecp02 = "0";
            private string _ovenhp01 = "0";
            private string _ovenhp02 = "0";
            private string _photocd = "0";
            private string _macro = "0";
            private string _totalpitch = "0";

            public string CoaterVCD01
            {
                get { return _coatervcd01; }
                set { _coatervcd01 = value; }
            }

            public string CoaterVCD02
            {
                get { return _coatervcd02; }
                set { _coatervcd02 = value; }
            }

            public string CV03HighTurnTable
            {
                get { return _cv03highturntable; }
                set { _cv03highturntable = value; }
            }

            public string CV03LowTurnTable
            {
                get { return _cv03lowturntable; }
                set { _cv03lowturntable = value; }
            }

            public string BMPS_Exposure
            {
                get { return _exposure; }
                set { _exposure = value; }
            }

            public string BMPS_Exposure2
            {
                get { return _exposure2; }
                set { _exposure2 = value; }
            }

            public string RGB_ExposureCP01
            {
                get { return _exposurecp01; }
                set { _exposurecp01 = value; }
            }

            public string RGB_ExposureCP02
            {
                get { return _exposurecp02; }
                set { _exposurecp02 = value; }
            }

            public string Photo_OvenHP01
            {
                get { return _ovenhp01; }
                set { _ovenhp01 = value; }
            }

            public string Photo_OvenHP02
            {
                get { return _ovenhp02; }
                set { _ovenhp02 = value; }
            }

            public string Photo_CD
            {
                get { return _photocd; }
                set { _photocd = value; }
            }

            public string Macro
            {
                get { return _macro; }
                set { _macro = value; }
            }

            public string TotalPitch
            {
                get { return _totalpitch; }
                set { _totalpitch = value; }
            }
            #endregion

            #region [Rework Line]
            private string _etching = "0";
            private string _stripper = "0";

            public string Etching
            {
                get { return _etching; }
                set { _etching = value; }
            }

            public string Stripper
            {
                get { return _stripper; }
                set { _stripper = value; }
            }
            #endregion

            #region [MSK Line]
            private string _euv = "0";
            private string _maskinspection = "0";
            private string _maskcleaner = "0";
            private string _maskaoi = "0"; //Add for T3

            public string EUV
            {
                get { return _euv; }
                set { _euv = value; }
            }

            public string MaskInspection
            {
                get { return _maskinspection; }
                set { _maskinspection = value; }
            }

            public string MaskCleaner
            {
                get { return _maskcleaner; }
                set { _maskcleaner = value; }
            }

            public string MaskAOI
            {
                get { return _maskaoi; }
                set { _maskaoi = value; }
            }
            #endregion

            #region [Repair Line]
            private string _repair01 = "0";
            private string _repair02 = "0";
            private string _repair03 = "0";

            public string Repair01
            {
                get { return _repair01; }
                set { _repair01 = value; }
            }

            public string Repair02
            {
                get { return _repair02; }
                set { _repair02 = value; }
            }

            public string Repair03
            {
                get { return _repair03; }
                set { _repair03 = value; }
            }
            #endregion

            #region [MQC Line]
            private string _ttp = "0";
            private string _mcpd = "0";
            private string _sp = "0";
            private string _cd = "0";

            public string TTP
            {
                get { return _ttp; }
                set { _ttp = value; }
            }

            public string MCPD
            {
                get { return _mcpd; }
                set { _mcpd = value; }
            }

            public string SP
            {
                get { return _sp; }
                set { _sp = value; }
            }

            public string CD
            {
                get { return _cd; }
                set { _cd = value; }
            }
                
                
            #endregion

            #region [MAC Line]
            private string _macro01 = "0";
            private string _macro02 = "0";
            private string _macro03 = "0";

            public string Macro01
            {
                get { return _macro01; }
                set { _macro01 = value; }
            }

            public string Macro02
            {
                get { return _macro02; }
                set { _macro02 = value; }
            }

            public string Macro03
            {
                get { return _macro03; }
                set { _macro03 = value; }
            }
            #endregion

            #region[PSH Line] 
            //Add for T3
            private string _TDpsh1 = "0";
            private string _TDpsh2 = "0";

            public string TDPSH1
            {
                get { return _TDpsh1; }
                set { _TDpsh1 = value; }
            }
            public string TDPSH2
            {
                get { return _TDpsh2; }
                set { _TDpsh2 = value; }
            }
            #endregion

            #region[AOI Line]
            private string _aoi = "0";

            public string AOI
            {
                get { return _aoi; }
                set { _aoi = value; }
            }
            #endregion

            public object Clone()
            {
                CFTrackingData cFTrackingData = (CFTrackingData)this.MemberwiseClone();
                return cFTrackingData;
            }
        }

        /// <summary>
        /// CF廠 All Line CF Special Reserved
        /// </summary>
        [Serializable]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class CFCFSpecialReserved : ICloneable
        {
            #region [Photo Line]
            private string _hpslotnumber = "0";
            private string _cpslotnumber = "0";
            private string _thkmurajudgeddata = "0";

            public string HPSlotNumber
            {
                get { return _hpslotnumber; }
                set { _hpslotnumber = value; }
            }

            public string CPSlotNumber
            {
                get { return _cpslotnumber; }
                set { _cpslotnumber = value; }
            }

            public string THKMuraJudgedData
            {
                get { return _thkmurajudgeddata; }
                set { _thkmurajudgeddata = value; }
            }
            #endregion

            #region [MSK Line]
            private string _stocknumber = "0";
            private string _stockslotnumber = "0";

            public string StockNumber
            {
                get { return _stocknumber; }
                set { _stocknumber = value; }
            }

            public string StockSlotNumber
            {
                get { return _stockslotnumber; }
                set { _stockslotnumber = value; }
            }

            #endregion

            #region[PSH Line] 
            //Add for T3
            private string _forcepshbit = "0";

            public string ForcePSHbit
            {
                get { return _forcepshbit; }
                set { _forcepshbit = value; }
            }
            #endregion

            public object Clone()
            {
                CFCFSpecialReserved cFCFSpecialReserved = (CFCFSpecialReserved)this.MemberwiseClone();
                return cFCFSpecialReserved;
            }
        }

        /// <summary>
        /// CF廠 All Line EQP Flag1
        /// </summary>
        [Serializable]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class CFEQPFlag1 : ICloneable
        {
            #region [Common]
            private string _ovenoverbakeflag = "0";
            [Category("CF Common")]
            public string OvenOverBakeFlag
            {
                get { return _ovenoverbakeflag; }
                set { _ovenoverbakeflag = value; }
            }
            #endregion

            #region [Photo Line]
            private string _cleanerturnmodeflag = "0";
            private string _coaterturnmodeflag = "0";
            private string _developerturnmodeflag = "0";
            private string _oventurnmodeflag = "0";
            private string _turntable01_cv01 = "0";
            private string _turntable02_cv02 = "0";
            private string _turntable03_cv03high = "0";
            private string _turntable04_cv03low = "0";
            private string _turntable05_cv05 = "0";
            private string _turntable06_cv06 = "0";
            private string _turntableinformation_cv03 = "0";
            private string _thkbypassflag = "0";
            private string _shortcutpermissionflag_cv06="0";

            private string _totalPitchOfflineInspectionFlag = "0";// For T3 Total Pitch Offline Inspeciont机差


            public string CleanerTurnModeFlag
            {
                get { return _cleanerturnmodeflag; }
                set { _cleanerturnmodeflag = value; }
            }

            public string CoaterTurnModeFlag
            {
                get { return _coaterturnmodeflag; }
                set { _coaterturnmodeflag = value; }
            }

            public string DeveloperTurnModeFlag
            {
                get { return _developerturnmodeflag; }
                set { _developerturnmodeflag = value; }
            }

            public string OvenTurnModeFlag
            {
                get { return _oventurnmodeflag; }
                set { _oventurnmodeflag = value; }
            }

            public string TurnTable01_CV01
            {
                get { return _turntable01_cv01; }
                set { _turntable01_cv01 = value; }
            }

            public string TurnTable02_CV02
            {
                get { return _turntable02_cv02; }
                set { _turntable02_cv02 = value; }
            }

            public string TurnTable03_CV03HighTurnTable
            {
                get { return _turntable03_cv03high; }
                set { _turntable03_cv03high = value; }
            }

            public string TurnTable04_CV03LowTurnTable
            {
                get { return _turntable04_cv03low; }
                set { _turntable04_cv03low = value; }
            }

            public string TurnTable05_CV05
            {
                get { return _turntable05_cv05; }
                set { _turntable05_cv05 = value; }
            }

            public string TurnTable06_CV06
            {
                get { return _turntable06_cv06; }
                set { _turntable06_cv06 = value; }
            }

            public string TurnTableInformation_CV03
            {
                get { return _turntableinformation_cv03; }
                set { _turntableinformation_cv03 = value; }
            }

            public string THKBypassFlag
            {
                get { return _thkbypassflag; }
                set { _thkbypassflag = value; }
            }

            /// <summary>
            /// For T3 BM Total Pitch Offline Inspeciont机差
            /// </summary>
            public string TotalPitchOfflineInspectionFlag
            {
                get { return _totalPitchOfflineInspectionFlag; }
                set { _totalPitchOfflineInspectionFlag = value; }
            }

            public string ShortCutPermissionFlag_CV06
            {
                get { return _shortcutpermissionflag_cv06; }
                set { _shortcutpermissionflag_cv06 = value; }
            }
            #endregion

            #region [Rework Line]
            private string _etchingprocessabnormalflag = "0";
            private string _stripperprocessabnormalflag = "0";

            public string EtchingProcessAbnormalFlag
            {
                get { return _etchingprocessabnormalflag; }
                set { _etchingprocessabnormalflag = value; }
            }

            public string StripperProcessAbnormalFlag
            {
                get { return _stripperprocessabnormalflag; }
                set { _stripperprocessabnormalflag = value; }
            }
            #endregion

            #region [Repair Line]
            private string _inkrepairglass = "0";
            private string _repairglass = "0";

            public string InkRepairGlass
            {
                get { return _inkrepairglass; }
                set { _inkrepairglass = value; }
            }

            public string RepairGlass
            {
                get { return _repairglass; }
                set { _repairglass = value; }
            }
            #endregion

            #region [MAC Line]
            private string _fipglass = "0";
            private string _mqcglass = "0";
            private string _bmacroglass = "0";

            public string FIPGlass
            {
                get { return _fipglass; }
                set { _fipglass = value; }
            }

            public string MQCGlass
            {
                get { return _mqcglass; }
                set { _mqcglass = value; }
            }

            public string BMacroGlass
            {
                get { return _bmacroglass; }
                set { _bmacroglass = value; }
            }
            #endregion

            public object Clone()
            {
                CFEQPFlag1 cFEQPFlag1 = (CFEQPFlag1)this.MemberwiseClone();
                return cFEQPFlag1;
            }
        }

        /// <summary>
        /// CF廠 All Line EQP Flag2
        /// </summary>
        [Serializable]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class CFEQPFlag2 : ICloneable
        {
            #region [Photo Line]
            private string _exposureqtimeoverrwflag = "0";
            private string _exposureqtimeoverngflag = "0";
            private string _developerqtimeoverngflag = "0";
            private string _hpcpoverbakeflag = "0";
            private string _ovenoverbakeflag = "0";
            private string _exposurengflag = "0";
            private string _exposureprocessflag = "0";
            private string _titlerbypassflag = "0";
            private string _aoibypassflag = "0";
            private string _aoi_cdol_inspectionflag = "0";
            private string _shortcutglassflag_forcv06 = "0";
            private string _shortcutforcedsamplingflag_forcv06 = "0";
            private string _shortcutfailflag = "0";
            private string _mp1 = "0";
            private string _mp2 = "0";

            private string _totalPitchOfflineInspectionFlag = "0";// For T3 Total Pitch Offline Inspeciont机差


            public string ExposureQTimeOverRWFlag
            {
                get { return _exposureqtimeoverrwflag; }
                set { _exposureqtimeoverrwflag = value; }
            }

            public string ExposureQTimeOverNGFlag
            {
                get { return _exposureqtimeoverngflag; }
                set { _exposureqtimeoverngflag = value; }
            }

            public string DeveloperQTimeOverNGFlag
            {
                get { return _developerqtimeoverngflag; }
                set { _developerqtimeoverngflag = value; }
            }

            public string HPCPOverBakeFlag
            {
                get { return _hpcpoverbakeflag; }
                set { _hpcpoverbakeflag = value; }
            }

            public string OvenOverBakeFlag
            {
                get { return _ovenoverbakeflag; }
                set { _ovenoverbakeflag = value; }
            }

            public string ExposureNGFlag
            {
                get { return _exposurengflag; }
                set { _exposurengflag = value; }
            }

            public string ExposureProcessFlag
            {
                get { return _exposureprocessflag; }
                set { _exposureprocessflag = value; }
            }

            public string TitlerBypassFlag
            {
                get { return _titlerbypassflag; }
                set { _titlerbypassflag = value; }
            }

            public string AOIBypassFlag
            {
                get { return _aoibypassflag; }
                set { _aoibypassflag = value; }
            }

            public string AOI_CDOL_InspectionFlag
            {
                get { return _aoi_cdol_inspectionflag; }
                set { _aoi_cdol_inspectionflag = value; }
            }

            public string ShortCutGlassFlag_ForCV06
            {
                get { return _shortcutglassflag_forcv06; }
                set { _shortcutglassflag_forcv06 = value; }
            }

            public string ShortCutForcedSamplingFlag_ForCV06
            {
                get { return _shortcutforcedsamplingflag_forcv06; }
                set { _shortcutforcedsamplingflag_forcv06 = value; }
            }

            public string ShortCutFailFlag
            {
                get { return _shortcutfailflag; }
                set { _shortcutfailflag = value; }
            }

            public string MP1
            {
                get { return _mp1; }
                set { _mp1 = value; }
            }

            public string MP2
            {
                get { return _mp2; }
                set { _mp2 = value; }
            }

            /// <summary>
            /// For T3 BM Total Pitch Offline Inspeciont机差
            /// </summary>
            public string TotalPitchOfflineInspectionFlag
            {
                get { return _totalPitchOfflineInspectionFlag; }
                set { _totalPitchOfflineInspectionFlag = value; }
            }
            #endregion

            public object Clone()
            {
                CFEQPFlag2 cFEQPFlag2 = (CFEQPFlag2)this.MemberwiseClone();
                return cFEQPFlag2;
            }
        }

        /// <summary>
        /// CF廠 Photo Line Marco Reserve Flag
        /// </summary>
        [Serializable]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class CFMarcoReserveFlag : ICloneable
        {
            private string _bm = "0";
            private string _r = "0";
            private string _g = "0";
            private string _b = "0";
            private string _oc = "0";
            private string _ps = "0";

            public string BM
            {
                get { return _bm; }
                set { _bm = value; }
            }

            public string R
            {
                get { return _r; }
                set { _r = value; }
            }

            public string G
            {
                get { return _g; }
                set { _g = value; }
            }

            public string B
            {
                get { return _b; }
                set { _b = value; }
            }

            public string OC
            {
                get { return _oc; }
                set { _oc = value; }
            }

            public string PS
            {
                get { return _ps; }
                set { _ps = value; }
            }

            public object Clone()
            {
                CFMarcoReserveFlag cFMarcoReserveFlag = (CFMarcoReserveFlag)this.MemberwiseClone();
                return cFMarcoReserveFlag;
            }
        }


        /// <summary>
        /// CF廠 Photo Line Process Back Up
        /// </summary>
        [Serializable]
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public class CFProcessBackUpFlag : ICloneable
        {
            private string _bm = "0";
            private string _r = "0";
            private string _g = "0";
            private string _b = "0";
            private string _oc = "0";
            private string _ps = "0";

            public string BM
            {
                get { return _bm; }
                set { _bm = value; }
            }

            public string R
            {
                get { return _r; }
                set { _r = value; }
            }

            public string G
            {
                get { return _g; }
                set { _g = value; }
            }

            public string B
            {
                get { return _b; }
                set { _b = value; }
            }

            public string OC
            {
                get { return _oc; }
                set { _oc = value; }
            }

            public string PS
            {
                get { return _ps; }
                set { _ps = value; }
            }

            public object Clone()
            {
                CFProcessBackUpFlag cFProcessBackUp = (CFProcessBackUpFlag)this.MemberwiseClone();
                return cFProcessBackUp;
            }
        }

       /// <summary>
        /// CF廠 All Line Abnormal Code
        /// </summary>
        [Serializable]
        [TypeConverter(typeof(ExpandableObjectConverter))]  
        public class CFAbnormalCode : ICloneable
        {
            private string _arrayPhotoPre_InlineID = "0"; //INT
            private string _coa2maskname = string.Empty;
            private string _ttpflag = string.Empty;
            private string _alnside = string.Empty;
            private string _ovenside = string.Empty;
            private string _vcdside = string.Empty;
            private string _prlot = string.Empty;
            private string _cspnumber = string.Empty;
            private string _hpchamber = string.Empty;
            private string _cpchamber = string.Empty;
            private string _dispensespeed = string.Empty;

            public string ArrayPhotoPre_InlineID
            {
                get { return _arrayPhotoPre_InlineID; }
                set { _arrayPhotoPre_InlineID = value; }
            }

            public string COA2MASKNAME
            {
                get { return _coa2maskname; }
                set { _coa2maskname = value; }
            }

            public string TTPFLAG
            {
                get { return _ttpflag; }
                set { _ttpflag = value; }
            }

            public string ALNSIDE
            {
                get { return _alnside; }
                set { _alnside = value; }
            }

            public string OVENSIDE
            {
                get { return _ovenside; }
                set { _ovenside = value; }
            }

            public string VCDSIDE
            {
                get { return _vcdside; }
                set { _vcdside = value; }
            }

            public string PRLOT
            {
                get { return _prlot; }
                set { _prlot = value; }
            }

            public string CSPNUMBER
            {
                get { return _cspnumber; }
                set { _cspnumber = value; }
            }

            public string HPCHAMBER
            {
                get { return _hpchamber; }
                set { _hpchamber = value; }
            }

            public string CPCHAMBER
            {
                get { return _cpchamber; }
                set { _cpchamber = value; }
            }

            public string DISPENSESPEED
            {
                get { return _dispensespeed; }
                set { _dispensespeed = value; }
            }

            public object Clone()
            {
                CFAbnormalCode cFAbnormalCode = (CFAbnormalCode)this.MemberwiseClone();
                return cFAbnormalCode;
            }
        }

        /// <summary>
        /// CF廠 Short Cut - 當同一個Cassette的第一片玻璃做完下一條Line的Recipe ID Check時，要更新此Flag到所有同一個Cassette的Job
        /// </summary>
        public bool CFShortCutRecipeIDCheckFlag
        {
            get { return _cfShortCutRecipeIDCheckFlag; }
            set { _cfShortCutRecipeIDCheckFlag = value; }
        }

        /// <summary>
        /// CF廠 Short Cut - 當同一個Cassette的第一片玻璃做完下一條Line的Recipe Parameter Check時，要更新此Flag到所有同一個Cassette的Job
        /// </summary>
        public bool CFShortCutRecipeParameterCheckFlag
        {
            get { return _cfShortCutRecipeParameterCheckFlag; }
            set { _cfShortCutRecipeParameterCheckFlag = value; }
        }

        /// <summary>
        /// CF廠 Short Cut - Recipe ID Registe 的結果
        /// </summary> 
        public eRecipeCheckResult CfShortCutRecipeIDCheckResult
        {
            get { return _cfShortCutRecipeIDCheckResult; }
            set { _cfShortCutRecipeIDCheckResult = value; }
        }

        /// <summary>
        /// CF廠 Short Cut - Recipe Parameter Request 的結果
        /// </summary> 
        public eRecipeCheckResult CFShortCutrecipeParameterRequestResult
        {
            get { return _cfShortCutrecipeParameterRequestResult; }
            set { _cfShortCutrecipeParameterRequestResult = value; }
        }

        /// <summary>
        /// CF廠 Short Cut - 確認是否有產生新WIP
        /// </summary>
        public bool CfShortCutWIPCheck
        {
            get { return _cfShortCutWIPCheck; }
            set { _cfShortCutWIPCheck = value; }
        }
            
        /// <summary>
        /// CF廠 Short Cut - 確認是否已經報過 CFShortCutGlassProcessEnd 給 MES
        /// </summary>
        public bool CFShortCutTrackOut
        {
            get { return _cfShortCutTrackOut; }
            set { _cfShortCutTrackOut = value; }
        }

        public object Clone()
        {
            JobCfSpecial cf = (JobCfSpecial)this.MemberwiseClone();

            //cf.EQPFlag = this.EQPFlag.Clone() as eEQPFlag;　//強制轉型，不會報Exception，故不使用此寫法。 
            cf.CFSpecialReserved = (CFCFSpecialReserved)this.CFSpecialReserved.Clone();
            cf.EQPFlag1 = (CFEQPFlag1)this.EQPFlag1.Clone();
            cf.EQPFlag2 = (CFEQPFlag2)this.EQPFlag2.Clone();
            cf.TrackingData = (CFTrackingData)this.TrackingData.Clone();
            cf.InspJudgedData1 = (CFInspJudgedData1)this.InspJudgedData1.Clone();
            cf.InspJudgedData2 = (CFInspJudgedData2)this.InspJudgedData2.Clone();
            cf.InspReservations = (CFINSPReservations)this.InspReservations.Clone();
            cf.EQPReservations = (CFEQPReservations)this.EQPReservations.Clone();
            cf.AbnormalCode = (CFAbnormalCode)this.AbnormalCode.Clone();
            cf.CFMarcoReserve = (CFMarcoReserveFlag)this.CFMarcoReserve.Clone();
            cf.CFProcessBackUp = (CFProcessBackUpFlag)this.CFProcessBackUp.Clone();

            return cf;
        }
    }
}
