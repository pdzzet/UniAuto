using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class APCDataReportRequest : Message
    {
        public class TrxBody : Body
        {
            public string LINENAME { get; set; }

            public string EQUIPMENTNO { get; set; }

            //add by box.zhai  20161103 区分BC向SECS_CSOT机台请求回复的值是 Item ID还是Item Name
            //PLC  SECS_NIKON机台不需要使用
            public string SECSREQUESTBYIDORNAME { get; set; }

            public TrxBody()
            {
                LINENAME = string.Empty;
                EQUIPMENTNO = string.Empty;
            }
        }

        public TrxBody BODY { get; set; }

        public new Return RETURN { get { return _return; } set { _return = value; } }

        public APCDataReportRequest()
        {
            this.Direction = Spec.DirType.OPI_TO_BC;
            this.WaitReply = "APCDataReportReply";
            this.HEADER.MESSAGENAME = GetType().Name;
            this.BODY = new TrxBody();
        }

        public override Body GetBody()
        {
            return this.BODY;
        }
    }
}
