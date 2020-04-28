using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
    [XmlRoot("MESSAGE")]
    public class ChamberRunModeChanged : Message
    {
        public class CHAMBERc
        {
            public string CHAMBERNAME { get; set; }

            public string CHAMBERRUNMODE { get; set; }

            public CHAMBERc()
            {
                CHAMBERNAME = string.Empty;
                CHAMBERRUNMODE = string.Empty;
            }
        }

        public class MACHINE
        {
            public string MACHINENAME { get; set; }

            [XmlArray("CHAMBERLIST")]
            [XmlArrayItem("CHAMBER")]
            public List<CHAMBERc> CHAMBERLIST { get; set; }

            public MACHINE()
            {
                MACHINENAME = string.Empty;
                CHAMBERLIST = new List<CHAMBERc>();
            }
        }

        public class TrxBody : Body
		{
			public string LINENAME { get; set; }

            public string LINEOPERMODE { get; set; }

            [XmlArray("MACHINE")]
            public List<MACHINE> MACHINE { get; set; }

            public string TIMESTAMP { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
                LINEOPERMODE = string.Empty;
                MACHINE = new List<MACHINE>();
                TIMESTAMP = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public ChamberRunModeChanged()
		{
			this.Direction = Spec.DirType.BC_TO_MES;
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
