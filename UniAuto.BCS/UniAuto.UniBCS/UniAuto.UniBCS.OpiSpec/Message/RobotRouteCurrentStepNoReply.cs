using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class RobotRouteCurrentStepNoReply : Message
    {
        public class TrxBody : Body
        {
            public string LINENAME { get; set; }

            public string ROBOTNAME { get; set; }

            public string CASSETTESEQNO { get; set; }

            public string JOBSEQNO { get; set; }

            public string GLASSID { get; set; }

            public string CURRENTROUTEID { get; set; }

            public string CURRENTSTEPNO { get; set; }

            public string NEXTSTEPNO { get; set; }

            public TrxBody()
            {
                LINENAME = string.Empty;
                ROBOTNAME = string.Empty;
                CASSETTESEQNO = string.Empty;
                JOBSEQNO = string.Empty;
                GLASSID = string.Empty;
                CURRENTROUTEID = string.Empty;
                CURRENTSTEPNO = string.Empty;
                NEXTSTEPNO = string.Empty;
            }
        }

        public TrxBody BODY { get; set; }

        public new Return RETURN { get { return _return; } set { _return = value; } }

        public RobotRouteCurrentStepNoReply()
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
