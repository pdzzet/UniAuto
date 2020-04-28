using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class RobotStopRunReasonReply : Message
    {
        public class TrxBody : Body
        {
            public string LINENAME { get; set; }

            public string ROBOTNAME { get; set; }

            public string CASSETTESEQNO { get; set; }

            public string JOBSEQNO { get; set; }

            public string GLASSID { get; set; }

            public string REAL_STEPID { get; set; }

            public string REAL_NEXT_STEPID { get; set; }

            [XmlArray("JOBREASONLIST")]
            [XmlArrayItem("JOBREASON")]
            public List<REASONc> JOBREASONLIST { get; set; }

            [XmlArray("ROBOTREASONLIST")]
            [XmlArrayItem("ROBOTREASON")]
            public List<REASONc> ROBOTREASONLIST { get; set; }

            public TrxBody()
            {
                LINENAME = string.Empty;
                ROBOTNAME = string.Empty;
                CASSETTESEQNO = string.Empty;
                JOBSEQNO = string.Empty;
                GLASSID = string.Empty;
                REAL_STEPID = string.Empty;
                REAL_NEXT_STEPID = string.Empty;

                JOBREASONLIST = new List<REASONc>();
                ROBOTREASONLIST = new List<REASONc>();

            }
        }

        public class REASONc
        {
            public string STOP_REASON { get; set; }

            public REASONc()
            {
                STOP_REASON = string.Empty;
            }
        }
        public TrxBody BODY { get; set; }

        public new Return RETURN { get { return _return; } set { _return = value; } }

        public RobotStopRunReasonReply()
        {
            this.Direction = Spec.DirType.BC_TO_OPI;
            this.WaitReply = "";
            this.HEADER.MESSAGENAME = GetType().Name;
            this.BODY = new TrxBody();
        }

        public override Body GetBody()
        {
            return this.BODY;
        }
    }
}
