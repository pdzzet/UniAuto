using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBCSDEFECTCODEHISTORYTRX

	/// <summary>
	/// SBCSDEFECTCODEHISTORYTRX object for NHibernate mapped table 'SBCS_DEFECTCODEHISTORY_TRX'.
	/// </summary>
	public class DEFECTCODEHISTORY
	{
		#region Member Variables
		
		protected long _id;
		protected DateTime _uPDATETIME;
		protected string _nODEID;
		protected string _nODENO;
		protected int _cASSETTESEQNO;
		protected int _jOBSEQNO;
		protected string _jOBID;
		protected string _pLANID;
		protected string _dEFECTCODES;
		protected string _rEMARK;
        protected string _tRANSACTIONID;

		#endregion

		#region Constructors

        public DEFECTCODEHISTORY() { }

        public DEFECTCODEHISTORY(DateTime uPDATETIME, string nODEID, string nODENO, int cASSETTESEQNO, int jOBSEQNO, string jOBID, string pLANID, string dEFECTCODE1, string rEMARK, string tRANSACTIONID)
        {
            this._uPDATETIME = uPDATETIME;
            this._nODEID = nODEID;
            this._nODENO = nODENO;
            this._cASSETTESEQNO = cASSETTESEQNO;
            this._jOBSEQNO = jOBSEQNO;
            this._jOBID = jOBID;
            this._pLANID = pLANID;
            this._dEFECTCODES = dEFECTCODE1;
            this._rEMARK = rEMARK;
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
			set
			{				
				_nODENO = value;
			}
		}

		public virtual int CASSETTESEQNO
		{
			get { return _cASSETTESEQNO; }
			set { _cASSETTESEQNO = value; }
		}

		public virtual int JOBSEQNO
		{
			get { return _jOBSEQNO; }
			set { _jOBSEQNO = value; }
		}

		public virtual string JOBID
		{
			get { return _jOBID; }
			set
			{				
				_jOBID = value;
			}
		}

		public virtual string PLANID
		{
			get { return _pLANID; }
			set
			{				
				_pLANID = value;
			}
		}

		public virtual string DEFECTCODES
		{
			get { return _dEFECTCODES; }
			set
			{				
				_dEFECTCODES = value;
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

        public virtual string TRANSACTIONID
        {
            get { return _tRANSACTIONID; }
            set { _tRANSACTIONID = value; }
        }

		#endregion
	}
	#endregion
}