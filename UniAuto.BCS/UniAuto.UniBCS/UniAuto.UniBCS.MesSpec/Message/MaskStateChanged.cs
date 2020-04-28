using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class MaskStateChanged : Message
	{
		public class MASKc
		{
			public string MASKPOSITION { get; set; }

			public string MASKNAME { get; set; }

			public string MASKSTATE { get; set; }

			[XmlIgnore]
			public int MASKUSECOUNTint { get; set; }

			public string MASKUSECOUNT
			{
				get{ return MASKUSECOUNTint.ToString(); }
				set
				{
					int tmp = 0;
					if(int.TryParse(value, out tmp))
						MASKUSECOUNTint = tmp;
					else
						MASKUSECOUNTint = 0;
				}
			}

			public string UNITNAME { get; set; }

			public string CLEANRESULT { get; set; }

			public string REASONCODE { get; set; }

			public string HEADID { get; set; }

			public MASKc()
			{
				MASKPOSITION = string.Empty;
				MASKNAME = string.Empty;
				MASKSTATE = string.Empty;
				MASKUSECOUNTint = 0;
				UNITNAME = string.Empty;
				CLEANRESULT = string.Empty;
				REASONCODE = string.Empty;
				HEADID = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string MACHINENAME { get; set; }

			public string MACHINERECIPENAME { get; set; }

			[XmlArray("MASKLIST")]
			[XmlArrayItem("MASK")]
			public List<MASKc> MASKLIST { get; set; }

			public string OPERATOR { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				MACHINENAME = string.Empty;
				MACHINERECIPENAME = string.Empty;
				MASKLIST = new List<MASKc>();
				OPERATOR = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public MaskStateChanged()
		{
			this.Direction = Spec.DirType.BC_TO_MES;
			this.WaitReply = "MaskStateChangedReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
