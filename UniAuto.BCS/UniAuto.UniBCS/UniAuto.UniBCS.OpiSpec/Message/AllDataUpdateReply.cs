using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class AllDataUpdateReply : Message
	{
		public class EQUIPMENTc
		{
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

			[XmlArray("PORTLIST")]
			[XmlArrayItem("PORT")]
			public List<PORTc> PORTLIST { get; set; }

            [XmlArray("DENSEBOXLIST")]
            [XmlArrayItem("DENSEBOX")]
            public List<DENSEBOXc> DENSEBOXLIST { get; set; }

			public EQUIPMENTc()
			{
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
				PORTLIST = new List<PORTc>();
                DENSEBOXLIST = new List<DENSEBOXc>();
			}
		}

        public class PALLETc
        {
            public string PALLETNO { get; set; }

            public string PALLETID { get; set; }

            public string PALLETMODE { get; set; }

            public string PALLETDATAREQUEST { get; set; }

            public PALLETc()
            {
                PALLETNO = string.Empty;
                PALLETID = string.Empty;
                PALLETMODE = string.Empty;
                PALLETDATAREQUEST = string.Empty;
            }
        }

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

		public class PORTc
		{
			public string PORTNO { get; set; }

			public string PORTID { get; set; }

            public string LINEID { get; set; }

			public string CASSETTESEQNO { get; set; }

			public string CASSETTEID { get; set; }

			public string PORTSTATUS { get; set; }

			public string CASSETTESTATUS { get; set; }

			public string PORTTYPE { get; set; }

			public string PORTMODE { get; set; }

			public string PORTENABLEMODE { get; set; }

			public string PORTTRANSFERMODE { get; set; }

            //public string PORTOPERMODE { get; set; }

			public string PORTGRADE { get; set; }

			public string PORTCNT { get; set; }

			public string SUBCSTSTATE { get; set; }

			public string JOBEXISTSLOT { get; set; }

			public string PORTDOWN { get; set; }

            public string PARTIALFULLMODE { get; set; }

            public string LOADINGCASSETTETYPE { get; set; }

            public string PRODUCTTYPE { get; set; }

            public string PROCESSTYPE_ARRAY { get; set; }

            public string ASSIGNMENT_GAP { get; set; }

			public PORTc()
			{
				PORTNO = string.Empty;
				PORTID = string.Empty;
                LINEID = string.Empty;
				CASSETTESEQNO = string.Empty;
				CASSETTEID = string.Empty;
				PORTSTATUS = string.Empty;
				CASSETTESTATUS = string.Empty;
				PORTTYPE = string.Empty;
				PORTMODE = string.Empty;
				PORTENABLEMODE = string.Empty;
				PORTTRANSFERMODE = string.Empty;
                //PORTOPERMODE = string.Empty;
				PORTGRADE = string.Empty;
				PORTCNT = string.Empty;
				SUBCSTSTATE = string.Empty;
				JOBEXISTSLOT = string.Empty;
				PORTDOWN = string.Empty;
                PARTIALFULLMODE = string.Empty;
                LOADINGCASSETTETYPE = string.Empty;
                PRODUCTTYPE = string.Empty;
                PROCESSTYPE_ARRAY = string.Empty;
                ASSIGNMENT_GAP = string.Empty;
			}
		}

        //Add By Hujunpeng&Yangzhenteng For OPI Display 20180905
        public class MaterialRealWeight
        {
            public string MaterialForCF01ID { get; set; }
            public string MaterialForCF01Weight { get; set; }
            public string MaterialForCF01Status { get; set; }
            public string MaterialForCF02ID { get; set; }
            public string MaterialForCF02Weight { get; set; }
            public string MaterialForCF02Status { get; set; }

            public string MaterialTK01ID { get; set; }
            public string MaterialTK01Weight { get; set; }
            public string MaterialTK02ID { get; set; }
            public string MaterialTK02Weight { get; set; }

            public string MaterialTK03ID { get; set; }
            public string MaterialTK03Weight { get; set; }
            public string MaterialTK04ID { get; set; }
            public string MaterialTK04Weight { get; set; }

            public string MaterialTK05ID { get; set; }
            public string MaterialTK05Weight { get; set; }
            public string MaterialTK06ID { get; set; }
            public string MaterialTK06Weight { get; set; }

            public string MaterialTK07ID { get; set; }
            public string MaterialTK07Weight { get; set; }
            public string MaterialTK08ID { get; set; }
            public string MaterialTK08Weight { get; set; }

            public MaterialRealWeight()
            {
                MaterialForCF01ID = string.Empty;
                MaterialForCF01Weight = string.Empty;
                MaterialForCF02ID = string.Empty;
                MaterialForCF02Weight = string.Empty;

                MaterialTK01ID = string.Empty;
                MaterialTK01Weight = string.Empty;
                MaterialTK02ID = string.Empty;
                MaterialTK02Weight = string.Empty;

                MaterialTK03ID = string.Empty;
                MaterialTK03Weight = string.Empty;
                MaterialTK04ID = string.Empty;
                MaterialTK04Weight = string.Empty;

                MaterialTK05ID = string.Empty;
                MaterialTK05Weight = string.Empty;
                MaterialTK06ID = string.Empty;
                MaterialTK06Weight = string.Empty;

                MaterialTK07ID = string.Empty;
                MaterialTK07Weight = string.Empty;
                MaterialTK08ID = string.Empty;
                MaterialTK08Weight = string.Empty;
            }
        }

        //Add By hujunpeng For PI T/C数量监控 20190723
        public class PIJobCountReport
        {
            public string ProductGroupOwner01 { get; set; }
            public string TFTCount01 { get; set; }
            public string CFCount01 { get; set; }

            public string ProductGroupOwner02 { get; set; }
            public string TFTCount02 { get; set; }
            public string CFCount02 { get; set; }

            public string ProductGroupOwner03 { get; set; }
            public string TFTCount03 { get; set; }
            public string CFCount03 { get; set; }

            public string ProductGroupOwner04 { get; set; }
            public string TFTCount04 { get; set; }
            public string CFCount04 { get; set; }

            public string ProductGroupOwner05 { get; set; }
            public string TFTCount05 { get; set; }
            public string CFCount05 { get; set; }

            public string ProductGroupOwner06 { get; set; }
            public string TFTCount06 { get; set; }
            public string CFCount06 { get; set; }

            public string ProductGroupOwner07 { get; set; }
            public string TFTCount07 { get; set; }
            public string CFCount07 { get; set; }

            public string ProductGroupOwner08 { get; set; }
            public string TFTCount08 { get; set; }
            public string CFCount08 { get; set; }

            public string ProductGroupOwner09 { get; set; }
            public string TFTCount09 { get; set; }
            public string CFCount09 { get; set; }

            public string ProductGroupOwner10 { get; set; }
            public string TFTCount10 { get; set; }
            public string CFCount10 { get; set; }

            public PIJobCountReport()
            {
                ProductGroupOwner01 = string.Empty;
                TFTCount01 = string.Empty;
                CFCount01 = string.Empty;

                ProductGroupOwner02 = string.Empty;
                TFTCount02 = string.Empty;
                CFCount02 = string.Empty;

                ProductGroupOwner03 = string.Empty;
                TFTCount03 = string.Empty;
                CFCount03 = string.Empty;

                ProductGroupOwner04 = string.Empty;
                TFTCount04 = string.Empty;
                CFCount04 = string.Empty;

                ProductGroupOwner05 = string.Empty;
                TFTCount05 = string.Empty;
                CFCount05 = string.Empty;

                ProductGroupOwner06 = string.Empty;
                TFTCount06 = string.Empty;
                CFCount06 = string.Empty;

                ProductGroupOwner07 = string.Empty;
                TFTCount07 = string.Empty;
                CFCount07 = string.Empty;

                ProductGroupOwner08 = string.Empty;
                TFTCount08 = string.Empty;
                CFCount08 = string.Empty;

                ProductGroupOwner09 = string.Empty;
                TFTCount09 = string.Empty;
                CFCount09 = string.Empty;

                ProductGroupOwner10 = string.Empty;
                TFTCount10 = string.Empty;
                CFCount10 = string.Empty;
            }
        }
        public class DENSEBOXc
        {
            public string PORTID { get; set; }

            public string PORTNO { get; set; }

            public string PORTENABLEMODE { get; set; }

            public string PORTPACKINGMODE { get; set; }

            public string BOXID01 { get; set; }

            public string BOXID02 { get; set; }

            public string UNPACKINGSOURCE { get; set; }

            public string PAPER_BOXID { get; set; }

            public string BOXTYPE { get; set; }

            public string DENSEBOXDATAREQUEST { get; set; }

            public DENSEBOXc()
            {
                PORTID = string.Empty;
                PORTNO = string.Empty;
                PORTENABLEMODE = string.Empty;
                PORTPACKINGMODE = string.Empty;
                BOXID01 = string.Empty;
                BOXID02 = string.Empty;
                UNPACKINGSOURCE = string.Empty;
                PAPER_BOXID = string.Empty;
                BOXTYPE = string.Empty;
                DENSEBOXDATAREQUEST = string.Empty;
            }
        }

        public class LINEc
        {
            public string LINEID { get; set; }

            public string MESCONTROLSTATENAM { get; set; }

            public LINEc()
            {
                LINEID = string.Empty;
                MESCONTROLSTATENAM = string.Empty;
            }
        }

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string LINETYPE { get; set; }

			public string FACTORYTYPE { get; set; }

            [XmlArray("LINEIDLIST")]
            [XmlArrayItem("LINE")]
            public List<LINEc> LINEIDLIST { get; set; }

			public string INDEXEROPERATIONMODE { get; set; }

			public string PLCSTATUS { get; set; }

			public string LINEOPERMODE { get; set; }

            public string SHORTCUTMODE { get; set; }

			public string LINESTATUSNAME { get; set; }

			public string COOLRUNSETCOUNT { get; set; }

			public string COOLRUNREMAINCOUNT { get; set; }

            public string ROBOT_FETCH_SEQ_MODE { get; set; }

            //public string BCSTATUS { get; set; }

            //public string EQUIPMENTRUNMODE { get; set; }

			[XmlArray("EQUIPMENTLIST")]
			[XmlArrayItem("EQUIPMENT")]
			public List<EQUIPMENTc> EQUIPMENTLIST { get; set; }

            [XmlArray("PALLETLIST")]
            [XmlArrayItem("PALLET")]
            public List<PALLETc> PALLETLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				LINETYPE = string.Empty;
				FACTORYTYPE = string.Empty;
                LINEIDLIST = new List<LINEc>();
				INDEXEROPERATIONMODE = string.Empty;
				PLCSTATUS = string.Empty;
				LINEOPERMODE = string.Empty;
                SHORTCUTMODE = string.Empty;
				LINESTATUSNAME = string.Empty;
				COOLRUNSETCOUNT = string.Empty;
				COOLRUNREMAINCOUNT = string.Empty;
                ROBOT_FETCH_SEQ_MODE = string.Empty;
                //BCSTATUS = string.Empty;
                //EQUIPMENTRUNMODE = string.Empty;
				EQUIPMENTLIST = new List<EQUIPMENTc>();
                PALLETLIST = new List<PALLETc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public AllDataUpdateReply()
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
