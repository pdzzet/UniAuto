using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class CheckRecipeParameter : Message
	{
		public class LINEc
		{
			public string LINENAME { get; set; }

			public string PORTNAME { get; set; }

			public string CARRIERNAME { get; set; }

			[XmlArray("TIMEOUTEQPLIST")]
			[XmlArrayItem("MACHINENAME")]
			public List<string> TIMEOUTEQPLIST { get; set; }

			[XmlArray("RECIPEPARANOCHECKLIST")]
			[XmlArrayItem("MACHINENAME")]
			public List<string> RECIPEPARANOCHECKLIST { get; set; }

			[XmlArray("CIMOFFEQPLIST")]
			[XmlArrayItem("MACHINENAME")]
			public List<string> CIMOFFEQPLIST { get; set; }

			[XmlArray("LINERECIPENAMELIST")]
			[XmlArrayItem("LINERECIPE")]
			public List<LINERECIPEc> LINERECIPENAMELIST { get; set; }

			public LINEc()
			{
				LINENAME = string.Empty;
				PORTNAME = string.Empty;
				CARRIERNAME = string.Empty;
				TIMEOUTEQPLIST = new List<string>();
				RECIPEPARANOCHECKLIST = new List<string>();
				CIMOFFEQPLIST = new List<string>();
				LINERECIPENAMELIST = new List<LINERECIPEc>();
			}
		}

		public class LINERECIPEc
		{
			public string LINERECIPENAME { get; set; }

			[XmlArray("MACHINELIST")]
			[XmlArrayItem("MACHINE")]
			public List<MACHINEc> MACHINELIST { get; set; }

			public LINERECIPEc()
			{
				LINERECIPENAME = string.Empty;
				MACHINELIST = new List<MACHINEc>();
			}
		}

		public class MACHINEc
		{
			public string MACHINENAME { get; set; }

			public string RECIPEID { get; set; }

			[XmlArray("RECIPEPARALIST")]
			[XmlArrayItem("PARA")]
			public List<PARAc> RECIPEPARALIST { get; set; }

			public MACHINEc()
			{
				MACHINENAME = string.Empty;
				RECIPEID = string.Empty;
				RECIPEPARALIST = new List<PARAc>();
			}
		}

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
			[XmlArray("LINELIST")]
			[XmlArrayItem("LINE")]
			public List<LINEc> LINELIST { get; set; }

			public TrxBody()
			{
				LINELIST = new List<LINEc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public CheckRecipeParameter()
		{
			this.Direction = Spec.DirType.BC_TO_MES;
			this.WaitReply = "CheckRecipeParameterReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
