using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class MaxCutGlassProcessEndReply : Message
	{
		public class PRODUCTc
		{
			public string PRODUCTNAME { get; set; }

			public string SAMPLETYPE { get; set; }

			public PRODUCTc()
			{
				PRODUCTNAME = string.Empty;
				SAMPLETYPE = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string MACHINENAME { get; set; }

			public string PRODUCTQUANTITY { get; set; }

			[XmlIgnore]
			public DateTime TIMESTAMPdt { get; set; }

			public string TIMESTAMP
			{
				get { return this.TIMESTAMPdt.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo); }
				set
				{
					if(string.IsNullOrEmpty(value))
						this.TIMESTAMPdt = DateTime.MinValue;
					else
						this.TIMESTAMPdt = DateTime.Parse(value);
				}
			}

			[XmlArray("PRODUCTLIST")]
			[XmlArrayItem("PRODUCT")]
			public List<PRODUCTc> PRODUCTLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				MACHINENAME = string.Empty;
				PRODUCTQUANTITY = string.Empty;
				TIMESTAMPdt = DateTime.MinValue;
				PRODUCTLIST = new List<PRODUCTc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public MaxCutGlassProcessEndReply()
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
