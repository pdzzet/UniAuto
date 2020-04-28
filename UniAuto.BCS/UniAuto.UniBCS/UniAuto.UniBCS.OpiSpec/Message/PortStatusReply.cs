using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class PortStatusReply : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

            public string LINEID { get; set; }

			public string MESCONTROLSTATENAM { get; set; }

			public string EQUIPMENTNO { get; set; }

			public string PORTNO { get; set; }

			public string PORTID { get; set; }

			public string CASSETTESEQNO { get; set; }

			public string CASSETTEID { get; set; }

			public string PORTSTATUS { get; set; }

			public string CASSETTESTATUS { get; set; }

			public string PORTTYPE { get; set; }

			public string PORTMODE { get; set; }

			public string PORTENABLEMODE { get; set; }

			public string PORTTRANSFERMODE { get; set; }

            //public string PORTOPERMODE { get; set; }

			public string PORTGRADE { get; set; }

			public string PORTCNT { get; set; }

			public string SUBCSTSTATE { get; set; }

			public string JOBEXISTSLOT { get; set; }

			public string PORTDOWN { get; set; }

            public string PARTIALFULLMODE { get; set; }

            public string LOADINGCASSETTETYPE { get; set; }

            public string PRODUCTTYPE { get; set; }

            public string PROCESSTYPE_ARRAY { get; set; }

            public string ASSIGNMENT_GAP { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
                LINEID = string.Empty;
				MESCONTROLSTATENAM = string.Empty;
				EQUIPMENTNO = string.Empty;
				PORTNO = string.Empty;
				PORTID = string.Empty;
				CASSETTESEQNO = string.Empty;
				CASSETTEID = string.Empty;
				PORTSTATUS = string.Empty;
				CASSETTESTATUS = string.Empty;
				PORTTYPE = string.Empty;
				PORTMODE = string.Empty;
				PORTENABLEMODE = string.Empty;
				PORTTRANSFERMODE = string.Empty;
                //PORTOPERMODE = string.Empty;
				PORTGRADE = string.Empty;
				PORTCNT = string.Empty;
				SUBCSTSTATE = string.Empty;
				JOBEXISTSLOT = string.Empty;
				PORTDOWN = string.Empty;
                PARTIALFULLMODE = string.Empty;
                LOADINGCASSETTETYPE = string.Empty;
                PRODUCTTYPE = string.Empty;
                PROCESSTYPE_ARRAY = string.Empty;
                ASSIGNMENT_GAP = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public PortStatusReply()
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
