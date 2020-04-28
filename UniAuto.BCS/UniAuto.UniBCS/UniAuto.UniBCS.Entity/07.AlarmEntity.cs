using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniAuto.UniBCS.Entity
{
    public class eALARM_STATE
    {
        public const string CLEAR = "CLEAR";

        public const string SET = "SET";
    }

    //[Serializable]
    //public class AlarmData : EntityData
    //{
    //    private string _lineID = string.Empty;
    //    private string _nodeNo = string.Empty;
    //    private string _unitNo = "0";
    //    private string _alarmLevel = string.Empty;
    //    private string _alarmID = string.Empty;
    //    private string _alarmCode = string.Empty;
    //    private string _alarmText = string.Empty;
    //    private string _serverName = string.Empty;

    //    public string LINEID
    //    {
    //        get { return _lineID; }
    //        set { _lineID = value; }
    //    }

    //    public string NODENO
    //    {
    //        get { return _nodeNo; }
    //        set { _nodeNo = value; }
    //    }

    //    public string UNITNO
    //    {
    //        get { return _unitNo; } 
    //        set { _unitNo = value; } 
    //    }

    //    public string ALARMLEVEL
    //    {
    //        get { return _alarmLevel; } 
    //        set { _alarmLevel = value; } 
    //    }

    //    public string ALARMID
    //    {
    //        get { return _alarmID; } 
    //        set { _alarmID = value; } 
    //    }

    //    public string ALARMCODE
    //    {
    //        get { return _alarmCode; } 
    //        set { _alarmCode = value; } 
    //    }

    //    public string ALARMTEXT
    //    {
    //        get { return _alarmText; } 
    //        set { _alarmText = value; } 
    //    }

    //    public string SERVERNAME
    //    {
    //        get { return _serverName; } 
    //        set { _serverName = value; } 
    //    }

    //}
    /// <summary>
    /// 正在發生的Alarm資料
    /// </summary>
    [Serializable]
    public class HappeningAlarm
    {
        private AlarmEntityData _alarm = null;
        private DateTime _occurDateTime = DateTime.Now;

        public HappeningAlarm(AlarmEntityData alarm, DateTime occurTime)
        {
            _alarm = alarm;
            _occurDateTime = occurTime;
        }

        public AlarmEntityData Alarm
        {
            get { return _alarm; }
            set { _alarm = value; }
        }
        public DateTime OccurDateTime
        {
            get { return _occurDateTime; }
            set { _occurDateTime = value; }
        }
    }
    /// <summary>
    /// 機台正在發生的Alarm記錄
    /// </summary>
    [Serializable]
    public class AlarmHistoryFile : EntityFile
    {
        private string _eqpNo = string.Empty;
        private string _eqpID = string.Empty;

        public string EQPNo
        {
            get { return _eqpNo; }
            set { _eqpNo = value; }
        }

        public string EQPID
        {
            get { return _eqpID; }
            set 
            {
                _eqpID = value;
                SetFilename(_eqpID + ".bin");
            }
        }

        public AlarmHistoryFile() { }

        public AlarmHistoryFile(string eqpNo, string eqpID)
        {
            EQPNo = eqpNo;
            EQPID = eqpID;
        }
        public Dictionary<string, HappeningAlarm> HappingAlarms = new Dictionary<string, HappeningAlarm>();
    }
}
