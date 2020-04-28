using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.BcSocketSpec
{
	[XmlRoot("MESSAGE")]
	public class RecipeIDRegisterCheckReply : Message
	{
		public class MACHINEc
		{
			public string MACHINENAME { get; set; }

			public string LOCALNAME { get; set; }

			public string RECIPEID { get; set; }

			[XmlIgnore]
			public bool VALIRESULTbool { get; set; }

			public string VALIRESULT
			{
				get{ return VALIRESULTbool ? "Y" : "N"; }
				set{ VALIRESULTbool = (string.Compare(value, "Y", true) == 0); }
			}

			public MACHINEc()
			{
				MACHINENAME = string.Empty;
				LOCALNAME = string.Empty;
				RECIPEID = string.Empty;
				VALIRESULTbool = false;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string LINERECIPENAME { get; set; }

			[XmlArray("MACHINELIST")]
			[XmlArrayItem("MACHINE")]
			public List<MACHINEc> MACHINELIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				LINERECIPENAME = string.Empty;
				MACHINELIST = new List<MACHINEc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public RecipeIDRegisterCheckReply()
		{
			this.Direction = Spec.DirType.PASSIVE_TO_ACTIVE;
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
