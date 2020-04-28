using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class ChangeTankReport : Message
	{
		public class TANKc
		{
			public string TANKNAME { get; set; }

			public string NEWTANKNAME { get; set; }

			public string CHAMBERID { get; set; }

			public TANKc()
			{
				TANKNAME = string.Empty;
				NEWTANKNAME = string.Empty;
				CHAMBERID = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string MACHINENAME { get; set; }

			public string MATERIALTYPE { get; set; }

			[XmlArray("TANKLIST")]
			[XmlArrayItem("TANK")]
			public List<TANKc> TANKLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				MACHINENAME = string.Empty;
				MATERIALTYPE = string.Empty;
				TANKLIST = new List<TANKc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public ChangeTankReport()
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
