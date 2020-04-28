using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class POLStateChangedReply : Message
	{
		public class MATERIALc
		{
			public string POSITION { get; set; }

			public string MATERIALNAME { get; set; }

			public string COUNT { get; set; }

			public string LIFEQTIME { get; set; }

			public MATERIALc()
			{
				POSITION = string.Empty;
				MATERIALNAME = string.Empty;
				COUNT = string.Empty;
				LIFEQTIME = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string MACHINENAME { get; set; }

			public string LINERECIPENAME { get; set; }

			public string MATERIALSTATE { get; set; }

			public string MATERIALTYPE { get; set; }

			public string CARTNAME { get; set; }

			public string PARTNO { get; set; }

			[XmlArray("MATERIALLIST")]
			[XmlArrayItem("MATERIAL")]
			public List<MATERIALc> MATERIALLIST { get; set; }

			[XmlIgnore]
			public bool VALIRESULTbool { get; set; }

			public string VALIRESULT
			{
				get{ return VALIRESULTbool ? "Y" : "N"; }
				set{ VALIRESULTbool = (string.Compare(value, "Y", true) == 0); }
			}

			public TrxBody()
			{
				LINENAME = string.Empty;
				MACHINENAME = string.Empty;
				LINERECIPENAME = string.Empty;
				MATERIALSTATE = string.Empty;
				MATERIALTYPE = string.Empty;
				CARTNAME = string.Empty;
				PARTNO = string.Empty;
				MATERIALLIST = new List<MATERIALc>();
				VALIRESULTbool = false;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public POLStateChangedReply()
		{
			this.Direction = Spec.DirType.MES_TO_BC;
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
