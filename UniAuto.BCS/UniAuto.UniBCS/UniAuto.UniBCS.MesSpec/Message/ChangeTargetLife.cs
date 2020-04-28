using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class ChangeTargetLife : Message
	{
		public class CHAMBERc
		{
			public string CHAMBERID { get; set; }

			public string QUANTITY { get; set; }

			public string AVERAGE { get; set; }

			public CHAMBERc()
			{
				CHAMBERID = string.Empty;
				QUANTITY = string.Empty;
				AVERAGE = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string MACHINENAME { get; set; }

			public string PRODUCTNAME { get; set; }

			public string MATERIALTYPE { get; set; }

			[XmlArray("CHAMBERLIST")]
			[XmlArrayItem("CHAMBER")]
			public List<CHAMBERc> CHAMBERLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				MACHINENAME = string.Empty;
				PRODUCTNAME = string.Empty;
				MATERIALTYPE = string.Empty;
				CHAMBERLIST = new List<CHAMBERc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public ChangeTargetLife()
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
