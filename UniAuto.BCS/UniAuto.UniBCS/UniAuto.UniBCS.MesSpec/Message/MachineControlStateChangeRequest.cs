using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class MachineControlStateChangeRequest : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string CONTROLSTATENAME { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				CONTROLSTATENAME = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public MachineControlStateChangeRequest()
		{
			this.Direction = Spec.DirType.MES_TO_BC;
			this.WaitReply = "MachineControlStateChangeReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
