using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
	
    #region SBRMROBOTROUTESTEP

    /// <summary>
    /// SBRMROBOTROUTESTEP object for NHibernate mapped table 'SBRM_ROBOT_ROUTE_STEP'.
    /// </summary>
    [Serializable]
    public class RobotRouteStepEntityData : EntityData, ICloneable
    {
        #region Member Variables

        protected long _id;
        protected string _sERVERNAME;
        protected string _rOBOTNAME;
        protected string _rOUTEID;
        protected int _sTEPID;
        protected string _lINETYPE;
        protected string _dESCRIPTION;
        protected string _rOBOTACTION;
        protected string _rOBOTUSEARM;
        protected string _rOBOTRULE;
        protected string _sTAGEIDLIST;
        protected string _iNPUTTRACKDATA;
        protected string _oUTPUTTRACKDATA;
        protected string _rEMARKS;
        protected DateTime _lASTUPDATETIME;
        protected int _nEXTSTEPID;
        protected string _cROSSSTAGEFLAG;

        #endregion

        #region Constructors

        public RobotRouteStepEntityData() { }

        public RobotRouteStepEntityData(string sERVERNAME, string rOBOTNAME, string rOUTEID, int sTEPID, string lINETYPE, string dESCRIPTION, string rOBOTACTION, string rOBOTUSEARM, string rOBOTRULE, string sTAGEIDLIST, string iNPUTTRACKDATA, string oUTPUTTRACKDATA, string rEMARKS, DateTime lASTUPDATETIME, int nEXTSTEPID, string cROSSSTAGEFLAG)
        {
            this._sERVERNAME = sERVERNAME;
            this._rOBOTNAME = rOBOTNAME;
            this._rOUTEID = rOUTEID;
            this._sTEPID = sTEPID;
            this._lINETYPE = lINETYPE;
            this._dESCRIPTION = dESCRIPTION;
            this._rOBOTACTION = rOBOTACTION;
            this._rOBOTUSEARM = rOBOTUSEARM;
            this._rOBOTRULE = rOBOTRULE;
            this._sTAGEIDLIST = sTAGEIDLIST;
            this._iNPUTTRACKDATA = iNPUTTRACKDATA;
            this._oUTPUTTRACKDATA = oUTPUTTRACKDATA;
            this._rEMARKS = rEMARKS;
            this._lASTUPDATETIME = lASTUPDATETIME;
            this._nEXTSTEPID = nEXTSTEPID;
            this._cROSSSTAGEFLAG = cROSSSTAGEFLAG;
        }

        #endregion

        #region Public Properties

        public virtual long Id
        {
            get { return _id; }
            set { _id = value; }
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

        public virtual string ROBOTACTION
        {
            get { return _rOBOTACTION; }
            set
            {
                _rOBOTACTION = value;
            }
        }

        public virtual string ROBOTUSEARM
        {
            get { return _rOBOTUSEARM; }
            set
            {
                _rOBOTUSEARM = value;
            }
        }

        public virtual string ROBOTRULE
        {
            get { return _rOBOTRULE; }
            set
            {
                _rOBOTRULE = value;
            }
        }

        public virtual string STAGEIDLIST
        {
            get { return _sTAGEIDLIST; }
            set
            {
                _sTAGEIDLIST = value;
            }
        }

        public virtual string INPUTTRACKDATA
        {
            get { return _iNPUTTRACKDATA; }
            set
            {
                _iNPUTTRACKDATA = value;
            }
        }

        public virtual string OUTPUTTRACKDATA
        {
            get { return _oUTPUTTRACKDATA; }
            set
            {
                _oUTPUTTRACKDATA = value;
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

        public virtual int NEXTSTEPID
        {
            get { return _nEXTSTEPID; }
            set { _nEXTSTEPID = value; }
        }

        public virtual string CROSSSTAGEFLAG
        {
            get { return _cROSSSTAGEFLAG; }
            set { _cROSSSTAGEFLAG = value; }
        }

        #endregion

        public virtual object Clone()
        {
            RobotRouteStepEntityData routestep = (RobotRouteStepEntityData)this.MemberwiseClone();
            return routestep;
        }

    }
    #endregion

    #region [ 20151007 old SBRMROBOTROUTESTEP ]

    /// <summary>
    /// SBRMROBOTROUTESTEP object for NHibernate mapped table 'SBRM_ROBOT_ROUTE_STEP'. 20150817 add for Clone .以後更新DB時要特別注意
    /// </summary>
    //[Serializable]
    //public class RobotRouteStepEntityData:EntityData,ICloneable
    //{
    //    #region Member Variables

    //    protected long _id;
    //    protected string _sERVERNAME;
    //    protected string _rOBOTNAME;
    //    protected string _rOUTEID;
    //    protected int _sTEPID;
    //    protected string _lINETYPE;
    //    protected string _dESCRIPTION;
    //    protected string _rOBOTACTION;
    //    protected string _rOBOTUSEARM;
    //    protected string _rOBOTRULE;
    //    protected string _sTAGEIDLIST;
    //    protected string _iNPUTTRACKDATA;
    //    protected string _oUTPUTTRACKDATA;
    //    protected string _rEMARKS;
    //    protected DateTime _lASTUPDATETIME;

    //    #endregion

    //    #region Constructors

    //    public RobotRouteStepEntityData() { }

    //    public RobotRouteStepEntityData(string sERVERNAME, string rOBOTNAME, string rOUTEID, int sTEPID, string lINETYPE, string dESCRIPTION, string rOBOTACTION, string rOBOTUSEARM, string rOBOTRULE, string sTAGEIDLIST, string iNPUTTRACKDATA, string oUTPUTTRACKDATA, string rEMARKS, DateTime lASTUPDATETIME)
    //    {
    //        this._sERVERNAME = sERVERNAME;
    //        this._rOBOTNAME = rOBOTNAME;
    //        this._rOUTEID = rOUTEID;
    //        this._sTEPID = sTEPID;
    //        this._lINETYPE = lINETYPE;
    //        this._dESCRIPTION = dESCRIPTION;
    //        this._rOBOTACTION = rOBOTACTION;
    //        this._rOBOTUSEARM = rOBOTUSEARM;
    //        this._rOBOTRULE = rOBOTRULE;
    //        this._sTAGEIDLIST = sTAGEIDLIST;
    //        this._iNPUTTRACKDATA = iNPUTTRACKDATA;
    //        this._oUTPUTTRACKDATA = oUTPUTTRACKDATA;
    //        this._rEMARKS = rEMARKS;
    //        this._lASTUPDATETIME = lASTUPDATETIME;
    //    }

    //    #endregion

    //    #region Public Properties

    //    public virtual long Id
    //    {
    //        get {return _id;}
    //        set {_id = value;}
    //    }

    //    public virtual string SERVERNAME
    //    {
    //        get { return _sERVERNAME; }
    //        set
    //        {				
    //            _sERVERNAME = value;
    //        }
    //    }

    //    public virtual string ROBOTNAME
    //    {
    //        get { return _rOBOTNAME; }
    //        set
    //        {				
    //            _rOBOTNAME = value;
    //        }
    //    }

    //    public virtual string ROUTEID
    //    {
    //        get { return _rOUTEID; }
    //        set
    //        {				
    //            _rOUTEID = value;
    //        }
    //    }

    //    public virtual int STEPID
    //    {
    //        get { return _sTEPID; }
    //        set
    //        {				
    //            _sTEPID = value;
    //        }
    //    }

    //    public virtual string LINETYPE
    //    {
    //        get { return _lINETYPE; }
    //        set
    //        {				
    //            _lINETYPE = value;
    //        }
    //    }

    //    public virtual string DESCRIPTION
    //    {
    //        get { return _dESCRIPTION; }
    //        set
    //        {				
    //            _dESCRIPTION = value;
    //        }
    //    }

    //    public virtual string ROBOTACTION
    //    {
    //        get { return _rOBOTACTION; }
    //        set
    //        {				
    //            _rOBOTACTION = value;
    //        }
    //    }

    //    public virtual string ROBOTUSEARM
    //    {
    //        get { return _rOBOTUSEARM; }
    //        set
    //        {				
    //            _rOBOTUSEARM = value;
    //        }
    //    }

    //    public virtual string ROBOTRULE
    //    {
    //        get { return _rOBOTRULE; }
    //        set
    //        {				
    //            _rOBOTRULE = value;
    //        }
    //    }

    //    public virtual string STAGEIDLIST
    //    {
    //        get { return _sTAGEIDLIST; }
    //        set
    //        {				
    //            _sTAGEIDLIST = value;
    //        }
    //    }

    //    public virtual string INPUTTRACKDATA
    //    {
    //        get { return _iNPUTTRACKDATA; }
    //        set
    //        {				
    //            _iNPUTTRACKDATA = value;
    //        }
    //    }

    //    public virtual string OUTPUTTRACKDATA
    //    {
    //        get { return _oUTPUTTRACKDATA; }
    //        set
    //        {				
    //            _oUTPUTTRACKDATA = value;
    //        }
    //    }

    //    public virtual string REMARKS
    //    {
    //        get { return _rEMARKS; }
    //        set
    //        {				
    //            _rEMARKS = value;
    //        }
    //    }

    //    public virtual DateTime LASTUPDATETIME
    //    {
    //        get { return _lASTUPDATETIME; }
    //        set { _lASTUPDATETIME = value; }
    //    }



    //    #endregion

    //    public virtual object Clone()
    //    {
    //        RobotRouteStepEntityData routestep = (RobotRouteStepEntityData)this.MemberwiseClone();
    //        return routestep;
    //    }
    //}
    #endregion

}