using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class MaskProcessEnd : Message
	{
		public class MASKc
		{
			public string MASKPOSITION { get; set; }

			public string MASKNAME { get; set; }

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

			public string CLEANRESULT { get; set; }

			public string UNITNAME { get; set; }

			public string HEADID { get; set; }

			public MASKc()
			{
				MASKPOSITION = string.Empty;
				MASKNAME = string.Empty;
				MASKUSECOUNTint = 0;
				CLEANRESULT = string.Empty;
				UNITNAME = string.Empty;
				HEADID = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string MACHINENAME { get; set; }

			public string MASKCARRIERNAME { get; set; }

			[XmlIgnore]
			public bool ABORTFLAGbool { get; set; }

			public string ABORTFLAG
			{
				get{ return ABORTFLAGbool ? "Y" : "N"; }
				set{ ABORTFLAGbool = (string.Compare(value, "Y", true) == 0); }
			}

			[XmlArray("MASKLIST")]
			[XmlArrayItem("MASK")]
			public List<MASKc> MASKLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				MACHINENAME = string.Empty;
				MASKCARRIERNAME = string.Empty;
				ABORTFLAGbool = false;
				MASKLIST = new List<MASKc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public MaskProcessEnd()
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
