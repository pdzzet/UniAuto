using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBCSTERMINALMESSAGETRX

	/// <summary>
	/// SBCSTERMINALMESSAGETRX object for NHibernate mapped table 'SBCS_TERMINALMESSAGE_TRX'.
	/// </summary>
	public class TERMINALMESSAGEHISTORY
	{
		#region Member Variables
		
		protected long _id;
		protected DateTime _uPDATETIME;
		protected string _tRANSACTIONID;
		protected string _lINEID;
		protected string _cAPTION;
		protected string _tERMINALTEXT;

		#endregion

		#region Constructors

		public TERMINALMESSAGEHISTORY() { }

        public TERMINALMESSAGEHISTORY(DateTime uPDATETIME, string tRANSACTIONID, string lINEID, string cAPTION, string tERMINALTEXT)
		{
			this._uPDATETIME = uPDATETIME;
			this._tRANSACTIONID = tRANSACTIONID;
			this._lINEID = lINEID;
			this._cAPTION = cAPTION;
			this._tERMINALTEXT = tERMINALTEXT;
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

		public virtual string TRANSACTIONID
		{
			get { return _tRANSACTIONID; }
			set
			{				
				_tRANSACTIONID = value;
			}
		}

		public virtual string LINEID
		{
			get { return _lINEID; }
			set
			{				
				_lINEID = value;
			}
		}

		public virtual string CAPTION
		{
			get { return _cAPTION; }
			set
			{				
				_cAPTION = value;
			}
		}

		public virtual string TERMINALTEXT
		{
			get { return _tERMINALTEXT; }
			set
			{				
				_tERMINALTEXT = value;
			}
		}

		

		#endregion
	}
	#endregion
}