using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class ChangePlanRequest : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string CURRENTPLANNAME { get; set; }

			public string CARRIERNAME { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				CURRENTPLANNAME = string.Empty;
				CARRIERNAME = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public ChangePlanRequest()
		{
			this.Direction = Spec.DirType.BC_TO_MES;
			this.WaitReply = "ChangePlanReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
