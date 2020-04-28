using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class RecipeParameterChangeReply : Message
	{
		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string MACHINENAME { get; set; }

			public string RECIPEID { get; set; }

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
				MACHINENAME = string.Empty;
				RECIPEID = string.Empty;
				VALIRESULTbool = false;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public RecipeParameterChangeReply()
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
