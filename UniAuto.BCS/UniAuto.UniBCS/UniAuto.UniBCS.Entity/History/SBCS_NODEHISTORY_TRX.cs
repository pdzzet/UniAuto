using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBCSNODEHISTORYTRX

	/// <summary>
	/// SBCSNODEHISTORYTRX object for NHibernate mapped table 'SBCS_NODEHISTORY_TRX'.
	/// </summary>
	public class EQUIPMENTHISTORY : EntityData
	{
		#region Member Variables
		
		protected long _id;
        protected DateTime _uPDATETIME = DateTime.Now;
		protected string _lINEID;
		protected string _nODEID;
		protected string _nODENO;
        protected string _nODEATTRIBUTE;
		protected string _cIMMODE;
		protected string _uPSTREAMINLINEMODE;
		protected string _dOWNSTREAMINLINEMODE;
		protected string _cURRENTRECIPEID;
		protected string _cURRENTSTATUS;
		protected int _cFJOBCOUNT;
		protected int _tFTJOBCOUNT;
		protected string _aLARMEXIST;
		protected string _eQUIPMENTOPERATORMODE;
		protected string _aUTORECIPECHANGEFLAG;

        protected string _eQUIPMENTRUNMODE;
        protected string _pARTIALFULLMODE;
        protected string _jOBDATACHECKMODE;
        protected string _rECIPEIDCHECKMODE;
        protected string _pRODUCTTYPECHECKMODE;
        protected string _gROUPINDEXCHECKMODE;
        protected string _pRODUCTIDCHECKMODE;
        protected string _jOBDUPLICATECHECKMODE;
        protected string _cOAVERSIONCHECKCHECKMODE;
        protected string _sAMPLINGRULE;
        protected string _sAMPLINGUNIT;
        protected string _sIDEINFORMATION;
        protected string _tRANSACTIONID;


		#endregion

		#region Constructors

		public EQUIPMENTHISTORY() { }

        public EQUIPMENTHISTORY(DateTime uPDATETIME, string lINEID, string nODEID, string nODENO, string nODEATTRIBUTE, string cIMMODE, string uPSTREAMINLINEMODE, 
            string dOWNSTREAMINLINEMODE, string cURRENTRECIPEID, string cURRENTSTATUS, int cFJOBCOUNT, int tFTJOBCOUNT, string aLARMEXIST, string eQUIPMENTOPERATORMODE,
            string aUTORECIPECHANGEFLAG, string tRANSACTIONID)
		{
			this._uPDATETIME = uPDATETIME;
			this._lINEID = lINEID;
			this._nODEID = nODEID;
			this._nODENO = nODENO;
            this._nODEATTRIBUTE = nODEATTRIBUTE;
			this._cIMMODE = cIMMODE;
			this._uPSTREAMINLINEMODE = uPSTREAMINLINEMODE;
			this._dOWNSTREAMINLINEMODE = dOWNSTREAMINLINEMODE;
			this._cURRENTRECIPEID = cURRENTRECIPEID;
			this._cURRENTSTATUS = cURRENTSTATUS;
			this._cFJOBCOUNT = cFJOBCOUNT;
			this._tFTJOBCOUNT = tFTJOBCOUNT;
			this._aLARMEXIST = aLARMEXIST;
			this._eQUIPMENTOPERATORMODE = eQUIPMENTOPERATORMODE;
			this._aUTORECIPECHANGEFLAG = aUTORECIPECHANGEFLAG;
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

		public virtual string LINEID
		{
			get { return _lINEID; }
			set
			{				
				_lINEID = value;
			}
		}

		public virtual string NODEID
		{
			get { return _nODEID; }
			set
			{				
				_nODEID = value;
			}
		}

		public virtual string NODENO
		{
			get { return _nODENO; }
			set { _nODENO = value; }
		}

        public virtual string NODEATTRIBUTE
		{
            get { return _nODEATTRIBUTE; }
			set
			{
                _nODEATTRIBUTE = value;
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

		public virtual string UPSTREAMINLINEMODE
		{
			get { return _uPSTREAMINLINEMODE; }
			set
			{				
				_uPSTREAMINLINEMODE = value;
			}
		}

		public virtual string DOWNSTREAMINLINEMODE
		{
			get { return _dOWNSTREAMINLINEMODE; }
			set
			{				
				_dOWNSTREAMINLINEMODE = value;
			}
		}

		public virtual string CURRENTRECIPEID
		{
			get { return _cURRENTRECIPEID; }
			set
			{				
				_cURRENTRECIPEID = value;
			}
		}

		public virtual string CURRENTSTATUS
		{
			get { return _cURRENTSTATUS; }
			set
			{				
				_cURRENTSTATUS = value;
			}
		}

		public virtual int CFJOBCOUNT
		{
			get { return _cFJOBCOUNT; }
			set { _cFJOBCOUNT = value; }
		}

		public virtual int TFTJOBCOUNT
		{
			get { return _tFTJOBCOUNT; }
			set { _tFTJOBCOUNT = value; }
		}

		public virtual string ALARMEXIST
		{
			get { return _aLARMEXIST; }
			set
			{				
				_aLARMEXIST = value;
			}
		}

		public virtual string EQUIPMENTOPERATORMODE
		{
			get { return _eQUIPMENTOPERATORMODE; }
			set
			{				
				_eQUIPMENTOPERATORMODE = value;
			}
		}

		public virtual string AUTORECIPECHANGEFLAG
		{
			get { return _aUTORECIPECHANGEFLAG; }
			set
			{				
				_aUTORECIPECHANGEFLAG = value;
			}
		}

        public virtual string  EQUIPMENTRUNMODE
        {
            get{return _eQUIPMENTRUNMODE;}
            set{_eQUIPMENTRUNMODE=value;}
        }
        public virtual string PARTIALFULLMODE
        {
            get{return _pARTIALFULLMODE;}
            set{_pARTIALFULLMODE=value;}
        }

        public virtual string JOBDATACHECKMODE
        {
            get{return _jOBDATACHECKMODE;}
            set{_jOBDATACHECKMODE=value;}
        }

        public virtual string RECIPEIDCHECKMODE
        {
            get{return _rECIPEIDCHECKMODE;}
            set{_rECIPEIDCHECKMODE=value;}
        }

        public virtual string PRODUCTTYPECHECKMODE
        {
            get{return _pRODUCTTYPECHECKMODE;}
            set{_pRODUCTTYPECHECKMODE=value;}
        }

        public virtual string GROUPINDEXCHECKMODE
        {
            get{return _gROUPINDEXCHECKMODE;}
            set{_gROUPINDEXCHECKMODE=value;}
        }

        public virtual string PRODUCTIDCHECKMODE
        {
            get{return _pRODUCTIDCHECKMODE;}
            set{_pRODUCTIDCHECKMODE=value;}
        }

        public virtual string JOBDUPLICATECHECKMODE
        {
            get {return _jOBDUPLICATECHECKMODE;}
            set{_jOBDUPLICATECHECKMODE=value;}
        }

        public virtual string COAVERSIONCHECKCHECKMODE
        {
            get { return _cOAVERSIONCHECKCHECKMODE; }
            set { _cOAVERSIONCHECKCHECKMODE = value; }
        }


        public virtual string SAMPLINGRULE 
        {
            get{return _sAMPLINGRULE;}
            set{_sAMPLINGRULE=value;}

        }
        public virtual string SAMPLINGUNIT 
        {
            get{return _sAMPLINGUNIT;}
            set{_sAMPLINGUNIT=value;}
        }
        public virtual string SIDEINFORMATION
        {
            get { return _sIDEINFORMATION; }
            set { _sIDEINFORMATION = value; }
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