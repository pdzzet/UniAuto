using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class POLStateChanged : Message
	{
		public class MATERIALc
		{
			public string POSITION { get; set; }

			public string MATERIALNAME { get; set; }

            public string LOTID { get; set; }

            public string LOTNO { get; set; }

			public string COUNT { get; set; }

			public MATERIALc()
			{
                POSITION = string.Empty;
                MATERIALNAME = string.Empty;
                LOTID = string.Empty;
                LOTNO = string.Empty;
				COUNT = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string MACHINENAME { get; set; }

            public string RECIPEID { get; set; }

			public string MATERIALSTATE { get; set; }

			public string MATERIALTYPE { get; set; }

			public string CARTNAME { get; set; }

			public string PARTNO { get; set; }

            public string ISRTP { get; set; }

			[XmlArray("MATERIALLIST")]
			[XmlArrayItem("MATERIAL")]
			public List<MATERIALc> MATERIALLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				MACHINENAME = string.Empty;
                RECIPEID = string.Empty;
				MATERIALSTATE = string.Empty;
				MATERIALTYPE = string.Empty;
				CARTNAME = string.Empty;
				PARTNO = string.Empty;
                ISRTP = string.Empty;
				MATERIALLIST = new List<MATERIALc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public POLStateChanged()
		{
			this.Direction = Spec.DirType.BC_TO_MES;
			this.WaitReply = "POLStateChangedReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
