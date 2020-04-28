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
    public class PositionEntityFile : EntityFile{ }

    public class PositionData : Entity
    {
        public PositionEntityData Data { get; private set; }
        
        private bool _positionForAPCFlag = false; //表示是否計算過

        public bool PositionForAPCFlag
        {
            get { return _positionForAPCFlag; }
            set { _positionForAPCFlag = value; }
        }

        private string _value = "";

        public string Value
        {
            get { return _value; }
            set { _value = value; }

        }

        public PositionData(PositionEntityData data)
        {
            Data = data;
        }
    }
}
