using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class MachineModeChangeReply : Message
	{
		public class MACHINEc
		{
			public string MACHINENAME { get; set; }

			public string LINEOPERMODE { get; set; }

			[XmlArray("UNITLIST")]
			[XmlArrayItem("UNIT")]
			public List<UNITc> UNITLIST { get; set; }

			public MACHINEc()
			{
				MACHINENAME = string.Empty;
				LINEOPERMODE = string.Empty;
				UNITLIST = new List<UNITc>();
			}
		}

		public class UNITc
		{
			public string UNITNAME { get; set; }

			public string LINEOPERMODE { get; set; }

			public UNITc()
			{
				UNITNAME = string.Empty;
				LINEOPERMODE = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string LINEOPERMODE { get; set; }

			[XmlArray("MACHINELIST")]
			[XmlArrayItem("MACHINE")]
			public List<MACHINEc> MACHINELIST { get; set; }

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
			public bool VALIRESULTbool { get; set; }

			public string VALIRESULT
			{
				get{ return VALIRESULTbool ? "Y" : "N"; }
				set{ VALIRESULTbool = (string.Compare(value, "Y", true) == 0); }
			}

			public TrxBody()
			{
				LINENAME = string.Empty;
				LINEOPERMODE = string.Empty;
				MACHINELIST = new List<MACHINEc>();
				TIMESTAMPdt = DateTime.MinValue;
				VALIRESULTbool = false;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public MachineModeChangeReply()
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
