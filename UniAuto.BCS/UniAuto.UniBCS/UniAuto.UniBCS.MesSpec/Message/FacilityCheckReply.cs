using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class FacilityCheckReply : Message
	{
		public class MACHINEc
		{
			public string MACHINENAME { get; set; }

			public string MACHINESTATENAME { get; set; }

			[XmlArray("FACILITYPARALIST")]
			[XmlArrayItem("PARA")]
			public List<PARAc> FACILITYPARALIST { get; set; }

			public MACHINEc()
			{
				MACHINENAME = string.Empty;
				MACHINESTATENAME = string.Empty;
				FACILITYPARALIST = new List<PARAc>();
			}
		}

		public class PARAc
		{
			public string RECIPEID { get; set; }

			public string PARANAME { get; set; }

			public string VALUETYPE { get; set; }

			public string PARAVALUE { get; set; }

			public PARAc()
			{
				RECIPEID = string.Empty;
				PARANAME = string.Empty;
				VALUETYPE = string.Empty;
				PARAVALUE = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			[XmlArray("MACHINELIST")]
			[XmlArrayItem("MACHINE")]
			public List<MACHINEc> MACHINELIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				MACHINELIST = new List<MACHINEc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public FacilityCheckReply()
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
