using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.BcSocketSpec
{
	[XmlRoot("MESSAGE")]
	public class JOBShortCutPermitReply : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public JOBShortCutPermitReply()
		{
			this.Direction = Spec.DirType.PASSIVE_TO_ACTIVE;
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
