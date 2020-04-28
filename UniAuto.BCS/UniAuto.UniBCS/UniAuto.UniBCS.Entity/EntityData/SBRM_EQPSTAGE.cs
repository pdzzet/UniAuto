using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	#region SBRMEQPSTAGE

	/// <summary>
	/// SBRMEQPSTAGE object for NHibernate mapped table 'SBRM_EQPSTAGE'.
	/// </summary>
	public class RobotPositionEntityData : EntityData
	{
		#region Member Variables
		
		protected long _id;
		protected string _lineType;
		protected string _rOBOTPOSITIONNO;
		protected string _dESCRIPTION;
		protected string _nODENO;
		protected string _dIRECTION;

		#endregion

		#region Constructors

		public RobotPositionEntityData() { }

		public RobotPositionEntityData( string lineType, string rOBOTPOSITIONNO, string dESCRIPTION, string nODENO, string dIRECTION )
		{
			this._lineType = lineType;
			this._rOBOTPOSITIONNO = rOBOTPOSITIONNO;
			this._dESCRIPTION = dESCRIPTION;
			this._nODENO = nODENO;
			this._dIRECTION = dIRECTION;
		}

		#endregion

		#region Public Properties

		public virtual long Id
		{
			get {return _id;}
			set {_id = value;}
		}

		public virtual string LineType
		{
			get { return _lineType; }
			set
			{				
				_lineType = value;
			}
		}

		public virtual string ROBOTPOSITIONNO
		{
			get { return _rOBOTPOSITIONNO; }
			set
			{				
				_rOBOTPOSITIONNO = value;
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

		public virtual string NODENO
		{
			get { return _nODENO; }
			set
			{				
				_nODENO = value;
			}
		}

		public virtual string DIRECTION
		{
			get { return _dIRECTION; }
			set
			{				
				_dIRECTION = value;
			}
		}

		

		#endregion
	}
	#endregion
}