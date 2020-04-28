using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class CheckLocalPPIDRequest : Message
	{
		public class MACHINEc
		{
			public string MACHINENAME { get; set; }

			public string LOCALNAME { get; set; }

			public string RECIPEID { get; set; }

			public MACHINEc()
			{
				MACHINENAME = string.Empty;
				LOCALNAME = string.Empty;
				RECIPEID = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string LOTNAME { get; set; }

			public string LOCALRECIPENAME { get; set; }

			[XmlArray("MACHINELIST")]
			[XmlArrayItem("MACHINE")]
			public List<MACHINEc> MACHINELIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				LOTNAME = string.Empty;
				LOCALRECIPENAME = string.Empty;
				MACHINELIST = new List<MACHINEc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public CheckLocalPPIDRequest()
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
