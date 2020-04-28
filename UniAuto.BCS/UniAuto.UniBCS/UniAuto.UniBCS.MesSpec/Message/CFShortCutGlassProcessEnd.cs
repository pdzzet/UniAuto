using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
	[XmlRoot("MESSAGE")]
	public class CFShortCutGlassProcessEnd : Message
	{
		public class PRODUCTc
		{
			public string POSITION { get; set; }

			public string PRODUCTNAME { get; set; }

			public string HOSTPRODUCTNAME { get; set; }

			public string DENSEBOXID { get; set; }

			public string PRODUCTJUDGE { get; set; }

			public string PRODUCTGRADE { get; set; }

			public string SUBPRODUCTGRADES { get; set; }

			public string PAIRPRODUCTNAME { get; set; }

			public string LOTNAME { get; set; }

			public string PRODUCTRECIPENAME { get; set; }

			public string HOSTPRODUCTRECIPENAME { get; set; }

			public string PRODUCTSPECNAME { get; set; }

			public string PROCESSOPERATIONNAME { get; set; }

			public string PRODUCTOWNER { get; set; }

			public string VCRREADFLAG { get; set; }

			[XmlArray("ABNORMALCODELIST")]
			[XmlArrayItem("CODE")]
			public List<CODEc> ABNORMALCODELIST { get; set; }

			[XmlIgnore]
			public bool HOLDFLAGbool { get; set; }

			public string HOLDFLAG
			{
				get{ return HOLDFLAGbool ? "Y" : "N"; }
				set{ HOLDFLAGbool = (string.Compare(value, "Y", true) == 0); }
			}

			public string HOLDMACHINE { get; set; }

			public string HOLDOPERATOR { get; set; }

			[XmlArray("PSHEIGHTLIST")]
			[XmlArrayItem("SITEVALUE")]
			public List<string> PSHEIGHTLIST { get; set; }

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

			public string SHORTCUTFLAG { get; set; }

			[XmlIgnore]
			public bool GMURAFLAGbool { get; set; }

			public string GMURAFLAG
			{
				get{ return GMURAFLAGbool ? "Y" : "N"; }
				set{ GMURAFLAGbool = (string.Compare(value, "Y", true) == 0); }
			}

			public string QTAPFLAG { get; set; }

			public string SAMPLEFLAG { get; set; }

			public string MASKNAME { get; set; }

			public string CHAMBERNAME { get; set; }

			[XmlIgnore]
			public bool PROCESSRESULTbool { get; set; }

			public string PROCESSRESULT
			{
				get{ return PROCESSRESULTbool ? "Y" : "N"; }
				set{ PROCESSRESULTbool = (string.Compare(value, "Y", true) == 0); }
			}

			public string ITOSIDEFLAG { get; set; }

			[XmlArray("MATERIALLIST")]
			[XmlArrayItem("MATERIAL")]
			public List<MATERIALc> MATERIALLIST { get; set; }

			public string SOURCEDURABLETYPE { get; set; }

			public string SAMPLETYPE { get; set; }

			public string USEDCOUNT { get; set; }

			public string CENGFLAG { get; set; }

			[XmlIgnore]
			public bool PROCESSFLAGbool { get; set; }

			public string PROCESSFLAG
			{
				get{ return PROCESSFLAGbool ? "Y" : "N"; }
				set{ PROCESSFLAGbool = (string.Compare(value, "Y", true) == 0); }
			}

			public string PROCESSCOMMUNICATIONSTATE { get; set; }

			public string CQLTFLAG { get; set; }

			public string FMAFLAG { get; set; }

			public string PPID { get; set; }

			public string HOSTPPID { get; set; }

            public string EXPOSUREDOPERATION { get; set; }

			public PRODUCTc()
			{
				POSITION = string.Empty;
				PRODUCTNAME = string.Empty;
				HOSTPRODUCTNAME = string.Empty;
				DENSEBOXID = string.Empty;
				PRODUCTJUDGE = string.Empty;
				PRODUCTGRADE = string.Empty;
				SUBPRODUCTGRADES = string.Empty;
				PAIRPRODUCTNAME = string.Empty;
				LOTNAME = string.Empty;
				PRODUCTRECIPENAME = string.Empty;
				HOSTPRODUCTRECIPENAME = string.Empty;
				PRODUCTSPECNAME = string.Empty;
				PROCESSOPERATIONNAME = string.Empty;
				PRODUCTOWNER = string.Empty;
				VCRREADFLAG = string.Empty;
				ABNORMALCODELIST = new List<CODEc>();
				HOLDFLAGbool = false;
				HOLDMACHINE = string.Empty;
				HOLDOPERATOR = string.Empty;
				PSHEIGHTLIST = new List<string>();
				DUMUSEDCOUNT = string.Empty;
				CFTYPE1REPAIRCOUNTint = 0;
				CFTYPE2REPAIRCOUNTint = 0;
				CARBONREPAIRCOUNTint = 0;
				LASERREPAIRCOUNTint = 0;
				SHORTCUTFLAG = string.Empty;
				GMURAFLAGbool = false;
				QTAPFLAG = string.Empty;
				SAMPLEFLAG = string.Empty;
				MASKNAME = string.Empty;
				CHAMBERNAME = string.Empty;
				PROCESSRESULTbool = false;
				ITOSIDEFLAG = string.Empty;
				MATERIALLIST = new List<MATERIALc>();
				SOURCEDURABLETYPE = string.Empty;
				SAMPLETYPE = string.Empty;
				USEDCOUNT = string.Empty;
				CENGFLAG = string.Empty;
				PROCESSFLAGbool = false;
				PROCESSCOMMUNICATIONSTATE = string.Empty;
				CQLTFLAG = string.Empty;
				FMAFLAG = string.Empty;
				PPID = string.Empty;
				HOSTPPID = string.Empty;
                EXPOSUREDOPERATION = string.Empty;
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

		public class MATERIALc
		{
			public string MATERIALTYPE { get; set; }

			public string MATERIALNAME { get; set; }

			public MATERIALc()
			{
				MATERIALTYPE = string.Empty;
				MATERIALNAME = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string LINERECIPENAME { get; set; }

			public string HOSTLINERECIPENAME { get; set; }

			public string PPID { get; set; }

			public string HOSTPPID { get; set; }

			public string PRODUCTQUANTITY { get; set; }

			[XmlIgnore]
			public DateTime TIMESTAMPdt { get; set; }

			public string TIMESTAMP
			{
				get { return this.TIMESTAMPdt.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo); }
				set
				{
					if(string.IsNullOrEmpty(value))
						this.TIMESTAMPdt = DateTime.MinValue;
					else
						this.TIMESTAMPdt = DateTime.Parse(value);
				}
			}

			[XmlIgnore]
			public bool CFSHORTCUTMODEbool { get; set; }

			public string CFSHORTCUTMODE
			{
				get{ return CFSHORTCUTMODEbool ? "Y" : "N"; }
				set{ CFSHORTCUTMODEbool = (string.Compare(value, "Y", true) == 0); }
			}

			public string PERMITFLAG { get; set; }

			[XmlArray("PRODUCTLIST")]
			[XmlArrayItem("PRODUCT")]
			public List<PRODUCTc> PRODUCTLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				LINERECIPENAME = string.Empty;
				HOSTLINERECIPENAME = string.Empty;
				PPID = string.Empty;
				HOSTPPID = string.Empty;
				PRODUCTQUANTITY = string.Empty;
				TIMESTAMPdt = DateTime.MinValue;
				CFSHORTCUTMODEbool = false;
				PERMITFLAG = string.Empty;
				PRODUCTLIST = new List<PRODUCTc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public CFShortCutGlassProcessEnd()
		{
			this.Direction = Spec.DirType.BC_TO_MES;
			this.WaitReply = "CFShortCutGlassProcessEndReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
