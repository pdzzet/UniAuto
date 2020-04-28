using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class MaterialStatusReply : Message
    {
        public class TrxBody : Body
        {
            public string MATERIALSTATUS { get; set; }

            public string MATERIALVALUE { get; set; }

            public string MATERIALNAME { get; set; }

            public string SLOTNO { get; set; }

            public TrxBody()
            {
                MATERIALSTATUS = string.Empty;
                MATERIALVALUE = string.Empty;
                MATERIALNAME = string.Empty;
                SLOTNO = string.Empty;
            }
        }

        public TrxBody BODY { get; set; }

        public new Return RETURN { get { return _return; } set { _return = value; } }

        public MaterialStatusReply()
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