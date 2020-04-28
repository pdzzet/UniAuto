using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class POLAttachComplete : Message
	{
		public class MATERIALc
		{
			public string MATERIALTYPE { get; set; }

			public string POSITION { get; set; }

			public string CARTNAME { get; set; }

			public string PARTNO { get; set; }

			public string MATERIALNAME { get; set; }

            public string ISRTP { get; set; }

			public MATERIALc()
			{
				MATERIALTYPE = string.Empty;
				POSITION = string.Empty;
				CARTNAME = string.Empty;
				PARTNO = string.Empty;
				MATERIALNAME = string.Empty;
                ISRTP = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string MACHINENAME { get; set; }

			public string PRODUCTNAME { get; set; }

			public string HOSTPRODUCTNAME { get; set; }

			[XmlArray("MATERIALLIST")]
			[XmlArrayItem("MATERIAL")]
			public List<MATERIALc> MATERIALLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				MACHINENAME = string.Empty;
				PRODUCTNAME = string.Empty;
				HOSTPRODUCTNAME = string.Empty;
				MATERIALLIST = new List<MATERIALc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public POLAttachComplete()
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
