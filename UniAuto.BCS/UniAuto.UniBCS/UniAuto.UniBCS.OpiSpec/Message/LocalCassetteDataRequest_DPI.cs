using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class LocalCassetteDataRequest_DPI : Message
    {
        public class PORTc
        {
            public string PORTNO { get; set; }

            public string PORTID { get; set; }

            public string CASSETTEID { get; set; }

            public PORTc()
            {
                PORTNO = string.Empty;
                PORTID = string.Empty;
                CASSETTEID = string.Empty;
            }
        }

        public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string EQUIPMENTNO { get; set; }

            [XmlArray("PORTLIST")]
            [XmlArrayItem("PORT")]
            public List<PORTc> PORTLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				EQUIPMENTNO = string.Empty;
                PORTLIST = new List<PORTc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public LocalCassetteDataRequest_DPI()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
            this.WaitReply = "LocalCassetteDataReply_DPI";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
    }
}
