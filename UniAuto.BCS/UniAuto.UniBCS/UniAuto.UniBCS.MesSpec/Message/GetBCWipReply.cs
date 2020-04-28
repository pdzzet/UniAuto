using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class GetBCWipReply : Message
	{
		public class PRODUCTc
		{
			public string PRODUCTNAME { get; set; }

			public string HOSTPRODUCTNAME { get; set; }

			public PRODUCTc()
			{
				PRODUCTNAME = string.Empty;
				HOSTPRODUCTNAME = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string LINERECIPENAME { get; set; }

			public string REASONCODE { get; set; }

			public string EVENTCOMMENT { get; set; }

			public string PRODUCTQUANTITY { get; set; }

			[XmlArray("PRODUCTLIST")]
			[XmlArrayItem("PRODUCT")]
			public List<PRODUCTc> PRODUCTLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				LINERECIPENAME = string.Empty;
				REASONCODE = string.Empty;
				EVENTCOMMENT = string.Empty;
				PRODUCTQUANTITY = string.Empty;
				PRODUCTLIST = new List<PRODUCTc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public GetBCWipReply()
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
