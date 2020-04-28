using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class BoxLabelInformationReply : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string MACHINENAME { get; set; }

			public string BOXNAME { get; set; }

			public string MODELNAME { get; set; }

			public string MODELVERSION { get; set; }

			public string CASEID { get; set; }

			public string WEEKCODE { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				MACHINENAME = string.Empty;
				BOXNAME = string.Empty;
				MODELNAME = string.Empty;
				MODELVERSION = string.Empty;
				CASEID = string.Empty;
				WEEKCODE = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public BoxLabelInformationReply()
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
