using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UniOPI
{
    public class BCS_EquipmentFetchGlassRuleReply
    {
        public bool IsReply { get; set; }  //判斷SlotPositionReply是否已回復

        public DateTime LastRequestDate { get; set; } //最後發送request的時間     

        public string RuleName_1  { get; set; } 

        public string RuleValue_1 { get; set; }

        public string RuleName_2  { get; set; } 


        public string RuleValue_2 { get; set; } 

        public string GetRuleName_1()
        {
            return GetProportionalNameDesc(RuleName_1);
        }

        public string GetRuleName_2()
        {
            return GetProportionalNameDesc(RuleName_2);
        }

        public BCS_EquipmentFetchGlassRuleReply()
        {
            IsReply = true;
 
            LastRequestDate = Convert.ToDateTime("2010-01-01 00:00:00");

            RuleName_1 = string.Empty;
            RuleValue_1 = string.Empty;
            RuleName_2 = string.Empty;
            RuleValue_2 = string.Empty;
        }

        private string GetProportionalNameDesc(string RuleName)
        {
            int _num = 0;

            if (int.TryParse(RuleName, out _num))
            {
                FetchGlassProportionalName _ftechName = FormMainMDI.G_OPIAp.CurLine.FetchGlassProportionalNames.Find(r => r.ProportionalNameNo.Equals(_num));

                if (_ftechName == null) return string.Format("{0}-UnKnown", RuleName);

                return _ftechName.ProportionalNameDesc; 

            }
            else
            {
                return RuleName;
            }

        }
    }
}
