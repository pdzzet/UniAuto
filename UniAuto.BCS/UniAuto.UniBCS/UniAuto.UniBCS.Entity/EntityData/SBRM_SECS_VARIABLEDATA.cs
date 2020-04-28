using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBRMSECSVARIABLEDATA

	/// <summary>
	/// SBRMSECSVARIABLEDATA object for NHibernate mapped table 'SBRM_SECS_VARIABLEDATA'.
	/// </summary>
	public class SECSVARIABLEDATA:EntityData
	{
		#region Member Variables
		
		protected long _id;
		protected string _lINETYPE;
		protected string _lINEID;
		protected string _sERVERNAME;
		protected string _nODENO;
		protected string _dATATYPE;
		protected string _tRID;
		protected string _iTEMNAME;
		protected string _iTEMTYPE;
		protected string _iTEMID;
		protected string _dESCRIPTION;
		protected string _iTEMSET;
		protected string _sP1;
		protected string _sP2;
		protected string _sP3;

		#endregion

		#region Constructors

		public SECSVARIABLEDATA() { }

		public SECSVARIABLEDATA( string lINETYPE, string lINEID, string sERVERNAME, string nODENO, string dATATYPE, string tRID, string iTEMNAME, string iTEMTYPE, string iTEMID, string dESCRIPTION, string iTEMSET, string sP1, string sP2, string sP3 )
		{
			this._lINETYPE = lINETYPE;
			this._lINEID = lINEID;
			this._sERVERNAME = sERVERNAME;
			this._nODENO = nODENO;
			this._dATATYPE = dATATYPE;
			this._tRID = tRID;
			this._iTEMNAME = iTEMNAME;
			this._iTEMTYPE = iTEMTYPE;
			this._iTEMID = iTEMID;
			this._dESCRIPTION = dESCRIPTION;
			this._iTEMSET = iTEMSET;
			this._sP1 = sP1;
			this._sP2 = sP2;
			this._sP3 = sP3;
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

		public virtual string DATATYPE
		{
			get { return _dATATYPE; }
			set
			{				
				_dATATYPE = value;
			}
		}

		public virtual string TRID
		{
			get { return _tRID; }
			set
			{				
				_tRID = value;
			}
		}

		public virtual string ITEMNAME
		{
			get { return _iTEMNAME; }
			set
			{				
				_iTEMNAME = value;
			}
		}

		public virtual string ITEMTYPE
		{
			get { return _iTEMTYPE; }
			set
			{				
				_iTEMTYPE = value;
			}
		}

		public virtual string ITEMID
		{
			get { return _iTEMID; }
			set
			{				
				_iTEMID = value;
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

		public virtual string ITEMSET
		{
			get { return _iTEMSET; }
			set
			{				
				_iTEMSET = value;
			}
		}

		public virtual string SP1
		{
			get { return _sP1; }
			set
			{				
				_sP1 = value;
			}
		}

		public virtual string SP2
		{
			get { return _sP2; }
			set
			{				
				_sP2 = value;
			}
		}

		public virtual string SP3
		{
			get { return _sP3; }
			set
			{				
				_sP3 = value;
			}
		}

		

		#endregion
	}
	#endregion
}