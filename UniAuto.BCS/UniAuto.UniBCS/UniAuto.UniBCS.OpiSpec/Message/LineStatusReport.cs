using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class LineStatusReport : Message
    {
        public class TrxBody : Body
        {
            public string LINENAME { get; set; }

            public string LINETYPE { get; set; }

            public string FACTORYTYPE { get; set; }

            public string LINEID { get; set; }

            public string MESCONTROLSTATENAM { get; set; }

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

            public TrxBody()
            {
                LINENAME = string.Empty;
                LINETYPE = string.Empty;
                FACTORYTYPE = string.Empty;
                LINEID = string.Empty;
                MESCONTROLSTATENAM = string.Empty;
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
            }
        }

        public TrxBody BODY { get; set; }

        public new Return RETURN { get { return _return; } set { _return = value; } }

        public LineStatusReport()
        {
            this.Direction = Spec.DirType.BC_TO_OPI;
            this.WaitReply = "";//"LineStatusReportReply";
            this.HEADER.MESSAGENAME = GetType().Name;
            this.BODY = new TrxBody();
        }

        public override Body GetBody()
        {
            return this.BODY;
        }
    }
}
