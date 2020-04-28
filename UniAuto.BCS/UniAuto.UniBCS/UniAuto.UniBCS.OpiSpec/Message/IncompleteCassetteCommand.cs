using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class IncompleteCassetteCommand : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string OPERATORID { get; set; }

			public string INCOMPLETEDATE { get; set; }

			public string PORTID { get; set; }

			public string CASSETTEID { get; set; }

			public string MESTRXID { get; set; }

			public string FILENAME { get; set; }

			public string COMMAND { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				OPERATORID = string.Empty;
				INCOMPLETEDATE = string.Empty;
				PORTID = string.Empty;
				CASSETTEID = string.Empty;
				MESTRXID = string.Empty;
				FILENAME = string.Empty;
				COMMAND = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public IncompleteCassetteCommand()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
			this.WaitReply = "IncompleteCassetteCommandReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
