//Add By Yangzhenteng For OPI Display 20180904
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class MaterialRealWeightReport : Message
    {
        public class TrxBody : Body
        {
            public string MaterialForCF01ID { get; set; }
            public string MaterialForCF01Weight { get; set; }
            public string MaterialForCF01Status { get; set; }
            public string MaterialForCF02ID { get; set; }
            public string MaterialForCF02Weight { get; set; }
            public string MaterialForCF02Status { get; set; }

            public string MaterialTK01ID { get; set; }
            public string MaterialTK01Weight { get; set; }
            public string MaterialTK02ID { get; set; }
            public string MaterialTK02Weight { get; set; }

            public string MaterialTK03ID { get; set; }
            public string MaterialTK03Weight { get; set; }
            public string MaterialTK04ID { get; set; }
            public string MaterialTK04Weight { get; set; }

            public string MaterialTK05ID { get; set; }
            public string MaterialTK05Weight { get; set; }
            public string MaterialTK06ID { get; set; }
            public string MaterialTK06Weight { get; set; }

            public string MaterialTK07ID { get; set; }
            public string MaterialTK07Weight { get; set; }
            public string MaterialTK08ID { get; set; }
            public string MaterialTK08Weight { get; set; }

            public TrxBody()
            {
                MaterialForCF01ID = string.Empty;
                MaterialForCF01Weight = string.Empty;
                MaterialForCF01Status = string.Empty;
                MaterialForCF02ID = string.Empty;
                MaterialForCF02Weight = string.Empty;
                MaterialForCF02Status = string.Empty;

                MaterialTK01ID = string.Empty;
                MaterialTK01Weight = string.Empty;
                MaterialTK02ID = string.Empty;
                MaterialTK02Weight = string.Empty;

                MaterialTK03ID = string.Empty;
                MaterialTK03Weight = string.Empty;
                MaterialTK04ID = string.Empty;
                MaterialTK04Weight = string.Empty;

                MaterialTK05ID = string.Empty;
                MaterialTK05Weight = string.Empty;
                MaterialTK06ID = string.Empty;
                MaterialTK06Weight = string.Empty;

                MaterialTK07ID = string.Empty;
                MaterialTK07Weight = string.Empty;
                MaterialTK08ID = string.Empty;
                MaterialTK08Weight = string.Empty;
            }
        }
        public TrxBody BODY { get; set; }
        public new Return RETURN { get { return _return; } set { _return = value; } }
        public MaterialRealWeightReport()
        {
            this.Direction = Spec.DirType.BC_TO_OPI;
            this.WaitReply = "";//"MaterialRealWeightReport";
            this.HEADER.MESSAGENAME = GetType().Name;
            this.BODY = new TrxBody();
        }
        public override Body GetBody()
        {
            return this.BODY;
        }
    }
}


