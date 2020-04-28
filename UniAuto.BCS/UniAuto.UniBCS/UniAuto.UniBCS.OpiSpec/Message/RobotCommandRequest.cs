using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class RobotCommandRequest : Message 
	{
        public class COMMANDc
        {
            public string COMMAND_SEQ { get; set; }

            public string ROBOT_COMMAND { get; set; }

            public string ARM_SELECT { get; set; }

            public string TARGETPOSITION { get; set; }

            public string TARGETSLOTNO { get; set; }

            public COMMANDc()
            {
                COMMAND_SEQ = string.Empty;
                ROBOT_COMMAND = string.Empty;
                ARM_SELECT = string.Empty;
                TARGETPOSITION = string.Empty;
                TARGETSLOTNO = string.Empty;
            }
        }

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string EQUIPMENTNO { get; set; }

			public string ROBOTNAME { get; set; }

            [XmlArray("COMMANDLIST")]
            [XmlArrayItem("COMMAND")]
            public List<COMMANDc> COMMANDLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				EQUIPMENTNO = string.Empty;
				ROBOTNAME = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public RobotCommandRequest()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
            this.WaitReply = "RobotCommandReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
