using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class MachineDataUpdate : Message
	{
		public class MACHINEc
		{
			public string MACHINENAME { get; set; }

			public string MACHINESTATENAME { get; set; }

			[XmlIgnore]
			public bool MATERIALCHANGEFLAGbool { get; set; }

			public string MATERIALCHANGEFLAG
			{
				get{ return MATERIALCHANGEFLAGbool ? "Y" : "N"; }
				set{ MATERIALCHANGEFLAGbool = (string.Compare(value, "Y", true) == 0); }
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

			public string LINEOPERMODE { get; set; }

			public string BCSERIALNO { get; set; }

			public string BCRECIPEIDLENGTH { get; set; }

			[XmlArray("UNITLIST")]
			[XmlArrayItem("UNIT")]
			public List<UNITc> UNITLIST { get; set; }

			public MACHINEc()
			{
				MACHINENAME = string.Empty;
				MACHINESTATENAME = string.Empty;
				MATERIALCHANGEFLAGbool = false;
				ALARMCODE = string.Empty;
				ALARMTEXT = string.Empty;
				ALARMTIMESTAMPdt = DateTime.MinValue;
				LINEOPERMODE = string.Empty;
				BCSERIALNO = string.Empty;
				BCRECIPEIDLENGTH = string.Empty;
				UNITLIST = new List<UNITc>();
			}
		}

		public class UNITc
		{
			public string UNITNAME { get; set; }

			public string UNITSTATENAME { get; set; }

			[XmlIgnore]
			public bool MATERIALCHANGEFLAGbool { get; set; }

			public string MATERIALCHANGEFLAG
			{
				get{ return MATERIALCHANGEFLAGbool ? "Y" : "N"; }
				set{ MATERIALCHANGEFLAGbool = (string.Compare(value, "Y", true) == 0); }
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

			public string LINEOPERMODE { get; set; }

			public UNITc()
			{
				UNITNAME = string.Empty;
				UNITSTATENAME = string.Empty;
				MATERIALCHANGEFLAGbool = false;
				ALARMCODE = string.Empty;
				ALARMTEXT = string.Empty;
				ALARMTIMESTAMPdt = DateTime.MinValue;
				LINEOPERMODE = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string LINESTATENAME { get; set; }

			[XmlIgnore]
			public bool MATERIALCHANGEFLAGbool { get; set; }

			public string MATERIALCHANGEFLAG
			{
				get{ return MATERIALCHANGEFLAGbool ? "Y" : "N"; }
				set{ MATERIALCHANGEFLAGbool = (string.Compare(value, "Y", true) == 0); }
			}

			public string LINEOPERMODE { get; set; }

			public string PARTIALFULLMODE { get; set; }

			public string CSTOPERMODE { get; set; }

			[XmlIgnore]
			public bool CFSHORTCUTMODEbool { get; set; }

			public string CFSHORTCUTMODE
			{
				get{ return CFSHORTCUTMODEbool ? "Y" : "N"; }
				set{ CFSHORTCUTMODEbool = (string.Compare(value, "Y", true) == 0); }
			}

			[XmlArray("MACHINELIST")]
			[XmlArrayItem("MACHINE")]
			public List<MACHINEc> MACHINELIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				LINESTATENAME = string.Empty;
				MATERIALCHANGEFLAGbool = false;
				LINEOPERMODE = string.Empty;
				PARTIALFULLMODE = string.Empty;
				CSTOPERMODE = string.Empty;
				CFSHORTCUTMODEbool = false;
				MACHINELIST = new List<MACHINEc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public MachineDataUpdate()
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
