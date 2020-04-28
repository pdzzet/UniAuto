using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class ForceCleanOutCommandReportReply : Message
    {
        public class COMMANDc
        {
            public string EQUIPMENTNO { get; set; }

            public string BCSTATUS { get; set; }

            public string EQSTATUS { get; set; }
            

            public COMMANDc()
            {
                EQUIPMENTNO = string.Empty;
                BCSTATUS = string.Empty;
                EQSTATUS = string.Empty;
                
            }
        }

        public class TrxBody : Body
		{
			public string LINENAME { get; set; }

            public string COMMANDTYPE { get; set; }

            public string SETSTATUS { get; set; }

            public string USERID { get; set; }

            [XmlArray("COMMANDLIST")]
            [XmlArrayItem("COMMAND")]
            public List<COMMANDc> COMMANDLIST { get; set; }

			public TrxBody()
			{
				LINENAME = string.Empty;
                COMMANDTYPE = string.Empty;
                SETSTATUS = string.Empty;
                USERID = string.Empty;
                COMMANDLIST = new List<COMMANDc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public ForceCleanOutCommandReportReply()
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
