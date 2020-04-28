using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class MaterialStateChanged : Message
	{
		public class MATERIALc
		{
			public string MATERIALTYPE { get; set; }

			public string MATERIALNAME { get; set; }

			public string MATERIALSTATE { get; set; }

			public string USEDCOUNT { get; set; }

			public string GROUPID { get; set; }

			public string UNITID { get; set; }

			public string HEADID { get; set; }

			public MATERIALc()
			{
				MATERIALTYPE = string.Empty;
				MATERIALNAME = string.Empty;
				MATERIALSTATE = string.Empty;
				USEDCOUNT = string.Empty;
				GROUPID = string.Empty;
				UNITID = string.Empty;
				HEADID = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string MACHINENAME { get; set; }

            public string RECIPEID { get; set; }

			public string MATERIALMODE { get; set; }

			public string PRODUCTNAME { get; set; }

			[XmlArray("MATERIALLIST")]
			[XmlArrayItem("MATERIAL")]
			public List<MATERIALc> MATERIALLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				MACHINENAME = string.Empty;
                RECIPEID = string.Empty;
				MATERIALMODE = string.Empty;
				PRODUCTNAME = string.Empty;
				MATERIALLIST = new List<MATERIALc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public MaterialStateChanged()
		{
			this.Direction = Spec.DirType.BC_TO_MES;
			this.WaitReply = "MaterialStateChangedReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
