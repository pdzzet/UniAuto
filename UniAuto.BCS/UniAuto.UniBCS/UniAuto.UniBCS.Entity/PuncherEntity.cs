using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace UniAuto.UniBCS.Entity
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]  
    public class Puncher:Entity
    {
        private string _puncherName;
        private string _equipmentID;
        private DateTime _createTime = DateTime.Now;

        public DateTime CreateTime {
            get { return _createTime; }
            set { _createTime = value; }
        }

        public string EquipmentID {
            get { return _equipmentID; }
            set { _equipmentID = value; }
        }
        private int _currentCount;
        private int _maxCount;

        public int MaxCount {
            get { return _maxCount; }
            set { _maxCount = value; }
        }

        public int CurrentCount {
            get { return _currentCount; }
            set { _currentCount = value; }
        }

        public string PuncherName {
            get { return _puncherName; }
            set { _puncherName = value; }
        }
    }
}
