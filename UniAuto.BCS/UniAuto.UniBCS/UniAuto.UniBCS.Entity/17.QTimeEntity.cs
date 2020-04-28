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
    public class QtimeEntityFile : EntityFile
    {
        private string _jobno= string.Empty;
        private string _jobid= string.Empty;  //需要嗎？
        private DateTime _updateTime;

        public string JobNo  //Cassette Sequence No + Job Sequence No
        {
            get { return _jobno; }
            set { _jobno = value; }
        }
        public string JobID //目前沒有用途
        {
            get { return _jobid; }
            set { _jobid = value; }
        }
        public DateTime updateTime
        {
            get { return _updateTime; }
            set { _updateTime = value; }
        }
    }

    public class Qtime : Entity
    {
        public QtimeEntityData Data { get; private set; }

        public QtimeEntityFile File { get; private set; }

        public Qtime(QtimeEntityData data, QtimeEntityFile file)
        {
            Data = data;
            File = file;
        }
    }
}
