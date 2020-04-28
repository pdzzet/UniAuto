using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class FacilityParameterRequest : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public FacilityParameterRequest()
		{
			this.Direction = Spec.DirType.MES_TO_BC;
			this.WaitReply = "FacilityParameterReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
