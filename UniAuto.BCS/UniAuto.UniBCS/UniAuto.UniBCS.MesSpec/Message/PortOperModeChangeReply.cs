using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class PortOperModeChangeReply : Message
	{
		public class PORTc
		{
			public string PORTNAME { get; set; }

			public string PORTOPERMODE { get; set; }

			[XmlIgnore]
			public bool VALIRESULTbool { get; set; }

			public string VALIRESULT
			{
				get{ return VALIRESULTbool ? "Y" : "N"; }
				set{ VALIRESULTbool = (string.Compare(value, "Y", true) == 0); }
			}

			public PORTc()
			{
				PORTNAME = string.Empty;
				PORTOPERMODE = string.Empty;
				VALIRESULTbool = false;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			[XmlArray("PORTLIST")]
			[XmlArrayItem("PORT")]
			public List<PORTc> PORTLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				PORTLIST = new List<PORTc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public PortOperModeChangeReply()
		{
			this.Direction = Spec.DirType.MES_TO_BC;
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
