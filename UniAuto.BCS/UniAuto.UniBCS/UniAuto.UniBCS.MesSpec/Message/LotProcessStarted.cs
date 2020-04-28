using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class LotProcessStarted : Message
	{
		public class LOTc
		{
			public string LOTNAME { get; set; }

			public string PRODUCTRECIPENAME { get; set; }

			public string PRODUCTSPECNAME { get; set; }

			public string PROCESSOPERATIONNAME { get; set; }

			public string PRODUCTQUANTITY { get; set; }

			public string PRODUCTOWNER { get; set; }

			public string LINERECIPENAME { get; set; }

			public string HOSTLINERECIPENAME { get; set; }

			public string PPID { get; set; }

			public string HOSTPPID { get; set; }

			public LOTc()
			{
				LOTNAME = string.Empty;
				PRODUCTRECIPENAME = string.Empty;
				PRODUCTSPECNAME = string.Empty;
				PROCESSOPERATIONNAME = string.Empty;
				PRODUCTQUANTITY = string.Empty;
				PRODUCTOWNER = string.Empty;
				LINERECIPENAME = string.Empty;
				HOSTLINERECIPENAME = string.Empty;
				PPID = string.Empty;
				HOSTPPID = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string PORTNAME { get; set; }

			public string CARRIERNAME { get; set; }

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

			[XmlArray("LOTLIST")]
			[XmlArrayItem("LOT")]
			public List<LOTc> LOTLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				PORTNAME = string.Empty;
				CARRIERNAME = string.Empty;
				TIMESTAMPdt = DateTime.MinValue;
				LOTLIST = new List<LOTc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public LotProcessStarted()
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
