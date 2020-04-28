using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class RecipeRegisterRequestReply : Message
	{
		public class EQREGISTERRESULTc
		{
			public string EQUIPMENTID { get; set; }

			public string RECIPENO { get; set; }

			public string RETURNTEXT { get; set; }

			public EQREGISTERRESULTc()
			{
				EQUIPMENTID = string.Empty;
				RECIPENO = string.Empty;
				RETURNTEXT = string.Empty;
			}
		}

		public class RECIPENUMBERc
		{
			public string EQUIPMENTID { get; set; }

			public string RECIPENUMBER { get; set; }

			public string LOCALNAME { get; set; }

			public RECIPENUMBERc()
			{
				EQUIPMENTID = string.Empty;
				RECIPENUMBER = string.Empty;
				LOCALNAME = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string LINERECIPENAME { get; set; }

			public string BCREGISTERRESULT { get; set; }

			[XmlArray("EQREGISTERRESULTLIST")]
			[XmlArrayItem("EQREGISTERRESULT")]
			public List<EQREGISTERRESULTc> EQREGISTERRESULTLIST { get; set; }

			[XmlArray("RECIPENUMBERLIST")]
			[XmlArrayItem("RECIPENUMBER")]
			public List<RECIPENUMBERc> RECIPENUMBERLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				LINERECIPENAME = string.Empty;
				BCREGISTERRESULT = string.Empty;
				EQREGISTERRESULTLIST = new List<EQREGISTERRESULTc>();
				RECIPENUMBERLIST = new List<RECIPENUMBERc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public RecipeRegisterRequestReply()
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
