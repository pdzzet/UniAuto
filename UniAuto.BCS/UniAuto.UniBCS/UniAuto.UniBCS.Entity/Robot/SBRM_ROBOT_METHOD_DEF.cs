using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBRMROBOTMETHODDEF

	/// <summary>
	/// SBRMROBOTMETHODDEF object for NHibernate mapped table 'SBRM_ROBOT_METHOD_DEF'.
	/// </summary>
	public class RobotMethodEntityData:EntityData
	{
		#region Member Variables
		
		protected long _id;
		protected string _oBJECTNAME;
		protected string _mETHODNAME;
		protected string _mETHODRULETYPE;
		protected string _dESCRIPTION;
		protected string _aUTHOR;
		protected DateTime _lASTUPDATEDATE;
		protected string _iSENABLED;
		protected string _rEMARKS;
		protected string _fUNCKEY;

		#endregion

		#region Constructors

		public RobotMethodEntityData() { }

        public RobotMethodEntityData(string oBJECTNAME, string mETHODNAME, string mETHODRULETYPE, string dESCRIPTION, string aUTHOR, DateTime lASTUPDATEDATE, string iSENABLED, string rEMARKS, string fUNCKEY)
		{
			this._oBJECTNAME = oBJECTNAME;
			this._mETHODNAME = mETHODNAME;
			this._mETHODRULETYPE = mETHODRULETYPE;
			this._dESCRIPTION = dESCRIPTION;
			this._aUTHOR = aUTHOR;
			this._lASTUPDATEDATE = lASTUPDATEDATE;
			this._iSENABLED = iSENABLED;
			this._rEMARKS = rEMARKS;
			this._fUNCKEY = fUNCKEY;
		}

		#endregion

		#region Public Properties

		public virtual long Id
		{
			get {return _id;}
			set {_id = value;}
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

		public virtual string METHODRULETYPE
		{
			get { return _mETHODRULETYPE; }
			set
			{				
				_mETHODRULETYPE = value;
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

		public virtual string AUTHOR
		{
			get { return _aUTHOR; }
			set
			{				
				_aUTHOR = value;
			}
		}

		public virtual DateTime LASTUPDATEDATE
		{
			get { return _lASTUPDATEDATE; }
			set { _lASTUPDATEDATE = value; }
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

		public virtual string FUNCKEY
		{
			get { return _fUNCKEY; }
			set
			{				
				_fUNCKEY = value;
			}
		}

		#endregion
	}
	#endregion
}