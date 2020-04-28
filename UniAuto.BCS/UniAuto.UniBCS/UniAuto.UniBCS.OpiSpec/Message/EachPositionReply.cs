using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class EachPositionReply : Message
	{
		public class POSITIONc
		{
            //public string UNITNO { get; set; }

			public string POSITIONNO { get; set; }

			public string CASSETTESEQNO { get; set; }

			public string JOBSEQNO { get; set; }

            public string JOBID { get; set; }

			public POSITIONc()
			{
                //UNITNO = string.Empty;
				POSITIONNO = string.Empty;
				CASSETTESEQNO = string.Empty;
				JOBSEQNO = string.Empty;
                JOBID = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string EQUIPMENTNO { get; set; }

			public string REPORTPOSITIONNAME { get; set; }

			public string UNITNO { get; set; }

			public string PLCTRXNO { get; set; }

			[XmlArray("POSITIONLIST")]
			[XmlArrayItem("POSITION")]
			public List<POSITIONc> POSITIONLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				EQUIPMENTNO = string.Empty;
				REPORTPOSITIONNAME = string.Empty;
				UNITNO = string.Empty;
				PLCTRXNO = string.Empty;
				POSITIONLIST = new List<POSITIONc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public EachPositionReply()
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
