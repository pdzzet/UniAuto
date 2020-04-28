using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class UnloadingPortSettingReportReply : Message
    {
        public class TrxBody : Body
		{
			public string LINENAME { get; set; }
            public string EQUIPMENTNO { get; set; }
            public string PORTNO { get; set; }
            public string OKSTOREQTIME { get; set; }
            public string OKPRODUCTTYPECHECKMODE { get; set; }
            public string NGSTOREQTIME { get; set; }
            public string NGPRODUCTTYPECHECKMODE { get; set; }
            public string NGPORTJUDGE { get; set; }
            public string PDSTOREQTIME { get; set; }
            public string PDPRODUCTTYPECHECKMODE { get; set; }
            public string RPSTOREQTIME { get; set; }
            public string RPPRODUCTTYPECHECKMODE { get; set; }
            public string IRSTOREQTIME { get; set; }
            public string IRPRODUCTTYPECHECKMODE { get; set; }
            public string MIXSTOREQTIME { get; set; }
            public string MIXPRODUCTTYPECHECKMODE { get; set; }
            public string MIXPORTJUDGE { get; set; }
            public string OPERATORID { get; set; }

            public TrxBody()
			{
				LINENAME = string.Empty;
                EQUIPMENTNO = string.Empty;
                PORTNO = string.Empty;
                OKSTOREQTIME = string.Empty;
                OKPRODUCTTYPECHECKMODE = string.Empty;
                NGSTOREQTIME = string.Empty;
                NGPRODUCTTYPECHECKMODE = string.Empty;
                NGPORTJUDGE = string.Empty;
                PDSTOREQTIME = string.Empty;
                PDPRODUCTTYPECHECKMODE = string.Empty;
                RPSTOREQTIME = string.Empty;
                RPPRODUCTTYPECHECKMODE = string.Empty;
                IRSTOREQTIME = string.Empty;
                IRPRODUCTTYPECHECKMODE = string.Empty;
                MIXSTOREQTIME = string.Empty;
                MIXPRODUCTTYPECHECKMODE = string.Empty;
                MIXPORTJUDGE = string.Empty;
                OPERATORID = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public UnloadingPortSettingReportReply()
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
