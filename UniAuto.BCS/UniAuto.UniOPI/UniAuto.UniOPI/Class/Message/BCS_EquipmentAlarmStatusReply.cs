using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniOPI
{
    public class BCS_EquipmentAlarmStatusReply
    {
        public bool IsReply { get; set; }  //判斷EquipmentDataLinkStatusReply是否已回復

        public List<Alarm> Lst_RealAlarm { get; set; }

        public DateTime LastRequestDate { get; set; } //最後發送request的時間   

        public BCS_EquipmentAlarmStatusReply()
        {
            IsReply = true;

            Lst_RealAlarm = new List<Alarm>();

            LastRequestDate = Convert.ToDateTime("2010-01-01 00:00:00");
        }
    }

    public class Alarm
    {
        private string alarmID = string.Empty;
        private string alarmText = string.Empty;
        private string alarmLevel = string.Empty;
        private string alarmUnit = string.Empty;
        private string alarmCode = string.Empty;

        public string AlarmID
        {
            get { return alarmID; }
            set { alarmID = value; }
        }
        public string AlarmText
        {
            get { return alarmText; }
            set { alarmText = value; }
        }
        public string AlarmLevel
        {
            get { return alarmLevel; }
            set { alarmLevel = value; }
        }
        public string AlarmUnit
        {
            get { return alarmUnit; }
            set { alarmUnit = value; }
        }

        public string AlarmCode
        {
            get { return alarmCode; }
            set { alarmCode = value; }
        }
    }
}
