using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.BcSocketSpec
{
	[XmlRoot("MESSAGE")]
	public class RecipeIDRegisterCheckRequest : Message
	{
		public class MACHINEc
		{
			public string MACHINENAME { get; set; }

			public string LOCALNAME { get; set; }

			public string RECIPEID { get; set; }

			public MACHINEc()
			{
				MACHINENAME = string.Empty;
				LOCALNAME = string.Empty;
				RECIPEID = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string SOURCELINENAME { get; set; }

			public string DESTINATIONLINENAME { get; set; }

			public string LINERECIPENAME { get; set; }

			[XmlArray("MACHINELIST")]
			[XmlArrayItem("MACHINE")]
			public List<MACHINEc> MACHINELIST { get; set; }

			public TrxBody()
			{
				SOURCELINENAME = string.Empty;
				DESTINATIONLINENAME = string.Empty;
				LINERECIPENAME = string.Empty;
				MACHINELIST = new List<MACHINEc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public RecipeIDRegisterCheckRequest()
		{
			this.Direction = Spec.DirType.ACTIVE_TO_PASSIVE;
			this.WaitReply = "RecipeIDRegisterCheckReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
