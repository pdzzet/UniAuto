using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class RecipeParameterRequest : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string LINERECIPENAME { get; set; }

			public string PPID { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				LINERECIPENAME = string.Empty;
				PPID = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public RecipeParameterRequest()
		{
			this.Direction = Spec.DirType.MES_TO_BC;
			this.WaitReply = "RecipeParameterReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
