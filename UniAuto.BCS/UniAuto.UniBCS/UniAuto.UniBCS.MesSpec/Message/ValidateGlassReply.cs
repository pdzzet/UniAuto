using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class ValidateGlassReply : Message
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

			public string PRODUCTPROCESSTYPE { get; set; }

			public string PROCESSTYPE { get; set; }

			public string LINERECIPENAME { get; set; }

			public string NODESTACK { get; set; }

			public string PRODUCTSPECGROUP { get; set; }

			public string PRODUCTGCPTYPE { get; set; }

			[XmlArray("PROCESSLINELIST")]
			[XmlArrayItem("PROCESSLINE")]
			public List<PROCESSLINEc> PROCESSLINELIST { get; set; }

			[XmlArray("STBPRODUCTSPECLIST")]
			[XmlArrayItem("STBPRODUCTSPEC")]
			public List<STBPRODUCTSPECc> STBPRODUCTSPECLIST { get; set; }

			public string SUBPRODUCTSPECS { get; set; }

			public string SUBPRODUCTNAMES { get; set; }

			public string PRODUCTSIZETYPE { get; set; }

			public string PRODUCTSIZE { get; set; }

			public string ORIENTEDSITE { get; set; }

			public string ORIENTEDFACTORYNAME { get; set; }

			public string CURRENTSITE { get; set; }

			public string CURRENTFACTORYNAME { get; set; }

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
				PRODUCTPROCESSTYPE = string.Empty;
				PROCESSTYPE = string.Empty;
				LINERECIPENAME = string.Empty;
				NODESTACK = string.Empty;
				PRODUCTSPECGROUP = string.Empty;
				PRODUCTGCPTYPE = string.Empty;
				PROCESSLINELIST = new List<PROCESSLINEc>();
				STBPRODUCTSPECLIST = new List<STBPRODUCTSPECc>();
				SUBPRODUCTSPECS = string.Empty;
				SUBPRODUCTNAMES = string.Empty;
				PRODUCTSIZETYPE = string.Empty;
				PRODUCTSIZE = string.Empty;
				ORIENTEDSITE = string.Empty;
				ORIENTEDFACTORYNAME = string.Empty;
				CURRENTSITE = string.Empty;
				CURRENTFACTORYNAME = string.Empty;
				PRODUCTLIST = new List<PRODUCTc>();
			}
		}

		public class PROCESSLINEc
		{
			public string LINENAME { get; set; }

			public string PRODUCTSPECNAME { get; set; }

			public string PRODUCTSPECVER { get; set; }

			public string LINERECIPENAME { get; set; }

			public string PPID { get; set; }

			[XmlArray("MACHINEQTIMELIST")]
			[XmlArrayItem("MACHINEQTIME")]
			public List<MACHINEQTIMEc> MACHINEQTIMELIST { get; set; }

			public PROCESSLINEc()
			{
				LINENAME = string.Empty;
				PRODUCTSPECNAME = string.Empty;
				PRODUCTSPECVER = string.Empty;
				LINERECIPENAME = string.Empty;
				PPID = string.Empty;
				MACHINEQTIMELIST = new List<MACHINEQTIMEc>();
			}
		}

		public class STBPRODUCTSPECc
		{
			public string CUTPRODUCTSPECNAME { get; set; }

			public string CUTPRODUCTSPECVER { get; set; }

			public string PRODUCTSPECNAME { get; set; }

			public string PRODUCTSPECVER { get; set; }

			public string LINERECIPENAME { get; set; }

			public string PPID { get; set; }

			[XmlArray("MACHINEQTIMELIST")]
			[XmlArrayItem("MACHINEQTIME")]
			public List<MACHINEQTIMEc> MACHINEQTIMELIST { get; set; }

			public STBPRODUCTSPECc()
			{
				CUTPRODUCTSPECNAME = string.Empty;
				CUTPRODUCTSPECVER = string.Empty;
				PRODUCTSPECNAME = string.Empty;
				PRODUCTSPECVER = string.Empty;
				LINERECIPENAME = string.Empty;
				PPID = string.Empty;
				MACHINEQTIMELIST = new List<MACHINEQTIMEc>();
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

			[XmlArray("MACHINEQTIMELIST")]
			[XmlArrayItem("MACHINEQTIME")]
			public List<MACHINEQTIMEc> MACHINEQTIMELIST { get; set; }

			public string FMAFLAG { get; set; }

			public string MHUFLAG { get; set; }

			[XmlArray("ACDEFECTLIST")]
			[XmlArrayItem("ACDEFECT")]
			public List<ACDEFECTc> ACDEFECTLIST { get; set; }

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
				MACHINEQTIMELIST = new List<MACHINEQTIMEc>();
				FMAFLAG = string.Empty;
				MHUFLAG = string.Empty;
				ACDEFECTLIST = new List<ACDEFECTc>();
			}
		}

		public class MACHINEQTIMEc
		{
			public string STARTLOCALNO { get; set; }

			public string STARTUNITS { get; set; }

			public string STARTEVENT { get; set; }

			public string ENDLOCALNO { get; set; }

			public string ENDUNITS { get; set; }

			public string ENDEVENT { get; set; }

			public string QTIME { get; set; }

			public MACHINEQTIMEc()
			{
				STARTLOCALNO = string.Empty;
				STARTUNITS = string.Empty;
				STARTEVENT = string.Empty;
				ENDLOCALNO = string.Empty;
				ENDUNITS = string.Empty;
				ENDEVENT = string.Empty;
				QTIME = string.Empty;
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

		public class ACDEFECTc
		{
			public string ACDEFECTCODE { get; set; }

			public string ACDEFECTADDRESS { get; set; }

			public ACDEFECTc()
			{
				ACDEFECTCODE = string.Empty;
				ACDEFECTADDRESS = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string MACHINENAME { get; set; }

			public string LINERECIPENAME { get; set; }

			public string LINEOPERMODE { get; set; }

			public string SELECTEDPOSITIONMAP { get; set; }

			[XmlIgnore]
			public bool CLEANFLAGbool { get; set; }

			public string CLEANFLAG
			{
				get{ return CLEANFLAGbool ? "Y" : "N"; }
				set{ CLEANFLAGbool = (string.Compare(value, "Y", true) == 0); }
			}

			public string CARRIERSETCODE { get; set; }

			[XmlIgnore]
			public bool AOIBYPASSbool { get; set; }

			public string AOIBYPASS
			{
				get{ return AOIBYPASSbool ? "Y" : "N"; }
				set{ AOIBYPASSbool = (string.Compare(value, "Y", true) == 0); }
			}

			[XmlIgnore]
			public bool EXPSAMPLINGbool { get; set; }

			public string EXPSAMPLING
			{
				get{ return EXPSAMPLINGbool ? "Y" : "N"; }
				set{ EXPSAMPLINGbool = (string.Compare(value, "Y", true) == 0); }
			}

			[XmlIgnore]
			public bool AUTOCLAVESAMPLINGbool { get; set; }

			public string AUTOCLAVESAMPLING
			{
				get{ return AUTOCLAVESAMPLINGbool ? "Y" : "N"; }
				set{ AUTOCLAVESAMPLINGbool = (string.Compare(value, "Y", true) == 0); }
			}

			[XmlIgnore]
			public bool AUTOCLAVESKIPbool { get; set; }

			public string AUTOCLAVESKIP
			{
				get{ return AUTOCLAVESKIPbool ? "Y" : "N"; }
				set{ AUTOCLAVESKIPbool = (string.Compare(value, "Y", true) == 0); }
			}

			[XmlIgnore]
			public bool RECIPEPARAVALIDATIONFLAGbool { get; set; }

			public string RECIPEPARAVALIDATIONFLAG
			{
				get{ return RECIPEPARAVALIDATIONFLAGbool ? "Y" : "N"; }
				set{ RECIPEPARAVALIDATIONFLAGbool = (string.Compare(value, "Y", true) == 0); }
			}

			[XmlArray("RECIPEPARANOCHECKLIST")]
			[XmlArrayItem("MACHINENAME")]
			public List<string> RECIPEPARANOCHECKLIST { get; set; }

			public string PRODUCTQUANTITY { get; set; }

			public string PLANNEDPRODUCTSPECNAME { get; set; }

			public string PLANNEDSOURCEPART { get; set; }

			public string PLANNEDPROCESSOPERATIONNAME { get; set; }

			[XmlIgnore]
			public int PLANNEDQUANTITYint { get; set; }

			public string PLANNEDQUANTITY
			{
				get{ return PLANNEDQUANTITYint.ToString(); }
				set
				{
					int tmp = 0;
					if(int.TryParse(value, out tmp))
						PLANNEDQUANTITYint = tmp;
					else
						PLANNEDQUANTITYint = 0;
				}
			}

            public string PLANNEDGROUPNAME { get; set; }    //2015/9/14 add by Frank

			public string UPKOWNERTYPE { get; set; }

			[XmlArray("LOTLIST")]
			[XmlArrayItem("LOT")]
			public List<LOTc> LOTLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				MACHINENAME = string.Empty;
				LINERECIPENAME = string.Empty;
				LINEOPERMODE = string.Empty;
				SELECTEDPOSITIONMAP = string.Empty;
				CLEANFLAGbool = false;
				CARRIERSETCODE = string.Empty;
				AOIBYPASSbool = false;
				EXPSAMPLINGbool = false;
				AUTOCLAVESAMPLINGbool = false;
				AUTOCLAVESKIPbool = false;
				RECIPEPARAVALIDATIONFLAGbool = false;
				RECIPEPARANOCHECKLIST = new List<string>();
				PRODUCTQUANTITY = string.Empty;
				PLANNEDPRODUCTSPECNAME = string.Empty;
				PLANNEDSOURCEPART = string.Empty;
				PLANNEDPROCESSOPERATIONNAME = string.Empty;
				PLANNEDQUANTITYint = 0;
                PLANNEDGROUPNAME = string.Empty; //2015/9/14 add by Frank
				UPKOWNERTYPE = string.Empty;
				LOTLIST = new List<LOTc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public ValidateGlassReply()
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
