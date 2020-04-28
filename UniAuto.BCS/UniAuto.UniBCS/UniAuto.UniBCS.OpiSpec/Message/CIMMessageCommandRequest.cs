using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class CIMMessageCommandRequest : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string OPERATORID { get; set; }

			public string EQUIPMENTNO { get; set; }

            public string TOUCHPANELNO { get; set; }

			public string MESSAGEID { get; set; }

			public string COMMAND { get; set; }

			public string MESSAGETEXT { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				OPERATORID = string.Empty;
				EQUIPMENTNO = string.Empty;
                TOUCHPANELNO = string.Empty;
				MESSAGEID = string.Empty;
				COMMAND = string.Empty;
				MESSAGETEXT = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public CIMMessageCommandRequest()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
			this.WaitReply = "CIMMessageCommandReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
