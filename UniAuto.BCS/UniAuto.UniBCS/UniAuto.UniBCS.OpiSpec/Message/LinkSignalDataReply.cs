using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class LinkSignalDataReply : Message
	{
		public class JOBDATAc
		{
            public string JOBADDRESS { get; set; }

			public string CASSETTESEQNO { get; set; }

			public string JOBSEQNO { get; set; }

			public string PRODUCTTYPE { get; set; }

			public string SUBSTRATETYPE { get; set; }

			public string JOBTYPE { get; set; }

			public string JOBJUDGE { get; set; }

			public string JOBGRADE { get; set; }

			public string GLASSID { get; set; }

			public string PPID { get; set; }

			public string TRACKINGDATA { get; set; }

			public string EQPFLAG { get; set; }

            public JOBDATAc()
			{
                JOBADDRESS = string.Empty;
				CASSETTESEQNO = string.Empty;
				JOBSEQNO = string.Empty;
				PRODUCTTYPE = string.Empty;
				SUBSTRATETYPE = string.Empty;
				JOBTYPE = string.Empty;
				JOBJUDGE = string.Empty;
				JOBGRADE = string.Empty;
				GLASSID = string.Empty;
				PPID = string.Empty;
				TRACKINGDATA = string.Empty;
				EQPFLAG = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string UPSTREAMEQUIPMENTNO { get; set; }

			public string UPSTREAMEQUIPMENTUNITNO { get; set; }

			public string DOWNSTREAMEQUIPMENTNO { get; set; }

			public string DOWNSTREAMEQUIPMENTUNITNO { get; set; }

            public string UPSTREAMSEQUENCENO { get; set; }

            public string DOWNSTREAMSEQUENCENO { get; set; }

			public string UPSTREAMSIGNAL { get; set; }

			public string DOWNSTREAMSIGNAL { get; set; }

            [XmlArray("UPSTREAMJOBDATALIST")]
            [XmlArrayItem("JOBDATA")]
            public List<JOBDATAc> UPSTREAMJOBDATALIST { get; set; }

            [XmlArray("DOWNSTREAMJOBDATALIST")]
            [XmlArrayItem("JOBDATA")]
            public List<JOBDATAc> DOWNSTREAMJOBDATALIST { get; set; }

			public string UPSTREAMBITADDRESS { get; set; }

			public string DOWNSTREAMBITADDRESS { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				UPSTREAMEQUIPMENTNO = string.Empty;
				UPSTREAMEQUIPMENTUNITNO = string.Empty;
				DOWNSTREAMEQUIPMENTNO = string.Empty;
				DOWNSTREAMEQUIPMENTUNITNO = string.Empty;
                UPSTREAMSEQUENCENO = string.Empty;
                DOWNSTREAMSEQUENCENO = string.Empty;
				UPSTREAMSIGNAL = string.Empty;
				DOWNSTREAMSIGNAL = string.Empty;
                UPSTREAMJOBDATALIST = new List<JOBDATAc>();
                DOWNSTREAMJOBDATALIST = new List<JOBDATAc>();
				UPSTREAMBITADDRESS = string.Empty;
				DOWNSTREAMBITADDRESS = string.Empty;				
				
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public LinkSignalDataReply()
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
