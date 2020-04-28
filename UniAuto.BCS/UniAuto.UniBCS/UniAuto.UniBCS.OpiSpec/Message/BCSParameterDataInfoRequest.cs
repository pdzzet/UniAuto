using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class BCSParameterDataInfoRequest : Message
    {
        public class PARAMETERc
        {
            public string EQUIPMENTNO { get; set; }
            public string NAME { get; set; }

            public PARAMETERc()
            {
                EQUIPMENTNO= string.Empty;
                NAME = string.Empty;
            }
        }

        public class TrxBody : Body
		{
			public string LINENAME { get; set; }

            [XmlArray("PARAMETERLIST")]
            [XmlArrayItem("PARAMETER")]
            public List<PARAMETERc> PARAMETERLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;

                PARAMETERLIST = new List<PARAMETERc>();

			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public BCSParameterDataInfoRequest()
		{
			this.Direction = Spec.DirType.OPI_TO_BC;
            this.WaitReply = "BCSParameterDataInfoReply";
			this.HEADER.MESSAGENAME = GetType().Name;
			this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
    }
}
