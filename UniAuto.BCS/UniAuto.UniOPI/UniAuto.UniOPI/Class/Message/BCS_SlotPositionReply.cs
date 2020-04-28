using System;
using System.Collections.Generic;

namespace UniOPI
{
    public class BCS_SlotPositionReply
    {
        public bool IsReply { get; set; }  //判斷SlotPositionReply是否已回復

        public bool IsLoadData { get; set; } //判斷是否已經取得DB資料

        public List<SlotPosition> Lst_SlotPosition { get; set; }

        //public DataTable Dt_SlotPosition { get; set; }

        public DateTime LastRequestDate { get; set; } //最後發送request的時間        

        public BCS_SlotPositionReply()
        {
            IsReply = true;
            IsLoadData = false;
            LastRequestDate = Convert.ToDateTime("2010-01-01 00:00:00");
            Lst_SlotPosition = new List<SlotPosition>();

            //Dt_SlotPosition = new DataTable();

            //Dt_SlotPosition.Columns.Add("NodeNo");
            //Dt_SlotPosition.Columns.Add("PortNo");
            //Dt_SlotPosition.Columns.Add("PositionDesc");
            //Dt_SlotPosition.Columns.Add("PositionName");
            //Dt_SlotPosition.Columns.Add("PositionNo");
            //Dt_SlotPosition.Columns.Add("CassetteSeqNo");
            //Dt_SlotPosition.Columns.Add("JobSeqNo");
            //Dt_SlotPosition.Columns.Add("JobID");
            //Dt_SlotPosition.Columns.Add("RecipeName");
            //Dt_SlotPosition.Columns.Add("PPID");
            //Dt_SlotPosition.Columns.Add("TrackingValue");
            //Dt_SlotPosition.Columns.Add("SamplingSlotFlag");
        }
    }

    public class SlotPosition
    {
        public string NodeNo { get; set; }
        public string PortNo { get; set; }
        public string PositionDesc { get; set; }
        public string PositionName { get; set; }
        public int PositionNo { get; set; }
        public string CassetteSeqNo { get; set; }
        public string JobSeqNo { get; set; }
        public string JobID { get; set; }
        public string RecipeName { get; set; }
        public string PPID { get; set; }
        public string TrackingValue { get; set; }
        public string SamplingSlotFlag { get; set; }
        public string EQPRTCFlag { get; set; }  //add by yang
    }
}
