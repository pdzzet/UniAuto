using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBRMNODE

	/// <summary>
	/// SBRMNODE object for NHibernate mapped table 'SBRM_NODE'.
	/// </summary>
	public class EquipmentEntityData : EntityData
	{
		#region Member Variables
		
		protected long _id;
		protected string _lINEID;
		protected string _sERVERNAME;
		protected string _nODENO;
		protected string _nODEID;
		protected string _rEPORTMODE;
		protected string _nODEATTRIBUTE;
		protected int _rECIPEIDX;
		protected int _rECIPELEN;
		protected string _rECIPESEQ;
		protected int _uNITCOUNT;
		protected string _nODENAME;
		protected string _rECIPEREGVALIDATIONENABLED;
		protected string _uSERUNMODE;
		protected string _uSEINDEXERMODE;
		protected string _uSEEDCREPORT;
		protected int _vCRCOUNT;
		protected int _mPLCINTERLOCKCOUNT;
		protected string _pOSITIONPLCTRXNO;
		protected string _oPITYPE;
		protected string _eQPPROFILE;
		protected string _rECIPEPARAVALIDATIONENABLED;
		protected string _aPCREPORT;
		protected int _aPCREPORTTIME;
		protected string _eNERGYREPORT;
		protected int _eNERGYREPORTTIME;
        protected string _vCRTYPE;

		#endregion

		#region Constructors

		public EquipmentEntityData() { }

		public EquipmentEntityData( string lINEID, string sERVERNAME, string nODENO, string nODEID, string rEPORTMODE, string nODEATTRIBUTE, int rECIPEIDX, int rECIPELEN, string rECIPESEQ, int uNITCOUNT, string nODENAME, string rECIPEREGVALIDATIONENABLED, string uSERUNMODE, string uSEINDEXERMODE, string uSEEDCREPORT, int vCRCOUNT, int mPLCINTERLOCKCOUNT, string pOSITIONPLCTRXNO, string oPITYPE, string eQPPROFILE, string rECIPEPARAVALIDATIONENABLED, string aPCREPORT, int aPCREPORTTIME, string eNERGYREPORT, int eNERGYREPORTTIME, string vCRTYPE )
		{
			this._lINEID = lINEID;
			this._sERVERNAME = sERVERNAME;
			this._nODENO = nODENO;
			this._nODEID = nODEID;
			this._rEPORTMODE = rEPORTMODE;
			this._nODEATTRIBUTE = nODEATTRIBUTE;
			this._rECIPEIDX = rECIPEIDX;
			this._rECIPELEN = rECIPELEN;
			this._rECIPESEQ = rECIPESEQ;
			this._uNITCOUNT = uNITCOUNT;
			this._nODENAME = nODENAME;
			this._rECIPEREGVALIDATIONENABLED = rECIPEREGVALIDATIONENABLED;
			this._uSERUNMODE = uSERUNMODE;
			this._uSEINDEXERMODE = uSEINDEXERMODE;
			this._uSEEDCREPORT = uSEEDCREPORT;
			this._vCRCOUNT = vCRCOUNT;
			this._mPLCINTERLOCKCOUNT = mPLCINTERLOCKCOUNT;
			this._pOSITIONPLCTRXNO = pOSITIONPLCTRXNO;
			this._oPITYPE = oPITYPE;
			this._eQPPROFILE = eQPPROFILE;
			this._rECIPEPARAVALIDATIONENABLED = rECIPEPARAVALIDATIONENABLED;
			this._aPCREPORT = aPCREPORT;
			this._aPCREPORTTIME = aPCREPORTTIME;
			this._eNERGYREPORT = eNERGYREPORT;
			this._eNERGYREPORTTIME = eNERGYREPORTTIME;
            this._vCRTYPE = vCRTYPE;
		}

		#endregion

		#region Public Properties

		public virtual long Id
		{
			get {return _id;}
			set {_id = value;}
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

		public virtual string NODEID
		{
			get { return _nODEID; }
			set
			{				
				_nODEID = value;
			}
		}

		public virtual string REPORTMODE
		{
			get { return _rEPORTMODE; }
			set
			{				
				_rEPORTMODE = value;
			}
		}

		public virtual string NODEATTRIBUTE
		{
			get { return _nODEATTRIBUTE; }
			set
			{				
				_nODEATTRIBUTE = value;
			}
		}

		public virtual int RECIPEIDX
		{
			get { return _rECIPEIDX; }
			set { _rECIPEIDX = value; }
		}

		public virtual int RECIPELEN
		{
			get { return _rECIPELEN; }
			set { _rECIPELEN = value; }
		}

		public virtual string RECIPESEQ
		{
			get { return _rECIPESEQ; }
			set
			{				
				_rECIPESEQ = value;
			}
		}

		public virtual int UNITCOUNT
		{
			get { return _uNITCOUNT; }
			set { _uNITCOUNT = value; }
		}

		public virtual string NODENAME
		{
			get { return _nODENAME; }
			set
			{				
				_nODENAME = value;
			}
		}

		public virtual string RECIPEREGVALIDATIONENABLED
		{
			get { return _rECIPEREGVALIDATIONENABLED; }
			set
			{				
				_rECIPEREGVALIDATIONENABLED = value;
			}
		}

		public virtual string USERUNMODE
		{
			get { return _uSERUNMODE; }
			set
			{				
				_uSERUNMODE = value;
			}
		}

		public virtual string USEINDEXERMODE
		{
			get { return _uSEINDEXERMODE; }
			set
			{				
				_uSEINDEXERMODE = value;
			}
		}

		public virtual string USEEDCREPORT
		{
			get { return _uSEEDCREPORT; }
			set
			{				
				_uSEEDCREPORT = value;
			}
		}

		public virtual int VCRCOUNT
		{
			get { return _vCRCOUNT; }
			set { _vCRCOUNT = value; }
		}

		public virtual int MPLCINTERLOCKCOUNT
		{
			get { return _mPLCINTERLOCKCOUNT; }
			set { _mPLCINTERLOCKCOUNT = value; }
		}

		public virtual string POSITIONPLCTRXNO
		{
			get { return _pOSITIONPLCTRXNO; }
			set
			{				
				_pOSITIONPLCTRXNO = value;
			}
		}

		public virtual string OPITYPE
		{
			get { return _oPITYPE; }
			set
			{				
				_oPITYPE = value;
			}
		}

		public virtual string EQPPROFILE
		{
			get { return _eQPPROFILE; }
			set
			{				
				_eQPPROFILE = value;
			}
		}

		public virtual string RECIPEPARAVALIDATIONENABLED
		{
			get { return _rECIPEPARAVALIDATIONENABLED; }
			set
			{				
				_rECIPEPARAVALIDATIONENABLED = value;
			}
		}

		public virtual string APCREPORT
		{
			get { return _aPCREPORT; }
			set
			{				
				_aPCREPORT = value;
			}
		}

		public virtual int APCREPORTTIME
		{
			get { return _aPCREPORTTIME; }
			set { _aPCREPORTTIME = value; }
		}

		public virtual string ENERGYREPORT
		{
			get { return _eNERGYREPORT; }
			set
			{				
				_eNERGYREPORT = value;
			}
		}

		public virtual int ENERGYREPORTTIME
		{
			get { return _eNERGYREPORTTIME; }
			set { _eNERGYREPORTTIME = value; }
		}

        public virtual string VCRTYPE
        {
            get { return _vCRTYPE; }
            set { _vCRTYPE = value; }
        }

		#endregion
	}
	#endregion
}