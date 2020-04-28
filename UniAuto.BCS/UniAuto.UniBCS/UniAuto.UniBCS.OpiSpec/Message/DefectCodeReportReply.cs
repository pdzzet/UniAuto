using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;


namespace UniAuto.UniBCS.OpiSpec
{
    [XmlRoot("MESSAGE")]
    public class DefectCodeReportReply : Message 
    {
        public class DEFECTc
        {
            public string CASSETTE_SEQNO { get; set; }
            public string JOB_SEQNO { get; set; }
            public string CHIP_POSITION { get; set; }
            public string UNITNO { get; set; }
            public string DEFECT_CODE { get; set; }

            public DEFECTc()
            {
                CASSETTE_SEQNO = string.Empty;
                JOB_SEQNO = string.Empty;
                CHIP_POSITION = string.Empty;
                UNITNO = string.Empty;
                DEFECT_CODE = string.Empty;
            }
        }

        public class TrxBody : Body
		{
			public string LINENAME { get; set; }
            public string EQUIPMENTNO { get; set; }

            [XmlArray("DEFECTLIST")]
            [XmlArrayItem("DEFECT")]
            public List<DEFECTc> DEFECTLIST { get; set; }

            public TrxBody()
			{
				LINENAME = string.Empty;
                EQUIPMENTNO = string.Empty;

                DEFECTLIST = new List<DEFECTc>();
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public DefectCodeReportReply()
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
 