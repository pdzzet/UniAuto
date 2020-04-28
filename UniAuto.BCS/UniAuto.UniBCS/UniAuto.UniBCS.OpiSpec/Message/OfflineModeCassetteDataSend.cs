using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class OfflineModeCassetteDataSend : Message
    {
        public class LOTDATAc
        {
            public string LOTNAME { get; set; }

            public string PROCESSOPERATIONNAME { get; set; }

            public string PRODUCTOWNER { get; set; }

            public string PRODUCTSPECNAME { get; set; }

            public string BCPRODUCTTYPE { get; set; }

            public string BCPRODUCTTYPE_CUT { get; set; }

            public string PRODUCTID { get; set; }

            public string PRODUCTID_CUT { get; set; }

            public string CFREWORKCOUNT { get; set; }
            
            public string TARGETCSTID_CF { get; set; }

            public string LINERECIPENAME { get; set; }

            public string PPID { get; set; }

            public string CSTSETTINGCODE { get; set; }

            public string CSTSETTINGCODE_CUT { get; set; }

            public string CSTSETTINGCODE2 { get; set; }

            public string PCSCSTSETTINGCODELIST { get; set; }//20170717 huangjiayin modify for pcs

            public string PCSBLOCKSIZELIST { get; set; }//20170717 huangjiayin modify for pcs

            public string BLOCKSIZE1 { get; set; }//20170717 huangjiayin modify for pcs

            public string BLOCKSIZE2 { get; set; }//20170717 huangjiayin modify for pcs

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
                BCPRODUCTTYPE = string.Empty;
                BCPRODUCTTYPE_CUT = string.Empty;
                PRODUCTID = string.Empty;
                PRODUCTID_CUT = string.Empty;
                CFREWORKCOUNT = string.Empty;                
                TARGETCSTID_CF = string.Empty;
                LINERECIPENAME = string.Empty;
                PPID = string.Empty;
                CSTSETTINGCODE = string.Empty;
                CSTSETTINGCODE_CUT = string.Empty;
                CSTSETTINGCODE2 = string.Empty;
                PCSCSTSETTINGCODELIST = string.Empty;//20170717 huangjiayin modify for pcs
                PCSBLOCKSIZELIST = string.Empty;
                BLOCKSIZE1 = string.Empty;
                BLOCKSIZE2 = string.Empty;
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

            public PROCESSLINEc()
            {
                LINENAME = string.Empty;
                LINERECIPENAME = string.Empty;
                PPID = string.Empty;
                CSTSETTINGCODE = string.Empty;
            }
        }

        public class STBPRODUCTSPECc
        {
            public string LINENAME { get; set; }

            public string LINERECIPENAME { get; set; }

            public string PPID { get; set; }

            public string CSTSETTINGCODE { get; set; }

            public STBPRODUCTSPECc()
            {
                LINENAME = string.Empty;
                LINERECIPENAME = string.Empty;
                PPID = string.Empty;
                CSTSETTINGCODE = string.Empty;
            }
        }

        public class PRODUCTDATAc
        {
            public string SLOTNO { get; set; }

            [XmlIgnore]
            public bool PROCESSFLAGbool { get; set; }

            public string PROCESSFLAG
            {
                get { return PROCESSFLAGbool ? "Y" : "N"; }
                set { PROCESSFLAGbool = (string.Compare(value, "Y", true) == 0); }
            }

            public string PRODUCTNAME { get; set; }

            public string PRODUCTRECIPENAME { get; set; }

            public string OPI_PPID { get; set; }

            public string PRODUCTTYPE { get; set; }

            public string PRODUCTGRADE { get; set; }

            public string PRODUCTJUDGE { get; set; }

            public string GROUPID { get; set; }

            public string PROCESSTYPE { get; set; }

            public string TARGETCSTID { get; set; }

            public string INSPRESERVATION { get; set; }

            public string PREINLINEID { get; set; }

            public string FLOWPRIORITY { get; set; }

            public string NETWORKNO { get; set; }

            public string OWNERTYPE { get; set; }

            public string OWNERID { get; set; }

            public string REVPROCESSOPERATIONNAME { get; set; }

            public string EQPFLAG { get; set; }

            public string SUBSTRATETYPE { get; set; }

            public string COAVERSION { get; set; }

            public string SCRAPCUTFLAG { get; set; }

            public string PANELSIZE { get; set; }

            public string TRUNANGLEFLAG { get; set; }

            public string BLOCK_SIZE { get; set; }

            public string TARGET_SLOTNO { get; set; }

            public string CFINLINEREWORKMAXCOUNT { get; set; }

            public string BLOCK_OX_INFO { get; set; }

            public string GLASS_THICKNESS { get; set; }
            
            public string OPERATION_ID { get; set; }
            
            public string PI_LIQUID_TYPE { get; set; }
            
            public string ASSEMBLE_SEQNO { get; set; }
            
            public string UV_MASK_USE_COUNT { get; set; }

            public string OXR { get; set; }

            public string REJUDGE_COUNT { get; set; }

            public string VENDER_NAME { get; set; }

            public string BUR_CHECK_COUNT { get; set; }

            public string DOT_REPAIR_COUNT { get; set; }

            public string LINE_REPAIR_COUNT { get; set; }

            public string MAX_REWORK_COUNT { get; set; }

            public string CURRENT_REWORK_COUNT { get; set; }

            public string OQC_BANK { get; set; }

            public PRODUCTDATAc()
            {
                SLOTNO = string.Empty;
                PROCESSFLAGbool = false;
                PRODUCTNAME = string.Empty;
                PRODUCTRECIPENAME = string.Empty;
                OPI_PPID = string.Empty;
                PRODUCTTYPE = string.Empty;
                PRODUCTGRADE = string.Empty;
                PRODUCTJUDGE = string.Empty;
                GROUPID = string.Empty;
                PROCESSTYPE = string.Empty;
                TARGETCSTID = string.Empty;
                INSPRESERVATION = string.Empty;
                PREINLINEID = string.Empty;
                FLOWPRIORITY = string.Empty;
                NETWORKNO = string.Empty;
                OWNERTYPE = string.Empty;
                OWNERID = string.Empty;
                REVPROCESSOPERATIONNAME = string.Empty;
                EQPFLAG = string.Empty;
                SUBSTRATETYPE = string.Empty;
                COAVERSION = string.Empty;
                SCRAPCUTFLAG = string.Empty;
                PANELSIZE = string.Empty;
                TRUNANGLEFLAG = string.Empty;
                BLOCK_SIZE = string.Empty;
                TARGET_SLOTNO = string.Empty;
                CFINLINEREWORKMAXCOUNT = string.Empty;
                BLOCK_OX_INFO = string.Empty;
                GLASS_THICKNESS = string.Empty;
                OPERATION_ID = string.Empty;
                PI_LIQUID_TYPE = string.Empty;
                ASSEMBLE_SEQNO = string.Empty;
                UV_MASK_USE_COUNT= string.Empty;
                OXR = string.Empty;
                REJUDGE_COUNT = string.Empty;
                VENDER_NAME = string.Empty;
                BUR_CHECK_COUNT = string.Empty;
                DOT_REPAIR_COUNT = string.Empty;
                LINE_REPAIR_COUNT = string.Empty;
                MAX_REWORK_COUNT = string.Empty;
                CURRENT_REWORK_COUNT = string.Empty;
                OQC_BANK = string.Empty;
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
                PRODUCTQUANTITY = string.Empty;
                REMAPFLAG = string.Empty;
                CSTSETTINGCODE = string.Empty;
                LOTLIST = new List<LOTDATAc>();
            }
        }

        public TrxBody BODY { get; set; }

        public new Return RETURN { get { return _return; } set { _return = value; } }

        public OfflineModeCassetteDataSend()
        {
            this.Direction = Spec.DirType.OPI_TO_BC;
            this.WaitReply = "OfflineModeCassetteDataSendReply";
            this.HEADER.MESSAGENAME = GetType().Name;
            this.BODY = new TrxBody();
        }

        public override Body GetBody()
        {
            return this.BODY;
        }
    }
}
