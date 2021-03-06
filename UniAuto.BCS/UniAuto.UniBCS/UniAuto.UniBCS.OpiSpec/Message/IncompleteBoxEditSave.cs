﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class IncompleteBoxEditSave : Message
    {
        public class ABNORMALCODEc
        {
            public string ABNORMALSEQ { get; set; }

            public string ABNORMALCODE { get; set; }

            public ABNORMALCODEc()
            {
                ABNORMALSEQ = string.Empty;
                ABNORMALCODE = string.Empty;
            }
        }

        public class PRODUCTc
        {
            public string POSITION { get; set; }

            public string PRODUCTNAME { get; set; }

            public string HOSTPRODUCTNAME { get; set; }

            public string SHORTCUTFLAG { get; set; }

            public string BOXULDFLAG { get; set; }

            public string DPIPROCESSFLAG { get; set; }

            public string RTPFLAG { get; set; }

            public string PPID { get; set; }

            public string HOSTPPID { get; set; }

            [XmlArray("ABNORMALCODELIST")]
            [XmlArrayItem("ABNORMALCODE")]
            public List<ABNORMALCODEc> ABNORMALCODELIST { get; set; }

            public PRODUCTc()
            {
                POSITION = string.Empty;
                PRODUCTNAME = string.Empty;
                HOSTPRODUCTNAME = string.Empty;
                SHORTCUTFLAG = string.Empty;
                BOXULDFLAG = string.Empty;
                DPIPROCESSFLAG = string.Empty;
                RTPFLAG = string.Empty;
                PPID = string.Empty;
                HOSTPPID = string.Empty;
                ABNORMALCODELIST = new List<ABNORMALCODEc>();
            }
        }

        public class BOXc
        {
            public string BOXNAME { get; set; }

            public string PRODUCTQUANTITY { get; set; }

            [XmlArray("PRODUCTLIST")]
            [XmlArrayItem("PRODUCT")]
            public List<PRODUCTc> PRODUCTLIST { get; set; }

            public BOXc()
            {
                BOXNAME = string.Empty;
                PRODUCTQUANTITY = string.Empty;
                PRODUCTLIST = new List<PRODUCTc>();
            }
        }

        public class TrxBody : Body
        {
            public string LINENAME { get; set; }

            public string OPERATORID { get; set; }

            public string INCOMPLETEDATE { get; set; }

            public string PORTID { get; set; }

            public string MESTRXID { get; set; }

            public string FILENAME { get; set; }

            public string LINERECIPENAME { get; set; }

            public string HOSTLINERECIPENAME { get; set; }

            public string PPID { get; set; }

            public string HOSTPPID { get; set; }

            public string BOXQUANTITY { get; set; }

            public string SAMPLEFLAG { get; set; }

            [XmlArray("BOXLIST")]
            [XmlArrayItem("BOX")]
            public List<BOXc> BOXLIST { get; set; }

            public TrxBody()
            {
                LINENAME = string.Empty;
                OPERATORID = string.Empty;
                INCOMPLETEDATE = string.Empty;
                PORTID = string.Empty;
                MESTRXID = string.Empty;
                FILENAME = string.Empty;
                LINERECIPENAME = string.Empty;
                HOSTLINERECIPENAME = string.Empty;
                PPID = string.Empty;
                HOSTPPID = string.Empty;
                BOXQUANTITY = string.Empty;
                SAMPLEFLAG = string.Empty;
                BOXLIST = new List<BOXc>();
            }
        }

        public TrxBody BODY { get; set; }

        public new Return RETURN { get { return _return; } set { _return = value; } }

        public IncompleteBoxEditSave()
        {
            this.Direction = Spec.DirType.OPI_TO_BC;
            this.WaitReply = "IncompleteBoxEditSaveReply";
            this.HEADER.MESSAGENAME = GetType().Name;
            this.BODY = new TrxBody();
        }

        public override Body GetBody()
        {
            return this.BODY;
        }
    }
}