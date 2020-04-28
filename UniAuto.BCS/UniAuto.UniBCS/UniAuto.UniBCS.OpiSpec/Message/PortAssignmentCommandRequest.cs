using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class PortAssignmentCommandRequest : Message
    {
        public class ASSIGNMENTc
        {
            public string EQUIPMENTNO { get; set; }

            public string PORTNO { get; set; }

            public string PORTID { get; set; }

            public string ASSIGNMENT { get; set; }

            public ASSIGNMENTc()
            {
                EQUIPMENTNO = string.Empty;
                PORTNO = string.Empty;
                PORTID = string.Empty;
                ASSIGNMENT = string.Empty;
            }
        }

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

            [XmlArray("ASSIGNMENTLIST")]
            [XmlArrayItem("ASSIGNMENT")]
            public List<ASSIGNMENTc> ASSIGNMENTLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
                ASSIGNMENTLIST = new List<ASSIGNMENTc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public PortAssignmentCommandRequest()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
            this.WaitReply = "PortAssignmentCommandReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
    }
}
