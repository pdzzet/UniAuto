using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;

namespace UniAuto.UniBCS.Entity
{
	/// <summary>
	/// 對應File, 修改Property後呼叫Save(), 會序列化存檔
	/// </summary>
	[Serializable]
	public class PortEntityFile : EntityFile
	{
        private ePortStatus _status = ePortStatus.UN;
        private eCassetteStatus _cassetteStatus = eCassetteStatus.UNKNOWN;
        private eCompletedCassetteData _completedCassetteData = eCompletedCassetteData.Unknown;
        private eLoadingCstType _loadingCassetteType = eLoadingCstType.Unknown;
        private eQTime _qTimeFlag = eQTime.Unknown;
        private eParitalFull _partialFullFlag = eParitalFull.Unknown;
        private eBACV_ByPass _BACVByPassFlag = eBACV_ByPass.Unknown;
        private eDistortion _distortionFlag = eDistortion.Unknown;
        private eDirection _directionFlag = eDirection.Unknown;
        private eGlassExist _glassExist = eGlassExist.Unknown;
        //private eCstCmdRetCode _CstCmdRetCode = eCstCmdRetCode.Unknown;
        private ePortType _type = ePortType.Unknown;
        private ePortMode _mode = ePortMode.Unknown;
        private ePortTransferMode _transferMode = ePortTransferMode.Unknown;
        private ePortEnableMode _enableMode = ePortEnableMode.Unknown;
        private ePortDown _downStatus = ePortDown.Normal;
        private eOPISubCstState _oPI_SubCstState = eOPISubCstState.NONE;


        private string _cassetteSequenceNo = "0";
        private string _cassetteID = string.Empty;
        private string _jobCountInCassette = "0";
        private string _productquantity = "0";
        private string _plannedquantity = "0";
        private string _startByCount = "0";
        private string _plannedsourcepart = string.Empty;
        private string _plannedgroupname = string.Empty;    //2015/9/9 add by Frank for CF
        private string _operationID = string.Empty;
        private IList<string> _BOXIDS = new List<string>(); //Add By Yangzhenteng
        private string _jobExistenceSlot; //= new String('0', 32);
        private bool[] _arrayJobExistenceSlot;// = new bool[32];
        private string _cassetteSetCode = string.Empty;
        private string _productType = "0";
        private ePortOperMode _opermode = ePortOperMode.Unknown;  //Cell PK/UPK Port
        private string _grade = string.Empty;
        private string _processType = "0";  //add by bruce 2015/7/22 by Port Report 
        private string _processTime = "0"; //add by bruce 2015/7/22 by ELA report
        private string _cassetteCleanPPID = string.Empty; //add by bruce 2015/10/7 by CAC line use
        //private List<ProductID> _waitUseProductID = new List<ProductID>();
        private string _plannedCassetteID = string.Empty; //add by cc.kuang 2015/07/27 for changer plan LDRQ CST ID
        private int _samplingCount = 0;
        //Cell Special
        private eCompleteCassetteReason _completedCassetteReason = eCompleteCassetteReason.Normal;
        private eBitResult _dpiSampligFlag = eBitResult.OFF;
        private eBitResult _autoClaveByPass = eBitResult.OFF;
        private eBitResult _cellCst = eBitResult.OFF;
        private string _mappingGrade = string.Empty;
        private string _useGrade = string.Empty;
        private string _virtualPortMode = "0";
        private string _maxSlotCount = "0";
        private string _portInterlockStatus = "0";
        private string _portIntelockNo = "";
        private eBoxType _boxType = eBoxType.NODE;
        private ePalletMode _portPackMode = ePalletMode.UNKNOWN;
        private string _portBoxID1 = string.Empty;
        private string _portBoxID2 = string.Empty;
        private string _portUnPackSource = "0";
        private string _portDBDataRequest = "0";
        private eCELLPortAssignment _portAssignment = eCELLPortAssignment.UNKNOW;
        private bool _empty = false;//add for QPP emptybox by sy      
        private string _mes_ValidateBoxReply = string.Empty;
        private int _robotWaitProcCount = 0;  //Waton add 20150317 For CELL PMT Both Port Auto Abort.
        private DateTime _storeInDateTime = DateTime.MinValue;

        private DateTime _PortLastGlassFetchOutTime = DateTime.MinValue; // Add By Yangzhenteng20191108

        private List<Sorter> _sorterJobgrade = new List<Sorter>();
        private int _fetchJobCount = 0;//add by hujunpeng 20190725 for CVD700 混run，Deng，20190823

        public List<Sorter> SorterJobGrade
        {
            get { return _sorterJobgrade; }
            set { _sorterJobgrade = value; }
        }

        public DateTime StoreInDateTime
        {
            get { return _storeInDateTime; }
            set { _storeInDateTime = value; }
        }

        //Add By Yangzhenteng 20191108 For CVD700 Glass Fetch Out 
        public DateTime PortLastGlassFetchOutTime
        {
            get { return _PortLastGlassFetchOutTime; }
            set { _PortLastGlassFetchOutTime = value; }
        }

        public ePalletMode PortPackMode
        {
            get { return _portPackMode; }
            set { _portPackMode = value; }
        }
        public eBoxType BoxType
        {
            get { return _boxType; }
            set { _boxType = value; }
        }
        public string PortBoxID1
        {
            get { return _portBoxID1; }
            set { _portBoxID1 = value; }
        }
        public string PortBoxID2
        {
            get { return _portBoxID2; }
            set { _portBoxID2 = value; }
        }
        public string PortUnPackSource
        {
            get { return _portUnPackSource; }
            set { _portUnPackSource = value; }
        }
        public string PortDBDataRequest
        {
            get { return _portDBDataRequest; }
            set { _portDBDataRequest = value; }
        }
        public string Mes_ValidateBoxReply
        {
            get { return _mes_ValidateBoxReply; }
            set { _mes_ValidateBoxReply = value; }
        }
        public eCELLPortAssignment PortAssignment
        {
            get { return _portAssignment; }
            set { _portAssignment = value; }
        }        
        public bool Empty
        {
            get { return _empty; }
            set { _empty = value; }
        }
        //Waton add 20150317 For CELL PMT Both Port Auto Abort.
        public int  RobotWaitProcCount  
        {
            get { return _robotWaitProcCount; }
            set { _robotWaitProcCount = value; }
        }

        public string PortIntelockNo
        {
            get { return _portIntelockNo; }
            set { _portIntelockNo = value; }
        }

        public string PortInterlockStatus
        {
            get { return _portInterlockStatus; }
            set { _portInterlockStatus = value; }
        }

        public ePortStatus Status
        {
            get { return _status; }
            set { _status = value; }
        }

        public eCassetteStatus CassetteStatus
        {
            get { return _cassetteStatus; }
            set { _cassetteStatus = value; }
        }

        public string CassetteSequenceNo
        {
            get { return _cassetteSequenceNo; }
            set { _cassetteSequenceNo = value; }
        }

        public string CassetteID
        {
            get { return _cassetteID; }
            set { _cassetteID = value; }
        }

        public string JobCountInCassette
        {
            get { return _jobCountInCassette; }
            set { _jobCountInCassette = value; }
        }

        public string ProductQuantity
        {
            get { return _productquantity; }
            set { _productquantity = value; }
        }

        public string PlannedQuantity
        {
            get { return _plannedquantity; }
            set { _plannedquantity = value; }
        }
        
        public string StartByCount
        {
            get { return _startByCount; }
            set { _startByCount = value; }
        }

        public string PlannedSourcePart
        {
            get { return _plannedsourcepart; }
            set { _plannedsourcepart = value; }
        }
        //2015/9/9 add by Frank for CF
        public string PlannedGroupName
        {
            get { return _plannedgroupname; }
            set { _plannedgroupname = value; }
        }

        public eCompletedCassetteData CompletedCassetteData
        {
            get { return _completedCassetteData; }
            set { _completedCassetteData = value; }
        }

        public string OperationID
        {
            get { return _operationID; }
            set { _operationID = value; }
        }
       public IList<string> BoxIDList
       {
            get { return _BOXIDS; }
            set { _BOXIDS = value; }
        }  // Add By Yangzhenteng

        public string ProductType
        {
            get { return _productType; }
            set { _productType = value; }
        }

        public string JobExistenceSlot
        {
            get { return _jobExistenceSlot; }
            set 
            { 
                _jobExistenceSlot = value;

                char[] existenceSlot = _jobExistenceSlot.ToCharArray();
                for (int i = 0; i < _arrayJobExistenceSlot.Length; i++)
                {
                    _arrayJobExistenceSlot[i] = existenceSlot[i].Equals('1');
                }
            }
        }

        public bool[] ArrayJobExistenceSlot
        {
            get { return _arrayJobExistenceSlot; }
            set
            {
                _arrayJobExistenceSlot = value;
                string tmp = string.Empty;
                for (int i = 0; i < _arrayJobExistenceSlot.Length; i++)
                {
                    tmp += _arrayJobExistenceSlot[i] ? "1" : "0";
                }
                _jobExistenceSlot = tmp;
            }
        }

        //public List<ProductID> WaitUseProductID
        //{
        //    get { return _waitUseProductID; }
        //    set { _waitUseProductID = value; }
        //}

        public eLoadingCstType LoadingCassetteType
        {
            get { return _loadingCassetteType; }
            set { _loadingCassetteType = value; }
        }

        public eQTime QTimeFlag
        {
            get { return _qTimeFlag; }
            set { _qTimeFlag = value; }
        }

        public eParitalFull PartialFullFlag
        {
            get { return _partialFullFlag; }
            set { _partialFullFlag = value; }
        }

        public eBACV_ByPass BACVByPassFlag
        {
            get { return _BACVByPassFlag; }
            set { _BACVByPassFlag = value; }
        }
        /// <summary>
        /// CST Cleaner 
        /// </summary>
        public eDistortion DistortionFlag
        {
            get { return _distortionFlag; }
            set { _distortionFlag = value; }
        }
        /// <summary>
        /// CST Cleaner 
        /// </summary>
        public eDirection DirectionFlag
        {
            get { return _directionFlag; }
            set { _directionFlag = value; }
        }
        /// <summary>
        /// CST Cleaner 
        /// </summary>
        public eGlassExist GlassExist
        {
            get { return _glassExist; }
            set { _glassExist = value; }
        }

        public ePortType Type
        {
            get { return _type; }
            set { _type = value; }
        }

        public ePortMode Mode
        {
            get { return _mode; }
            set { _mode = value; }
        }

        public ePortTransferMode TransferMode
        {
            get { return _transferMode; }
            set { _transferMode = value; }
        }

        public ePortEnableMode EnableMode
        {
            get { return _enableMode; } 
            set { _enableMode = value; } 
        }

        public string CassetteSetCode
        {
            get { return _cassetteSetCode; }
            set { _cassetteSetCode = value; }
        }

        public ePortDown DownStatus
        {
            get { return _downStatus; }
            set { _downStatus = value; }
        }

        public eOPISubCstState OPI_SubCstState
        {
            get { return _oPI_SubCstState; }
            set { _oPI_SubCstState = value; }
        }

        public ePortOperMode OperMode //Cell PK/UPK Port
        {
            get { return _opermode; }
            set { _opermode = value; }
        }

        public string Grade
        {
            get { return _grade; }
            set { _grade = value; }
        }

        public eCompleteCassetteReason CompletedCassetteReason
        {
            get { return _completedCassetteReason; }
            set { _completedCassetteReason = value; }
        }

        public eBitResult DPISampligFlag
        {
            get { return _dpiSampligFlag; }
            set { _dpiSampligFlag = value; }
        }

        public eBitResult AutoClaveByPass
        {
            get { return _autoClaveByPass; }
            set { _autoClaveByPass = value; }
        }

        public eBitResult CellCst
        {
            get { return _cellCst; }
            set { _cellCst = value; }
        }

        public string MappingGrade
        {
            get { return _mappingGrade; }
            set { _mappingGrade = value; }
        }

        public string UseGrade
        {
            get { return _useGrade; }
            set { _useGrade = value; }
        }

        public string VirtualPortMode
        {
            get { return _virtualPortMode; }
            set { _virtualPortMode = value; }
        }

        public string MaxSlotCount
        {
            get { return _maxSlotCount; }
            set { _maxSlotCount = value; }
        }
        public int SamplingCount
        {
            get { return _samplingCount; }
            set { _samplingCount = value; }
        }

        public PortEntityFile() { }

        public PortEntityFile(int maxCount)
        {
            _jobExistenceSlot = new String('0', maxCount);
            _arrayJobExistenceSlot = new bool[maxCount];
            for (int i = 0; i < maxCount; i++)
            {
                _arrayJobExistenceSlot[i] = false;
            }
        }

        public string ProcessType
        {
            get { return _processType; }
            set { _processType = value; }
        }

        public string ProcessTime
        {
            get { return _processTime; }
            set { _processTime = value; }
        }

        public string CassetteCleanPPID
        {
            get { return _cassetteCleanPPID; }
            set { _cassetteCleanPPID = value; }
        }

        public string PlannedCassetteID
        {
            get { return _plannedCassetteID; }
            set { _plannedCassetteID = value; }
        }

        ///// <summary>
        ///// RedimArrayJobExis
        ///// </summary>
        ///// <param name="maxCount"></param>
        //public void RedimArrayJoExistenceSlot(int maxCount)
        //{
        //    bool[] arrayJobExisSlot = new bool[maxCount];
        //    if (_arrayJobExistenceSlot != null)
        //    {
        //        int min = Math.Min(arrayJobExisSlot.Length, _arrayJobExistenceSlot.Length);
        //        Array.Copy(_arrayJobExistenceSlot,0, arrayJobExisSlot,0, min);
        //        _arrayJobExistenceSlot = arrayJobExisSlot;
        //    }
        //}
	}

    public class Port : Entity
	{
		public PortEntityData Data { get; private set; }

		public PortEntityFile File { get; private set; }

		public Port(PortEntityData data, PortEntityFile file)
		{
			Data = data;
			File = file;
		}
	}

    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))] 
    public class Sorter:ICloneable
    {
        private string _sorterGrade = string.Empty;
        private int _gradeCount = 0;
        private int _productType = 0;
        private int _priorityGrade = 0;
        private bool _toMes = false;


        public Sorter(string SorterGrade,int GradeCount, int ProductType, int PriorityGrade)
        {
            // TODO: Complete member initialization
            this._sorterGrade = SorterGrade;
            this._gradeCount = GradeCount;
            this._productType = ProductType;
            this._priorityGrade = PriorityGrade;
            this._toMes = ToMES;
        }

        public string SorterGrade
        {
            get { return _sorterGrade; }
            set { _sorterGrade = value; }
        }

        public int GradeCount
        {
            get { return _gradeCount; }
            set { _gradeCount = value; }
        }

        public int ProductType
        {
            get { return _productType; }
            set { _productType = value; }
        }

        public int PriorityGrade
        {
            get { return _priorityGrade; }
            set { _priorityGrade = value; }
        }

        public bool ToMES
        {
            get { return _toMes; }
            set { _toMes = value; }
        }

        public object Clone()
        {
            Sorter st = (Sorter)this.MemberwiseClone();
            return st;
        }
    }  
}
