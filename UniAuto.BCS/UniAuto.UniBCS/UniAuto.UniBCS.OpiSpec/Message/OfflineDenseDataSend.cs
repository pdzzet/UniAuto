using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class OfflineDenseDataSend : Message
    {
        public class TrxBody : Body
        {
            public string LINENAME { get; set; }

            public string EQUIPMENTNO { get; set; }

            public string PORTNO { get; set; }

            public string BOXID01 { get; set; }

            public string BOXID02 { get; set; }

            public string PRODUCTTYPE { get; set; }

            public string JOBGRADE01 { get; set; }

            public string JOBGRADE02 { get; set; }

            public string CASSETTESETTINGCODE01 { get; set; }

            public string CASSETTESETTINGCODE02 { get; set; }

            public string BOXGLASSCOUNT01 { get; set; }

            public string BOXGLASSCOUNT02 { get; set; }

            public TrxBody()
            {
                LINENAME = string.Empty;
                EQUIPMENTNO = string.Empty;
                PORTNO = string.Empty;
                BOXID01 = string.Empty;
                BOXID02 = string.Empty;
                PRODUCTTYPE = string.Empty;
                JOBGRADE01 = string.Empty;
                JOBGRADE02 = string.Empty;
                CASSETTESETTINGCODE01 = string.Empty;
                CASSETTESETTINGCODE02 = string.Empty;
                BOXGLASSCOUNT01 = string.Empty;
                BOXGLASSCOUNT02 = string.Empty;
            }
        }

        public TrxBody BODY { get; set; }

        public new Return RETURN { get { return _return; } set { _return = value; } }

        public OfflineDenseDataSend()
        {
            this.Direction = Spec.DirType.OPI_TO_BC;
            this.WaitReply = "OfflineDenseDataSendReply";
            this.HEADER.MESSAGENAME = GetType().Name;
            this.BODY = new TrxBody();
        }

        public override Body GetBody()
        {
            return this.BODY;
        }
    }
}
