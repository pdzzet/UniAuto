using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class BoxProcessStarted : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string LINERECIPENAME { get; set; }

			public string HOSTLINERECIPENAME { get; set; }

            public string PPID { get; set; }

            public string HOSTPPID { get; set; }

			public string BOXQUANTITY { get; set; }

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

			[XmlArray("BOXLIST")]
			[XmlArrayItem("BOXNAME")]
			public List<string> BOXLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				LINERECIPENAME = string.Empty;
				HOSTLINERECIPENAME = string.Empty;
                PPID = string.Empty;
                HOSTPPID = string.Empty;
				BOXQUANTITY = string.Empty;
				TIMESTAMPdt = DateTime.MinValue;
				BOXLIST = new List<string>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public BoxProcessStarted()
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
