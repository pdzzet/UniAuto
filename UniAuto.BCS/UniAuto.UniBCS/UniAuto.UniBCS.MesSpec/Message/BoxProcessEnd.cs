using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class BoxProcessEnd : Message
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
			public string POSITION { get; set; }

			public string PRODUCTNAME { get; set; }

			public string HOSTPRODUCTNAME { get; set; }

			public string SHORTCUTFLAG { get; set; }

			[XmlArray("ABNORMALCODELIST")]
			[XmlArrayItem("CODE")]
			public List<CODEc> ABNORMALCODELIST { get; set; }

			public string BOXULDFLAG { get; set; }

			public string DPIPROCESSFLAG { get; set; }

            public string RTPFLAG { get; set; }

            public string PPID { get; set; }

            public string HOSTPPID { get; set; }

			public PRODUCTc()
			{
				POSITION = string.Empty;
				PRODUCTNAME = string.Empty;
				HOSTPRODUCTNAME = string.Empty;
				SHORTCUTFLAG = string.Empty;
				ABNORMALCODELIST = new List<CODEc>();
				BOXULDFLAG = string.Empty;
				DPIPROCESSFLAG = string.Empty;
                RTPFLAG = string.Empty;
                PPID = string.Empty;
                HOSTPPID = string.Empty;
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

			public string PORTNAME { get; set; }

			public string LINERECIPENAME { get; set; }

			public string HOSTLINERECIPENAME { get; set; }

            public string PPID { get; set; }

            public string HOSTPPID { get; set; }

			public string BOXQUANTITY { get; set; }

			public string SAMPLEFLAG { get; set; }

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
			[XmlArrayItem("BOX")]
			public List<BOXc> BOXLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				PORTNAME = string.Empty;
				LINERECIPENAME = string.Empty;
				HOSTLINERECIPENAME = string.Empty;
                PPID = string.Empty;
                HOSTPPID = string.Empty;
				BOXQUANTITY = string.Empty;
				SAMPLEFLAG = string.Empty;
				TIMESTAMPdt = DateTime.MinValue;
				BOXLIST = new List<BOXc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public BoxProcessEnd()
		{
			this.Direction = Spec.DirType.BC_TO_MES;
			this.WaitReply = "BoxProcessEndReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
