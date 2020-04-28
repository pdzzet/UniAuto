using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class JobDataReply : Message
	{
        public class CSTCONTROLc
        {
            public string SOURCECST { get; set; }

            public string TRAGETCST { get; set; }

            public string PRODUCTTYPE { get; set; }

            public string PRODUCTSPECNAME { get; set; }

            public string OWERID { get; set; }

            public string PROCESSOPERATORNAME { get; set; }

            public string GROUPINDEX { get; set; }

            public string OWERTYPE { get; set; }

            public string PRODUCTOWER { get; set; }

            public CSTCONTROLc()
            {
                SOURCECST = string.Empty;
                TRAGETCST = string.Empty;
                PRODUCTTYPE = string.Empty;
                PRODUCTSPECNAME = string.Empty;
                OWERID = string.Empty;
                PROCESSOPERATORNAME = string.Empty;
                GROUPINDEX = string.Empty;
                OWERTYPE = string.Empty;
                PRODUCTOWER = string.Empty;
            }
        }

		public class ABNORMALFLAGc
		{
			public string VNAME { get; set; }

			public string VVALUE { get; set; }

			public ABNORMALFLAGc()
			{
				VNAME = string.Empty;
				VVALUE = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string GLASSID { get; set; }

			public string CASSETTESEQUENCENO { get; set; }

			public string JOBSEQUENCENO { get; set; }

			public string GROUPINDEX { get; set; }

			public string PRODUCTTYPE { get; set; }

			public string CSTOPERATIONMODE { get; set; }

			public string SUBSTRATETYPE { get; set; }

			public string CIMMODE { get; set; }

			public string JOBTYPE { get; set; }

			public string JOBJUDGE { get; set; }

			public string SAMPLINGSLOTFLAG { get; set; }

            //public string OXINFORMATIONREQUESTFLAG { get; set; }

			public string FIRSTRUN { get; set; }

			public string JOBGRADE { get; set; }

			public string PPID { get; set; }

            public CSTCONTROLc CSTCONTROL { get; set; }

			[XmlArray("ABNORMALFLAGLIST")]
			[XmlArrayItem("ABNORMALFLAG")]
			public List<ABNORMALFLAGc> ABNORMALFLAGLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				GLASSID = string.Empty;
				CASSETTESEQUENCENO = string.Empty;
				JOBSEQUENCENO = string.Empty;
				GROUPINDEX = string.Empty;
				PRODUCTTYPE = string.Empty;
				CSTOPERATIONMODE = string.Empty;
				SUBSTRATETYPE = string.Empty;
				CIMMODE = string.Empty;
				JOBTYPE = string.Empty;
				JOBJUDGE = string.Empty;
				SAMPLINGSLOTFLAG = string.Empty;
                //OXINFORMATIONREQUESTFLAG = string.Empty;
				FIRSTRUN = string.Empty;
				JOBGRADE = string.Empty;
				PPID = string.Empty;
                CSTCONTROL = new CSTCONTROLc();
				ABNORMALFLAGLIST = new List<ABNORMALFLAGc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public JobDataReply()
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
