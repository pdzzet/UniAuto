using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class LocalCassetteDataReply : Message
	{
		public class LOTDATAc
		{
			public string LOTNAME { get; set; }

			public string PROCESSOPERATIONNAME { get; set; }

			public string PRODUCTOWNER { get; set; }

			public string PRODUCTSPECNAME { get; set; }

			public string CFREWORKCOUNT { get; set; }            

            public string TARGETCSTID_CF { get; set; }

            public string LINERECIPENAME { get; set; }

            public string PPID { get; set; }

            public string CSTSETTINGCODE { get; set; }

            public string CSTSETTINGCODE_CUT { get; set; }

            public string BCPRODUCTTYPE { get; set; }
            
            public string BCPRODUCTTYPE_CUT { get; set; }

            public string PRODUCTID { get; set; }

            public string PRODUCTID_CUT { get; set; }

			[XmlArray("PROCESSLINELIST")]
			[XmlArrayItem("PROCESSLINE")]
			public List<PROCESSLINEc> PROCESSLINELIST { get; set; }

			[XmlArray("STBPRODUCTSPECLIST")]
			[XmlArrayItem("STBPRODUCTSPEC")]
			public List<STBPRODUCTSPECc> STBPRODUCTSPECLIST { get; set; }

			[XmlArray("PRODUCTLIST")]
			[XmlArrayItem("PRODUCTDATA")]
			public List<PRODUCTDATAc> PRODUCTLIST { get; set; }

			public LOTDATAc()
			{
				LOTNAME = string.Empty;
				PROCESSOPERATIONNAME = string.Empty;
				PRODUCTOWNER = string.Empty;
				PRODUCTSPECNAME = string.Empty;
				CFREWORKCOUNT = string.Empty;                
                TARGETCSTID_CF = string.Empty;
				LINERECIPENAME = string.Empty;
				PPID = string.Empty;
                CSTSETTINGCODE = string.Empty;
                CSTSETTINGCODE_CUT= string.Empty ;
                BCPRODUCTTYPE = string.Empty;
                BCPRODUCTTYPE_CUT = string.Empty;
                PRODUCTID = string.Empty;
                PRODUCTID_CUT = string.Empty ;
				PROCESSLINELIST = new List<PROCESSLINEc>();
				STBPRODUCTSPECLIST = new List<STBPRODUCTSPECc>();
				PRODUCTLIST = new List<PRODUCTDATAc>();
			}
		}

		public class PROCESSLINEc
		{
			public string LINENAME { get; set; }

			public string LINERECIPENAME { get; set; }

			public string PPID { get; set; }

            public string CSTSETTINGCODE { get; set; }

            public string BCPRODUCTTYPE { get; set; }

            public string PRODUCTID { get; set; }

			public PROCESSLINEc()
			{
				LINENAME = string.Empty;
				LINERECIPENAME = string.Empty;
				PPID = string.Empty;
                CSTSETTINGCODE = string.Empty;
                BCPRODUCTTYPE = string.Empty;
                PRODUCTID = string.Empty;
			}
		}

		public class STBPRODUCTSPECc
		{
			public string LINENAME { get; set; }

			public string LINERECIPENAME { get; set; }

			public string PPID { get; set; }

            public string CSTSETTINGCODE { get; set; }

            public string BCPRODUCTTYPE { get; set; }

            public string PRODUCTID { get; set; }

			public STBPRODUCTSPECc()
			{
				LINENAME = string.Empty;
				LINERECIPENAME = string.Empty;
				PPID = string.Empty;
                CSTSETTINGCODE = string.Empty;
                BCPRODUCTTYPE = string.Empty;
                PRODUCTID = string.Empty;
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

			public string PROCESSTYPE { get; set; }

			public string TARGETCSTID { get; set; }

			public string NETWORKNO { get; set; }

			public string OWNERTYPE { get; set; }

			public string OWNERID { get; set; }

			public string REVPROCESSOPERATIONNAME { get; set; }

            public string TARGET_SLOTNO { get; set; }

            public string CFINLINEREWORKMAXCOUNT { get; set; }

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
				PROCESSTYPE = string.Empty;
				TARGETCSTID = string.Empty;
				NETWORKNO = string.Empty;
				OWNERTYPE = string.Empty;
				OWNERID = string.Empty;
				REVPROCESSOPERATIONNAME = string.Empty;
                TARGET_SLOTNO = string.Empty;
                CFINLINEREWORKMAXCOUNT = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string EQUIPMENTNO { get; set; }

			public string PORTNO { get; set; }

			public string PORTID { get; set; }

			public string CASSETTEID { get; set; }

			public string PRODUCTQUANTITY { get; set; }

            public string CSTSETTINGCODE { get; set; }

			[XmlArray("LOTLIST")]
			[XmlArrayItem("LOTDATA")]
			public List<LOTDATAc> LOTLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				EQUIPMENTNO = string.Empty;
				PORTNO = string.Empty;
				PORTID = string.Empty;
				CASSETTEID = string.Empty;
				PRODUCTQUANTITY = string.Empty;
                CSTSETTINGCODE = string.Empty;
				LOTLIST = new List<LOTDATAc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public LocalCassetteDataReply()
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
