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
    public class DailyCheckEntityFile : EntityFile
    {
        //private DateTime _occurDateTime = DateTime.Now;

        //private string _value;

        //public string Value
        //{
        //    get { return _value; }
        //    set { _value = value; }
        //}

        //public DateTime OccurDateTime
        //{
        //    get { return _occurDateTime; }
        //    set { _occurDateTime = value; }
        //}
    }

    public class DailyCheckData : Entity
    {
        public DailyCheckEntityData Data { get; private set; }

        private string _value = "";

        public string Value
        {
            get { return _value; }
            set { _value = value; }

        }

        //public DailyCheckEntityFile File { get; private set; }

        //public DailyCheckData(DailyCheckEntityData data, DailyCheckEntityFile file)
        public DailyCheckData(DailyCheckEntityData data)
        {
            Data = data;
            //File = file;
        }
    }
}
