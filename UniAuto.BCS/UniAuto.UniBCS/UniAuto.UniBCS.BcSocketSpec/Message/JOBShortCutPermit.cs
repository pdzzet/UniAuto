using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.BcSocketSpec
{
	[XmlRoot("MESSAGE")]
	public class JOBShortCutPermit : Message
	{
		public class MESLOTc
		{
			public string LOTNAME { get; set; }

			public string PRODUCTSPECNAME { get; set; }

			public string PRODUCTSPECVER { get; set; }

			public string PROCESSFLOWNAME { get; set; }

			public string PROCESSOPERATIONNAME { get; set; }

			public string PRODUCTOWNER { get; set; }

			public string PRDCARRIERSETCODE { get; set; }

			public string SALEORDER { get; set; }

			public MESLOTc()
			{
				LOTNAME = string.Empty;
				PRODUCTSPECNAME = string.Empty;
				PRODUCTSPECVER = string.Empty;
				PROCESSFLOWNAME = string.Empty;
				PROCESSOPERATIONNAME = string.Empty;
				PRODUCTOWNER = string.Empty;
				PRDCARRIERSETCODE = string.Empty;
				SALEORDER = string.Empty;
			}
		}

		public class MESPRODUCTc
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

			[XmlArray("ABNORMALFLAGLIST")]
			[XmlArrayItem("ABNORMALFLAG")]
			public List<ABNORMALFLAGc> ABNORMALFLAGLIST { get; set; }

			public string GROUPID { get; set; }

			public string PRODUCTTYPE { get; set; }

			[XmlArray("LCDROPLIST")]
			[XmlArrayItem("LCDROP")]
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

			public string CFTYPE2REPAIRCOUNTINT { get; set; }

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

			public string LASERREPAIRCOUNTINT { get; set; }

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

			public string REWORKCOUNTINT { get; set; }

			public string SHORTCUTFLAG { get; set; }

			public string OWNERTYPE { get; set; }

			public string OWNERID { get; set; }

			public string REVPROCESSOPERATIONNAME { get; set; }

			public string TARGETPORTNAME { get; set; }

			public MESPRODUCTc()
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
				ABNORMALFLAGLIST = new List<ABNORMALFLAGc>();
				GROUPID = string.Empty;
				PRODUCTTYPE = string.Empty;
				LCDROPLIST = new List<string>();
				DUMUSEDCOUNT = string.Empty;
				CFTYPE1REPAIRCOUNTint = 0;
				CFTYPE2REPAIRCOUNTINT = string.Empty;
				CARBONREPAIRCOUNTint = 0;
				LASERREPAIRCOUNTINT = string.Empty;
				LASERREPAIRCOUNTint = 0;
				ITOSIDEFLAG = string.Empty;
				REWORKCOUNTINT = string.Empty;
				SHORTCUTFLAG = string.Empty;
				OWNERTYPE = string.Empty;
				OWNERID = string.Empty;
				REVPROCESSOPERATIONNAME = string.Empty;
				TARGETPORTNAME = string.Empty;
			}
		}

		public class CFSPECIALc
		{
			public string RTCFLAG { get; set; }

			public string LOADERBUFFERINGFLAG { get; set; }

			public string CFSPECIALRESERVED { get; set; }

			public string SOURCEPORTNO { get; set; }

			public string TARGETPORTNO { get; set; }

			public string COAVERSION { get; set; }

			public string PREINLINEID { get; set; }

			public string DUMMYUSEDCOUNT { get; set; }

			public string TARGETCSTID { get; set; }

			public string COATERCSPNO { get; set; }

			public CFSPECIALc()
			{
				RTCFLAG = string.Empty;
				LOADERBUFFERINGFLAG = string.Empty;
				CFSPECIALRESERVED = string.Empty;
				SOURCEPORTNO = string.Empty;
				TARGETPORTNO = string.Empty;
				COAVERSION = string.Empty;
				PREINLINEID = string.Empty;
				DUMMYUSEDCOUNT = string.Empty;
				TARGETCSTID = string.Empty;
				COATERCSPNO = string.Empty;
			}
		}

		public class ABNORMALFLAGc
		{
			public string VNAME { get; set; }

			public string VVALUE { get; set; }

			public ABNORMALFLAGc()
			{
				VNAME = string.Empty;
				VVALUE = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string CASSETTESEQNO { get; set; }

			public string JOBSEQNO { get; set; }

			public string JOBID { get; set; }

			public string GROUPINDEX { get; set; }

			public string PRODUCTTYPE { get; set; }

			public string CSTOPERATIONMODE { get; set; }

			public string SUBSTRATETYPE { get; set; }

			public string CIMMODE { get; set; }

			public string JOBTYPE { get; set; }

			public string JOBJUDGE { get; set; }

			public string SAMPLINGSLOTFLAG { get; set; }

			public string OXINFORMATIONREQFLAG { get; set; }

			public string FIRSTRUN { get; set; }

			public string JOBGRADE { get; set; }

			public string GLASSCHIPMASKCUTID { get; set; }

			public string PPID { get; set; }

			public string INSPRESERVATIONS { get; set; }

			public string EQPRESERVATIONS { get; set; }

			public string LASTGLASSFLAG { get; set; }

			public string INSPJUDGEDDATA { get; set; }

			public string TRACKINGDATA { get; set; }

			public string EQPFLAG { get; set; }

			public string OXRINFORMATION { get; set; }

			public string CHIPCOUNT { get; set; }

			public string VCRJOBID { get; set; }

			public string EQPJOBID { get; set; }

			public string MESJOBID { get; set; }

			public string FROMCSTID { get; set; }

			public string FROMSLOTNO { get; set; }

			public string MESLOTNAME { get; set; }

			public MESLOTc MESLOT { get; set; }

			public MESPRODUCTc MESPRODUCT { get; set; }

			public CFSPECIALc CFSPECIAL { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				CASSETTESEQNO = string.Empty;
				JOBSEQNO = string.Empty;
				JOBID = string.Empty;
				GROUPINDEX = string.Empty;
				PRODUCTTYPE = string.Empty;
				CSTOPERATIONMODE = string.Empty;
				SUBSTRATETYPE = string.Empty;
				CIMMODE = string.Empty;
				JOBTYPE = string.Empty;
				JOBJUDGE = string.Empty;
				SAMPLINGSLOTFLAG = string.Empty;
				OXINFORMATIONREQFLAG = string.Empty;
				FIRSTRUN = string.Empty;
				JOBGRADE = string.Empty;
				GLASSCHIPMASKCUTID = string.Empty;
				PPID = string.Empty;
				INSPRESERVATIONS = string.Empty;
				EQPRESERVATIONS = string.Empty;
				LASTGLASSFLAG = string.Empty;
				INSPJUDGEDDATA = string.Empty;
				TRACKINGDATA = string.Empty;
				EQPFLAG = string.Empty;
				OXRINFORMATION = string.Empty;
				CHIPCOUNT = string.Empty;
				VCRJOBID = string.Empty;
				EQPJOBID = string.Empty;
				MESJOBID = string.Empty;
				FROMCSTID = string.Empty;
				FROMSLOTNO = string.Empty;
				MESLOTNAME = string.Empty;
				MESLOT = new MESLOTc();
				MESPRODUCT = new MESPRODUCTc();
				CFSPECIAL = new CFSPECIALc();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public JOBShortCutPermit()
		{
			this.Direction = Spec.DirType.ACTIVE_TO_PASSIVE;
			this.WaitReply = "JOBShortCutPermitReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
