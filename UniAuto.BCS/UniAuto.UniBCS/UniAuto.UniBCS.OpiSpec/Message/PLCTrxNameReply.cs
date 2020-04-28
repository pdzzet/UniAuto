using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class PLCTrxNameReply : Message
	{
		public class PLCTRXc
		{
			public string PLCTRXNAME { get; set; }

			public PLCTRXc()
			{
				PLCTRXNAME = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			[XmlArray("PLCTRXLIST")]
			[XmlArrayItem("PLCTRX")]
			public List<PLCTRXc> PLCTRXLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				PLCTRXLIST = new List<PLCTRXc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public PLCTrxNameReply()
		{
			this.Direction = Spec.DirType.BC_TO_OPI;
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
