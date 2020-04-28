using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class MachinePauseCommandRequest : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string MACHINENAME { get; set; }

			[XmlIgnore]
			public bool PAUSECOMMANDbool { get; set; }

			public string PAUSECOMMAND
			{
				get{ return PAUSECOMMANDbool ? "Y" : "N"; }
				set{ PAUSECOMMANDbool = (string.Compare(value, "Y", true) == 0); }
			}

			public TrxBody()
			{
				LINENAME = string.Empty;
				MACHINENAME = string.Empty;
				PAUSECOMMANDbool = false;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public MachinePauseCommandRequest()
		{
			this.Direction = Spec.DirType.MES_TO_BC;
			this.WaitReply = "MachinePauseCommandReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
