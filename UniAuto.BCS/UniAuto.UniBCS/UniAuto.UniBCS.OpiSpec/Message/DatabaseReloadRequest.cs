using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class DatabaseReloadRequest : Message
	{
		public class MODIFYc
		{
			public string MODIFYKEY { get; set; }

			public string MODIFYTYPE { get; set; }

			public MODIFYc()
			{
				MODIFYKEY = string.Empty;
				MODIFYTYPE = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string OPERATORID { get; set; }

			public string TABLENAME { get; set; }

			[XmlArray("MODIFYLIST")]
			[XmlArrayItem("MODIFY")]
			public List<MODIFYc> MODIFYLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				OPERATORID = string.Empty;
				TABLENAME = string.Empty;
				MODIFYLIST = new List<MODIFYc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public DatabaseReloadRequest()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
			this.WaitReply = "DatabaseReloadReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
