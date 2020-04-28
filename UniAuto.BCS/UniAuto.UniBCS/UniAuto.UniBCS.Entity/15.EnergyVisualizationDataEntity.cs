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
    public class EnergyVisualizationEntityFile : EntityFile
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

    public class EnergyVisualizationData : Entity
    {
        public EnergyVisualizationEntityData Data { get; private set; }

        private string _value = "";

        public string Value
        {
            get { return _value; }
            set { _value = value; }

        }

        //public EnergyVisualizationEntityFile File { get; private set; }

        //public EnergyVisualizationData(EnergyVisualizationEntityData data, EnergyVisualizationEntityFile file)
        public EnergyVisualizationData(EnergyVisualizationEntityData data)
        {
            Data = data;
            //File = file;
        }
    }
}
