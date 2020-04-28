using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class MaskLocationChanged : Message
	{
		public class MASKc
		{
			public string MASKNAME { get; set; }

			public string MASKLOCATION { get; set; }

			public MASKc()
			{
				MASKNAME = string.Empty;
				MASKLOCATION = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string MACHINENAME { get; set; }

			[XmlArray("MASKLIST")]
			[XmlArrayItem("MASK")]
			public List<MASKc> MASKLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				MACHINENAME = string.Empty;
				MASKLIST = new List<MASKc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public MaskLocationChanged()
		{
			this.Direction = Spec.DirType.BC_TO_MES;
			this.WaitReply = "MaskLocationChangedReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
