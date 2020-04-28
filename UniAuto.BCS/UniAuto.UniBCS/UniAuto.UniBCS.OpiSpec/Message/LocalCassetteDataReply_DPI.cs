using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class LocalCassetteDataReply_DPI : Message
    {
        public class PORTc
        {
            public string PORTNO { get; set; }

            public string PORTID { get; set; }

            public string CASSETTEID { get; set; }

            public string CSTSETTINGCODE { get; set; }

            public string PRODUCTQUANTITY { get; set; }

            public LOTDATAc LOTDATA { get; set; }

            public PORTc()
            {
                PORTNO = string.Empty;
                PORTID = string.Empty;
                CASSETTEID = string.Empty;
                CSTSETTINGCODE = string.Empty;
                PRODUCTQUANTITY = string.Empty;
                LOTDATA = new LOTDATAc();
            }
        }

        public class LOTDATAc
		{
			public string LOTNAME { get; set; }

			public string PROCESSOPERATIONNAME { get; set; }

			public string PRODUCTOWNER { get; set; }

			public string PRODUCTSPECNAME { get; set; }

			public string BCPRODUCTTYPE { get; set; }

            public string CSTSETTINGCODE { get; set; }

			[XmlArray("PRODUCTLIST")]
			[XmlArrayItem("PRODUCTDATA")]
			public List<PRODUCTDATAc> PRODUCTLIST { get; set; }

			public LOTDATAc()
			{
				LOTNAME = string.Empty;
				PROCESSOPERATIONNAME = string.Empty;
				PRODUCTOWNER = string.Empty;
				PRODUCTSPECNAME = string.Empty;
				BCPRODUCTTYPE = string.Empty;
                CSTSETTINGCODE = string.Empty;
				PRODUCTLIST = new List<PRODUCTDATAc>();
			}
		}

		public class PRODUCTDATAc
		{
			public string SLOTNO { get; set; }

			[XmlIgnore]
			public bool PROCESSFLAGbool { get; set; }

			public string PROCESSFLAG
			{
				get{ return PROCESSFLAGbool ? "Y" : "N"; }
				set{ PROCESSFLAGbool = (string.Compare(value, "Y", true) == 0); }
			}

			public string PRODUCTNAME { get; set; }

			public string PRODUCTRECIPENAME { get; set; }

			public string PPID { get; set; }

			public string PRODUCTTYPE { get; set; }

			public string PRODUCTGRADE { get; set; }

			public string PRODUCTJUDGE { get; set; }

			public string GROUPID { get; set; }

			public string SUBPRODUCTGRADES { get; set; }

			public string ABNORMALCODE { get; set; }

			public string OWNERTYPE { get; set; }

			public string OWNERID { get; set; }

			public string REVPROCESSOPERATIONNAME { get; set; }

			public PRODUCTDATAc()
			{
				SLOTNO = string.Empty;
				PROCESSFLAGbool = false;
				PRODUCTNAME = string.Empty;
				PRODUCTRECIPENAME = string.Empty;
				PPID = string.Empty;
				PRODUCTTYPE = string.Empty;
				PRODUCTGRADE = string.Empty;
				PRODUCTJUDGE = string.Empty;
				GROUPID = string.Empty;
				SUBPRODUCTGRADES = string.Empty;
				ABNORMALCODE = string.Empty;
				OWNERTYPE = string.Empty;
				OWNERID = string.Empty;
				REVPROCESSOPERATIONNAME = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string EQUIPMENTNO { get; set; }

            [XmlArray("PORTLIST")]
            [XmlArrayItem("PORT")]
            public List<PORTc> PORTLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				EQUIPMENTNO = string.Empty;
                PORTLIST = new List<PORTc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public LocalCassetteDataReply_DPI()
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
