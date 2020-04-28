using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class RobotFetchSequenceModeChangeRequest : Message
    {
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string ROBOT_FETCH_SEQUENCE_MODE { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				ROBOT_FETCH_SEQUENCE_MODE = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public RobotFetchSequenceModeChangeRequest()
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
