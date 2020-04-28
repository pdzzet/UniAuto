using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniOPI
{
    public class BCS_EachPositionReply
    {
        public bool IsReply { get; set; }  //判斷SlotPositionReply是否已回復

        public List<PositionInfo> Lst_Position { get; set; }

        public string PositionNodeNo { get; set; }

        public string PositionUnitNo_DB { get; set; } //DB原始資料
        
        public string PositionUnitNo { get; set; } //補足兩碼

        public string PositionUnitType { get; set; }

        public string PositionTrxNo { get; set; }

        public bool ReportPositionName { get; set; } //是否上報POSITION NAME- FOR SECS 機台, Y:表示POSITION NO內填寫的內容為POSITION NAME, N:表示填寫POSITION NO)

        public DateTime LastRequestDate { get; set; } //最後發送request的時間        

        public BCS_EachPositionReply()
        {
            IsReply = true;

            LastRequestDate = Convert.ToDateTime("2010-01-01 00:00:00");
            Lst_Position = new List<PositionInfo>();
        }
    }

    public class PositionInfo
    {
        //public string NodeNo { get; set; }
        //public string UnitNo { get; set; }
        //public string UnitType { get; set; }
        //public string PositionTrxNo { get; set; }
        public int PositionNo { get; set; }
        public string PositionName { get; set; }
        public string CassetteSeqNo { get; set; }
        public string JobSeqNo { get; set; }
        public string JobID { get; set; }
    }
}
