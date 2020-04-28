using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBCSUNITHISTORYTRX

	/// <summary>
	/// SBCSUNITHISTORYTRX object for NHibernate mapped table 'SBCS_UNITHISTORY_TRX'.
	/// </summary>
	public class UNITHISTORY : EntityData
	{
		#region Member Variables
		
		protected long _id;
        protected DateTime _uPDATETIME = DateTime.Now;
		protected string _nODEID;
		protected string _nODENO;
        protected string _uNITNO;
		protected string _uNITID;
		protected string _uNITSTATUS;
		protected string _uNITTYPE;
        protected string _tRANSACTIONID;

		#endregion

		#region Constructors

		public UNITHISTORY() { }

        public UNITHISTORY(DateTime uPDATETIME, string nODEID, string nODENO, string uNITNO, string uNITID, string uNITSTATUS, string uNITTYPE, string tRANSACTIONID)
		{
			this._uPDATETIME = uPDATETIME;
			this._nODEID = nODEID;
			this._nODENO = nODENO;
			this._uNITNO = uNITNO;
			this._uNITID = uNITID;
			this._uNITSTATUS = uNITSTATUS;
			this._uNITTYPE = uNITTYPE;
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

		public virtual string UNITNO
		{
			get { return _uNITNO; }
			set { _uNITNO = value; }
		}

		public virtual string UNITID
		{
			get { return _uNITID; }
			set
			{				
				_uNITID = value;
			}
		}

		public virtual string UNITSTATUS
		{
			get { return _uNITSTATUS; }
			set
			{				
				_uNITSTATUS = value;
			}
		}

		public virtual string UNITTYPE
		{
			get { return _uNITTYPE; }
			set
			{				
				_uNITTYPE = value;
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