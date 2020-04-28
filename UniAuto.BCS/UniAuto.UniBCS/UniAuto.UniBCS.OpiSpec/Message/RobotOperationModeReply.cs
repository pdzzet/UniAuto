using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class RobotOperationModeReply : Message
	{
		public class ROBOTPOSITIONc
		{
			public string ROBOTPOSITIONNO { get; set; }

			public string OPERATIONMODE { get; set; }

			public ROBOTPOSITIONc()
			{
				ROBOTPOSITIONNO = string.Empty;
				OPERATIONMODE = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string EQUIPMENTNO { get; set; }

			[XmlArray("ROBOTPOSITIONLIST")]
			[XmlArrayItem("ROBOTPOSITION")]
			public List<ROBOTPOSITIONc> ROBOTPOSITIONLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				EQUIPMENTNO = string.Empty;
				ROBOTPOSITIONLIST = new List<ROBOTPOSITIONc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public RobotOperationModeReply()
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
