using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class ClientDisconnectRequest : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string USERID { get; set; }

			public string LOGINSERVERIP { get; set; }

			public string USERGROUP { get; set; }

			public string LOGINTIME { get; set; }

			public string OPERATORID { get; set; }

			public string REASON { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				USERID = string.Empty;
				LOGINSERVERIP = string.Empty;
				USERGROUP = string.Empty;
				LOGINTIME = string.Empty;
				OPERATORID = string.Empty;
				REASON = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public ClientDisconnectRequest()
		{
			this.Direction = Spec.DirType.BC_TO_OPI;
			this.WaitReply = "ClientDisconnectReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
