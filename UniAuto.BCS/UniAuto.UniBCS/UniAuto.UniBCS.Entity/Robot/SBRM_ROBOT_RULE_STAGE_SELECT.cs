using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBRMROBOTRULESTAGESELECT

	/// <summary>
	/// SBRMROBOTRULESTAGESELECT object for NHibernate mapped table 'SBRM_ROBOT_RULE_STAGE_SELECT'.
	/// </summary>
	public class RobotRouteRuleStageSelectEntityData:EntityData
	{
		#region Member Variables
		
		protected long _id;
		protected string _sERVERNAME;
		protected string _rOBOTNAME;
		protected string _rOUTEID;
		protected int _sTEPID;
		protected string _iTEMID;
		protected int _iTEMSEQ;
		protected string _dESCRIPTION;
		protected string _oBJECTNAME;
		protected string _iSENABLED;
		protected string _mETHODNAME;
		protected string _rEMARKS;
		protected DateTime _lASTUPDATETIME;

		#endregion

		#region Constructors

		public RobotRouteRuleStageSelectEntityData() { }

		public RobotRouteRuleStageSelectEntityData( string sERVERNAME, string rOBOTNAME, string rOUTEID, int sTEPID, string iTEMID, int iTEMSEQ, string dESCRIPTION, string oBJECTNAME, string iSENABLED, string mETHODNAME, string rEMARKS, DateTime lASTUPDATETIME )
		{
			this._sERVERNAME = sERVERNAME;
			this._rOBOTNAME = rOBOTNAME;
			this._rOUTEID = rOUTEID;
			this._sTEPID = sTEPID;
			this._iTEMID = iTEMID;
			this._iTEMSEQ = iTEMSEQ;
			this._dESCRIPTION = dESCRIPTION;
			this._oBJECTNAME = oBJECTNAME;
			this._iSENABLED = iSENABLED;
			this._mETHODNAME = mETHODNAME;
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

		public virtual string ROUTEID
		{
			get { return _rOUTEID; }
			set
			{				
				_rOUTEID = value;
			}
		}

		public virtual int STEPID
		{
			get { return _sTEPID; }
			set { _sTEPID = value; }
		}

		public virtual string ITEMID
		{
			get { return _iTEMID; }
			set
			{				
				_iTEMID = value;
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

		public virtual string ISENABLED
		{
			get { return _iSENABLED; }
			set
			{				
				_iSENABLED = value;
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