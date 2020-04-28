using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class AreYouThereRequest : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string FACTORYTYPE { get; set; }

			public string USERID { get; set; }

			public string USERGROUP { get; set; }

			public string LOGINTIME { get; set; }

			public string LOGINSERVERIP { get; set; }

			public string LOGINSERVERNAME { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				FACTORYTYPE = string.Empty;
				USERID = string.Empty;
				USERGROUP = string.Empty;
				LOGINTIME = string.Empty;
				LOGINSERVERIP = string.Empty;
				LOGINSERVERNAME = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public AreYouThereRequest()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
			this.WaitReply = "AreYouThereReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
