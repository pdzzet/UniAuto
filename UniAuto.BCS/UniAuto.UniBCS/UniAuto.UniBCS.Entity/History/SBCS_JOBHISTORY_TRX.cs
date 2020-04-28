using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBCSJOBHISTORYTRX

	/// <summary>
	/// SBCSJOBHISTORYTRX object for NHibernate mapped table 'SBCS_JOBHISTORY_TRX'.
	/// </summary>
    public class JOBHISTORY : EntityData
	{
		#region Member Variables
		
		protected long _id;
        protected DateTime _uPDATETIME = DateTime.Now;
		protected string _eVENTNAME;
		protected int _cASSETTESEQNO;
		protected int _jOBSEQNO;
		protected string _jOBID;
		protected int _gROUPINDEX;
		protected int _pRODUCTTYPE;
		protected string _cSTOPERATIONMODE;
		protected string _sUBSTRATETYPE;
		protected string _cIMMODE;
		protected string _jOBTYPE;
		protected string _jOBJUDGE;
		protected string _sAMPLINGSLOTFLAG;
		protected string _oXRINFORMATIONREQUESTFLAG;
		protected string _fIRSTRUNFLAG;
		protected string _jOBGRADE;
		protected string _pPID;
		protected string _iNSPRESERVATIONS;
		protected string _lASTGLASSFLAG;
		protected string _iNSPJUDGEDDATA;
		protected string _tRACKINGDATA;
		protected string _eQPFLAG;
		protected string _oXRINFORMATION;
		protected int _cHIPCOUNT;
        protected string _nODENO;
        protected string _uNITNO;
        protected string _pORTNO;
        protected string _sLOTNO;
        protected string _nODEID;
        protected string _sourceCassetteid;
        protected string _currentCassetteid;
        protected string _pathNo;// for  LinkSignal Path NO 20150406 tom
        //Watson Add 20150424 For New CSOT 
        protected string _vCR_GlassID;
        protected string _vcrNO;
        protected string _vcr_Result;
        protected string _chip_Name;
        protected string _target_CasSettingCode;
        protected string _aBNORMALCODE;
        protected string _runMode;
        protected string _turn_Angle;
        protected string _pRODUCTSPECNAME;
        protected string _pRODUCTSPECVER;
        protected string _pROCESSFLOWNAME;
        protected string _pROCESSOPERATIONNAME;
        protected string _pRODUCTOWNER;
        protected string _pRODUCTSIZE;
        protected string _lINERECIPENAME;
        protected string _nODESTACK;
        protected string _pRODUCTNAME;
        protected string _group_ID;
        protected string _owner_Type;
        protected string _cOAVERSION;
        protected string _sAMPLINGVALUE;
        protected string _tARGETCASSETTEID;
        protected string _uNITID;
        protected string _tRANSACTIONID;
        #endregion

		#region Constructors

		public JOBHISTORY() { }
        
        public JOBHISTORY(DateTime uPDATETIME, string eVENTNAME, int cASSETTESEQNO, int jOBSEQNO, string jOBID, int gROUPINDEX, int pRODUCTTYPE, string cSTOPERATIONMODE, string sUBSTRATETYPE, string cIMMODE, string jOBTYPE, string jOBJUDGE, string sAMPLINGSLOTFLAG, string oXRINFORMATIONREQUESTFLAG, string fIRSTRUNFLAG, string jOBGRADE, string pPID, string iNSPRESERVATIONS, string lASTGLASSFLAG, string iNSPJUDGEDDATA, string tRACKINGDATA, string eQPFLAG, string oXRINFORMATION, int cHIPCOUNT,string nODENO,string uNITNO,string pORTNO,string sLOTNO,string nODEID, string _vCR_GlassID
            , string _vcrNO, string _vcr_Result, string _chip_Name, string _target_CasSettingCode, string _aBNORMALCODE, string _runMode, string _turn_Angle, string _pRODUCTSPECNAME, string _pRODUCTSPECVER, string _pROCESSFLOWNAME, string _pROCESSOPERATIONNAME, string _pRODUCTOWNER, string _pRODUCTSIZE, string _lINERECIPENAME, string _nODESTACK, string _pRODUCTNAME, string _group_ID, string _owner_Type, string cOAVERSION, string sAMPLINGVALUE, string tARGETCASSETTEID, string uNITID, string tRANSACTIONID)
		{
			this._uPDATETIME = uPDATETIME;
			this._eVENTNAME = eVENTNAME;
			this._cASSETTESEQNO = cASSETTESEQNO;
			this._jOBSEQNO = jOBSEQNO;
			this._jOBID = jOBID;
			this._gROUPINDEX = gROUPINDEX;
			this._pRODUCTTYPE = pRODUCTTYPE;
			this._cSTOPERATIONMODE = cSTOPERATIONMODE;
			this._sUBSTRATETYPE = sUBSTRATETYPE;
			this._cIMMODE = cIMMODE;
			this._jOBTYPE = jOBTYPE;
			this._jOBJUDGE = jOBJUDGE;
			this._sAMPLINGSLOTFLAG = sAMPLINGSLOTFLAG;
			this._oXRINFORMATIONREQUESTFLAG = oXRINFORMATIONREQUESTFLAG;
			this._fIRSTRUNFLAG = fIRSTRUNFLAG;
			this._jOBGRADE = jOBGRADE;
			this._pPID = pPID;
			this._iNSPRESERVATIONS = iNSPRESERVATIONS;
			this._lASTGLASSFLAG = lASTGLASSFLAG;
			this._iNSPJUDGEDDATA = iNSPJUDGEDDATA;
			this._tRACKINGDATA = tRACKINGDATA;
			this._eQPFLAG = eQPFLAG;
			this._oXRINFORMATION = oXRINFORMATION;
			this._cHIPCOUNT = cHIPCOUNT;
            this._nODENO = NODENO;
            this._uNITNO= uNITNO;
            this._pORTNO = pORTNO;
            this._sLOTNO = sLOTNO;
            this._nODEID = nODEID;
            this._vCR_GlassID = _vCR_GlassID;
            this._vcrNO = _vcrNO;
            this._vcr_Result = _vcr_Result;
            this._chip_Name = _chip_Name;
            this._target_CasSettingCode = _target_CasSettingCode;
            this._aBNORMALCODE = _aBNORMALCODE;
            this._runMode = _runMode;
            this._turn_Angle = _turn_Angle;
            this. _pRODUCTSPECNAME = _pRODUCTSPECNAME;
            this._pRODUCTSPECVER = _pRODUCTSPECVER;
            this._pROCESSFLOWNAME = _pROCESSFLOWNAME;
            this._pROCESSOPERATIONNAME = _pROCESSOPERATIONNAME ;
            this._pRODUCTOWNER = _pRODUCTOWNER;
            this. _pRODUCTSIZE = _pRODUCTSIZE;
            this._lINERECIPENAME = _lINERECIPENAME;
            this._nODESTACK = _nODESTACK;
            this._pRODUCTNAME = _pRODUCTNAME;
            this._group_ID = _group_ID;
            this._owner_Type = _owner_Type;
            this._cOAVERSION = cOAVERSION;
            this._sAMPLINGVALUE = sAMPLINGVALUE;
            this._tARGETCASSETTEID = tARGETCASSETTEID;
            this._uNITID = uNITID;
            this._tRANSACTIONID = tRANSACTIONID;
        }

		#endregion

		#region Public Properties

		public virtual long Id
		{
			get {return _id;}
			set {_id = value;}
		}

		public virtual DateTime UPDATETIME
		{
			get { return _uPDATETIME; }
			set { _uPDATETIME = value; }
		}

		public virtual string EVENTNAME
		{
			get { return _eVENTNAME; }
			set
			{				
				_eVENTNAME = value;
			}
		}

		public virtual int CASSETTESEQNO
		{
			get { return _cASSETTESEQNO; }
			set { _cASSETTESEQNO = value; }
		}

		public virtual int JOBSEQNO
		{
			get { return _jOBSEQNO; }
			set { _jOBSEQNO = value; }
		}

		public virtual string JOBID
		{
			get { return _jOBID; }
			set
			{				
				_jOBID = value;
			}
		}

		public virtual int GROUPINDEX
		{
			get { return _gROUPINDEX; }
			set { _gROUPINDEX = value; }
		}

		public virtual int PRODUCTTYPE
		{
			get { return _pRODUCTTYPE; }
			set { _pRODUCTTYPE = value; }
		}

		public virtual string CSTOPERATIONMODE
		{
			get { return _cSTOPERATIONMODE; }
			set
			{				
				_cSTOPERATIONMODE = value;
			}
		}

		public virtual string SUBSTRATETYPE
		{
			get { return _sUBSTRATETYPE; }
			set
			{				
				_sUBSTRATETYPE = value;
			}
		}

		public virtual string CIMMODE
		{
			get { return _cIMMODE; }
			set
			{				
				_cIMMODE = value;
			}
		}

		public virtual string JOBTYPE
		{
			get { return _jOBTYPE; }
			set
			{				
				_jOBTYPE = value;
			}
		}

		public virtual string JOBJUDGE
		{
			get { return _jOBJUDGE; }
			set
			{				
				_jOBJUDGE = value;
			}
		}

		public virtual string SAMPLINGSLOTFLAG
		{
			get { return _sAMPLINGSLOTFLAG; }
			set
			{				
				_sAMPLINGSLOTFLAG = value;
			}
		}

		public virtual string OXRINFORMATIONREQUESTFLAG
		{
			get { return _oXRINFORMATIONREQUESTFLAG; }
			set
			{				
				_oXRINFORMATIONREQUESTFLAG = value;
			}
		}

		public virtual string FIRSTRUNFLAG
		{
			get { return _fIRSTRUNFLAG; }
			set
			{				
				_fIRSTRUNFLAG = value;
			}
		}

		public virtual string JOBGRADE
		{
			get { return _jOBGRADE; }
			set
			{				
				_jOBGRADE = value;
			}
		}

		public virtual string PPID
		{
			get { return _pPID; }
			set
			{				
				_pPID = value;
			}
		}

		public virtual string INSPRESERVATIONS
		{
			get { return _iNSPRESERVATIONS; }
			set
			{				
				_iNSPRESERVATIONS = value;
			}
		}

		public virtual string LASTGLASSFLAG
		{
			get { return _lASTGLASSFLAG; }
			set
			{				
				_lASTGLASSFLAG = value;
			}
		}

		public virtual string INSPJUDGEDDATA
		{
			get { return _iNSPJUDGEDDATA; }
			set
			{				
				_iNSPJUDGEDDATA = value;
			}
		}

		public virtual string TRACKINGDATA
		{
			get { return _tRACKINGDATA; }
			set
			{				
				_tRACKINGDATA = value;
			}
		}

		public virtual string EQPFLAG
		{
			get { return _eQPFLAG; }
			set
			{				
				_eQPFLAG = value;
			}
		}

		public virtual string OXRINFORMATION
		{
			get { return _oXRINFORMATION; }
			set
			{				
				_oXRINFORMATION = value;
			}
		}

		public virtual int CHIPCOUNT
		{
			get { return _cHIPCOUNT; }
			set { _cHIPCOUNT = value; }
		}

		public virtual string NODENO
        {
            get { return _nODENO; }
			set { _nODENO = value; }
        }

        public virtual string UNITNO
        {
            get { return _uNITNO; }
            set { _uNITNO = value; }
        }

        public virtual string PORTNO
        {
            get { return _pORTNO; }
            set { _pORTNO = value; }
        }

        public virtual string SLOTNO
        {
            get { return _sLOTNO; }
            set { _sLOTNO = value; }
        }

        public virtual string NODEID
        {
            get { return _nODEID; }
            set { _nODEID = value; }
        }

        public virtual string SOURCECASSETTEID
        {
            get { return _sourceCassetteid; }
            set { _sourceCassetteid = value; ; }

        }

        public virtual string CURRENTCASSETTEID
        {
            get { return _currentCassetteid; }
            set { _currentCassetteid = value; }
        }

        public virtual string PATHNO
        {
            get { return _pathNo; }
            set { _pathNo = value; }
        }

        public virtual string VCRREADGLASSID
        {
            get { return _vCR_GlassID; }
            set { _vCR_GlassID = value; }
        }
        public virtual string VCRNO
        {
            get { return _vcrNO; }
            set { _vcrNO = value; }
        }
        public virtual string VCRRESULT
        {
            get { return _vcr_Result; }
            set { _vcr_Result = value; }
        }
        public virtual string CHIPNAME
        {
            get { return _chip_Name; }
            set { _chip_Name = value; }
        }
        public virtual string TARGETCASSETTESETTINGCODE
        {
            get { return _target_CasSettingCode; }
            set { _target_CasSettingCode = value; }
        }
        public virtual string ABNORMALCODE
        {
            get { return _aBNORMALCODE; }
            set { _aBNORMALCODE = value; }
        }
        public virtual string RUNMODE
        {
            get { return _runMode; }
            set { _runMode = value; }
        }
        public virtual string TURNANGLE
        {
            get { return _turn_Angle; }
            set { _turn_Angle = value; }
        }   
        public virtual string PRODUCTSPECNAME
        {
            get { return _pRODUCTSPECNAME; }
            set { _pRODUCTSPECNAME = value; }
        }
        public virtual string PRODUCTSPECVER
        {
            get { return _pRODUCTSPECVER; }
            set { _pRODUCTSPECVER = value; }
        }
        public virtual string PROCESSFLOWNAME
        {
            get { return _pROCESSFLOWNAME; }
            set { _pROCESSFLOWNAME = value; }
        }
        public virtual string PROCESSOPERATIONNAME
        {
            get { return _pROCESSOPERATIONNAME; }
            set { _pROCESSOPERATIONNAME = value; }
        }
        public virtual string PRODUCTOWNER
        {
            get { return _pRODUCTOWNER; }
            set { _pRODUCTOWNER = value; }
        }

        public virtual string PRODUCTSIZE
        {
            get { return _pRODUCTSIZE; }
            set { _pRODUCTSIZE = value; }
        }

        public virtual string LINERECIPENAME
        {
            get { return _lINERECIPENAME; }
            set { _lINERECIPENAME = value; }
        }

        public virtual string NODESTACK
        {
            get { return _nODESTACK; }
            set { _nODESTACK = value; }
        }

        public virtual string PRODUCTNAME
        {
            get { return _pRODUCTNAME; }
            set { _pRODUCTNAME = value; }
        }

        public virtual string GROUPID
        {
            get { return _group_ID; }
            set { _group_ID = value; }
        }

        public virtual string OWNERTYPE
        {
            get { return _owner_Type; }
            set { _owner_Type = value; }
        }

        public virtual string COAVERSION
        {
            get { return _cOAVERSION; }
            set { _cOAVERSION = value; }
        }

        public virtual string SAMPLINGVALUE
        {
            get { return _sAMPLINGVALUE; }
            set { _sAMPLINGVALUE = value; }
        }

        public virtual string TARGETCASSETTEID
        {
            get { return _tARGETCASSETTEID; }
            set { _tARGETCASSETTEID = value; }
        }

        public virtual string UNITID
        {
            get { return _uNITID; }
            set { _uNITID = value; }
        }

        public virtual string TRANSACTIONID
        {
            get { return _tRANSACTIONID; }
            set { _tRANSACTIONID = value; }
        }
		#endregion
	}
	#endregion
}