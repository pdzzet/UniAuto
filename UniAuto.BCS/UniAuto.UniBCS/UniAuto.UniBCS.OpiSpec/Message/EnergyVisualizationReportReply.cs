using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class EnergyVisualizationReportReply : Message
    {
        public class DATAc
        {
            public string NAME { get; set; }

            public string VALUE { get; set; }

            public DATAc()
            {
                NAME = string.Empty;
                VALUE = string.Empty;
            }
        }
        public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string EQUIPMENTNO { get; set; }

            [XmlArray("DATALIST")]
            [XmlArrayItem("DATA")]
            public List<DATAc> DATALIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				EQUIPMENTNO = string.Empty;
                DATALIST = new List<DATAc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public EnergyVisualizationReportReply()
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
