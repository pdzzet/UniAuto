using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class BCControlCommandInfoReply : Message
    {
        public class ITEMc
        {
            public string EQUIPMENTNO { get; set; }

            public string UNITNO { get; set; }

            public string COMMAND { get; set; }

            public ITEMc()
            {
                EQUIPMENTNO = string.Empty;
                UNITNO = string.Empty;
                COMMAND = string.Empty;
            }
        }

        public class TrxBody : Body
        {
            public string LINENAME { get; set; }

            public string COMMANDTYPE { get; set; }

            [XmlArray("ITEMLIST")]
            [XmlArrayItem("ITEM")]
            public List<ITEMc> ITEMLIST { get; set; }

            public TrxBody()
            {
                LINENAME = string.Empty;
                COMMANDTYPE = string.Empty;
                ITEMLIST = new List<ITEMc>();
            }
        }

        public TrxBody BODY { get; set; }

        public new Return RETURN { get { return _return; } set { _return = value; } }

        public BCControlCommandInfoReply()
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
