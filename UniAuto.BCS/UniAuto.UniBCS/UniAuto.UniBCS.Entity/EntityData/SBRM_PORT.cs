using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBRMPORT

	/// <summary>
	/// SBRMPORT object for NHibernate mapped table 'SBRM_PORT'.
	/// </summary>
    public class PortEntityData : EntityData
	{
		#region Member Variables
		
		protected long _id;
		protected string _lINEID;
		protected string _sERVERNAME;
		protected string _nODENO;
		protected string _nODEID;
		protected string _pORTNO;
		protected string _pORTID;
		protected int _mAXCOUNT;
		protected string _pORTATTRIBUTE;
		protected string _pROCESSSTARTTYPE;
		protected string _pOSITIONPLCTRXNO;
		protected string _mAPPINGENABLE;
		protected string _cSTTYPE;

		#endregion

		#region Constructors

		public PortEntityData() { }

		public PortEntityData( string lINEID, string sERVERNAME, string nODENO, string nODEID, string pORTNO, string pORTID, int mAXCOUNT, string pORTATTRIBUTE, string pROCESSSTARTTYPE, string pOSITIONPLCTRXNO, string mAPPINGENABLE, string cSTTYPE )
		{
			this._lINEID = lINEID;
			this._sERVERNAME = sERVERNAME;
			this._nODENO = nODENO;
			this._nODEID = nODEID;
			this._pORTNO = pORTNO;
			this._pORTID = pORTID;
			this._mAXCOUNT = mAXCOUNT;
			this._pORTATTRIBUTE = pORTATTRIBUTE;
			this._pROCESSSTARTTYPE = pROCESSSTARTTYPE;
			this._pOSITIONPLCTRXNO = pOSITIONPLCTRXNO;
			this._mAPPINGENABLE = mAPPINGENABLE;
			this._cSTTYPE = cSTTYPE;
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

		public virtual string PORTNO
		{
			get { return _pORTNO; }
			set
			{				
				_pORTNO = value;
			}
		}

		public virtual string PORTID
		{
			get { return _pORTID; }
			set
			{				
				_pORTID = value;
			}
		}

		public virtual int MAXCOUNT
		{
			get { return _mAXCOUNT; }
			set { _mAXCOUNT = value; }
		}

		public virtual string PORTATTRIBUTE
		{
			get { return _pORTATTRIBUTE; }
			set
			{				
				_pORTATTRIBUTE = value;
			}
		}

		public virtual string PROCESSSTARTTYPE
		{
			get { return _pROCESSSTARTTYPE; }
			set
			{				
				_pROCESSSTARTTYPE = value;
			}
		}

		public virtual string POSITIONPLCTRXNO
		{
			get { return _pOSITIONPLCTRXNO; }
			set
			{				
				_pOSITIONPLCTRXNO = value;
			}
		}

		public virtual string MAPPINGENABLE
		{
			get { return _mAPPINGENABLE; }
			set
			{				
				_mAPPINGENABLE = value;
			}
		}

		public virtual string CSTTYPE
		{
			get { return _cSTTYPE; }
			set
			{				
				_cSTTYPE = value;
			}
		}

		

		#endregion
	}
	#endregion
}