using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class CIMMessageDataReply : Message
	{
		public class MESSAGEc
		{
			public string MESSAGEID { get; set; }

            public string TOUCHPANELNO { get; set; }

			public string MESSAGEDATETIME { get; set; }

			public string MESSAGETEXT { get; set; }

			public MESSAGEc()
			{
				MESSAGEID = string.Empty;
                TOUCHPANELNO = string.Empty;
				MESSAGEDATETIME = string.Empty;
				MESSAGETEXT = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string EQUIPMENTNO { get; set; }

			[XmlArray("MESSAGELIST")]
			[XmlArrayItem("MESSAGE")]
			public List<MESSAGEc> MESSAGELIST { get; set; }

			public string MESSAGEID { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				EQUIPMENTNO = string.Empty;
				MESSAGELIST = new List<MESSAGEc>();
				MESSAGEID = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public CIMMessageDataReply()
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
