using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class EquipmentStatusReply : Message
	{
		public class SAMPLINGSIDEc
		{
			public string ITEMNAME { get; set; }

			public string SIDESTATUS { get; set; }

			public SAMPLINGSIDEc()
			{
				ITEMNAME = string.Empty;
				SIDESTATUS = string.Empty;
			}
		}

		public class VCRc
		{
			public string VCRNO { get; set; }

			public string VCRENABLEMODE { get; set; }

			public VCRc()
			{
				VCRNO = string.Empty;
				VCRENABLEMODE = string.Empty;
			}
		}

		public class INTERLOCKc
		{
			public string INTERLOCKNO { get; set; }

			public string INTERLOCKSTATUS { get; set; }

			public INTERLOCKc()
			{
				INTERLOCKNO = string.Empty;
				INTERLOCKSTATUS = string.Empty;
			}
		}

		public class UNITc
		{
			public string UNITNO { get; set; }

			public string UNITID { get; set; }

			public string CURRENTSTATUS { get; set; }

			public string TFTJOBCNT { get; set; }

			public string CFJOBCNT { get; set; }

            public string UNITRUNMODE { get; set; }

            public string HSMSSTATUS { get; set; }

            #region For CELL Buffer
            public string BF_WARNING_GLASS_COUNT { get; set; }

            public string BF_CURRENT_COUNT { get; set; }

            public string BF_TOTAL_SLOT_COUNT { get; set; }

            public string BF_WARNING_STATUS { get; set; }

            public string BF_STORE_OVER_ALIVE { get; set; }
            #endregion

			public UNITc()
			{
				UNITNO = string.Empty;
				UNITID = string.Empty;
				CURRENTSTATUS = string.Empty;
				TFTJOBCNT = string.Empty;
				CFJOBCNT = string.Empty;
                UNITRUNMODE = string.Empty;
                HSMSSTATUS = string.Empty;
                BF_WARNING_GLASS_COUNT = string.Empty;
                BF_CURRENT_COUNT = string.Empty;
                BF_TOTAL_SLOT_COUNT = string.Empty;
                BF_WARNING_STATUS = string.Empty;
                BF_STORE_OVER_ALIVE = string.Empty;
			}
		}

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string EQUIPMENTNO { get; set; }

			public string EQUIPMENTALIVE { get; set; }

			public string CIMMODE { get; set; }

			public string HSMSSTATUS { get; set; }

			public string HSMSCONTROLMODE { get; set; }

			public string UPSTREAMINLINEMODE { get; set; }

			public string DOWNSTREAMINLINEMODE { get; set; }

			public string LOCALALARMSTATUS { get; set; }

			public string AUTORECIPECHANGEMODE { get; set; }

			public string PARTIALFULLMODE { get; set; }

			public string BYPASSMODE { get; set; }

			public string TURNTABLEMODE { get; set; }

			public string CURRENTSTATUS { get; set; }

			public string CSTOPERMODE { get; set; }

			public string CURRENTRECIPEID { get; set; }

			public string EQUIPMENTRUNMODE { get; set; }

            //public string EQUIPMENTRUNMODE2 { get; set; }

			public string TFTJOBCNT { get; set; }

			public string CFJOBCNT { get; set; }

			public string DMYJOBCNT { get; set; }

			public string THROUGHDMYJOBCNT { get; set; }

			public string THICKNESSDMYJOBCNT { get; set; }

            public string UNASSEMBLEDTFTDMYJOBCNT { get; set; }//sy add 20160826

            public string ITODMYJOBCNT { get; set; }//sy add 20160826

            public string NIPDMYJOBCNT { get; set; }//sy add 20160826

            public string METALONEDMYJOBCNT { get; set; }//sy add 20160826

			public string UVMASKJOBCNT { get; set; }

			public string CASSETTEQTIME { get; set; }

			public string INSPECTIONIDLETIME { get; set; }

			public string EQPOPERATIONMODE { get; set; }

			public string BYPASSINSP01MODE { get; set; }

			public string BYPASSINSP02MODE { get; set; }

			public string HIGHCVMODE { get; set; }

			public string NEXTLINEBCSTATUS { get; set; }

			public string JOBDATACHECKMODE { get; set; }

			public string COAVERSIONCHECKMODE { get; set; }

			public string JOBDUPLICATECHECKMODE { get; set; }

			public string PRODUCTIDCHECKMODE { get; set; }

			public string GROUPINDEXCHECKMODE { get; set; }

			public string RECIPEIDCHECKMODE { get; set; }

            public string PRODUCTTYPECHECKMODE { get; set; }

            public string MATERIALSTATUS { get; set; }

            public string LASTGLASSID { get; set; }

            public string LASTRECIVETIME { get; set; }

            public string TIMEOUTFLAG { get; set; }

            public string CV07_STATUS { get; set; }

			[XmlArray("SAMPLINGSIDELIST")]
			[XmlArrayItem("SAMPLINGSIDE")]
			public List<SAMPLINGSIDEc> SAMPLINGSIDELIST { get; set; }

			[XmlArray("VCRLIST")]
			[XmlArrayItem("VCR")]
			public List<VCRc> VCRLIST { get; set; }

			[XmlArray("INTERLOCKLIST")]
			[XmlArrayItem("INTERLOCK")]
			public List<INTERLOCKc> INTERLOCKLIST { get; set; }

			[XmlArray("UNITLIST")]
			[XmlArrayItem("UNIT")]
			public List<UNITc> UNITLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				EQUIPMENTNO = string.Empty;
				EQUIPMENTALIVE = string.Empty;
				CIMMODE = string.Empty;
				HSMSSTATUS = string.Empty;
				HSMSCONTROLMODE = string.Empty;
				UPSTREAMINLINEMODE = string.Empty;
				DOWNSTREAMINLINEMODE = string.Empty;
				LOCALALARMSTATUS = string.Empty;
				AUTORECIPECHANGEMODE = string.Empty;
				PARTIALFULLMODE = string.Empty;
				BYPASSMODE = string.Empty;
				TURNTABLEMODE = string.Empty;
				CURRENTSTATUS = string.Empty;
				CSTOPERMODE = string.Empty;
				CURRENTRECIPEID = string.Empty;
				EQUIPMENTRUNMODE = string.Empty;
				TFTJOBCNT = string.Empty;
				CFJOBCNT = string.Empty;
				DMYJOBCNT = string.Empty;
				THROUGHDMYJOBCNT = string.Empty;
                THICKNESSDMYJOBCNT = string.Empty;
                UNASSEMBLEDTFTDMYJOBCNT = string.Empty;//sy add 20160826
                ITODMYJOBCNT = string.Empty;//sy add 20160826
                NIPDMYJOBCNT = string.Empty;//sy add 20160826
                METALONEDMYJOBCNT = string.Empty;//sy add 20160826
				UVMASKJOBCNT = string.Empty;
				CASSETTEQTIME = string.Empty;
				INSPECTIONIDLETIME = string.Empty;
				EQPOPERATIONMODE = string.Empty;
				BYPASSINSP01MODE = string.Empty;
				BYPASSINSP02MODE = string.Empty;
				HIGHCVMODE = string.Empty;
				NEXTLINEBCSTATUS = string.Empty;
				JOBDATACHECKMODE = string.Empty;
				COAVERSIONCHECKMODE = string.Empty;
				JOBDUPLICATECHECKMODE = string.Empty;
				PRODUCTIDCHECKMODE = string.Empty;
				GROUPINDEXCHECKMODE = string.Empty;
				RECIPEIDCHECKMODE = string.Empty;
                PRODUCTTYPECHECKMODE = string.Empty;
                MATERIALSTATUS = string.Empty;
                LASTGLASSID = string.Empty;
                LASTRECIVETIME = string.Empty;
                TIMEOUTFLAG = string.Empty;
                CV07_STATUS = string.Empty;

				SAMPLINGSIDELIST = new List<SAMPLINGSIDEc>();
				VCRLIST = new List<VCRc>();
				INTERLOCKLIST = new List<INTERLOCKc>();
				UNITLIST = new List<UNITc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public EquipmentStatusReply()
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
