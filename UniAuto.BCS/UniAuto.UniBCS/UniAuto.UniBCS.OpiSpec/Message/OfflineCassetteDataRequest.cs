using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class OfflineCassetteDataRequest : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string EQUIPMENTNO { get; set; }

			public string PORTNO { get; set; }

			public string PORTID { get; set; }

			public string CASSETTEID { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				EQUIPMENTNO = string.Empty;
				PORTNO = string.Empty;
				PORTID = string.Empty;
				CASSETTEID = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public OfflineCassetteDataRequest()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
			this.WaitReply = "OfflineCassetteDataReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
