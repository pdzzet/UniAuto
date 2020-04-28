using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBRMSUBBLOCKDEF

	/// <summary>
	/// SBRMSUBBLOCKDEF object for NHibernate mapped table 'SBRM_SUBBLOCK_DEF'.
	/// </summary>
	public class SubBlockDefEntityData:EntityData
	{
		#region Member Variables
		
		protected long _id;
		protected string _lINEID;
		protected string _sERVERNAME;
		protected string _sUBBLOCKID;
		protected string _nODENO;
		protected string _uNITNO;
		protected string _rEMARK;

		#endregion

		#region Constructors

		public SubBlockDefEntityData() { }

        public SubBlockDefEntityData(string lINEID, string sERVERNAME, string sUBBLOCKID, string nODENO, string uNITNO, string rEMARK)
		{
			this._lINEID = lINEID;
			this._sERVERNAME = sERVERNAME;
			this._sUBBLOCKID = sUBBLOCKID;
			this._nODENO = nODENO;
			this._uNITNO = uNITNO;
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

		public virtual string NODENO
		{
			get { return _nODENO; }
			set
			{				
				_nODENO = value;
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