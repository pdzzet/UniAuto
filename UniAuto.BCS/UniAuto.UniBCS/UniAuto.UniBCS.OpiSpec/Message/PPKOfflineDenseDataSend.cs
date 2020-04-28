using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class PPKOfflineDenseDataSend : Message
    {
        public class TrxBody : Body
        {
            public string LINENAME { get; set; }

            public string EQUIPMENTNO { get; set; }

            public string PORTNO { get; set; }

            public string BOXID { get; set; }

            public string PAPER_BOXID { get; set; }

            public string BOXTYPE { get; set; }

            public string PRODUCTTYPE { get; set; }

            public string JOBGRADE { get; set; }

            public TrxBody()
            {
                LINENAME = string.Empty;
                EQUIPMENTNO = string.Empty;
                PORTNO = string.Empty;
                BOXID = string.Empty;
                PAPER_BOXID = string.Empty;
                PRODUCTTYPE = string.Empty;
                BOXTYPE = string.Empty;
                JOBGRADE = string.Empty;
            }
        }

        public TrxBody BODY { get; set; }

        public new Return RETURN { get { return _return; } set { _return = value; } }

        public PPKOfflineDenseDataSend()
        {
            this.Direction = Spec.DirType.OPI_TO_BC;
            this.WaitReply = "PPKOfflineDenseDataSendReply";
            this.HEADER.MESSAGENAME = GetType().Name;
            this.BODY = new TrxBody();
        }

        public override Body GetBody()
        {
            return this.BODY;
        }
    }
}
