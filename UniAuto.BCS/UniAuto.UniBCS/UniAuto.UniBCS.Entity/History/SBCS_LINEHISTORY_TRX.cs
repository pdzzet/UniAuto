using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBCSLINEHISTORYTRX

	/// <summary>
	/// SBCSLINEHISTORYTRX object for NHibernate mapped table 'SBCS_LINEHISTORY_TRX'.
	/// </summary>
	public class LINEHISTORY : EntityData
	{
		#region Member Variables
		
		protected long _id;
        protected DateTime _uPDATETIME = DateTime.Now;
		protected string _lINEID;
		protected string _lINETYPE;
		protected string _fABTYPE;
		protected string _lINESTATUS;
		protected string _cSTOPERATIONMODE;
		protected int _eQPCOUNT;
		protected string _lINEOPERATIONMODE;
		protected string _mESCONNECTIONSTATE;
		protected string _oNLINECONTROLSTATE;
        protected string _tRANSACTIONID;
        protected string _iNDEXEROPERATIONMODE;
        protected string _sHORTCUTMODE;

		#endregion

		#region Constructors

		public LINEHISTORY() { }

        public LINEHISTORY(DateTime uPDATETIME, string lINEID, string lINETYPE, string fABTYPE, string lINESTATUS, string cSTOPERATIONMODE, int eQPCOUNT,
            string lINEOPERATIONMODE, string mESCONNECTIONSTATE, string oNLINECONTROLSTATE, string tRANSACTIONID, string iNDEXEROPERATIONMODE, string sHORTCUTMODE)
        {
            this._uPDATETIME = uPDATETIME;
            this._lINEID = lINEID;
            this._lINETYPE = lINETYPE;
            this._fABTYPE = fABTYPE;
            this._lINESTATUS = lINESTATUS;
            this._cSTOPERATIONMODE = cSTOPERATIONMODE;
            this._eQPCOUNT = eQPCOUNT;
            this._lINEOPERATIONMODE = lINEOPERATIONMODE;
            this._mESCONNECTIONSTATE = mESCONNECTIONSTATE;
            this._oNLINECONTROLSTATE = oNLINECONTROLSTATE;
            this._tRANSACTIONID = tRANSACTIONID;
            this._iNDEXEROPERATIONMODE = iNDEXEROPERATIONMODE;
            this._sHORTCUTMODE = sHORTCUTMODE;
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

		public virtual string LINETYPE
		{
			get { return _lINETYPE; }
			set
			{				
				_lINETYPE = value;
			}
		}

		public virtual string FABTYPE
		{
			get { return _fABTYPE; }
			set
			{				
				_fABTYPE = value;
			}
		}

		public virtual string LINESTATUS
		{
			get { return _lINESTATUS; }
			set
			{				
				_lINESTATUS = value;
			}
		}

		public virtual string CSTOPERATIONMODE
		{
			get { return _cSTOPERATIONMODE; }
			set
			{				
				_cSTOPERATIONMODE = value;
			}
		}

		public virtual int EQPCOUNT
		{
			get { return _eQPCOUNT; }
			set { _eQPCOUNT = value; }
		}

		public virtual string LINEOPERATIONMODE
		{
			get { return _lINEOPERATIONMODE; }
			set
			{				
				_lINEOPERATIONMODE = value;
			}
		}

		public virtual string MESCONNECTIONSTATE
		{
			get { return _mESCONNECTIONSTATE; }
			set
			{				
				_mESCONNECTIONSTATE = value;
			}
		}

		public virtual string ONLINECONTROLSTATE
		{
			get { return _oNLINECONTROLSTATE; }
			set
			{				
				_oNLINECONTROLSTATE = value;
			}
		}

        public virtual string TRANSACTIONID
        {
            get { return _tRANSACTIONID; }
            set
            {
                _tRANSACTIONID = value;
            }
        }

        public virtual string INDEXEROPERATIONMODE
        {
            get { return _iNDEXEROPERATIONMODE; }
            set
            {
                _iNDEXEROPERATIONMODE = value;
            }
        }

        public virtual string SHORTCUTMODE
        {
            get { return _sHORTCUTMODE; }
            set
            {
                _sHORTCUTMODE = value;
            }
        }

		#endregion
	}
	#endregion
}