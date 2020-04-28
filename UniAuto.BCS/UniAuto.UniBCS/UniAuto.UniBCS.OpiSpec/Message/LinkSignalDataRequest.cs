using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
	[XmlRoot("MESSAGE")]
	public class LinkSignalDataRequest : Message
	{

        public class SEQUENCENO_WORDc
        {
            public string SEQUENCENO_WORD { get; set; }

            public SEQUENCENO_WORDc()
            {
                SEQUENCENO_WORD = string.Empty;
            }
        }

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string UPSTREAMEQUIPMENTNO { get; set; }

			public string UPSTREAMEQUIPMENTUNITNO { get; set; }

            public string UPSTREAMSEQUENCENO_BIT { get; set; }

            [XmlArray("UPSTREAMSEQUENCENOLIST")]
            [XmlArrayItem("SEQUENCENO_WORD")]
            public List<SEQUENCENO_WORDc> UPSTREAMSEQUENCENOLIST { get; set; }

			public string DOWNSTREAMEQUIPMENTNO { get; set; }

			public string DOWNSTREAMEQUIPMENTUNITNO { get; set; }

            public string DOWNSTREAMSEQUENCENO_BIT { get; set; }

            [XmlArray("DOWNSTREAMSEQUENCENOLIST")]
            [XmlArrayItem("SEQUENCENO_WORD")]
            public List<SEQUENCENO_WORDc> DOWNSTREAMSEQUENCENOLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				UPSTREAMEQUIPMENTNO = string.Empty;
				UPSTREAMEQUIPMENTUNITNO = string.Empty;
                UPSTREAMSEQUENCENO_BIT = string.Empty;
				DOWNSTREAMEQUIPMENTNO = string.Empty;
				DOWNSTREAMEQUIPMENTUNITNO = string.Empty;
                DOWNSTREAMSEQUENCENO_BIT = string.Empty;

                UPSTREAMSEQUENCENOLIST = new List<SEQUENCENO_WORDc>();
                DOWNSTREAMSEQUENCENOLIST = new List<SEQUENCENO_WORDc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

		public LinkSignalDataRequest()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
			this.WaitReply = "LinkSignalDataReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
	}
}
