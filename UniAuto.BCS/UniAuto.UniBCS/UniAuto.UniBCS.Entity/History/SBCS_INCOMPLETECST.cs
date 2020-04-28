using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBCSINCOMPLETECST

	/// <summary>
	/// SBCSINCOMPLETECST object for NHibernate mapped table 'SBCS_INCOMPLETECST'.
	/// </summary>
	public class INCOMPLETECST : EntityData
	{
		#region Member Variables
		
		protected long _id;
        protected DateTime _uPDATETIME = DateTime.Now;
		protected string _cASSETTEID;
		protected int _cASSETTESEQNO;
		protected string _pORTID;
		protected string _mESTRXID;
		protected string _fILENAME;
		protected string _sTATE;
		protected string _nGREASON;

		#endregion

		#region Constructors

		public INCOMPLETECST() { }

        public INCOMPLETECST(DateTime uPDATETIME, string cASSETTEID, int cASSETTESEQNO, string pORTID, string mESTRXID, string fILENAME, string sTATE, string nGREASON)
		{
			this._uPDATETIME = uPDATETIME;
			this._cASSETTEID = cASSETTEID;
			this._cASSETTESEQNO = cASSETTESEQNO;
			this._pORTID = pORTID;
			this._mESTRXID = mESTRXID;
			this._fILENAME = fILENAME;
			this._sTATE = sTATE;
			this._nGREASON = nGREASON;
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

		public virtual string CASSETTEID
		{
			get { return _cASSETTEID; }
			set
			{				
				_cASSETTEID = value;
			}
		}

		public virtual int CASSETTESEQNO
		{
			get { return _cASSETTESEQNO; }
			set { _cASSETTESEQNO = value; }
		}

		public virtual string PORTID
		{
			get { return _pORTID; }
			set
			{				
				_pORTID = value;
			}
		}

		public virtual string MESTRXID
		{
			get { return _mESTRXID; }
			set
			{				
				_mESTRXID = value;
			}
		}

		public virtual string FILENAME
		{
			get { return _fILENAME; }
			set { _fILENAME = value; }
		}

		public virtual string STATE
		{
			get { return _sTATE; }
			set
			{				
				_sTATE = value;
			}
		}

		public virtual string NGREASON
		{
			get { return _nGREASON; }
			set
			{				
				_nGREASON = value;
			}
		}

		

		#endregion
	}
	#endregion
}