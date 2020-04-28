using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class LineModeChangeRequest : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

            public string LINEID { get; set; }

			public string OPERATORID { get; set; }

			public string LINEMODE { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
                LINEID = string.Empty;
				OPERATORID = string.Empty;
				LINEMODE = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public LineModeChangeRequest()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
			this.WaitReply = "LineModeChangeReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
