using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniOPI
{
    public class BCS_IonizerFanModeReportReply
    {
        public bool IsReply { get; set; }  //判斷SlotPositionReply是否已回復

        public DateTime LastRequestDate { get; set; } //最後發送request的時間      

        public string EnableMode { get; set; }  //Fan No#01 ~  Fan No#32 Enable Mode(32bit) -- 0: Disable / 1:Enable

        public BCS_IonizerFanModeReportReply()
        {
            IsReply = true;
 
            LastRequestDate = Convert.ToDateTime("2010-01-01 00:00:00");

            EnableMode = "0".PadLeft(32, '0');
        }
    }
}
