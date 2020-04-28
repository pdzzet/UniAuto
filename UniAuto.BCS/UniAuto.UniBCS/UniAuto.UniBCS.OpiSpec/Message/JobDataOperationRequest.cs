using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class JobDataOperationRequest : Message
    {
        public class JOBDATAc
        {
            public string CASSETTESEQNO { get; set; }

            public string JOBSEQNO { get; set; }

            public string GLASSID { get; set; }

            public JOBDATAc()
            {
                CASSETTESEQNO = string.Empty;
                JOBSEQNO = string.Empty;
                GLASSID = string.Empty;
            }
        }
        public class TrxBody : Body
		{
			public string LINENAME { get; set; }

            public string COMMAND { get; set; }

            [XmlArray("JOBDATALIST")]
            [XmlArrayItem("JOBDATA")]
            public List<JOBDATAc> JOBDATALIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
                COMMAND = string.Empty;
                JOBDATALIST = new List<JOBDATAc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public JobDataOperationRequest()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
            this.WaitReply = "JobDataOperationReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
    }
}
