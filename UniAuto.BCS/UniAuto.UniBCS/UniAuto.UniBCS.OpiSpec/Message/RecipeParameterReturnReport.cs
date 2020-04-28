using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class RecipeParameterReturnReport : Message
	{
		public class EQUIPMENTc
		{
			public string EQUIPMENTNO { get; set; }

			public string RECIPENO { get; set; }

			public string RETURN { get; set; }

			public string NGMESSAGE { get; set; }

			[XmlArray("DATALIST")]
			[XmlArrayItem("DATA")]
			public List<DATAc> DATALIST { get; set; }

			public EQUIPMENTc()
			{
				EQUIPMENTNO = string.Empty;
				RECIPENO = string.Empty;
				RETURN = string.Empty;
				NGMESSAGE = string.Empty;
				DATALIST = new List<DATAc>();
			}
		}

		public class DATAc
		{
			public string NO { get; set; }

			public string PARAMETERNAME { get; set; }

			public string VALUE { get; set; }

			public string EXPRESSION { get; set; }

			public DATAc()
			{
				NO = string.Empty;
				PARAMETERNAME = string.Empty;
				VALUE = string.Empty;
				EXPRESSION = string.Empty;
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

		public RecipeParameterReturnReport()
		{
			this.Direction = Spec.DirType.BC_TO_OPI;
            this.WaitReply = ""; // "RecipeParameterReturnReportReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
