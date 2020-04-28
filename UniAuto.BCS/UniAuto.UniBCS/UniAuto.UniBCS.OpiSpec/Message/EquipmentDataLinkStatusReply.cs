using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class EquipmentDataLinkStatusReply : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string BATONPASSSTATUS { get; set; }

			public string BATONPASSINTERRUPTION { get; set; }

			public string DATALINKSTOP { get; set; }

			public string STATIONLOOPSTATUS { get; set; }

			public string BATONPASSEACHSTATION { get; set; }

			public string CYCLETRANSMISSIONSTATUS { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				BATONPASSSTATUS = string.Empty;
				BATONPASSINTERRUPTION = string.Empty;
				DATALINKSTOP = string.Empty;
				STATIONLOOPSTATUS = string.Empty;
				BATONPASSEACHSTATION = string.Empty;
				CYCLETRANSMISSIONSTATUS = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public EquipmentDataLinkStatusReply()
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
