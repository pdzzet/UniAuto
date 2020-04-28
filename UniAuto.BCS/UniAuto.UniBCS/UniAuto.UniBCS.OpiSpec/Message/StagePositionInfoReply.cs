using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class StagePositionInfoReply : Message
    {
        public class JOBC
        {
            public string FRONT_JOBEXIST { get; set; }

            public string FRONT_CASSETTESEQNO { get; set; }

            public string FRONT_JOBSEQNO { get; set; }

            public string BACK_JOBEXIST { get; set; }

            public string BACK_CASSETTESEQNO { get; set; }

            public string BACK_JOBSEQNO { get; set; }

            public JOBC()
            {
                FRONT_JOBEXIST = string.Empty;
                FRONT_CASSETTESEQNO = string.Empty;
                FRONT_JOBSEQNO = string.Empty;
                BACK_JOBEXIST = string.Empty;
                BACK_CASSETTESEQNO = string.Empty;
                BACK_JOBSEQNO = string.Empty;
            }
        }

        public class TrxBody : Body
        {
            public string LINENAME { get; set; }

            public string EQUIPMENTNO { get; set; }

            public string ROBOTNAME { get; set; }

            public string STAGEID { get; set; }

            public string SENDYREADY { get; set; }

            public string RECEIVEREADY { get; set; }

            public string DOUBLEGLASSEXIST { get; set; }

            public string EXCHANGEPOSSIBLE { get; set; }

            [XmlArray("JOBLIST")]
            [XmlArrayItem("JOB")]
            public List<JOBC> JOBLIST { get; set; }

            public TrxBody()
            {
                LINENAME = string.Empty;
                EQUIPMENTNO = string.Empty;
                ROBOTNAME = string.Empty;
                STAGEID = string.Empty;
                SENDYREADY = string.Empty;
                RECEIVEREADY = string.Empty;
                DOUBLEGLASSEXIST = string.Empty;
                EXCHANGEPOSSIBLE = string.Empty;
            }
        }

        public TrxBody BODY { get; set; }

        public new Return RETURN { get { return _return; } set { _return = value; } }

        public StagePositionInfoReply()
        {
            this.Direction = Spec.DirType.BC_TO_OPI;
            this.WaitReply = string.Empty;
            this.HEADER.MESSAGENAME = GetType().Name;
            this.BODY = new TrxBody();
        }

        public override Body GetBody()
        {
            return this.BODY;
        }
    }
}
