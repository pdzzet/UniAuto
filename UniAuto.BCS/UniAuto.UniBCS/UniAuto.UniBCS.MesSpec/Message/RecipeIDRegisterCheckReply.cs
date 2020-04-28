using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class RecipeIDRegisterCheckReply : Message
	{
		public class MACHINEc
		{
			public string MACHINENAME { get; set; }

			public string LOCALNAME { get; set; }

			public string RECIPEID { get; set; }

			public string EVENTCOMMENT { get; set; }

			[XmlIgnore]
			public bool VALIRESULTbool { get; set; }

			public string VALIRESULT
			{
				get{ return VALIRESULTbool ? "Y" : "N"; }
				set{ VALIRESULTbool = (string.Compare(value, "Y", true) == 0); }
			}

			public string VALICODE { get; set; }

			public MACHINEc()
			{
				MACHINENAME = string.Empty;
				LOCALNAME = string.Empty;
				RECIPEID = string.Empty;
				EVENTCOMMENT = string.Empty;
				VALIRESULTbool = false;
				VALICODE = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string LINERECIPENAME { get; set; }

			public string ACTIONTYPE { get; set; }

			[XmlIgnore]
			public bool VALIRESULTbool { get; set; }

			public string VALIRESULT
			{
				get{ return VALIRESULTbool ? "Y" : "N"; }
				set{ VALIRESULTbool = (string.Compare(value, "Y", true) == 0); }
			}

			public string VALICODE { get; set; }

			[XmlArray("MACHINELIST")]
			[XmlArrayItem("MACHINE")]
			public List<MACHINEc> MACHINELIST { get; set; }

			public string EVENTUSER { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				LINERECIPENAME = string.Empty;
				ACTIONTYPE = string.Empty;
				VALIRESULTbool = false;
				VALICODE = string.Empty;
				MACHINELIST = new List<MACHINEc>();
				EVENTUSER = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public RecipeIDRegisterCheckReply()
		{
			this.Direction = Spec.DirType.BC_TO_MES;
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
