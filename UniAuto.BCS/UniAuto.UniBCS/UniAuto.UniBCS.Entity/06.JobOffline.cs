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
      public class OFFLINE_CstBody : ICloneable
      {
            public string LINENAME { get; set; }

            public string EQUIPMENTNO { get; set; }

            public string PORTNO { get; set; }

            public string PORTID { get; set; }

            public string CASSETTEID { get; set; }

            public string LINEOPERMODE { get; set; }

            public string PRODUCTQUANTITY { get; set; }

            public string CLEANFLAG { get; set; }

            public string REMAPFLAG { get; set; }

            public string CSTSETTINGCODE { get; set; }

            [XmlArray("LOTLIST")]
            [XmlArrayItem("LOTDATA")]
            public List<LOTDATAc> LOTLIST { get; set; }

            public OFFLINE_CstBody()
            {
                  LINENAME = string.Empty;
                  EQUIPMENTNO = string.Empty;
                  PORTNO = string.Empty;
                  PORTID = string.Empty;
                  CASSETTEID = string.Empty;
                  LINEOPERMODE = string.Empty;
                  PRODUCTQUANTITY = string.Empty;
                  CLEANFLAG = string.Empty;
                  REMAPFLAG = string.Empty;
                  CSTSETTINGCODE = string.Empty;
                  LOTLIST = new List<LOTDATAc>();
            }

            public object Clone()
            {
                  OFFLINE_CstBody body = (OFFLINE_CstBody)this.MemberwiseClone();
                  body.LOTLIST = new List<LOTDATAc>();
                  if (this.LOTLIST != null)
                  {
                        foreach (LOTDATAc lot in this.LOTLIST)
                        {
                              body.LOTLIST.Add((LOTDATAc)lot.Clone());
                        }
                  }

                  return body;
            }
      }

      [Serializable]
      [TypeConverter(typeof(ExpandableObjectConverter))]
      public class LOTDATAc : ICloneable
      {
            public string LOTNAME { get; set; }

            public string PROCESSOPERATIONNAME { get; set; }

            public string PRODUCTOWNER { get; set; }

            public string PRODUCTSPECNAME { get; set; }

            public string BCPRODUCTTYPE { get; set; }

            public string BCPRODUCTTYPE_POL { get; set; }

            public string BCPRODUCTTYPE_CUT { get; set; }

            public string BCPRODUCTTYPE_CUT_CROSS { get; set; }

            public string PRODUCTID { get; set; }

            public string PRODUCTID_POL { get; set; }

            public string PRODUCTID_CUT { get; set; }

            public string PRODUCTID_CUT_CROSS { get; set; }

            public string CFREWORKCOUNT { get; set; }

            public string CFINLINEREWORKMAXCOUNT { get; set; }

            public string TARGETCSTID_CF { get; set; }

            public string LINERECIPENAME { get; set; }

            public string PPID { get; set; }

            public string OPI_CURRENTLINEPPID { get; set; }

            public string OPI_CROSSLINEPPID { get; set; }

            public string CSTSETTINGCODE { get; set; }

            [XmlArray("PROCESSLINELIST")]
            [XmlArrayItem("PROCESSLINE")]
            public List<OFFLINEPROCESSLINEc> PROCESSLINELIST { get; set; }

            [XmlArray("STBPRODUCTSPECLIST")]
            [XmlArrayItem("STBPRODUCTSPEC")]
            public List<OFFLINESTBPRODUCTSPECc> STBPRODUCTSPECLIST { get; set; }

            [XmlArray("PRODUCTLIST")]
            [XmlArrayItem("PRODUCTDATA")]
            public List<OFFLINEPRODUCTDATAc> PRODUCTLIST { get; set; }

            //20151222 cy add for CUT
            public string CSTSETTINGCODE_CUT { get; set; }

            public LOTDATAc()
            {
                  LOTNAME = string.Empty;
                  PROCESSOPERATIONNAME = string.Empty;
                  PRODUCTOWNER = string.Empty;
                  PRODUCTSPECNAME = string.Empty;
                  BCPRODUCTTYPE = string.Empty;
                  BCPRODUCTTYPE_POL = string.Empty;
                  BCPRODUCTTYPE_CUT = string.Empty;
                  BCPRODUCTTYPE_CUT_CROSS = string.Empty;
                  PRODUCTID = string.Empty;
                  PRODUCTID_POL = string.Empty;
                  PRODUCTID_CUT = string.Empty;
                  PRODUCTID_CUT_CROSS = string.Empty;
                  CFREWORKCOUNT = string.Empty;
                  CFINLINEREWORKMAXCOUNT = string.Empty;
                  TARGETCSTID_CF = string.Empty;
                  LINERECIPENAME = string.Empty;
                  PPID = string.Empty;
                  OPI_CURRENTLINEPPID = string.Empty;
                  OPI_CROSSLINEPPID = string.Empty;
                  CSTSETTINGCODE = string.Empty;
                  PROCESSLINELIST = new List<OFFLINEPROCESSLINEc>();
                  STBPRODUCTSPECLIST = new List<OFFLINESTBPRODUCTSPECc>();
                  PRODUCTLIST = new List<OFFLINEPRODUCTDATAc>();
                  CSTSETTINGCODE_CUT = string.Empty;
            }

            public object Clone()
            {
                  LOTDATAc lot = (LOTDATAc)this.MemberwiseClone();
                  lot.PRODUCTLIST = new List<OFFLINEPRODUCTDATAc>();
                  if (this.PRODUCTLIST != null)
                  {
                        foreach (OFFLINEPRODUCTDATAc p in this.PRODUCTLIST)
                        {
                              lot.PRODUCTLIST.Add((OFFLINEPRODUCTDATAc)p.Clone());
                        }
                  }

                  lot.PROCESSLINELIST = new List<OFFLINEPROCESSLINEc>();
                  if (this.PROCESSLINELIST != null)
                  {
                        foreach (OFFLINEPROCESSLINEc pc in this.PROCESSLINELIST)
                        {
                              lot.PROCESSLINELIST.Add((OFFLINEPROCESSLINEc)pc.Clone());
                        }
                  }
                  lot.STBPRODUCTSPECLIST = new List<OFFLINESTBPRODUCTSPECc>();
                  if (this.STBPRODUCTSPECLIST != null)
                  {
                        foreach (OFFLINESTBPRODUCTSPECc st in this.STBPRODUCTSPECLIST)
                        {
                              lot.STBPRODUCTSPECLIST.Add((OFFLINESTBPRODUCTSPECc)st.Clone());
                        }
                  }
                  return lot;
            }
      }

      [Serializable]
      [TypeConverter(typeof(ExpandableObjectConverter))]
      public class OFFLINEPROCESSLINEc : ICloneable
      {
            public string LINENAME { get; set; }

            public string LINERECIPENAME { get; set; }

            public string PPID { get; set; }

            public string CSTSETTINGCODE { get; set; }

            public OFFLINEPROCESSLINEc()
            {
                  LINENAME = string.Empty;
                  LINERECIPENAME = string.Empty;
                  PPID = string.Empty;
                  CSTSETTINGCODE = string.Empty;
            }

            public object Clone()
            {
                  OFFLINEPROCESSLINEc pl = (OFFLINEPROCESSLINEc)this.MemberwiseClone();
                  return pl;
            }
      }

      [Serializable]
      [TypeConverter(typeof(ExpandableObjectConverter))]
      public class OFFLINESTBPRODUCTSPECc : ICloneable
      {
            public string LINENAME { get; set; }

            public string LINERECIPENAME { get; set; }

            public string PPID { get; set; }

            public string CSTSETTINGCODE { get; set; }

            public OFFLINESTBPRODUCTSPECc()
            {
                  LINENAME = string.Empty;
                  LINERECIPENAME = string.Empty;
                  PPID = string.Empty;
                  CSTSETTINGCODE = string.Empty;
            }

            public object Clone()
            {
                  OFFLINESTBPRODUCTSPECc st = (OFFLINESTBPRODUCTSPECc)this.MemberwiseClone();
                  return st;
            }
      }

      [Serializable]
      [TypeConverter(typeof(ExpandableObjectConverter))]
      public class OFFLINEPRODUCTDATAc : ICloneable
      {
            public string SLOTNO { get; set; }

            public string PROCESSFLAG { get; set; }

            public string PRODUCTNAME { get; set; }

            public string PRODUCTRECIPENAME { get; set; }

            public string OPI_PPID { get; set; }

            public string PRODUCTTYPE { get; set; }

            public string PRODUCTGRADE { get; set; }

            public string PRODUCTJUDGE { get; set; }

            public string GROUPID { get; set; }

            public string TEMPERATUREFLAG { get; set; }

            public string PROCESSTYPE { get; set; }

            public string TARGETCSTID { get; set; }

            public string INSPRESERVATION { get; set; }

            public string PREINLINEID { get; set; }

            public string FLOWPRIORITY { get; set; }

            public string NETWORKNO { get; set; }

            public string REPAIRCOUNT { get; set; }

            public string ABNORMALCODE { get; set; }

            public string OWNERTYPE { get; set; }

            public string OWNERID { get; set; }

            public string REVPROCESSOPERATIONNAME { get; set; }

            public string EQPFLAG { get; set; }

            public string SUBSTRATETYPE { get; set; }

            public string AGINGENABLE { get; set; }

            public string OXR { get; set; }

            public string OXRFLAG { get; set; }

            public string COAVERSION { get; set; }

            public string SCRAPCUTFLAG { get; set; }

            public string PANELSIZE { get; set; }

            public string PANELSIZE_CROSS { get; set; }

            public string TRUNANGLE { get; set; }

            public string PANELSIZEFLAG { get; set; }

            public string PANELSIZEFLAG_CROSS { get; set; }

            public string REWORKCOUNT { get; set; }

            public string ARRAYTTPEQVERSION { get; set; }

            public string NODESTACK { get; set; }

            public string REPAIRRESULT { get; set; }

            public string TARGET_SLOTNO { get; set; }

            public string CFINLINEREWORKMAXCOUNT { get; set; }

            //20151222 cy add for CUT
            public string REJUDGE_COUNT { get; set; }
            public string VENDOR_NAME { get; set; }
            public string BUR_CHECK_COUNT { get; set; }

            public OFFLINEPRODUCTDATAc()
            {
                  SLOTNO = string.Empty;
                  PROCESSFLAG = string.Empty;
                  PRODUCTNAME = string.Empty;
                  PRODUCTRECIPENAME = string.Empty;
                  OPI_PPID = string.Empty;
                  PRODUCTTYPE = string.Empty;
                  PRODUCTGRADE = string.Empty;
                  PRODUCTJUDGE = string.Empty;
                  GROUPID = string.Empty;
                  TEMPERATUREFLAG = string.Empty;
                  PROCESSTYPE = string.Empty;
                  TARGETCSTID = string.Empty;
                  INSPRESERVATION = string.Empty;
                  PREINLINEID = string.Empty;
                  FLOWPRIORITY = string.Empty;
                  NETWORKNO = string.Empty;
                  REPAIRCOUNT = string.Empty;
                  ABNORMALCODE = string.Empty;
                  OWNERTYPE = string.Empty;
                  OWNERID = string.Empty;
                  REVPROCESSOPERATIONNAME = string.Empty;
                  EQPFLAG = string.Empty;
                  SUBSTRATETYPE = string.Empty;
                  AGINGENABLE = string.Empty;
                  OXR = string.Empty;
                  OXRFLAG = string.Empty;
                  COAVERSION = string.Empty;
                  SCRAPCUTFLAG = string.Empty;
                  PANELSIZE = string.Empty;
                  PANELSIZE_CROSS = string.Empty;
                  TRUNANGLE = string.Empty;
                  PANELSIZEFLAG = string.Empty;
                  PANELSIZEFLAG_CROSS = string.Empty;
                  REWORKCOUNT = string.Empty;
                  ARRAYTTPEQVERSION = string.Empty;
                  NODESTACK = string.Empty;
                  REPAIRRESULT = string.Empty;
                  TARGET_SLOTNO = string.Empty;
                  CFINLINEREWORKMAXCOUNT = string.Empty;
                  REJUDGE_COUNT = "0";
                  BUR_CHECK_COUNT = "0";
                  VENDOR_NAME = string.Empty;
            }

            public object Clone()
            {
                  PRODUCTc p = (PRODUCTc)this.MemberwiseClone();
                  return p;
            }
      }
}
