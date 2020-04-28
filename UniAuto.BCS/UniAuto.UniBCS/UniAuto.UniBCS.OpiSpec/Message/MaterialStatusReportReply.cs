using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class MaterialStatusReportReply : Message
    {
        public class MATERIALc
        {
            public string MATERIALNAME { get; set; }

            public string SLOTNO { get; set; }

            public string MATERIALSTATUS { get; set; }

            public string MATERIALTYPE { get; set; }

            public MATERIALc()
            {
                MATERIALTYPE = string.Empty;
                MATERIALNAME = string.Empty;
                SLOTNO = string.Empty;
                MATERIALSTATUS = string.Empty;
            }
        }

        public class EQUIPMENTc
        {
            public string EQUIPMENTNO { get; set; }

            [XmlArray("MATERIALLIST")]
            [XmlArrayItem("MATERIAL")]
            public List<MATERIALc> MATERIALLIST { get; set; }

            public EQUIPMENTc()
            {
                EQUIPMENTNO = string.Empty;
                MATERIALLIST = new List<MATERIALc>();
            }
        }

        public class TrxBody : Body
        {
            public string LINENAME { get; set; }

            //EQUIPMENTLIST
            [XmlArray("EQUIPMENTLIST")]
            [XmlArrayItem("EQUIPMENT")]
            public List<EQUIPMENTc> EQUIPMENTLIST { get; set; }

            public TrxBody()
            {
                LINENAME = string.Empty;
                EQUIPMENTLIST = new List<EQUIPMENTc>();
            }
        }

        public TrxBody BODY { get; set; }

        public new Return RETURN { get { return _return; } set { _return = value; } }

        public MaterialStatusReportReply()
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
