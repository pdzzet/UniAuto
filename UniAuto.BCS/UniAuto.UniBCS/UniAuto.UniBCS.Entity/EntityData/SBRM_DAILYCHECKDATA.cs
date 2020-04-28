using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBRMDAILYCHECKDATA

	/// <summary>
	/// SBRMDAILYCHECKDATA object for NHibernate mapped table 'SBRM_DAILYCHECKDATA'.
	/// </summary>
	public class DailyCheckEntityData:EntityData
	{
		#region Member Variables
		
		protected long _id;
		protected string _lINETYPE;
		protected string _lINEID;
		protected string _sERVERNAME;
		protected string _nODENO;
		protected string _sVID;
		protected string _pARAMETERNAME;
		protected string _iTEM;
		protected string _sITE;
		protected string _dESCRIPTION;
		protected string _rANGE;
		protected string _oPERATOR;
		protected string _dOTRATIO;
		protected string _rEPORTTO;
		protected int _rEPORTUNITNO;
		protected string _uNIT;
		protected string _eXPRESSION;
		protected string _wOFFSET;
		protected string _wPOINTS;
		protected string _bOFFSET;
		protected string _bPOINTS;
		protected string _jOBDATAITEMNAME;

		#endregion

		#region Constructors

		public DailyCheckEntityData() { }

		public DailyCheckEntityData( string lINETYPE, string lINEID, string sERVERNAME, string nODENO, string sVID, string pARAMETERNAME, string iTEM, string sITE, string dESCRIPTION, string rANGE, string oPERATOR, string dOTRATIO, string rEPORTTO, int rEPORTUNITNO, string uNIT, string eXPRESSION, string wOFFSET, string wPOINTS, string bOFFSET, string bPOINTS, string jOBDATAITEMNAME )
		{
			this._lINETYPE = lINETYPE;
			this._lINEID = lINEID;
			this._sERVERNAME = sERVERNAME;
			this._nODENO = nODENO;
			this._sVID = sVID;
			this._pARAMETERNAME = pARAMETERNAME;
			this._iTEM = iTEM;
			this._sITE = sITE;
			this._dESCRIPTION = dESCRIPTION;
			this._rANGE = rANGE;
			this._oPERATOR = oPERATOR;
			this._dOTRATIO = dOTRATIO;
			this._rEPORTTO = rEPORTTO;
			this._rEPORTUNITNO = rEPORTUNITNO;
			this._uNIT = uNIT;
			this._eXPRESSION = eXPRESSION;
			this._wOFFSET = wOFFSET;
			this._wPOINTS = wPOINTS;
			this._bOFFSET = bOFFSET;
			this._bPOINTS = bPOINTS;
			this._jOBDATAITEMNAME = jOBDATAITEMNAME;
		}

		#endregion

		#region Public Properties

		public virtual long Id
		{
			get {return _id;}
			set {_id = value;}
		}

		public virtual string LINETYPE
		{
			get { return _lINETYPE; }
			set
			{				
				_lINETYPE = value;
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

		public virtual string SERVERNAME
		{
			get { return _sERVERNAME; }
			set
			{				
				_sERVERNAME = value;
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

		public virtual string SVID
		{
			get { return _sVID; }
			set
			{				
				_sVID = value;
			}
		}

		public virtual string PARAMETERNAME
		{
			get { return _pARAMETERNAME; }
			set
			{				
				_pARAMETERNAME = value;
			}
		}

		public virtual string ITEM
		{
			get { return _iTEM; }
			set
			{				
				_iTEM = value;
			}
		}

		public virtual string SITE
		{
			get { return _sITE; }
			set
			{				
				_sITE = value;
			}
		}

		public virtual string DESCRIPTION
		{
			get { return _dESCRIPTION; }
			set
			{				
				_dESCRIPTION = value;
			}
		}

		public virtual string RANGE
		{
			get { return _rANGE; }
			set
			{				
				_rANGE = value;
			}
		}

		public virtual string OPERATOR
		{
			get { return _oPERATOR; }
			set
			{				
				_oPERATOR = value;
			}
		}

		public virtual string DOTRATIO
		{
			get { return _dOTRATIO; }
			set
			{				
				_dOTRATIO = value;
			}
		}

		public virtual string REPORTTO
		{
			get { return _rEPORTTO; }
			set
			{				
				_rEPORTTO = value;
			}
		}

		public virtual int REPORTUNITNO
		{
			get { return _rEPORTUNITNO; }
			set { _rEPORTUNITNO = value; }
		}

		public virtual string UNIT
		{
			get { return _uNIT; }
			set
			{				
				_uNIT = value;
			}
		}

		public virtual string EXPRESSION
		{
			get { return _eXPRESSION; }
			set
			{				
				_eXPRESSION = value;
			}
		}

		public virtual string WOFFSET
		{
			get { return _wOFFSET; }
			set
			{				
				_wOFFSET = value;
			}
		}

		public virtual string WPOINTS
		{
			get { return _wPOINTS; }
			set
			{				
				_wPOINTS = value;
			}
		}

		public virtual string BOFFSET
		{
			get { return _bOFFSET; }
			set
			{				
				_bOFFSET = value;
			}
		}

		public virtual string BPOINTS
		{
			get { return _bPOINTS; }
			set
			{				
				_bPOINTS = value;
			}
		}

		public virtual string JOBDATAITEMNAME
		{
			get { return _jOBDATAITEMNAME; }
			set
			{				
				_jOBDATAITEMNAME = value;
			}
		}

		

		#endregion
	}
	#endregion
}