using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class RecipeRegisterValidationCommandRequest : Message
	{
		public class LINEc
		{
			public string RECIPELINENAME { get; set; }

			[XmlArray("RECIPECHECKLIST")]
			[XmlArrayItem("RECIPECHECK")]
			public List<RECIPECHECKc> RECIPECHECKLIST { get; set; }

			public LINEc()
			{
				RECIPELINENAME = string.Empty;
				RECIPECHECKLIST = new List<RECIPECHECKc>();
			}
		}

		public class RECIPECHECKc
		{
			public string RECIPENAME { get; set; }

			[XmlArray("EQUIPMENTLIST")]
			[XmlArrayItem("EQUIPMENT")]
			public List<EQUIPMENTc> EQUIPMENTLIST { get; set; }

			public RECIPECHECKc()
			{
				RECIPENAME = string.Empty;
				EQUIPMENTLIST = new List<EQUIPMENTc>();
			}
		}

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

			public string PORTNO { get; set; }

			[XmlArray("LINELIST")]
			[XmlArrayItem("LINE")]
			public List<LINEc> LINELIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				PORTNO = string.Empty;
				LINELIST = new List<LINEc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public RecipeRegisterValidationCommandRequest()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
			this.WaitReply = "RecipeRegisterValidationCommandReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
