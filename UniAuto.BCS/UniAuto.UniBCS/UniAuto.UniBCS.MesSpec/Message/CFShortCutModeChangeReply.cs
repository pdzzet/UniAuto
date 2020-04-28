using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class CFShortCutModeChangeReply : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			[XmlIgnore]
			public bool CFSHORTCUTMODEbool { get; set; }

			public string CFSHORTCUTMODE
			{
				get{ return CFSHORTCUTMODEbool ? "Y" : "N"; }
				set{ CFSHORTCUTMODEbool = (string.Compare(value, "Y", true) == 0); }
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
				CFSHORTCUTMODEbool = false;
				VALIRESULTbool = false;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public CFShortCutModeChangeReply()
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
