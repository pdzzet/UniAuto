using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class QtimeSetChanged : Message
	{
		public class QTIMEFROMc
		{
			public string QFROMNAME { get; set; }

			public string TRACELEVEL { get; set; }

			public QTIMEFROMc()
			{
				QFROMNAME = string.Empty;
				TRACELEVEL = string.Empty;
			}
		}

		public class QTIMETOc
		{
			public string QTONAME { get; set; }

			public string TRACELEVEL { get; set; }

			public QTIMETOc()
			{
				QTONAME = string.Empty;
				TRACELEVEL = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string QTIME { get; set; }

			public QTIMEFROMc QTIMEFROM { get; set; }

			public QTIMETOc QTIMETO { get; set; }

			public string EVENTUSER { get; set; }

			[XmlIgnore]
			public DateTime DATETIMEdt { get; set; }

			public string DATETIME
			{
				get { return this.DATETIMEdt.ToString("yyyyMMddHHmmss", System.Globalization.DateTimeFormatInfo.InvariantInfo); }
				set
				{
					if(string.IsNullOrEmpty(value))
						this.DATETIMEdt = DateTime.MinValue;
					else
					{
						string str = DateTimeFormat.Format(value);
						this.DATETIMEdt = DateTime.Parse(str);
					}
				}
			}

			public TrxBody()
			{
				LINENAME = string.Empty;
				QTIME = string.Empty;
				QTIMEFROM = new QTIMEFROMc();
				QTIMETO = new QTIMETOc();
				EVENTUSER = string.Empty;
				DATETIMEdt = DateTime.MinValue;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public QtimeSetChanged()
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
