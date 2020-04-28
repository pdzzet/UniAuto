using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBCSCIMMESSAGETRX

	/// <summary>
	/// SBCSCIMMESSAGETRX object for NHibernate mapped table 'SBCS_CIMMESSAGE_TRX'.
	/// </summary>
    public class CIMMESSAGEHISTORY : EntityData
	{
		#region Member Variables
		
		protected long _id;
        protected DateTime _uPDATETIME = DateTime.Now;
		protected string _nODEID;
		protected string _nODENO;
		protected string _mESSAGEID;
		protected string _mESSAGETEXT;
		protected string _mESSAGESTATUS;
		protected string _oPERATORID;
		protected string _rEMARK;

		#endregion

		#region Constructors

		public CIMMESSAGEHISTORY() { }

        public CIMMESSAGEHISTORY(DateTime uPDATETIME, string nODEID, string nODENO, string mESSAGEID, string mESSAGETEXT, string mESSAGESTATUS, string oPERATORID, string rEMARK)
		{
			this._uPDATETIME = uPDATETIME;
			this._nODEID = nODEID;
			this._nODENO = nODENO;
			this._mESSAGEID = mESSAGEID;
			this._mESSAGETEXT = mESSAGETEXT;
			this._mESSAGESTATUS = mESSAGESTATUS;
			this._oPERATORID = oPERATORID;
			this._rEMARK = rEMARK;
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
			set
			{				
				_nODENO = value;
			}
		}

		public virtual string MESSAGEID
		{
			get { return _mESSAGEID; }
			set
			{				
				_mESSAGEID = value;
			}
		}

		public virtual string MESSAGETEXT
		{
			get { return _mESSAGETEXT; }
			set
			{				
				_mESSAGETEXT = value;
			}
		}

		public virtual string MESSAGESTATUS
		{
			get { return _mESSAGESTATUS; }
			set
			{				
				_mESSAGESTATUS = value;
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

		public virtual string REMARK
		{
			get { return _rEMARK; }
			set
			{				
				_rEMARK = value;
			}
		}

		

		#endregion
	}
	#endregion
}