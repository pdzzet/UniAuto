using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBRMROBOTROUTECONDITION

	/// <summary>
	/// SBRMROBOTROUTECONDITION object for NHibernate mapped table 'SBRM_ROBOT_ROUTE_CONDITION'.
	/// </summary>
    
    [Serializable]
	public class RobotRouteConditionEntityData:EntityData
	{
		#region Member Variables
		
		protected long _id;
		protected string _rOUTEID;
		protected string _rOBOTNAME;
		protected string _cONDITIONID;
		protected string _sERVERNAME;
		protected string _lINETYPE;
		protected string _dESCRIPTION;
		protected string _oBJECTNAME;
		protected string _mETHODNAME;
		protected string _iSENABLED;
		protected string _rEMARKS;
		protected DateTime _lASTUPDATETIME;
    protected int _cONDITIONSEQ;
    protected int _rOUTEPRIORITY;

		#endregion

		#region Constructors

		public RobotRouteConditionEntityData() { }

        public RobotRouteConditionEntityData(string rOUTEID, string rOBOTNAME, string cONDITIONID, string sERVERNAME, string lINETYPE, string dESCRIPTION, string oBJECTNAME, string mETHODNAME, string iSENABLED, string rEMARKS, DateTime lASTUPDATETIME, int cONDITIONSEQ, int rOUTEPRIORITY)
		{
			this._rOUTEID = rOUTEID;
			this._rOBOTNAME = rOBOTNAME;
			this._cONDITIONID = cONDITIONID;
			this._sERVERNAME = sERVERNAME;
			this._lINETYPE = lINETYPE;
			this._dESCRIPTION = dESCRIPTION;
			this._oBJECTNAME = oBJECTNAME;
			this._mETHODNAME = mETHODNAME;
			this._iSENABLED = iSENABLED;
			this._rEMARKS = rEMARKS;
			this._lASTUPDATETIME = lASTUPDATETIME;
			this._cONDITIONSEQ = cONDITIONSEQ;
			this._rOUTEPRIORITY = rOUTEPRIORITY;
		}

		#endregion

		#region Public Properties

		public virtual long Id
		{
			get {return _id;}
			set {_id = value;}
		}

		public virtual string ROUTEID
		{
			get { return _rOUTEID; }
			set
			{				
				_rOUTEID = value;
			}
		}

		public virtual string ROBOTNAME
		{
			get { return _rOBOTNAME; }
			set
			{				
				_rOBOTNAME = value;
			}
		}

		public virtual string CONDITIONID
		{
			get { return _cONDITIONID; }
			set
			{				
				_cONDITIONID = value;
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

		public virtual string LINETYPE
		{
			get { return _lINETYPE; }
			set
			{				
				_lINETYPE = value;
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

		public virtual string OBJECTNAME
		{
			get { return _oBJECTNAME; }
			set
			{				
				_oBJECTNAME = value;
			}
		}

		public virtual string METHODNAME
		{
			get { return _mETHODNAME; }
			set
			{				
				_mETHODNAME = value;
			}
		}

		public virtual string ISENABLED
		{
			get { return _iSENABLED; }
			set
			{				
				_iSENABLED = value;
			}
		}

		public virtual string REMARKS
		{
			get { return _rEMARKS; }
			set
			{				
				_rEMARKS = value;
			}
		}

		public virtual DateTime LASTUPDATETIME
		{
			get { return _lASTUPDATETIME; }
			set { _lASTUPDATETIME = value; }
		}

    public virtual int CONDITIONSEQ 
    {
      get { return _cONDITIONSEQ; }
      set { _cONDITIONSEQ = value; }
    }
		
		public virtual int ROUTEPRIORITY 
    {
      get { return _rOUTEPRIORITY; }
      set { _rOUTEPRIORITY = value; }
    }

		#endregion
	}
	#endregion
}