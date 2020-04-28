using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class ProductIn : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string MACHINENAME { get; set; }

			public string UNITNAME { get; set; }

			public string PORTNAME { get; set; }

			public string TRACELEVEL { get; set; }

			public string LOTNAME { get; set; }

			public string CARRIERNAME { get; set; }

			public string POSITION { get; set; }

			public string PRODUCTNAME { get; set; }

			public string HOSTPRODUCTNAME { get; set; }

			public string LINERECIPENAME { get; set; }

			public string HOSTRECIPENAME { get; set; }

			public string BCRECIPEID { get; set; }

			public string HOSTRECIPEID { get; set; }

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

			[XmlIgnore]
			public int PROCESSINGTIMEint { get; set; }

			public string PROCESSINGTIME
			{
				get{ return PROCESSINGTIMEint.ToString(); }
				set
				{
					int tmp = 0;
					if(int.TryParse(value, out tmp))
						PROCESSINGTIMEint = tmp;
					else
						PROCESSINGTIMEint = 0;
				}
			}

			[XmlIgnore]
			public bool CROSSLINEFLAGbool { get; set; }

			public string CROSSLINEFLAG
			{
				get{ return CROSSLINEFLAGbool ? "Y" : "N"; }
				set{ CROSSLINEFLAGbool = (string.Compare(value, "Y", true) == 0); }
			}

			public string GLASSSIZE { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				MACHINENAME = string.Empty;
				UNITNAME = string.Empty;
				PORTNAME = string.Empty;
				TRACELEVEL = string.Empty;
				LOTNAME = string.Empty;
				CARRIERNAME = string.Empty;
				POSITION = string.Empty;
				PRODUCTNAME = string.Empty;
				HOSTPRODUCTNAME = string.Empty;
				LINERECIPENAME = string.Empty;
				HOSTRECIPENAME = string.Empty;
				BCRECIPEID = string.Empty;
				HOSTRECIPEID = string.Empty;
				TIMESTAMPdt = DateTime.MinValue;
				PROCESSINGTIMEint = 0;
				CROSSLINEFLAGbool = false;
				GLASSSIZE = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public ProductIn()
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
