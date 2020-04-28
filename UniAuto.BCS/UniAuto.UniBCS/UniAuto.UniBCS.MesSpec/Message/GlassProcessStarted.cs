using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class GlassProcessStarted : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string PRODUCTNAME { get; set; }

			public string HOSTPRODUCTNAME { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				PRODUCTNAME = string.Empty;
				HOSTPRODUCTNAME = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public GlassProcessStarted()
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
