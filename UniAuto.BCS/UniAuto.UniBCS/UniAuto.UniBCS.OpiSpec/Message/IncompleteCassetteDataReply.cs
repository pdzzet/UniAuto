using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class IncompleteCassetteDataReply : Message
    {
        public class PRODUCTc
        {
            public string POSITION { get; set; }

            public string PRODUCTNAME { get; set; }

            public string HOSTPRODUCTNAME { get; set; }

            public string DENSEBOXID { get; set; }

            public string PRODUCTJUDGE { get; set; }

            public string PRODUCTGRADE { get; set; }

            //public string SUBPRODUCTGRADES { get; set; }

            public string PAIRPRODUCTNAME { get; set; }

            public string LOTNAME { get; set; }

            public string PRODUCTRECIPENAME { get; set; }

            public string HOSTPRODUCTRECIPENAME { get; set; }

            public string PRODUCTSPECNAME { get; set; }

            public string PROCESSOPERATIONNAME { get; set; }

            public string PRODUCTOWNER { get; set; }

            public string VCRREADFLAG { get; set; }

            public string SHORTCUTFLAG { get; set; }

            public string SAMPLEFLAG { get; set; }

            [XmlIgnore]
            public bool PROCESSFLAGbool { get; set; }

            public string PROCESSFLAG
            {
                get { return PROCESSFLAGbool ? "Y" : "N"; }
                set { PROCESSFLAGbool = (string.Compare(value, "Y", true) == 0); }
            }

            public string PROCESSCOMMUNICATIONSTATE { get; set; }

            public PRODUCTc()
            {
                POSITION = string.Empty;
                PRODUCTNAME = string.Empty;
                HOSTPRODUCTNAME = string.Empty;
                DENSEBOXID = string.Empty;
                PRODUCTJUDGE = string.Empty;
                PRODUCTGRADE = string.Empty;
                //SUBPRODUCTGRADES = string.Empty;
                PAIRPRODUCTNAME = string.Empty;
                LOTNAME = string.Empty;
                PRODUCTRECIPENAME = string.Empty;
                HOSTPRODUCTRECIPENAME = string.Empty;
                PRODUCTSPECNAME = string.Empty;
                PROCESSOPERATIONNAME = string.Empty;
                PRODUCTOWNER = string.Empty;
                VCRREADFLAG = string.Empty;
                SHORTCUTFLAG = string.Empty;
                SAMPLEFLAG = string.Empty;
                PROCESSFLAGbool = false;
                PROCESSCOMMUNICATIONSTATE = string.Empty;
            }
        }

        public class TrxBody : Body
        {
            public string LINENAME { get; set; }

            public string INCOMPLETEDATE { get; set; }

            public string PORTID { get; set; }

            public string CASSETTEID { get; set; }

            public string CARRIERNAME { get; set; }

            public string LINERECIPENAME { get; set; }

            public string HOSTLINERECIPENAME { get; set; }

            public string PPID { get; set; }

            public string HOSTPPID { get; set; }

            public string RETURNMSG { get; set; }

            public string MESTRXID { get; set; }

            public string FILENAME { get; set; }

            [XmlArray("INCOMPLETECASSETTEDATALIST")]
            [XmlArrayItem("PRODUCT")]
            public List<PRODUCTc> INCOMPLETECASSETTEDATALIST { get; set; }

            public TrxBody()
            {
                LINENAME = string.Empty;
                INCOMPLETEDATE = string.Empty;
                PORTID = string.Empty;
                CASSETTEID = string.Empty;
                CARRIERNAME = string.Empty;
                LINERECIPENAME = string.Empty;
                HOSTLINERECIPENAME = string.Empty;
                PPID = string.Empty;
                HOSTPPID = string.Empty;
                RETURNMSG = string.Empty;
                MESTRXID = string.Empty;
                FILENAME = string.Empty;
                INCOMPLETECASSETTEDATALIST = new List<PRODUCTc>();
            }
        }

        public TrxBody BODY { get; set; }

        public new Return RETURN { get { return _return; } set { _return = value; } }

        public IncompleteCassetteDataReply()
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
