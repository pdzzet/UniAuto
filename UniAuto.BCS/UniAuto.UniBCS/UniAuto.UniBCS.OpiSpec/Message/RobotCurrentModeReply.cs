using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class RobotCurrentModeReply : Message
    {
        public class EQUIPMENTc
        {
            public string EQUIPMENTNO { get; set; }

            [XmlArray("ROBOTLIST")]
            [XmlArrayItem("ROBOT")]
            public List<ROBOTc> ROBOTLIST { get; set; }

            public EQUIPMENTc()
            {
                EQUIPMENTNO = string.Empty;
                ROBOTLIST = new List<ROBOTc>();
            }
        }

        public class ARMc
        {
            public string ARMNO { get; set; }
            public string ARM_ENABLE { get; set; }
            public string MAX_GLASS_COUNT { get; set; }
            public string FORK_FRONT_JOBEXIST { get; set; }
            public string FORK_FRONT_CSTSEQ { get; set; }
            public string FORK_FRONT_JOBSEQ { get; set; }
            public string FORK_FRONT_TRACKINGVALUE { get; set; }
            public string FORK_BACK_JOBEXIST { get; set; }
            public string FORK_BACK_CSTSEQ { get; set; }
            public string FORK_BACK_JOBSEQ { get; set; }
            public string FORK_BACK_TRACKINGVALUE { get; set; }

            public ARMc()
            {
                ARMNO = string.Empty;
                ARM_ENABLE = string.Empty;
                MAX_GLASS_COUNT = string.Empty;
                FORK_FRONT_JOBEXIST = string.Empty;
                FORK_FRONT_CSTSEQ = string.Empty;
                FORK_FRONT_JOBSEQ = string.Empty;
                FORK_BACK_JOBEXIST = string.Empty;
                FORK_BACK_CSTSEQ = string.Empty;
                FORK_BACK_JOBSEQ = string.Empty;
                FORK_FRONT_TRACKINGVALUE = string.Empty;
                FORK_BACK_TRACKINGVALUE = string.Empty;
            }
        }

        public class ROBOTc
        {
            public string ROBOTNAME { get; set; }

            public string ROBOTMODE { get; set; }

            public string SAMEEQFLAG { get; set; }

            public string ROBOTSTATUS { get; set; }

            public string HAVE_ROBOT_CMD { get; set; }

            public string CURRENT_POSITION { get; set; }

            public string HOLD_STATUS { get; set; }

            [XmlArray("ARMLIST")]
            [XmlArrayItem("ARM")]
            public List<ARMc> ARMLIST { get; set; }

            public ROBOTc()
            {
                ROBOTNAME = string.Empty;
                ROBOTMODE = string.Empty;
                SAMEEQFLAG = string.Empty;
                ROBOTSTATUS = string.Empty;
                HAVE_ROBOT_CMD = string.Empty;
                CURRENT_POSITION = string.Empty;
                HOLD_STATUS = string.Empty;
            }
        }

        public class TrxBody : Body
        {
            public string LINENAME { get; set; }


            [XmlArray("EQUIPMENTLIST")]
            [XmlArrayItem("EQUIPMENT")]
            public List<EQUIPMENTc> EQUIPMENTLIST { get; set; }

            public TrxBody()
            {
                LINENAME = string.Empty;
                EQUIPMENTLIST = new List<EQUIPMENTc>();
            }
        }

        public TrxBody BODY { get; set; }

        public new Return RETURN { get { return _return; } set { _return = value; } }

        public RobotCurrentModeReply()
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
