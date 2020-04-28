using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class RobotMessageReport : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

            public string EQUIPMENTNO { get; set; }

            public string ROBOTNAME { get; set; }

            public string MSG_DATETIME { get; set; }

            public string MSG_TYPE { get; set; }

            public string MSG_DETAIL { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
                EQUIPMENTNO = string.Empty;
                ROBOTNAME = string.Empty;
                MSG_DATETIME = string.Empty;
                MSG_TYPE = string.Empty;
                MSG_DETAIL = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public RobotMessageReport()
		{
			this.Direction = Spec.DirType.BC_TO_OPI;
            this.WaitReply = ""; // "RobotCommandReportReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
