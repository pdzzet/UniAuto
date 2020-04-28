using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class ValidatePalletReply : Message
	{
		public class BOXc
		{
			public string BOXNAME { get; set; }

			public string PRODUCTQUANTITY { get; set; }

			[XmlArray("PRODUCTLIST")]
			[XmlArrayItem("PRODUCT")]
			public List<PRODUCTc> PRODUCTLIST { get; set; }

			public BOXc()
			{
				BOXNAME = string.Empty;
				PRODUCTQUANTITY = string.Empty;
				PRODUCTLIST = new List<PRODUCTc>();
			}
		}

		public class PRODUCTc
		{
			public string PRODUCTNAME { get; set; }

			[XmlArray("ABNORMALCODELIST")]
			[XmlArrayItem("CODE")]
			public List<CODEc> ABNORMALCODELIST { get; set; }

			public PRODUCTc()
			{
				PRODUCTNAME = string.Empty;
				ABNORMALCODELIST = new List<CODEc>();
			}
		}

		public class CODEc
		{
			public string ABNORMALSEQ { get; set; }
			
			public string ABNORMALCODE { get; set; }

			public CODEc()
			{
                ABNORMALSEQ = string.Empty;
				ABNORMALCODE = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string LINERECIPENAME { get; set; }

			public string PALLETNAME { get; set; }

			public string BOXQUANTITY { get; set; }

			public string PORTNAME { get; set; }

            public string CARRIERSETCODE { get; set; }

			[XmlArray("BOXLIST")]
			[XmlArrayItem("BOX")]
			public List<BOXc> BOXLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				LINERECIPENAME = string.Empty;
				PALLETNAME = string.Empty;
				BOXQUANTITY = string.Empty;
				PORTNAME = string.Empty;
                CARRIERSETCODE = string.Empty;
				BOXLIST = new List<BOXc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public ValidatePalletReply()
		{
			this.Direction = Spec.DirType.MES_TO_BC;
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
