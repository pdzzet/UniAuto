using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class SECSFunctionRequest : Message
	{
		public class PARAMETERc
		{
			public string NAME { get; set; }

			public string VALUE { get; set; }

			public PARAMETERc()
			{
				NAME = string.Empty;
				VALUE = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string EQUIPMENTNO { get; set; }

			public string SECSNAME { get; set; }

			[XmlArray("PARAMETERLIST")]
			[XmlArrayItem("PARAMETER")]
			public List<PARAMETERc> PARAMETERLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				EQUIPMENTNO = string.Empty;
				SECSNAME = string.Empty;
				PARAMETERLIST = new List<PARAMETERc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public SECSFunctionRequest()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
			this.WaitReply = "SECSFunctionReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
