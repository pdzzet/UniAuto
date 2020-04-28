using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBRMRECIPECROSS

	/// <summary>
	/// SBRMRECIPECROSS object for NHibernate mapped table 'SBRM_RECIPE_CROSS'.
	/// </summary>
	public class RECIPECROSS
	{
		#region Member Variables
		
		protected long _id;
		protected string _fABTYPE;
		protected string _lINETYPE;
		protected string _oNLINECONTROLSTATE;
		protected string _lINERECIPENAME;
		protected string _pPID;
		protected DateTime _lASTUPDATEDT;
		protected string _uPDATEOPERATOR;
		protected string _uPDATELINEID;
		protected string _uPDATEPCIP;
		protected string _rEMARK;
		protected string _rECIPETYPE;

		#endregion

		#region Constructors

		public RECIPECROSS() { }

		public RECIPECROSS( string fABTYPE, string lINETYPE, string oNLINECONTROLSTATE, string lINERECIPENAME, string pPID, DateTime lASTUPDATEDT, string uPDATEOPERATOR, string uPDATELINEID, string uPDATEPCIP, string rEMARK, string rECIPETYPE )
		{
			this._fABTYPE = fABTYPE;
			this._lINETYPE = lINETYPE;
			this._oNLINECONTROLSTATE = oNLINECONTROLSTATE;
			this._lINERECIPENAME = lINERECIPENAME;
			this._pPID = pPID;
			this._lASTUPDATEDT = lASTUPDATEDT;
			this._uPDATEOPERATOR = uPDATEOPERATOR;
			this._uPDATELINEID = uPDATELINEID;
			this._uPDATEPCIP = uPDATEPCIP;
			this._rEMARK = rEMARK;
			this._rECIPETYPE = rECIPETYPE;
		}

		#endregion

		#region Public Properties

		public virtual long Id
		{
			get {return _id;}
			set {_id = value;}
		}

		public virtual string FABTYPE
		{
			get { return _fABTYPE; }
			set
			{				
				_fABTYPE = value;
			}
		}

		public virtual string LINETYPE
		{
			get { return _lINETYPE; }
			set
			{				
				_lINETYPE = value;
			}
		}

		public virtual string ONLINECONTROLSTATE
		{
			get { return _oNLINECONTROLSTATE; }
			set
			{				
				_oNLINECONTROLSTATE = value;
			}
		}

		public virtual string LINERECIPENAME
		{
			get { return _lINERECIPENAME; }
			set
			{				
				_lINERECIPENAME = value;
			}
		}

		public virtual string PPID
		{
			get { return _pPID; }
			set
			{				
				_pPID = value;
			}
		}

		public virtual DateTime LASTUPDATEDT
		{
			get { return _lASTUPDATEDT; }
			set { _lASTUPDATEDT = value; }
		}

		public virtual string UPDATEOPERATOR
		{
			get { return _uPDATEOPERATOR; }
			set
			{				
				_uPDATEOPERATOR = value;
			}
		}

		public virtual string UPDATELINEID
		{
			get { return _uPDATELINEID; }
			set
			{				
				_uPDATELINEID = value;
			}
		}

		public virtual string UPDATEPCIP
		{
			get { return _uPDATEPCIP; }
			set
			{				
				_uPDATEPCIP = value;
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

		public virtual string RECIPETYPE
		{
			get { return _rECIPETYPE; }
			set
			{				
				_rECIPETYPE = value;
			}
		}

		

		#endregion
	}
	#endregion
}