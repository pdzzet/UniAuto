using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBRMROBOTRULEJOBSELECT

	/// <summary>
	/// SBRMROBOTRULEJOBSELECT object for NHibernate mapped table 'SBRM_ROBOT_RULE_JOB_SELECT'.
	/// </summary>
	public class  RobotRuleJobSelectEntityData:EntityData
	{
		#region Member Variables
		
		protected long _id;
		protected string _sERVERNAME;
		protected string _rOBOTNAME;
		protected string _iTEMID;
		protected string _lINETYPE;
		protected string _sELECTTYPE;
		protected int _iTEMSEQ;
		protected string _dESCRIPTION;
		protected string _oBJECTNAME;
		protected string _sTAGETYPE;
		protected string _mETHODNAME;
		protected string _iSENABLED;
		protected string _rEMARKS;
		protected DateTime _lASTUPDATETIME;

		#endregion

		#region Constructors

		public RobotRuleJobSelectEntityData() { }

		public RobotRuleJobSelectEntityData( string sERVERNAME, string rOBOTNAME, string iTEMID, string lINETYPE, string sELECTTYPE, int iTEMSEQ, string dESCRIPTION, string oBJECTNAME, string sTAGETYPE, string mETHODNAME, string iSENABLED, string rEMARKS, DateTime lASTUPDATETIME )
		{
			this._sERVERNAME = sERVERNAME;
			this._rOBOTNAME = rOBOTNAME;
			this._iTEMID = iTEMID;
			this._lINETYPE = lINETYPE;
			this._sELECTTYPE = sELECTTYPE;
			this._iTEMSEQ = iTEMSEQ;
			this._dESCRIPTION = dESCRIPTION;
			this._oBJECTNAME = oBJECTNAME;
			this._sTAGETYPE = sTAGETYPE;
			this._mETHODNAME = mETHODNAME;
			this._iSENABLED = iSENABLED;
			this._rEMARKS = rEMARKS;
			this._lASTUPDATETIME = lASTUPDATETIME;
		}

		#endregion

		#region Public Properties

		public virtual long Id
		{
			get {return _id;}
			set {_id = value;}
		}

		public virtual string SERVERNAME
		{
			get { return _sERVERNAME; }
			set
			{				
				_sERVERNAME = value;
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

		public virtual string ITEMID
		{
			get { return _iTEMID; }
			set
			{				
				_iTEMID = value;
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

		public virtual string SELECTTYPE
		{
			get { return _sELECTTYPE; }
			set
			{				
				_sELECTTYPE = value;
			}
		}

		public virtual int ITEMSEQ
		{
			get { return _iTEMSEQ; }
			set { _iTEMSEQ = value; }
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

		public virtual string STAGETYPE
		{
			get { return _sTAGETYPE; }
			set
			{				
				_sTAGETYPE = value;
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

		

		#endregion
	}
	#endregion
}