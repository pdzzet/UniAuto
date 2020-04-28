using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class CheckBoxNameRequest : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string PALLETNAME { get; set; }

			public string BOXNAME { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				PALLETNAME = string.Empty;
				BOXNAME = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public CheckBoxNameRequest()
		{
			this.Direction = Spec.DirType.BC_TO_MES;
			this.WaitReply = "CheckBoxNameReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
