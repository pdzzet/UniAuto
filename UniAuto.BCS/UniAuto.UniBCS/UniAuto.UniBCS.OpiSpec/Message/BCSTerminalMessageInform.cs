using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class BCSTerminalMessageInform : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

            public string DATETIME { get; set; }

			public string CAPTION { get; set; }

			public string TERMINALTEXT { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				DATETIME = string.Empty;
				CAPTION = string.Empty;
				TERMINALTEXT = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public BCSTerminalMessageInform()
		{
			this.Direction = Spec.DirType.BC_TO_OPI;
            this.WaitReply = ""; // "BCSTerminalMessageInformReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
