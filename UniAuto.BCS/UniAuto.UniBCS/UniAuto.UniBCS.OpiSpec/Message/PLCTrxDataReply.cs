using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class PLCTrxDataReply : Message
	{
		public class EVENTGROUPc
		{
			public string NAME { get; set; }

			public string DIR { get; set; }

			[XmlArray("EVENTLIST")]
			[XmlArrayItem("EVENT")]
			public List<EVENTc> EVENTLIST { get; set; }

			public EVENTGROUPc()
			{
				NAME = string.Empty;
				DIR = string.Empty;
				EVENTLIST = new List<EVENTc>();
			}
		}

		public class EVENTc
		{
			public string NAME { get; set; }

			public string DEVCODE { get; set; }

			public string ADDR { get; set; }

			public string POINTS { get; set; }

			public string SKIPDECODE { get; set; }

			[XmlArray("ITEMLIST")]
			[XmlArrayItem("ITEM")]
			public List<ITEMc> ITEMLIST { get; set; }

			public EVENTc()
			{
				NAME = string.Empty;
				DEVCODE = string.Empty;
				ADDR = string.Empty;
				POINTS = string.Empty;
				SKIPDECODE = string.Empty;
				ITEMLIST = new List<ITEMc>();
			}
		}

		public class ITEMc
		{
			public string NAME { get; set; }

			public string VAL { get; set; }

			public string WOFFSET { get; set; }

			public string WPOINTS { get; set; }

			public string BOFFSET { get; set; }

			public string BPOINTS { get; set; }

			public string EXPERESSION { get; set; }

			public ITEMc()
			{
				NAME = string.Empty;
				VAL = string.Empty;
				WOFFSET = string.Empty;
				WPOINTS = string.Empty;
				BOFFSET = string.Empty;
				BPOINTS = string.Empty;
				EXPERESSION = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string PLCTRXNAME { get; set; }

			[XmlArray("EVENTGROUPLIST")]
			[XmlArrayItem("EVENTGROUP")]
			public List<EVENTGROUPc> EVENTGROUPLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				PLCTRXNAME = string.Empty;
				EVENTGROUPLIST = new List<EVENTGROUPc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public PLCTrxDataReply()
		{
			this.Direction = Spec.DirType.BC_TO_OPI;
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
