using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniOPI
{
    public class BCS_GlassGradeMappingChangeReportReply
    {
        public bool IsReply { get; set; }  //判斷SlotPositionReply是否已回復

        public DateTime LastRequestDate { get; set; } //最後發送request的時間        

        public string GlassGrade_OK { get; set; }

        public string GlassGrade_NG { get; set; }


        public BCS_GlassGradeMappingChangeReportReply()
        {
            IsReply = true;

            LastRequestDate = Convert.ToDateTime("2010-01-01 00:00:00");

            GlassGrade_OK = "0".PadLeft(16, '0');

            GlassGrade_NG = "0".PadLeft(16, '0');

        }

    }
}
