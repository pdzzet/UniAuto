using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class ChangePVDMaterialLife : Message
	{
		public class CHAMBERc
		{
			public string MATERIALTYPE { get; set; }

			public string CHAMBERID { get; set; }

			public string QUANTITY { get; set; }

			public CHAMBERc()
			{
				MATERIALTYPE = string.Empty;
				CHAMBERID = string.Empty;
				QUANTITY = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string MACHINENAME { get; set; }

			public string PRODUCTNAME { get; set; }

			[XmlArray("CHAMBERLIST")]
			[XmlArrayItem("CHAMBER")]
			public List<CHAMBERc> CHAMBERLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				MACHINENAME = string.Empty;
				PRODUCTNAME = string.Empty;
				CHAMBERLIST = new List<CHAMBERc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public ChangePVDMaterialLife()
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
