using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class ForceCleanOutCommand : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string COMMAND { get; set; }

			public string STATUS { get; set; }

            public string USERID { get; set; }
            
			public TrxBody()
			{
				LINENAME = string.Empty;
				COMMAND = string.Empty;
				STATUS = string.Empty;
                USERID = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public ForceCleanOutCommand()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
			this.WaitReply = "ForceCleanOutCommandReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
