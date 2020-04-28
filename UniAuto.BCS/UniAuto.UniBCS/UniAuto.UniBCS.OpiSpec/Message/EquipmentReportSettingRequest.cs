using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class EquipmentReportSettingRequest : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string EQUIPMENTNO { get; set; }

			public string REPORTTYPE { get; set; }

			public string REPORTENABLE { get; set; }

			public string REPORTTIME { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				EQUIPMENTNO = string.Empty;
				REPORTTYPE = string.Empty;
				REPORTENABLE = string.Empty;
				REPORTTIME = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public EquipmentReportSettingRequest()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
			this.WaitReply = "EquipmentReportSettingRequestReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
