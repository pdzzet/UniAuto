using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class BoxProcessLineRequest : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string PORTNAME { get; set; }

			public string BOXNAME { get; set; }

			public string PROCESSLINE { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				PORTNAME = string.Empty;
				BOXNAME = string.Empty;
				PROCESSLINE = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public BoxProcessLineRequest()
		{
			this.Direction = Spec.DirType.BC_TO_MES;
			this.WaitReply = "BoxProcessLineReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
