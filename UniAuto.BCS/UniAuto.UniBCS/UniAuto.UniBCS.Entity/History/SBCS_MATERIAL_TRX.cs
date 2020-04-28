using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBCSMATERIALTRX

	/// <summary>
	/// SBCSMATERIALTRX object for NHibernate mapped table 'SBCS_MATERIAL_TRX'.
	/// </summary>
	public class MATERIALHISTORY
	{
		#region Member Variables
		
		protected long _id;
		protected string _nODEID;
		protected string _uNITNO;
		protected DateTime _uPDATETIME;
		protected string _mATERIALID;
		protected string _mATERIALCOUNT;
		protected string _mATERIALSTATUS;
		protected string _mATERIALTYPE;
		protected string _oPERATORID;
		protected string _sLOTNO;
		protected string _oLDMATERIALID;
		protected string _pERMITCODE;
        protected string _tRANSACTIONID;

		#endregion

		#region Constructors

		public MATERIALHISTORY() { }

        public MATERIALHISTORY(string nODEID, string uNITNO, DateTime uPDATETIME, string mATERIALID, string mATERIALCOUNT, string mATERIALSTATUS, string mATERIALTYPE, string oPERATORID, string sLOTNO, string oLDMATERIALID, string pERMITCODE, string tRANSACTIONID)
		{
			this._nODEID = nODEID;
			this._uNITNO = uNITNO;
			this._uPDATETIME = uPDATETIME;
			this._mATERIALID = mATERIALID;
			this._mATERIALCOUNT = mATERIALCOUNT;
			this._mATERIALSTATUS = mATERIALSTATUS;
			this._mATERIALTYPE = mATERIALTYPE;
			this._oPERATORID = oPERATORID;
			this._sLOTNO = sLOTNO;
			this._oLDMATERIALID = oLDMATERIALID;
			this._pERMITCODE = pERMITCODE;
            this._tRANSACTIONID = tRANSACTIONID;
		}

		#endregion

		#region Public Properties

		public virtual long Id
		{
			get {return _id;}
			set {_id = value;}
		}

		public virtual string NODEID
		{
			get { return _nODEID; }
			set
			{				
				_nODEID = value;
			}
		}

		public virtual string UNITNO
		{
			get { return _uNITNO; }
			set
			{				
				_uNITNO = value;
			}
		}

		public virtual DateTime UPDATETIME
		{
			get { return _uPDATETIME; }
			set { _uPDATETIME = value; }
		}

		public virtual string MATERIALID
		{
			get { return _mATERIALID; }
			set
			{				
				_mATERIALID = value;
			}
		}

		public virtual string MATERIALCOUNT
		{
			get { return _mATERIALCOUNT; }
			set { _mATERIALCOUNT = value; }
		}

		public virtual string MATERIALSTATUS
		{
			get { return _mATERIALSTATUS; }
			set
			{				
				_mATERIALSTATUS = value;
			}
		}

		public virtual string MATERIALTYPE
		{
			get { return _mATERIALTYPE; }
			set
			{				
				_mATERIALTYPE = value;
			}
		}

		public virtual string OPERATORID
		{
			get { return _oPERATORID; }
			set
			{				
				_oPERATORID = value;
			}
		}

		public virtual string SLOTNO
		{
			get { return _sLOTNO; }
			set
			{				
				_sLOTNO = value;
			}
		}

		public virtual string OLDMATERIALID
		{
			get { return _oLDMATERIALID; }
			set
			{				
				_oLDMATERIALID = value;
			}
		}

		public virtual string PERMITCODE
		{
			get { return _pERMITCODE; }
			set
			{				
				_pERMITCODE = value;
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


		#endregion
	}
	#endregion
}