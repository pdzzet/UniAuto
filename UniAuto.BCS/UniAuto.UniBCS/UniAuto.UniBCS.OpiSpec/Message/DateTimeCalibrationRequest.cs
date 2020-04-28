using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class DateTimeCalibrationRequest : Message
	{
		public class EQUIPMENTc
		{
			public string EQUIPMENTNO { get; set; }

			public EQUIPMENTc()
			{
				EQUIPMENTNO = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string OPERATORID { get; set; }

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

			[XmlArray("EQUIPMENTLIST")]
			[XmlArrayItem("EQUIPMENT")]
			public List<EQUIPMENTc> EQUIPMENTLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				OPERATORID = string.Empty;
				DATETIMEdt = DateTime.MinValue;
				EQUIPMENTLIST = new List<EQUIPMENTc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public DateTimeCalibrationRequest()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
			this.WaitReply = "DateTimeCalibrationReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
