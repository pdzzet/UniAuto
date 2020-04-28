using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class RobotUnloaderDispatchRuleReply : Message
    {
        public class PORTc
        {
            public string EQUIPMENTNO { get; set; }

            public string PORTNO { get; set; }

            public string PORTID { get; set; }  

            public string GRADE_1 { get; set; }

            public string GRADE_2 { get; set; }

            public string GRADE_3 { get; set; }

            //public string GRADE_4 { get; set; }

            //public string ABNORMALCODE_1 { get; set; }

            //public string ABNORMALCODE_2 { get; set; }

            //public string ABNORMALCODE_3 { get; set; }

            //public string ABNORMALCODE_4 { get; set; }

            //public string ABNORMALFLAG { get; set; }

            public string OPERATORID { get; set; }

            public PORTc()
            {
                EQUIPMENTNO = string.Empty;
                PORTNO = string.Empty;
                PORTID = string.Empty;
                GRADE_1 = string.Empty;
                GRADE_2 = string.Empty;
                GRADE_3 = string.Empty;
                //GRADE_4 = string.Empty;
                //ABNORMALCODE_1 = string.Empty;
                //ABNORMALCODE_2 = string.Empty;
                //ABNORMALCODE_3 = string.Empty;
                //ABNORMALCODE_4 = string.Empty;
                //ABNORMALFLAG = string.Empty;
                OPERATORID = string.Empty;
            }
        }

        public class TrxBody : Body
        {
            public string LINENAME { get; set; }

            [XmlArray("PORTLIST")]
            [XmlArrayItem("PORT")]
            public List<PORTc> PORTLIST { get; set; }

            public TrxBody()
            {
                LINENAME = string.Empty;
                PORTLIST = new List<PORTc>();
            }
        }

        public TrxBody BODY { get; set; }

        public new Return RETURN { get { return _return; } set { _return = value; } }

        public RobotUnloaderDispatchRuleReply()
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
