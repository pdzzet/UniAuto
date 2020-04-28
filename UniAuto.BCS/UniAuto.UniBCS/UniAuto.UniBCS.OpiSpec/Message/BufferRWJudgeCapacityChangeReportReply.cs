using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class BufferRWJudgeCapacityChangeReportReply : Message
    {
        public class TrxBody : Body
		{
			public string LINENAME { get; set; }
            public string EQUIPMENTNO { get; set; }
            public string BUFFERJUDGECAPACITY { get; set; }
            public string BF01RWJUDGECAPACITY { get; set; }
            public string BF02RWJUDGECAPACITY { get; set; }
            public string OPERATORID { get; set; }

            public TrxBody()
			{
				LINENAME = string.Empty;
                EQUIPMENTNO = string.Empty;
                BUFFERJUDGECAPACITY = string.Empty;
                BF01RWJUDGECAPACITY = string.Empty;
                BF02RWJUDGECAPACITY = string.Empty;
                OPERATORID = string.Empty;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public BufferRWJudgeCapacityChangeReportReply()
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
