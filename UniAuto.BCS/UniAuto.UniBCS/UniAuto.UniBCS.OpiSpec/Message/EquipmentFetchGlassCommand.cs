using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class EquipmentFetchGlassCommand : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string EQUIPMENTNO { get; set; }

			public string RULENAME1 { get; set; }

			public string RULEVALUE1 { get; set; }

			public string RULENAME2 { get; set; }

			public string RULEVALUE2 { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				EQUIPMENTNO = string.Empty;
				RULENAME1 = string.Empty;
				RULEVALUE1 = string.Empty;
				RULENAME2 = string.Empty;
				RULEVALUE2 = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public EquipmentFetchGlassCommand()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
			this.WaitReply = "EquipmentFetchGlassCommandReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
