using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class RobotRouteCurrentStepChangeRequest : Message
    {
        public class TrxBody : Body
        {
            public string LINENAME { get; set; }

            public string CASSETTESEQNO { get; set; }

            public string JOBSEQNO { get; set; }

            public string NEWSTEPNO { get; set; }

            public string NEXTSTEPNO { get; set; }

            public TrxBody()
            {
                LINENAME = string.Empty;
                CASSETTESEQNO = string.Empty;
                JOBSEQNO = string.Empty;
                NEWSTEPNO = string.Empty;
                NEXTSTEPNO = string.Empty;
            }
        }

        public TrxBody BODY { get; set; }

        public new Return RETURN { get { return _return; } set { _return = value; } }

        public RobotRouteCurrentStepChangeRequest()
        {
            this.Direction = Spec.DirType.OPI_TO_BC;
            this.WaitReply = "RobotRouteCurrentStepChangeReply";
            this.HEADER.MESSAGENAME = GetType().Name;
            this.BODY = new TrxBody();
        }

        public override Body GetBody()
        {
            return this.BODY;
        }
    }
}
