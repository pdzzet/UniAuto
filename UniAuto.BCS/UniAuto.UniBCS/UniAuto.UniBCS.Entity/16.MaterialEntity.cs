using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    /// <summary>
    /// 對應File, 修改Property後呼叫Save(), 會序列化存檔
    /// </summary>
    [Serializable]
    public class MaterialEntity : EntityFile  
    {
        private eMaterialEQtype _eqType = eMaterialEQtype.Normal;
        private string _nodeNo = string.Empty; //管理用
        private string _unitNo = string.Empty;
	    private string _operatorID = string.Empty;
        private eMaterialStatus _materialStatus = eMaterialStatus.NONE;
        private eMaterialMode _materialMode = eMaterialMode.NONE;   //Add by marine for MES 2015/7/13
	    private string _materialSlotNo = string.Empty;
	    private string _materialID = string.Empty;
        private string _materialPort = string.Empty;
        private string _materialvalue;
        private string _headID = string.Empty;
        private string _tankNo = string.Empty;
        private string _materialAliveTime = "0";
        private string _useCount = string.Empty;
        private string _groupId = string.Empty;
        private string _materialType = string.Empty;
        private string _partNo = string.Empty;
        private string _materialCompSts = string.Empty;   //Watson Add 20141021 For OPI
        private string _materialPosition = string.Empty;     //Watson Add 20141021 For OPI
        private string _materialWeight = string.Empty;       //Watson Add 20141021 For OPI
        private string _materialCartridgeID = string.Empty; //Chia-Chi Add 20141024 
        private string _materialRecipeID = string.Empty;  //Edison Add 20141027
        private string _materialCount = string.Empty;   //Add by marine for MES 2015/7/9
        private string _materialAbnormalCode = string.Empty;    //Add by marine for MES 2015/7/9
        private string _usedTime = string.Empty;    //Add by marine for MES 2015/7/9
        private string _materialstate = string.Empty;   //Add by marine for MES 2015/7/9
        private string _valiresult = string.Empty;  //Add by marine for MES 2015/7/9
        private string _materialwarningtime = string.Empty;  //Add by marine for MES 2015/7/9
        private string _meterialbatchsame = string.Empty;   //Add by marine for MES 2018/2/27
        private string _warmcount = string.Empty;//Add by hujunpeng 2018/5/8
        private string _cellvalidateresult = string.Empty; //Watson add 20141231 For Cell UVA
        private List<string> _cellPOLMaterial = new List<string>();  //Jun Add 20150115 For Cell POLMaterial
        private string _polMaterialType = "0";
        private string _UVMaskUseCount = string.Empty;  //shihyang Add 20150909 For T3 Cell PIL Material
        private string _lotID = string.Empty; //shihyang Add 20150909 For T3 Cell POL Material
        private string _lotNo = string.Empty;   //shihyang Add 20150909 For T3 Cell POL Material
        private string _inUseTime = string.Empty;    //shihyang Add 20150909 For T3 Cell POL Material
        private string _materialSpecName = string.Empty;    //sy Add 20160601 For MES SPEC 1.43
        private List<MaterialEntity> _cellPAMMateril = new List<MaterialEntity>(); //20151112 cy add for cell PAM material
        private string _site = string.Empty;//add by hujunpeng 20190223


        public string Site//add by hujunpeng 20190223
        {
            get { return _site; }
            set { _site=value;}
        }
        //20151112 cy add for cell PAM material
        public List<MaterialEntity> CellPAMMateril
        {
              get { return _cellPAMMateril; }
              set { _cellPAMMateril = value; }
        }

        public string MaterialRecipeID
        {
            get { return _materialRecipeID; }
            set { _materialRecipeID = value; }
        }

        public string PartNo
        {
            get { return _partNo; }
            set { _partNo = value; }
        }

        public string MaterialType
        {
            get { return _materialType; }
            set
            {
                _materialType = value;
                if (_eqType == eMaterialEQtype.MaskEQ)
                    _filename = string.Format("{0}_{1}_{2}.xml", _nodeNo, _materialSlotNo, _materialType);
                else
                    _filename = string.Format("{0}_{1}_{2}_{3}.xml", _nodeNo, _unitNo, _materialPort, _materialID);
            }
        }

        public string GroupId
        {
            get { return _groupId; }
            set { _groupId = value; }
        }

        public string UseCount
        {
            get { return _useCount; }
            set { _useCount = value; }
        }


        public string MaterialAliveTime
        {
            get { return _materialAliveTime; }
            set { _materialAliveTime = value; }
        }
        public string TankNo
        {
            get { return _tankNo; }
            set { _tankNo = value; }
        }

        public string NodeNo
        {
            get {  return _nodeNo;  }
            set
            {
                _nodeNo = value;
                if (_eqType == eMaterialEQtype.MaskEQ)
                    _filename = string.Format("{0}_{1}_{2}.xml", _nodeNo, _materialSlotNo, _materialType);
                else
                    _filename = string.Format("{0}_{1}_{2}_{3}.xml", _nodeNo, _unitNo, _materialPort, _materialID);
            }
        }
        public string UnitNo
        {
            get { return _unitNo; }
            set
            {
                _unitNo = value;
                if (_eqType == eMaterialEQtype.MaskEQ)
                    _filename = string.Format("{0}_{1}_{2}.xml", _nodeNo, _materialSlotNo, _materialType);
                else
                    _filename = string.Format("{0}_{1}_{2}_{3}.xml", _nodeNo, _unitNo, _materialPort, _materialID);
            }
        }
        public string OperatorID
        {
            get { return _operatorID;  }
            set { _operatorID = value; }
        }
        public eMaterialStatus MaterialStatus
        {
            get { return _materialStatus; }
            set { _materialStatus = value; }
        }
        //Add by marine for MES 2015/7/13
        public eMaterialMode eMaterialMode
        {
            get { return _materialMode;}
            set { _materialMode = value;}
        }
        public string MaterialSlotNo
        {
            get { return _materialSlotNo; }
            set { _materialSlotNo = value; }
        }
        public string MaterialID
        {
            get { return _materialID; }
            set
            {
                _materialID = value;
                if (_eqType == eMaterialEQtype.MaskEQ)
                    _filename = string.Format("{0}_{1}_{2}.xml", _nodeNo, _materialSlotNo, _materialType);
                else
                    _filename = string.Format("{0}_{1}_{2}_{3}.xml", _nodeNo, _unitNo, _materialPort, _materialID);
            }
        }
        public string MaterialPort
        {
            get { return _materialPort; }
            set
            {
                _materialPort = value;
                if (_eqType == eMaterialEQtype.MaskEQ)
                    _filename = string.Format("{0}_{1}_{2}.xml", _nodeNo, _materialSlotNo, _materialType);
                else
                    _filename = string.Format("{0}_{1}_{2}_{3}.xml", _nodeNo, _unitNo, _materialPort, _materialID);
            }
        }

        public string MaterialValue
        {
            get { return _materialvalue; }
            set { _materialvalue = value; }
        }
        public string HEADID
        {
            get { return _headID; }
            set { _headID = value; }
        }
        public eMaterialEQtype EQType
        {
            get { return _eqType; }
            set { _eqType = value; }
        }
        //Watson Add 20141021 For OPI
        public string MaterialCompleteStatus
        {
            get {return _materialCompSts;}
            set {_materialCompSts = value;}
        }
        //Watson Add 20141021 For OPI
         public string MaterialPosition
        {
            get {return  _materialPosition;}
            set { _materialPosition = value; }
        }
         //Watson Add 20141021 For OPI
         public string MaterialWeight
        {
            get { return _materialWeight; }
            set { _materialWeight = value; }
        }
        //Add by marine for MES 2015/7/9
         public string MaterialCount 
         {
             get { return _materialCount; }
             set { _materialCount=value;}
         }
        //Add by marine for MES 2015/7/9
         public string MaterialAbnormalCode 
         {
             get { return _materialAbnormalCode; }
             set { _materialAbnormalCode = value; }
         }
        //Add by marine for MES 2015/7/9
         public string UsedTime 
         {
             get { return _usedTime; }
             set { _usedTime = value;}
         }
        //Add by marine for MES 2015//7/9
         public string MaterialState
         {
             get { return _materialstate; }
             set { _materialstate = value; }
         }
        //Add by marine for MES 2015/7/9
         public string ValiResult
         {
             get { return _valiresult; }
             set { _valiresult = value; }
         }
         //Add by marine for MES 2015/7/9
         public string MaterialWarningTime 
         {
             get { return _materialwarningtime; }
             set { _materialwarningtime=value;}
         }
         //Add by marine for MES 2018/2/27
         public string MaterialBatchSame
         {
             get { return _meterialbatchsame; }
             set { _meterialbatchsame = value; }
         }
         //Add by hujunpeng 2018/5/8
         public string WarmCount
         {
             get { return _warmcount; }
             set { _warmcount = value; }
         }
        public string MaterialCartridgeID
        {
            get { return _materialCartridgeID; }
            set { _materialCartridgeID = value; }
        }

        public string CellValidateResult
        {
            get { return _cellvalidateresult; }
            set { _cellvalidateresult = value; }
        }

        public List<string> CellPOLMaterial
        {
            get { return _cellPOLMaterial; }
            set { _cellPOLMaterial = value; }
        }

        public string PolMaterialType
        {
            get { return _polMaterialType; }
            set { _polMaterialType = value; }
        }

        public string UVMaskUseCount
        {
            get { return _UVMaskUseCount; }
            set { _UVMaskUseCount = value; }
        }

        public string LotID
        {
            get { return _lotID; }
            set { _lotID = value; }
        }

        public string LotNo
        {
            get { return _lotNo; }
            set { _lotNo = value; }
        }

        public string InUseTime
        {
            get { return _inUseTime; }
            set { _inUseTime = value; }
        }

        public string MaterialSpecName
        {
            get { return _materialSpecName; }
            set { _materialSpecName = value; }
        }
    }
}
