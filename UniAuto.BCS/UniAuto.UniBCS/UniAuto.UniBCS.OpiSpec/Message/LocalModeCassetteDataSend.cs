using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class LocalModeCassetteDataSend : Message
	{
		public class LOTDATAc
		{
			public string LINERECIPENAME { get; set; }

			public string PPID { get; set; }

			public string CURRENTLINEPPID { get; set; }

			public string CROSSLINEPPID { get; set; }

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

			public string LOTNAME { get; set; }

			[XmlArray("PRODUCTLIST")]
			[XmlArrayItem("PRODUCTDATA")]
			public List<PRODUCTDATAc> PRODUCTLIST { get; set; }

			public LOTDATAc()
			{
				LINERECIPENAME = string.Empty;
				PPID = string.Empty;
				CURRENTLINEPPID = string.Empty;
				CROSSLINEPPID = string.Empty;
                CSTSETTINGCODE = string.Empty;
                CSTSETTINGCODE_CUT = string.Empty;
                BCPRODUCTTYPE = string.Empty;
                BCPRODUCTTYPE_CUT = string.Empty;
                PRODUCTID = string.Empty;
                PRODUCTID_CUT = string.Empty;
				PROCESSLINELIST = new List<PROCESSLINEc>();
				STBPRODUCTSPECLIST = new List<STBPRODUCTSPECc>();
				LOTNAME = string.Empty;
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

			public string PRODUCTRECIPENAME { get; set; }

			public string PPID { get; set; }

			public PRODUCTDATAc()
			{
				SLOTNO = string.Empty;
				PROCESSFLAGbool = false;
				PRODUCTRECIPENAME = string.Empty;
				PPID = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string EQUIPMENTNO { get; set; }

			public string PORTNO { get; set; }

			public string PORTID { get; set; }

			public string CASSETTEID { get; set; }

            public string REMAPFLAG { get; set; }

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
                REMAPFLAG = string.Empty;
                CSTSETTINGCODE = string.Empty;
				LOTLIST = new List<LOTDATAc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public LocalModeCassetteDataSend()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
			this.WaitReply = "LocalModeCassetteDataSendReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
