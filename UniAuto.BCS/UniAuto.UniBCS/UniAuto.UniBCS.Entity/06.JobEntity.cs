/*****************************************************
 * 20141128  Add Property CreateTime lastUpdateTime For Job Create  and Update  tom 
 * 
 * 
 * 
 * 
 * 
 * ***************************************************/




using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UniAuto.UniBCS.MISC;
using System.ComponentModel;

namespace UniAuto.UniBCS.Entity
{
    /// <summary>
    /// 對應File, 修改Property後呼叫Save(), 會序列化存檔
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]  
    public class Job : EntityFile,ICloneable
    {
        //一個 叫 Job ID，一個叫VCRJobID，另一個叫EQPJobID
        //JobID是指最後，最新取得的ID
        //VCR Job ID是指經VCR讀取的ID，但也要upate Job ID
        //EQP Job ID是指機台每次上報就update，但同時也要update Job ID
        //還有一個MESJob ID
        //這是一開始MES當初download的一個ID

        #region BC Variable
        private string _jobKey = string.Empty;
        private string _vcrJobID = string.Empty;
        private string _eqpJobID = string.Empty;
        private string _sourcePortID = string.Empty;
        private string _targetPortID = string.Empty;
        private string _fromCstID = string.Empty;
        private string _toCstID = string.Empty;
        private string _toSlotNo = "0";
        private string _currentSlotNo = "0";    // add by bruce 20160412 for t2 Issue 
        private string _fromSlotNo = string.Empty;
        private bool _removeFlag = false; //機台做Remove動作時，不能真得刪除wip，只能更新此flag
        private string _removeReason = string.Empty; //Watson Add 20150310 For Remove Reason.
        private bool _removethelastFlag = false; //機台做Remove動作，而且是LastFlag的wip
        private string _currentEQPNo = string.Empty;
        private string _currentUNITNo = string.Empty;
        private string _lineRecipeName = string.Empty;
        private DateTime _jobProcessStartTime = DateTime.Now; //Job Service
        private eVCR_EVENT_RESULT _vCR_Result = eVCR_EVENT_RESULT.NOUSE;
        private List<HoldInfo> _hldInforList = new List<HoldInfo>();
        private string _chamberName = string.Empty;
        private string _hostDefectCodeData = string.Empty;
        private string _targetCSTID = string.Empty;
        private DateTime _createTime = DateTime.Now;
        private string _mMGFlag = "N";
        private string _robortProcFlag =  keyCELLROBOTProcessFlag.NO_PROCESS;
        private bool _robortRTCFlag = false;
        private string _ArrayCrosslineID = string.Empty;    //add by bruce 2015/10/8 for ELA Cross line use
        private bool _ArrayELAOverQtimeFlag = false;        //add by bruce 2015/10/8 for ELA Cross line use
        private bool _ArrayELAOverQtimeWarring = false;        //add by cc.kuang 2016/032 for ELA QTime Warring
        private bool _OEESendFlag = false;        //add by cc.kuang 2016/01/25 for OEE use
        private bool _OEERecvFlag = false;        //add by cc.kuang 2016/01/25 for OEE use
        private DateTime _WaitForProcessTime = DateTime.Now; //add by cc.kuang 2016/03/28 for Robot fetch from CST use
        private bool _TrackingDataBypassHoldFlag = false;
        [Category("BC")]
        public DateTime CreateTime
        {
            get { return _createTime; }
            set { _createTime = value; }
        }
        private DateTime _lastUpdateTime = DateTime.Now;
         [Category("BC")]
        public DateTime LastUpdateTime
        {
            get { return _lastUpdateTime; }
            set { _lastUpdateTime = value; }
        }
        //private ProductType _internalProductType = new ProductType();

          [Category("BC")]
        public string ChamberName
        {
            get { return _chamberName; }
            set { _chamberName = value; }
        } 
        [ReadOnly(true)]
        [Category("BC")]
        public string JobKey
        {
            get { return _jobKey; }
            set { _jobKey = value; }
        }
          [Category("BC")]
        public string VCRJobID
        {
            get { return _vcrJobID; } 
            set { _vcrJobID = value; } 
        }
          [Category("BC")]
        public string EQPJobID
        {
            get { return _eqpJobID; } 
            set { _eqpJobID = value; } 
        }
          [Category("BC")]
        public string SourcePortID
        {
            get { return _sourcePortID; } 
            set { _sourcePortID = value; } 
        }
          [Category("BC")]
        public string TargetPortID
        {
            get { return _targetPortID; } 
            set { _targetPortID = value; } 
        }
          [Category("BC")]
        public string FromCstID
        {
            get { return _fromCstID; } 
            set { _fromCstID = value; } 
        }
          [Category("BC")]
        public string ToCstID
        {
            get { return _toCstID; } 
            set { _toCstID = value; } 
        }
        [Category("BC")]
        public string FromSlotNo
        {
            get { return _fromSlotNo; } 
            set { _fromSlotNo = value; } 
        }
        [Category("BC")]
        public string ToSlotNo
        {
            get { return _toSlotNo; } 
            set { _toSlotNo = value; } 
        }
        [Category("BC")]
        public string CurrentEQPNo
        {
            get { return _currentEQPNo; }
            set { _currentEQPNo = value; }
        }
        [Category("BC")]
        public string CurrentUNITNo
        {
              get { return _currentUNITNo; }
              set { _currentUNITNo = value; }
        }

        /// <summary>
        /// Changer Mode使用
        /// 來源為Changer Plan
        /// </summary>
        [Category("BC")]
        public string TargetCSTID
        {
            get { return _targetCSTID; }
            set { _targetCSTID = value; }
        }

        /// <summary>
        /// 拆解MES的DefectCode, 再組成新的資料寫到File
        /// (以逗號來拆解, 再補足5碼, 再以逗號組成)
        /// </summary>
        [Category("BC")]
        public string HostDefectCodeData
        {
            get { return _hostDefectCodeData; }
            set { _hostDefectCodeData = value; }
        }
        /// <summary>
        /// 機台做Remove動作時，不能真得刪除wip，只能更新此flag
        /// 可能在Regesiter回來
        /// </summary>
        [Category("BC")]
        public bool RemoveFlag
        {
            get { return _removeFlag; } 
            set { _removeFlag = value; } 
        }

        [Category("BC")]
        public string RemoveReason
        {
            get { return _removeReason; }
            set { _removeReason = value; }
        }
        /// <summary>
        /// 機台做Remove動作時，不能真得刪除wip，只能更新此flag
        /// 可能在Regesiter回來
        /// </summary>
        [Category("BC")]
        public bool RemoveandtheLastFlag
        {
            get { return _removethelastFlag; }
            set { _removethelastFlag = value; }
        }
        [Category("BC")]
        public DateTime JobProcessStartTime
        {
            get {return  _jobProcessStartTime; } //Job Service
            set { _jobProcessStartTime = value; } 
        }
        [Category("BC")]
        public eVCR_EVENT_RESULT VCR_Result
        {
            get { return _vCR_Result; }
            set { _vCR_Result = value; }
        }
        /// <summary>
        /// 存儲 Hold Data
        /// </summary>
        [Category("BC")]
        public List<HoldInfo> HoldInforList
        {
			get { return _hldInforList; }
			set { _hldInforList = value; }
        }

        /// <summary>
        /// 目前BCS正在使用的LineRecipeName
        /// </summary>
        [Category("BC")]
        public string LineRecipeName
        {
            get { return _lineRecipeName; }
            set { _lineRecipeName = value; }
        } 

        //public ProductType InternalProductType
        //{
        //    get { return _internalProductType; }
        //    set { _internalProductType = value; }
        //}
        [Category("BC")]
        public string MMGFlag
        {
            get { return _mMGFlag; }
            set { _mMGFlag = value; }
        }
        //Watson Add 20150316 For Both Port Auto Abort command
        [Category("BC")]
        public string RobotProcessFlag
        {
            get { return _robortProcFlag; }
            set { _robortProcFlag = value; }
        }
        //cc.kuang Add 20150925 Robot Service use for glass put to cst tmporarily
        [Category("BC")]
        public bool RobortRTCFlag
        {
            get { return _robortRTCFlag; }
            set { _robortRTCFlag = value; }
        }

        //add by bruce 2015/10/08 Array ELA cross line use
        [Category("BC")]
        public string ArrayELACrossLineID
        {
            get { return _ArrayCrosslineID; }
            set { _ArrayCrosslineID = value; }
        }

        [Category("BC")]
        public bool ArrayELAOverQtimeFlag
        {
            get { return _ArrayELAOverQtimeFlag; }
            set { _ArrayELAOverQtimeFlag = value; }
        }

        [Category("BC")]
        public bool ArrayELAOverQtimeWarring
        {
            get { return _ArrayELAOverQtimeWarring; }
            set { _ArrayELAOverQtimeWarring = value; }
        }

        [Category("BC")]
        public bool OEESendFlag
        {
            get { return _OEESendFlag; }
            set { _OEESendFlag = value; }
        }

        [Category("BC")]
        public bool OEERecvFlag
        {
            get { return _OEERecvFlag; }
            set { _OEERecvFlag = value; }
        }
        
        [Category("BC")]
        public DateTime WaitForProcessTime
        {
            get { return _WaitForProcessTime; }
            set { _WaitForProcessTime = value; }
        }

        //add by bruce 20160328 Abnormal process Tracking Data by pass hold flag
        [Category("BC")]
        public bool TrackingDataBypassHoldFlag  
        {
            get { return _TrackingDataBypassHoldFlag; }
            set { _TrackingDataBypassHoldFlag = value; }
        }

        //add by bruce 20160412 for t2 Issue 
        [Category("BC")]
        public string CurrentSlotNo
        {
            get { return _currentSlotNo; }
            set { _currentSlotNo = value; }
        }
        #endregion

        #region PLC Common
        private string _cassetteSequenceNo = "0";
        private string _jobSequenceNo = "0";
        private string _groupIndex = "0";
        //private string _productType = "0";
        private ProductType _productType = new ProductType();
        private ProductID _productID = new ProductID();

        private eSubstrateType _substrateType = eSubstrateType.Cassette;
        private eBitResult _cimMode = eBitResult.OFF;
        private eJobType _jobType = eJobType.Unknown;
        private eCSTOperationMode _CstOperationMode = eCSTOperationMode.CTOC;

        private string _jobJudge = "0";  //INT
        private int _joblotpriority = 0;//add by hujunpeng 20190617 for array优先run货
        private string _samplingSlotFlag = "0";  //INT
        private string _oxrInformationRequestFlag = "0";  //INT
        private string _firstRunFlag = "0";  //INT
        private string _jobGrade = string.Empty;
        private string _glassChipMaskBlockID = string.Empty;    //modify by bruce 2015/7/7
        private string _subProductName = string.Empty;      //modify by marine 2015/8/21
        private string _glassChipMaskCutID = string.Empty; 
        //private string _hostPPID = string.Empty;
        private string _ppID = string.Empty;
        private string _mesPPID = string.Empty;
        private string _inspReservations = "0";  //BIN
        private string _eqpReservations = "0";  //BIN
        private string _lastGlassFlag = "0";  //BIN
        private bool _aPCLastGlassFlag = false;  //BIN        
        private string _inspJudgedData = "0";  //BIN
        private string _inspJudgedData2 = "0";  //BIN
        private string _trackingData = "0";  //BIN
        private string _eqpFlag = "0";  //BIN
        private string _eqpFlag2 = "0";  //BIN
        private string _cfSpecialReserved = "0";  //BIN
        //private string _cfinspReservations = "0";  //BIN
        //private string _cFEQPReservations = "0";  //BIN
        private string _oxrInformation = string.Empty; //sy modify for 全廠統一用ASCII
        private int _chipCount = 0;
        private string _cfmarcoReserveFlag = "0"; //BIN
        private string _processBackUp = "0"; //BIN

        [ReadOnly(true),Category("PLC")]
        public string CassetteSequenceNo
        {
            get { return _cassetteSequenceNo; }
            set { _cassetteSequenceNo = value; }
        }
        [ReadOnly(true),Category("PLC")]
        public string JobSequenceNo
        {
            get { return _jobSequenceNo; }
            set { _jobSequenceNo = value; }
        }
        [Category("PLC")]
        public string GroupIndex
        {
            get { return _groupIndex; } 
            set { _groupIndex = value; } 
        }
        [Category("PLC")]
        public ProductType ProductType
        {
            get { return _productType; } 
            set { _productType = value; }
        }
        [Category("PLC")]
        public ProductID ProductID
        {
            get { return _productID; }
            set { _productID = value; }
        }
        [Category("PLC")]
        public eSubstrateType SubstrateType
        {
            get { return _substrateType; }
            set { _substrateType = value; } 
        }
        [Category("PLC")]
        public eBitResult CIMMode
        {
            get { return _cimMode; } 
            set { _cimMode = value; } 
        }
        [Category("PLC")]
        public eJobType JobType
        {
            get { return _jobType; } 
            set { _jobType = value; } 
        }
        //目前EQP IO Report "1：Kind To Kind mode  2：Cassette To Cassette mode"
        //但是JOB DATA IO is " 0: Kind to Kind   1: CST to CST"
        [Category("PLC")]
        public eCSTOperationMode CSTOperationMode
        {
            get { return _CstOperationMode; }
            set { _CstOperationMode = value; }
        }
        [Category("PLC")]
        public string JobJudge
        {
            get { return _jobJudge; } 
            set { _jobJudge = value; } 
        }
        [Category("PLC")]
        public int JobLotPriority//add by hujunpeng 20190617
        {
            get { return _joblotpriority; }
            set { _joblotpriority = value; }
        }
        [Category("PLC")]
        public string SamplingSlotFlag
        {
            get { return _samplingSlotFlag; } 
            set { _samplingSlotFlag = value; } 
        }
        [Category("PLC")]
        public string OXRInformationRequestFlag
        {
            get { return _oxrInformationRequestFlag; } 
            set { _oxrInformationRequestFlag = value; } 
        }
        [Category("PLC")]
        public string FirstRunFlag
        {
            get { return _firstRunFlag; } 
            set { _firstRunFlag = value; } 
        }
        [Category("PLC")]
        public string JobGrade
        {
            get
            {
                return _jobGrade.Trim(); // 此处Trim 让ROBOT 比较Port Grade 20150406
            } 
            set { _jobGrade = value; } 
        }
        [Category("PLC")]
        public string GlassChipMaskBlockID  //modiy by bruce 2015/7/7
        {
            get { return _glassChipMaskBlockID; }   //modiy by bruce 2015/7/7
            set { _glassChipMaskBlockID = value; }  //modiy by bruce 2015/7/7
        }
        public string SubProductName  //modiy by marine 2015/7/7
        {
            get { return _subProductName; }   //modiy by marine 2015/7/7
            set { _subProductName = value; }  //modiy by marine 2015/7/7
        }
        public string GlassChipMaskCutID  
        {
            get { return _glassChipMaskCutID; }   
            set { _glassChipMaskCutID = value; }  
        }
        //public string HostPPID
        //{
        //    get { return _hostPPID; }
        //    set { _hostPPID = value; }
        //}

        //沒有分號的PPID FOR EQP  
        //會加跨號的機台如 CF FBMPH, FBRPH, FBGPH, FBBPH, FBSPH, FBWPH 有
        //如FBRPH Line 沒有機台L10, L15 and L20 要補上，要寫
        [Category("PLC")]
        public string PPID                //此ppid會補上虛擬機台及跳號機台要寫給機台的，不能上報
        {
            get { return _ppID; } 
            set { _ppID = value; } 
        }

        /// <summary>
        /// Watson Add For MES PPID 可能在Local Mode是會被改變的
        /// 有分號，要上報的
        /// 但不會加跨號的機台如 CF FBMPH, FBRPH, FBGPH, FBBPH, FBSPH, FBWPH 有
        /// 如FBRPH Line 沒有機台L10, L15 and L20 
        /// </summary>
        [Category("PLC")]
        public string MES_PPID    //此ppid不會補上虛擬機台及跳號機台，但需要上報給MES的
        {
            get { return _mesPPID; }
            set { _mesPPID = value; } 
        }
        [Category("PLC")]
        public string INSPReservations
        {
            get { return _inspReservations; }
            set { _inspReservations = value; }
        }
        [Category("PLC")]
        public string EQPReservations
        {
            get { return _eqpReservations; }
            set { _eqpReservations = value; }
        }
        [Category("PLC")]
        public string LastGlassFlag
        {
            get { return _lastGlassFlag; }
            set { _lastGlassFlag = value; }
        }
        [Category("PLC")]
        public bool APCLastGlassFlag
        {
            get { return _aPCLastGlassFlag; }
            set { _aPCLastGlassFlag = value; }
        }        
        [Category("PLC")]
        public string InspJudgedData
        {
            get { return _inspJudgedData; } 
            set { _inspJudgedData = value; } 
        }
        [Category("PLC")]
        public string InspJudgedData2
        {
            get { return _inspJudgedData2; }
            set { _inspJudgedData2 = value; }
        }
        [Category("PLC")]
        public string TrackingData
        {
            get { return _trackingData; } 
            set { _trackingData = value; } 
        }
        [Category("PLC")]
        public string EQPFlag
        {
            get { return _eqpFlag; }
            set { _eqpFlag = value; }
        }
        [Category("PLC")]
        public string EQPFlag2
        {
            get { return _eqpFlag2; }
            set { _eqpFlag2 = value; }
        }
        [Category("PLC")]
        public string CFSpecialReserved
        {
            get { return _cfSpecialReserved; }
            set { _cfSpecialReserved = value; }
        }
        //[Category("PLC")]
        //public string CFInspReservations
        //{
        //    get { return _cfinspReservations; }
        //    set { _cfinspReservations = value; }
        //}
        //[Category("PLC")]
        //public string CFEQPReservations
        //{
        //    get { return _cFEQPReservations; }
        //    set { _cFEQPReservations = value; }
        //}
        [Category("PLC")]
        public string OXRInformation
        {
            get { return _oxrInformation; } 
            set { _oxrInformation = value; }
        }
        [Category("PLC")]
        public int ChipCount
        {
            get { return _chipCount; } 
            set { _chipCount = value; } 
        }
        [Category("PLC")]
        public string CFMarcoReserveFlag
        {
            get { return _cfmarcoReserveFlag; }
            set { _cfmarcoReserveFlag = value; }
        }
        [Category("PLC")]
        public string CFProcessBackUp
        {
            get { return _processBackUp; }
            set { _processBackUp = value; }
        }
        #endregion

        #region MES
        private MES_CstBody _mesCstBody = new MES_CstBody();
        private PRODUCTc _mesProduct = new PRODUCTc();
        [Category("MES")]
        public MES_CstBody MesCstBody
        {
            get { return _mesCstBody; }
            set { _mesCstBody = value; }
        }
        [Category("MES")]
        public PRODUCTc MesProduct
        {
            get { return _mesProduct; } 
            set { _mesProduct = value; } 
        }
        #endregion

        #region PLC Shop Special Data
        private JobArraySpecial _arraySpecial = new JobArraySpecial();
        private JobCellSpecial _cellSpecial = new JobCellSpecial();
        private JobCfSpecial _cfSpecial = new JobCfSpecial();

        [Category("Array Special")]
        public JobArraySpecial ArraySpecial
        {
            get { return _arraySpecial; } 
            set { _arraySpecial = value; } 
        }
        [Category("Cell Special")]
        public JobCellSpecial CellSpecial
        {
            get { return _cellSpecial; } 
            set { _cellSpecial = value; } 
        }
        [Category("CF Special")]
        public JobCfSpecial CfSpecial
        {
            get { return _cfSpecial; } 
            set { _cfSpecial = value; } 
        }
        #endregion

        #region [ For Robot Use Only ]

        private JobRobot _robotWIP = new JobRobot();

        [Category("Robot WIP")]
        public JobRobot RobotWIP
        {
            get { return _robotWIP; }
            set { _robotWIP = value; } 
        }

        #endregion

        /// <summary>
        /// For FIle Data 使用 20150401 Tom
        /// </summary>
        [Category("FileData")]
        public string OWNERTYPE
        {
            get
            {
                if (string.IsNullOrEmpty(MesProduct.OWNERTYPE))
                    return "";
                else
                {
                    return MesProduct.OWNERTYPE.Length > 0 ? MesProduct.OWNERTYPE.Substring(MesProduct.OWNERTYPE.Length - 1, 1) : "";
                }
            }
        }
        /// <summary>
        /// For FIle Data Use 20150409 Jun
        /// </summary>
        [Category("FileData")]
        public string GLASSTYPE
        {
            get
            {
                string ownerType = string.Empty;
                string ownerID = string.Empty;
                string productType = string.Empty;

                if (string.IsNullOrEmpty(MesProduct.OWNERTYPE))
                    ownerType = " ";
                else
                    ownerType = MesProduct.OWNERTYPE.Length > 0 ? MesProduct.OWNERTYPE.Substring(MesProduct.OWNERTYPE.Length - 1, 1) : " ";

                if (string.IsNullOrEmpty(MesProduct.OWNERID))
                    ownerID = ownerID.PadRight(10, ' ');
                else
                    ownerID = MesProduct.OWNERID.Length < 11 ? MesProduct.OWNERID.PadRight(10, ' ') : MesProduct.OWNERID.Substring(0, 10);

                if (MesCstBody.LOTLIST.Count > 0 && !string.IsNullOrEmpty(MesCstBody.LOTLIST[0].PRODUCTOWNER))
                    productType = MesCstBody.LOTLIST[0].PRODUCTOWNER.Length > 0 ? MesCstBody.LOTLIST[0].PRODUCTOWNER.Substring(0, 1) : " ";
                else
                    productType = " ";

                return string.Format("{0}_{1}_{2}", ownerType, ownerID, productType);
            }
        }
        /// <summary>
        /// For File Data Use 20150421 Tom
        /// 修改 如果MES给空白则File Data也给空白 
        /// </summary>
        [Category("FileData")]
        public string PANELSIZE
        {
            get
            {
                float productSize = 0;
                string pdSize="";
                StringBuilder sb = new StringBuilder();
                if (MesCstBody.LOTLIST.Count > 0 &&　!string.IsNullOrEmpty(MesCstBody.LOTLIST[0].SUBPRODUCTSIZES))
                {
                    bool result = float.TryParse(MesCstBody.LOTLIST[0].SUBPRODUCTSIZES, out productSize);
                    if (result)
                    {
                        productSize  *=100;
                    }
                     pdSize = productSize.ToString().PadLeft(5, '0');
                }
                else
                {
                    for (int i = 0; i < 400; i++)
                    {
                        sb.Append("   ,     ;"); 
                    }
                    return sb.ToString(0, sb.Length - 1);
                }
               
                pdSize = pdSize.Substring(pdSize.Length - 5);
                string[] productNames = null;
                //if (MesCstBody.LOTLIST.Count > 0 && !string.IsNullOrEmpty(MesCstBody.LOTLIST[0].SUBPRODUCTNAMES)) cc.kuang t3 mes has't this item 20150701
                //{
                    //productNames = MesCstBody.LOTLIST[0].SUBPRODUCTNAMES.Split(';');
                //}
                if (false)
                {
                    ;
                }
                else
                {
                    for (int i = 0; i < 400 ; i++)
                    {
                        sb.Append("   ,     ;");
                    }
                    return sb.ToString(0, sb.Length - 1);
                }
                int arrayLen=0;
                if (productNames != null)
                {
                    arrayLen=productNames.Length;
                    for (int i = 0; i < productNames.Length; i++)
                    {
                        string t=productNames[i].PadLeft(3,'0');
                        sb.AppendFormat("{0},{1};", t.Substring(t.Length-3), pdSize);
                    }
                }
                int count=400-arrayLen;
                for (int i = 0; i < 400 - arrayLen; i++)
                {
                    sb.Append("   ,     ;");
                }
                return sb.ToString(0,sb.Length-1);
            }
        }

        [Category("Other")]
        public List<Qtimec> QtimeList { get; set; }
         [Category("Other")]
        private List<DefectCode> _defectCodes=new List<DefectCode>();

        private SerializableDictionary<string, ProcessFlow> _jobProcessFlows = new SerializableDictionary<string, ProcessFlow>();
         [Category("Other")]
        public SerializableDictionary<string, ProcessFlow> JobProcessFlows
        {
            get { return _jobProcessFlows; }
            set { _jobProcessFlows = value; }
        }

         private SerializableDictionary<string, ProcessFlow> _jobProcessFlowsForOEE = new SerializableDictionary<string, ProcessFlow>();
         
        [Category("Other")]
         public SerializableDictionary<string, ProcessFlow> JobProcessFlowsForOEE {
             get {

                 if (_jobProcessFlowsForOEE == null)
                     _jobProcessFlowsForOEE = new SerializableDictionary<string, ProcessFlow>();
                 return _jobProcessFlowsForOEE; 
             }
             set { _jobProcessFlowsForOEE = value; }
         }


         [Category("Other")]
        public List<DefectCode> DefectCodes
        {
            get { return _defectCodes; }
            set { _defectCodes = value; }
        }


        public Job()
        {
            _createTime = _jobProcessStartTime = DateTime.Now;
        }

        public Job(int _cstSeqNo, int _slotNo)
        {
            SetNewJobKey(_cstSeqNo, _slotNo);
        }

        public void SetNewJobKey(int _cstSeqNo, int _slotNo)
        {
            _cassetteSequenceNo = _cstSeqNo.ToString();
            _jobSequenceNo = _slotNo.ToString();
            _jobKey = _cassetteSequenceNo + "_" + JobSequenceNo;
            _filename = string.Format("{0}_{1}.bin", _cassetteSequenceNo, _jobSequenceNo);
            _createTime = _jobProcessStartTime = DateTime.Now;
        }

        public object Clone()
        {
            Job job = (Job)this.MemberwiseClone();

            job._mesCstBody = (MES_CstBody)this.MesCstBody.Clone();
            job.MesProduct = (PRODUCTc)this.MesProduct.Clone();
            job.CfSpecial = (JobCfSpecial)this.CfSpecial.Clone();
            job.CellSpecial = (JobCellSpecial)this.CellSpecial.Clone();
            job.ArraySpecial = (JobArraySpecial)this.ArraySpecial.Clone();

            //for Robot Use
            job.RobotWIP = (JobRobot)this.RobotWIP.Clone();

            job.JobProcessFlows = new SerializableDictionary<string, ProcessFlow>();
            if (this.JobProcessFlows != null)
            {
                foreach (string key in this.JobProcessFlows.Keys)
                {
                    job.JobProcessFlows.Add(key, (ProcessFlow)this.JobProcessFlows[key].Clone());
                }
            }
            job.JobProcessFlowsForOEE = new SerializableDictionary<string, ProcessFlow>();
            if (this.JobProcessFlowsForOEE != null)
            {
                foreach (string key in this.JobProcessFlowsForOEE.Keys)
                {
                    job.JobProcessFlowsForOEE.Add(key, (ProcessFlow)this.JobProcessFlowsForOEE[key].Clone());
                }
            }

            job._hldInforList = new List<HoldInfo>();
            if (_hldInforList  != null)
            {
               
                foreach (HoldInfo info in _hldInforList)
                {
                    job._hldInforList.Add((HoldInfo)info.Clone());
                }
            }
            job._defectCodes = new List<DefectCode>();
            if (_defectCodes != null)
            {
                foreach (DefectCode dc in _defectCodes)
                {
                    job._defectCodes.Add((DefectCode)dc.Clone());
                }
            }           
            job.QtimeList = new List<Qtimec>();
            if (this.QtimeList != null)
            {
                foreach (Qtimec qt in this.QtimeList)
                {
                    job.QtimeList.Add((Qtimec)qt.Clone());
                }
            }
            
            return job;
        }
    }

    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]  
    public class HoldInfo:ICloneable
    {
        private string _nodeNo = "";

        public string NodeNo
        {
            get { return _nodeNo; }
            set { _nodeNo = value; }
        }
        private string _nodeId = "";

        public string NodeID
        {
            get { return _nodeId; }
            set { _nodeId = value; }
        }
        private string _unitNo = "";

        public string UnitNo
        {
            get { return _unitNo; }
            set { _unitNo = value; }
        }
        private string _unitId = "";

        public string UnitID
        {
            get { return _unitId; }
            set { _unitId = value; }
        }
        private string _operatorId = "";

        public string OperatorID
        {
            get { return _operatorId; }
            set { _operatorId = value; }
        }

        private string _holdReason = "";

        public string HoldReason
        {
            get { return _holdReason; }
            set { _holdReason = value; }
        }

        public object Clone()
        {
            HoldInfo holdInfo = (HoldInfo)this.MemberwiseClone();
            return holdInfo;
        }
    }


    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]  
    public class DefectCode:ICloneable
    {
        private string _cstSeqNo;
        private string _jobSeqNo;
        private string _eqpNo;
        private string _unitNo;
        private string _chipPostion;
        private string _defectCodes;

        public string CSTSeqNo
        {
            get { return _cstSeqNo; }
            set { _cstSeqNo = value; }
        }

        public string JobSeqNo
        {
            get { return _jobSeqNo; }
            set { _jobSeqNo = value; }
        }

        public string DefectCodes
        {
            get { return _defectCodes; }
            set { _defectCodes = value; }
        }
        public string EqpNo
        {
            get { return _eqpNo; }
            set { _eqpNo = value; }
        }
        public string UnitNo
        {
            get { return _unitNo; }
            set { _unitNo = value; }
        }
        public string ChipPostion
        {
            get { return _chipPostion; }
            set { _chipPostion = value; }
        }


        public object Clone()
        {
            DefectCode dc = (DefectCode)this.MemberwiseClone();
            return dc;
        }
    }
    //public class Job : Entity
    //{
    //    public JobEntityFile File { get; private set; }

    //    public Job(JobEntityFile file)
    //    {
    //        File = file;
    //    }
    //}
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]  
    public class ProcessFlow:ICloneable
    {
        private string _machineName;
        private DateTime _startTime = DateTime.MinValue;
        private DateTime _endTime = DateTime.MinValue;
        private string _slotNo;

        private SerializableDictionary<string, ProcessFlow> _unitProcessFlows = new SerializableDictionary<string, ProcessFlow>();

        private List<ProcessFlow> _extendUnitProcessFlows = new List<ProcessFlow>();

        public SerializableDictionary<string, ProcessFlow> UnitProcessFlows
        {
            get { return _unitProcessFlows; }
            set { _unitProcessFlows = value; }
        }

        public List<ProcessFlow> ExtendUnitProcessFlows
        {
            get {
                return _extendUnitProcessFlows==null? _extendUnitProcessFlows=new List<ProcessFlow>():_extendUnitProcessFlows; 
            }

            set { _extendUnitProcessFlows = value; }        
        }

        public DateTime StartTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }
        public DateTime EndTime
        {
            get { return _endTime; }
            set { _endTime = value; }
        }

        /// <summary>
        /// MachineName =EquipmentID_UnitID
        /// 没有UnitID
        /// MachineName=EquipemntID
        /// </summary>
        public string MachineName
        {
            get { return _machineName; }
            set { _machineName = value; }
        }

        public string SlotNO
        {
            get { return _slotNo; }
            set { _slotNo = value; }
        }

        public object Clone()
        {
            ProcessFlow flow = (ProcessFlow)this.MemberwiseClone();
            flow.UnitProcessFlows = new SerializableDictionary<string, ProcessFlow>();
            if (this.UnitProcessFlows != null)
            {
                foreach (string key in this.UnitProcessFlows.Keys)
                {
                    flow.UnitProcessFlows.Add(key, (ProcessFlow)this.UnitProcessFlows[key].Clone());
                }
            }
            flow.ExtendUnitProcessFlows = new List<ProcessFlow>();
            if (this.ExtendUnitProcessFlows != null && this.ExtendUnitProcessFlows.Count > 0)
            {
                foreach (ProcessFlow f in this.ExtendUnitProcessFlows)
                {
                    flow.ExtendUnitProcessFlows.Add((ProcessFlow)f.Clone());
                }
            }
            return flow;
        }
    }
}
