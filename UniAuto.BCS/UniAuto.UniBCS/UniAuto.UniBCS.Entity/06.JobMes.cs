using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;

namespace UniAuto.UniBCS.Entity
{
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]  
    public class LOTc : ICloneable
    {
        public string LOTNAME { get; set; }

        public string PRODUCTSPECNAME { get; set; }

        public string PRODUCTSPECVER { get; set; }

        public string PROCESSFLOWNAME { get; set; }

        public string PROCESSOPERATIONNAME { get; set; }

        public string PRODUCTOWNER { get; set; }

        public string PRDCARRIERSETCODE { get; set; }

        public string SALEORDER { get; set; }

        //public string ISPIREWORK { get; set; } cc.kuang t3 not use this item 20150701

        public string PAIRPRODUCTSPECNAME { get; set; } //Modify By Yangzhenteng 2017/12/21 For PIL Special

        public string PRODUCTSIZETYPE { get; set; }

        public string PRODUCTSIZE { get; set; }

        public string BCPRODUCTTYPE { get; set; }

        public string BCPRODUCTID { get; set; }

        public string PRODUCTPROCESSTYPE { get; set; }

        public string PROCESSTYPE { get; set; }

        public string LINERECIPENAME { get; set; }

        public string PPID { get; set; }

        [XmlArray("LINEQTIMELIST")]
        [XmlArrayItem("LINEQTIME")]
        public List<LINEQTIMEc> LINEQTIMELIST { get; set; }

        public string NODESTACK { get; set; }

        public string PRODUCTSPECGROUP { get; set; }

        public string GRADERANKGROUP { get; set; }//20161130  Add by MengHui For Cell FileData
        public string PRODUCTGCPTYPE { get; set; }

        [XmlArray("PROCESSLINELIST")]
        [XmlArrayItem("PROCESSLINE")]
        public List<PROCESSLINEc> PROCESSLINELIST { get; set; }

        [XmlArray("STBPRODUCTSPECLIST")]
        [XmlArrayItem("STBPRODUCTSPEC")]
        
        public List<STBPRODUCTSPECc> STBPRODUCTSPECLIST { get; set; }

        public string ISMIXEDLAYOUT { get; set; } //Add By Yangzhenteng 20190316;

        public string PRODUCTSPECLAYOUT { get; set; } //cc.kuang add for t3 20150701

        public string SUBPRODUCTSPECLAYOUT { get; set; } //sy add for t3 20151202

        public string SUBPRODUCTSPECS { get; set; }

        public string SUBPRODUCTNAMES { get; set; }//sy add for t3 20151123

        public string SUBPRODUCTSPECNAME { get; set; }//sy add for t3 20151123

        public string SUBPRODUCTPOSITIONS { get; set; } //cc.kuang add for t3 20150701

        public string SUBPRODUCTLINES { get; set; }

        public string SUBPRODUCTSIZETYPES { get; set; }

        public string SUBPRODUCTSIZES { get; set; }

        public string SUBPRODUCTORIGINID { get; set; } //cc.kuang add for t3 20150701

        public string SUBPRODUCTCARRIERSETCODES { get; set; } //sy add for t3 20151229

        [XmlArray("SUBPRODUCTSPECLIST")]                              //Add By Yangzhenteng 20190316;
        [XmlArrayItem("SUBPRODUCTSPEC")]                               //Add By Yangzhenteng 20190316;
        public List<SUBPRODUCTSPECc> SUBPRODUCTSPECLIST { get; set; } //Add By Yangzhenteng 20190316;   

        public string ORIENTEDSITE { get; set; }

        public string ORIENTEDFACTORYNAME { get; set; }

        public string CURRENTSITE { get; set; }

        public string CURRENTFACTORYNAME { get; set; }

        private string _PRODUCTTHICKNESS;

        //此处需要做个转换个FileData 中Thickness   20150302  Tom
        //cc.kuang add for t3 20150701
        public string PRODUCTTHICKNESS 
        { 
            get {
                if(_PRODUCTTHICKNESS=="0.3")
                    return "1";
                else if(_PRODUCTTHICKNESS=="0.4")
                    return "2";
                else if (_PRODUCTTHICKNESS == "0.5")
                    return "3";
                else if (_PRODUCTTHICKNESS == "0.6")
                    return "4";
                else if (_PRODUCTTHICKNESS == "0.7")
                    return "5";
                else 
                    return "0";
            }
            set
            {
                _PRODUCTTHICKNESS = value;
            }
                
        }

        public string CFREWORKCOUNT { get; set; }

        public string CFINLINEREWORKMAXCOUNT { get; set; } // 20150727 Add by Frank For CF Photo Line 

        public string TARGETCSTID_CF { get; set; }  // 20150727 Add by Frank For CF Photo Line

        public string AUTOSAMPLEFLAG { get; set; }  // 20160712 Add by Frank For All Shop

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
            //ISPIREWORK = string.Empty; cc.kuang t3 not use this item 20150701
            PAIRPRODUCTSPECNAME = string.Empty; //cc.kuang t3 not use this item 20150701//remark by huangjiayin 20171222
            PRODUCTSIZETYPE = string.Empty;
            PRODUCTSIZE = string.Empty;
            BCPRODUCTTYPE = string.Empty;
            BCPRODUCTID = string.Empty;
            PRODUCTPROCESSTYPE = string.Empty;
            PROCESSTYPE = string.Empty;
            LINERECIPENAME = string.Empty;
            PPID = string.Empty;
            LINEQTIMELIST = new List<LINEQTIMEc>();
            NODESTACK = string.Empty;
            PRODUCTSPECGROUP = string.Empty;
            GRADERANKGROUP = string.Empty;//20161130  Add by MengHui For Cell FileData
            PRODUCTGCPTYPE = string.Empty;
            PROCESSLINELIST = new List<PROCESSLINEc>();
            STBPRODUCTSPECLIST = new List<STBPRODUCTSPECc>();
            ISMIXEDLAYOUT = string.Empty; //Add By Yangzhenteng20190316   
            PRODUCTSPECLAYOUT = string.Empty;
            SUBPRODUCTSPECLAYOUT = string.Empty;
            SUBPRODUCTSPECS = string.Empty;
            SUBPRODUCTPOSITIONS = string.Empty;
            //SUBPRODUCTNAMES = string.Empty; cc.kuang t3 not use this item 20150701
            SUBPRODUCTLINES = string.Empty;
            SUBPRODUCTSIZETYPES = string.Empty;
            SUBPRODUCTSIZES = string.Empty;
            SUBPRODUCTORIGINID = string.Empty;
            SUBPRODUCTCARRIERSETCODES = string.Empty;// 20160104 Add by sy For PCS CUT Line 
            SUBPRODUCTSPECLIST = new List<SUBPRODUCTSPECc>(); //Add By Yangzhenteng20190316;
            ORIENTEDSITE = string.Empty;
            ORIENTEDFACTORYNAME = string.Empty;
            CURRENTSITE = string.Empty;
            CURRENTFACTORYNAME = string.Empty;
            PRODUCTTHICKNESS = string.Empty;
            CFREWORKCOUNT = string.Empty;
            CFINLINEREWORKMAXCOUNT = string.Empty; // 20150727 Add by Frank For CF Photo Line 
            TARGETCSTID_CF = string.Empty; // 20150727 Add by Frank For CF Photo Line
            PRODUCTLIST = new List<PRODUCTc>();
            AUTOSAMPLEFLAG = string.Empty;  // 20160712 Add by Frank For All Shop
        }

        public object Clone()
        {
            LOTc lot = (LOTc)this.MemberwiseClone();
            lot.PRODUCTLIST = new List<PRODUCTc>();
            if (this.PRODUCTLIST != null)
            {
                foreach (PRODUCTc p in this.PRODUCTLIST)
                {
                    lot.PRODUCTLIST.Add((PRODUCTc)p.Clone());
                }
            }
            lot.LINEQTIMELIST = new List<LINEQTIMEc>();
            if (this.LINEQTIMELIST != null)
            {
                foreach (LINEQTIMEc lq in this.LINEQTIMELIST)
                {
                    lot.LINEQTIMELIST.Add((LINEQTIMEc)lq.Clone());
                }
            }
            lot.PROCESSLINELIST = new List<PROCESSLINEc>();
            if (this.PROCESSLINELIST != null)
            {
                foreach (PROCESSLINEc pc in this.PROCESSLINELIST)
                {
                    lot.PROCESSLINELIST.Add((PROCESSLINEc)pc.Clone());
                }
            }
            lot.STBPRODUCTSPECLIST = new List<STBPRODUCTSPECc>();
            if (this.STBPRODUCTSPECLIST != null)
            {
                foreach (STBPRODUCTSPECc st in this.STBPRODUCTSPECLIST)
                {
                    lot.STBPRODUCTSPECLIST.Add((STBPRODUCTSPECc)st.Clone());
                }
            }

            if (lot.ISMIXEDLAYOUT == "Y")  //Add By Yangzhenteng20190316 For Cut Type5
            {
                lot.SUBPRODUCTSPECLIST = new List<SUBPRODUCTSPECc>();
                if (this.SUBPRODUCTSPECLIST != null)
                {
                    foreach (SUBPRODUCTSPECc s in this.SUBPRODUCTSPECLIST)
                    {
                        lot.SUBPRODUCTSPECLIST.Add((SUBPRODUCTSPECc)s.Clone());
                    }
                }
            }
            else
            { }
            return lot;
        }
    }

    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]  
    public class LINEQTIMEc:ICloneable
    {
        public string LINENAME { get; set; }

        public List<MACHINEQTIMEc> MACHINEQTIMELIST { get; set; }

        public LINEQTIMEc()
        {
            LINENAME = string.Empty;
            MACHINEQTIMELIST = new List<MACHINEQTIMEc>();
        }

        public object Clone()
        {
            LINEQTIMEc lq = (LINEQTIMEc)this.MemberwiseClone();
            lq.MACHINEQTIMELIST = new List<MACHINEQTIMEc>();
            if (this.MACHINEQTIMELIST != null)
            {
                foreach (MACHINEQTIMEc mc in this.MACHINEQTIMELIST)
                {
                    lq.MACHINEQTIMELIST.Add((MACHINEQTIMEc)mc.Clone());
                }
            }
            return lq;
        }
    }
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]  
    public class PROCESSLINEc:ICloneable
    {
        public string LINENAME { get; set; }

        public string PRODUCTSPECNAME { get; set; }

        public string PRODUCTSPECVER { get; set; }

        public string BCPRODUCTTYPE { get; set; }

        public string BCPRODUCTID { get; set; }

        public string CARRIERSETCODE { get; set; }

        public string LINERECIPENAME { get; set; }

        public string PPID { get; set; }

        public string RECIPEPARAVALIDATIONFLAG { get; set; }

        public PROCESSLINEc()
        {
            LINENAME = string.Empty;
            PRODUCTSPECNAME = string.Empty;
            PRODUCTSPECVER = string.Empty;
            BCPRODUCTTYPE = string.Empty;
            BCPRODUCTID = string.Empty;
            CARRIERSETCODE = string.Empty;
            LINERECIPENAME = string.Empty;
            PPID = string.Empty;
            RECIPEPARAVALIDATIONFLAG = string.Empty;
        }

        public object Clone()
        {
            PROCESSLINEc pl = (PROCESSLINEc)this.MemberwiseClone();
            return pl;
        }
    }
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]  
    public class STBPRODUCTSPECc:ICloneable
    {
        public string LINENAME { get; set; }

        public string PRODUCTSPECNAME { get; set; }

        public string PRODUCTSPECVER { get; set; }

        public string PRODUCTOWNER { get; set; }

        public string OWNERID { get; set; }

        public string BCPRODUCTTYPE { get; set; }

        public string BCPRODUCTID { get; set; }

        public string CARRIERSETCODE { get; set; }

        public string LINERECIPENAME { get; set; }

        public string PPID { get; set; }

        public STBPRODUCTSPECc()
        {
            LINENAME = string.Empty;
            PRODUCTSPECNAME = string.Empty;
            PRODUCTSPECVER = string.Empty;
            PRODUCTOWNER = string.Empty;
            OWNERID = string.Empty;
            BCPRODUCTTYPE = string.Empty;
            BCPRODUCTID = string.Empty;
            CARRIERSETCODE = string.Empty;
            LINERECIPENAME = string.Empty;
            PPID = string.Empty;
        }

        public object Clone()
        {
            STBPRODUCTSPECc st =(STBPRODUCTSPECc) this.MemberwiseClone();
            return st;
        }
    }
    //Add By Yangzhenteng 20190316
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]

    public class SUBPRODUCTSPECc : ICloneable
    {
        public string SUBPRODUCTSPECS { get; set; }
        public string SUBPRODUCTSPECLAYOUT { get; set; }
        public string SUBPRODUCTCARRIERSETCODES { get; set; }
        public SUBPRODUCTSPECc()
        {
            SUBPRODUCTSPECS = string.Empty;
            SUBPRODUCTSPECLAYOUT = string.Empty;
            SUBPRODUCTCARRIERSETCODES = string.Empty;
        }
        public object Clone()
        {
            SUBPRODUCTSPECc su = (SUBPRODUCTSPECc)this.MemberwiseClone();
            return su;
        }
    }

    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]  
    public class PRODUCTc:ICloneable
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

        public string SUBPRODUCTQUANTITY { get; set; } //cc.kuang add for t3 20150701

        public string SUBPRODUCTDEFECTCODE { get; set; }

        public string SUBPRODUCTJPSGRADE { get; set; }

        public string SUBPRODUCTJPSCODE { get; set; }

        public string SUBPRODUCTJPSFLAG { get; set; }

        public string SUBPRODUCTSPECNAEM { get; set; }  //Add by marine for T3 MES 2015/8/21

        public string SUBPRODUCTPOSITION { get; set; }  //Add by marine for T3 MES 2015/8/21

        public string SUBPRODUCTSIZE { get; set; }  //Add by marine for T3 MES 2015/8/21

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
            get { return CFTYPE1REPAIRCOUNTint.ToString(); }
            set
            {
                int tmp = 0;
                if (int.TryParse(value, out tmp))
                    CFTYPE1REPAIRCOUNTint = tmp;
                else
                    CFTYPE1REPAIRCOUNTint = 0;
            }
        }

        [XmlIgnore]
        public int CFTYPE2REPAIRCOUNTint { get; set; }

        public string CFTYPE2REPAIRCOUNT
        {
            get { return CFTYPE2REPAIRCOUNTint.ToString(); }
            set
            {
                int tmp = 0;
                if (int.TryParse(value, out tmp))
                    CFTYPE2REPAIRCOUNTint = tmp;
                else
                    CFTYPE2REPAIRCOUNTint = 0;
            }
        }

        [XmlIgnore]
        public int CARBONREPAIRCOUNTint { get; set; }

        public string CARBONREPAIRCOUNT
        {
            get { return CARBONREPAIRCOUNTint.ToString(); }
            set
            {
                int tmp = 0;
                if (int.TryParse(value, out tmp))
                    CARBONREPAIRCOUNTint = tmp;
                else
                    CARBONREPAIRCOUNTint = 0;
            }
        }

        [XmlIgnore]
        public int LASERREPAIRCOUNTint { get; set; }

        public string LASERREPAIRCOUNT
        {
            get { return LASERREPAIRCOUNTint.ToString(); }
            set
            {
                int tmp = 0;
                if (int.TryParse(value, out tmp))
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
 
        public string OWNERTYPE { get ;  set  ;}       

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
            get { return PROCESSFLAGbool ? "Y" : "N"; }
            set { PROCESSFLAGbool = (string.Compare(value, "Y", true) == 0); }
        }

        public string ARRAYTTPEQVERCODE { get; set; }

        public string RTPFLAG { get; set; }

        public string MAXINLINERWCOUNT { get; set; } //cc.kuang add for t3 20150701

        public string MACROFLAG { get; set; } //cc.kuang add for t3 20150701

        public string ALIGNERNAME { get; set; } //cc.kuang add for t3 20150701

        public string SOURCELOTNAME { get; set; } //cc.kuang add for t3 20150701

        public string LASTMAINPPID { get; set; } //cc.kuang add for t3 20150701

        public string LASTMAINEQPNAME { get; set; } //cc.kuang add for t3 20150701

        public string LASTMAINCHAMBERNAME { get; set; } //cc.kuang add for t3 20150701

        public string GLASSTURNFLAG { get; set; } //cc.kuang add for t3 20150701

        public string CELLTTPFLAG { get; set; } //cc.kuang add for t3 20150701

        public string MAXRWCOUNT { get; set; } //cc.kuang add for t3 20150701

        public string TARGET_SLOTNO { get; set; }   // 20150727 Add by Frank For CF Photo Line

        public string MESPROCESSFLAG     //add by yang 20161112    mark by yang 2017/1/6,不要给初值
        {
            get;set;
        //   get { return MESPROCESSFLAGbool ? "Y" : "N"; }
         //   set { MESPROCESSFLAGbool = (string.Compare(value, "Y", true) == 0); }
        }

        

        //public string INLINERWCOUNTint { get; set; } cc.kuang t3 not use this item 20150701
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
            SUBPRODUCTSPECNAEM = string.Empty; //Add by marine for T3 MES 2015/8/21
            SUBPRODUCTPOSITION = string.Empty; //Add by marine for T3 MES 2015/8/21
            SUBPRODUCTSIZE = string.Empty; //Add by marine for T3 MES 2015/8/21
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
            FMAFLAG = string.Empty;
            MHUFLAG = string.Empty;
            DEFECTLIST = new List<DEFECTc>();
            ARRAYPRODUCTSPECVER = string.Empty;
            AGINGENABLE = string.Empty;
            PROCESSFLAGbool = false;
            MESPROCESSFLAG = string.Empty;
            ARRAYTTPEQVERCODE = string.Empty;
            RTPFLAG = string.Empty;
            //INLINERWCOUNTint = string.Empty;
            MAXINLINERWCOUNT = string.Empty;
            MACROFLAG = string.Empty;
            ALIGNERNAME = string.Empty;
            SOURCELOTNAME = string.Empty;
            LASTMAINPPID = string.Empty;
            LASTMAINEQPNAME = string.Empty;
            LASTMAINCHAMBERNAME = string.Empty;
            GLASSTURNFLAG = string.Empty;
            CELLTTPFLAG = string.Empty;
            MAXRWCOUNT = string.Empty;
            TARGET_SLOTNO = string.Empty;            
        }

        public object Clone()
        {
            PRODUCTc p = (PRODUCTc)this.MemberwiseClone();
            p.DEFECTLIST = new List<DEFECTc>();
            if (this.DEFECTLIST != null)
            {
                foreach (DEFECTc dc in this.DEFECTLIST)
                {
                    p.DEFECTLIST.Add((DEFECTc)dc.Clone());
                }
            }
            p.REWORKLIST = new List<REWORKc>();
            if (this.REWORKLIST != null)
            {
                foreach (REWORKc rw in this.REWORKLIST)
                {
                    p.REWORKLIST.Add((REWORKc)rw.Clone());
                }
            }
            p.ABNORMALCODELIST = new List<CODEc>();
            if (this.ABNORMALCODELIST != null)
            {
                foreach (CODEc cc in this.ABNORMALCODELIST)
                {
                    p.ABNORMALCODELIST.Add((CODEc)cc.Clone());
                }
            }
            p.LCDROPLIST = new List<string>();
            if (this.LCDROPLIST != null)
                p.LCDROPLIST.AddRange(this.LCDROPLIST);
            return p;
        }
    }
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]  
    public class MACHINEQTIMEc:ICloneable
    {
        public string STARTMACHINE { get; set; }

        public string STARTUNITS { get; set; }

        public string STARTEVENT { get; set; }

        public string RECIPEID { get; set; }

        public string ENDMACHINE { get; set; }

        public string ENDUNITS { get; set; }

        public string ENDEVENT { get; set; }

        public string QTIME { get; set; }

        public string CFRWQTIME { get; set; }

        public MACHINEQTIMEc()
        {
            STARTMACHINE = string.Empty;
            STARTUNITS = string.Empty;
            STARTEVENT = string.Empty;
            RECIPEID = string.Empty;
            ENDMACHINE = string.Empty;
            ENDUNITS = string.Empty;
            ENDEVENT = string.Empty;
            QTIME = string.Empty;
            CFRWQTIME = string.Empty;
        }

        public object Clone()
        {
            MACHINEQTIMEc mc = (MACHINEQTIMEc)this.MemberwiseClone();
            return mc;
        }
    }
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]  
    public class CODEc:ICloneable
    {
        public string ABNORMALSEQ { get; set; }

        public string ABNORMALCODE { get; set; }

        public string ABNORMALVALUE { get; set; }   //Add for MES 2015/7/14

        public CODEc()
        {
            ABNORMALSEQ = string.Empty;
            ABNORMALCODE = string.Empty;
            ABNORMALVALUE = string.Empty;   //Add for MES 2015/7/14
        }

        public object Clone()
        {
            CODEc cc = (CODEc)this.MemberwiseClone();
            return cc;
        }
    }
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]  
    public class REWORKc:ICloneable
    {
        public string REWORKFLOWNAME { get; set; }

        public int REWORKCOUNTint { get; set; }

        public string REWORKCOUNT
        {
            get { return REWORKCOUNTint.ToString(); }
            set
            {
                int tmp = 0;
                if (int.TryParse(value, out tmp))
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

        public object Clone()
        {
            REWORKc rc = (REWORKc)this.MemberwiseClone();
            return rc;
        }
    }
    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]  
    public class DEFECTc:ICloneable
    {
        public string SUBPRODUCTNAME { get; set; }

        public string ARRAYDEFECTCODES { get; set; }

        public string ARRAYDEFECTADDRESS { get; set; }

        public string CFDEFECTCODES { get; set; }

        public string CFDEFECTADDRESS { get; set; }

        //public string PIDEFECTCODES { get; set; } cc.kuang t3 not use this item 20150701

        //public string PIDEFECTADDRESS { get; set; } cc.kuang t3 not use this item 20150701

        //public string ODFDEFECTCODES { get; set; } cc.kuang t3 not use this item 20150701

        //public string ODFDEFECTADDRESS { get; set; } cc.kuang t3 not use this item 20150701

        public DEFECTc()
        {
            SUBPRODUCTNAME = string.Empty;
            ARRAYDEFECTCODES = string.Empty;
            ARRAYDEFECTADDRESS = string.Empty;
            CFDEFECTCODES = string.Empty;
            CFDEFECTADDRESS = string.Empty;
            //PIDEFECTCODES = string.Empty;
            //PIDEFECTADDRESS = string.Empty;
            //ODFDEFECTCODES = string.Empty;
            //ODFDEFECTADDRESS = string.Empty;
        }

        public object Clone()
        {
            DEFECTc dc = (DEFECTc)this.MemberwiseClone();
            return dc;
        }
    }

    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]  
    public class BOXc:ICloneable
    {
        public string BOXNAME { get; set; }

        public string PRODUCTSPECNAME { get; set; }

        public string PRODUCTOWNER { get; set; }

        public string BOXGRADE { get; set; }

        public string PROCESSOPERATIONNAME { get; set; }

        public string PRDCARRIERSETCODE { get; set; }

        public string PRODUCTQUANTITY { get; set; }

        public string NODESTACK { get; set; }

        public string PRODUCTSIZETYPE { get; set; }

        public string PRODUCTSIZE { get; set; }

        public string PRODUCTSPECGROUP { get; set; }

        public string PRODUCTGCPTYPE { get; set; }

        public string ORIENTEDSITE { get; set; }

        public string ORIENTEDFACTORYNAME { get; set; }

        public string CURRENTSITE { get; set; }

        public string CURRENTFACTORYNAME { get; set; }

        public List<PRODUCTc> PRODUCTLIST { get; set; }

        public BOXc()
        {
            BOXNAME = string.Empty;
            PRODUCTSPECNAME = string.Empty;
            PRODUCTOWNER = string.Empty;
            BOXGRADE = string.Empty;
            PROCESSOPERATIONNAME = string.Empty;
            PRDCARRIERSETCODE = string.Empty;
            PRODUCTQUANTITY = string.Empty;
            NODESTACK = string.Empty;
            PRODUCTSIZETYPE = string.Empty;
            PRODUCTSIZE = string.Empty;
            PRODUCTSPECGROUP = string.Empty;
            PRODUCTGCPTYPE = string.Empty;
            ORIENTEDSITE = string.Empty;
            ORIENTEDFACTORYNAME = string.Empty;
            CURRENTSITE = string.Empty;
            CURRENTFACTORYNAME = string.Empty;
            PRODUCTLIST = new List<PRODUCTc>();
        }

        public object Clone()
        {
            BOXc box = (BOXc)this.MemberwiseClone();
            PRODUCTLIST = new List<PRODUCTc>();
            if (PRODUCTLIST != null)
            {
                foreach (PRODUCTc p in PRODUCTLIST)
                {
                    PRODUCTLIST.Add((PRODUCTc)p.Clone());
                }
            }
            return box;
        }
    }

    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class SCRAPGRADEc : ICloneable
    {
        public string PRODUCTGRADE { get; set; }

        public SCRAPGRADEc()
        {
            PRODUCTGRADE = string.Empty;
        }

        public object Clone()
        {
            SCRAPGRADEc scrapgrade = (SCRAPGRADEc)this.MemberwiseClone();
            return scrapgrade;
        }
    }

    [Serializable]
    [TypeConverter(typeof(ExpandableObjectConverter))]  
    public class MES_CstBody : ICloneable
    {
        public string LINENAME { get; set; }

        public string PORTNAME { get; set; }

        public string CARRIERNAME { get; set; }

        public string LINERECIPENAME { get; set; }

        public string LINEOPERMODE { get; set; }

        public string SELECTEDPOSITIONMAP { get; set; }

        [XmlIgnore]
        public bool CLEANFLAGbool { get; set; }

        public string CLEANFLAG
        {
            get { return CLEANFLAGbool ? "Y" : "N"; }
            set { CLEANFLAGbool = (string.Compare(value, "Y", true) == 0); }
        }

        public string CARRIERSETCODE { get; set; }

        [XmlIgnore]
        public bool AOIBYPASSbool { get; set; }

        public string AOIBYPASS
        {
            get { return AOIBYPASSbool ? "Y" : "N"; }
            set { AOIBYPASSbool = (string.Compare(value, "Y", true) == 0); }
        }

        [XmlIgnore]
        public bool EXPSAMPLINGbool { get; set; }

        public string EXPSAMPLING
        {
            get { return EXPSAMPLINGbool ? "Y" : "N"; }
            set { EXPSAMPLINGbool = (string.Compare(value, "Y", true) == 0); }
        }

        [XmlIgnore]
        public bool AUTOCLAVESAMPLINGbool { get; set; }

        public string AUTOCLAVESAMPLING
        {
            get { return AUTOCLAVESAMPLINGbool ? "Y" : "N"; }
            set { AUTOCLAVESAMPLINGbool = (string.Compare(value, "Y", true) == 0); }
        }

        [XmlIgnore]
        public bool AUTOCLAVESKIPbool { get; set; }

        public string AUTOCLAVESKIP
        {
            get { return AUTOCLAVESKIPbool ? "Y" : "N"; }
            set { AUTOCLAVESKIPbool = (string.Compare(value, "Y", true) == 0); }
        }

        [XmlIgnore]
        public bool RECIPEPARAVALIDATIONFLAGbool { get; set; }

        public string RECIPEPARAVALIDATIONFLAG
        {
            get { return RECIPEPARAVALIDATIONFLAGbool ? "Y" : "N"; }
            set { RECIPEPARAVALIDATIONFLAGbool = (string.Compare(value, "Y", true) == 0); }
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
            get { return PLANNEDQUANTITYint.ToString(); }
            set
            {
                int tmp = 0;
                if (int.TryParse(value, out tmp))
                    PLANNEDQUANTITYint = tmp;
                else
                    PLANNEDQUANTITYint = 0;
            }
        }

        public string PLANNEDGROUPNAME { get; set; }

        public string UPKOWNERTYPE { get; set; }

        [XmlArray("SCRAPGRADELIST")]
        [XmlArrayItem("SCRAPGRADE")]
        public List<LOTc> SCRAPGRADELIST { get; set; }

        [XmlArray("LOTLIST")]
        [XmlArrayItem("LOT")]
        public List<LOTc> LOTLIST { get; set; }

        public MES_CstBody()
        {
            LINENAME = string.Empty;
            PORTNAME = string.Empty;
            CARRIERNAME = string.Empty;
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
            PLANNEDGROUPNAME = string.Empty;
            UPKOWNERTYPE = string.Empty;
            LOTLIST = new List<LOTc>();
        }

        public object Clone()
        {
            MES_CstBody body = (MES_CstBody)this.MemberwiseClone();
            body.LOTLIST = new List<LOTc>();
            if (this.LOTLIST != null)
            {
                foreach (LOTc lot in this.LOTLIST)
                {
                    body.LOTLIST.Add((LOTc)lot.Clone());
                }
            }
            body.RECIPEPARANOCHECKLIST = new List<string>();
            if (this.RECIPEPARANOCHECKLIST != null)
                body.RECIPEPARANOCHECKLIST.AddRange(this.RECIPEPARANOCHECKLIST);
            return body;
        }
    }

}

