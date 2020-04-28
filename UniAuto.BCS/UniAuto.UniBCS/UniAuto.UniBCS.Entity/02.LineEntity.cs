using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
	/// <summary>
	/// 對應File, 修改Property後呼叫Save(), 會序列化存檔
	/// </summary>
	[Serializable]
	public class LineEntityFile : EntityFile
	{
        //private eEQPStatus _status = eEQPStatus.STOP;
        private string _status = "DOWN";
        private eHostMode _hostMode = eHostMode.OFFLINE;
        private eHostMode _preHostMode = eHostMode.OFFLINE;
        private eHostMode _opictlmode = eHostMode.OFFLINE;
        private eINDEXER_OPERATION_MODE _indexOperMode = eINDEXER_OPERATION_MODE.SAMPLING_MODE;
        private eShortCutMode _cfShortCutMode = eShortCutMode.Disable;
        private eUPKEquipmentRunMode _upkEquipmentRunMode = eUPKEquipmentRunMode.CF;
        private ePLAN_STATUS _planStatus = ePLAN_STATUS.NO_PLAN;

        private string _operatorID = string.Empty;
        private string _plcStatus = "Disconnected";
        private bool _onBoradCardAlive = true;

      
        private string _hsmsStatus = string.Empty;
        private string _lineOperMode = eMES_LINEOPERMODE.NORMAL;
        private DateTime _dailyCheckLastDT = DateTime.Now;
        private DateTime _lastCuttingStartEventTime = DateTime.Now;//20161209 sy add for BC shutdown check
        private double _dailyCheckIntervalS;
        private bool _array_Material_Change = false;
        private int _coolRunSetCount = 1000;
        private int _coolRunRemainCount = 1000;
        private string _cellLineOperMode = "0";
        private string _currentPlanID = string.Empty;
        private string _standByPlanID = string.Empty;
        private string _mesConnectState = "Disconnect";//Tibrv 连线状态
        private string _RobotFetchSeqMode = "1";
        private string _LineRecipeName = "";
        private int _FetchCompensationTime = 0;
        private int _SendOverTimeAlarm = 0;
        private int _SendOverTimeWarring = 0;
        private bool _dummyRequestFlag = false; // 20160718 add by Frank for Dummy Request Thread
        private string _panelInformationReplyLastPPID = string.Empty;//20171120 by huangjiayin
        private DateTime _panelInformationReplyLastTime = DateTime.Now;
        private bool _panelInformationReplyLastRecipeIDCheckResult = false;


        public string MesConnectState
        {
            get {
                if (string.IsNullOrEmpty(_mesConnectState))
                {
                    _mesConnectState = "Disconnect";
                }
                return _mesConnectState; }
            set { _mesConnectState = value; }
        }

        private Dictionary<string, clsGroupIndex> _cellGroupIndex = new Dictionary<string, clsGroupIndex>();
        private Dictionary<string, clsDispatchRule> _uddispatchrule = new Dictionary<string, clsDispatchRule>();
        private Dictionary<string, bool> _cgmoFlagCheck = new Dictionary<string, bool>();
        private Dictionary<string, string> _planVCR = new Dictionary<string, string>();//add by hujunpeng 20190522 for auto change DCR status

        /// <summary>
        /// Array TBWET, TBSTR 
        /// 當機台正在換酸, 換鹼時, 要把此Flag Turn True
        /// </summary>

        public Dictionary<string, string> PlanVCR
        {
            get { return _planVCR; }
            set { _planVCR = value; }
        }
        public bool Array_Material_Change
        {
            get { return _array_Material_Change; }
            set { _array_Material_Change = value; }
        }

        //public eEQPStatus Status
        public string Status
		{
			get { return _status;  }
			set {  _status = value;  }
		}

        public eHostMode OPICtlMode
        {
            get { return _opictlmode; }
            set { _opictlmode = value; }
        }

        public eHostMode HostMode
        {
            get { return _hostMode; }
            set { _hostMode = value; }
        }

        public eHostMode PreHostMode
        {
            get { return _preHostMode; }
            set { _preHostMode = value; }
        }

        public eINDEXER_OPERATION_MODE IndexOperMode
        { 
            get { return _indexOperMode; }
            set { _indexOperMode = value; }
        }

        public eShortCutMode CFShortCutMode
        {
            get { return _cfShortCutMode; }
            set { _cfShortCutMode = value; }
        }

        public eUPKEquipmentRunMode UPKEquipmentRunMode
        {
            get { return _upkEquipmentRunMode; }
            set { _upkEquipmentRunMode = value; }
        }

        public ePLAN_STATUS PlanStatus
        {
            get { return _planStatus; }
            set { _planStatus = value; }
        }

        public string OperatorID
        {
            get { return _operatorID; }
            set { _operatorID = value; }
        }

        public string PLCStatus
        {
            get { return _plcStatus; }
            set { _plcStatus = value; }
        }

        public string HSMSStatus
        {
            get { return _hsmsStatus; }
            set { _hsmsStatus = value; }
        }

        public string LineOperMode
        {
            get { return _lineOperMode; }
            set { _lineOperMode = value; }
        }

        public string CellLineOperMode
        {
            get { return _cellLineOperMode; }
            set { _cellLineOperMode = value; }
        }

        public string CurrentPlanID
        {
            get { return _currentPlanID; }
            set { _currentPlanID = value; }
        }

        public string StandByPlanID
        {
            get { return _standByPlanID; }
            set { _standByPlanID = value; }
        }

        public Dictionary<string, clsGroupIndex> CellGroupIndex
        {
            get { return _cellGroupIndex; }
            set { _cellGroupIndex = value; }
        }

        public DateTime DailyCheckLastDT
        {
            get { return _dailyCheckLastDT; }
            set { _dailyCheckLastDT = value; }
        }

        public DateTime LastCuttingStartEventTime
        {
            get { return _lastCuttingStartEventTime; }
            set { _lastCuttingStartEventTime = value; }
        }
        /// <summary>
        /// Daily Check处理间隔时间
        /// </summary>
        public double DailyCheckIntervalS
        {
            get { return _dailyCheckIntervalS; }
            set { _dailyCheckIntervalS = value; }
        }

        public int CoolRunSetCount
        {
            get { return _coolRunSetCount; }
            set { _coolRunSetCount = value; }
        }

        public int CoolRunRemainCount
        {
            get { return _coolRunRemainCount; }
            set { _coolRunRemainCount = value; }
        }

        public Dictionary<string, clsDispatchRule> UnlaoderDispatchRule
        {
            get { return _uddispatchrule; }
            set { _uddispatchrule = value; }
        }

        public Dictionary<string, bool> CGMOFlagCheck
        {
            get { return _cgmoFlagCheck; }
            set { _cgmoFlagCheck = value; }
        }

        //20141223 add for Keep Line Use Port List<PortID,PortID> 
        private List<string> _lineUsePortList = new List<string>();

        public List<string> LineUsePortList
        {
            get { return _lineUsePortList; }
            set { _lineUsePortList = value; }
        }

        public bool OnBoradCardAlive
        {
            get { return _onBoradCardAlive; }
            set { _onBoradCardAlive = value; }
        }
        public string RobotFetchSeqMode //add by bruce 2015/7/17 OPI add new Transation Item
        {
            get { return _RobotFetchSeqMode; }
            set { _RobotFetchSeqMode = value; }
        }

        public string LineRecipeName //add by cc.kuang 2016/01/26, use in PHL line material mount/status/..
        {
            get { return _LineRecipeName; }
            set { _LineRecipeName = value; }
        }

        public int FetchCompensationTime //add by cc.kuang 2016/02/03, use in ELA Fetch Glass from CST..
        {
            get { return _FetchCompensationTime; }
            set { _FetchCompensationTime = value; }
        }

        public int SendOverTimeAlarm //add by cc.kuang 2016/02/03, use in ELA Store Glass to CST if Alarm..
        {
            get { return _SendOverTimeAlarm; }
            set { _SendOverTimeAlarm = value; }
        }

        public int SendOverTimeWarring //add by cc.kuang 2016/02/03, use in ELA Send CIM Msg..
        {
            get { return _SendOverTimeWarring; }
            set { _SendOverTimeWarring = value; }
        }

        public bool DummyRequestFlag    // 20160718 add by Frank for Dummy Request Thread
        {
            get { return _dummyRequestFlag; }
            set { _dummyRequestFlag = value; }
        }

        // 20171120 by huangjiayin
        public string PanelInformationReplyLastPPID
        {
            get { return _panelInformationReplyLastPPID; }
            set { _panelInformationReplyLastPPID = value; }
        }

        public DateTime PanelInformationReplyLastTime
        {
            get { return _panelInformationReplyLastTime; }
            set { _panelInformationReplyLastTime = value; } 
        }

        public bool PanelInformationReplyLastRecipeIDCheckResult
        {
            get { return _panelInformationReplyLastRecipeIDCheckResult; }
            set { _panelInformationReplyLastRecipeIDCheckResult = value; } 
        }

	}

	public class Line : Entity
	{

		public LineEntityData Data { get; private set; }

		public LineEntityFile File { get; private set; }

        public Line(LineEntityData data, LineEntityFile file)
        {
            Data = data;
            File = file;
        }
	}
}
