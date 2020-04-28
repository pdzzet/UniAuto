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
    public class APCDataReportEntityFile : EntityFile
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

    public class APCDataReport : Entity
    {
        public APCDataReportEntityData Data { get; private set; }

        private string _value = "";

        public string Value
        {
            get { return _value; }
            set { _value = value; }

        }

        //public APCDataReportEntityFile File { get; private set; }

        //public APCDataReport(APCDataReportEntityData data, APCDataReportEntityFile file)
        public APCDataReport(APCDataReportEntityData data)
        {
            Data = data;
            //File = file;
        }
    }
}
