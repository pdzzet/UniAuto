//RobotRouteMstEntityData:EntityData

using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
    #region SBRMROBOTROUTEMST

    /// <summary>
    /// SBRMROBOTROUTEMST object for NHibernate mapped table 'SBRM_ROBOT_ROUTE_MST'.
    /// </summary>
    public class RobotRouteMstEntityData : EntityData
    {
        #region Member Variables

        protected long _id;
        protected string _sERVERNAME;
        protected string _rOBOTNAME;
        protected string _rOUTEID;
        protected string _rOUTENAME;
        protected string _lINETYPE;
        protected string _iSENABLED;
        protected string _dESCRIPTION;
        protected string _rEMARKS;
        protected DateTime _lASTUPDATETIME;
        protected string _rTCMODEFLAG;
        protected int _rOUTEPRIORITY;
        protected string _rTCFORCERETURNFLAG;

        #endregion

        #region Constructors

        public RobotRouteMstEntityData() { }

        public RobotRouteMstEntityData(string sERVERNAME, string rOBOTNAME, string rOUTEID, string rOUTENAME, string lINETYPE, string iSENABLED, string dESCRIPTION, string rEMARKS, DateTime lASTUPDATETIME, string rTCMODEFLAG, int rOUTEPRIORITY, string rTCFORCERETURNFLAG)
        {
            this._sERVERNAME = sERVERNAME;
            this._rOBOTNAME = rOBOTNAME;
            this._rOUTEID = rOUTEID;
            this._rOUTENAME = rOUTENAME;
            this._lINETYPE = lINETYPE;
            this._iSENABLED = iSENABLED;
            this._dESCRIPTION = dESCRIPTION;
            this._rEMARKS = rEMARKS;
            this._lASTUPDATETIME = lASTUPDATETIME;
            this._rTCMODEFLAG = rTCMODEFLAG;
            this._rOUTEPRIORITY = rOUTEPRIORITY;
            this._rTCFORCERETURNFLAG = rTCFORCERETURNFLAG;
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

        public virtual string ROUTENAME
        {
            get { return _rOUTENAME; }
            set
            {
                _rOUTENAME = value;
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

        public virtual string ISENABLED
        {
            get { return _iSENABLED; }
            set
            {
                _iSENABLED = value;
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

        public virtual string RTCMODEFLAG
        {
            get { return _rTCMODEFLAG; }
            set
            {
                _rTCMODEFLAG = value;
            }
        }

		    public virtual int ROUTEPRIORITY 
        {
            get { return _rOUTEPRIORITY; }
            set { _rOUTEPRIORITY = value; }
        }
        
        public virtual string RTCFORCERETURNFLAG
        {
            get { return _rTCFORCERETURNFLAG; }
            set
            {
                _rTCFORCERETURNFLAG = value;
            }
        }
        

        #endregion
    }
    #endregion
}