using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class PortOperModeChangeRequest : Message
	{
		public class PORTc
		{
			public string PORTNAME { get; set; }

			public string PORTOPERMODE { get; set; }

			public PORTc()
			{
				PORTNAME = string.Empty;
				PORTOPERMODE = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			[XmlArray("PORTLIST")]
			[XmlArrayItem("PORT")]
			public List<PORTc> PORTLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				PORTLIST = new List<PORTc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public PortOperModeChangeRequest()
		{
			this.Direction = Spec.DirType.BC_TO_MES;
			this.WaitReply = "PortOperModeChangeReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
