using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class ClientInformationReply : Message
	{
		public class CLIENTc
		{
			public string USERID { get; set; }

			public string USERGROUP { get; set; }

			public string LOGINTIME { get; set; }

			public string LOGINSERVERIP { get; set; }

			public string LOGINSERVERNAME { get; set; }

			public CLIENTc()
			{
				USERID = string.Empty;
				USERGROUP = string.Empty;
				LOGINTIME = string.Empty;
				LOGINSERVERIP = string.Empty;
				LOGINSERVERNAME = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			[XmlArray("CLIENTLIST")]
			[XmlArrayItem("CLIENT")]
			public List<CLIENTc> CLIENTLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				CLIENTLIST = new List<CLIENTc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public ClientInformationReply()
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
