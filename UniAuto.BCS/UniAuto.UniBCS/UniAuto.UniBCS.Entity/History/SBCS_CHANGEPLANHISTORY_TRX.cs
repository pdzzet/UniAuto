using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBCSCHANGEPLANHISTORY

	/// <summary>
	/// SBCSCHANGEPLANHISTORY object for NHibernate mapped table 'SBCS_CHANGEPLANHISTORY'.
	/// </summary>
	public class CHANGEPLANHISTORY : EntityData
	{
		#region Member Variables
		
		protected long _id;
		protected string _pLANID;
		protected string _sOURCECASSETTEID;
		protected string _tARGETASSETTEID;
		protected string _pLANSTATUS;
        protected DateTime _uPDATETIME = DateTime.Now;

		#endregion

		#region Constructors

		public CHANGEPLANHISTORY() { }

		public CHANGEPLANHISTORY( string pLANID, string sOURCECASSETTEID, string tARGETASSETTEID, string pLANSTATUS, DateTime uPDATETIME )
		{
			this._pLANID = pLANID;
			this._sOURCECASSETTEID = sOURCECASSETTEID;
			this._tARGETASSETTEID = tARGETASSETTEID;
			this._pLANSTATUS = pLANSTATUS;
			this._uPDATETIME = uPDATETIME;
		}

		#endregion

		#region Public Properties

		public virtual long Id
		{
			get {return _id;}
			set {_id = value;}
		}

		public virtual string PLANID
		{
			get { return _pLANID; }
			set
			{				
				_pLANID = value;
			}
		}

		public virtual string SOURCECASSETTEID
		{
			get { return _sOURCECASSETTEID; }
			set
			{				
				_sOURCECASSETTEID = value;
			}
		}

		public virtual string TARGETASSETTEID
		{
			get { return _tARGETASSETTEID; }
			set
			{				
				_tARGETASSETTEID = value;
			}
		}

		public virtual string PLANSTATUS
		{
			get { return _pLANSTATUS; }
			set
			{				
				_pLANSTATUS = value;
			}
		}

		public virtual DateTime UPDATETIME
		{
			get { return _uPDATETIME; }
			set { _uPDATETIME = value; }
		}

		

		#endregion
	}
	#endregion
}