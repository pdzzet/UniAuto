using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class MachineStateChanged : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string MACHINENAME { get; set; }

			public string MACHINESTATENAME { get; set; }

			[XmlIgnore]
			public bool MATERIALCHANGEFLAGbool { get; set; }

			public string MATERIALCHANGEFLAG
			{
				get{ return MATERIALCHANGEFLAGbool ? "Y" : "N"; }
				set{ MATERIALCHANGEFLAGbool = (string.Compare(value, "Y", true) == 0); }
			}

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

			public string ALARMCODE { get; set; }

			public string ALARMTEXT { get; set; }

			[XmlIgnore]
			public DateTime ALARMTIMESTAMPdt { get; set; }

			public string ALARMTIMESTAMP
			{
				get { return this.ALARMTIMESTAMPdt.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo); }
				set
				{
					if(string.IsNullOrEmpty(value))
						this.ALARMTIMESTAMPdt = DateTime.MinValue;
					else
						this.ALARMTIMESTAMPdt = DateTime.Parse(value);
				}
			}

			[XmlIgnore]
			public bool CROSSLINEFLAGbool { get; set; }

			public string CROSSLINEFLAG
			{
				get{ return CROSSLINEFLAGbool ? "Y" : "N"; }
				set{ CROSSLINEFLAGbool = (string.Compare(value, "Y", true) == 0); }
			}

			public string OPERATOR { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				MACHINENAME = string.Empty;
				MACHINESTATENAME = string.Empty;
				MATERIALCHANGEFLAGbool = false;
				TIMESTAMPdt = DateTime.MinValue;
				ALARMCODE = string.Empty;
				ALARMTEXT = string.Empty;
				ALARMTIMESTAMPdt = DateTime.MinValue;
				CROSSLINEFLAGbool = false;
				OPERATOR = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public MachineStateChanged()
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
