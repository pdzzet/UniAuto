using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class RecipeParameterReportRequest : Message
	{
		public class EQUIPMENTc
		{
			public string EQUIPMENTNO { get; set; }

			public string RECIPENO { get; set; }

			public EQUIPMENTc()
			{
				EQUIPMENTNO = string.Empty;
				RECIPENO = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string RECIPELINENAME { get; set; }

			[XmlArray("EQUIPMENTLIST")]
			[XmlArrayItem("EQUIPMENT")]
			public List<EQUIPMENTc> EQUIPMENTLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				RECIPELINENAME = string.Empty;
				EQUIPMENTLIST = new List<EQUIPMENTc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public RecipeParameterReportRequest()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
			this.WaitReply = "RecipeParameterReportRequestReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
