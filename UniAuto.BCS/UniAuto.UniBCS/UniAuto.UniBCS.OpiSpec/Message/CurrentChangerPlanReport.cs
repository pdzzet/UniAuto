using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class CurrentChangerPlanReport : Message
	{
		public class CHANGERPLANc
		{
            public string CURRENTPLANID { get; set; }
            public string STANDBYPLANID { get; set; }

            public string PLANSTATUS { get; set; }

            [XmlArray("PRODUCTLIST")]
            [XmlArrayItem("PRODUCT")]
            public List<PRODUCTc> PRODUCTLIST { get; set; }
            public List<PRODUCTc> STANDBYPRODUCTLIST { get; set; }


            public CHANGERPLANc()
            {
                CURRENTPLANID = string.Empty;
                STANDBYPLANID = string.Empty;
                PLANSTATUS = string.Empty;
                PRODUCTLIST = new List<PRODUCTc>();
                STANDBYPRODUCTLIST = new List<PRODUCTc>();
            }
		}

		public class PRODUCTc
		{
			public string SLOTNO { get; set; }

			public string PRODUCTNAME { get; set; }

			public string SOURCECSTID { get; set; }

			public string TARGETCSTID { get; set; }

			public string HAVEBEENUSE { get; set; }

			public PRODUCTc()
			{
				SLOTNO = string.Empty;
				PRODUCTNAME = string.Empty;
				SOURCECSTID = string.Empty;
				TARGETCSTID = string.Empty;
				HAVEBEENUSE = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public CHANGERPLANc CHANGERPLAN { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				CHANGERPLAN = new CHANGERPLANc();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public CurrentChangerPlanReport()
		{
			this.Direction = Spec.DirType.BC_TO_OPI;
            this.WaitReply = ""; // "CurrentChangerPlanReportReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
