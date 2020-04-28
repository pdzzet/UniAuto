//RobotStageEntityData : EntityData

using System;
using System.Collections;

namespace UniAuto.UniBCS.Entity
{
    #region SBRMROBOTSTAGE

    /// <summary>
    /// SBRMROBOTSTAGE object for NHibernate mapped table 'SBRM_ROBOT_STAGE'.
    /// </summary>
    public class RobotStageEntityData : EntityData
    {
        #region Member Variables

        protected long _id;
        protected string _sERVERNAME;
        protected string _rOBOTNAME;
        protected string _sTAGEID;
        protected string _sTAGENAME;
        protected string _lINEID;
        protected string _nODENO;
        protected string _sTAGEIDBYNODE;
        protected string _sTAGETYPE;
        protected int _pRIORITY;
        protected string _sTAGEREPORTTRXNAME;
        protected string _sTAGEJOBDATATRXNAME;
        protected string _iSMULTISLOT;
        protected int _sLOTMAXCOUNT;
        protected string _rECIPECHENCKFLAG;
        protected string _dUMMYCHECKFLAG;
        protected string _gETREADYFLAG;
        protected string _pUTREADYFLAG;
        protected string _pREFETCHFLAG;
        protected string _wAITFRONTFLAG;
        protected string _uPSTREAMPATHTRXNAME;
        protected string _uPSTREAMJOBDATAPATHTRXNAME;
        protected string _dOWNSTREAMPATHTRXNAME;
        protected string _dOWNSTREAMJOBDATAPATHTRXNAME;
        protected string _tRACKDATASEQLIST;
        protected string _cASSETTETYPE;
        protected string _rEMARKS;
        protected string _iSENABLED;
        protected string _sLOTFETCHSEQ;
        protected string _sLOTSTORESEQ;
        protected string _eXCHANGETYPE;
        protected string _eQROBOTIFTYPE;
        protected string _rTCREWORKFLAG;

        #endregion

        #region Constructors

        public RobotStageEntityData() { }

        public RobotStageEntityData(string sERVERNAME, string rOBOTNAME, string sTAGEID, string sTAGENAME, string lINEID, string nODENO, string sTAGEIDBYNODE, string sTAGETYPE, int pRIORITY, string sTAGEREPORTTRXNAME, string sTAGEJOBDATATRXNAME, string iSMULTISLOT, int sLOTMAXCOUNT, string rECIPECHENCKFLAG, string dUMMYCHECKFLAG, string gETREADYFLAG, string pUTREADYFLAG, string pREFETCHFLAG, string wAITFRONTFLAG, string uPSTREAMPATHTRXNAME, string uPSTREAMJOBDATAPATHTRXNAME, string dOWNSTREAMPATHTRXNAME, string dOWNSTREAMJOBDATAPATHTRXNAME, string tRACKDATASEQLIST, string cASSETTETYPE, string rEMARKS, string iSENABLED, string sLOTFETCHSEQ, string sLOTSTORESEQ, string eXCHANGETYPE, string eQROBOTIFTYPE, string rTCREWORKFLAG)
        {
            this._sERVERNAME = sERVERNAME;
            this._rOBOTNAME = rOBOTNAME;
            this._sTAGEID = sTAGEID;
            this._sTAGENAME = sTAGENAME;
            this._lINEID = lINEID;
            this._nODENO = nODENO;
            this._sTAGEIDBYNODE = sTAGEIDBYNODE;
            this._sTAGETYPE = sTAGETYPE;
            this._pRIORITY = pRIORITY;
            this._sTAGEREPORTTRXNAME = sTAGEREPORTTRXNAME;
            this._sTAGEJOBDATATRXNAME = sTAGEJOBDATATRXNAME;
            this._iSMULTISLOT = iSMULTISLOT;
            this._sLOTMAXCOUNT = sLOTMAXCOUNT;
            this._rECIPECHENCKFLAG = rECIPECHENCKFLAG;
            this._dUMMYCHECKFLAG = dUMMYCHECKFLAG;
            this._gETREADYFLAG = gETREADYFLAG;
            this._pUTREADYFLAG = pUTREADYFLAG;
            this._pREFETCHFLAG = pREFETCHFLAG;
            this._wAITFRONTFLAG = wAITFRONTFLAG;
            this._uPSTREAMPATHTRXNAME = uPSTREAMPATHTRXNAME;
            this._uPSTREAMJOBDATAPATHTRXNAME = uPSTREAMJOBDATAPATHTRXNAME;
            this._dOWNSTREAMPATHTRXNAME = dOWNSTREAMPATHTRXNAME;
            this._dOWNSTREAMJOBDATAPATHTRXNAME = dOWNSTREAMJOBDATAPATHTRXNAME;
            this._tRACKDATASEQLIST = tRACKDATASEQLIST;
            this._cASSETTETYPE = cASSETTETYPE;
            this._rEMARKS = rEMARKS;
            this._iSENABLED = iSENABLED;
            this._sLOTFETCHSEQ = sLOTFETCHSEQ;
            this._sLOTSTORESEQ = sLOTSTORESEQ;
            this._eXCHANGETYPE = eXCHANGETYPE;
            this._eQROBOTIFTYPE = eQROBOTIFTYPE;
            this._rTCREWORKFLAG = rTCREWORKFLAG;
            
            
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

        public virtual string STAGEID
        {
            get { return _sTAGEID; }
            set
            {
                _sTAGEID = value;
            }
        }

        public virtual string STAGENAME
        {
            get { return _sTAGENAME; }
            set
            {
                _sTAGENAME = value;
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

        public virtual string STAGEIDBYNODE
        {
            get { return _sTAGEIDBYNODE; }
            set
            {
                _sTAGEIDBYNODE = value;
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

        public virtual int PRIORITY
        {
            get { return _pRIORITY; }
            set { _pRIORITY = value; }
        }

        public virtual string STAGEREPORTTRXNAME
        {
            get { return _sTAGEREPORTTRXNAME; }
            set
            {
                _sTAGEREPORTTRXNAME = value;
            }
        }

        public virtual string STAGEJOBDATATRXNAME
        {
            get { return _sTAGEJOBDATATRXNAME; }
            set
            {
                _sTAGEJOBDATATRXNAME = value;
            }
        }

        public virtual string ISMULTISLOT
        {
            get { return _iSMULTISLOT; }
            set
            {
                _iSMULTISLOT = value;
            }
        }

        public virtual int SLOTMAXCOUNT
        {
            get { return _sLOTMAXCOUNT; }
            set { _sLOTMAXCOUNT = value; }
        }

        public virtual string RECIPECHENCKFLAG
        {
            get { return _rECIPECHENCKFLAG; }
            set
            {
                _rECIPECHENCKFLAG = value;
            }
        }

        public virtual string DUMMYCHECKFLAG
        {
            get { return _dUMMYCHECKFLAG; }
            set
            {
                _dUMMYCHECKFLAG = value;
            }
        }

        public virtual string GETREADYFLAG
        {
            get { return _gETREADYFLAG; }
            set
            {
                _gETREADYFLAG = value;
            }
        }

        public virtual string PUTREADYFLAG
        {
            get { return _pUTREADYFLAG; }
            set
            {
                _pUTREADYFLAG = value;
            }
        }

        public virtual string PREFETCHFLAG
        {
            get { return _pREFETCHFLAG; }
            set
            {
                _pREFETCHFLAG = value;
            }
        }

        public virtual string WAITFRONTFLAG
        {
            get { return _wAITFRONTFLAG; }
            set
            {
                _wAITFRONTFLAG = value;
            }
        }

        public virtual string UPSTREAMPATHTRXNAME
        {
            get { return _uPSTREAMPATHTRXNAME; }
            set
            {
                _uPSTREAMPATHTRXNAME = value;
            }
        }

        public virtual string UPSTREAMJOBDATAPATHTRXNAME
        {
            get { return _uPSTREAMJOBDATAPATHTRXNAME; }
            set
            {
                _uPSTREAMJOBDATAPATHTRXNAME = value;
            }
        }

        public virtual string DOWNSTREAMPATHTRXNAME
        {
            get { return _dOWNSTREAMPATHTRXNAME; }
            set
            {
                _dOWNSTREAMPATHTRXNAME = value;
            }
        }

        public virtual string DOWNSTREAMJOBDATAPATHTRXNAME
        {
            get { return _dOWNSTREAMJOBDATAPATHTRXNAME; }
            set
            {
                _dOWNSTREAMJOBDATAPATHTRXNAME = value;
            }
        }

        public virtual string TRACKDATASEQLIST
        {
            get { return _tRACKDATASEQLIST; }
            set
            {
                _tRACKDATASEQLIST = value;
            }
        }

        public virtual string CASSETTETYPE
        {
            get { return _cASSETTETYPE; }
            set
            {
                _cASSETTETYPE = value;
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

        public virtual string ISENABLED
        {
            get { return _iSENABLED; }
            set
            {
                _iSENABLED = value;
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

        public virtual string SLOTSTORESEQ
        {
            get { return _sLOTSTORESEQ; }
            set
            {
                _sLOTSTORESEQ = value;
            }
        }
        
        public virtual string EXCHANGETYPE
        {
            get { return _eXCHANGETYPE; }
            set
            {
                _eXCHANGETYPE = value;
            }
        }
        
        public virtual string EQROBOTIFTYPE
        {
            get { return _eQROBOTIFTYPE; }
            set
            {
                _eQROBOTIFTYPE = value;
            }
        }
        
        public virtual string RTCREWORKFLAG
        {
            get { return _rTCREWORKFLAG; }
            set
            {
                _rTCREWORKFLAG = value;
            }
        }
        


        #endregion
    }
    #endregion
}