using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class ValidateMaskByCarrierRequest : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string PORTNAME { get; set; }

			public string LINERECIPENAME { get; set; }

			public string MASKCARRIERNAME { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				PORTNAME = string.Empty;
				LINERECIPENAME = string.Empty;
				MASKCARRIERNAME = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public ValidateMaskByCarrierRequest()
		{
			this.Direction = Spec.DirType.BC_TO_MES;
			this.WaitReply = "ValidateMaskByCarrierReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
