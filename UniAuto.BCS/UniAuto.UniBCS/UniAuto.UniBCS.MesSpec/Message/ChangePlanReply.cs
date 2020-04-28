using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class ChangePlanReply : Message
	{
		public class SOURCEc
		{
			public string CARRIERNAME { get; set; }

			[XmlArray("PRODUCTLIST")]
			[XmlArrayItem("PRODUCT")]
			public List<PRODUCTc> PRODUCTLIST { get; set; }

			public SOURCEc()
			{
				CARRIERNAME = string.Empty;
				PRODUCTLIST = new List<PRODUCTc>();
			}
		}

		public class PRODUCTc
		{
			public string PRODUCTNAME { get; set; }

			public string PAIRCARRIERNAME { get; set; }

			public PRODUCTc()
			{
				PRODUCTNAME = string.Empty;
				PAIRCARRIERNAME = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string NEXTPLANNAME { get; set; }

			[XmlArray("SOURCELIST")]
			[XmlArrayItem("SOURCE")]
			public List<SOURCEc> SOURCELIST { get; set; }

			[XmlArray("TARGETLIST")]
			[XmlArrayItem("CARRIERNAME")]
			public List<string> TARGETLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				NEXTPLANNAME = string.Empty;
				SOURCELIST = new List<SOURCEc>();
				TARGETLIST = new List<string>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public ChangePlanReply()
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
