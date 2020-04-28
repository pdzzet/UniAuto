using System;
using System.Collections.Generic;

namespace UniOPI
{
    public class DEFECTc
    {
        public string CassetteSeqNo { get; set; }
        public string JobSeqNo { get; set; }
        public string UnitNo { get; set; }
        public string UnitID { get; set; }
        public string DefectCode { get; set; }
        public string ChipPosition { get; set; }

        public DEFECTc()
        {
            CassetteSeqNo = string.Empty;
            JobSeqNo = string.Empty;
            UnitNo = string.Empty;
            UnitID = string.Empty;
            DefectCode = string.Empty;
            ChipPosition = string.Empty;
        }
    }

    public class BCS_DefectCodeReply
    {
        public bool IsReply { get; set; }

        public DateTime LastRequestDate { get; set; }

        public List<DEFECTc> LstDefect { get; set; }

        public BCS_DefectCodeReply()
        {
            IsReply = true;

            LastRequestDate = Convert.ToDateTime("2010-01-01 00:00:00");
            this.LstDefect = new List<DEFECTc>();
        }

    }
}
