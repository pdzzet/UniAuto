using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class CFShortCutPermitRequest : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string PRODUCTNAME { get; set; }

			public string HOSTPRODUCTNAME { get; set; }

			public string PROCESSLINENAME { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				PRODUCTNAME = string.Empty;
				HOSTPRODUCTNAME = string.Empty;
				PROCESSLINENAME = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public CFShortCutPermitRequest()
		{
			this.Direction = Spec.DirType.BC_TO_MES;
			this.WaitReply = "CFShortCutPermitReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
