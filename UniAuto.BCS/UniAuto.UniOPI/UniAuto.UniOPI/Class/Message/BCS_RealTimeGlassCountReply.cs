using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniOPI
{
    public class BCS_RealTimeGlassCountReply
    {
        public bool IsReply { get; set; }  //判斷RealTimeGlassCountReply是否已回復

        public DateTime LastRequestDate { get; set; } //最後發送request的時間     

        public Dictionary<string, RealTimeGlassCount> Dic_RealTimeGlassCount { get; set; }  //// Key: NODENO

        public BCS_RealTimeGlassCountReply()
        {
            IsReply = true;
 
            LastRequestDate = Convert.ToDateTime("2010-01-01 00:00:00");

            Dic_RealTimeGlassCount = new Dictionary<string, RealTimeGlassCount>();
        }

    }

    public class RealTimeGlassCount
    {
        public int AssemblyTFTCount { get; set; }
        public int NotAssemblyTFTCount { get; set; }
        public int CFGlassCount { get; set; }
        public int ThroughGlassCount { get; set; }
        public int PIThicknessDummy { get; set; }
        public int UVMaskGlassCount { get; set; }
        public int TotalCount { get; set; }
    }
}
