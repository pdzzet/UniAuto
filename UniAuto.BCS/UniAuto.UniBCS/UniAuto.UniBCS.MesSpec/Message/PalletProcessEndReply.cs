using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class PalletProcessEndReply : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string PALLETNAME { get; set; }

			public string BOXQUANTITY { get; set; }

			[XmlArray("BOXLIST")]
			[XmlArrayItem("BOXNAME")]
			public List<string> BOXLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				PALLETNAME = string.Empty;
				BOXQUANTITY = string.Empty;
				BOXLIST = new List<string>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public PalletProcessEndReply()
		{
			this.Direction = Spec.DirType.MES_TO_BC;
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
