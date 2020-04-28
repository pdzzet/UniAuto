using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBCSALARMHISTORYTRX

	/// <summary>
	/// SBCSALARMHISTORYTRX object for NHibernate mapped table 'SBCS_ALARMHISTORY_TRX'.
	/// </summary>
	public class ALARMHISTORY:EntityData
	{
		#region Member Variables
		
		protected long _id;
		protected string _eVENTNAME;
        protected DateTime _uPDATETIME = DateTime.Now;
		protected string _aLARMID;
		protected string _aLARMCODE;
		protected string _aLARMLEVEL;
		protected string _aLARMTEXT;
		protected string _aLARMSTATUS;
		protected string _nODEID;
		protected string _aLARMUNIT;
        protected string _tRANSACTIONID;

		#endregion

		#region Constructors

		public ALARMHISTORY() { }

        public ALARMHISTORY(string eVENTNAME, DateTime uPDATETIME, string aLARMID, string aLARMCODE, string aLARMLEVEL, string aLARMTEXT, string aLARMSTATUS, string nODEID, string aLARMUNIT, string tRANSACTIONID)
		{
			this._eVENTNAME = eVENTNAME;
			this._uPDATETIME = uPDATETIME;
			this._aLARMID = aLARMID;
			this._aLARMCODE = aLARMCODE;
			this._aLARMLEVEL = aLARMLEVEL;
			this._aLARMTEXT = aLARMTEXT;
			this._aLARMSTATUS = aLARMSTATUS;
			this._nODEID = nODEID;
			this._aLARMUNIT = aLARMUNIT;
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

		public virtual string ALARMID
		{
			get { return _aLARMID; }
			set
			{				
				_aLARMID = value;
			}
		}

		public virtual string ALARMCODE
		{
			get { return _aLARMCODE; }
			set
			{				
				_aLARMCODE = value;
			}
		}

		public virtual string ALARMLEVEL
		{
			get { return _aLARMLEVEL; }
			set
			{				
				_aLARMLEVEL = value;
			}
		}

		public virtual string ALARMTEXT
		{
			get { return _aLARMTEXT; }
			set
			{				
				_aLARMTEXT = value;
			}
		}

		public virtual string ALARMSTATUS
		{
			get { return _aLARMSTATUS; }
			set
			{				
				_aLARMSTATUS = value;
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

		public virtual string ALARMUNIT
		{
			get { return _aLARMUNIT; }
			set
			{				
				_aLARMUNIT = value;
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