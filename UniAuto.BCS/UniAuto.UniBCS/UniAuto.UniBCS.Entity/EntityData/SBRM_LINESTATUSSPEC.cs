using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBRMLINESTATUSSPEC

	/// <summary>
	/// SBRMLINESTATUSSPEC object for NHibernate mapped table 'SBRM_LINESTATUSSPEC'.
	/// </summary>
	public class LineStatusSpec
	{
		#region Member Variables
		
		protected long _id;
		protected string _lINETYPE;
		protected string _cONDITIONSTATUS;
		protected int _cONDITIONSEQNO;
		protected string _eQPNOLIST;
		protected DateTime _uPDATETIME = DateTime.Now;
		protected string _oPERATORID;

		#endregion

		#region Constructors

		public LineStatusSpec() { }

		public LineStatusSpec( string lINETYPE, string cONDITIONSTATUS, int cONDITIONSEQNO, string eQPNOLIST, DateTime uPDATETIME, string oPERATORID )
		{
			this._lINETYPE = lINETYPE;
			this._cONDITIONSTATUS = cONDITIONSTATUS;
			this._cONDITIONSEQNO = cONDITIONSEQNO;
			this._eQPNOLIST = eQPNOLIST;
			this._uPDATETIME = uPDATETIME;
			this._oPERATORID = oPERATORID;
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

		public virtual string CONDITIONSTATUS
		{
			get { return _cONDITIONSTATUS; }
			set
			{				
				_cONDITIONSTATUS = value;
			}
		}

		public virtual int CONDITIONSEQNO
		{
			get { return _cONDITIONSEQNO; }
			set { _cONDITIONSEQNO = value; }
		}

		public virtual string EQPNOLIST
		{
			get { return _eQPNOLIST; }
			set
			{				
				_eQPNOLIST = value;
			}
		}

		public virtual DateTime UPDATETIME
		{
			get { return _uPDATETIME; }
			set { _uPDATETIME = value; }
		}

		public virtual string OPERATORID
		{
			get { return _oPERATORID; }
			set
			{				
				_oPERATORID = value;
			}
		}

		

		#endregion
	}
	#endregion
}