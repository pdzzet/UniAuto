using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBCSPPKEVENTHISTORYTRX

	/// <summary>
	/// SBCSPPKEVENTHISTORYTRX object for NHibernate mapped table 'SBCS_PPKEVENTHISTORY_TRX'.
	/// </summary>
	public class PPKEVENTHISTORY
	{
		#region Member Variables
		
		protected long _id;
		protected string _eVENTNAME;
		protected DateTime _uPDATETIME;
		protected string _nODENO;
		protected string _cARNO;
		protected string _bOXCOUNT;
		protected string _bOXID01;
        protected string _bOXID02;
        protected string _bOXTYPE;
		protected string _pORTNO;
		protected string _pACKUNPACKMODE;
        protected string _sTAGEORPORTORPALLETORCAR;
		protected string _sTAGENO;
		protected string _pALLETID;
		protected string _pALLETNO;
		protected string _rEMOVEREASONFLAG;
		protected string _sOURCE;
		protected string _wEIGHT;
		protected string _sAMPLINGFLAG;
		protected string _rETURNCODE;
		protected string _pRODUCTTYPE;
		protected string _gRADE01;
		protected string _gRADE02;
		protected string _cASSETTESETTINGCODE01;
		protected string _cASSETTESETTINGCODE02;
		protected string _bOXGLASSCOUNT01;
        protected string _bOXGLASSCOUNT02;
        protected string _nODEID;
        protected string _lOTNAME;
        protected string _rEQUESTTYPE;
        protected string _mAXCOUNT;
        protected string _remark;
        protected string _tRANSACTIONID;

		#endregion

		#region Constructors

		public PPKEVENTHISTORY() { }

        public PPKEVENTHISTORY(string eVENTNAME, DateTime uPDATETIME, string nODENO, string cARNO, string bOXCOUNT, string bOXID01, string bOXID02, string bOXTYPE, string pORTNO, string pACKUNPACKMODE, string sTAGEORPORTORPALLETORCAR, string sTAGENO, string pALLETID, string pALLETNO, string rEMOVEREASONFLAG, string sOURCE, string wEIGHT, string sAMPLINGFLAG, string rETURNCODE, string pRODUCTTYPE, string gRADE01, string gRADE02, string cASSETTESETTINGCODE01, string cASSETTESETTINGCODE02, string bOXGLASSCOUNT01, string bOXGLASSCOUNT02, string lOTNAME, string rEQUESTTYPE, string mAXCOUNT, string tRANSACTIONID)
		{
			this._eVENTNAME = eVENTNAME;
			this._uPDATETIME = uPDATETIME;
			this._nODENO = nODENO;
			this._cARNO = cARNO;
			this._bOXCOUNT = bOXCOUNT;
			this._bOXID01 = bOXID01;
			this._bOXID02 = bOXID02;
            this._bOXTYPE = bOXTYPE;
			this._pORTNO = pORTNO;
			this._pACKUNPACKMODE = pACKUNPACKMODE;
            this._sTAGEORPORTORPALLETORCAR = sTAGEORPORTORPALLETORCAR;
			this._sTAGENO = sTAGENO;
			this._pALLETID = pALLETID;
			this._pALLETNO = pALLETNO;
			this._rEMOVEREASONFLAG = rEMOVEREASONFLAG;
			this._sOURCE = sOURCE;
			this._wEIGHT = wEIGHT;
			this._sAMPLINGFLAG = sAMPLINGFLAG;
			this._rETURNCODE = rETURNCODE;
			this._pRODUCTTYPE = pRODUCTTYPE;
			this._gRADE01 = gRADE01;
			this._gRADE02 = gRADE02;
			this._cASSETTESETTINGCODE01 = cASSETTESETTINGCODE01;
			this._cASSETTESETTINGCODE02 = cASSETTESETTINGCODE02;
			this._bOXGLASSCOUNT01 = bOXGLASSCOUNT01;
			this._bOXGLASSCOUNT02 = bOXGLASSCOUNT02;
            this._lOTNAME = lOTNAME;
            this._rEQUESTTYPE = rEQUESTTYPE;
            this._mAXCOUNT = mAXCOUNT;
            this._tRANSACTIONID = tRANSACTIONID;
		}

		#endregion

		#region Public Properties

		public virtual long Id
		{
			get {return _id;}
			set {_id = value;}
		}

		public virtual string EVENTNAME
		{
			get { return _eVENTNAME; }
			set
			{				
				_eVENTNAME = value;
			}
		}

		public virtual DateTime UPDATETIME
		{
			get { return _uPDATETIME; }
			set { _uPDATETIME = value; }
		}

		public virtual string NODENO
		{
			get { return _nODENO; }
			set
			{				
				_nODENO = value;
			}
		}

		public virtual string CARNO
		{
			get { return _cARNO; }
			set
			{				
				_cARNO = value;
			}
		}

		public virtual string BOXCOUNT
		{
			get { return _bOXCOUNT; }
			set
			{				
				_bOXCOUNT = value;
			}
		}

		public virtual string BOXID01
		{
			get { return _bOXID01; }
			set
			{				
				_bOXID01 = value;
			}
		}

		public virtual string BOXID02
		{
			get { return _bOXID02; }
			set
			{				
				_bOXID02 = value;
			}
		}

        public virtual string BOXTYPE
        {
            get { return _bOXTYPE; }
            set
            {
                _bOXTYPE = value;
            }
        }
		public virtual string PORTNO
		{
			get { return _pORTNO; }
			set
			{				
				_pORTNO = value;
			}
		}

		public virtual string PACKUNPACKMODE
		{
			get { return _pACKUNPACKMODE; }
			set
			{				
				_pACKUNPACKMODE = value;
			}
		}

        public virtual string STAGEORPORTORPALLETORCAR
		{
            get { return _sTAGEORPORTORPALLETORCAR; }
			set
			{
                _sTAGEORPORTORPALLETORCAR = value;
			}
		}

		public virtual string STAGENO
		{
			get { return _sTAGENO; }
			set
			{				
				_sTAGENO = value;
			}
		}

		public virtual string PALLETID
		{
			get { return _pALLETID; }
			set
			{				
				_pALLETID = value;
			}
		}

		public virtual string PALLETNO
		{
			get { return _pALLETNO; }
			set
			{				
				_pALLETNO = value;
			}
		}

		public virtual string REMOVEREASONFLAG
		{
			get { return _rEMOVEREASONFLAG; }
			set
			{				
				_rEMOVEREASONFLAG = value;
			}
		}

		public virtual string SOURCE
		{
			get { return _sOURCE; }
			set
			{				
				_sOURCE = value;
			}
		}

		public virtual string WEIGHT
		{
			get { return _wEIGHT; }
			set
			{				
				_wEIGHT = value;
			}
		}

		public virtual string SAMPLINGFLAG
		{
			get { return _sAMPLINGFLAG; }
			set
			{				
				_sAMPLINGFLAG = value;
			}
		}

		public virtual string RETURNCODE
		{
			get { return _rETURNCODE; }
			set
			{				
				_rETURNCODE = value;
			}
		}

		public virtual string PRODUCTTYPE
		{
			get { return _pRODUCTTYPE; }
			set
			{				
				_pRODUCTTYPE = value;
			}
		}

		public virtual string GRADE01
		{
			get { return _gRADE01; }
			set
			{				
				_gRADE01 = value;
			}
		}

		public virtual string GRADE02
		{
			get { return _gRADE02; }
			set
			{				
				_gRADE02 = value;
			}
		}

		public virtual string CASSETTESETTINGCODE01
		{
			get { return _cASSETTESETTINGCODE01; }
			set
			{				
				_cASSETTESETTINGCODE01 = value;
			}
		}

		public virtual string CASSETTESETTINGCODE02
		{
			get { return _cASSETTESETTINGCODE02; }
			set
			{				
				_cASSETTESETTINGCODE02 = value;
			}
		}

		public virtual string BOXGLASSCOUNT01
		{
			get { return _bOXGLASSCOUNT01; }
			set
			{				
				_bOXGLASSCOUNT01 = value;
			}
		}

		public virtual string BOXGLASSCOUNT02
		{
			get { return _bOXGLASSCOUNT02; }
			set
			{				
				_bOXGLASSCOUNT02 = value;
			}
		}

        public virtual string NODEID {
            get { return _nODEID; }
            set { _nODEID = value; }
        }

        public virtual string LOTNAME
        {
            get { return _lOTNAME; }
            set
            {
                _lOTNAME = value;
            }
        }

        public virtual string REQUESTTYPE
        {
            get { return _rEQUESTTYPE; }
            set
            {
                _rEQUESTTYPE = value;
            }
        }

        public virtual string MAXCOUNT
        {
            get { return _mAXCOUNT; }
            set
            {
                _mAXCOUNT = value;
            }
        }

        public virtual string REMARK {
            get { return _remark; }
            set { _remark = value; }
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