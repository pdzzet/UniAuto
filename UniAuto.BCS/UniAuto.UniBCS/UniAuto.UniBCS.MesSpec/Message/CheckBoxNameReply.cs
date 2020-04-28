using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class CheckBoxNameReply : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string PALLETNAME { get; set; }

			public string BOXNAME { get; set; }

			[XmlIgnore]
			public bool VALIRESULTbool { get; set; }

			public string VALIRESULT
			{
				get{ return VALIRESULTbool ? "Y" : "N"; }
				set{ VALIRESULTbool = (string.Compare(value, "Y", true) == 0); }
			}

			public string VALICODE { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				PALLETNAME = string.Empty;
				BOXNAME = string.Empty;
				VALIRESULTbool = false;
				VALICODE = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public CheckBoxNameReply()
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
