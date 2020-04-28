using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class LocalModeCassetteDataSend_DPI : Message
    {
        public class PORTc
        {
            public string PORTNO { get; set; }

            public string PORTID { get; set; }

            public string CASSETTEID { get; set; }

            public string CSTSETTINGCODE { get; set; }
            
            public LOTDATAc LOTDATA { get; set; }

            public PORTc()
            {
                PORTNO = string.Empty;
                PORTID = string.Empty;
                CASSETTEID = string.Empty;
                CSTSETTINGCODE = string.Empty;
                LOTDATA = new LOTDATAc();
            }
        }

        public class LOTDATAc
		{
			public string LOTNAME { get; set; }

            public string CSTSETTINGCODE { get; set; }

			[XmlArray("PRODUCTLIST")]
			[XmlArrayItem("PRODUCTDATA")]
			public List<PRODUCTDATAc> PRODUCTLIST { get; set; }

			public LOTDATAc()
			{
				LOTNAME = string.Empty;
				PRODUCTLIST = new List<PRODUCTDATAc>();
			}
		}

		public class PRODUCTDATAc
		{
			public string SLOTNO { get; set; }

			[XmlIgnore]
			public bool PROCESSFLAGbool { get; set; }

			public string PROCESSFLAG
			{
				get{ return PROCESSFLAGbool ? "Y" : "N"; }
				set{ PROCESSFLAGbool = (string.Compare(value, "Y", true) == 0); }
			}

			public string PRODUCTRECIPENAME { get; set; }

			public string PPID { get; set; }

			public PRODUCTDATAc()
			{
				SLOTNO = string.Empty;
				PROCESSFLAGbool = false;
				PRODUCTRECIPENAME = string.Empty;
				PPID = string.Empty;
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

        public LocalModeCassetteDataSend_DPI()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
            this.WaitReply = "LocalModeCassetteDataSendReply_DPI";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
    }
}
