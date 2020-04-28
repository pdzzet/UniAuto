using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class RobotStageInfoRequestReply : Message
    {
        public class ROBOTc
        {
            public string ROBOTNAME { get; set; }

            [XmlArray("STAGELIST")]
            [XmlArrayItem("STAGE")]
            public List<STAGEc> STAGELIST { get; set; }

            public ROBOTc()
            {
                ROBOTNAME = string.Empty;
                STAGELIST = new List<STAGEc>();
            }
        }

        public class STAGEc
        {
            public string STAGEID { get; set; }

            public string STAGESTATUS { get; set; }

            public string STAGESCSTSEQ { get; set; }

            public string STAGESJOBSEQ { get; set; }

            [XmlArray("JOBLIST")]
            [XmlArrayItem("JOB")]
            public List<JOBc> JOBLIST { get; set; }

            public STAGEc()
            {
                STAGEID = string.Empty;
                STAGESTATUS = string.Empty;
                STAGESCSTSEQ = string.Empty;
                STAGESJOBSEQ = string.Empty;

                JOBLIST = new List<JOBc>();
            }
        }

        public class JOBc
        {
            public string SLOTNO { get; set; }

            public string JOBEXIST { get; set; }

            public string CASSETTESEQNO { get; set; }

            public string JOBSEQNO { get; set; }

            public string TRACKINGVALUE { get; set; }

            public JOBc()
            {
                SLOTNO = string.Empty;
                JOBEXIST = string.Empty;
                CASSETTESEQNO = string.Empty;
                JOBSEQNO = string.Empty;
                TRACKINGVALUE = string.Empty;
            }
        }

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

            [XmlArray("ROBOTLIST")]
            [XmlArrayItem("ROBOT")]
            public List<ROBOTc> ROBOTLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
                ROBOTLIST = new List<ROBOTc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public RobotStageInfoRequestReply()
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
