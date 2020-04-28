using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class ValidateMaskByCarrierReply : Message
	{
		public class MASKc
		{
			public string MASKPOSITION { get; set; }

            public string MASKDETAILTYPE { get; set; }

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

			public string MASKLIMITUSECOUNT { get; set; }

			public MASKc()
			{
				MASKPOSITION = string.Empty;
                MASKDETAILTYPE = string.Empty;
				MASKNAME = string.Empty;
				MASKUSECOUNTint = 0;
				MASKLIMITUSECOUNT = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string PORTNAME { get; set; }

			public string LINERECIPENAME { get; set; }

			public string MASKCARRIERNAME { get; set; }

			[XmlIgnore]
			public bool VALIRESULTbool { get; set; }

			public string VALIRESULT
			{
				get{ return VALIRESULTbool ? "Y" : "N"; }
				set{ VALIRESULTbool = (string.Compare(value, "Y", true) == 0); }
			}

			[XmlArray("MASKLIST")]
			[XmlArrayItem("MASK")]
			public List<MASKc> MASKLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				PORTNAME = string.Empty;
				LINERECIPENAME = string.Empty;
				MASKCARRIERNAME = string.Empty;
				VALIRESULTbool = false;
				MASKLIST = new List<MASKc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public ValidateMaskByCarrierReply()
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
