using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class ProductProcessData : Message
	{
		public class ITEMc
		{
			public string ITEMNAME { get; set; }

			[XmlArray("SITELIST")]
			[XmlArrayItem("SITE")]
			public List<SITEc> SITELIST { get; set; }

			public ITEMc()
			{
				ITEMNAME = string.Empty;
				SITELIST = new List<SITEc>();
			}
		}

		public class SITEc
		{
			public string SITENAME { get; set; }

			public string SITEVALUE { get; set; }

			public SITEc()
			{
				SITENAME = string.Empty;
				SITEVALUE = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string MACHINENAME { get; set; }

			public string UNITNAME { get; set; }

			public string LOTNAME { get; set; }

			public string CARRIERNAME { get; set; }

			public string PRODUCTNAME { get; set; }

			public string PRODUCTSPECNAME { get; set; }

			public string PRODUCTSPECVER { get; set; }

			public string PROCESSOPERATIONNAME { get; set; }

			public string LINERECIPENAME { get; set; }

			[XmlArray("ITEMLIST")]
			[XmlArrayItem("ITEM")]
			public List<ITEMc> ITEMLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				MACHINENAME = string.Empty;
				UNITNAME = string.Empty;
				LOTNAME = string.Empty;
				CARRIERNAME = string.Empty;
				PRODUCTNAME = string.Empty;
				PRODUCTSPECNAME = string.Empty;
				PRODUCTSPECVER = string.Empty;
				PROCESSOPERATIONNAME = string.Empty;
				LINERECIPENAME = string.Empty;
				ITEMLIST = new List<ITEMc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public ProductProcessData()
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
