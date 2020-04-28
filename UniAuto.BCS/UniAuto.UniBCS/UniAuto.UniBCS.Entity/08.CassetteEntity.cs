using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.ComponentModel;

namespace UniAuto.UniBCS.Entity
{
    /// <summary>
    /// 對應File, 修改Property後呼叫Save(), 會序列化存檔
    /// </summary>
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Cassette : EntityFile
    {
        private string _lineID = string.Empty;
        private string _nodeID = string.Empty;
        private string _nodeNo = string.Empty;
        private string _portID = string.Empty;
        private string _portNo = string.Empty;
        private string _cassetteID = string.Empty;
        private eBoxType _boxType = eBoxType.NODE;  //Add for T3 MES by marine 2015/8/19
        private string _subBoxID = string.Empty;  //Add for T3 MES by marine 2015/8/19
        private string _boxName = string.Empty;  //Add for T3 MES by sy 2015/11/23
        private bool _isBoxed = false;//Add for T3  by sy 2015/12/13
        private string _grade = string.Empty;  //Add for T3 PPK  by sy 2016/1/18
        private string _productType = string.Empty;  //Add for T3 PPK  by sy 2016/1/18
        private string _cassetteSequenceNo = "0";
        private string _lineRecipeName = string.Empty;
        private string _pPID = string.Empty;
        private bool _isProcessed = false;

        private string _trackingData = string.Empty;

        private IList<Job> _jobs = new List<Job>();
        private DateTime _loadTime = DateTime.Now;
        private DateTime _startTime = DateTime.Now;
        private DateTime _endTime = DateTime.Now;
        private eCstControlCmd _cassetteControlCommand = eCstControlCmd.None;
        private MES_CstBody _mes_CstData = new MES_CstBody();
        private OFFLINE_CstBody _offline_CstData = new OFFLINE_CstBody();
        private string _mes_ValidateCassetteReply = string.Empty;
        private string _ldCassetteSettingCode = string.Empty;

        private string _reasonCode = string.Empty;
        private string _reasonText = string.Empty;
        private string _quitCstReasonCode = string.Empty;   // add by bruce 2016/1/1 for Abort or Cancel Cst Bc Download CIM Message use
        private string _crossLineFlag = "N";
        private string _firstGlassCheckReport = "";  //Jun Add 20150404 Dense機台不會報First Glass Check Event
        private bool _empty = false;//add for QPP emptybox by sy
        private bool _coolRunStart = false;

        //Watson Add 20141212 For MES Spec 
        //If Lot started in Offline and ends normal in Online, report ABORTFLAG as blank.
        private bool _isOffLineProcessStarted = true; //內定為True.經LotProcessStart變更為False
        //Watson Add 20150102 For judge CELL DenseBox Processed 
        private eboxReport _cellBoxProcessed = eboxReport.NOProcess;
        //Watson Add 20141212 For MES Spec 
        //If Lot started in Offline and ends normal in Online, report ABORTFLAG as blank.
        [Category("Entity")]
        public bool IsOffLineProcessStarted
        {
            get { return _isOffLineProcessStarted; }
            set { _isOffLineProcessStarted = value; }
        }
        [Category("Entity")]
        public string LineID
        {
            get { return _lineID; }
            set { _lineID = value; }
        }
        [Category("Entity")]
        public string NodeID
        {
            get { return _nodeID; }
            set { _nodeID = value; }
        }
        [Category("Entity")]
        public string NodeNo
        {
            get { return _nodeNo; }
            set { _nodeNo = value; }
        }
        [Category("Entity")]
        public string PortID
        {
            get { return _portID; }
            set { _portID = value; }
        }
        [Category("Entity")]
        public string PortNo
        {
            get { return _portNo; }
            set { _portNo = value; }
        }
        [Category("Entity"), ReadOnly(true)]
        public string CassetteID
        {
            get { return _cassetteID; }
            set { _cassetteID = value; }
        }

        //Add for T3 MES by marine 2015/8/19
        [Category("Entity")]
        public eBoxType eBoxType
        {
            get { return _boxType; }
            set { _boxType = value; }
        }

        //Add for T3 MES by marine 2015/8/19
        [Category("Entity"), ReadOnly(true)]
        public string SubBoxID
        {
            get { return _subBoxID; }
            set { _subBoxID = value; }
        }

        //Add for T3 MES by sy 2015/11/23
        [Category("Entity")]
        public string BoxName
        {
            get { return _boxName; }
            set { _boxName = value; }
        }

        [Category("Entity")]
        public string Grade
        {
            get { return _grade; }
            set { _grade = value; }
        }

        [Category("Entity")]
        public string ProductType
        {
            get { return _productType; }
            set { _productType = value; }
        }

        [Category("Entity"), ReadOnly(true)]
        public string CassetteSequenceNo
        {
            get { return _cassetteSequenceNo; }
            set { _cassetteSequenceNo = value; }
        }

        [Browsable(false)]
        public IList<Job> Jobs
        {
            get { return _jobs; }
            set { _jobs = value; }
        }
        [Category("Entity")]
        public DateTime LoadTime
        {
            get { return _loadTime; }
            set { _loadTime = value; }
        }
        [Category("Entity")]
        public DateTime StartTime
        {
            get { return _startTime; }
            set { _startTime = value; }
        }
        [Category("Entity")]
        public DateTime EndTime
        {
            get { return _endTime; }
            set { _endTime = value; }
        }
        [Category("Entity")]
        public eCstControlCmd CassetteControlCommand
        {
            get { return _cassetteControlCommand; }
            set { _cassetteControlCommand = value; }
        }
        [Category("Entity")]
        public string ReasonCode
        {
            get { return _reasonCode; }
            set { _reasonCode = value; }
        }

        public string QuitCstReasonCode
        {
            get { return _quitCstReasonCode; }
            set { _quitCstReasonCode = value; }
        }

        [Category("Entity")]
        public string ReasonText
        {
            get { return _reasonText; }
            set { _reasonText = value; }
        }
        [Category("Entity")]
        public string LineRecipeName
        {
            get { return _lineRecipeName; }
            set { _lineRecipeName = value; }
        }
        [Category("Entity")]
        public string PPID
        {
            get { return _pPID; }
            set { _pPID = value; }
        }

        [Category("Entity")]
        public bool IsBoxed
        {
            get { return _isBoxed; }
            set { _isBoxed = value; }
        }
        /// <summary>
        /// Cst 是否開始抽片 
        /// </summary>
        [Category("Entity")]
        public bool IsProcessed
        {
            get { return _isProcessed; }
            set { _isProcessed = value; }
        }
        [Category("MES")]
        public MES_CstBody MES_CstData
        {
            get { return _mes_CstData; }
            set { _mes_CstData = value; }
        }
        [Category("Offline MES")]
        public OFFLINE_CstBody OFFLINE_CstData
        {
            get { return _offline_CstData; }
            set { _offline_CstData = value; }
        }
        [Category("MES")]
        public string Mes_ValidateCassetteReply
        {
            get { return _mes_ValidateCassetteReply; }
            set { _mes_ValidateCassetteReply = value; }
        }
        [Category("Entity")]
        public string LDCassetteSettingCode
        {
            get { return _ldCassetteSettingCode; }
            set { _ldCassetteSettingCode = value; }
        }
        [Category("Entity")]
        public string CrossLineFlag
        {
            get { return _crossLineFlag; }
            set { _crossLineFlag = value; }
        }
        [Category("Entity")]
        public string FirstGlassCheckReport
        {
            get { return _firstGlassCheckReport; }
            set { _firstGlassCheckReport = value; }
        }
        [Category("Entity")]
        public eboxReport CellBoxProcessed
        {
            get { return _cellBoxProcessed; }
            set { _cellBoxProcessed = value; }
        }
        [Category("Entity")]
        public string TrackingData
        {
            get { return _trackingData; }
            set { _trackingData = value; }
        }
        [Category("Entity")]
        public bool CoolRunStart
        {
            get { return _coolRunStart; }
            set { _coolRunStart = value; }
        }
        [Category("Entity")]
        public bool Empty
        {
            get { return _empty; }
            set { _empty = value; }
        }
        public object Clone()
        {
            Cassette cassette = (Cassette)this.MemberwiseClone();
            return cassette;
        }
    }
}
