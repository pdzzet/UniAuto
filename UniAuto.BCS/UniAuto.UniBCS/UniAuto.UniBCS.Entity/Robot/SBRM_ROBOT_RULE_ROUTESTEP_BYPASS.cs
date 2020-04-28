using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{

    //RobotRuleRouteStepByPassEntityData:EntityData
    #region SBRMROBOTRULEROUTESTEPBYPASS

    /// <summary>
    /// SBRMROBOTROUTESTEPBYPASS object for NHibernate mapped table 'SBRM_ROBOT_ROUTE_STEP_BYPASS'.
    /// </summary>
    public class RobotRuleRouteStepByPassEntityData : EntityData
    {
        #region Member Variables

        protected long _id;
        protected string _sERVERNAME;
        protected string _rOBOTNAME;
        protected string _rOUTEID;
        protected int _sTEPID;
        protected string _bYPASSCONDITIONID;
        protected string _dESCRIPTION;
        protected int _gOTOSTEPID;
        protected string _oBJECTNAME;
        protected string _mETHODNAME;
        protected int _bYPASSITEMSEQ;
        protected string _iSENABLED;
        protected string _rEMARKS;
        protected DateTime _lASTUPDATETIME;

        #endregion

        #region Constructors

        public RobotRuleRouteStepByPassEntityData() { }

        public RobotRuleRouteStepByPassEntityData(string sERVERNAME, string rOBOTNAME, string rOUTEID, int sTEPID, string bYPASSCONDITIONID, string dESCRIPTION, int gOTOSTEPID, string oBJECTNAME, string mETHODNAME, int bYPASSITEMSEQ, string iSENABLED, string rEMARKS, DateTime lASTUPDATETIME)
        {
            this._sERVERNAME = sERVERNAME;
            this._rOBOTNAME = rOBOTNAME;
            this._rOUTEID = rOUTEID;
            this._sTEPID = sTEPID;
            this._bYPASSCONDITIONID = bYPASSCONDITIONID;
            this._dESCRIPTION = dESCRIPTION;
            this._gOTOSTEPID = gOTOSTEPID;
            this._oBJECTNAME = oBJECTNAME;
            this._mETHODNAME = mETHODNAME;
            this._bYPASSITEMSEQ = bYPASSITEMSEQ;
            this._iSENABLED = iSENABLED;
            this._rEMARKS = rEMARKS;
            this._lASTUPDATETIME = lASTUPDATETIME;
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

        public virtual string BYPASSCONDITIONID
        {
            get { return _bYPASSCONDITIONID; }
            set
            {
                _bYPASSCONDITIONID = value;
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

        public virtual int GOTOSTEPID
        {
            get { return _gOTOSTEPID; }
            set
            {
                _gOTOSTEPID = value;
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

        public virtual int BYPASSITEMSEQ
        {
            get { return _bYPASSITEMSEQ; }
            set { _bYPASSITEMSEQ = value; }
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