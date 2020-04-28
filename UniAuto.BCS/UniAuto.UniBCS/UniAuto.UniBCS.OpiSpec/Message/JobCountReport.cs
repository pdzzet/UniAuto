using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class JobCountReport : Message
	{
		public class UNITc
		{
			public string UNITNO { get; set; }

			public string UNITID { get; set; }

			public string TFTJOBCNT { get; set; }

			public string CFJOBCNT { get; set; }

			public UNITc()
			{
				UNITNO = string.Empty;
				UNITID = string.Empty;
				TFTJOBCNT = string.Empty;
				CFJOBCNT = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string EQUIPMENTNO { get; set; }

			public string TFTJOBCNT { get; set; }

			public string CFJOBCNT { get; set; }

			public string DMYJOBCNT { get; set; }

			public string THROUGHDMYJOBCNT { get; set; }

            public string THICKNESSDMYJOBCNT { get; set; }

            public string UNASSEMBLEDTFTDMYJOBCNT { get; set; }//sy add 20160826

            public string ITODMYJOBCNT { get; set; }//sy add 20160826

            public string NIPDMYJOBCNT { get; set; }//sy add 20160826

            public string METALONEDMYJOBCNT { get; set; }//sy add 20160826

			public string UVMASKJOBCNT { get; set; }

			[XmlArray("UNITLIST")]
			[XmlArrayItem("UNIT")]
			public List<UNITc> UNITLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				EQUIPMENTNO = string.Empty;
				TFTJOBCNT = string.Empty;
				CFJOBCNT = string.Empty;
				DMYJOBCNT = string.Empty;
				THROUGHDMYJOBCNT = string.Empty;
                THICKNESSDMYJOBCNT = string.Empty;
                UNASSEMBLEDTFTDMYJOBCNT = string.Empty;//sy add 20160826
                ITODMYJOBCNT = string.Empty;//sy add 20160826
                NIPDMYJOBCNT = string.Empty;//sy add 20160826
                METALONEDMYJOBCNT = string.Empty;//sy add 20160826
				UVMASKJOBCNT = string.Empty;
				UNITLIST = new List<UNITc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public JobCountReport()
		{
			this.Direction = Spec.DirType.BC_TO_OPI;
			this.WaitReply = "JobCountReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
