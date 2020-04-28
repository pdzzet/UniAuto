using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class ProductTypeInfoRequestReply : Message
    {
        public class EQUIPMENTc
        {
            public string EQUIPMENTNO { get; set; }

            public string PRODUCTTYPE { get; set; }

            public string UNIT01_PRODUCTTYPE { get; set; }

            public string UNIT02_PRODUCTTYPE { get; set; }

            public string UNIT03_PRODUCTTYPE { get; set; }

            public string UNIT04_PRODUCTTYPE { get; set; }
            
            public string UNIT05_PRODUCTTYPE { get; set; }
            
            public string UNIT06_PRODUCTTYPE { get; set; }
            
            public string UNIT07_PRODUCTTYPE { get; set; }
            
            public string UNIT08_PRODUCTTYPE { get; set; }

            public EQUIPMENTc()
            {
                EQUIPMENTNO = string.Empty;
                PRODUCTTYPE = string.Empty;
                UNIT01_PRODUCTTYPE = string.Empty;
                UNIT02_PRODUCTTYPE = string.Empty;
                UNIT03_PRODUCTTYPE = string.Empty;
                UNIT04_PRODUCTTYPE = string.Empty;
                UNIT05_PRODUCTTYPE = string.Empty;
                UNIT06_PRODUCTTYPE = string.Empty;
                UNIT07_PRODUCTTYPE = string.Empty;
                UNIT08_PRODUCTTYPE = string.Empty;
            }
        }

        public class TrxBody : Body
        {
            public string LINENAME { get; set; }

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

        public ProductTypeInfoRequestReply()
        {
            this.Direction = Spec.DirType.OPI_TO_BC;
            this.WaitReply = "ProductTypeInfoRequestReply";
            this.HEADER.MESSAGENAME = GetType().Name;
            this.BODY = new TrxBody();
        }

        public override Body GetBody()
        {
            return this.BODY;
        }
    }
}