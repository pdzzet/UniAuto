//Add By hujunpeng For PI T/C数量监控 20190723
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class PIJobCountReport : Message
    {
        public const string ProductGroupOwner = "ProductGroupOwner";
        public const string TFTCount = "TFTCount";
        public const string CFCount = "CFCount";
        public class TrxBody : Body
        {

            public string ProductGroupOwner01 { get; set; }
            public string TFTCount01 { get; set; }
            public string CFCount01 { get; set; }

            public string ProductGroupOwner02 { get; set; }
            public string TFTCount02 { get; set; }
            public string CFCount02 { get; set; }

            public string ProductGroupOwner03 { get; set; }
            public string TFTCount03 { get; set; }
            public string CFCount03 { get; set; }

            public string ProductGroupOwner04 { get; set; }
            public string TFTCount04 { get; set; }
            public string CFCount04 { get; set; }

            public string ProductGroupOwner05 { get; set; }
            public string TFTCount05 { get; set; }
            public string CFCount05 { get; set; }

            public string ProductGroupOwner06 { get; set; }
            public string TFTCount06 { get; set; }
            public string CFCount06 { get; set; }

            public string ProductGroupOwner07 { get; set; }
            public string TFTCount07 { get; set; }
            public string CFCount07 { get; set; }

            public string ProductGroupOwner08 { get; set; }
            public string TFTCount08 { get; set; }
            public string CFCount08 { get; set; }

            public string ProductGroupOwner09 { get; set; }
            public string TFTCount09 { get; set; }
            public string CFCount09 { get; set; }

            public string ProductGroupOwner10 { get; set; }
            public string TFTCount10 { get; set; }
            public string CFCount10 { get; set; }
            public TrxBody()
            {
                ProductGroupOwner01 = string.Empty;
                TFTCount01 = string.Empty;
                CFCount01 = string.Empty;

                ProductGroupOwner02 = string.Empty;
                TFTCount02 = string.Empty;
                CFCount02 = string.Empty;

                ProductGroupOwner03 = string.Empty;
                TFTCount03 = string.Empty;
                CFCount03 = string.Empty;

                ProductGroupOwner04 = string.Empty;
                TFTCount04 = string.Empty;
                CFCount04 = string.Empty;

                ProductGroupOwner05 = string.Empty;
                TFTCount05 = string.Empty;
                CFCount05 = string.Empty;

                ProductGroupOwner06 = string.Empty;
                TFTCount06 = string.Empty;
                CFCount06 = string.Empty;

                ProductGroupOwner07 = string.Empty;
                TFTCount07 = string.Empty;
                CFCount07 = string.Empty;

                ProductGroupOwner08 = string.Empty;
                TFTCount08 = string.Empty;
                CFCount08 = string.Empty;

                ProductGroupOwner09 = string.Empty;
                TFTCount09 = string.Empty;
                CFCount09 = string.Empty;

                ProductGroupOwner10 = string.Empty;
                TFTCount10 = string.Empty;
                CFCount10 = string.Empty;
            }
        }
        public TrxBody BODY { get; set; }
        public new Return RETURN { get { return _return; } set { _return = value; } }
        public PIJobCountReport()
        {
            this.Direction = Spec.DirType.BC_TO_OPI;
            this.WaitReply = "";//"PIJobCountReport";
            this.HEADER.MESSAGENAME = GetType().Name;
            this.BODY = new TrxBody();
        }
        public override Body GetBody()
        {
            return this.BODY;
        }
    }
}


