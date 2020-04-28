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
    public class ProcessDataEntityFile : EntityFile
    {
       
    }

    public class ProcessData : Entity
    {
        public ProcessDataEntityData Data { get; private set; }

        private string _value = "";

        public string Value
        {
            get { return _value; }
            set { _value = value; }
        }
       // public ProcessDataEntityFile File { get; private set; }

        public ProcessData(ProcessDataEntityData data)
        {
            Data = data;
        }
    }


}
