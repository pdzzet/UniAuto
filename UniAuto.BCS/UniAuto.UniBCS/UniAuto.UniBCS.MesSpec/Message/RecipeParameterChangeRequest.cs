using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class RecipeParameterChangeRequest : Message
	{
		public class PARAc
		{
			public string TRACELEVEL { get; set; }

			public string PARANAME { get; set; }

			public string PARANAMEORIENTED { get; set; }

			public string PARAVALUE { get; set; }

			public PARAc()
			{
				TRACELEVEL = string.Empty;
				PARANAME = string.Empty;
				PARANAMEORIENTED = string.Empty;
				PARAVALUE = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string MACHINENAME { get; set; }

			public string RECIPEID { get; set; }

			[XmlArray("RECIPEPARALIST")]
			[XmlArrayItem("PARA")]
			public List<PARAc> RECIPEPARALIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				MACHINENAME = string.Empty;
				RECIPEID = string.Empty;
				RECIPEPARALIST = new List<PARAc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public RecipeParameterChangeRequest()
		{
			this.Direction = Spec.DirType.BC_TO_MES;
			this.WaitReply = "RecipeParameterChangeReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
