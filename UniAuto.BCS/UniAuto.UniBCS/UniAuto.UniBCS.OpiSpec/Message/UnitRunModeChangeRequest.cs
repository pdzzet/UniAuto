using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class UnitRunModeChangeRequest : Message
    {
        public class UNITc
        {
            public string UNITNO { get; set; }
            public string NEW_RUNMODE { get; set; }

            public UNITc()
            {
                UNITNO = string.Empty;
                NEW_RUNMODE = string.Empty;
            }
        }

		public class TrxBody : Body
		{
			public string LINENAME { get; set; }

			public string EQUIPMENTNO { get; set; }

            public string NEW_RUNMODE { get; set; }

            [XmlArray("UNITLIST")]
            [XmlArrayItem("UNIT")]
            public List<UNITc> UNITLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
				EQUIPMENTNO = string.Empty;
                NEW_RUNMODE = string.Empty;
                UNITLIST = new List<UNITc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public UnitRunModeChangeRequest()
		{
            this.Direction = Spec.DirType.OPI_TO_BC;
            this.WaitReply = "UnitRunModeChangeReply";
            this.HEADER.MESSAGENAME = GetType().Name;
            this.BODY = new TrxBody();
		}

		public override Body GetBody()
		{
			return this.BODY;
		}
    }
}
