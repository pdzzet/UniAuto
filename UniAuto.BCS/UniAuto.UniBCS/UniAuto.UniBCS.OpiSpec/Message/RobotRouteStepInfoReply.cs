using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class RobotRouteStepInfoReply : Message
    {
        public class ITEMc
        {
            public string VNAME { get; set; }

            public string VVALUE { get; set; }

            public ITEMc()
            {
                VNAME = string.Empty;
                VVALUE = string.Empty;
            }
        }

        public class TrxBody : Body
        {
            public string LINENAME { get; set; }

            public string CASSETTESEQNO { get; set; }

            public string JOBSEQNO { get; set; }

            public string GLASSID { get; set; }

            public string ROUTE_ID { get; set; }

            public string REAL_STEPID { get; set; }

            public string REAL_NEXT_STEPID { get; set; }

            [XmlArray("ITEMLIST")]
            [XmlArrayItem("PARAMETER")]
            public List<ITEMc> ITEMLIST { get; set; }

            public TrxBody()
            {
                LINENAME = string.Empty;
                CASSETTESEQNO = string.Empty;
                JOBSEQNO = string.Empty;
                GLASSID = string.Empty;
                ROUTE_ID = string.Empty;
                REAL_STEPID = string.Empty;
                REAL_NEXT_STEPID = string.Empty;
                ITEMLIST = new List<ITEMc>();
            }
        }

        public TrxBody BODY { get; set; }

        public new Return RETURN { get { return _return; } set { _return = value; } }

        public RobotRouteStepInfoReply()
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
