using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
    #region SBRMPOSITIONDATA

    /// <summary>
	/// SBRMENERGYVISUALIZATIONDATA object for NHibernate mapped table 'SBRM_ENERGYVISUALIZATIONDATA'.
	/// </summary>
	public class PositionEntityData : EntityData
	{
		#region Member Variables
		
		protected long _id;
		protected string _lINEID;
		protected string _nODENO;
		protected string _uNITTYPE;
        protected string _uNITNO;
        protected int _pOSITIONNO;
        protected string _pOSITIONNAME;
		#endregion

		#region Constructors

		public PositionEntityData() { }

        public PositionEntityData(string lINEID, string nODENO, string uNITTYPE, string uNITNO, int pOSITIONNO, string pOSITIONNAME)
		{
			this._lINEID = lINEID;
			this._nODENO = nODENO;
            this._uNITTYPE = uNITTYPE;
            this._uNITNO = uNITNO;
            this._pOSITIONNO = pOSITIONNO;
            this._pOSITIONNAME = pOSITIONNAME;
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

        public virtual string UNITNO
		{
            get { return _uNITNO; }
			set
			{
                _uNITNO = value;
			}
		}

        public virtual string POSITIONNAME
        {
            get { return _pOSITIONNAME; }
            set
            {
                _pOSITIONNAME = value;
            }
        }

        public virtual int POSITIONNO
        {
            get { return _pOSITIONNO; }
            set
            {
                _pOSITIONNO = value;
            }
        }
		#endregion
	}
	#endregion
}