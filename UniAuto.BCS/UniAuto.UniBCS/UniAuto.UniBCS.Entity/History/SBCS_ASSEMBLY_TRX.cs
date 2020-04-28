using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBCSASSEMBLYTRX

	/// <summary>
	/// SBCSASSEMBLYTRX object for NHibernate mapped table 'SBCS_ASSEMBLY_TRX'.
	/// </summary>
	public class ASSEMBLYHISTORY
	{
		#region Member Variables
		
		protected long _id;
		protected string _nODEID;
		protected DateTime _uPDATETIME;
		protected string _tFTCASSETTESEQNO;
        protected string _tFTJOBSEQNO;
		protected string _tFTJOBID;
        protected string _cFCASSETTESEQNO;
        protected string _cFJOBSEQNO;
		protected string _cFJOBID;

		#endregion

		#region Constructors

		public ASSEMBLYHISTORY() { }

        public ASSEMBLYHISTORY(string nODEID, DateTime uPDATETIME, string tFTCASSETTESEQNO, string tFTJOBSEQNO, string tFTJOBID, string cFCASSETTESEQNO, string cFJOBSEQNO, string cFJOBID)
		{
			this._nODEID = nODEID;
			this._uPDATETIME = uPDATETIME;
			this._tFTCASSETTESEQNO = tFTCASSETTESEQNO;
			this._tFTJOBSEQNO = tFTJOBSEQNO;
			this._tFTJOBID = tFTJOBID;
			this._cFCASSETTESEQNO = cFCASSETTESEQNO;
			this._cFJOBSEQNO = cFJOBSEQNO;
			this._cFJOBID = cFJOBID;
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

		public virtual DateTime UPDATETIME
		{
			get { return _uPDATETIME; }
			set { _uPDATETIME = value; }
		}

		public virtual string TFTCASSETTESEQNO
		{
			get { return _tFTCASSETTESEQNO; }
			set { _tFTCASSETTESEQNO = value; }
		}

		public virtual string TFTJOBSEQNO
		{
			get { return _tFTJOBSEQNO; }
			set { _tFTJOBSEQNO = value; }
		}

		public virtual string TFTJOBID
		{
			get { return _tFTJOBID; }
			set
			{				
				_tFTJOBID = value;
			}
		}

		public virtual string CFCASSETTESEQNO
		{
			get { return _cFCASSETTESEQNO; }
			set { _cFCASSETTESEQNO = value; }
		}

		public virtual string CFJOBSEQNO
		{
			get { return _cFJOBSEQNO; }
			set { _cFJOBSEQNO = value; }
		}

		public virtual string CFJOBID
		{
			get { return _cFJOBID; }
			set
			{				
				_cFJOBID = value;
			}
		}

		

		#endregion
	}
	#endregion
}