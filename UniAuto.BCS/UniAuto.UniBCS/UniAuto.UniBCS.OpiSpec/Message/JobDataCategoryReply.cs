using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class JobDataCategoryReply : Message
	{
		public class EQUIPMENTITEMc
		{
			public string EQUIPMENTNO { get; set; }

            public string UNITNO { get; set; }

            public string PORTNO { get; set; }

			[XmlArray("JOBDATALIST")]
			[XmlArrayItem("JOB")]
			public List<JOBc> JOBDATALIST { get; set; }

			public EQUIPMENTITEMc()
			{
				EQUIPMENTNO = string.Empty;
                UNITNO = string.Empty;
                PORTNO = string.Empty;
				JOBDATALIST = new List<JOBc>();
			}
		}

		public class JOBc
		{
			public string CASSETTESEQNO { get; set; }

			public string SLOTNO { get; set; }

			public string JOBSEQNO { get; set; }

			public string PRODUCTTYPE { get; set; }

			public string JOBTYPE { get; set; }

			public string JOBJUDGE { get; set; }

			public string JOBGRADE { get; set; }

			public string GLASSID { get; set; }

			public string PPID { get; set; }

			public string TRACKINGDATA { get; set; }

            public string SAMPLINGFLAG { get; set; }

            public string PRODUCTSPECVER { get; set; }

            public string OWNERID { get; set; }

            public string PROCESSOPERATIONNAME { get; set; }

            public string LINERECIPENAME { get; set; }

            //public string OXR { get; set; }

			public JOBc()
			{
				CASSETTESEQNO = string.Empty;
				SLOTNO = string.Empty;
				JOBSEQNO = string.Empty;
				PRODUCTTYPE = string.Empty;
				JOBTYPE = string.Empty;
				JOBJUDGE = string.Empty;
				JOBGRADE = string.Empty;
				GLASSID = string.Empty;
				PPID = string.Empty;
				TRACKINGDATA = string.Empty;
                SAMPLINGFLAG = string.Empty;
                PRODUCTSPECVER = string.Empty;
                OWNERID = string.Empty;
                PROCESSOPERATIONNAME = string.Empty;
                LINERECIPENAME = string.Empty;
                //OXR = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

            public string REMOVEFLAG { get; set; }

			[XmlArray("EQUIPMENTLIST")]
			[XmlArrayItem("EQUIPMENTITEM")]
			public List<EQUIPMENTITEMc> EQUIPMENTLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
                REMOVEFLAG = string.Empty;
				EQUIPMENTLIST = new List<EQUIPMENTITEMc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public JobDataCategoryReply()
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
