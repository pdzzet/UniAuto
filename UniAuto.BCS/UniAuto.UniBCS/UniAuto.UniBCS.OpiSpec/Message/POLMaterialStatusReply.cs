using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class POLMaterialStatusReply : Message
	{
		public class EQUIPMENTc
		{
			public string EQUIPMENTNO { get; set; }

			[XmlArray("MATERIALLIST")]
			[XmlArrayItem("MATERIAL")]
			public List<MATERIALc> MATERIALLIST { get; set; }

			public EQUIPMENTc()
			{
				EQUIPMENTNO = string.Empty;
				MATERIALLIST = new List<MATERIALc>();
			}
		}

		public class MATERIALc
		{
			public string UNITNO { get; set; }

            public string RECIPEID { get; set; }

			public string MATERIALID { get; set; }

			public string MATERIAL_STATUS { get; set; }

            public string OPERATIONERID { get; set; }

            public string LOTID01 { get; set; }

            public string LOTNO01 { get; set; }

            public string COUNT01 { get; set; }

            public string LOTID02 { get; set; }

            public string LOTNO02 { get; set; }

            public string COUNT02 { get; set; }

            public string LOTID03 { get; set; }

            public string LOTNO03 { get; set; }

            public string COUNT03 { get; set; }

            public string LOTID04 { get; set; }

            public string LOTNO04 { get; set; }

            public string COUNT04 { get; set; }			

            public string LOTID05 { get; set; }

            public string LOTNO05 { get; set; }

            public string COUNT05 { get; set; }	
	
            public string PORTID { get; set; }		

			public MATERIALc()
			{
                UNITNO = string.Empty;
				MATERIALID = string.Empty;
                MATERIAL_STATUS = string.Empty;
                OPERATIONERID = string.Empty;
                LOTID01 = string.Empty;
				LOTNO01 = string.Empty;
                COUNT01 = string.Empty;
                LOTID02 = string.Empty;
                LOTNO02 = string.Empty;
                COUNT02 = string.Empty;
                LOTID03 = string.Empty;
                LOTNO03 = string.Empty;
                COUNT03 = string.Empty;
                LOTID04= string.Empty;
                LOTNO04 = string.Empty;
                COUNT04 = string.Empty;
                LOTID05 = string.Empty;
                LOTNO05 = string.Empty;
                COUNT05 = string.Empty;				
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			[XmlArray("EQUIPMENTLIST")]
			[XmlArrayItem("EQUIPMENT")]
			public List<EQUIPMENTc> EQUIPMENTLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				EQUIPMENTLIST = new List<EQUIPMENTc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public POLMaterialStatusReply()
		{
			this.Direction = Spec.DirType.BC_TO_OPI;
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
