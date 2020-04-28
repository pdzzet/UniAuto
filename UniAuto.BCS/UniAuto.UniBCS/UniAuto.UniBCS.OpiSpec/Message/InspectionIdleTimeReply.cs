using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class InspectionIdleTimeReply : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string OPERATORID { get; set; }

			public string EQUIPMENTNO { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				OPERATORID = string.Empty;
				EQUIPMENTNO = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public InspectionIdleTimeReply()
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
