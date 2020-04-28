//Add By hujunpeng For PI T/C数量监控 20190723
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UniAuto.UniBCS.OpiSpec;
namespace UniOPI
{
    public class PIJobCount
    {
        public string ProductGroupOwner01 { get; set; }
        public int TFTCount01 { get; set; }
        public int CFCount01 { get; set; }

        public string ProductGroupOwner02 { get; set; }
        public int TFTCount02 { get; set; }
        public int CFCount02 { get; set; }

        public string ProductGroupOwner03 { get; set; }
        public int TFTCount03 { get; set; }
        public int CFCount03 { get; set; }

        public string ProductGroupOwner04 { get; set; }
        public int TFTCount04 { get; set; }
        public int CFCount04 { get; set; }

        public string ProductGroupOwner05 { get; set; }
        public int TFTCount05 { get; set; }
        public int CFCount05 { get; set; }

        public string ProductGroupOwner06 { get; set; }
        public int TFTCount06 { get; set; }
        public int CFCount06 { get; set; }

        public string ProductGroupOwner07 { get; set; }
        public int TFTCount07 { get; set; }
        public int CFCount07 { get; set; }

        public string ProductGroupOwner08 { get; set; }
        public int TFTCount08 { get; set; }
        public int CFCount08 { get; set; }

        public string ProductGroupOwner09 { get; set; }
        public int TFTCount09 { get; set; }
        public int CFCount09 { get; set; }

        public string ProductGroupOwner10 { get; set; }
        public int TFTCount10 { get; set; }
        public int CFCount10 { get; set; }

        public PIJobCount()
        {
            ProductGroupOwner01 = string.Empty;
            TFTCount01 = 0;
            CFCount01 = 0;

            ProductGroupOwner02 = string.Empty;
            TFTCount02 = 0;
            CFCount02 = 0;

            ProductGroupOwner03 = string.Empty;
            TFTCount03 = 0;
            CFCount03 = 0;

            ProductGroupOwner04 = string.Empty;
            TFTCount04 = 0;
            CFCount04 = 0;

            ProductGroupOwner05 = string.Empty;
            TFTCount05 = 0;
            CFCount05 = 0;

            ProductGroupOwner06 = string.Empty;
            TFTCount06 = 0;
            CFCount06 = 0;

            ProductGroupOwner07 = string.Empty;
            TFTCount07 = 0;
            CFCount07 = 0;

            ProductGroupOwner08 = string.Empty;
            TFTCount08 = 0;
            CFCount08 = 0;

            ProductGroupOwner09 = string.Empty;
            TFTCount09 = 0;
            CFCount09 = 0;

            ProductGroupOwner10 = string.Empty;
            TFTCount10 = 0;
            CFCount10 = 0;
        }
        public void SetPIJobCountInfo(PIJobCountReport PIJobCount)
        {
            int _num = 0;
            this.ProductGroupOwner01 = PIJobCount.BODY.ProductGroupOwner01;
            this.TFTCount01 = (int.TryParse(PIJobCount.BODY.TFTCount01, out _num) == true ? int.Parse(PIJobCount.BODY.TFTCount01) : 0);
            this.CFCount01 = (int.TryParse(PIJobCount.BODY.CFCount01, out _num) == true ? int.Parse(PIJobCount.BODY.CFCount01) : 0);

            this.ProductGroupOwner02 = PIJobCount.BODY.ProductGroupOwner02;
            this.TFTCount02 = (int.TryParse(PIJobCount.BODY.TFTCount02, out _num) == true ? int.Parse(PIJobCount.BODY.TFTCount02) : 0);
            this.CFCount02 = (int.TryParse(PIJobCount.BODY.CFCount02, out _num) == true ? int.Parse(PIJobCount.BODY.CFCount02) : 0);

            this.ProductGroupOwner03 = PIJobCount.BODY.ProductGroupOwner03;
            this.TFTCount03 = (int.TryParse(PIJobCount.BODY.TFTCount03, out _num) == true ? int.Parse(PIJobCount.BODY.TFTCount03) : 0);
            this.CFCount03 = (int.TryParse(PIJobCount.BODY.CFCount03, out _num) == true ? int.Parse(PIJobCount.BODY.CFCount03) : 0);

            this.ProductGroupOwner04 = PIJobCount.BODY.ProductGroupOwner04;
            this.TFTCount04 = (int.TryParse(PIJobCount.BODY.TFTCount04, out _num) == true ? int.Parse(PIJobCount.BODY.TFTCount04) : 0);
            this.CFCount04 = (int.TryParse(PIJobCount.BODY.CFCount04, out _num) == true ? int.Parse(PIJobCount.BODY.CFCount04) : 0);

            this.ProductGroupOwner05 = PIJobCount.BODY.ProductGroupOwner05;
            this.TFTCount05 = (int.TryParse(PIJobCount.BODY.TFTCount05, out _num) == true ? int.Parse(PIJobCount.BODY.TFTCount05) : 0);
            this.CFCount05 = (int.TryParse(PIJobCount.BODY.CFCount05, out _num) == true ? int.Parse(PIJobCount.BODY.CFCount05) : 0);

            this.ProductGroupOwner06 = PIJobCount.BODY.ProductGroupOwner06;
            this.TFTCount06 = (int.TryParse(PIJobCount.BODY.TFTCount06, out _num) == true ? int.Parse(PIJobCount.BODY.TFTCount06) : 0);
            this.CFCount06 = (int.TryParse(PIJobCount.BODY.CFCount06, out _num) == true ? int.Parse(PIJobCount.BODY.CFCount06) : 0);

            this.ProductGroupOwner07 = PIJobCount.BODY.ProductGroupOwner07;
            this.TFTCount07 = (int.TryParse(PIJobCount.BODY.TFTCount07, out _num) == true ? int.Parse(PIJobCount.BODY.TFTCount07) : 0);
            this.CFCount07 = (int.TryParse(PIJobCount.BODY.CFCount07, out _num) == true ? int.Parse(PIJobCount.BODY.CFCount07) : 0);

            this.ProductGroupOwner08 = PIJobCount.BODY.ProductGroupOwner08;
            this.TFTCount08 = (int.TryParse(PIJobCount.BODY.TFTCount08, out _num) == true ? int.Parse(PIJobCount.BODY.TFTCount08) : 0);
            this.CFCount08 = (int.TryParse(PIJobCount.BODY.CFCount08, out _num) == true ? int.Parse(PIJobCount.BODY.CFCount08) : 0);

            this.ProductGroupOwner09 = PIJobCount.BODY.ProductGroupOwner09;
            this.TFTCount09 = (int.TryParse(PIJobCount.BODY.TFTCount09, out _num) == true ? int.Parse(PIJobCount.BODY.TFTCount09) : 0);
            this.CFCount09 = (int.TryParse(PIJobCount.BODY.CFCount09, out _num) == true ? int.Parse(PIJobCount.BODY.CFCount09) : 0);

            this.ProductGroupOwner10 = PIJobCount.BODY.ProductGroupOwner10;
            this.TFTCount10 = (int.TryParse(PIJobCount.BODY.TFTCount10, out _num) == true ? int.Parse(PIJobCount.BODY.TFTCount10) : 0);
            this.CFCount10 = (int.TryParse(PIJobCount.BODY.CFCount10, out _num) == true ? int.Parse(PIJobCount.BODY.CFCount10) : 0);
        }
        public void SetPIJobCountInfo(AllDataUpdateReply.PIJobCountReport PIJobCount)
        {
            int _num = 0;
            this.ProductGroupOwner01 = PIJobCount.ProductGroupOwner01;
            this.TFTCount01 = (int.TryParse(PIJobCount.TFTCount01, out _num) == true ? int.Parse(PIJobCount.TFTCount01) : 0);
            this.CFCount01 = (int.TryParse(PIJobCount.CFCount01, out _num) == true ? int.Parse(PIJobCount.CFCount01) : 0);

            this.ProductGroupOwner02 = PIJobCount.ProductGroupOwner02;
            this.TFTCount02 = (int.TryParse(PIJobCount.TFTCount02, out _num) == true ? int.Parse(PIJobCount.TFTCount02) : 0);
            this.CFCount02 = (int.TryParse(PIJobCount.CFCount02, out _num) == true ? int.Parse(PIJobCount.CFCount02) : 0);

            this.ProductGroupOwner03 = PIJobCount.ProductGroupOwner03;
            this.TFTCount03 = (int.TryParse(PIJobCount.TFTCount03, out _num) == true ? int.Parse(PIJobCount.TFTCount03) : 0);
            this.CFCount03 = (int.TryParse(PIJobCount.CFCount03, out _num) == true ? int.Parse(PIJobCount.CFCount03) : 0);

            this.ProductGroupOwner04 = PIJobCount.ProductGroupOwner04;
            this.TFTCount04 = (int.TryParse(PIJobCount.TFTCount04, out _num) == true ? int.Parse(PIJobCount.TFTCount04) : 0);
            this.CFCount04 = (int.TryParse(PIJobCount.CFCount04, out _num) == true ? int.Parse(PIJobCount.CFCount04) : 0);

            this.ProductGroupOwner05 = PIJobCount.ProductGroupOwner05;
            this.TFTCount05 = (int.TryParse(PIJobCount.TFTCount05, out _num) == true ? int.Parse(PIJobCount.TFTCount05) : 0);
            this.CFCount05 = (int.TryParse(PIJobCount.CFCount05, out _num) == true ? int.Parse(PIJobCount.CFCount05) : 0);

            this.ProductGroupOwner06 = PIJobCount.ProductGroupOwner06;
            this.TFTCount06 = (int.TryParse(PIJobCount.TFTCount06, out _num) == true ? int.Parse(PIJobCount.TFTCount06) : 0);
            this.CFCount06 = (int.TryParse(PIJobCount.CFCount06, out _num) == true ? int.Parse(PIJobCount.CFCount06) : 0);

            this.ProductGroupOwner07 = PIJobCount.ProductGroupOwner07;
            this.TFTCount07 = (int.TryParse(PIJobCount.TFTCount07, out _num) == true ? int.Parse(PIJobCount.TFTCount07) : 0);
            this.CFCount07 = (int.TryParse(PIJobCount.CFCount07, out _num) == true ? int.Parse(PIJobCount.CFCount07) : 0);

            this.ProductGroupOwner08 = PIJobCount.ProductGroupOwner08;
            this.TFTCount08 = (int.TryParse(PIJobCount.TFTCount08, out _num) == true ? int.Parse(PIJobCount.TFTCount08) : 0);
            this.CFCount08 = (int.TryParse(PIJobCount.CFCount08, out _num) == true ? int.Parse(PIJobCount.CFCount08) : 0);

            this.ProductGroupOwner09 = PIJobCount.ProductGroupOwner09;
            this.TFTCount09 = (int.TryParse(PIJobCount.TFTCount09, out _num) == true ? int.Parse(PIJobCount.TFTCount09) : 0);
            this.CFCount09 = (int.TryParse(PIJobCount.CFCount09, out _num) == true ? int.Parse(PIJobCount.CFCount09) : 0);

            this.ProductGroupOwner10 = PIJobCount.ProductGroupOwner10;
            this.TFTCount10 = (int.TryParse(PIJobCount.TFTCount10, out _num) == true ? int.Parse(PIJobCount.TFTCount10) : 0);
            this.CFCount10 = (int.TryParse(PIJobCount.CFCount10, out _num) == true ? int.Parse(PIJobCount.CFCount10) : 0);

        }
    }
}


