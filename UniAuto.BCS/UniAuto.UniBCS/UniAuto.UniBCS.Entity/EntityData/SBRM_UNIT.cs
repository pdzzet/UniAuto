using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBRMUNIT

	/// <summary>
	/// SBRMUNIT object for NHibernate mapped table 'SBRM_UNIT'.
	/// </summary>
	public class UnitEntityData:EntityData
	{
		#region Member Variables
		
		protected long _id;
		protected string _lINEID;
		protected string _nODEID;
		protected string _uNITNO;
		protected string _uNITID;
		protected string _nODENO;
		protected string _uNITTYPE;
		protected string _sERVERNAME;
		protected string _pOSITIONPLCTRXNO;
		protected string _uNITATTRIBUTE;

		#endregion

		#region Constructors

		public UnitEntityData() { }

		public UnitEntityData( string lINEID, string nODEID, string uNITNO, string uNITID, string nODENO, string uNITTYPE, string sERVERNAME, string pOSITIONPLCTRXNO, string uNITATTRIBUTE )
		{
			this._lINEID = lINEID;
			this._nODEID = nODEID;
			this._uNITNO = uNITNO;
			this._uNITID = uNITID;
			this._nODENO = nODENO;
			this._uNITTYPE = uNITTYPE;
			this._sERVERNAME = sERVERNAME;
			this._pOSITIONPLCTRXNO = pOSITIONPLCTRXNO;
			this._uNITATTRIBUTE = uNITATTRIBUTE;
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

		public virtual string NODEID
		{
			get { return _nODEID; }
			set
			{				
				_nODEID = value;
			}
		}

		public virtual string UNITNO
		{
			get { return _uNITNO; }
			set
			{				
				_uNITNO = value;
			}
		}

		public virtual string UNITID
		{
			get { return _uNITID; }
			set
			{				
				_uNITID = value;
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

		public virtual string UNITTYPE
		{
			get { return _uNITTYPE; }
			set
			{				
				_uNITTYPE = value;
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

		public virtual string POSITIONPLCTRXNO
		{
			get { return _pOSITIONPLCTRXNO; }
			set
			{				
				_pOSITIONPLCTRXNO = value;
			}
		}

		public virtual string UNITATTRIBUTE
		{
			get { return _uNITATTRIBUTE; }
			set
			{				
				_uNITATTRIBUTE = value;
			}
		}

		

		#endregion
	}
	#endregion
}