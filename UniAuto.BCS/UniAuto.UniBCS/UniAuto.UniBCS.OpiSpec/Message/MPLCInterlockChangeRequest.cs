using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class MPLCInterlockChangeRequest : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string EQUIPMENTNO { get; set; }

			public string MPLCINTERLOCKNO { get; set; }

			public string MPLCINTERLOCK { get; set; }

			public string PLCTRX { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				EQUIPMENTNO = string.Empty;
				MPLCINTERLOCKNO = string.Empty;
				MPLCINTERLOCK = string.Empty;
				PLCTRX = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public MPLCInterlockChangeRequest()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
			this.WaitReply = "MPLCInterlockChangeReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
