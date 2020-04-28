using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class PortDataUpdate : Message
	{
		public class PORTc
		{
			public string PORTNAME { get; set; }

			public string PORTTYPE { get; set; }

			public string PORTUSETYPE { get; set; }

			public string PORTACCESSMODE { get; set; }

			public string PORTTRANSFERSTATE { get; set; }

			public string CARRIERNAME { get; set; }

			public string PORTENABLEFLAG { get; set; }

			public string PORTOPERMODE { get; set; }

			public string CARRIERSETCODE { get; set; }

			public PORTc()
			{
				PORTNAME = string.Empty;
				PORTTYPE = string.Empty;
				PORTUSETYPE = string.Empty;
				PORTACCESSMODE = string.Empty;
				PORTTRANSFERSTATE = string.Empty;
				CARRIERNAME = string.Empty;
				PORTENABLEFLAG = string.Empty;
				PORTOPERMODE = string.Empty;
				CARRIERSETCODE = string.Empty;
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

		public PortDataUpdate()
		{
			this.Direction = Spec.DirType.BC_TO_MES;
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
