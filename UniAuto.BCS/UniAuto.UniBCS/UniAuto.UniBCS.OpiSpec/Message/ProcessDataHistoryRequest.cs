using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class ProcessDataHistoryRequest : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string EQUIPMENTNO { get; set; }

			public string JOBSEQNO { get; set; }

			public string CSTSEQNO { get; set; }

			public string JOBID { get; set; }

			public string TRXID { get; set; }

			public string FILENAME { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				EQUIPMENTNO = string.Empty;
				JOBSEQNO = string.Empty;
				CSTSEQNO = string.Empty;
				JOBID = string.Empty;
				TRXID = string.Empty;
				FILENAME = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public ProcessDataHistoryRequest()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
			this.WaitReply = "ProcessDataHistoryReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
