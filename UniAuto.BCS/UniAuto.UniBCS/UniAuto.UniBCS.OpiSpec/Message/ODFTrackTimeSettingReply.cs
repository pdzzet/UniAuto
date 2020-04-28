using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class ODFTrackTimeSettingReply : Message
    {
        public class UNITc
        {
            public string UNITNO { get; set; }

            public UNITc()
            {
                UNITNO = string.Empty;
            }
        }

        public class DELAYTIMEc
        {
            public string SEQNO { get; set; }

            public string VALUE { get; set; }

            public DELAYTIMEc()
            {   
                SEQNO = string.Empty;
                VALUE = string.Empty;
            }
        }

        public class EQUIPMENTc
        {
            public string EQUIPMENTNO { get; set; }

            [XmlArray("UNITLIST")]
            [XmlArrayItem("UNIT")]
            public List<UNITc> UNITLIST { get; set; }

            [XmlArray("DELAYTIMELIST")]
            [XmlArrayItem("DELAYTIME")]
            public List<DELAYTIMEc> DELAYTIMELIST { get; set; }

            public EQUIPMENTc()
            {
                EQUIPMENTNO = string.Empty;
                UNITLIST = new List<UNITc>();
                DELAYTIMELIST = new List<DELAYTIMEc>();
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

        public ODFTrackTimeSettingReply()
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
