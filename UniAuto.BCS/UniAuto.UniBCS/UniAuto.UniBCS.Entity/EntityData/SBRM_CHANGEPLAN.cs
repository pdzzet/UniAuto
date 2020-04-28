using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBRMCHANGEPLAN

	/// <summary>
	/// SBRMCHANGEPLAN object for NHibernate mapped table 'SBRM_CHANGEPLAN'.
	/// </summary>
	public class CHANGEPLAN 
	{
		#region Member Variables
		
		protected long _id;
        protected string _sERVERNAME;
		protected string _lINEID;
		protected string _pLANID;
		protected string _sOURCECASSETTEID;
		protected string _sLOTNO;
		protected string _jOBID;
		protected string _tARGETASSETTEID;
        protected string _tARGETSLOTNO;
        protected DateTime _uPDATETIME = DateTime.Now;
		protected string _oPERATORID;

		#endregion

		#region Constructors

		public CHANGEPLAN() { }

        public CHANGEPLAN(string sERVERNAME, string lINEID, string pLANID, string sOURCECASSETTEID, string sLOTNO, string jOBID, string tARGETASSETTEID, string tARGETSLOTNO, DateTime uPDATETIME, string oPERATORID)
		{
            this._sERVERNAME = sERVERNAME;
			this._lINEID = lINEID;
			this._pLANID = pLANID;
			this._sOURCECASSETTEID = sOURCECASSETTEID;
			this._sLOTNO = sLOTNO;
			this._jOBID = jOBID;
			this._tARGETASSETTEID = tARGETASSETTEID;
            this._tARGETSLOTNO = tARGETSLOTNO;
			this._uPDATETIME = uPDATETIME;
			this._oPERATORID = oPERATORID;
		}

		#endregion

		#region Public Properties

		public virtual long Id
		{
			get {return _id;}
			set {_id = value;}
		}

        public virtual string SERVERNAME
        {
            get { return _sERVERNAME; }
            set
            {
                _sERVERNAME = value;
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

		public virtual string SLOTNO
		{
			get { return _sLOTNO; }
			set
			{				
				_sLOTNO = value;
			}
		}

		public virtual string JOBID
		{
			get { return _jOBID; }
			set
			{				
				_jOBID = value;
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

        public virtual string TARGETSLOTNO
        {
            get { return _tARGETSLOTNO; }
            set
            {
                _tARGETSLOTNO = value;
            }
        }

		public virtual DateTime UPDATETIME
		{
			get { return _uPDATETIME; }
			set { _uPDATETIME = value; }
		}

		public virtual string OPERATORID
		{
			get { return _oPERATORID; }
			set
			{				
				_oPERATORID = value;
			}
		}

		

		#endregion
	}
	#endregion
}