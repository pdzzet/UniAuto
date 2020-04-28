using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class MachinePauseCommandReply : Message
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

			public string EXERESULT { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				MACHINENAME = string.Empty;
				PAUSECOMMANDbool = false;
				EXERESULT = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public MachinePauseCommandReply()
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
