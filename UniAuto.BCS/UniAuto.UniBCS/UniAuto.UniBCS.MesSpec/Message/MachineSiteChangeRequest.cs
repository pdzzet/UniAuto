using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class MachineSiteChangeRequest : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string NEWSITE { get; set; }

			public string MACHINEENABLE { get; set; }

			public string EVENTUSER { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				NEWSITE = string.Empty;
				MACHINEENABLE = string.Empty;
				EVENTUSER = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public MachineSiteChangeRequest()
		{
			this.Direction = Spec.DirType.BC_TO_MES;
			this.WaitReply = "MachineSiteChangeReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
