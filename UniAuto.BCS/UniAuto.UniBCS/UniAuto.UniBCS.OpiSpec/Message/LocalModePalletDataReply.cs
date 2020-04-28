using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class LocalModePalletDataReply : Message
    {
        public class DENSEBOXc
        {
            public string DNESEBOXIDNO { get; set; }

            public string DNESEBOXID { get; set; }

            public DENSEBOXc()
            {
                DNESEBOXIDNO = string.Empty;
                DNESEBOXID = string.Empty;
            }
        }

        public class TrxBody : Body
		{
			public string LINENAME { get; set; }

            public string PALLETNO { get; set; }

            public string PALLETID { get; set; }

            public string DENSEBOXCOUNT { get; set; }

            [XmlArray("DENSEBOXLIST")]
            [XmlArrayItem("DENSEBOX")]
            public List<DENSEBOXc> DENSEBOXLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
                PALLETNO = string.Empty;
                PALLETID = string.Empty;
                DENSEBOXCOUNT = string.Empty;
                DENSEBOXLIST = new List<DENSEBOXc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public LocalModePalletDataReply()
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
