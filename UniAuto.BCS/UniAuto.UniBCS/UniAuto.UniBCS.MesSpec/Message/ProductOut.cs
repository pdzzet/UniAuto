using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class ProductOut : Message
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

			public string PRODUCTJUDGE { get; set; }

			public string PRODUCTGRADE { get; set; }

			public string SUBPRODUCTGRADES { get; set; }

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
			public bool CROSSLINEFLAGbool { get; set; }

			public string CROSSLINEFLAG
			{
				get{ return CROSSLINEFLAGbool ? "Y" : "N"; }
				set{ CROSSLINEFLAGbool = (string.Compare(value, "Y", true) == 0); }
			}

			public string GLASSSIZE { get; set; }

			[XmlIgnore]
			public bool LASTGLASSFLAGbool { get; set; }

			public string LASTGLASSFLAG
			{
				get{ return LASTGLASSFLAGbool ? "Y" : "N"; }
				set{ LASTGLASSFLAGbool = (string.Compare(value, "Y", true) == 0); }
			}

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
				PRODUCTJUDGE = string.Empty;
				PRODUCTGRADE = string.Empty;
				SUBPRODUCTGRADES = string.Empty;
				TIMESTAMPdt = DateTime.MinValue;
				CROSSLINEFLAGbool = false;
				GLASSSIZE = string.Empty;
				LASTGLASSFLAGbool = false;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public ProductOut()
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
