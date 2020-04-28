using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace UniAuto.UniBCS.MesSpec
{
    [XmlRoot("MESSAGE")]
    public class ProductInspectionDataReport : Message
    {
        public class TrxBody : Body
		{
            public string LINENAME { get; set; }

            public string MACHINENAME { get; set; }

            public string PRODUCTNAME { get; set; }

            public string INSPECTIONRESULT { get; set; }

            public string INSPECTIONTIME { get; set; }

            /// <summary>
            /// DEFECTCODELIST Values: DEFECT1;DEFECT2;...
            /// </summary>
            public string DEFECTCODELIST { get; set; }

			[XmlIgnore]
			public DateTime TIMESTAMPdt { get; set; }

			public string TIMESTAMP
			{
				get { return this.TIMESTAMPdt.ToString("yyyy-MM-dd HH:mm:ss.fff", System.Globalization.DateTimeFormatInfo.InvariantInfo); }
				set
				{
					if(string.IsNullOrEmpty(value))
						this.TIMESTAMPdt = DateTime.MinValue;
					else
						this.TIMESTAMPdt = DateTime.Parse(value);
				}
			}

			public TrxBody()
			{
				LINENAME = string.Empty;
                MACHINENAME = string.Empty;
                PRODUCTNAME = string.Empty;
                INSPECTIONRESULT = string.Empty;
                INSPECTIONTIME = string.Empty;
                DEFECTCODELIST = string.Empty;
				TIMESTAMPdt = DateTime.MinValue;
			}
		}

		public TrxBody BODY { get; set; }

		public new Return RETURN { get { return _return; } set { _return = value; } }

        public ProductInspectionDataReport()
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
