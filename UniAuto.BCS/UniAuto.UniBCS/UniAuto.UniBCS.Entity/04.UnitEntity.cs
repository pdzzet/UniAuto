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
	public class UnitEntityFile : EntityFile
	{
        private eEQPStatus _status = eEQPStatus.NOUNIT;

        private string _currentAlarmCode = string.Empty;
        private int _tftJobCount = 0 ;
        private int _cfJobCount = 0 ;
        private int _productType = 0;
        private string _mesStatus = "DOWN";
        private string _runMode = string.Empty;
        private string _chamberRunMode = string.Empty;
        private string _oldChamberRunMode = string.Empty;
        private string _ptiUnitID = string.Empty;
        private int _unassembledTFTCount = 0; //t3 cell add for PI
        private int _ITODummyCount = 0; //t3 cell add for PI
        private int _NIPDummyCount = 0; //t3 cell add for PI
        private int _metalOneDummyCount = 0; //t3 cell add for PI
        private int _DUMMYCount = 0;
        private int _thicknessDummyCount = 0;
        private int _throughDummyCount = 0;
        private int _UVMASKCount = 0;
        private string _currentRecipeID = string.Empty ;    //add by bruce 20160217 Array 附屬設備使用

        public int DummyJobCount
        {
            get { return _DUMMYCount; }
            set { _DUMMYCount = value; }
        }
        public int ThicknessDummyJobCount
        {
            get { return _thicknessDummyCount; }
            set { _thicknessDummyCount = value; }
        }
        public int ThroughDummyJobCount
        {
            get { return _throughDummyCount; }
            set { _throughDummyCount = value; }
        }
        public int UVMASKJobCount
        {
            get { return _UVMASKCount; }
            set { _UVMASKCount = value; }
        }

        public int UnassembledTFTJobCount //t3 cell add for PI
        {
            get { return _unassembledTFTCount; }
            set { _unassembledTFTCount = value; }
        }
        public int ITODummyJobCount //t3 cell add for PI
        {
            get { return _ITODummyCount; }
            set { _ITODummyCount = value; }
        }
        public int NIPDummyJobCount //t3 cell add for PI
        {
            get { return _NIPDummyCount; }
            set { _NIPDummyCount = value; }
        }
        public int MetalOneDummyJobCount //t3 cell add for PI
        {
            get { return _metalOneDummyCount; }
            set { _metalOneDummyCount = value; }
        }

        public string RunMode
        {
            get { return _runMode; }
            set { _runMode = value; }
        }
        public string ChamberRunMode
        {
            get { return _chamberRunMode; }
            set { _chamberRunMode = value;}
        }
        public string OldChamberRunMode
        {
            get { return _oldChamberRunMode; }
            set { _oldChamberRunMode = value; }
        }

        public eEQPStatus Status
		{
			get {  return _status;  }
			set { _status = value;  }
		}

        public string CurrentAlarmCode
        {
            get { return _currentAlarmCode; }
            set { _currentAlarmCode = value; }
        }

        public int  TFTProductCount
        {
            get { return _tftJobCount; }
            set { _tftJobCount = value; }
        }
        public int CFProductCount
        {
            get { return _cfJobCount; }
            set { _cfJobCount = value; }
        }

        public int ProductType
        {
            get { return _productType; }
            set { _productType = value; }
        }
        public string MESStatus
        {
            get { return _mesStatus; }
            set { _mesStatus = value; }
        }

        #region Array Special
        public string CurrentRecipeID   //add by bruce 20160217 Array 附屬設備使用
        {
            get { return _currentRecipeID ; }
            set { _currentRecipeID = value; }
        }
        #endregion

        #region [SECS]
        private eEQPStatus _preStatus = eEQPStatus.SETUP;
        private string _preMesStatus = "DOWN";
        //For SECS Unit
        private string _hSMSControlMode = "OFF-LINE";

        public string HSMSControlMode
        {
            get { return _hSMSControlMode; }
            set { _hSMSControlMode = value; }
        }

        public eEQPStatus PreStatus
        {
            get { return _preStatus; }
            set { _preStatus = value; }
        }
        public string PreMesStatus
        {
            get { return _preMesStatus; }
            set { _preMesStatus = value; }
        }
        #endregion
	}

    public class Unit : Entity
	{
		public UnitEntityData Data { get; private set; }

		public UnitEntityFile File { get; private set; }

        #region[SECS Special]
        private bool _hsmsConnected = false;
        private bool _secsCommunicated = false;
        private bool _hsmsSelected = false;
        private string _mDLN = string.Empty;
        private string _sOFTREV = string.Empty;
        private string _hsmsConnStatus = "DISCONNECTED";

        public string HsmsConnStatus
        {
            get { return _hsmsConnStatus; }
            set { _hsmsConnStatus = value; }
        }

        public string SOFTREV
        {
            get { return _sOFTREV; }
            set { _sOFTREV = value; }
        }

        public string MDLN
        {
            get { return _mDLN; }
            set { _mDLN = value; }
        }

        public bool HsmsSelected
        {
            get { return _hsmsSelected; }
            set { _hsmsSelected = value; }
        }

        public bool SecsCommunicated
        {
            get { return _secsCommunicated; }
            set { _secsCommunicated = value; }
        }

        public bool HsmsConnected
        {
            get { return _hsmsConnected; }
            set { _hsmsConnected = value; }
        }
        #endregion

		public Unit(UnitEntityData data, UnitEntityFile file)
		{
			Data = data;
			File = file;
		}
	}
}
