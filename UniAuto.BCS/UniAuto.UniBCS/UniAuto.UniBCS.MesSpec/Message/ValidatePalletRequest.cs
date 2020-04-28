using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class ValidatePalletRequest : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string PORTNAME { get; set; }

			public string PALLETNAME { get; set; }

			public string BOXQUANTITY { get; set; }

			public string PORTMODE { get; set; }

			[XmlArray("BOXLIST")]
			[XmlArrayItem("BOXNAME")]
			public List<string> BOXLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				PORTNAME = string.Empty;
				PALLETNAME = string.Empty;
				BOXQUANTITY = string.Empty;
				PORTMODE = string.Empty;
				BOXLIST = new List<string>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public ValidatePalletRequest()
		{
			this.Direction = Spec.DirType.BC_TO_MES;
			this.WaitReply = "ValidatePalletReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
