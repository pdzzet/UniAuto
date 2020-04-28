using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class RecipeIDRegisterCheckRequest : Message
	{
		public class MACHINEc
		{
			public string MACHINENAME { get; set; }

			public string LOCALNAME { get; set; }

			public string RECIPEID { get; set; }

			public string EVENTCOMMENT { get; set; }

			public MACHINEc()
			{
				MACHINENAME = string.Empty;
				LOCALNAME = string.Empty;
				RECIPEID = string.Empty;
				EVENTCOMMENT = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string LINERECIPENAME { get; set; }

			public string ACTIONTYPE { get; set; }

			[XmlArray("MACHINELIST")]
			[XmlArrayItem("MACHINE")]
			public List<MACHINEc> MACHINELIST { get; set; }

			public string EVENTUSER { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				LINERECIPENAME = string.Empty;
				ACTIONTYPE = string.Empty;
				MACHINELIST = new List<MACHINEc>();
				EVENTUSER = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public RecipeIDRegisterCheckRequest()
		{
			this.Direction = Spec.DirType.MES_TO_BC;
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
