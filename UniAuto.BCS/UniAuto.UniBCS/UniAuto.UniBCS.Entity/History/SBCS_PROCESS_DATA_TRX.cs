using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBCSPROCESSDATATRX

	/// <summary>
	/// SBCSPROCESSDATATRX object for NHibernate mapped table 'SBCS_PROCESS_DATA_TRX'.
	/// </summary>
	public class PROCESSDATAHISTORY
	{
		#region Member Variables
		
		protected long _id;
		protected int _cASSETTESEQNO;
		protected int _jOBSEQNO;
		protected string _jOBID;
		protected string _tRXID;
		protected string _mESCONTROLSTATE;
		protected string _nODEID;
		protected DateTime _uPDATETIME;
		protected string _fILENAMA;
		protected int _pROCESSTIME;
		protected string _lOCALPROCESSSTARTTIME;
		protected string _lOCALPROCSSSENDTIME;
		protected int _uNIT1PROCESSTIME;
		protected string _uNIT1PROCESSSTARTTIME;
		protected string _uNIT1PROCESSENDTIME;
		protected int _uNIT2PROCESSTIME;
		protected string _uNIT2PROCESSSTARTTIME;
		protected string _uNIT2PROCESSENDTIME;
		protected int _uNIT3PROCESSTIME;
		protected string _uNIT3PROCESSSTARTTIME;
		protected string _uNIT3PROCESSENDTIME;
		protected int _uNIT4PROCESSTIME;
		protected string _uNIT4PROCESSSTARTTIME;
		protected string _uNIT4PROCESSENDTIME;
		protected int _uNIT5PROCESSTIME;
		protected string _uNIT5PROCESSSTARTTIME;
		protected string _uNIT5PROCESSENDTIME;
		protected int _uNIT6PROCESSTIME;
		protected string _uNIT6PROCESSSTARTTIME;
		protected string _uNIT6PROCESSENDTIME;
		protected int _uNIT7PROCESSTIME;
		protected string _uNIT7PROCESSSTARTTIME;
		protected string _uNIT7PROCESSENDTIME;
		protected int _uNIT8PROCESSTIME;
		protected string _uNIT8PROCESSSTARTTIME;
		protected string _uNIT8PROCESSENDTIME;
		protected int _uNIT9PROCESSTIME;
		protected string _uNIT9PROCESSSTARTTIME;
		protected string _uNIT9PROCESSENDTIME;
		protected int _uNIT10PROCESSTIME;
		protected string _uNIT10PROCESSSTARTTIME;
		protected string _uNIT10PROCESSENDTIME;
		protected int _uNIT11PROCESSTIME;
		protected string _uNIT11PROCESSSTARTTIME;
		protected string _uNIT11PROCESSENDTIME;
		protected int _uNIT12PROCESSTIME;
		protected string _uNIT12PROCESSSTARTTIME;
		protected string _uNIT12PROCESSENDTIME;
		protected int _uNIT13PROCESSTIME;
		protected string _uNIT13PROCESSSTARTTIME;
		protected string _uNIT13PROCESSENDTIME;
		protected int _uNIT14PROCESSTIME;
		protected string _uNIT14PROCESSSTARTTIME;
		protected string _uNIT14PROCESSENDTIME;
		protected int _uNIT15PROCESSTIME;
		protected string _uNIT15PROCESSSTARTTIME;
		protected string _uNIT15PROCESSENDTIME;

		#endregion

		#region Constructors

		public PROCESSDATAHISTORY() { }

        public PROCESSDATAHISTORY(int cASSETTESEQNO, int jOBSEQNO, string jOBID, string tRXID, string mESCONTROLSTATE, string nODEID, DateTime uPDATETIME, string fILENAMA, int pROCESSTIME, string lOCALPROCESSSTARTTIME, string lOCALPROCSSSENDTIME, int uNIT1PROCESSTIME, string uNIT1PROCESSSTARTTIME, string uNIT1PROCESSENDTIME, int uNIT2PROCESSTIME, string uNIT2PROCESSSTARTTIME, string uNIT2PROCESSENDTIME, int uNIT3PROCESSTIME, string uNIT3PROCESSSTARTTIME, string uNIT3PROCESSENDTIME, int uNIT4PROCESSTIME, string uNIT4PROCESSSTARTTIME, string uNIT4PROCESSENDTIME, int uNIT5PROCESSTIME, string uNIT5PROCESSSTARTTIME, string uNIT5PROCESSENDTIME, int uNIT6PROCESSTIME, string uNIT6PROCESSSTARTTIME, string uNIT6PROCESSENDTIME, int uNIT7PROCESSTIME, string uNIT7PROCESSSTARTTIME, string uNIT7PROCESSENDTIME, int uNIT8PROCESSTIME, string uNIT8PROCESSSTARTTIME, string uNIT8PROCESSENDTIME, int uNIT9PROCESSTIME, string uNIT9PROCESSSTARTTIME, string uNIT9PROCESSENDTIME, int uNIT10PROCESSTIME, string uNIT10PROCESSSTARTTIME, string uNIT10PROCESSENDTIME, int uNIT11PROCESSTIME, string uNIT11PROCESSSTARTTIME, string uNIT11PROCESSENDTIME, int uNIT12PROCESSTIME, string uNIT12PROCESSSTARTTIME, string uNIT12PROCESSENDTIME, int uNIT13PROCESSTIME, string uNIT13PROCESSSTARTTIME, string uNIT13PROCESSENDTIME, int uNIT14PROCESSTIME, string uNIT14PROCESSSTARTTIME, string uNIT14PROCESSENDTIME, int uNIT15PROCESSTIME, string uNIT15PROCESSSTARTTIME, string uNIT15PROCESSENDTIME)
		{
			this._cASSETTESEQNO = cASSETTESEQNO;
			this._jOBSEQNO = jOBSEQNO;
			this._jOBID = jOBID;
			this._tRXID = tRXID;
			this._mESCONTROLSTATE = mESCONTROLSTATE;
			this._nODEID = nODEID;
			this._uPDATETIME = uPDATETIME;
			this._fILENAMA = fILENAMA;
			this._pROCESSTIME = pROCESSTIME;
			this._lOCALPROCESSSTARTTIME = lOCALPROCESSSTARTTIME;
			this._lOCALPROCSSSENDTIME = lOCALPROCSSSENDTIME;
			this._uNIT1PROCESSTIME = uNIT1PROCESSTIME;
			this._uNIT1PROCESSSTARTTIME = uNIT1PROCESSSTARTTIME;
			this._uNIT1PROCESSENDTIME = uNIT1PROCESSENDTIME;
			this._uNIT2PROCESSTIME = uNIT2PROCESSTIME;
			this._uNIT2PROCESSSTARTTIME = uNIT2PROCESSSTARTTIME;
			this._uNIT2PROCESSENDTIME = uNIT2PROCESSENDTIME;
			this._uNIT3PROCESSTIME = uNIT3PROCESSTIME;
			this._uNIT3PROCESSSTARTTIME = uNIT3PROCESSSTARTTIME;
			this._uNIT3PROCESSENDTIME = uNIT3PROCESSENDTIME;
			this._uNIT4PROCESSTIME = uNIT4PROCESSTIME;
			this._uNIT4PROCESSSTARTTIME = uNIT4PROCESSSTARTTIME;
			this._uNIT4PROCESSENDTIME = uNIT4PROCESSENDTIME;
			this._uNIT5PROCESSTIME = uNIT5PROCESSTIME;
			this._uNIT5PROCESSSTARTTIME = uNIT5PROCESSSTARTTIME;
			this._uNIT5PROCESSENDTIME = uNIT5PROCESSENDTIME;
			this._uNIT6PROCESSTIME = uNIT6PROCESSTIME;
			this._uNIT6PROCESSSTARTTIME = uNIT6PROCESSSTARTTIME;
			this._uNIT6PROCESSENDTIME = uNIT6PROCESSENDTIME;
			this._uNIT7PROCESSTIME = uNIT7PROCESSTIME;
			this._uNIT7PROCESSSTARTTIME = uNIT7PROCESSSTARTTIME;
			this._uNIT7PROCESSENDTIME = uNIT7PROCESSENDTIME;
			this._uNIT8PROCESSTIME = uNIT8PROCESSTIME;
			this._uNIT8PROCESSSTARTTIME = uNIT8PROCESSSTARTTIME;
			this._uNIT8PROCESSENDTIME = uNIT8PROCESSENDTIME;
			this._uNIT9PROCESSTIME = uNIT9PROCESSTIME;
			this._uNIT9PROCESSSTARTTIME = uNIT9PROCESSSTARTTIME;
			this._uNIT9PROCESSENDTIME = uNIT9PROCESSENDTIME;
			this._uNIT10PROCESSTIME = uNIT10PROCESSTIME;
			this._uNIT10PROCESSSTARTTIME = uNIT10PROCESSSTARTTIME;
			this._uNIT10PROCESSENDTIME = uNIT10PROCESSENDTIME;
			this._uNIT11PROCESSTIME = uNIT11PROCESSTIME;
			this._uNIT11PROCESSSTARTTIME = uNIT11PROCESSSTARTTIME;
			this._uNIT11PROCESSENDTIME = uNIT11PROCESSENDTIME;
			this._uNIT12PROCESSTIME = uNIT12PROCESSTIME;
			this._uNIT12PROCESSSTARTTIME = uNIT12PROCESSSTARTTIME;
			this._uNIT12PROCESSENDTIME = uNIT12PROCESSENDTIME;
			this._uNIT13PROCESSTIME = uNIT13PROCESSTIME;
			this._uNIT13PROCESSSTARTTIME = uNIT13PROCESSSTARTTIME;
			this._uNIT13PROCESSENDTIME = uNIT13PROCESSENDTIME;
			this._uNIT14PROCESSTIME = uNIT14PROCESSTIME;
			this._uNIT14PROCESSSTARTTIME = uNIT14PROCESSSTARTTIME;
			this._uNIT14PROCESSENDTIME = uNIT14PROCESSENDTIME;
			this._uNIT15PROCESSTIME = uNIT15PROCESSTIME;
			this._uNIT15PROCESSSTARTTIME = uNIT15PROCESSSTARTTIME;
			this._uNIT15PROCESSENDTIME = uNIT15PROCESSENDTIME;
		}

		#endregion

		#region Public Properties

		public virtual long Id
		{
			get {return _id;}
			set {_id = value;}
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

		public virtual string TRXID
		{
			get { return _tRXID; }
			set
			{				
				_tRXID = value;
			}
		}

		public virtual string MESCONTROLSTATE
		{
			get { return _mESCONTROLSTATE; }
			set
			{				
				_mESCONTROLSTATE = value;
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

		public virtual DateTime UPDATETIME
		{
			get { return _uPDATETIME; }
			set { _uPDATETIME = value; }
		}

		public virtual string FILENAMA
		{
			get { return _fILENAMA; }
			set
			{				
				_fILENAMA = value;
			}
		}

		public virtual int PROCESSTIME
		{
			get { return _pROCESSTIME; }
			set { _pROCESSTIME = value; }
		}

		public virtual string LOCALPROCESSSTARTTIME
		{
			get { return _lOCALPROCESSSTARTTIME; }
			set
			{				
				_lOCALPROCESSSTARTTIME = value;
			}
		}

		public virtual string LOCALPROCSSSENDTIME
		{
			get { return _lOCALPROCSSSENDTIME; }
			set
			{				
				_lOCALPROCSSSENDTIME = value;
			}
		}

		public virtual int UNIT1PROCESSTIME
		{
			get { return _uNIT1PROCESSTIME; }
			set { _uNIT1PROCESSTIME = value; }
		}

		public virtual string UNIT1PROCESSSTARTTIME
		{
			get { return _uNIT1PROCESSSTARTTIME; }
			set
			{				
				_uNIT1PROCESSSTARTTIME = value;
			}
		}

		public virtual string UNIT1PROCESSENDTIME
		{
			get { return _uNIT1PROCESSENDTIME; }
			set
			{				
				_uNIT1PROCESSENDTIME = value;
			}
		}

		public virtual int UNIT2PROCESSTIME
		{
			get { return _uNIT2PROCESSTIME; }
			set { _uNIT2PROCESSTIME = value; }
		}

		public virtual string UNIT2PROCESSSTARTTIME
		{
			get { return _uNIT2PROCESSSTARTTIME; }
			set
			{				
				_uNIT2PROCESSSTARTTIME = value;
			}
		}

		public virtual string UNIT2PROCESSENDTIME
		{
			get { return _uNIT2PROCESSENDTIME; }
			set
			{				
				_uNIT2PROCESSENDTIME = value;
			}
		}

		public virtual int UNIT3PROCESSTIME
		{
			get { return _uNIT3PROCESSTIME; }
			set { _uNIT3PROCESSTIME = value; }
		}

		public virtual string UNIT3PROCESSSTARTTIME
		{
			get { return _uNIT3PROCESSSTARTTIME; }
			set
			{				
				_uNIT3PROCESSSTARTTIME = value;
			}
		}

		public virtual string UNIT3PROCESSENDTIME
		{
			get { return _uNIT3PROCESSENDTIME; }
			set
			{				
				_uNIT3PROCESSENDTIME = value;
			}
		}

		public virtual int UNIT4PROCESSTIME
		{
			get { return _uNIT4PROCESSTIME; }
			set { _uNIT4PROCESSTIME = value; }
		}

		public virtual string UNIT4PROCESSSTARTTIME
		{
			get { return _uNIT4PROCESSSTARTTIME; }
			set
			{				
				_uNIT4PROCESSSTARTTIME = value;
			}
		}

		public virtual string UNIT4PROCESSENDTIME
		{
			get { return _uNIT4PROCESSENDTIME; }
			set
			{				
				_uNIT4PROCESSENDTIME = value;
			}
		}

		public virtual int UNIT5PROCESSTIME
		{
			get { return _uNIT5PROCESSTIME; }
			set { _uNIT5PROCESSTIME = value; }
		}

		public virtual string UNIT5PROCESSSTARTTIME
		{
			get { return _uNIT5PROCESSSTARTTIME; }
			set
			{				
				_uNIT5PROCESSSTARTTIME = value;
			}
		}

		public virtual string UNIT5PROCESSENDTIME
		{
			get { return _uNIT5PROCESSENDTIME; }
			set
			{				
				_uNIT5PROCESSENDTIME = value;
			}
		}

		public virtual int UNIT6PROCESSTIME
		{
			get { return _uNIT6PROCESSTIME; }
			set { _uNIT6PROCESSTIME = value; }
		}

		public virtual string UNIT6PROCESSSTARTTIME
		{
			get { return _uNIT6PROCESSSTARTTIME; }
			set
			{				
				_uNIT6PROCESSSTARTTIME = value;
			}
		}

		public virtual string UNIT6PROCESSENDTIME
		{
			get { return _uNIT6PROCESSENDTIME; }
			set
			{				
				_uNIT6PROCESSENDTIME = value;
			}
		}

		public virtual int UNIT7PROCESSTIME
		{
			get { return _uNIT7PROCESSTIME; }
			set { _uNIT7PROCESSTIME = value; }
		}

		public virtual string UNIT7PROCESSSTARTTIME
		{
			get { return _uNIT7PROCESSSTARTTIME; }
			set
			{				
				_uNIT7PROCESSSTARTTIME = value;
			}
		}

		public virtual string UNIT7PROCESSENDTIME
		{
			get { return _uNIT7PROCESSENDTIME; }
			set
			{				
				_uNIT7PROCESSENDTIME = value;
			}
		}

		public virtual int UNIT8PROCESSTIME
		{
			get { return _uNIT8PROCESSTIME; }
			set { _uNIT8PROCESSTIME = value; }
		}

		public virtual string UNIT8PROCESSSTARTTIME
		{
			get { return _uNIT8PROCESSSTARTTIME; }
			set
			{				
				_uNIT8PROCESSSTARTTIME = value;
			}
		}

		public virtual string UNIT8PROCESSENDTIME
		{
			get { return _uNIT8PROCESSENDTIME; }
			set
			{				
				_uNIT8PROCESSENDTIME = value;
			}
		}

		public virtual int UNIT9PROCESSTIME
		{
			get { return _uNIT9PROCESSTIME; }
			set { _uNIT9PROCESSTIME = value; }
		}

		public virtual string UNIT9PROCESSSTARTTIME
		{
			get { return _uNIT9PROCESSSTARTTIME; }
			set
			{				
				_uNIT9PROCESSSTARTTIME = value;
			}
		}

		public virtual string UNIT9PROCESSENDTIME
		{
			get { return _uNIT9PROCESSENDTIME; }
			set
			{				
				_uNIT9PROCESSENDTIME = value;
			}
		}

		public virtual int UNIT10PROCESSTIME
		{
			get { return _uNIT10PROCESSTIME; }
			set { _uNIT10PROCESSTIME = value; }
		}

		public virtual string UNIT10PROCESSSTARTTIME
		{
			get { return _uNIT10PROCESSSTARTTIME; }
			set
			{				
				_uNIT10PROCESSSTARTTIME = value;
			}
		}

		public virtual string UNIT10PROCESSENDTIME
		{
			get { return _uNIT10PROCESSENDTIME; }
			set
			{				
				_uNIT10PROCESSENDTIME = value;
			}
		}

		public virtual int UNIT11PROCESSTIME
		{
			get { return _uNIT11PROCESSTIME; }
			set { _uNIT11PROCESSTIME = value; }
		}

		public virtual string UNIT11PROCESSSTARTTIME
		{
			get { return _uNIT11PROCESSSTARTTIME; }
			set
			{				
				_uNIT11PROCESSSTARTTIME = value;
			}
		}

		public virtual string UNIT11PROCESSENDTIME
		{
			get { return _uNIT11PROCESSENDTIME; }
			set
			{				
				_uNIT11PROCESSENDTIME = value;
			}
		}

		public virtual int UNIT12PROCESSTIME
		{
			get { return _uNIT12PROCESSTIME; }
			set { _uNIT12PROCESSTIME = value; }
		}

		public virtual string UNIT12PROCESSSTARTTIME
		{
			get { return _uNIT12PROCESSSTARTTIME; }
			set
			{				
				_uNIT12PROCESSSTARTTIME = value;
			}
		}

		public virtual string UNIT12PROCESSENDTIME
		{
			get { return _uNIT12PROCESSENDTIME; }
			set
			{				
				_uNIT12PROCESSENDTIME = value;
			}
		}

		public virtual int UNIT13PROCESSTIME
		{
			get { return _uNIT13PROCESSTIME; }
			set { _uNIT13PROCESSTIME = value; }
		}

		public virtual string UNIT13PROCESSSTARTTIME
		{
			get { return _uNIT13PROCESSSTARTTIME; }
			set
			{				
				_uNIT13PROCESSSTARTTIME = value;
			}
		}

		public virtual string UNIT13PROCESSENDTIME
		{
			get { return _uNIT13PROCESSENDTIME; }
			set
			{				
				_uNIT13PROCESSENDTIME = value;
			}
		}

		public virtual int UNIT14PROCESSTIME
		{
			get { return _uNIT14PROCESSTIME; }
			set { _uNIT14PROCESSTIME = value; }
		}

		public virtual string UNIT14PROCESSSTARTTIME
		{
			get { return _uNIT14PROCESSSTARTTIME; }
			set
			{				
				_uNIT14PROCESSSTARTTIME = value;
			}
		}

		public virtual string UNIT14PROCESSENDTIME
		{
			get { return _uNIT14PROCESSENDTIME; }
			set
			{				
				_uNIT14PROCESSENDTIME = value;
			}
		}

		public virtual int UNIT15PROCESSTIME
		{
			get { return _uNIT15PROCESSTIME; }
			set { _uNIT15PROCESSTIME = value; }
		}

		public virtual string UNIT15PROCESSSTARTTIME
		{
			get { return _uNIT15PROCESSSTARTTIME; }
			set
			{				
				_uNIT15PROCESSSTARTTIME = value;
			}
		}

		public virtual string UNIT15PROCESSENDTIME
		{
			get { return _uNIT15PROCESSENDTIME; }
			set
			{				
				_uNIT15PROCESSENDTIME = value;
			}
		}

		

		#endregion
	}
	#endregion
}