using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class SlotPositionReply : Message
	{
		public class POSITIONc
		{
			public string POSITIONNO { get; set; }

			public string CASSETTESEQNO { get; set; }

			public string JOBSEQNO { get; set; }

			public string JOBID { get; set; }

			public string TRACKINGVALUE { get; set; }

			public string SAMPLINGSLOTFLAG { get; set; }

            public string RECIPENAME { get; set; }

            public string PPID { get; set; }

            public string EQPRTCFLAG { get; set; } //add by yang

			public POSITIONc()
			{
				POSITIONNO = string.Empty;
				CASSETTESEQNO = string.Empty;
				JOBSEQNO = string.Empty;
				JOBID = string.Empty;
				TRACKINGVALUE = string.Empty;
				SAMPLINGSLOTFLAG = string.Empty;
                RECIPENAME = string.Empty;
                PPID = string.Empty;
                EQPRTCFLAG = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string EQUIPMENTNO { get; set; }

			public string PORTNO { get; set; }

			public string PORTID { get; set; }

			[XmlArray("POSITIONLIST")]
			[XmlArrayItem("POSITION")]
			public List<POSITIONc> POSITIONLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				EQUIPMENTNO = string.Empty;
				PORTNO = string.Empty;
				PORTID = string.Empty;
				POSITIONLIST = new List<POSITIONc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public SlotPositionReply()
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
