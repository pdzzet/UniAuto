using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class CFShortCutPermitReply : Message
	{
		public class LOTc
		{
			public string LOTNAME { get; set; }

			public string PRODUCTSPECNAME { get; set; }

			public string PRODUCTSPECVER { get; set; }

			public string PROCESSFLOWNAME { get; set; }

			public string PROCESSOPERATIONNAME { get; set; }

			public string PRODUCTOWNER { get; set; }

			public string PRDCARRIERSETCODE { get; set; }

			public string SALEORDER { get; set; }

			public string PRODUCTSIZETYPE { get; set; }

			public string PRODUCTSIZE { get; set; }

			public string BCPRODUCTTYPE { get; set; }

			public string BCPRODUCTID { get; set; }

			public string PRODUCTPROCESSTYPE { get; set; }

			public string PROCESSTYPE { get; set; }

			public string LINERECIPENAME { get; set; }

			public string PPID { get; set; }

			public string SUBPRODUCTSPECS { get; set; }

			public string SUBPRODUCTNAMES { get; set; }

			public string SUBPRODUCTLINES { get; set; }

			public string SUBPRODUCTSIZETYPES { get; set; }

			public string SUBPRODUCTSIZES { get; set; }

			public string ORIENTEDSITE { get; set; }

			public string ORIENTEDFACTORYNAME { get; set; }

			public string CURRENTSITE { get; set; }

			public string CURRENTFACTORYNAME { get; set; }

			public string PRODUCTTHICKNESS { get; set; }

			[XmlArray("PRODUCTLIST")]
			[XmlArrayItem("PRODUCT")]
			public List<PRODUCTc> PRODUCTLIST { get; set; }

			public LOTc()
			{
				LOTNAME = string.Empty;
				PRODUCTSPECNAME = string.Empty;
				PRODUCTSPECVER = string.Empty;
				PROCESSFLOWNAME = string.Empty;
				PROCESSOPERATIONNAME = string.Empty;
				PRODUCTOWNER = string.Empty;
				PRDCARRIERSETCODE = string.Empty;
				SALEORDER = string.Empty;
				PRODUCTSIZETYPE = string.Empty;
				PRODUCTSIZE = string.Empty;
				BCPRODUCTTYPE = string.Empty;
				BCPRODUCTID = string.Empty;
				PRODUCTPROCESSTYPE = string.Empty;
				PROCESSTYPE = string.Empty;
				LINERECIPENAME = string.Empty;
				PPID = string.Empty;
				SUBPRODUCTSPECS = string.Empty;
				SUBPRODUCTNAMES = string.Empty;
				SUBPRODUCTLINES = string.Empty;
				SUBPRODUCTSIZETYPES = string.Empty;
				SUBPRODUCTSIZES = string.Empty;
				ORIENTEDSITE = string.Empty;
				ORIENTEDFACTORYNAME = string.Empty;
				CURRENTSITE = string.Empty;
				CURRENTFACTORYNAME = string.Empty;
				PRODUCTTHICKNESS = string.Empty;
				PRODUCTLIST = new List<PRODUCTc>();
			}
		}

		public class PRODUCTc
		{
			public string POSITION { get; set; }

			public string PRODUCTNAME { get; set; }

			public string ARRAYPRODUCTNAME { get; set; }

			public string CFPRODUCTNAME { get; set; }

			public string ARRAYPRODUCTSPECNAME { get; set; }

			public string ARRAYLOTNAME { get; set; }

			public string DENSEBOXID { get; set; }

			public string PRODUCTJUDGE { get; set; }

			public string PRODUCTGRADE { get; set; }

			public string SOURCEPART { get; set; }

			public string PRODUCTRECIPENAME { get; set; }

			public string SUBPRODUCTGRADES { get; set; }

			public string SUBPRODUCTDEFECTCODE { get; set; }

			public string SUBPRODUCTJPSGRADE { get; set; }

			public string SUBPRODUCTJPSCODE { get; set; }

			public string SUBPRODUCTJPSFLAG { get; set; }

			public string ARRAYSUBPRODUCTGRADE { get; set; }

			public string CFSUBPRODUCTGRADE { get; set; }

			[XmlArray("ABNORMALCODELIST")]
			[XmlArrayItem("CODE")]
			public List<CODEc> ABNORMALCODELIST { get; set; }

			public string GROUPID { get; set; }

			public string PRODUCTTYPE { get; set; }

			[XmlArray("LCDROPLIST")]
			[XmlArrayItem("LCDROPAMOUNT")]
			public List<string> LCDROPLIST { get; set; }

			public string DUMUSEDCOUNT { get; set; }

			[XmlIgnore]
			public int CFTYPE1REPAIRCOUNTint { get; set; }

			public string CFTYPE1REPAIRCOUNT
			{
				get{ return CFTYPE1REPAIRCOUNTint.ToString(); }
				set
				{
					int tmp = 0;
					if(int.TryParse(value, out tmp))
						CFTYPE1REPAIRCOUNTint = tmp;
					else
						CFTYPE1REPAIRCOUNTint = 0;
				}
			}

			[XmlIgnore]
			public int CFTYPE2REPAIRCOUNTint { get; set; }

			public string CFTYPE2REPAIRCOUNT
			{
				get{ return CFTYPE2REPAIRCOUNTint.ToString(); }
				set
				{
					int tmp = 0;
					if(int.TryParse(value, out tmp))
						CFTYPE2REPAIRCOUNTint = tmp;
					else
						CFTYPE2REPAIRCOUNTint = 0;
				}
			}

			[XmlIgnore]
			public int CARBONREPAIRCOUNTint { get; set; }

			public string CARBONREPAIRCOUNT
			{
				get{ return CARBONREPAIRCOUNTint.ToString(); }
				set
				{
					int tmp = 0;
					if(int.TryParse(value, out tmp))
						CARBONREPAIRCOUNTint = tmp;
					else
						CARBONREPAIRCOUNTint = 0;
				}
			}

			[XmlIgnore]
			public int LASERREPAIRCOUNTint { get; set; }

			public string LASERREPAIRCOUNT
			{
				get{ return LASERREPAIRCOUNTint.ToString(); }
				set
				{
					int tmp = 0;
					if(int.TryParse(value, out tmp))
						LASERREPAIRCOUNTint = tmp;
					else
						LASERREPAIRCOUNTint = 0;
				}
			}

			public string ITOSIDEFLAG { get; set; }

			[XmlIgnore]
			public int REWORKCOUNTint { get; set; }

			public string REWORKCOUNT
			{
				get{ return REWORKCOUNTint.ToString(); }
				set
				{
					int tmp = 0;
					if(int.TryParse(value, out tmp))
						REWORKCOUNTint = tmp;
					else
						REWORKCOUNTint = 0;
				}
			}

			[XmlArray("REWORKLIST")]
			[XmlArrayItem("REWORK")]
			public List<REWORKc> REWORKLIST { get; set; }

			public string SHORTCUTFLAG { get; set; }

			public string OWNERTYPE { get; set; }

			public string OWNERID { get; set; }

			public string REVPROCESSOPERATIONNAME { get; set; }

			public string TARGETPORTNAME { get; set; }

			public string CHAMBERRUNMODE { get; set; }

			public string TEMPERATUREFLAG { get; set; }

			public string MACHINEPROCESSSEQ { get; set; }

			public string SCRAPCUTFLAG { get; set; }

			public string PPID { get; set; }

			public string FMAFLAG { get; set; }

			public string MHUFLAG { get; set; }

			[XmlArray("DEFECTLIST")]
			[XmlArrayItem("DEFECT")]
			public List<DEFECTc> DEFECTLIST { get; set; }

			public string ARRAYPRODUCTSPECVER { get; set; }

			public string AGINGENABLE { get; set; }

			[XmlIgnore]
			public bool PROCESSFLAGbool { get; set; }

			public string PROCESSFLAG
			{
				get{ return PROCESSFLAGbool ? "Y" : "N"; }
				set{ PROCESSFLAGbool = (string.Compare(value, "Y", true) == 0); }
			}

			public PRODUCTc()
			{
				POSITION = string.Empty;
				PRODUCTNAME = string.Empty;
				ARRAYPRODUCTNAME = string.Empty;
				CFPRODUCTNAME = string.Empty;
				ARRAYPRODUCTSPECNAME = string.Empty;
				ARRAYLOTNAME = string.Empty;
				DENSEBOXID = string.Empty;
				PRODUCTJUDGE = string.Empty;
				PRODUCTGRADE = string.Empty;
				SOURCEPART = string.Empty;
				PRODUCTRECIPENAME = string.Empty;
				SUBPRODUCTGRADES = string.Empty;
				SUBPRODUCTDEFECTCODE = string.Empty;
				SUBPRODUCTJPSGRADE = string.Empty;
				SUBPRODUCTJPSCODE = string.Empty;
				SUBPRODUCTJPSFLAG = string.Empty;
				ARRAYSUBPRODUCTGRADE = string.Empty;
				CFSUBPRODUCTGRADE = string.Empty;
				ABNORMALCODELIST = new List<CODEc>();
				GROUPID = string.Empty;
				PRODUCTTYPE = string.Empty;
				LCDROPLIST = new List<string>();
				DUMUSEDCOUNT = string.Empty;
				CFTYPE1REPAIRCOUNTint = 0;
				CFTYPE2REPAIRCOUNTint = 0;
				CARBONREPAIRCOUNTint = 0;
				LASERREPAIRCOUNTint = 0;
				ITOSIDEFLAG = string.Empty;
				REWORKCOUNTint = 0;
				REWORKLIST = new List<REWORKc>();
				SHORTCUTFLAG = string.Empty;
				OWNERTYPE = string.Empty;
				OWNERID = string.Empty;
				REVPROCESSOPERATIONNAME = string.Empty;
				TARGETPORTNAME = string.Empty;
				CHAMBERRUNMODE = string.Empty;
				TEMPERATUREFLAG = string.Empty;
				MACHINEPROCESSSEQ = string.Empty;
				SCRAPCUTFLAG = string.Empty;
				PPID = string.Empty;
				FMAFLAG = string.Empty;
				MHUFLAG = string.Empty;
				DEFECTLIST = new List<DEFECTc>();
				ARRAYPRODUCTSPECVER = string.Empty;
				AGINGENABLE = string.Empty;
				PROCESSFLAGbool = false;
			}
		}

		public class CODEc
		{
            public string ABNORMALSEQ { get; set; }

			public string ABNORMALCODE { get; set; }

			public CODEc()
			{
				ABNORMALSEQ = string.Empty;
				ABNORMALCODE = string.Empty;
			}
		}

		public class REWORKc
		{
			public string REWORKFLOWNAME { get; set; }

			[XmlIgnore]
			public int REWORKCOUNTint { get; set; }

			public string REWORKCOUNT
			{
				get{ return REWORKCOUNTint.ToString(); }
				set
				{
					int tmp = 0;
					if(int.TryParse(value, out tmp))
						REWORKCOUNTint = tmp;
					else
						REWORKCOUNTint = 0;
				}
			}

			public REWORKc()
			{
				REWORKFLOWNAME = string.Empty;
				REWORKCOUNTint = 0;
			}
		}

		public class DEFECTc
		{
			public string SUBPRODUCTNAME { get; set; }

			public string TFTDEFECTCODES { get; set; }

			public string TFTDEFECTADDRESS { get; set; }

			public string CFDEFECTCODES { get; set; }

			public string CFDEFECTADDRESS { get; set; }

			public DEFECTc()
			{
				SUBPRODUCTNAME = string.Empty;
				TFTDEFECTCODES = string.Empty;
				TFTDEFECTADDRESS = string.Empty;
				CFDEFECTCODES = string.Empty;
				CFDEFECTADDRESS = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string LINERECIPENAME { get; set; }

			public string SELECTEDPOSITIONMAP { get; set; }

			public string PERMITFLAG { get; set; }

			[XmlArray("LOTLIST")]
			[XmlArrayItem("LOT")]
			public List<LOTc> LOTLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				LINERECIPENAME = string.Empty;
				SELECTEDPOSITIONMAP = string.Empty;
				PERMITFLAG = string.Empty;
				LOTLIST = new List<LOTc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public CFShortCutPermitReply()
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
