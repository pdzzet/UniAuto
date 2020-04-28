using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBCSPORTHISTORYTRX

	/// <summary>
	/// SBCSPORTHISTORYTRX object for NHibernate mapped table 'SBCS_PORTHISTORY_TRX'.
	/// </summary>
	public class PORTHISTORY : EntityData
	{
		#region Member Variables
		
		protected long _id;
        protected DateTime _uPDATETIME = DateTime.Now;
		protected string _lINEID;
		protected string _nODEID;
		protected string _pORTID;
		protected int _pORTNO;
		protected string _pORTTYPE;
		protected string _pORTMODE;
		protected string _pORTENABLEMODE;
		protected string _pORTTRANSFERMODE;
		protected string _pORTSTATUS;
		protected int _cASSETTESEQNO;
		protected string _cASSETTESTATUS;
		protected string _qTIMEFLAG;
        protected string _tRANSACTIONID;
        protected string _gRADE;
        protected string _pRODUCTTYPE;
        protected string _cASSETTESETCODE;

		#endregion

		#region Constructors

		public PORTHISTORY() { }

        public PORTHISTORY(DateTime uPDATETIME, string lINEID, string nODEID, string pORTID, int pORTNO, string pORTTYPE, string pORTMODE, string pORTENABLEMODE,
                            string pORTTRANSFERMODE, string pORTSTATUS, int cASSETTESEQNO, string cASSETTESTATUS, string qTIMEFLAG, string tRANSACTIONID, string gRADE, string pRODUCTTYPE, string cASSETTESETCODE)
		{
			this._uPDATETIME = uPDATETIME;
			this._lINEID = lINEID;
			this._nODEID = nODEID;
			this._pORTID = pORTID;
			this._pORTNO = pORTNO;
			this._pORTTYPE = pORTTYPE;
			this._pORTMODE = pORTMODE;
			this._pORTENABLEMODE = pORTENABLEMODE;
			this._pORTTRANSFERMODE = pORTTRANSFERMODE;
			this._pORTSTATUS = pORTSTATUS;
			this._cASSETTESEQNO = cASSETTESEQNO;
			this._cASSETTESTATUS = cASSETTESTATUS;
			this._qTIMEFLAG = qTIMEFLAG;
            this._tRANSACTIONID = tRANSACTIONID;
            this._gRADE = gRADE;
            this._pRODUCTTYPE = pRODUCTTYPE;
            this._cASSETTESETCODE = cASSETTESETCODE;
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

		public virtual string PORTID
		{
			get { return _pORTID; }
			set
			{				
				_pORTID = value;
			}
		}

		public virtual int PORTNO
		{
			get { return _pORTNO; }
			set { _pORTNO = value; }
		}

		public virtual string PORTTYPE
		{
			get { return _pORTTYPE; }
			set
			{				
				_pORTTYPE = value;
			}
		}

		public virtual string PORTMODE
		{
			get { return _pORTMODE; }
			set
			{				
				_pORTMODE = value;
			}
		}

		public virtual string PORTENABLEMODE
		{
			get { return _pORTENABLEMODE; }
			set
			{				
				_pORTENABLEMODE = value;
			}
		}

		public virtual string PORTTRANSFERMODE
		{
			get { return _pORTTRANSFERMODE; }
			set
			{				
				_pORTTRANSFERMODE = value;
			}
		}

		public virtual string PORTSTATUS
		{
			get { return _pORTSTATUS; }
			set
			{				
				_pORTSTATUS = value;
			}
		}

		public virtual int CASSETTESEQNO
		{
			get { return _cASSETTESEQNO; }
			set { _cASSETTESEQNO = value; }
		}

		public virtual string CASSETTESTATUS
		{
			get { return _cASSETTESTATUS; }
			set
			{				
				_cASSETTESTATUS = value;
			}
		}

		public virtual string QTIMEFLAG
		{
			get { return _qTIMEFLAG; }
			set
			{				
				_qTIMEFLAG = value;
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

        public virtual string GRADE
        {
            get { return _gRADE; }
            set
            {
                _gRADE = value;
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

        public virtual string CASSETTESETCODE
        {
            get { return _cASSETTESETCODE; }
            set
            {
                _cASSETTESETCODE = value;
            }
        }
		#endregion
	}
	#endregion
}