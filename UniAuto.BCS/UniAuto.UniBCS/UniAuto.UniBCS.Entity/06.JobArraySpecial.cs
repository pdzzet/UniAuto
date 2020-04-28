using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace UniAuto.UniBCS.Entity
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]  
    public class JobArraySpecial:ICloneable
    {
        private string _glassFlowType = "0";
        private string _processType = "0";
        private string _rtcFlag = "0";
        private string _mainEQInFlag = "0";
        private string _recipeGroupNumber = "0";
        private string _sorterGrade = string.Empty;
        private string _sourcePortNo = "0";
        private string _targetPortNo = "0";
        //private string _turnAngle = "0";// qiumin add 20170829 For ATS400 
        private bool _isMMG = false;
        private string _exposuremaskid = string.Empty;  //Watson add 20141215 For Photo Line Mask
        // 判斷玻璃是否做過製程, 用在報給MES Process Reslut的依據
        private bool _photoIsProcessed = false;

        private string _dNS_SB_HP_NUM = "0";
        private string _dNS_SB_CP_NUM = "0";
        private string _dNS_HB_HP_NUM = "0";
        private string _dNS_VCD_NUM = "0";
        private string _dNS_MATERIALID = string.Empty;        //add by bruce 2015/12/29 DNS of material ID
        private string _dNS_MATERIAL_CONSUMEABLE ="0";   //add by bruce 2015/12/29 DNS of material consumeable 
        private string _dNS_LCCTCtPrsAve = "0";   //add by qiumin check glass coater skip or ok   20171227

        private string _targetSequenceNo = "0"; //t3 use cc.kuang 2015/07/02
        private string _targetLoadLockNo = "0"; //t3 use cc.kuang 2015/07/02
        private string _lastMainPPID = string.Empty; //t3 use for Insp glass file cc.kuang 2015/07/14
        private string _lastMainEqpName = string.Empty; //t3 use for Insp glass file cc.kuang 2015/07/14
        private string _lastMainChamberName = string.Empty; //t3 use for Insp glass file cc.kuang 2015/07/14
        private string _MQCInspectionFlag=string.Empty; //add by bruce 2015/07/27 ELA cross line use
        private string _ELA1By1Flag = string.Empty;  //add by qiumin 20171017 ELA one by one run
        private string _BackupProcessFlag = string.Empty;   // add by bruce 2015/07/27 ELA cross line use
        private DateTime  _jobProcessStartedTime = DateTime.Now; // add by qiumin 20171222 for Array check EQP process time 
        private string _currentRecipeID = string.Empty; //add by hujunpeng 20190416
        private string _ProcessDataNGFlag = "0";  //add by Yangzhenteng 20191107 PHL_EDGEEXP Process Data [SCAN04_EXPENERGY]=null        
        private string _FLRFirstGlassSendOutFlag = "0";  //add by Yangzhenteng 20191107 For FLR 优先出片
        public string CurrentRecipeID //add by hujunpeng 20190416
        {
            get { return _currentRecipeID;}
            set { _currentRecipeID=value;}
        }
        public DateTime JobProcessStartedTime   // add by qiumin 20171222 for Array check EQP process time 
        {
            get { return _jobProcessStartedTime; }
            set { _jobProcessStartedTime = value; }
        }
        public string GlassFlowType
        {
            get { return _glassFlowType; }
            set { _glassFlowType = value; }
        }

        public string ProcessType
        {
            get { return _processType; }
            set { _processType = value; }
        }

        public string RtcFlag
        {
            get { return _rtcFlag; }
            set { _rtcFlag = value; }
        }

        public string MainEQInFlag
        {
            get { return _mainEQInFlag; }
            set { _mainEQInFlag = value; }
        }

        public string RecipeGroupNumber
        {
            get { return _recipeGroupNumber; }
            set { _recipeGroupNumber = value; }
        }

        public string SorterGrade
        {
            get { return _sorterGrade; }
            set { _sorterGrade = value; }
        }

        public string SourcePortNo
        {
            get { return _sourcePortNo; }
            set { _sourcePortNo = value; }
        }

        public string TargetPortNo
        {
            get { return _targetPortNo; }
            set { _targetPortNo = value; }
        }

        public bool IsMMG
        {
            get { return _isMMG; }
            set { _isMMG = value; }
        }
        //Watson add 20141215 For Photo Line Mask
        public string ExposureMaskID
        {
            get { return _exposuremaskid; }
            set { _exposuremaskid = value; }
        }

        //cc.kuang add 20150702 For Changer Function use
        public string TargetSequenceNo
        {
            get { return _targetSequenceNo; }
            set { _targetSequenceNo = value; }
        }
        // qiumin add 20170829 For ATS400 
       /* public string TurnAngle   
        {
            get { return _turnAngle; }
            set { _turnAngle = value; }
        }*/

        //cc.kuang add 20150702 For PVD use
        public string TargetLoadLockNo
        {
            get { return _targetLoadLockNo; }
            set { _targetLoadLockNo = value; }
        }

        //cc.kuang add 20150714 For Insp glass file use
        public string LastMainPPID
        {
            get { return _lastMainPPID; }
            set { _lastMainPPID = value; }
        }

        //cc.kuang add 20150714 For Insp glass file use
        public string LastMainEqpName
        {
            get { return _lastMainEqpName; }
            set { _lastMainEqpName = value; }
        }

        //cc.kuang add 20150714 For Insp glass file use
        public string LastMainChamberName
        {
            get { return _lastMainChamberName; }
            set { _lastMainChamberName = value; }
        }

        //add by bruce 2015/07/27 ELA cross line use
        public string BackupProcessFlag
        {
            get { return _BackupProcessFlag; }
            set { _BackupProcessFlag = value; }
        }

        //add by bruce 2015/07/27 ELA cross line use
        public string MQCInspectionFlag
        {
            get { return _MQCInspectionFlag ; }
            set { _MQCInspectionFlag = value; }
        }
        //add by qiumin 20171017 ELA one by one run
        public string ELA1BY1Flag
        {
            get { return _ELA1By1Flag; }
            set { _ELA1By1Flag = value; }
        }

        //add by Yangzhenteng 20191107 PHL
        public string ProcessDataNGFlag
        {
            get { return _ProcessDataNGFlag; }
            set { _ProcessDataNGFlag = value; }
        }

        //add by Yangzhenteng 20191107 FLR
        public string FLRFirstGlassSendOutFlag
        {
            get { return _FLRFirstGlassSendOutFlag; }
            set { _FLRFirstGlassSendOutFlag = value; }
        }

        public bool PhotoIsProcessed
        {
            get { return _photoIsProcessed; }
            set { _photoIsProcessed = value; }
        }

        #region for DNS Process Data
        /// <summary>
        /// SBUseHPNum
        /// </summary>

        public string DNS_LCCTCtPrsAve
        {
            get { return _dNS_LCCTCtPrsAve; }
            set { _dNS_LCCTCtPrsAve= value; }
 
        }

        public string DNS_SB_HP_NUM
        {
            get { return _dNS_SB_HP_NUM; }
            set { _dNS_SB_HP_NUM = value; }
        }
        /// <summary>
        /// SBUseCPNum
        /// </summary>
        public string DNS_SB_CP_NUM
        {
            get { return _dNS_SB_CP_NUM; }
            set { _dNS_SB_CP_NUM = value; }
        }
        /// <summary>
        /// HBUseHPNum
        /// </summary>
        public string DNS_HB_HP_NUM
        {
            get { return _dNS_HB_HP_NUM; }
            set { _dNS_HB_HP_NUM = value; }
        }
        /// <summary>
        /// LCDRprcCh
        /// </summary>
        public string DNS_VCD_NUM
        {
            get { return _dNS_VCD_NUM; }
            set { _dNS_VCD_NUM = value; }
        }

        /// <summary>
        /// LCRegBcd
        /// </summary>
        public string DNS_MATERIALID
        {
            get { return _dNS_MATERIALID; }
            set { _dNS_MATERIALID = value; }
        }

        /// <summary>
        /// LCCTDpQt
        /// </summary>
        public string DNS_MATERIAL_CONSUMEABLE
        {
            get { return _dNS_MATERIAL_CONSUMEABLE; }
            set { _dNS_MATERIAL_CONSUMEABLE = value; }
        }

        #endregion
        public object Clone()
        {
            JobArraySpecial arr = (JobArraySpecial)this.MemberwiseClone();
            return arr;
        }
    }
}
