using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class PanelReportByGlass : Message
	{
		public class ORIGINALPRODUCTc
		{
			public string ORIGINALPRODUCTNAME { get; set; }

			[XmlArray("PRODUCTLIST")]
			[XmlArrayItem("PRODUCT")]
			public List<PRODUCTc> PRODUCTLIST { get; set; }

			public ORIGINALPRODUCTc()
			{
				ORIGINALPRODUCTNAME = string.Empty;
				PRODUCTLIST = new List<PRODUCTc>();
			}
		}

		public class PRODUCTc
		{
			public string PRODUCTNAME { get; set; }

			public string MACOJUDGE { get; set; }

			public string MURACODES { get; set; }

			public string EVENTCOMMENT { get; set; }

			public PRODUCTc()
			{
				PRODUCTNAME = string.Empty;
				MACOJUDGE = string.Empty;
				MURACODES = string.Empty;
				EVENTCOMMENT = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			[XmlArray("ORIGINALPRODUCTLIST")]
			[XmlArrayItem("ORIGINALPRODUCT")]
			public List<ORIGINALPRODUCTc> ORIGINALPRODUCTLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				ORIGINALPRODUCTLIST = new List<ORIGINALPRODUCTc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public PanelReportByGlass()
		{
			this.Direction = Spec.DirType.BC_TO_MES;
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
