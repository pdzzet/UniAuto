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
    public class APCDataDownloadEntityFile : EntityFile
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

    public class APCDataDownload : Entity
    {
        public APCDataDownloadEntityData Data { get; private set; }

        private string _value = "";

        public string Value
        {
            get { return _value; }
            set { _value = value; }

        }

        //public APCDataDownloadEntityFile File { get; private set; }

        //public APCDataDownload(APCDataDownloadEntityData data, APCDataDownloadEntityFile file)
        public APCDataDownload(APCDataDownloadEntityData data)
        {
            Data = data;
            //File = file;
        }
    }
}
