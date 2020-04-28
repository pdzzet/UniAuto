using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class DisconnectUsersRequest : Message
	{
		public class USERc
		{
			public string USERID { get; set; }

			public string LOGINSERVERIP { get; set; }

			public string USERGROUP { get; set; }

			public string LOGINTIME { get; set; }

			public USERc()
			{
				USERID = string.Empty;
				LOGINSERVERIP = string.Empty;
				USERGROUP = string.Empty;
				LOGINTIME = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			[XmlArray("USERLIST")]
			[XmlArrayItem("USER")]
			public List<USERc> USERLIST { get; set; }

			public string OPERATORID { get; set; }

			public string REASON { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				USERLIST = new List<USERc>();
				OPERATORID = string.Empty;
				REASON = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public DisconnectUsersRequest()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
			this.WaitReply = "DisconnectUsersReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
