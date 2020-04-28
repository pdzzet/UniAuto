using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class RobotStageInfoReport : Message
    {
		public class STAGEc
		{
            public string ROBOTNAME { get; set; }

            public string STAGEID { get; set; }

            public string STAGESTATUS { get; set; }

            [XmlArray("JOBLIST")]
            [XmlArrayItem("JOB")]
            public List<JOBc> JOBLIST { get; set; }

			public STAGEc()
			{
                ROBOTNAME = string.Empty;
                STAGEID = string.Empty;
                STAGESTATUS = string.Empty;

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

            [XmlArray("STAGELIST")]
            [XmlArrayItem("STAGE")]
			public List<STAGEc> STAGELIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				STAGELIST = new List<STAGEc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public RobotStageInfoReport()
		{
			this.Direction = Spec.DirType.BC_TO_OPI;
            this.WaitReply = ""; // "RobotStageInfoReportReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
    }
}
