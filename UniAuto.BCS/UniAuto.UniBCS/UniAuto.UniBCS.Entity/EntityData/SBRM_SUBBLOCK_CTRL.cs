using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBRMSUBBLOCKCTRL

	/// <summary>
	/// SBRMSUBBLOCKCTRL object for NHibernate mapped table 'SBRM_SUBBLOCK_CTRL'.
	/// </summary>
	public class SubBlockCtrlEntityData:EntityData
	{
		#region Member Variables
		
		protected long _id;
		protected string _lINEID;
		protected string _sERVERNAME;
		protected string _sUBBLOCKID;
		protected string _sTARTEQP;
		protected string _cONTROLEQP;
		protected string _sTARTEVENTMSG;
		protected string _iNTERLOCKNO;
		protected string _nEXTSUBBLOCKEQP;
		protected string _nEXTSUBBLOCKEQPLIST;
		protected string _eNABLED;
		protected string _iNTERLOCKREPLYNO;
		protected string _rEMARK;

		#endregion

		#region Constructors

		public SubBlockCtrlEntityData() { }

		public SubBlockCtrlEntityData( string lINEID, string sERVERNAME, string sUBBLOCKID, string sTARTEQP, string cONTROLEQP, string sTARTEVENTMSG, string iNTERLOCKNO, string nEXTSUBBLOCKEQP, string nEXTSUBBLOCKEQPLIST, string eNABLED, string iNTERLOCKREPLYNO, string rEMARK )
		{
			this._lINEID = lINEID;
			this._sERVERNAME = sERVERNAME;
			this._sUBBLOCKID = sUBBLOCKID;
			this._sTARTEQP = sTARTEQP;
			this._cONTROLEQP = cONTROLEQP;
			this._sTARTEVENTMSG = sTARTEVENTMSG;
			this._iNTERLOCKNO = iNTERLOCKNO;
			this._nEXTSUBBLOCKEQP = nEXTSUBBLOCKEQP;
			this._nEXTSUBBLOCKEQPLIST = nEXTSUBBLOCKEQPLIST;
			this._eNABLED = eNABLED;
			this._iNTERLOCKREPLYNO = iNTERLOCKREPLYNO;
			this._rEMARK = rEMARK;
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

		public virtual string SUBBLOCKID
		{
			get { return _sUBBLOCKID; }
			set
			{				
				_sUBBLOCKID = value;
			}
		}

		public virtual string STARTEQP
		{
			get { return _sTARTEQP; }
			set
			{				
				_sTARTEQP = value;
			}
		}

		public virtual string CONTROLEQP
		{
			get { return _cONTROLEQP; }
			set
			{				
				_cONTROLEQP = value;
			}
		}

		public virtual string STARTEVENTMSG
		{
			get { return _sTARTEVENTMSG; }
			set
			{				
				_sTARTEVENTMSG = value;
			}
		}

		public virtual string INTERLOCKNO
		{
			get { return _iNTERLOCKNO; }
			set
			{				
				_iNTERLOCKNO = value;
			}
		}

		public virtual string NEXTSUBBLOCKEQP
		{
			get { return _nEXTSUBBLOCKEQP; }
			set
			{				
				_nEXTSUBBLOCKEQP = value;
			}
		}

		public virtual string NEXTSUBBLOCKEQPLIST
		{
			get { return _nEXTSUBBLOCKEQPLIST; }
			set
			{				
				_nEXTSUBBLOCKEQPLIST = value;
			}
		}

		public virtual string ENABLED
		{
			get { return _eNABLED; }
			set
			{				
				_eNABLED = value;
			}
		}

		public virtual string INTERLOCKREPLYNO
		{
			get { return _iNTERLOCKREPLYNO; }
			set
			{				
				_iNTERLOCKREPLYNO = value;
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

		

		#endregion
	}
	#endregion
}
