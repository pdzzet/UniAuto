using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBRMROBOT

	/// <summary>
	/// SBRMROBOT object for NHibernate mapped table 'SBRM_ROBOT'.
	/// </summary>
    
	public class RobotEntityData:EntityData
	{
		#region Member Variables
		
		protected long _id;
		protected string _rOBOTNAME;
		protected string _sERVERNAME;
		protected string _lINETYPE;
		protected string _lINEID;
		protected string _nODENO;
		protected string _pORTFETCHSEQ;
		protected string _sLOTFETCHSEQ;
		protected string _pORTSTORESEQ;
		protected string _sLOTSTORESEQ;
		protected string _uNITNO;
		protected int _rOBOTARMQTY;
		protected int _aRMJOBQTY;
		protected string _rEMARKS;

		#endregion

		#region Constructors

		public RobotEntityData() { }

        public RobotEntityData(string rOBOTNAME, string sERVERNAME, string lINETYPE, string lINEID, string nODENO, string pORTFETCHSEQ, string sLOTFETCHSEQ, string pORTSTORESEQ, string sLOTSTORESEQ, string uNITNO, int rOBOTARMQTY, int aRMJOBQTY, string rEMARKS)
		{
			this._rOBOTNAME = rOBOTNAME;
			this._sERVERNAME = sERVERNAME;
			this._lINETYPE = lINETYPE;
			this._lINEID = lINEID;
			this._nODENO = nODENO;
			this._pORTFETCHSEQ = pORTFETCHSEQ;
			this._sLOTFETCHSEQ = sLOTFETCHSEQ;
			this._pORTSTORESEQ = pORTSTORESEQ;
			this._sLOTSTORESEQ = sLOTSTORESEQ;
			this._uNITNO = uNITNO;
			this._rOBOTARMQTY = rOBOTARMQTY;
			this._aRMJOBQTY = aRMJOBQTY;
			this._rEMARKS = rEMARKS;
		}

		#endregion

		#region Public Properties

		public virtual long Id
		{
			get {return _id;}
			set {_id = value;}
		}

		public virtual string ROBOTNAME
		{
			get { return _rOBOTNAME; }
			set
			{				
				_rOBOTNAME = value;
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

		public virtual string LINEID
		{
			get { return _lINEID; }
			set
			{				
				_lINEID = value;
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

		public virtual string PORTFETCHSEQ
		{
			get { return _pORTFETCHSEQ; }
			set
			{				
				_pORTFETCHSEQ = value;
			}
		}

		public virtual string SLOTFETCHSEQ
		{
			get { return _sLOTFETCHSEQ; }
			set
			{				
				_sLOTFETCHSEQ = value;
			}
		}

		public virtual string PORTSTORESEQ
		{
			get { return _pORTSTORESEQ; }
			set
			{				
				_pORTSTORESEQ = value;
			}
		}

		public virtual string SLOTSTORESEQ
		{
			get { return _sLOTSTORESEQ; }
			set
			{				
				_sLOTSTORESEQ = value;
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

		public virtual int ROBOTARMQTY
		{
			get { return _rOBOTARMQTY; }
			set { _rOBOTARMQTY = value; }
		}

		public virtual int ARMJOBQTY
		{
			get { return _aRMJOBQTY; }
			set { _aRMJOBQTY = value; }
		}

		public virtual string REMARKS
		{
			get { return _rEMARKS; }
			set
			{				
				_rEMARKS = value;
			}
		}

		

		#endregion
	}
	#endregion
}