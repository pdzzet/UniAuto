using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class DenseStatusReport : Message
    {
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

            public string MESCONTROLSTATENAM { get; set; }

            public string EQUIPMENTNO { get; set; }

            public string PORTNO { get; set; }

            public string PORTID { get; set; }

            public string PORTENABLEMODE { get; set; }

            public string PORTPACKINGMODE { get; set; }

            public string BOXID01 { get; set; }

            public string BOXID02 { get; set; }

            public string UNPACKINGSOURCE { get; set; }

            //public string BOXID { get; set; }

            public string PAPER_BOXID { get; set; }

            public string BOXTYPE { get; set; }

            public string DENSEBOXDATAREQUEST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
                MESCONTROLSTATENAM = string.Empty;
                EQUIPMENTNO = string.Empty;
                PORTNO = string.Empty;
                PORTID = string.Empty;
                PORTENABLEMODE = string.Empty;
                PORTPACKINGMODE = string.Empty;
                BOXID01 = string.Empty;
                BOXID02 = string.Empty;
                UNPACKINGSOURCE = string.Empty;
                //BOXID = string.Empty;
                PAPER_BOXID = string.Empty;
                BOXTYPE = string.Empty;
                DENSEBOXDATAREQUEST = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public DenseStatusReport()
		{
			this.Direction = Spec.DirType.BC_TO_OPI;
            this.WaitReply = ""; // "DenseStatusReportReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
    }
}
